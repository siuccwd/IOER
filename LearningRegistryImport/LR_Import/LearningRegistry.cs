using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

//using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LRWarehouse.Business;
using LRWarehouse.DAL;


namespace LearningRegistryCache2
{
    public class LearningRegistry : BaseDataController
    {
        private ResourceManager resourceManager = new ResourceManager();
        private OLDDM.SubmitterManager submitterManager = new OLDDM.SubmitterManager();
        //TODO - use resource property
        private ResourcePropertyManager resourcePropManager = new ResourcePropertyManager();
        private AuditReportingManager auditReportingManager = new AuditReportingManager();
        //private NameValueManager nameValueManager = new NameValueManager();
        private NsdlHandler nsdlHandler = new NsdlHandler();
        private LomHandler lomHandler = new LomHandler();
        private CommParaHandler commParaHandler = new CommParaHandler();
        private LRParadataHandler lrParaHandler = new LRParadataHandler();
        private LrmiHandler lrmiHandler = new LrmiHandler();
        

        private OLDDM.LearningRegistryManager registryManager = new OLDDM.LearningRegistryManager();
        private int maxRecords = 10;
        int recordCntr = 0;
        private string ourSignerTag = "";

        public static int reportId = 0;
        public static string fileName = "";
        public static string resourceIdList = "";
        public static string EmailBody = "";
        public static int lrRecordsRead = 0;
        public static int lrRecordsProcessed = 0;
        public static int lrRecordsSpam = 0;
        public static int lrRecordsUnknownSchema = 0;
        public static int lrRecordsEmptySchema = 0;
        public static int lrRecordsBadDataType = 0;
        public static int lrRecordsBadPayloadPlacement = 0;

        public LearningRegistry()
        {
            ourSignerTag = ConfigurationManager.AppSettings["ourSigner"].ToString();
        }

