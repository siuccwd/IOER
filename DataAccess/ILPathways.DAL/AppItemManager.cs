using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;
using ILPathways.Business;
using ILPathways.Common;
using MyEntity = ILPathways.Business.AppItem;

namespace ILPathways.DAL
{
    /// <summary>
    /// Data access manager for AppItem
    /// </summary>
    public class AppItemManager : BaseDataManager
    {
        static string className = "AppItemManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "AppItemGet";
        const string SELECT_PROC = "AppItemSelect";
        const string SELECT_RELATED_PROC = "AppItemSelectRelated";
        const string DELETE_PROC = "AppItemDelete";
        const string INSERT_PROC = "AppItemInsert";
        const string UPDATE_PROC = "AppItemUpdate";


        /// <summary>
        /// Default constructor
        /// </summary>
        public AppItemManager()
        { }//

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Retrieve database connection string for SqlQuery table
        /// </summary>
        /// <returns>Connection string</returns>
        private static string GetMyConnectionString()
        {
            string connectionString = "";

            //if ( GetAppKeyValue( "isAppItemUsingworkNetCommon", "no" ) == "yes" )
            //{
            //  connectionString = GetApplicationCommonCon();
            //} else
            //{
            connectionString = ContentConnection();
            //}

            return connectionString;

        }// 

        /// <summary>
        /// Delete an AppItem record
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( string pRowId, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            bool successful = false;

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = new Guid( pRowId );

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
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
        /// Add an AppItem record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create( AppItem entity, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            string newId = "";

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 29 ];
            sqlParameters[ 0 ] = new SqlParameter( "@VersionNbr", SqlDbType.Int );
            sqlParameters[ 0 ].Value = entity.VersionNbr;

            sqlParameters[ 1 ] = new SqlParameter( "@SequenceNbr", SqlDbType.Int );
            sqlParameters[ 1 ].Value = entity.SequenceNbr;

