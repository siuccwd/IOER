using System;
using System.Collections.Generic;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Represents an object that describes a ResourceVersion
    /// Notes:
    /// - The Created (date) property is in the base class. For an imported ResourceVersion, it is the date the document was created in the LR
    /// </summary>
    [Serializable]
	public class ResourceVersion : BaseBusinessDataEntity
    {

        #region Properties
        //private Guid _resourceId;
        //public Guid ResourceId
        //{
        //    get { return this._resourceId; }
        //    set { this._resourceId = value; }
        //}
        //public string ResourceIdString
        //{
        //    get { return this._resourceId.ToString(); }
        //}

        private int _resourceIntId;
        public int ResourceIntId
        {
            get { return this._resourceIntId; }
            set { this._resourceIntId = value; }
        }
        //public Guid ResourceVersionId
        //{
        //    get { return this.RowId; }
        //    set { this.RowId = value; }
        //}

        private string _title = "";
		public string Title
		{
			get { return this._title; }
			set { this._title = value; }
		}

        private string _description = "";
		public string Description
		{
			get { return this._description; }
			set { this._description = value; }
		}

        private string _lRDocId;
        public string LRDocId
        {
            get { return this._lRDocId; }
            set { this._lRDocId = value; }
        }
        public string DocId
        {
            get { return this._lRDocId; }
            set { this._lRDocId = value; }
        }
        private string _publisher = "";
		public string Publisher
		{
			get { return this._publisher; }
			set { this._publisher = value; }
		}

        private string _creator = "";
        public string Creator
        {
            get { return this._creator; }
            set { this._creator = value; }
        }

        private string _rights = "";
        public string Rights
        {
            get { return this._rights; }
            set { this._rights = value; }
        }

        public int AccessRightsId { get; set; }
        private string _accessRights = "";
		public string AccessRights
		{
			get { return this._accessRights; }
			set { this._accessRights = value; }
		}

        public int InteractivityTypeId { get; set; }
        private string _interactivityType = "";
        public string InteractivityType
        {
            get { return this._interactivityType; }
            set { this._interactivityType = value; }
        }

        /// <summary>
        /// Modified is typically the date a resource was modified in the Learning Registry.
        /// LastUpdateDate (in the base class) would the date a record was updated within the application
        /// </summary>
		public DateTime Modified
		{
			get { return this.LastUpdated; }
            set { this.LastUpdated = value; }
		}
        private string _submitter = "";
        public string Submitter
        {
            get { return this._submitter; }
            set { this._submitter = value; }
        }

        public DateTime Imported
        {
            get { return this.Created; }
            set { this.Created = value; }
        }

        private string _typicalLearningTime = "";
        public string TypicalLearningTime
        {
            get { return this._typicalLearningTime; }
            set { this._typicalLearningTime = value; }
        }
        private bool _isSkeletonFromParadata;
        public bool IsSkeletonFromParadata
        {
            get { return this._isSkeletonFromParadata; }
            set { this._isSkeletonFromParadata = value; }
        }

        private string _requirements = "";
        public string Requirements
        {
            get { return this._requirements; }
            set { this._requirements = value; }
        }

        private string _sortTitle = "";
        public string SortTitle
        {
            get { return this._sortTitle; }
            set { this._sortTitle = value; }
        }

        private string _schema = "";
        public string Schema
        {
            get { return this._schema; }
            set { this._schema = value; }
        }

      
        #endregion

        #region Helper Properties

        private string _resourceUrl;
        /// <summary>
        /// Get the parent resource Url
        /// </summary>
        public string ResourceUrl
        {
            get { return this._resourceUrl; }
            set { this._resourceUrl = value; }
        }
        private bool _resourceIsActive;
        /// <summary>
        /// Get whether resource is active
        /// </summary>
        public bool ResourceIsActive
        {
            get { return this._resourceIsActive; }
            set { this._resourceIsActive = value; }
        }

        private string _subjects = "";
        public string Subjects
        {
            get { return this._subjects; }
            set { this._subjects = value; }
        }

        private string _educationLevels = "";
        public string EducationLevels
        {
            get { return this._educationLevels; }
            set { this._educationLevels = value; }
        }

        private string _keywords = "";
        public string Keywords
        {
            get { return this._keywords; }
            set { this._keywords = value; }
        }
        private string _languageList = "";
        public string LanguageList
        {
            get { return this._languageList; }
            set { this._languageList = value; }
        }

        private string _audienceList = "";
        public string AudienceList
        {
            get { return this._audienceList; }
            set { this._audienceList = value; }
        }

        //public string ResourceTypesList { get; set; }
        private string _resourceTypesList = "";
        public string ResourceTypesList
        {
            get 
            {
                if ( _resourceTypesList != null && _resourceTypesList.Length > 0 )
                {
                    if ( _resourceTypesList.ToLower().IndexOf( "<li>" ) > -1 )
                        return "<ul>" + _resourceTypesList + "</ul>";
                    else
                        return _resourceTypesList ;
                }
                else
                {
                    return "";
                }
            }
            set { this._resourceTypesList = value; }
        }
        #endregion

        #region Behaviours

        public string GetResourceTypesList()
        {
            if ( ResourceTypesList != null && ResourceTypesList.Length > 0 )
            {
                return "<ul>" + ResourceTypesList + "</ul>";
            }
            else
            {
                return "";
            }
        }
        #endregion
        
    }
}
