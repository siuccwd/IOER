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
    
    public partial class Library_FollowingSummary
    {
        public string Library { get; set; }
        public string LibraryType { get; set; }
        public int LibraryId { get; set; }
        public Nullable<int> LibraryResourceCount { get; set; }
        public int LibrarySectionId { get; set; }
        public string LibrarySection { get; set; }
        public string LibrarySectionType { get; set; }
        public int ResourceIntId { get; set; }
        public System.DateTime DateAddedToCollection { get; set; }
        public Nullable<int> libResourceCreatedById { get; set; }
        public string Title { get; set; }
        public Nullable<int> libraryFollowerId { get; set; }
        public Nullable<int> collectionFollowerId { get; set; }
    }
}
