using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class Community
    {
        public Community()
        {
        }

        public int Id { get; set; }
        public string Title { get; set; }

        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int ContactId { get; set; }
        public DateTime Created { get; set; }

        public List<CommunityPosting> Postings { get; set; }
    }
}
