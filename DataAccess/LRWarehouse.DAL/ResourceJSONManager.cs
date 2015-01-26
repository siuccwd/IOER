using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LRWarehouse.Business;
using System.Data;

namespace LRWarehouse.DAL
{
    public class ResourceJSONManager : BaseDataManager
    {
      #region == new CRS Json methods
      public List<CRSResource> GetCRSResourcesFromDataSet( DataSet ds )
      {
        var output = new List<CRSResource>();
        var evaluations = new ResourceEvaluationManager().GetEvaluationsWithCount( 0 );

        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
        {
          var added = false;
          foreach ( var rating in evaluations )
          {
            if ( rating.intID.ToString() == Get( dr, "intID" ) )
            {
              output.Add( GetCRSResourceFromDataRow( dr, rating.count, rating.score ) );
              added = true;
              break;
            }
          }
          if ( !added )
          {
            output.Add( GetCRSResourceFromDataRow( dr, 0, -1 ) );
          }
        }

        return output;
      }

      public List<CRSResource> GetCRSResourcesFromDataRows( List<DataRow> rows, List<ResourceEvaluationManager.EvaluationResult> evaluations, int flagID, ref List<double> counters )
      {
        var output = new List<CRSResource>();

        foreach ( DataRow dr in rows )
        {
          var added = false;
          foreach ( var rating in evaluations )
          {
            if ( rating.intID.ToString() == Get( dr, "intID" ) )
            {
              output.Add( GetCRSResourceFromDataRow( dr, rating.count, rating.score ) );
              added = true;
              break;
            }
          }
          if ( !added )
          {
            output.Add( GetCRSResourceFromDataRow( dr, 0, -1 ) );
          }

          counters[ flagID ]++;
        }

        return output;
      }

      public List<CRSResource> GetCRSResourcesFromDataRows( List<DataRow> rows, List<ResourceEvaluationManager.EvaluationResult> evaluations )
      {
        var temp = new List<double>() { 0.0 };
        return GetCRSResourcesFromDataRows( rows, evaluations, 0, ref temp );
      }

      public List<CRSResource> GetCRSResourcesFromDataRows( List<DataRow> rows )
      {
        var evaluations = new ResourceEvaluationManager().GetEvaluationsWithCount( 0 );
        return GetCRSResourcesFromDataRows( rows, evaluations );
      }

      public CRSResource GetCRSResourceFromDataRow( DataRow dr, int evaluationCount, double evaluationScore )
      {
        var resource = new CRSResource();
        //Single value fields
        resource.lrDocID = MakeString( Get( dr, "lrDocId" ) );
        resource.created = MakeDate( Get( dr, "created" ) );
        resource.createdByID = MakeInt( Get( dr, "createdByID" ) );
        resource.creator = MakeString( Get( dr, "creator" ) );
        resource.description = MakeString( Get( dr, "description" ) );
        resource.intID = MakeInt( Get( dr, "intID" ) );
        resource.publisher = MakeString( Get( dr, "publisher" ) );
        resource.submitter = MakeString( Get( dr, "submitter" ) );
        resource.timeRequired = MakeString( Get( dr, "timeRequired" ) );
        resource.title = MakeString( Get( dr, "title" ) );
        resource.sortTitle = MakeString( Get( dr, "sortTitle" ) ); //Missing - MP added 14-05-28
        resource.url = MakeString( Get( dr, "url" ) );
        resource.versionID = MakeInt( Get( dr, "versionID" ) );
        resource.timestamp = long.Parse( DateTime.Parse( resource.created ).ToString( "yyyyMMddHHmmss" ) );
        resource.requirements = MakeString( Get( dr, "requirements" ) ); ;

        //Multi value fields and alias fields
        resource.keywords = MakeStringArray( Get( dr, "keywords" ) ).ToList<string>();
        resource.urlParts = MakeURLParts( Get( dr, "url" ) ).ToList<string>();
        resource.libraryIDs = MakeIntArray( Get( dr, "libraryIDs" ) ).ToList<int>();
        resource.collectionIDs = MakeIntArray( Get( dr, "collectionIDs" ) ).ToList<int>();
        resource.standardAliases = MakeStandardParts2( Get( dr, "standardNotations" ) ).ToList<string>();
        resource.gradeLevelAliases = MakeStringArray( Get( dr, "gradeLevelAliases" ) ).ToList<string>();
        resource.isleSectionIDs = MakeIntArray( Get( dr, "targetSiteIDs" ) ).ToList<int>();
        //Guarantee all Resources show in IOER and Central sites
        if ( !resource.isleSectionIDs.Contains( 1 ) ) { resource.isleSectionIDs.Add( 1 ); }
        if ( !resource.isleSectionIDs.Contains( 5 ) ) { resource.isleSectionIDs.Add( 5 ); }

        //Usage Rights
        resource.usageRights.title = MakeString( Get( dr, "usageRights" ) );
        resource.usageRights.id = MakeInt( Get( dr, "usageRightsID" ) );
        resource.usageRights.url = MakeString( Get( dr, "usageRightsURL" ) );
        resource.usageRights.iconURL = MakeString( Get( dr, "usageRightsIconURL" ) );
        resource.usageRights.miniIconURL = MakeString( Get( dr, "usageRightsMiniIconURL" ) );
        resource.usageRights.description = MakeString( Get( dr, "usageRightsDescription" ) );
        
        //Fields and Tags
        resource.accessRights = MakeCRSField( dr, "accessRightsID", "accessRights", "Access Rights" );
        resource.accessibilityAPI = MakeCRSField( dr, "accessibilityApiIDs", "accessibilityApis", "Accessibility API" );
        resource.accessibilityControl = MakeCRSField( dr, "accessibilityControlIDs", "accessibilityControls", "Accessibility Control" );
        resource.accessibilityFeature = MakeCRSField( dr, "accessibilityFeatureIDs", "accessibilityFeatures", "Accessibility Feature" );
        resource.accessibilityHazard = MakeCRSField( dr, "accessibilityHazardIDs", "accessibilityHazard", "Accessibility Hazard" );
        resource.assessmentType = MakeCRSField( dr, "asssessmentTypeID", "assessmentType", "Assessment Type" );
        resource.educationalRole = MakeCRSField( dr, "audienceIDs", "audiences", "End User" );
        resource.careerCluster = MakeCRSField( dr, "clusterIDs", "clusters", "Career Cluster" );
        resource.educationalUse = MakeCRSField( dr, "educationalUseIDs", "educationalUses", "Educational Use" );
        resource.gradeLevel = MakeCRSField( dr, "gradeLevelIDs", "gradeLevels", "Grade Level" );
        resource.itemType = MakeCRSField( dr, "itemTypeIDs", "itemTypes", "Item Type" );
        resource.inLanguage = MakeCRSField( dr, "languageIDs", "languages", "Language" );
        resource.mediaType = MakeCRSField( dr, "mediaTypeIDs", "mediaTypes", "Media Type" );
        resource.learningResourceType = MakeCRSField( dr, "resourceTypeIDs", "resourceTypes", "Resource Type" );
        resource.standards = MakeCRSField( dr, "standardIDs", "standardNotations", "Standards" );
        resource.training = MakeCRSField( dr, "trainingIDs", "training", "Training" );
        resource.disabilityTopic = MakeCRSField( dr, "disabilityTopicIDs", "disabilityTopics", "Disability Topic" );
        resource.jobs = MakeCRSField( dr, "jobIDs", "jobs", "Jobs" );
        resource.networking = MakeCRSField( dr, "networkingIDs", "networking", "Networking" );
        resource.k12Subject = MakeCRSField( dr, "k12SubjectIDs", "k12Subjects", "K-12 Subject" );
        resource.resources = MakeCRSField( dr, "resourceIDs", "resources", "Resources" );
        //resource.workSupportService = MakeCRSField( dr, "workSupportServiceIDs", "workSupportServices", "Work Support Services" );
        resource.wfePartner = MakeCRSField( dr, "wfePartnerIDs", "wfePartners", "Workforce Education Partners" );
        resource.explore = MakeCRSField( dr, "exploreIDs", "explore", "Explore" );
        //resource.workNetSubject = MakeCRSField( dr, "workNetSubjectIDs", "workNetSubjects", "workNet Subject" );
        resource.region = MakeCRSField( dr, "regionIDs", "regions", "Region" );
        resource.targetSite = MakeCRSField( dr, "targetSiteIDs", "targetSites", "Target Site" );
        resource.qualify = MakeCRSField( dr, "qualifyIDs", "qualify", "Qualify" );
        resource.layoffAssistance = MakeCRSField( dr, "layoffAssistIDs", "layoffAssist", "Layoff Assistance" );
        resource.wioaWorks = MakeCRSField( dr, "wioaWorksIDs", "wioaWorks", "WIOA Works" );

        //Paradata
        resource.paradata.views.detail = MakeInt( Get( dr, "detailViews" ) );
        resource.paradata.views.resource = MakeInt( Get( dr, "viewsCount" ) );
        resource.paradata.ratings.likes = MakeInt( Get( dr, "likeCount" ) );
        resource.paradata.ratings.dislikes = MakeInt( Get( dr, "dislikeCount" ) );
        resource.paradata.ratings.score = MakeInt( Get( dr, "likesSummary" ) );
        resource.paradata.comments = MakeInt( Get( dr, "commentsCount" ) );
        resource.paradata.favorites = MakeInt( Get( dr, "favorites" ) );
        resource.paradata.evaluations.count = evaluationCount;
        resource.paradata.evaluations.score = evaluationScore;

        return resource;
      }
      private CRSField MakeCRSField( DataRow dr, string makeIDs, string makeTags, string title )
      {
        return new CRSField() { ids = MakeIntArray( Get( dr, makeIDs ) ).ToList<int>(), tags = MakeStringArray( Get( dr, makeTags ) ).ToList<string>(), title = title };
      }

