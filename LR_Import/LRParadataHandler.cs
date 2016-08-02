using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
//using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using Isle.BizServices;
using LRWarehouse.Business;

namespace LearningRegistryCache2
{
    public class LRParadataHandler : ParadataController
    {
        private AuditReportingServices auditReportingManager = new AuditReportingServices();
        //private ResourcePropertyManager propertyManager = new ResourcePropertyManager();

        public LRParadataHandler()
        {
        }

        public void LrParadataMap(string docId, ref string url, string payloadPlacement, XmlDocument record)
        {
            string status = "";
            // Begin common logic for all paradata schemas
            XmlDocument payload = new XmlDocument();
            bool isValid = true;
            string errorMessage = "";

            int minRatingValue, maxRatingValue, ratingCount;
            decimal lrRatingValue;
            XmlNodeList list;
            string action = "";

            Resource resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
            if (!isValid)
            {
                // Skip this record - it's already logged in LoadCommonParadata()
                VerifyResourceVersionRecordExists(resource);
                return;
            }
            // End common logic for all paradata schemas

            // Verb lists
            string viewedDelimited = "viewed|taught|incorporated|add resource|used|adopted|accessed";
            string bookmarkedDelimited = "bookmarked|favorite|favorited|watched|shared|shared with read rights|shared with write rights|recommended";
            string alignedDelimited = "matched|aligned";
            string taggedDelimited = "tagged";
            string ratedDelimited = "rated";
            string replacedDelimited = "supercedes";
            string commentDelimited = "comment|commented";
            string deletedDelimited = "out of date|not available|deleted";
            string ignoredDelimited = "same as|similar to|composed of|assessment of";

            // Get action
            list = payload.GetElementsByTagName("action");
            if (list.Count == 0)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, "No action found!");
                return;
            }
            action = TrimWhitespace(list.Item(0).InnerText).ToLower();

