using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Isle.DTO.Filters;

namespace Isle.DTO
{
    public class SiteTagCategory
    {

        public SiteTagCategory()
        {
            TagValues = new List<TagFilterBase>();
        }
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CategoryId { get; set; }
        /// <summary>
        /// Allowing site title for a category to differ from the base title
        /// </summary>
        public string Title { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Get/Set SortOrder
        /// Defaults to base TagCategory value on initial created, and then can be overridden to be site specific
        /// </summary>
        public int SortOrder { get; set; }

        //consider flattening TagCategory values here to simplify
        public string Description { get; set; }
        public string SchemaTag { get; set; }

        public List<TagFilterBase> TagValues { get; set; }
    }
}
