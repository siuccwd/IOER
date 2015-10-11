using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using Microsoft.ApplicationBlocks.Data;

using LinkChecker2.App_Code.BusObj;
using Elasticsearch;
using Elasticsearch.Net;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.ConnectionPool;

namespace LinkChecker2.App_Code.DAL
{
/*    public class ElasticSearchManager : BaseDataManager
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        ResourceJSONManager jsonManager = new ResourceJSONManager();
        string className = "ElasticSearchManager";
        string IoerReadOnlyConnString = "";
        ElasticsearchClient client;
        string type = "resource";

        public ElasticSearchManager()
        {
            IoerReadOnlyConnString = ConfigurationManager.ConnectionStrings["IoerReadOnlyConnString"].ConnectionString;
            var node = new Uri(GetAppKeyValue("elasticSearchUrl"));
            var connectionPool = new SniffingConnectionPool(new[] { node });
            var config = new ConnectionConfiguration(connectionPool).MaximumRetries(10).DisablePing(true);
            client = new ElasticsearchClient(config);

        }

        public void DeleteByIntID(int id, ref string status)
        {
            DeleteResource(id);
        }

        public void DeleteByIntID(List<int> ids, ref string status)
        {
            DeleteResources(ids);
        }

        public void DeleteResource(int id)
        {
            DeleteResources(new List<int> { id });
        }
        public void DeleteResources(List<int> ids)
        {

            //Collection 7
            var bulkC7 = new List<object>();
            foreach (var item in ids)
            {
                bulkC7.Add(new { delete = new { _index = "collection7", _type = type, _id = item } });
            }
            client.Bulk(bulkC7);
        }

        public DataSet GetSqlDataForElasticSearch(int resourceIntId, ref string status)
        {
            return GetSqlDataForElasticSearch(resourceIntId.ToString(), ref status);
        }

        public DataSet GetSqlDataForElasticSearch(string resourceIntIdList, ref string status)
        {
            status = "successful";
            #region SqlParameters
            SqlParameter[] sqlParameter = new SqlParameter[1];
            sqlParameter[0] = new SqlParameter("@ResourceIntId", resourceIntIdList);
            #endregion

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(IoerReadOnlyConnString, CommandType.StoredProcedure, "Resource_BuildElasticSearch", sqlParameter);
                if (DoesDataSetHaveRows(ds))
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetSqlDataForElasticSearch(): " + ex.ToString());
                status = className + ".GetSqlDataForElasticSearch(): " + ex.Message;

                return null;
            }

        }

        public void BulkUpload(DataSet resourceList)
        {
            List<ResourceJSONFlat> flats = new List<ResourceJSONFlat>();
            StringBuilder builder = new StringBuilder();
            string newline = Environment.NewLine;
            //string toWrite = "";
            int count = 0;
            foreach (DataRow dr in resourceList.Tables[0].Rows)
            {
                if (count <= 1000)
                {
                    ResourceJSONFlat flat = jsonManager.GetJSONFlatFromDataRow(dr);
                    string header = "{ \"index\": { \"_index\": \"collection5\", \"_type\": \"resource\", \"_id\": \"" + flat.versionID + "\" } }";
                    string jsonData = serializer.Serialize(flat);
                    builder.Append(header + newline + jsonData + newline);
                    //toWrite = toWrite + header + Environment.NewLine + jsonData + Environment.NewLine;
                    count++;
                }
                if (count == 1000)
                {
                    int tries = 0;
                    while (tries < 5)
                    {
                        try
                        {
                            //ContactServer( "POST", toWrite, "_bulk" );
                            ContactServer("POST", builder.ToString(), "_bulk");
                            tries = 5;
                        }
                        catch
                        {
                            tries++;
                        }
                    }
                    //toWrite = "";
                    builder.Clear();
                    count = 0;
                    Thread.Sleep(8000);
                }
            }
            if (count > 0)
            {
                //ContactServer( "POST", toWrite, "_bulk" );
                ContactServer("POST", builder.ToString(), "_bulk");
            }

        }

        protected string ContactServer(string method, string json, string urlAddendum)
        {
            int maxAttempts = 4;
            bool success = false;
            int attemptNbr = 0;
            while (attemptNbr < maxAttempts && !success)
            {
                try
                {
                    string collection = GetAppKeyValue("elasticSearchCollection");
                    string wsUrl = GetAppKeyValue("elasticSearchUrl", "http://192.168.1.17:9200/") + collection + urlAddendum;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(wsUrl);
                    request.ContentType = "application/json; charset=utf-8";
                    request.Method = method;

                    byte[] byteData = Encoding.UTF8.GetBytes(json);
                    request.ContentLength = byteData.Length;
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(byteData, 0, byteData.Length);
                    request.Timeout = 15000;
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    success = true;
                    //Console.WriteLine("Server contact successful");

                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    return reader.ReadToEnd();
                }
                catch (TimeoutException tex)
                {
                    LogError("Timeout Encountered at " + DateTime.Now + ": " + tex.ToString());
                    return "Timeout Error: " + tex.ToString();
                }
                catch (Exception ex)
                {
                    LogError("ElasticSearch Exception encountered: " + ex.ToString());
                    attemptNbr++;
                    return "General Error: " + ex.ToString();
                }
            }
            return null;
        }

    } */
}
