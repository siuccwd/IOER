using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class CollectionDTO
    {
        public CollectionDTO()
        {
            resources = new List<ResourceDTO>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Url { get; set; }

        public List<ResourceDTO> resources { get; set; }
    }
}
