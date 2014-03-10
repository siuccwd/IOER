using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace Isle.DataContracts
{
    [DataContract]
    public class PatronGetRequest : BaseRequest
    {
        [DataMember]
        public Guid? UserId { get; set; }
        [DataMember]
        public Guid? PatronId { get; set; }   
		//[DataMember]
		//public PatronSearchTypeEnum PatronSearchType { get; set; }
    }

}
