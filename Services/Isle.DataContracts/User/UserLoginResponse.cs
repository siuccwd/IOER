using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class UserLoginResponse:BaseResponse
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Zipcode { get; set; }
        [DataMember]
        public string RowId { get; set; }

        
    }
}