            sqlParameters[ 2 ] = new SqlParameter( "@ParentRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 2 ].Value = entity.ParentRowId;

            sqlParameters[ 3 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
            sqlParameters[ 3 ].Value = entity.TypeId;

            sqlParameters[ 4 ] = new SqlParameter( "@Title", SqlDbType.VarChar );
            sqlParameters[ 4 ].Size = 200;
            sqlParameters[ 4 ].Value = entity.Title;

            sqlParameters[ 5 ] = new SqlParameter( "@Description", SqlDbType.VarChar );
            sqlParameters[ 5 ].Size = -1;
            sqlParameters[ 5 ].Value = entity.Description;

            sqlParameters[ 6 ] = new SqlParameter( "@AppItemCode", SqlDbType.VarChar );
            sqlParameters[ 6 ].Size = 50;
            sqlParameters[ 6 ].Value = entity.AppItemCode;

            sqlParameters[ 7 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 7 ].Size = 50;
            sqlParameters[ 7 ].Value = entity.Category;

            sqlParameters[ 8 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 8 ].Size = 50;
            sqlParameters[ 8 ].Value = entity.Subcategory;

            sqlParameters[ 9 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
            sqlParameters[ 9 ].Size = 50;
            sqlParameters[ 9 ].Value = entity.Status;

            sqlParameters[ 10 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
            sqlParameters[ 10 ].Value = entity.IsActive;

            sqlParameters[ 11 ] = new SqlParameter( "@StartDate", SqlDbType.DateTime );
            if ( entity.StartDate == DateTime.MinValue )
                sqlParameters[ 11 ].Value = DBNull.Value;
            else
                sqlParameters[ 11 ].Value = entity.StartDate;

            sqlParameters[ 12 ] = new SqlParameter( "@EndDate", SqlDbType.DateTime );
            if ( entity.EndDate == DateTime.MinValue )
                sqlParameters[ 12 ].Value = DBNull.Value;
            else
                sqlParameters[ 12 ].Value = entity.EndDate;

            sqlParameters[ 13 ] = new SqlParameter( "@ExpiryDate", SqlDbType.DateTime );
            if ( entity.ExpiryDate == DateTime.MinValue )
                sqlParameters[ 13 ].Value = DBNull.Value;
            else
                sqlParameters[ 13 ].Value = entity.ExpiryDate;

            sqlParameters[ 14 ] = new SqlParameter( "@CreatedById", SqlDbType.Int );
            sqlParameters[ 14 ].Value = entity.CreatedById;

            sqlParameters[ 15 ] = new SqlParameter( "@Approved", SqlDbType.DateTime );
            if ( entity.Approved == DateTime.MinValue )
                sqlParameters[ 15 ].Value = DBNull.Value;
            else
                sqlParameters[ 15 ].Value = entity.Approved;

            sqlParameters[ 16 ] = new SqlParameter( "@ApprovedById", SqlDbType.Int );
            sqlParameters[ 16 ].Value = entity.ApprovedById;

            sqlParameters[ 17 ] = new SqlParameter( "@RelatedObjectRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 17 ].Value = entity.RelatedObjectRowId;

            sqlParameters[ 18 ] = new SqlParameter( "@ImageId", SqlDbType.Int );
            sqlParameters[ 18 ].Value = entity.ImageId;

            sqlParameters[ 19 ] = new SqlParameter( "@String1", SqlDbType.VarChar );
            sqlParameters[ 19 ].Size = 200;
            sqlParameters[ 19 ].Value = entity.String1;
            sqlParameters[ 20 ] = new SqlParameter( "@String2", SqlDbType.VarChar );
            sqlParameters[ 20 ].Size = 200;
            sqlParameters[ 20 ].Value = entity.String2;
            sqlParameters[ 21 ] = new SqlParameter( "@BigString1", SqlDbType.VarChar );
            sqlParameters[ 21 ].Size = -1;
            sqlParameters[ 21 ].Value = entity.BigString1;
            sqlParameters[ 22 ] = new SqlParameter( "@BigString2", SqlDbType.VarChar );
            sqlParameters[ 22 ].Size = -1;
            sqlParameters[ 22 ].Value = entity.BigString2;

            //mp 11-03-10
            sqlParameters[ 23 ] = new SqlParameter( "@DocumentRowId", SqlDbType.VarChar );
            sqlParameters[ 23 ].Size = 50;
            sqlParameters[ 23 ].Value = entity.DocumentRowId.ToString();

            sqlParameters[ 24 ] = new SqlParameter( "@ShortString1", entity.ShortString1 );
            sqlParameters[ 25 ] = new SqlParameter( "@ShortString2", entity.ShortString2 );
            //mp 12-03-21
            sqlParameters[ 26 ] = new SqlParameter( "@String3", entity.String3 );
            sqlParameters[ 27 ] = new SqlParameter( "@String4", entity.String4 );

            //sqlParameters[ 26 ] = new SqlParameter( "@ShortString3", entity.ShortString3 );
            //sqlParameters[ 27 ] = new SqlParameter( "@ShortString4", entity.ShortString4 );
            //sqlParameters[ 28 ] = new SqlParameter( "@ShortString5", entity.ShortString5 );
            //sqlParameters[ 29 ] = new SqlParameter( "@ShortString6", entity.ShortString6 );
            //sqlParameters[ 30 ] = new SqlParameter( "@ShortString7", entity.ShortString7 );
            //sqlParameters[ 31 ] = new SqlParameter( "@ShortString8", entity.ShortString8 );

            //sqlParameters[ 32 ] = new SqlParameter( "@String3", entity.String3 );
            //sqlParameters[ 33 ] = new SqlParameter( "@String4", entity.String4 );

            //sqlParameters[ 34 ] = new SqlParameter( "@Int1", entity.Int1 );

            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = dr[ 0 ].ToString();
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Create() " );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        /// <summary>
        /// /// Update an AppItem record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( AppItem entity )
        {
            string message = "successful";
            string connectionString = GetMyConnectionString();

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 29 ];
            sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = entity.RowId;

            sqlParameters[ 1 ] = new SqlParameter( "@VersionNbr", SqlDbType.Int );
            sqlParameters[ 1 ].Value = entity.VersionNbr;

            sqlParameters[ 2 ] = new SqlParameter( "@SequenceNbr", SqlDbType.Int );
            sqlParameters[ 2 ].Value = entity.SequenceNbr;

            sqlParameters[ 3 ] = new SqlParameter( "@ParentRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 3 ].Value = entity.ParentRowId;

            sqlParameters[ 4 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
            sqlParameters[ 4 ].Value = entity.TypeId;

            sqlParameters[ 5 ] = new SqlParameter( "@Title", SqlDbType.VarChar );
            sqlParameters[ 5 ].Size = 200;
            sqlParameters[ 5 ].Value = entity.Title;

            sqlParameters[ 6 ] = new SqlParameter( "@Description", SqlDbType.VarChar );
            sqlParameters[ 6 ].Size = -1;
            sqlParameters[ 6 ].Value = entity.Description;

            sqlParameters[ 7 ] = new SqlParameter( "@AppItemCode", SqlDbType.VarChar );
            sqlParameters[ 7 ].Size = 50;
            sqlParameters[ 7 ].Value = entity.AppItemCode;

            sqlParameters[ 8 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 8 ].Size = 50;
            sqlParameters[ 8 ].Value = entity.Category;

            sqlParameters[ 9 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 9 ].Size = 50;
            sqlParameters[ 9 ].Value = entity.Subcategory;

            sqlParameters[ 10 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
            sqlParameters[ 10 ].Size = 50;
            sqlParameters[ 10 ].Value = entity.Status;

            sqlParameters[ 11 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
            sqlParameters[ 11 ].Value = entity.IsActive;

            sqlParameters[ 12 ] = new SqlParameter( "@StartDate", SqlDbType.DateTime );
            if ( entity.StartDate == DateTime.MinValue )
                sqlParameters[ 12 ].Value = DBNull.Value;
            else
                sqlParameters[ 12 ].Value = entity.StartDate;

            sqlParameters[ 13 ] = new SqlParameter( "@EndDate", SqlDbType.DateTime );
            if ( entity.EndDate == DateTime.MinValue )
                sqlParameters[ 13 ].Value = DBNull.Value;
            else
                sqlParameters[ 13 ].Value = entity.EndDate;

            sqlParameters[ 14 ] = new SqlParameter( "@ExpiryDate", SqlDbType.DateTime );
            if ( entity.ExpiryDate == DateTime.MinValue )
                sqlParameters[ 14 ].Value = DBNull.Value;
            else
                sqlParameters[ 14 ].Value = entity.ExpiryDate;

            sqlParameters[ 15 ] = new SqlParameter( "@LastUpdatedById", SqlDbType.Int );
            sqlParameters[ 15 ].Value = entity.LastUpdatedById;

            sqlParameters[ 16 ] = new SqlParameter( "@Approved", SqlDbType.DateTime );
            if ( entity.Approved == DateTime.MinValue )
                sqlParameters[ 16 ].Value = DBNull.Value;
            else
                sqlParameters[ 16 ].Value = entity.Approved;

            sqlParameters[ 17 ] = new SqlParameter( "@ApprovedById", SqlDbType.Int );
            sqlParameters[ 17 ].Value = entity.ApprovedById;

            sqlParameters[ 18 ] = new SqlParameter( "@RelatedObjectRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 18 ].Value = entity.RelatedObjectRowId;

            sqlParameters[ 19 ] = new SqlParameter( "@ImageId", SqlDbType.Int );
            sqlParameters[ 19 ].Value = entity.ImageId;

            sqlParameters[ 20 ] = new SqlParameter( "@String1", SqlDbType.VarChar );
            sqlParameters[ 20 ].Size = 200;
            sqlParameters[ 20 ].Value = entity.String1;
            sqlParameters[ 21 ] = new SqlParameter( "@String2", SqlDbType.VarChar );
            sqlParameters[ 21 ].Size = 200;
            sqlParameters[ 21 ].Value = entity.String2;
            sqlParameters[ 22 ] = new SqlParameter( "@BigString1", SqlDbType.VarChar );
            sqlParameters[ 22 ].Size = -1;
            sqlParameters[ 22 ].Value = entity.BigString1;
            sqlParameters[ 23 ] = new SqlParameter( "@BigString2", SqlDbType.VarChar );
            sqlParameters[ 23 ].Size = -1;
            sqlParameters[ 23 ].Value = entity.BigString2;

            //mp 11-03-10
            sqlParameters[ 24 ] = new SqlParameter( "@DocumentRowId", SqlDbType.VarChar );
            sqlParameters[ 24 ].Size = 50;
            sqlParameters[ 24 ].Value = entity.DocumentRowId.ToString();
            sqlParameters[ 25 ] = new SqlParameter( "@ShortString1", SqlDbType.VarChar );
            sqlParameters[ 25 ].Size = 50;
            sqlParameters[ 25 ].Value = entity.ShortString1;
            sqlParameters[ 26 ] = new SqlParameter( "@ShortString2", SqlDbType.VarChar );
            sqlParameters[ 26 ].Size = 50;
            sqlParameters[ 26 ].Value = entity.ShortString2;
            //mp 12-03-21
            sqlParameters[ 27 ] = new SqlParameter( "@Short3", entity.String3 );
            sqlParameters[ 28 ] = new SqlParameter( "@Short4", entity.String4 );

            //sqlParameters[ 29 ] = new SqlParameter( "@ShortString3", entity.ShortString3 );
            //sqlParameters[ 30 ] = new SqlParameter( "@ShortString4", entity.ShortString4 );
            //sqlParameters[ 31 ] = new SqlParameter( "@ShortString5", entity.ShortString5 );
            //sqlParameters[ 32 ] = new SqlParameter( "@ShortString6", entity.ShortString6 );
            //sqlParameters[ 33 ] = new SqlParameter( "@ShortString7", entity.ShortString7 );
            //sqlParameters[ 34 ] = new SqlParameter( "@ShortString8", entity.ShortString8 );

            //sqlParameters[ 35 ] = new SqlParameter( "@Int1", entity.Int1 );
            #endregion

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, UPDATE_PROC, sqlParameters );
                message = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Update() " );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;

        }//
        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get AppItem record
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns>AppItem</returns>
        public AppItem Get( string pRowId )
        {
            string connectionString = GetMyConnectionString();
            AppItem entity = new AppItem();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = new Guid( pRowId );


                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
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
        /// Get AppItem record using AppItem code
        /// </summary>
        /// <param name="appItemCode"></param>
        /// <returns>AppItem</returns>
        public AppItem GetByCode( string appItemCode )
        {
            string connectionString = GetMyConnectionString();
            AppItem entity = new AppItem();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@AppItemCode", SqlDbType.VarChar );
                sqlParameters[ 0 ].Size = 50;
                sqlParameters[ 0 ].Value = appItemCode;


                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "AppItem_GetByCode", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
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
                LogError( ex, className + ".GetByCode() " );
                entity.Message = "Unsuccessful: " + className + ".GetByCode(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;

            }

        }//

        /// <summary>
        /// Get AppItem record returning an interface - allows return of any object that implements IAppItem
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns>IAppItem</returns>
        public IAppItem GetAny( string pRowId )
        {
            string connectionString = GetMyConnectionString();
            AppItem entity = new AppItem();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = new Guid( pRowId );


                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, GET_PROC, sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
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
        /// Select AppItem related data using passed parameters
        /// </summary>
        /// <param name="pTypeId"></param>
        /// <param name="pStatus"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public DataSet Select( int pTypeId, string pStatus, string keyword )
        {
            return Select( pTypeId, "", "", pStatus, BusinessConstants.ZERO_ROW_ID, "", keyword );
        }

        /// <summary>
        /// Select AppItem related data using passed parameters, v1.0 (calls v2.0.  Retained for backward compatibility)
        /// </summary>
        /// <param name="pTypeId"></param>
        /// <param name="pCategory"></param>
        /// <param name="pSubCategory"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public DataSet Select( int pTypeId, string pCategory, string pSubCategory, string pStatus )
        {
            return Select( pTypeId, pCategory, pSubCategory, pStatus, BusinessConstants.ZERO_ROW_ID, "", "" );
        }

        /// <summary>
        /// Select AppItem related data using passed parameters, v2.0
        /// </summary>
        /// <param name="pTypeId"></param>
        /// <param name="pCategory"></param>
        /// <param name="pSubcategory"></param>
        /// <param name="pStatus"></param>
        /// <param name="parentRowId"></param>
        /// <param name="appItemCode"></param>
        /// <returns></returns>
        public DataSet Select( int pTypeId, string pCategory, string pSubcategory, string pStatus, string parentRowId, string appItemCode, string keyword )
        {
            string connectionString = GetMyConnectionString();

            SqlParameter[] sqlParameters = new SqlParameter[ 7 ];
            sqlParameters[ 0 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pTypeId;

            sqlParameters[ 1 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 50;
            sqlParameters[ 1 ].Value = pCategory;

            sqlParameters[ 2 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 2 ].Size = 50;
            sqlParameters[ 2 ].Value = pSubcategory;

            sqlParameters[ 3 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
            sqlParameters[ 3 ].Size = 50;
            sqlParameters[ 3 ].Value = pStatus;

            sqlParameters[ 4 ] = new SqlParameter( "@parentRowId", SqlDbType.UniqueIdentifier );
            if ( parentRowId == "" )
            {
                sqlParameters[ 4 ].Value = new Guid( BusinessConstants.ZERO_ROW_ID );
            }
            else
            {
                sqlParameters[ 4 ].Value = new Guid( parentRowId );
            }

            sqlParameters[ 5 ] = new SqlParameter( "@AppItemCode", SqlDbType.VarChar );
            sqlParameters[ 5 ].Size = 50;
            sqlParameters[ 5 ].Value = appItemCode;

            sqlParameters[ 6 ] = new SqlParameter( "@Keyword", SqlDbType.VarChar );
            sqlParameters[ 6 ].Size = 100;
            sqlParameters[ 6 ].Value = keyword;

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "AppItemSelect", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, this.GetType().ToString() + ".Select() " );
                return null;

            }
        }
        /// <summary>
        /// Search for AppItems using a passed sql filter
        /// </summary>
        /// <param name="pFilter">Dynamic filter - no where clause</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet Search( string pFilter, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string sortOrder = "";
            return Search( pFilter, sortOrder, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }
        /// <summary>
        /// Search for AppItems using a passed sql filter
        /// </summary>
        /// <param name="pFilter">Dynamic filter - no where clause</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet Search( string pFilter, string pSortOrder, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            //use read only connection
            string connectionString = GetReadOnlyConnection();
            string customPagingSelect = "AppItem_Search";

            try
            {
                if ( pFilter.Length > 0 )
                    pFilter = " Where " + pFilter;

                SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Filter", SqlDbType.VarChar );
                sqlParameters[ 0 ].Size = 1000;
                sqlParameters[ 0 ].Value = pFilter;

                sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", SqlDbType.VarChar );
                sqlParameters[ 1 ].Size = 100;
                sqlParameters[ 1 ].Value = pSortOrder;

                sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
                sqlParameters[ 2 ].Value = pStartPageIndex;

                sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
                sqlParameters[ 3 ].Value = pMaximumRows;

                sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
                sqlParameters[ 4 ].Direction = ParameterDirection.Output;

                DataSet ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, customPagingSelect, sqlParameters );
                string rows = sqlParameters[ 4 ].Value.ToString();
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
            catch ( Exception e )
            {
                LogError( className + ".Search(): " + e.ToString() );
                return null;
            }
        }//

        /// <summary>
        /// Select rows of data with matching RelatedRowId
        /// </summary>
        /// <param name="pTypeId"></param>
        /// <param name="pRelatedRowId"></param>
        /// <returns></returns>
        public DataSet SelectRelated( int pTypeId, string pRelatedRowId )
        {
            string connectionString = GetMyConnectionString();

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
            sqlParameters[ 0 ].Value = pTypeId;

            sqlParameters[ 1 ] = new SqlParameter( "@RelatedRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 1 ].Value = new Guid( pRelatedRowId );
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, SELECT_RELATED_PROC, sqlParameters );
                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Select() " );
                return null;
            }
        }

        /// <summary>
        /// Select rows of data where Title or Description fields contain pSearchText
        /// </summary>
        /// <param name="pSearchText"></param>
        /// <returns></returns>
        public DataSet SearchAppItems( string pSearchText )
        {
            string connectionString = GetMyConnectionString();
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( @"SearchText", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 1000;
            sqlParameters[ 0 ].Value = pSearchText;
            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "AppItemTextSearch", sqlParameters );
                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SearchAppItems() " );
                return null;
            }
        }

        public DataSet SearchAppItemsNotInGroup( string pGroupId, int pAppItemType, string pCategory, string pSubcategory )
        {
            string connectionString = GetMyConnectionString();

            #region SqlParameters
            SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ParentRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = new Guid( pGroupId );
            sqlParameters[ 1 ] = new SqlParameter( "@AppItemType", SqlDbType.Int );
            sqlParameters[ 1 ].Value = pAppItemType;
            sqlParameters[ 2 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 2 ].Size = 50;
            sqlParameters[ 2 ].Value = pCategory;
            sqlParameters[ 3 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 3 ].Size = 50;
            sqlParameters[ 3 ].Value = pSubcategory;
            #endregion

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "AppItemSelectNotInGroup", sqlParameters );
                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SearchASppItemsNotInGroup() " );
                return null;
            }
        }

        public DataSet SearchAppItemsInGroup( string pGroupCode, int pAppItemType, string pCategory, string pSubcategory, string pStatus )
        {
            string connectionString = GetMyConnectionString();
            // Get Group RowId (ParentRowId)
            AppItem group = GetByCode( pGroupCode );
            #region SqlParameters
            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ParentRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = group.RowId;
            sqlParameters[ 1 ] = new SqlParameter( "@AppItemType", SqlDbType.Int );
            sqlParameters[ 1 ].Value = pAppItemType;
            sqlParameters[ 2 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 2 ].Size = 50;
            sqlParameters[ 2 ].Value = pCategory;
            sqlParameters[ 3 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 3 ].Size = 50;
            sqlParameters[ 3 ].Value = pSubcategory;
            sqlParameters[ 4 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
            sqlParameters[ 4 ].Size = 50;
            sqlParameters[ 4 ].Value = pStatus;
            #endregion

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "AppItemSelectInGroup", sqlParameters );
                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SearchASppItemsNotInGroup() " );
                return null;
            }

        }

        /// <summary>
        /// Retrieve all unique categories for the provided item type
        /// </summary>
        /// <param name="pTypeId"></param>
        /// <returns></returns>
        public DataSet SelectTypeCategories( int pTypeId )
        {
            string connectionString = GetMyConnectionString();
            string sql = "Select distinct Category from AppItem where len(Category) > 0 AND TypeId = @TypeId or @TypeId = 0 Order by Category";
            DataSet ds = new DataSet();

            try
            {
                if ( pTypeId == 1030 )
                {
                    SqlConnection con = new SqlConnection( connectionString );

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.Parameters.AddWithValue( "@TypeId ", pTypeId );
                    cmd.CommandText = sql;

                    SqlDataAdapter dAdapter = new SqlDataAdapter();
                    dAdapter.SelectCommand = cmd;
                    con.Open();

                    dAdapter.Fill( ds );
                    con.Close();
                }
                //----------------
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
                sqlParameters[ 0 ].Value = pTypeId;

                ds = SqlHelper.ExecuteDataset( connectionString, System.Data.CommandType.Text, sql, sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }

                return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectTypeCategories() " );
                return null;
            }
        }

        /// <summary>
        /// Retrieve all unique subcategories for the provided item type and category
        /// </summary>
        /// <param name="pTypeId"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public DataSet SelectTypeSubcategories( int pTypeId, string category )
        {
            string connectionString = GetMyConnectionString();
            string sql = "Select distinct Subcategory from AppItem where len(Subcategory) > 0 AND (TypeId = @TypeId or @TypeId = 0) " +
                                     "And (Category = @Category or @Category = '') " +
                                     "Order by 1";

            //string sql = String.Format("Select distinct Subcategory from AppItem where TypeId = {0} AND Category = '{1}' Order by 1", pTypeId, category);

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
                sqlParameters[ 0 ].Value = pTypeId;
                sqlParameters[ 1 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
                sqlParameters[ 1 ].Value = category;

                DataSet ds = SqlHelper.ExecuteDataset( connectionString, System.Data.CommandType.Text, sql, sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }

                //AddEntryToTable( ds.Tables[ 0 ], 0, "Select a Subcategory", "Subcategory", "Subcategory" );
                return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectTypeSubcategories() " );
                return null;
            }
        }

        public DataSet SelectTypeStatus( int pTypeId )
        {
            string connectionString = GetMyConnectionString();
            string sql = "Select distinct Status from AppItem where len(Status) > 0 AND TypeId = @TypeId or @TypeId = 0 Order by Status";

            //string sql = String.Format( "Select distinct Status from AppItem where TypeId = {0} Order by 1", pTypeId );

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
                sqlParameters[ 0 ].Value = pTypeId;

                DataSet ds = SqlHelper.ExecuteDataset( connectionString, System.Data.CommandType.Text, sql, sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }

                //AddEntryToTable( ds.Tables[ 0 ], 0, "Select a Subcategory", "Subcategory", "Subcategory" );
                return ds;

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectTypeSubcategories() " );
                return null;
            }
        }

        #endregion

        #region ====== AppItemType Methods ===============================================

        /// <summary>
        /// Select AppItem related data using passed parameters
        /// </summary>
        /// <returns></returns>
        public DataSet AppItemTypeSelect()
        {
            string connectionString = GetMyConnectionString();

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "AppItemTypeSelect" );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".Select() " );
                return null;

            }
        }
        #endregion

