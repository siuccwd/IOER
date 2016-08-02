using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using LRWarehouse.Business;
using LearningRegistryCache2.App_Code.Classes;
using Isle.BizServices;

namespace LearningRegistryCache2
{
    public class BaseDataController
    {
        private List<ProhibitedKeyword> prohibitedKeywords;
        protected ImportServices importManager;
        public ResourceBizService resourceBizService;
        private AuditReportingServices auditReportingManager;

        public static List<ManualResetEvent> doneEvents = new List<ManualResetEvent>(1000);

        public BaseDataController()
        {
            importManager = new ImportServices();
            resourceBizService = new ResourceBizService();
            auditReportingManager = new AuditReportingServices();

            string status = "successful";
            prohibitedKeywords = importManager.GetProhibitedKeywordRules(ref status);
            
        }

        #region === Keyword Helper Methods ===
        /// <summary>
        /// Adds one or more keywords to a resource
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="record"></param>
        /// <param name="tagName"></param>
        public void AddKeywords(Resource resource, XmlDocument record, string tagName, ref string message)
        {
            message = "";
            int nbrKeywords = 0;
            int maxKeywords = int.Parse(ConfigurationManager.AppSettings["maxKeywordsToProcess"].ToString()); // Speed processing - some people are submitting hundreds of keywords!!!
            bool isProhibitedKeyword = false;
            XmlNodeList list = record.GetElementsByTagName(tagName);
            List<string> keywordSubstrings = prohibitedKeywords.Where(x => x.IsRegex == false).Select(x => x.Keyword).ToList();
            List<string> keywordExpressionStrings = prohibitedKeywords.Where(x => x.IsRegex == true).Select(x => x.Keyword).ToList();

            foreach (XmlNode node in list)
            {
                string keyword = node.InnerText.Trim();

                if (keywordSubstrings.Any(x => keyword.IndexOf(x) > -1))
                {
                    isProhibitedKeyword = true;
                }
                if (!isProhibitedKeyword)
                {
                    foreach (string keywordExpression in keywordExpressionStrings)
                    {
                        Regex rex = new Regex(keywordExpression, RegexOptions.IgnoreCase);
                        if (rex.IsMatch(keyword))
                        {
                            isProhibitedKeyword = true;
                            break;
                        }
                    }
                }

                if (!isProhibitedKeyword)
                {
                    nbrKeywords++;
                    if (maxKeywords > 0 && nbrKeywords > maxKeywords)
                    {
                        message += string.Format("Number of keywords exceeds {0}.  First {0}/{1} keywords processed, remainder ignored. ", maxKeywords, list.Count);
                        break;
                    }

                    AddKeyword(resource, keyword);
                }
            }
        }

        /// <summary>
        /// Adds a single keyword to the database
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="keyword"></param>
        public void AddKeyword(Resource resource, string keyword)
        {
            string status = "successful";
            int amp = keyword.IndexOf("&");
            int scolon = keyword.IndexOf(";");
            if (scolon > -1 && amp == -1)
            {
                // key contains ";" but does not contain "&" (ie contains semicolon but no HTML entities), so split by semicolon
                string[] keyList = keyword.Split(';');
                foreach (string keyItem in keyList)
                {
                    if (!SkipKey(keyItem))
                    {
                        string key = ApplyKeyEditRules(keyItem);
                        ResourceChildItem keys = new ResourceChildItem();
                        //keys.ResourceId = resource.RowId;
                        keys.ResourceIntId = resource.Id;
                        keys.OriginalValue = key;
                        importManager.CreateKeyword(keys, ref status);
                        if (status != "successful")
                        {
                            auditReportingManager.LogMessage(LearningRegistry.reportId, "", resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
                        }
                    }
                }
            }
            else if (!SkipKey(keyword))
            {
                keyword = ApplyKeyEditRules(keyword);
                ResourceChildItem keys = new ResourceChildItem();
                //keys.ResourceId = resource.RowId;
                keys.ResourceIntId = resource.Id;
                keys.OriginalValue = keyword;
                importManager.CreateKeyword(keys, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, "", resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, status);
                }
            }
        }// AddKeyword

