using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    /// <summary>
    /// Summary description for FormMessage.
    /// </summary>
    [Serializable]
    public class FormMessage : GenericMessage
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        public FormMessage()
        { }

        #region Properties
        private string _title = string.Empty;
        /// <summary>
        /// Gets/Sets the FormMessage's title (optional)
        /// </summary>
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }// 

        private string cssClass = string.Empty;
        /// <summary>
        /// Gets/Sets the FormMessage's associated CssClass
        /// </summary>
        public string CssClass
        {
            get { return this.cssClass; }
            set { this.cssClass = value; }
        }


        private bool _showPopup = false;
        /// <summary>
        /// Gets/Sets the FormMessage's ShowPopup
        /// True if message is to alsoi be shown in a popup
        /// </summary>
        public bool ShowPopup
        {
            get { return this._showPopup; }
            set { this._showPopup = value; }
        }

        private bool _isFormatted = false;
        /// <summary>
        /// Gets/Sets the FormMessage's IsFormatted property
        /// True if message is formatted as HTML
        /// </summary>
        public bool IsFormatted
        {
            get { return this._isFormatted; }
            set { this._isFormatted = value; }
        }

        #endregion

        #region Behaviours
        /// <summary>
        /// Return true if a message exists
        /// </summary>
        public bool IsPresent()
        {
            return ( this.Text.Length > 0 );
        }
        public bool IsEmpty()
        {
            return ( this.Text.Length == 0 );
        }

        #endregion
    }
}
