using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using AutoMapper;

using Isle.DataContracts;
using LRWarehouse.Business;
using LRWarehouse.DAL;
using Isle.BizServices;
using ResBiz = IOERBusinessEntities;
using ThisUser = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
    public class ResourceBizService : ServiceHelper
    {
        private string thisClassName = "ResourceBizService";
        ResourceVersionManager myVersionManager = new ResourceVersionManager();
        CleanseUrlManager cleanseUrlManager = new CleanseUrlManager();

        #region resource queries
        /// <summary>
        /// Strip fragment identifiers, convert %20 to +, and otherwise clean a URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string CleanseUrl(string url)
        {
            url = url.Replace("ioer.ilsharedlearning.net", "ioer.ilsharedlearning.org");
            url = url.Replace("ioer.ilsharedlearning.com", "ioer.ilsharedlearning.org");
            url = url.Replace("ioer.ilsharedlearning.info", "ioer.ilsharedlearning.org");
            url = url.Replace("ioer.ilsharedlearning.mobi", "ioer.ilsharedlearning.org");
            url = url.Replace("%20", "+");

            /* According to IETF RFC 3986 (http://tools.ietf.org/html/rfc3986#page-24), the fragment identifier 
             * is terminated by the end of the URL.  Therefore it is safe to truncate everything at the optional '#'
             * character. */
            if (url.IndexOf("#") > -1)
            {
                url = url.Substring(0, url.IndexOf("#"));
            }

            try
            {
                Uri locator = new Uri(url);
                string status = "";
                DataSet cleansingRules = cleanseUrlManager.GetCleansingRules(locator.Host, ref status);
                if (CleanseUrlManager.DoesDataSetHaveRows(cleansingRules))
                {
                    string absolutePath = locator.AbsolutePath;
                    string queryString = locator.Query.Substring(1, locator.Query.Length - 1); // strip leading ?
                    queryString = queryString.Replace("?", "&"); // Change embedded ? to & - don't trust the URL!
                    List<string> parameterList = queryString.Split('&').ToList<string>();
                    foreach (DataRow dr in cleansingRules.Tables[0].Rows)
                    {
                        string parameter = CleanseUrlManager.GetRowColumn(dr, "urlParameterToDrop", "");
                        for (int i = 0; i < parameterList.Count; i++)
                        {
                            int pos = parameterList[i].IndexOf("=");
                            if (pos > -1)
                            {
                                if (parameterList[i].Substring(0, pos) == parameter)
                                {
                                    parameterList.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                    queryString = string.Empty;
                    foreach (string keyvalue in parameterList)
                    {
                        if (queryString == string.Empty)
                        {
                            queryString += "?" + keyvalue;
                        }
                        else
                        {
                            queryString += "&" + keyvalue;
                        }
                    }
                    if (queryString.Length > 1)
                    {
                        url = ReconstructUrl(locator, absolutePath, queryString);
                    }
                    else
                    {
                        url = ReconstructUrl(locator, absolutePath, "");
                    }
                }
            }
            catch (Exception ex)
            {
                CleanseUrlManager.LogError("BaseDataController.CleanseUrl(): " + ex.ToString());
            }


            return url;
        }

        /// <summary>
        /// Rebuild a URL from a URI, absolute path, and query string.  Currently used by CleanseUrl
        /// </summary>
        /// <param name="locator"></param>
        /// <param name="absolutePath"></param>
        /// <param name="queryString"></param>
        /// <returns></returns>
        private string ReconstructUrl(Uri locator, string absolutePath, string queryString)
        {
            string retVal = locator.Scheme + "://" + locator.Host;
            if (!locator.IsDefaultPort)
            {
                retVal += ":" + locator.Port;
            }
            retVal += absolutePath;
            if (queryString != null && queryString.Length > 0)
            {
                if (queryString.IndexOf("?") != 0)
                {
                    queryString = "?" + queryString;
                }
                retVal += queryString;
            }

            return retVal;
        }


        /// <summary>
        /// Check if url already exists in the resource table.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>true if found, otherwise false</returns>
        public static bool DoesUrlExist(string url)
        {
            bool exists = false;
            ResBiz.ResourceEntities ctx = new ResBiz.ResourceEntities();
            ResBiz.Resource res = ctx.Resources.FirstOrDefault( s => s.ResourceUrl == url );
            if ( res != null && res.Id > 0 )
                exists = true;

            return exists;

        }

        public static string UpdateFavorite( int resourceId )
        {
            return new ResourceManager().UpdateFavorite (resourceId);
        }
        #endregion


        #region resource queries
        public static ResourceVersion ResourceVersion_Get( int rvId )
        {

            ResourceVersion rv = new ResourceVersionManager().Get( rvId );

            return rv;
        }
        /// <summary>
        /// Get RV by url - it is possible to have multiple versions, so returning a list. The caller will have to handle.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<ResourceVersion> ResourceVersion_GetByUrl( string url )
        {

            return new ResourceVersionManager().GetByUrl( url );
        }
        #endregion


        #region patron queries

        #endregion

        #region OLD methods, called from webservices
        public ResourceGetResponse ResourceGet( ResourceGetRequest request )
        {
            //Mapper.CreateMap<ResourceGetRequest, ResourceVersion>();
            //ResourceVersion searchBEO = Mapper.Map<ResourceGetRequest, ResourceVersion>( request );

            ResourceVersion myBEO = myVersionManager.Display( request.ResourceVersionId );

            //ResourceDataContract dataContract = Mapper.Map<ResourceVersion, ResourceDataContract>( myBEO );
            ResourceDataContract dataContract = MapToContract( myBEO );


            ResourceGetResponse searchResponse = new ResourceGetResponse { Resource = dataContract };

            return searchResponse;

        }


        public ResourceSearchResponse ResourceSearch( ResourceSearchRequest request )
        {
            //Mapper.CreateMap<ResourceGetRequest, ResourceVersion>();
            //ResourceVersion searchBEO = Mapper.Map<ResourceGetRequest, ResourceVersion>( request );
            int totalRows = 0;

            //search
            DataSet ds = new LRManager().Search( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, request.OutputRelTables, ref totalRows );

            List<ResourceDataContract> dataContractList = new List<ResourceDataContract>();


            foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
            {
                //LRWarehouse.Business.ResourceVersion row = myVersionManager.Fill( dr, true );
                //ResourceDataContract dataContract = Mapper.Map<ResourceVersion, ResourceDataContract>( row );
                ResourceDataContract dataContract = Fill( dr, false );
                dataContractList.Add( dataContract );

            } //end foreach

            ////map from entity list to data contract list
            //List<ResourceDataContract> dataContractList = Mapper.Map<List<ResourceVersion>, List<ResourceDataContract>>( beoList );

            ResourceSearchResponse searchResponse = new ResourceSearchResponse { ResourceList = dataContractList };
            searchResponse.ResultCount = dataContractList.Count;
            searchResponse.TotalRows = totalRows;

            //DataSet ds = lrManager.Search( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, ref totalRows );

            //ResourceDataContract dataContract = Mapper.Map<ResourceVersion, ResourceDataContract>( myBEO );

            //ResourceSearchResponse searchResponse = new ResourceSearchResponse { ResourceList = dataContract };
            //searchResponse.ResultCount = partnerDataContractList.Count;

            return searchResponse;
        }

        public ResourceSearchResponse ResourceFTSearch( ResourceSearchRequest request )
        {
            int totalRows = 0;

            //search
            DataSet ds = new LRManager().Search( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, request.OutputRelTables, ref totalRows );

            List<ResourceDataContract> dataContractList = new List<ResourceDataContract>();


            foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
            {
                ResourceDataContract dataContract = Fill( dr, false );
                dataContractList.Add( dataContract );

            } //end foreach

            ResourceSearchResponse searchResponse = new ResourceSearchResponse { ResourceList = dataContractList };
            searchResponse.ResultCount = dataContractList.Count;
            searchResponse.TotalRows = totalRows;

            return searchResponse;
        }
        public ResourceDataContract Fill( DataRow dr, bool includeRelatedData )
        {
            ResourceDataContract entity = new ResourceDataContract();

            //string rowId = GetRowColumn( dr, "RowId", "" );
            //if ( rowId.Length > 35 )
            //    entity.RowId = new Guid( rowId );

            //rowId = GetRowColumn( dr, "ResourceId", "" );
            //if ( rowId.Length > 35 )
            //    entity.ResourceId = new Guid( rowId );

            //NEW - get integer version of resource id
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.ResourceVersionIntId = GetRowColumn( dr, "ResourceVersionIntId", 0 );

            entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "" );

            //get parent url
            entity.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );
            //entity.LRDocId = GetRowColumn( dr, "DocId", "" );
            entity.Publisher = GetRowColumn( dr, "Publisher", "" );
            entity.Creator = GetRowColumn( dr, "Creator", "" );
            //entity.Submitter = GetRowColumn( dr, "Submitter", "" );
            //entity.TypicalLearningTime = GetRowColumn( dr, "TypicalLearningTime", "" );

            entity.LikeCount = GetRowColumn( dr, "LikeCount", 0 );
            entity.DislikeCount = GetRowColumn( dr, "DislikeCount", 0 );

            entity.Rights = GetRowColumn( dr, "Rights", "" );
            entity.AccessRights = GetRowColumn( dr, "AccessRights", "" );
            entity.AccessRightsId = GetRowColumn( dr, "AccessRightsId", 0 );

            //entity.InteractivityTypeId = GetRowColumn( dr, "InteractivityTypeId", 0 );
            // entity.InteractivityType = GetRowColumn( dr, "InteractivityType", "" );

            entity.Modified = GetRowColumn( dr, "Modified", System.DateTime.MinValue );
            //entity.Created = GetRowColumn( dr, "Imported", System.DateTime.MinValue );
            // entity.SortTitle = GetRowColumn( dr, "SortTitle", "" );
            // entity.Schema = GetRowColumn( dr, "Schema", "" );

            if ( includeRelatedData == true )
            {

                entity.Subjects = GetRowColumn( dr, "Subjects", "" );
                entity.EducationLevels = GetRowColumn( dr, "EducationLevels", "" );
                entity.Keywords = GetRowColumn( dr, "Keywords", "" );
                entity.LanguageList = GetRowColumn( dr, "LanguageList", "" );
                entity.ResourceTypesList = GetRowColumn( dr, "ResourceTypesList", "" );
                // entity.AudienceList = GetRowColumn( dr, "AudienceList", "" );
                if ( entity.ResourceTypesList.Length > 0 )
                {
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&lt;", "<" );
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&gt;", ">" );
                }
            }

            return entity;
        }//

        private ResourceDataContract MapToContract( ResourceVersion request )
        {
            ResourceDataContract dataContract = new ResourceDataContract();
            //dataContract.ResourceId = request.ResourceId;
            //dataContract.ResourceVersionId = request.ResourceVersionId;
            //dataContract.RowId = request.ResourceVersionId;

            dataContract.Title = request.Title;
            dataContract.Description = request.Description;

            dataContract.AccessRights = request.AccessRights;
            dataContract.Creator = request.Creator;
            dataContract.EducationLevels = request.EducationLevels;
            dataContract.Imported = request.Imported;
            dataContract.Keywords = request.Keywords;
            dataContract.LanguageList = request.LanguageList;
            //dataContract.LRDocId = request.LRDocId;
            dataContract.Modified = request.Modified;
            dataContract.Publisher = request.Publisher;
            dataContract.ResourceUrl = request.ResourceUrl;
            dataContract.Rights = request.Rights;
            dataContract.Subjects = request.Subjects;
            dataContract.Submitter = request.Submitter;
            dataContract.TypicalLearningTime = request.TypicalLearningTime;

            return dataContract;
        }

        public ResourceSearchResponse ResourceSearch2( ResourceSearchRequest request )
        {
            LRManager lrManager = new LRManager();
            //Mapper.CreateMap<ResourceGetRequest, ResourceVersion>();
            //ResourceVersion searchBEO = Mapper.Map<ResourceGetRequest, ResourceVersion>( request );
            int totalRows = 0;

            //search
            List<ResourceVersion> beoList = lrManager.SearchToList( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, request.OutputRelTables, ref totalRows );

            //map from entity list to data contract list
            List<ResourceDataContract> dataContractList = Mapper.Map<List<ResourceVersion>, List<ResourceDataContract>>( beoList );

            ResourceSearchResponse searchResponse = new ResourceSearchResponse { ResourceList = dataContractList };
            searchResponse.ResultCount = dataContractList.Count;
            searchResponse.TotalRows = totalRows;

            //DataSet ds = lrManager.Search( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, ref totalRows );

            //ResourceDataContract dataContract = Mapper.Map<ResourceVersion, ResourceDataContract>( myBEO );

            //ResourceSearchResponse searchResponse = new ResourceSearchResponse { ResourceList = dataContract };
            //searchResponse.ResultCount = partnerDataContractList.Count;

            return searchResponse;

        }


        public ResourceSearchResponse ResourceSearch_FullText( ResourceSearchRequest request )
        {
            LRManager lrManager = new LRManager();
            //Mapper.CreateMap<ResourceGetRequest, ResourceVersion>();
            //ResourceVersion searchBEO = Mapper.Map<ResourceGetRequest, ResourceVersion>( request );
            int totalRows = 0;

            //search
            List<ResourceVersion> beoList = lrManager.SearchToList( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, request.OutputRelTables, ref totalRows );

            //map from entity list to data contract list
            List<ResourceDataContract> dataContractList = Mapper.Map<List<ResourceVersion>, List<ResourceDataContract>>( beoList );

            ResourceSearchResponse searchResponse = new ResourceSearchResponse { ResourceList = dataContractList };
            searchResponse.ResultCount = dataContractList.Count;
            searchResponse.TotalRows = totalRows;

            //DataSet ds = lrManager.Search( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, ref totalRows );

            //ResourceDataContract dataContract = Mapper.Map<ResourceVersion, ResourceDataContract>( myBEO );

            //ResourceSearchResponse searchResponse = new ResourceSearchResponse { ResourceList = dataContract };
            //searchResponse.ResultCount = partnerDataContractList.Count;

            return searchResponse;

        }
        #endregion

        public ResourceStandardCollection SelectResourceStandardsByVersion( int resourceVersionId )
        {
            ResourceVersion entity = new ResourceVersionManager().Get( resourceVersionId );
            if ( entity != null && entity.Id > 0 )
            {
                return SelectResourceStandards( entity.ResourceIntId );
            }
            else
            {
                return new ResourceStandardCollection();
            }
        }

        public ResourceStandardCollection SelectResourceStandards( int resourceIntID )
        {
            ResourceStandardManager standardManager = new ResourceStandardManager();
            ResourceStandardCollection standardsList = standardManager.Select( resourceIntID );
            return standardsList;

        }

        /// <summary>
        /// execute dynamic sql against the content db
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataSet DoQuery( string sql )
        {
            return DatabaseManager.DoQuery( sql );
        }

        public static DataSet ExecuteProc( string procedureName, SqlParameter[] parameters )
        {
            return DatabaseManager.ExecuteProc( procedureName, parameters );
        }
    }
}
