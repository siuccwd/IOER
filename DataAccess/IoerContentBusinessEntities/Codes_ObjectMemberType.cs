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
    
    public partial class Codes_ObjectMemberType
    {
        public Codes_ObjectMemberType()
        {
            this.Object_Member = new HashSet<Object_Member>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
    
        public virtual ICollection<Object_Member> Object_Member { get; set; }
    }
}
