using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class UserLoginRequest:BaseRequest
    {
        [DataMember]
        public string ServiceAccount { get; set; }

        [DataMember]
        public bool IsQuickLoginType { get; set; }

        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string UniqueId { get; set; }
        
    }
}