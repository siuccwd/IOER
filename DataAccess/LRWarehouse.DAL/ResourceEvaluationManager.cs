using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using LRWarehouse.Business;
using Microsoft.ApplicationBlocks.Data;

namespace LRWarehouse.DAL
{
    public class ResourceEvaluationManager : BaseDataManager
    {
        const string className = "ResourceEvaluationManager";
        const string INSERT_PROC = "[Resource.EvaluationInsert]";
        const string GET_PROC = "[Resource.EvaluationGet]";
        const string SELECT_PROC = "[Resource.EvaluationSelect]";

        /// <summary>
        /// Insert Resource Evaluation row
        /// </summary>
        /// <param name="evaluation"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private int Create( ResourceEvaluation evaluation, ref string status )
        {
            status = "successful";
            int retVal = 0;

            try
            {
                #region parameters
                SqlParameter[] parameters = new SqlParameter[ 8 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", evaluation.ResourceIntId );
                parameters[ 1 ] = new SqlParameter( "@CreatedById", evaluation.CreatedById );
                parameters[ 2 ] = new SqlParameter( "@StandardId", evaluation.StandardId );
                parameters[ 3 ] = new SqlParameter( "@RubricId", evaluation.RubricId );
                parameters[ 4 ] = new SqlParameter( "@Value", evaluation.Value );
                parameters[ 5 ] = new SqlParameter( "@ScaleMin", evaluation.ScaleMin );
                parameters[ 6 ] = new SqlParameter( "@ScaleMax", evaluation.ScaleMax );
                parameters[ 7 ] = new SqlParameter( "@CriteriaInfo", SqlDbType.VarChar );
                parameters[ 7 ].Size = 500;
                parameters[ 7 ].Value = evaluation.CriteriaInfo;
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, INSERT_PROC, parameters );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    retVal = GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "Id", 0 );
                }
            }
            catch ( Exception ex )
            {
                LogError( className + ".Create(): " + ex.ToString() );
                status = className + ".Create(): " + ex.Message;
            }

