using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    /// <summary>
    /// Class for a Code Tag Category (a tag value for searching and tagging)
    /// </summary>
    [Serializable]
    public class CodesTagCategory
    {
        public static int TAG_CATEGORY_AccessRights = 1;
        public static int TAG_CATEGORY_AccessibilityApi = 2;
        public static int TAG_CATEGORY_AccessibilityControl  = 3;
        public static int TAG_CATEGORY_AccessibilityFeature = 4;
        public static int TAG_CATEGORY_AccessibilityHazard = 5;
        public static int TAG_CATEGORY_AlignmentType = 6;
        public static int TAG_CATEGORY_AudienceType = 7;
        public static int TAG_CATEGORY_CareerCluster = 8;
        public static int TAG_CATEGORY_CareerPlanning = 9;
        public static int TAG_CATEGORY_DisabilityTopic = 10;

        public static int TAG_CATEGORY_EducationalUse = 11;
        public static int TAG_CATEGORY_Employer = 12;
        public static int TAG_CATEGORY_GradeLevel = 13;
        public static int TAG_CATEGORY_GroupType = 14;
        public static int TAG_CATEGORY_ItemType = 15;
        public static int TAG_CATEGORY_JobPreparation = 16;
        public static int TAG_CATEGORY_Language = 17;
        public static int TAG_CATEGORY_ResourceFormat = 18;
        public static int TAG_CATEGORY_ResourceType = 19;
        public static int TAG_CATEGORY_Subject = 20;

        public static int TAG_CATEGORY_VeteransService = 21;
        public static int TAG_CATEGORY_WorkforceEducationPartner = 22;
        public static int TAG_CATEGORY_WorkSupportService = 23;
        public static int TAG_CATEGORY_WorkplaceSkills = 24;
        public static int TAG_CATEGORY_AssessmentType = 25;
        public static int TAG_CATEGORY_Region = 26;

        public CodesTagCategory()
        {
            Title = "";
            Description = "";
            SchemaTag = "";
            SortOrder = 10;
            IsActive = true;
        }
    
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public string SchemaTag { get; set; }
    
        public List<CodesTagValue> TagValues {get; set;}
    
    }
}
