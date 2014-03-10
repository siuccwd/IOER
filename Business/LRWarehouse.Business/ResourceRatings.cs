using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    public class ResourceRatings
    {
        public ResourceRatings()
        {
            standardRatings = new List<ResourceRating>();
            rubricRatings = new List<ResourceRating>();
        }
        public List<ResourceRating> standardRatings { get; set; }
        public List<ResourceRating> rubricRatings { get; set; }
    }

    public class ResourceRating
    {
        public ResourceRating()
        {
            myRating = -1;
            communityRating = 0.0;
        }
        public int id { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public double communityRating { get; set; }
        public int myRating { get; set; }
        public int ratingCount { get; set; }
        public int alignmentTypeID { get; set; }
        public string alignmentType { get; set; }
    }
}
