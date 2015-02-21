using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRWarehouse.Business
{
    [Serializable]
    public class CodesSiteTagCategory
    {
        #region tag categories
        public static int ACCESS_RIGHTS_CATEGORY_Id = 1;
        public static int ACCESSIBILITY_API_CATEGORY_Id = 2;
        public static int ACCESSIBILITY_CONTROL_CATEGORY_Id = 3;
        public static int ACCESSIBILITY_FEATURE_CATEGORY_Id = 4;
        public static int ACCESSIBILITY_HAZARD_CATEGORY_Id = 5;
        public static int ALIGNMENT_TYPE_CATEGORY_Id = 6;
        public static int AUDIENCE_TYPE_CATEGORY_Id = 7;
        public static int CAREER_CLUSTER_CATEGORY_Id = 8;
        public static int CAREER_PLANNING_CATEGORY_Id = 9;
        public static int DISABILITY_TOPIC_CATEGORY_Id = 10;
        public static int EDUCATIONAL_USE_CATEGORY_Id = 11;
        public static int EMPLOYER_PROGRAM_CATEGORY_Id = 12;
        public static int GRADE_LEVEL_CATEGORY_Id = 13;
        public static int GROUP_TYPE_CATEGORY_Id = 14;
        public static int ITEM_TYPE_CATEGORY_Id = 15;
        public static int JOB_PREPARATION_CATEGORY_Id = 16;
        public static int LANGUAGE_CATEGORY_Id = 17;
        public static int MEDIA_TYPE_CATEGORY_Id = 18;
        public static int RESOURCE_TYPE_CATEGORY_Id = 19;
        public static int K12_SUBJECT_CATEGORY_Id = 20;
        public static int VETERANS_SERVICE_CATEGORY_Id = 21;
        public static int WFE_PARTNER_CATEGORY_Id = 22;
        public static int WORK_SUPPORT_SERVICE_CATEGORY_Id = 23;
        public static int WORK_PLACE_SKILL_CATEGORY_Id = 24;
        public static int ASSESSMENT_TYPE_CATEGORY_Id = 25;
        public static int REGION_CATEGORY_Id = 26;
        public static int TARGET_SITE_CATEGORY_Id = 27;
        public static int WORKNET_SUBJECT_CATEGORY_Id = 28;

        #endregion 

        public CodesSiteTagCategory()
        {
            HasTagCategory = true;
            TagCategory = new CodesTagCategory();
        }
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int CategoryId { get; set; }
        /// <summary>
        /// Allowing site title for a category to differ from the base title
        /// </summary>
        public string Title { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Get/Set SortOrder
        /// Defaults to base TagCategory value on initial created, and then can be overridden to be site specific
        /// </summary>
        public int SortOrder { get; set; }

        //consider flattening TagCategory values here to simplify
        public string Description { get; set; }
        public string SchemaTag { get; set; }

        //public string Description
        //{
        //    get
        //    {
        //        if ( TagCategory != null && TagCategory.Description != null)
        //            return TagCategory.Description;
        //        else
        //            return "";
        //    }
        //}


        //public string SchemaTag
        //{
        //    get
        //    {
        //        if ( TagCategory != null && TagCategory.SchemaTag != null )
        //            return TagCategory.SchemaTag;
        //        else
        //            return "";
        //    }
        //}

        public bool HasTagCategory { get; set; }
        /// <summary>
        /// Get/Set the base TagCategory
        /// TagCategory will contain the TagValues (List<CodesTagValue> )
        /// </summary>
        public CodesTagCategory TagCategory { get; set; }


    }
}
