using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class PatronDataContract : BaseContract
    {
        [DataMember]
        public Guid UserId { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public Guid PatronStatusId { get; set; }
        [DataMember]
        public string PatronStatus { get; set; }
        [DataMember]
        public Guid? GenderId { get; set; }
        [DataMember]
        public string Gender { get; set; }
        [DataMember]
        public Guid? AgeGroupId { get; set; }
        [DataMember]
        public string AgeGroup { get; set; }        
        [DataMember]
        public string ImageUrl { get; set; }

        

    }
}