            return retVal;
        }// Create

        public int CreateStandardEvaluation( ResourceEvaluation evaluation, ref string status )
        {
            status = "successful";
            int retVal = 0;

            try
            {
                #region parameters
                //will look up by resId and standard id, then do add
                SqlParameter[] parameters = new SqlParameter[ 4 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", evaluation.ResourceIntId );
                parameters[ 1 ] = new SqlParameter( "@StandardId", evaluation.StandardId );
                parameters[ 2 ] = new SqlParameter( "@CreatedById", evaluation.CreatedById );
                parameters[ 3 ] = new SqlParameter( "@Value", evaluation.Value );

                #endregion

                DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, INSERT_PROC, parameters );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    retVal = GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "Id", 0 );
                }
            }
            catch ( Exception ex )
            {
                LogError( className + ".CreateStandardEvaluation(): " + ex.ToString() );
                status = className + ".CreateStandardEvaluation(): " + ex.Message;
            }

            return retVal;
        }// Create

        /// <summary>
        /// Retrieves evaluation row by ID
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [Obsolete]
        private ResourceEvaluation Get( int pId, ref string status )
        {
            status = "successful";
            ResourceEvaluation evaluation = new ResourceEvaluation();
            evaluation.IsValid = false;

            try
            {
                #region parameters
                SqlParameter[] parameters = new SqlParameter[ 1 ];
                parameters[ 0 ] = new SqlParameter( "@Id", pId );
                #endregion

                DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, GET_PROC, parameters );
                if ( DoesDataSetHaveRows( ds ) )
                {
                    evaluation = Fill( ds.Tables[ 0 ].Rows[ 0 ] );
                }
            }
            catch ( Exception ex )
            {
                LogError( className + ".Get(): " + ex.ToString() );
                status = className + ".Get(): " + ex.Message;
            }

            return evaluation;
        }// Get

        /// <summary>
        /// Create ResourceEvaluation from DataRow
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        private ResourceEvaluation Fill( DataRow dr )
        {
            ResourceEvaluation eval = new ResourceEvaluation();
            eval.Id = GetRowColumn( dr, "Id", 0 );
            eval.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            eval.Created = GetRowColumn( dr, "Created", DateTime.MinValue );
            // Cannot use standard GetRowColumn for StandardId and RubricId because one of these will always be null
            eval.StandardId = GetRowPossibleColumn( dr, "StandardId", 0 );
            eval.RubricId = GetRowPossibleColumn( dr, "RubricId", 0 );
            try
            {
                eval.StandardId = int.Parse( dr[ "StandardId" ].ToString() );
            }
            catch ( NullReferenceException nex )
            {
                eval.StandardId = 0;
            }
            catch ( Exception ex )
            {
                //LogError( className + ".Fill(): " + ex.ToString() );
            }
            try
            {
                eval.RubricId = int.Parse( dr[ "RubricId" ].ToString() );
            }
            catch ( NullReferenceException nex )
            {
                eval.RubricId = 0;
            }
            catch ( Exception ex )
            {
                // LogError( className + ".Fill(): " + ex.ToString() );
            }
            eval.Value = GetRowColumn( dr, "Value", ( decimal )0.0 );
            eval.ScaleMin = GetRowColumn( dr, "ScaleMin", 0 );
            eval.ScaleMax = GetRowColumn( dr, "ScaleMax", 0 );
            eval.CriteriaInfo = GetRowColumn( dr, "CriteriaInfo", "" );

            return eval;
        }

        /// <summary>
        /// Select Evaluations
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="createdById"></param>
        /// <param name="standardId"></param>
        /// <param name="rubricId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public DataSet Select( int resourceIntId, int createdById, int standardId, int rubricId, ref string status )
        {
            DataSet ds = new DataSet();
            status = "successful";

            try
            {
                #region parameters
                SqlParameter[] parameters = new SqlParameter[ 4 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                parameters[ 1 ] = new SqlParameter( "@CreatedById", createdById );
                parameters[ 2 ] = new SqlParameter( "@StandardId", standardId );
                parameters[ 3 ] = new SqlParameter( "@RubricId", rubricId );
                #endregion

                ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, parameters );
                if ( !DoesDataSetHaveRows( ds ) )
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                status = className + ".Select(): " + ex.Message;
                LogError( className + ".Select(): " + ex.ToString() );
            }

            return ds;
        }
        /// <summary>
        /// Select Resource Standard Evaluations
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="createdById"></param>
        /// <param name="standardId"></param>
        /// <param name="rubricId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public DataSet Resource_StandardEvaluations_Select( int resourceIntId, int createdById, int standardId, ref string status )
        {
            DataSet ds = new DataSet();
            status = "successful";

            try
            {
                #region parameters
                SqlParameter[] parameters = new SqlParameter[ 4 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", resourceIntId );
                parameters[ 1 ] = new SqlParameter( "@CreatedById", createdById );
                parameters[ 2 ] = new SqlParameter( "@StandardId", standardId );
                parameters[ 3 ] = new SqlParameter( "@RubricId", 0 );
                #endregion

                ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, SELECT_PROC, parameters );
                if ( !DoesDataSetHaveRows( ds ) )
                {
                    return null;
                }
            }
            catch ( Exception ex )
            {
                status = className + ".Resource_StandardEvaluations_Select(): " + ex.Message;
                LogError( className + ".Resource_StandardEvaluations_Select(): " + ex.ToString() );
            }

            return ds;
        }
        /// <summary>
        /// Factory Method style way of getting all of the Rubrics and Standards ratings info for a given Resource, including user info if available
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ResourceRatings GetRatingsForResource( int resourceIntId, int userId )
        {
            ResourceRatings ratings = new ResourceRatings();

            ResourceStandardCollection resourceStandardInfo = new ResourceStandardManager().Select( resourceIntId );
            //Insert the standards, so that they show up even if they don't have ratings.
            foreach ( ResourceStandard item in resourceStandardInfo )
            {
                var standard = new ResourceRating();
                standard.code = item.StandardNotationCode;
                standard.description = item.StandardDescription;
                standard.alignmentType = ( item.AlignmentTypeValue == "" ? "Aligns to" : item.AlignmentTypeValue );
                standard.alignmentTypeID = item.AlignmentTypeCodeId;
                standard.id = item.StandardId;
                ratings.standardRatings.Add( standard );
            }

            //Need a proc for this. Will come in a later iteration
            var sql = "SELECT * FROM [Resource.Evaluation] WHERE [ResourceIntId] = " + resourceIntId;
            DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.Text, sql );
            var rubricInfoSql = "SELECT [Id], [Notation], [Description] FROM [Rubric.Node] WHERE ParentId IN (SELECT Id FROM [Rubric.Node] WHERE ParentId IS NULL)";
            DataSet rubricInfo = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.Text, rubricInfoSql );

            if ( !DoesDataSetHaveRows( ds ) ) { return ratings; }

            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
                //Determine whether it is a standard rating or a dimension rating
                var standardID = GetRowInt( dr, "StandardId" );
                var rubricID = GetRowInt( dr, "RubricId" );

                //If it is a rubric...
                if ( standardID == 0 )
                {
                    bool needNewInfo = false;
                    UpdateRating( ratings.rubricRatings, dr, "RubricId", userId, ref needNewInfo );
                    if ( needNewInfo )
                    {
                        //There doesn't seem to be any way to retrieve this information from the database--we have a lot of empty tables that look like they were supposed to contain this info.  Most importantly, there doesn't seem to be a way to determine which of the rubrics the dimension belongs to(!).
                        //ratings.rubricRatings.Last().code = "";
                        //ratings.rubricRatings.Last().description = "Dimension " + ratings.rubricRatings.Last().id;
                        foreach ( DataRow rubricRow in rubricInfo.Tables[ 0 ].Rows )
                        {
                            if ( GetRowInt( rubricRow, "Id" ) == ratings.rubricRatings.Last().id )
                            {
                                ratings.rubricRatings.Last().code = GetRowColumn( rubricRow, "Notation" );
                                ratings.rubricRatings.Last().description = GetRowColumn( rubricRow, "Description" );
                            }
                        }
                    }

                }
                //If it is a standard...
                else if ( rubricID == 0 )
                {
                    bool needNewInfo = false;
                    UpdateRating( ratings.standardRatings, dr, "StandardId", userId, ref needNewInfo );
                    if ( needNewInfo )
                    {
                        //Get the metadata information for this standard...shouldn't ever need this!
                        foreach ( ResourceStandard item in resourceStandardInfo )
                        {
                            foreach ( ResourceRating thisRating in ratings.standardRatings )
                            {
                                if ( thisRating.id == item.StandardId )
                                {
                                    thisRating.code = item.StandardNotationCode;
                                    thisRating.description = item.StandardDescription;
                                    thisRating.alignmentType = ( item.AlignmentTypeValue == "" ? "Aligns to" : item.AlignmentTypeValue );
                                    thisRating.alignmentTypeID = item.AlignmentTypeCodeId;
                                }
                            }
                        }
                    }
                }
            }

            //Finishing touches
            FinishRatingScores( ratings.rubricRatings );
            FinishRatingScores( ratings.standardRatings );

            return ratings;
        }
        #region Helper Methods for GetRatingsForResource
        protected void FinishRatingScores( List<ResourceRating> list )
        {
            foreach ( ResourceRating item in list )
            {
                if ( item.ratingCount == 0 ) { item.communityRating = 0.0; }
                else
                {
                    item.communityRating = ( item.communityRating / item.ratingCount );
                }
            }
        }
        protected void UpdateRating( List<ResourceRating> list, DataRow dr, string ID, int userID, ref bool needNewInfo )
        {
            bool accountedFor = false;
            foreach ( ResourceRating item in list )
            {
                if ( item.id == GetRowInt( dr, ID ) )
                {
                    AppendScore( dr, item, userID );
                    accountedFor = true;
                }
            }
            if ( !accountedFor )
            {
                ResourceRating item = new ResourceRating();
                item.id = GetRowInt( dr, ID );
                AppendScore( dr, item, userID );
                list.Add( item );
                needNewInfo = true;
            }
        }
        protected void AppendScore( DataRow dr, ResourceRating item, int userID )
        {
            item.communityRating += getNormalizedRating( dr );
            if ( userID == GetRowInt( dr, "CreatedById" ) )
            {
                item.myRating = getNormalizedRating( dr );
            }
            item.ratingCount++;
        }
        protected int getNormalizedRating( DataRow input )
        {
            int score = GetRowInt( input, "Value" ) - 1;
            return ( score > 3 ? 3 : score < 0 ? 0 : score );
        }
        protected int GetRowInt( DataRow dr, string column )
        {
            return GetID( DatabaseManager.GetRowPossibleColumn( dr, column ) );
        }
        protected int GetID( string input )
        {
            try
            {
                return int.Parse( input );
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        public List<ResourceRating> RateStandard( int userID, int standardID, int score, int intID )
        {
            var evaluation = new ResourceEvaluation();
            string status = "";
            evaluation.CreatedById = userID;
            evaluation.StandardId = standardID;
            evaluation.ScaleMax = 4;
            evaluation.ScaleMin = 1;
            evaluation.Value = score;
            evaluation.ResourceIntId = intID;

            if ( HasUserRatedThisStandard( standardID, userID, intID ) == true ) 
                { return null; } //No double-dipping
            this.Create( evaluation, ref status );

            var ratings = GetRatingsForResource( intID, userID ); //Have to get everything to ensure proper recalculation
            return ratings.standardRatings;
        }

        public bool HasUserRatedThisStandard( int standardID, int userID, int intID )
        {
            //Need a proc for this--Resource.EvaluationSelect allows for nulls. For this method to work properly, however, all three must match
            DataSet ds = DatabaseManager.DoQuery( "SELECT * FROM [Resource.Evaluation] WHERE [StandardId] = " + standardID + " AND [CreatedById] = " + userID + " AND [ResourceIntId] = " + intID );

            return DoesDataSetHaveRows( ds );
        }

        public double GetResourceEvaluationScore( int intID, ref int count )
        {
          double totalScore = 0.0;
          string status = "";
          double rubricScore = 0.0;
          double standardsScore = 0.0;
          int rubricCounter = 0;
          int standardsCounter = 0;
          double overallScore = 0.0;

          //ResourceEvaluationManager rManager = new ResourceEvaluationManager();
          DataSet ds = Select( intID, 0, 0, 0, ref status );
          if ( DoesDataSetHaveRows( ds ) )
          {
            //Tally up the raw info
            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
              double value = 0.0;
              if ( GetRowPossibleColumn( dr, "Value" ) == null || GetRowPossibleColumn( dr, "Value" ).ToLower() == "null" )
              {
                value = 0.0;
              }
              else
              {
                try
                {
                  value = double.Parse( GetRowPossibleColumn( dr, "Value" ) );
                  if ( value > 3.0 )
                  {
                    value = 3.0;
                  }
                  //Compensate for the +1 that happens when a score is inserted, which compensates for the nullifying of scores of 0
                  value = value - 1;
                }
                catch ( Exception ex )
                {
                  //LoggingHelper.DoTrace( ex.ToString() + " Value: " + GetRowPossibleColumn( dr, "Value" ) + " EndValue" );
                }
              }
              if ( GetRowPossibleColumn( dr, "RubricId" ) != "" )
              {
                rubricScore = rubricScore + value;
                rubricCounter++;
              }
              if ( GetRowPossibleColumn( dr, "StandardId" ) != "" )
              {
                standardsScore = standardsScore + value;
                standardsCounter++;
              }
            }

            //Do the averaging
            if ( rubricCounter > 0 )
            {
              rubricScore = rubricScore / rubricCounter;
            }
            if ( standardsCounter > 0 )
            {
              standardsScore = standardsScore / standardsCounter;
            }

            count = rubricCounter;

            //Calculate the final score
            overallScore = rubricScore + standardsScore / 2;

            return overallScore;
          }

          return totalScore;
        }

        public Dictionary<int, double> GetEvaluations( int intID )
        {
          int count = 0;
          Dictionary<int, double> items = new Dictionary<int, double>();
          DataSet ds = DatabaseManager.DoQuery( "SELECT DISTINCT [ResourceIntId] FROM [Resource.Evaluation]" + ( intID == 0 ? "" : " WHERE [ResourceIntId] = " + intID ) );
          if ( DoesDataSetHaveRows( ds ) )
          {
            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
              int resourceIntID = int.Parse( GetRowColumn( dr, "ResourceIntId" ) );
              double totalScore = GetResourceEvaluationScore( resourceIntID, ref count );
              items.Add( resourceIntID, totalScore );
            }
          }

          return items;

        }

        public List<EvaluationResult> GetEvaluationsWithCount( int intID )
        {
          var output = new List<EvaluationResult>();
          DataSet ds = DatabaseManager.DoQuery( "SELECT DISTINCT [ResourceIntId] FROM [Resource.Evaluation]" + ( intID == 0 ? "" : " WHERE [ResourceIntId] = " + intID ) );
          if ( DoesDataSetHaveRows( ds ) )
          {
            foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
            {
              int count = 0;
              int resourceIntID = int.Parse( GetRowColumn( dr, "ResourceIntId" ) );
              double totalScore = GetResourceEvaluationScore( resourceIntID, ref count );
              output.Add( new EvaluationResult() { count = count, intID = resourceIntID, score = totalScore } );
            }
          }

          return output;
        }
        
        public class EvaluationResult
        {
          public int intID { get; set; }
          public double score { get; set; }
          public int count { get; set; }
        }

    }
}
