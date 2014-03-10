using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class ErrorDataContract
    {
        public ErrorDataContract()
        {
            Message = "";
            MessageId = "";
        }
        public ErrorDataContract( string Id, string message )
        {
            Message = message;
            MessageId = Id;
        }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public string MessageId { get; set; }       

    }
}
