using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    [Serializable]
    public class CodesTagValue
    {
        public CodesTagValue()
        {
            Keywords = new List<string>();
            TagValueKeywords = new List<CodesTagValueKeyword>();
        }
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public int CodeId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string SchemaTag { get; set; }
        public int WarehouseTotal { get; set; }

        public List<string> Keywords { get; set; }
        public List<CodesTagValueKeyword> TagValueKeywords { get; set; }
    }
}
