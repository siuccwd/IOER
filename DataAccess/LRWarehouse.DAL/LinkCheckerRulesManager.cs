using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using System.Linq;
using System.Text;

namespace LRWarehouse.DAL
{
    public class LinkCheckerRulesManager : BaseDataManager
    {
        public DataSet GetKnown404Pages(ref string status)
        {
            status = "successful";
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[LinkCheckerRules.GetKnown404Pages]");
                return ds;
            }
            catch (Exception ex)
            {
                LogError("LinkCheckerRulesManager.GetKnown404Pages(): " + ex.ToString());
                status = ex.Message;
            }

            return null;
        }


        public DataSet GetKnownBadContent(ref string status)
        {
            status = "successful";
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[LinkCheckerRules.GetKnownBadContent]");
                return ds;
            }
            catch (Exception ex)
            {
                LogError("LinkCheckerRulesManager.GetKnown404Pages(): " + ex.ToString());
                status = ex.Message;
            }

            return null;
        }

        public DataSet GetKnownBadTitle(ref string status)
        {
            status = "successful";
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[LinkCheckerRules.GetKnownBadTitle]");
                return ds;
            }
            catch (Exception ex)
            {
                LogError("LinkCheckerRulesManager.GetKnown404Pages(): " + ex.ToString());
                status = ex.Message;
            }

            return null;
        }

    }
}
