using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class BaseResponse
    {
        public BaseResponse()
        {
            Error = new ErrorDataContract();
            //StatusId = StatusEnumDataContract.Success;
            MessageList = new List<ErrorDataContract>();
        }

        [DataMember]
        public StatusEnumDataContract Status { get; set; }
        [DataMember]
        public ErrorDataContract Error { get; set; }
        [DataMember]
        public List<ErrorDataContract> MessageList { get; set; }
    }
}
