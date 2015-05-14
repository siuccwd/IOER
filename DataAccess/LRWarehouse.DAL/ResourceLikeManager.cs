using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using Microsoft.ApplicationBlocks.Data;
using ResourceLike = LRWarehouse.Business.ResourceLike;

namespace LRWarehouse.DAL
{
    public class ResourceLikeManager : BaseDataManager
    {
        const string className = "ResourceLikeManager";
        const string INSERT_PROC = "[Resource.LikeInsert]";
        const string GET_PROC = "[Resource.LikeGet]";
        const string SELECT_PROC = "[Resource.LikeSelect]";

        public int Create(ResourceLike entity, ref string status)
        {
            status = "successful";
            int retVal = 0;
            
            try
            {
                #region sqlParameters
                SqlParameter[] parameter = new SqlParameter[3];
              //  parameter[0] = new SqlParameter("@ResourceId", entity.ResourceId);
                parameter[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parameter[1] = new SqlParameter("@IsLike", entity.IsLike);
                parameter[2] = new SqlParameter("@CreatedById", entity.CreatedById);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, INSERT_PROC, parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    retVal = GetRowColumn(ds.Tables[0].Rows[0], "Id", 0);
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".Create(): " + ex.ToString());
                status = ex.Message;
            }
            
            return retVal;
        }

        public ResourceLike HasLike( int id, int userId, ref string status )
        {
            ResourceLike like = new ResourceLike();
            status = "successful";

            try
            {
                #region sqlParameters
                SqlParameter[] parameter = new SqlParameter[ 1 ];
                parameter[ 0 ] = new SqlParameter( "@Id", id );
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, GET_PROC, parameter );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    like = Fill( ds.Tables[ 0 ].Rows[ 0 ] );
                }
            }
            catch ( Exception ex )
            {
                LogError( className + ".Get(): " + ex.ToString() );
                status = ex.Message;
                return null;
            }

            return like;
        }

        public ResourceLike Get(int id, ref string status)
        {
            ResourceLike like = new ResourceLike();
            status = "successful";

            try
            {
                #region sqlParameters
                SqlParameter[] parameter = new SqlParameter[1];
                parameter[0] = new SqlParameter("@Id", id);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, GET_PROC, parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    like = Fill(ds.Tables[0].Rows[0]);
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".Get(): " + ex.ToString());
                status = ex.Message;
                return null;
            }

            return like;
        }

        public ResourceLike Fill(DataRow dr)
        {
            ResourceLike like = new ResourceLike();
            like.Id = GetRowColumn(dr, "Id", 0);
           // like.ResourceId = new Guid(GetRowColumn(dr, "ResourceId", DEFAULT_GUID));
            like.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            like.IsLike = GetRowColumn(dr, "IsLike", false);
            like.Created = GetRowColumn(dr, "Created", DateTime.Now);
            like.CreatedById = GetRowColumn(dr, "CreatedById", 0);

            return like;
        }


        public DataSet Select(int resourceId, DateTime startDate, DateTime endDate, string isLike, ref string status)
        {
            status = "successful";

            try
            {
                #region sqlParameters
                SqlParameter[] parameter = new SqlParameter[4];
                parameter[0] = new SqlParameter("@ResourceIntId", resourceId);
                parameter[1] = new SqlParameter("@StartDate", SqlDbType.DateTime);
                if (startDate < SqlDateTime.MinValue)
                {
                    parameter[1].Value = SqlDateTime.MinValue;
                }
                parameter[2] = new SqlParameter("@EndDate", SqlDbType.DateTime);
                if (endDate < SqlDateTime.MinValue || endDate > SqlDateTime.MaxValue)
                {
                    parameter[2].Value = DateTime.Now;
                }
                parameter[3] = new SqlParameter("@IsLike", isLike);
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".Select(): " + ex.ToString());
                status = ex.Message;
                return null;
            }
        }
    }
}
