using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public class FollowingCollection
    {
        public FollowingCollection()
        {
            LibraryIds = new List<int>();
            CollectionIds = new List<int>();
            Message = "";
        }
        public int UserId { get; set; }
        public List<int> LibraryIds { get; set;}

        public List<int> CollectionIds { get; set;}

        public string Message { get; set; }
    }
}
