using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    [Serializable]
    public class CodesTagValueKeyword
    {
        public int Id { get; set; }
        public int TagValueId { get; set; }
        public string Keyword { get; set; }
        public bool IsActive { get; set; }

        public System.DateTime Created { get; set; }
    }
}
