using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isle.DTO
{
    public class SiteFiltersDTO
    {
        public SiteFiltersDTO()
        {
            Message = "";
            FilterList = new List<SiteFiltersTagsDTO>();
        }
        public int Id { get; set; }

        public string SiteName { get; set; }

        public int FiltersCount { get; set; }

        public List<SiteFiltersTagsDTO> FilterList { get; set; }

        public bool IsValid { get; set; }
        public string Message { get; set; }
    }
}
