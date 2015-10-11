using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.Controls.RubricsV2.Rubrics
{
	public partial class RubricCore : System.Web.UI.UserControl
	{
		public RubricCore()
		{
			Introduction = new Dimension();
			Dimensions = new List<Dimension>();
			Finish = new Dimension();
		}
		public int RubricId { get; set; }
		public int ResourceId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ResourceUrl { get; set; }
		public Dimension Introduction { get; set; }
		public List<Dimension> Dimensions { get; set; }
		public Dimension Finish { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{

		}
	}

	public class Dimension
	{
		public int DimensionId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public double ScorePercent { get; set; }
	}
}