using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using System.Linq;
using System.Text;


namespace LinkChecker.library
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
            int maxRows = GetAppKeyValue( "nbrRowsToCheck", 20000 );

            return GetLeastRecentlyChecked( maxRows );
        }

        public List<ResourceLink> GetLeastRecentlyChecked( int maxRows )
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


        public static string SetResourceActiveState( int resourceId, bool isActive )
        {
            string status = "successful";
            try
            {
                SqlParameter[] arParms = new SqlParameter[ 2 ];
                arParms[ 0 ] = new SqlParameter( "@ResourceId", resourceId );
                arParms[ 1 ] = new SqlParameter( "@IsActive", isActive );

                SqlHelper.ExecuteNonQuery( WorkNet2013Connection(), CommandType.StoredProcedure, "Resource_SetActiveState", arParms );
            }
            catch ( Exception ex )
            {
                LogError(  "ResourceLinkManager.SetResourceActiveState(): " + ex.ToString() );
                status = ex.Message;
            }
            return status;
        }

    }
}
