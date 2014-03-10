using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Isle.DataContracts
{
    [DataContract]
    public class CodesDataContract 
    {
        [DataMember]
        public string TableName { get; set; }
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Title { get; set; }
        //????? - should be able to just format title as needed
        [DataMember]
        public string FormattedTitle { get; set; }

        [DataMember]
        public int WarehouseTotal { get; set; }

    }
}
