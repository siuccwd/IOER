using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ILPathways.Business
{
    [Serializable]
    public class ContentPartner
    {

        public int Id { get; set; }
        public int ContentId { get; set; }
        public int UserId { get; set; }
        public int PartnerTypeId { get; set; }
        public System.DateTime Created { get; set; }
        public int CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public int LastUpdatedById { get; set; }

        public ContentItem Content { get; set; }
    }
}
