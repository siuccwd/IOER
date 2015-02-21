using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Isle.DTO.Resource;

namespace Isle.DTO.Content
{
    [Serializable]
    public class ContentTag
    {
        public ContentTag()
        {
            OriginalValue = "";
            //Codes_TagCategory = "";
            Codes_TagValue = new CodesTagValue();
        }
        public int Id { get; set; }
        public int ResourceIntId { get; set; }
        public int TagValueId { get; set; }
        public System.DateTime Created { get; set; }
        public int CreatedById { get; set; }
        public string OriginalValue { get; set; }

        //external
        public int CategoryId { get; set; }
        //public CodesTagValue Codes_TagCategory { get; set; }
        public CodesTagValue Codes_TagValue { get; set; }
    }
}
