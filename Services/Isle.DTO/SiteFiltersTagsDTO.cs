using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isle.DTO
{
    public class SiteFiltersTagsDTO
    {

        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CategoryId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string SchemaTag { get; set; }
        public int SortOrder { get; set; }

        public List<SiteFilterValueDTO> FilterValues { get; set; }

    }
}
