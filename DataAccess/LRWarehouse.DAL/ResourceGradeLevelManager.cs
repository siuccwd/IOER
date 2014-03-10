using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using MyEntity = LRWarehouse.Business.ResourceChildItem;
using Entity = LRWarehouse.Business.ResourceChildItem;
using EntityCollection = System.Collections.Generic.List<LRWarehouse.Business.ResourceChildItem>;

namespace LRWarehouse.DAL
{
    public class ResourceGradeLevelManager : BaseDataManager, IResourceIntManager
    {
        const string className = "ResourceGradeLevelManager";
                /// <summary>
        /// Base procedures
        /// </summary>
        //const string SELECT_PROC = "[Resource.GradeLevel_SelectedCodes]";
        const string SELECT_PROC = "[Resource.GradeLevelSelect]";
        //using same proc for select and selected codes as the source education level procs was essentially selected codes
        const string SELECTED_CODES_PROC = "[Resource.GradeLevel_SelectedCodes]";
        const string DELETE_PROC = "[Resource.GradeLevel_Delete]";
        const string INSERT_PROC = "[Resource.GradeLevel_Insert]";
        const string IMPORT_PROC = "[Resource.GradeLevel_Import]";
        public ResourceGradeLevelManager()
        {
        }

        #region Import Methods
        /// <summary>
        /// Import education level - multiple inserts are possible, so only a count is returned
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Import( Entity entity, ref string statusMessage )
        {
            statusMessage = "";
            int pTotalRows = 0;
            int outputCol = 2;
            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@resourceIntId", entity.ResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@OriginalValue", entity.OriginalValue );
                sqlParameters[ outputCol ] = new SqlParameter( "@TotalRows", SqlDbType.Int );
                sqlParameters[ outputCol ].Direction = ParameterDirection.Output;

                ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, IMPORT_PROC, sqlParameters );
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

                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( className + ".Import(): " + ex.ToString() );
                statusMessage = ex.Message;
            }
            return pTotalRows;
        }
        /// <summary>
        /// ???? is an update necessary?
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string Update( Entity entity )
        {
            string statusMessage = "successful";
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@Id", entity.Id );
                sqlParameters[ 1 ] = new SqlParameter( "@CodeId", entity.CodeId );
                sqlParameters[ 2 ] = new SqlParameter( "@MappedValue", "" );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[Resource.GradeLevel_Update]", sqlParameters );
            }
            catch ( Exception ex )
            {
                LogError( className + ".Update(): " + ex.ToString() );
                statusMessage = ex.Message;
            }
            return statusMessage;
        }
        #endregion


        #region ======Resource.GradeLevel Methods ===============================================
        public bool Delete( int pResourceId, int pGradeLevelId, ref string statusMessage )
        {
            return Delete( pResourceId, pGradeLevelId, 0, ref statusMessage );
        }//

        /// <summary>
        /// Delete an GradeLevel record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
        {
            return Delete( 0, 0, pId, ref statusMessage );
        }//

        private bool Delete( int pResourceId, int pGradeLevelId, int pId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@GradeLevelId", pGradeLevelId );
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

        public int Create( Entity gradeMap, ref string statusMessage )
        {
            int newID = 0;
            try
            {
                newID = int.Parse( Insert2( gradeMap.ResourceIntId, gradeMap.CodeId, gradeMap.CreatedById, ref statusMessage ) );
            }
            catch (Exception ex) {
                newID = 0;
            }
            return newID;
        }

        public void Insert( int pResourceIntId, int pGradeLevelId, int pCreatedById, ref string statusMessage, ref bool isValid, ref string newId )
        {
            newId = "";

            isValid = true;
            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@resourceIntId", pResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@GradeLevelId", pGradeLevelId );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", pCreatedById );
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
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
                LogError( ex, className + string.Format( ".Insert() for resource: {0} and pGradeLevelId: {1}", pResourceIntId, pGradeLevelId ) );
                statusMessage = className + "- Unsuccessful: Insert(): " + ex.Message.ToString();
                isValid = false;
            }

        }

        public string Insert2( int pResourceIntId, int pGradeLevelId, int pCreatedById, ref string statusMessage )
        {
            bool isValid = false;
            string newId = "";
            Insert( pResourceIntId, pGradeLevelId, pCreatedById, ref statusMessage, ref isValid, ref newId );
            return newId;
        }

        public bool Insert( int pResourceIntId, int pGradeLevelId, int pCreatedById, ref string statusMessage )
        {
            bool isValid = false;
            string newId = "";
            Insert( pResourceIntId, pGradeLevelId, pCreatedById, ref statusMessage, ref isValid, ref newId );
            return isValid;
        }


        public EntityCollection Select( int resourceIntId, string originalValue )
        {
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                //sqlParameters[ 1 ] = new SqlParameter( "@OriginalValue", originalValue );

                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );
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
                LogError( className + ".Select(): " + ex.ToString() );
                return null;
            }
            return collection;
        }

        public List<MyEntity> Select( int resourceIntId )
        {
            List<MyEntity> collection = new List<MyEntity>();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );

                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, sqlParameters );
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
                LogError( className + ".Select(): " + ex.ToString() );
                return null;
            }
            return collection;
        }
        public Entity Fill( DataRow dr )
        {
            Entity entity = new Entity();
            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.ResourceIntId = GetRowColumn( dr, "resourceIntId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "OriginalLevel", "" );
            entity.CodeId = GetRowColumn( dr, "GradeLevelId", 0 );

            return entity;
        }

        public DataSet SelectedCodes( int pResourceIntId ) 
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, SELECTED_CODES_PROC, sqlParameters );

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
        /// Adds/deletes Resource GradeLevel (only actual changes, rather than all)
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pNewSelectedItems"></param>
        /// <param name="pUnSelectedItems"></param>
        /// <returns></returns>
        public bool ApplyChanges( int pResourceIntId,
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
                        int pGradeLevelId = Int32.Parse( newItem );
                        Insert( pResourceIntId, pGradeLevelId, pCreatedById, ref statusMessage );

                        counter++;
                    }
                }

                foreach ( string removedItem in pUnSelectedItems.Split( '|' ) )
                {
                    if ( removedItem.Length > 0 )
                    {
                        int id = Int32.Parse( removedItem );
                        Delete( pResourceIntId, id, ref statusMessage );
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
        #endregion



    }
}
