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
    
    public partial class Resource_Evaluation
    {
        public Resource_Evaluation()
        {
            this.Resource_EvaluationSection = new HashSet<Resource_EvaluationSection>();
        }
    
        public int Id { get; set; }
        public int ResourceIntId { get; set; }
        public int EvaluationId { get; set; }
        public int Score { get; set; }
        public bool UserHasCertification { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> StandardId { get; set; }
        public Nullable<int> RubricId { get; set; }
        public Nullable<double> Value { get; set; }
        public Nullable<int> ScaleMin { get; set; }
        public Nullable<int> ScaleMax { get; set; }
        public string CriteriaInfo { get; set; }
    
        public virtual ICollection<Resource_EvaluationSection> Resource_EvaluationSection { get; set; }
        public virtual Evaluation Evaluation { get; set; }
        public virtual Resource Resource { get; set; }
    }
}