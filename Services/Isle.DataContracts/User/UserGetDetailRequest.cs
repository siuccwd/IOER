using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class GetUserDetailRequest:BaseRequest
    {
        [DataMember]
        public Guid? UserId { get; set; }
        [DataMember]
        public string LoginId { get; set; }
        [DataMember]
        public string Email { get; set; }
		//[DataMember]
		//public UserSearchTypeEnum UserSearchType { get; set; }   
        
    }
}
