using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    
   [DataContract]
    public class SiteFiltersTagsDTO 
    {

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public int SiteId { get; set; }
        [DataMember]
        public int CategoryId { get; set; }

        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Description { get; set; }
       //no need to return IsActive
        //[DataMember]
        //public bool IsActive { get; set; }
        [DataMember]
        public string SchemaTag { get; set; }
        [DataMember]
        public int SortOrder { get; set; }

        [DataMember]
        public List<SiteFilterValueDTO> FilterValues { get; set; }

    }
} //
