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
    
    public partial class Patron_Profile
    {
        public int UserId { get; set; }
        public string MainPhone { get; set; }
        public string JobTitle { get; set; }
        public Nullable<int> PublishingRoleId { get; set; }
        public string RoleProfile { get; set; }
        public Nullable<int> OrganizationId { get; set; }
        public System.DateTime Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public System.DateTime LastUpdated { get; set; }
        public Nullable<int> LastUpdatedId { get; set; }
        public string Notes { get; set; }
        public string ImageUrl { get; set; }
    
        public virtual Patron Patron { get; set; }
    }
}
