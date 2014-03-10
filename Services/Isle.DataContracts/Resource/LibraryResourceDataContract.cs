using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class LibraryResourceDataContract : ResourceDataContract
    {

        [DataMember]
        public string LibrarySection { get; set; }

        [DataMember]
        public int LibrarySectionId { get; set; }

        [DataMember]
        public int NbrComments { get; set; }
        [DataMember]
        public int NbrStandards { get; set; }
    }
}
