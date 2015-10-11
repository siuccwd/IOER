using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;

using MyEntity = LinkChecker2.App_Code.BusObj.ResourceLink;
using Known404Page = LinkChecker2.App_Code.BusObj.Known404Page;
using KnownBadTitle = LinkChecker2.App_Code.BusObj.KnownBadTitle;
using KnownBadContent = LinkChecker2.App_Code.BusObj.KnownBadContent;
using ResourceRedirect = LinkChecker2.App_Code.BusObj.ResourceRedirect;
using MyManager = LinkChecker2.App_Code.DAL.ResourceLinkManager;
using RedirectManager = LinkChecker2.App_Code.DAL.ResourceRedirectManager;
using SearchManager = Isle.BizServices.ResourceV2Services;
using ESManager = LRWarehouse.DAL.ElasticSearchManager;


namespace LinkChecker2
{
    public class ThreadedLinkCheck
    {
        public List<MyEntity> Links { get; set; }
        public bool ThreadComplete;

        MyManager myManager = new MyManager();
        SearchManager searchManager = new SearchManager();
        RedirectManager redirectManager = new RedirectManager();
        ESManager esManager = new ESManager();

        int nbrRedirects = 0;
        int checkedRecords = 0;
        int i = 0;
        string resourceList = "";
        string redirectUrl = "";
        string originalUrl = "";

        public ThreadedLinkCheck()
        {
            this.Links = new List<MyEntity>();
            this.ThreadComplete = false;
        }

        public void CheckLinks()
        {
            foreach (MyEntity link in Links)
            {
                i++;

                nbrRedirects = 0;
                originalUrl = link.ResourceUrl;
                CheckLink(link);
            }//foreach

            this.ThreadComplete = true;
        }

