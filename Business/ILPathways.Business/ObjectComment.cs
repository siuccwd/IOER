using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ObjectComment : BaseBusinessDataEntity
    {
        public ObjectComment()
        {
            Comment = "";
        }

        public int ParentId { get; set; }
        public string Comment { get; set; }

        //derived
        public string LastName { get; set; }
    }
}
