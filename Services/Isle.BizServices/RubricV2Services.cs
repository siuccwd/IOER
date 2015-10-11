using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.DAL;
using LRWarehouse.Business;

namespace Isle.BizServices
{
	public class RubricV2Services
	{

		//Get just the rubrics, no dimensions
		public List<RubricV2> GetRubricList()
		{
			return GetRubricList( 0 );
		}

		//Get one or more rubric top level data
		public List<RubricV2> GetRubricList( int rubricID )
		{
			var output = new List<RubricV2>();

			var data = DatabaseManager.DoQuery( "SELECT Id, Title, Description, Url FROM [Evaluation] WHERE IsActive = 1 " + ( rubricID > 0 ? " AND Id = " + rubricID : "" ) );
			foreach ( DataRow dr in data.Tables[0].Rows )
			{
				output.Add( new RubricV2()
				{
					RubricId = dr.Field<int>("Id"),
					Title = dr.Field<string>("Title"),
					Description = dr.Field<string>("Description"),
					RubricUrl = dr.Field<string>("Url")
				} );
			}

			return output;
		}

		//Get one or more rubric top level data and indicate whether or not the user already used a specific rubric with a specific resource
		public List<RubricV2> GetRubricListWithEvaluationDataForResource( int rubricID, int userID, int resourceID )
		{
			var output = GetRubricList( rubricID );
			var data = DatabaseManager.DoQuery( "SELECT EvaluationId, Score FROM [Resource.Evaluation] WHERE ResourceIntId = " + resourceID + ( userID > 0 ? " AND CreatedById = " + userID : "" ) );
			foreach ( DataRow dr in data.Tables[ 0 ].Rows )
			{
				var target = output.Where( t => t.RubricId == dr.Field<int>( "EvaluationId" ) ).FirstOrDefault();
				if ( target != null )
				{
					target.HasUserRating = true;
					target.OverallScore = ( double ) dr.Field<int>( "Score" );
				}
			}
			return output;
		}

		//Get a particular Rubric
		public RubricV2 GetRubric( int rubricID )
		{
			var rubric = GetRubricList( rubricID ).First();

			rubric.Dimensions = GetDimensionsForRubric( rubricID );

			return rubric;
		}

		//Get a particular Rubric, with evaluation data for a user and a resource
		public RubricV2 GetRubricWithEvaluationDataForResource( int rubricID, int userID, int resourceID )
		{
			var rubric = GetRubricListWithEvaluationDataForResource( rubricID, userID, resourceID ).First();

			rubric.Dimensions = GetDimensionsForRubric( rubricID );

			return rubric;
		}

		//Get the dimensions for a rubric
		public List<DimensionV2> GetDimensionsForRubric( int rubricID )
		{
			var dimensions = new List<DimensionV2>();

			var data = DatabaseManager.DoQuery( "SELECT Id, Title, Description FROM [Evaluation.Dimension] WHERE IsActive = 1 AND EvaluationId = " + rubricID );
			foreach ( DataRow dr in data.Tables[ 0 ].Rows )
			{
				dimensions.Add( new DimensionV2()
				{
					DimensionId = dr.Field<int>( "Id" ),
					Title = dr.Field<string>( "Title" ),
					Description = dr.Field<string>( "Description" )
				} );
			}

			return dimensions;
		}

		//Issue a rating - expects score to already be set to 0-100 scale
		public void IssueRating( RubricV2 rubric, int resourceID, int userID )
		{
			//If the user already has rated this resource, replace the old rating
			var oldEval = GetRubricWithEvaluationDataForResource( rubric.RubricId, userID, resourceID );
			if ( oldEval.HasUserRating )
			{
				DatabaseManager.ExecuteSql( "DELETE FROM [Resource.Evaluation] WHERE ResourceIntId = " + resourceID + " AND CreatedById = " + userID + " AND EvaluationId = " + rubric.RubricId );
			}

			//Issue new score
			var intScore = ( int ) rubric.OverallScore;

			//Setup dimension DTOs
			var dims = new List<Isle.DTO.ResourceEvaluationDimension>();
			foreach ( var item in rubric.Dimensions )
			{
				dims.Add( new Isle.DTO.ResourceEvaluationDimension()
				{
					Id = item.DimensionId,
					CreatedById = userID,
					EvalDimensionId = item.DimensionId,
					Score = (int) item.ScorePercent,
				} );
			}

			var dto = new Isle.DTO.ResourceEvaluationDTO() {
				CreatedById = userID,
				Score = intScore,
				EvaluationId = rubric.RubricId,
				ResourceIntId = resourceID,
				Dimensions = dims,
			};

			var status = "";

			ResourceBizService.Evaluations_SaveEvaluation( dto, ref status );
		}

	}
}
