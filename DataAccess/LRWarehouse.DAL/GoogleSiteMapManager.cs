using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.Business;

namespace LRWarehouse.DAL
{
    public class GoogleSiteMapManager : BaseDataManager
    {
        public GoogleSiteMapManager()
        {
            // Set up connection strings
            ConnString = LRWarehouse();
            ReadOnlyConnString = LRWarehouseRO();
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
    }
}
