using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using Entity = LRWarehouse.Business.ResourceChildItem;
using MyEntity = LRWarehouse.Business.ResourceChildItem;
using EntityCollection = System.Collections.Generic.List<LRWarehouse.Business.ResourceChildItem>;

namespace LRWarehouse.DAL
{
    public class ResourceIntendedAudienceManager : BaseDataManager, IResourceManager
    {
        const string className = "ResourceIntendedAudienceManager";

        public ResourceIntendedAudienceManager()
        {
            //base constructor sets common connection strings
            //ConnString = LRWarehouse();
            //ReadOnlyConnString = LRWarehouseRO();
        }


        #region ====== Core Methods ===============================================
        public bool Delete( string pResourceId, int pAudienceId, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@AudienceId", pAudienceId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[Resource.IntendedAudienceDelete]", sqlParameters );
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

        public string CreateFromEntity( Entity entity, ref string statusMessage )
        {
            return Create( "", entity.ResourceIntId, entity.CodeId, entity.OriginalValue, entity.CreatedById, ref statusMessage );
           
        }

        /// <summary>
        /// Insert Resource.IntendedAudienceInsert
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <param name="pAudienceId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Insert( string pResourceId, int pAudienceId, int pCreatedById, ref string statusMessage )
        {
            string result = Create( pResourceId, pAudienceId, "", pCreatedById, ref statusMessage );
            if ( result != "" ) { return true; }
            else { return false; }
        }

        public string Create( string pResourceId, int pResourceIntId, int pAudienceId, string pOriginalValue, int pCreatedbyId, ref string statusMessage )
        {
            statusMessage = "successful";
            string newId = "";
            string proc = "[Resource.IntendedAudienceInsert]";


            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 4 ];
                //sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId.ToString() );
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@AudienceId", pAudienceId );
                sqlParameters[ 2 ] = new SqlParameter( "@OriginalAudience", pOriginalValue );
                sqlParameters[ 3 ] = new SqlParameter( "@CreatedbyId", pCreatedbyId );

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, proc, sqlParameters );

                if ( dr.HasRows )
                {
                    dr.Read();
                    newId = dr[ 0 ].ToString();
                    //entity.RowId = new Guid(newId);
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( "ResourceIntendedAudienceManager.Insert(): " + ex.ToString() );
                statusMessage = ex.ToString();
            }
            
