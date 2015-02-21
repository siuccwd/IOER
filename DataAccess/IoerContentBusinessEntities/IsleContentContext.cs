using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoerContentBusinessEntities
{
    /// <summary>
    /// Instantiate IsleContent context withOUT lazy loading
    /// </summary>
    public class IsleContentContext : IsleContentEntities
    {
        public IsleContentContext()
        {
            this.Configuration.LazyLoadingEnabled = false;

            ///workaround to force EntityFramework.SqlServer to be included when publishing
            bool instanceExists = System.Data.Entity.SqlServer.SqlProviderServices.Instance != null;
        }
    }
}
