using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class LibraryDTO
    {
        public LibraryDTO()
        {
            collections = new List<CollectionDTO>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }

        public int PublicAccessLevel { get; set; }
       
        /// <summary>
        /// Get/Set the access level for members of the same org
        /// </summary>
        public int OrgAccessLevel { get; set; }

        public bool UserHasDirectAccess { get; set; }
        public bool UserNeedsApproval{ get; set; }

        public List<CollectionDTO> collections { get; set; }
    }
}
