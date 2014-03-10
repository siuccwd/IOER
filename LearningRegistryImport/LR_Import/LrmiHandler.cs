using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LRWarehouse.Business;
using LRWarehouse.DAL;


namespace LearningRegistryCache2
{
    public class LrmiHandler : MetadataController
    {
        private ResourceAgeRangeManager ageRangeManager;
        private ResourceClusterManager clusterManager;
        private ResourceGradeLevelManager gradeLevelManager;
        private AuditReportingManager auditReportingManager;
        private ResourceStandardManager standardManager;
        private ResourceGradeLevelManager educationManager;
        private ResourcePropertyManager propManager;
        private ResourceLanguageManager languageManager;
        private ResourceEducationUseManager edUseManager;
        private ResourceGroupTypeManager groupTypeManager;
        private ResourceItemTypeManager itemTypeManager;
        private ResourceAssessmentTypeManager asmtTypeManager;

        public LrmiHandler()
        {
            ageRangeManager = new ResourceAgeRangeManager();
            clusterManager = new ResourceClusterManager();
            auditReportingManager = new AuditReportingManager();
            gradeLevelManager = new ResourceGradeLevelManager();
            standardManager = new ResourceStandardManager();
            educationManager = new ResourceGradeLevelManager();
            propManager = new ResourcePropertyManager();
            languageManager = new ResourceLanguageManager();
            edUseManager = new ResourceEducationUseManager();
            groupTypeManager = new ResourceGroupTypeManager();
            itemTypeManager = new ResourceItemTypeManager();
            asmtTypeManager = new ResourceAssessmentTypeManager();
        }


        public void LrmiMap(string docId, string url, string payloadPlacement, XmlDocument record)
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


