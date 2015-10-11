using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using LRWarehouse.Business;
using Isle.BizServices;
using Patron = LRWarehouse.Business.Patron;

namespace IOER.Controls.RubricsV2
{
	public partial class RubricsV2 : BaseUserControl
	{
		public RubricsV2()
		{
			ActiveRubric = new RubricV2();
		}

		public List<RubricV2> AvailableRubrics { get; set; }
		public RubricV2 ActiveRubric { get; set; }

		RubricV2Services service = new RubricV2Services();

		protected void Page_Load( object sender, EventArgs e )
		{
			//Validate resource
			if ( !GetResource() ) { return; }

			//Validate user
			GetUser();

			//Get rubric data
			GetRubricData();

			//If postback, select rubric
			if ( IsPostBack )
			{
				//Always do this
				LoadSelectedRubric();

				var postbackType = Request.Form[ "postbackType" ];
				if ( postbackType == "selectRubric" )
				{
					//
				}
				else if ( postbackType == "submitRating" )
				{
					IssueRating();
				}
			}
			else
			{

			}
		}

		//Get the user
		private void GetUser()
		{
			if ( IsUserAuthenticated() )
			{

			}
			else
			{
				Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
				return;
			}

			//Not sure if we need to do some kind of authorization checks
		}

		//Get resource data
		private bool GetResource()
		{
			var resourceID = int.Parse( Request.Params[ "resourceID" ] ?? (string) Page.RouteData.Values["resourceID"] ?? "0" );
			var resource = ResourceBizService.Resource_FillSummary( resourceID );
			if ( resource == null || resource.Id == 0 )
			{
				errorBox.Visible = true;
				errorMessage.Text = "That is an invalid Resource ID.";
				evaluationSelector.Visible = false;
				evaluator.Visible = false;
				return false;
			}

			ActiveRubric.ResourceId = resource.Id;
			ActiveRubric.ResourceUrl = resource.ResourceUrl;
			return true;
		}

		//Get rubric data
		private void GetRubricData()
		{
			AvailableRubrics = service.GetRubricListWithEvaluationDataForResource( 0, WebUser.Id, ActiveRubric.ResourceId );
		}

		//Get data for a chosen rubric
		private void LoadSelectedRubric()
		{
			evaluationSelector.Visible = false;
			evaluator.Visible = true;
			var id = int.Parse( Request.Form[ "rubricChoice" ] );
			var tempID = ActiveRubric.ResourceId;
			var tempURL = ActiveRubric.ResourceUrl;
			ActiveRubric = service.GetRubricWithEvaluationDataForResource( id, WebUser.Id, ActiveRubric.ResourceId );
			ActiveRubric.ResourceId = tempID;
			ActiveRubric.ResourceUrl = tempURL;

			ActiveRubric.Introduction.DimensionId = 0;
			ActiveRubric.Introduction.Title = "Introduction";
			ActiveRubric.Introduction.Description = ActiveRubric.Description;

			ActiveRubric.Finish.Title = "Ready to Finish";
			ActiveRubric.Finish.DimensionId = 999;
			ActiveRubric.Finish.Description = "If you are happy with your choices, click the Finish button.";
		}

		//Issue a rating
		private void IssueRating()
		{
			//Get score for each dimension
			var scores = new List<double>();
			foreach ( var item in ActiveRubric.Dimensions )
			{
				try
				{
					var score = double.Parse( Request.Form[ "score_" + item.DimensionId ] ); //May be set to NA, which will intentionally fail this test
					score = (score * 100) / 3; //convert 0-3 scale to 0-100 scale
					score = score > 100 ? 100 : score < 0 ? 0 : score; //Clamp score values to protect against spoofing
					item.ScorePercent = score;
					scores.Add( score );
				}
				catch
				{
					//Do not factor N/A scores into overall score
					//However, database expects N/As stored as -1
					item.ScorePercent = -1;
				}
			}
			if ( scores.Count() == 0 )
			{
				//Not sure what to do if no dimensions are applicable - presumably nothing
			}
			else
			{
				double average = Math.Ceiling( scores.Average() );
				ActiveRubric.OverallScore = average;
				service.IssueRating( ActiveRubric, ActiveRubric.ResourceId, WebUser.Id );
				Response.Redirect( "/resource/" + ActiveRubric.ResourceId + "?evaluationFinished=true" );
			}
		}

	}
}