using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class CodeSearchRequest : BaseRequest
    {
        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public string IdColumn { get; set; }
        [DataMember]
        public string TitleColumn { get; set; }
        [DataMember]
        public string Filter { get; set; }
        [DataMember]
        public string OrderBy { get; set; }
        [DataMember]
        public bool IncludeTotals { get; set; }
        [DataMember]
        public bool UseWarehouseTotalTitle { get; set; }
       
    }
}
