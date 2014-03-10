using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class GroupDataContract : BaseContract
    {
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string GroupCode { get; set; }
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int GroupTypeId { get; set; }
        [DataMember]
        public string GroupType { get; set; }


        [DataMember]
        public int ContactId { get; set; }

        [DataMember]
        public int OrgId { get; set; }

        [DataMember]
        public int ParentGroupId { get; set; }
    }
}
