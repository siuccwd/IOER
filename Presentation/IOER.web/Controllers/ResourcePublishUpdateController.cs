using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

using ILPathways.Library;
using ILPathways.Services;
using ILPathways.Utilities;
using LearningRegistry;
using LearningRegistry.RDDD;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using Resource = LRWarehouse.Business.Resource;
using Thumbnailer = LRWarehouse.DAL.ResourceThumbnailManager;

namespace ILPathways.Controllers
{
    public class ResourcePublishUpdateController : BaseUserControl
    {
        protected int resourceIntID;
        protected System.Guid resourceID;
        protected int userID;
        [Obsolete]
        private void PublishToAllXXX( Resource resourceEntity, string keyData, string PgpKeyringLocation, ref int resourceVersionID )
        {
            string statusMessage = "";
            PublishToAllXXXX( resourceEntity, ref resourceVersionID, ref statusMessage );
        }

        /// <summary>
        /// Publish resource to LR (depending on lrPublishAction) and IOER database
        /// </summary>
        /// <param name="resourceEntity"></param>
        /// <param name="keyData"></param>
        /// <param name="PgpKeyringLocation"></param>
        /// <param name="resourceVersionID"></param>
        /// <param name="statusMessage"></param>
        public void PublishToAllXXXX( Resource resourceEntity, ref int resourceVersionID, ref string statusMessage )
        {
            Resource tempEntity = PublishToLearningRegistry( resourceEntity, ref statusMessage ); //Different because it will add the doc_ID to the entity and return it

            if ( tempEntity.IsValid )
            {
                try
                {
                    //int resourceVersionID = 0;
                    PublishToDatabase( tempEntity, ref resourceVersionID, ref statusMessage );
                    UpdateElasticSearch( resourceVersionID );
                    LoggingHelper.DoTrace( "Published using full tool, Thumbnail step is next. IntID: " + tempEntity.Id + ", url: " + tempEntity.ResourceUrl );
                    new Thumbnailer().CreateThumbnail( tempEntity.Id, tempEntity.ResourceUrl );
                }
                catch ( Exception ex )
                {
                    LoggingHelper.LogError( "Error in ResourcePublishUpdateController PublishToAll: " + ex.ToString() );
                }
            }
        }

