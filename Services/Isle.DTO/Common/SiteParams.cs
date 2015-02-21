using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Isle.DTO.Common
{
  public class SiteParams
  {
    public SiteParams()
    {
      cssThemes = new List<string>();
    }

    public int siteID { get; set; }
    public string siteTitle { get; set; }
    public List<string> cssThemes { get; set; }
    public bool hasStandardsBrowser { get; set; }
    public string apiRoot { get; set; }

  }
}