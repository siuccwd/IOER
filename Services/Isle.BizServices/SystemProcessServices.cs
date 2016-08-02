using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Entity = LRWarehouse.Business.SystemProcess;
using LRWarehouse.DAL;

namespace Isle.BizServices
{
    public class SystemProcessServices
    {
        SystemProcessManager systemProcessManager;

        public SystemProcessServices()
        {
            systemProcessManager = new SystemProcessManager();
        }

        public int Create(Entity entity, ref string status)
        {
            return systemProcessManager.Create(entity, ref status);
        }

        public Entity Get(int id, ref string status)
        {
            return systemProcessManager.Get(id, ref status);
        }

        public Entity GetByCode(string code, ref string status)
        {
            return systemProcessManager.GetByCode(code, ref status);
        }

        public string Update(Entity entity)
        {
            return systemProcessManager.Update(entity);
        }

        public string UpdateLastRun(Entity entity)
        {
            return systemProcessManager.UpdateLastRun(entity);
        }
    }
}
