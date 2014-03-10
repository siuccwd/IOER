using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class CodeSearchResponse : BaseResponse
    {
        [DataMember]
        public List<CodesDataContract> ResultList { get; set; }

        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public int ResultCount { get; set; }

        [DataMember]
        public int TotalRows { get; set; }
    }
}
