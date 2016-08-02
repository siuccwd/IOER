using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using Isle.BizServices;
using LRWarehouse.Business;


namespace LearningRegistryCache2
{
    public class LrmiHandler : MetadataController
    {
        private AuditReportingServices auditReportingManager;
        private LR_Import.ResourceVersionController versionManager;

        public LrmiHandler()
        {
            auditReportingManager = new AuditReportingServices();
            versionManager = new LR_Import.ResourceVersionController();
        }


        public bool LrmiMap(string docId, string url, string payloadPlacement, XmlDocument record)
        {

            // Begin common logic for all metadata schemas
            XmlDocument payload = new XmlDocument();
            bool isValid = true;
            string title = "";
            string description = "";

            Resource resource = LoadCommonMetadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
            if (!isValid)
            {
                // Check to see if Resource.Version record exists.  If not, remove it.  Then skip this record
                VerifyResourceVersionRecordExists(resource);
                return false;
            }
            // End common logic for all metadata schemas


            XmlNodeList list = payload.GetElementsByTagName("name");
            foreach (XmlNode titleNode in list)
            {
                XmlDocument doc2 = new XmlDocument();
                XmlNodeList list2 = null;
                if (titleNode.ChildNodes != null && titleNode.ChildNodes.Count > 0)
                {
                    doc2.LoadXml("<root>" + titleNode.InnerXml + "</root>");
                    list2 = doc2.GetElementsByTagName("name");
                }
                if (titleNode.ParentNode.Name.ToLower() == "resource_data")
                {
                    if (list2 != null && list2.Count > 0)
                    {
                        title = TrimWhitespace(list2[0].InnerText);
                    }
                    else
                    {
                        title = TrimWhitespace(titleNode.InnerText);
                    }
                    if (title.Length > resource.Version.Title.Length)
                    {
                        resource.Version.Title = StripTrailingPeriod(title);
                    }
                    break;
                }
                else if (titleNode.ParentNode.Name.ToLower() == "items")
                {
                    if (list2 != null && list2.Count > 0)
                    {
                        title = TrimWhitespace(list2[0].InnerText);
                    }
                    else
                    {
                        title = TrimWhitespace(titleNode.InnerText);
                    }
                    if (title.Length > resource.Version.Title.Length)
                    {
                        resource.Version.Title = StripTrailingPeriod(title);
                    }
                    break;
                }
                else if (titleNode.ParentNode.Name.ToLower() == "properties" &&
                  (titleNode.ParentNode.ParentNode.Name.ToLower() == "items" ||
                   titleNode.ParentNode.ParentNode.Name.ToLower() == "resource_data"))
                {
                    if (list2 != null && list2.Count > 0)
                    {
                        title = TrimWhitespace(list2[0].InnerText);
                    }
                    else
                    {
                        title = TrimWhitespace(titleNode.InnerText);
                    }
                    if (title.Length > resource.Version.Title.Length)
                    {
                        resource.Version.Title = StripTrailingPeriod(title);
                    }
                    break;
                }
            }
            // if no title, reject the record with an error message
            if (resource.Version.Title == null || resource.Version.Title == "")
            {
                VerifyResourceVersionRecordExists(resource);
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Program, "No title found for resource. Resource skipped.");
                return false;
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
                description = CleanseDescription(TrimWhitespace(node.InnerText));
                if (description.Length > resource.Version.Description.Length)
                {
                    resource.Version.Description = description;
                }
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
            if (list.Count == 0)
            {
                list = payload.GetElementsByTagName("accessRights");
            }
            foreach (XmlNode node in list)
            {
                string accessRights = TrimWhitespace(node.InnerText);
                string rvAccessRights = string.Empty;
                resource.Version.AccessRightsId = importManager.MapAccessRights(accessRights, ref rvAccessRights);
                resource.Version.AccessRights = rvAccessRights;
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
                if (resource.Version.RowId == null || resource.Version.RowId == new Guid(ImportServices.DEFAULT_GUID))
                {
                    if (AddResource(resource) == false)
                    {
                        //REJECT WHOLE RESOURCE!!! and end. Need to add to a summary count
                        VerifyResourceVersionRecordExists(resource);
                        return false;
                    }
                }
                else
                {
                    UpdateResource(resource);
                }

                // Now do name/value pairs
                AddKeywords(resource, record, payload, ref status);
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
                AddAccessibility(resource, payload);
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
                    LearningRegistry.resourceIdList.Add(resource.Id);
                }
                else
                {
                    // Deactivate resource
                    resource.IsActive = false;
                    importManager.ResourceSetActiveState( resource.Id, false );
                }
            }
            catch (Exception ex)
            {
            }

