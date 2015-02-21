using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GatewayBusinessEntities
{
    /// <summary>
    /// Instantiate Gateway context withOUT lazy loading
    /// </summary>
    public class GatewayContext : GatewayEntities1
    {
        public GatewayContext()
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    }
}
