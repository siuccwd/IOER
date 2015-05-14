using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class LibraryActivitySummary
    {
        public LibraryActivitySummary()
        {
            Collections = new List<CollectionActivitySummary>();
        }
        public int LibraryId { get; set; }
        public string Library { get; set; }


        public int LibraryViews { get; set; }
        public int ResourceViews { get; set; }

        public List<CollectionActivitySummary> Collections { get; set; }

    }

    public class CollectionActivitySummary
    {
        public int CollectionId { get; set; }
        public string Collection { get; set; }
        public int CollectionViews { get; set; }

    }
}
