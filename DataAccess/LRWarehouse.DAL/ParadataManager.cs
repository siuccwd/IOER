using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Microsoft.ApplicationBlocks.Data;
using System.Text;

using LRWarehouse.Business;

namespace LRWarehouse.DAL
{
    public class ParadataManager : BaseDataManager
    {
        const string className = "ParadataManager";
        public ParadataManager()
        {
        }

        public List<ParadataRating> GetLikeSummaries(DateTime startDate, DateTime endDate, ref string status)
        {
            status = "successful";
            List<ParadataRating> returnList = new List<ParadataRating>();
            try
            {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@StartDate", startDate);
                parameters[1] = new SqlParameter("@EndDate", endDate);

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[ParadataPublish.Like]", parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ParadataRating rating = ParadataRatingFill(dr);
                        returnList.Add(rating);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetLikeSummaries(): " + ex.ToString());
                status = className + ".GetLikeSummaries(): " + ex.Message;
            }

            return returnList;
        }// GetLikeSummaries

        public List<ParadataRating> GetRatings(DateTime startDate, DateTime endDate, ref string status)
        {
            status = "successful";
            List<ParadataRating> returnList = new List<ParadataRating>();
            try
            {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@StartDate", startDate);
                parameters[1] = new SqlParameter("@EndDate", endDate);

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[ParadataPublish.Evaluation]", parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        ParadataRating rating = ParadataRatingFill(dr);
                        returnList.Add(rating);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetRatings(): " + ex.ToString());
                status = className + ".GetRatings(): " + ex.Message;
            }

            return returnList;
        }// GetRatings

        public List<BaseParadataSummary> GetFavorites(DateTime startDate, DateTime endDate, ref string status)
        {
            status = "successful";
            List<BaseParadataSummary> returnList = new List<BaseParadataSummary>();
            try
            {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@StartDate", startDate);
                parameters[1] = new SqlParameter("@EndDate", endDate);

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[ParadataPublish.Favorite]", parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        BaseParadataSummary summary = ParadataSummaryFill(dr);
                        returnList.Add(summary);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetFavorites(): " + ex.ToString());
                status = className + ".GetFavorites(): " + ex.Message;
            }

            return returnList;
        }// GetFavorites

        public List<BaseParadataSummary> GetViews(DateTime startDate, DateTime endDate, ref string status)
        {
            status = "successful";
            List<BaseParadataSummary> returnList = new List<BaseParadataSummary>();
            try
            {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@StartDate", startDate);
                parameters[1] = new SqlParameter("@EndDate", endDate);

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[ParadataPublish.View]", parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        BaseParadataSummary summary = ParadataSummaryFill(dr);
                        returnList.Add(summary);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetViews(): " + ex.ToString());
                status = className + ".GetViews(): " + ex.Message;
            }

            return returnList;
        }// GetViews

        public List<BaseParadataItem> GetComments(DateTime startDate, DateTime endDate, ref string status)
        {
            status = "successful";
            List<BaseParadataItem> returnList = new List<BaseParadataItem>();
            try
            {
                SqlParameter[] parameters = new SqlParameter[2];
                parameters[0] = new SqlParameter("@StartDate", startDate);
                parameters[1] = new SqlParameter("@EndDate", endDate);

                DataSet ds = SqlHelper.ExecuteDataset(ReadOnlyConnString, CommandType.StoredProcedure, "[ParadataPublish.Comment]", parameters);
                if (DoesDataSetHaveRows(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        BaseParadataItem item = ParadataItemFill(dr);
                        returnList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(className + ".GetComments(): " + ex.ToString());
                status = className + ".GetComments(): " + ex.Message;
            }

            return returnList;
        }// GetComments

        private ParadataRating ParadataRatingFill(DataRow dr)
        {
            ParadataRating rating = new ParadataRating();
            rating.ActorDescription = GetRowColumn(dr, "ActorDescription", "");
            rating.ActorUserType = GetRowColumn(dr, "ActorUserType", "");
            rating.DateRange = GetRowColumn(dr, "DateRange", "");
            rating.ContextId = GetRowColumn(dr, "ContextId", "");
            rating.ScaleMin = GetRowColumn(dr, "ScaleMin", 0);
            rating.ScaleMax = GetRowColumn(dr, "ScaleMax", 5);
            rating.SampleSize = GetRowColumn(dr, "SampleSize", 0);
            rating.Value = GetRowColumn(dr, "Value", (decimal)0.0);
            rating.ResourceUrl = GetRowColumn(dr, "ResourceUrl", "");
            rating.RelatedObjectType = GetRowColumn(dr, "RelatedObjectType", "");
            rating.RelatedObjectId = GetRowColumn(dr, "RelatedObjectId", "");
            rating.RelatedObjectContent = GetRowColumn(dr, "RelatedObjectContent", "");

            return rating;
        }

        private BaseParadataSummary ParadataSummaryFill(DataRow dr)
        {
            BaseParadataSummary summary = new BaseParadataSummary();
            summary.ActorDescription = GetRowColumn(dr, "ActorDescription", "");
            summary.ActorUserType = GetRowColumn(dr, "ActorUserType", "");
            summary.DateRange = GetRowColumn(dr, "DateRange", "");
            summary.ContextId = GetRowColumn(dr, "ContextId", "");
            summary.Value = GetRowColumn(dr, "Value", (decimal)0.0);
            summary.ResourceUrl = GetRowColumn(dr, "ResourceUrl", "");
            summary.RelatedObjectType = GetRowColumn(dr, "RelatedObjectType", "");
            summary.RelatedObjectId = GetRowColumn(dr, "RelatedObjectId", "");
            summary.RelatedObjectContent = GetRowColumn(dr, "RelatedObjectContent", "");

            return summary;
        }

        private BaseParadataItem ParadataItemFill(DataRow dr)
        {
            BaseParadataItem item = new BaseParadataItem();
            item.ActorDescription = GetRowColumn(dr, "ActorDescription", "");
            item.ActorUserType = GetRowColumn(dr, "ActorUserType", "");
            item.ContextId = GetRowColumn(dr, "ContextId", "");
            item.ResourceUrl = GetRowColumn(dr, "ResourceUrl", "");
            item.RelatedObjectType = GetRowColumn(dr, "RelatedObjectType", "");
            item.RelatedObjectId = GetRowColumn(dr, "RelatedObjectId", "");
            item.RelatedObjectContent = GetRowColumn(dr, "RelatedObjectContent", "");
            item.Date = GetRowColumn(dr, "Date", DateTime.Now);
            item.Comment = GetRowColumn(dr, "Comment", "");

            return item;
        }
    }
}


