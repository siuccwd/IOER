using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using LRWarehouse.DAL;
using System.Data;
using LRWarehouse.Business;

using System.Web.Script.Serialization;

namespace ILPathways.LRW.controls
{
    public partial class Rubric3 : BaseUserControl
    {
        //Global variables
        public string resourceURL;
        protected int resourceIntID;
        protected int userID;
        public string returnMessage;
        private int resourceVID;

        //JSON stuff
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        public string rubricData;
        public string standardsData;

        protected void Page_Load( object sender, EventArgs e )
        {
            InitPage();
        }

        #region initialization

        protected void InitPage()
        {
            if ( IsUserAuthenticated() )
            {
                userID = WebUser.Id;
            }

            if ( Session[ "ResourceIntID" ] == null | Session[ "ResourceURL" ] == null )
            {
                return;
            }
            resourceIntID = int.Parse( Session[ "ResourceIntID" ].ToString() );
            resourceVID = int.Parse( Session[ "DetailVID" ].ToString() );
            //resourceIntID = 50362;
            RenderStandards( resourceIntID );
            DumpRubricToJSON();
            resourceURL = Session[ "ResourceURL" ].ToString();
            //resourceURL = "http://yrardtgasrga/";
            try
            {
                Comments.currentUserGUID = new System.Guid( Session[ "CurrentUserGUID" ].ToString() );
                Comments.currentResourceIntID = resourceIntID;
                Comments.usable = true;
            }
            catch ( Exception ex )
            {
                Comments.Visible = false;
                return;
            }
        }

        #endregion

        #region retrieval

        protected void RenderStandards( int ResourceIntID )
        {
            ResourceStandardManager manager = new ResourceStandardManager();
            ResourceStandardCollection standards = manager.Select( ResourceIntID );
            //ResourceStandardCollection standards = manager.Select( 50362 ); //Temporary, for testing
            jsonStandards jsonStandards = new jsonStandards();
            foreach ( ResourceStandard standard in standards )
            {
                jsonStandards.AddListItem( 
                    standard.StandardId.ToString(), 
                    standard.StandardNotationCode, 
                    standard.StandardUrl, 
                    standard.StandardDescription 
                );
            }
            jsonStandards.FinishList();
            standardsData = "standardsData = " + serializer.Serialize( jsonStandards ) + ";";
        }

        protected void DumpRubricToJSON()
        {
            string sql = rubricSelect.Text;
            DataSet ds = DatabaseManager.DoQuery( sql );
            jsonRubric rubric = new jsonRubric();
            if ( DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    rubric.AddListItem(
                        DatabaseManager.GetRowPossibleColumn( dr, "Id" ),
                        DatabaseManager.GetRowPossibleColumn( dr, "ParentId" ),
                        DatabaseManager.GetRowPossibleColumn( dr, "Notation" ),
                        DatabaseManager.GetRowPossibleColumn( dr, "pUrl" ),
                        DatabaseManager.GetRowPossibleColumn( dr, "Description" ) //Would like to have a Rubric ID - ask Jerome
                    );
                }
                rubric.FinishList();
            }
            rubricData = "rubricData = " + serializer.Serialize( rubric ) + ";";

        }

        #endregion

        #region subclasses
        [Serializable]
        public class jsonRubric
        {
            public jsonRubricItem[] items;
            private List<jsonRubricItem> listItems = new List<jsonRubricItem>();

            public void AddListItem( string id, string parentID, string notation, string pURL, string description )
            {
                jsonRubricItem item = new jsonRubricItem();
                item.id = id;
                item.parentID = parentID;
                item.notation = notation;
                item.pURL = pURL;
                item.description = description;
                listItems.Add( item );
            }
            public void FinishList()
            {
                items = listItems.ToArray();
            }
        }
        
        [Serializable]
        public class jsonRubricItem
        {
            public string id;
            public string parentID;
            public string notation;
            public string pURL;
            public string description;
        }

        [Serializable]
        public class jsonStandards
        {
            public jsonStandard[] items;
            private List<jsonStandard> listItems = new List<jsonStandard>();

