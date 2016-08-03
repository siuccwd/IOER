﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LRWarehouse.Business;
using System.Web.Script.Serialization;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using Elasticsearch;
using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.ConnectionPool;

namespace LRWarehouse.DAL
{
  public class ElasticSearchManager : BaseDataManager
  {
    #region Class stuff
    //Global stuff
    ElasticsearchClient client;
    ResourceJSONManager jsonManager;
    JavaScriptSerializer serializer;
    ResourceEvaluationManager evaluationManager;
    string status = "";
    string type = "resource";
	string esCollection = "mainSearchCollection";
    //Setup class
    public ElasticSearchManager() {
      var node = new Uri( GetAppKeyValue( "elasticSearchUrl" ) );
	  esCollection = GetAppKeyValue( "elasticSearchCollection", "mainSearchCollection" );

      var connectionPool = new SniffingConnectionPool(new[] { node });
      var config = new ConnectionConfiguration( connectionPool ).MaximumRetries(10).DisablePing(true);
      client = new ElasticsearchClient( config );
      jsonManager = new ResourceJSONManager();
      serializer = new JavaScriptSerializer();
      evaluationManager = new ResourceEvaluationManager();
    }
    #endregion
    #region Major methods
    //Search
    public string Search( string queryJSON )
    {
      return Search( queryJSON, esCollection );
    }
    public string Search( string queryJSON, string indexName )
    {
      return Search( queryJSON, indexName, type );
    }
    public string Search( string queryJSON, string indexName, string type )
    {
      var response = client.Search<string>( indexName, type, queryJSON ).Response;
      return response;
    }

    //More Like This
    public string FindMoreLikeThis( int intID, string text, string[] fields, int minFieldMatches, int maxFieldMatches )
    {
			var parameters = new
			{
				query = new
				{
					more_like_this = new
					{
						fields = fields,
						like_text = text,
						min_term_freq = minFieldMatches <= 0 ? 1 : minFieldMatches,
						max_query_terms = maxFieldMatches < minFieldMatches ? minFieldMatches + 5 : maxFieldMatches,
					}
				}
			};
      //return Search( serializer.Serialize( parameters ) );
      return client.Mlt<string>( esCollection, type, intID.ToString(), parameters ).Response;
    }

	#region old obsolete methods
	//Create or replace record(s) in the index
	[Obsolete]
	private void RefreshResourceOld( int id )
    {
      RefreshResourcesOld( new List<int> { id } );
    }
	  [Obsolete]
    private void RefreshResourcesOld( List<int> ids )
    {
      //Get Evaluation Data
      var evaluations = evaluationManager.GetEvaluationsWithCount( 0 );
      //Set list of IDs
      var idList = serializer.Serialize(ids).Replace("[", "").Replace("]","");

      //Collection 5
      var dataC5 = GetSqlDataForElasticSearchCollection5( idList, ref status );
      CreateOrReplaceResourcesInCollection5( dataC5, evaluations );

      //Collection 6
      var dataC6 = GetSqlDataForElasticSearchCollection6( idList, ref status );
      CreateOrReplaceResourcesInCollection6( dataC6, evaluations );

      //Collection 7
      //this method should only be called by the method in resourcev2services and thus collection7 should be handled by that
    }

    //Create or update from existing DataSets (useful for full index rebuilds)
	  [Obsolete]
	  private void CreateOrReplaceResourcesInCollection5( DataSet dataC5, List<ResourceEvaluationManager.EvaluationResult> evaluations )
    {
      //Return if no data
      if ( !DoesDataSetHaveRows( dataC5 ) ) { return; }
      //Create a bulk list
      var bulkC5 = new List<object>();
      foreach ( DataRow row in dataC5.Tables[ 0 ].Rows )
      {
        AddBulkCollection5( row, evaluations, ref bulkC5 );
      }
      //Do the bulk upload
      client.Bulk( bulkC5 );
    }
	  [Obsolete]
	  public void AddBulkCollection5( DataRow row, List<ResourceEvaluationManager.EvaluationResult> evaluations, ref List<object> bulkC5 )
    {
      //Get a record
      var flat = jsonManager.GetJSONFlatFromDataRow( row );
      //Pre-inject any relevant evaluations
      var eval = evaluations.Where( i => i.intID == flat.intID ).FirstOrDefault();
      if ( eval != null )
      {
        flat.evaluationCount = eval.count;
        flat.evaluationScoreTotal = ( eval.score == -1.0 ? -1 : Convert.ToInt32( eval.score * 100 ) );
      }
      //Add the two rows to the bulk list
      bulkC5.Add( new { index = new { _index = "collection5", _type = type, _id = flat.intID } } );
      bulkC5.Add( flat );
    }
	  [Obsolete]
	  private void CreateOrReplaceResourcesInCollection6( DataSet dataC6, List<ResourceEvaluationManager.EvaluationResult> evaluations )
    {
      //Return if no data
      if ( !DoesDataSetHaveRows( dataC6 ) ) { return; }
      //Create a bulk list
      var bulkC6 = new List<object>();
      foreach ( DataRow row in dataC6.Tables[ 0 ].Rows )
      {
        AddBulkCollection6( row, evaluations, ref bulkC6 );
      }
      //Do the bulk upload
      client.Bulk( bulkC6 );
    }
	  [Obsolete]
	  public void AddBulkCollection6( DataRow row, List<ResourceEvaluationManager.EvaluationResult> evaluations, ref List<object> bulkC6 )
    {
      //Get a record
      var crs = jsonManager.GetCRSResourceFromDataRow( row, 0, -1 );
      //Pre-inject any relevant evaluations
      var eval = evaluations.Where( i => i.intID == crs.intID ).FirstOrDefault();
      if ( eval != null )
      {
        crs.paradata.evaluations.count = eval.count;
        crs.paradata.evaluations.score = eval.score;
      }
      //Add the two rows to the bulk list
      bulkC6.Add( new { index = new { _index = "collection6", _type = type, _id = crs.intID } } );
      bulkC6.Add( crs );
    }
	#endregion

