using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class ResourceSearchResponse : BaseResponse
    {

        [DataMember]
        public int ResultCount { get; set; }

        [DataMember]
        public int TotalRows { get; set; }

        [DataMember]
        public List<ResourceDataContract> ResourceList { get; set; }

    }
}