        #region ====== Success story Methods ===============================================
        /// <summary>
        /// Create a Story AppItem		==> NOT USED
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        private int CreateStory( AppItem entity, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            int newId = 0;

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 19 ];
            sqlParameters[ 0 ] = new SqlParameter( "@VersionNbr", SqlDbType.Int );
            sqlParameters[ 0 ].Value = entity.VersionNbr;

            sqlParameters[ 1 ] = new SqlParameter( "@SequenceNbr", SqlDbType.Int );
            sqlParameters[ 1 ].Value = entity.SequenceNbr;

            sqlParameters[ 2 ] = new SqlParameter( "@ParentRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 2 ].Value = entity.ParentRowId;

            sqlParameters[ 3 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
            sqlParameters[ 3 ].Value = entity.TypeId;

            sqlParameters[ 4 ] = new SqlParameter( "@Title", SqlDbType.VarChar );
            sqlParameters[ 4 ].Size = 100;
            sqlParameters[ 4 ].Value = entity.Title;

            sqlParameters[ 5 ] = new SqlParameter( "@Description", SqlDbType.VarChar );
            sqlParameters[ 5 ].Size = -1;
            sqlParameters[ 5 ].Value = entity.Description;

            sqlParameters[ 6 ] = new SqlParameter( "@AppItemCode", SqlDbType.VarChar );
            sqlParameters[ 6 ].Size = 50;
            sqlParameters[ 6 ].Value = entity.AppItemCode;

            sqlParameters[ 7 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 7 ].Size = 50;
            sqlParameters[ 7 ].Value = entity.Category;

            sqlParameters[ 8 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 8 ].Size = 50;
            sqlParameters[ 8 ].Value = entity.Subcategory;

            sqlParameters[ 9 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
            sqlParameters[ 9 ].Size = 50;
            sqlParameters[ 9 ].Value = entity.Status;

            sqlParameters[ 10 ] = new SqlParameter( "@IsActive", SqlDbType.Bit );
            sqlParameters[ 10 ].Value = entity.IsActive;

            sqlParameters[ 11 ] = new SqlParameter( "@StartDate", SqlDbType.DateTime );
            if ( entity.StartDate == DateTime.MinValue )
                sqlParameters[ 11 ].Value = DBNull.Value;
            else
                sqlParameters[ 11 ].Value = entity.StartDate;

            sqlParameters[ 12 ] = new SqlParameter( "@EndDate", SqlDbType.DateTime );
            if ( entity.EndDate == DateTime.MinValue )
                sqlParameters[ 12 ].Value = DBNull.Value;
            else
                sqlParameters[ 12 ].Value = entity.EndDate;

            sqlParameters[ 13 ] = new SqlParameter( "@ExpiryDate", SqlDbType.DateTime );
            if ( entity.ExpiryDate == DateTime.MinValue )
                sqlParameters[ 13 ].Value = DBNull.Value;
            else
                sqlParameters[ 13 ].Value = entity.ExpiryDate;

            sqlParameters[ 14 ] = new SqlParameter( "@CreatedById", SqlDbType.Int );
            sqlParameters[ 14 ].Value = entity.CreatedById;

            sqlParameters[ 15 ] = new SqlParameter( "@Approved", SqlDbType.DateTime );
            if ( entity.Approved == DateTime.MinValue )
                sqlParameters[ 15 ].Value = DBNull.Value;
            else
                sqlParameters[ 15 ].Value = entity.Approved;

            sqlParameters[ 16 ] = new SqlParameter( "@ApprovedById", SqlDbType.Int );
            sqlParameters[ 16 ].Value = entity.ApprovedById;

            sqlParameters[ 17 ] = new SqlParameter( "@RelatedObjectRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 17 ].Value = entity.RelatedObjectRowId;

            sqlParameters[ 18 ] = new SqlParameter( "@ImageId", SqlDbType.Int );
            sqlParameters[ 18 ].Value = entity.ImageId;
            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "AppItemInsert", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = 1;
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".CreateStory() " );
                statusMessage = className + "-  Unsuccessful: CreateStory(): " + ex.Message.ToString();
                entity.Message = statusMessage;
                entity.IsValid = false;
            }

            return newId;
        }

        /// <summary>
        /// Return all details of a story (or letter)
        /// - used in the DisplayStory.ascx control
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public DataSet GetStoryDetail( string pRowId )
        {
            string connectionString = GetReadOnlyConnection();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.VarChar );
                sqlParameters[ 0 ].Value = pRowId;

                DataSet ds = SqlHelper.ExecuteDataset( connectionString, "SuccessStory_Detail2", sqlParameters );
                if ( ds.HasErrors )
                {
                    return null;
                }
                else
                {
                    return ds;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".GetStoryDetail() " );

                return null;

            }

        }//

