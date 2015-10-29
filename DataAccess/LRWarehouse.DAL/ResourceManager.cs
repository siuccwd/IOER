using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Web.Configuration;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.Business;
using ErrorType = LRWarehouse.Business.ErrorType;
using ErrorRouting = LRWarehouse.Business.ErrorRouting;
using ResourceSubject = LRWarehouse.Business.ResourceSubject;
using Isle.DTO;


namespace LRWarehouse.DAL
{
    /// <summary>
    /// CRUD methods for Resource
    /// See LRManager for Search methods
    /// </summary>
	/// <remarks>15-10-19 mparsons - changed to use new versions for resource create and update, to avoid conflicts with the import, until the latter is updated</remarks>
    public class ResourceManager : BaseDataManager
    {
        static string thisClassName = "ResourceManager";
        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[ResourceGet]";
        const string SELECT_PROC = "[ResourceSelect]";
        const string SEARCH_PROC = "[Resource_Search]";
        const string SEARCH_FT_PROC = "[Resource_Search_FT]";
        const string DELETE_PROC = "[ResourceDelete]";
        const string INSERT_PROC = "[ResourceInsert2]";
		const string UPDATE_PROC = "[ResourceUpdateById2]";

        #region Data Manager Declarations
        AuditReportingManager auditReportingManager = new AuditReportingManager();
        ResourcePropertyManager propertyManager = new ResourcePropertyManager();
        ResourceSubjectManager subjectManager = new ResourceSubjectManager();
        ResourceGradeLevelManager educationLevelManager = new ResourceGradeLevelManager();
        ResourceStandardManager standardManager = new ResourceStandardManager();
        ResourceIntendedAudienceManager audienceManager = new ResourceIntendedAudienceManager();
        ResourceTypeManager typeManager = new ResourceTypeManager();
        ResourceFormatManager formatManager = new ResourceFormatManager();
        //TODO: Add cluster manager
        #endregion


        public ResourceManager()
        {
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }
        #region ====== Core Methods ===============================================

        /// <summary>
        /// Delete resource by RowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( Guid pRowId, ref string statusMessage )
        {
            return Delete( 0, pRowId.ToString(), ref statusMessage );
        }//

        public string Delete( string pRowId )
        {
            string statusMessage = "";
            Delete( 0, pRowId, ref statusMessage );
            return statusMessage;
        }//
        public bool Delete( string pRowId, ref string statusMessage )
        {
            return Delete( 0, pRowId, ref statusMessage );
        }//
        public bool Delete( int id, ref string statusMessage )
        {
            return Delete( id, "", ref statusMessage );
        }//
        /// <summary>
        /// Delete an Resource record
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        private bool Delete( int id, string pRowId, ref string statusMessage )
        {
            string connectionString = LRWarehouse();
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Id", id );
            sqlParameters[ 1 ] = new SqlParameter( "@RowId", pRowId );

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".Delete() " );
                statusMessage = thisClassName + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//



        /// <summary>
        /// Create a resource
        /// If successful, create a Resource.PublishedBy record
        /// </summary>
        /// <param name="pEntity"></param>
        /// <param name="createdById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        //private int Create( Resource pEntity, int createdById, int pPublishedForOrgId, ref string statusMessage )
        //{
        //    ///create the resource
        //    int resourceIntId = Create( pEntity, pPublishedForOrgId, ref statusMessage );

        //    // Ensure entity was created (an id exists), and not already done in the Create method
        //    //check pEntity.CreatedById == 0 (to prevent dup insert), as the create method will do the published by if present
        //    if ( pEntity != null && pEntity.Id > 0 && pEntity.CreatedById == 0 && createdById > 0 )
        //    {
        //        resourceIntId = pEntity.Id;
        //        Create_ResourcePublishedBy( resourceIntId, createdById, pPublishedForOrgId, ref statusMessage );
        //    }
        //    else
        //    {
        //        //need to log something

        //    }

        //    return resourceIntId;
        //}

		/// <summary>
		/// is this used???
		/// </summary>
		/// <param name="resourceUrl"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int CreateByUrl( string resourceUrl, ref string statusMessage )
		{
			Resource pEntity = new Resource();
			pEntity.ResourceUrl = resourceUrl;
			return Create( pEntity, ref statusMessage );
		}//

