using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Microsoft.ApplicationBlocks.Data;
using LRWarehouse.Business;
using Rule = LRWarehouse.Business.Rule;

namespace LRWarehouse.DAL
{
    public class RulesManager : BaseDataManager
  {

    public DataSet rules;
    public DataSet pathwayRules;
    public RulesManager()
    {
      rules = LoadRules();
      pathwayRules = LoadPathwayRules();
    }

    private DataSet LoadRules()
    {
      DataSet ds = new DataSet();
      try
      {
        ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Map.Rules_Load]");
      }
      catch (Exception ex)
      {
        LogError(ex, "RulesManager().LoadRules: ");
        ds = null;
        throw;
      }
      return ds;
    }

    private DataSet LoadPathwayRules()
    {
      DataSet ds = new DataSet();
      try
      {
        ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "[Map.PathwayRules_Load]");
      }
      catch (Exception ex)
      {
        LogError(ex, "RulesManager.LoadPathwayRules: ");
        ds = null;
        throw;
      }

      return ds;
    }

    public Rule Fill(DataRow dr)
    {
        Rule rule = new Rule();
        rule.Id = GetRowColumn(dr, "Id", 0);
        rule.PropertyTypeId = GetRowColumn(dr, "PropertyTypeId", 0);
        rule.PropertyType = GetRowColumn(dr, "PropertyType", "");
        rule.OriginalValue = GetRowColumn(dr, "OriginalValue", "");
        rule.IsRegex = GetRowColumn(dr, "IsRegex", false);
        rule.IsCaseSensitive = GetRowColumn(dr, "IsCaseSensitive", false);
        rule.ImportWithoutTranslation = GetRowColumn(dr, "ImportWithoutTranslation", false);
        rule.DoNotImport = GetRowColumn(dr, "DoNotImport", false);
        rule.MappedValue = GetRowColumn(dr, "MappedValue", "");
        rule.Sequence = GetRowColumn(dr, "Sequence", 0);
        rule.IsActive = GetRowColumn(dr, "IsActive", false);
        rule.Created = GetRowColumn(dr, "Created", DateTime.Now);
        rule.CreatedBy = GetRowColumn(dr, "CreatedBy", "");
        rule.LastUpdated = GetRowColumn(dr, "LastUpdated", DateTime.Now);
        rule.LastUpdatedBy = GetRowColumn(dr, "LastUpdatedBy", "");
        rule.MappedId = GetRowColumn(dr, "MappedId", 0);

        return rule;
    }

    public CareerClusterCollection LoadCareerClusters()
    {
        DataSet ds = new DataSet();
        CareerClusterCollection collection = new CareerClusterCollection();
        try
        {
            ds = SqlHelper.ExecuteDataset(ConnString, CommandType.StoredProcedure, "CareerClusterSelect");
            if (DoesDataSetHaveRows(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    CareerCluster cluster = FillCareerCluster(dr);
                    collection.Add(cluster);
                }
            }
            return collection;
        }
        catch (Exception ex)
        {
            LogError(ex, "RulesManager.LoadCareerClusters(): ");
            return null;
        }
    }

    public CareerCluster FillCareerCluster(DataRow dr)
    {
        CareerCluster cluster = new CareerCluster();
        cluster.Clusterid = GetRowColumn(dr, "Id", 0);
        cluster.ClusterName = GetRowColumn(dr, "CareerCluster", "");
        cluster.IlPathwayName = GetRowColumn(dr, "HighGrowthName", "");

        return cluster;
    }

  }
}
