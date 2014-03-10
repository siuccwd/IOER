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
{
    public class ResourceKeywordManager : BaseDataManager
    {
        const string className = "ResourceKeywordManager";

        /// <summary>
        /// Base procedures
        /// </summary>
        const string GET_PROC = "[Resource.KeywordGet]";
        const string SELECT_PROC = "[Resource.KeywordSelect]";
        const string DELETE_PROC = "[Resource.KeywordDelete]";
        const string INSERT_PROC = "[Resource.KeywordInsert]";

        public ResourceKeywordManager()
        {
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }
        #region Core
        /// <summary>
        /// Add an ResourceKeyword record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string Create( MyEntity entity, ref string statusMessage )
        {
            string newId = "";
            try
            {
                #region parameters
                SqlParameter[] sqlParameters = new SqlParameter[ 3 ];
                sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameters[ 1 ] = new SqlParameter( "@OriginalValue", entity.OriginalValue );
                sqlParameters[ 2 ] = new SqlParameter( "@CreatedById", entity.CreatedById );
                
                #endregion

                SqlDataReader dr = SqlHelper.ExecuteReader( ConnString, CommandType.StoredProcedure, INSERT_PROC, sqlParameters );
                if ( dr.HasRows )
                {
                    dr.Read();
                    //newId will be zeroes if the keyword being added exists in other tables
                    newId = dr[ 0 ].ToString();
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch ( Exception ex )
            {
                LogError( ex, className + string.Format( ".Create() for ResourceId: {0}, CodeId: {1}, Vale: {2}, and CreatedBy: {3}", entity.ResourceId.ToString(), entity.CodeId, entity.OriginalValue, entity.CreatedById ) );
                statusMessage = className + "- Unsuccessful: Create(): " + ex.Message.ToString();
            }

            return newId;
        }


        #endregion

        #region retrieve methods
        /// <summary>
        /// now obsolete
        /// </summary>
        /// <param name="pResourceId"></param>
        /// <returns></returns>
        private Entity Get( string pResourceId )
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceId", pResourceId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.KeywordGet]", sqlParameter );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    Entity entity = Fill( ds.Tables[ 0 ].Rows[ 0 ] );
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceKeywordManager.Get(): " + ex.ToString() );
                return null;
            }
        }

        /// <summary>
        /// ??????????????? a get is for a single value
        /// the proc expects id, not resourceIntId
        /// this can't work as is
        /// </summary>
        /// <param name="pResourceIntId"></param>
        /// <returns></returns>
        public Entity Get( int pResourceIntId )
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.KeywordGet]", sqlParameter );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    Entity entity = Fill( ds.Tables[ 0 ].Rows[ 0 ] );
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                LogError( "ResourceKeywordManager.Get(): " + ex.ToString() );
                return null;
            }
        }

        public List<MyEntity> Select( int pResourceIntId )
        {

            List<MyEntity> collection = new List<MyEntity>();

            SqlParameter[] sqlParameters = new SqlParameter[ 2 ];
            sqlParameters[ 0 ] = new SqlParameter( "@ResourceIntId", pResourceIntId );
            sqlParameters[ 1 ] = new SqlParameter( "@OriginalValue", "" );

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

        public EntityCollection Select( int resourceIntId, string originalValue )
        {
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 2 ];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                sqlParameter[ 1 ] = new SqlParameter( "@OriginalValue", originalValue );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, SELECT_PROC, sqlParameter );
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
                LogError( "ResourceKeywordManager.Select(): " + ex.ToString() );
                return null;
            }

            return collection;
        }

        public Entity Fill( DataRow dr )
        {
            Entity entity = new Entity();
            //entity.ResourceId = new Guid( GetRowColumn( dr, "ResourceId", DEFAULT_GUID ) );
            //entity.OriginalValue = GetRowColumn( dr, "OriginalValue", "" );

            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.OriginalValue = GetRowColumn( dr, "Keyword", "" );
            entity.CreatedById = GetRowColumn( dr, "CreatedById", 0 );
            entity.Created = GetRowColumn( dr, "Created", entity.DefaultDate );
            return entity;
        }
        #endregion

    }
}
