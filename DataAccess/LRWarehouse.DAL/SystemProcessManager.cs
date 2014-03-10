using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.ApplicationBlocks.Data;
using Entity = LRWarehouse.Business.SystemProcess;

namespace LRWarehouse.DAL
{
    public class SystemProcessManager : BaseDataManager
    {

        const string className = "SystemProcessManager";

        public SystemProcessManager()
        {
            // base Constructor sets connection strings
        }

        public int Create(Entity entity, ref string status)
        {
            int retVal = 0;
            status = "successful";

            try
            {
                SqlParameter[] parameters = new SqlParameter[5];
                parameters[0] = new SqlParameter("@Code", SqlDbType.VarChar);
                parameters[0].Size = 50;
                parameters[0].Value = entity.Code;
                parameters[1] = new SqlParameter("@Title", SqlDbType.VarChar);
                parameters[1].Size = 100;
                parameters[1].Value = entity.Title;
                parameters[2] = new SqlParameter("@Description", SqlDbType.VarChar);
                parameters[2].Size = 1000;
                parameters[2].Value = entity.Description;
                parameters[3] = new SqlParameter("@CreatedBy", SqlDbType.VarChar);
                parameters[3].Size = 75;
                parameters[3].Value = entity.CreatedBy;
                parameters[4] = new SqlParameter("@LastRunDate", SqlDbType.DateTime);
                parameters[4].Value = entity.LastRunDate;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[System.ProcessInsert]", parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    retVal = GetRowColumn(ds.Tables[0].Rows[0], "Id", 0);
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".Create(): " + ex.ToString());
                status = className + ".Create(): " + ex.ToString();
            }

            return retVal;
        }

        public Entity Get(int id, ref string status) 
        {
            Entity entity = new Entity();
            status = "successful";

            try 
            {
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0] = new SqlParameter("@Id",SqlDbType.Int);
                parameters[0].Value=id;

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString,CommandType.StoredProcedure,"[System.ProcessGet]", parameters);
                if(DoesDataSetHaveRows(ds)) 
                {
                    entity = Fill(ds.Tables[0].Rows[0]);
                }
                else 
                {
                    entity = null;
                }
            }
            catch(Exception ex) 
            {
                LogError(className+".Get(): "+ex.ToString());
                status=className+".Get(): "+ex.ToString();
            }

            return entity;
        }

        public Entity GetByCode(string code, ref string status) 
        {
            Entity entity = new Entity();
            status = "successful";

            try 
            {
                SqlParameter[] parameters = new SqlParameter[1];
                parameters[0]=new SqlParameter("@Code",SqlDbType.VarChar);
                parameters[0].Size=50;
                parameters[0].Value=code;
                
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString,CommandType.StoredProcedure,"[System.ProcessGetByCode]",parameters);
                if(DoesDataSetHaveRows(ds)) {
                    entity = Fill(ds.Tables[0].Rows[0]);
                }
                else 
                {
                    entity = null;
                }
            }
            catch (Exception ex) 
            {
                LogError(className+".GetByCode(): "+ex.ToString());
                status=className+".GetByCode(): "+ex.ToString();
            }

            return entity;
        }
        

        public string Update(Entity entity) 
        {
            string retVal = "successful";

            try 
            {
                SqlParameter[] parameters = new SqlParameter[5];
                parameters[0]=new SqlParameter("@Id",entity.Id);
                parameters[1]=new SqlParameter("@Title",entity.Title);
                parameters[2]=new SqlParameter("@Description",entity.Description);
                parameters[3]=new SqlParameter("@LastUpdatedBy",entity.LastUpdatedBy);
                parameters[4]=new SqlParameter("@LastRunDate",SqlDbType.DateTime);
                parameters[4].Value=entity.LastRunDate;

                SqlHelper.ExecuteNonQuery(ConnString,CommandType.StoredProcedure,"[System.ProcessUpdate]",parameters);
            }
            catch (Exception ex) 
            {
                retVal=className+".Update(): "+ex.ToString();
                LogError(retVal);
            }

            return retVal;
        }


        public string UpdateLastRun( Entity entity )
        {
            string retVal = "successful";

            try
            {
                SqlParameter[] parameters = new SqlParameter[ 4 ];
                parameters[ 0 ] = new SqlParameter( "@Id", entity.Id);
                parameters[ 1 ] = new SqlParameter( "@StringParameter", entity.StringParameter );
                parameters[ 2 ] = new SqlParameter( "@IntParameter", entity.IntParameter );
                parameters[ 3 ] = new SqlParameter( "@LastUpdatedBy",  entity.LastUpdatedBy);

                SqlHelper.ExecuteNonQuery( ConnString, CommandType.StoredProcedure, "[System.ProcessUpdateLastRun]", parameters );
            }
            catch ( Exception ex )
            {
                retVal = className + ".UpdateLastRun(): " + ex.ToString();
                LogError( retVal );
            }

            return retVal;
        }

        public Entity Fill(DataRow dr)
        {
            Entity entity = new Entity();
            entity.Id = GetRowColumn(dr, "Id", 0);
            entity.Code = GetRowColumn(dr, "Code", "");
            entity.Title = GetRowColumn(dr, "Title", "");
            entity.Description = GetRowColumn(dr, "Description", "");
            entity.StringParameter = GetRowColumn( dr, "StringParameter", "" );
            entity.IntParameter = GetRowColumn(dr, "IntParameter", 0);

            entity.Created = GetRowColumn(dr, "Created", DateTime.Now);
            entity.CreatedBy = GetRowColumn(dr, "CreatedBy", "");
            entity.LastUpdated = GetRowColumn(dr, "LastUpdated", DateTime.Now);
            entity.LastUpdatedBy = GetRowColumn(dr, "LastUpdatedBy", "");
            entity.LastRunDate = GetRowColumn(dr, "LastRunDate", SqlDateTime.MinValue.Value);

            return entity;
        }
    }
}
