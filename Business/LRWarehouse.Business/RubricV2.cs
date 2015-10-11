using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Script.Serialization;

namespace LRWarehouse.Business
{
	public class RubricV2
	{
		public RubricV2()
		{
			Introduction = new DimensionV2();
			Dimensions = new List<DimensionV2>();
			Finish = new DimensionV2();
		}
		public int RubricId { get; set; }
		public int ResourceId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string ResourceUrl { get; set; }
		public DimensionV2 Introduction { get; set; }
		public List<DimensionV2> Dimensions { get; set; }
		public DimensionV2 Finish { get; set; }
		public double OverallScore { get; set; }
		public bool HasUserRating { get; set; } //Did the user already use this rubric with this resource?
		public string RubricUrl { get; set; }

		public List<DimensionV2> GetMergedDimensions()
		{
			var list = new List<DimensionV2>() { Introduction };
			foreach ( var item in Dimensions )
			{
				list.Add( item );
			}
			list.Add( Finish );
			return list;
		}
		public List<int> GetMyIds(bool includeIntroAndFinish)
		{
			if ( includeIntroAndFinish )
			{
				return GetMergedDimensions().Select( m => m.DimensionId ).ToList();
			}
			else
			{
				return Dimensions.Select( m => m.DimensionId ).ToList();
			}
		}
		public string GetMyIdsJSON( bool includeIntroAndfinish )
		{
			return new JavaScriptSerializer().Serialize( GetMyIds( includeIntroAndfinish ) );
		}
		public string GetOverallScoreWord()
		{
			return OverallScore > 75 ? "Superior" : OverallScore > 50 ? "Strong" : OverallScore > 25 ? "Limited" : OverallScore > 0 ? "Very Weak" : "Not Applicable";
		}
	}


	public class DimensionV2
	{
		public int DimensionId { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public double ScorePercent { get; set; }
	}
}
