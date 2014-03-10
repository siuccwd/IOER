using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

using LRWarehouse.Business;

namespace LearningRegistryCache2.App_Code.DataManagers
{
    public class LearningRegistryManager : BaseDataManager
    {
        private string ConsumeServerConString;

        public LearningRegistryManager()
        {
            ConsumeServerConString = ConfigurationManager.AppSettings["learningRegistryConsume"].ToString();
        }

        /*public XmlDocument ListRecords(DateTime pFrom, DateTime pTo, string pMetadataPrefix)
        {
          int passNo = 0;
          XmlDocument internalList = new XmlDocument();
          XmlDocument returnList = new XmlDocument();
          returnList.LoadXml("<root></root>");
          bool endOfList = false;
                string returnToken = "";
          try
          {
                    internalList = ListRecords( pFrom, pTo, pMetadataPrefix, ref returnToken );
            do
            {
              XmlNodeList documents = internalList.GetElementsByTagName("oai:metadata");
              foreach (XmlNode document in documents)
              {
                XmlNode node = returnList.ImportNode(document, true);
                returnList.DocumentElement.AppendChild(node);
              }
              XmlNodeList resumeToken = internalList.GetElementsByTagName("oai:resumptionToken");
              if (resumeToken == null || resumeToken.Count == 0)
              {
                endOfList = true;
              }
              else
              {
                //Thread.Sleep(5000); // Play "nice"
                            //Error	11	A property or indexer may not be passed as an out or ref parameter	C:\Inetpub\wwwroot\VOS_2005\LearningRegistryCache2\LearningRegistryCache2\App_Code\DataManagers\LearningRegistryManager.cs	49	73	LearningRegistryCache2

                            returnToken = resumeToken[ 0 ].InnerText;
                            internalList = ListRecords( pFrom, pTo, pMetadataPrefix, ref returnToken );
              }
              passNo++;
              Console.WriteLine("\r\n Pass #{0}", passNo);
            } while (!endOfList);
          }
          catch (Exception ex)
          {
                    LogError( "LearningRegistryManager.ListRecords(): " + ex.ToString() );
            Console.Write("ListRecords1(): ");
            throw;
          }

          return returnList;
        } */

