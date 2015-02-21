using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Isle.DTO
{
    public class StandardDTO
    {
        public StandardDTO()
        {
            
        }
        public int Id { get; set; }

        public string NotationCode { get; set; }
        public string Description { get; set; }

        public string AlignmentType { get; set; }
        public string AlignmentDegree { get; set; }

        public string StandardUrl { get; set; }

    }
}
