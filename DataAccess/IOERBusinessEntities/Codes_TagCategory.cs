//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IOERBusinessEntities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Codes_TagCategory
    {
        public Codes_TagCategory()
        {
            this.Codes_SiteTagCategory = new HashSet<Codes_SiteTagCategory>();
            this.Codes_TagValue = new HashSet<Codes_TagValue>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string SchemaTag { get; set; }
    
        public virtual ICollection<Codes_SiteTagCategory> Codes_SiteTagCategory { get; set; }
        public virtual ICollection<Codes_TagValue> Codes_TagValue { get; set; }
    }
}