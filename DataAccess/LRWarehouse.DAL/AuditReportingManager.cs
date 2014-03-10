using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;


using Entity = LRWarehouse.Business.AuditReportDetail;

namespace LRWarehouse.DAL
{
    public class AuditReportingManager : BaseDataManager
    {
        public AuditReportingManager()
        {
            // Set up connection strings
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
        }

        public int CreateReport()
        {
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "AuditReport_Insert");
                if (DoesDataSetHaveRows(ds))
                {
                    int retVal = GetRowColumn(ds.Tables[0].Rows[0], "ReportId", 0);
                    return retVal;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "AuditReportingManager.CreateReport(): ");
                throw;
            }
        }

        public string CreateReportDetail(Entity entity)
        {
            string retVal = "successful";
            try
            {
                SqlParameter[] arParms = new SqlParameter[7];
                arParms[0] = new SqlParameter("@ReportId", entity.ReportId);
                arParms[1] = new SqlParameter("@FileName", SqlDbType.VarChar);
                arParms[1].Size = 100;
                arParms[1].Value = entity.FileName;
                arParms[2] = new SqlParameter("@DocID", SqlDbType.VarChar);
                arParms[2].Size = 100;
                arParms[2].Value = entity.DocId;
                arParms[3] = new SqlParameter("@URI", SqlDbType.VarChar);
                arParms[3].Size = 500;
                arParms[3].Value = entity.Uri;
                arParms[4] = new SqlParameter("@MessageType", SqlDbType.Char);
                arParms[4].Size = 1;
                arParms[4].Value = entity.MessageType;
                arParms[5] = new SqlParameter("@MessageRouting", SqlDbType.VarChar);
                arParms[5].Size = 2;
                arParms[5].Value = entity.MessageRouting;
                arParms[6] = new SqlParameter("@Message", SqlDbType.VarChar);
                arParms[6].Size = 200;
                arParms[6].Value = entity.Message;

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[AuditReport.Detail_Insert]", arParms);
            }
            catch (Exception ex)
            {
                LogError(ex, "AuditReportingManager.CreateReportDetail(): ");
                retVal = ex.Message;
            }

            return retVal;
        }

        public void LogMessage(int pReportId, string pFileName, string pDocId, string pUri, string pMessageType, string pRouting, string pMessage)
        {
            Entity entity = new Entity();
            entity.ReportId = pReportId;
            entity.FileName = pFileName;
            entity.DocId = pDocId;
            entity.Uri = pUri;
            entity.MessageType = pMessageType;
            entity.MessageRouting = pRouting;
            entity.Message = pMessage;

            CreateReportDetail(entity);
        }


    }
}