            public void AddListItem( string id, string notation, string url, string description )
            {
                jsonStandard item = new jsonStandard();
                item.id = id;
                item.notation = notation;
                item.url = url;
                item.description = description;
                listItems.Add( item );
            }
            public void FinishList()
            {
                items = listItems.ToArray();
            }
        }

        [Serializable]
        public class jsonStandard
        {
            public string id;
            public string notation;
            public string url;
            public string description;
        }

        public class scoreHolder
        {
            public string rubricShorthand;
            public string id;
            public string score;
            public string max;
        }
        #endregion

        #region form actions
        public void btnFinish_Click( object sender, EventArgs e )
        {
            string[] values = BreakString( hdnScores.Value, "&" );
            ResourceEvaluation[] scoreStandards = GetScores( values[ 0 ], true );
            ResourceEvaluation[] scoreDimensions = GetScores( values[ 1 ], false );
            ResourceEvaluationManager manager = new ResourceEvaluationManager();
            string status = "";
            string[] criteria = new string[0];

            if ( values.Length > 2 ) //Store the selected/not selected criteria
            {
                criteria = BreakString( values[ 2 ], "~" );
                for ( var i = 0 ; i < criteria.Length ; i++ )
                {
                    criteria[ i ] = BreakString( criteria[ i ], ":" )[ 1 ];
                }
            }

            for ( int i = 0 ; i < scoreStandards.Length ; i++ )
            {
                manager.Create( scoreStandards[ i ], ref status );
            }
            for ( int i = 0 ; i < scoreDimensions.Length ; i++ )
            {
                if ( criteria.Length > 0 )
                {
                    scoreDimensions[ i ].CriteriaInfo = criteria[ i ];
                }
                manager.Create( scoreDimensions[ i ], ref status );
            }

            UpdateIndexScore();
            SetConsoleSuccessMessage( "Your evaluation has been received. Thanks!" );
            Response.Redirect( "/ResourceDetail.aspx?vid=" + resourceVID );
        }
        #endregion

        #region helper methods
        public string[] BreakString( string data, string separator )
        {
            return data.Split( new string[] { separator }, StringSplitOptions.RemoveEmptyEntries );
        }

        public ResourceEvaluation[] GetScores( string valueString, bool isStandards )
        {
            string[] values = BreakString( BreakString( valueString, "=" )[ 1 ], "," );
            List<scoreHolder> holders = new List<scoreHolder>();
            List<ResourceEvaluation> evaluations = new List<ResourceEvaluation>();
            for ( int i = 0 ; i < values.Length ; i++ )
            {
                string[] scores = BreakString( values[ i ], "|" );
                if ( scores[ 2 ] == "NaN" ) //Javascript's "Not a Number" error. In this case it means a rating was skipped.
                {
                    //Ratings aren't allowed to be skipped. Something goofed.
                    SetConsoleErrorMessage( "There was an error processing your request. Please try again." );
                    Response.Redirect( "/ResourceDetail.aspx?vid=" + resourceVID );
                }
                ResourceEvaluation evaluation = new ResourceEvaluation();

                evaluation.ResourceIntId = resourceIntID;
                evaluation.CreatedById = WebUser.Id;
                evaluation.Value = int.Parse( scores[ 2 ] );
                evaluation.ScaleMin = 0;
                evaluation.ScaleMax = int.Parse( scores[ 3 ] );
                evaluation.CriteriaInfo = "";
                if ( isStandards )
                {
                    evaluation.StandardId = int.Parse( scores[ 1 ] );
                }
                else
                {
                    evaluation.RubricId = int.Parse( scores[ 1 ] );
                }
                evaluations.Add( evaluation );

            }
            return evaluations.ToArray<ResourceEvaluation>();
        }
        #endregion

