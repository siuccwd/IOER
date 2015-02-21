using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO.Content
{
    public class Content_TagSummary
    {
        public int ContentTagId { get; set; }
        public int ContentId { get; set; }
        public int TagValueId { get; set; }
        public int CodeId { get; set; }
        public string TagTitle { get; set; }


        public System.DateTime Created { get; set; }
        public int CreatedById { get; set; }

        //external
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; }
        public string AliasValues { get; set; }

    }
}
