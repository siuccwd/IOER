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
            RowId = Guid.NewGuid();
            UsingContentStandards = true;
            ChildItems = new List<ContentItem>();
            ContentStandards = new List<Content_StandardSummary>();
            ContentChildrenStandards = new List<Content_StandardSummary>();

            Standards = new List<ContentResourceStandard>();
            ChildrenStandards = new List<ContentResourceStandard>();
            DocumentPrivacyMessage = "";
            Timeframe = "";
            DocumentUrl = "";
            AutoPreviewUrl = "";
            FileName = "";
            FilePath = "";
            DocumentPath = "";
            //NodeResources = new List<NodeResource>();
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
        public static int EXTERNAL_URL_CONTENT_ID = 41;

        public static int CURRICULUM_CONTENT_ID = 50;
        public static int MODULE_CONTENT_ID = 52;
        public static int UNIT_CONTENT_ID = 54;
        public static int LESSON_CONTENT_ID = 56;
        public static int ACTIVITY_CONTENT_ID = 58;
        public static int ASSESSMENT_CONTENT_ID = 59;

        public static int FEATURED_CONTENT_SORT_ORDER = -1;

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
        public static int MY_ORG_AND_STUDENTS_PRIVILEGE = 5;
        public static int ISLE_ORG_MEMBER_PRIVILEGE = 6;

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
        private string _contentType = "";
        /// <summary>
        /// Gets/Sets ContentType
        /// </summary>
        public string ContentType
        {
            get
            {
                if ( _contentType.Length > 0 )
                    return this._contentType;
                else
                {
                    if ( TypeId == 0 )
                    {
                        return "";
                    }
                    else
                    {
                        if ( TypeId == 59 ) return "Assessment";
                        else if ( TypeId == 58 ) return "Activity";
                        else if ( TypeId == 56 ) return "Lesson";
                        else if ( TypeId == 54 ) return "Unit";
                        else if ( TypeId == 52 ) return "Module";
                        else if ( TypeId == 50 ) return "Curriculum";
                        else if ( TypeId == 40 ) return "Document";
                        else if ( TypeId == 30 ) return "Lesson Plan";
                        else if ( TypeId == 15 ) return "Template";
                        else if ( TypeId == 10 ) return "Content";
                        else return "Content";
                    }
                }
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

        public int ParentId { get; set; }
        public int SortOrder { get; set; }

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
                if ( StatusId > 0 && _status == "" )
                    _status = GetStatusTitle( StatusId );
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
        public string GetStatusTitle( int statusId )
        {
            string title = "";

            if ( statusId == 1 )
                return "Draft";
            else if ( statusId == 2 )
                return "In Progress";
            else if ( statusId == 3 )
                return "Submitted";
            else if ( statusId == 4 )
                return "Revisions Required";
            else if ( statusId == 5 )
                return "Published";
            else if ( statusId == 8 )
                return "Inactive";
            else 
                return "";
        }
        private bool _isPublished = false;
        /// <summary>
        /// Gets/Sets IsPublished
        /// TODO - should probably just depend on the statusId = 5??
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
        private int ResourceVersionId
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
                    //just in case
                    if ( _resourceVersionId == 0 )
                        _resourceIntId = 0;

                }
            }
        }//
        private int _resourceIntId;
        /// <summary>
        /// Gets/Sets the ResourceIntId
        /// </summary>
        public int ResourceIntId
        {
            get
            {
                return this._resourceIntId;
            }
            set
            {
                if ( this._resourceIntId == value )
                {
                    //Ignore set
                }
                else
                {
                    this._resourceIntId = value;
                    HasChanged = true;
                    if ( _resourceIntId == 0 )
                        _resourceVersionId = 0;
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

        public string ImageUrl { get; set; }
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
        public string Timeframe { get; set; }
        #endregion

        #region document properties
        public bool IsDocumentType {
            get 
            {
                if ( TypeId == DOCUMENT_CONTENT_ID )
                    return true;
                else
                    return false;
            }
        }
        public bool IsReferenceUrlType
        {
            get
            {
                if ( TypeId == EXTERNAL_URL_CONTENT_ID )
                    return true;
                else
                    return false;
            }
        }
        public string AutoPreviewUrl { get; set; }

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
        private string _documentUrl = "";
        /// <summary>
        /// Gets/Sets ResourceUrl for the attached document 
        /// - use case where a single document is uploaded 
        /// - should this be from the DocumentVersion??
        /// - should be blank if user doesn't have access
        /// </summary>
        public string DocumentUrl
        {
            get
            {
                return this._documentUrl;
            }
            set
            {
                if ( this._documentUrl == value )
                {
                    //Ignore set
                }
                else
                {
                    this._documentUrl = value.Trim();
                    HasChanged = true;
                }
            }
        }
        /// <summary>
        /// TitleWithResourceUrl - if a resourceId exists,  is used for ???
        /// </summary>
        public string TitleWithResourceUrl { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string DocumentPath { get; set; }
        public string MimeType { get; set; }

        public bool CanViewDocument { get; set; }
        public string DocumentPrivacyMessage { get; set; }
        /// <summary>
        /// Return absolute path to the file
        /// </summary>
        /// <returns></returns>
        public string FileLocation()
        {
            if ( FileName == null || FileName.Trim() == "" )
                return "";
            if ( FilePath == null || FilePath.Trim() == "" )
                return "";

            if ( FilePath.Trim().EndsWith( "\\" ) )
                return FilePath + FileName;
            else
                return FilePath + "\\" + FileName;
        }
        #endregion

        #region helper properties
       
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

        private bool _renumberChildren = false;
        /// <summary>
        /// set to true, if system should renumber children on update
        /// </summary>
        public bool RenumberingChildren
        {
            get
            {
                return _renumberChildren;
            }
            set
            {
                this._renumberChildren = value;
            }
        } //

        /// <summary>
        /// Return true if content item is a hierarchy type
        /// </summary>
        public bool IsHierarchyType
        {
            get
            {
                if ( TypeId >= CURRICULUM_CONTENT_ID && TypeId < ACTIVITY_CONTENT_ID )
                    return true;
                else
                    return false;
            }
        }

        public bool HasChildItems
        {
            get
            {
                if ( ChildItems != null && ChildItems.Count > 0 )
                    return true;
                else 
                    return false;
            }
        }

        /// <summary>
        /// Get/Set list of content child document items
        /// </summary>
        public List<ContentItem> ChildItems { get; set; }


        public bool HasNodeResources
        {
            get
            {
                if ( NodeResources != null && NodeResources.Count > 0 )
                    return true;
                else
                    return false;
            }
        }

        public List<NodeResource> NodeResources { get; set; }
        #endregion

        #region === standards related ===
        /// <summary>
        /// in transition - originally was showing resource.standards. However in curriculum view, will (transition) show content.standards
        /// </summary>
        public bool UsingContentStandards {get;set;}

        /// <summary>
        /// Return true if current item has standards directly associated with it.
        /// </summary>
        public bool HasStandards
        {
            get
            {
                if ( UsingContentStandards )
                {
                    if ( ContentStandards != null && ContentStandards.Count > 0 )
                        return true;
                    else
                        return false;
                }
                else
                {
                    if ( Standards != null && Standards.Count > 0 )
                        return true;
                    else
                        return false;
                }
            }
        }
        public List<Content_StandardSummary> ContentStandards { get; set; }
        public List<Content_StandardSummary> ContentChildrenStandards { get; set; }

        public List<ContentResourceStandard> Standards { get; set; }

        /// <summary>
        /// Return true if content child items have standards
        /// </summary>
        public bool HasChildStandards
        {
            get
            {
                if ( UsingContentStandards )
                {
                    if ( ContentChildrenStandards != null && ContentChildrenStandards.Count > 0 )
                        return true;
                    else
                        return false;
                }
                else
                {
                    if ( ChildrenStandards != null && ChildrenStandards.Count > 0 )
                        return true;
                    else
                        return false;
                }
            }
        }
        public List<ContentResourceStandard> ChildrenStandards { get; set; }
        #endregion

        #region Resource url properties
        /// <summary>
        /// Return true if this record is associated with a Resource record
        /// </summary>
        /// <returns></returns>
        public bool HasResourceId()
        {
            //|| ResourceVersionId > 0
            if ( ResourceIntId > 0  )
                return true;
            else
                return false;
        }
                
        public string ResourceUrl { get; set; }
        public string ResourceFriendlyUrl { get; set; }
        public string ResourceFriendlyTitle { get; set; }
        public string ResourceImageUrl { get; set; }
        public string ResourceThumbnailImageUrl { get; set; }

        #endregion
    } // end class 

    public class NodeResource
    {
        public NodeResource()
        {
        }

        public int Id { get; set; }
        public int ParentId { get; set; }
        public int TypeId { get; set; }
        public int StatusId { get; set; }
        
        public string ContentType { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }

        public Guid DocumentRowId { get; set; }
        public string DocumentUrl { get; set; }
        public string MimeType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string DocumentPath { get; set; }
    }
} // end Namespace 

