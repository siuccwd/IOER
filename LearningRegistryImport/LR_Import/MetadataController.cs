using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
//using LearningRegistryCache2.App_Code.Classes;
using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using LR_Import;
using Newtonsoft.Json;

namespace LearningRegistryCache2
{
    public class MetadataController : BaseDataController
    {
        protected AuditReportingManager reportingManager = new AuditReportingManager();
        protected ResourceGradeLevelManager educationManager = new ResourceGradeLevelManager();
        protected ResourceFormatManager formatManager = new ResourceFormatManager();
        protected ResourceIntendedAudienceManager audienceManager = new ResourceIntendedAudienceManager();
        protected ResourceManager resourceManager = new ResourceManager();
        //protected ResourceVersionManager versionManager = new ResourceVersionManager();
        protected ResourcePropertyManager propertyManager = new ResourcePropertyManager();
        protected ResourceStandardManager standardManager = new ResourceStandardManager();
        protected ResourceSubjectManager subjectManager = new ResourceSubjectManager();
        protected ResourceTypeManager typeManager = new ResourceTypeManager();
        protected OLDDM.SubmitterManager submitterManager = new OLDDM.SubmitterManager();
        protected ResourceAgeRangeManager ageRangeManager = new ResourceAgeRangeManager();

        protected ResourceVersionController versionManager = new ResourceVersionController();

        public MetadataController()
        {
        }

        protected Resource LoadCommonMetadata(string docId, string url, string payloadPlacement, XmlDocument xdoc, XmlDocument xpayload, ref bool isValid)
        {
            Resource resource = new Resource();
            isValid = true;
            XmlNodeList list = null;
            string submitterName;
            string payload = "";
            list = xdoc.GetElementsByTagName("submitter");
            if (list.Count == 0)
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url,
                    ErrorType.Error, ErrorRouting.Technical, "Submitter not found.  Substituting 'Unknown'.");
                submitterName = "Unknown";
            }
            else
            {
                submitterName = TrimWhitespace(list[0].InnerText);
            }

