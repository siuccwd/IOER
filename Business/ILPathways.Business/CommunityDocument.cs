using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class CommunityDocument
    {
        public CommunityDocument()
        {
        }

        public int Id { get; set; }
        public int PostingId { get; set; }
        public Guid DocumentId { get; set; }
        public int CreatedById { get; set; }
        public DateTime Created { get; set; }


        
    }
}