      //New as of January 2015
      public ResourceJSONV7 GetResourceJSONV7FromDataRow( DataRow dr, int evaluationCount, double evaluationScore )
      {
        var res = new ResourceJSONV7();

        //Single value fields
        res.versionID = MakeInt( Get( dr, "versionID" ) );
        res.intID = MakeInt( Get( dr, "intID" ) );
        res.created = MakeDate( Get( dr, "created" ) );
        res.timestamp = long.Parse( DateTime.Parse( res.created ).ToString( "yyyyMMddHHmmss" ) );
        res.createdByID = MakeInt( Get( dr, "createdByID" ) );
        res.title = MakeString( Get( dr, "title" ) );
        res.sortTitle = MakeString( Get( dr, "sortTitle" ) ); 
        res.description = MakeString( Get( dr, "description" ) );
        res.url = MakeString( Get( dr, "url" ) );
        res.lrDocID = MakeString( Get( dr, "lrDocId" ) );
        res.requirements = MakeString( Get( dr, "requirements" ) );
        res.timeRequired = MakeString( Get( dr, "timeRequired" ) );
        res.creator = MakeString( Get( dr, "creator" ) );
        res.publisher = MakeString( Get( dr, "publisher" ) );
        res.submitter = MakeString( Get( dr, "submitter" ) );

        //Multi value fields and alias fields
        res.keywords = MakeStringArray( Get( dr, "keywords" ) ).ToList<string>();
        res.urlParts = MakeURLParts( Get( dr, "url" ) ).ToList<string>();
        res.standardAliases = MakeStandardParts2( Get( dr, "standardNotations" ) ).ToList<string>();
        res.gradeLevelAliases = MakeStringArray( Get( dr, "gradeLevelAliases" ) ).ToList<string>();
        res.libraryIDs = MakeIntArray( Get( dr, "libraryIDs" ) ).ToList<int>();
        res.collectionIDs = MakeIntArray( Get( dr, "collectionIDs" ) ).ToList<int>();
        res.isleSectionIDs = MakeIntArray( Get( dr, "targetSiteIDs" ) ).ToList<int>();
        //Guarantee all ress show in IOER and Central sites
        if ( !res.isleSectionIDs.Contains( 1 ) ) { res.isleSectionIDs.Add( 1 ); }
        if ( !res.isleSectionIDs.Contains( 5 ) ) { res.isleSectionIDs.Add( 5 ); }

        //Usage Rights
        res.usageRights.title = MakeString( Get( dr, "usageRights" ) );
        res.usageRights.id = MakeInt( Get( dr, "usageRightsID" ) );
        res.usageRights.url = MakeString( Get( dr, "usageRightsURL" ) );
        res.usageRights.iconURL = MakeString( Get( dr, "usageRightsIconURL" ) );
        res.usageRights.miniIconURL = MakeString( Get( dr, "usageRightsMiniIconURL" ) );
        res.usageRights.description = MakeString( Get( dr, "usageRightsDescription" ) );

        //Paradata
        res.paradata.views.detail = MakeInt( Get( dr, "detailViews" ) );
        res.paradata.views.resource = MakeInt( Get( dr, "viewsCount" ) );
        res.paradata.ratings.likes = MakeInt( Get( dr, "likeCount" ) );
        res.paradata.ratings.dislikes = MakeInt( Get( dr, "dislikeCount" ) );
        res.paradata.ratings.score = MakeInt( Get( dr, "likesSummary" ) );
        res.paradata.comments = MakeInt( Get( dr, "commentsCount" ) );
        res.paradata.favorites = MakeInt( Get( dr, "favorites" ) );
        res.paradata.evaluations.count = evaluationCount;
        res.paradata.evaluations.score = evaluationScore;

        //Tag List
        //would like a dynamic way to get all of the appropriate fields

        //Return data
        return res;
      }

      #endregion

