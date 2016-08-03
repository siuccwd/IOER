using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.Business;
using LRWarehouse.Business.ResourceV2;
using MyEntity = LRWarehouse.Business.ResourceChildItem;
using MyEntityCollection = System.Collections.Generic.List<LRWarehouse.Business.ResourceChildItem>;
using CodeGradeLevel = LRWarehouse.Business.CodeGradeLevel;
using CodeGradeLevelCollection = LRWarehouse.Business.CodeGradeLevelCollection;

namespace LRWarehouse.DAL
{
    public class CodeTableManager : BaseDataManager
    {
        static string thisClassName = "CodeTableManager";

        #region generic
        public static DataSet SelectCodes( string resourceTable, string titleField)
        {
            return SelectCodes( resourceTable, titleField, "Title" );
        }

        public static DataSet SelectCodes( string resourceTable, string titleField, string sortField )
        {
            if ( sortField == null || sortField.Trim().Length == 0 )
                sortField = "Title";
            return SelectCodesWithValues( resourceTable, titleField, sortField, false );
        }

        
        public static DataSet SelectCodesWithValues( string resourceTable, string titleField )
        {
            return SelectCodesWithValues( resourceTable, titleField, "Title", true );
        }

        public static DataSet SelectCodesExternalSite(string externalSite, int id, ref string statusMessage)
        {
            statusMessage = "successful";
            DataSet ds = null;
            try
            {
                #region parameters
                SqlParameter[] parms = new SqlParameter[2];
                parms[0] = new SqlParameter("@Id", id);
                parms[1] = new SqlParameter("@ExternalSite", externalSite);
                #endregion

                ds = SqlHelper.ExecuteDataset(LRWarehouseRO(), CommandType.StoredProcedure, "[Codes.ExternalSiteSelect]", parms);
            }
            catch (Exception ex)
            {
                LogError(ex, thisClassName + string.Format(".SelectCodesExternalSite() for ExternalSite: {0} and Id: {1}", externalSite, id));
                statusMessage = thisClassName + "- Unsuccessful: ExternalAccount_Create(): " + ex.Message.ToString();
            }

            return ds;
        }

        private static DataSet SelectCodesWithValues( string resourceTable, string titleField, string sortField, bool mustHaveValues )
        {
            if ( sortField == null || sortField.Trim().Length == 0 )
                sortField = "Title";

            string sql = string.Format( "SELECT [Id], [Title], [Title] + ' (' + isnull(convert(varchar,WareHouseTotal),0) + ')' As FormattedTitle FROM [{0}] Where IsActive = 1 ORDER BY {1}", resourceTable, sortField );
            string sql2 = string.Format( "SELECT [Id], [Title], [Title] + ' (' + isnull(convert(varchar,WareHouseTotal),0) + ')' As FormattedTitle FROM [{0}] Where IsActive = 1 && WareHouseTotal > 0 ORDER BY {1}", resourceTable, sortField );
            DataSet ds;
            if (mustHaveValues)
                ds= DatabaseManager.DoQuery( sql );
            else
                ds = DatabaseManager.DoQuery( sql2 );

            return ds;
        }

		#endregion

		#region  [Codes_TagValue]

		/// <summary>
		/// Return all active Code.TagValue rows
		/// </summary>
		/// <param name="pWithValuesOnly">Set to true to only return codes with warehouse value > 0</param>
		/// <returns></returns>
		public static DataSet Codes_TagValue_GetAll( bool pWithValuesOnly )
		{
			return Codes_TagValue_GetAll( 0, pWithValuesOnly );
		}
		/// <summary>
		/// Retrieve all Code.TagValue rows, or all for an resource
		/// </summary>
		/// <param name="resourceId">Provide a resourceId to only return codes used with the resource</param>
		/// <param name="pWithValuesOnly">Set to true to only return codes with warehouse value > 0</param>
		/// <returns></returns>
		public static DataSet Codes_TagValue_GetAll( int resourceId, bool pWithValuesOnly )
		{
			SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
			sqlParameters[ 0 ] = new SqlParameter( "@resourceID", resourceId );
			sqlParameters[ 1 ] = new SqlParameter( "@WithValuesOnly", pWithValuesOnly );
			

			using ( SqlConnection conn = new SqlConnection( LRWarehouseRO() ) )
			{
				DataSet ds = new DataSet();
				try
				{
					ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[Codes.TagValueGetAll]", sqlParameters );
					

					if ( ds.HasErrors )
					{
						return null;
					}
					return ds;
				}
				catch ( Exception ex )
				{
					LogError( ex, thisClassName + ".Codes_TagValue_GetAll() " );
					return null;
				}
			}
		}




