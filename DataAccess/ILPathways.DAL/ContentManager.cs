using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;

using ILPathways.Business;

namespace ILPathways.DAL
{
    public class ContentManager : BaseDataManager
	{
		static string className = "ContentManager";
		/// <summary>
		/// Base procedures
		/// </summary>
		const string GET_PROC = "[ContentGet]";
		const string SELECT_PROC = "[ContentSelect]";
		const string DELETE_PROC = "[ContentDelete]";
		const string INSERT_PROC = "[ContentInsert]";
		const string UPDATE_PROC = "[ContentUpdate]";


		/// <summary>
		/// Default constructor
		/// </summary>
		public ContentManager()
		{ }//

		#region ====== Core Methods ===============================================
		/// <summary>
		/// Delete a ContentItem record
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
		{
			bool successful = false;

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
			sqlParameters[ 0 ].Value = pId;

			try
			{
                SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
				successful = true;
			}
			catch ( Exception ex )
			{
				LogError( ex, className + ".Delete() " );
				statusMessage = className + "- Unsuccessful: Delete(): " + ex.Message.ToString();

				successful = false;
			}
			return successful;
		}//


		/// <summary>
		/// Add a ContentItem record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        [Obsolete]
        private int Create( ContentItem entity, ref string statusMessage )
		{
			int newId = 0;
            NotifyAdmin( className + "Create()", string.Format("Call to an obsolete method. Title: {0}, userId: {1} ", entity.Title, entity.CreatedById) );
			try
			{
				#region parameters
                SqlParameter[] sqlParameters = new SqlParameter[17];
                sqlParameters[0] = new SqlParameter("@TypeId", SqlDbType.Int);
                sqlParameters[0].Value = entity.TypeId;
                sqlParameters[1] = new SqlParameter("@Title",  entity.Title);
                sqlParameters[2] = new SqlParameter("@Summary", entity.Summary);
                sqlParameters[3] = new SqlParameter("@Description", entity.Description);

                sqlParameters[4] = new SqlParameter("@StatusId", SqlDbType.Int);
                sqlParameters[4].Value = entity.StatusId;

                sqlParameters[5] = new SqlParameter("@PrivilegeTypeId", SqlDbType.Int);
                sqlParameters[5].Value = entity.PrivilegeTypeId;

                sqlParameters[ 6 ] = new SqlParameter( "@ConditionsOfUseId", entity.ConditionsOfUseId );
                sqlParameters[ 7 ] = new SqlParameter( "@IsActive", entity.IsActive );
                sqlParameters[ 8 ] = new SqlParameter( "@OrgId", entity.OrgId );
                sqlParameters[ 9 ] = new SqlParameter( "@IsOrgContentOwner", entity.IsOrgContentOwner );
                sqlParameters[ 10 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
                sqlParameters[ 11 ] = new SqlParameter( "@UseRightsUrl", entity.UsageRightsUrl );

                sqlParameters[ 12 ] = new SqlParameter( "@DocumentUrl", entity.DocumentUrl );
                sqlParameters[ 13 ] = new SqlParameter( "@DocumentRowId", entity.DocumentRowId.ToString() );
                sqlParameters[ 14 ] = new SqlParameter( "@SortOrder", entity.SortOrder );
                sqlParameters[ 15 ] = new SqlParameter( "@Timeframe", entity.Timeframe );
                sqlParameters[ 16 ] = new SqlParameter( "@ParentId", entity.ParentId );
	
				#endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
				if ( dr.HasRows )
				{
					dr.Read();
					newId = int.Parse( dr[ 0 ].ToString() );
                    entity.Id = newId;
                    entity.IsValid = true;
				}
				dr.Close();
				dr = null;
				statusMessage = "successful";

			}
			catch ( Exception ex )
			{
				//provide helpful info about failing entity
			    LogError( ex, className + string.Format( ".Create() for title: {0} and CreatedBy: {1}", entity.Title, entity.CreatedById ) );	
				statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
				
				entity.Message = statusMessage;
				entity.IsValid = false;
			}

			return newId;
		}

		/// <summary>
		/// Update an Content record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
        [Obsolete]
        private string Update( ContentItem entity )
		{
			string message = "successful";

			try
			{
                NotifyAdmin( className + "Update()", string.Format( "Call to an obsolete method. Title: {0}, userId: {1} ", entity.Title, entity.LastUpdatedById ) );
				#region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 17 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id);

                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );

                sqlParameters[ 2 ] = new SqlParameter( "@Summary", entity.Summary );

                sqlParameters[ 3 ] = new SqlParameter( "@Description", entity.Description );

                sqlParameters[ 4 ] = new SqlParameter( "@StatusId", entity.StatusId);

                sqlParameters[ 5 ] = new SqlParameter( "@PrivilegeTypeId", entity.PrivilegeTypeId );
                sqlParameters[ 6 ] = new SqlParameter( "@ConditionsOfUseId", entity.ConditionsOfUseId );
                sqlParameters[ 7 ] = new SqlParameter( "@IsActive", entity.IsActive);

                sqlParameters[ 8 ] = new SqlParameter( "@IsPublished", entity.IsPublished);

                sqlParameters[ 9 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById );
                sqlParameters[ 10 ] = new SqlParameter( "@UsageRightsUrl", entity.UsageRightsUrl );
				sqlParameters[ 11 ] = new SqlParameter( "@ParentId", entity.ParentId );

                sqlParameters[ 12 ] = new SqlParameter( "@DocumentUrl", entity.DocumentUrl );
                sqlParameters[ 13 ] = new SqlParameter( "@DocumentRowId", entity.DocumentRowId.ToString() );
                sqlParameters[ 14 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameters[ 15 ] = new SqlParameter( "@SortOrder", entity.SortOrder );
                sqlParameters[ 16 ] = new SqlParameter( "@Timeframe", entity.Timeframe );
           
				
				#endregion

				SqlHelper.ExecuteNonQuery( ContentConnection(), UPDATE_PROC, sqlParameters );
				message = "successful";

			}
			catch ( Exception ex )
			{
				LogError( ex, className + string.Format( ".Update() for Id: {0}, Title: {1} and LastUpdatedBy: {2}", entity.Id.ToString(), entity.Title.ToString(), entity.LastUpdatedById ) );
				message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
				entity.Message = message;
				entity.IsValid = false;
			}

			return message;

		}//


        /// <summary>
        /// Update content item with related resourceIntId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        private string UpdateResourceIntId( int id, int resourceIntId )
        {
            string message = "successful";

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", id );
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                #endregion

                SqlHelper.ExecuteNonQuery( ContentConnection(), "[Content.UpdateResourceIntId]", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".UpdateResourceIntId() for Id: {0}, and resourceIntId: {2}", id, resourceIntId ) );
                message = className + "- Unsuccessful: UpdateResourceIntId(): " + ex.Message.ToString();

            }

