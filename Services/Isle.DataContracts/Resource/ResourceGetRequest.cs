using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class ResourceGetRequest : BaseRequest
    {
        [DataMember]
        public int ResourceId { get; set; }
        [DataMember]
        public int ResourceVersionId { get; set; }   

        
    }

}