            return newId;
        }

        public string Update( Entity entity )
        {
            string statusMessage = "successful";
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@RowId", entity.RowId );
                sqlParameters[ 1 ] = new SqlParameter( "@AudienceId", entity.CodeId );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[Resource.IntendedAudienceUpdate]", sqlParameters );
            }
            catch ( Exception ex )
            {
                LogError( "ResourceIntendedAudienceManager.Update(): " + ex.ToString() );
                statusMessage = ex.Message;
            }

            return statusMessage;
        }

        public string Create( string pResourceId, int pAudienceId, string pOriginalValue, int pCreatedbyId, ref string statusMessage )
        {
            return Create( pResourceId, 0, pAudienceId, pOriginalValue, pCreatedbyId, ref statusMessage );
        }

        /// <summary>
        /// Adds/deletes Resource IntendedAudience (only actual changes, rather than all)
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
            string connectionString = LRWarehouse();
            string statusMessage = "";
            try
            {
                int counter = 0;

                foreach ( string newItem in pNewSelectedItems.Split( '|' ) )
                {
                    if ( newItem.Length > 0 )
                    {
                        int AudienceId = Int32.Parse( newItem );
                        Insert( pResourceId, AudienceId, pCreatedById, ref statusMessage );

                        counter++;
                    }
                }

                foreach ( string removedItem in pUnSelectedItems.Split( '|' ) )
                {
                    if ( removedItem.Length > 0 )
                    {
                        int id = Int32.Parse( removedItem );
                        Delete( pResourceId, id, ref statusMessage );


                        //SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                        //sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );
                        //sqlParameters[ 1 ] = new SqlParameter( "@AudienceId", id );

                        //SqlHelper.ExecuteNonQuery( connectionString, CommandType.StoredProcedure, "[Resource.IntendedAudienceDelete]", sqlParameters );

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

        public int Import(Entity entity, ref string status)
        {
            int pTotalRows = 0;
            int pOutputColumn = 3;
            DataSet ds = new DataSet();
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[5];
                sqlParameters[0] = new SqlParameter("@ResourceId", DEFAULT_GUID); //OBSOLETE
                sqlParameters[1] = new SqlParameter("@IntendedAudienceId", SqlDbType.Int);
                sqlParameters[1].Value = entity.CodeId;
                sqlParameters[2] = new SqlParameter("@OriginalValue", SqlDbType.VarChar);
                sqlParameters[2].Size = 100;
                sqlParameters[2].Value = entity.OriginalValue;
                sqlParameters[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
                sqlParameters[3].Value = pTotalRows;
                sqlParameters[3].Direction = ParameterDirection.Output;
                sqlParameters[4] = new SqlParameter("@ResourceIntId", SqlDbType.Int);
                sqlParameters[4].Value = entity.ResourceIntId;
                #endregion

                ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.IntendedAudience_Import]", sqlParameters);
                string rows = sqlParameters[pOutputColumn].Value.ToString();
                try
                {
                    pTotalRows = int.Parse(rows);
                }
                catch
                {
                    pTotalRows = 0;
                }

                status = "successful";
            }
            catch (Exception ex)
            {
                LogError("ResourceIntendedAudienceManager.Import(): " + ex.ToString());
                status = ex.Message;
            }

            return pTotalRows;
        }

        public int ImportV2(Entity entity, ref string status)
        {
            int pTotalRows = 0;
            int pOutputColumn = 3;
            DataSet ds = new DataSet();
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[5];
                sqlParameters[0] = new SqlParameter("@ResourceId", DEFAULT_GUID); //OBSOLETE
                sqlParameters[1] = new SqlParameter("@IntendedAudienceId", SqlDbType.Int);
                sqlParameters[1].Value = entity.CodeId;
                sqlParameters[2] = new SqlParameter("@OriginalValue", SqlDbType.VarChar);
                sqlParameters[2].Size = 100;
                sqlParameters[2].Value = entity.OriginalValue;
                sqlParameters[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
                sqlParameters[3].Value = pTotalRows;
                sqlParameters[3].Direction = ParameterDirection.Output;
                sqlParameters[4] = new SqlParameter("@ResourceIntId", SqlDbType.Int);
                sqlParameters[4].Value = entity.ResourceIntId;
                #endregion

                ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.IntendedAudience_ImportV2]", sqlParameters);
                string rows = sqlParameters[pOutputColumn].Value.ToString();
                try
                {
                    pTotalRows = int.Parse(rows);
                }
                catch
                {
                    pTotalRows = 0;
                }

                status = "successful";
            }
            catch (Exception ex)
            {
                LogError("ResourceIntendedAudienceManager.ImportV2(): " + ex.ToString());
                status = ex.Message;
            }

            return pTotalRows;

        }
        #endregion

        #region ====== Retrieval Methods ===============================================

        public Entity Get( string rowId )
        {
            Entity entity = new Entity();
            SqlParameter[] sqlParameters = new SqlParameter[1];
            try
            {
                sqlParameters[0] = new SqlParameter("@RowId", rowId);

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.IntendedAudienceGet]", sqlParameters);
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
                LogError(className + ".Get(): " + ex.ToString());
                entity.IsValid = false;
                entity.Message = ex.ToString();
            }

            return entity;
        }

        [Obsolete]
        private Entity GetByResourceAndAudienceId(string resourceId, int audienceId )
        {
            Entity entity = new Entity();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", resourceId );
                sqlParameters[ 1 ] = new SqlParameter( "@AudienceId", audienceId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.IntendedAudienceGetByResourceIdAudienceId]", sqlParameters );
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
                LogError( className + ".Get(): " + ex.ToString() );
                entity.IsValid = false;
                entity.Message = ex.ToString();
            }

            return entity;
        }

        /// <summary>
        /// OBSOLETE
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        private EntityCollection SelectCollection( string resourceId )
        {
            EntityCollection collection = new EntityCollection();
            DataSet ds = Select( resourceId, 0 );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    Entity entity = Fill( dr );
                    collection.Add( entity );
                }
            }
            return collection;  
        }

        public EntityCollection SelectCollection( int pResourceIntId )
        {
            EntityCollection collection = new EntityCollection();
            DataSet ds = Select( "", pResourceIntId );
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    Entity entity = Fill( dr );
                    collection.Add( entity );
                }
            }
            return collection;
        }
        private DataSet Select( string pResourceId, int pResourceIntId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", "" );
                sqlParameters[ 1 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.IntendedAudienceSelect]", sqlParameters );

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
        /// OBSOLETE
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        private EntityCollection SelectCollectionV2(string resourceId)
        {
            EntityCollection collection = new EntityCollection();
            DataSet ds = SelectV2(resourceId, 0);
            if (DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    Entity entity = Fill(dr);
                    collection.Add(entity);
                }
            }
            return collection;
        }

        public EntityCollection SelectCollectionV2(int pResourceIntId)
        {
            EntityCollection collection = new EntityCollection();
            DataSet ds = SelectV2("", pResourceIntId);
            if (DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    Entity entity = Fill(dr);
                    collection.Add(entity);
                }
            }
            return collection;
        }
        private DataSet SelectV2(string pResourceId, int pResourceIntId)
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[2];
                sqlParameters[0] = new SqlParameter("@ResourceId", "");
                sqlParameters[1] = new SqlParameter("@ResourceIntId", pResourceIntId);

                ds = SqlHelper.ExecuteDataset(LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.IntendedAudienceSelectV2]", sqlParameters);

                if (ds.HasErrors)
                {
                    return null;
                }
                return ds;
            }
            catch (Exception ex)
            {
                LogError(ex, className + ".SelectV2() ");
                return null;

            }
        }

        public List<MyEntity> Select( int pResourceIntId )
        {

            List<MyEntity> collection = new List<MyEntity>();

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ResourceId", "" );
            sqlParameters[ 1 ] = new SqlParameter( "@pResourceIntId", pResourceIntId );

            DataSet ds = new DataSet();
            try
            {
                ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.IntendedAudienceSelect]", sqlParameters );

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

        public DataSet SelectedCodes( int pResourceId )
        {

            DataSet ds = new DataSet();
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[ 1 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceId );

                ds = SqlHelper.ExecuteDataset( LRWarehouseRO(), CommandType.StoredProcedure, "[Resource.IntendedAudience_SelectedCodes]", sqlParameters );

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
        public Entity Fill( DataRow dr )
        {
            Entity entity = new Entity();
            entity.RowId = new Guid( GetRowColumn( dr, "RowId", DEFAULT_GUID ) );
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            //entity.ResourceId = new Guid( GetRowColumn( dr, "ResourceId", DEFAULT_GUID ) );

            entity.OriginalValue = GetRowColumn( dr, "OriginalAudience", "" );
            entity.CodeId = GetRowColumn( dr, "AudienceId", 0 );

            return entity;
        }
        #endregion
    }
}
