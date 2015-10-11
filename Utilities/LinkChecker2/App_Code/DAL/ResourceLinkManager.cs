using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using SqlHelper = Microsoft.ApplicationBlocks.Data.SqlHelper;
using System.Linq;
using System.Text;
using LinkChecker2.App_Code.BusObj;

namespace LinkChecker2.App_Code.DAL
{
    public class ResourceLinkManager : BaseDataManager
    {

        public void LinkAddUpdate()
        {
            SqlConnection conn = new SqlConnection(ConnString);
            conn.Open();
            try
            {
                SqlCommand cmd = new SqlCommand("EXEC [Resource.LinkAddUpdate]", conn);
                cmd.CommandTimeout = 6000; // Up to 10 minutes (as of 8/9/13, this took 6'20" on test server
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.LinkAddUpdate(): " + ex.ToString());
            }
            finally
            {
                conn.Close();
            }
        }

        public List<ResourceLink> GetLeastRecentlyChecked()
        {
            int maxRows = GetAppKeyValue("nbrRowsToCheck", 20000);

            return GetLeastRecentlyChecked(maxRows);
        }

        public List<ResourceLink> GetLeastRecentlyChecked(int maxRows)
        {
            List<ResourceLink> list = new List<ResourceLink>(maxRows);

            SqlParameter[] parameter = new SqlParameter[1];
            parameter[0] = new SqlParameter("@MaxRows", SqlDbType.Int);
            parameter[0].Value = maxRows;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.LinkSelectLeastRecentlyChecked]", parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ResourceLink link = Fill(dr);
                        list.Add(link);
                    }
                    return list;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.GetLeastRecentlyChecked(): " + ex.ToString());
                return null;
            }
        }

        public List<ResourceLink> GetItemsForPhase2()
        {
            List<ResourceLink> list = new List<ResourceLink>();
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.LinkSelectForPhase2]");
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ResourceLink link = Fill(dr);
                        list.Add(link);
                    }
                    return list;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.GetItemsForPhase2(): " + ex.ToString());
                return null;
            }
        }


        public ResourceLink Fill(DataRow dr)
        {
            ResourceLink link = new ResourceLink();
            link.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
            link.LastCheckDate = GetRowColumn(dr, "LastCheckDate", DateTime.Now);
            link.HostName = GetRowColumn(dr, "HostName", "");
            link.IsDeleted = GetRowColumn(dr, "IsDeleted", false);
            link.NbrDnsErrors = GetRowColumn(dr, "NbrDnsErrors", 0);
            link.NbrInternalServerErrors = GetRowColumn(dr, "NbrInternalServerErrors", 0);
            link.NbrTimeouts = GetRowColumn(dr, "NbrTimeouts", 0);
            link.ResourceUrl = GetRowColumn(dr, "ResourceUrl", "");
            link.NbrUnableToConnect = GetRowColumn(dr, "NbrUnableToConnect", 0);

            return link;
        }

        public ResourceLink Get(int resourceIntId)
        {

            SqlParameter[] parameter = new SqlParameter[1];
            parameter[0] = new SqlParameter("@ResourceIntId", SqlDbType.Int);
            parameter[0].Value = resourceIntId;

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.LinkGet]", parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    ResourceLink link = Fill(ds.Tables[0].Rows[0]);
                    return link;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.Get(): " + ex.ToString());
                return null;
            }
        }

        public ResourceLink GetByUrl(string url)
        {
            SqlParameter[] parameter = new SqlParameter[1];
            parameter[0] = new SqlParameter("@Url", url);

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.LinkGetByUrl]", parameter);
                if (DoesDataSetHaveRows(ds))
                {
                    ResourceLink link = Fill(ds.Tables[0].Rows[0]);
                    return link;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.Get(): " + ex.ToString());
                return null;
            }
        }

        public string Update(ResourceLink link)
        {
            string status = "successful";
            SqlParameter[] parameter = new SqlParameter[7];
            parameter[0] = new SqlParameter("@ResourceIntId", SqlDbType.Int);
            parameter[0].Value = link.ResourceIntId;
            parameter[1] = new SqlParameter("@LastCheckDate", SqlDbType.DateTime);
            parameter[1].Value = link.LastCheckDate;
            parameter[2] = new SqlParameter("@IsDeleted", SqlDbType.Bit);
            parameter[2].Value = link.IsDeleted;
            parameter[3] = new SqlParameter("@NbrDnsErrors", SqlDbType.Int);
            parameter[3].Value = link.NbrDnsErrors;
            parameter[4] = new SqlParameter("@NbrTimeouts", SqlDbType.Int);
            parameter[4].Value = link.NbrTimeouts;
            parameter[5] = new SqlParameter("@NbrInternalServerErrors", SqlDbType.Int);
            parameter[5].Value = link.NbrInternalServerErrors;
            parameter[6] = new SqlParameter("@NbrUnableToConnect", SqlDbType.Int);
            parameter[6].Value = link.NbrUnableToConnect;

            try
            {
                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.LinkUpdate]", parameter);
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.Update(): " + ex.ToString());
                status = "ResourceLinkManager.Update(): " + ex.Message;
            }

            return status;
        }

        public List<int> GetDeletedResourceIntIds(ref string status)
        {
            List<int> ints = new List<int>(); status = "successful";
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.Text, "SELECT ResourceIntId FROM [Resource.Link] WHERE IsDeleted = 'True'");

                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        int resIntId = GetRowColumn(dr, "ResourceIntId", 0);
                        ints.Add(resIntId);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.GetDeletedResourceIntIds(): " + ex.ToString());
                status = "ResourceLinkManager.GetDeletedResourceIntIds(): " + ex.Message;
            }

            return ints;
        }

        #region ====== Get Bad Page/Title/Content Rules ======

        public List<Known404Page> GetKnown404Pages()
        {
            List<Known404Page> pages = new List<Known404Page>();
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "Known404GetAll");
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Known404Page page = new Known404Page
                        {
                            Id = GetRowColumn(dr, "Id", 0),
                            Url = GetRowColumn(dr, "Url", ""),
                            IsRegex = GetRowColumn(dr, "IsRegEx", false),
                            Created = GetRowColumn(dr, "Created", DateTime.Now),
                            CreatedBy = GetRowColumn(dr, "CreatedBy", ""),
                            LastUpdated = GetRowColumn(dr, "LastUpdated", DateTime.Now),
                            LastUpdatedBy = GetRowColumn(dr, "LastUpdatedBy", "")
                        };
                        pages.Add(page);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.GetKnown404PageByUrl(): " + ex.ToString());
                return null;
            }
            return pages;
        }

        public List<KnownBadTitle> GetKnownBadTitles()
        {
            List<KnownBadTitle> titles = new List<KnownBadTitle>();
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "KnownBadTitleGetAll");
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        KnownBadTitle title = new KnownBadTitle
                        {
                            Id = GetRowColumn(dr, "Id", 0),
                            HostName = GetRowColumn(dr, "HostName", ""),
                            Title = GetRowColumn(dr, "Title", ""),
                            TitleIsRegex = GetRowColumn(dr, "TitleIsRegex", false),
                            Created = GetRowColumn(dr, "Created", DateTime.Now),
                            CreatedBy = GetRowColumn(dr, "CreatedBy", ""),
                            LastUpdated = GetRowColumn(dr, "LastUpdated", DateTime.Now),
                            LastUpdatedBy = GetRowColumn(dr, "LastUpdatedBy", "")
                        };
                        titles.Add(title);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.GetKnownBadTitles(): " + ex.ToString());
                return null;
            }

            return titles;
        }

        public List<KnownBadContent> GetKnownBadContentRules()
        {
            List<KnownBadContent> badContentItems = new List<KnownBadContent>();
            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "KnownBadContentGetAll");
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        KnownBadContent badContentItem = new KnownBadContent
                        {
                            Id = GetRowColumn(dr, "Id", 0),
                            HostName = GetRowColumn(dr, "HostName", ""),
                            Content = GetRowColumn(dr, "Content", ""),
                            Created = GetRowColumn(dr, "Created", DateTime.Now),
                            CreatedBy = GetRowColumn(dr, "CreatedBy", ""),
                            LastUpdated = GetRowColumn(dr, "LastUpdated", DateTime.Now),
                            LastUpdatedBy = GetRowColumn(dr, "LastUpdatedBy", "")
                        };
                        badContentItems.Add(badContentItem);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("ResourceLinkManager.GetKnownBadTitles(): " + ex.ToString());
                return null;
            }

            return badContentItems;
        }
        #endregion
    }
}
