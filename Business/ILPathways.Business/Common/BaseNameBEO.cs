using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    public class BaseNameBEO : BaseBEO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid? LastUpdatedBy { get; set; }
        public string LastUpdatedByName { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }
}