        /// <summary>
        /// Create a resource
        /// 15-03-25 mparsons - the resourse entity now includes a new property: PublishedForOrgId
        /// </summary>
        /// <param name="pEntity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( Resource pEntity, ref string statusMessage )
        {
            statusMessage = "";
            //string rowId = "";
            int newId = 0;

            try
            {
                SqlParameter[] arParms = new SqlParameter[ 4 ];
                arParms[ 0 ] = new SqlParameter( "@ResourceUrl", pEntity.ResourceUrl);
                arParms[ 1 ] = new SqlParameter( "@ViewCount", pEntity.ViewCount);
                arParms[ 2 ] = new SqlParameter( "@FavoriteCount", pEntity.FavoriteCount);
				arParms[ 3 ] = new SqlParameter( "@ImageUrl", pEntity.ImageUrl );
				
                //TODO - remove HasPathwayGradeLevel
				//arParms[ 3 ] = new SqlParameter( "@HasPathwayGradeLevel", SqlDbType.Bit );
				//arParms[ 3 ].Value = pEntity.HasPathwayGradeLevel;

				SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, arParms );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                    pEntity.Id = newId;
                    //if ( dr.VisibleFieldCount > 1 )
                    //    rowId = dr[ 1 ].ToString();

                    if ( pEntity != null && pEntity.Id > 0 && pEntity.CreatedById > 0 )
                    {
                        Create_ResourcePublishedBy( pEntity.Id, pEntity.CreatedById, pEntity.PublishedForOrgId, ref statusMessage );
                    }
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( "ResourceManager.Create(): " + ex.ToString() );
                statusMessage = ex.Message;
            }
            return newId;
        }//