        #region elasticsearch update
        protected void UpdateIndexScore()
        {
            ResourceEvaluationManager manager = new ResourceEvaluationManager();
            string status = "";
            DataSet evaluations = manager.Select( resourceIntID, 0, 0, 0, ref status );
            if ( DoesDataSetHaveRows( evaluations ) )
            {
                //Gather Scores
                RubricScoreHolder holder = new RubricScoreHolder();
                RubricScoreJSON scoresOutput = new RubricScoreJSON();
                int evaluatingUserID = 0;
                int evaluationCount = 0;
                foreach ( DataRow dr in evaluations.Tables[ 0 ].Rows )
                {
                    if ( evaluatingUserID != ( int ) GetScore( dr, "CreatedById" ) )
                    {
                        evaluationCount++;
                        evaluatingUserID = ( int ) GetScore( dr, "CreatedById" );
                    }
                    //CCSS Score
                    if ( DatabaseManager.GetRowPossibleColumn( dr, "StandardId" ).Length > 0 )
                    {
                        decimal value = GetScore( dr, "Value" );
                        decimal max = GetScore( dr, "ScaleMax" );
                        holder.ccssAlignment += value;
                        holder.ccssAlignmentMax += max;
                        holder.AddStandard( ( int ) GetScore( dr, "StandardId" ), value, max );
                    }
                    //Dimensions
                    if ( DatabaseManager.GetRowPossibleColumn( dr, "RubricId" ).Length > 0 )
                    {
                        holder.dimensions[ ( int ) GetScore( dr, "RubricId" ) - 1 ] += GetScore( dr, "Value" );
                        holder.dimensionsMax[ ( int ) GetScore( dr, "RubricId" ) - 1 ] += GetScore( dr, "ScaleMax" );
                    }
                }
                //Calculate Scores
                scoresOutput.evaluationCount = evaluationCount;
                if ( holder.ccssAlignmentMax == 0 )
                {
                    scoresOutput.ccssAlignmentScore = 0;
                }
                else
                {
                    scoresOutput.ccssAlignmentScore = holder.ccssAlignment / holder.ccssAlignmentMax;
                }
                decimal totalScores = 0;
                decimal totalMaxes = 0;
                for ( int i = 0 ; i < 4 ; i++ )
                {
                    scoresOutput.dimensionsScores[ i ] = holder.dimensions[ i ] / holder.dimensionsMax[ i ];
                    totalScores += holder.dimensions[ i ];
                    totalMaxes += holder.dimensionsMax[ i ];
                }
                scoresOutput.dimensionsScore = totalScores / totalMaxes;
                scoresOutput.overallScore = ( scoresOutput.dimensionsScore + scoresOutput.ccssAlignmentScore ) / 2;
                holder.Finish();
                scoresOutput.standards = holder.standards;

                //Update ElasticSearch
                ElasticSearchManager esManager = new ElasticSearchManager();
                esManager.SetEvaluationScoreTotal( resourceIntID, scoresOutput.overallScore );
            }
        }
        public decimal GetScore( DataRow dr, string columnName )
        {
            try
            {
                return decimal.Parse( DatabaseManager.GetRowColumn( dr, columnName ) );
            }
            catch ( Exception ex )
            {
                return 0;
            }
        }
        public class RubricScoreHolder
        {
            private List<RubricStandardObject> listStandards = new List<RubricStandardObject>();
            public decimal ccssAlignment = 0;
            public decimal ccssAlignmentMax = 0;
            public RubricStandardObject[] standards;
            public decimal[] dimensions = new decimal[] { 0, 0, 0, 0 };
            public decimal[] dimensionsMax = new decimal[] { 0, 0, 0, 0 };

            public void AddStandard( int id, decimal score, decimal max )
            {
                RubricStandardObject standard = new RubricStandardObject();
                standard.id = id;
                standard.score = score;
                standard.max = max;
                listStandards.Add( standard );
            }
            public void Finish()
            {
                standards = listStandards.ToArray<RubricStandardObject>();
            }
        }
        public class RubricStandardObject
        {
            public decimal score;
            public decimal max;
            public int id;
        }
        [Serializable]
        public class RubricScoreJSON
        {
            public decimal ccssAlignmentScore = 0;
            public decimal[] dimensionsScores = new decimal[] { 0, 0, 0, 0 };
            public decimal dimensionsScore = 0;
            public decimal overallScore = 0;
            public RubricStandardObject[] standards;
            public int evaluationCount = 0;
        }
        #endregion

    }
}