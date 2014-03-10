using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class PatronUpdateRequest : BaseRequest
    {
        [DataMember]
        public PatronDataContract Patron { get; set; }
        
    }
}
