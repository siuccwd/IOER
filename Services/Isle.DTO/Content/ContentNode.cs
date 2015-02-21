using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class ContentNode
    {
        public ContentNode()
        {
            ChildNodes = new List<ContentNode>();
        }

        public int Id { get; set; }
        public int ParentId { get; set; }
        public int TypeId { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }

        public List<ContentNode> ChildNodes { get; set; }
    }

}
