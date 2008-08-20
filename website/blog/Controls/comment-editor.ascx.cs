﻿using System;

namespace KMBlog
{
    public partial class CommentEditor : System.Web.UI.UserControl
    {

        public bool IsValid()
        {
            return !String.IsNullOrEmpty(Text);
        }

        public string Name
        {
            get
            {
                return txtName.Text;
            }
            set
            {
                txtName.Text = value;
            }
        }

        public string URL
        {
            get
            {
                return txtURL.Text;
            }
            set
            {
                txtURL.Text = value;
            }
        }

        public string Text
        {
            get
            {
				return comment_text.Value;
            }
            set
            {
				comment_text.Value = value;
            }
        }

        public void ClearValues()
        {
            txtName.Text = String.Empty;
			comment_text.Value = String.Empty;
            txtURL.Text = String.Empty;
        }
    }
}