using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
//using ILPathways.Common;

namespace Isle.DataContracts
{
    [DataContract]
    public class UserDataContract : BaseContract
    {
        [DataMember]
        public string LoginId { get; set; } 
        [DataMember]
        public string Email { get; set; }
		//[DataMember]
		//public AddressDataContract Address { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string NewPassword { get; set; }
        [DataMember]
        public bool Active { get; set; }
        [DataMember]
        public Guid UserTypeId { get; set; }
        [DataMember]
        public string UserType { get; set; }
		//[DataMember]
		//public UserTypeEnum UserTypeEnum { get; set; }  
		//[DataMember]
		//public LoginStatusEnum LoginStatus { get; set; }
        [DataMember]
        public bool IsPreRegistration { get; set; }
        
               
    }
}
