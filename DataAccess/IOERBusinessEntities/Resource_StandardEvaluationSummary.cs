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
    
    public partial class Resource_StandardEvaluationSummary
    {
        public int ResourceIntId { get; set; }
        public int StandardId { get; set; }
        public string NotationCode { get; set; }
        public string Description { get; set; }
        public Nullable<int> AverageScorePercent { get; set; }
        public Nullable<int> TotalEvals { get; set; }
    }
}
