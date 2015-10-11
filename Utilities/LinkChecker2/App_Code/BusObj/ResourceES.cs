using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkChecker2.App_Code.BusObj
{
    public class ResourceES : ResourceBase
    {
        public ResourceES()
        {
            GradeAliases = new List<string>();
            LibraryIds = new List<int>();
            CollectionIds = new List<int>();
            StandardIds = new List<int>();
            StandardNotations = new List<string>();
            Paradata = new ParadataES();
            Fields = new List<FieldES>();
        }

        //Special Fields
        public List<string> GradeAliases { get; set; }
        public List<int> LibraryIds { get; set; }
        public List<int> CollectionIds { get; set; }
        public List<int> StandardIds { get; set; }
        public List<string> StandardNotations { get; set; }
        public ParadataES Paradata { get; set; }
        public List<FieldES> Fields { get; set; }
    }
}
