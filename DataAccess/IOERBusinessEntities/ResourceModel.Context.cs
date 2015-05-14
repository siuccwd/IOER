﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ResourceEntities : DbContext
    {
        public ResourceEntities()
            : base("name=ResourceEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<EmailNotice> EmailNotices { get; set; }
        public DbSet<Patron> Patrons { get; set; }
        public DbSet<Patron_Profile> Patron_Profile { get; set; }
        public DbSet<Codes_Subject> Codes_Subject { get; set; }
        public DbSet<Codes_TextType> Codes_TextType { get; set; }
        public DbSet<ConditionOfUse> ConditionOfUses { get; set; }
        public DbSet<Resource_Text> Resource_Text { get; set; }
        public DbSet<Codes_CareerCluster> Codes_CareerCluster { get; set; }
        public DbSet<Resource_Version_Summary> Resource_Version_Summary { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<System_GenerateLoginId> System_GenerateLoginId { get; set; }
        public DbSet<Codes_Site> Codes_Site { get; set; }
        public DbSet<Codes_SiteTagCategory> Codes_SiteTagCategory { get; set; }
        public DbSet<Codes_TagCategory> Codes_TagCategory { get; set; }
        public DbSet<Resource_Cluster> Resource_Cluster { get; set; }
        public DbSet<Resource_GradeLevel> Resource_GradeLevel { get; set; }
        public DbSet<Resource_Keyword> Resource_Keyword { get; set; }
        public DbSet<Resource_Subject> Resource_Subject { get; set; }
        public DbSet<Resource_Version> Resource_Version { get; set; }
        public DbSet<Codes_AudienceType> Codes_AudienceType { get; set; }
        public DbSet<Codes_GradeLevel> Codes_GradeLevel { get; set; }
        public DbSet<Resource_IntendedAudience> Resource_IntendedAudience { get; set; }
        public DbSet<Publish_Pending> Publish_Pending { get; set; }
        public DbSet<Resource_PublishedBy> Resource_PublishedBy { get; set; }
        public DbSet<StandardBody> StandardBodies { get; set; }
        public DbSet<StandardBody_Node> StandardBody_Node { get; set; }
        public DbSet<StandardBody_NodeGradeLevel> StandardBody_NodeGradeLevel { get; set; }
        public DbSet<StandardBody_Subject> StandardBody_Subject { get; set; }
        public DbSet<Resource_Tag> Resource_Tag { get; set; }
        public DbSet<Codes_TagValue> Codes_TagValue { get; set; }
        public DbSet<Resource_Standard> Resource_Standard { get; set; }
        public DbSet<Resource_AssessmentType> Resource_AssessmentType { get; set; }
        public DbSet<Resource_EducationUse> Resource_EducationUse { get; set; }
        public DbSet<Resource_Format> Resource_Format { get; set; }
        public DbSet<Resource_GroupType> Resource_GroupType { get; set; }
        public DbSet<Resource_ItemType> Resource_ItemType { get; set; }
        public DbSet<Resource_Language> Resource_Language { get; set; }
        public DbSet<Resource_ResourceType> Resource_ResourceType { get; set; }
        public DbSet<Patron_Following> Patron_Following { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Evaluation_Dimension> Evaluation_Dimension { get; set; }
        public DbSet<Codes_TagValueKeyword> Codes_TagValueKeyword { get; set; }
        public DbSet<Resource_Evaluation> Resource_Evaluation { get; set; }
        public DbSet<Resource_EvaluationSection> Resource_EvaluationSection { get; set; }
        public DbSet<Resource_StandardEvaluation> Resource_StandardEvaluation { get; set; }
        public DbSet<Resource_EvalDimensionsSummary> Resource_EvalDimensionsSummary { get; set; }
        public DbSet<Resource_EvaluationsSummary> Resource_EvaluationsSummary { get; set; }
        public DbSet<Resource_StandardEvaluationSummary> Resource_StandardEvaluationSummary { get; set; }
        public DbSet<Resource_StandardEvaluationList> Resource_StandardEvaluationList { get; set; }
        public DbSet<Codes_SiteTagCategory_Summary> Codes_SiteTagCategory_Summary { get; set; }
        public DbSet<Codes_TagCategoryValue_summary> Codes_TagCategoryValue_summary { get; set; }
        public DbSet<Patron_ResourceSummary> Patron_ResourceSummary { get; set; }
        public DbSet<Patron_Summary> Patron_Summary { get; set; }
        public DbSet<Resource_Like> Resource_Like { get; set; }
        public DbSet<Resource_LikeSummary> Resource_LikeSummary { get; set; }
        public DbSet<Resource_View> Resource_View { get; set; }
    }
}
