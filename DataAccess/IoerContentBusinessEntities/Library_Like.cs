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
    
    public partial class Library_Like
    {
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public bool IsLike { get; set; }
        public System.DateTime Created { get; set; }
        public int CreatedById { get; set; }
    
        public virtual Library Library { get; set; }
    }
}
