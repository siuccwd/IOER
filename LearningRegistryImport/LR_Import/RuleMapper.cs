using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
//using ResourceMap = LearningRegistryCache2.App_Code.Classes.ResourceMap;
//using ResourceCluster = LearningRegistryCache2.App_Code.Classes.ResourceCluster;
//using ResourceProperty = LearningRegistryCache2.App_Code.Classes.ResourceProperty;
//using ResourcePropertyCollection = LearningRegistryCache2.App_Code.Classes.ResourcePropertyCollection;
using Rule = LRWarehouse.Business.Rule;
//using LearningRegistryCache2.App_Code.Classes;
using OLDDM = LearningRegistryCache2.App_Code.DataManagers;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace LearningRegistryCache2
{
    public class RuleMapper
    {
        protected DataTable Rules;
        protected DataTable ClusterRules;
        protected RulesManager rulesManager;

        public RuleMapper()
        {
            // Load all rules
            rulesManager = new RulesManager();
            Rules = rulesManager.rules.Tables[0];
            ClusterRules = rulesManager.pathwayRules.Tables[0];
        }

        public ResourceChildItem DoMapping(ResourceProperty property)
        {
            string filter = "";
            ResourceChildItem map = new ResourceChildItem();

            map.ResourceId = property.ResourceId;
            map.OriginalValue = property.Value;

            filter = string.Format("PropertyTypeId = {0} AND OriginalValue = '{1}' AND IsActive = 'True' AND IsRegEx = 'False'",property.PropertyTypeId, property.Value.Replace("'","''"));
            DataRow[] rows = Rules.Select(filter);
            if (rows.Length > 0)
            {
                map.MappedValue = RulesManager.GetRowColumn(rows[0], "MappedValue", "");
                map.CodeId = RulesManager.GetRowColumn(rows[0], "MappedId", 0);
            }
            else
            {
                filter = string.Format("PropertyTypeId = {0} AND IsRegEx = 'True' AND IsActive = 'True'", property.PropertyTypeId);
                rows = Rules.Select(filter);
                foreach (DataRow row in rows)
                {
                    Rule rule = rulesManager.Fill(row);
                    Regex regex;
                    if (rule.IsCaseSensitive)
                    {
                        regex = new Regex(rule.OriginalValue, RegexOptions.None);
                    }
                    else
                    {
                        regex = new Regex(rule.OriginalValue, RegexOptions.IgnoreCase);
                    }
                    MatchCollection matches = regex.Matches(property.Value);
                    if (matches.Count > 0)
                    {
                        map.MappedValue = rule.MappedValue;
                        map.CodeId = rule.MappedId;
                        break;
                    }
                }
            }

            return map;
        }

        public ResourceCluster DoSubjectMapping(ResourcePropertyCollection collection, string clusterTitle)
        {
            bool isMapped = false;
            string filter = "";
            ResourceCluster cluster = new ResourceCluster();
            cluster.ResourceId = collection[0].ResourceId;
            DataRow[] rows;

            foreach (ResourceProperty property in collection)
            {
                if (property.PropertyType == "subject")
                {
                    filter = string.Format("PropertyTypeId = {0} AND IsRegEx = 'False' AND IsActive = 'True' AND OriginalValue = '{1}'",
                        property.PropertyTypeId, property.Value.Replace("'","''"));
                    rows = ClusterRules.Select(filter);
                    if (rows.Length > 0)
                    {
                        cluster.ResourceId = property.ResourceId;
                        cluster.ClusterId = BaseDataManager.GetRowColumn(rows[0], "MappedId", 0);
                        isMapped = true;
                    }
                    else
                    {
                        filter = string.Format("PropertyTypeId = {0} AND IsRegEx = 'True' AND IsActive = 'True'", property.PropertyTypeId);
                        foreach (DataRow row in rows)
                        {
                            Rule rule = rulesManager.Fill(row);
                            Regex regex;
                            if (rule.IsCaseSensitive)
                            {
                                regex = new Regex(rule.OriginalValue, RegexOptions.None);
                            }
                            else
                            {
                                regex = new Regex(rule.OriginalValue, RegexOptions.IgnoreCase);
                            }
                            MatchCollection matches = regex.Matches(property.Value);
                            if (matches.Count > 0)
                            {
                                cluster.ClusterId = rule.MappedId;
                                cluster.ResourceId = property.ResourceId;
                                isMapped = true;
                                break;
                            }
                        }
                    }
                    if (isMapped)
                    {
                        break;
                    }
                }
            }
            return cluster;
        }

    }
}
