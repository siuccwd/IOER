using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    [Serializable]
    public class CodesSite
    {
         public CodesSite()
        {
            SiteTagCategories = new List<CodesSiteTagCategory>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SchemaTag { get; set; }
        public bool HasStandardsBrowser { get; set; }
        public string CssThemes { get; set; }
        public string ApiRoot { get; set; }

        public virtual List<CodesSiteTagCategory> SiteTagCategories { get; set; }
    }
}
