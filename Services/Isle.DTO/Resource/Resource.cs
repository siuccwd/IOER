using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Isle.DTO.Filters;

namespace Isle.DTO.Resource
{
  public class Resource
  {
    public Resource()
    {
      title = "";
      description = "";
      url = "";
      lrDocID = "";
      requirements = "";
      created = "";
      creator = "";
      publisher = "";
      submitter = "";
      keywords = new List<string>();
      usageRights = new UsageRights();
      paradata = new ParadataSummary();
      filters = new List<Filter>();
      standards = new List<Standard>();
      //timeRequired = new TimeRequired();
    }
    public int id { get; set; }
    public int createdByID { get; set; }

    /// <summary>
    /// Will need resource version ID for updates (or assume just use most recent active RV??
    /// </summary>
    public int resourceVersionID { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string lrDocID { get; set; }
    public string requirements { get; set; }
    //public TimeRequired timeRequired { get; set; }
    public string created { get; set; }
    public string creator { get; set; }
    public string publisher { get; set; }
    public string submitter { get; set; }
    public List<string> keywords { get; set; }
    public UsageRights usageRights { get; set; }
    public ParadataSummary paradata { get; set; }

    public List<int> filterIds { get; set; }
    public List<Filter> filters { get; set; }

    public List<int> standardIds { get; set; }
    public List<Standard> standards { get; set; }

    public int accessRightsId { get; set; }
    public List<Tag> accessRights { get; set; }
    public List<Tag> inLanguage { get; set; }

    public List<string> subjects { get; set; }

    public List<int> targetSiteIds { get; set; }
  }

  public class TimeRequired
  {
    public TimeRequired()
    {
      encoded = "";
      summary = "";
    }
    public string encoded { get; set; }
    public string summary { get; set; }
    public int days { get; set; }
    public int hours { get; set; }
    public int minutes { get; set; }
  }

  public class Standard
  {
    public Standard()
    {
      code = "";
      description = "";
    }
    public int id { get; set; }
    public string code { get; set; }
    public string description { get; set; }
  }

  public class UsageRights
  {
    public UsageRights()
    {
      title = "";
      description = "";
      url = "";
      iconURL = "";
      miniIconURL = "";
    }
    public int id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string url { get; set; }
    public string iconURL { get; set; }
    public string miniIconURL { get; set; }
  }

  public class ParadataSummary
  {
    public ParadataSummary()
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
    public class Views
    {
      public int detail { get; set; }
      public int resource { get; set; }
    }
    public class Evaluations
    {
      public int count { get; set; }
      public double score { get; set; }
    }
    public class Ratings
    {
      public int likes { get; set; }
      public int dislikes { get; set; }
      public double score { get; set; }
    }
  }

}