            return isValid;
        }// LrmiMap

        protected void AddKeywords(Resource resource, XmlDocument record, XmlDocument payload, ref string status)
        {
            string internalStatus = "";
            base.AddKeywords(resource, record, "keys", ref internalStatus);
            status += internalStatus;
            internalStatus = "";
            base.AddKeywords(resource, payload, "keywords", ref internalStatus);
            status += internalStatus;
        }

        protected void AddAccessibility(Resource resource, XmlDocument payload)
        {
            string status = "successful";

            // Accessibility Features
            XmlNodeList list = payload.GetElementsByTagName("accessibilityFeature");
            foreach (XmlNode node in list)
            {
                ResourceChildItem feature = new ResourceChildItem();
                feature.ResourceIntId = resource.Id;
                feature.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportAccessibilityFeature(feature);
            }

            // Accessibility Hazards
            list = payload.GetElementsByTagName("accessibilityHazard");
            foreach (XmlNode node in list)
            {
                ResourceAccessibilityHazard hazard = new ResourceAccessibilityHazard();
                hazard.ResourceIntId = resource.Id;
                hazard.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportAccessibilityHazard(hazard);
            }

            // Accessibility APIs
            list = payload.GetElementsByTagName("accessibilityAPI");
            foreach (XmlNode node in list)
            {
                ResourceChildItem api = new ResourceChildItem();
                api.ResourceIntId = resource.Id;
                api.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportAccessibilityApi(api);
            }

            // Accessibility Controls
            list = payload.GetElementsByTagName("accessibilityControl");
            foreach (XmlNode node in list)
            {
                ResourceChildItem control = new ResourceChildItem();
                control.ResourceIntId = resource.Id;
                control.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportAccessibilityControl(control);
            }
        }

