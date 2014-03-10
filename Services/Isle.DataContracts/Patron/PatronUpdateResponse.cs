using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class PatronUpdateResponse : BaseResponse
    {
        [DataMember]
        public PatronDataContract Patron {get;set;}
    }
}
