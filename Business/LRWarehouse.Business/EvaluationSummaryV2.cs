using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
	public class EvaluationSummaryV2
	{
		public int ResourceId { get; set; }
		public int EvaluationId { get; set; } //ID of the evaluation session itself
		public int ContextId { get; set; } //Standard or Dimension ID
		public double ScorePercent { get; set; }
		public int TotalEvaluations { get; set; }
		public int CreatedById { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}

}