        protected void AddSubjects(Resource resource, XmlDocument payload)
        {
            string status = "successful";
            int nbrSubjects = 0;
            int maxSubjects = int.Parse(ConfigurationManager.AppSettings["maxSubjectsToProcess"]);

            // Process subjects from the schema.org "about" property.
            XmlNodeList list = payload.GetElementsByTagName("about");
            XmlDocument doc2 = new XmlDocument();
            XmlNodeList list2 = null;
            foreach (XmlNode node in list)
            {
                if (node.ChildNodes != null && node.ChildNodes.Count > 0)
                {
                    doc2.LoadXml("<root>" + node.InnerXml + "</root>");
                    list2 = doc2.GetElementsByTagName("name");
                }
                string subject = "";
                if (list2 != null && list2.Count > 0)
                {
                    subject = TrimWhitespace(list2[0].InnerText);
                }
                else
                {
                    subject = TrimWhitespace(node.InnerText);
                }
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
                            resSubject.ResourceIntId = resource.Id;
                            resSubject.Subject = ApplySubjectEditRules(subjectItem);
                            resSubject.Subject = TrimWhitespace(resSubject.Subject);
                            status = importManager.CreateSubject(resSubject);
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
                    resSubject.ResourceIntId = resource.Id;
                    resSubject.Subject = subject;
                    status = importManager.CreateSubject(resSubject);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId,
                            resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
            }

            // Process subjects from the LRMI 1.1 alignmentObject property
            list = payload.GetElementsByTagName("educationalAlignment");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc2 = new XmlDocument();
                xdoc2.LoadXml(node.OuterXml);
                list2 = xdoc2.GetElementsByTagName("alignmentType");
                foreach (XmlNode node2 in list2)
                {
                    string alignmentType = TrimWhitespace(node2.InnerText);
                    if (alignmentType.ToLower() == "educationalsubject")
                    {
                        XmlNodeList list3 = xdoc2.GetElementsByTagName("targetName");
                        if (list3.Count == 0)
                        {
                            list3 = xdoc2.GetElementsByTagName("targetDescription");
                        }
                        foreach (XmlNode node3 in list3)
                        {
                            string subject = TrimWhitespace(node3.InnerText);
                            int amp = subject.IndexOf("&");
                            int scolon = subject.IndexOf(";");
                            if (scolon > -1 && amp == -1)
                            {
                                // Subject contains ';' but does not contain '&' (ie contains semicolon but no HTML entities), so split by ';'
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
                                        resSubject.ResourceIntId = resource.Id;
                                        resSubject.Subject = ApplySubjectEditRules(subjectItem);
                                        resSubject.Subject = TrimWhitespace(resSubject.Subject);
                                        status = importManager.CreateSubject(resSubject);
                                        if (status != "successful")
                                        {
                                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
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
                                resSubject.ResourceIntId = resource.Id;
                                resSubject.Subject = subject;
                                status = importManager.CreateSubject(resSubject);
                                if (status != "successful")
                                {
                                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                                        ErrorType.Error, ErrorRouting.Technical, status);
                                }
                                nbrSubjects++;
                            }
                        }
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
            int maxDetailStandards = 0;
            if (!int.TryParse(ConfigurationManager.AppSettings["maxDetailedStandardsToProcess"], out maxDetailStandards))
            {
                maxDetailStandards = 25;
            }
            if (maxDetailStandards == 0)
            {
                maxDetailStandards = int.MaxValue;
            }

            int nbrStandards = CountStandards(payload);
            bool hasTooManyStandards = false;
            if (nbrStandards > maxDetailStandards)
            {
                hasTooManyStandards = true;
            }
            XmlNodeList list = payload.GetElementsByTagName("educationalAlignment");
            foreach (XmlNode node in list)
            {
                XmlDocument doc2 = new XmlDocument();
                doc2.LoadXml(node.OuterXml);

                XmlNodeList list2 = doc2.GetElementsByTagName("targetUrl");
                string targetUrl = "";
                foreach (XmlNode node2 in list2)
                {
                    string value = TrimWhitespace(node2.InnerText);
                    if (value != null && value.Substring(0, 4).ToLower() == "http")
                    {
                        targetUrl = value;
                        break;
                    }
                }

                XmlNodeList list3 = doc2.GetElementsByTagName("targetName");
                string targetName = "";
                foreach (XmlNode node3 in list3)
                {
                    string value = TrimWhitespace(node3.InnerText);
                    if (value != null && value != string.Empty)
                    {
                        targetName = value;
                        break;
                    }
                }

                XmlNodeList list4 = doc2.GetElementsByTagName("alignmentType");
                string alignmentType = "";
                foreach (XmlNode node4 in list4)
                {
                    string value = TrimWhitespace(node4.InnerText);
                    if (value != null && value != string.Empty)
                    {
                        alignmentType = value;
                        break;
                    }
                }

                ProcessStandard(resource, targetUrl, targetName, alignmentType, hasTooManyStandards);
            }// foreach
        }// AddStandards

        protected int CountStandards(XmlDocument payload)
        {
            int nbrStandards = 0;
            XmlNodeList list = payload.GetElementsByTagName("educationalAlignment");
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(node.OuterXml);
                XmlNodeList list2 = xdoc.GetElementsByTagName("alignmentType");
                foreach (XmlNode node2 in list2)
                {
                    string value = TrimWhitespace(node2.InnerText);
                    if (value == "teaches" || value == "assesses" || value == "requires" || value == "")
                    {
                        nbrStandards++;
                    }
                }
            }

            return nbrStandards;
        }

        protected void ProcessStandard(Resource resource, string targetUrl, string targetName, string alignmentType, bool processToDomain)
        {
            if (processToDomain && alignmentType.ToLower() != "educationlevel")
            {
                GetStandardsDomain(ref targetUrl, ref targetName);
            }
            string status = "successful"; 
            ResourceStandard standard = new ResourceStandard();
            standard.ResourceIntId = resource.Id;
            if (targetUrl.Length > 300)
            {
                standard.StandardUrl = targetUrl.Substring(0, 300);
            }
            else
            {
                standard.StandardUrl = targetUrl;
            }

            if (targetName.Length > 300)
            {
                standard.StandardNotationCode = targetName.Substring(0, 300);
            }
            else
            {
                standard.StandardNotationCode = targetName;
            }

            standard.AlignmentTypeValue = alignmentType;

            // educationLevel denotes grade level, which is not a standard - skip educationLevels!
            if (standard.AlignmentTypeValue.ToLower() != "educationlevel")
            {

                importManager.ImportStandard(standard, ref status);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }

        protected void GetStandardsDomain(ref string targetUrl, ref string targetName)
        {
            StandardItem si = importManager.StandardItem_Get(0, "", targetName, "");
            if (si == null)
            {
                si = importManager.StandardItem_Get(0, targetUrl, "", "");
                if (si == null)
                {
                    si = importManager.StandardItem_Get(0, "", "", targetUrl);
                }
            }
            if (si != null)
            {
                StandardItem itemToTest = importManager.StandardItem_Get(si.Id);
                while (itemToTest.ParentId != null && (itemToTest.LevelType.ToLower() != "domain" && itemToTest.LevelType.ToLower() != "domain?"))
                {
                    itemToTest = importManager.StandardItem_Get(itemToTest.ParentId);
                }
                if (itemToTest.IsValid && (itemToTest.LevelType.ToLower() == "domain" || itemToTest.LevelType.ToLower() == "domain?"))
                {
                    targetUrl = "";
                    targetName = itemToTest.NotationCode;
                }
/*                string[] dotNotationParts = si.NotationCode.Split('.');
                // Search for domain
                string domainNotation = "";
                bool foundDomain = false;
                for (int i = 0; i < dotNotationParts.Length; i++)
                {
                    domainNotation = "";
                    for (int j = 0; j < (dotNotationParts.Length - i); j++) 
                    {
                        domainNotation += dotNotationParts[j] + ".";
                    }
                    domainNotation = domainNotation.Substring(0, domainNotation.Length - 1);
                    StandardItem itemToTest = sdm.StandardItem_Get(0, "", domainNotation, "");
                    if (itemToTest.IsValid && (itemToTest.LevelType.ToLower() == "domain" || itemToTest.LevelType.ToLower() == "domain?"))
                    {
                        targetUrl = "";
                        targetName = domainNotation;
                        foundDomain = true;
                        break;
                    }
                    if (foundDomain)
                    {
                        break;
                    }
                } */
            }
        }

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
                    status = importManager.ImportAgeRange(level);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId,
                            resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
            }
            ResourceAgeRange finalLevel = importManager.GetAgeRangeByIntId(resource.Id);
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
                            string[] gradez = TrimWhitespace(node3.InnerText).Split(',');
                            foreach (string grade in gradez)
                            {
                                ResourceChildItem level = new ResourceChildItem();
                                level.ResourceIntId = resource.Id;
                                level.OriginalValue = TrimWhitespace(grade);
                                importManager.ImportGradeLevel(level, ref status);
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
                audience.ResourceIntId = resource.Id;
                audience.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportAudience(audience, ref status);
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
                format.ResourceIntId = resource.Id;
                format.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportFormat(format, ref status);
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
                type.ResourceIntId = resource.Id;
                type.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportType(type, ref status);
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
            if (list == null || list.Count == 0)
            {
                list = payload.GetElementsByTagName("language");
            }
            foreach (XmlNode node in list)
            {
                XmlDocument xdoc2 = new XmlDocument();
                xdoc2.LoadXml(node.OuterXml);
                XmlNodeList list2 = xdoc2.GetElementsByTagName("name");
                if (list2.Count == 0)
                {
                    ResourceChildItem language = new ResourceChildItem();
                    language.ResourceIntId = resource.Id;
                    language.OriginalValue = TrimWhitespace(node.InnerText);
                    importManager.ImportLanguage(language, ref status);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
                else
                {
                    foreach (XmlNode node2 in list2)
                    {
                        ResourceChildItem language = new ResourceChildItem();
                        language.ResourceIntId = resource.Id;
                        language.OriginalValue = TrimWhitespace(node2.InnerText);
                        importManager.ImportLanguage(language, ref status);
                        if (status != "successful")
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl,
                                ErrorType.Error, ErrorRouting.Technical, status);
                        }
                    }// foreach node2 in list2
                }
            }// foreach node in list

            resource.Language = importManager.SelectLanguage(resource.Id, "");
            if (resource.Language == null || resource.Language.Count == 0)
            {
                // If no languages found, assume English but log it.
                ResourceChildItem language = new ResourceChildItem();
                language.ResourceIntId = resource.Id;
                language.OriginalValue = "English";
                importManager.ImportLanguage(language, ref status);
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
                edUse.ResourceIntId = resource.Id;
                edUse.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportEducationalUse(edUse, ref status);
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
                groupType.ResourceIntId = resource.Id;
                groupType.OriginalValue = TrimWhitespace(node.InnerText);
                importManager.ImportGroupType(groupType, ref status);
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
                itemType.ResourceIntId = resource.Id;
                itemType.OriginalValue = TrimWhitespace(node.InnerText);
                status = importManager.ImportItemType(itemType);
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
                status = importManager.ImportAssessmentType(asmtType);
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
