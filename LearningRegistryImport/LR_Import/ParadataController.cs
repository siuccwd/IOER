using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
//using mshtml;
using System.Xml;
//using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LearningRegistryCache2.App_Code.Classes;
using ILPathways.Utilities;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using LR_Import;
using Newtonsoft.Json;

namespace LearningRegistryCache2
{
    public class ParadataController : BaseDataController
    {
        protected AuditReportingManager reportingManager = new AuditReportingManager();
        protected ResourceManager resourceManager = new ResourceManager();
        //protected ResourceVersionManager versionManager = new ResourceVersionManager();
        protected ResourceVersionController versionManager = new ResourceVersionController();

        protected ResourceStandardManager standardManager = new ResourceStandardManager();
        protected ParadataManager paradataManager = new ParadataManager();
        protected RatingSummaryManager ratingSummaryManager = new RatingSummaryManager();
        protected ResourceLikeSummaryManager likeSummaryManager = new ResourceLikeSummaryManager();
        protected RatingTypeManager ratingTypeManager = new RatingTypeManager();
        protected ResourcePropertyManager propertyManager = new ResourcePropertyManager();
        protected ResourceCommentManager commentManager = new ResourceCommentManager();
        protected OLDDM.SubmitterManager submitterManager = new OLDDM.SubmitterManager();

        protected int wnMinRatingValue = 1;
        protected int wnMaxRatingValue = 5;

        public ParadataController()
        {
        }

        protected Resource LoadCommonParadata(string docId, string url, string payloadPlacement, XmlDocument xdoc, XmlDocument xpayload, ref bool isValid)
        {
            string status = "successful";
            XmlNodeList list;
            string payload = "";
            bool createMetadataFromParadata = false;
            if (!bool.TryParse(ConfigurationManager.AppSettings["createMetadataFromParadata"], out createMetadataFromParadata))
            {
                createMetadataFromParadata = false;
            }

            if (payloadPlacement == "inline")
            {
                list = xdoc.GetElementsByTagName("resource_data");
                if (list[0].InnerXml.IndexOf("<resource_data>") > 1)
                {
                    Regex resourceDataNodeEx = new Regex(@"<resource_data>([\s\S]*)</resource_data>");
                    payload = resourceDataNodeEx.Match(list[0].InnerXml).Value;
                    payload = payload.Replace("<resource_data>", "");
                    payload = payload.Replace("</resource_data>", "");
                    payload = payload.Replace("&lt;", "<").Replace("&gt;", ">");
                    payload = payload.Replace(">>", ">");
                }
                else
                {
                    payload = list[0].OuterXml.Replace("&lt;", "<").Replace("&gt;", ">");
                    payload = payload.Replace(">>", ">");
                }
                payload = "<root>" + payload + "</root>";
                //xpayload = new XmlDocument();
                try
                {
                    xpayload.LoadXml(payload);
                }
                catch (Exception ex)
                {
                    /* sometimes when these come through the data in the payload isn't XML, it's JSON.
                     * Attempt a conversion to XML and reprocess.  If this fails, log error and set isValid to false so record is skipped.
                     */
                    try
                    {
                        XmlDocument tempX = JsonConvert.DeserializeXmlNode(payload, "record");
                        xpayload.LoadXml(tempX.OuterXml);
                    }
                    catch (Exception ex2)
                    {
                        isValid = false;
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                    }
                }

            }

            // If the submitter is OER Commons, go do the OER Commons processing
            Resource resource = new Resource();
            list = xdoc.GetElementsByTagName("submitter");
            if (list[0].InnerText.Trim().IndexOf("OER Commons") > -1 && url.ToLower().IndexOf("oercommons.org") > -1)
            {
                resource = DoOERCommonsProcessing(docId, url, xdoc, xpayload, ref isValid);
                isValid = resource.IsValid;
                return resource;
            }
            else
            {
                resource = resourceManager.GetByResourceUrl(url.Replace("'","''"), ref status);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, status);
                }
                if (resource == null || resource.Message.ToLower() == "not found")
                {
                    if (createMetadataFromParadata)
                    {
                        // Resource does not exist, create it
                        resource = new Resource();
                        resource.ResourceUrl = url;
                        resource.FavoriteCount = 0;
                        resource.ViewCount = 0;
                        resourceManager.Create(resource, ref status);
                        if (status != "successful")
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, status);
                        }

