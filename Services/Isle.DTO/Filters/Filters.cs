using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Isle.DTO.Filters
{
  public class TagFilterBase
  {

      /// <summary>
      /// Codes.TagValue.Id
      /// </summary>
    public int id { get; set; }
    public string title { get; set; }
    /// <summary>
    /// Codes.TagValue.CodeId
    /// - not part of a filter, only a tag!
    /// </summary>
    public int codeID { get; set; }

    [JsonIgnore]
    public bool serializeDescription { get; set; }
    public bool ShouldSerializedescription() { return serializeDescription; }
    public string description { get; set; }
  }

  public class Filter : TagFilterBase
  {
    public Filter()
    {
      tags = new List<Tag>();
    }
    public string schema { get; set; }
    public List<Tag> tags { get; set; }
  }

  public class Tag : TagFilterBase
  {

    public Tag()
    {
        keywords = new List<string>();
        tagValueKeywords = new List<TagValueKeyword>();
        hasKeywords = false;
    }

    [JsonIgnore]
    public bool serializeSelected { get; set; }
    public bool ShouldSerializeselected() { return serializeSelected; }
    public bool selected { get; set; }


    public bool hasKeywords { get; set; }
    /// <summary>
    /// Optional template keywords for tagging
    /// </summary>
    public List<string> keywords { get; set; }
    public List<TagValueKeyword> tagValueKeywords { get; set; }

    [JsonIgnore]
    public bool serializeCounts { get; set; }
    public bool ShouldSerializecounts() { return serializeCounts; }
    public int counts { get; set; }
  }

  public class CachedFilters
  {
    public CachedFilters()
    {
      filters = new List<Filter>();
      lastUpdated = DateTime.Now;
    }
    public List<Filter> filters { get; set; }
    public DateTime lastUpdated { get; set; }
    public int siteID { get; set; }
    public bool mustHaveValues { get; set; }
    public bool includeDescriptions { get; set; }
    public bool includeCounts { get; set; }
  }

  public class TagValueKeyword
  {
      public int Id { get; set; }
      public int TagValueId { get; set; }
      public string Keyword { get; set; }
  }
}
