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
using Entity = LRWarehouse.Business.ResourceStandard;
using EntityCollection = LRWarehouse.Business.ResourceStandardCollection;

namespace LRWarehouse.DAL
{
    public class ResourceStandardManager : BaseDataManager
    {
        const string className = "ResourceStandardManager";

        public ResourceStandardManager()
        {
        }


        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete a ResourceStandard record using rowId
        /// </summary>
        /// <param name="pRowId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Delete( int id, ref string statusMessage )
        {
            bool successful;

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
                sqlParameter[ 0 ] = new SqlParameter( "@Id", id );

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[Resource.StandardDelete]", sqlParameter );
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
        public string Create(Entity entity, ref string statusMessage)
        {
            statusMessage = "";
            string newId = "";
            int id = 0;
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 6 ];
                //sqlParameter[ 0 ] = new SqlParameter( "@ResourceId", entity.ResourceId );
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", entity.ResourceIntId );
                sqlParameter[ 1 ] = new SqlParameter( "@StandardId", entity.StandardId );
                sqlParameter[ 2 ] = new SqlParameter( "@StandardUrl", entity.StandardUrl );
                sqlParameter[ 3 ] = new SqlParameter( "@AlignedById", entity.CreatedById );
                sqlParameter[ 4 ] = new SqlParameter( "@AlignmentTypeCodeId", entity.AlignmentTypeCodeId );
                sqlParameter[ 5 ] = new SqlParameter( "@AlignmentDegreeId", entity.AlignmentDegreeId );

                SqlDataReader dr = SqlHelper.ExecuteReader(ConnString, CommandType.StoredProcedure, "[Resource.StandardInsert]", sqlParameter);
                if (dr.HasRows)
                {
                    dr.Read();
                    newId = dr[0].ToString();
                    if ( Int32.TryParse( newId, out id ) == true )
                    {
                        entity.Id = id;
                    }
                    else if ( newId.Length == 36 )
                    {
                        entity.RowId = new Guid( newId );
                    }
                }
                dr.Close();
                dr = null;
                statusMessage = "successful";
            }
            catch (Exception ex)
            {
                LogError("ResourceStandardManager.Create(): " + ex.ToString());
                statusMessage = ex.Message;
            }
            return newId;
        }

        public string Import(Entity entity, ref string statusMessage)
        {
            statusMessage = "successful";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[6];
                sqlParameter[0] = new SqlParameter("@ResourceId", entity.ResourceId);
                sqlParameter[1] = new SqlParameter("@StandardId", entity.StandardId);
                sqlParameter[2] = new SqlParameter("@OriginalValue", entity.StandardUrl);
                sqlParameter[3] = new SqlParameter("@TotalRows", SqlDbType.Int);
                sqlParameter[3].Value = 0;
                sqlParameter[3].Direction = ParameterDirection.Output;
                sqlParameter[4] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                sqlParameter[5] = new SqlParameter("@AlignmentTypeValue", entity.AlignmentTypeValue);

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.Standard_Import]", sqlParameter);
            }
            catch (Exception ex)
            {
                LogError("ResourceStandardManager.Import(): " + ex.ToString());
                statusMessage = ex.Message;
            }

            return statusMessage;
        }
        #endregion

        #region ====== Retrieval Methods ===============================================

        public Entity Get( int id )
        {
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
                sqlParameter[ 0 ] = new SqlParameter( "@Id", id );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.StandardGet]", sqlParameter );
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
                LogError( "ResourceStandardManager.Get(): " + ex.ToString() );
                return null;
            }
        }

        public EntityCollection Select( int resourceId )
        {
            EntityCollection collection = new EntityCollection();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[ 1 ];
                sqlParameter[ 0 ] = new SqlParameter( "@ResourceIntId", resourceId );

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, "[Resource.StandardSelect]", sqlParameter );
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
                LogError( "ResourceStandardManager.Select(): " + ex.ToString() );
                return null;
            }

            return collection;
        }

        public Entity Fill( DataRow dr )
        {
            Entity entity = new Entity();
            entity.Id = GetRowColumn( dr, "Id", 0 );
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            //entity.ResourceId = new Guid( GetRowColumn( dr, "ResourceId", DEFAULT_GUID ) );
            entity.StandardUrl = GetRowColumn( dr, "StandardUrl", "" );
            entity.StandardId = GetRowColumn( dr, "StandardId", 0 );
            entity.CreatedById = GetRowColumn( dr, "AlignedById", 0 );
            entity.StandardNotationCode = GetRowColumn( dr, "NotationCode", "" );
            entity.StandardDescription = GetRowColumn( dr, "Standard", "" );

            entity.AlignmentTypeCodeId = GetRowColumn( dr, "AlignmentTypeCodeId", 0 );
            entity.AlignmentTypeValue = GetRowColumn( dr, "AlignmentTypeValue", "" );

            entity.AlignmentDegreeId = GetRowColumn( dr, "AlignmentDegreeId", 0 );
            entity.AlignmentDegree = GetRowColumn( dr, "AlignmentDegree", "" );

            return entity;
        }

        #endregion
    }
}
