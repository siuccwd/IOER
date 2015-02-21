using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class SiteFiltersDTO
    {
        public SiteFiltersDTO()
        {
            Message = "";
            FilterList = new List<SiteFiltersTagsDTO>();
        }
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string SiteName { get; set; }

        [DataMember]
        public int FiltersCount { get; set; }

        [DataMember]
        public List<SiteFiltersTagsDTO> FilterList { get; set; }

        [DataMember]
        public bool IsValid { get; set; }
        [DataMember]
        public string Message { get; set; }
    }
}
