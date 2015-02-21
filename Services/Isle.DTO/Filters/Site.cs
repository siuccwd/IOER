using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class Site
    {
        public Site()
        {
            SiteTagCategories = new List<SiteTagCategory>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SchemaTag { get; set; }
        public bool HasStandardsBrowser { get; set; }
        public string CssThemes { get; set; }
        public string ApiRoot { get; set; }

        public virtual List<SiteTagCategory> SiteTagCategories { get; set; }
    }
}
