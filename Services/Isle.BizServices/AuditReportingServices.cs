using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace Isle.BizServices
{
    public class AuditReportingServices : Isle.BusinessServices.BaseService
    {
        private AuditReportingManager myManager;

        public AuditReportingServices()
        {
            myManager = new AuditReportingManager();
        }

        public int CreateReport()
        {
            return myManager.CreateReport();
        }

        public void LogMessage(int pReportId, string pFileName, string pDocId, string pUri, string pMessageType, string pRouting, string pMessage)
        {
            myManager.LogMessage(pReportId, pFileName, pDocId, pUri, pMessage, pRouting, pMessage);
        }
    }
}
