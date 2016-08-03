using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoerContentBusinessEntities
{
    /// <summary>
    /// Instantiate Gateway context withOUT lazy loading
    /// </summary>
	public class GatewayContext : IsleContentContext
    {
        public GatewayContext()
        {
            this.Configuration.LazyLoadingEnabled = false;
        }
    }
}
