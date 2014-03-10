using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class LocationDataContract
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }
        //[DataMember]
        //public int Lwia { get; set; }

        [DataMember]
        public string MainPhone { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string Address2 { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string Zipcode { get; set; }

        [DataMember]
        public string State { get; set; }
    }
}
