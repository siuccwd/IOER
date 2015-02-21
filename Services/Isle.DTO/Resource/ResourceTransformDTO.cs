using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LRWarehouse.Business;

namespace Isle.DTO
{
    /// <summary>
    /// ResourceTransformDTO - used to transform client DTO to format for database persistence, and back
    /// </summary>
    public class ResourceTransformDTO
    {
        public ResourceTransformDTO()
        {
            ResourceUrl= "";
            Resource_Version = new ResourceVersion();

            ResourceTagsIds = new List<int>();
            ResourceTags = new List<ResourceTag>();
            ResourceStandardIds = new List<int>();
            Resource_Keywords = new List<string>();

        }
        //resource
        public int Id { get; set; }
        /// <summary>
        /// Add ResourceId as alias to Id
        /// </summary>
        public int ResourceId
        {
            get { return this.Id; }
            set { this.Id = value; }
        }

        public string ResourceUrl { get; set; }
        public System.Guid RowId { get; set; }
        public int CreatedById { get; set; }
        
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public System.DateTime Created { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public bool IsActive { get; set; }

        //status properties
        public bool ResourcePublished { get; set; }


        public int SelectedCollectionID { get; set; }

        [Obsolete]
        public bool HasPathwayGradeLevel { get; set; }

        //resource version
        //public string Title { get; set; }
        //public string Description { get; set; }

        //public string Publisher { get; set; }
        //public string Creator { get; set; }
        //public string Requirements { get; set; }

        public  ResourceVersion Resource_Version { get; set; }

        public int LanguageId { get; set; }

        public  List<string> Resource_Keywords { get; set; }
        public List<string> Resource_Subjects { get; set; }


        public List<int> ResourceStandardIds { get; set; }
        private List<ResourceStandard> _standard = new ResourceStandardCollection();
        public List<ResourceStandard> Standards
        {
            get { return this._standard; }
            set { this._standard = value; }
        }

        //list of sites to display resource (by default site id)
        public List<int> TargetSiteIds { get; set; }

        public List<int> ResourceTagsIds { get; set; }
        //tags ==> dont' want to have specific, as need to handle all sites
        //when populating from db, will include categoryId, and titles
        public List<ResourceTag> ResourceTags { get; set; }

        /// <summary>
        /// Select tags, using categoryId ==> only works when populated from database/EntityFrameworks, not sure about via interface as yet!
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<ResourceTag> SelectTags( int categoryId )
        {
            List<ResourceTag> tags = new List<ResourceTag>();
            //select request tags by category
            tags = ResourceTags
                        .Where( s => s.Codes_TagValue.CategoryId == categoryId )
                        .OrderBy(s => s.TagValueId )
                        .ToList();

            return tags;
        }

    } //
}
