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
    public class ProhibitedKeywordManager : BaseDataManager
    {
        public List<ProhibitedKeyword> GetProhibitedKeywordRules(ref string status)
        {
            status = "successful";
            List<ProhibitedKeyword> prohibitedKeywords = new List<ProhibitedKeyword>();
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Import.ProhibitedKeywordGetAll]");
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ProhibitedKeyword pk = Fill(dr);
                        prohibitedKeywords.Add(pk);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ProhibitedKeywordManager.GetProhibitedKeywordRules(): " + ex.ToString());
                status = ex.Message;
            }

            return prohibitedKeywords;
        }

        private ProhibitedKeyword Fill(DataRow dr)
        {
            ProhibitedKeyword pk = new ProhibitedKeyword {
                Id = GetRowColumn(dr,"Id",0),
                Keyword = GetRowColumn(dr,"ProhibitedKeyword",""),
                IsRegex = GetRowColumn(dr,"IsRegex",false),
                Created=GetRowColumn(dr,"Created",DateTime.Now),
                CreatedBy=GetRowColumn(dr,"CreatedBy",""),
                LastUpdated=GetRowColumn(dr,"LastUpdated",DateTime.Now),
                LastUpdatedBy=GetRowColumn(dr,"LastUpdatedBy","")
            };

            return pk;
        }
    }
}
