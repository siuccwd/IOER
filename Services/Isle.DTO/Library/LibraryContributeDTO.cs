using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    /// <summary>
    /// DTO for libraries to which a user can contribute
    /// </summary>
    public class LibraryContributeDTO
    {
        public LibraryContributeDTO()
        {
            libraries = new List<LibrarySummaryDTO>();
        }

        public int userId { get; set; }
        /// <summary>
        /// useDirectOnly - if true, only show libraries user has direct explicit access.
        /// Potentially could persist in user's profile to remember between uses
        /// </summary>
        public bool useDirectOnly { get; set; }

        public List<LibrarySummaryDTO> libraries { get; set; }
    }

    /// <summary>
    /// LibrarySummaryDTO
    /// </summary>
    public class LibrarySummaryDTO
    {
        public LibrarySummaryDTO()
        {
            collections = new List<CollectionSummaryDTO>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public int LibraryTypeId { get; set; }
        public string LibraryType { get; set; }
        public string ImageUrl { get; set; }
        /// <summary>
        /// Get/Set the access level for members of the same org
        /// </summary>
        public int PublicAccessLevel { get; set; }

        /// <summary>
        /// Get/Set the access level for members of the same org
        /// </summary>
        public int OrgAccessLevel { get; set; }
        public int CreatedById { get; set; }


        public bool IsPersonalLibrary { get; set; }

        /// <summary>
        /// AccessReason - for debugging mostly
        /// </summary>
        public string AccessReason { get; set; }
        public bool UserHasExplicitAccess { get; set; }
        public bool UserNeedsApproval { get; set; }

        public bool IsResourceInLibrary { get; set; }
        public int DefaultCollectionID { get; set; }


        public List<CollectionSummaryDTO> collections { get; set; }
    }

    public class CollectionSummaryDTO
    {
        public int id { get; set; }
        public string title { get; set; }
        public string avatarURL { get; set; }


        public bool UserHasDirectAccess { get; set; }
        public bool UserNeedsApproval { get; set; }

    }

}
