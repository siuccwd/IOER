using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;

using MyEntity = LRWarehouse.Business.ResourceChildItem;
using Entity = LRWarehouse.Business.ResourceChildItem;
using EntityCollection = System.Collections.Generic.List<LRWarehouse.Business.ResourceChildItem>;

namespace LRWarehouse.DAL
{/// <summary>
    /// Data access manager for ResourceType
    /// </summary>
    public class ResourceTypeManager : BaseDataManager, IResourceIntManager
    {
        const string className = "ResourceTypeManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Resource.ResourceTypeGet]";
        const string SELECT_PROC = "[Resource.ResourceTypeSelect]";
        const string DELETE_PROC = "[Resource.ResourceTypeDelete]";
        const string INSERT_PROC = "[Resource.ResourceTypeInsert]";
        const string UPDATE_PROC = "[Resource.ResourceTypeUpdate]";
        const string IMPORT_PROC = "[Resource.ResourceTypeImport]";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceTypeManager()
        {
            //base constructor sets common connection strings
        }//

        #region ====== Core Methods ===============================================
    

        /// <summary>
        /// Add an ResourceType record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create( MyEntity entity, ref string statusMessage )
        {
            return Create( entity.ResourceIntId, entity.CodeId, entity.MappedValue, entity.CreatedById, ref statusMessage );
        }

        public string Create( int pResourceIntId, int pCodeId, string pOriginalValue, ref string statusMessage )
        {
            return Create( pResourceIntId, pCodeId, pOriginalValue, 0, ref statusMessage );
        }

        public string Create( int pResourceIntId, int pCodeId, string pOriginalValue, int pCreatedById, ref string statusMessage )
        {
            statusMessage = "successful";
            string newId = "";
            bool isDup = false;
            string msg = "";

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceTypeId", pCodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@OriginalValue", pOriginalValue );
                sqlParameters[ 3 ] = new SqlParameter( "@CreatedbyId", pCreatedById );

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = GetRowColumn( dr, "Id", "" );
                    isDup = GetRowPossibleColumn( dr, "IsDuplicate", false );
                    msg = GetRowColumn( dr, "Message", "" );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( "ResourceTypeManager.Create(string pResourceId, int pCodeId, string pOriginalValue, int pCreatedById): " + ex.ToString() );
                statusMessage = ex.ToString();
            }
            
