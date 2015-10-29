using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.Business;

namespace LRWarehouse.DAL
{
    public class GoogleSiteMapManager : BaseDataManager
    {
        Regex badTitleCharacters;
        string className;

        public GoogleSiteMapManager()
        {
            // Set up connection strings
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
            badTitleCharacters = new Regex(@"[^0-9A-Za-z_\-]");
            className = "GoogleSiteMapManager";
        }

        public List<string> GetDetailPageEntries(ref string status)
        {
            status = "successful";
            List<string> entries = new List<string>();
            string detailPageUrlTemplate = GetAppKeyValue("detailPageUrlTemplate", "");
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[SiteMap.DetailPagesGet]");
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int resourceIntId = GetRowColumn(dr, "Id", 0);
                        string sortTitle = GetRowColumn(dr, "SortTitle", "");
                        sortTitle = ResourceVersion.UrlFriendlyTitle(sortTitle);
                        sortTitle = HttpUtility.HtmlEncode(sortTitle);
                        sortTitle = badTitleCharacters.Replace(sortTitle, "");
                        string entry = string.Format(detailPageUrlTemplate, resourceIntId, sortTitle);

                        entries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GoogleSiteMapManager.GetDetailPageEntries(): ");
                status = ex.Message;
            }

            return entries;
        }

        public List<string> GetLibraryEntries(ref string status)
        {
            status = "successful";
            List<string> entries = new List<string>();
            string libraryPageUrlTemplate = GetAppKeyValue("libraryPageUrlTemplate", "");

            SqlParameter[] parms = new SqlParameter[5];
            parms[0] = new SqlParameter("@Filter", "lib.IsDiscoverable = 'True' AND lib.PublicAccessLevel > 1" );
            parms[1] = new SqlParameter("@SortOrder", "lib.Id DESC");
            parms[2] = new SqlParameter("@StartPageIndex", 1);
            parms[3] = new SqlParameter("@PageSize", int.MaxValue);
            parms[4] = new SqlParameter("@TotalRows", 0);
            parms[4].Direction = ParameterDirection.Output;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "IsleContent.dbo.[LibrarySearch]", parms);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int libraryId = GetRowColumn(dr, "Id", 0);
                        string libraryTitle = GetRowColumn(dr, "Title", "");
                        libraryTitle = ResourceVersion.UrlFriendlyTitle(libraryTitle);
                        libraryTitle = HttpUtility.HtmlEncode(libraryTitle);
                        libraryTitle = badTitleCharacters.Replace(libraryTitle, "");

                        string entry = string.Format(libraryPageUrlTemplate, libraryId, libraryTitle);
                        entries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GoogleSiteMapManager.GetLibraryEntries(): ");
                status = ex.Message;
            }

            return entries;
        }

        public List<string> GetLearningListEntries(ref string status)
        {
            status = "successful";
            string documentTemplate = GetAppKeyValue("documentPageUrlTemplate", "http://ioer.ilsharedlearning.org{0}");
            string urlTemplate = GetAppKeyValue("urlTemplate", "{0}");
            string learningListNodeTemplate = GetAppKeyValue("learningListNodeTemplate","http://ioer.ilsharedlearning.org/learninglist/{0}/{1}");
            List<string> retVal = new List<string>();
            DataSet ds = new DataSet();

            try
            {
                ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "IsleContent.dbo.[SiteMap.LearningListNodesGet]");
            }
            catch (Exception ex)
            {
                LogError(this.className + ".GetLearningListEntries(): " + ex.ToString());
                status = ex.Message;
            }

            if (DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string url = string.Empty;
                    int id = GetRowColumn(dr, "Id", 0);
                    int typeId = GetRowColumn(dr, "TypeId", 0);
                    string title = GetRowPossibleColumn(dr, "Title", "");
                    title = ResourceVersion.UrlFriendlyTitle(title);
                    title = HttpUtility.HtmlEncode(title);
                    title = badTitleCharacters.Replace(title, "");
                    switch (typeId)
                    {
                        case 40:
                            string relativeUrl = GetRowPossibleColumn(dr, "DocumentUrl", "");
                            url = string.Format(documentTemplate, relativeUrl);
                            break;
                        case 41:
                            string absoluteUrl = GetRowPossibleColumn(dr, "DocumentUrl", "");
                            url = string.Format(urlTemplate, absoluteUrl);
                            break;
                        case 50:
                            url = string.Format(learningListNodeTemplate, id, title);
                            break;
                        case 52:
                            url = string.Format(learningListNodeTemplate, id, title);
                            break;
                        case 54:
                            url = string.Format(learningListNodeTemplate, id, title);
                            break;
                        case 56:
                            url = string.Format(learningListNodeTemplate, id, title);
                            break;
                    }

                    retVal.Add(url);
                }
            }

            // Exclude external URLs from site map
            Regex ioerMatch = new Regex(GetAppKeyValue("ioerRegex",""),RegexOptions.IgnoreCase);
            retVal = retVal.Where(x => ioerMatch.IsMatch(x)).ToList();
            return retVal;
        }
    }
}
