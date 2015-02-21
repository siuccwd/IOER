using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.Business;

namespace LRWarehouse.DAL
{
    public class BlacklistedHostManager : BaseDataManager
    {
        public BlacklistedHost GetByHostname(string hostName, ref string status)
        {
            status = "successful";

            SqlParameter[] parm = new SqlParameter[1];
            parm[0] = new SqlParameter("@Hostname", hostName);

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Blacklist.HostsGetByHost]", parm);
                if (DoesDataSetHaveRows(ds))
                {
                    BlacklistedHost entity = Fill(ds.Tables[0].Rows[0]);
                    return entity;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError("BlacklistedHostManager.GetByHostname(): " + ex.ToString());
                status = ex.Message;
                return null;
            }
        }

        protected BlacklistedHost Fill(DataRow dr)
        {
            BlacklistedHost entity = new BlacklistedHost();
            entity.Id = GetRowColumn(dr, "Id", 0);
            entity.Hostname = GetRowColumn(dr, "Hostname", "");
            entity.RecordSource = GetRowColumn(dr, "RecordSource", "");
            entity.Created = GetRowColumn(dr, "Created", DateTime.Now);
            entity.CreatedById = GetRowColumn(dr, "CreatedById", 0);
            entity.LastUpdated = GetRowColumn(dr, "LastUpdated", DateTime.Now);
            entity.LastUpdatedById = GetRowColumn(dr, "LastUpdatedId", 0);

            return entity;
        }
    }
}
