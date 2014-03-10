using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class LocationSearchRequest : BaseRequest
    {
        [DataMember]
        public string Zipcode { get; set; }
        [DataMember]
        public int Miles { get; set; }
        [DataMember]
        public string PathwayName { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public int StartingPageNbr { get; set; }
        [DataMember]
        public int PageSize { get; set; }
    }
}