        /// <summary>
        /// SelectSuccessStories
        /// </summary>
        /// <param name="ptype"></param>
        /// <param name="pcat"></param>
        /// <param name="psubcat"></param>
        /// <param name="pstatus"></param>
        /// <param name="puserid"></param>
        /// <returns></returns>
        public DataSet SelectSuccessStories( int ptype, string pcat, string psubcat, string pstatus, int puserid )
        {
            int rows = 5;

            return SelectSuccessStories( ptype, pcat, psubcat, pstatus, puserid, rows );

        }

        /// <summary>
        /// SelectSuccessStories
        /// </summary>
        /// <param name="ptype"></param>
        /// <param name="pcat"></param>
        /// <param name="psubcat"></param>
        /// <param name="pstatus"></param>
        /// <param name="puserid"></param>
        /// <param name="rows">Rows to be returned</param>
        /// <returns></returns>
        public DataSet SelectSuccessStories( int ptype, string pcat, string psubcat, string pstatus, int puserid, int rows )
        {
            string connectionString = GetMyConnectionString();

            SqlParameter[] sqlParameters = new SqlParameter[ 6 ];
            sqlParameters[ 0 ] = new SqlParameter( "@TypeId", SqlDbType.Int );
            sqlParameters[ 0 ].Value = ptype;
            sqlParameters[ 1 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 50;
            sqlParameters[ 1 ].Value = pcat;
            sqlParameters[ 2 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 2 ].Size = 50;
            sqlParameters[ 2 ].Value = psubcat;
            sqlParameters[ 3 ] = new SqlParameter( "@Status", SqlDbType.VarChar );
            sqlParameters[ 3 ].Size = 50;
            sqlParameters[ 3 ].Value = pstatus;
            sqlParameters[ 4 ] = new SqlParameter( "@UserId", SqlDbType.Int );
            sqlParameters[ 4 ].Value = puserid;
            sqlParameters[ 5 ] = new SqlParameter( "@Rows", SqlDbType.Int );
            sqlParameters[ 5 ].Value = rows;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset( connectionString, "SuccessStorySelect", sqlParameters );
                if ( ds.HasErrors )
                {
                    return null;
                }
                else
                {
                    return ds;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, className + "- .GetSuccessStories() " );
                return null;
            }
        }