                        list = xdoc.GetElementsByTagName("submitter");
                        resource.Version.Submitter = TrimWhitespace(list[0].InnerText);
                        // Since metadata doesn't exist, let's create a skeleton metadata version.
                        resource.Version.ResourceIntId = resource.Id;
                        resource.Version.LRDocId = docId;
                        resource.Version.Title = new OLDDM.HttpManager().GetPageTitle(url, ref status);
                        if (status != "successful")
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, status);
                        }
                        else
                        {
                            status = "OK";
                            if (!IsGoodTitle(resource.Version.Title, ref status))
                            {
                                resource.Version.IsActive = false;
                                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Program, status);
                            }

                            ManualResetEvent doneEvent = new ManualResetEvent(false);
                            doneEvents.Add(doneEvent);
                            ImageGenerator imageGenerator = new ImageGenerator(resource.ResourceUrl, resource.Id, doneEvent);
                            ThreadPool.QueueUserWorkItem(imageGenerator.ImageGeneratorThreadPoolCallback, doneEvent);
                        }
                        try
                        {
                            Uri uri = new Uri(url);
                            resource.Version.Publisher = uri.Host;
                            resource.Version.Creator = uri.Host;
                        }
                        catch (Exception ex)
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                            resource.Version.Publisher = "Unknown";
                            resource.Version.Creator = "Unknown";
                        }
                        resource.Version.Modified = DateTime.Now;
                        resource.Version.Imported = DateTime.Now;
                        resource.Version.Created = DateTime.Now;
                        list = xdoc.GetElementsByTagName("payload_schema");
                        foreach (XmlNode node in list)
                        {
                            resource.Version.Schema = TrimWhitespace(node.InnerText);
                            break;
                        }
                        resource.Version.IsSkeletonFromParadata = true;
                        if (resource.Version.Title == null || resource.Version.Title == "")
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Program,
                                "No title found.  Using URL for title.");
                            resource.Version.Title = url;
                        }
                        versionManager.Create(resource.Version, ref status);

                        if (status == "successful")
                        {
                            LearningRegistry.resourceIdList.Add(resource.Id);
                            return resource;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        resource.IsValid = isValid = false;
                        return resource;
                    }
                }
                return resource;
            }
        }

        protected Resource DoOERCommonsProcessing(string docId, string url, XmlDocument xdoc, XmlDocument xpayload, ref bool isValid)
        {
            string status = "successful";
            Resource resource = new Resource();
            XmlNodeList list;
            bool createMetadataFromParadata = false;
            if (!bool.TryParse(ConfigurationManager.AppSettings["createMetadataFromParadata"], out createMetadataFromParadata))
            {
                createMetadataFromParadata = false;
            }

            string page = new OLDDM.HttpManager().GetPage(url, ref status);
            if (status != "successful")
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, status + " Resource skipped.");
                resource.IsValid = isValid = false;
            }
            Regex titleRegex = new Regex(@"<title>(.*)</title>");
            Regex descriptionRegex = new Regex("<dd itemprop=\"description\">([\\s\\S]+?)</dd>");
            Regex itempropRegex = new Regex("itemprop=\"(.+?)\"");
            Regex authorRegex = new Regex("<span itemprop=\"author\">(.+?)</span>");
            Regex metaRegex = new Regex(@"<meta (.+?) ?/?>");
            Regex contentRegex = new Regex("content=\"(.+?)\"");
            status = "OK";
            MatchCollection metaMatches = metaRegex.Matches(page);
            // Must do two passes through metaMatches.  First pass creates the resource, 2nd pass processes keywords.
            foreach (Match match in metaMatches)
            {
                string itemprop = match.Value;
                Match itempropMatch = itempropRegex.Match(itemprop);
                if (itempropMatch.Value == "itemprop=\"url\"")
                {
                    Match contentMatch = contentRegex.Match(itemprop);
                    url = contentMatch.Value.Replace("content=\"", "").Replace("\"", "");
                    url = TrimWhitespace(url);
                    resource = resourceManager.GetByResourceUrl(url, ref status);
                    if (status != "successful")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, status);
                    }
                    if (resource == null || resource.RowId.ToString() == resource.DEFAULT_GUID)
                    {
                        if (createMetadataFromParadata)
                        {
                            // Resource does not exist, create it
                            resource = new Resource();
                            resource.ResourceUrl = url;
                            resource.FavoriteCount = 0;
                            resource.ViewCount = 0;
                            resourceManager.Create(resource, ref status);
                            resource.Version.ResourceIntId = resource.Id;
                            list = xdoc.GetElementsByTagName("submitter");
                            resource.Version.Submitter = list[0].InnerText.Trim();
                            resource.Version.LRDocId = docId;
                            resource.Version.Modified = DateTime.Now;
                            resource.Version.Imported = DateTime.Now;
                            resource.Version.Created = DateTime.Now;
                            Match titleMatch = titleRegex.Match(page);
                            resource.Version.Title = titleMatch.Value.Replace("<title>", "");
                            if (resource.Version.Title.IndexOf("|") > -1)
                            {
                                resource.Version.Title = resource.Version.Title.Substring(0, resource.Version.Title.IndexOf("|"));
                            }
                            resource.Version.Title = TrimWhitespace(resource.Version.Title);
                            Match authorMatch = authorRegex.Match(page);
                            resource.Version.Creator = authorMatch.Value.Replace("<span itemprop=\"author\">", "");
                            resource.Version.Creator = resource.Version.Creator.Replace("</span>", "");
                            resource.Version.Creator = TrimWhitespace(resource.Version.Creator);
                            resource.Version.Publisher = resource.Version.Creator;
                            Match descriptionMatch = descriptionRegex.Match(page);
                            resource.Version.Description = descriptionMatch.Value.Replace("<dd itemprop=\"description\">", "");
                            resource.Version.Description = resource.Version.Description.Replace("</dd>", "");
                            resource.Version.Description = TrimWhitespace(resource.Version.Description);

                            status = "OK";
                            if (!IsGoodTitle(resource.Version.Title, ref status))
                            {
                                resource.Version.IsActive = false;
                                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Program, status);
                            }
                            versionManager.Create(resource.Version, ref status);
                        }
                        else
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Warning, ErrorRouting.Program,
                                "Paradata found without pre-existing metadata.  Paradata ignored.");
                            resource.IsValid = isValid = false;
                            return resource;
                        }
                    }
                }
            }
            // Check to see if we created the resource.  If not, write to error log
            if (resource.RowId.ToString() == resource.DEFAULT_GUID)
            {
                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, "Could not create resource.  HTML on OER Commons page may be malformed.");
                resource.IsValid = false;
                return resource;
            }
            // Second pass through metaMatches handles keywords
            foreach(Match match in metaMatches)
            {
                string itemprop = match.Value;
                Match itempropMatch = itempropRegex.Match(itemprop);
                if (itempropMatch.Value == "itemprop=\"keywords\"")
                {
                    Match contentMatch = contentRegex.Match(itemprop);
                    string content = contentMatch.Value.Replace("content=\"", "").Replace("\"","");
                    string[] contents = content.Split(',');
                    foreach (string keyword in contents)
                    {
                        AddKeyword(resource, TrimWhitespace(keyword));
/*                        ResourceProperty prop = new ResourceProperty();
                        prop.ResourceId = resource.RowId;
                        prop.Name = "keyword";
                        prop.PropertyTypeId = iPropertyType;
                        prop.Value = keyword;
                        propertyManager.Create(prop, ref status);*/
                    }
                }
            }
            return resource;
        }

        protected RatingSummary GetRating(string resourceId, int ratingTypeId, string type, string identifier, string description, string docId, int resourceIntId)
        {
            RatingSummary rating = new RatingSummary();
            RatingType ratingType = new RatingType();
            string status = "successful";

            rating = ratingSummaryManager.Get(resourceId, ratingTypeId, type, identifier);
            if (rating == null)
            {
                // Rating not found
                rating = new RatingSummary();
                rating.ResourceId = new Guid(resourceId);
                rating.ResourceIntId = resourceIntId;
                // Look up the rating type.
                ratingType = ratingTypeManager.Get(0, type, identifier);
                if (ratingType == null || ratingType.Message.ToLower() == "not found")
                {
                    // Rating type not found, create one.
                    status = "successful";
                    ratingType = new RatingType();
                    ratingType.Type = type;
                    ratingType.Identifier = identifier;
                    ratingType.Description = description;

                    rating.RatingTypeId = ratingTypeManager.Create(ratingType, ref status);
                }
                else
                {
                    // Rating type found, use it.
                    rating.RatingTypeId = ratingType.Id;
                }
                status = ratingSummaryManager.Create(rating);
                if (status != "successful")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, "", ErrorType.Error, ErrorRouting.Technical, status);
                }
            }

            return rating;
        }


        protected void CalculateNewRating(RatingSummary rating, int min, int max, int count, decimal value)
        {
            const int wnMinRating = 0;
            const int wnMaxRating = 3;

            value = UtilityManager.ConvertRatingRange(min, max, value, wnMinRating, wnMaxRating);
/*            if (min != wnMinRating && max != wnMaxRating)
            {
                // Convert the value to the ISLE scale
                value = value - min;
                if (min == 0)
                {
                    value = value * (wnMaxRating - wnMinRating) / max;
                }
                else
                {
                    value = value * (wnMaxRating - wnMinRating) / (max / min);
                }
                value += wnMinRating;
            }*/

            decimal intermediateTotal = Math.Round(value * (decimal)count);
            rating.RatingTotal += (int)intermediateTotal;
            rating.RatingCount += count;
            rating.RatingAverage = rating.RatingTotal / rating.RatingCount;
        }

        protected void CalculateNewLikes(ResourceLikeSummary likeSummary, int minRatingValue, int maxRatingValue, int ratingCount, decimal value)
        {
            const int wnMinRating = 0;
            const int wnMaxRating = 3;

            value = UtilityManager.ConvertRatingRange(minRatingValue, maxRatingValue, value, wnMinRating, wnMaxRating);
            if (value < (decimal)1.05)
            {
                likeSummary.DislikeCount += ratingCount;
            }
            else if (value > (decimal)1.80)
            {
                likeSummary.LikeCount += ratingCount;
            }
            else
            {
                decimal percentage = (value - (decimal)1.05) / (decimal)0.75;
                int likes = (int)Math.Round(ratingCount * percentage);
                int dislikes = ratingCount - likes;
                likeSummary.LikeCount += likes;
                likeSummary.DislikeCount += dislikes;
            }
        }

        

        public void VerifyResourceVersionRecordExists(Resource resource)
        {
            string status = "successful";
            if (resource != null)
            {
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
        }


    }
}