        #region - using integer version id
        //public void UpdateAllXX( Resource resourceEntity, int resourceVersionID )
        //{
        //    UpdateDatabaseXX( resourceEntity, ref resourceVersionID );
        //    //LR updates handled by external update method
        //}
        //public void UpdateDatabaseXX( Resource resourceEntity, ref int resourceVersionID )
        //{
        //    string statusMessage = "";
        //    PublishToDatabase( resourceEntity, ref resourceVersionID, ref statusMessage );
        //    UpdateElasticSearch( resourceVersionID );
        //}
        public void UpdateElasticSearch( int resourceVersionID )
        {
            //ElasticSearchManager eManager = new ElasticSearchManager();
            //ResourceJSONManager jManager = new ResourceJSONManager();
            //ResourceJSONFlat[] flats = jManager.GetJSONFlatByVersionID( resourceVersionID );
            //eManager.CreateOrReplaceRecord( flats );
            new ElasticSearchManager().RefreshResource( new ResourceService().GetIntIDFromVersionID( resourceVersionID ) );
        }
        public void PublishToDatabase( Resource resourceEntity, ref int resourceVersionID, ref string statusMessage )
        {
            bool doingUpdate = false;
            ResourceManager resourceManager = new ResourceManager();
            ResourceClusterManager clusterManager = new ResourceClusterManager();
            ResourceTypeManager typeManager = new ResourceTypeManager();
            ResourceFormatManager formatManager = new ResourceFormatManager();
            ResourceVersionManager versionManager = new ResourceVersionManager();
            ResourceKeywordManager keywordManager = new ResourceKeywordManager();
            ResourceGradeLevelManager gradeManager = new ResourceGradeLevelManager();
            ResourceLanguageManager languageManager = new ResourceLanguageManager();
            ResourceStandardManager standardManager = new ResourceStandardManager();
            ResourceSubjectManager subjectManager = new ResourceSubjectManager();
            ResourceIntendedAudienceManager intendedAudienceManager = new ResourceIntendedAudienceManager();
            ResourceRelatedUrlManager relatedURLManager = new ResourceRelatedUrlManager();
            ResourceGroupTypeManager groupTypeManager = new ResourceGroupTypeManager();
            ResourceEducationUseManager edUseManager = new ResourceEducationUseManager();
            ResourceItemTypeManager itemTypeManager = new ResourceItemTypeManager();
            ResourceAssessmentTypeManager assessmentTypeManager = new ResourceAssessmentTypeManager();

            //If creating a new resource (publishing)
            if ( resourceVersionID == 0 )
            {
                //Create the initial resource
                resourceIntID = resourceManager.Create( resourceEntity, resourceEntity.CreatedById, ref statusMessage );
                resourceVersionID = resourceEntity.Version.Id;

                //get rowId, just in case
                resourceID = resourceEntity.RowId;

                doingUpdate = false;
            }
            else
            {
                ResourceVersion rv = new ResourceVersionManager().Get( resourceVersionID );
                if ( rv != null && rv.Id > 0 )
                {
                    resourceIntID = rv.ResourceIntId;
                    resourceID = resourceEntity.RowId;

                    //?????
                    //resourceEntity.RowId = rv.ResourceId;

                    //Figure out what the Resource Version should be
                    resourceEntity.Version = MergeVersions( resourceEntity.Version, rv );
                }
                else
                {
                    statusMessage = "Error - resource version record was not found";
                    return;
                }
                doingUpdate = true;
            }

            //Get the user ID
            userID = resourceEntity.CreatedById;

            //Most data goes here
            //resourceEntity.Version.ResourceId = resourceID;
            resourceEntity.Version.ResourceIntId = resourceIntID;
            resourceEntity.Version.CreatedById = userID;
            resourceEntity.LastUpdated = DateTime.Now;
            if ( doingUpdate )
            {
                versionManager.UpdateById( resourceEntity.Version );
                //DatabaseManager.DoQuery( "UPDATE [Resource.Version] SET AccessRights = '" + resourceEntity.Version.AccessRights + "', AccessRightsId = " + resourceEntity.Version.AccessRightsId + " WHERE Id = " + resourceVersionID );  //TODO: do this properly. won't work due to permissions right now. need a proc.
            }
            else
            {
                //versionManager.Create( resourceEntity.Version, ref statusMessage );
                //resourceVersionID = resourceEntity.Version.Id;
                resourceVersionID = versionManager.Create( resourceEntity.Version, ref statusMessage );
            }

            //Related URL
            CreateFromMap( resourceEntity.relatedURL, null, relatedURLManager.Create, ref statusMessage );

            //Language
            CreateFromMap( resourceEntity.Language, languageManager.CreateFromEntity, null, ref statusMessage );

            //Intended Audience
            CreateFromMap( resourceEntity.Audience, intendedAudienceManager.CreateFromEntity, null, ref statusMessage );

            //Resource Type
            CreateFromMap( resourceEntity.ResourceType, typeManager.Create, null, ref statusMessage );

            //Resource Format
            CreateFromMap( resourceEntity.ResourceFormat, formatManager.CreateFromEntity, null, ref statusMessage );

            //Group Type
            CreateFromMap( resourceEntity.GroupType, null, groupTypeManager.Create, ref statusMessage );

            //Educational Use
            CreateFromMap( resourceEntity.EducationalUse, null, edUseManager.Create, ref statusMessage );

            //Item Type
            CreateFromMap( resourceEntity.ItemType, null, itemTypeManager.Create, ref statusMessage );

            //Keywords
            CreateFromMap( resourceEntity.Keyword, keywordManager.Create, null, ref statusMessage );

            //Grade Level
            CreateFromMap( resourceEntity.Gradelevel, null, gradeManager.Create, ref statusMessage );

            //Subjects
            CreateFromMap( resourceEntity.SubjectMap, subjectManager.CreateFromEntity, null, ref statusMessage );
            
            //Cluster
            CreateFromMap( resourceEntity.ClusterMap, clusterManager.CreateFromEntity, null, ref statusMessage );

            //Resource Standard
            foreach ( ResourceStandard standard in resourceEntity.Standard ) //Can't use CreateFromMap due to ResourceStandard having legitimately needed extra stuff
            {
                //standard.ResourceId = resourceID;
                standard.ResourceIntId = resourceIntID;
                standard.CreatedById = userID;
                standardManager.Create( standard, ref statusMessage );
            }

            //Assessment Type
            if ( resourceEntity.AssessmentType.CodeId != 0 )
            {
                ResourceChildItem assessmentType = resourceEntity.AssessmentType;
                //assessmentType.ResourceId = resourceID;
                assessmentType.ResourceIntId = resourceIntID;
                assessmentType.CreatedById = userID;
                assessmentTypeManager.Create( assessmentType, ref statusMessage );
            }

            //should already have resourceVersionID
            if (resourceVersionID > 0)
            {
                //statusMessage = statusMessage + "<br />Successful DB Publish!<br />Resource ID:<br /><span id=\"resourceDetailID\">" + resourceVersionID + "</span>";
                statusMessage = resourceVersionID.ToString();
            }
            else
            {
                //statusMessage = statusMessage + "<br />Failed to Publish to Database!<br />" + statusMessage;
                statusMessage = "";
            }

        }

