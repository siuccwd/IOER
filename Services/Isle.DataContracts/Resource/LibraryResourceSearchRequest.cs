using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{

    [DataContract]
    public class LibraryResourceSearchRequest : BaseRequest
    {
        [DataMember]
        public int LibraryId { get; set; }
        [DataMember]
        public string Filter { get; set; }
        [DataMember]
        public string Keywords { get; set; }
        [DataMember]
        public string SortOrder { get; set; }

        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int StartingPageNbr { get; set; }
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public bool OutputRelTables { get; set; }


    }

}
