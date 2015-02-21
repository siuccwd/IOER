using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ILPathways.Business;

namespace Isle.BizServices.DTO
{
    public class CurriculumDTO
    {
        public CurriculumDTO()
        {
            Curriculum = new ContentItem();
            Message = "";
        }
        public int StartingNodeId { get; set; }

        public ContentItem Curriculum { get; set; }
        public string Message { get; set; }
    }
}
