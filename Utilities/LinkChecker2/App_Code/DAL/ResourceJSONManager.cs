using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using LinkChecker2.App_Code.BusObj;

namespace LinkChecker2.App_Code.DAL
{
/*    public class ResourceJSONManager
    {
        public ResourceJSONFlat GetJSONFlatFromDataRow(DataRow dr)
        {
            ResourceJSONFlat resource = new ResourceJSONFlat();

            //Single value items
            resource.created = MakeDate(Get(dr, "created"));
            resource.createdByID = MakeInt(Get(dr, "createdByID"));
            resource.creator = MakeString(Get(dr, "creator"));
            resource.description = MakeString(Get(dr, "description"));
            resource.isBasedOnURL = MakeString(Get(dr, "isBasedOnURL"));
            resource.intID = MakeInt(Get(dr, "intID"));
            resource.publisher = MakeString(Get(dr, "publisher"));
            resource.submitter = MakeString(Get(dr, "submitter"));
            resource.timeRequired = MakeString(Get(dr, "timeRequired"));
            resource.title = MakeString(Get(dr, "title"));
            resource.url = MakeString(Get(dr, "url"));
            resource.versionID = MakeInt(Get(dr, "versionID"));
            resource.timestamp = long.Parse(DateTime.Parse(resource.created).ToString("yyyyMMddHHmmss"));

            //Multi value items
            resource.keywords = MakeStringArray(Get(dr, "keywords"));
            resource.subjects = MakeStringArray(Get(dr, "subjects"));
            resource.urlParts = MakeURLParts(Get(dr, "url"));
            resource.libraryIDs = MakeIntArray(Get(dr, "libraryIDs"));
            resource.collectionIDs = MakeIntArray(Get(dr, "collectionIDs"));

            //Objects
            resource.accessRightsID = MakeInt(Get(dr, "accessRightsID"));
            resource.accessRights = MakeString(Get(dr, "accessRights"));
            resource.alignmentTypeIDs = MakeIntArray(Get(dr, "alignmentTypeIDs"));
            resource.alignmentTypes = MakeStringArray(Get(dr, "alignmentTypes"));
            resource.assessmentTypeID = MakeInt(Get(dr, "assessmentTypeID"));
            resource.assessmentType = MakeString(Get(dr, "assessmentType"));
            resource.audienceIDs = MakeIntArray(Get(dr, "audienceIDs"));
            resource.audiences = MakeStringArray(Get(dr, "audiences"));
            resource.clusterIDs = MakeIntArray(Get(dr, "clusterIDs"));
            resource.clusters = MakeStringArray(Get(dr, "clusters"));
            resource.educationalUseIDs = MakeIntArray(Get(dr, "educationalUseIDs"));
            resource.educationalUses = MakeStringArray(Get(dr, "educationalUses"));
            resource.gradeLevelIDs = MakeIntArray(Get(dr, "gradeLevelIDs"));
            resource.gradeLevels = MakeStringArray(Get(dr, "gradeLevels"));
            resource.gradeLevelAliases = MakeStringArray(Get(dr, "gradeLevelAliases"));
            if (resource.gradeLevelIDs.Length == 0)
            {   //Should handle potential incompatibilities
                resource.gradeLevelIDs = MakeIntArray(Get(dr, "educationLevelIDs"));
                resource.gradeLevels = MakeStringArray(Get(dr, "educationLevels"));
                resource.gradeLevelAliases = MakeStringArray(Get(dr, "educationLevelAliases"));
            }
            resource.groupTypeIDs = MakeIntArray(Get(dr, "groupTypeIDs"));
            resource.groupTypes = MakeStringArray(Get(dr, "groupTypes"));
            resource.itemTypeIDs = MakeIntArray(Get(dr, "itemTypeIDs"));
            resource.itemTypes = MakeStringArray(Get(dr, "itemTypes"));
            resource.languageIDs = MakeIntArray(Get(dr, "languageIDs"));
            resource.languages = MakeStringArray(Get(dr, "languages"));
            resource.mediaTypeIDs = MakeIntArray(Get(dr, "mediaTypeIDs"));
            resource.mediaTypes = MakeStringArray(Get(dr, "mediaTypes"));
            resource.resourceTypeIDs = MakeIntArray(Get(dr, "resourceTypeIDs"));
            resource.resourceTypes = MakeStringArray(Get(dr, "resourceTypes"));
            resource.standardIDs = MakeIntArray(Get(dr, "standardIDs"));
            resource.standardNotations = MakeStringArray(Get(dr, "standardNotations"));
            resource.notationParts = MakeStandardParts(Get(dr, "standardNotations"));
            resource.usageRights = MakeString(Get(dr, "usageRights"));
            resource.usageRightsID = MakeInt(Get(dr, "usageRightsID"));
            resource.usageRightsURL = MakeString(Get(dr, "usageRightsURL"));
            resource.usageRightsIconURL = MakeString(Get(dr, "usageRightsIconURL"));
            resource.usageRightsMiniIconURL = MakeString(Get(dr, "usageRightsMiniIconURL"));

            //Paradata
            resource.likesSummary = MakeInt(Get(dr, "likesSummary"));
            resource.evaluationCount = MakeInt(Get(dr, "evaluationCount"));
            resource.commentsCount = MakeInt(Get(dr, "commentsCount"));
            resource.favorites = MakeInt(Get(dr, "favorites"));
            resource.resourceViews = MakeInt(Get(dr, "viewsCount"));
            resource.detailViews = MakeInt(Get(dr, "detailViews"));
            resource.viewsCount = MakeInt(Get(dr, "viewsCount")); //temporary
            resource.evaluationScoreTotal = -1;
            resource.likeCount = MakeInt(Get(dr, "likeCount"));
            resource.dislikeCount = MakeInt(Get(dr, "dislikeCount"));

            return resource;
        }

        public string MakeDate(string input)
        {
            return input.Replace(" AM", "").Replace(" PM", "");
        }

        protected int MakeInt(string input)
        {
            try
            {
                return int.Parse(input.Replace("NULL", "").Trim());
            }
            catch
            {
                return 0;
            }
        }

        protected string MakeString(string input)
        {
            string data = input.Replace("  ", "")
                .Replace("NULL", "")
                .Replace(@"{", "&#123;")
                .Replace(@"}", "&#125;")
                .Replace(@"\n\t\t\t\t\t\t\t\t\t", " ")
                .Replace(@"          ", " ")
                .Replace(@"&amp;146;", "'")
                .Replace(@"&amp;147;", "\"")
                .Replace(@"&amp;148;", "\"")
                .Replace(@"&lt;p&gt;", "")
                .Replace(@"&lt;/p&gt;", "")
                .Trim();
            if (data.Length > 1000)
            {
                return data.Substring(0, 997) + "...";
            }
            else
            {
                return data;
            }
        }

        protected string[] MakeStringArray(string input)
        {
            string[] data = MakeString(input).Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < data.Length; i++)
            {
                if (i < 20)
                {
                    data[i] = data[i].Replace("\"", "").Trim();
                }
            }
            return data;
        }
        protected int[] MakeIntArray(string input)
        {
            List<int> items = new List<int>();
            string[] toParse = MakeStringArray(input);
            foreach (string item in toParse)
            {
                try
                {
                    items.Add(int.Parse(item.Replace("NULL", "").Trim()));
                }
                catch
                {
                    items.Add(0);
                }
            }
            return items.ToArray<int>();
        }
        protected string[] MakeURLParts(string input)
        {
            string[] parts = input.Replace("NULL", "")
                .Replace("http://", "")
                .Replace("https://", "")
                .Replace("ftp://", "")
                .Replace(".", "/")
                .Trim()
                .Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
            //Unrequired value-added URL things:
            List<string> stuff = parts.ToList<string>();
            try
            {
                string commonNameParts = input.Trim().Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[1];
                stuff.Add(commonNameParts);
            }
            catch { }
            try
            {
                string[] commonDomainParts = input.Trim().Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                string commonDomain = commonDomainParts[commonDomainParts.Length - 2] + "." + commonDomainParts[commonDomainParts.Length - 1];
                stuff.Add(commonDomain);
            }
            catch { }
            return stuff.ToArray<string>();

        }

        protected string[] MakeStandardParts(string input)
        {
            List<string> listFinal = new List<string>();
            string[] searchString = input.Replace("NULL", "").Trim().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string standard in searchString)
            {
                if (standard.IndexOf("CCSS.") == 0)
                {
                    string[] parts = standard.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    listFinal.Add(parts[0]);
                    listFinal.Add(parts[1]);
                    listFinal.Add(parts[2]);
                    listFinal.Add(parts[0] + "." + parts[1]);
                    listFinal.Add(parts[1] + "." + parts[2]);
                    listFinal.Add(parts[0] + "." + parts[1] + "." + parts[2]);
                    string rest = "";
                    for (int i = 3; i < parts.Length; i++)
                    {
                        rest = rest + "." + parts[i];
                    }
                    rest = rest.Substring(1);
                    listFinal.Add(rest);
                }
                else
                {
                    listFinal.Add(standard);
                }
            }
            List<string> reallyFinal = new List<string>();
            foreach (string item in listFinal)
            {
                if (!reallyFinal.Contains(item))
                {
                    reallyFinal.Add(item);
                }
            }
            return reallyFinal.ToArray<string>();
        }

        protected string Get(DataRow dr, string targetColumn)
        {
            return BaseDataManager.GetRowColumn(dr, targetColumn, "");
        }

    } */
}
