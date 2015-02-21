using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LRWarehouse.Business;

namespace Isle.DTO
{
    /// <summary>
    /// Starter DTO - may have separate ones for a summary, and for detailed
    /// Primary use is from a webservice, so only should include properties for the display purpose
    /// </summary>
    public class ResourceDTO
    {
        public ResourceDTO()
        {
            GradeLevels = new List<string>();
            Subjects = new List<string>();
            Standards = new List<StandardDTO>();
        }
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string OerUrl { get; set; }
        public string ResourceUrl { get; set; }

        public string Publisher { get; set; }
        public string Creator { get; set; }
        public string Requirements { get; set; }

        public List<string> GradeLevels { get; set; }
        public List<string> Subjects { get; set; }
        public List<StandardDTO> Standards { get; set; }

    }
}
