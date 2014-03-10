using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class PatronSearchRequest : BaseRequest
    {
        [DataMember]
        public Guid? UserId { get; set; }
        
        public Guid? PatronId { get; set; } 
		//[DataMember]
		//public PatronSearchTypeEnum PatronSearchType { get; set; }

    }

}
