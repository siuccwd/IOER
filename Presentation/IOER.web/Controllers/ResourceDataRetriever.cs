using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using System.Data;

namespace ILPathways.Controllers
{
    public class ResourceDataRetriever
    {
        ResourceManager                 resourceManager                 = new ResourceManager();
        ResourceAssessmentTypeManager   resourceAssessmentTypeManager   = new ResourceAssessmentTypeManager();
        ResourceClusterManager          resourceClusterManager          = new ResourceClusterManager();
        ResourceEducationUseManager     resourceEducationalUseManager   = new ResourceEducationUseManager();
        ResourceFormatManager           resourceFormatManager           = new ResourceFormatManager();
        ResourceGradeLevelManager       resourceGradeLevelManager       = new ResourceGradeLevelManager();
        ResourceGroupTypeManager        resourceGroupTypeManager        = new ResourceGroupTypeManager();
        ResourceIntendedAudienceManager resourceIntendedAudienceManager = new ResourceIntendedAudienceManager();
        ResourceItemTypeManager         resourceItemTypeManager         = new ResourceItemTypeManager();
        ResourceKeywordManager          resourceKeywordManager          = new ResourceKeywordManager();
        ResourceLanguageManager         resourceLanguageManager         = new ResourceLanguageManager();
        ResourceRelatedUrlManager       resourceRelatedUrlManager       = new ResourceRelatedUrlManager();
        ResourceStandardManager         resourceStandardManager         = new ResourceStandardManager();
        ResourceSubjectManager          resourceSubjectManager          = new ResourceSubjectManager();
        ResourceTypeManager             resourceTypeManager             = new ResourceTypeManager();
        ResourceVersionManager          resourceVersionManager          = new ResourceVersionManager();


        public ResourcePackage GetAllResourceData( int resourceIntID )
        {
            ResourcePackage resource = new ResourcePackage();
            resource.ParentResource = resourceManager.Get( resourceIntID );
            resource.Version = resourceVersionManager.Get( resourceIntID );

            resource.AssessmentType = CreateMapFromDataSet( resourceAssessmentTypeManager.Select( resourceIntID ), "AssessmentType" ).First<ResourceChildItem>();
            resource.ClusterMap = CreateResourceChildItem( resourceClusterManager.Select( resourceIntID ), "Cluster" );
            resource.EducationalUse = CreateResourceChildItem( resourceEducationalUseManager.Select( resourceIntID, 0 ), "OriginalType" );
            //13-08-28 mp change method to SelectList (was conflict with another select while cleaning up references to ResourceId
            resource.ResourceFormat = resourceFormatManager.SelectList( resourceIntID );
            resource.Gradelevel = resourceGradeLevelManager.Select( resourceIntID );
            resource.GroupType = CreateResourceChildItem( resourceGroupTypeManager.Select( resourceIntID ), "GroupType" );
            resource.Audience = resourceIntendedAudienceManager.Select( resourceIntID );

            resource.ItemType = CreateResourceChildItem( resourceItemTypeManager.Select( resourceIntID ), "ItemType" );
            resource.Keyword = resourceKeywordManager.Select( resourceIntID );
            resource.Language = resourceLanguageManager.Select( resourceIntID );

            resource.relatedURL = CreateResourceChildItem( resourceRelatedUrlManager.Select( resourceIntID ), "RelatedUrl" );
            resource.Standard = resourceStandardManager.Select( resourceIntID );

            resource.SubjectMap         = resourceSubjectManager.Select( resourceIntID ); 
            resource.ResourceType = resourceTypeManager.SelectList( resourceIntID ); 

            return resource;
        }
        private List<ResourceChildItem> CreateResourceChildItem( DataSet ds, string columnName )
        {
            List<ResourceChildItem> collection = new List<ResourceChildItem>();
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    ResourceChildItem map = new ResourceChildItem();
                    try
                    {
                        map.Id = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Id" ) );
                        map.CodeId = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Id" ) );
                    }
                    catch { }
                    try
                    {
                        map.Created = DateTime.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Created" ) );
                    }
                    catch { }
                    try
                    {
                        map.CreatedById = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "CreatedById" ) );
                    }
                    catch { }
                    try
                    {
                        map.OriginalValue = DatabaseManager.GetRowPossibleColumn( dr, columnName );
                    }
                    catch { }
                    collection.Add( map );
                }
            }
            return collection;
        }

        private List<ResourceChildItem> CreateMapFromDataSet( DataSet ds, string columnName )
        {
            List<ResourceChildItem> collection = new List<ResourceChildItem>();
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    ResourceChildItem map = new ResourceChildItem();
                    try
                    {
                        map.Id = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Id" ) );
                        map.CodeId = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Id" ) );
                    }
                    catch { }
                    try
                    {
                        map.Created = DateTime.Parse( DatabaseManager.GetRowPossibleColumn( dr, "Created" ) );
                    }
                    catch { }
                    try
                    {
                        map.CreatedById = int.Parse( DatabaseManager.GetRowPossibleColumn( dr, "CreatedById" ) );
                    }
                    catch { }
                    try
                    {
                        map.OriginalValue = DatabaseManager.GetRowPossibleColumn( dr, columnName );
                    }
                    catch { }
                    collection.Add( map );
                }
            }
            return collection;
        }
    }
}
