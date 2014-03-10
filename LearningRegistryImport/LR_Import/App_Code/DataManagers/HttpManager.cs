using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace LearningRegistryCache2.App_Code.DataManagers
{
  public class HttpManager
  {
    public HttpManager()
    {
    }

    public string GetPageTitle(string url, ref string status)
    {
      status = "successful";
      string title = "";
      string page = GetPage(url, ref status);
      if (page == "")
      {
          return "";
      }
      else
      {
          Regex titleRegex = new Regex("<title>(.*)</title>");
          Match titleMatch = titleRegex.Match(page);
          title = titleMatch.Value.Replace("<title>", "").Replace("</title>", "");

          return title;
      }
    }

    public string GetPage(string url, ref string status)
    {
        string page = "";
        status = "successful";
        try
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream());
            page = sr.ReadToEnd().Trim();

            return page;
        }
        catch (Exception ex)
        {
            BaseDataManager.LogError("HttpManager.GetPage(): " + ex.ToString());
            status = "HttpManager.GetPage(): " + ex.Message;
        }

        return page;
    }
  }
}
