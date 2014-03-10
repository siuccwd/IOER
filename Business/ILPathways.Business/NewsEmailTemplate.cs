using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class NewsEmailTemplate : BaseBusinessDataEntity
    {
        ///<summary>
        ///Initializes a new instance of the workNet.BusObj.Entity.AppItemAnnouncementTemplate class.
        ///</summary>
        public NewsEmailTemplate() { }

        #region Properties created from dictionary for NewsEmailTemplate
        private string _newsItemCode = "";
        /// <summary>
        /// Gets/Sets NewsItemCode
        /// </summary>
        public string NewsItemCode
        {
            get
            {
                return this._newsItemCode;
            }
            set
            {
                if ( this._newsItemCode == value )
                {
                    //Ignore set
                }
                else
                {
                    this._newsItemCode = value.Trim();
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

        private string _searchUrl = "";
        /// <summary>
        /// Gets/Sets SearchUrl - home for news. 
        /// MP-for now:
        /// - Assume page is default or SearchNews for WIF
        /// - Path is the same as for DisplayUrl
        /// 121210 mparsons - should come directly from table now
        /// </summary>
        public string SearchUrl
        {
            get
            {
                //if ( _searchUrl.Trim() == "" && _displayUrl.Trim().Length > 0)  
                //{
                //  int pos = DisplayUrl.ToLower().LastIndexOf( "/" );
                //  int dotpos = DisplayUrl.ToLower().LastIndexOf( "." );
                //  if (pos > 0)
                //    _searchUrl = DisplayUrl.Substring( 0, pos + 1 ) + SearchPageName();
                //}
                return this._searchUrl;
            }
            set
            {
                if ( this._searchUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._searchUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _displayUrl = "";
        /// <summary>
        /// Gets/Sets DisplayUrl
        /// </summary>
        public string DisplayUrl
        {
            get
            {
                return this._displayUrl;
            }
            set
            {
                if ( this._displayUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._displayUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _unsubscribeUrl = "";
        /// <summary>
        /// Gets/Sets UnsubscribeUrl
        /// </summary>
        public string UnsubscribeUrl
        {
            get
            {
                return this._unsubscribeUrl;
            }
            set
            {
                if ( this._unsubscribeUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._unsubscribeUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _confirmUrl = "";
        /// <summary>
        /// Gets/Sets ConfirmUrl
        /// </summary>
        public string ConfirmUrl
        {
            get
            {
                return this._confirmUrl;
            }
            set
            {
                if ( this._confirmUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._confirmUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _template = "";
        /// <summary>
        /// Gets/Sets Template
        /// </summary>
        public string Template
        {
            get
            {
                return this._template;
            }
            set
            {
                if ( this._template == value )
                {
                    //Ignore set
                }
                else
                {
                    this._template = value.Trim();
                    HasChanged = true;
                }
            }
        }


        #endregion
        private string SearchPageName()
        {
            if ( "WIFE WIFP".IndexOf( NewsItemCode ) > -1 )
                return "SearchNews.htm";
            else
                return ""; //allow default to current channel default
        }
    } // end class 
} // end Namespace 

