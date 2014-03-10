using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
//using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace LearningRegistryCache2
{
    public class NsdlHandler : MetadataController
    {
        private ResourceClusterManager clusterManager;
        private AuditReportingManager auditReportingManager;
        private ResourceStandardManager standardManager;
        private ResourceGradeLevelManager gradeLevelManager;
        private ResourcePropertyManager propManager;
        private ResourceLanguageManager languageManager;

        public NsdlHandler()
        {
            clusterManager = new ResourceClusterManager();
            auditReportingManager = new AuditReportingManager();
            standardManager = new ResourceStandardManager();
            gradeLevelManager = new ResourceGradeLevelManager();
            propManager = new ResourcePropertyManager();
            languageManager = new ResourceLanguageManager();
        }

        public void NsdlMap(string docId, string url, string payloadPlacement, XmlDocument record)
        {
            // Begin common logic for all metadata schemas
            XmlDocument payload = new XmlDocument();
            bool isValid = false;

            Resource resource = LoadCommonMetadata(docId, url, payloadPlacement, record, payload, ref isValid);
            if (!isValid)
            {
                // Check to see if Resource.Version record exists.  If not, remove it.  Then skip this record
                VerifyResourceVersionRecordExists(resource);
                return;
            }
            // End common logic for all metadata schemas

            /* recordCntr++;
            if (recordCntr < 6)
            {
                //for debug output xml for first n records
                SaveDoc(record);
            } */

            XmlNodeList titleList = payload.GetElementsByTagName("dc:title");
            foreach (XmlNode title in titleList)
            {
                resource.Version.Title = TrimWhitespace(title.InnerText);
                break;
            }
            //if no title, reject the record with an error message
            if (resource.Version.Title == null || resource.Version.Title == "")
            {
                VerifyResourceVersionRecordExists(resource);
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Program, "No title found for resource.  Resource skipped.");
                return;
            }
            //If title does not meet minimum requirements, make record inactive and log it.
            string status = "OK";
            if (!IsGoodTitle(resource.Version.Title, ref status))
            {
                resource.Version.IsActive = false;
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Program, status);
            }

            XmlNodeList descriptionList = payload.GetElementsByTagName("dc:description");
            foreach (XmlNode description in descriptionList)
            {
                resource.Version.Description = CleanseDescription(TrimWhitespace(description.InnerText));
            }
            if (resource.Version.Description == null || resource.Version.Description == "")
            {
                resource.Version.Description = CleanseDescription(resource.Version.Title);
            }

            XmlNodeList publisherList = payload.GetElementsByTagName("dc:publisher");
            foreach (XmlNode publisher in publisherList)
            {
                resource.Version.Publisher = TrimWhitespace(publisher.InnerText);
            }

            XmlNodeList creatorList = payload.GetElementsByTagName("dc:creator");
            foreach (XmlNode creator in creatorList)
            {
                resource.Version.Creator = TrimWhitespace(creator.InnerText);
            }
            if ((resource.Version.Publisher == null || resource.Version.Publisher == "") && (resource.Version.Creator == null || resource.Version.Creator == ""))
            {
                try
                {
                    Uri tempUri = new Uri(resource.ResourceUrl);
                    resource.Version.Publisher = resource.Version.Creator = tempUri.Host;
                }
                catch (Exception ex)
                {
                    resource.Version.Publisher = resource.Version.Creator = "Unknown";
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, ErrorType.Warning, url, ErrorRouting.Technical, "Unknown publisher/bad URL");
                }
            }
            if (resource.Version.Publisher == null || resource.Version.Publisher == "")
            {
                resource.Version.Publisher = resource.Version.Creator;
            }
            if (resource.Version.Creator == null || resource.Version.Creator == "")
            {
                resource.Version.Creator = resource.Version.Publisher;
            }

            XmlNodeList rightsList = payload.GetElementsByTagName("dc:rights");
            foreach (XmlNode rights in rightsList)
            {
                if (rights.Attributes["xsi:type"] != null && rights.Attributes["xsi:type"].Value == "dct:URI")
                {
                    resource.Version.Rights = TrimWhitespace(rights.InnerText);
                }
            }

            XmlNodeList accessRightsList = payload.GetElementsByTagName("dct:accessRights");
            foreach (XmlNode accessRights in accessRightsList)
            {
                resource.Version.AccessRights = TrimWhitespace(accessRights.InnerText);
            }

            XmlNodeList typicalLearningTimeList = payload.GetElementsByTagName("ieee:typicalLearningTime");
            foreach (XmlNode typicalLearningTime in typicalLearningTimeList)
            {
                resource.Version.TypicalLearningTime = TrimWhitespace(typicalLearningTime.InnerText);
                // take only the first value
                break;
            }

            XmlNodeList schemaList = record.GetElementsByTagName("payload_schema");
            foreach (XmlNode node in schemaList)
            {
                resource.Version.Schema = TrimWhitespace(node.InnerText);
                break;
            }

            XmlNodeList requirementsList = record.GetElementsByTagName("dct:Requires");
            foreach (XmlNode node in requirementsList)
            {
                resource.Version.Requirements = TrimWhitespace(node.InnerText);
                break;
            }

            XmlNodeList interactivityType = record.GetElementsByTagName("ieee:interactivityType");
            if (interactivityType.Count > 1)
            {
                resource.Version.InteractivityType = TrimWhitespace(interactivityType[0].InnerText);
                resource.Version.InteractivityTypeId = 0;  // Force a re-map
            }

            try
            {
                if (resource.Version.RowId == null || resource.Version.RowId == new Guid(ResourceManager.DEFAULT_GUID))
                {
                    AddResource(resource);
                }
                else
                {
                    UpdateResource(resource);
                }
                //Now do name/value pairs
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
                AddSubjects(resource, payload);
                AddStandards(resource, payload);
                string[] gradeLevels = AddGradeLevels(resource, payload);
                if (gradeLevels.Length > 0)
                {
                    ConvertGradesToAgeRange(resource, gradeLevels);
                }
                AddAudiences(resource, payload);
                AddResourceFormats(resource, payload);
                AddResourceTypes(resource, payload);
                AddLanguages(resource, payload);

                if (CheckForGoodLanguage(resource.Id) == true)
                {
                    // Add to Elastic Search index
                    UpdateElasticSearchList(resource.Id);
                }
                else
                {
                    // Deactivate resource - no good language
                    resource.IsActive = false;
                    resourceManager.UpdateByRowId( resource );
                }
                // AddInteractivityType(resource, payload); //Handled in code above (ieee:interactivityType)


            }
            catch (Exception ex)
            {
            }
        } //NsdlMap

        /// <summary>
        /// Call this only when the URL can't be found in the envelope!
        /// </summary>
        /// <param name="docId"></param>
        /// <param name="payloadPlacement"></param>
        /// <param name="xdoc"></param>
        /// <returns></returns>
        public string ExtractUrlFromPayload(string docId, string payloadPlacement, XmlDocument xdoc)
        {
            string url = "";
            string payload = "";
            XmlDocument xpayload = new XmlDocument();
            XmlNodeList list;
            if (payloadPlacement == "inline")
            {
                list = xdoc.GetElementsByTagName("resource_data");
                if (list[0].InnerXml.IndexOf("<resource_data>") > -1)
                {
                    Regex resourceDataNodeEx = new Regex(@"<resource_data>([\s\S]*)</resource_data>");
                    payload = resourceDataNodeEx.Match(list[0].InnerXml).Value;
                    payload = payload.Replace("<resource_data>", "");
                    payload = payload.Replace("</resource_data>", "");
                    payload = payload.Replace("&lt;", "<").Replace("&gt;", ">");
                    payload = payload.Replace(">>", ">");
                    payload = payload.Replace("\\\"", "\"");
                }
                else
                {
                    payload = list[0].OuterXml.Replace("&lt;", "<").Replace("&gt;", ">");
                    payload = payload.Replace(">>", ">");
                    payload = payload.Replace("\\\"", "\"");
                }
                // Can't have this with any whitespace in front of it.  Some LR records have this with whitespace in front of it, or with other tags preceeding it.
                payload = payload.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", "");
                //xpayload = new XmlDocument();
                try
                {
                    xpayload.LoadXml(payload);
                }
                catch (Exception ex)
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                }

                list = xpayload.GetElementsByTagName("dc:identifier");
                if (list != null && list.Count > 0)
                {
                    url = TrimWhitespace(list[0].InnerText);
                }
            }
            else
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, "ExtractUrlFromPayload(): only 'inline' payload is supported.'");
            }

            return url;
        }

        protected void AddSubjects(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            int nbrSubjects = 0;
            int maxSubjects = int.Parse(ConfigurationManager.AppSettings["maxSubjectsToProcess"]);
            XmlNodeList list = payload.GetElementsByTagName("dc:subject");
            foreach (XmlNode node in list)
            {
                string subject = TrimWhitespace(node.InnerText);
                int amp = subject.IndexOf("&");
                int scolon = subject.IndexOf(";");
                if (scolon > -1 && amp == -1)
                {
                    // subject contains ; but does not contain & (ie contains semicolon but no HTML entities), so split by semicolon
                    string[] subjectList = subject.Split(';');
                    foreach (string subjectItem in subjectList)
                    {
                        if (!SkipSubject(subjectItem))
                        {
                            if (maxSubjects > 0 && nbrSubjects > maxSubjects)
                            {
                                break;
                            }
                            ResourceSubject resSubject = new ResourceSubject();
                            resSubject.ResourceId = resource.RowId;
                            resSubject.ResourceIntId = resource.Id;
                            resSubject.Subject = ApplySubjectEditRules(subjectItem);
                            resSubject.Subject = TrimWhitespace(resSubject.Subject);
                            status = subjectManager.Create(resSubject);
                            if (status != "successful")
                            {
                                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                                    ErrorType.Error, ErrorRouting.Technical, status);
                            }
                            nbrSubjects++;
                        }
                    }
                }
                else if (!SkipSubject(subject))
                {
                    if (maxSubjects > 0 && nbrSubjects > maxSubjects)
                    {
                        break;
                    }
                    subject = ApplySubjectEditRules(subject);
                    subject = TrimWhitespace(subject);
                    ResourceSubject resSubject = new ResourceSubject();
                    resSubject.ResourceId = resource.RowId;
                    resSubject.ResourceIntId = resource.Id;
                    resSubject.Subject = subject;
                    status = subjectManager.Create(resSubject);
                    if (status != "successful")
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
            }
            if (nbrSubjects > maxSubjects)
            {
                string message = string.Format("Number of keywords exceeds {0}.  First {0} subjects processed, remainder ignored. ", maxSubjects);
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.Version.ResourceUrl, ErrorType.Warning, ErrorRouting.Program, message);
            }

        }// AddSubjects

        protected void AddStandards(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("dct:conformsTo");
            foreach (XmlNode node in list)
            {
                string value = TrimWhitespace(node.InnerText);
                ResourceStandard standard = new ResourceStandard();
                standard.ResourceId = resource.RowId;
                standard.ResourceIntId = resource.Id;
                standard.StandardId = 0;
                standard.StandardUrl = value;
                if (standard.StandardUrl.Length > 300)
                {
                    standard.StandardUrl = standard.StandardUrl.Substring(0, 300);
                }

                standardManager.Import(standard, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }// AddStandards

        protected string[] AddGradeLevels(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            ArrayList grades = new ArrayList();

            XmlNodeList list = payload.GetElementsByTagName("dct:educationLevel");
            foreach (XmlNode node in list)
            {
                ResourceChildItem level = new ResourceChildItem();
                level.ResourceId = resource.RowId;
                level.ResourceIntId = resource.Id;
                level.OriginalValue = TrimWhitespace(node.InnerText);
                gradeLevelManager.Import(level, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
                grades.Add(level.OriginalValue);
            }

            resource.Gradelevel = gradeLevelManager.Select(resource.Id, "");
            if ( resource.Gradelevel == null || resource.Gradelevel.Count == 0 )
            {
                // If no education levels found, assume "unknown" but log it.
                ResourceChildItem level = new ResourceChildItem();
                level.ResourceId = resource.RowId;
                level.OriginalValue = "Unknown";
                gradeLevelManager.Import(level, ref status);
                if (status == "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Warning, ErrorRouting.Technical, "No education level was found for resource.  Unknown assumed.  Please verify education level.");
                }
                else
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }

            if (grades.Count > 0)
            {
                string[] retVal = new string[grades.Count];
                for (int i = 0; i < grades.Count; i++)
                {
                    retVal[i] = grades[i].ToString();
                }
                return retVal;
            }
            else
            {
                return new string[0];
            }
        }// AddEducationLevels

        protected void AddAudiences(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("dct:audience");
            foreach (XmlNode node in list)
            {
                ResourceChildItem audience = new ResourceChildItem();
                audience.ResourceId = resource.RowId;
                audience.ResourceIntId = resource.Id;
                audience.OriginalValue = TrimWhitespace(node.InnerText);
                audienceManager.Import(audience, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }// AddAudiences

        protected void AddResourceFormats(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("dc:format");
            foreach (XmlNode node in list)
            {
                ResourceChildItem format = new ResourceChildItem();
                format.ResourceId = resource.RowId;
                format.ResourceIntId = resource.Id;
                format.OriginalValue = TrimWhitespace(node.InnerText);
                formatManager.Import(format, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
                if (resource.Version.Requirements == "" && format.OriginalValue.Length > 20)
                {
                    resource.Version.Requirements = format.OriginalValue;
                }
            }
        }// AddResourceFormats

        protected void AddResourceTypes(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("dc:type");
            foreach (XmlNode node in list)
            {
                ResourceChildItem type = new ResourceChildItem();
                type.ResourceId = resource.RowId;
                type.ResourceIntId = resource.Id;
                type.OriginalValue = TrimWhitespace(node.InnerText);
                typeManager.Import(type, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }

        protected void AddLanguages(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("dct:language");
            if (list == null || list.Count == 0)
            {
                list = payload.GetElementsByTagName("dc:language");
            }
            foreach (XmlNode node in list)
            {
                ResourceChildItem language = new ResourceChildItem();
                language.ResourceId = resource.RowId;
                language.ResourceIntId = resource.Id;
                language.OriginalValue = TrimWhitespace(node.InnerText);
                languageManager.Import(language, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }

            resource.Language = languageManager.Select(resource.Id, "");
            if (resource.Language == null || resource.Language.Count == 0)
            {
                // If no languages found, assume English but log it
                ResourceChildItem language = new ResourceChildItem();
                language.ResourceId = resource.RowId;
                language.ResourceIntId = resource.Id;
                language.OriginalValue = "English";
                languageManager.Import(language, ref status);
                if (status == "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Warning,
                        ErrorRouting.Program, "No language was found for resource.  English language assumed.  Please verify this resource's language.");
                }
                else
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                        ErrorRouting.Technical, status);
                }
            }
        }// AddLanguages
    }
}
