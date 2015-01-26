using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using LRWarehouse.Business;
using System.Web.Script.Serialization;
using Isle.BizServices;

namespace ILPathways.Controls.Rubrics
{
  public partial class RubricCore : BaseUserControl
  {
    public RubricCore()
    {
      rubricData = new RubricDTO();
      rubricDataString = "{}";
    }

    private JavaScriptSerializer serializer = new JavaScriptSerializer();
    public RubricDTO rubricData { get; set; }
    public string rubricDataString { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      InitPage();
    }

    private void InitPage()
    {
      /* Regardless */
      rubricInfrastructure.Visible = true;
      //Resource URL
      rubricData.resourceURL = Request.Params[ "resourceURL" ];
      //Resource Int ID
      rubricData.resourceIntID = int.Parse( Request.Params[ "resourceIntID" ] );

      //Skip loading data if posting scores--jump straight to click handler instead.
      if ( hdnScoreHolder.Value == "selectingRubric" )
      {
        /* User has selected rubric + preview mode */
        if ( IsPostBack )
        {
          //Get preview option
          rubricData.previewMode = Request.Params[ "previewOption" ];
          //If a rubric was selected, show its control. Otherwise do nothing (the user will be taken back to the rubric selection page).
          if ( rblSelectRubric.SelectedItem != null )
          {
            rubricData.rubricID = int.Parse( rblSelectRubric.SelectedItem.Value );
            LoadRubric( rubricData.rubricID );
          }
          else
          {
            //Force load achieve rubric
            rubricData.rubricID = 1;
            LoadRubric( rubricData.rubricID );
          }
        }
        /* The user has not selected a rubric yet */
        else
        {
          rubricInfrastructure.Visible = false;
          ShowUsableRubrics();
        }
      }
    }

    private void LoadRubric( int id )
    {      
      //Get data
      var rubricRawData = ResourceBizService.Evaluation_GetComponents( rubricData.rubricID, true );

      //Short name is also the name of the control
      rubricData.selectedRubric = rubricRawData.ShortName;
      //Title
      rubricData.title = rubricRawData.Title;
      //Load dimensions
      foreach ( var item in rubricRawData.Dimensions )
      {
        rubricData.dimensions.Add( new BaseRubricDimensionModule() { id = item.Id, score = 0, name = item.Title } );
      }
      //Show the current Rubric control
      var currentRubric = ( BaseRubricModule ) FindControl( rubricData.selectedRubric );
      //currentRubric.rubricID = rubricData.rubricID; //may not need this
      rubricDataString = serializer.Serialize( rubricData );
      currentRubric.Visible = true;
      selectRubric.Visible = false;
    }


    private void ShowUsableRubrics()
    {
      var usableRubrics = ResourceBizService.Evaluations_GetAllTools( ( Patron ) WebUser ).OrderBy( m => m.Id ).ToList();
      foreach ( var item in usableRubrics )
      {
        var tempTitle = "";
        if ( !item.CanUserDoEvaluation )
        {
          if ( item.HasUserCompletedEvaluation ) { tempTitle = " (You already completed this Evaluation for this Resource.)"; }
          else if ( item.RequiresCertification ) { tempTitle = " (Requires Certification)"; }
          else { tempTitle = " (You cannot use this evaluation right now)"; } //Shouldn't happen
        }

        var rbl = new ListItem() { Value = item.Id.ToString(), Text = item.Title + tempTitle, Enabled = item.CanUserDoEvaluation };
        rblSelectRubric.Items.Add( rbl );
      }
    }

    private void SaveScores()
    {
      var evaluation = serializer.Deserialize<RubricDTO>( hdnScoreHolder.Value );
      var status = "";
      var canEvaluate = true;
      var hasCompleted = false;
      ResourceBizService.Evaluations_UserEvaluationStatus( evaluation.rubricID, evaluation.resourceIntID, (Patron) WebUser, ref hasCompleted, ref canEvaluate );
      if ( WebUser == null || WebUser.Id == 0 || !canEvaluate ) { return; }
      var dto = new Isle.DTO.ResourceEvaluationDTO(){
        ResourceIntId = evaluation.resourceIntID,
        EvaluationId = evaluation.rubricID,
        CreatedById = WebUser.Id,
        UserHasCertification = true //Need a way to double check this if it isn't covered by the "canEvaluate" bool
      };
      var totalScore = 0;
      var appliedDimensions = 0;
      foreach ( var item in evaluation.dimensions )
      {
        if ( item.id == 0 ) { continue; } //Don't save the overall score that comes back from the client DTO
        dto.Dimensions.Add( new Isle.DTO.ResourceEvaluationDimension()
        {
          CreatedById = WebUser.Id,
          EvalDimensionId = item.id,
          Score = (int) item.score
        } );
        if ( item.score != -1 ) {
          totalScore += (int) item.score;
          appliedDimensions++; 
        }
      }
      if ( appliedDimensions == 0 ) { dto.Score = 0; }
      else { dto.Score = ( totalScore / appliedDimensions ); }

      Isle.BizServices.ResourceBizService.Evaluations_SaveAchieveEvaluation( dto, ref status );
    }

    public void btnSubmitScores_Click( object sender, EventArgs e )
    {
      SaveScores();
      Response.Redirect( "/Resource/" + rubricData.resourceIntID + "?evaluationFinished=true" );
    }

    
  }
}