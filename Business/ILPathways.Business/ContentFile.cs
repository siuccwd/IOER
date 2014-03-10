using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentFile : BaseBusinessDataEntity, IBaseObject
    {
        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.ContentFile class.
        ///</summary>
        public ContentFile()
        {
            RelatedDocument = new DocumentVersion();
        }


        #region Properties

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

        /// <summary>
        /// Optional organization id
        /// </summary>
        public int OrgId { get; set; }

        private string _resourceUrl = "";
        /// <summary>
        /// Gets/Sets ResourceUrl
        /// </summary>
        public string DocumentUrl
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

        public string FileName { get; set; }
        public string FilePath { get; set; }

        private Guid _documentRowId;
        public Guid DocumentRowId
        {
            get
            {
                return this._documentRowId;
            }
            set
            {
                if ( this._documentRowId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._documentRowId = value;
                    HasChanged = true;
                }
            }
        }
        #endregion

        #region Helpers
        public DocumentVersion RelatedDocument { get; set; }
       
        #endregion
    } // end class 
} // end Namespace 

