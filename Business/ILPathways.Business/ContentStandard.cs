using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentStandard
    {
        public int Id { get; set; }
        public int ContentId { get; set; }
        public int StandardId { get; set; }
        public int AlignmentTypeCodeId { get; set; }
        public int UsageTypeId { get; set; }
        public System.DateTime Created { get; set; }
        public int CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public int LastUpdatedById { get; set; }
    }
}
