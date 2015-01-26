using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class CommunityPosting
    {

        public CommunityPosting()
        {
        }

        public int Id { get; set; }
        /// <summary>
        /// Note: posting can be in multiple communities. Need to use the PostItems collection. 
        /// The get may default this to the first community as a precaution
        /// </summary>
        public int CommunityId { get; set; }
        public string Message { get; set; }
        public int CreatedById { get; set; }
        public DateTime Created { get; set; }
        public int PostingTypeId { get; set; }
        public string PostingType { get; set; }

        public int RelatedPostingId { get; set; }
        public string PostingStatus { get; set; }

        #region Related data
        public string UserFullName { get; set; }
        public string UserImageUrl { get; set; }

        public List<CommunityPosting> ChildPostings { get; set; }
        public List<CommunityPostItem> PostItems { get; set; }
        #endregion
    }
}
