using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.ApplicationBlocks.Data;

using ILPathways.Utilities;
using Isle.DTO.Reports;

namespace LRWarehouse.DAL
{
    public class ReportingManager : BaseDataManager
    {
        private string ContentConnString;
        private string className = "LRWarehouse.DAL.ReportingManager";

        public ReportingManager()
        {
            ContentConnString = GetDatabaseCon("contentConString");
        }

        public List<OrgLibraryView> GetLibraryViews(int? pOrgId, DateTime? pStartDate, DateTime? pEndDate, ref string status)
        {
            status = "successful";
            List<OrgLibraryView> list = new List<OrgLibraryView>();
            List<OrgCollectionView> collectionViews = new List<OrgCollectionView>();
            List<OrgResourceView> resourceViews = new List<OrgResourceView>();

            #region parameters
            SqlParameter[] parms = new SqlParameter[3];
            parms[0] = new SqlParameter("@StartDate", SqlDbType.DateTime);
            if (pStartDate != null) parms[0].Value = pStartDate;
            parms[1] = new SqlParameter("@EndDate", SqlDbType.DateTime);
            if (pEndDate != null) parms[1].Value = pEndDate;
            parms[2] = new SqlParameter("@OrgId", SqlDbType.Int);
            if (pOrgId != null) parms[2].Value = pOrgId;
            #endregion

            try
            {
                DataSet ds = SqlHelper.ExecuteDataset(ContentConnString, CommandType.StoredProcedure, "[Activity.OrgSummaryReportLibrary]", parms);
                if (DoesDataSetHaveRows(ds))
                {
                    #region Read from database
                    // Read library list
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        OrgLibraryView libView = new OrgLibraryView
                        {
                            ObjectOrgId = GetRowPossibleColumn(dr, "ObjectOrgId", 0),
                            ObjectId = GetRowPossibleColumn(dr, "ObjectId", 0),
                            ObjectTitle = GetRowPossibleColumn(dr, "ObjectTitle", ""),
                            NbrViews = GetRowPossibleColumn(dr, "NbrViews", 0)
                        };
                        list.Add(libView);
                    }
                    
                    // Read collection list
                    foreach (DataRow dr in ds.Tables[1].Rows)
                    {
                        OrgCollectionView collView = new OrgCollectionView
                        {
                            ObjectOrgId = GetRowPossibleColumn(dr, "ObjectOrgId", 0),
                            ObjectId = GetRowPossibleColumn(dr, "ObjectId", 0),
                            ObjectTitle = GetRowPossibleColumn(dr, "ObjectTitle", ""),
                            ObjectParentId = GetRowPossibleColumn(dr, "ObjectParentId", 0),
                            NbrViews = GetRowPossibleColumn(dr, "NbrViews", 0)
                        };
                        collectionViews.Add(collView);
                    }

                    // Read resource list
                    foreach (DataRow dr in ds.Tables[2].Rows)
                    {
                        OrgResourceView resView = new OrgResourceView
                        {
                            ObjectOrgId = GetRowPossibleColumn(dr, "ObjectOrgId", 0),
                            ObjectId = GetRowPossibleColumn(dr, "ObjectId", 0),
                            ObjectTitle = GetRowPossibleColumn(dr, "ObjectTitle", ""),
                            ObjectParentId = GetRowPossibleColumn(dr, "ObjectParentId", 0),
                            ObjectRootId = GetRowPossibleColumn(dr, "ObjectRootId", 0),
                            NbrViews = GetRowPossibleColumn(dr, "NbrViews", 0)
                        };
                        resourceViews.Add(resView);
                    }
                    #endregion

                    // Link child entities with parents/root entities
                    foreach (OrgLibraryView library in list)
                    {
                        List<OrgCollectionView> collections = collectionViews.Where(x => x.ObjectParentId == library.ObjectId).ToList();
                        library.OrgCollectionViews = collections;
                        List<OrgResourceView> resources = resourceViews.Where(x => x.ObjectRootId == library.ObjectId).ToList();
                        library.OrgResourceViews = resources;
                        foreach (OrgCollectionView collection in library.OrgCollectionViews)
                        {
                            resources = resourceViews.Where(x => x.ObjectRootId == library.ObjectId && x.ObjectParentId == collection.ObjectId).ToList();
                            collection.OrgResourceViews = resources;
                        }//foreach collection
                    }// foreach library

                    return list;
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetLibraryViews(): " + ex.ToString());
                status = ex.Message;
            }

            return list;
        }
    }
}