            // Handle unknown paradata types
            if (viewedDelimited.IndexOf(action) == -1 && bookmarkedDelimited.IndexOf(action) == -1 &&
                alignedDelimited.IndexOf(action) == -1 && taggedDelimited.IndexOf(action) == -1 &&
                ratedDelimited.IndexOf(action) == -1 && replacedDelimited.IndexOf(action) == -1 &&
                deletedDelimited.IndexOf(action) == -1 && commentDelimited.IndexOf(action) == -1 &&
                ignoredDelimited.IndexOf(action) == -1)
            {
                // Log as unknown paradata type
                errorMessage = string.Format("Unknown paradata type \"{0}\"", action);
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Error, ErrorRouting.Technical, errorMessage);
                return;
            }

            // Process Page Views
            if (viewedDelimited.IndexOf(action) > -1 && action != "rated") // "rated" appears in "incorporated" so specifically exclude that!
            {
                resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                HandleViews(resource, payload, docId);
                //resource.AddKeyword(record, "keys");
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
            }
 

            // Process Favorites
            if (bookmarkedDelimited.IndexOf(action) > -1)
            {
                resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                HandleFavorites(resource, payload, docId);
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
                // resource.AddKeyword(record, "keys");
            }


            // Process Standards
            if (alignedDelimited.IndexOf(action) > -1)
            {
                resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                HandleStandards(resource, payload, docId);
                //resource.AddKeyword(record, "keys");
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
            }


            // Process Tagged
            if (taggedDelimited.IndexOf(action) > -1)
            {
                resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                HandleTagged(resource, payload, docId);
                // Don't handle keywords here - it's handled internally by tagged
            }


            // Process supercedes
            if (replacedDelimited.IndexOf(action) > -1)
            {
                XmlNodeList superceded = payload.GetElementsByTagName("related");
                if (superceded.Count == 0)
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url,
                        ErrorType.Error, ErrorRouting.Technical, "Related not found on a supercedes");
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                string originalUrl = superceded[0].InnerText.Trim();
                resource = LoadCommonParadata(docId, ref originalUrl, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    return;
                }
                HandleSupercedes(resource, record, payload, docId);
                //resource.AddKeyword(record, "keys");
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }

            }

            // Process comments
            if (commentDelimited.IndexOf(action) > -1)
            {
                resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                HandleComment(resource, payload, docId);
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
            }


            // Process delete
            if (deletedDelimited.IndexOf(action) > -1)
            {
                resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                HandleDelete(resource, record, payload, docId);
                // Don't handle keywords here - this is a delete! Seriously!!
            }


            // Process ratings
            if (ratedDelimited.IndexOf(action) > -1)
            {
                resource = LoadCommonParadata(docId, ref url, payloadPlacement, record, payload, ref isValid);
                if (!isValid)
                {
                    // Skip this record - it's already logged in LoadCommonParadata()
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                HandleRatings(resource, payload, docId);
                //resource.AddKeyword(record, "keys");
                AddKeywords(resource, record, "keys", ref status);
                if (status != "")
                {
                    reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, url, ErrorType.Warning, ErrorRouting.Program, status);
                }
            }

            LearningRegistry.resourceIdList.Add(resource.Id);
        }// LrParadataMap()

        protected void HandleViews(Resource resource, XmlDocument payload, string docId)
        {
            try
            {
                XmlNodeList list;
                list = payload.GetElementsByTagName("measure");
                int viewCount = 0;
                if (list.Count == 0)
                {
                    // Measure not found, assume 1
                    viewCount = 1;
                }
                else
                {
                    list = payload.GetElementsByTagName("measureType");
                    if (list.Count == 0)
                    {
                        // Measure type not found, flag as error and skip record
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical,
                            "measureType not found in measure");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                    list = payload.GetElementsByTagName("value");
                    if (list.Count == 0)
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical,
                            "value not found in measure");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                    viewCount = int.Parse(TrimWhitespace(list.Item(0).InnerText));
                }

                resource.ViewCount += viewCount;
                importManager.UpdateResource(resource);
            }
            catch (Exception ex)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                VerifyResourceVersionRecordExists(resource);
                return;
            }

        } //HandleViews

        protected void HandleFavorites(Resource resource, XmlDocument payload, string docId)
        {
            XmlNodeList list;
            try
            {
                list = payload.GetElementsByTagName("measure");
                int favoriteCount = 0;
                if (list.Count == 0)
                {
                    //Measure not found, assume 1
                    favoriteCount = 1;
                }
                else
                {
                    list = payload.GetElementsByTagName("measureType");
                    if (list.Count == 0)
                    {
                        //Measure type not found, flag as error and skip record
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, "measureType not found in measure");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                    list = payload.GetElementsByTagName("value");
                    if (list.Count == 0)
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, "value not found in measure");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                    favoriteCount = int.Parse(TrimWhitespace(list.Item(0).InnerText));
                }

                resource.FavoriteCount += favoriteCount;
                importManager.UpdateResource(resource);
            }
            catch (Exception ex)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                VerifyResourceVersionRecordExists(resource);
                return;
            }
        } //HandleFavorites

        protected void HandleComment(Resource resource, XmlDocument payload, string docId)
        {
            XmlNodeList list;
            string status = "successful";

            try
            {
                list = payload.GetElementsByTagName("comment");
                if (list.Count == 0)
                {
                    // No comment supplied, we cannot process this!
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, "No comment field found in a comment paradata item!");
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }

                ResourceComment comment = new ResourceComment();
                comment.ResourceIntId = resource.Id;
                XmlNodeList dateList = payload.GetElementsByTagName("date");
                if (dateList.Count != 0)
                {
                    comment.Created = DateTime.Parse(TrimWhitespace(dateList[0].InnerText));
                }
                else
                {
                    comment.Created = DateTime.Now;
                }
                //comment.ResourceIntId = resource.Id;
                comment.CreatedById = 0;
                comment.Comment = TrimWhitespace(list[0].InnerText);
                XmlNodeList actors = payload.GetElementsByTagName("actor");
                if (actors.Count == 0)
                {
                    comment.CreatedBy = "User";
                }
                else
                {
                    comment.Commenter = actors[0].OuterXml;
                    comment.CreatedBy = BuildCreatedBy(actors[0]);
                }
                comment.DocId = docId;
                bool isDuplicate = CheckForDuplicates(comment);

                if (!isDuplicate)
                {
                    status = importManager.ImportComment(comment);
                    if (status != "successful")
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, status);
                    }
                }
            }
            catch (Exception ex)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Technical, ex.Message);
            }

            return;
        }

        protected bool CheckForDuplicates(ResourceComment comment)
        {
            bool result = false;
            string comparisonText = comment.Comment.ToLower();
            string dq = "\"";
            Regex massageEx = new Regex(@"\s|[!@#$%^&*()\-_=+\[{\]};:'\" + dq + @",./<>?]");
            comparisonText = massageEx.Replace(comparisonText, "");

            DataSet ds = importManager.SelectComment(comment.ResourceIntId);
            if (DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string text = GetRowColumn(dr, "Comment", "");
                    text = text.ToLower();
                    text = massageEx.Replace(text, "");
                    if (text == comparisonText)
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        protected string BuildCreatedBy(XmlNode node)
        {
            BaseBusinessDataEntity bbde = new BaseBusinessDataEntity();
            StringBuilder createdBy = new StringBuilder();
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(node.OuterXml);

            // Get the type of person who commented.
            XmlNodeList list = xdoc.GetElementsByTagName("objectType");
            if (list.Count == 0)
            {
                createdBy.Append("User ");
            }
            else
            {
                createdBy.Append(TrimWhitespace(list[0].InnerText) + " ");
            }

            // Put in grade level or indicate multiple grade levels
            list = xdoc.GetElementsByTagName("description");
            if (list.Count == 1)
            {
                string grade = TrimWhitespace(list[0].InnerText);
                if (grade.Length == 1)
                {
                    // If length is 1, it's probably a grade level (P, K, 1-9).
                    createdBy.Append("of grade " + grade + " ");
                }
                else
                {
                    //length > 1, test for numeric.  If numeric, it's a P-12 grade level.
                    if (bbde.IsNumeric(grade))
                    {
                        createdBy.Append("of grade " + grade + " ");
                    }
                    else
                    {
                        // Not numeric, could be College or Adult.
                        createdBy.Append(grade + " ");
                    }
                }
            }
            else if (list.Count > 1)
            {
                createdBy.Append("of multiple grades ");
            }

            return TrimWhitespace(createdBy.ToString());
        }

        
        void HandleStandards(Resource resource, XmlDocument payload, string docId)
        {
            XmlNodeList list;
            string status = "successful";
            try
            {
                list = payload.GetElementsByTagName("related");
                if (list.Count == 0)
                {
                    list = payload.GetElementsByTagName("id");
                    if (list.Count == 0)
                    {
                        // related not found, flag as error and skip record
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, "neither related nor id is found in payload");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                }
                XmlDocument tempXdoc = new XmlDocument();
                tempXdoc.LoadXml(list.Item(0).OuterXml);
                list = tempXdoc.GetElementsByTagName("id");
                if (list.Count == 0)
                {
                    //id not found within related, check for id anywhere in the payload.
                    list = payload.GetElementsByTagName("id");
                    if (list.Count == 0)
                    {
                        //id not found anywhere in record.  Flag as error and skip record
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, "id not found within related or anywhere within record.");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                }
                string identifier = TrimWhitespace(list.Item(0).InnerText);
                ResourceStandard standard = new ResourceStandard();
                standard.ResourceIntId = resource.Id;
                standard.StandardId = 0;
                standard.StandardUrl = identifier;
                if (standard.StandardUrl.Length > 300)
                {
                    standard.StandardUrl = standard.StandardUrl.Substring(0, 300);
                }
                importManager.ImportStandard(standard, ref status);
                if (status != "successful")
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, status);
                }
                return;
            }
            catch (Exception ex)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                return;
            }
        } //HandleStandards

        protected void HandleTagged(Resource resource, XmlDocument payload, string docId)
        {
            XmlNodeList list;
            string status = "successful";
            try
            {
                list = payload.GetElementsByTagName("keys");
                if (list.Count > 0)
                {
                    //resource.AddKeyword(payload, "keys");
                    AddKeywords(resource, payload, "keys", ref status);
                    if (status != "")
                    {
                        reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Warning, ErrorRouting.Program, status);
                    }

                }
                else
                {
                    list = payload.GetElementsByTagName("related");
                    if (list.Count > 0)
                    {
                        //resource.AddKeyword(payload, "related");
                        AddKeywords(resource, payload, "related", ref status);
                        if (status != "")
                        {
                            reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Warning, ErrorRouting.Program, status);
                        }
                    }
                    else
                    {
                        list = payload.GetElementsByTagName("content");
                        if (list.Count > 0)
                        {
                            //resource.AddKeyword(payload, "content");
                            AddKeywords(resource, payload, "content", ref status);
                            if (status != "")
                            {
                                reportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Warning, ErrorRouting.Program, status);
                            }
                        }
                        else
                        {
                            auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                                ErrorType.Error, ErrorRouting.Technical, "no keys found for \"tagged\" verb.");
                            VerifyResourceVersionRecordExists(resource);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                VerifyResourceVersionRecordExists(resource);
                return;
            }
        } // HandleTagged

        protected void HandleSupercedes(Resource resource, XmlDocument record, XmlDocument payload, string docId)
        {
            XmlNodeList list;
            string status = "successful";
            try
            {
                list = record.GetElementsByTagName("submitter");
                if (list.Count == 0)
                {
                    // No submitter supplied, we cannot process this!
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, "No submitter found for a supercedes.");
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                string submitterName = TrimWhitespace(list[0].InnerText);
                Resource res = new LR_Import.ResourceVersionController().GetByResourceUrlAndSubmitter(resource.ResourceUrl, submitterName);
                if (res == null)
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, "Resource matching url and submitter not found in database (Submitter not trusted)");
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }

                list = record.GetElementsByTagName("object");
                if (list.Count == 0)
                {
                    // No supercedes URL specified!
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, "Object that supercedes original url not found");
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                resource.ResourceUrl = TrimWhitespace(list[0].InnerText);
                importManager.UpdateResource(resource);
            }
            catch (Exception ex)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                return;
            }
        } //HandleSupercedes


        protected void HandleDelete(Resource resource, XmlDocument record, XmlDocument payload, string docId)
        {
            XmlNodeList list;
            string status = "successful";
            try
            {
                list = record.GetElementsByTagName("submitter");
                if (list.Count == 0)
                {
                    // No submitter supplied, we cannot process this!
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, "No submitter found for a delete.");
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }

                string submitterName = TrimWhitespace(list[0].InnerText);
                Resource res = new LR_Import.ResourceVersionController().GetByResourceUrlAndSubmitter(resource.ResourceUrl, submitterName);
                if (res == null)
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, "Resource matching url and submitter not found in database (Submitter not trusted)");
                    VerifyResourceVersionRecordExists(resource);
                    return;
                }
                
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                       ErrorType.Warning, ErrorRouting.Technical, string.Format("Deleting resource. ID: {0}, Url:{1} ", res.Id, res.ResourceUrl));

                //15-05-18 mparsons. This deletes seems incorrect - passing resource rowId to delete a version? Don't know if this is the problem with the RV being deleted. 
                //versionManager.Delete(res.RowId.ToString(), ref status);

                //resourceManager.Delete(res.RowId.ToString());

            } catch (Exception ex) {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                VerifyResourceVersionRecordExists(resource);
                return;
            }
        } //HandleDelete


        protected void HandleRatings(Resource resource, XmlDocument payload, string docId)
        {
            string ratingType = "";
            string ratingIdentifier = "";
            string ratingDescription = "";
            int minRatingValue = 1;
            int maxRatingValue = 5;
            int ratingCount = 1;
            decimal lrRatingValue = (decimal)0.0;
            XmlNodeList list;

            list = payload.GetElementsByTagName("related");
            if (list.Count == 0)
            {
                ratingType = "None";
                ratingIdentifier = "None";
                ratingDescription = "";
            }
            else
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(list[0].OuterXml);
                list = xdoc.GetElementsByTagName("type");
                if (list.Count != 0)
                {
                    ratingType = TrimWhitespace(list[0].InnerText);
                }
                list = xdoc.GetElementsByTagName("id");
                if (list.Count != 0)
                {
                    ratingIdentifier = TrimWhitespace(list[0].InnerText);
                }
                else
                {
                    list = xdoc.GetElementsByTagName("identifier");
                    if (list.Count != 0)
                    {
                        ratingIdentifier = TrimWhitespace(list[0].InnerText);
                    }
                }
                list = xdoc.GetElementsByTagName("description");
                if (list.Count != 0)
                {
                    ratingDescription = TrimWhitespace(list[0].InnerText);
                }
                else
                {
                    list = xdoc.GetElementsByTagName("content");
                    if (list.Count != 0)
                    {
                        ratingDescription = list[0].InnerText.Trim();
                    }
                }
                if (ratingIdentifier.Length > 0 && ratingDescription.Length == 0)
                {
                    ratingDescription = ratingIdentifier;
                }
                if (ratingIdentifier.Length == 0 && ratingDescription.Length > 0)
                {
                    ratingIdentifier = ratingDescription;
                }
                if (ratingIdentifier.Length == 0)
                {
                    ratingIdentifier = "None";
                }
            }

            list = payload.GetElementsByTagName("measure");
            if (list.Count == 0)
            {
                auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                    ErrorType.Error, ErrorRouting.Technical, "Measure tag not found. Rating record skipped.");
                VerifyResourceVersionRecordExists(resource);
                return;
            }
            else
            {
                try
                {
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(list[0].OuterXml);
                    list = xdoc.GetElementsByTagName("scaleMin");
                    if (list.Count == 0)
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, "scaleMin tag not found. Rating record skipped.");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                    minRatingValue = int.Parse(TrimWhitespace(list[0].InnerText));
                    list = xdoc.GetElementsByTagName("scaleMax");
                    if (list.Count == 0)
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, "scaleMax tag not found. Rating record skipped.");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                    maxRatingValue = int.Parse(list[0].InnerText.Trim());
                    list = xdoc.GetElementsByTagName("sampleSize");
                    if (list.Count == 0)
                    {
                        ratingCount = 1;
                    }
                    else
                    {
                        ratingCount = int.Parse(TrimWhitespace(list[0].InnerText));
                    }
                    list = xdoc.GetElementsByTagName("value");
                    if (list.Count == 0)
                    {
                        auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                            ErrorType.Error, ErrorRouting.Technical, "value tag not found. Rating record skipped.");
                        VerifyResourceVersionRecordExists(resource);
                        return;
                    }
                    list[0].InnerText = TrimWhitespace(list[0].InnerText).Replace("\"", ""); // Get rid of quotes, if present
                    lrRatingValue = decimal.Parse(TrimWhitespace(list[0].InnerText));

                    RatingSummary rating = GetRating(resource.RowId.ToString(), 0,
                        ratingType, ratingIdentifier, ratingDescription, docId, resource.Id);
                    CalculateNewRating(rating, minRatingValue, maxRatingValue, ratingCount, lrRatingValue);
                    if (rating.ResourceId == null || rating.ResourceId.ToString() == ImportServices.DEFAULT_GUID)
                    {
                        importManager.RatingSummaryCreate(rating);
                    }
                    else
                    {
                        importManager.RatingSummaryUpdate(rating);
                    }

                    if (ratingIdentifier == "None")
                    {
                        // Handle as Like/Dislike summary
                        string status = "successful";
                        ResourceLikeSummary like = importManager.LikeSummaryGet(resource.Id, ref status);
                        if (status != "successful")
                        {
                            auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl, ErrorType.Error,
                                ErrorRouting.Technical, status);
                        }
                        if (like == null)
                        {
                            like = new ResourceLikeSummary();
                            like.Id = 0;
                            like.ResourceIntId = resource.Id;
                            like.ResourceId = resource.RowId;
                            like.LikeCount = 0;
                            like.DislikeCount = 0;
                            like.LastUpdated = DateTime.Now;
                        }
                        CalculateNewLikes(like, minRatingValue, maxRatingValue, ratingCount, lrRatingValue);
                        if (like.Id == 0)
                        {
                            importManager.LikeSummaryCreate(like, ref status);
                        }
                        else
                        {
                            importManager.LikeSummaryUpdate(like, ref status);
                        }
                        if (status != "successful")
                        {
                            auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                                ErrorType.Error, ErrorRouting.Technical, status);
                        }
                    }
                }
                catch (Exception ex)
                {
                    auditReportingManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, docId, resource.ResourceUrl,
                        ErrorType.Error, ErrorRouting.Technical, ex.ToString());
                    return;
                }
            }
        } //HandleRating
    }
}
