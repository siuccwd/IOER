using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkChecker2.App_Code.BusObj
{
    public class ResourceJSONFlat
    {
        //Single value items
        public string created;
        public int createdByID;
        public string creator;
        public string description;
        public string isBasedOnURL;
        public int intID;
        public string publisher;
        public string submitter;
        public string timeRequired;
        public string title;
        public string url;
        public string usageRights;
        public int usageRightsID;
        public string usageRightsURL;
        public string usageRightsIconURL;
        public string usageRightsMiniIconURL;
        public int versionID;
        public long timestamp;

        //Multi value items
        public string[] keywords;
        public string[] subjects;
        public string[] urlParts;
        public int[] libraryIDs;
        public int[] collectionIDs;

        //Objects
        public string accessRights;
        public int accessRightsID;
        public int assessmentTypeID;
        public string assessmentType;
        public int[] alignmentTypeIDs;
        public string[] alignmentTypes;
        public int[] audienceIDs;
        public string[] audiences;
        public int[] clusterIDs;
        public string[] clusters;
        public int[] educationalUseIDs;
        public string[] educationalUses;
        public int[] gradeLevelIDs;
        public string[] gradeLevels;
        public string[] gradeLevelAliases;
        public int[] groupTypeIDs;
        public string[] groupTypes;
        public int[] itemTypeIDs;
        public string[] itemTypes;
        public int[] languageIDs;
        public string[] languages;
        public int[] mediaTypeIDs;
        public string[] mediaTypes;
        public string[] notationParts;
        public int[] resourceTypeIDs;
        public string[] resourceTypes;
        public int[] standardIDs;
        public string[] standardNotations;

        //Paradata
        public int likesSummary;
        public int evaluationCount;
        public int evaluationScoreTotal;
        public int commentsCount;
        public int favorites;
        public int resourceViews;
        public int detailViews;
        public int viewsCount; //temporary
        public int likeCount;
        public int dislikeCount;
    }
}