        /// <summary>
        /// SelectSuccessStories
        /// </summary>
        /// <param name="pCategory"></param>
        /// <param name="pSubCategory"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows">Rows to be returned</param>
        /// <returns></returns>
        public DataSet SuccessStoriesSearch( string pCategory, string pSubCategory, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string connectionString = GetReadOnlyConnection();

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Category", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 50;
            sqlParameters[ 0 ].Value = pCategory;
            sqlParameters[ 1 ] = new SqlParameter( "@Subcategory", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 50;
            sqlParameters[ 1 ].Value = pSubCategory;

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "SuccessStory_Search", sqlParameters );
                string rows = sqlParameters[ 4 ].Value.ToString();
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
                else
                {
                    return ds;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, className + "- .SuccessStoriesSearch() " );
                return null;
            }
        }
        public DataSet SuccessStoriesSearch( string pFilter, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string connectionString = GetReadOnlyConnection();

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 500;
            sqlParameters[ 0 ].Value = pFilter;
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 100;
            sqlParameters[ 1 ].Value = "";

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "SuccessStory_Search2", sqlParameters );
                string rows = sqlParameters[ 4 ].Value.ToString();
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
                else
                {
                    return ds;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, className + "- .SuccessStoriesSearch() " );
                return null;
            }
        }

        public DataSet SuccessStoriesSearch3( string pFilter, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            string connectionString = GetReadOnlyConnection();

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 500;
            sqlParameters[ 0 ].Value = pFilter;
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 100;
            sqlParameters[ 1 ].Value = "";

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "SuccessStory_Search3", sqlParameters );
                string rows = sqlParameters[ 4 ].Value.ToString();
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
                else
                {
                    return ds;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, className + "- .SuccessStoriesSearch() " );
                return null;
            }
        }
        /// <summary>
        /// SelectSuccessStories
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pSortOrder"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows">Rows to be returned</param>
        /// <returns></returns>
        public DataSet SuccessStoriesSearchByCongDist( string pFilter, string pSortOrder, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            //TODO - move to AppItemManager after JG does check-in
            string connectionString = GetReadOnlyConnection();

            SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
            sqlParameters[ 0 ] = new SqlParameter( "@Filter", SqlDbType.VarChar );
            sqlParameters[ 0 ].Size = 500;
            sqlParameters[ 0 ].Value = pFilter;
            sqlParameters[ 1 ] = new SqlParameter( "@SortOrder", SqlDbType.VarChar );
            sqlParameters[ 1 ].Size = 100;
            sqlParameters[ 1 ].Value = pSortOrder;

            sqlParameters[ 2 ] = new SqlParameter( "@StartPageIndex", SqlDbType.Int );
            sqlParameters[ 2 ].Value = pStartPageIndex;

            sqlParameters[ 3 ] = new SqlParameter( "@PageSize", SqlDbType.Int );
            sqlParameters[ 3 ].Value = pMaximumRows;

            sqlParameters[ 4 ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
            sqlParameters[ 4 ].Direction = ParameterDirection.Output;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset( connectionString, CommandType.StoredProcedure, "SuccessStory_SearchByCongDistrict", sqlParameters );
                string rows = sqlParameters[ 4 ].Value.ToString();
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
                else
                {
                    return ds;
                }

            }
            catch ( Exception ex )
            {
                LogError( ex, className + "- .SuccessStoriesSearch() " );
                return null;
            }
        }

        #endregion

        #region ====== AppItem.Contact Methods ===============================================
        /// <summary>
        /// Delete an AppItemContact record 
        /// </summary>
        /// <param name="pParentRowId"></param>
        /// <param name="pUserId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool AppItemContact_Delete( string pAppItemRowId, int pUserId, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            bool successful;

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@AppItemRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = new Guid( pAppItemRowId );
            sqlParameters[ 1 ] = new SqlParameter( "@UserId", SqlDbType.Int );
            sqlParameters[ 1 ].Value = pUserId;

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "AppItemContact_Delete", sqlParameters );
                successful = true;
                statusMessage = "";
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".AppItemContact_Delete() " );
                statusMessage = className + "- Unsuccessful: AppItemContact_Delete(): " + ex.Message.ToString();

                successful = false;
            }
            return successful;
        }//

        /// <summary>
        /// Add an AppItemContact record
        /// </summary>
        /// <param name="pParentRowId"></param>
        /// <param name="pUserId"></param>
        /// <param name="pCreatedBy"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool AppItemContact_Create( AppItemContact entity, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            bool successful;

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
            sqlParameters[ 0 ] = new SqlParameter( "@AppItemRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = entity.AppItemRowId;
            sqlParameters[ 1 ] = new SqlParameter( "@UserId", SqlDbType.Int );
            sqlParameters[ 1 ].Value = entity.UserId;
            sqlParameters[ 2 ] = new SqlParameter( "@CreatedBy", SqlDbType.VarChar );
            sqlParameters[ 2 ].Size = 50;
            sqlParameters[ 2 ].Value = entity.CreatedBy;

            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "AppItemContact_Insert", sqlParameters );
                statusMessage = "successful";
                successful = true;
                dr.Close();
                dr = null;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".AppItemContact_Create() " );
                statusMessage = className + "- Unsuccessful: AppItemContact_Create(): " + ex.Message.ToString();
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Get an AppItemContact record
        /// </summary>
        /// <param name="pParentRowId"></param>
        /// <param name="pUserId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public AppItemContact AppItemContact_Get( string pAppItemRowId, int pUserId, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            AppItemContact entity = new AppItemContact();

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@AppItemRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = new Guid( pAppItemRowId );
            sqlParameters[ 1 ] = new SqlParameter( "@UserId", SqlDbType.Int );
            sqlParameters[ 1 ].Value = pUserId;

            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "AppItemContact_Get", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = AppItemContact_Fill( dr );
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
                LogError( ex, className + ".AppItemContact_Get() " );
                entity.Message = "Unsuccessful: " + className + ".AppItemContact_Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;
            }
        }

        /// <summary>
        /// Fill an AppItemContact object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>AppItem</returns>
        public AppItemContact AppItemContact_Fill( SqlDataReader dr )
        {
            AppItemContact entity = new AppItemContact();
            string statusMessage = string.Empty;

            string rowId = GetRowColumn( dr, "AppItemRowId", "" );
            entity.RowId = new Guid( rowId );

            entity.UserId = GetRowColumn( dr, "UserId", 0 );
            if ( entity.UserId > 0 )
            {
               // entity.RelatedContact = UserManager.GetUser( entity.UserId, ref statusMessage );
            }

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );



            //
            return entity;
        }
        #endregion


        #region ====== AppItem.StoryProperties Methods ===============================================
        /// <summary>
        /// Add an AppItemStoryProperties record
        /// </summary>
        /// <param name="pParentRowId"></param>
        /// <param name="pUserId"></param>
        /// <param name="pCreatedBy"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool AppItemStoryProperties_Create( AppItemStoryProperties entity, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            bool successful;
            string rowsAffected = "";

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 22 ];

            sqlParameters[ 0 ] = new SqlParameter( "@AppItemRowId", entity.AppItemRowId );
            sqlParameters[ 1 ] = new SqlParameter( "@StoryType", entity.StoryType );
            sqlParameters[ 2 ] = new SqlParameter( "@IwdsServiceType", entity.IwdsServiceType );
            sqlParameters[ 3 ] = new SqlParameter( "@TargetUserId", entity.TargetUserId );
            sqlParameters[ 4 ] = new SqlParameter( "@FirstName", entity.FirstName );
            sqlParameters[ 5 ] = new SqlParameter( "@LastName", entity.LastName );
            sqlParameters[ 6 ] = new SqlParameter( "@TargetTitle", entity.TargetTitle );
            sqlParameters[ 7 ] = new SqlParameter( "@Address1", entity.Address1 );
            sqlParameters[ 8 ] = new SqlParameter( "@Address2", entity.Address2 );
            sqlParameters[ 9 ] = new SqlParameter( "@City", entity.City );
            sqlParameters[ 10 ] = new SqlParameter( "@Zipcode", entity.Zipcode );
            //sqlParameters[ 10 ] = new SqlParameter( "@StateCode", entity.StateCode );

            sqlParameters[ 11 ] = new SqlParameter( "@CongDistrict", entity.CongDistrict );
            sqlParameters[ 12 ] = new SqlParameter( "@ServiceLocation", entity.ServiceLocation );
            sqlParameters[ 13 ] = new SqlParameter( "@CipCode", entity.CipCode );
            sqlParameters[ 14 ] = new SqlParameter( "@ServiceId", entity.ServiceId );
            sqlParameters[ 15 ] = new SqlParameter( "@ServiceDesc", entity.ServiceDesc );
            sqlParameters[ 16 ] = new SqlParameter( "@IndustryId", entity.IndustryId );
            sqlParameters[ 17 ] = new SqlParameter( "@YouthYearRound", entity.YouthYearRound );
            sqlParameters[ 18 ] = new SqlParameter( "@YouthEducation", entity.YouthEducation );
            sqlParameters[ 19 ] = new SqlParameter( "@YouthSummer", entity.YouthSummer );
            sqlParameters[ 20 ] = new SqlParameter( "@Lwia", entity.Lwia );
            sqlParameters[ 21 ] = new SqlParameter( "@CreatedById", entity.CreatedById );

            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, CommandType.StoredProcedure, "[AppItem.StoryPropertiesInsert]", sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    rowsAffected = dr[ 0 ].ToString();
                }
                statusMessage = "successful";
                successful = true;

                dr.Close();
                dr = null;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".AppItemStoryProperties_Create() " );
                statusMessage = className + "- Unsuccessful: AppItemStoryProperties_Create(): " + ex.Message.ToString();
                successful = false;
            }

            return successful;
        }

        public bool AppItemStoryProperties_Update( AppItemStoryProperties entity, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            statusMessage = "";
            bool successful = true;
            #region parameters

            SqlParameter[] sqlParameters = new SqlParameter[ 20 ];

            sqlParameters[ 0 ] = new SqlParameter( "@AppItemRowId", entity.AppItemRowId );
            sqlParameters[ 1 ] = new SqlParameter( "@StoryType", entity.StoryType );
            sqlParameters[ 2 ] = new SqlParameter( "@IwdsServiceType", entity.IwdsServiceType );
            sqlParameters[ 3 ] = new SqlParameter( "@TargetUserId", entity.TargetUserId );
            sqlParameters[ 4 ] = new SqlParameter( "@FirstName", entity.FirstName );
            sqlParameters[ 5 ] = new SqlParameter( "@LastName", entity.LastName );
            sqlParameters[ 6 ] = new SqlParameter( "@TargetTitle", entity.TargetTitle );
            sqlParameters[ 7 ] = new SqlParameter( "@Address1", entity.Address1 );
            sqlParameters[ 8 ] = new SqlParameter( "@Address2", entity.Address2 );
            sqlParameters[ 9 ] = new SqlParameter( "@City", entity.City );
            sqlParameters[ 10 ] = new SqlParameter( "@Zipcode", entity.Zipcode );
            //sqlParameters[ 11 ] = new SqlParameter( "@StateCode", entity.StateCode );

            sqlParameters[ 11 ] = new SqlParameter( "@CongDistrict", entity.CongDistrict );
            sqlParameters[ 12 ] = new SqlParameter( "@ServiceLocation", entity.ServiceLocation );
            sqlParameters[ 13 ] = new SqlParameter( "@CipCode", entity.CipCode );
            sqlParameters[ 14 ] = new SqlParameter( "@ServiceId", entity.ServiceId );
            sqlParameters[ 15 ] = new SqlParameter( "@ServiceDesc", entity.ServiceDesc );
            sqlParameters[ 16 ] = new SqlParameter( "@IndustryId", entity.IndustryId );
            sqlParameters[ 17 ] = new SqlParameter( "@YouthYearRound", entity.YouthYearRound );
            sqlParameters[ 18 ] = new SqlParameter( "@YouthEducation", entity.YouthEducation );
            sqlParameters[ 19 ] = new SqlParameter( "@YouthSummer", entity.YouthSummer );

            #endregion

            try
            {
                SqlHelper.ExecuteNonQuery( connectionString, "[AppItem.StoryPropertiesUpdate]", sqlParameters );
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".AppItemStoryProperties_Update() " );
                statusMessage = className + "- Unsuccessful: AppItemStoryProperties_Update(): " + ex.Message.ToString();
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Get an AppItemStoryProperties record
        /// </summary>
        /// <param name="pAppItemRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public AppItemStoryProperties AppItemStoryProperties_Get( string pAppItemRowId, ref string statusMessage )
        {
            string connectionString = GetMyConnectionString();
            AppItemStoryProperties entity = new AppItemStoryProperties();

            #region parameters
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@AppItemRowId", SqlDbType.UniqueIdentifier );
            sqlParameters[ 0 ].Value = new Guid( pAppItemRowId );

            #endregion

            try
            {
                SqlDataReader dr = SqlHelper.ExecuteReader( connectionString, "[AppItem.StoryPropertiesGet]", sqlParameters );

                if ( dr.HasRows )
                {
                    // it should return only one record.
                    while ( dr.Read() )
                    {
                        entity = AppItemStoryProperties_Fill( dr );
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
                LogError( ex, className + ".AppItemStoryProperties_Get() " );
                entity.Message = "Unsuccessful: " + className + ".AppItemStoryProperties_Get(): " + ex.Message.ToString();
                entity.IsValid = false;
                return entity;
            }
        }

        /// <summary>
        /// Fill an AppItem.StoryProperties object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>AppItem</returns>
        public AppItemStoryProperties AppItemStoryProperties_Fill( SqlDataReader dr )
        {
            AppItemStoryProperties entity = new AppItemStoryProperties();
            string statusMessage = string.Empty;

            string rowId = GetRowColumn( dr, "AppItemRowId", "" );
            entity.RowId = new Guid( rowId );

            entity.TargetUserId = GetRowColumn( dr, "TargetUserId", 0 );
            if ( entity.TargetUserId > 0 )
            {
               // entity.RelatedContact = UserManager.GetUser( entity.TargetUserId, ref statusMessage );
            }

            entity.StoryType = GetRowColumn( dr, "StoryType", 0 );
            entity.IwdsServiceType = GetRowColumn( dr, "IwdsServiceType", "" );

            entity.FirstName = GetRowColumn( dr, "FirstName", "" );
            entity.LastName = GetRowColumn( dr, "LastName", "" );
            entity.TargetTitle = GetRowColumn( dr, "TargetTitle", "" );
            entity.Address1 = GetRowColumn( dr, "Address1", "" );
            entity.Address2 = GetRowColumn( dr, "Address2", "" );
            entity.City = GetRowColumn( dr, "City", "" );
            entity.Zipcode = GetRowColumn( dr, "Zipcode", "" );
            entity.StateCode = GetRowColumn( dr, "StateCode", "" );

            entity.Lwia = GetRowColumn( dr, "Lwia", 0 );
            entity.CongDistrict = GetRowColumn( dr, "CongDistrict", "" );
            entity.ServiceLocation = GetRowColumn( dr, "ServiceLocation", "" );
            entity.CipCode = GetRowColumn( dr, "CipCode", "" );

            entity.ServiceId = GetRowColumn( dr, "ServiceId", 0 );
            entity.ServiceDesc = GetRowColumn( dr, "ServiceDesc", "" );

            entity.IndustryId = GetRowColumn( dr, "IndustryId", 0 );
            entity.CareerCluster = GetRowColumn( dr, "CareerCluster", "" );

            entity.YouthYearRound = GetRowColumn( dr, "YouthYearRound", false );
            entity.YouthEducation = GetRowColumn( dr, "YouthEducation", false );
            entity.YouthSummer = GetRowColumn( dr, "YouthSummer", false );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );


            //
            return entity;
        }
        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an AppItem object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>AppItem</returns>
        public AppItem Fill( SqlDataReader dr )
        {
            AppItem entity = new AppItem();
            string statusMessage = string.Empty;

            string rowId = GetRowColumn( dr, "RowId", "" );
            entity.RowId = new Guid( rowId );

            entity.VersionNbr = GetRowColumn( dr, "VersionNbr", 0 );
            entity.SequenceNbr = GetRowColumn( dr, "SequenceNbr", 0 );

            rowId = GetRowColumn( dr, "ParentRowId", "" );
            if ( rowId.Length > 35 )
                entity.ParentRowId = new Guid( rowId );

            entity.TypeId = GetRowColumn( dr, "TypeId", 0 );
            entity.AppItemTypeTitle = GetRowColumn( dr, "AppItemTypeTitle", "" );

            entity.Title = GetRowColumn( dr, "Title", "" );
            entity.Description = GetRowColumn( dr, "Description", "" );

            entity.AppItemCode = GetRowColumn( dr, "AppItemCode", "" );
            entity.Category = GetRowColumn( dr, "Category", "" );
            entity.Subcategory = GetRowColumn( dr, "Subcategory", "" );
            entity.Status = GetRowColumn( dr, "Status", "" );
            entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.StartDate = GetRowColumn( dr, "StartDate", System.DateTime.MinValue );
            entity.EndDate = GetRowColumn( dr, "EndDate", System.DateTime.MinValue );
            entity.ExpiryDate = GetRowColumn( dr, "ExpiryDate", System.DateTime.MinValue );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );

            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            entity.Approved = GetRowColumn( dr, "Approved", System.DateTime.MinValue );
            entity.ApprovedById = GetRowColumn( dr, "ApprovedById", 0 );
            entity.ApprovedBy = GetRowColumn( dr, "ApprovedBy", "" );

            if ( entity.CreatedById > 0 && entity.CreatedBy.Length == 0 )
            {
                //AppUser crBy = UserManager.GetUser( entity.CreatedById, ref statusMessage );
                //if ( crBy != null && crBy.LastName.Length > 0 )
                //{
                //    entity.CreatedBy = crBy.FullName();
                //    entity.TempProperty1 = crBy.SummaryAsHtml();
                //}
            }


            if ( entity.LastUpdatedById > 0 && entity.LastUpdatedBy.Length == 0 )
            {
                if ( entity.LastUpdatedById == entity.CreatedById )
                {
                    entity.LastUpdatedBy = entity.CreatedBy;
                }
                else
                {
                    //AppUser upBy = UserManager.GetUser( entity.LastUpdatedById, ref statusMessage );
                    //if ( upBy != null && upBy.LastName.Length > 0 )
                    //{
                    //    entity.LastUpdatedBy = upBy.FullName();
                    //    entity.TempProperty2 = upBy.SummaryAsHtml();
                    //}
                }
            }

            if ( entity.ApprovedById > 0 && entity.ApprovedBy.Length == 0 )
            {
                if ( entity.ApprovedById == entity.LastUpdatedById )
                {
                    entity.ApprovedBy = entity.LastUpdatedBy;
                }
                else
                {
                    //AppUser apBy = UserManager.GetUser( entity.ApprovedById, ref statusMessage );
                    //if ( apBy != null && apBy.LastName.Length > 0 )
                    //{
                    //    entity.ApprovedBy = apBy.FullName();
                    //}
                }
            }

            rowId = GetRowColumn( dr, "RelatedObjectRowId", "" );
            //if expecting a RowId and it is not found is probably an error condition and someone should be notified!!
            if ( rowId.Length > 35 )
                entity.RelatedObjectRowId = new Guid( rowId );

            entity.ImageId = GetRowColumn( dr, "ImageId", 0 );
            if ( entity.ImageId > 0 )
            {
               // entity.AppItemImage = ImageStoreManager.Get( entity.ImageId );
            }
            entity.String1 = GetRowColumn( dr, "String1", "" );
            entity.String2 = GetRowColumn( dr, "String2", "" );
            entity.String3 = GetRowColumn( dr, "String3", "" );
            entity.String4 = GetRowColumn( dr, "String4", "" );

            entity.BigString1 = GetRowColumn( dr, "BigString1", "" );
            entity.BigString2 = GetRowColumn( dr, "BigString2", "" );

            //special check by AppItem type
            if ( entity.TypeId == AppItem.FAQItemType )
            {
                //?? or use string1??
              //  entity.FaqSubcategory = FAQManager.FAQSubcategory_Get( entity.Category, entity.Subcategory );

            } if ( entity.TypeId == AppItem.SuccessStoryItemType
                    || entity.TypeId == AppItem.SuccessLetterItemType )
            {
                entity.StoryProperties = AppItemStoryProperties_Get( entity.RowId.ToString(), ref statusMessage );
            }

            rowId = GetRowColumn( dr, "DocumentRowId", "" );
            if ( rowId.Length > 35 )
                entity.DocumentRowId = new Guid( rowId );

            entity.ShortString1 = GetRowColumn( dr, "ShortString1", "" );
            entity.ShortString2 = GetRowColumn( dr, "ShortString2", "" );
            //entity.ShortString3 = GetRowColumn( dr, "ShortString3", "" );
            //entity.ShortString4 = GetRowColumn( dr, "ShortString4", "" );
            //entity.ShortString5 = GetRowColumn( dr, "ShortString5", "" );
            //entity.ShortString6 = GetRowColumn( dr, "ShortString6", "" );
            //entity.ShortString7 = GetRowColumn( dr, "ShortString7", "" );
            //entity.ShortString8 = GetRowColumn( dr, "ShortString8", "" );

            //entity.Int1 = GetRowColumn( dr, "Int1", 0 );

            //
            return entity;
        }

        /// <summary>
        /// Fill an AppItem object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>AppItem</returns>
        public AppItem Fill( DataRow dr )
        {
            AppItem entity = new AppItem();
            string statusMessage = string.Empty;
            string rowId = GetRowColumn( dr, "RowId", "" );
            entity.RowId = new Guid( rowId );

            entity.VersionNbr = GetRowColumn( dr, "VersionNbr", 0 );
            entity.SequenceNbr = GetRowColumn( dr, "SequenceNbr", 0 );

            rowId = GetRowColumn( dr, "ParentRowId", "" );
            if ( rowId.Length > 35 )
                entity.ParentRowId = new Guid( rowId );

            entity.TypeId = GetRowColumn( dr, "TypeId", 0 );
            entity.AppItemTypeTitle = GetRowColumn( dr, "AppItemTypeTitle", "" );

            entity.Title = GetRowColumn( dr, "Title", "" );
            entity.Description = GetRowColumn( dr, "Description", "" );

            entity.AppItemCode = GetRowColumn( dr, "AppItemCode", "" );
            entity.Category = GetRowColumn( dr, "Category", "" );
            entity.Subcategory = GetRowColumn( dr, "Subcategory", "" );
            entity.Status = GetRowColumn( dr, "Status", "" );
            entity.IsActive = GetRowColumn( dr, "IsActive", false );
            entity.StartDate = GetRowColumn( dr, "StartDate", System.DateTime.MinValue );
            entity.EndDate = GetRowColumn( dr, "EndDate", System.DateTime.MinValue );
            entity.ExpiryDate = GetRowColumn( dr, "ExpiryDate", System.DateTime.MinValue );

            entity.Created = GetRowColumn( dr, "Created", System.DateTime.MinValue );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.CreatedBy = GetRowColumn( dr, "CreatedBy", "" );

            entity.LastUpdated = GetRowColumn( dr, "LastUpdated", System.DateTime.MinValue );
            entity.LastUpdatedById = GetRowColumn( dr, "LastUpdatedById", 0 );
            entity.LastUpdatedBy = GetRowColumn( dr, "LastUpdatedBy", "" );

            entity.Approved = GetRowColumn( dr, "Approved", System.DateTime.MinValue );
            entity.ApprovedById = GetRowColumn( dr, "ApprovedById", 0 );
            entity.ApprovedBy = GetRowColumn( dr, "ApprovedBy", "" );

            if ( entity.CreatedById > 0 && entity.CreatedBy.Length == 0 )
            {
                //AppUser crBy = UserManager.GetUser( entity.CreatedById, ref statusMessage );
                //if ( crBy != null && crBy.LastName.Length > 0 )
                //{
                //    entity.CreatedBy = crBy.FullName();
                //    entity.TempProperty1 = crBy.SummaryAsHtml();
                //}
            }

            if ( entity.LastUpdatedById > 0 && entity.LastUpdatedBy.Length == 0 )
            {
                if ( entity.LastUpdatedById == entity.CreatedById )
                {
                    entity.LastUpdatedBy = entity.CreatedBy;
                }
                else
                {
                    //AppUser upBy = UserManager.GetUser( entity.LastUpdatedById, ref statusMessage );
                    //if ( upBy != null && upBy.LastName.Length > 0 )
                    //{
                    //    entity.LastUpdatedBy = upBy.FullName();
                    //    entity.TempProperty2 = upBy.SummaryAsHtml();
                    //}
                }
            }

            if ( entity.ApprovedById > 0 && entity.ApprovedBy.Length == 0 )
            {
                if ( entity.ApprovedById == entity.LastUpdatedById )
                {
                    entity.ApprovedBy = entity.LastUpdatedBy;
                }
                else
                {
                    //AppUser apBy = UserManager.GetUser( entity.ApprovedById, ref statusMessage );
                    //if ( apBy != null && apBy.LastName.Length > 0 )
                    //{
                    //    entity.ApprovedBy = apBy.FullName();
                    //}
                }
            }

            rowId = GetRowColumn( dr, "RelatedObjectRowId", "" );
            //if expecting a RowId and it is not found is probably an error condition and someone should be notified!!
            if ( rowId.Length > 35 )
                entity.RelatedObjectRowId = new Guid( rowId );

            entity.ImageId = GetRowColumn( dr, "ImageId", 0 );
            if ( entity.ImageId > 0 )
            {
               // entity.AppItemImage = ImageStoreManager.Get( entity.ImageId );
            }

            entity.String1 = GetRowColumn( dr, "String1", "" );
            entity.String2 = GetRowColumn( dr, "String2", "" );
            entity.String3 = GetRowColumn( dr, "String3", "" );
            entity.String4 = GetRowColumn( dr, "String4", "" );

            entity.BigString1 = GetRowColumn( dr, "BigString1", "" );
            entity.BigString2 = GetRowColumn( dr, "BigString2", "" );

            //special check by AppItem type
            if ( entity.TypeId == AppItem.FAQItemType )
            {
                //?? or use string1??
              //  entity.FaqSubcategory = FAQManager.FAQSubcategory_Get( entity.Category, entity.Subcategory );

            } if ( entity.TypeId == AppItem.SuccessStoryItemType
                  || entity.TypeId == AppItem.SuccessLetterItemType )
            {
                entity.StoryProperties = AppItemStoryProperties_Get( entity.RowId.ToString(), ref statusMessage );
            }

            rowId = GetRowColumn( dr, "DocumentRowId", "" );
            if ( rowId.Length > 35 )
                entity.DocumentRowId = new Guid( rowId );

            entity.ShortString1 = GetRowColumn( dr, "ShortString1", "" );
            entity.ShortString2 = GetRowColumn( dr, "ShortString2", "" );
            //entity.ShortString3 = GetRowColumn( dr, "ShortString3", "" );
            //entity.ShortString4 = GetRowColumn( dr, "ShortString4", "" );
            //entity.ShortString5 = GetRowColumn( dr, "ShortString5", "" );
            //entity.ShortString6 = GetRowColumn( dr, "ShortString6", "" );
            //entity.ShortString7 = GetRowColumn( dr, "ShortString7", "" );
            //entity.ShortString8 = GetRowColumn( dr, "ShortString8", "" );

            //entity.Int1 = GetRowColumn( dr, "Int1", 0 );

            //
            return entity;
        }

        #endregion

        #region ====== Testing Methods ===============================================
        /// <summary>
        /// Fill an AppItem object from a SqlDataReader
        /// </summary>
        /// <param name="pRowId">AppItem RowId</param>
        /// <returns>IAppItem</returns>
        //public IAppItem GetHelp( string pRowId )
        //{

        //   // IAppItem announcement = new AnnouncementItem();

        //    IAppItem help = new HelpItem();

        //    help = Get( pRowId );
        //    announcement = Get( pRowId );

        //    announcement.RowId = help.RowId;

        //    return help;
        //}//

        #endregion

    }
}
