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
    
    public partial class Codes_OrgMemberRole
    {
        public Codes_OrgMemberRole()
        {
            this.Organization_MemberRole = new HashSet<Organization_MemberRole>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<bool> IsActive { get; set; }
    
        public virtual ICollection<Organization_MemberRole> Organization_MemberRole { get; set; }
    }
}