            return message;

        }//

        /// <summary>
        /// Set as a featured item
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public string SetAsFeaturedItem( int parentId, int contentId )
        {
            string message = "successful";

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ParentId", parentId );
                sqlParameters[ 1 ] = new SqlParameter( "@ContentId", contentId );
                #endregion

                SqlHelper.ExecuteNonQuery( ContentConnection(), "[Content.SetAsFeaturedItem]", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".SetAsFeaturedItem for parentId: {0}, contentId: {1}", parentId, contentId ));
                message = className + "- Unsuccessful: SetAsFeaturedItem(): " + ex.Message.ToString();
            }

            return message;

        }//

        /// <summary>
        /// Remove a featured item - sets sort order to zero
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public string RemoveFeaturedItem( int contentId )
        {
            string message = "successful";

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ContentId", contentId );
                sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", 10 );
                #endregion

                SqlHelper.ExecuteNonQuery( ContentConnection(), "[Content.ChangeSortOrder]", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".RemoveFeaturedItem for contentId: {0}", contentId ) );
                message = className + "- Unsuccessful: RemoveFeaturedItem(): " + ex.Message.ToString();
            }

            return message;

        }//
		#endregion

		#region ====== Retrieval Methods ===============================================
		/// <summary>
		/// Get Content record
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
        public ContentItem Get( int pId )
		{
            DoTrace( 8, className + ".Get( int pId ) " + pId.ToString() );
            return Get( pId, "" );
		}//

        public ContentItem GetByRowId( string pRowId )
        {
            return Get( 0, pRowId );
        }//

        private ContentItem Get( int pId, string pRowId )
        {
            string connectionString = ContentConnectionRO();
            ContentItem entity = new ContentItem();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", pId );
                sqlParameters[ 1 ] = new SqlParameter( "@RowId", pRowId );


                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        DoTrace( 8, className + ".Get( int pId, string pRowId ) doing fill "  );
                        entity = Fill( dr );
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
                LogError( ex, className + ".Get() " );
                entity.Message = "Unsuccessful: " + className + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

		/// <summary>
		/// Retrieve templates for organization/parent
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="parm2"></param>
		/// <returns></returns>
        public DataSet SelectOrgTemplates( int orgId )
		{
			string connectionString = ContentConnectionRO();

			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@OrgId", orgId);

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Content_SelectTemplates]", sqlParameters );

                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, className + ".SelectOrgTemplates() " );
                    return null;

                }
            }
		}

		/// <summary>
		/// Search for Content related data using passed parameters
		/// - uses custom paging
		/// - only requested range of rows will be returned
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
        public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
			if ( pMaximumRows < 1 )
				pMaximumRows = 25;

            string searchProcedure = "ContentSearch";
            int outputCol = 4;
			SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
			sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );

			sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
			sqlParameters[ 2 ].Value = pStartPageIndex;

			sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
			sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( ContentConnectionRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, searchProcedure, sqlParameters );
                    //get output paramter
                    string rows = sqlParameters[ outputCol ].Value.ToString();
                    try
                    {
                        pTotalRows = Int32.Parse( rows );
                    }
                    catch
                    {
                        pTotalRows = 0;
                    }



                    if ( ds.HasErrors )
                    {
                        return null;
                    }
                    return ds;
                }
                catch ( Exception ex )
                {
                    LogError( ex, className + ".Search() " );
                    return null;

                }
            }
		}

		/// <summary>
		/// Fill an Content object from a SqlDataReader
		/// </summary>
		/// <param name="dr">SqlDataReader</param>
		/// <returns>Content</returns>
        public ContentItem Fill( SqlDataReader dr )
		{
			ContentItem entity = new ContentItem();

			entity.IsValid = true;

			entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.ParentId = GetRowColumn( dr, "ParentId", 0 );

			entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "missing" );
            entity.Summary = GetRowColumn( dr, "Summary", "" );
            //entity.IsPublished = GetRowColumn( dr, "IsPublished", false );
			entity.IsActive = GetRowColumn( dr, "IsActive", false );

            entity.TypeId = GetRowColumn( dr, "TypeId", 0 );
            entity.ContentType = GetRowColumn( dr, "ContentType", "" );

            entity.StatusId = GetRowColumn( dr, "StatusId", 0 );
            entity.Status = GetRowColumn( dr, "Status", "" );
            
            entity.PrivilegeTypeId = GetRowColumn( dr, "PrivilegeTypeId", 0 );
            entity.PrivilegeType = GetRowColumn( dr, "PrivilegeType", "" );
            entity.ConditionsOfUseId = GetRowColumn( dr, "ConditionsOfUseId", 0 );
            entity.ConditionsOfUse = GetRowColumn( dr, "ConditionsOfUse", "" );
            entity.ConditionsOfUseIconUrl = GetRowColumn( dr, "ConditionsOfUseIconUrl", "" );
            //need to check for custom - or get both and let interface handle
            //- if custom exists, then ConditionsOfUseUrl will have been set to be the same
            entity.ConditionsOfUseUrl = GetRowColumn( dr, "ConditionsOfUseUrl", "" );
            entity.UsageRightsUrl = GetRowColumn( dr, "UseRightsUrl", "" );

            entity.ResourceIntId = GetRowPossibleColumn( dr, "ResourceIntId", 0 );
			entity.DisplayTemplateId = GetRowPossibleColumn( dr, "DisplayTemplateId", 1 );
            entity.SortOrder = GetRowPossibleColumn( dr, "SortOrder", 10 );
            entity.DocumentUrl = GetRowPossibleColumn( dr, "DocumentUrl", "" );
            entity.Timeframe = GetRowPossibleColumn( dr, "Timeframe", "" );
            entity.ImageUrl = GetRowPossibleColumn( dr, "ImageUrl", "" );

            //entity.IsOrgContentOwner = GetRowColumn( dr, "IsOrgContentOwner", false );
            entity.OrgId = GetRowColumn( dr, "OrgId", 0 );
            entity.Organization = GetRowColumn( dr, "Organization", "" );
			if ( entity.OrgId > 0 )
			{
				entity.ContentOrganization = OrganizationManager.Get( entity.OrgId );
			}
            entity.ParentOrgId = GetRowColumn( dr, "ParentOrgId", 0 );
            entity.ParentOrganization = GetRowColumn( dr, "ParentOrganization", "" );

            //entity.Approved = GetRowPossibleColumn( dr, "Approved", System.DateTime.MinValue );
            //entity.ApprovedById = GetRowPossibleColumn( dr, "ApprovedById", 0 );

			entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
			entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
			entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            entity.Author = GetRowColumn( dr, "Author", "" );
			entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
			entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
			entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            string rowId = GetRowColumn( dr, "RowId", "" );
            //if expecting a RowId and it is not found is probably an error condition and someone should be notified!!
            if ( rowId.Length > 35 )
            {
                entity.RowId = new Guid( rowId );
            }

            rowId = GetRowPossibleColumn( dr, "DocumentRowId", "" );
            if ( rowId.Length == 36 && rowId != ContentItem.DEFAULT_GUID )
            {
                DoTrace( 6, className + ".Fill( SqlDataReader dr ) get related document " );
                entity.DocumentRowId = new Guid( rowId );
                entity.RelatedDocument = DocumentStoreManager.Get( entity.DocumentRowId );
                if ( entity.RelatedDocument != null && entity.RelatedDocument.HasValidRowId() )
                {
                    entity.FileName = entity.RelatedDocument.FileName;
                    entity.FilePath = entity.RelatedDocument.FilePath;
                }
            }
			return entity;
		}//

        #endregion
		#region === Content.Keyword
		
		/// <summary>
		/// Add an Content Keyword record
		/// </summary>
		/// <param name="contentId"></param>
		/// <param name="keyword"></param>
		/// <param name="createdById"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int ContentKeyword_Create( int contentId, string keyword, int createdById, ref string statusMessage )
		{
			int newId = 0;
			try
			{
				#region parameters
				SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
				sqlParameters[ 0 ] = new SqlParameter( "@ContentId", contentId );
				sqlParameters[ 1 ] = new SqlParameter( "@Keyword", keyword );
				sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", createdById );

				#endregion

				SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, "[Content.KeywordInsert]", sqlParameters );
				if ( dr.HasRows )
				{
					dr.Read();
					//newId will be zeroes if the keyword being added exists in other tables
					int.TryParse( dr[ 0 ].ToString(), out newId);
				}
				dr.Close();
				dr = null;
				//only set on errors
				statusMessage = "";
			}
			catch ( Exception ex )
			{
				LogError( ex, className + string.Format( ".ContentKeyword_Create() for contentId: {0}, Value: {1}, and CreatedBy: {2}", contentId, keyword, createdById ) );
				statusMessage = className + "- Unsuccessful: ContentKeyword_Create(): " + ex.Message.ToString();
			}

			return newId;
		}

		/// <summary>
		/// Delete a single Content.Keywork record
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool ContentKeyword_Delete( int pId, ref string statusMessage )
		{
			bool successful = false;

			SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
			sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
			sqlParameters[ 0 ].Value = pId;
			sqlParameters[ 1 ] = new SqlParameter( "@ContentId", SqlDbType.Int );
			sqlParameters[ 1 ].Value = 0;
			try
			{
				SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, "[Content.KeywordDelete]", sqlParameters );
				successful = true;
			}
			catch ( Exception ex )
			{
				LogError( ex, className + ".ContentKeyword_Delete() " );
				statusMessage = className + "- Unsuccessful: ContentKeyword_Delete(): " + ex.Message.ToString();

				successful = false;
			}
			return successful;
		}
		/// <summary>
		/// Delete all Content.keyword records for the content item
		/// </summary>
		/// <param name="pContentId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool ContentKeyword_DeleteAll( int pContentId, ref string statusMessage )
		{
			bool successful = false;

			SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
			sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
			sqlParameters[ 0 ].Value = 0;
			sqlParameters[ 1 ] = new SqlParameter( "@ContentId", SqlDbType.Int );
			sqlParameters[ 1 ].Value = pContentId;
			try
			{
				SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, "[Content.KeywordDelete]", sqlParameters );
				successful = true;
			}
			catch ( Exception ex )
			{
				LogError( ex, className + ".ContentKeyword_Delete() " );
				statusMessage = className + "- Unsuccessful: ContentKeyword_Delete(): " + ex.Message.ToString();

				successful = false;
			}
			return successful;
		}
		public static List<ContentKeyword> ContentKeyword_Select( int pContentId )
		{

			List<ContentKeyword> collection = new List<ContentKeyword>();
			ContentKeyword entity = new ContentKeyword();
			SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
			sqlParameters[ 0 ] = new SqlParameter( "@ContentId", pContentId );

			DataSet ds = new DataSet();
			try
			{
				ds = SqlHelper.ExecuteDataset( ContentConnectionRO(), CommandType.StoredProcedure, "[Content.KeywordSelect]", sqlParameters );

				if ( DoesDataSetHaveRows( ds ) )
				{
					foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
					{
						entity = new ContentKeyword();
						entity.Id = GetRowColumn( dr, "Id", 0 );
						entity.ContentId = GetRowColumn( dr, "ContentId", 0 );
						entity.Keyword = GetRowColumn( dr, "Keyword", "" );
						entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
						entity.Created = GetRowColumn( dr, "Created", DateTime.Now );
						collection.Add( entity );
					}
				}
				return collection;
			}
			catch ( Exception ex )
			{
				LogError( ex, className + ".Select( int pResourceIntId ) " );
				return null;

			}
		}

		#endregion
		#region ====== ContentSupplement Section ===============================================
		public bool ContentSupplementDelete( int pId, ref string statusMessage )
        {
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pId;

            try
            {
                SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, "[Content.SupplementDelete]", sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentSupplementDelete() " );
                statusMessage = className + "- Unsuccessful: ContentSupplementDelete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }

        /// <summary>
        /// Add an Content record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentSupplementCreate( ContentSupplement entity, ref string statusMessage )
        {
            int newId = 0;
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 8 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ParentId", SqlDbType.Int );
                sqlParameters[ 0 ].Value = entity.ParentId;

                sqlParameters[ 1 ] = new SqlParameter( "@Title", SqlDbType.VarChar );
                sqlParameters[ 1 ].Size = 200;
                sqlParameters[ 1 ].Value = entity.Title;

                sqlParameters[ 2 ] = new SqlParameter( "@Description", SqlDbType.VarChar );
                sqlParameters[ 2 ].Size = -1;
                sqlParameters[ 2 ].Value = entity.Description;

                sqlParameters[ 3 ] = new SqlParameter( "@ResourceUrl", SqlDbType.VarChar );
                sqlParameters[ 3 ].Size = 200;
                sqlParameters[ 3 ].Value = entity.ResourceUrl;

                sqlParameters[ 4 ] = new SqlParameter( "@PrivilegeTypeId", SqlDbType.Int );
                sqlParameters[ 4 ].Value = entity.PrivilegeTypeId;

                sqlParameters[ 5 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
                sqlParameters[ 5 ].Value = entity.IsActive;

                sqlParameters[ 6 ] = new SqlParameter( "@CreatedById", SqlDbType.Int );
                sqlParameters[ 6 ].Value = entity.CreatedById;
                sqlParameters[ 7 ] = new SqlParameter( "@DocumentRowId", entity.DocumentRowId.ToString() );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, "[Content.SupplementInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                    entity.Id = newId;
                    entity.IsValid = true;
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                //provide helpful info about failing entity
                LogError( ex, className + string.Format( ".ContentSupplementCreate() for title: {0} and CreatedBy: {1}", entity.Title, entity.CreatedById ) );
                statusMessage = className + "- Unsuccessful: ContentSupplementCreate(): " + ex.Message.ToString();

                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        public string ContentSupplementUpdate( ContentSupplement entity )
		{
			string message = "successful";

			try
			{

				#region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 8 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", SqlDbType.Int );
                sqlParameters[ 0 ].Value = entity.Id;

                sqlParameters[ 1 ] = new SqlParameter( "@Title", SqlDbType.VarChar );
                sqlParameters[ 1 ].Size = 200;
                sqlParameters[ 1 ].Value = entity.Title;

                sqlParameters[ 2 ] = new SqlParameter( "@Description", SqlDbType.VarChar );
                sqlParameters[ 2 ].Size = -1;
                sqlParameters[ 2 ].Value = entity.Description;

                sqlParameters[ 3 ] = new SqlParameter( "@ResourceUrl", SqlDbType.VarChar );
                sqlParameters[ 3 ].Size = 200;
                sqlParameters[ 3 ].Value = entity.ResourceUrl;

                sqlParameters[ 4 ] = new SqlParameter( "@PrivilegeTypeId", SqlDbType.Int );
                sqlParameters[ 4 ].Value = entity.PrivilegeTypeId;

                sqlParameters[ 5 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
                sqlParameters[ 5 ].Value = entity.IsActive;

                sqlParameters[ 6 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById );
                sqlParameters[ 7 ] = new SqlParameter( "@DocumentRowId", entity.DocumentRowId.ToString() );

				#endregion

				SqlHelper.ExecuteNonQuery( ContentConnection(), "[Content.SupplementUpdate]", sqlParameters );
				message = "successful";

			}
			catch ( Exception ex )
			{
				LogError( ex, className + string.Format( ".ContentSupplementUpdate() for Id: {0} and ???: {1} and LastUpdatedBy: {2}", entity.Id.ToString(), entity.Title.ToString(), entity.LastUpdatedBy ) );

				message = className + "- Unsuccessful: ContentSupplementUpdate(): " + ex.Message.ToString();
				entity.Message = message;
				entity.IsValid = false;
			}

			return message;

		}//

        public ContentSupplement ContentSupplementGet( int id )
        {
            string connectionString = ContentConnectionRO();
            ContentSupplement entity = new ContentSupplement();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", id );
                sqlParameters[ 1 ] = new SqlParameter( "@RowId", "" );
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Content.SupplementGet]", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = ContentSupplementFill( dr );
                    }
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentSupplementGet() " );
                entity.Message = "Unsuccessful: " + className + ".ContentSupplementGet(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//
        public ContentSupplement ContentSupplementGetByRowId( string id )
        {
            string connectionString = ContentConnectionRO();
            ContentSupplement entity = new ContentSupplement();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", 0 );
                sqlParameters[ 1 ] = new SqlParameter( "@RowId", id );
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Content.SupplementGet]", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = ContentSupplementFill( dr );
                    }
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentSupplementGetByRowId() " );
                entity.Message = "Unsuccessful: " + className + ".ContentSupplementGetByRowId(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//
        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public List<ContentSupplement> ContentSupplementsSelectList( int parentId )
        {
            List<ContentSupplement> collection = new List<ContentSupplement>();
            DataSet ds = ContentSupplementsSelect( parentId );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    ContentSupplement entity = ContentSupplementFill( dr, false );
                    collection.Add( entity );
                }
            }

            return collection;
        }

        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet ContentSupplementsSelect( int parentId )
        {
            string connectionString = ContentConnectionRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ParentId", parentId );
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[Content.SupplementSelect]", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentSupplementsSelect() " );
                return null;

            }
        }

        public ContentSupplement ContentSupplementFill( SqlDataReader dr )
        {
            ContentSupplement entity = new ContentSupplement();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
			entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "missing" );
			//entity.IsActive = GetRowColumn( dr, "IsActive", false );

            entity.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );

            entity.ParentId = GetRowColumn( dr, "ParentId", 0 );
            
            entity.PrivilegeTypeId = GetRowColumn( dr, "PrivilegeTypeId", 0 );
            entity.PrivilegeType = GetRowColumn( dr, "PrivilegeType", "" );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            string rowId = GetRowColumn( dr, "DocumentRowId", "" );
            //if expecting a RowId and it is not found is probably an error condition and someone should be notified!!
            if ( rowId.Length > 35 )
                entity.DocumentRowId = new Guid( rowId );
            return entity;
        }//
        public ContentSupplement ContentSupplementFill( DataRow dr, bool fillLazy )
        {
            ContentSupplement entity = new ContentSupplement();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "missing" );
            //entity.IsActive = GetRowColumn( dr, "IsActive", false );

            entity.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );

            entity.ParentId = GetRowColumn( dr, "ParentId", 0 );

            entity.PrivilegeTypeId = GetRowColumn( dr, "PrivilegeTypeId", 0 );
            entity.PrivilegeType = GetRowColumn( dr, "PrivilegeType", "" );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            string rowId = GetRowColumn( dr, "DocumentRowId", "" );
            //if expecting a RowId and it is not found is probably an error condition and someone should be notified!!
            if ( rowId.Length > 35 )
            {
                entity.DocumentRowId = new Guid( rowId );
                if ( fillLazy == false )
                {
                    //fill document
                    entity.RelatedDocument = DocumentStoreManager.Get( rowId );

                }
            }
            return entity;
        }//

        #endregion


        #region ====== ContentReference Section ===============================================
        public bool ContentReferenceDelete( int pId, ref string statusMessage )
        {
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@id", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pId;

            try
            {
                SqlHelper.ExecuteNonQuery( ContentConnection(), CommandType.StoredProcedure, "[Content.ReferenceDelete]", sqlParameters );
                successful = true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentReferenceDelete() " );
                statusMessage = className + "- Unsuccessful: ContentReferenceDelete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }
        /// <summary>
        /// Add an Content record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentReferenceCreate( ContentReference entity, ref string statusMessage )
        {
            int newId = 0;
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 8 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ParentId", entity.ParentId);

                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title);
                sqlParameters[ 2 ] = new SqlParameter( "@Author", entity.Author);
                sqlParameters[ 3 ] = new SqlParameter( "@Publisher", entity.Publisher );
                sqlParameters[ 4 ] = new SqlParameter( "@ISBN", entity.ISBN );
                sqlParameters[ 5 ] = new SqlParameter( "@ReferenceUrl", entity.ReferenceUrl );
                sqlParameters[ 6 ] = new SqlParameter( "@AdditionalInfo", entity.AdditionalInfo );
                sqlParameters[ 7 ] = new SqlParameter( "@CreatedById", entity.CreatedById);

                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, "[Content.ReferenceInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                    entity.Id = newId;
                    entity.IsValid = true;
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                //provide helpful info about failing entity
                LogError( ex, className + string.Format( ".ContentReferenceCreate() for title: {0} and CreatedBy: {1}", entity.Title, entity.CreatedById ) );
                statusMessage = className + "- Unsuccessful: ContentReferenceCreate(): " + ex.Message.ToString();

                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        public string ContentReferenceUpdate( ContentReference entity )
        {
            string message = "successful";

            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 8 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", SqlDbType.Int );
                sqlParameters[ 0 ].Value = entity.Id;

                sqlParameters[ 1 ] = new SqlParameter( "@Title", entity.Title );
                sqlParameters[ 2 ] = new SqlParameter( "@Author", entity.Author );
                sqlParameters[ 3 ] = new SqlParameter( "@Publisher", entity.Publisher );
                sqlParameters[ 4 ] = new SqlParameter( "@ISBN", entity.ISBN );
                sqlParameters[ 5 ] = new SqlParameter( "@ReferenceUrl", entity.ReferenceUrl );
                sqlParameters[ 6 ] = new SqlParameter( "@AdditionalInfo", entity.AdditionalInfo );
                sqlParameters[ 7 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById);

                #endregion

                SqlHelper.ExecuteNonQuery( ContentConnection(), "[Content.ReferenceUpdate]", sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".ContentReferenceUpdate() for Id: {0} and and LastUpdatedBy: {2}", entity.Id.ToString(), entity.LastUpdatedBy ) );

                message = className + "- Unsuccessful: ContentReferenceUpdate(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//

        public ContentReference ContentReferenceGet( int id )
        {
            string connectionString = ContentConnectionRO();
            ContentReference entity = new ContentReference();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", id );
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[Content.ReferenceGet]", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = ContentReferenceFill( dr );
                    }
                }
                dr.Close();
                dr = null;
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentReferenceGet() " );
                entity.Message = "Unsuccessful: " + className + ".ContentReferenceGet(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//


        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public List<ContentReference> ContentReferencesSelectList( int parentId )
        {
            List<ContentReference> collection = new List<ContentReference>();
            DataSet ds = ContentReferencesSelect( parentId );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    ContentReference entity = ContentReferenceFill( dr );
                    collection.Add( entity );
                }
            }

            return collection;
        }
        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet ContentReferencesSelect( int parentId )
        {
            string connectionString = ContentConnectionRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ParentId", parentId );
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[Content.ReferenceSelect]", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentReferencesSelect() " );
                return null;

            }
        }
        public ContentReference ContentReferenceFill( SqlDataReader dr )
        {
            ContentReference entity = new ContentReference();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.Title = GetRowColumn( dr, "Title", "" );
            entity.Author = GetRowColumn( dr, "Author", "" );
            entity.Publisher = GetRowColumn( dr, "Publisher", "" );
            entity.ISBN = GetRowColumn( dr, "ISBN", "" );
            entity.ReferenceUrl = GetRowColumn( dr, "ReferenceUrl", "" );
            entity.AdditionalInfo = GetRowColumn( dr, "AdditionalInfo", "" );

            entity.ParentId = GetRowColumn( dr, "ParentId", 0 );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            return entity;
        }//

        public ContentReference ContentReferenceFill( DataRow dr )
        {
            ContentReference entity = new ContentReference();

            entity.IsValid = true;

            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.Title = GetRowColumn( dr, "Title", "" );
            entity.Author = GetRowColumn( dr, "Author", "" );
            entity.Publisher = GetRowColumn( dr, "Publisher", "" );
            entity.ISBN = GetRowColumn( dr, "ISBN", "" );
            entity.ReferenceUrl = GetRowColumn( dr, "ReferenceUrl", "" );
            entity.AdditionalInfo = GetRowColumn( dr, "AdditionalInfo", "" );

            entity.ParentId = GetRowColumn( dr, "ParentId", 0 );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            //entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );
            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            //entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            return entity;
        }//


        #endregion


        #region ====== Content History =====================================
        public int ContentHistory_Create( int pContentId, string pAction, string pDescription, int pCreatedById, ref string statusMessage )
        {
            int newId = 0;
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ContentId", pContentId );
                sqlParameters[ 1 ] = new SqlParameter( "@Action", pAction );
                sqlParameters[ 2 ] = new SqlParameter( "@Description", pDescription );
                sqlParameters[ 3 ] = new SqlParameter( "@CreatedById", pCreatedById );

                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ContentConnection(), CommandType.StoredProcedure, "[Content.HistoryInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = int.Parse( dr[ 0 ].ToString() );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                //provide helpful info about failing entity
                LogError( ex, className + string.Format( ".ContentHistory_Create() for ContentId: {0} and CreatedBy: {1}", pContentId, pCreatedById ) );
                statusMessage = className + "- Unsuccessful: ContentHistory_Create(): " + ex.Message.ToString();
            }

            return newId;
        }
        public DataSet ContentHistory_Select( int contentId )
        {
            string connectionString = ContentConnectionRO();

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ContentId", contentId );
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[Content.HistorySelect]", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentHistory_Select() " );
                return null;

            }
        }
        #endregion


        #region ====== Codes ===============================================
        public DataSet ContentType_Select()
        {
            string connectionString = ContentConnectionRO();
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[ContentTypeSelect]" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentType_Select() " );
                return null;

            }
        }

        
        public DataSet ContentStatusCodes_Select()
        {
            string connectionString = ContentConnectionRO();
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[Codes.ContentStatusSelect]" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentStatusCodes_Select() " );
                return null;

            }
        }
        public DataSet ContentPrivilegeCodes_Select()
        {
            string connectionString = ContentConnectionRO();
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "[Codes.ContentPrivilegeSelect]" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ContentPrivilegeSelect() " );
                return null;

            }
        }
        #endregion

    }
}
