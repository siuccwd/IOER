using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOERBusinessEntities
{
    /// <summary>
    /// Instantiate ResourceEntities context withOUT lazy loading
    /// </summary>
    public class ResourceContext : ResourceEntities
    {
        public ResourceContext()
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    }
}
