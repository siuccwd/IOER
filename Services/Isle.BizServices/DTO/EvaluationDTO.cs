using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.BizServices
{
    /// <summary>
    /// DTO for an Evaluation object
    /// </summary>
    public class EvaluationDTO
    {
        public EvaluationDTO()
        {
            Dimensions = new List<EvaluationDimensionDTO>();
            PrivilegeCode = "";
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public bool RequiresCertification { get; set; }
        public string PrivilegeCode { get; set; }

        public int WarehouseTotal { get; set; }

        public bool CanUserDoEvaluation { get; set; }
        public bool HasUserCompletedEvaluation{ get; set; }

        public List<EvaluationDimensionDTO> Dimensions { get; set; }
    }

    /// <summary>
    /// Dimensions for an evaluation
    /// </summary>
    public class EvaluationDimensionDTO
    {

        public int Id { get; set; }
        public int EvaluationId { get; set; }
        public int DimensionId { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int WarehouseTotal { get; set; }
    }
}
