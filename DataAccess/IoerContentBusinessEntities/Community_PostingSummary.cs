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
    
    public partial class Community_PostingSummary
    {
        public int CommunityId { get; set; }
        public string Community { get; set; }
        public int CreatedById { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public string UserFullName { get; set; }
        public string UserImageUrl { get; set; }
        public int Id { get; set; }
        public string Message { get; set; }
        public int RelatedPostingId { get; set; }
        public int PostingId { get; set; }
        public Nullable<int> PostingTypeId { get; set; }
        public string PostingType { get; set; }
        public string PostingStatus { get; set; }
        public Nullable<int> ChildPostings { get; set; }
        public Nullable<int> OrgId { get; set; }
        public bool IsApproved { get; set; }
    }
}
