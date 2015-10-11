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
    public class ResourceRedirectManager : BaseDataManager
    {
        public string Create(ResourceRedirect entity)
        {
            string status = "successful";
            try
            {
                SqlParameter[] parms = new SqlParameter[3];
                parms[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);
                parms[1] = new SqlParameter("@OldUrl", entity.OldUrl);
                parms[2] = new SqlParameter("@NewUrl", entity.NewUrl);

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.RedirectAdd]", parms);
            }
            catch (Exception ex)
            {
                status = ex.Message;
                LogError("ResourceRedirectManager.Create(): " + ex.ToString());
            }

            return status;
        }// Create

        public string Delete(ResourceRedirect entity)
        {
            string status = "successful";
            try
            {
                SqlParameter[] parms = new SqlParameter[1];
                parms[0] = new SqlParameter("@ResourceIntId", entity.ResourceIntId);

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.RedirectDelete]", parms);
            }
            catch (Exception ex)
            {
                status = ex.Message;
                LogError("ResourceRedirectManager.Delete(): " + ex.ToString());
            }

            return status;
        }// Delete

        public List<ResourceRedirect> Select(string filter, ref string status)
        {
            status = "successful";
            List<ResourceRedirect> list = new List<ResourceRedirect>();

            if (filter.ToLower().IndexOf("where") == -1 && filter.Length > 0)
            {
                filter = " WHERE " + filter;
            }

            try
            {
                SqlParameter[] parms = new SqlParameter[1];
                parms[0] = new SqlParameter("@filter", filter);

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.RedirectSelect]", parms);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ResourceRedirect redirect = new ResourceRedirect();
                        redirect.ResourceIntId = GetRowColumn(dr, "ResourceIntId", 0);
                        redirect.OldUrl = GetRowColumn(dr, "OldUrl", "");
                        redirect.NewUrl = GetRowColumn(dr, "NewUrl", "");
                        redirect.Created = GetRowColumn(dr, "Created", DateTime.Now);
                        list.Add(redirect);
                    }
                }
            }
            catch (Exception ex)
            {
                status = ex.Message;
                LogError("ResourceRedirectManager.Select(): " + ex.ToString());
            }

            return list;
        }// Select

        public DataSet GetResourceCollections(int resourceIntId, ref string status)
        {
            status = "successful";

            try
            {
                SqlParameter[] parms = new SqlParameter[1];
                parms[0] = new SqlParameter("@ResourceIntId", resourceIntId);

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[Resource.CollectionGet]", parms);
                if (DoesDataSetHaveRows(ds))
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                status = "ResourceRedirectManager.GetResourceCollections(): " + ex.Message;
                LogError("ResourceRedirectManager.GetResourceCollections(): " + ex.ToString());
                return null;
            }
        }

        public string AddResourceCollection(int resourceIntId, int collectionId, DateTime created, int createdById)
        {
            string status = "successful";

            try
            {
                SqlParameter[] parms = new SqlParameter[4];
                parms[0] = new SqlParameter("@ResourceIntId", resourceIntId);
                parms[1] = new SqlParameter("@LibrarySectionId", collectionId);
                parms[2] = new SqlParameter("@Created", created);
                parms[3] = new SqlParameter("@CreatedById", createdById);

                SqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, "[Resource.CollectionAdd]", parms);
            }
            catch (Exception ex)
            {
                status = "Resource.RedirectManager.AddResourceCollection(): " + ex.Message;
                LogError("Resource.RedirectManager.AddResourceCollection(): " + ex.ToString());
            }

            return status;
        }
    }
}