        /// <summary>
        /// Checks to see if a keyword should be skipped.  Returns true if keyword should be skipped.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private bool SkipKey(string keyword)
        {
            bool retVal = false;
            if (keyword.Length > 2 && keyword.Substring(0, 1) == "\"" && keyword.Substring(keyword.Length - 1, 1) == "\"")
            {
                // keyword begins and ends with a quotation mark.  Check for internal quotes.  Note that stripping quotes requires the string to be at least 2 characters long.
                string tKey = keyword.Substring(1, keyword.Length - 2);
                if (tKey.IndexOf("\"") == -1)
                {
                    // no internal quotes found, strip quotes
                    keyword = keyword.Replace("\"", "");
                }
            }
            if (keyword.Length == 0)
            {
                retVal = true;
            }
            if (keyword.IndexOf("http:") > -1 || keyword.IndexOf("https:") > -1)
            {
                retVal = true;
            }
            if (keyword == "LRE" || keyword == "LRE4" || keyword == "learning resource exchange" || keyword == "LRE metadata application profile" || keyword == "LOM" || keyword == "EUN" || keyword == "ilox" || keyword == "needs" || keyword == "lr-test-data" || keyword.IndexOf("ncs-NSDL-COLLECTION") > -1)
            {
                retVal = true;
            }

            try
            {
                float f = float.Parse(keyword);
                retVal = true;
            }
            catch
            {
                // Do nothing, just checking to see if key is numeric.  It isn't so keep it.
            }

            return retVal;
        }// SkipKey

        /// <summary>
        /// Applies key edit rules to keyword
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private string ApplyKeyEditRules(string keyword)
        {
            if (keyword.Length > 2 && keyword.Substring(0, 1) == "\"" && keyword.Substring(keyword.Length - 1, 1) == "\"")
            {
                // keyword begins and ends with a quotation mark.  Check for internal quotes.  Note that stripping quotes requires the string to be at least 2 characters long.
                string tKey = keyword.Substring(1, keyword.Length - 2);
                if (tKey.IndexOf("\"") == -1)
                {
                    // no internal quotes found, strip quotes
                    keyword = keyword.Replace("\"", "");
                }
            }

            return keyword;
        }

#endregion

        #region === Career Cluster Helpers ===