            //Resource oldResource = versionManager.GetByResourceUrlAndSubmitter(url, submitterName);
            //string searchUrl = url.Replace("'", "''");
            Resource oldResource = versionManager.GetActiveVersionByResourceUrl(url);
            if (oldResource != null && oldResource.RowId.ToString() != BaseDataManager.DEFAULT_GUID)
            {
                resource = oldResource;
                ResourceVersion version = CopyOldToNewResourceVersion(resource.Version);
                new ResourceVersionManager().SetActiveState(false, resource.Version.Id);
                resource.Version = version;
                resource.Version.ResourceIntId = resource.Id;
                resource.Version.LRDocId = docId;
                resource.Version.Submitter = submitterName;
                //isOldVersion = true;
                LearningRegistry.deleteResourceIdList += string.Format("{0},", resource.Id);
            }
            else
            {
                //Get resource, if it exists, otherwise create it
                string status = "successful";
                oldResource = resourceManager.GetByResourceUrl(url, ref status);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, status);
                }
                if (oldResource == null || oldResource.RowId.ToString() == BaseDataManager.DEFAULT_GUID)
                {
                    status = "successful";
                    resource.ResourceUrl = url;
                    resource.FavoriteCount = 0;
                    resource.ViewCount = 0;
                    resourceManager.Create(resource, ref status);
                    resource.Version.ResourceIntId = resource.Id;

                    resource.Version.Submitter = submitterName;
                    //isOldVersion = false;
                }
                else
                {
                    resource = oldResource;
                    resource.Version.Submitter = submitterName;
                    resource.Version.ResourceIntId = resource.Id;
                    //isOldVersion = false;
                }
                resource.ResourceUrl = url;
                resource.Version.LRDocId = docId;
            }
            if (payloadPlacement == "inline")
            {
                list = xdoc.GetElementsByTagName("resource_data");
                if (list[0].InnerXml.IndexOf("<resource_data>") > -1)
                {
                    Regex resourceDataNodeEx = new Regex(@"<resource_data>([\s\S]*)</resource_data>");
                    payload = resourceDataNodeEx.Match(list[0].InnerXml).Value;
                    //payload = payload.Replace("<resource_data>", "");
                    //payload = payload.Replace("</resource_data>", "");
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
                payload = payload.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>","");
                //xpayload = new XmlDocument();
                try
                {
                    xpayload.LoadXml(payload);
                }
                catch (Exception ex)
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                    isValid = false;
                }
                list = xdoc.GetElementsByTagName("node_timestamp");
                foreach (XmlNode node in list)
                {
                    string strDate = TrimWhitespace(node.InnerText);
                    resource.Version.Modified = BaseDataManager.ConvertLRTimeToDateTime(strDate);
                    break;
                }
                list = xdoc.GetElementsByTagName("create_timestamp");
                foreach (XmlNode node in list)
                {
                    string strDate = TrimWhitespace(node.InnerText);
                    resource.Version.Created = BaseDataManager.ConvertLRTimeToDateTime(strDate);
                    break;
                }
                resource.Version.Imported = DateTime.Now;
                if (resource.Version.Created < DateTime.Parse("1900-01-01"))
                {
                    resource.Version.Created = resource.Version.Modified;
                }
                if (resource.Version.Modified < SqlDateTime.MinValue)
                {
                    resource.Version.Modified = (DateTime)SqlDateTime.MinValue;
                }
                if (resource.Version.Created < SqlDateTime.MinValue)
                {
                    resource.Version.Created = (DateTime)SqlDateTime.MinValue;
                }
            }
            else
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, "Payload placement is not \"inline\".");
                isValid = false;
            }

            return resource;
        }

        private ResourceVersion CopyOldToNewResourceVersion(ResourceVersion version)
        {
            ResourceVersion vers = new ResourceVersion();
            vers.Id = 0;
            vers.ResourceIntId = version.ResourceIntId;
            vers.Title = version.Title;
            vers.Description = version.Description;
            vers.LRDocId = version.LRDocId;
            vers.Publisher = version.Publisher;
            vers.Creator = version.Creator;
            vers.Rights = version.Rights;
            vers.AccessRights = version.AccessRights;
            vers.Modified = version.Modified;
            vers.Submitter = version.Submitter;
            vers.Imported = version.Imported;
            vers.Created = version.Created;
            vers.TypicalLearningTime = version.TypicalLearningTime;
            vers.IsSkeletonFromParadata = false;
            vers.IsActive = true;
            vers.Requirements = version.Requirements;
            vers.SortTitle = version.SortTitle;
            vers.Schema = version.Schema;
            vers.AccessRightsId = version.AccessRightsId;
            vers.InteractivityTypeId = version.InteractivityTypeId;
            vers.InteractivityType = version.InteractivityType;

            return vers;
        }


        public void AddResource(Resource resource)
        {
            string statusMessage = "successful";
            resource.Version.ResourceIntId = resource.Id;
            string resourceId = versionManager.Create(resource.Version, ref statusMessage);
            if (statusMessage != "successful")
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, statusMessage);
            }

            // Thumbnail generation is now done in a separate process.  This code is no longer needed.
            /*ManualResetEvent doneEvent = new ManualResetEvent(false);
            doneEvents.Add(doneEvent);
            ImageGenerator imageGenerator = new ImageGenerator(resource.ResourceUrl, resource.Id, doneEvent);
            ThreadPool.QueueUserWorkItem(imageGenerator.ImageGeneratorThreadPoolCallback, doneEvent);*/
        }

        public void UpdateResource(Resource resource)
        {
            string statusMessage = "successful";
            resource.Version.ResourceIntId = resource.Id;
            statusMessage = versionManager.Update(resource.Version);
            if (statusMessage != "successful")
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, statusMessage);
            }
        }


        public bool SkipSubject(string subject)
        {
            bool retVal = false;
            if (subject.Length > 1 && subject.Substring(0, 1) == "\"" && subject.Substring(subject.Length - 1, 1) == "\"")
            {
                // subject begins and ends with a quotation mark - check for internal quotes.  Note that stripping quotes requires the subject to be at least 2 characters long.
                string tSubject = subject.Substring(1, subject.Length - 2);
                if (tSubject.IndexOf("\"") == -1)
                {
                    // no internal quotes found.  Strip all quotes from subject.
                    subject = subject.Replace("\"", "");
                }
            }

            if (subject.Length == 0)
            {
                retVal = true;
            }
            if (subject.IndexOf("http:") > -1 || subject.IndexOf("https:") > -1)
            {
                retVal = true;
            }

            return retVal;
        }

        public string ApplySubjectEditRules(string subject)
        {
            if (subject.Length > 1 && subject.Substring(0, 1) == "\"" && subject.Substring(subject.Length - 1, 1) == "\"")
            {
                // subject begins and ends with a quotation mark - check for internal quotes.  Note that stripping quotes requires the subject to be at least 2 characters long.
                string tSubject = subject.Substring(1, subject.Length - 2);
                if (tSubject.IndexOf("\"") == -1)
                {
                    // no internal quotes found.  Strip all quotes from subject.
                    subject = subject.Replace("\"", "");
                }
            }

            return subject;
        }

        public void VerifyResourceVersionRecordExists(Resource resource)
        {
            string status = "successful";
            DataSet ds = versionManager.GetByResourceUrl(resource.ResourceUrl);

            if (ResourceVersionManager.DoesDataSetHaveRows(ds))
            {
                // do nothing - it is verified
            }
            else
            {
                // Resource record exists without Resource.Version.  Attempt to delete the Resource record.
                status = resourceManager.Delete(resource.RowId.ToString());
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }

        protected string CleanseDescription(string description)
        {
            description = description.Replace("&amp;", "&");
            description = description.Replace("\\r\\n", " ");
            description = description.Replace("\r\n", " ");
            description = description.Replace("\\t", " ");
            description = description.Replace("\t", " ");

            Regex stripHtml = new Regex(@"&lt;\S(.*?)&gt;");
            description = stripHtml.Replace(description, "");

            return description;
        }


        protected string CleanUpAgeRange(string input)
        {
            Regex whiteSpace = new Regex(@"\s+");           // Regex for removing all whitespace from string
            Regex leadingDash = new Regex(@"^-+");          // Regex for removing leading "-" characters
            Regex multiDash = new Regex(@"--+");            // Regex for removing multiple "-" characters anywhere in the string (there must be at least two)
            Regex twoNumbers = new Regex(@"[0-9]-[0-9U]");  // Regex for testing if two numbers separated by a "-" are present
            Regex nonEndingPlus = new Regex(@"\+(?!$)");    // Regex for matching a "+" that is not at the end of the string
            Regex endingPlus = new Regex(@"\+$");           // Regex for matching a "+" that is at the end of the string so we can replace with "-99"
            Regex trailingDash = new Regex(@"-+$");         // Regex for removing trailing dashes
            Regex multipleNines = new Regex(@"99+");        // Regex for matching two or more 9's
            Regex gtAge = new Regex(@"^>[0-9]*");           // Regex for ages greater than a certain age
            Regex ltAge = new Regex(@"^<[0-9]*");           // Regex for ages less than a certain age
            string[] monthAbbrevs = { "", "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };

            Match match;

            // Remove whitespace
            string output = whiteSpace.Replace(input, "");
            // If equal to "U-" change to "0-99"
            if (output == "U-")
            {
                output = "0-99";
            }
            // Convert month abbreviations to numbers
            for (int i = 1; i <= 12; i++)
            {
                Regex monthAbbrev = new Regex(monthAbbrevs[i], RegexOptions.IgnoreCase);
                output = monthAbbrev.Replace(output, i.ToString());
            }
            // Replace HTML entities for > and < with > and <
            output = output.Replace("&gt;", ">");
            output = output.Replace("&lt;", "<");
            // Convert ">age" to "age-99"
            match = gtAge.Match(output);
            if (match.Value != string.Empty)
            {
                output = output.Substring(match.Index + 1, match.Length - 1) + "-99";
            }
            // Convert "<age" to "0-age"
            match = ltAge.Match(output);
            if (match.Value != string.Empty)
            {
                output = "0-" + output.Substring(match.Index + 1, match.Length - 1);
            }
            // Change + to - if it is not the last character in the string
            output = nonEndingPlus.Replace(output, "-");
            // Change ending + to "-99"
            output = endingPlus.Replace(output, "-99");
            // change two or more 9's to 99
            output = multipleNines.Replace(output, "99");
            // Strip leading dashes
            output = leadingDash.Replace(output, "");
            // Strip multiple dashes within the string
            output = multiDash.Replace(output, "-");
            // If there are two numbers, strip trailing dashes
            match = twoNumbers.Match(output);
            if (match.Value != string.Empty)
            {
                output = trailingDash.Replace(output, "");
            }
            else
            {
                // There are not two numbers, replace trailing dash with -99
                output = trailingDash.Replace(output, "-99");
                // By this point there should be two numbers separated by a dash.  If not, then make it a range with only 1 number
                if (output.IndexOf("-") == -1)
                {
                    // No dash found
                    output = output + "-" + output;
                }
            }
            // Replace -U with -99
            output = output.Replace("-U", "-99");

            // Make sure ages are in correct order (for example, convert "99-14" to "14-99")
            // Also drop leading zeroes.
            try
            {
                string[] ages = output.Split('-');
                int age1 = int.Parse(ages[0]);
                int age2 = int.Parse(ages[1]);
                if (age1 > age2)
                {
                    output = age2.ToString() + "-" + age1.ToString();
                }
                else
                {
                    output = age1.ToString() + "-" + age2.ToString();
                }
            }
            catch (Exception ex)
            {
                //Ignore
            }

            return output;
        }

        /// <summary>
        /// Crosswalk Age Ranges to Grade Levels where possible.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="age"></param>
        /// <param name="status"></param>
        protected void ConvertAgeToGrade(Resource resource, string age, ref string status)
        {
            status = "successful";
            ResourceGradeLevelManager gradeLevelManager = new ResourceGradeLevelManager();

            ResourceAgeRange level = new ResourceAgeRange();
            level.ResourceIntId = resource.Id;
            level.OriginalValue = age;
            int dashIndex = level.OriginalValue.IndexOf("-");
            int length = level.OriginalValue.Length;
            level.FromAge = int.Parse(level.OriginalValue.Substring(0, dashIndex));
            level.ToAge = int.Parse(level.OriginalValue.Substring(dashIndex + 1, length - dashIndex - 1));
            if (level.ToAge > 21)
            {
                // After high school, age range to grade level tends to break down.  Map to "Adult Education."
                ResourceChildItem generalPublic = new ResourceChildItem();
                generalPublic.ResourceIntId = resource.Id;
                generalPublic.OriginalValue = "Adult Education";
                gradeLevelManager.Import(generalPublic, ref status);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical,
                        status);
                }
                if (level.FromAge > 13)
                {
                    // It's adult ed and high school, so let's do high school too
                    ResourceChildItem highSchool = new ResourceChildItem();
                    highSchool.ResourceIntId = resource.Id;
                    highSchool.OriginalValue = "High School";
                    status = "successful";
                    gradeLevelManager.Import(highSchool, ref status);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical,
                            status);
                    }
                }
            }

            // If it's not Adult Ed, don't try to outsmart age ranges - take it as whatever it's tagged.
            else
            {
                // Map to grade levels
                CodeGradeLevelCollection grades = CodeTableManager.GradeLevelGetByAgeRange(level.FromAge, level.ToAge, false, ref status);
                foreach (CodeGradeLevel grade in grades)
                {
                    ResourceChildItem gradeLevel = new ResourceChildItem();
                    gradeLevel.ResourceIntId = resource.Id;
                    gradeLevel.OriginalValue = grade.Title;
                    gradeLevel.CodeId = grade.Id;
                    gradeLevelManager.Import(gradeLevel, ref status);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical,
                            status);
                    }
                }
            }
        }// ConvertAgeToGrade

        protected void ConvertGradesToAgeRange(Resource resource, string[] grades)
        {
            bool k12GradeLevelPresent = false;
            foreach (string title in grades)
            {
                if (title.ToLower().IndexOf("pre-k") > -1)
                {
                    k12GradeLevelPresent = true;
                    break;
                }
                if (title.ToLower().IndexOf("kindergarten") > -1)
                {
                    k12GradeLevelPresent = true;
                    break;
                }
                if (title.ToLower().IndexOf("grade") > -1)
                {
                    k12GradeLevelPresent = true;
                    break;
                }
            }
            if (!k12GradeLevelPresent)
            {
                // If there's no PreK-12, do not convert
                return;
            }
            bool firstTimeThru = true;
            ResourceAgeRange gradeLevel = new ResourceAgeRange();
            foreach (string title in grades)
            {
                CodeGradeLevel level = CodeTableManager.GradeLevelGetByTitle(title);

                gradeLevel.ResourceIntId = resource.Id;
                if (gradeLevel.FromAge == 0)
                {
                    gradeLevel.FromAge = level.FromAge;
                }
                if (gradeLevel.ToAge == 0)
                {
                    gradeLevel.ToAge = level.ToAge;
                }
                if (level.FromAge < gradeLevel.FromAge)
                {
                    gradeLevel.FromAge = level.FromAge;
                    firstTimeThru = false;
                }
                if (level.ToAge > gradeLevel.ToAge)
                {
                    gradeLevel.ToAge = level.ToAge;
                    firstTimeThru = false;
                }
            }
            string status = ageRangeManager.Import(gradeLevel);
            if (status != "successful")
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical,
                    status);
            }
        }// ConvertGradesToAgeRange

        protected bool CheckForGoodLanguage(int resourceIntId)
        {
            bool isGoodLanguage = true;
            ResourceLanguageManager languageManager = new ResourceLanguageManager();
            List<ResourceChildItem> languages = languageManager.Select(resourceIntId);
            DataSet goodLanguages = languageManager.SelectCodeTable();
            if (languages.Count > 0)
            {
                // At least one language is present.  Check for English, Spanish, Polish, Chinese or Russian.  If one of these is present, the language is good.
                bool goodLanguageFound = false;
                foreach (ResourceChildItem language in languages)
                {
                    DataView dv = goodLanguages.Tables[0].DefaultView;
                    dv.RowFilter = "Id = " + language.CodeId.ToString();
                    if (dv.Count > 0)
                    {
                        goodLanguageFound = true;
                        break;
                    }
                }
                isGoodLanguage = goodLanguageFound;
            }

            return isGoodLanguage;
        }


    }
}
