using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    
     [DataContract]
    public class GroupGetResponse : BaseResponse
    {
        [DataMember]
        public GroupDataContract Group { get; set; }
        
    }
}
