using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
    public class ResourceEvaluationsSummaryDTO
    {
        public ResourceEvaluationsSummaryDTO()
        {
            CerifiedEvaluations = new List<ResourceEvaluationDTO>();
            UnCertifiedEvaluations = new List<ResourceEvaluationDTO>();
        }

        public List<ResourceEvaluationDTO> CerifiedEvaluations { get; set; }
        public List<ResourceEvaluationDTO> UnCertifiedEvaluations { get; set; }

        public bool CurrentUserHasACertifiedEvaluation { get; set; }
        public bool CurrentUserHasAnUnCertifiedEvaluation { get; set; }

        public bool CurrentUserHasCertification { get; set; }
    }

    


    public class ResourceEvaluationDTO
    {
        public ResourceEvaluationDTO()
        {
            Dimensions = new List<ResourceEvaluationDimension>();
        }

        public int Id { get; set; }
        public int ResourceIntId { get; set; }
        public int EvaluationId { get; set; }
        public string EvaluationTitle { get; set; }
        public int CreatedById { get; set; }
        public int Score { get; set; }
        public bool UserHasCertification { get; set; }

        public List<ResourceEvaluationDimension> Dimensions { get; set; }
    }

    public class ResourceEvaluationDimension
    {

        public int Id { get; set; }
        public int ResourceEvalId { get; set; }
       // public int StandardId { get; set; }
        public int EvalDimensionId { get; set; }
        public int Score { get; set; }
        public int CreatedById { get; set; }
        public System.DateTime Created { get; set; }
    }
}