        public void ExtractData(string startDate, string endDate, bool isBatch)
        {
            int maxRecordsInFile=int.Parse(ConfigurationManager.AppSettings["maxRecordsInFile"].ToString());
            reportId = auditReportingManager.CreateReport();

            XmlDocument newDoc = new XmlDocument();
            newDoc.LoadXml("<root></root>");
            int passNbr = 1;
            string resumeToken = "";
            string lrStartDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(startDate)).ToString("yyyy-MM-dd HH:mm:ss").Replace(" ","T")+"Z";
            string lrEndDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(endDate)).ToString("yyyy-MM-dd HH:mm:ss").Replace(" ","T")+"Z";
            do
            {
                XmlDocument document = registryManager.BasicListRecords(lrStartDate, lrEndDate, ref resumeToken);
                XmlNodeList records = document.GetElementsByTagName("record");
                foreach (XmlNode record in records)
                {
                    XmlNode node = newDoc.ImportNode(record, true);
                    newDoc.DocumentElement.AppendChild(node);
                    lrRecordsRead++;
                }

                if (newDoc.DocumentElement.ChildNodes.Count >= maxRecordsInFile)
                {
                    SaveDoc(newDoc, DateTime.Parse(endDate), isBatch);
                    newDoc = new XmlDocument();
                    newDoc.LoadXml("<root></root>");
                }
            } while (resumeToken != null && (resumeToken != "" && resumeToken != "null"));
            SaveDoc(newDoc, DateTime.Parse(endDate), isBatch);
        }

        public void ProcessPath(string path, bool isBatch)
        {
            bool honorDisallowedTimes = bool.Parse(ConfigurationManager.AppSettings["honorDisallowedTimes"]);
            string restrictedDays = ConfigurationManager.AppSettings["disallowedDays"];
            TimeSpan restrictedStartTime = TimeSpan.Parse(ConfigurationManager.AppSettings["disallowBatchStart"]);
            TimeSpan restrictedEndTime = TimeSpan.Parse(ConfigurationManager.AppSettings["disallowBatchEnd"]);
            string currentDayOfWeek = "";
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            if (isBatch && honorDisallowedTimes)
            {
                currentDayOfWeek = ((int)DateTime.Now.DayOfWeek).ToString();
                if (restrictedDays.IndexOf(currentDayOfWeek) > -1)
                {
                    // We are in one of the days where we do not want to allow import during the day, check to see if we're within the timeframe
                    if (currentTime > restrictedStartTime && currentTime < restrictedEndTime)
                    {
                        Console.WriteLine("Processing ended early due to time restrictions");
                        return;
                    }
                }
            }

            char[] delim = new char[1];
            delim[0]='\\';
            string[] pathParts = path.Split(delim);
            string directory = "";
            for (int i = 0; i < pathParts.Length - 1; i++)
            {
                directory += pathParts[i] + "\\";
            }
            string[] filePaths = Directory.GetFiles(directory, pathParts[pathParts.Length - 1]);
            foreach (string file in filePaths)
            {

                Console.WriteLine("Starting " + file);
                ProcessFile(file);
                if (isBatch)
                {
                    MoveToArchive(file);
                }

                // Check for time restriction
                if (isBatch && honorDisallowedTimes)
                {
                    currentDayOfWeek = ((int)DateTime.Now.DayOfWeek).ToString();
                    if (restrictedDays.IndexOf(currentDayOfWeek) > -1)
                    {
                        // We are in one of the days where we do not want to allow import during the day, check to see if we're within the timeframe
                        currentTime = DateTime.Now.TimeOfDay;
                        if (currentTime > restrictedStartTime && currentTime < restrictedEndTime)
                        {
                            Console.WriteLine("Processing ended early due to time restrictions");
                            break;
                        }
                    }
                }
                
            } //foreach file
        }

        public string GetFilePath(DateTime date, bool isBatch)
        {
            string retVal = "";
            if (isBatch)
            {
                retVal = ConfigurationManager.AppSettings["xml.batchPath"];
            }
            else
            {
                retVal = ConfigurationManager.AppSettings["xml.path"].Replace("[date]", date.ToString("yyyy-MM-dd_hhmmss"));
            }

            return retVal;
        }

        private void MoveToArchive(string file)
        {
            string archivePath = Path.Combine(Path.GetDirectoryName(file),"archive");
            if (!Directory.Exists(archivePath))
            {
                Directory.CreateDirectory(archivePath);
            }
            File.Move(file, Path.Combine(archivePath, Path.GetFileName(file)));
        }

        public void ProcessFile(string file)
        {
            fileName = file;
            bool recordIsSpam = false;

            int recordCount = 0;
            reportId = auditReportingManager.CreateReport();
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            resourceIdList = "";


            XmlNodeList records = doc.GetElementsByTagName("record");

            foreach (XmlNode record in records)
            {
                recordCount++;
                Console.WriteLine(recordCount.ToString());
                recordIsSpam = SpamChecker.IsSpam(record.OuterXml);
                if (!recordIsSpam)
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(record.OuterXml);
                    ProcessNode(xdoc);
                    lrRecordsProcessed++;
                }
                else
                {
                    lrRecordsSpam++;
                }

            } //foreach record

            // Update ElasticSearch
            string status = "successful";
            if (resourceIdList.Length > 1)
            {
                resourceIdList = resourceIdList.Substring(1, resourceIdList.Length - 2);
                DataSet resourceList = searchManager.GetSqlDataForElasticSearch(resourceIdList, ref status);
                if (ElasticSearchManager.DoesDataSetHaveRows(resourceList))
                {
                    searchManager.BulkUpload(resourceList);
                }
            }
            // Update data warehouse totals
            string whtStatus = DatabaseManager.UpdateWarehouseTotals();
            if (whtStatus != "successful")
            {
                auditReportingManager.LogMessage(reportId, fileName, "", "", ErrorType.Error, ErrorRouting.Technical, whtStatus);
            }
        }

        public void ProcessNode(XmlDocument xdoc)
        {

            string docID = "";
            string url = "";
            string resourceDataType = "";
            string payloadPlacement = "";

            XmlDocument xpayload = new XmlDocument();

            Resource resource = new Resource();

            XmlNodeList list;

            // Determine if the signer is us.  If we signed the document, skip it.
            string signer = GetSigner(xdoc);
            if (signer.ToLower() == ourSignerTag.ToLower())
            {
                return;
            }

            // We didn't sign it, processing continues...
            // Get docID, resource locator, data type, and payload placement
            list = xdoc.GetElementsByTagName("doc_ID");
            if (list.Count > 0)
            {
                docID = TrimWhitespace(list[0].InnerText);
            }
            else
            {
                docID = "";
            }
            resource.Version.LRDocId = docID;
            list = xdoc.GetElementsByTagName("resource_locator");
            if (list.Count > 0)
            {
                url = TrimWhitespace(list[0].InnerText);
            }
            else
            {
                url = "";
            }
            resource.ResourceUrl = url;

            list = xdoc.GetElementsByTagName("resource_data_type");
            if (list.Count > 0)
            {
                resourceDataType = TrimWhitespace(list[0].InnerText);
            }
            else
            {
                resourceDataType = "";
            }
            list = xdoc.GetElementsByTagName("payload_placement");
            if (list.Count > 0)
            {
                payloadPlacement = TrimWhitespace(list[0].InnerText);
            }
            else
            {
                payloadPlacement = "";
            }
            XmlNodeList schemaList = xdoc.GetElementsByTagName("payload_schema");
            // Check docID, resource locator, data type, and payload placement.  If these do not pass, log it and continue with the next record.  
            bool passedEdits = true;
            if (url == "")
            {
                // URL not present in envelope - try to extract from payload instead.  Seems to be a problem only for some NSDL/DC resources.
                bool found = false;
                foreach (XmlNode node in schemaList)
                {
                    string payloadSchema = TrimWhitespace(node.InnerText);
                    switch (payloadSchema.ToLower())
                    {
                        case "nsdl_dc":
                            url = nsdlHandler.ExtractUrlFromPayload(docID, payloadPlacement, xdoc);
                            found = true;
                            break;
                        case "oai_dc":
                            url = nsdlHandler.ExtractUrlFromPayload(docID, payloadPlacement, xdoc);
                            found = true;
                            break;
                        case "nsdl dc 1.02.020":
                            url = nsdlHandler.ExtractUrlFromPayload(docID, payloadPlacement, xdoc);
                            found = true;
                            break;
                        case "dc":
                            url = nsdlHandler.ExtractUrlFromPayload(docID, payloadPlacement, xdoc);
                            found = true;
                            break;
                        case "dc 1.1":
                            url = nsdlHandler.ExtractUrlFromPayload(docID, payloadPlacement, xdoc);
                            found = true;
                            break;
                        default:
                            break;
                    }
                    if (url == "")
                    {
                        // No URL in payload either!
                        passedEdits = false;
                        auditReportingManager.LogMessage(reportId, fileName, docID, url, ErrorType.Error, ErrorRouting.Technical, "No URL found");
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }
            if (resourceDataType != "metadata" && resourceDataType != "paradata")
            {
                passedEdits = false;
                auditReportingManager.LogMessage(reportId, fileName, docID, url, ErrorType.Error, ErrorRouting.Technical, "Invalid resource data type");
                lrRecordsBadDataType++;
            }
            if (payloadPlacement == "none")
            {
                //TODO: Write Delete process
                //ProcessDelete(xdoc);
            }
            else if (payloadPlacement != "inline")
            {
                passedEdits = false;
                auditReportingManager.LogMessage(reportId, fileName, docID, url, ErrorType.Error, ErrorRouting.Technical, "Payload placement is not \"inline.\"");
                lrRecordsBadPayloadPlacement++;
            }

            // If the edits passed so far, continue processing
            if (passedEdits)
            {
                bool found = false;
                foreach (XmlNode node in schemaList)
                {
                    string payloadSchema = TrimWhitespace(node.InnerText);
                    if (payloadSchema.ToLower().IndexOf("json-ld") > -1 || payloadSchema.ToLower().IndexOf("jsonld") > -1 || payloadSchema.ToLower().IndexOf("lrmi") > -1)
                    {
                        lrmiHandler.LrmiMap(docID, url, payloadPlacement, xdoc);
                        found = true;
                    }
                    else
                    {
                        switch (payloadSchema.ToLower())
                        {
                            case "nsdl_dc":
                                nsdlHandler.NsdlMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "nsdl dc 1.02.020":
                                nsdlHandler.NsdlMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "dc":
                                nsdlHandler.NsdlMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "oai_dc":
                                nsdlHandler.NsdlMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "dc 1.1":
                                nsdlHandler.NsdlMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            // Some ID10T decided it would be great to plug in a payload schema of "hashtags".  They all seem to be NSDL so I'm gonna handle 'em as such.
                            // Commented out once initial load is done.
                            case "hashtags":
                                nsdlHandler.NsdlMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "lom":
                                lomHandler.LomMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "lomv1.0adl-rv1.0":
                                lomHandler.LomMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "ieee lom 2002":
                                lomHandler.LomMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "lrmi":
                                lrmiHandler.LrmiMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "comm_para 1.0":
                                commParaHandler.CommParaMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "comm_para":
                                commParaHandler.CommParaMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "oai_paradata":
                                commParaHandler.CommParaMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            case "lr paradata 1.0":
                                lrParaHandler.LrParadataMap(docID, url, payloadPlacement, xdoc);
                                found = true;
                                break;
                            default:
                                break;
                        }
                    }
                    if (found)
                    {
/*                        if (!IsTestEnv())
                        {// Production - don't overload Elastic Search with requests
                            Console.WriteLine("Sleeping 15 seconds to not overwhelm ElasticSearch");
                            System.Threading.Thread.Sleep(15000);
                        } */
                        break;
                    }
                }
                if (!found)
                {
                    if (schemaList != null && schemaList.Count > 0)
                    {
                        string payloadSchema = TrimWhitespace(schemaList[0].InnerText);
                        auditReportingManager.LogMessage(reportId, fileName, docID, url, ErrorType.Error, ErrorRouting.Technical,
                            string.Format("Unknown metadata/paradata schema \"{0}\"", payloadSchema));
                        lrRecordsUnknownSchema++;
                    }
                    else
                    {
                        auditReportingManager.LogMessage(reportId, fileName, docID, url, ErrorType.Error, ErrorRouting.Technical,
                            "Empty metadata/paradata schema!");
                        lrRecordsEmptySchema++;
                    }
                }
            }
        }

/*        public void ProcessDelete(XmlDocument xdoc)
        {
            ResourceVersionManager rvm = new ResourceVersionManager();
            ResourceCommentManager rcm = new ResourceCommentManager();
            XmlNodeList nodes = xdoc.GetElementsByTagName("replaces");
            foreach(XmlNode node in nodes) {
                string replaces = TrimWhitespace(node.InnerText);
                string filter = string.Format("DocId = '{0}' AND Submitter = {1}",replaces, GetSigner(xdoc));
                DataSet ds = rvm.Select(filter);
                if(ResourceVersionManager.DoesDataSetHaveRows(ds)) {

        } */

        public void ExtractVerb(string path, string verb)
        {
            string directory = Path.GetDirectoryName(path);
            string files = directory + "\\*.xml";
            string outFile = Path.Combine(directory, verb + ".xml");
            int nbrRecords = 0;

            XmlDocument newDoc = new XmlDocument();
            newDoc.LoadXml("<root></root>");

            string[] filePaths = Directory.GetFiles(directory, "*.xml");
            foreach (string file in filePaths)
            {
                Console.WriteLine("Starting " + file);
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(file);

                XmlNodeList list = xdoc.GetElementsByTagName("record");
                foreach (XmlNode node in list)
                {
                    XmlDocument record = new XmlDocument();
                    record.LoadXml(node.OuterXml);
                    XmlNodeList xAction = record.GetElementsByTagName("action");
                    if (xAction.Count > 0 && xAction[0].InnerText.ToLower() == verb.ToLower())
                    {
                        XmlNode newNode = newDoc.ImportNode(node, true);
                        newDoc.DocumentElement.AppendChild(newNode);
                        nbrRecords++;
                        Console.WriteLine(nbrRecords);
                    }
                }
            }
            newDoc.Save(outFile);

        }

        public void ExtractSchema(string path, string schema)
        {
            string directory = Path.GetDirectoryName(path);
            string files = directory + "\\*.xml";
            string outFile = Path.Combine(directory, schema + ".xml");
            int nbrRecords = 0;
            bool found = false;

            XmlDocument newDoc = new XmlDocument();
            newDoc.LoadXml("<root></root>");

            string[] filePaths = Directory.GetFiles(directory, "*.xml");
            foreach (string file in filePaths)
            {
                Console.WriteLine("Starting " + file);
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(file);

                XmlNodeList list = xdoc.GetElementsByTagName("record");
                foreach (XmlNode node in list)
                {
                    XmlDocument record = new XmlDocument();
                    record.LoadXml(node.OuterXml);
                    XmlNodeList xSchema = record.GetElementsByTagName("payload_schema");
                    found = false;
                    foreach (XmlNode sNode in xSchema)
                    {
                        if (sNode.InnerText.ToLower() == schema.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        XmlNode newNode = newDoc.ImportNode(node, true);
                        newDoc.DocumentElement.AppendChild(newNode);
                        nbrRecords++;
                        Console.WriteLine(nbrRecords);
                        if ((nbrRecords % 200) == 0)
                        {
                            newDoc.Save(outFile.Replace(".xml","") + nbrRecords.ToString() + ".xml");
                            newDoc = new XmlDocument();
                            newDoc.LoadXml("<root></root>");
                        }
                    }
                }
            }
            newDoc.Save(outFile.Replace(".xml", "") + nbrRecords.ToString() + ".xml");
        }

        public void UndeleteResources()
        {
            DataSet ds = resourceManager.GetListOfResourcesToUndelete(true);
            if (ResourceManager.DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string listOfResources = ResourceManager.GetRowColumn(dr, "ResourceIds", "");
                    string status = "successful";
                    DataSet resourceList = searchManager.GetSqlDataForElasticSearch(listOfResources, ref status);
                    if (ElasticSearchManager.DoesDataSetHaveRows(resourceList))
                    {
                        searchManager.BulkUpload(resourceList);
                        // Pause so as to not overwhelm ElasticSearch
                        Console.WriteLine("Sleeping 60 seconds so as to not overwhelm ElasticSearch");
                        System.Threading.Thread.Sleep(60000);
                    }
                }
            }
        }


        protected void GetDocId(Resource resource, XmlDocument record)
        {
            XmlNodeList list = record.GetElementsByTagName("doc_ID");
            resource.Version.LRDocId = TrimWhitespace(list[0].InnerText);
        }

        protected void GetCreatedDate(Resource resource, XmlDocument record)
        {
            XmlNodeList list = record.GetElementsByTagName("datestamp");
            string sqlDate = TrimWhitespace(list[0].InnerText);
            if (sqlDate == null || sqlDate == "")
            {
                resource.Version.Created = DateTime.Now;
            }
            else
            {
                sqlDate = sqlDate.Replace("T", " ").Replace("Z", "");
                resource.Version.Created = DateTime.Parse(sqlDate);
                if (resource.Version.Created < System.Data.SqlTypes.SqlDateTime.MinValue)
                {
                    resource.Version.Created = DateTime.Now;
                }
            }
        }


        protected void SaveDoc(XmlDocument xdoc, DateTime date, bool isBatch)
        {
            string docLoc = GetFilePath(date, isBatch);
            // Check if folder exists.  If not, then create it.
            bool folderExists = Directory.Exists(docLoc);
            if (!folderExists)
            {
                Directory.CreateDirectory(docLoc);
            }

            docLoc += ConfigurationManager.AppSettings["xml.log"].ToString();
            // Save the document to a file. White space is
            // preserved (no white space).
            xdoc.PreserveWhitespace = true;
            docLoc = docLoc.Replace("[date]", System.DateTime.Now.ToString("yyyy-MM-dd_hhmmss_") + System.DateTime.Now.Millisecond.ToString());
            Console.WriteLine(string.Format("Storing {0}",docLoc));
            xdoc.Save(docLoc);
        } //

        public static bool IsTestEnv()
        {
            bool isTestEnv = false;
            try
            {
                isTestEnv = bool.Parse(ConfigurationManager.AppSettings["IsTestEnv"].ToString());
            }
            catch (Exception ex)
            {
                // Default to false
            }

            return isTestEnv;
        }
    }
}
