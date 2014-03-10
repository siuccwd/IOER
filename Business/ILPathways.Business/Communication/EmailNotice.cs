using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    ///<summary>
    ///Represents an object that describes a EmailNotice
    ///</summary>
    [Serializable]
    public class EmailNotice : BaseBusinessDataEntity
    {
        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.EmailNotice class.
        ///</summary>
        public EmailNotice() { }



        #region Properties created from dictionary for EmailNotice


        private string _noticeCode = "";
        /// <summary>
        /// Gets/Sets NoticeCode
        /// </summary>
        public string NoticeCode
        {
            get
            {
                return this._noticeCode;
            }
            set
            {
                if ( this._noticeCode == value )
                {
                    //Ignore set
                }
                else
                {
                    this._noticeCode = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _title = "";
        /// <summary>
        /// Gets/Sets Title
        /// </summary>
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                if ( this._title == value )
                {
                    //Ignore set
                }
                else
                {
                    this._title = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _description = "";
        /// <summary>
        /// Gets/Sets Description
        /// </summary>
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                if ( this._description == value )
                {
                    //Ignore set
                }
                else
                {
                    this._description = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _category = "";
        /// <summary>
        /// Gets/Sets Category
        /// </summary>
        public string Category
        {
            get
            {
                return this._category;
            }
            set
            {
                if ( this._category == value )
                {
                    //Ignore set
                }
                else
                {
                    this._category = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _filter = "";
        /// <summary>
        /// Gets/Sets Filter
        /// </summary>
        public string Filter
        {
            get
            {
                return this._filter;
            }
            set
            {
                if ( this._filter == value )
                {
                    //Ignore set
                }
                else
                {
                    this._filter = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _fromEmail = "";
        /// <summary>
        /// Gets/Sets FromEmail
        /// </summary>
        public string FromEmail
        {
            get
            {
                return this._fromEmail;
            }
            set
            {
                if ( this._fromEmail == value )
                {
                    //Ignore set
                }
                else
                {
                    this._fromEmail = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _ccEmail = "";
        /// <summary>
        /// Gets/Sets CcEmail
        /// </summary>
        public string CcEmail
        {
            get
            {
                return this._ccEmail;
            }
            set
            {
                if ( this._ccEmail == value )
                {
                    //Ignore set
                }
                else
                {
                    this._ccEmail = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _bccEmail = "";
        /// <summary>
        /// Gets/Sets BccEmail
        /// </summary>
        public string BccEmail
        {
            get
            {
                return this._bccEmail;
            }
            set
            {
                if ( this._bccEmail == value )
                {
                    //Ignore set
                }
                else
                {
                    this._bccEmail = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _subject = "";
        /// <summary>
        /// Gets/Sets Subject
        /// </summary>
        public string Subject
        {
            get
            {
                return this._subject;
            }
            set
            {
                if ( this._subject == value )
                {
                    //Ignore set
                }
                else
                {
                    this._subject = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _htmlBody = "";
        /// <summary>
        /// Gets/Sets HtmlBody
        /// </summary>
        public string HtmlBody
        {
            get
            {
                return this._htmlBody;
            }
            set
            {
                if ( this._htmlBody == value )
                {
                    //Ignore set
                }
                else
                {
                    this._htmlBody = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _textBody = "";
        /// <summary>
        /// Gets/Sets TextBody
        /// </summary>
        public string TextBody
        {
            get
            {
                return this._textBody;
            }
            set
            {
                if ( this._textBody == value )
                {
                    //Ignore set
                }
                else
                {
                    this._textBody = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _languageCode = "";
        /// <summary>
        /// Gets/Sets LanguageCode
        /// </summary>
        public string LanguageCode
        {
            get
            {
                return this._languageCode;
            }
            set
            {
                if ( this._languageCode == value )
                {
                    //Ignore set
                }
                else
                {
                    this._languageCode = value.Trim();
                    HasChanged = true;
                }
            }
        }

        #endregion
    } // end class 
} // end Namespace 

