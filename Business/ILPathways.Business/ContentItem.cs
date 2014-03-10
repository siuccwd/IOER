using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentItem : BaseBusinessDataEntity
    {

        ///<summary>
        ///Initializes a new instance of the ILPathways.Business.ContentItem class.
        ///</summary>
        public ContentItem()
        {

        }

        /// <summary>
        /// content types
        /// </summary>
        public static int GENERAL_CONTENT_ID = 10;
        public static int TEMPLATE_CONTENT_ID = 15;
        public static int PERSONAL_HOME_PAGE_CONTENT_ID = 20;
        public static int ORG_HOME_PAGE_CONTENT_ID = 25;
        public static int LESSON_PLAN_CONTENT_ID = 30;
        public static int DOCUMENT_CONTENT_ID = 40;

        /// <summary>
        /// Status types
        /// </summary>
        public static int DRAFT_STATUS = 1;
        public static int INPROGRESS_STATUS = 2;
        public static int SUBMITTED_STATUS = 3;
        public static int REVISIONS_REQUIRED_STATUS = 4;
        public static int PUBLISHED_STATUS = 5;
        public static int INACTIVE_STATUS = 8;

        public static int PUBLIC_PRIVILEGE = 1;
        public static int MY_ORG_PRIVILEGE = 2;
        public static int MY_REGION_PRIVILEGE = 3;
        public static int MY_STATE_PRIVILEGE = 4;
        public static int MY_ORG_AND_CLIENTS_PRIVILEGE = 5;

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

        private string _summary = "";
        /// <summary>
        /// Gets/Sets Summary - for Learning Registry
        /// </summary>
        public string Summary
        {
            get
            {
                return this._summary;
            }
            set
            {
                if ( this._summary == value )
                {
                    //Ignore set
                }
                else
                {
                    this._summary = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private int _typeId;
        /// <summary>
        /// Gets/Sets TypeId
        /// </summary>
        public int TypeId
        {
            get
            {
                return this._typeId;
            }
            set
            {
                if ( this._typeId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._typeId = value;
                    HasChanged = true;
                }
            }
        }

        private bool _isOrgContent;
        /// <summary>
        /// Gets/Sets IsOrgContentOwner
        /// </summary>
        public bool IsOrgContentOwner
        {
            get
            {
                return this._isOrgContent;
            }
            set
            {
                if ( this._isOrgContent == value )
                {
                    //Ignore set
                }
                else
                {
                    this._isOrgContent = value;
                    HasChanged = true;
                }
            }
        }

        private int _orgId;
        /// <summary>
        /// Gets/Sets OrgId
        /// </summary>
        public int OrgId
        {
            get
            {
                return this._orgId;
            }
            set
            {
                if ( this._orgId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._orgId = value;
                    HasChanged = true;
                }
            }
        }

        private string _organization = "";
        /// <summary>
        /// Gets/Sets Organization
        /// </summary>
        public string Organization
        {
            get
            {
                return this._organization;
            }
            set
            {
                this._organization = value.Trim();
            }
        }

        private string _contentType = "";
        /// <summary>
        /// Gets/Sets ContentType
        /// </summary>
        public string ContentType
        {
            get
            {
                return this._contentType;
            }
            set
            {
                if ( this._contentType == value )
                {
                    //Ignore set
                }
                else
                {
                    this._contentType = value.Trim();
                    HasChanged = true;
                }
            }
        }
        private int _statusId;
        /// <summary>
        /// Gets/Sets StatusId
        /// </summary>
        public int StatusId
        {
            get
            {
                return this._statusId;
            }
            set
            {
                if ( this._statusId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._statusId = value;
                    HasChanged = true;
                }
            }
        }


        private string _status = "";
        /// <summary>
        /// Gets/Sets Status
        /// </summary>
        public string Status
        {
            get
            {
                return this._status;
            }
            set
            {
                if ( this._status == value )
                {
                    //Ignore set
                }
                else
                {
                    this._status = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private bool _isPublished = false;
        /// <summary>
        /// Gets/Sets IsPublished
        /// </summary>
        public bool IsPublished
        {
            get
            {
                return this._isPublished;
            }
            set
            {
                if ( this._isPublished == value )
                {
                    //Ignore set
                }
                else
                {
                    this._isPublished = value;
                    HasChanged = true;
                }
            }
        }

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


        private int _conditionsOfUseId;
        /// <summary>
        /// Gets/Sets ConditionsOfUseId
        /// </summary>
        public int ConditionsOfUseId
        {
            get
            {
                return this._conditionsOfUseId;
            }
            set
            {
                if ( this._conditionsOfUseId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._conditionsOfUseId = value;
                    HasChanged = true;
                }
            }
        }

        private DateTime _approved;
        /// <summary>
        /// Gets/Sets Approved
        /// </summary>
        public DateTime Approved
        {
            get
            {
                return this._approved;
            }
            set
            {
                if ( this._approved == value )
                {
                    //Ignore set
                }
                else
                {
                    this._approved = value;
                    HasChanged = true;
                }
            }
        }

        private int _approvedById;
        /// <summary>
        /// Gets/Sets ApprovedById
        /// </summary>
        public int ApprovedById
        {
            get
            {
                return this._approvedById;
            }
            set
            {
                if ( this._approvedById == value )
                {
                    //Ignore set
                }
                else
                {
                    this._approvedById = value;
                    HasChanged = true;
                }
            }
        }

        private int _resourceVersionId;
        /// <summary>
        /// Gets/Sets the ResourceVersionId
        /// </summary>
        public int ResourceVersionId
        {
            get
            {
                return this._resourceVersionId;
            }
            set
            {
                if ( this._resourceVersionId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._resourceVersionId = value;
                    HasChanged = true;
                }
            }
        }//

        private string _useRightsUrl = "";
        /// <summary>
        /// Gets/Sets UseRightsUrl
        /// </summary>
        public string UseRightsUrl
        {
            get
            {
                return this._useRightsUrl;
            }
            set
            {
                if ( this._useRightsUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._useRightsUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }

        private string _resourceUrl = "";
        /// <summary>
        /// Gets/Sets ResourceUrl for the attached document - use case where a single document is uploaded 
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

        #region composite properties

        private string _conditionsOfUse;
        /// <summary>
        /// Gets/Sets ConditionsOfUse
        /// </summary>
        public string ConditionsOfUse
        {
            get
            {
                return this._conditionsOfUse;
            }
            set
            {
                if ( this._conditionsOfUse == value )
                {
                    //Ignore set
                }
                else
                {
                    this._conditionsOfUse = value;
                    HasChanged = true;
                }
            }
        }
        private string _conditionsOfUseUrl;
        /// <summary>
        /// Gets/Sets ConditionsOfUseUrl
        /// </summary>
        public string ConditionsOfUseUrl
        {
            get
            {
                return this._conditionsOfUseUrl;
            }
            set
            {
                if ( this._conditionsOfUseUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._conditionsOfUseUrl = value;
                    HasChanged = true;
                }
            }
        }
        private string _conditionsOfUseIconUrl;
        /// <summary>
        /// Gets/Sets ConditionsOfUse
        /// </summary>
        public string ConditionsOfUseIconUrl
        {
            get
            {
                return this._conditionsOfUseIconUrl;
            }
            set
            {
                if ( this._conditionsOfUseIconUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._conditionsOfUseIconUrl = value;
                    HasChanged = true;
                }
            }
        }
        private string _author = "";
        /// <summary>
        /// Gets/Sets Author
        /// </summary>
        public string Author
        {
            get
            {
                return this._author;
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
                }
            }
        }

        private string _approvedBy = "";
        /// <summary>
        /// Gets/Sets ApprovedBy
        /// </summary>
        public string ApprovedBy
        {
            get
            {
                return this._approvedBy;
            }
            set
            {
                if ( this._approvedBy == value )
                {
                    //Ignore set
                }
                else
                {
                    this._approvedBy = value.Trim();
                    HasChanged = true;
                }
            }
        }

        #endregion

        #region Helpers
        public DocumentVersion RelatedDocument { get; set; }
                
        /// <summary>
        /// Returns true if content is 'owned' by the org
        /// indicated by the presence of OrgId, otherwise latter is null
        /// </summary>
        /// <returns></returns>
        public bool IsOrgContent()
        {
            if ( IsOrgContentOwner )
                return true;
            else
                return false;
        }
        public bool HasOrg()
        {
            if ( ContentOrg != null && ContentOrg.Id > 0 )
                return true;
            else
                return false;
        }
        private Organization _org;
        /// <summary>
        /// Gets/Sets ContentOrg
        /// </summary>
        public Organization ContentOrg
        {
            get
            {
                return this._org;
            }
            set
            {
                this._org = value;
            }
        }

        /// <summary>
        ///  Get/Set ParentOrgId
        /// </summary>
        private int _parentOrgId = 0;
        public int ParentOrgId
        {
            get
            {
                return _parentOrgId;
            }
            set
            {
                this._parentOrgId = value;
            }
        } //

        private string _parentOrganization = "";
        /// <summary>
        /// Gets/Sets ParentOrganization
        /// </summary>
        public string ParentOrganization
        {
            get
            {
                return this._parentOrganization;
            }
            set
            {
                this._parentOrganization = value.Trim();
            }
        }

       
        #endregion

    } // end class 
} // end Namespace 

