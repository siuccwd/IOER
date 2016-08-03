using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using MyEntity = LRWarehouse.Business.ResourceChildItem;

namespace LRWarehouse.DAL
{
    public class ResourceItemTypeManager : BaseDataManager, IResourceIntManager
    {
        const string className = "ResourceItemTypeManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string SELECT_PROC = "[Resource.ItemTypeSelect]";
        const string DELETE_PROC = "[Resource.ItemTypeDelete]";
        const string INSERT_PROC = "[Resource.ItemTypeInsert]";
        const string UPDATE_PROC = "[Resource.ItemTypeUpdate]";
        const string IMPORT_PROC = "[Resource.ItemType_Import]";
        const string IMPORT_PROCV2 = "[Resource.ItemType_ImportV2]";

        /// <summary>
        /// Default constructor
        /// </summary>
        public ResourceItemTypeManager()
        {
            //base constructor sets common connection strings
        }//

        #region ====== Core Methods ===============================================
        public bool Delete( int pResourceId, int pItemTypeId, ref string statusMessage )
        {
            return Delete( pResourceId, pItemTypeId, 0, ref statusMessage );
        }//

        /// <summary>
        /// Delete an ItemType record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
        {
            return Delete( 0, 0, pId, ref statusMessage );
        }//

        private bool Delete( int pResourceId, int pItemTypeId, int pId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@ItemTypeId", pItemTypeId );
                sqlParameters[ 2 ] = new SqlParameter( "@Id", pId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, DELETE_PROC, sqlParameters );
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
        /// Required when implementing IResourceManager (issue with int vs string pResourceId)
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pItemTypeId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Insert( int pResourceId, int pItemTypeId, int pCreatedById, ref string statusMessage )
        {

            //if ( Int32.TryParse( pResourceId, out resourceId ) )
            if ( pResourceId > 0 )
            {
                int id = Create( pResourceId, pItemTypeId, pCreatedById, ref statusMessage );
                return true;
            }
            else
            {
                return false;
            }
        }
        public int Create( MyEntity entity, ref string statusMessage )
        {
            
            return Create( entity.ResourceIntId, entity.CodeId, entity.CreatedById, ref statusMessage );
        }

        public int Create( int pResourceId, int pItemTypeId, int pCreatedById, ref string statusMessage )
        {
            statusMessage = "successful";
            int newId = 0;

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                //sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", SqlDbType.UniqueIdentifier );
                //sqlParameters[ 0 ].Value = entity.ResourceId;
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@ItemTypeId", pItemTypeId );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedbyId", pCreatedById );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = GetRowColumn( dr, "Id", 0 );
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( className + ".Create(): " + ex.ToString() );
                statusMessage = ex.ToString();
            }

            return newId;
        }
        /// <summary>
        /// Update a Resource.ItemType record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( MyEntity entity )
        {
            string message = "successful";
            using ( SqlConnection conn = new SqlConnection( this.ConnString ) )
            {
                try
                {

                    #region parameters
                    SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                    sqlParameters[ 0 ] = new SqlParameter( "@RowId", entity.Id);
                    sqlParameters[ 1 ] = new SqlParameter( "@ItemTypeId", entity.CodeId );
                    sqlParameters[ 2 ] = new SqlParameter( "@LastUpdatedById", entity.LastUpdatedById );

                    #endregion

                    SqlHelper.ExecuteNonQuery( conn, UPDATE_PROC, sqlParameters );
                    message = "successful";

                }
                catch ( Exception ex )
                {
                    LogError( ex, className + string.Format( ".Update() for Id: {0} and LastUpdatedBy: {1}", entity.Id, entity.LastUpdatedBy ) );
                
                    message = className + "- Unsuccessful: Update(): " + ex.Message.ToString();
                    entity.Message = message;
                    entity.IsValid = false;
                }
            }
            return message;

        }//
        
        public DataSet SelectedCodes( string pResourceId )
        {
            int resourceId = 0;

            if ( Int32.TryParse( pResourceId, out resourceId ) )
            {
                return SelectedCodes( resourceId );
            }
            else
            {
                return null;
            }
        }

        public DataSet SelectedCodes( int pResourceId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.ItemType_SelectedCodes]", sqlParameters );

                if ( ds.HasErrors )
                {
                    return null;
                }
                return ds;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".SelectedCodes() " );
                return null;

            }
        }

        /// <summary>
        /// placeholder as required when implementing IResourceManager (issue with int vs string pResourceId)
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pNewSelectedItems"></param>
        /// <param name="pUnSelectedItems"></param>
        /// <returns></returns>
        public bool ApplyChanges( string pResourceId,
                                    int pCreatedById,
                                    string pNewSelectedItems,
                                    string pUnSelectedItems )
        {
            int resourceId = 0;

            if ( Int32.TryParse( pResourceId, out resourceId ) )
            {
                return ApplyChanges( resourceId, pCreatedById, pNewSelectedItems, pUnSelectedItems );
            }
            else
            {
                return false;
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
        public bool ApplyChanges( int pResourceId,
                                    int pCreatedById,
                                    string pNewSelectedItems,
                                    string pUnSelectedItems )
        {
            string connectionString = LRWarehouse();
            string statusMessage = "";
            try
            {
                int counter = 0;

                foreach ( string newItem in pNewSelectedItems.Split( '|' ) )
                {
                    if ( newItem.Length > 0 )
                    {
                        int codeId = Int32.Parse( newItem );
                        Insert( pResourceId, codeId, pCreatedById, ref statusMessage );

                        counter++;
                    }
                }

                foreach ( string removedItem in pUnSelectedItems.Split( '|' ) )
                {
                    if ( removedItem.Length > 0 )
                    {
                        int id = Int32.Parse( removedItem );
                        Delete( pResourceId, id, ref statusMessage );

                        counter++;
                    }

                }

                return true;
            }
            catch ( Exception ex )
            {
                LogError( ex, className + ".ApplyChanges() " );
                return false;
            }
        }

        public string Import(MyEntity entity)
        {
            string retVal = "successful";
            string connectionString = LRWarehouse();

            try
            {
                #region SQL Parameters
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parameters[1] = new SqlParameter("@ItemTypeId", entity.CodeId);
                parameters[2] = new SqlParameter("@OriginalValue", entity.OriginalValue);
                parameters[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
                parameters[3].Direction = ParameterDirection.Output;
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, IMPORT_PROC, parameters);
            }
            catch (Exception ex)
            {
                retVal = ex.Message;
                LogError(className + ".Import(): " + ex.ToString());
            }

            return retVal;
        }// Import

        public string ImportV2(MyEntity entity)
        {
            string retVal = "successful";
            string connectionString = LRWarehouse();

            try
            {
                #region SQL Parameters
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parameters[1] = new SqlParameter("@ItemTypeId", entity.CodeId);
                parameters[2] = new SqlParameter("@OriginalValue", entity.OriginalValue);
                parameters[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
                parameters[3].Direction = ParameterDirection.Output;
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, IMPORT_PROCV2, parameters);
            }
            catch (Exception ex)
            {
                retVal = ex.Message;
                LogError(className + ".ImportV2(): " + ex.ToString());
            }

            return retVal;
        }// ImportV2

        #endregion

        #region ====== Retrieval Methods ===============================================
        /// <summary>
        /// Select ItemType related data using passed parameters
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet Select( int resourceId )
        {

            //replace following with actual nbr of parameters and do assignments
            SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceId );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );

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

    }
}
