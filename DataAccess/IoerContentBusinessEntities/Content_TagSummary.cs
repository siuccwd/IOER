//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IoerContentBusinessEntities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Content_TagSummary
    {
        public int ContentTagId { get; set; }
        public int ContentId { get; set; }
        public int TagValueId { get; set; }
        public int CodeId { get; set; }
        public string TagTitle { get; set; }
        public int CategoryId { get; set; }
        public string AliasValues { get; set; }
        public string CategoryTitle { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
    }
}
