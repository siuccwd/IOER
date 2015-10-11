using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinkChecker2.App_Code.BusObj
{
    public class ResourceBase
    {
        public ResourceBase()
        {
            Keywords = new List<string>();
            IsleSectionIds = new List<int>();
            UsageRights = new UsageRights();
        }
        public int ResourceId { get; set; }
        public int VersionId { get; set; } //Hopefully we won't need this one day!
        public string LrDocId { get; set; }
        public string Title { get; set; }
        public string UrlTitle { get; set; } //aka SortTitle
        public string Description { get; set; }
        public string Requirements { get; set; }
        public string Url { get; set; }
        public string ResourceCreated { get; set; } //stringified date
        public string Creator { get; set; }
        public string Publisher { get; set; }
        public string Submitter { get; set; }
        public string ThumbnailUrl { get; set; }
        public List<string> Keywords { get; set; }
        public List<int> IsleSectionIds { get; set; }
        public UsageRights UsageRights { get; set; }
    }


    //Paradata for use with Elastic Search
    public class ParadataES : ParadataBase
    {
        public int Comments { get; set; }
        public int Evaluations { get; set; }
        public double EvaluationsScore { get; set; }
        public double Rating { get; set; }
    }

    //Field for use with ElasticSearch. Contains tags in a different format
    public class FieldES : FieldBase
    {
        public FieldES()
        {
            Ids = new List<int>();
            Tags = new List<string>();
        }
        public List<int> Ids { get; set; }
        public List<string> Tags { get; set; }
    }

    public class ParadataBase
    {
        public int Favorites { get; set; }
        public int ResourceViews { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }

    //Base TagCategoryField thing
    public class FieldBase
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Schema { get; set; }
    }

    //Base tag
    public class TagBase
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class UsageRights
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string IconUrl { get; set; }
        public string MiniIconUrl { get; set; }
    }

}
