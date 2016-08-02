using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace Isle.BizServices
{
    public class ImportServices : Isle.BusinessServices.BaseService
    {
        private ProhibitedKeywordManager prohibitedKeywordManager;

        private ResourceAccessibilityApiManager accessibilityApiManager;
        private ResourceAccessibilityControlManager accessibilityControlManager;
        private ResourceAccessibilityFeatureManager accessibilityFeatureManager;
        private ResourceAccessibilityHazardManager accessibilityHazardManager;
        
        private ResourceAgeRangeManager ageRangeManager;
        private ResourceAssessmentTypeManager assessmentTypeManager;
        private ResourceClusterManager clusterManager;
        private ResourceCommentManager commentManager;

        private ResourceEducationUseManager edUseManager;
        private ResourceFormatManager formatManager;
        private ResourceGradeLevelManager gradeLevelManager;
        private ResourceGroupTypeManager groupTypeManager;
        private ResourceIntendedAudienceManager audienceManager;
        private ResourceItemTypeManager itemTypeManager;
        
        private ResourceKeywordManager keywordManager;
        private ResourceLanguageManager languageManager;
        private ResourceStandardManager standardManager;
        private StandardDataManager standardDataManager;
        private ResourceSubjectManager subjectManager;
        private ResourceTypeManager typeManager;
        private ResourceVersionManager versionManager;
        private ResourceViewManager viewManager;
        private ResourceManager resourceManager;

        private ResourceV2Services searchManager;
        private SystemProcessManager systemProcessManager;

        private bool useV1Procs;
        private bool useV2Procs;

        public ImportServices()
        {
            useV1Procs = GetAppKeyValue("useV1Procs", true);
            useV2Procs = GetAppKeyValue("useV2Procs", false);
            if (useV1Procs == false && useV2Procs == false)
                throw new System.Configuration.ConfigurationErrorsException("useV1Procs and useV2Procs cannot both be false.  At least one must be true or no saving of data will occur!");

            prohibitedKeywordManager = new ProhibitedKeywordManager();

            accessibilityApiManager = new ResourceAccessibilityApiManager();
            accessibilityControlManager = new ResourceAccessibilityControlManager();
            accessibilityFeatureManager = new ResourceAccessibilityFeatureManager();
            accessibilityHazardManager = new ResourceAccessibilityHazardManager();

            ageRangeManager = new ResourceAgeRangeManager();
            assessmentTypeManager = new ResourceAssessmentTypeManager();
            clusterManager = new ResourceClusterManager();
            commentManager = new ResourceCommentManager();

            edUseManager = new ResourceEducationUseManager();
            formatManager = new ResourceFormatManager();
            gradeLevelManager = new ResourceGradeLevelManager();
            groupTypeManager = new ResourceGroupTypeManager();
            audienceManager = new ResourceIntendedAudienceManager();
            itemTypeManager = new ResourceItemTypeManager();

            keywordManager = new ResourceKeywordManager();
            languageManager = new ResourceLanguageManager();
            standardManager = new ResourceStandardManager();
            standardDataManager = new StandardDataManager();
            subjectManager = new ResourceSubjectManager();
            typeManager = new ResourceTypeManager();
            versionManager = new ResourceVersionManager();
            viewManager = new ResourceViewManager();
            resourceManager = new ResourceManager();

            searchManager = new ResourceV2Services();
            systemProcessManager = new SystemProcessManager();
        }

        #region ====== Prohibited Keyword Manager ======
        public List<ProhibitedKeyword> GetProhibitedKeywordRules(ref string status)
        {
            List<ProhibitedKeyword> keywordRules = prohibitedKeywordManager.GetProhibitedKeywordRules(ref status);

            return keywordRules;
        }
        #endregion

        #region ====== AccessibilityAPI methods ======
        public string ImportAccessibilityApi(ResourceChildItem entity)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = accessibilityApiManager.Import(entity);
            if (useV2Procs)
            {
                retVal = accessibilityApiManager.ImportV2(entity);
            }

            return retVal;
        }

        public List<ResourceChildItem> SelectAccessibilityApi(int resourceIntId, int codeId)
        {
            string status = "";
            return accessibilityApiManager.Select(resourceIntId, codeId, ref status);
        }
        #endregion

        #region ====== AccessibilityControls methods ======
        public string ImportAccessibilityControl(ResourceChildItem entity)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = accessibilityControlManager.Import(entity);
            if (useV2Procs)
                retVal = accessibilityControlManager.ImportV2(entity);

            return retVal;
        }

        public List<ResourceChildItem> SelectAccessibilityControl(int resourceIntId, int codeId)
        {
            string status = "";
            return accessibilityControlManager.Select(resourceIntId, codeId, ref status);
        }
        #endregion


        #region ====== AccessibilityFeature methods ======
        public string ImportAccessibilityFeature(ResourceChildItem feature)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = accessibilityFeatureManager.Import(feature);
            if (useV2Procs)
                retVal = accessibilityFeatureManager.ImportV2(feature);

            return retVal;
        }

        public List<ResourceChildItem> SelectAccessibilityFeature(int resourceIntId, int codeId)
        {
            string status = "";
            return accessibilityFeatureManager.Select(resourceIntId, codeId, ref status);
        }
        #endregion

        #region ====== AccessibilityHazard methods ======
        public string ImportAccessibilityHazard(ResourceAccessibilityHazard accessibilityHazard)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = accessibilityHazardManager.Import(accessibilityHazard);
            if (useV2Procs)
                retVal = accessibilityHazardManager.ImportV2(accessibilityHazard);

            return retVal;
        }

        public DataSet GetAccessibilityHazardCodes()
        {
            return CodeTableManager.GetAccessibilityHazardCodes();
        }

        public List<ResourceAccessibilityHazard> GetAccessibilityHazard(int resourceIntId, int codeId)
        {
            string status = "";
            return accessibilityHazardManager.Select(resourceIntId, 0, ref status);
        }
        #endregion

        #region ====== AgeRangeManager methods ======
        public string ImportAgeRange(ResourceAgeRange range)
        {
            return ageRangeManager.Import(range);
        }

        public ResourceAgeRange GetAgeRangeByIntId(int resourceIntId)
        {
            return ageRangeManager.GetByIntId(resourceIntId);
        }
        #endregion

        #region ====== AssessmentTypeManager methods ======
        public string ImportAssessmentType(ResourceChildItem asmtType)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = assessmentTypeManager.Import(asmtType);
            if(useV2Procs)
                retVal = assessmentTypeManager.ImportV2(asmtType);

            return retVal;
        }

        public DataSet GetAssessmentTypes(int resourceIntId)
        {
            return assessmentTypeManager.Select(resourceIntId);
        }
        #endregion

        #region ====== AudienceManager methods ======
        public string CreateAudience(ResourceChildItem audience, ref string status)
        {
            return audienceManager.CreateFromEntity(audience, ref status);
        }

        public List<ResourceChildItem> SelectAudience(int resourceIntId)
        {
            List<ResourceChildItem> retVal = new List<ResourceChildItem>();
            if(useV1Procs)
                retVal = audienceManager.SelectCollection(resourceIntId);
            if (useV2Procs)
                retVal = audienceManager.SelectCollectionV2(resourceIntId);

            return retVal;
        }

        public int ImportAudience(ResourceChildItem entity, ref string status)
        {
            int retVal = -1;
            if(useV1Procs)
                retVal = audienceManager.Import(entity, ref status);
            if (useV2Procs)
            {
                retVal = audienceManager.ImportV2(entity, ref status);
            }

            return retVal;
        }
        #endregion

        #region ====== BaseDataManager methods ======
        public static void LogError(string message)
        {
            BaseDataManager.LogError(message);
        }

        public static void DoTrace(int level, string message)
        {
            BaseDataManager.DoTrace(level, message);
        }

        public static string DEFAULT_GUID
        {
            get { return BaseDataManager.DEFAULT_GUID; }
        }

        public DateTime ConvertLRTimeToDateTime(string timeToConvert)
        {
            return BaseDataManager.ConvertLRTimeToDateTime(timeToConvert);
        }
        #endregion

        #region ====== CodeTableManager methods ======
        public CodeGradeLevelCollection GradeLevelGetByAgeRange(int fromAge, int toAge, bool isEducationBand, ref string status)
        {
            return CodeTableManager.GradeLevelGetByAgeRange(fromAge, toAge, isEducationBand, ref status);
        }
        public CodeGradeLevel GradeLevelGetByTitle(string title)
        {
            return CodeTableManager.GradeLevelGetByTitle(title);
        }
        #endregion

        #region ====== CommentManager methods ======
        public string ImportComment(ResourceComment comment)
        {
            return new ResourceCommentManager().Import(comment);
        }

        public DataSet SelectComment(int resourceIntId)
        {
            return new ResourceCommentManager().Select(resourceIntId);
        }
        #endregion

        #region ====== ClusterManager methods ======
        public string ImportCluster(ResourceCluster cluster)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = clusterManager.Import(cluster);
            if (useV2Procs)
                retVal = clusterManager.ImportV2(cluster);

            return retVal;
        }

        public DataSet GetClusterByIntId(int resourceIntId)
        {
            return clusterManager.Select(resourceIntId);
        }
        #endregion

        #region ====== DatabaseManager methods ======
        public string UpdateWarehouseTotals()
        {
            return DatabaseManager.UpdateWarehouseTotals();
        }

        public string UpdatePublisherTotals()
        {
            return DatabaseManager.UpdatePublisherTotals();
        }
        #endregion

        #region ====== EducationalUse methods ======
        public int ImportEducationalUse(ResourceChildItem edUse, ref string status)
        {
            int retVal = -1;
            if(useV1Procs)
                retVal = edUseManager.Import(edUse, ref status);
            if (useV2Procs)
                retVal = edUseManager.ImportV2(edUse, ref status);

            return retVal;
        }

        public DataSet GetEducationalUseByIntId(int resourceIntId, int educationUseId)
        {
            return edUseManager.Select(resourceIntId, educationUseId);
        }
        #endregion

        #region ====== Format methods ======
        public string ImportFormat(ResourceChildItem format, ref string status)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = formatManager.Import(format, ref status);
            if(useV2Procs)
                retVal = formatManager.ImportV2(format, ref status);

            return retVal;
        }

        public List<ResourceChildItem> SelectFormat(int resourceIntId)
        {
            List<ResourceChildItem> retVal = new List<ResourceChildItem>();
            if(useV1Procs)
                retVal = formatManager.Select(resourceIntId);
            if (useV2Procs)
                retVal = formatManager.SelectV2(resourceIntId);

            return retVal;
        }
        #endregion

        #region ====== GradeLevel methods ======
        public int ImportGradeLevel(ResourceChildItem level, ref string status)
        {
            int retVal = -1;
            if(useV1Procs)
                retVal = gradeLevelManager.Import(level, ref status);
            if (useV2Procs)
                retVal = gradeLevelManager.ImportV2(level, ref status);

            return retVal;
        }

        public List<ResourceChildItem> SelectGradeLevel(int resourceIntId, string originalValue)
        {
            List<ResourceChildItem> retVal = new List<ResourceChildItem>();
            if(useV1Procs)
                retVal = gradeLevelManager.Select(resourceIntId, originalValue);
            if (useV2Procs)
                retVal = gradeLevelManager.Select(resourceIntId, originalValue);

            return retVal;
        }
        #endregion

        #region ====== GroupType methods ======
        public int ImportGroupType(ResourceChildItem entity, ref string status)
        {
            int retVal = -1;
            if(useV1Procs)
                retVal = groupTypeManager.Import(entity, ref status);
            if (useV2Procs)
                retVal = groupTypeManager.ImportV2(entity, ref status);

            return retVal;
        }

        public DataSet SelectGroupType(int resourceIntId)
        {
            return groupTypeManager.Select(resourceIntId);
        }
        #endregion

        #region ====== ItemType methods ======
        public string ImportItemType(ResourceChildItem itemType)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = itemTypeManager.Import(itemType);
            if (useV2Procs)
                retVal = itemTypeManager.ImportV2(itemType);

            return retVal;
        }
        #endregion

        #region ====== KeywordManager methods ======
        public string CreateKeyword(ResourceChildItem keyword, ref string status)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = keywordManager.Create(keyword, ref status);
            if (useV2Procs)
                retVal = keywordManager.CreateV2(keyword, ref status);

            return retVal;
        }

        public List<ResourceChildItem> SelectKeys(int resourceIntId, string originalValue)
        {
            return keywordManager.Select(resourceIntId, originalValue);
        }
        #endregion

        #region ====== LanguageManager methods ======
        public string ImportLanguage(ResourceChildItem language, ref string status)
        {
            string retVal = "";
            if(useV1Procs)
                retVal = languageManager.Import(language, ref status);
            if (useV2Procs)
                retVal = languageManager.ImportV2(language, ref status);

            return retVal;
        }

        public List<ResourceChildItem> SelectLanguage(int resourceIntId, string language)
        {
            List<ResourceChildItem> retVal = new List<ResourceChildItem>();
            if(useV1Procs)
                retVal = languageManager.Select(resourceIntId, language);
            if (useV2Procs)
                retVal = languageManager.SelectV2(resourceIntId, language);

            return retVal;
        }

        public DataSet SelectLanguageCodeTable()
        {
            DataSet retVal = new DataSet();
            if(useV1Procs)
                retVal = languageManager.SelectCodeTable();
            if (useV2Procs)
                retVal = languageManager.SelectCodeTableV2();

            return retVal;
        }
        #endregion

        #region ====== LikeManager methods ======
        public List<ResourceLike> LikeSelect(int resourceIntId, DateTime startDate, DateTime endDate, string isLike, ref string status)
        {
            ResourceLikeManager likeManager = new ResourceLikeManager();
            List<ResourceLike> likes = new List<ResourceLike>();
            DataSet ds = likeManager.Select(resourceIntId, startDate, endDate, isLike, ref status);
            if (DatabaseManager.DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ResourceLike entity = likeManager.Fill(dr);
                    likes.Add(entity);
                }
            }

            return likes;
        }

        public int LikeImport(ResourceLike entity, ref string status)
        {
            return new ResourceLikeManager().Create(entity, ref status);
        }
        #endregion

        #region ====== LikeSummary methods ======
        public ResourceLikeSummary LikeSummaryGet(int resourceIntId, ref string status)
        {
            return new ResourceLikeSummaryManager().Get(resourceIntId, ref status);
        }

        public int LikeSummaryCreate(ResourceLikeSummary entity, ref string status)
        {
            return new ResourceLikeSummaryManager().Create(entity, ref status);
        }

        public void LikeSummaryUpdate(ResourceLikeSummary entity, ref string status) {
            new ResourceLikeSummaryManager().Update(entity, ref status);
        }
        #endregion

        #region ====== RatingSummary methods ======
        public RatingSummary RatingSummaryGet(string resourceId, int ratingTypeId, string type, string identifier)
        {
            return new RatingSummaryManager().Get(resourceId, ratingTypeId, type, identifier);
        }

        public string RatingSummaryCreate(RatingSummary entity)
        {
            return new RatingSummaryManager().Create(entity);
        }

        public string RatingSummaryUpdate(RatingSummary entity)
        {
            return new RatingSummaryManager().Update(entity);
        }
        #endregion

        #region ====== RatingType methods ======
        public RatingType RatingTypeGet(int id, string type, string identifier)
        {
            return new RatingTypeManager().Get(id, type, identifier);
        }

        public int RatingTypeCreate(RatingType entity, ref string status)
        {
            return new RatingTypeManager().Create(entity, ref status);
        }
        #endregion

        #region ====== RelatedUrl methods ======
        public List<ResourceChildItem> SelectRelatedUrls(int resourceIntId)
        {
            ResourceRelatedUrlManager urlManager = new ResourceRelatedUrlManager();
            List<ResourceChildItem> entities = new List<ResourceChildItem>();
            DataSet ds = urlManager.Select(resourceIntId);
            if (DatabaseManager.DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    ResourceChildItem entity = urlManager.Fill(dr);
                    entities.Add(entity);
                }
            }

            return entities;
        }

        public int CreateRelatedUrl(ResourceChildItem entity, ref string status)
        {
            return new ResourceRelatedUrlManager().Create(entity, ref status);
        }
        #endregion

        #region ====== ResourceManager methods ======
        public Resource GetByResourceUrl(string resourceUrl, ref string status)
        {
            return resourceManager.GetByResourceUrl(resourceUrl, ref status);
        }

        public string ResourceUpdateById(Resource entity)
        {
            return resourceManager.UpdateById(entity);
        }

        public string ResourceSetActiveState(int resourceIntId, bool isActive)
        {
            return resourceManager.SetResourceActiveState(resourceIntId, isActive);
        }

        public int CreateResource(Resource resource, ref string status)
        {
            return resourceManager.Create(resource, ref status);
        }

        public string DeleteResource(string rowId)
        {
            return resourceManager.Delete(rowId);
        }

        public string UpdateResource(Resource resource)
        {
            return resourceManager.UpdateById(resource);
        }

        public ResourceCollection SelectResourceCollection(string filter, ref string status)
        {
            return resourceManager.SelectCollection(filter, ref status);
        }

        public Resource GetResourceByIntId(int resourceIntId, ref string status)
        {
            string filter = string.Format("Id = {0}", resourceIntId);
            status = "successful";
            DataSet ds = resourceManager.Select(filter, ref status);
            if (ResourceManager.DoesDataSetHaveRows(ds))
            {
                return resourceManager.Fill(ds.Tables[0].Rows[0]);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region ====== ResourceStandardEvaluation methods ======
        //TODO: Put stuff here
        #endregion

        #region ====== StandardDataManager methods ======

        public StandardItem StandardItem_Get(int id)
        {
            return StandardItem_Get(id, "", "", "");
        }

        public StandardItem StandardItem_Get(int id, string standardUrl, string notationCode, string altUrl)
        {
            return standardDataManager.StandardItem_Get(id, standardUrl, notationCode, altUrl);
        }
        #endregion

        #region ====== StandardManager methods ======
        public string ImportStandard(ResourceStandard standard, ref string status)
        {
            return standardManager.Import(standard, ref status);
        }

        public ResourceStandardCollection SelectStandards(int resourceIntId)
        {
            return standardManager.Select(resourceIntId);
        }
        #endregion

        #region ====== SubjectManager methods ======
        public string CreateSubject(ResourceSubject subject)
        {
            return subjectManager.Create(subject);
        }

        public List<ResourceChildItem> SelectSubject(int resourceIntId)
        {
            return subjectManager.Select(resourceIntId);
        }
        #endregion

        #region ====== TypeManager methods ======
        public string ImportType(ResourceChildItem resourceType, ref string status)
        {
            string retVal = "";
            if (useV1Procs)
                retVal = typeManager.Import(resourceType, ref status);
            if (useV2Procs)
                retVal = typeManager.ImportV2(resourceType, ref status);

            return retVal;
        }

        public List<ResourceChildItem> SelectType(int resourceIntId)
        {
            List<ResourceChildItem> retVal = new List<ResourceChildItem>();
            if (useV1Procs)
                retVal = typeManager.Select(resourceIntId);
            if (useV2Procs)
                retVal = typeManager.SelectV2(resourceIntId);

            return retVal;
        }
        #endregion

        #region ====== ResourceV2Services methods ======
        public void ImportRefreshResources(List<int> resourceIdList)
        {
            searchManager.ImportRefreshResources(resourceIdList);
        }
        #endregion

        #region ====== VersionManager methods ======
        public DataSet SelectVersion(string filter)
        {
            return versionManager.Select(filter);
        }

        public DataSet SelectVersion(string filter, int nbrRetries)
        {
            return versionManager.Select(filter, nbrRetries);
        }

        public ResourceVersion FillVersion(DataRow dr, bool includeRelatedData)
        {
            return versionManager.Fill(dr, includeRelatedData);
        }

        public ResourceVersion FillVersion(SqlDataReader dr, bool includeRelatedData)
        {
            return versionManager.Fill(dr, includeRelatedData);
        }

        public int CreateVersion(ResourceVersion version, ref string status)
        {
            return versionManager.Create(version, ref status);
        }

        public string ImportVersion(ResourceVersion version)
        {
            return versionManager.Import(version);
        }


        public bool DeleteVersion(string rowId, ref string status)
        {
            return versionManager.Delete(rowId, ref status);
        }

        public string VersionSetActiveState(bool activeState, int versionId)
        {
            return versionManager.SetActiveState(activeState, versionId);
        }

        public int MapAccessRights(string lrValue, ref string codeValue)
        {
            return versionManager.MapAccessRights(lrValue, ref codeValue);
        }

        public void ResourceVersion_FixRightsId()
        {
            string result = new ResourceVersionManager().ResourceVersion_FixRightsId();
            result = new ResourceV2Services().HandlePendingResourcesToReindex();
        }

        #endregion

        #region ====== ResourceViewManager methods (used for merging resources only in the import) ======
        public DataSet SelectDetailView(int resourceIntId)
        {
            return viewManager.SelectDetailPageView(resourceIntId);
        }

        public string UpdateDetailPageView(int id, int resourceIntId)
        {
            return viewManager.UpdateDetailPageView(id, resourceIntId);
        }

        public DataSet SelectView(int resourceIntId)
        {
            return viewManager.Select(resourceIntId);
        }

        public string UpdateView(int id, int resourceIntId)
        {
            return viewManager.UpdateView(id, resourceIntId);
        }

        #endregion

        #region ====== Helper methods ======
        private string GetAppKeyValue(string keyName)
        {
            return GetAppKeyValue(keyName, "");
        }

        private string GetAppKeyValue(string keyName, string defaultValue)
        {
            string appValue = defaultValue;
            try
            {
                appValue = System.Configuration.ConfigurationManager.AppSettings[keyName];
                if (appValue == null)
                    appValue = defaultValue;
            }
            catch
            {
                appValue = defaultValue;
            }

            return appValue;
        }

        private int GetAppKeyValue(string keyName, int defaultValue)
        {
            int appValue = -1;
            try
            {
                appValue = int.Parse(System.Configuration.ConfigurationManager.AppSettings[keyName]);
            }
            catch
            {
                appValue = defaultValue;
            }

            return appValue;
        }

        private bool GetAppKeyValue(string keyName, bool defaultValue)
        {
            bool appValue = false;
            try
            {
                appValue = bool.Parse(System.Configuration.ConfigurationManager.AppSettings[keyName]);
            }
            catch
            {
                appValue = defaultValue;
            }

            return appValue;
        }
        #endregion
    }
}