        #region helper methods
        //Delicious modularity
        protected delegate int CreateMethodInt( ResourceChildItem mapItem, ref string statusMessage );
        protected delegate string CreateMethodString( ResourceChildItem mapItem, ref string statusMessage );
        protected void CreateFromMap( List<ResourceChildItem> mapItems, CreateMethodString createMethodString, CreateMethodInt createMethodInt, ref string statusMessage )
        {
            foreach ( ResourceChildItem mapItem in mapItems )
            {
                mapItem.CreatedById = WebUser.Id;
                //mapItem.ResourceId = resourceID;
                mapItem.ResourceIntId = resourceIntID;
                if ( mapItem.CreatedById == 0 )
                {
                    mapItem.CreatedById = userID;
                }
                try { mapItem.CodeId = int.Parse( mapItem.MappedValue ); }
                catch { } //Only need to worry about this for real ints
                if ( createMethodString == null )
                {
                    createMethodInt( mapItem, ref statusMessage );
                }
                else
                {
                    createMethodString( mapItem, ref statusMessage );
                }
            }
        }

        protected ResourceVersion MergeVersions( ResourceVersion incoming, ResourceVersion stored )
        {
            ResourceVersion result = stored;
            if ( incoming.AccessRights != "" & incoming.AccessRightsId != 0 )
            {
                result.AccessRights = incoming.AccessRights;
                result.AccessRightsId = incoming.AccessRightsId;
            }
            if ( incoming.TypicalLearningTime != "" )
            {
                result.TypicalLearningTime = incoming.TypicalLearningTime;
            }
            return result;
        }

        #endregion

        #endregion

