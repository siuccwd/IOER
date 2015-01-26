using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentSupplement : BaseBusinessDataEntity
    {

        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.ContentSupplement class.
        ///</summary>
        public ContentSupplement()
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

        private int _parentId;
        /// <summary>
        /// Gets/Sets the id of the parent item
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

        private int _privilegeTypeId;
        /// <summary>
        /// Gets/Sets PrivilegeTypeId
        /// </summary>
        public int PrivilegeTypeId
        {
            get
            {
                return this._privilegeTypeId;
            }
            set
            {
                if ( this._privilegeTypeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._privilegeTypeId = value;
                    HasChanged = true;
                }
            }
        }

        private string _privilegeType = "";
        /// <summary>
        /// Gets/Sets PrivilegeType
        /// </summary>
        public string PrivilegeType
        {
            get
            {
                return this._privilegeType;
            }
            set
            {
                if ( this._privilegeType == value )
                {
                    //Ignore set
                }
                else
                {
                    this._privilegeType = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _resourceUrl = "";
        /// <summary>
        /// Gets/Sets ResourceUrl
        /// </summary>
        public string ResourceUrl
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
        /// <summary>
        /// HasDocument - does a related document exist in object
        /// </summary>
        /// <returns></returns>
        public bool HasDocument()
        {
            if ( RelatedDocument != null && RelatedDocument.CreatedById > 0 )
                return true;
            else
                return false;
        }
        #endregion
    } // end class 
} // end Namespace 