            return newId;
        }


        /// <summary>
        /// Import
        /// the Insert is now the same as this essentially, so probably will not use
        /// May still be used by the process to resolve mapping issues, but likely to be called from a sproc
        /// MP-removed ResourceId - obsolete
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Import( MyEntity entity, ref string statusMessage )
        {
            string newId = "";
            bool isDup = false;
            string msg = "";
            int outputCol = 3;
            int pTotalRows = 0;
            DataSet ds = new DataSet();

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 5 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", "" ); //entity.ResourceId
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceTypeId", entity.CodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@OriginalValue", entity.OriginalValue );
                sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
                sqlParameters[ outputCol ].Direction = ParameterDirection.Output;
                sqlParameters[ 4 ] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                #endregion
                //??
                ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, IMPORT_PROC, sqlParameters );

                string rows = sqlParameters[ outputCol ].Value.ToString();
                try
                {
                    pTotalRows = Int32.Parse( rows );
                }
                catch
                {
                    pTotalRows = 0;
                }
                //OR ????????????????????
                //SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, IMPORT_PROC, sqlParameters );
                //if ( dr.HasRows )
                //{
                //    dr.Read();
                //    newId = GetRowColumn( dr, "Id", "" );
                //    isDup = GetRowPossibleColumn( dr, "IsDuplicate", false );
                //    msg = GetRowColumn( dr, "Message", "" );
                //}
                //dr.Close();
                //dr = null;
                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Import() for ResourceId: {0}, CodeId: {1}, Vale: {2}, and CreatedBy: {3}", entity.ResourceIntId.ToString(), entity.CodeId, entity.OriginalValue, entity.CreatedById ) );
                statusMessage = className + "- Unsuccessful: Import(): " + ex.Message.ToString();
            }

            return newId;
        }

        /// <summary>
        /// Add an ResourceType record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Update( MyEntity entity )
        {
            string message = "successful";
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", entity.RowId );
                sqlParameters[ 1 ] = new SqlParameter( "@CodeId", entity.CodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@MappedValue", entity.OriginalValue );
                #endregion

                SqlHelper.ExecuteNonQuery( ConnString, UPDATE_PROC, sqlParameters );
                message = "successful";
            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Update() for ResourceId: {0}, CodeId: {1}, Vale: {2}, and UpdatedBy: {4}", entity.ResourceIntId.ToString(), entity.CodeId, entity.OriginalValue, entity.LastUpdatedById ) );
                message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                entity.Message = message;
                entity.IsValid = false;
            }

            return message;
        }

        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Get ResourceType record via rowId ==> un likely, except maybe to delete?
        /// </summary>
        /// <param name="pRowId"></param>
        /// <returns></returns>
        public MyEntity Get( string pRowId )
        {
            MyEntity entity = new MyEntity();

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = new Guid( pRowId );

                SqlDataReader dr = SqlHelper.ExecuteReader( ReadOnlyConnString, GET_PROC, sqlParameters );

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


        public List<MyEntity> SelectList( int pResourceIntId )
        {

            List<MyEntity> collection = new List<MyEntity>();

            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@pResourceIntId", pResourceIntId );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        MyEntity entity = Fill( dr );
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


        /// <summary>
        /// select all resource types for passed resourceIntId
        /// 13-08-28 mp - updated to match the proc
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <returns></returns>
        public EntityCollection Select( int pResourceIntId )
        {
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.ResourceTypeSelect]", sqlParameter );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        Entity entity = Fill( dr );
                        collection.Add( entity );
                    }
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceTypeManager.Select(): " + ex.ToString() );
                return null;
            }
            return collection;
        }

        /// <summary>
        /// OBSOLETE
        /// </summary>
        /// <param name="rowId"></param>
        /// <param name="resourceId"></param>
        /// <param name="codeId"></param>
        /// <returns></returns>
        private EntityCollection Select( string rowId, string resourceId, int codeId )
        {
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 3 ];
                sqlParameter[ 0 ] = new SqlParameter( "@RowId", rowId );
                sqlParameter[ 1 ] = new SqlParameter( "@ResourceId", resourceId );
                sqlParameter[ 2 ] = new SqlParameter( "@ResourceTypeId", codeId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.ResourceTypeSelect]", sqlParameter );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        Entity entity = Fill( dr );
                        collection.Add( entity );
                    }
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceTypeManager.Select(): " + ex.ToString() );
                return null;
            }
            return collection;
        }

        #endregion

        #region ====== Helper Methods ===============================================
        /// <summary>
        /// Fill an ResourceType object from a SqlDataReader
        /// </summary>
        /// <param name="dr">SqlDataReader</param>
        /// <returns>MyEntity</returns>
        public MyEntity Fill( SqlDataReader dr )
        {
            MyEntity entity = new MyEntity();

            entity.IsValid = true;
            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            //rowId = GetRowColumn( dr, "ResourceId", "" );
            //if ( rowId.Length > 35 )
            //    entity.ResourceId = new Guid( rowId );
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.CodeId = GetRowColumn( dr, "CodeId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "OriginalValue", "" );
            entity.MappedValue = GetRowPossibleColumn( dr, "MappedValue" );

            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.Created = GetRowColumn( dr, "Created", DateTime.MinValue );
            return entity;
        }//

        public Entity Fill( DataRow dr )
        {
            Entity entity = new Entity();
            string rowId = GetRowColumn( dr, "RowId", "" );
            if ( rowId.Length > 35 )
                entity.RowId = new Guid( rowId );

            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "OriginalType", "" );
            entity.MappedValue = GetRowPossibleColumn( dr, "MappedValue" );

            entity.CodeId = GetRowColumn( dr, "ResourceTypeId", 0 );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.Created = GetRowColumn( dr, "Created", DateTime.MinValue );

            return entity;
        }

        #endregion


        #region Import methods
        public Entity GetResourceMap(string rowId)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[1];
                sqlParameter[0] = new SqlParameter("@RowId", SqlDbType.UniqueIdentifier);
                sqlParameter[0].Value = new Guid(rowId);
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.ResourceTypeGet]", sqlParameter);
                if (DoesDataSetHaveRows(ds))
                {
                    Entity entity = Fill(ds.Tables[0].Rows[0]);
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceTypeManager.Get(): " + ex.ToString());
                return null;
            }
        }
        /*
        public string Create(Entity entity, ref string statusMessage)
        {
            statusMessage = "";
            string newId = "";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[4];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameter[1] = new SqlParameter("@ResourceTypeId", entity.CodeId);
                sqlParameter[2] = new SqlParameter("@OriginalValue", entity.OriginalValue);
                sqlParameter[ 3 ] = new SqlParameter( "@CreatedById", entity.CreatedById );

                SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.StoredProcedure, "[Resource.ResourceTypeInsert]", sqlParameter);
                if (dr.HasRows)
                {
                    dr.Read();
                    newId = dr[0].ToString();
                    entity.RowId = new Guid(newId);
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch (Exception ex)
            {
                LogError("ResourceTypeManager.Create(): " + ex.ToString());
                statusMessage = ex.Message;
            }
            return newId;
        }
        */
        #endregion

        public DataSet SelectedCodes( string pResourceId )
        {
            int resourceId = 0;

            if ( Int32.TryParse( pResourceId, out resourceId ) )
            {
                return SelectedCodes( resourceId );
            } else 
            {
                return null;
            }
        }

        public DataSet SelectedCodes( int pResourceIntId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[1];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                ds = SqlHelper.ExecuteDataset(LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.ResourceType_SelectedCodes]", sqlParameters);

                if (ds.HasErrors)
                {
                    return null;
                }
                return ds;
            }
            catch (Exception ex)
            {
                LogError(ex, className + ".SelectedCodes() ");
                return null;

            }
        }

        /// <summary>
        /// Adds/deletes Resource EducationLevel (only actual changes, rather than all)
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pNewSelectedItems"></param>
        /// <param name="pUnSelectedItems"></param>
        /// <returns></returns>
        public bool ApplyChanges(int pResourceId,
                                int pCreatedById,
                                string pNewSelectedItems,
                                string pUnSelectedItems)
        {
            string connectionString = LRWarehouse();
            string statusMessage = "";
            try
            {
                int counter = 0;

                foreach (string newItem in pNewSelectedItems.Split('|'))
                {
                    if (newItem.Length > 0)
                    {
                        int codeId = Int32.Parse(newItem);
                        Create(pResourceId, codeId, "", pCreatedById, ref statusMessage);

                        counter++;
                    }
                }

                foreach (string removedItem in pUnSelectedItems.Split('|'))
                {
                    if (removedItem.Length > 0)
                    {
                        int id = Int32.Parse(removedItem);
                        SqlParameter[] sqlParameters = new SqlParameter[2];
                        sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                        sqlParameters[1] = new SqlParameter("@ResourceTypeId", id);

                        SqlHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "[Resource.ResourceTypeDelete]", sqlParameters);

                        counter++;
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                LogError( ex, className + ".ApplyChanges() " );
                return false;
            }
        }

        public bool Insert( int pResourceId, int pCodeId, int pCreatedById, ref string statusMessage )
        {
            string result = Create( pResourceId, pCodeId, "", pCreatedById, ref statusMessage );
            if ( result != "" ) { return true; }
            else { return false; }
        }

    }
}