        protected void manualPublishSavedDocs()
        {
            string[] filenames = Directory.GetFiles( @"C:\elasticSearchJSON\savedDocs\" );
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string status = "";
            string docID = "";
            foreach ( string name in filenames )
            {
                string data = File.ReadAllText( name );
                lr_document doc = serializer.Deserialize<lr_document>( data );
                lr_Envelope envelope = BuildSignedEnvelope( doc, ref status );
                PublishLREnvelope( envelope, ref status, ref docID );
                System.IO.File.Move( name, name.Replace( ".json", "-published.json" ) );
            }
        }

        public lr_Envelope BuildSignedEnvelope( lr_document doc, ref string statusMessage )
        {
            string PgpKeyringLocation = Path.Combine( HttpRuntime.AppDomainAppPath, "App_Data/lrpriv.asc" );
            string keyData = File.ReadAllText( PgpKeyringLocation );
            string[] PublicKeyLocations = new string[] { "http://pgp.mit.edu:11371/pks/lookup?op=get&search=0x6ce0837335049763" };
            string UserID = "ISLEOER (Data Signing Key) <info@siuccwd.com>";
            string password = "89k7SMteVzPUY";

            PgpSigner signer = new PgpSigner( PublicKeyLocations, keyData, UserID, password );
            doc = signer.Sign( doc );
            lr_Envelope envelope = BuildEnvelope( doc );

            return envelope;
        }

        public void PublishLREnvelope( lr_Envelope envelope, ref string statusMessage, ref string lrDocID )
        {
            string node = UtilityManager.GetAppKeyValue( "learningRegistryNodePublish" ); 
            string clientID = "info@siuccwd.com";
            string clientPassword = "in5t@ll3r";

            LRClient client = new LRClient( node, clientID, clientPassword );
            try
            {
                PublishResponse response = client.Publish( envelope );

                SetConsoleSuccessMessage( "Successful Publish!<br />Learning Registry Document ID:<br />" + response.document_results.ElementAt( 0 ).doc_ID );
                statusMessage = statusMessage + "Successful Publish!";
                lrDocID = response.document_results.ElementAt( 0 ).doc_ID;
            }
            catch ( Exception ex )
            {
                SetConsoleErrorMessage( "Publish Failed: " + ex.Message.ToString() );
            }
        }

        public bool BuildSaveLRDocument( Resource resourceEntity, ref string statusMessage )
        {
            lr_document doc = BuildDocument( ref resourceEntity );
            lr_Envelope envelope = BuildSignedEnvelope( doc, ref statusMessage );
            bool isValid = true;

            PublishPending entity = new PublishPending();
            //issue - don't have id yet, need to call this after the database publish
            entity.ResourceId = resourceEntity.Id;
            entity.ResourceVersionId = resourceEntity.Version.Id;
            entity.Reason = "Resource requires approval.";
            entity.CreatedById = resourceEntity.CreatedById;
            entity.LREnvelope = new JavaScriptSerializer().Serialize( envelope );

            int id = new ResourceManager().PublishPending_Create( entity, ref statusMessage );

            return isValid;
        }

        public Resource PublishToLearningRegistry( Resource resourceEntity, ref string statusMessage )
        {
            string LRDocID = "";
            lr_document doc = BuildDocument( ref resourceEntity );
            lr_Envelope envelope = BuildSignedEnvelope( doc, ref statusMessage );
            PublishLREnvelope( envelope, ref statusMessage, ref LRDocID );
            resourceEntity.Version.LRDocId = LRDocID;
            return resourceEntity;
        }

        public bool PublishSavedEnvelope( int resourceVersionID, ref string statusMessage )
        {
            bool isValid = true;
            string LRDocID = "";
            ResourceManager manager = new ResourceManager();

            try
            {
                PublishPending entity = manager.PublishPending_GetByRVId( resourceVersionID );
                if ( entity != null && entity.Id > 0 )
                {
                    //Publish to LR
                    lr_Envelope envelope = new JavaScriptSerializer().Deserialize<lr_Envelope>(entity.LREnvelope);
                    PublishLREnvelope( envelope, ref statusMessage, ref LRDocID );

                    //Publish to ElasticSearch
                    //ResourceJSONFlat flat = new ResourceJSONManager().GetJSONFlatByVersionID( resourceVersionID )[ 0 ];
                    //new ElasticSearchManager().CreateOrReplaceRecord( flat );
                    new ElasticSearchManager().RefreshResource( new ResourceService().GetIntIDFromVersionID( resourceVersionID ) );

                    //Set status messages
                    SetConsoleSuccessMessage( "Successful Publish of saved resource" );
                    LoggingHelper.DoTrace( 5, "Successful Publish of saved resource - LR Document ID:<br />" + LRDocID );

                    //Update entity
                    entity.IsPublished = true;
                    entity.PublishedDate = DateTime.Now;
                    manager.PublishPending_Update( entity );
                    ResourceVersionManager rvManager = new ResourceVersionManager();
                    ResourceVersion version = rvManager.Get( resourceVersionID );
                    if ( version != null && version.Id > 0 )
                    {
                        version.LRDocId = LRDocID;
                        rvManager.Update_LrDocId( version );
                    }
                    else
                    {
                        isValid = false;
                        statusMessage = string.Format( "Error - unable to retrieve the resource version record (in order to update the LR docId: {0}).", LRDocID );
                        SetConsoleErrorMessage( statusMessage );
                    }
                }
                else
                {
                    statusMessage = "Error - unable to retrieve the requested publish-pending resource";
                    SetConsoleErrorMessage( statusMessage );
                    isValid = false;
                }
            }
            catch ( Exception ex )
            {
                SetConsoleErrorMessage( "Error Publishing to Learning Registry: " + ex.ToString() );
                statusMessage = statusMessage + "Publish of saved resource Failed: " + ex.ToString();
                isValid = false;
            }
            return isValid;
        }

        #region LR Document methods

        private lr_document BuildDocument(ref Resource resourceEntity )
        {
            string resourceData = BuildPayloadLRMI_JSON( resourceEntity );

            lr_document doc = new lr_document();

            //Required Fields
            doc.resource_data_type = "metadata";
            doc.payload_placement = "inline";
            doc.payload_schema = new List<string>( new string[] { "LRMI" } );

            //Most of the data is in here:
            doc.resource_data = resourceData;

            //Resource Locator
            doc.resource_locator = resourceEntity.ResourceUrl;

            //Submitter Informaton
            lr_identity identity = new lr_identity();
            identity.submitter_type = "agent";
            identity.submitter = "ISLE OER on Behalf of " + resourceEntity.Version.Submitter;
            identity.signer = "ISLE OER"; 
            doc.identity = identity;

            //Keywords
            foreach ( ResourceChildItem keyword in resourceEntity.Keyword )
            {
                doc.keys.Add( keyword.OriginalValue.Trim() );
            }

            return doc;
        }

        private string BuildPayloadLRMI_JSON( Resource entity )
        {
            ResourceJSONManager jsonManager = new ResourceJSONManager();
            ResourceJSONLRMI resource = jsonManager.GetJSONLRMIFromResource( entity );

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize( resource );
        }

        protected lr_Envelope BuildEnvelope( lr_document doc )
        {
            lr_Envelope envelope = new lr_Envelope();

            envelope.documents.Add( doc );
            return envelope;
        }

        #endregion
    }
}
