using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public enum StatusEnumDataContract
    {
        [EnumMember]
        Success,
        [EnumMember]
        Failure,    
        [EnumMember]
        NoData   
    }
}