        public void CheckLink(MyEntity link)
        {
            string status = "successful";
            Regex knownBadProtocols = new Regex(@"(^title)|(^docid)|(^grade)", RegexOptions.IgnoreCase);
            Regex knownHttpProtocols = new Regex(@"(^//)|(^http:)|(^https:)|(^www)", RegexOptions.IgnoreCase);
            Regex leadingWhitespace = new Regex(@"^\s*");
            Regex trailingWhitespace = new Regex(@"\s*$");
            link.ResourceUrl = leadingWhitespace.Replace(link.ResourceUrl, "");
            link.ResourceUrl = trailingWhitespace.Replace(link.ResourceUrl, "");
            if (knownBadProtocols.IsMatch(link.ResourceUrl))
            {
                Console.WriteLine(link.ResourceUrl + " known bad protocol.");
                Program.WriteLogFile(link, "Known bad protocol", "Resource deleted from search.");
                link.LastCheckDate = DateTime.Now;
                link.IsDeleted = true;
                //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                searchManager.DeleteResource(link.ResourceIntId);
                myManager.Update(link);
            }
            else if (knownHttpProtocols.IsMatch(link.ResourceUrl))
            {
                HttpWebResponse response = null;
                HttpWebRequest request = null;
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                try
                {
                    link.LastCheckDate = DateTime.Now;
                    if (link.ResourceUrl.IndexOf("//") == 0)
                    {
                        link.ResourceUrl = "http:" + link.ResourceUrl;
                    }
                    else if (link.ResourceUrl.IndexOf("http") != 0)
                    {
                        link.ResourceUrl = "http://" + link.ResourceUrl;
                    }
                    // Check for match to known 404 pages
                    bool isKnown404 = false;
                    // Check for exact match
                    if (Program.known404Pages.Where(x => x.IsRegex == false).ToList().Where(y => y.Url == link.ResourceUrl).Count() > 0)
                    {
                        isKnown404 = true;
                    }
                    // Check for RegEx match
                    if (!isKnown404)
                    {
                        foreach (Known404Page page in Program.known404Pages.Where(x => x.IsRegex == true).ToList())
                        {
                            Regex uri = new Regex(page.Url);
                            if (uri.IsMatch(link.ResourceUrl))
                            {
                                isKnown404 = true;
                                break;
                            }
                        }
                    }
                    if (isKnown404)
                    {
                        throw new System.Web.HttpException(404, "Not Found");
                    }
                    request = (HttpWebRequest)WebRequest.Create(link.ResourceUrl);
                    request.AllowAutoRedirect = false;
                    request.KeepAlive = false; // may fix protocol violation Section=ResponseStatusLine errors.
                    request.Timeout = 15000; //15 seconds
                    request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";
                    response = (HttpWebResponse)request.GetResponse();

                    var urlStatusCode = (int)response.StatusCode;
                    if (urlStatusCode == 200 || response.StatusCode.ToString() == "OK")
                    {
                        Stream stream = response.GetResponseStream();
                        StreamReader sr = new StreamReader(stream);
                        string pageContent = sr.ReadToEnd();

                        // Check for known bad page titles
                        if (Program.knownBadTitles.Select(x => x.HostName.ToLower()).Contains(link.HostName.ToLower()))
                        {
                            string title = ExtractTitleTag(pageContent);
                            if (title != null && title != string.Empty)
                            {
                                List<KnownBadTitle> kbt = Program.knownBadTitles.Where(x => x.HostName.ToLower() == link.HostName.ToLower()).ToList();
                                if (kbt.Where(x => x.TitleIsRegex == false).Select(x => x.Title.ToLower()).Contains(title.ToLower()))
                                {
                                    throw new System.Web.HttpException(404, "Not Found");
                                }
                                kbt = kbt.Where(x => x.TitleIsRegex == true).ToList();
                                foreach (KnownBadTitle bt in kbt)
                                {
                                    Regex regEx = new Regex(bt.Title);
                                    if (regEx.IsMatch(title))
                                    {
                                        throw new System.Web.HttpException(404, "Not Found");
                                    }
                                }
                            }
                        }
                        // Check for known bad content
                        if (ContentCheck.UsesBlackHatTechniques(pageContent))
                        {
                            throw new System.Web.HttpException(404, "Not Found");
                        }
                        List<KnownBadContent> knownBadContentRules = Program.knownBadContent
                            .Where(x => x.HostName.ToLower().Contains(link.HostName.ToLower()) || x.HostName == "all")
                            .ToList();
                        foreach (KnownBadContent badContentRule in knownBadContentRules)
                        {
                            Regex regEx = new Regex(badContentRule.Content, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            if (regEx.IsMatch(pageContent))
                            {
                                throw new System.Web.HttpException(404, "Not Found");
                            }
                        }

                        // Check for redirects using meta refresh
                        int nbrDnsErrors = link.NbrDnsErrors;
                        int nbrInternalServerErrors = link.NbrInternalServerErrors;
                        int nbrTimeouts = link.NbrTimeouts;
                        int nbrUnableToConnect = link.NbrUnableToConnect;
                        Regex metaRefreshRegex = new Regex("<meta\\s+?http-equiv\\s*?=\\s*?['\"]refresh['\"]\\s+?content\\s*?=\\s*?['\"]0;[\\s\\S]+?>", RegexOptions.IgnoreCase);
                        if (metaRefreshRegex.Matches(pageContent).Count > 0)
                        {
                            // We have a match, so check the page the redirection is for
                            nbrRedirects++;
                            if (nbrRedirects <= 10)
                            {
                                string match = metaRefreshRegex.Match(pageContent).Value;
                                Regex urlEx = new Regex("(URL\\s*?=\\s*?['\"]\\S*?['\"])|(URL\\s*?=\\s*?\\S*)", RegexOptions.IgnoreCase);
                                string urlToCheck = urlEx.Match(match).Value;
                                urlToCheck = urlToCheck.Replace("\"", "").Replace("'", "").Replace(">", "");
                                //urlEx = new Regex("url\\s*?=\\s*?['\"]?\\S*?['\"]", RegexOptions.IgnoreCase);
                                urlEx = new Regex("url\\s*?=\\s*?", RegexOptions.IgnoreCase);
                                urlToCheck = urlEx.Replace(urlToCheck, "");
                                link.ResourceUrl = urlToCheck;
                                CheckLink(link);
                            }
                            nbrRedirects--;
                        }

                        if (nbrDnsErrors == link.NbrDnsErrors &&
                            nbrInternalServerErrors == link.NbrInternalServerErrors &&
                            nbrTimeouts == link.NbrTimeouts &&
                            nbrUnableToConnect == link.NbrUnableToConnect)
                        {
                            // Go ahead and update the link since nothing changed, otherwise the update has already been done so skip this part.
                            i++;
                            string percent = (Math.Round(decimal.Divide(i, Links.Count == 0 ? Program.linksCount : Links.Count), 5) * 100).ToString("0.0000000");
                            Console.WriteLine(link.ResourceUrl + " is Okay " + "( " + percent.Substring(0, percent.Length - 2) + "% )");
                            link.NbrDnsErrors = 0;
                            link.NbrInternalServerErrors = 0;
                            link.NbrTimeouts = 0;
                            link.NbrUnableToConnect = 0;
                            myManager.Update(link);
                        }
                        checkedRecords++;
                    }
                    else if (response.StatusCode == HttpStatusCode.RedirectMethod || response.StatusCode == HttpStatusCode.MovedPermanently ||
                        response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    {
                        nbrRedirects++;
                        if (nbrRedirects <= 10)
                        {
                            redirectUrl = response.Headers["Location"].ToString().Replace("////", "//");
                            if (!knownHttpProtocols.IsMatch(redirectUrl) && redirectUrl.IndexOf("http") != 0 && redirectUrl.IndexOf("//") != 0)
                            {
                                if (redirectUrl.IndexOf("/") == 0)
                                {
                                    redirectUrl = "http://" + link.HostName + redirectUrl;
                                }
                                else
                                {
                                    redirectUrl = "http://" + link.HostName + "/" + redirectUrl;
                                }
                            }
                            link.ResourceUrl = redirectUrl;
                            CheckLink(link);
                            if (nbrRedirects == 1) // back to original level
                            {
                                ResourceRedirect redir = new ResourceRedirect();
                                redir.ResourceIntId = link.ResourceIntId;
                                redir.OldUrl = originalUrl;
                                redir.NewUrl = redirectUrl;
                                redirectManager.Create(redir);
                                
                                link.ResourceUrl = originalUrl;
                                myManager.Update(link);
                            }
                        }
                        else
                        {
                            // Too many redirect attempts!
                            Program.WriteLogFile(link, "Too many redirects were attempted.", "No action taken.");
                        }
                        --nbrRedirects;
                    }

                    else
                    {
                        //checkedRecords++;
                        WebException wbx = new WebException("#" + i + " failed with: " + response.StatusCode, null, (WebExceptionStatus)response.StatusCode, response);

                        throw new WebException("#" + i + " failed with: " + response.StatusCode, (WebExceptionStatus)response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.ToString().Contains("The operation has timed out"))
                    {
                        Console.WriteLine(link.ResourceUrl + " timed out.");

                        link.NbrTimeouts++;
                        if (Program.ThresholdReached(link.NbrTimeouts, "timeout"))
                        {
                            link.IsDeleted = true;
                            //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            searchManager.DeleteResource(link.ResourceIntId);
                            Program.WriteLogFile(link, "Timed out threshold exceeded.", "Resource deleted from search.");
                        }
                        else
                        {
                            Program.WriteLogFile(link, "Timed out.", "No action taken.");
                        }
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("(404) Not Found") || ex.ToString().Contains("Not Found"))
                    {
                        Console.WriteLine(link.ResourceUrl + " was not found. (404)");
                        Program.WriteLogFile(link, "Not found (404).", "Resource deleted from search.");
                        link.IsDeleted = true;
                        //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                        searchManager.DeleteResource(link.ResourceIntId);
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("(403) Forbidden"))
                    {
                        Console.WriteLine(link.ResourceUrl + " is Forbidden. (403)");
                        Program.WriteLogFile(link, "Is forbidden (403).", "Resource deleted from search.");
                        link.IsDeleted = true;
                        //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                        searchManager.DeleteResource(link.ResourceIntId);
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("Unable to connect") || ex.ToString().Contains("Server Unavailable"))
                    {
                        Console.WriteLine(link.ResourceUrl + " cannot connect to server.");

                        link.NbrUnableToConnect++;
                        if (Program.ThresholdReached(link.NbrUnableToConnect, "can't connect"))
                        {
                            link.IsDeleted = true;
                            //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            searchManager.DeleteResource(link.ResourceIntId);
                            Program.WriteLogFile(link, "Cannot connect threshold exceeded.", "Resource deleted from search.");
                        }
                        else
                        {
                            Program.WriteLogFile(link, "Cannot connect to server.", "No action taken.");
                        }
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("Too many automatic redirections were attempted"))
                    {
                        // This is probably because you must login.  Ignore this error
                        myManager.Update(link);
                        Program.WriteLogFile(link, "Too many automatic redirections were attempted.", "No action taken.");
                    }
                    else if (ex.ToString().Contains("Invalid URI"))
                    {
                        WebException wex = (WebException)ex;
                        Console.WriteLine(link.ResourceUrl + " Invalid URI");
                        Program.WriteLogFile(link, "Invalid URI", "Resource deleted from search.");
                        link.IsDeleted = true;
                        //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                        searchManager.DeleteResource(link.ResourceIntId);
                        myManager.Update(link);
                    }
                    else if (ex.ToString().Contains("(410) Gone"))
                    {
                        Console.WriteLine(link.ResourceUrl + " is gone (410)");
                        Program.WriteLogFile(link, "Gone (410)", "Resource deleted from search.");
                        link.IsDeleted = true;
                        searchManager.DeleteResource(link.ResourceIntId);
                        myManager.Update(link);
                    }
                    else if (ex is WebException)
                    {
                        WebException wex = (WebException)ex;
                        if (wex.Status == WebExceptionStatus.NameResolutionFailure)
                        {
                            link.NbrDnsErrors++;
                            if (Program.ThresholdReached(link.NbrDnsErrors, "dns"))
                            {
                                link.IsDeleted = true;
                                //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                                searchManager.DeleteResource(link.ResourceIntId);
                                Program.WriteLogFile(link, "DNS error threshold exceeded.", "Resource deleted from search.");
                            }
                            else
                            {
                                Program.WriteLogFile(link, "DNS error.", "No action taken.");
                            }
                            myManager.Update(link);
                        }
                        else if (wex.Status == WebExceptionStatus.TrustFailure)
                        {
                            nbrRedirects++;
                            if (nbrRedirects <= 10)
                            {
                                link.ResourceUrl = link.ResourceUrl.Replace("https://", "http://");
                                CheckLink(link);
                            }
                            else
                            {
                                Program.WriteLogFile(link, "Too many redirects were attempted.", "No action taken.");
                            }
                            --nbrRedirects;
                        }
                        else if (wex.Status == WebExceptionStatus.ConnectionClosed || wex.Status == WebExceptionStatus.ReceiveFailure || wex.Status == WebExceptionStatus.SendFailure)
                        {
                            Console.WriteLine(link.ResourceUrl + " connection to server was closed.");

                            link.NbrUnableToConnect++;
                            if (Program.ThresholdReached(link.NbrUnableToConnect, "can't connect"))
                            {
                                link.IsDeleted = true;
                                //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                                searchManager.DeleteResource(link.ResourceIntId);
                                Program.WriteLogFile(link, "Connection to server was closed threshold exceeded.", "Resource deleted from search.");
                            }
                            else
                            {
                                Program.WriteLogFile(link, "Connection to server was closed.", "No action taken.");
                            }
                            myManager.Update(link);
                        }
                        else if (wex.Status == WebExceptionStatus.SecureChannelFailure)
                        {
                            if (ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
                            {
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                                nbrRedirects++;
                                CheckLink(link);
                                --nbrRedirects;
                                ServicePointManager.SecurityProtocol = (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls);
                            }
                            else if (ServicePointManager.SecurityProtocol == SecurityProtocolType.Ssl3)
                            {
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                                nbrRedirects++;
                                CheckLink(link);
                                --nbrRedirects;
                                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                            }
                            else
                            {
                                Program.WriteLogFile(link, wex.Message + " I don't know what to do with this.", "No action taken.");
                            }
                        }
                        else if (wex.Response != null)
                        {
                            if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.BadRequest)
                            {
                                link.NbrInternalServerErrors++;
                                if (Program.ThresholdReached(link.NbrInternalServerErrors, "internal"))
                                {
                                    link.IsDeleted = true;
                                    //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                                    searchManager.DeleteResource(link.ResourceIntId);
                                    Program.WriteLogFile(link, "Bad Request error (400) threshold exceeded.", "Resource deleted from search.");
                                }
                                else
                                {
                                    Program.WriteLogFile(link, "Bad Request error (400).", "No action taken.");
                                }
                                myManager.Update(link);
                            }
                            else if (((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.InternalServerError)
                            {
                                link.NbrInternalServerErrors++;
                                if (Program.ThresholdReached(link.NbrInternalServerErrors, "internal"))
                                {
                                    link.IsDeleted = true;
                                    //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                                    searchManager.DeleteResource(link.ResourceIntId);
                                    Program.WriteLogFile(link, "Internal Server error (500) threshold exceeded.", "Resource deleted from search.");
                                }
                                else
                                {
                                    Program.WriteLogFile(link, "Internal Server error (500).", "No action taken.");
                                }
                                myManager.Update(link);
                            }
                            else
                            {
                                HttpWebResponse resp = (HttpWebResponse)wex.Response;
                                if (resp.StatusCode == HttpStatusCode.RedirectMethod || resp.StatusCode == HttpStatusCode.MovedPermanently ||
                                    resp.StatusCode == HttpStatusCode.Redirect || resp.StatusCode == HttpStatusCode.TemporaryRedirect)
                                {
                                    nbrRedirects++;
                                    if (nbrRedirects <= 10)
                                    {
                                        redirectUrl = response.Headers["Location"].ToString().Replace("////", "//");
                                        link.ResourceUrl = redirectUrl;
                                        CheckLink(link);
                                        if (nbrRedirects == 1) // back to original level
                                        {
                                            ResourceRedirect redir = new ResourceRedirect();
                                            redir.ResourceIntId = link.ResourceIntId;
                                            redir.OldUrl = originalUrl;
                                            redir.NewUrl = redirectUrl;
                                            redirectManager.Create(redir);
                                        }
                                    }
                                    else
                                    {
                                        // Too many redirect attempts!
                                        Program.WriteLogFile(link, "Too many redirects were attempted.", "No action taken.");
                                    }
                                    --nbrRedirects;
                                }
                                else
                                {
                                    MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                                    Program.WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                                }
                            }
                        }
                        else
                        {
                            MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                            Program.WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                        }
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                }
            }
            else if (link.ResourceUrl.IndexOf("ftp:") > -1)
            {
                FtpWebResponse response = null;
                StreamReader reader = null;
                try
                {
                    link.LastCheckDate = DateTime.Now;
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(link.ResourceUrl);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    response = (FtpWebResponse)request.GetResponse();
                    if ((int)response.StatusCode == 150 || (int)response.StatusCode == 125)
                    {
                        // OK, file found
                        Stream responseStream = response.GetResponseStream();
                        reader = new StreamReader(responseStream);
                        reader.ReadToEnd();
                        string percent = (Math.Round(decimal.Divide(i, Links.Count), 5) * 100).ToString("0.0000000");
                        Console.WriteLine(link.ResourceUrl + " is Okay " + "( " + percent.Substring(0, percent.Length - 2) + "% )");
                        link.NbrDnsErrors = 0;
                        link.NbrInternalServerErrors = 0;
                        link.NbrTimeouts = 0;
                        myManager.Update(link);
                        checkedRecords++;
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.IndexOf("The remote name could not be resolved: ") > -1)
                    {
                        link.NbrDnsErrors++;
                        if (Program.ThresholdReached(link.NbrDnsErrors, "dns"))
                        {
                            link.IsDeleted = true;
                            //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            searchManager.DeleteResource(link.ResourceIntId);
                            Program.WriteLogFile(link, "DNS error threshold exceeded.", "Resource deleted from search.");
                        }
                        else
                        {
                            Program.WriteLogFile(link, "DNS error.", "No action taken.");
                        }
                        myManager.Update(link);
                    }
                    else if (ex.Message.IndexOf("Unable to connect") > -1)
                    {
                        Console.WriteLine(link.ResourceUrl + " cannot connect to server.");

                        link.NbrUnableToConnect++;
                        if (Program.ThresholdReached(link.NbrUnableToConnect, "can't connect"))
                        {
                            link.IsDeleted = true;
                            //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            searchManager.DeleteResource(link.ResourceIntId);
                            Program.WriteLogFile(link, "Cannot connect threshold exceeded.", "Resource deleted from search.");
                        }
                        else
                        {
                            Program.WriteLogFile(link, "Cannot connect to server.", "No action taken.");
                        }
                        myManager.Update(link);
                    }
                    else if (ex.Message.IndexOf("The requested URI is invalid for this FTP command") > -1)
                    {
                        link.NbrUnableToConnect++;
                        if (Program.ThresholdReached(link.NbrUnableToConnect, "can't connect"))
                        {
                            link.IsDeleted = true;
                            //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            searchManager.DeleteResource(link.ResourceIntId);
                            Program.WriteLogFile(link, ex.Message, "Resource deleted from search.");
                        }
                        else
                        {
                            Program.WriteLogFile(link, ex.Message, "No action taken.");
                        }
                        myManager.Update(link);
                    }
                    else if (ex is WebException)
                    {
                        WebException wex = (WebException)ex;
                        if ((int)((FtpWebResponse)(wex.Response)).StatusCode == 550)
                        {
                            // Not found
                            Console.WriteLine(link.ResourceUrl + " was not found. (550)");
                            Program.WriteLogFile(link, "Not found (550).", "Resource deleted from search.");
                            link.IsDeleted = true;
                            //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            searchManager.DeleteResource(link.ResourceIntId);
                            myManager.Update(link);
                        }
                        else if ((int)((FtpWebResponse)(wex.Response)).StatusCode == 530)
                        {
                            // Not logged in
                            Console.WriteLine(link.ResourceUrl + " not logged in. (530)");
                            Program.WriteLogFile(link, "Not logged in (530).", "Resource deleted from search.");
                            link.IsDeleted = true;
                            //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                            searchManager.DeleteResource(link.ResourceIntId);
                            myManager.Update(link);
                        }
                        else if ((int)((FtpWebResponse)(wex.Response)).StatusCode == 450)
                        {
                            // Internal error (temporary)
                            Console.WriteLine(link.ResourceUrl + " Temporary internal error. (450).  Try again later.");
                            link.NbrInternalServerErrors++;
                            if (Program.ThresholdReached(link.NbrInternalServerErrors, "InternalThreshold"))
                            {
                                Program.WriteLogFile(link, "Temporary internal error (450) threshold exceeded.", "Resource deleted from search.");
                                link.IsDeleted = true;
                                //searchManager.DeleteByIntID(link.ResourceIntId, ref status);
                                searchManager.DeleteResource(link.ResourceIntId);
                            }
                            else
                            {
                                Program.WriteLogFile(link, "Temporary internal error (450).  Try again later.", "No action taken.");
                            }
                            myManager.Update(link);
                        }
                        else
                        {
                            MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                            Program.WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                        }
                    }
                    else
                    {
                        MyManager.LogError(link.ResourceUrl + " " + ex.ToString());
                        Program.WriteLogFile(link, ex.Message + " I don't know what to do with this.", "No action taken.");
                    }
                }
                finally
                {
                    if (response != null)
                    {
                        response.Close();
                    }
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            else
            {
                MyManager.LogError("Unknown Protocol for URL " + link.ResourceUrl, false);
                Program.WriteLogFile(link, "Unknown protocol.", "No action taken.");
            }

        }

        string ExtractTitleTag(string pageContent)
        {
            // Avoid malformed XHTML by extracting only the title tag
            Regex titleEx = new Regex(@"<title([\s\S]*?)>([\s\S])*?</title>");
            Match titleMatch = titleEx.Match(pageContent);
            if (titleMatch != null)
            {
                string title = titleEx.Match(pageContent).Value;
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml("<root>\n" + title + "\n</root>");
                XmlNodeList nodes = xdoc.GetElementsByTagName("title");
                if (nodes != null && nodes.Count > 0)
                {
                    title = TrimWhitespace(nodes[0].InnerXml);
                    return title;
                }
            }
            return null;
        }

        /// <summary>
        /// Regular "Trim()" does not handle \r and \n.  This handles it.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        string TrimWhitespace(string str)
        {
            Regex leadingWhitespace = new Regex(@"^\s+");
            Regex trailingWhitespace = new Regex(@"\s+$");

            str = leadingWhitespace.Replace(str, "");
            str = trailingWhitespace.Replace(str, "");

            return str;
        }

    }
}
