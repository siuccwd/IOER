using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace Isle.BizServices
{
    public class BlackListedHostServices : Isle.BusinessServices.BaseService
    {
        private BlacklistedHostManager blackListedHostManager;

        public BlackListedHostServices()
        {
            blackListedHostManager = new BlacklistedHostManager();
        }

        public BlacklistedHost GetByHostname(string hostName, ref string status)
        {
            return blackListedHostManager.GetByHostname(hostName, ref status);
        }
    }
}
