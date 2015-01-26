using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Text;
using Microsoft.ApplicationBlocks.Data;
using ResourceCluster = LRWarehouse.Business.ResourceCluster;
using ClusterMap = LRWarehouse.Business.ResourceChildItem;

namespace LRWarehouse.DAL
{
    public class ResourceClusterManager : BaseDataManager, IResourceIntManager
    {
        static string className = "ResourceClusterManager";

        public ResourceClusterManager()
        {
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }

        #region ====== Methods using ResourceIntId ===============================================
        //TODO - convert create to use int *****************************

        public bool Delete( int pResourceIntId, int pClusterId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@ClusterId", pClusterId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[Resource.ClusterDelete2]", sqlParameters );
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


        public string CreateFromEntity( ClusterMap cluster, ref string statusMessage )
        {
            return Insert( cluster.ResourceIntId, cluster.CodeId, cluster.CreatedById, ref statusMessage ).ToString();
        }

        /// <summary>
        /// Add an Resource.Cluster record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Insert( int pResourceIntId, int pClusterId, int pCreatedById, ref string statusMessage )
        {
            string connectionString = LRWarehouse();
            bool isValid = true;
            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@ClusterId", pClusterId );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", pCreatedById );
                #endregion

                SqlHelper.ExecuteNonQuery( connectionString, "[Resource.ClusterInsert2]", sqlParameters );

                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Insert() for pResourceIntId: {0} and pClusterId: {1}", pResourceIntId, pClusterId ) );
                statusMessage = className + "- Unsuccessful: Insert(): " + ex.Message.ToString();
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        /// Adds/deletes resource clusters (only actual changes, rather than all)
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
                        int codeId = Int32.Parse( newItem );
                        Insert( pResourceIntId, codeId, pCreatedById, ref statusMessage );

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


        public ResourceCluster Get(int resourceIntId, int clusterId)
        {
            ResourceCluster entity = new ResourceCluster();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[2];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@ClusterId", clusterId );

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.ClusterGet2]", sqlParameters);
                if (DoesDataSetHaveRows(ds))
                {

                    entity = Fill(ds.Tables[0].Rows[0]);
                }
                else
                {
                    entity.IsValid = false;
                    entity.Message = "Not found";
                }
            }
            catch (Exception ex)
            {
                entity.Message = ex.ToString();
                entity.IsValid = false;
                LogError(ex, "ResourceClusterManager().Get: ");
            }

            return entity;
        }

        /// <summary>
        /// get all clusters and indicate where selected for provided cluster id
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <returns></returns>
        public DataSet Select( int pResourceIntId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.ClusterSelect2]", sqlParameters );

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

        public DataSet SelectedCodes( int pResourceIntId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.ClusterSelect_SelectedCodes2]", sqlParameters );

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

        public string Import(ResourceCluster cluster)
        {
            string status = "successful";
            string connectionString = LRWarehouse();

            try
            {
                #region SQL Parameters
                SqlParameter[] parameters = new SqlParameter[4];
                parameters[0] = new SqlParameter("@ResourceIntId", cluster.ResourceIntId);
                parameters[1] = new SqlParameter("@ClusterId", cluster.ClusterId);
                parameters[2] = new SqlParameter("@OriginalValue", cluster.Title);
                parameters[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(connectionString, CommandType.StoredProcedure, "[Resource.Cluster_Import]", parameters);
            }
            catch (Exception ex) {
                LogError(className+".Import(): "+ex.ToString());
                status = ex.Message;
            }

            return status;
        }
        #endregion 

        #region ====== String Methods - OBSOLETE ===============================================
        /// <summary>
        /// Delete an ResourceCluster record using rowId
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <param name="pClusterId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        private bool Delete( string pResourceId, int pClusterId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@ClusterId", pClusterId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[Resource.ClusterDelete]", sqlParameters );
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

        private string Create( ResourceCluster entity )
        {
            string status = "successful";
            //try
            //{
            //    ResourceCluster exists = Get(entity.ResourceIntId, entity.ClusterId);
            //    //coordinate switchover
            //    //ResourceCluster exists = Get( entity.ResourceIntId, entity.ClusterId );

            //    if ( exists == null || exists.IsValid == false)
            //    {
            //        SqlParameter[] sqlParameters = new SqlParameter[ 3 ];  
            //        sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", entity.ResourceId );
            //        sqlParameters[ 1 ] = new SqlParameter( "@ClusterId", entity.ClusterId );
            //        sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
            //        //sqlParameters[ 3 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );

            //        SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.ClusterInsert]", sqlParameters);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LogError(ex, className + ".Create: ");
            //    status = ex.Message;
            //}

            return status;
        }

        /// <summary>
        /// Add an Resource.Cluster record ==> OLD, OBSOLETE
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        private bool Insert( string pResourceId, int pClusterId, int pCreatedById, ref string statusMessage )
        {
            //just to prevent errors - shouldn't be used
            //return false;

            string connectionString = LRWarehouse();
            bool isValid = true;
            try
            {

                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@ClusterId", pClusterId );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", pCreatedById );
                
                #endregion

                SqlHelper.ExecuteNonQuery( connectionString, "[Resource.ClusterInsert]", sqlParameters );

                statusMessage = "successful";

            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Insert() for RowId: {0} and pClusterId: {1}", pResourceId, pClusterId ) );
                statusMessage = className + "- Unsuccessful: Insert(): " + ex.Message.ToString();
                isValid = false;
            }

            return isValid;
        }
        /// <summary>
        /// Adds/deletes resource clusters (only actual changes, rather than all) - OBSOLETE
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="pNewSelectedItems"></param>
        /// <param name="pUnSelectedItems"></param>
        /// <returns></returns>
        private bool ApplyChanges( string pResourceId,
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



        private ResourceCluster Get( string pResourceId, int clusterId )
        {
            ResourceCluster entity = new ResourceCluster();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", SqlDbType.UniqueIdentifier );
                sqlParameters[ 0 ].Value = new Guid( pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@ClusterId", clusterId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.ClusterGet]", sqlParameters );
                if ( DoesDataSetHaveRows( ds ) )
                {

                    entity = Fill( ds.Tables[ 0 ].Rows[ 0 ] );
                }
                else
                {
                    entity.IsValid = false;
                    entity.Message = "Not found";
                }
            }
            catch ( Exception ex )
            {
                entity.Message = ex.ToString();
                entity.IsValid = false;
                LogError( ex, "ResourceClusterManager().Get: " );
            }

            return entity;
        }
        /// <summary>
        /// get all clusters and indicate where selected for provided cluster id
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <returns></returns>
        private DataSet Select( string pResourceId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceId", pResourceId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.ClusterSelect]", sqlParameters );

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

        private DataSet SelectedCodes( string pResourceId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.ClusterSelect_SelectedCodes]", sqlParameters );

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

        #endregion

        public ResourceCluster Fill(DataRow dr)
        {
            ResourceCluster cluster = new ResourceCluster();

            //cluster.ResourceId = new Guid(GetRowColumn(dr, "ResourceId", DEFAULT_GUID));
            cluster.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            cluster.ClusterId = GetRowColumn(dr, "ClusterId", 0);
            return cluster;
        }

    }
}
