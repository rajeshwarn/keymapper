﻿namespace KMBlog
{
    using System;
    using System.Diagnostics;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Security;

    public class Global : System.Web.HttpApplication
    {
        public static string GetBlogPath()
        {
            return @"/keymapper/blog/";
        }

        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie cookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (cookie == null)
            {
                return;
            }

            FormsAuthenticationTicket ticket = null;
            try
            {
                ticket = FormsAuthentication.Decrypt(cookie.Value);
            }
            catch (Exception)
            {
                return;
            }

            if (ticket == null)
            {
                return;
            }

            string role = ticket.UserData;
            Trace.Write("Role: " + role);

            FormsIdentity id = new FormsIdentity(ticket);

            GenericPrincipal principal = new GenericPrincipal(id, new string[] { role });
            Context.User = principal;
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            string newPath = this.GetRewrittenUrl();

            if (String.IsNullOrEmpty(newPath) == false)
            {
                // Response.Write(newPath);
                // Response.Redirect(newPath);
                HttpContext.Current.RewritePath(newPath);
            }
        }

        private string GetRewrittenUrl()
        {
            string path = Request.Url.ToString();
            if (path.IndexOf("blog") == -1)
            {
                return String.Empty;
            }

            // URL can be one of these formats (optionally excluding final backslash)
            // blog/user-key-mappings/ (specific post slug)
            // blog/2008/08/01/ (posts on 1st August 2008)
            // blog/2008/08/ (posts in August 2008)
            // blog/2008/ (posts in 2008)
            // blog/category/key-mapper/ (all posts in the key mapper category)

            // maybe: blog/2008/category/key-mapper/ (posts in 2008 in the keymapper category)

            // Only interested ineveything after /blog/
            string[] pathWords = path.Substring(path.IndexOf("blog") + 4).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathWords.GetLength(0) == 0)
            {
                return String.Empty;
            }

            /* To detect:
            1) Is the word following blog a number? In that case it's a date.
            2) Is the first word 'category'?
            3) Otherwise - only expecting one word and it's the post slug.
            Use the last word for this as if there are multiple words like
            /blog/category/foo/post-slug then just use the slug. */

            if (System.Char.IsDigit(pathWords[0][0]))
            {
                string dateRange;
                if (this.GetDateRange(pathWords, out dateRange) == false)
                {
                    return String.Empty;
                }
                else
                {
                    return Request.ApplicationPath + "/blog/default.aspx?d=" + dateRange;
                }
            }

            if (pathWords[0].ToLower() == "category")
            {
                int catId = DataAccess.CreateInstance().GetCategoryIdFromSlug(pathWords[1]);
                if (catId > 0)
                {
                    return Request.ApplicationPath + "/blog/default.aspx?c=" + catId.ToString();
                }
                else
                {
                    return String.Empty; // TODO: implement some kind of 'category not found'
                }
            }

            // Otherwise: post slug.
            string slug = pathWords[pathWords.GetLength(0) - 1];

            if (!string.IsNullOrEmpty(slug))
            {
                int postId = DataAccess.CreateInstance().GetPostIdFromSlug(slug.Replace("/", string.Empty));
                if (postId > 0)
                {
                    return Request.ApplicationPath + "/blog/default.aspx?p=" + postId.ToString();
                }
                else
                {
                    return String.Empty;
                }
            }

            return String.Empty;
        }

        private bool GetDateRange(string[] pathWords, out string dateRange)
        {
            dateRange = String.Empty;
            int words = pathWords.GetLength(0);

            /* Array could be any of:
             2008
             2008,8
             2008,8,1 */

            int year = this.GetWordValue(pathWords[0]);
            if (year == 0 || year < 2008 || year > 2999)
            {
                return false;
            }

            dateRange = year.ToString();

            if (words == 1)
            {
                return true;
            }

            int month = this.GetWordValue(pathWords[1]);
            if (month < 1 || month > 12)
            {
                return true;
            }
            else
            {
                dateRange += month.ToString().PadLeft(2, '0');
                if (words == 2)
                {
                    return true;
                }
            }

            int day = this.GetWordValue(pathWords[2]);
            if (day == 0 || day > DateTime.DaysInMonth(year, month))
            {
                return true;
            }
            else
            {
                dateRange += day.ToString().PadLeft(2, '0');
                return true;
            }
        }

        private int GetWordValue(string word)
        {
            foreach (char character in word.ToCharArray())
            {
                if (System.Char.IsDigit(character) == false)
                {
                    return 0;
                }
            }

            // String could be too long for an int so use TryParse just in case..
            int value = 0;
            if (Int32.TryParse(word, out value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }
    }
}