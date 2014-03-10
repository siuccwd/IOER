using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;



namespace LRWarehouse.DAL
{
    public class CleanseUrlManager : BaseDataManager
    {
        public DataSet GetCleansingRules(string host, ref string status)
        {
            status = "successful";
            SqlParameter[] sqlParameter = new SqlParameter[1];
            sqlParameter[0] = new SqlParameter("@Host", host);

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Map.CleanseUrlSelect]", sqlParameter);

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
                status = "CleanseUrlManager.GetCleansingRules(): " + ex.Message;
                LogError("CleanseUrlManager.GetCleansingRules(): " + ex.ToString());
                return null;
            }
        }
    }
}