            XmlNodeList list = payload.GetElementsByTagName("name");
            foreach (XmlNode title in list)
            {
                if (title.ParentNode.Name.ToLower() == "resource_data")
                {
                    resource.Version.Title = TrimWhitespace(title.InnerText);
                    break;
                }
            }
            // if no title, reject the record with an error message
            if (resource.Version.Title == null || resource.Version.Title == "")
            {
                VerifyResourceVersionRecordExists(resource);
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Program, "No title found for resource. Resource skipped.");
                return;
            }
            //If title does not meet minimum requirements, make record inactive and log it.
            string status = "OK";
            if (!IsGoodTitle(resource.Version.Title, ref status))
            {
                resource.Version.IsActive = false;
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Program, status);
            }

            list = payload.GetElementsByTagName("description");
            foreach (XmlNode node in list)
            {
                resource.Version.Description = CleanseDescription(TrimWhitespace(node.InnerText));
                break;
            }

            list = payload.GetElementsByTagName("publisher");
            foreach (XmlNode node in list)
            {
                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(node.OuterXml);
                XmlNodeList list2 = doc2.GetElementsByTagName("name");
                if (list2 != null && list2.Count > 0)
                {
                    resource.Version.Publisher = TrimWhitespace(list2[0].InnerText);
                }
                else
                {
                    resource.Version.Publisher = TrimWhitespace(node.InnerText);
                }
                break;
            }
            list = payload.GetElementsByTagName("author");
            foreach (XmlNode node in list)
            {
                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(node.OuterXml);
                XmlNodeList list2 = doc2.GetElementsByTagName("name");
                if (list2 != null && list2.Count > 0)
                {
                    resource.Version.Creator = TrimWhitespace(list2[0].InnerText);
                }
                else
                {
                    resource.Version.Creator = TrimWhitespace(node.InnerText);
                }
                break;
            }
            if ((resource.Version.Publisher == null || resource.Version.Publisher == "") &&
                (resource.Version.Creator == null || resource.Version.Creator == ""))
            {
                try
                {
                    Uri tempUri = new Uri(resource.ResourceUrl);
                    resource.Version.Publisher = resource.Version.Creator = tempUri.Host;
                }
                catch (Exception ex)
                {
                    resource.Version.Publisher = resource.Version.Creator = "Unknown";
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Warning, ErrorRouting.Technical, "Unknown publisher/bad URL");
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

            list = payload.GetElementsByTagName("useRightsUrl");
            foreach (XmlNode node in list)
            {
                if (node.InnerText.IndexOf("useRightsUrl") == -1)
                {
                    resource.Version.Rights = TrimWhitespace(node.InnerText);
                    break;
                }
            }

            // Access Rights - ISLE extension to LRMI
            list = payload.GetElementsByTagName("accessRestrictions");
            foreach (XmlNode node in list)
            {
                resource.Version.AccessRights = TrimWhitespace(node.InnerText);
                break;
            }

            list = payload.GetElementsByTagName("timeRequired");
            foreach (XmlNode node in list)
            {
                resource.Version.TypicalLearningTime = TrimWhitespace(node.InnerText);
                break;
            }

            list = payload.GetElementsByTagName("payload_schema");
            foreach (XmlNode node in list)
            {
                resource.Version.Schema = TrimWhitespace(node.InnerText);
                break;
            }

            list = payload.GetElementsByTagName("requires");
            foreach (XmlNode node in list)
            {
                resource.Version.Requirements = TrimWhitespace(node.InnerText);
                break;
            }

            list = payload.GetElementsByTagName("interactivityType");
            foreach (XmlNode node in list)
            {
                resource.Version.InteractivityType = TrimWhitespace(node.InnerText);
                resource.Version.InteractivityTypeId = 0;
                break;
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

                // Now do name/value pairs
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
                AddSubjects(resource, payload);
                AddStandards(resource, payload);
                string ageRange = "";
                bool hasAgeRange = false;
                bool hasGradeLevels = false;
                AddAgeRange(resource, payload, ref ageRange, ref hasAgeRange);
                string[] gradeLevels = AddGradeLevels(resource, payload, ref hasGradeLevels);
                if (hasAgeRange && !hasGradeLevels)
                {
                    ConvertAgeToGrade(resource, ageRange, ref status);
                }
                if (hasGradeLevels && !hasAgeRange)
                {
                    ConvertGradesToAgeRange(resource, gradeLevels);
                }
                AddAudiences(resource, payload);
                AddResourceFormats(resource, payload);
                AddResourceTypes(resource, payload);
                AddLanguages(resource, payload);
                AddEducationalUse(resource, payload);
                AddGroupType(resource, payload);
                AddItemType(resource, payload);
                AddAssessmentType(resource, payload);

                if (CheckForGoodLanguage(resource.Id) == true)
                {
                    // Add to Elastic Search Index
                    UpdateElasticSearchList(resource.Id);
                }
                else
                {
                    // Deactivate resource
                    resource.IsActive = false;
                    resourceManager.SetResourceActiveState(false, resource.Id);
                }
            }
            catch (Exception ex)
            {
            }
        }// LrmiMap

        protected void AddSubjects(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            int nbrSubjects = 0;
            int maxSubjects = int.Parse(ConfigurationManager.AppSettings["maxSubjectsToProcess"]);
            XmlNodeList list = payload.GetElementsByTagName("about");
            foreach (XmlNode node in list)
            {
                string subject = TrimWhitespace(node.InnerText);
                int amp = subject.IndexOf("&");
                int scolon = subject.IndexOf(";");
                if (scolon > -1 && amp == -1)
                {
                    // Subject contains ; but does not contain & (ie contains semicolon but no HTML entities), so split by semicolon
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
                                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId,
                                    resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
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
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId,
                            resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
            }
            if (nbrSubjects > maxSubjects)
            {
                string message = string.Format("Number of subjects exceeds {0}.  First {0} subjects processed, remainder ignored.", maxSubjects);
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                    ErrorType.Warning, ErrorRouting.Program, message);
            }
        }// AddSubjects

        protected void AddStandards(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("educationalAlignment");
            foreach (XmlNode node in list)
            {
                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(node.OuterXml);
                XmlNodeList list2 = doc2.GetElementsByTagName("targetName");
                foreach (XmlNode node2 in list2)
                {
                    string value = TrimWhitespace(node2.InnerText);
                    ResourceStandard standard = new ResourceStandard();
                    standard.ResourceId = resource.RowId;
                    standard.ResourceIntId = resource.Id;
                    if (value.Length > 300)
                    {
                        standard.StandardUrl = value.Substring(0, 300);
                    }
                    else
                    {
                        standard.StandardUrl = value;
                    }
                    // Get alignment type
                    XmlNodeList list3 = doc2.GetElementsByTagName("alignmentType");
                    if (list3 == null || list3.Count == 0)
                    {
                        standard.AlignmentTypeValue = "";
                    }
                    else
                    {
                        standard.AlignmentTypeValue = list3[0].InnerText;
                    }

                    // educationLevel denotes grade level, which is not a standard - skip educationLevels!
                    if (standard.AlignmentTypeValue.ToLower() != "educationlevel")
                    {

                        standardManager.Import(standard, ref status);
                        if (status != "successful")
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                                ErrorType.Error, ErrorRouting.Technical, status);
                        }
                    }
                }
            }
        }// AddStandards

        protected void AddAgeRange(Resource resource, XmlDocument payload, ref string ageRange, ref bool hasAgeRange)
        {
            string status = "successful";
            hasAgeRange = false;
            XmlNodeList list = payload.GetElementsByTagName("typicalAgeRange");
            foreach (XmlNode node in list)
            {
                ResourceAgeRange level = new ResourceAgeRange();
                level.ResourceIntId = resource.Id;
                level.OriginalValue = CleanUpAgeRange(TrimWhitespace(node.InnerText));
                int dashIndex = level.OriginalValue.IndexOf("-");
                int length = level.OriginalValue.Length;
                if (length >= 3)
                {
                    level.FromAge = int.Parse(level.OriginalValue.Substring(0, dashIndex));
                    level.ToAge = int.Parse(level.OriginalValue.Substring(dashIndex + 1, length - dashIndex - 1));
                    hasAgeRange = true;
                    status = ageRangeManager.Import(level);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId,
                            resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
            }
            ResourceAgeRange finalLevel = ageRangeManager.GetByIntId(resource.Id);
            ageRange = finalLevel.FromAge.ToString() + "-" + finalLevel.ToAge.ToString();
        }// AddAgeRange

        protected string[] AddGradeLevels(Resource resource, XmlDocument payload, ref bool hasGradeLevels)
        {
            string status = "successful";
            ArrayList grades = new ArrayList();
            hasGradeLevels = false;
            XmlNodeList list = payload.GetElementsByTagName("educationalAlignment");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc2 = new XmlDocument();
                xdoc2.LoadXml(node.OuterXml);
                XmlNodeList list2 = xdoc2.GetElementsByTagName("alignmentType");
                foreach (XmlNode node2 in list2)
                {
                    string alignmentType = TrimWhitespace(node2.InnerText);
                    if (alignmentType.ToLower() == "educationlevel")
                    {
                        XmlNodeList list3 = xdoc2.GetElementsByTagName("targetName");
                        if (list3.Count == 0)
                        {
                            list3 = xdoc2.GetElementsByTagName("targetDescription");
                        }
                        foreach (XmlNode node3 in list3)
                        {
                            hasGradeLevels = true;
                            ResourceChildItem level = new ResourceChildItem();
                            level.ResourceIntId = resource.Id;
                            level.OriginalValue = TrimWhitespace(node3.InnerText);
                            gradeLevelManager.Import(level, ref status);
                            if (status != "successful")
                            {
                                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical,
                                    status);
                            }
                            grades.Add(level.OriginalValue);
                        }
                    }
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

        }// AddGradeLevels

        protected void AddAudiences(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("educationalRole");
            foreach (XmlNode node in list)
            {
                ResourceChildItem audience = new ResourceChildItem();
                audience.ResourceId = resource.RowId;
                audience.ResourceIntId = resource.Id;
                audience.OriginalValue = TrimWhitespace(node.InnerText);
                audienceManager.Import(audience, ref status);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }// AddAudiences

        protected void AddResourceFormats(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("mediaType");
            foreach (XmlNode node in list)
            {
                ResourceChildItem format = new ResourceChildItem();
                format.ResourceId = resource.RowId;
                format.ResourceIntId = resource.Id;
                format.OriginalValue = TrimWhitespace(node.InnerText);
                formatManager.Import(format, ref status);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
                if (resource.Version.Requirements == "" && format.OriginalValue.Length > 20)
                {
                    resource.Version.Requirements = TrimWhitespace(node.InnerText);
                    versionManager.Update(resource.Version);
                }
            }
        }// AddResourceFormats

        protected void AddResourceTypes(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("learningResourceType");
            foreach (XmlNode node in list)
            {
                ResourceChildItem type = new ResourceChildItem();
                type.ResourceId = resource.RowId;
                type.ResourceIntId = resource.Id;
                type.OriginalValue = TrimWhitespace(node.InnerText);
                typeManager.Import(type, ref status);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }// AddResourceTypes

        protected void AddLanguages(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("inLanguage");
            foreach (XmlNode node in list)
            {
                if (node.InnerXml == null || node.InnerXml == "")
                {
                    ResourceChildItem language = new ResourceChildItem();
                    language.ResourceId = resource.RowId;
                    language.ResourceIntId = resource.Id;
                    language.OriginalValue = TrimWhitespace(node.InnerText);
                    languageManager.Import(language, ref status);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
                else
                {
                    XmlDocument xdoc2 = new XmlDocument();
                    xdoc2.LoadXml(node.OuterXml);
                    XmlNodeList list2 = xdoc2.GetElementsByTagName("name");
                    foreach (XmlNode node2 in list2)
                    {
                        ResourceChildItem language = new ResourceChildItem();
                        language.ResourceId = resource.RowId;
                        language.ResourceIntId = resource.Id;
                        language.OriginalValue = TrimWhitespace(node2.InnerText);
                        languageManager.Import(language, ref status);
                        if (status != "successful")
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                                ErrorType.Error, ErrorRouting.Technical, status);
                        }
                    }// foreach node2 in list2
                }
            }// foreach node in list

            resource.Language = languageManager.Select(resource.Id, "");
            if (resource.Language == null || resource.Language.Count == 0)
            {
                // If no languages found, assume English but log it.
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

        protected void AddEducationalUse(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("educationalUse");
            foreach (XmlNode node in list)
            {
                ResourceChildItem edUse = new ResourceChildItem();
                edUse.ResourceId = resource.RowId;
                edUse.ResourceIntId = resource.Id;
                edUse.OriginalValue = TrimWhitespace(node.InnerText);
                edUseManager.Import(edUse, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                        ErrorRouting.Technical, status);
                }
            }
        }// AddEducationalUse

        protected void AddGroupType(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("groupType");
            foreach (XmlNode node in list)
            {
                ResourceChildItem groupType = new ResourceChildItem();
                groupType.ResourceId = resource.RowId;
                groupType.ResourceIntId = resource.Id;
                groupType.OriginalValue = TrimWhitespace(node.InnerText);
                groupTypeManager.Import(groupType, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                        ErrorRouting.Technical, status);
                }
                status = "successful";
            }
        }// AddGroupType

        protected void AddItemType(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("itemType");
            foreach (XmlNode node in list)
            {
                ResourceChildItem itemType = new ResourceChildItem();
                itemType.ResourceId = resource.RowId;
                itemType.ResourceIntId = resource.Id;
                itemType.OriginalValue = TrimWhitespace(node.InnerText);
                status = itemTypeManager.Import(itemType);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                        ErrorRouting.Technical, status);
                }
                status = "successful";
            }
        }// AddItemType

        protected void AddAssessmentType(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            XmlNodeList list = payload.GetElementsByTagName("assessmentType");
            foreach (XmlNode node in list)
            {
                ResourceChildItem asmtType = new ResourceChildItem();
                asmtType.ResourceIntId = resource.Id;
                asmtType.OriginalValue = TrimWhitespace(node.InnerText);
                status = asmtTypeManager.Import(asmtType);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                        ErrorRouting.Technical, status);
                }
                status = "successful";
            }
        }// AddAssessmentType
    }
}
