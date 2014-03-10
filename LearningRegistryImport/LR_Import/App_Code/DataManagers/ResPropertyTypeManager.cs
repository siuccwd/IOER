using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Microsoft.ApplicationBlocks.Data;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace LearningRegistryCache2.App_Code.DataManagers
{
    public class ResPropertyTypeManager : BaseDataManager
    {
        public DataSet Lookup(string title)
        {
            try
            {
                SqlParameter[] sqlParameters = new SqlParameter[1];
                sqlParameters[0] = new SqlParameter("@Title", SqlDbType.VarChar);
                sqlParameters[0].Size = 50;
                sqlParameters[0].Value = title;

                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Codes.ResPropertyType_Lookup]", sqlParameters);
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
                new AuditReportingManager().LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", "", ErrorType.Error, ErrorRouting.Technical,
                    "ResPropertyTypeManager.Lookup(): " + ex.ToString());
                return null;
            }

        }
    }
}
