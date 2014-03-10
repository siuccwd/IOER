using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
  public class ResourceJSON
  {
    public ResourceJSON()
    {
      keywords = new List<string>();
      urlParts = new List<string>();
      standards = new JSONStandards();
      usageRights = new JSONUsageRights();
      fields = new List<MultiValueField>();
    }
    public int versionID;
    public int intID;
    public int timestamp;
    public string title { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string LRDocID { get; set; }
    public string requirements { get; set; }
    public string timeRequired { get; set; }
    public string created { get; set; }
    public string creator { get; set; }
    public string publisher { get; set; }
    public string signer { get; set; }
    public string originalVersionURL { get; set; }
    public List<string> keywords { get; set; }
    public List<string> urlParts { get; set; }
    public JSONStandards standards { get; set; }
    public JSONUsageRights usageRights { get; set; }
    public JSONParadataSummary paradataSummary { get; set; }
    public List<MultiValueField> fields { get; set; }
    /*public MultiValueField isleSection { get; set; }
    public MultiValueField alignmentType { get; set; }
    public MultiValueField itemType { get; set; }
    public MultiValueField gradeLevel { get; set; }
    public MultiValueField subject { get; set; }
    public MultiValueField accessRights { get; set; }
    public MultiValueField careerCluster { get; set; }
    public MultiValueField educationalUse { get; set; }
    public MultiValueField endUser { get; set; }
    public MultiValueField groupType { get; set; }
    public MultiValueField language { get; set; }
    public MultiValueField mediaType { get; set; }
    public MultiValueField resourceType { get; set; }
    public MultiValueField accessibilityFeature { get; set; }
    public MultiValueField accessibilityHazard { get; set; }
    public MultiValueField accessibilityAPI { get; set; }
    public MultiValueField accessibilityControl { get; set; }
    public MultiValueField careerPlanning { get; set; }
    public MultiValueField employer { get; set; }
    public MultiValueField disability { get; set; }
    public MultiValueField jobPreparation { get; set; }
    public MultiValueField veterans { get; set; }
    public MultiValueField workforceEducationPartners { get; set; }
    public MultiValueField workplaceSkills { get; set; }
    public MultiValueField workSupportServices { get; set; }*/
  }
  public class MultiValueField
  {
    public MultiValueField()
    {
      names = new List<string>();
      ids = new List<int>();
    }
    public string esName { get; set; }
    public string title { get; set; }
    public string type { get; set; }
    public int fieldID { get; set; }
    public List<string> names { get; set; }
    public List<int> ids { get; set; }
  }
  public class JSONParadataSummary
  {
    public int commentsCount { get; set; }
    public int detailViews { get; set; }
    public int resourceViews { get; set; }
    public int likes { get; set; }
    public int dislikes { get; set; }
    public int ratingScore { get; set; }
    public int evaluationsCount { get; set; }
    public double evaluationsScore { get; set; }
    public int favoritesCount { get; set; }
  }
  public class JSONStandards : MultiValueField
  {
    public JSONStandards()
    {
      parts = new List<string>();
    }
    public List<string> parts { get; set; }
  }
  public class JSONUsageRights : MultiValueField
  {
    public string url { get; set; }
    public string iconURL { get; set; }
    public string miniIconURL { get; set; }
  }

    /// <summary>
    /// ResourceJSONFlat - used for elasticSearch and anywhere else that needs everything in a flat layer
    /// </summary>
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

    /// <summary>
    /// ResourceJSONObject - hierarchical object relationship oriented variant
    /// </summary>
    public class ResourceJSONObject
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
        public int versionID;
        public long timestamp;

        //Multi value items
        public string[] keywords;
        public string[] subjects;
        public string[] urlParts;
        public int[] libraryIDs;
        public int[] collectionIDs;

        //Objects
        public ResourceJSONObjectItem accessRights;
        public ResourceJSONObjectItem assessmentType;
        public ResourceJSONObjectItem[] audiences;
        public ResourceJSONObjectItem[] clusters;
        public ResourceJSONObjectItem[] educationalUses;
        public ResourceJSONObjectGradeLevel[] gradeLevels;
        public ResourceJSONObjectItem[] groupTypes;
        public ResourceJSONObjectItem[] itemTypes;
        public ResourceJSONObjectItem[] languages;
        public ResourceJSONObjectItem[] mediaTypes;
        public ResourceJSONObjectItem[] resourceTypes;
        public ResourceJSONObjectStandard[] standards;
        public ResourceJSONObjectUsageRights usageRights;
        
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

    /// <summary>
    /// ResourceJSONElasticSearch - condensed version used for ElasticSearch index
    /// </summary>
    public class ResourceJSONElasticSearch
    {
        //Single value items
        public string created;
        public string creator;
        public string description;
        public int intID;
        public string submitter;
        public string publisher;
        public string title;
        public string url;
        public int versionID;
        public long timestamp;

        //Multi value items
        public string[] keywords;
        public string[] subjects;
        public string[] urlParts;
        public int[] libraryIDs;
        public int[] collectionIDs;

        //Objects
        public int accessRightsID;
        public string accessRights;
        public int[] languageIDs;
        public string[] languages;
        public int[] clusterIDs;
        public string[] clusters;
        public int[] audienceIDs;
        public string[] audiences;
        public int[] educationalUseIDs;
        public string[] educationalUses;
        public int[] gradeLevelIDs;
        public string[] gradeLevels;
        public string[] gradeLevelAliases;
        public int[] groupTypeIDs;
        public string[] groupTypes;
        public int[] mediaTypeIDs;
        public string[] mediaTypes;
        public int[] resourceTypeIDs;
        public string[] resourceTypes;
        public int[] standardIDs;
        public string[] standardNotations;
        public string[] notationParts;
        public string usageRights;
        public int usageRightsID;
        public string usageRightsURL;
        public string usageRightsIconURL;
        public string usageRightsMiniIconURL;

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

    /// <summary>
    /// ResourceJSONLRMI - used for the Learning Registry publishing envelope
    /// </summary>
    [Serializable]
    public class ResourceJSONLRMI
    {
        public string name;
        public string url;
        public string description;
        public string author;
        public string publisher;
        public string submitter;
        public string isBasedOnUrl;
        public string inLanguage;
        public string dateCreated;
        public string timeRequired;
        public string accessRestrictions;
        public string useRightsUrl;
        public string requires;
        public string[] about;
        public ResourceJSONLRMIEducationalAlignment[] educationalAlignment;
        public ResourceJSONLRMICareerCluster[] careerCluster;
        public string[] educationalRole;
        public string[] typicalAgeRange;
        public string[] educationalUse;
        public string[] learningResourceType;
        public string[] mediaType;
        public string[] groupType;
        public string[] itemType;
    }
    [Serializable]
    public class ResourceJSONLRMIEducationalAlignment
    {
        public string targetName;
        public string alignmentType;
        public string targetUrl;
        public string educationLevel;
        public string educationalFramework;
    }
    [Serializable]
    public class ResourceJSONLRMICareerCluster
    {
        public string country;
        public string region;
        public string value;
    }

    public class ResourceJSONObjectItem
    {
        public int ID;
        public string name;
        public int createdByID;
    }
    public class ResourceJSONObjectGradeLevel : ResourceJSONObjectItem 
    {
        public string[] alias;
    }
    public class ResourceJSONObjectStandard : ResourceJSONObjectItem
    {
        public int alignmentTypeID;
        public string alignmentType;
        public string[] notationParts;
    }
    public class ResourceJSONObjectUsageRights : ResourceJSONObjectItem
    {
        public string usageRightsURL;
        public string usageRightsIconURL;
        public string usageRightsMiniIconURL;
    }
}
