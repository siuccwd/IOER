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
        public int CommunityId { get; set; }
        public string Message { get; set; }
        public int CreatedById { get; set; }
        public DateTime Created { get; set; }

        public int RelatedPostingId { get; set; }

        #region Related data
        public string UserFullName { get; set; }
        public string UserImageUrl { get; set; }

        #endregion
    }
}
