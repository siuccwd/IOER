﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Represents an object that describes a Resource
    /// </summary>
    [Serializable]
	public class Resource : BaseBusinessDataEntity
    {
        public Resource()
        {
        }

        #region === Properties ===
       // public int Id { get; set; }

        private string _resourceUrl = "";
		public string ResourceUrl
		{
			get { return this._resourceUrl; }
			set { this._resourceUrl = value; }
		}
        
        //public bool IsActive { get; set; }
        public int PublishedForOrgId { get; set; }

        private int _viewCount;
        public int ViewCount
        {
            get { return this._viewCount; }
            set { this._viewCount = value; }
        }

        private int _favoriteCount;
        public int FavoriteCount
        {
            get { return this._favoriteCount; }
            set { this._favoriteCount = value; }
        }

        //public System.DateTime Created { get; set; }
        //public int CreatedById { get; set; }
        
        //public System.DateTime LastUpdated { get; set; }

        //public System.Guid RowId { get; set; }

        private bool _hasPathwayGradeLevel = true;
        [ObsoleteAttribute]
        public bool HasPathwayGradeLevel
        {
            get { return this._hasPathwayGradeLevel; }
            set { this._hasPathwayGradeLevel = value; }
        }
        #endregion


        #region external properties - single

        private ResourceVersion _version = new ResourceVersion();
        public ResourceVersion Version
        {
            get { return this._version; }
            set { this._version = value; }
        }

        private List<ResourceChildItem> _interactivityType = new List<ResourceChildItem>();
        public List<ResourceChildItem> InteractivityType
        {
            get { return this._interactivityType; }
            set { this._interactivityType = value; }
        }
        
        #endregion


        #region ResourceTag properties
        private  List<ResourceTag> _resourceTags = new List<ResourceTag>();
        public List<ResourceTag> ResourceTags
        {
            get { return this._resourceTags; }
            set { this._resourceTags = value; }
        }

        public List<ResourceTag> AccessTypeTags
        {
            get { return this._resourceTags; }
            set { this._resourceTags = value; }
        }
        #endregion


        #region resource child properties

        private List<ResourceChildItem> _relatedURL = new List<ResourceChildItem>();
        public List<ResourceChildItem> relatedURL
        {
            get { return this._relatedURL; }
            set { this._relatedURL = value; }
        }

        private List<ResourceChildItem> _educationalUse = new List<ResourceChildItem>();
        public List<ResourceChildItem> EducationalUse
        {
            get { return this._educationalUse; }
            set { this._educationalUse = value; }
        }

        private List<ResourceChildItem> _groupType = new List<ResourceChildItem>();
        public List<ResourceChildItem> GroupType
        {
            get { return this._groupType; }
            set { this._groupType = value; }
        }


        //private ResourcePropertyCollection _property = new ResourcePropertyCollection();
        //public ResourcePropertyCollection Property
        //{
        //    get { return this._property; }
        //    set { this._property = value; }
        //}

        private List<ResourceChildItem> _keyword = new List<ResourceChildItem>();
        public List<ResourceChildItem> Keyword
        {
            get { return this._keyword; }
            set { this._keyword = value; }
        }

        private ResourceSubjectCollection _subject = new ResourceSubjectCollection();
        public ResourceSubjectCollection Subject
        {
            get { return this._subject; }
            set { this._subject = value; }
        }

        private List<ResourceChildItem> _subjectMap = new List<ResourceChildItem>();
        public List<ResourceChildItem> SubjectMap
        {
            get { return this._subjectMap; }
            set { this._subjectMap = value; }
        }

        private List<ResourceChildItem> _educationLevel = new List<ResourceChildItem>();
        public List<ResourceChildItem> Gradelevel
        {
            get { return this._educationLevel; }
            set { this._educationLevel = value; }
        }

        private ResourceStandardCollection _standard = new ResourceStandardCollection();
        public ResourceStandardCollection Standard
        {
            get { return this._standard; }
            set { this._standard = value; }
        }

        private List<ResourceChildItem> _audience = new List<ResourceChildItem>();
        public List<ResourceChildItem> Audience
        {
            get { return this._audience; }
            set { this._audience = value; }
        }

        private List<ResourceChildItem> _resourceType = new List<ResourceChildItem>();
        public List<ResourceChildItem> ResourceType
        {
            get { return this._resourceType; }
            set { this._resourceType = value; }
        }

        private List<ResourceChildItem> _resourceFormat = new List<ResourceChildItem>();
        public List<ResourceChildItem> ResourceFormat
        {
            get { return this._resourceFormat; }
            set { this._resourceFormat = value; }
        }

        private List<ResourceChildItem> _itemType = new List<ResourceChildItem>();
        public List<ResourceChildItem> ItemType
        {
            get { return this._itemType; }
            set { this._itemType = value; }
        }

        private List<ResourceChildItem> _language = new List<ResourceChildItem>();
        public List<ResourceChildItem> Language
        {
            get { return this._language; }
            set { this._language = value; }
        }

        //private CareerClusterCollection _cluster = new CareerClusterCollection();
        //public CareerClusterCollection Cluster
        //{
        //    get { return this._cluster; }
        //    set { this._cluster = value; }
        //}

        private List<ResourceChildItem> _clusterMap = new List<ResourceChildItem>();
        public List<ResourceChildItem> ClusterMap
        {
            get { return this._clusterMap; }
            set { this._clusterMap = value; }
        }


        //Assessment Type should probably be part of the ResourceVersion object
        //However, I'm putting it here to reflect the structure found in the DAL and database
        private ResourceChildItem _assessmentType = new ResourceChildItem();
        public ResourceChildItem AssessmentType
        {
            get { return this._assessmentType; }
            set { this._assessmentType = value; }
        }

        #endregion


    }

    /// <summary>
    /// Used by the import
    /// </summary>
    public class ResourceCollection : List<Resource>
    {

        public ResourceCollection()
        {
        }

        public ResourceCollection( int capacity )
        {
            this.Capacity = capacity;
        }
    }

}