        public void AddCareerClusters(Resource resource, XmlDocument payload)
        {
            string status = "";
            XmlNodeList list = payload.GetElementsByTagName("careerCluster");
            foreach (XmlNode node in list)
            {
                ResourceCluster cluster = new ResourceCluster();
                cluster.ResourceIntId = resource.Id;
                cluster.ClusterId = 0;
                cluster.Title = TrimWhitespace(node.InnerText);
                status = importManager.ImportCluster(cluster);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, resource.Version.LRDocId, resource.ResourceUrl, ErrorType.Error,
                        ErrorRouting.Technical, status);
                }
            }
        } //AddCareerClusters

        #endregion

        #region Helper Methods

        private bool IsNumeric(string param)
        {
            try
            {
                double dbl = double.Parse(param);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private bool IsDate(string param)
        {
            try
            {
                DateTime dt = DateTime.Parse(param);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        protected bool IsGoodTitle(string title, ref string message)
        {
            bool retVal = true;
            StringBuilder sb = new StringBuilder();

            if (IsNumeric(title))
            {
                retVal = false;
                sb.Append(" Title is numeric;");
            }
            if (title.Length <= 10 && IsDate(title))
            {
                retVal = false;
                sb.Append(" Title is a date;");
            }
            if (title.Length < 6)
            {
                retVal = false;
                sb.Append(" Title does not meet minimum length requirements;");
            }
            if (sb.ToString().Length > 0)
            {
                message = "Title does not meet requirements:" + sb.ToString();
            }
            else
            {
                message = "OK";
            }

            return retVal;
        }

        /// <summary>
        /// Returns the signer field of an LR document
        /// </summary>
        /// <param name="xdoc"></param>
        /// <returns></returns>
        public string GetSigner(XmlDocument xdoc)
        {
            string retVal = "";
            XmlNodeList list;

            list = xdoc.GetElementsByTagName("signer");
            if (list == null || list.Count == 0)
            {
                list = xdoc.GetElementsByTagName("submitter");
            }

            if (list != null && list.Count != 0)
            {
                retVal = list[0].InnerText;
                int pos = retVal.ToLower().IndexOf("on behalf of");
                if (pos > -1)
                {
                    retVal = retVal.Substring(0, pos);
                }
            }

            retVal = TrimWhitespace(retVal);
            return retVal;
        }

        /// <summary>
        /// Regular "Trim()" does not handle \r and \n.  This handles it.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string TrimWhitespace(string str)
        {
            Regex leadingWhitespace = new Regex(@"^\s+");
            Regex trailingWhitespace = new Regex(@"\s+$");

            str = leadingWhitespace.Replace(str, "");
            str = trailingWhitespace.Replace(str, "");

            return str;
        }


        public static bool DoesDataSetHaveRows(DataSet ds)
        {
            if (ds == null)
                return false;
            if (ds.Tables == null || ds.Tables.Count == 0)
                return false;
            if (ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count == 0)
                return false;
            
            return true;
        }

        public static int GetRowColumn(DataRow dr, string columnName, int defaultValue)
        {
            int retVal = defaultValue;
            try
            {
                retVal = int.Parse(dr[columnName].ToString());
            }
            catch
            {
                // do nothing
            }

            return retVal;
        }

        public static int GetRowColumn(SqlDataReader dr, string columnName, int defaultValue)
        {
            int retVal = defaultValue;
            try
            {
                retVal = int.Parse(dr[columnName].ToString());
            }
            catch
            {
                // do nothing
            }

            return retVal;
        }

        public static DateTime GetRowColumn(SqlDataReader dr, string columnName, DateTime defaultValue)
        {
            DateTime retVal = defaultValue;
            try
            {
                retVal = DateTime.Parse(dr[columnName].ToString());
            }
            catch
            {
                // do nothing
            }

            return retVal;
        }

        public static DateTime GetRowColumn(DataRow dr, string columnName, DateTime defaultValue)
        {
            DateTime retVal = defaultValue;
            try
            {
                retVal = DateTime.Parse(dr[columnName].ToString());
            }
            catch
            {
                // do nothing
            }

            return retVal;
        }

        public static string GetRowColumn(DataRow dr, string columnName, string defaultValue)
        {
            string retVal = defaultValue;
            try
            {
                retVal = dr[columnName].ToString();
            }
            catch
            {
                // do nothing
            }

            return retVal;
        }

        public static string GetRowColumn(SqlDataReader dr, string columnName, string defaultValue)
        {
            string retVal = defaultValue;
            try
            {
                retVal = dr[columnName].ToString();
            }
            catch
            {
                // do nothing
            }

            return retVal;
        }

        protected string StripTrailingPeriod(string input)
        {
            Regex trailingPeriod = new Regex(@"\.+$"); // backslash escapes the period so that it is interpreted as an actual period, + makes it greedy, $ asserts end of string
            if (input == null)
            {
                return null;
            }
            string output = trailingPeriod.Replace(input, "");

            return output;
        }

        protected string FixPlusesInUrl(string input)
        {
            string retVal = "";
            if (input.IndexOf("+") == -1)
            {
                // There are no + signs, return what was input
                return input;
            }
            int posQuestionMark = input.IndexOf("?");
            if (posQuestionMark == -1)
            {
                retVal = input.Replace("+", "%20");
            }
            else
            {
                string originalPage = input.Substring(0, posQuestionMark);
                string newPage = originalPage.Replace("+", "%20");
                retVal = input.Replace(originalPage, newPage);
            }

            return retVal;
        }

#endregion
    }
}
