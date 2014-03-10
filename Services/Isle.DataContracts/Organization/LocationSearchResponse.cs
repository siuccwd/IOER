using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class LocationSearchResponse : BaseResponse
    {
        [DataMember]
        public List<LocationDataContract> ResultList { get; set; }

        [DataMember]
        public int ResultCount { get; set; }

        [DataMember]
        public int TotalRows { get; set; }
    }
}
