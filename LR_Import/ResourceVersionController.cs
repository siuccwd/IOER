using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

using LRWarehouse.Business;
using LRWarehouse.DAL;
using LearningRegistryCache2;

namespace LR_Import
{
    public class ResourceVersionController : BaseDataManager
    {
        static string className = "ResourceVersionController";

        private AuditReportingManager auditManager;
        ResourcePropertyManager propertyManager;
        ResourceSubjectManager subjectManager;
        ResourceGradeLevelManager educationLevelManager;
        ResourceStandardManager standardManager;
        ResourceIntendedAudienceManager audienceManager;
        ResourceTypeManager typeManager;
        ResourceFormatManager formatManager;

        ResourceManager resourceManager;
        ResourceVersionManager versionManager;


        public ResourceVersionController()
        {
            auditManager = new AuditReportingManager();
            propertyManager=new ResourcePropertyManager();
            subjectManager=new ResourceSubjectManager();
            educationLevelManager=new ResourceGradeLevelManager();
            standardManager=new ResourceStandardManager();
            audienceManager=new ResourceIntendedAudienceManager();
            typeManager=new ResourceTypeManager();
            formatManager=new ResourceFormatManager();

            resourceManager = new ResourceManager();
            versionManager = new ResourceVersionManager();
        }

        public Resource GetByResourceUrlAndSubmitter(string resourceUrl, string submitter)
        {
            string status = "successful";
            string filter = string.Format("ResourceUrl = '{0}' AND Submitter = '{1}'", resourceUrl, submitter.Replace("'", "''"));

            try
            {
                Resource resource = resourceManager.GetByResourceUrl(resourceUrl, ref status);
                if (status != "successful")
                {
                    auditManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", "", ErrorType.Error, ErrorRouting.Technical,
                        status);
                }
                if (resource == null)
                {
                    return null;
                }
                else
                {
                    DataSet ds = versionManager.Select(filter);
                    if (DoesDataSetHaveRows(ds))
                    {
                        resource.Version = versionManager.Fill(ds.Tables[0].Rows[0], false);
                    }
                }
                return resource;
            }
            catch (Exception ex)
            {
                auditManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", "", ErrorType.Error, ErrorRouting.Technical,
                    ex.ToString());
                LogError(className + ".GetByResourceUrlAndSubmitter(): " + ex.ToString());
                return null;
            }

        }

        public Resource GetActiveVersionByResourceUrl(string resourceUrl)
        {
            string status = "successful";
            string filter = string.Format("ResourceUrl='{0}' AND vers.IsActive = 'True'", resourceUrl.Replace("'", "''"));
            DataSet ds = new DataSet();
            Resource resource = new Resource();

            resource = resourceManager.GetByResourceUrl(resourceUrl, ref status);
            if (status != "successful")
            {
                auditManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", "", ErrorType.Error, ErrorRouting.Technical, status);
            }
            if (resource == null)
            {
                return null;
            }
            else
            {
                ds = versionManager.Select(filter, 4);
                if (DoesDataSetHaveRows(ds))
                {
                    resource.Version = versionManager.Fill(ds.Tables[0].Rows[0], false);
                }
            }
            return resource;
        }

        public DataSet GetByResourceUrl(string resourceUrl)
        {
            string filter = string.Format("ResourceUrl='{0}'", resourceUrl);
            DataSet ds = new DataSet();

            try
            {
                ds = versionManager.Select(filter);
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
                auditManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", resourceUrl, ErrorType.Error, ErrorRouting.Technical,
                    ex.ToString());
                LogError(className + ".GetByResourceUrl(): " + ex.ToString());
                return null;
            }
        }


        public void Create(ResourceVersion entity, ref string statusMessage)
        {
            statusMessage = "successful";

            try
            {
                statusMessage = versionManager.Import(entity);
            }
            catch (Exception ex)
            {
                auditManager.LogMessage(LearningRegistry.reportId, LearningRegistry.fileName, "", "", ErrorType.Error, ErrorRouting.Technical, className + ".Create" +  ex.ToString());
                LogError(className + ".Create(): " + ex.ToString());
                statusMessage = ex.Message;
            }
          //  return statusMessage;

        }

        public string Update(ResourceVersion entity)
        {
            string status = "successful";
            try
            {
                status =  versionManager.Import(entity);
            }
            catch (Exception ex)
            {
                LogError("ResourceVersionManager.Update(): " + ex.ToString());
                status = ex.Message;
            }

            return status;
        }

        public string Delete(string rowId)
        {
            string status = "successful";
            return Delete(rowId, ref status); ;
        }

        public string Delete(string rowId, ref string status)
        {
            status = "successful";
            try
            {
                //
                DoTrace(2, "LR_Import.ResourceVersionController. Request to delete ResourceVersion. RowId = " + rowId);
                versionManager.Delete(rowId, ref status);
            }
            catch (Exception ex)
            {
                LogError("ResourceVersionManager.Delete(): " + ex.ToString());
                status = ex.Message;
            }

            return status;
        }

    }
}