      #region data conversion
      public ResourceJSONFlat GetJSONFromResource( Resource entity )
        {
            ResourceJSONFlat resource = new ResourceJSONFlat();

            //Single Value Items
            resource.created = entity.Version.Created.ToShortDateString();
            resource.createdByID = entity.Version.CreatedById;
            resource.creator = entity.Version.Creator;
            resource.description = entity.Version.Description;
            resource.isBasedOnURL = GetStringArrayFromMap( entity.relatedURL ).FirstOrDefault<string>();
            resource.intID = entity.Version.ResourceIntId;
            resource.publisher = entity.Version.Publisher;
            resource.submitter = entity.Version.Submitter;
            resource.timeRequired = entity.Version.TypicalLearningTime;
            resource.title = entity.Version.Title;
            resource.url = entity.Version.ResourceUrl;
            resource.versionID = entity.Version.Id;
            resource.timestamp = long.Parse( entity.Created.ToString( "yyyyMMddHHmmss" ) );

            //Multi Value Items
            resource.keywords = GetStringArrayFromMap( entity.Keyword );
            resource.subjects = GetStringArrayFromMap( entity.SubjectMap );
            resource.urlParts = MakeURLParts( entity.Version.ResourceUrl );

            //Objects
            resource.accessRightsID = entity.Version.AccessRightsId;
            resource.accessRights = entity.Version.AccessRights;
            resource.assessmentTypeID = GetIntArrayFromMap( new List<ResourceChildItem> { entity.AssessmentType } ).FirstOrDefault<int>();
            resource.assessmentType = GetStringArrayFromMap( new List<ResourceChildItem> { entity.AssessmentType } ).FirstOrDefault<string>();
            resource.audienceIDs = GetIntArrayFromMap( entity.Audience );
            resource.audiences = GetStringArrayFromMap( entity.Audience );
            resource.clusterIDs = GetIntArrayFromMap( entity.ClusterMap );
            resource.clusters = GetStringArrayFromMap( entity.ClusterMap );
            resource.educationalUseIDs = GetIntArrayFromMap( entity.EducationalUse );
            resource.educationalUses = GetStringArrayFromMap( entity.EducationalUse );
            resource.gradeLevelIDs = GetIntArrayFromMap( entity.Gradelevel );
            resource.gradeLevels = GetStringArrayFromMap( entity.Gradelevel );
            resource.gradeLevelAliases = GetEducationAliases( entity );
            resource.groupTypeIDs = GetIntArrayFromMap( entity.GroupType );
            resource.groupTypes = GetStringArrayFromMap( entity.GroupType );
            resource.itemTypeIDs = GetIntArrayFromMap( entity.ItemType );
            resource.itemTypes = GetStringArrayFromMap( entity.ItemType );
            resource.languageIDs = GetIntArrayFromMap( entity.Language );
            resource.languages = GetStringArrayFromMap( entity.Language );
            resource.mediaTypeIDs = GetIntArrayFromMap( entity.ResourceFormat );
            resource.mediaTypes = GetStringArrayFromMap( entity.ResourceFormat );
            resource.resourceTypeIDs = GetIntArrayFromMap( entity.ResourceType );
            resource.resourceTypes = GetStringArrayFromMap( entity.ResourceType );
            GetStandardData( entity.Standard, ref resource );
            GetUsageRightsItems( entity.Version.Rights, ref resource );

            //Paradata
            resource.likesSummary = 0;
            resource.evaluationCount = 0;
            resource.evaluationScoreTotal = -1;
            resource.commentsCount = 0;
            resource.favorites = 0;
            resource.resourceViews = 0;
            resource.detailViews = 0;
            resource.viewsCount = 0;
            resource.likeCount = 0;
            resource.dislikeCount = 0;

            return resource;
        }
        public ResourceJSONFlat[] GetJSONFlatByVersionID( int versionID )
        {
            DataSet dsResourceIntID = DatabaseManager.DoQuery( "SELECT TOP 1 [ResourceIntId] FROM [Resource.Version] WHERE [Id] = " + versionID ); //TODO: replace with proc
            int intID = int.Parse( DatabaseManager.GetRowColumn( dsResourceIntID.Tables[ 0 ].Rows[ 0 ], "ResourceIntId" ) );
            return GetJSONFlatByIntID( intID );
        }
        public ResourceJSONFlat[] GetJSONFlatByIntID( int intID )
        {
            // 14-05-28 mp - changed to call method in ElasticSearchManager to ensure only one point of call
            string status = "";
            DataSet ds = new ElasticSearchManager().GetSqlDataForElasticSearchCollection5( intID.ToString(), ref status );

            //System.Data.SqlClient.SqlParameter[] parameters = new System.Data.SqlClient.SqlParameter[ 1 ];
            //parameters[ 0 ] = new System.Data.SqlClient.SqlParameter( "@ResourceIntId", intID );

            //DataSet ds = DatabaseManager.ExecuteProc( "Resource_BuildElasticSearch", parameters );
            return GetJSONArrayFromDataSet( ds );
        }
        public ResourceJSONFlat[] GetJSONArrayFromDataSet( DataSet ds )
        {
            List<ResourceJSONFlat> listJSON = new List<ResourceJSONFlat>();
            if ( DoesDataSetHaveRows( ds ) == true )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    listJSON.Add( GetJSONFlatFromDataRow( dr ) );
                }
            }
            return listJSON.ToArray<ResourceJSONFlat>();
        }
        public ResourceJSONFlat GetJSONFlatFromDataRow( DataRow dr )
        {
            ResourceJSONFlat resource = new ResourceJSONFlat();

            //Single value items
            resource.created = MakeDate( Get( dr, "created" ) );
            resource.createdByID = MakeInt( Get( dr, "createdByID" ) );
            resource.creator = MakeString( Get( dr, "creator" ) );
            resource.description = MakeString( Get( dr, "description" ) );
            resource.isBasedOnURL = MakeString( Get( dr, "isBasedOnURL" ) );
            resource.intID = MakeInt( Get( dr, "intID" ) );
            resource.publisher = MakeString( Get( dr, "publisher" ) );
            resource.submitter = MakeString( Get( dr, "submitter" ) );
            resource.timeRequired = MakeString( Get( dr, "timeRequired" ) );
            resource.title = MakeString( Get( dr, "title" ) );
            resource.url = MakeString( Get( dr, "url" ) );
            resource.versionID = MakeInt( Get( dr, "versionID" ) );
            resource.timestamp = long.Parse( DateTime.Parse( resource.created ).ToString( "yyyyMMddHHmmss" ) );

            //Multi value items
            resource.keywords = MakeStringArray( Get( dr, "keywords" ) );
            resource.subjects = MakeStringArray( Get( dr, "subjects" ) );
            resource.urlParts = MakeURLParts( Get( dr, "url" ) );
            resource.libraryIDs = MakeIntArray( Get( dr, "libraryIDs" ) );
            resource.collectionIDs = MakeIntArray( Get( dr, "collectionIDs" ) );

            //Objects
            resource.accessRightsID = MakeInt( Get( dr, "accessRightsID" ) );
            resource.accessRights = MakeString( Get( dr, "accessRights" ) );
            resource.alignmentTypeIDs = MakeIntArray( Get( dr, "alignmentTypeIDs" ) );
            resource.alignmentTypes = MakeStringArray( Get( dr, "alignmentTypes" ) );
            resource.assessmentTypeID = MakeInt( Get( dr, "assessmentTypeID" ) );
            resource.assessmentType = MakeString( Get( dr, "assessmentType" ) );
            resource.audienceIDs = MakeIntArray( Get( dr, "audienceIDs" ) );
            resource.audiences = MakeStringArray( Get( dr, "audiences" ) );
            resource.clusterIDs = MakeIntArray( Get( dr, "clusterIDs" ) );
            resource.clusters = MakeStringArray( Get( dr, "clusters" ) );
            resource.educationalUseIDs = MakeIntArray( Get( dr, "educationalUseIDs" ) );
            resource.educationalUses = MakeStringArray( Get( dr, "educationalUses" ) );
            resource.gradeLevelIDs = MakeIntArray( Get( dr, "gradeLevelIDs" ) );
            resource.gradeLevels = MakeStringArray( Get( dr, "gradeLevels" ) );
            resource.gradeLevelAliases = MakeStringArray( Get( dr, "gradeLevelAliases" ) );
            if ( resource.gradeLevelIDs.Length == 0 )
            {   //Should handle potential incompatibilities
                resource.gradeLevelIDs = MakeIntArray( Get( dr, "educationLevelIDs" ) );
                resource.gradeLevels = MakeStringArray( Get( dr, "educationLevels" ) );
                resource.gradeLevelAliases = MakeStringArray( Get( dr, "educationLevelAliases" ) );
            }
            resource.groupTypeIDs = MakeIntArray( Get( dr, "groupTypeIDs" ) );
            resource.groupTypes = MakeStringArray( Get( dr, "groupTypes" ) );
            resource.itemTypeIDs = MakeIntArray( Get( dr, "itemTypeIDs" ) );
            resource.itemTypes = MakeStringArray( Get( dr, "itemTypes" ) );
            resource.languageIDs = MakeIntArray( Get( dr, "languageIDs" ) );
            resource.languages = MakeStringArray( Get( dr, "languages" ) );
            resource.mediaTypeIDs = MakeIntArray( Get( dr, "mediaTypeIDs" ) );
            resource.mediaTypes = MakeStringArray( Get( dr, "mediaTypes" ) );
            resource.resourceTypeIDs = MakeIntArray( Get( dr, "resourceTypeIDs" ) );
            resource.resourceTypes = MakeStringArray( Get( dr, "resourceTypes" ) );
            resource.standardIDs = MakeIntArray( Get( dr, "standardIDs" ) );
            resource.standardNotations = MakeStringArray( Get( dr, "standardNotations" ) );
            resource.notationParts = MakeStandardParts( Get( dr, "standardNotations" ) );
            resource.usageRights = MakeString( Get( dr, "usageRights" ) );
            resource.usageRightsID = MakeInt( Get( dr, "usageRightsID" ) );
            resource.usageRightsURL = MakeString( Get( dr, "usageRightsURL" ) );
            resource.usageRightsIconURL = MakeString( Get( dr, "usageRightsIconURL" ) );
            resource.usageRightsMiniIconURL = MakeString( Get( dr, "usageRightsMiniIconURL" ) );

            //Paradata
            resource.likesSummary = MakeInt( Get( dr, "likesSummary" ) );
            resource.evaluationCount = MakeInt( Get( dr, "evaluationCount" ) );
            resource.commentsCount = MakeInt( Get( dr, "commentsCount" ) );
            resource.favorites = MakeInt( Get( dr, "favorites" ) );
            resource.resourceViews = MakeInt( Get( dr, "viewsCount" ) );
            resource.detailViews = MakeInt( Get( dr, "detailViews" ) );
            resource.viewsCount = MakeInt( Get( dr, "viewsCount" ) ); //temporary
            resource.evaluationScoreTotal = -1;
            resource.likeCount = MakeInt(Get(dr, "likeCount"));
            resource.dislikeCount = MakeInt(Get(dr, "dislikeCount"));

            return resource;
        }
        public Resource GetResourceFromResourceJSON( ResourceJSONFlat resource )
        {
            return GetResourceFromResourceJSON( resource, 0 );
        }
        public Resource GetResourceFromResourceJSON( ResourceJSONFlat resource, int createdByID )
        {
            Resource entity = new Resource();

            //Single value items
            entity.Version.Created = BuildDate( resource.created );
            entity.CreatedById = createdByID;
            entity.Version.CreatedById = createdByID;
            entity.Version.Creator = resource.creator;
            entity.Version.Description = resource.description;
            entity.relatedURL = BuildMapCollection( new string[] { resource.isBasedOnURL }, createdByID );
            entity.Id = resource.intID;
            entity.Version.ResourceIntId = resource.intID;
            entity.Version.Publisher = resource.publisher;
            entity.Version.Submitter = resource.submitter;
            entity.Version.TypicalLearningTime = resource.timeRequired;
            entity.Version.Title = resource.title;
            entity.ResourceUrl = resource.url;
            entity.Version.ResourceUrl = resource.url;
            entity.Version.Rights = resource.usageRightsURL;
            entity.Version.Id = resource.versionID;

            //Multi value items
            entity.Keyword = BuildMapCollection( resource.keywords, createdByID );
            entity.SubjectMap = BuildMapCollection( resource.subjects, createdByID );

            //Objects
            entity.Version.AccessRights = resource.accessRights;
            entity.Version.AccessRightsId = resource.accessRightsID;
            entity.AssessmentType = (ResourceChildItem)BuildMapCollection( new string[] { resource.assessmentType }, new int[] { resource.assessmentTypeID }, createdByID ).FirstOrDefault<ResourceChildItem>();
            entity.Audience = BuildMapCollection( resource.audiences, resource.audienceIDs, createdByID );
            entity.ClusterMap = BuildMapCollection( resource.clusters, resource.clusterIDs, createdByID );
            entity.EducationalUse = BuildMapCollection( resource.educationalUses, resource.educationalUseIDs, createdByID );
            entity.Gradelevel = BuildMapCollection( resource.gradeLevels, resource.gradeLevelIDs, createdByID );
            entity.GroupType = BuildMapCollection( resource.groupTypes, resource.groupTypeIDs, createdByID );
            entity.ItemType = BuildMapCollection( resource.itemTypes, resource.itemTypeIDs, createdByID );
            entity.Language = BuildMapCollection( resource.languages, resource.languageIDs, createdByID );
            entity.ResourceFormat = BuildMapCollection( resource.mediaTypes, resource.mediaTypeIDs, createdByID );
            entity.ResourceType = BuildMapCollection( resource.resourceTypes, resource.resourceTypeIDs, createdByID );
            entity.Standard = BuildStandardCollection( resource.standardNotations, resource.standardIDs, resource.alignmentTypes, resource.alignmentTypeIDs, createdByID );

            //Paradata
            // N/A

            return entity;
        }
        public ResourceJSONFlat GetJSONFlatFromJSONObject( ResourceJSONObject json )
        {
            ResourceJSONFlat resource = new ResourceJSONFlat();

            //Single value items
            resource.created = json.created;
            resource.createdByID = json.createdByID;
            resource.creator = json.creator;
            resource.description = json.description;
            resource.isBasedOnURL = json.isBasedOnURL;
            resource.intID = json.intID;
            resource.publisher = json.publisher;
            resource.submitter = json.submitter;
            resource.timeRequired = json.timeRequired;
            resource.title = json.title;
            resource.url = json.url;
            resource.usageRights = json.usageRights.usageRightsURL;
            resource.usageRightsID = json.usageRights.ID;
            resource.usageRights = json.usageRights.name;
            resource.usageRightsIconURL = json.usageRights.usageRightsIconURL;
            resource.usageRightsMiniIconURL = json.usageRights.usageRightsMiniIconURL;
            resource.versionID = json.versionID;
            resource.timestamp = json.timestamp;

            //Multi value items
            resource.keywords = json.keywords;
            resource.subjects = json.subjects;
            resource.urlParts = json.urlParts;
            resource.libraryIDs = json.libraryIDs;
            resource.collectionIDs = json.collectionIDs;

            //Objects
            resource.accessRightsID = json.accessRights.ID;
            resource.accessRights = json.accessRights.name;
            resource.assessmentTypeID = json.assessmentType.ID;
            resource.assessmentType = json.assessmentType.name;
            ConvertJSON( ref resource.audienceIDs, ref resource.audiences, json.audiences );
            ConvertJSON( ref resource.clusterIDs, ref resource.clusters, json.clusters );
            ConvertJSON( ref resource.educationalUseIDs, ref resource.educationalUses, json.educationalUses );
            ConvertJSONGrades( ref resource.gradeLevelIDs, ref resource.gradeLevels, ref resource.gradeLevelAliases, json.gradeLevels );
            ConvertJSON( ref resource.groupTypeIDs, ref resource.groupTypes, json.groupTypes );
            ConvertJSON( ref resource.itemTypeIDs, ref resource.itemTypes, json.itemTypes );
            ConvertJSON( ref resource.languageIDs, ref resource.languages, json.languages );
            ConvertJSON( ref resource.mediaTypeIDs, ref resource.mediaTypes, json.mediaTypes );
            ConvertJSON( ref resource.resourceTypeIDs, ref resource.resourceTypes, json.resourceTypes );
            ConvertJSONStandards( ref resource.standardIDs, ref resource.standardNotations, ref resource.standardNotations, ref resource.alignmentTypeIDs, ref resource.alignmentTypes, json.standards );

            //Paradata
            resource.likesSummary = json.likesSummary;
            resource.evaluationCount = json.evaluationCount;
            resource.evaluationScoreTotal = json.evaluationScoreTotal;
            resource.commentsCount = json.commentsCount;
            resource.favorites = json.favorites;
            resource.resourceViews = json.resourceViews;
            resource.detailViews = json.detailViews;
            resource.viewsCount = json.viewsCount; //temporary
            resource.likeCount = json.likeCount;
            resource.dislikeCount = json.dislikeCount;

            return resource;

        }
        public ResourceJSONObject GetJSONObjectFromJSONFlat( ResourceJSONFlat json )
        {
            ResourceJSONObject resource = new ResourceJSONObject();

            //Single value items
            resource.created = json.created;
            resource.createdByID = json.createdByID;
            resource.creator = json.creator;
            resource.description = json.description;
            resource.isBasedOnURL = json.isBasedOnURL;
            resource.intID = json.intID;
            resource.publisher = json.publisher;
            resource.submitter = json.submitter;
            resource.timeRequired = json.timeRequired;
            resource.title = json.title;
            resource.url = json.url;
            resource.usageRights.usageRightsURL = json.usageRights;
            resource.usageRights.ID = json.usageRightsID;
            resource.usageRights.name = json.usageRights;
            resource.usageRights.usageRightsIconURL = json.usageRightsIconURL;
            resource.usageRights.usageRightsMiniIconURL = json.usageRightsMiniIconURL;
            resource.versionID = json.versionID;
            resource.timestamp = json.timestamp;

            //Multi value items
            resource.keywords = json.keywords;
            resource.subjects = json.subjects;
            resource.urlParts = json.urlParts;
            resource.libraryIDs = json.libraryIDs;
            resource.collectionIDs = json.collectionIDs;

            //Objects
            resource.accessRights.ID = json.accessRightsID;
            resource.accessRights.name = json.accessRights;
            resource.assessmentType.ID = json.assessmentTypeID;
            resource.assessmentType.name = json.assessmentType;
            ConvertJSON( json.audienceIDs, json.audiences, ref resource.audiences );
            ConvertJSON( json.clusterIDs, json.clusters, ref resource.clusters );
            ConvertJSON( json.educationalUseIDs, json.educationalUses, ref resource.educationalUses );
            ConvertJSONGrades( json.gradeLevelIDs, json.gradeLevels, json.gradeLevelAliases, ref resource.gradeLevels );
            ConvertJSON( json.groupTypeIDs, json.groupTypes, ref resource.groupTypes );
            ConvertJSON( json.itemTypeIDs, json.itemTypes, ref resource.itemTypes );
            ConvertJSON( json.languageIDs, json.languages, ref resource.languages );
            ConvertJSON( json.mediaTypeIDs, json.mediaTypes, ref resource.mediaTypes );
            ConvertJSON( json.resourceTypeIDs, json.resourceTypes, ref resource.resourceTypes );
            ConvertJSONStandards( json.standardIDs, json.standardNotations, json.standardNotations, json.alignmentTypeIDs, json.alignmentTypes, ref resource.standards );

            //Paradata
            resource.likesSummary = json.likesSummary;
            resource.evaluationCount = json.evaluationCount;
            resource.evaluationScoreTotal = json.evaluationScoreTotal;
            resource.commentsCount = json.commentsCount;
            resource.favorites = json.favorites;
            resource.resourceViews = json.resourceViews;
            resource.detailViews = json.detailViews;
            resource.viewsCount = json.viewsCount; //temporary
            resource.likeCount = json.likeCount;
            resource.dislikeCount = json.dislikeCount;

            return resource;

        }
        public ResourceJSONElasticSearch GetJSONElasticSearchFromJSONFlat( ResourceJSONFlat json )
        {
            ResourceJSONElasticSearch search = new ResourceJSONElasticSearch();
            //Single value items
            search.created = json.created;
            search.creator = json.creator;
            search.description = json.description;
            search.intID = json.intID;
            search.publisher = json.publisher;
            search.submitter = json.submitter;
            search.title = json.title;
            search.url = json.url;
            search.versionID = json.versionID;
            search.timestamp = json.timestamp;

            //Multi value items
            search.keywords = json.keywords;
            search.subjects = json.subjects;
            search.urlParts = json.urlParts;
            search.libraryIDs = json.libraryIDs;
            search.collectionIDs = json.collectionIDs;

            //Objects
            search.accessRightsID = json.accessRightsID;
            search.accessRights = json.accessRights;
            search.languageIDs = json.languageIDs;
            search.languages = json.languages;
            search.clusterIDs = json.clusterIDs;
            search.clusters = json.clusters;
            search.audienceIDs = json.audienceIDs;
            search.audiences = json.audiences;
            search.gradeLevelIDs = json.gradeLevelIDs;
            search.gradeLevels = json.gradeLevels;
            search.gradeLevelAliases = json.gradeLevelAliases;
            search.resourceTypeIDs = json.resourceTypeIDs;
            search.resourceTypes = json.resourceTypes;
            search.mediaTypeIDs = json.mediaTypeIDs;
            search.mediaTypes = json.mediaTypes;
            search.educationalUseIDs = json.educationalUseIDs;
            search.educationalUses = json.educationalUses;
            search.groupTypeIDs = json.groupTypeIDs;
            search.groupTypes = json.groupTypes;
            search.standardIDs = json.standardIDs;
            search.standardNotations = json.standardNotations;
            search.notationParts = json.notationParts;
            search.usageRights = json.usageRights;
            search.usageRightsID = json.usageRightsID;
            search.usageRightsURL = json.usageRightsURL;
            search.usageRightsIconURL = json.usageRightsIconURL;
            search.usageRightsMiniIconURL = json.usageRightsMiniIconURL;

            //Paradata
            search.likesSummary = json.likesSummary;
            search.evaluationCount = json.likesSummary;
            search.evaluationScoreTotal = json.evaluationScoreTotal;
            search.commentsCount = json.commentsCount;
            search.favorites = json.favorites;
            search.resourceViews = json.resourceViews;
            search.detailViews = json.detailViews;
            search.viewsCount = json.viewsCount; //temporary
            search.likeCount = json.likeCount;
            search.dislikeCount = json.dislikeCount;

            return search;
        }
        public ResourceJSONLRMI GetJSONLRMIFromResource( Resource entity )
        {
            ResourceJSONLRMI resource = new ResourceJSONLRMI();

            resource.name = entity.Version.Title;
            resource.url = entity.Version.ResourceUrl;
            resource.description = entity.Version.Description;
            resource.author = entity.Version.Creator;
            resource.publisher = entity.Version.Publisher;
            resource.isBasedOnUrl = LRMIAddItemToMetadata( entity.relatedURL )[ 0 ];
            resource.inLanguage = LRMIAddItemToMetadata( entity.Language )[ 0 ];
            resource.dateCreated = entity.Version.Created.ToShortDateString();
            resource.timeRequired = entity.Version.TypicalLearningTime;
            resource.accessRestrictions = entity.Version.AccessRights;
            resource.useRightsUrl = entity.Version.Rights;
            resource.requires = entity.Version.Requirements;
            resource.about = LRMIAddItemToMetadata( entity.SubjectMap );
            resource.educationalAlignment = LRMIAddEducationalAlignment( entity, ref resource );
            resource.careerCluster = LRMIAddCareerClusters( entity );
            resource.educationalRole = LRMIAddItemToMetadata( entity.Audience );
            resource.educationalUse = LRMIAddItemToMetadata( entity.EducationalUse );
            resource.mediaType = LRMIAddItemToMetadata( entity.ResourceFormat );
            resource.groupType = LRMIAddItemToMetadata( entity.ResourceType );
            resource.itemType = LRMIAddItemToMetadata( entity.ItemType );

            return resource;
        }

        #endregion

        #region helper methods


        #region LRMI Metadata JSON helpers
        protected ResourceJSONLRMICareerCluster[] LRMIAddCareerClusters( Resource entity )
        {
            List<ResourceJSONLRMICareerCluster> listItems = new List<ResourceJSONLRMICareerCluster>();

            foreach ( ResourceChildItem clusterMap in entity.ClusterMap )
            {
                ResourceJSONLRMICareerCluster cluster = new ResourceJSONLRMICareerCluster();
                cluster.country = "US";
                cluster.region = "IL";
                cluster.value = clusterMap.OriginalValue;
            }

            return listItems.ToArray<ResourceJSONLRMICareerCluster>();

        }
        protected ResourceJSONLRMIEducationalAlignment[] LRMIAddEducationalAlignment( Resource entity, ref ResourceJSONLRMI resource )
        {
            List<ResourceJSONLRMIEducationalAlignment> listItems = new List<ResourceJSONLRMIEducationalAlignment>();
            List<string> ageRanges = new List<string>();

            foreach ( ResourceStandard standard in entity.Standard )
            {
                ResourceJSONLRMIEducationalAlignment edAlign = new ResourceJSONLRMIEducationalAlignment();
                edAlign.targetName = standard.StandardNotationCode;
                edAlign.alignmentType = standard.AlignmentTypeValue;
                edAlign.targetUrl = standard.StandardUrl;
                listItems.Add( edAlign );
            }

            foreach ( ResourceChildItem grade in entity.Gradelevel )
            {
                ResourceJSONLRMIEducationalAlignment gradeAlign = new ResourceJSONLRMIEducationalAlignment();
                gradeAlign.educationLevel = grade.OriginalValue;
                gradeAlign.educationalFramework = "US P-20";
                gradeAlign.targetName = grade.OriginalValue;
                listItems.Add( gradeAlign );

                DataSet ds = DatabaseManager.DoQuery( "SELECT DISTINCT TOP 1 [AgeLevel] FROM [Codes.GradeLevel] WHERE [Id] = '" + grade.CodeId + "'" ); //TODO: replace with proc
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    ageRanges.Add( DatabaseManager.GetRowPossibleColumn( ds.Tables[ 0 ].Rows[ 0 ], "AgeLevel" ) );
                }
            }

            resource.typicalAgeRange = ageRanges.ToArray<string>();
            return listItems.ToArray<ResourceJSONLRMIEducationalAlignment>();

        }
        protected string[] LRMIAddItemToMetadata( List<ResourceChildItem> collection )
        {
            List<string> listItems = new List<string>();
            foreach ( ResourceChildItem mapItem in collection )
            {
                listItems.Add( mapItem.OriginalValue );
            }
            string[] returnValue = listItems.ToArray<string>();
            if ( returnValue.Length > 0 )
            {
                return returnValue;
            }
            else
            {
                return new string[] { "" }; //ensure that we don't get any array index out of bounds errors
            }
        }
        #endregion

        #region Get from Resource object
        protected string[] GetStringArrayFromMap( List<ResourceChildItem> collection )
        {
            List<string> listItems = new List<string>();
            foreach ( ResourceChildItem map in collection )
            {
                listItems.Add( MakeString( map.OriginalValue ) );
            }
            return listItems.ToArray<string>();
        }
        protected int[] GetIntArrayFromMap( List<ResourceChildItem> collection )
        {
            List<int> listItems = new List<int>();
            foreach ( ResourceChildItem map in collection )
            {
                listItems.Add( MakeInt( map.Id.ToString() ) );
            }
            return listItems.ToArray<int>();
        }
        protected void GetStandardData( ResourceStandardCollection collection, ref ResourceJSONFlat resource )
        {
            List<int> listIDs = new List<int>();
            List<string> listValues = new List<string>();
            List<int> listAlignmentIDs = new List<int>();
            List<string> listAlignments = new List<string>();
            StringBuilder builder = new StringBuilder();
            foreach ( ResourceStandard standard in collection )
            {
                listIDs.Add( MakeInt( standard.StandardId.ToString() ) );
                string notation = MakeString( standard.StandardNotationCode );
                listValues.Add( notation );
                builder.Append( notation + "," );
                listAlignmentIDs.Add( standard.AlignmentTypeCodeId );
                listAlignments.Add( standard.AlignmentTypeValue );
            }
            resource.standardIDs = listIDs.ToArray<int>();
            resource.standardNotations = listValues.ToArray<string>();
            resource.notationParts = MakeStandardParts( builder.ToString() );
            resource.alignmentTypeIDs = listAlignmentIDs.ToArray<int>();
            resource.alignmentTypes = listAlignments.ToArray<string>();

        }
        protected string[] GetEducationAliases( Resource entity )
        {
            List<string> listItems = new List<string>();
            ResourceGradeLevelManager manager = new ResourceGradeLevelManager();
            DataSet ds = manager.SelectedCodes( entity.Id );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    foreach ( ResourceChildItem map in entity.Gradelevel )
                    {
                        string thisID = DatabaseManager.GetRowColumn( dr, "Id" );
                        if ( map.CodeId.ToString() == thisID | map.Id.ToString() == thisID )
                        {
                            string[] stuff = DatabaseManager.GetRowPossibleColumn( dr, "AliasValues" ).Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
                            for ( int i = 0 ; i < stuff.Length ; i++ )
                            {
                                listItems.Add( MakeString( stuff[ i ] ) );
                            }
                        }
                    }
                }
            }
            return listItems.ToArray<string>();
        }
        protected void GetUsageRightsItems( string rights, ref ResourceJSONFlat resource )
        {
            resource.usageRightsURL = rights;
            bool found = false;
            DataSet ds = DatabaseManager.ExecuteProc( "[Resource.ConditionsOfUse_Select]", new System.Data.SqlClient.SqlParameter[] { } );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    if ( DatabaseManager.GetRowColumn( dr, "Url" ) == rights )
                    {
                        found = true;
                        FillUsageRights( dr, ref resource );
                    }
                }
                if ( !found )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        if ( DatabaseManager.GetRowColumn( dr, "Title" ) == "Read the Fine Print" )
                        {
                            FillUsageRights( dr, ref resource );
                        }
                    }
                }
            }
        }
        protected void FillUsageRights( DataRow dr, ref ResourceJSONFlat resource )
        {
            resource.usageRightsID = MakeInt( DatabaseManager.GetRowColumn( dr, "Id" ) );
            resource.usageRights = MakeString( DatabaseManager.GetRowColumn( dr, "Title" ) );
            resource.usageRightsIconURL = MakeString( DatabaseManager.GetRowColumn( dr, "Url" ) );
            resource.usageRightsMiniIconURL = MakeString( DatabaseManager.GetRowColumn( dr, "MiniIconUrl" ) );
        }
        #endregion

        #region Make from Strings (mostly returned from DataSets)
        protected string Get( DataRow dr, string targetColumn )
        {
            return DatabaseManager.GetRowPossibleColumn( dr, targetColumn );
        }
        protected string MakeString( string input )
        {
            string data = input.Replace( "  ", "" )
                .Replace( "NULL", "" )
                .Replace( @"{", "&#123;" )
                .Replace( @"}", "&#125;" )
                .Replace( @"\n\t\t\t\t\t\t\t\t\t", " " )
                .Replace( @"          ", " " )
                .Replace( @"&amp;146;", "'" )
                .Replace( @"&amp;147;", "\"" )
                .Replace( @"&amp;148;", "\"" )
                .Replace( @"&lt;p&gt;", "")
                .Replace( @"&lt;/p&gt;", "")
                .Trim();
            if ( data.Length > 1000 )
            {
                return data.Substring( 0, 997 ) + "...";
            }
            else
            {
                return data;
            }
        }
        protected int MakeInt( string input )
        {
            try
            {
                return int.Parse( input.Replace( "NULL", "" ).Trim() );
            }
            catch
            {
                return 0;
            }
        }
        protected string[] MakeStringArray( string input )
        {
            string[] data = MakeString( input ).Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
            for ( int i = 0 ; i < data.Length ; i++ )
            {
                if ( i < 20 )
                {
                    data[ i ] = data[ i ].Replace( "\"", "" ).Trim();
                }
            }
            return data;
        }
        protected int[] MakeIntArray( string input )
        {
            List<int> items = new List<int>();
            string[] toParse = MakeStringArray( input );
            foreach ( string item in toParse )
            {
                try
                {
                    items.Add( int.Parse( item.Replace( "NULL", "" ).Trim() ) );
                }
                catch
                {
                    items.Add( 0 );
                }
            }
            return items.ToArray<int>();
        }
        protected string[] MakeURLParts( string input )
        {
          try
          {
            string[] parts = input.Replace( "NULL", "" )
                .Replace( "http://", "" )
                .Replace( "https://", "" )
                .Replace( "ftp://", "" )
                .Replace( ".", "/" )
                .Trim()
                .Split( new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries );
            //Unrequired value-added URL things:
            List<string> stuff = parts.ToList<string>();
            try
            {
                string commonNameParts = input.Trim().Split( new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries )[ 1 ];
                stuff.Add( commonNameParts );
            }
            catch { }
            try
            {
                string[] commonDomainParts = input.Trim().Split( new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries )[ 1 ].Split( new string[] { "." }, StringSplitOptions.RemoveEmptyEntries );
                string commonDomain = commonDomainParts[ commonDomainParts.Length - 2 ] + "." + commonDomainParts[ commonDomainParts.Length - 1 ];
                stuff.Add( commonDomain );
            }
            catch { }
            return stuff.ToArray<string>();

          }
          catch {
            return new String[] { "" };
          }
        }
        protected List<string> MakeStandardParts2( string input )
        {
          List<string> output = new List<string>();

          var items = input.Replace( "NULL", "" ).Trim().Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
          foreach ( var item in items )
          {
            var parts = item.Split( new string[] { "." }, StringSplitOptions.RemoveEmptyEntries );
            if ( parts.Length > 1 )
            {
              foreach ( var part in parts )
              {
                output.Add( part );
              }
              var build = parts[ 0 ];
              for ( var i = 1 ; i < parts.Length ; i++ )
              {
                build = build + "." + parts[ i ];
                output.Add( build );
              }
            }
            else
            {
              output.Add( parts[ 0 ] );
            }
          }

          return output;
        }
        protected string[] MakeStandardParts( string input )
        {
            List<string> listFinal = new List<string>();
            string[] searchString = input.Replace( "NULL", "" ).Trim().Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string standard in searchString )
            {
                if ( standard.IndexOf( "CCSS." ) == 0 )
                {
                    string[] parts = standard.Split( new string[] { "." }, StringSplitOptions.RemoveEmptyEntries );
                    listFinal.Add( parts[ 0 ] );
                    listFinal.Add( parts[ 1 ] );
                    listFinal.Add( parts[ 2 ] );
                    listFinal.Add( parts[ 0 ] + "." + parts[ 1 ] );
                    listFinal.Add( parts[ 1 ] + "." + parts[ 2 ] );
                    listFinal.Add( parts[ 0 ] + "." + parts[ 1 ] + "." + parts[ 2 ] );
                    string rest = "";
                    for ( int i = 3 ; i < parts.Length ; i++ )
                    {
                        rest = rest + "." + parts[ i ];
                    }
                    if ( rest.Length > 0 )
                    {
                      rest = rest.Substring( 1 );
                      listFinal.Add( rest );
                    }
                }
                else
                {
                    listFinal.Add( standard );
                }
            }
            List<string> reallyFinal = new List<string>();
            foreach ( string item in listFinal )
            {
                if ( !reallyFinal.Contains( item ) )
                {
                    reallyFinal.Add( item );
                }
            }
            return reallyFinal.ToArray<string>();
        }
        public string MakeDate( string input )
        {
            return input.Replace( " AM", "" ).Replace( " PM", "" );
        }
        #endregion

        #region Build Resource from JSON
        protected DateTime BuildDate( string input )
        {
            try
            {
                return DateTime.Parse( input );
            }
            catch ( Exception ex )
            {
                return DateTime.Now;
            }
        }
        protected List<ResourceChildItem> BuildMapCollection( string[] values, int createdByID )
        {
            int[] fakes = new int[ values.Length ];
            for ( int i = 0 ; i < fakes.Length ; i++ )
            {
                fakes[ i ] = 0;
            }
            return BuildMapCollection( values, fakes, createdByID );
        }
        protected List<ResourceChildItem> BuildMapCollection( string[] values, int[] IDs, int createdByID )
        {
            List<ResourceChildItem> collection = new List<ResourceChildItem>();
            for ( int i = 0 ; i < IDs.Length ; i++ )
            {
                ResourceChildItem map = new ResourceChildItem();
                map.OriginalValue = Assign( values, i );
                map.Id = Assign( IDs, i );
                map.CreatedById = createdByID;
                collection.Add( map );
            }
            return collection;
        }
        protected ResourceStandardCollection BuildStandardCollection( string[] codes, int[] standardIDs, string[] alignments, int[] alignmentIDs, int createdByID )
        {
            ResourceStandardCollection collection = new ResourceStandardCollection();
            for ( int i = 0 ; i < standardIDs.Length ; i++ )
            {
                ResourceStandard standard = new ResourceStandard();
                standard.StandardNotationCode = Assign( codes, i );
                standard.Id = Assign( standardIDs, i );
                standard.StandardId = Assign( standardIDs, i );
                standard.AlignmentTypeValue = Assign( alignments, i );
                standard.AlignmentTypeCodeId = Assign( alignmentIDs, i );
                collection.Add( standard );
            }
            return collection;
        }
        protected string Assign( string[] array, int i )
        {
            try
            {
                return array[ i ] == null ? "" : array[ i ];
            }
            catch ( IndexOutOfRangeException ex )
            {
                return "";
            }
        }
        protected int Assign( int[] array, int i )
        {
            try
            {
                return array[ i ] == null ? 0 : array[ i ];
            }
            catch ( IndexOutOfRangeException ex )
            {
                return 0;
            }
        }
        #endregion

        #region Convert between flat and object JSONs
        protected void ConvertJSON( ref int[] IDs, ref string[] values, ResourceJSONObjectItem[] source )
        {
            IDs = new int[source.Length];
            values = new string[source.Length];
            for ( int i = 0 ; i < source.Length ; i++ )
            {
                IDs[ i ] = source[ i ].ID;
                values[ i ] = source[ i ].name;
            }
        }
        protected void ConvertJSON( int[] IDs, string[] values, ref ResourceJSONObjectItem[] target )
        {
            target = new ResourceJSONObjectItem[ IDs.Length ];
            for ( int i = 0 ; i < IDs.Length ; i++ )
            {
                target[ i ] = new ResourceJSONObjectItem();
                target[ i ].ID = IDs[ i ];
                target[ i ].name = values[ i ];
            }
        }
        protected void ConvertJSONGrades( ref int[] IDs, ref string[] values, ref string[] aliases, ResourceJSONObjectGradeLevel[] grades )
        {
            IDs = new int[ grades.Length ];
            values = new string[ grades.Length ];
            aliases = new string[ grades.Length ];
            List<string> listAliases = new List<string>();
            for ( int i = 0 ; i < grades.Length ; i++ )
            {
                IDs[ i ] = grades[ i ].ID;
                values[ i ] = grades[ i ].name;
                for ( int j = 0 ; j < grades[ i ].alias.Length ; j++ )
                {
                    listAliases.Add( grades[ i ].alias[j] );
                }
            }
            aliases = listAliases.ToArray<string>();
        }
        protected void ConvertJSONGrades( int[] IDs, string[] values, string[] aliases, ref ResourceJSONObjectGradeLevel[] grades )
        {
            grades = new ResourceJSONObjectGradeLevel[ IDs.Length ];
            string[][] aliasesHolder = GetAliases( IDs );
            for ( int i = 0 ; i < IDs.Length ; i++ )
            {
                grades[ i ].ID = IDs[ i ];
                grades[ i ].name = values[ i ];
                grades[ i ].alias = aliasesHolder[ i ];
            }
        }
        protected void ConvertJSONStandards( ref int[] IDs, ref string[] values, ref string[] parts, ref int[] alignmentIDs, ref string[] alignments, ResourceJSONObjectStandard[] standard )
        {
            IDs = new int[ standard.Length ];
            values = new string[ standard.Length ];
            alignmentIDs = new int[ standard.Length ];
            alignments = new string[ standard.Length ];
            List<string> listParts = new List<string>();
            for ( int i = 0 ; i < standard.Length ; i++ )
            {
                IDs[ i ] = standard[ i ].ID;
                values[ i ] = standard[ i ].name;
                for ( int j = 0 ; j < standard[ i ].notationParts.Length ; j++ )
                {
                    listParts.Add( standard[ i ].notationParts[ j ] );
                }
                alignmentIDs[ i ] = standard[ i ].alignmentTypeID;
                alignments[ i ] = standard[ i ].alignmentType;

            }
            parts = listParts.ToArray<string>();
        }
        protected void ConvertJSONStandards( int[] IDs, string[] values, string[] parts, int[] alignmentIDs, string[] alignments, ref ResourceJSONObjectStandard[] standards )
        {
            standards = new ResourceJSONObjectStandard[ IDs.Length ];
            for ( int i = 0 ; i < IDs.Length ; i++ )
            {
                standards[ i ].ID = IDs[ i ];
                standards[ i ].name = values[ i ];
                standards[ i ].alignmentType = alignments[ i ];
                standards[ i ].alignmentTypeID = alignmentIDs[ i ];
                standards[ i ].notationParts = MakeStandardParts( values[ i ] );
            }
        }
        protected string[][] GetAliases( int[] IDs )
        {
            List<string[]> listStrings = new List<string[]>();
            StringBuilder builder = new StringBuilder();
            foreach ( int i in IDs )
            {
                builder.Append(" OR Id = " + i);
            }
            string query = builder.ToString().Substring(4);
            DataSet ds = DatabaseManager.DoQuery( "SELECT Id, AliasValues FROM [Codes.GradeLevel] WHERE " + query ); //TODO: convert to proc
            if(DatabaseManager.DoesDataSetHaveRows(ds))
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    int index = int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) ) - 1;
                    foreach ( int i in IDs )
                    {
                        if ( IDs[ i ] == index )
                        {
                            string[] items = DatabaseManager.GetRowColumn( dr, "Aliasvalues" ).Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries );
                            for(int j = 0; j < items.Length; j++)
                            {
                                items[ j ] = items[ j ].Trim();
                            }
                            listStrings.Add( items );
                        }
                    }
                }
            }
            return listStrings.ToArray<string[]>();
        }
        #endregion

        #endregion
    }
    
}
