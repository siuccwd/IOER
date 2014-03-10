using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class BaseContract
    {
        [DataMember]
        public int Id { get; set; }

        //[DataMember]
        //public Guid RowId { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public int CreatedById { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public int LastUpdatedById { get; set; }
        //[DataMember]
        //public Guid? LastUpdatedBy { get; set; }
        //[DataMember]
        //public string LastUpdatedByName { get; set; }
        [DataMember]
        public DateTime LastUpdated { get; set; }
    }
}
