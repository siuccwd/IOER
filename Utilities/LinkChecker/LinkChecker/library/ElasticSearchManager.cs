using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace LinkChecker.library
{
    public class ElasticSearchManager : BaseDataManager
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();

        public void DeleteByIntID(int resourceIntID, ref string response)
        {
            DeleteByIntID(resourceIntID.ToString(), ref response);
        }
        public void DeleteByIntID(string resourceIntID, ref string response)
        {
            //Setup the JSON
            dynamic deleter = new Dictionary<string, object>();
            dynamic term = new Dictionary<string, object>();
            term.Add("intID", resourceIntID);
            deleter.Add("term", term);

            //Do the delete
            ContactServer("DELETE", serializer.Serialize(deleter), "resource/_query");
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

    }
}