	//Delete a list of records in the index
    public void DeleteResource( int id )
    {
      DeleteResources( new List<int> { id } );
    }
    public void DeleteResources( List<int> ids )
    {
      //Collection 7
      var bulkC7 = new List<object>();
      foreach ( var item in ids )
      {
        bulkC7.Add( new { delete = new { _index = esCollection, _type = type, _id = item.ToString() } } );
      }

			DoChunkedBulkUpload( bulkC7 );
    }

    //Delete Entire Index
    public void DeleteEntireIndex( string indexName )
    {
      client.IndicesDelete( indexName );
    }

    //Create Entire Index with mapping
    public void CreateIndexWithMapping( string indexName, string mappingJSON )
    {
      client.IndicesCreate( indexName, mappingJSON );
    }

    //Bulk upload data
    public string DoBulkUpload( List<object> preformattedBulkData )
    {
      var response = client.Bulk( preformattedBulkData ).ToString();
      return response;
    }

		//Handle chunked bulk uploads
		public void DoChunkedBulkUpload( List<object> preformattedBulkData )
		{
			DoChunkedBulkUpload( preformattedBulkData, 1000 );
		}
		public void DoChunkedBulkUpload( List<object> preformattedBulkData, int evenNumberedChunkSize )
		{
			var chunkSize = evenNumberedChunkSize; //Must be an even number because bulk items are processed in pairs (unless they are deletes)
			var counter = 0;
			var temp = new List<object>();
			var max = preformattedBulkData.Count();
			var lastItem = max - 1;

			//For each item in the list
			for ( var i = 0 ; i < max ; i++ )
			{
				//Add the item to the list and increment the counter
				temp.Add( preformattedBulkData[ i ] );
				counter++;

				//If the counter hits the chunksize or this is the last item in the list, do the upload, then sleep
				if ( counter >= chunkSize || i == lastItem )
				{
					client.Bulk( temp );
					temp.Clear();
					counter = 0;
					System.Threading.Thread.Sleep( chunkSize * 10 ); //Don't overload elasticsearch
				}

			}

		}
		//


		#endregion
		#region Helper methods
		//Get SQL data for elasticsearch records
		public DataSet GetSqlDataForElasticSearchCollection5( string idList, ref string status )
    {
      return GetDataSet( idList, "Resource_BuildElasticSearch", ref status );
    }
    public DataSet GetSqlDataForElasticSearchCollection6( string idList, ref string status )
    {
      return GetDataSet( idList, "Resource_BuildElasticSearchV2", ref status );
    }
    private DataSet GetDataSet( string idList, string proc, ref string status )
    {
      status = "successful";
      var parameters = new SqlParameter[] { new SqlParameter("@ResourceIntId", idList) };
      try
      {
        DataSet ds = SqlHelper.ExecuteDataset( ReadOnlyConnString, CommandType.StoredProcedure, proc, parameters );
        if ( DoesDataSetHaveRows( ds ) )
        {
          return ds;
        }
        return null;
      }
      catch ( Exception ex )
      {
        LogError( "ElasticSearchManager." + proc + "(): " + ex.ToString() );
        status = "ElasticSearchManager." + proc + "(): " + ex.Message;

        return null;
      }
    }
    #endregion

    public class ResourceHeader
    {
      public ResourceHeader()
      {
        index = new ResourceHeaderData();
      }
      public ResourceHeaderData index { get; set; }
    }
    public class ResourceHeaderData
    {
      public string _index { get; set; }
      public string _type { get; set; }
      public string _id { get; set; }
    }
  }
}
