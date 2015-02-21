using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class ResourceStandardEvaluationSummary
    {
        public int ResourceIntId { get; set; }
        public int StandardId { get; set; }
        public string NotationCode { get; set; }
        public string Description { get; set; }
        public int AlignmentTypeId { get; set; }
        public string AlignmentType { get; set; }

        public int AverageRating { get; set; }

        public int TotalRatings { get; set; }

        public bool HasUserRated { get; set; }
    }
}
