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
    public class ResourceLanguageManager : BaseDataManager
    {
        private const string className = "ResourceLanguageManager";

        public ResourceLanguageManager()
        {
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }

        public string CreateFromEntity(Entity entity, ref string statusMessage)
        {
            string resourceRowId = "";
            if ( entity.ResourceId.ToString() != entity.DEFAULT_GUID )
                resourceRowId = entity.ResourceId.ToString();

            return Create( entity.ResourceIntId, entity.CodeId, entity.MappedValue, entity.CreatedById, ref statusMessage );
        }

        /// <summary>
        /// Create a Resource.Language record
        /// - assumes from interface, so original value will typically be blank but retained just in case
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <param name="pCodeId"></param>
        /// <param name="pOriginalValue">orig value should not be used for an insert, only import!!!</param>
        /// <param name="pCreatedById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create( int pResourceIntId, int pCodeId, string pOriginalValue, int pCreatedById, ref string statusMessage )
        {
            statusMessage = "";
            string newId = "";
            if ( pOriginalValue == pCodeId.ToString() )
                pOriginalValue = "";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 4 ];
                sqlParameter[ 0 ] = new SqlParameter( "@LanguageId", pCodeId );
                sqlParameter[ 1 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameter[ 2 ] = new SqlParameter( "@CreatedById", pCreatedById );
                sqlParameter[ 3 ] = new SqlParameter( "@OriginalLanguage", pOriginalValue );

                SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.StoredProcedure, "[Resource.LanguageInsert]", sqlParameter);
                if (dr.HasRows)
                {
                    dr.Read();
                    newId = dr[0].ToString();
                    //entity.RowId = new Guid(newId);
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch (Exception ex)
            {
                LogError("ResourceLanguageManager.Create(): " + ex.ToString());
                statusMessage = ex.Message;
            }
            return newId;
        }

        public string Import(Entity entity, ref string statusMessage)
        {
            statusMessage = "successful";
            string newId = "";
            //bool isDup = false;
            //string msg = "";
            int outputCol = 3;
            int pTotalRows = 0;
            DataSet ds = new DataSet();

            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[5];
                sqlParameters[0] = new SqlParameter("@ResourceId", entity.ResourceId);
                sqlParameters[1] = new SqlParameter("@LanguageId", entity.CodeId);
                sqlParameters[2] = new SqlParameter("@OriginalValue", entity.OriginalValue);
                sqlParameters[outputCol] = new SqlParameter("@TotalRows", SqlDbType.Int);
                sqlParameters[outputCol].Direction = ParameterDirection.Output;
                sqlParameters[4] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                #endregion

                ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.Language_Import]", sqlParameters);

                string rows = sqlParameters[outputCol].Value.ToString();
                try
                {
                    pTotalRows = Int32.Parse(rows);
                }
                catch
                {
                    pTotalRows = 0;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, className + string.Format(".Import() for ResourceId: {0}, CodeId: {1}, Value: {2}, and CreatedBy: {3}",
                    entity.ResourceId.ToString(), entity.CodeId, entity.OriginalValue, entity.CreatedById));
                statusMessage = className + " - Unsuccessful: Import(): " + ex.Message.ToString();
            }

            return newId;
        }
        public string Update(Entity entity)
        {
            string statusMessage = "successful";
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[2];
                sqlParameter[0] = new SqlParameter("@RowId", entity.RowId);
                sqlParameter[1] = new SqlParameter("@Language", entity.MappedValue);

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.LanguageUpdate]", sqlParameter);
            }
            catch (Exception ex)
            {
                LogError("ResourceLanguageManager.Update(): " + ex.ToString());
                statusMessage = ex.Message;
            }

            return statusMessage;
        }


        public Entity Get(string rowId)
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[1];
                sqlParameter[0] = new SqlParameter("@RowId", rowId);

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.LanguageGet]", sqlParameter);
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
                LogError("ResourceLanguageManager.Get(): " + ex.ToString());
                return null;
            }
        }

        public EntityCollection Select( int pResourceIntId, string language )
        {
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[2];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameter[ 1 ] = new SqlParameter( "@OriginalLanguage", language );


                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Resource.LanguageSelect]", sqlParameter);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Entity entity = Fill(dr);
                        collection.Add(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLanguageManager.Select(): " + ex.ToString());
                return null;
            }

            return collection;
        }
        public List<MyEntity> Select( int pResourceIntId )
        {
            List<MyEntity> collection = new List<MyEntity>();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 2 ];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
                sqlParameter[ 1 ] = new SqlParameter( "@OriginalLanguage", "" );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.LanguageSelect]", sqlParameter );
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
                LogError( "ResourceLanguageManager.Select(): " + ex.ToString() );
                return null;
            }

            return collection;
        }

        public Entity Fill(DataRow dr)
        {
            Entity entity = new Entity();
            entity.RowId = new Guid(GetRowColumn(dr, "RowId", DEFAULT_GUID));
            //entity.ResourceId = new Guid(GetRowColumn(dr, "ResourceId", DEFAULT_GUID));

            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.OriginalValue = GetRowColumn(dr, "OriginalLanguage", "");
            entity.CodeId = GetRowColumn(dr, "LanguageId", 0);

            return entity;
        }

        public DataSet SelectCodeTable()
        {
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Codes.LanguageSelect]");

                return ds;
            }
            catch (Exception ex)
            {
                LogError("ResourceLanguageManager.SelectCodeTable(): " + ex.ToString());
                return null;
            }
        }
    }
}
