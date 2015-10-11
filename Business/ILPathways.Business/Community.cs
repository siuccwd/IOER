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
        public int OrgId { get; set; }
        public int ContactId { get; set; }

        /// <summary>
        /// Get/Set the Public access level
        /// </summary>
        public EObjectAccessLevel PublicAccessLevel { get; set; }
        public int PublicAccessLevelInt
        {
            get
            {
                return ( int ) PublicAccessLevel;
            }
        }
        /// <summary>
        /// Get/Set the access level for members of the same org
        /// </summary>
        public EObjectAccessLevel OrgAccessLevel { get; set; }
        public int OrgAccessLevelInt
        {
            get
            {
                return ( int ) OrgAccessLevel;
            }
        }

		public bool IsModerated { get; set; }
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public int CreatedById { get; set; }

        public DateTime LastUpdated { get; set; }
        public int LastUpdatedById { get; set; }

        public List<CommunityPosting> Postings { get; set; }
    }
}
