using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
//using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace LearningRegistryCache2
{
    public class LomHandler : MetadataController
    {
        private ResourceClusterManager clusterManager;
        private AuditReportingManager auditReportingManager;

        public LomHandler()
        {
            clusterManager = new ResourceClusterManager();
            auditReportingManager=new AuditReportingManager();
        }

        public void LomMap(string docId, string url, string payloadPlacement, XmlDocument record)
        {
            XmlDocument payload = new XmlDocument();
            bool isValid = false;
            XmlNodeList list;
            XmlNamespaceManager nsm = null;

            Resource resource = LoadCommonMetadata(docId, url, payloadPlacement, record, payload, ref isValid);
            if (!isValid)
            {
                // Check to see if Resource.Version record exists.  If not, remove it.  Then skip this record
                VerifyResourceVersionRecordExists(resource);
                return;
            }
/*             if (payload.DocumentElement.Attributes["xmlns"] != null)
            {
                string xmlns = payload.DocumentElement.Attributes["xmlns"].Value;
                nsm = new XmlNamespaceManager(payload.NameTable);
                nsm.AddNamespace("MsBuild", xmlns);
            }

            // End common logic for all metadata schemas

            // It's easier to parse LOM using XPath rather than using GetElementsByTagName so we'll use XPath.
            if (nsm == null)
            {
                list = payload.DocumentElement.SelectNodes("/metadata"); //description/metadata/lom/general/title/*"); //string");
            }
            else
            {
                list = payload.DocumentElement.SelectNodes("/metadata/work/description/metadata/general/title/string", nsm);
            } */

            // Find title
            list = GetLomElementsByTagName(payload,"general");
            XmlDocument xdoc = new XmlDocument();
            bool found = false;
            foreach (XmlNode node in list)
            {
                xdoc.LoadXml(node.OuterXml);
                list = GetLomElementsByTagName(xdoc,"title");
                foreach(XmlNode node1 in list) {
                    xdoc.LoadXml(node1.OuterXml);
                    list = GetLomElementsByTagName(xdoc, "string");
                    foreach(XmlNode node2 in list) {
                        resource.Version.Title = TrimWhitespace(node2.InnerText);
                        found = true;
                        break;
                    }
                    if(found) {
                        break;
                    }
                }
                if(found) {
                    break;
                }
            }
            found = false;
            // If no title, reject the record with an error message
            if (resource.Version.Title == null || resource.Version.Title == "")
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Program, "No title found for resource.  Resource skipped.");
                VerifyResourceVersionRecordExists(resource);
                return;
            }
            
            //If title does not meet minimum requirements, make record inactive and log it.
            string status = "OK";
            if (!IsGoodTitle(resource.Version.Title, ref status))
            {
                resource.Version.IsActive = false;
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Program, status);
            }


            // Find description
            list = GetLomElementsByTagName(payload, "general");
            foreach (XmlNode node in list)
            {
                xdoc.LoadXml(node.OuterXml);
                list = GetLomElementsByTagName(xdoc, "description");
                foreach (XmlNode node1 in list)
                {
                    xdoc.LoadXml(node1.OuterXml);
                    list = GetLomElementsByTagName(xdoc, "string");
                    foreach (XmlNode node2 in list)
                    {
                        resource.Version.Description = CleanseDescription(TrimWhitespace(node2.InnerText));
                        found = true;
                        break;
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }
            found = false;
            // If no description, use title
            if (resource.Version.Description == null || resource.Version.Description == "")
            {
                resource.Version.Description = CleanseDescription(resource.Version.Title);
            }

            // Find publisher and Author - using host as something isn't right with the XML of LOM
            try
            {
                Uri uri = new Uri(resource.ResourceUrl);
                resource.Version.Publisher = resource.Version.Creator = uri.Host;
            }
            catch (UriFormatException ufe)
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId,
                    resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, "LOMHandler could not determine publisher.  Publisher populated with Unknown.");
                resource.Version.Publisher = resource.Version.Creator = "Unknown";
            }

            // Get Use rights
            found = false;
            list = GetLomElementsByTagName(payload, "rights");
            foreach (XmlNode node in list)
            {
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc, "description");
                foreach (XmlNode node1 in list1)
                {
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(node1.OuterXml);
                    XmlNodeList list2 = GetLomElementsByTagName(xd, "string");
                    foreach (XmlNode node2 in list2)
                    {
                        resource.Version.Rights = TrimWhitespace(node2.InnerText);
                        found = true;
                        break;
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // Get Access rights
            found = false;
            list = GetLomElementsByTagName(payload, "rights");
            foreach (XmlNode node in list)
            {
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc, "cost");
                foreach (XmlNode node1 in list1)
                {
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(node1.OuterXml);
                    XmlNodeList list2 = GetLomElementsByTagName(xd, "value");
                    foreach (XmlNode node2 in list2)
                    {
                        if (TrimWhitespace(node2.InnerText).ToLower() == "no")
                        {
                            resource.Version.AccessRights = "Free access";
                        }
                        else if (TrimWhitespace(node2.InnerText).ToLower() == "yes")
                        {
                            resource.Version.AccessRights = "Available for purchase";
                        }
                        else
                        {
                            resource.Version.AccessRights = TrimWhitespace(node2.InnerText);
                        }
                        found = true;
                        break;
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // Get typical learning time
            found=false;
            list = GetLomElementsByTagName(payload, "educational");
            foreach (XmlNode node in list)
            {
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc, "typicalLearningTime");
                foreach (XmlNode node1 in list1)
                {
                    XmlDocument xd = new XmlDocument();
                    xd.LoadXml(node1.OuterXml);
                    XmlNodeList list2 = GetLomElementsByTagName(xd, "duration");
                    foreach (XmlNode node2 in list2)
                    {
                        resource.Version.TypicalLearningTime = TrimWhitespace(node2.InnerText);
                        found = true;
                        break;
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            list = record.GetElementsByTagName("payload_schema");
            foreach (XmlNode node in list)
            {
                resource.Version.Schema = TrimWhitespace(node.InnerText);
                break;
            }

            // Get interactivity type
            found = false;
            list = GetLomElementsByTagName(record, "educational");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc1 = new XmlDocument();
                xdoc1.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc1, "interactivityType");
                foreach (XmlNode node1 in list1)
                {
                    XmlDocument xdoc2 = new XmlDocument();
                    xdoc2.LoadXml(node1.OuterXml);
                    XmlNodeList list2 = GetLomElementsByTagName(xdoc2, "value");
                    foreach (XmlNode node2 in list2)
                    {
                        resource.Version.InteractivityType = TrimWhitespace(node2.InnerText);
                        if (resource.Version.InteractivityType.Length > 100)
                        {
                            resource.Version.InteractivityType = resource.Version.InteractivityType.Substring(0, 100);
                        }
                        resource.Version.InteractivityTypeId = 0; // Force remapping!
                        found = true;
                        break;
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }


            //Now do name/value pairs
            try {

                if (resource.Version.RowId == null || resource.Version.RowId == new Guid(ResourceManager.DEFAULT_GUID))
                {
                    AddResource(resource);
                }
                else
                {
                    UpdateResource(resource);
                }

                //resource.AddKeyword(record, "keys");
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
                AddSubjects(resource, payload);
                string ageRange = AddEducationLevels(resource, payload);
                ConvertAgeToGrade(resource, ageRange, ref status);
                AddIntendedAudience(resource, payload);
                AddLanguage(resource, payload);

                AddResourceFormats(resource, payload);
                AddResourceTypes(resource, payload);

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
            }
            catch (Exception ex)
            {
            }
        
        }

        #region ====== Helper Methods ======
        private XmlNodeList GetLomElementsByTagName(XmlDocument xdoc, string tagName)
        {
            XmlNodeList list = xdoc.GetElementsByTagName(tagName);
            if (list.Count == 0)
            {
                tagName = "lom:" + tagName;
                list = xdoc.GetElementsByTagName(tagName);
            }
            return list;
        }

        private void AddSubjects(Resource resource, XmlDocument payload)
        {
            ResourceSubjectManager subjectManager = new ResourceSubjectManager();
            string status = "";
            int nbrSubjects = 0;
            int maxSubjects = int.Parse(ConfigurationManager.AppSettings["maxSubjectsToProcess"]);

            XmlNodeList list = GetLomElementsByTagName(payload, "classification");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc1 = new XmlDocument();
                xdoc1.LoadXml(node.OuterXml);
                XmlNodeList list2 = GetLomElementsByTagName(xdoc1, "purpose");
                foreach (XmlNode node2 in list2)
                {
                    XmlDocument xdoc2 = new XmlDocument();
                    xdoc2.LoadXml(node2.OuterXml);
                    XmlNodeList list3 = GetLomElementsByTagName(xdoc2, "value");
                    foreach (XmlNode node3 in list3)
                    {
                        if (node3.InnerText.Trim() == "discipline")
                        {
                            XmlNodeList list4 = GetLomElementsByTagName(xdoc1, "taxon");
                            foreach (XmlNode node4 in list4)
                            {
                                XmlDocument xdoc4 = new XmlDocument();
                                xdoc4.LoadXml(node4.OuterXml);
                                XmlNodeList list5 = GetLomElementsByTagName(xdoc4, "string");
                                foreach (XmlNode node5 in list5)
                                {
                                    string subject = TrimWhitespace(node5.InnerText);

                                    int amp = subject.IndexOf("&");
                                    int scolon = subject.IndexOf(";");
                                    if (scolon > -1 && amp == -1)
                                    {
                                        // subject contains ";" but does not contain "&" (ie contains semicolon but no HTML Entities), so split by semicolon
                                        string[] subjectList = subject.Split(';');
                                        foreach (string subjectItem in subjectList)
                                        {
                                            if (!SkipSubject(subjectItem))
                                            {
                                                if (maxSubjects > 0 && nbrSubjects > maxSubjects)
                                                {
                                                    break;
                                                }
                                                string subj = ApplySubjectEditRules(subjectItem);
                                                ResourceSubject resSubject = new ResourceSubject();
                                                resSubject.ResourceId = resource.RowId;
                                                resSubject.ResourceIntId = resource.Id;
                                                resSubject.Subject = TrimWhitespace(subj);
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
                                    else
                                    {
                                        if (maxSubjects > 0 && nbrSubjects > maxSubjects)
                                        {
                                            break;
                                        }
                                        subject = ApplySubjectEditRules(subject);
                                        ResourceSubject resSubject = new ResourceSubject();
                                        resSubject.ResourceId = resource.RowId;
                                        resSubject.ResourceIntId = resource.Id;
                                        resSubject.Subject = TrimWhitespace(subject);
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
                        }
                    }
                }
            }
            if (maxSubjects > 0 && nbrSubjects > maxSubjects)
            {
                string message = string.Format("Number of keywords exceeds {0}.  First {0} subjects processed, remainder ignored. ", maxSubjects);
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Warning, ErrorRouting.Program, message);

            }
        }// AddSubjects

        private string AddEducationLevels(Resource resource, XmlDocument payload)
        {
            string ageRange = "";
            Regex whitespaceRegex = new Regex(@"\s+"); // Regular expression to match on any whitespace.
            string status = "successful";
            ResourcePropertyManager propManager = new ResourcePropertyManager();
            ResourceKeywordManager keysManager = new ResourceKeywordManager();
            ResourceAgeRangeManager ageRangeManager = new ResourceAgeRangeManager();

            XmlNodeList list = GetLomElementsByTagName(payload, "educational");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc, "typicalAgeRange");
                foreach (XmlNode node1 in list1)
                {
                    XmlDocument xdoc1 = new XmlDocument();
                    xdoc1.LoadXml(node1.OuterXml);
                    XmlNodeList list2 = GetLomElementsByTagName(xdoc, "string");
                    foreach (XmlNode node2 in list2)
                    {
                        ageRange = CleanUpAgeRange(node2.InnerText.Trim());
                        ResourceAgeRange level = new ResourceAgeRange();
                        level.ResourceIntId = resource.Id;
                        level.OriginalValue = ageRange;
                        int dashIndex = ageRange.IndexOf("-");
                        int length = ageRange.Length;
                        if (length > 3)
                        {
                            level.FromAge = int.Parse(level.OriginalValue.Substring(0, dashIndex));
                            level.ToAge = int.Parse(level.OriginalValue.Substring(dashIndex + 1, length - dashIndex - 1));
                            status = ageRangeManager.Import(level);
                            if (status != "successful")
                            {
                                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                                    ErrorRouting.Technical, status);
                            }
                        }
                    }
                }
            }
            
            return ageRange;
        }// AddEducationLevels


        private void AddIntendedAudience(Resource resource, XmlDocument payload)
        {
            int iPropertyType = 0;
            string status = "successful";
            ResourceKeywordManager keysManager = new ResourceKeywordManager();

            XmlNodeList list = GetLomElementsByTagName(payload, "educational");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc, "intendedEndUserRole");
                foreach (XmlNode node1 in list1)
                {
                    XmlDocument xdoc1 = new XmlDocument();
                    xdoc1.LoadXml(node1.OuterXml);
                    XmlNodeList list2 = GetLomElementsByTagName(xdoc, "value");
                    foreach (XmlNode node2 in list2)
                    {
                        ResourceChildItem audience = new ResourceChildItem();
                        audience.ResourceId = resource.RowId;
                        audience.ResourceIntId = resource.Id;
                        audience.OriginalValue = TrimWhitespace(node2.InnerText);
                        audienceManager.CreateFromEntity(audience, ref status);
                        if (status != "successful")
                        {
                            auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                                ErrorRouting.Technical, status);
                        }
                    }
                }
            }

            resource.Keyword = keysManager.Select(resource.Id, "");
            resource.Audience = audienceManager.SelectCollection(resource.Id);
        }// AddIntendedAudience

        private void AddLanguage(Resource resource, XmlDocument payload)
        {
            int iPropertyType = 0;
            string status = "successful";
            ResourceLanguageManager languageManager = new ResourceLanguageManager();
            ResourceKeywordManager keysManager = new ResourceKeywordManager();

            XmlNodeList list = GetLomElementsByTagName(payload, "general");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc1 = new XmlDocument();
                xdoc1.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc1, "language");
                foreach (XmlNode node1 in list1)
                {
                    ResourceChildItem language = new ResourceChildItem();
                    language.ResourceId = resource.RowId;
                    language.ResourceIntId = resource.Id;
                    language.OriginalValue = TrimWhitespace(node1.InnerText);
                    languageManager.Import(language, ref status);
                    if (status != "successful")
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                            ErrorRouting.Technical, status);
                    }
                }
            }

            resource.Language = languageManager.Select(resource.Id, "");
            if (resource.Language == null || resource.Language.Count == 0)
            {
                // No language found - assume English but log it
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

            resource.Keyword = keysManager.Select(resource.Id, "");
        }// AddLanguage

        private void AddResourceTypes(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            ResourceTypeManager rtm = new ResourceTypeManager();
            ResourceKeywordManager keysManager = new ResourceKeywordManager();

            XmlNodeList list = GetLomElementsByTagName(payload, "educational");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc, "learningResourceType");
                foreach (XmlNode node1 in list1)
                {
                    XmlDocument xdoc1 = new XmlDocument();
                    xdoc1.LoadXml(node1.OuterXml);
                    XmlNodeList list2 = GetLomElementsByTagName(xdoc1, "value");
                    foreach (XmlNode node2 in list2)
                    {
                        ResourceChildItem type = new ResourceChildItem();
                        type.ResourceId = resource.RowId;
                        type.ResourceIntId = resource.Id;
                        type.OriginalValue = TrimWhitespace(node2.InnerText);

                        rtm.Import(type, ref status);
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
                }
            }

            resource.ResourceType = (List<ResourceChildItem>)rtm.Select( resource.Id );
            resource.Keyword = keysManager.Select(resource.Id, "");
        }// AddResourceTypes

        private void AddResourceFormats(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            ResourceFormatManager rfm = new ResourceFormatManager();
            ResourceKeywordManager keysManager = new ResourceKeywordManager();

            XmlNodeList list = GetLomElementsByTagName(payload, "technical");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list1 = GetLomElementsByTagName(xdoc, "format");
                foreach (XmlNode node1 in list1)
                {
                    ResourceChildItem format = new ResourceChildItem();
                    format.ResourceId = resource.RowId;
                    format.ResourceIntId = resource.Id;
                    format.OriginalValue = TrimWhitespace(node1.InnerText);

                    rfm.Import(format, ref status);
                    if (status != "successful")
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                            ErrorRouting.Technical, status);
                    }
                }
            }

            resource.ResourceFormat = (List<ResourceChildItem>)rfm.Select(resource.Id);
            //resource.Keyword = keysManager.Select(resource.RowId.ToString(), "");
        }// AddResourceFormats


        #endregion
    }
}
