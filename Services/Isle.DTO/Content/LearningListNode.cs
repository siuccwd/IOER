using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class LearningListNode
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublished { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Created { get; set; }
        public int CreatedById { get; set; }
        //??
        public bool IsUserAuthor { get; set; }
        public int PartnerTypeId { get; set; }
        public string PartnerType { get; set; }

        public bool CanUserEdit { get; set; }
    }
}
