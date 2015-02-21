using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Script.Serialization;

namespace Isle.DTO
{
    public class ResourceEvaluationSummaryDTO
    {
        public ResourceEvaluationSummaryDTO()
        {
            Dimensions = new List<ResourceEvaluationDimensionSummary>();
        }

        public int ResourceIntId { get; set; }
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; }
        public bool RequiresCertification { get; set; }
        [ScriptIgnore]
        public string PrivilegeCode { get; set; }

        public int HasCertificationTotal { get; set; }
        [ScriptIgnore]
        public int HasCertificationTotalScore { get; set; }
        public int CertifiedAverageScore { get; set; }

        public int NonCertificationTotal { get; set; }
        [ScriptIgnore]
        public int NonCertificationTotalScore { get; set; }
        public int NonCertifiedAverageScore { get; set; }

        public bool CanUserDoEvaluation { get; set; }
        public bool UserHasCompletedEvaluation { get; set; }

        public List<ResourceEvaluationDimensionSummary> Dimensions { get; set; }
    }


    public class ResourceEvaluationDimensionSummary
    {

        public int EvalDimensionId { get; set; }
        public string DimensionTitle { get; set; }

        public int HasCertificationTotal { get; set; }
        [ScriptIgnore]
        public int HasCertificationTotalScore { get; set; }
        public int CertifiedAverageScore { get; set; }

        public int NonCertificationTotal { get; set; }
        [ScriptIgnore]
        public int NonCertificationTotalScore { get; set; }
        public int NonCertifiedAverageScore { get; set; }

    }
}