        /// <summary>
        /// Create a ResourcePublishedBy record
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="createdById"></param>
        /// <param name="statusMessage"></param>
        public void Create_ResourcePublishedBy( int resourceIntId, int createdById, int pPublishedForOrgId, ref string statusMessage )
        {
            //create PublishedBy
            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@PublishedById", createdById );
                sqlParameters[ 2 ] = new SqlParameter( "@PublishedForOrgId", pPublishedForOrgId );
                #endregion

                SqlHelper.ExecuteNonQuery( LRWarehouse(), "[Resource.PublishedByInsert]", sqlParameters );

                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + string.Format( ".Create_ResourcePublishedBy() for Id: {0} and createdById: {1}", resourceIntId, createdById ) );
                statusMessage = thisClassName + "- Unsuccessful: Create_ResourcePublishedBy(): " + ex.Message.ToString();
            }
        }//

		/// <summary>
		/// Obsolete, now done in the ef manager
		/// </summary>
		/// <param name="pEntity"></param>
		/// <returns></returns>
		private string UpdateResourceImageUrl( Resource pEntity )
		{
			return UpdateById( pEntity );
		}

		/// <summary>
		/// update a resource using the integer Id
		/// </summary>
		/// <param name="pEntity"></param>
		/// <returns></returns>
		public string UpdateById( Resource pEntity )
		{
			string status = "successful";
			try
			{
				SqlParameter[] arParms = new SqlParameter[ 6 ];
				arParms[ 0 ] = new SqlParameter( "@Id", pEntity.Id );

				arParms[ 1 ] = new SqlParameter( "@ResourceUrl", pEntity.ResourceUrl );
				arParms[ 2 ] = new SqlParameter( "@ViewCount", pEntity.ViewCount );
				arParms[ 3 ] = new SqlParameter( "@FavoriteCount", pEntity.FavoriteCount );
				arParms[ 4 ] = new SqlParameter( "@ImageUrl", pEntity.ImageUrl );
				arParms[ 5 ] = new SqlParameter( "@IsActive", pEntity.IsActive );

				SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, UPDATE_PROC, arParms );
			}
			catch ( Exception ex )
			{
				LogError( "ResourceManager.UpdateById(): " + ex.ToString() );
				status = ex.Message;
			}
			return status;
		}


		//[Obsolete( "This method is obsolete; use UpdateById instead" )]
        /// <summary>
        /// update a resource using the RowId (OBSOLETE)
        /// </summary>
        /// <param name="pEntity"></param>
        /// <returns></returns>
		//private string UpdateByRowId( Resource pEntity )
		//{
		//	string status = "successful";
		//	try
		//	{
		//		SqlParameter[] arParms = new SqlParameter[ 6 ];
		//		arParms[ 0 ] = new SqlParameter( "@RowId", pEntity.RowId );

		//		arParms[ 1 ] = new SqlParameter( "@ResourceUrl", SqlDbType.VarChar );
		//		arParms[ 1 ].Size = 600;
		//		arParms[ 1 ].Value = pEntity.ResourceUrl;
		//		arParms[ 2 ] = new SqlParameter( "@ViewCount", SqlDbType.Int );
		//		arParms[ 2 ].Value = pEntity.ViewCount;
		//		arParms[ 3 ] = new SqlParameter( "@FavoriteCount", SqlDbType.Int );
		//		arParms[ 3 ].Value = pEntity.FavoriteCount;
		//		arParms[ 4 ] = new SqlParameter( "@ImageUrl", pEntity.ImageUrl );

		//		//arParms[ 4 ] = new SqlParameter( "@HasPathwayGradeLevel", SqlDbType.Bit );
		//		//arParms[ 4 ].Value = pEntity.HasPathwayGradeLevel;
		//		arParms[ 5 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
		//		arParms[ 5 ].Value = pEntity.IsActive;

		//		SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "ResourceUpdate", arParms );
		//	}
		//	catch ( Exception ex )
		//	{
		//		LogError( "ResourceManager.Update(): " + ex.ToString() );
		//		status = ex.Message;
		//	}
		//	return status;
		//}


        /// <summary>
        /// Update the favorite count (will add one to current)
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public string UpdateFavorite( int resourceId )
        {
            string status = "successful";
            try
            {
                SqlParameter[] arParms = new SqlParameter[ 1 ];
                arParms[ 0 ] = new SqlParameter( "@Id", resourceId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "ResourceUpdateFavorite", arParms );
            }
            catch ( Exception ex )
            {
                LogError( "ResourceManager.UpdateFavorite(): " + ex.ToString() );
                status = ex.Message;
            }
            return status;
        }

        /// <summary>
        /// Decrease the favorite count by 1 (typically means removed from library
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public string DecreaseFavoritesByOne( int resourceId )
        {
            string status = "successful";
            try
            {
                SqlParameter[] arParms = new SqlParameter[ 1 ];
                arParms[ 0 ] = new SqlParameter( "@Id", resourceId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "ResourceDecreaseFavorite", arParms );
            }
            catch ( Exception ex )
            {
                LogError( "ResourceManager.DecreaseFavoritesByOne(): " + ex.ToString() );
                status = ex.Message;
            }
            return status;
        }
        /// <summary>
        /// set resource state via resourceVersionId
        /// </summary>
        /// <param name="isActive"></param>
        /// <param name="resourceVersionId"></param>
        /// <returns></returns>
        //public string SetResourceActiveStateByResVersionId( bool isActive, int resourceVersionId )
        //{
        //    ResourceVersion rv = new ResourceVersionManager().Get( resourceVersionId );

        //    if ( rv != null && rv.ResourceIntId > 0 )
        //    {
        //        return SetResourceActiveState( rv.ResourceIntId, isActive );
        //    }
        //    else
        //    {
        //        return "Error - resource version was not found - hmmmm?";
        //    }
        //}

        /// <summary>
        /// set resource state via resource Id
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        public string SetResourceActiveState( int resourceId, bool isActive )
        {
            string status = "successful";
            try
            {
                SqlParameter[] arParms = new SqlParameter[ 2 ];
                arParms[ 0 ] = new SqlParameter( "@ResourceId", resourceId );
                arParms[ 1 ] = new SqlParameter( "@IsActive", isActive );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "Resource_SetActiveState", arParms );
            }
            catch ( Exception ex )
            {
                LogError( thisClassName + ".SetResourceActiveState(): " + ex.ToString() );
                status = ex.Message;
            }
            return status;
        }

        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get resource by RowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public Resource Get( string pRowId )
        {
            return Get( 0, pRowId );

        }//

        /// <summary>
        /// Get resource by Id
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public Resource Get( int pId )
        {
            return Get( pId, "" );

        }//
        /// <summary>
        /// Retrieve a Resource using the related resource version id
        /// </summary>
        /// <param name="resourceVersionId"></param>
        /// <returns></returns>
        public Resource GetByVersion( int resourceVersionId )
        {
            ResourceVersion rv = new ResourceVersionManager().Get( resourceVersionId );
            if ( rv != null && rv.ResourceIntId > 0 )
            {
                return Get( rv.ResourceIntId, "" );
            }
            else
            {
                return new Resource();
            }

        } //

        private Resource Get( int pId, string pRowId )
        {
            Resource entity = new Resource();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", pId );
                sqlParameters[ 1 ] = new SqlParameter( "@RowId", pRowId );

                DataSet ds = SqlHelper.ExecuteDataset( this.ReadOnlyConnString, CommandType.StoredProcedure, "ResourceGet", sqlParameters );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    entity = Fill( ds.Tables[ 0 ].Rows[ 0 ] );
                }
                else
                {
                    entity.IsValid = false;
                    entity.Message = "Not Found";
                }
            }
            catch ( Exception ex )
            {
                //TODO: Something
                LogError( "ResourceManager.Get(): " + ex.ToString() );
                entity.IsValid = false;
                entity.Message = ex.ToString();
            }

            return entity;
        }

        /*         public Resource GetWithChildren(string rowId)
                 {
                     Resource entity = new Resource();
                     try
                     {
                         SqlParameter[] arParms = new SqlParameter[1];
                         arParms[0] = new SqlParameter("@RowId", rowId);

                         DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource_GetWithChildren]", arParms);
                         if (DoesDataSetHaveRows(ds))
                         {
                             entity = FillWithChildren(ds.Tables[0].Rows[0]);
                         }
                         else
                         {
                             entity.IsValid = false;
                             entity.Message = "Not Found";
                         }
                     }
                     catch (Exception ex)
                     {
                         throw ex;
                     }

                     return entity;
                 } */
        public Resource GetByResourceUrl( string resourceUrl, ref string status )
        {
            Resource entity = new Resource();
            string filter = string.Format( "ResourceUrl = '{0}'", resourceUrl.Replace( "'", "''" ) );
            DataSet ds = Select( filter, ref status );
            if ( DoesDataSetHaveRows( ds ) )
            {
                entity = Fill( ds.Tables[ 0 ].Rows[ 0 ] );
            }
            else
            {
                entity.IsValid = false;
                entity.Message = "Not Found";
            }

            return entity;
        }

        public DataSet Select( string pFilter, ref string status )
        {
            status = "successful";
            try
            {
                if ( pFilter.Length > 0 && pFilter.ToLower().IndexOf( "where" ) != 0 )
                {
                    pFilter = "WHERE " + pFilter;
                }
                SqlParameter[] arParms = new SqlParameter[ 1 ];
                arParms[ 0 ] = new SqlParameter( "@filter", SqlDbType.VarChar );
                arParms[ 0 ].Size = 500;
                arParms[ 0 ].Value = pFilter;

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "ResourceSelect", arParms );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceManager.Select(): " + ex.ToString() );
                status = "ResourceManager.Select(): " + ex.Message;
                return null;
            }
        }

        public ResourceCollection SelectCollection( string pFilter, ref string status )
        {
            ResourceCollection collection = new ResourceCollection();
            DataSet ds = Select( pFilter, ref status );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    Resource entity = Fill( dr );
                    collection.Add( entity );
                }
            }

            return collection;
        }

		/// <summary>
		/// ResourceReader - ?????
		/// </summary>
		/// <param name="startingId"></param>
		/// <param name="setSize"></param>
		/// <param name="status"></param>
		/// <returns></returns>
        [Obsolete]
		private DataSet ResourceReader( int startingId, int setSize, ref string status )
        {
            status = "successful";
            try
            {

                SqlParameter[] arParms = new SqlParameter[ 3 ];
                arParms[ 0 ] = new SqlParameter( "@filter", "" );
                arParms[ 1 ] = new SqlParameter( "@StartingId", startingId );
                arParms[ 2 ] = new SqlParameter( "@DatasetSize", setSize );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "ResourcePagesReader", arParms );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceReader.Select(): " + ex.ToString() );
                status = "ResourceReader.Select(): " + ex.Message;
                return null;
            }
        }
        public Resource Fill( DataRow dr )
        {
            Resource entity = new Resource();

            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
            {
                entity.RowId = new Guid( rowId );
            }

            entity.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );
            entity.ViewCount = GetRowColumn( dr, "ViewCount", 0 );
            entity.FavoriteCount = GetRowColumn( dr, "FavoriteCount", 0 );
            entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", DateTime.Now );
			entity.ImageUrl = GetRowPossibleColumn(dr, "ImageUrl", "" );

            //entity.HasPathwayGradeLevel = GetRowColumn( dr, "HasPathwayGradeLevel", true );
            entity.Id = GetRowColumn( dr, "Id", 0 );

            string filter = string.Format( "ResourceId = '{0}'", entity.RowId );
            string status = "successful";

            //DataSet ds = propertyManager.Select(filter, ref status);
            //entity.Property = new ResourcePropertyCollection();
            //if (DoesDataSetHaveRows(ds))
            //{
            //    foreach (DataRow row in ds.Tables[0].Rows)
            //    {
            //        ResourceProperty prop = propertyManager.Fill(row);
            //        entity.Property.Add(prop);
            //    }
            //}
            ResourceSubjectCollection rs = new ResourceSubjectManager().Get( entity.Id, ref status );
            //ResourceSubjectCollection rs = new ResourceSubjectManager().Get(entity.RowId.ToString(), ref status);
            if ( rs != null )
            {
                entity.Subject = rs;
            }
            entity.Gradelevel = educationLevelManager.Select( entity.Id, "" );
            //entity.Standard = standardManager.Select(entity.RowId.ToString());
            entity.Audience = audienceManager.SelectCollection( entity.Id );
            entity.ResourceType = typeManager.Select( entity.Id );
            entity.ResourceFormat = ( List<ResourceChildItem> )formatManager.Select( entity.Id );

            //TODO: Add cluster

            return entity;
        }

        public Resource FillWithChildren( DataRow dr )
        {
            Resource resource = new Resource();
            resource.Version.RowId = new Guid( GetRowColumn( dr, "RowId", DEFAULT_GUID ) );
            resource.RowId =  new Guid( GetRowColumn( dr, "ResourceId", DEFAULT_GUID ) );
            //TODO - confirm
            resource.Id = GetRowColumn( dr, "Id", 0 );

            resource.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );
			resource.ImageUrl = GetRowPossibleColumn( dr, "ImageUrl", "" );
            resource.ViewCount = GetRowColumn( dr, "ViewCount", 0 );
            resource.FavoriteCount = GetRowColumn( dr, "FavoriteCount", 0 );

            resource.Version.LRDocId = GetRowColumn( dr, "DocId", "" );
            resource.Version.Title = GetRowColumn( dr, "Title", "" );
            resource.Version.Description = GetRowColumn( dr, "Description", "" );
            resource.Version.Publisher = GetRowColumn( dr, "Publisher", "" );
            resource.Version.Creator = GetRowColumn( dr, "Creator", "" );
            resource.Version.Rights = GetRowColumn( dr, "Rights", "" );
            resource.Version.AccessRights = GetRowColumn( dr, "AccesRights", "" );
            resource.Version.Modified = GetRowColumn( dr, "Modified", DateTime.Now );
            resource.Version.Submitter = GetRowColumn( dr, "Submitter", "" );
            resource.Version.Imported = GetRowColumn( dr, "Imported", DateTime.Now );
            resource.Version.Creator = GetRowColumn( dr, "Creator", "" );
            resource.Version.TypicalLearningTime = GetRowColumn( dr, "TypicalLearningTime", "" );
            resource.Version.IsSkeletonFromParadata = GetRowColumn( dr, "IsSkeletonFromParadata", false );

            //resource.HasPathwayGradeLevel = GetRowColumn( dr, "HasPathwayGradeLevel", true );
            //resource.Property = new ResourcePropertyCollection();

            string filter = string.Format( "ResourceId  = '{0}'", resource.RowId );
            string status = "successful";

            //DataSet ds = propertyManager.Select(filter, ref status);
            //if (DoesDataSetHaveRows(ds))
            //{
            //    foreach (DataRow row in ds.Tables[0].Rows)
            //    {
            //        ResourceProperty prop = propertyManager.Fill(row);
            //        resource.Property.Add(prop);
            //    }
            //}
            //TODO - ensure proc is returning Id
            ResourceSubjectCollection rs = subjectManager.Get( resource.Id, ref status );
            if ( rs != null )
            {
                resource.Subject = rs;
            }
            else
            {
                resource.Subject = new ResourceSubjectCollection();
            }
            resource.Gradelevel = educationLevelManager.Select( resource.Id, "" );
            resource.Standard = standardManager.Select( resource.Id );
            resource.Audience = audienceManager.SelectCollection( resource.Id );
            resource.ResourceType = typeManager.Select( resource.Id );
            resource.ResourceFormat = ( List<ResourceChildItem> )formatManager.Select( resource.Id );
            //TODO: Add cluster

            return resource;
        }
        #endregion
        #region == resource access

        public bool CanUserEditResource( int pResourceId, int userId )
        {
            bool canEdit = false;
            ObjectMember mbr = new ObjectMember();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@UserId", userId );

                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.CanEditMetaData]", sqlParameters );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        bool isAuthor = GetRowColumn( dr, "IsAuthor", false );
                        bool hasOrgAccess = GetRowColumn( dr, "HasOrgAccess", false );
                        if ( isAuthor || hasOrgAccess )
                            return true;
                    }
                }
                
            }
            catch ( Exception ex )
            {
                LogError( "ResourceManager.CanUserEditResource(): " + ex.ToString() );
            }

            return canEdit;
        }


		#endregion

		#region utilities
		/// <summary>
		/// Convert old style tags to new table
		/// </summary>
		/// <param name="resourceId"></param>
		/// <returns></returns>
		public bool ResourceTag_ConvertById( int resourceId )
		{
			string connectionString = LRWarehouse();
			bool successful = false;

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@resourceID", resourceId );

			try
			{
				SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "[ResourceTag.ConvertById]", sqlParameters );
				successful = true;
			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".ResourceTag_ConvertById() " );

				successful = false;
			}
			return successful;
		}//


		/// <summary>
		/// Get resource data from tag tables
		/// </summary>
		/// <param name="resourceId"></param>
		/// <returns></returns>
		public DataSet Resource_IndexV3TextsUpdate( int resourceId )
		{
			DataSet ds = new DataSet();

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@resourceID", resourceId );

			try
			{
				ds = SqlHelper.ExecuteDataset( LRWarehouse(), CommandType.StoredProcedure, "[Resource_IndexV3TextsUpdate]", sqlParameters );
				if ( ds.HasErrors )
				{
					return null;
				}
			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".Resource_IndexV3TextsUpdate() " );
				return null;

			}
			return ds;
		}//

		/// <summary>
		/// Get resource data from tag tables
		/// </summary>
		/// <param name="resourceId"></param>
		/// <returns></returns>
		public DataSet Resource_IndexV3TagsUpdate( int resourceId )
		{
			DataSet ds = new DataSet();

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@resourceID", resourceId );

			try
			{
				ds = SqlHelper.ExecuteDataset( LRWarehouse(), CommandType.StoredProcedure, "[Resource_IndexV3TagsUpdate]", sqlParameters );
				if ( ds.HasErrors )
				{
					return null;
				}
			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".Resource_IndexV3TagsUpdate() " );
				return null;

			}
			return ds;
		}//

		#endregion

		#region Delayed Publishing
		/// <summary>
		/// Call proc to create resource objects for all components of a hierarchy like a learing list
		/// Take a learning list content item, and publish all child content, clone tags. 	
		/// This proc will move recursively through the learning list children. 	
		/// @TopContentId is the learning list top node (or could be a hierarchy item below the top level).
		/// </summary>
		/// <param name="contentId"></param>
		/// <param name="resourceId"></param>
		/// <param name="createdById"></param>
		/// <param name="statusMessage"></param>
		/// <returns>A comma separated list of resource Ids</returns>
		public bool InitiateDelayedPublishing( int contentId, int resourceId, int createdById, ref string resourceList,  ref string statusMessage )
		{
			bool successful = false;
			resourceList = "";
			SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
			sqlParameters[ 0 ] = new SqlParameter( "@TopContentId", contentId );
			sqlParameters[ 1 ] = new SqlParameter( "@ContentId", 0 );
			sqlParameters[ 2 ] = new SqlParameter( "@TopResourceId", resourceId );
			sqlParameters[ 3 ] = new SqlParameter( "@CreatedById", createdById );
			//sqlParameters[ 3 ] = new SqlParameter( "@TopResourceVersionId", 0 );

			using ( SqlConnection conn = new SqlConnection( LRWarehouse() ) )
			{
				DataSet ds = new DataSet();
				try
				{
					//get resources list
					ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Resource.PublishRelatedContent]", sqlParameters );

					if ( DoesDataSetHaveRows( ds ) )
					{
						foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
						{
							resourceList = GetRowColumn( dr, "Resources", "" );
							//only return one
							DoTrace( 1, string.Format("ResourceManager.InitiateDelayedPublishing. ContentId: {0}, resId: {1}, resources: {2}", contentId, resourceId, resourceList) );
							successful = true;
							break;
						}
					}
					else
					{
						statusMessage = "Warning no resources were returnede";
					}

				}
				catch ( Exception ex )
				{
					LogError( ex, thisClassName + ".InitiateDelayedPublishing() " );
					statusMessage = thisClassName + "- Unsuccessful: InitiateDelayedPublishing(): " + ex.Message.ToString();

					successful = false;

				}
			}

	
			return successful;
		}//
		#endregion

		#region Publish Pending
		/// <summary>
        /// Delete an Publish.Pending record using rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool PublishPending_Delete( int id, ref string statusMessage )
        {
            bool successful = false;
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", id );

                    SqlHelper.ExecuteNonQuery( conn, CommandType.StoredProcedure, "TBD", sqlParameters );
                    successful = true;
                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".PublishPending_Delete() " );
                    statusMessage = thisClassName + "- Unsuccessful: Delete(): " + ex.Message.ToString();

                    successful = false;
                }
            }
            return successful;
        }//

        /// <summary>
        /// Add an Publish.Pending record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int PublishPending_Create( PublishPending entity, ref string statusMessage )
        {
            int newId = 0;
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {
                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", entity.ResourceId );
                    sqlParameters[ 1 ] = new SqlParameter( "@ResourceVersionId", entity.ResourceVersionId );
                    sqlParameters[ 2 ] = new SqlParameter( "@Reason", entity.Reason );
                    sqlParameters[ 3 ] = new SqlParameter( "@LREnvelope", entity.LREnvelope );
                    sqlParameters[ 4 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
                    #endregion

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, CommandType.StoredProcedure, "[Publish.PendingInsert]", sqlParameters );
                    if ( dr.HasRows )
                    {
                        dr.Read();
                        newId = Int32.Parse( dr[ 0 ].ToString() );
                    }
                    dr.Close();
                    dr = null;
                    statusMessage = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".Create() for RowId: {0} and CreatedBy: {1}", entity.RowId.ToString(), entity.CreatedBy ) );
                    statusMessage = thisClassName + "- Unsuccessful: Create(): " + ex.Message.ToString();
                    entity.Message = statusMessage;
                    entity.IsValid = false;
                }
            }
            return newId;
        }

        /// <summary>
        /// Update an Publish.Pending record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string PublishPending_Update( PublishPending entity )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {
                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
                    sqlParameters[ 1 ] = new SqlParameter( "@DocId", entity.DocId );
                    sqlParameters[ 2 ] = new SqlParameter( "@IsPublished", entity.IsPublished );
                    sqlParameters[ 3 ] = new SqlParameter( "@PublishedDate", entity.PublishedDate );
                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, "[Publish.PendingUpdate]", sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + string.Format( ".PublishPending_Update() for Id: {0} and DocId: {1}", entity.Id.ToString(), entity.DocId ) );

                    message = thisClassName + "- Unsuccessful: PublishPending_Update(): " + ex.Message.ToString();
                    entity.Message = message;
                    entity.IsValid = false;
                }
            }
            return message;
        }//

        public PublishPending PublishPending_GetByRVId( int pResourceVersionId )
        {
            return PublishPending_Get( 0, pResourceVersionId, 0 );
        }//
        public PublishPending PublishPending_GetByResId( int pResourceId )
        {
            return PublishPending_Get( pResourceId, 0, 0 );
        }//
        public PublishPending PublishPending_Get( int pId )
        {
            return PublishPending_Get( 0, 0, pId );
        }//
        private PublishPending PublishPending_Get( int pResourceId, int pResourceVersionId, int pId )
        {
            PublishPending entity = new PublishPending();
            using ( SqlConnection conn = new SqlConnection( ReadOnlyConnString ) )
            {
                try
                {
                    SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
                    sqlParameters[ 1 ] = new SqlParameter( "@ResourceVersionId", pResourceVersionId );
                    sqlParameters[ 2 ] = new SqlParameter( "@Id", pId );

                    SqlDataReader dr = SqlHelper.ExecuteReader( conn, "[Publish.PendingGet]", sqlParameters );

                    if ( dr.HasRows )
                    {
                        // it should return only one record.
                        while ( dr.Read() )
                        {
                            entity.IsValid = true;

                            entity.Id = GetRowColumn( dr, "Id", 0 );
                            entity.ResourceVersionId = GetRowColumn( dr, "ResourceVersionId", 0 );
                            entity.ResourceId = GetRowColumn( dr, "ResourceId", 0 );

                            entity.Reason = GetRowColumn( dr, "Reason", "" );
                            entity.LREnvelope = GetRowColumn( dr, "LREnvelope", "" );
                            entity.IsPublished = GetRowColumn( dr, "IsPublished", false );
                            entity.PublishedDate = GetRowColumn( dr, "PublishedDate", System.DateTime.MinValue );

                            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
                            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
                        }

                    }
                    else
                    {
                        entity.Message = "Record not found";
                        entity.IsValid = false;
                    }
                    dr.Close();
                    dr = null;
                    return entity;

                }
                catch ( Exception ex )
                {
                    LogError( ex, thisClassName + ".PublishPending_Get() " );
                    entity.Message = "Unsuccessful: " + thisClassName + ".PublishPending_Get(): " + ex.Message.ToString();
                    entity.IsValid = false;
                    return entity;

                }
            }

        }//
        #endregion

        #region ======== Link Checker Methods ========

        public string GetLinkCheckerConnString()
        {
            string conn = WebConfigurationManager.ConnectionStrings[ "LinkCheckerConString" ].ConnectionString;
            return conn;
        }

        public DataSet GetListOfResourcesToUndelete( bool doUndelete )
        {
            SqlParameter[] parms = new SqlParameter[ 1 ];
            parms[ 0 ] = new SqlParameter( "@DoUndelete", doUndelete );

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset( GetLinkCheckerConnString(), CommandType.StoredProcedure, "[Resource.UndoBadLinkCheck]", parms );
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( thisClassName + ".GetListOfResourcesToUndelete(): " + ex.ToString() );
                Console.WriteLine( thisClassName + ".GetListOfResourcesToUndelete(): " + ex.Message );
                return null;
            }
        }
        #endregion


        #region Xml stuff

        /*        private void AddResourceProperties(Resource entity, XmlDocument xdoc, string tagName, string collectionName)
        {
            string status = "successful";
            ResourcePropertyManager propManager = new ResourcePropertyManager();
            ResourceKeywordManager keysManager = new ResourceKeywordManager();
            ResourceChildItem keys = keysManager.Get(entity.RowId.ToString());
            if(keys==null) 
            {
                keys = new ResourceChildItem();
            }
            DataSet dsPropertyType = propManager.ResPropertyTypeLookup(collectionName);
            if (dsPropertyType != null)
            {
                int iPropertyType = GetRowColumn(dsPropertyType.Tables[0].Rows[0], "Id", 0);
                XmlNodeList nodeList = xdoc.GetElementsByTagName(tagName);
                bool itemDeletedFromKeywords = false;
                foreach (XmlNode node in nodeList)
                {
                    string value = string.Format("\"{0}\"",node.InnerText);
                    if (keys.OriginalValue.IndexOf(value) > -1)
                    {
                        // Property exists as a keyword, delete from keywords
                        keys.OriginalValue = keys.OriginalValue.Replace(value, "");
                        keys.OriginalValue = keys.OriginalValue.Replace(",,", ",");
                        itemDeletedFromKeywords = true;
                    }

                    ResourceProperty prop = new ResourceProperty();
                    prop.ResourceId = entity.RowId;
                    prop.Name = collectionName;
                    prop.PropertyTypeId = iPropertyType;
                    prop.Value = node.InnerText;

                    propManager.Create(prop, ref status);
                }


                DataSet ds = propManager.Select(string.Format("ResourceId = '{0}'", entity.RowId), ref status);
                if (DoesDataSetHaveRows(ds))
                {
                    entity.Property = new ResourcePropertyCollection();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ResourceProperty prop = propManager.Fill(dr);
                        entity.Property.Add(prop);
                    }
                }
            }
        } */


        private void AddResourceTypes( Resource entity, XmlDocument xdoc, string tagName )
        {
            string status = "successful";
            ResourceTypeManager rtm = new ResourceTypeManager();
            ResourceKeywordManager keysManager = new ResourceKeywordManager();

            //??? the following doesn't make sense. Calling with resource id, could return multiple rows.
            //      and the code is checking as if a single value is returned!!!!!!
            //ResourceMap keys = keysManager.Get(entity.Id);
            ResourceChildItem keys = new ResourceChildItem();
            if ( keys == null )
            {
                keys = new ResourceChildItem();
            }
            XmlNodeList list = xdoc.GetElementsByTagName( tagName );
            bool itemDeletedFromKeys = false;
            foreach ( XmlNode node in list )
            {
                string value = string.Format( "\"{0}\"", node.InnerText );
                //don't you need to normalize case before checking
                if ( keys.OriginalValue.IndexOf( value ) > -1 )
                {
                    keys.OriginalValue = keys.OriginalValue.Replace( value, "" );
                    keys.OriginalValue = keys.OriginalValue.Replace( ",,", "," );
                    itemDeletedFromKeys = true;
                }

                ResourceChildItem type = new ResourceChildItem();
                type.ResourceIntId = entity.Id;
                type.OriginalValue = node.InnerText;
                rtm.Create( type, ref status );
            }

            entity.ResourceType = rtm.Select( entity.Id );
        }

        private void AddResourceFormats( Resource entity, XmlDocument xdoc, string tagName )
        {
            string status = "successful";
            ResourceFormatManager rfm = new ResourceFormatManager();
            ResourceKeywordManager keysManager = new ResourceKeywordManager();

            //??? the following doesn't make sense. Calling with resource id, could return multiple rows.
            //      and the code is checking as if a single value is returned!!!!!!
            //ResourceMap keys = keysManager.Get(entity.Id);
            ResourceChildItem keys = new ResourceChildItem();
            if ( keys == null )
            {
                keys = new ResourceChildItem();
            }

            XmlNodeList list = xdoc.GetElementsByTagName( tagName );
            bool itemDeletedFromKeys = false;
            foreach ( XmlNode node in list )
            {
                string value = string.Format( "\"{0}\"", node.InnerText );
                if ( keys.OriginalValue.IndexOf( value ) > -1 )
                {
                    keys.OriginalValue = keys.OriginalValue.Replace( value, "" );
                    keys.OriginalValue = keys.OriginalValue.Replace( ",,", "," );
                    itemDeletedFromKeys = true;
                }

                ResourceChildItem format = new ResourceChildItem();
                format.ResourceIntId = entity.Id;
                format.OriginalValue = node.InnerText;
                rfm.CreateFromEntity( format, ref status );
            }

            entity.ResourceFormat = ( List<ResourceChildItem> )rfm.Select( entity.Id );
        }


        /*        private void AddResourcePropertiesByXpath( Resource entity, XmlDocument xdoc, string xpath, string collectionName )
                {
                    XmlNodeList list;
                    string status = "successful";
                    ResourcePropertyManager propManager = new ResourcePropertyManager();
                    DataSet dsPropertyType = propManager.ResPropertyTypeLookup(collectionName);
                    if (dsPropertyType != null)
                    {
                        int iPropertyType = GetRowColumn(dsPropertyType.Tables[0].Rows[0], "Id", 0);

                        list = xdoc.SelectNodes(xpath);
                        foreach (XmlNode node in list)
                        {
                            ResourceProperty prop = new ResourceProperty();
                            prop.ResourceId = entity.RowId;
                            prop.Name = collectionName;
                            prop.PropertyTypeId = iPropertyType;
                            prop.Value = node.InnerText;

                            propManager.Create(prop, ref status);
                        }

                        DataSet ds = propManager.Select(string.Format("ResourceId = '{0}'", entity.RowId), ref status);
                        if (DoesDataSetHaveRows(ds))
                        {
                            entity.Property = new ResourcePropertyCollection();
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                ResourceProperty prop = propManager.Fill(dr);
                                entity.Property.Add(prop);
                            }
                        }
                    }
                } */

        /*        private void AddResourcePropertiesByXpath( Resource entity, XmlDocument doc, string parentXpath, string selectorName,
                    string selectorValue, string valueSubXpath, string collectionName)
                {
                    string status = "successful";

                    ResourcePropertyManager propManager = new ResourcePropertyManager();
                    DataSet dsPropertyType = propManager.ResPropertyTypeLookup(collectionName);
                    if (dsPropertyType != null)
                    {
                        int iPropertyType = GetRowColumn(dsPropertyType.Tables[0].Rows[0], "Id", 0);
                        XmlNodeList list = doc.SelectNodes(parentXpath);
                        foreach (XmlNode node in list)
                        {
                            XmlDocument xd = new XmlDocument();
                            xd.Load(node.OuterXml);
                            XmlNodeList list2 = xd.GetElementsByTagName(selectorName);
                            foreach (XmlNode node2 in list2)
                            {
                                if (node2.InnerText.ToLower() == selectorValue.ToLower())
                                {
                                    XmlNodeList list3 = xd.SelectNodes(valueSubXpath);
                                    foreach (XmlNode node3 in list3)
                                    {
                                        ResourceProperty prop = new ResourceProperty();
                                        prop.ResourceId = entity.RowId;
                                        prop.Name = collectionName;
                                        prop.PropertyTypeId = iPropertyType;
                                        prop.Value = node3.InnerText;

                                        propManager.Create(prop, ref status);
                                    }
                                    DataSet ds = propManager.Select(string.Format("ResourceId = '{0}'", entity.RowId), ref status);
                                    if (DoesDataSetHaveRows(ds))
                                    {
                                        entity.Property = new ResourcePropertyCollection();
                                        foreach (DataRow dr in ds.Tables[0].Rows)
                                        {
                                            ResourceProperty prop = propManager.Fill(dr);
                                            entity.Property.Add(prop);
                                        }
                                    }
                                }
                            }
                        }
                    }
                } */

        #endregion

    }
}
