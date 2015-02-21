using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace LRWarehouse.Business
{
  public class CRSResource
  {
    public CRSResource() 
    {
      keywords = new List<string>();
      urlParts = new List<string>();
      standardAliases = new List<string>();
      gradeLevelAliases = new List<string>();
      libraryIDs = new List<int>();
      collectionIDs = new List<int>();
      isleSectionIDs = new List<int>();
      paradata = new JSONParadataSummary();
      usageRights = new JSONUsageRights();
      accessRights = new CRSField() { title = "Access Rights" };
      accessibilityAPI = new CRSField() { title = "Accessibility API" };
      accessibilityControl = new CRSField() { title = "Accessibility Control" };
      accessibilityFeature = new CRSField() { title = "Accessibility Feature" };
      accessibilityHazard = new CRSField() { title = "Accessibility Hazard" };
      assessmentType = new CRSField() { title = "AssessmentType" };
      educationalRole = new CRSField() { title = "End User" };
      careerCluster = new CRSField() { title = "Career Cluster" };
      training = new CRSField() { title = "Career Planning" };
      disabilityTopic = new CRSField() { title = "Disability Topic" };
      educationalUse = new CRSField() { title = "Educational Use" };
      networking = new CRSField() { title = "Employer Program" };
      gradeLevel = new CRSField() { title = "Grade Level" };
      groupType = new CRSField() { title = "Group Type" };
      itemType = new CRSField() { title = "Item Type" };
      jobs = new CRSField() { title = "Job Preparation" };
      inLanguage = new CRSField() { title = "Access Rights" };
      mediaType = new CRSField() { title = "Media Type" };
      learningResourceType = new CRSField() { title = "Resource Type" };
      k12Subject = new CRSField() { title = "K-12 Subject" };
      resources = new CRSField() { title = "Resource" };
      //workSupportService = new CRSField() { title = "Work Support Service" };
      wfePartner = new CRSField() { title = "Workforce Education Partner" };
      explore = new CRSField() { title = "Workplace Skills" };
      standards = new CRSField() { title = "Standards" };
      //workNetSubject = new CRSField() { title = "workNet Subject" };
      region = new CRSField() { title = "Region" };
      targetSite = new CRSField() { title = "Target Sites" };
      qualify = new CRSField() { title = "Qualify" };
      layoffAssistance = new CRSField() { title = "Layoff Assistance" };
      wioaWorks = new CRSField() { title = "WIOA Works" };
    }
    public int versionID { get; set; }
    public int intID { get; set; }
    public long timestamp { get; set; }
    public int createdByID { get; set; }
    public string title { get; set; }
    public string sortTitle { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string lrDocID { get; set; }
    public string requirements { get; set; }
    public string timeRequired { get; set; }
    public string created { get; set; }
    public string creator { get; set; }
    public string publisher { get; set; }
    public string submitter { get; set; }
    public List<string> keywords { get; set; }
    public List<string> urlParts { get; set; }
    public List<string> standardAliases { get; set; }
    public List<string> gradeLevelAliases { get; set; }
    public List<int> libraryIDs { get; set; }
    public List<int> collectionIDs { get; set; }
    public List<int> isleSectionIDs { get; set; }
    public JSONUsageRights usageRights { get; set; }
    public JSONParadataSummary paradata { get; set; }
    public CRSField accessRights { get; set; }
    public CRSField accessibilityAPI { get; set; }
    public CRSField accessibilityControl { get; set; }
    public CRSField accessibilityFeature { get; set; }
    public CRSField accessibilityHazard { get; set; }
    public CRSField assessmentType { get; set; }
    public CRSField educationalRole { get; set; }
    public CRSField careerCluster { get; set; }
    public CRSField training { get; set; }
    public CRSField disabilityTopic { get; set; }
    public CRSField educationalUse { get; set; }
    public CRSField networking { get; set; }
    public CRSField gradeLevel { get; set; }
    public CRSField groupType { get; set; }
    public CRSField itemType { get; set; }
    public CRSField jobs { get; set; }
    public CRSField inLanguage { get; set; }
    public CRSField mediaType { get; set; }
    public CRSField learningResourceType { get; set; }
    public CRSField k12Subject { get; set; }
    public CRSField resources { get; set; }
    //public CRSField workSupportService { get; set; }
    public CRSField wfePartner { get; set; }
    public CRSField explore { get; set; }
    public CRSField standards { get; set; }
    //public CRSField workNetSubject { get; set; } //workNet subject -- needs a better name ?
    public CRSField region { get; set; }
    public CRSField targetSite { get; set; }
    public CRSField qualify { get; set; }
    public CRSField layoffAssistance { get; set; }
    public CRSField wioaWorks { get; set; }
  }
  public class CRSField
  {
    public CRSField() 
    {
      tags = new List<string>();
      ids = new List<int>();
    }
    public string title { get; set; }
    public List<string> tags { get; set; }
    public List<int> ids { get; set; }
  }

  public class ResourceJSON
  {
    public ResourceJSON()
    {
      keywords = new List<string>();
      urlParts = new List<string>();
      standards = new MultiValueFieldWithAliases();
      usageRights = new JSONUsageRights();
      //fields = new List<MultiValueField>();
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
    public List<string> keywords { get; set; }
    public List<string> urlParts { get; set; }
    public MultiValueFieldWithAliases standards { get; set; }
    public MultiValueFieldWithAliases gradeLevel { get; set; }
    public JSONUsageRights usageRights { get; set; }
    public JSONParadataSummary paradata { get; set; }
    //public List<MultiValueField> fields { get; set; }
    public MultiValueField isleSection { get; set; }
    public MultiValueField alignmentType { get; set; }
    public MultiValueField itemType { get; set; }
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
    public MultiValueField training { get; set; }
    public MultiValueField networking { get; set; }
    public MultiValueField disability { get; set; }
    public MultiValueField jobs { get; set; }
    public MultiValueField veterans { get; set; }
    public MultiValueField workforceEducationPartners { get; set; }
    public MultiValueField explore { get; set; }
    public MultiValueField workSupportServices { get; set; }
  }
  public class MultiValueField
  {
    public MultiValueField()
    {
      titles = new List<string>();
      ids = new List<int>();
    }
    //public string esName { get; set; }
    public string header { get; set; }
    public string type { get; set; }
    public int fieldID { get; set; }
    public List<string> titles { get; set; }
    public List<int> ids { get; set; }
  }
  public class JSONParadataSummary
  {
    public JSONParadataSummary()
    {
      views = new Views();
      evaluations = new Evaluations();
      ratings = new Ratings();
    }
    public int comments { get; set; }
    public int favorites { get; set; }
    public Views views { get; set; }
    public Evaluations evaluations { get; set; }
    public Ratings ratings { get; set; }
    public class Views { 
      public int detail { get; set; } 
      public int resource { get; set; } 
    }
    public class Evaluations { 
      public int count { get; set; } 
      public double score { get; set; } 
    }
    public class Ratings { 
      public int likes { get; set; } 
      public int dislikes { get; set; } 
      public double score { get; set; } 
    }
  }
  public class MultiValueFieldWithAliases : MultiValueField
  {
    public MultiValueFieldWithAliases()
    {
      aliases = new List<string>();
    }
    public List<string> aliases { get; set; }
  }
  public class JSONUsageRights
  {
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
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


  //New as of January 2015
    public class ResourceJSONV7
    {
      public ResourceJSONV7()
      {
        fields = new List<TagCategory>();
      }
      public int versionID { get; set; }
      public int intID { get; set; }
      public string created { get; set; }
      public long timestamp { get; set; }
      public int createdByID { get; set; }
      public string title { get; set; }
      public string sortTitle { get; set; }
      public string description { get; set; }
      public string url { get; set; }
      public string lrDocID { get; set; }
      public string requirements { get; set; }
      public string timeRequired { get; set; }
      public string creator { get; set; }
      public string publisher { get; set; }
      public string submitter { get; set; }
      public List<string> keywords { get; set; }
      public List<string> urlParts { get; set; }
      public List<string> standardAliases { get; set; }
      public List<string> gradeLevelAliases { get; set; }
      public List<int> libraryIDs { get; set; }
      public List<int> collectionIDs { get; set; }
      public List<int> isleSectionIDs { get; set; }
      public JSONUsageRights usageRights { get; set; }
      public JSONParadataSummary paradata { get; set; }
      public List<TagCategory> fields { get; set; }
    }
    public class TagCategory
    {
      public TagCategory()
      {
        tags = new List<string>();
        tagIDs = new List<int>();
      }
      public int catID { get; set; }
      public string cat { get; set; }
      public List<string> tags { get; set; }
      public List<int> tagIDs { get; set; }
    }
}
