using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Xml;
//using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace LearningRegistryCache2
{
    public class CommParaHandler : ParadataController
    {
        private AuditReportingManager auditReportingManager = new AuditReportingManager();
        private string ratingTypeValue;
        public CommParaHandler()
        {
            ratingTypeValue = ConfigurationManager.AppSettings["commParaRatingType"].ToString();
        }

        public void CommParaMap(string docId, string url, string payloadPlacement, XmlDocument record)
        {
            // Begin common logic for all paradata schemas
            XmlDocument payload = new XmlDocument();
            bool isValid = true;

            int minRatingValue, maxRatingValue, ratingCount;
            decimal lrRatingValue;

            XmlNodeList list;
            string nodeType = "";

            Resource resource = LoadCommonParadata(docId, url, payloadPlacement, record, payload, ref isValid);
            if (!isValid)
            {
                // Skip this record - it's already logged in LoadCommonParadata()
                VerifyResourceVersionRecordExists(resource);
                return;
            }
            // End common logic for all paradata schemas

            // Verb lists
            string intViewedDelimited = "downloaded|embedded|useInClassroom";
            string intBookmarkedDelimited = "favorited|featured|recommended|subscribed";
            string intIgnoredDelimited = "awarded|cited|linkedFrom|modified|ranked|rated|relatedTo|tagged|voted|commented";
            string strTaggedDelimited = "tagged";
            string strIgnoredDelimited = "commented";
            string voteBookmarkedDelimited = "accurate|like|useful";
            string voteViewedDelimited = "useInClassroom";
            string starRatedDelimited = "star";
            string starIgnoredDelimited = "usability";

            // Process integers
            list = payload.GetElementsByTagName("integer");
            foreach (XmlNode node in list)
            {
                nodeType = node.Attributes["type"].Value;
                // Check for validity of nodeType on int
                if (intViewedDelimited.IndexOf(nodeType) == -1 &&
                    intBookmarkedDelimited.IndexOf(nodeType) == -1 &&
                    intIgnoredDelimited.IndexOf(nodeType) == -1)
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url,
                        ErrorType.Error, ErrorRouting.Technical, string.Format("{0} is an unknown integer type", nodeType));
                }
                else
                {
                    // Passed nodeType validity check.  Process views.
                    if (intViewedDelimited.IndexOf(nodeType) > -1)
                    {
                        try
                        {
                            int views = int.Parse(TrimWhitespace(node.InnerText));
                            resource.ViewCount += views;
                        }
                        catch (Exception ex)
                        {
                            auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url,
                                ErrorType.Error, ErrorRouting.Technical, string.Format("{0} value is not an integer", nodeType));
                        }
                    }
                    // Process Favorited
                    else if (intBookmarkedDelimited.IndexOf(nodeType) > -1)
                    {
                        try
                        {
                            int favorites = int.Parse(TrimWhitespace(node.InnerText));
                            resource.FavoriteCount += favorites;
                        }
                        catch (Exception ex)
                        {
                            auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url,
                                ErrorType.Error, ErrorRouting.Technical, string.Format("{0} value is not an integer", nodeType));
                        }
                    }
                }
            } //foreach

            // Write the basic record
            resourceManager.UpdateById(resource);

            // Process strings
            list = payload.GetElementsByTagName("string");
            foreach (XmlNode node in list)
            {
                if (node.Attributes["type"] != null)
                {
                    nodeType = node.Attributes["type"].Value;
                    // Check for validity of nodeType on string
                    if (strTaggedDelimited.IndexOf(nodeType) == -1 && strIgnoredDelimited.IndexOf(nodeType) == -1)
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url,
                            ErrorType.Error, ErrorRouting.Technical, string.Format("{0} is an unknown string type", nodeType));
                    }
                    else
                    {
                        // Passed node type validity check.  Process tagged
                        if (strTaggedDelimited.IndexOf(nodeType) > -1)
                        {
                            string status = "successful";
                            ResourceCollection resources = resourceManager.SelectCollection(string.Format("ResourceUrl = '{0}'", url), ref status);
                            if (status != "successful")
                            {
                                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, status);
                            }
                            string doc = string.Format("<root><keyword>{0}</keyword></root>", node.InnerText.Trim());
                            XmlDocument xdoc = new XmlDocument();
                            xdoc.LoadXml(doc);
                            //resource.AddKeyword(xdoc, "keyword");
                            AddKeywords(resource, record, "keyword", ref status);
                            if (status != "")
                            {
                                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                            }

                        }
                    }
                }
            } //foreach node

            // Process votes
            list = payload.GetElementsByTagName("vote");
            //TODO: Process votes
            if (list.Count > 0)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url,
                    ErrorType.Error, ErrorRouting.Technical, "Votes are not yet handled by the import.");
            }

            // Process ratings
            list = payload.GetElementsByTagName("rating");
            foreach (XmlNode node in list)
            {
                nodeType = node.Attributes["type"].Value;
                try
                {
                    if (nodeType != "star")
                    {
                        string message = string.Format("Rating type \"{0}\" is not a valid rating type.", nodeType);
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, message);
                    }
                    else
                    {
                        minRatingValue = int.Parse(node.Attributes["min"].Value);
                        maxRatingValue = int.Parse(node.Attributes["max"].Value);
                        ratingCount = int.Parse(node.Attributes["total"].Value);
                        lrRatingValue = decimal.Parse(node.InnerText.Trim());

                        if (ratingCount > 0)
                        {
                            RatingSummary rating = GetRating(resource.RowId.ToString(), 0, ratingTypeValue, ratingTypeValue, "", docId, resource.Id);
                            CalculateNewRating(rating, minRatingValue, maxRatingValue, ratingCount, lrRatingValue);
                            if (rating.ResourceId == null || rating.ResourceId.ToString() == RatingSummaryManager.DEFAULT_GUID)
                            {
                                ratingSummaryManager.Create(rating);
                            }
                            else
                            {
                                ratingSummaryManager.Update(rating);
                            }
                            string status = "successful";
                            ResourceLikeSummary like = likeSummaryManager.Get(resource.Id, ref status);
                            if (status != "successful")
                            {
                                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Error,
                                    ErrorRouting.Technical, status);
                            }
                            CalculateNewLikes(like, minRatingValue, maxRatingValue, ratingCount, lrRatingValue);
                            if (like.Id == 0)
                            {
                                likeSummaryManager.Create(like, ref status);
                            }
                            else
                            {
                                likeSummaryManager.Update(like, ref status);
                            }
                            if (status != "successful")
                            {
                                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Error,
                                    ErrorRouting.Technical, status);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                }

            } //foreach node
        } //CommParaMap
    }
}