		/// <summary>
		/// ??????????????????????? looks general, but hard-coded to grade level
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
		private static CodesTagValue CodesTagValue_Get( int pId )
		{
			string connectionString = GetReadOnlyConnection();
			CodesTagValue entity = new CodesTagValue();
			try
			{
				SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
				sqlParameters[ 0 ] = new SqlParameter( "@id", pId );

				DataSet ds = SqlHelper.ExecuteDataset( connectionString, "[Codes.GradeLevelGet]", sqlParameters );

				if ( DoesDataSetHaveRows( ds ) )
				{
					// it should return only one record.
					DataRow dr = ds.Tables[0].Rows[0];
					entity.Id = GetRowColumn(dr, "Id", 0);
					entity.CodeId = GetRowColumn( dr, "CodeId", 0 );
					entity.CategoryId = GetRowColumn( dr, "CategoryId", 0 );
					entity.Title = GetRowColumn(dr, "Title", "");
					entity.Description = GetRowColumn( dr, "Description", "" );
					entity.SortOrder = GetRowColumn( dr, "SortOrder", 10 );
					entity.IsActive = GetRowColumn( dr, "IsActive", false );
					entity.SchemaTag = GetRowColumn( dr, "SchemaTag", "" );
					entity.AliasValues = GetRowColumn( dr, "AliasValues", "" );
					entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );

					entity.Codes_TagCategory = CodesTagCategory_Get( entity.CategoryId );
			
				}
				else
				{
					
				}
				return entity;

			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".CodesTagValue_Get() " );
				return entity;
			}

		}//


		private static CodesTagCategory CodesTagCategory_Get( int pId )
		{
			string connectionString = GetReadOnlyConnection();
			CodesTagCategory entity = new CodesTagCategory();
			try
			{
				SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
				sqlParameters[ 0 ] = new SqlParameter( "@id", pId );

				DataSet ds = SqlHelper.ExecuteDataset( connectionString, "[Codes.GradeLevelGet]", sqlParameters );

				if ( DoesDataSetHaveRows( ds ) )
				{
					// it should return only one record.
					DataRow dr = ds.Tables[ 0 ].Rows[ 0 ];
					entity.Id = GetRowColumn( dr, "Id", 0 );
					entity.Title = GetRowColumn( dr, "Title", "" );
					entity.Description = GetRowColumn( dr, "Description", "" );
					entity.SortOrder = GetRowColumn( dr, "SortOrder", 10 );
					entity.IsActive = GetRowColumn( dr, "IsActive", false );
					entity.SchemaTag = GetRowColumn( dr, "SchemaTag", "" );
					//entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );

				}
				else
				{

				}
				return entity;

			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".CodesTagCategory_Get() " );
				return entity;

			}

		}//

		public static DataSet CodesTagCategory_Select()
		{
			try
			{
				
				DataSet ds = DatabaseManager.DoQuery( "SELECT [Id], [Title], [SchemaTag], [SortOrder] FROM [Codes.TagCategory] WHERE IsActive = 1 ORDER BY [SortOrder], [Title]" );
				
				return ds;

			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".CodesTagCategory_Select() " );
				return null;

			}

		}//
		public static DataSet Codes_SiteTagCategory_Select()
		{
			try
			{
				DataSet ds = DatabaseManager.DoQuery( "SELECT [SiteId], [CategoryId], [SortOrder] FROM [Codes.SiteTagCategory] WHERE [IsActive] = 1 ORDER BY [SortOrder]" );

				return ds;

			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".Codes_SiteTagCategory_Select() " );
				return null;

			}

		}//
		#endregion

		#region  [ConditionOfUse]
		public static DataSet ConditionsOfUse_Select()
        {
            return ConditionsOfUse_Select( false );
        }//
		public static DataSet ConditionsOfUse_Select( bool forNewContentOnly )
		{
			string connectionString = GetReadOnlyConnection();
			DataSet ds = new DataSet();
			try
			{
				SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
				sqlParameters[ 0 ] = new SqlParameter( "@IsAllowedForNewResource", forNewContentOnly );

				ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), "[ConditionsOfUse_Select]", sqlParameters );
				if ( ds.HasErrors )
				{
					return null;
				}
				return ds;

			}
			catch ( Exception ex )
			{
				LogError( ex, thisClassName + ".ConditionsOfUse_Select() " );

				return null;
			}

		}//
        #endregion

        #region  GradeLevel
		public static DataSet DeterminingAgeRanges( ResourceDTO input )
		{
			DataSet ageRangeDS = DatabaseManager.DoQuery(
					"SELECT codes.[Id], codes.[FromAge], codes.[ToAge], tags.[Id] AS TagId, tags.Title " +
					"FROM [Codes.GradeLevel] codes " +
					"LEFT JOIN [Codes.TagValue] tags ON tags.CodeId = codes.Id " +
					"WHERE tags.CategoryId = " + input.Fields.Where( m => m.Schema == "gradeLevel" ).FirstOrDefault().Id + " AND tags.IsActive = 1"
					);

			return ageRangeDS;
		} //
        public static CodeGradeLevel GradeLevelGet( int pId )
        {
            return GradeLevelGet( pId, "", "", "" );

        }//

        public static CodeGradeLevel GradeLevelGetByTitle( string title )
        {
            return GradeLevelGet( 0, title, "", "" );

        }//

        public static CodeGradeLevel GradeLevelGetByDesc( string desc )
        {
            return GradeLevelGet( 0, "", desc, "" );

        }//
        public static CodeGradeLevel GradeLevelGetByUrl( string url )
        {
            return GradeLevelGet( 0, "", "", url );

        }//

        /// <summary>
        /// Get GradeLevel record
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        private static CodeGradeLevel GradeLevelGet( int pId, string title, string desc, string url )
        {
            string connectionString = GetReadOnlyConnection();
            CodeGradeLevel entity = new CodeGradeLevel();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@id", pId);
                sqlParameters[ 1 ] = new SqlParameter( "@title", title );
                sqlParameters[ 2 ] = new SqlParameter( "@descriptions", desc );
                sqlParameters[ 3 ] = new SqlParameter( "@url", url );

                DataSet ds = SqlHelper.ExecuteDataset( connectionString, "[Codes.GradeLevelGet]", sqlParameters );

                if ( DoesDataSetHaveRows(ds) )
                {
                    // it should return only one record.
                    entity = GradeLevelFill( ds.Tables[0].Rows[0] );
                }
                else
                {
                    entity.Message = "Record not found";
                    entity.IsValid = false;
                }
                return entity;

            }
            catch ( Exception ex )
            {
                LogError( ex, thisClassName + ".Get() " );
                entity.Message = "Unsuccessful: " + thisClassName + ".Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        public static CodeGradeLevelCollection GradeLevelGetByAgeRange(int fromAge, int toAge, bool isEducationBand, ref string status)
        {
            string connectionString = LRWarehouseRO();
            status = "successful";
            CodeGradeLevelCollection collection = new CodeGradeLevelCollection();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[3];
                sqlParameters[0] = new SqlParameter("@MinAge", fromAge);
                sqlParameters[1] = new SqlParameter("@MaxAge", toAge);
                sqlParameters[2] = new SqlParameter("@IsEducationBand", isEducationBand);

                DataSet ds = SqlHelper.ExecuteDataset(connectionString, "[Codes.GradeLevelSelectByAgeRange]", sqlParameters);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        CodeGradeLevel gradeLevel = GradeLevelFill(dr);
                        collection.Add(gradeLevel);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(thisClassName + ".GradeLevelGetByAgeRange(): " + ex.ToString());
                status = ex.Message;
            }

            return collection;
        }

        private static CodeGradeLevel GradeLevelFill(DataRow dr)
        {
            CodeGradeLevel entity = new CodeGradeLevel();
            entity.Id = GetRowColumn(dr, "Id", 0);
            entity.Title = GetRowColumn(dr, "Title", "");
            entity.AgeLevel = GetRowColumn(dr, "AgeLevel", 0);
            entity.Description = GetRowColumn(dr, "Description", "");
            entity.IsPathwaysLevel = GetRowColumn(dr, "IsPathwaysLevel", false);
            entity.AlignmentUrl = GetRowColumn(dr, "AlignmentUrl", "");
            entity.SortOrder = GetRowColumn(dr, "SortOrder", 0);
            entity.WarehouseTotal = GetRowColumn(dr, "WarehouseTotal", 0);
            entity.GradeRange = GetRowColumn(dr, "GradeRange", "");
            entity.GradeGroup = GetRowColumn(dr, "GradeGroup", "");
            entity.IsActive = GetRowColumn(dr, "IsActive", false);
            entity.IsEducationBand = GetRowColumn(dr, "IsEducationBand", false);
            entity.FromAge = GetRowColumn(dr, "FromAge", 0);
            entity.ToAge = GetRowColumn(dr, "ToAge", 0);

            return entity;
        }
        #endregion

        #region Mapping tables ===

        /// <summary>
        /// Search for Map.CareerCluster data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet MapCareerCluster_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            int outputCol = 4;
            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( LRWarehouseRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[MapClusters.Search]", sqlParameters );
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
                    LogError( ex, thisClassName + ".MapCareerCluster_Search() " );
                    return null;
                }
            }
        }


        /// <summary>
        /// Search for Map.K12Subject data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static DataSet MapK12Subject_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            int outputCol = 4;
            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", pFilter );
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", pOrderBy );
            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", pStartPageIndex );
            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", pMaximumRows );

            sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

            using ( SqlConnection conn = new SqlConnection( LRWarehouseRO() ) )
            {
                DataSet ds = new DataSet();
                try
                {
                    ds = SqlHelper.ExecuteDataset( conn, CommandType.StoredProcedure, "[MapK12Subject.Search]", sqlParameters );
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
                    LogError( ex, thisClassName + ".MapK12Subject_Search() " );
                    return null;

                }
            }
        }
        #endregion

        #region AccessibilityHazard
        public static DataSet GetAccessibilityHazardCodes()
        {
            string sql = string.Format("SELECT Id, Title, Description, IsActive, WarehouseTotal, AntonymId, schemaValue, SortOrder FROM [Codes.AccessibilityHazard] WHERE IsActive = 'True' ORDER BY SortOrder");
            return DatabaseManager.DoQuery(sql);
        }
        #endregion

    }
}
