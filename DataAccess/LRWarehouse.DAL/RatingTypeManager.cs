using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.ApplicationBlocks.Data;
using Entity = LRWarehouse.Business.RatingType;
using ErrorType = LRWarehouse.Business.ErrorType;
using ErrorRouting = LRWarehouse.Business.ErrorRouting;

namespace LRWarehouse.DAL
{
    public class RatingTypeManager : BaseDataManager
    {
        private string className = "RatingTypeManager";

        public RatingTypeManager()
        {
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }

        public int Create(Entity entity, ref string status)
        {
            status = "successful";

            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[3];
                sqlParameter[0] = new SqlParameter("@Type", SqlDbType.VarChar);
                sqlParameter[0].Size = 50;
                sqlParameter[0].Value = entity.Type;
                sqlParameter[1] = new SqlParameter("@Identifier", SqlDbType.VarChar);
                sqlParameter[1].Size = 500;
                sqlParameter[1].Value = entity.Identifier;
                sqlParameter[2] = new SqlParameter("@Description", SqlDbType.VarChar);
                sqlParameter[2].Size = 200;
                sqlParameter[2].Value = entity.Description;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Codes.RatingTypeInsert]", sqlParameter);
                if (DoesDataSetHaveRows(ds))
                {
                    int id = GetRowColumn(ds.Tables[0].Rows[0], "Id", 0);
                    entity.Id = id;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                status = ex.ToString();
                LogError(ex, className+".Create(): ");
            }

            return entity.Id;
        }

        public Entity Get(int id, string type, string identifier)
        {
            Entity entity = new Entity();
            try
            {
                SqlParameter[] sqlParameter = new SqlParameter[3];
                sqlParameter[0] = new SqlParameter("@Id", SqlDbType.Int);
                sqlParameter[0].Value = id;
                sqlParameter[1] = new SqlParameter("@Type", SqlDbType.VarChar);
                sqlParameter[1].Size = 50;
                sqlParameter[1].Value = type;
                sqlParameter[2] = new SqlParameter("@Identifier", SqlDbType.VarChar);
                sqlParameter[2].Size = 500;
                sqlParameter[2].Value = identifier;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Codes.RatingTypeGet]", sqlParameter);
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
                LogError(ex, className+".Get(): ");
            }
            return entity;
        }

        public Entity Fill(DataRow dr)
        {
            Entity entity = new Entity();
            entity.Id = GetRowColumn(dr, "Id", 0);
            entity.Type = GetRowColumn(dr, "Type", "");
            entity.Identifier = GetRowColumn(dr, "Identifier", "");
            entity.Description = GetRowColumn(dr, "Description", "");
            entity.Created = GetRowColumn(dr, "Created", new DateTime(2000, 01, 01));

            return entity;
        }

    }
}
