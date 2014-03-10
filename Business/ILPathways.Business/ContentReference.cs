using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentReference : BaseBusinessDataEntity
    {

        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.ContentReference class.
        ///</summary>
        public ContentReference()        
        { }


        #region Properties
        private int _parentId;
        /// <summary>
        /// Gets/Sets the RowId of the parent AppItem
        /// </summary>
        public int ParentId
        {
            get
            {
                return this._parentId;
            }
            set
            {
                if ( this._parentId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._parentId = value;
                    HasChanged = true;
                }
            }
        }//

       
        private string _isbn = String.Empty;
        /// <summary>
        /// Gets/Sets ISBN
        /// </summary>
        public string ISBN
        {
            get
            {
                return this._isbn;
            }
            set
            {
                if ( this._isbn== value )
                {
                    //Ignore set
                }
                else
                {
                    this._isbn= value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _title = String.Empty;
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

        private string _author = String.Empty;
        /// <summary>
        /// Gets/Sets Author
        /// </summary>
        public string Author
        {
            get
            {
                return this._author ;
            }
            set
            {
                if ( this._author == value )
                {
                    //Ignore set
                }
                else
                {
                    this._author = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _publisher= String.Empty;
        /// <summary>
        /// Gets/Sets Publisher
        /// </summary>
        public string Publisher
        {
            get
            {
                return this._publisher;
            }
            set
            {
                if ( this._publisher== value )
                {
                    //Ignore set
                }
                else
                {
                    this._publisher= value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _resourceUrl = String.Empty;
        /// <summary>
        /// Gets/Sets ReferenceUrl
        /// </summary>
        public string ReferenceUrl
        {
            get
            {
                return this._resourceUrl;
            }
            set
            {
                if ( this._resourceUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._resourceUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _additionalInfo = String.Empty;
        /// <summary>
        /// Gets/Sets AdditionalInfo
        /// </summary>
        public string AdditionalInfo
        {
            get
            {
                return this._additionalInfo;
            }
            set
            {
                if ( this._additionalInfo == value )
                {
                    //Ignore set
                }
                else
                {
                    this._additionalInfo = value.Trim();
                    HasChanged = true;
                }
            }
        }

        #endregion

    } // end class 
} // end Namespace 

