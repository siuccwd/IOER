using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace Isle.DataContracts
{
    [DataContract]
    public class GroupGetRequest : BaseRequest
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string GroupCode { get; set; }

    }

}