        public XmlDocument BasicListRecords(string pFrom, string pTo, ref string resumeToken)
        {
            int nbrRetry = 4;
            int nbrSerializationFail = 10;
            int retryCount = 0;
            bool stopTrying = false;
            XmlDocument document = new XmlDocument();
            string jsonString = string.Empty;
            Regex fixIdentifierEx = new Regex("\"@[A-Za-z0-9]");
            while (!stopTrying)
            {
                try
                {
                    string url = ConsumeServerConString + "/harvest/listrecords";
                    if (pFrom.CompareTo("2000-01-01") > 0)
                    {
                        if (url.IndexOf("?") == -1)
                        {
                            url += string.Format("?from={0}", pFrom);
                        }
                        else
                        {
                            url += string.Format("&from={0}", pFrom);
                        }
                    }
                    if (pTo.CompareTo("2000-01-01") > 0)
                    {
                        if (url.IndexOf("?") == -1)
                        {
                            url += string.Format("?until={0}", pTo);
                        }
                        else
                        {
                            url += string.Format("&until={0}", pTo);
                        }
                    }
                    if (resumeToken != "")
                    {
                        if (url.IndexOf("?") == -1)
                        {
                            url += string.Format("?resumption_token={0}", resumeToken);
                        }
                        else
                        {
                            url += string.Format("&resumption_token={0}", resumeToken);
                        }
                    }
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = 20000; // in milliseconds
                    request.ReadWriteTimeout = 20000; // in milliseconds
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    jsonString = sr.ReadToEnd().Trim();
                    MatchCollection matches = fixIdentifierEx.Matches(jsonString);
                    foreach (Match match in matches)
                    {
                        string valueToReplace = match.Value;
                        string replacementValue = match.Value.Replace("@", "_");
                        jsonString = jsonString.Replace(valueToReplace, replacementValue);
                    }
                    sr.Close();
                    if (jsonString.IndexOf("<h1>Server Error</h1>") == -1)
                    {
                        document = JsonConvert.DeserializeXmlNode(jsonString, "root");
/*                        Dictionary<string, object> obj = jsonSerializer.DeserializeObject(jsonString);
                        SerializableDictionary<string, object> obj2 = obj.ToDictionary(p => p.Key, p => (object)p.Value);
                        StringWriter sw = new StringWriter();
                        XmlWriter xw = XmlWriter.Create(sw);
                        xmlSerializer.Serialize(xw, obj);
                        var xml = xw.ToString();
                        document.LoadXml(xml); */
                        stopTrying = true;
                    }
                    else
                    {
                        stopTrying = true;
                    }
                }
                catch (WebException wex)
                {
                    if (wex.Message == "The operation has timed out." || wex.Message == "The operation has timed out")
                    {
                        retryCount++;
                        if (retryCount >= nbrRetry)
                        {
                            Console.WriteLine("Timed out.  Giving up.");
                            stopTrying = true;
                        }
                        else
                        {
                            Console.WriteLine("Timed out.  Retrying...");
                        }
                    }
                }
                catch (IOException iex)
                {
                    if (iex.Message == "Received an unexpected EOF or 0 bytes from the transport stream.")
                    {
                        retryCount++;
                        if (retryCount >= nbrRetry)
                        {
                            stopTrying = true;
                            Console.WriteLine("EOF or 0 bytes encountered.  Giving up.");
                        }
                        else
                        {
                            Console.WriteLine("EOF or 0 bytes encountered.  Retrying...");
                        }
                    }
                }
                catch (JsonSerializationException jex)
                {
                    if (jsonString.IndexOf("resumption_token") > -1)
                    {
                        SerializeIndividually(document, jsonString);
                        stopTrying = true;
                    }
                    else
                    {
                        retryCount++;
                        LRWarehouse.DAL.AuditReportingManager arm = new LRWarehouse.DAL.AuditReportingManager();
                        if (retryCount >= nbrSerializationFail)
                        {
                            stopTrying = true;
                            Console.WriteLine(string.Format("Serialization failed {0}/{1} times.  Giving up.", retryCount, nbrSerializationFail));
                            arm.LogMessage(LearningRegistry.reportId, "", "", "", LRWarehouse.Business.ErrorType.Error, LRWarehouse.Business.ErrorRouting.Technical,
                                string.Format("Serialization failed {0}/{1} times.  Giving up.", retryCount, nbrSerializationFail));
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Serialization failed {0}/{1} times.  Waiting 10 seconds and trying again.", retryCount, nbrSerializationFail));
                            arm.LogMessage(LearningRegistry.reportId, "", "", "", LRWarehouse.Business.ErrorType.Warning, LRWarehouse.Business.ErrorRouting.Technical,
        string.Format("Serialization failed {0}/{1} times.  Waiting 10 seconds and trying again.", retryCount, nbrSerializationFail));
                            Thread.Sleep(10000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "LearningRegistryManager.BasicListRecords(): ");
                    Console.Write("ListRecords1(): " + ex.ToString());
                    throw;
                }
            }
            XmlNodeList resumptionTokenList = document.GetElementsByTagName("resumption_token");
            if (resumptionTokenList == null || resumptionTokenList.Count == 0)
            {
                resumeToken = "";
            }
            else
            {
                resumeToken = resumptionTokenList[0].InnerText;
            }
            return document;
        }

        public void SerializeIndividually(XmlDocument document, string inputJsonString)
        {
            LRWarehouse.DAL.AuditReportingManager arm = new LRWarehouse.DAL.AuditReportingManager();
            document.LoadXml("<root></root>");
            Regex identifierEx = new Regex("\"identifier\":\\s?\"(.*?)\"");
            Regex resumptionEx = new Regex("\"resumption_token\":\\s?\"(.*?)\"");
            Regex docIdEx = new Regex("\"(.*?)\"");
            string docId = string.Empty;
            MatchCollection matches = identifierEx.Matches(inputJsonString);
            foreach (Match match in matches)
            {
                try
                {
                    XmlDocument innerDoc = new XmlDocument();
                    string[] mStr = match.Groups[0].Value.Split(':');
                    Match innerMatch = docIdEx.Match(mStr[1]);
                    docId = innerMatch.Groups[0].Value.Replace("\"", "");
                    string url = string.Format(ConsumeServerConString + "/harvest/getrecord?request_id={0}&by_doc_ID=true", docId);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = 20000; // in milliseconds
                    request.ReadWriteTimeout = 20000; // in milliseconds
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string jsonString = sr.ReadToEnd().Trim();
                    sr.Close();
                    if (jsonString.IndexOf("<h1>Server Error</h1>") == -1)
                    {
                        innerDoc = JsonConvert.DeserializeXmlNode(jsonString, "root");
                        XmlNodeList records = innerDoc.GetElementsByTagName("record");
                        foreach (XmlNode record in records)
                        {
                            XmlNode node = document.ImportNode(record, true);
                            document.DocumentElement.AppendChild(node);
                        }
                    }
                }
                catch (Exception ex)
                {
                    arm.LogMessage(LearningRegistry.reportId, "", docId, "", LRWarehouse.Business.ErrorType.Error, LRWarehouse.Business.ErrorRouting.Technical,
                        "Could not serialize doc_ID: " + docId);
                }
            }
            Match resumptionMatch = resumptionEx.Match(inputJsonString);
            string[] rStr = resumptionMatch.Groups[0].Value.Split(':');
            Match rMatch = docIdEx.Match(rStr[1]);
            string xmlString = string.Format("<root><resumption_token>{0}</resumption_token></root>", rMatch.Groups[0].Value.Replace("\"", ""));
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlString);
            XmlNodeList rNodeList = xdoc.GetElementsByTagName("resumption_token");
            XmlNode rNode = document.ImportNode(rNodeList[0], true);
            document.DocumentElement.AppendChild(rNode);
        }


        public XmlDocument ListRecords(DateTime pFrom, DateTime pTo, string pMetadataPrefix, ref string resumeToken)
        {
            int nbrRetry = 4;
            int retryCount = 0;
            bool stopTrying = false;
            XmlDocument document = new XmlDocument();
            while (!stopTrying)
            {
                try
                {
                    string url = string.Format(ConsumeServerConString + "/OAI-PMH?verb=ListRecords&metadataPrefix={0}", pMetadataPrefix);
                    if (pFrom > DateTime.Parse("2000-01-01"))
                    {
                        url += string.Format("&from={0}", pFrom.Date.ToString("yyyy-MM-dd"));
                    }
                    if (pTo > DateTime.Parse("2000-01-01"))
                    {
                        url += string.Format("&until={0}", pTo.Date.ToString("yyyy-MM-dd"));
                    }
                    if (resumeToken != "")
                    {
                        url += string.Format("&resumptionToken={0}", resumeToken);
                    }
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Timeout = 20000; // in milliseconds
                    request.ReadWriteTimeout = 20000; // in milliseconds
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    StreamReader sr = new StreamReader(response.GetResponseStream());
                    string xmlString = sr.ReadToEnd().Trim();
                    sr.Close();
                    document.LoadXml(xmlString);
                    stopTrying = true; //success!!
                }
                catch (WebException wex)
                {
                    if (wex.Message == "The operation has timed out." || wex.Message == "The operation has timed out")
                    {
                        retryCount++;
                        if (retryCount >= nbrRetry)
                        {
                            Console.WriteLine("Timed out.  Giving up.");
                            stopTrying = true;
                        }
                        else
                        {
                            Console.WriteLine("\r\nTimed out.  Retrying...");
                        }
                    }
                }
                catch (IOException iex)
                {
                    if (iex.Message == "Received an unexpected EOF or 0 bytes from the transport stream.")
                    {
                        retryCount++;
                        if (retryCount >= nbrRetry)
                        {
                            stopTrying = true;
                            Console.WriteLine("EOF or 0 bytes encountered.  Giving up.");
                        }
                        else
                        {
                            Console.WriteLine("EOF or 0 bytes encountered.  Retrying...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "LearningRegistryManager.ListRecords(): ");
                    Console.Write("ListRecords1(): ");
                    throw;
                }
            }
            XmlNodeList resumptionTokenList = document.GetElementsByTagName("oai:resumptionToken");
            if (resumptionTokenList == null || resumptionTokenList.Count == 0)
            {
                resumeToken = "";
            }
            else
            {
                resumeToken = resumptionTokenList[0].InnerText;
            }
            return document;
        }
    }
}
