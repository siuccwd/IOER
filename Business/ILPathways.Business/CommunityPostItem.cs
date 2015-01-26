using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class CommunityPostItem
    {
        public CommunityPostItem()
        {
        }
        public int Id { get; set; }
        public int CommunityId { get; set; }
        public int PostingIdId { get; set; }
        public int CreatedById { get; set; }
        public DateTime Created { get; set; }

    }
}
