using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

using AutoMapper;
using ILPathways.Business;
using ILPathways.Utilities;
using Isle.BizServices;
using Isle.DataContracts;
using Isle.DTO;

using LRWarehouse.Business;
using LRWarehouse.DAL;
using EfMgr = IOERBusinessEntities.EFResourceManager;
using EIMgr = Isle.BizServices.ElasticIndexServices;
using ResBiz = IOERBusinessEntities;
using ThisUser = LRWarehouse.Business.Patron;
using Thumbnailer = LRWarehouse.DAL.ResourceThumbnailManager;

namespace Isle.BizServices
{
    public class ResourceBizService : ServiceHelper
    {
        private static string thisClassName = "ResourceBizService";
        ResourceVersionManager myVersionManager = new ResourceVersionManager();
        CleanseUrlManager cleanseUrlManager = new CleanseUrlManager();

        public static int AchieveEvaluationId = 1;

        #region == constants ===
        public static string PdfImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pdf_200x150.png";
        public static string PPTImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pptx_200x150.png";
        public static string WordImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_docx_200x150.png";
        public static string XlxImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_xlsx_200x150.png";
        public static string SwfImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_swf_200x200.png";
        //large

        public static string PdfLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pdf_400x300.png";
        public static string PPTLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_pptx_400x300.png";
        public static string WordLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_docx_400x300.png";
        public static string XlxLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_xlsx_400x300.png";
        public static string SwfLrgImageUrl = "//ioer.ilsharedlearning.org/images/icons/filethumbs/filethumb_swf_400x400.png";
        #endregion


        #region resource methods

        /// <summary>
        /// Create a resource and all related child records
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static int ResourceCompleteCreate( ResourceTransformDTO dto, bool skipLRPublish )
        {
            EfMgr mgr = new EfMgr();
            string statusMessage = "";
            string tempStatus = "";
            string lrDocID = "";
            bool success = true;
            bool successful = false;
            string status = "";
            string continueOnPublishError = UtilityManager.GetAppKeyValue( "continueOnPublishError", "yes" );

            try
            {
                Patron user = AccountServices.GetUser( dto.CreatedById );
                //TEMP - until resource can be properly populated
                skipLRPublish = true;
                if ( !skipLRPublish )
                {
                    Resource input = PopulateResource( dto );
                    PublishingServices.PublishToLearningRegistry( input, 
                        ref success, 
                        ref tempStatus, 
                        ref lrDocID );
                    if ( !success && !IsLocalHost() )
                    {
                        if ( continueOnPublishError == "no" )
                        {
                            successful = false;
                            SetConsoleErrorMessage( "Error: " + tempStatus );
                            status = statusMessage + " " + tempStatus + " ";
                            //versionID = 0;
                            return 0;
                        }
                        else
                        {
                            EmailManager.NotifyAdmin( "Error during LR Publish", "Error: " + tempStatus + "<p>The error was encountered during the LR publish. The system continued with saving to the database and elastic search. </p>" );
                        }
                    }
                }

                mgr.Resource_CompleteCreate( dto );


                LoggingHelper.DoTrace( 4, string.Format("ResourceBizService.ResourceCompleteCreate. calling PublishingServices.PublishToElasticSearch with resId= {0}.", dto.Id) );
                PublishingServices.PublishToElasticSearch( dto.Id, ref successful, ref status );

                if ( dto.Id > 0 && dto.SelectedCollectionID > 0 )
                {
                    new LibraryBizService().LibraryResourceCreate( dto.SelectedCollectionID, dto.Id, dto.CreatedById, ref statusMessage );
                }

                SetConsoleSuccessMessage( "Successfully published the Resource" );

                new Thumbnailer().CreateThumbnail( dto.Id, dto.ResourceUrl );
                         }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ResourceCompleteCreate" );
            }

            return dto.Id;
        } //
        private static Resource PopulateResource( ResourceTransformDTO dto )
        {
            Resource res = new Resource();


            return res;
        }
        public static bool ResourceCompleteUpdate( ResourceTransformDTO dto )
        {
            bool isValid = true;
            EfMgr mgr = new EfMgr();
            try
            {
                mgr.Resource_CompleteUpdate( dto );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".ResourceCompleteUpdate" );
            }

            return isValid;
        }

        public static ResourceTransformDTO Resource_CompleteGet( int resourceId )
        {

            return EfMgr.Resource_CompleteGet( resourceId );
        }
        /// <summary>
        /// Retrieve a Resource id using the related resource version id to retrieve the Resource record
        /// </summary>
        /// <param name="versionID"></param>
        /// <returns></returns>
        public int GetIntIDFromVersionID( int versionID )
        {
            if ( versionID > 0 )
            {
                Resource entity = ResourceGet_ViaVersionID( versionID );
                if ( entity != null )
                    return entity.Id;
                else
                    return 0;
            }
            else
                return 0;
        }

        /// <summary>
        /// Retrieve a Resource using the related resource version id to retrieve the Resource record
        /// </summary>
        /// <param name="versionID"></param>
        /// <returns></returns>
        public static Resource ResourceGet_ViaVersionID( int versionID )
        {
            return new ResourceManager().GetByVersion( versionID );
        }

        public string CheckBlacklist(string url)
        {
            string reputation = "Safe";
            string status = "successful";

            try
            {
                Uri uri = new Uri(url);

                BlacklistedHost bh = new BlacklistedHostManager().GetByHostname(uri.Host, ref status);
                if (bh != null)
                {
                    reputation = "Blacklisted";
                }
                else
                {
                    reputation = UtilityManager.CheckUnsafeUrl(url);
                }
            }
            catch (UriFormatException uex)
            {
                LogError(thisClassName + ".CheckBlacklist(): " + uex.ToString());
                reputation = uex.Message;
            }
            catch (FormatException fex)
            {
                LogError(thisClassName + ".CheckBlacklist(): " + fex.ToString());
                reputation = fex.Message;
            }
            catch (Exception ex)
            {
                LogError(thisClassName + ".CheckBlacklist(): " + ex.ToString());
                reputation = ex.Message;
            }

            return reputation;
        }
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
                    string queryString = "";
                    if (locator.Query.Length > 0)
                    {
                        queryString = locator.Query.Substring(1, locator.Query.Length - 1); // strip leading ?
                    }
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
            ResBiz.ResourceContext ctx = new ResBiz.ResourceContext();
            url = url.ToLower().Trim();
            string alturl = url;

            //need to check with and without trailing slash
            //really need to check with and without www
            //prob also http vs https
            if ( url.Trim().EndsWith( "/" ) )
                alturl = url.Substring( 0, url.Length - 1 );
            else
                alturl += "/";

            //sql is not case sensitive, not sure if entity framework is????
            ResBiz.Resource res = ctx.Resources.FirstOrDefault( s => s.ResourceUrl.ToLower() == url || s.ResourceUrl.ToLower() == alturl );
            if ( res != null && res.Id > 0 )
                exists = true;

            return exists;

        }

        public static string UpdateFavorite( int resourceId )
        {
            return new ResourceManager().UpdateFavorite (resourceId);
        }
        public static string DecreaseFavorites( int resourceId )
        {
            return new ResourceManager().DecreaseFavoritesByOne( resourceId );
        }

        /// <summary>
        /// Determine if user is the resource author 
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ObjectMember GetResourceAccess( int resourceId, int userId ) 
        {
            return EfMgr.GetResourceAccess( resourceId, userId );
        }
        #endregion

        #region == content related methods ===
        /// <summary>
        /// Set a resource inactive and remove from elastic search
        /// TODO - need to handle learning registry deletes
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="statusMessage"></param>
        public static void Resource_SetInactive( int resourceId, ref string statusMessage )
        {
            //need to get resource version, in order to get LR doc id
            ResourceVersion entity = new ResourceVersionManager().GetByResourceId( resourceId );

            statusMessage = new ResourceManager().SetResourceActiveState( resourceId, false );
            //TODO - need check for lr  doc id on rv record!
            EIMgr.RemoveResource( resourceId, ref statusMessage );

            //note can have a RV id not not be published to LR. Need to check for a resource docid
            if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
            {
                //post request to delete ==> this process would take care of actual delete of the Resource hierarchy
                string msg = string.Format( "Set resource: {0} to be inactive. BUT the process is incomplete.<br/>NEED TO ADD CODE TO REMOVE FROM THE LR, AND DO PHYSICAL DELETE OF THE RESOURCE HIERARCHY.", resourceId );

                EmailManager.NotifyAdmin( "JEROME - Resource set inactive - need to handle delete from Learning Registry", msg );
                msg = msg.Replace( "<br/>", "\\r\\n" );
                LoggingHelper.DoTrace( 3, msg );
            }
        }
        public static void Resource_SetInactiveByVersionId( int resourceVersionId, ref string statusMessage )
        {
            ResourceVersion entity = new ResourceVersionManager().Get( resourceVersionId );

            if ( entity != null && entity.ResourceIntId > 0 )
            {
                LoggingHelper.DoTrace( 2, string.Format( "@@@@@@ Resource_SetInactiveByResVersionId - Setting a resource version INACTVE. rId: {0}, rvId: {1}, title ", entity.ResourceIntId, resourceVersionId, entity.Title ) );

                statusMessage = new ResourceManager().SetResourceActiveState( entity.ResourceIntId, false );
                //should not have to do this, but old code is resource version oriented and seems necessary
                new ResourceVersionManager().SetActiveState( false, resourceVersionId );

                //EIMgr.RemoveResource( entity.ResourceIntId, ref statusMessage );
                EIMgr.RemoveResourceVersion( entity.Id, ref statusMessage );
                //again for new collection
                EIMgr.RemoveResource_NewCollection( entity.ResourceIntId, ref statusMessage );
            }
            else
            {
                statusMessage = "Error - resource version was not found - hmmmm?";
            }

        }

        /// <summary>
        /// OBSOLETE - VERIFY NOT NEEDED
        /// </summary>
        /// <param name="resourceVersionId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        [Obsolete]
        private  static bool ResourceVersion_SetInactive( int resourceVersionId, ref string statusMessage )
        {
            bool isValid = false;

            ResourceVersion entity = new ResourceVersionManager().Get( resourceVersionId );
            if ( entity != null && entity.Id > 0 )
            {
                LoggingHelper.DoTrace( 2, string.Format( "###### ResourceVersion_SetInactive - Setting a resource version INACTVE. rId: {0}, rvId: {1}, title ", entity.ResourceIntId, resourceVersionId, entity.Title ) );

                try
                {
                    new ResourceVersionManager().SetActiveState( false, resourceVersionId );
                    string response = "";
                    EIMgr.RemoveResourceVersion( resourceVersionId, ref response );
                    //var esManager = new ElasticSearchManager();
                    //new ElasticSearchManager().DeleteByVersionID( resourceVersionId, ref response );
                    statusMessage = "Resource Deactivated";
                }
                catch ( Exception ex )
                {
                    statusMessage = "There was a problem deactivating the Resource: " + ex.ToString();
                }


                //note can have a RV id not not be published to LR. Need to check for a resource docid
                if ( entity.LRDocId != null && entity.LRDocId.Length > 10 )
                {
                    //post request to delete ==> this process would take care of actual delete of the Resource hierarchy
                    //if ( IsTestEnv() )
                    //    statusMessage = "<br/>( WELL ALMOST - NEED TO ADD CODE TO REMOVE FROM THE LR, AND DO PHYSICAL DELETE OF THE RESOURCE HIERARCHY - JEROME)";
                }

            }

            return isValid;
        }

        /// <summary>
        /// use for to reactivate a resource version and resource.
        /// Used for inadvertant deactivates!
        /// </summary>
        /// <param name="resourceVersionId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool ResourceVersion_SetActive( int resourceVersionId, ref string statusMessage )
        {
            bool isValid = false;
            
            ResourceVersion entity = new ResourceVersionManager().Get( resourceVersionId );
            if ( entity != null && entity.Id > 0 )
            {

                LoggingHelper.DoTrace( 2, string.Format( "$$$$$$ Setting a resource version back to active. rId: {0}, rvId: {1}, title ", entity.ResourceIntId, resourceVersionId, entity.Title) );
                try
                {
                    new ResourceVersionManager().SetActiveState( true, resourceVersionId );
                    //just incase, do resource as well
                    statusMessage = new ResourceManager().SetResourceActiveState( entity.ResourceIntId, true );

                    //Update elasticsearch
                    //new ElasticSearchManager().RefreshRecord( entity.ResourceIntId );
                    new ElasticSearchManager().RefreshResource( entity.ResourceIntId );

                    statusMessage = "Resource Reactivated";
                }
                catch ( Exception ex )
                {
                    statusMessage = "There was a problem reactivating the Resource: " + ex.ToString();
                }


            }

            return isValid;
        }

        #endregion 
        
        #region == Fill methods - for ws, etc ===
        public static Resource Resource_FillSummary (int resourceId) 
        {
            Resource res = EfMgr.Resource_GetSummary( resourceId );
            res.Standard = new ResourceStandardManager().Select( resourceId );
            return res;
        }

        #endregion


        #region == evaluation methods ===
        public static int Evaluations_SaveAchieveEvaluation( ResourceEvaluationDTO evaluation, ref string statusMessage )
        {
            if ( evaluation.EvaluationId == 0 )
                evaluation.EvaluationId = AchieveEvaluationId;

            return Evaluations_SaveEvaluation( evaluation, ref statusMessage );
        }
        public static int Evaluations_SaveEvaluation( ResourceEvaluationDTO evaluation, ref string statusMessage )
        {
            ResBiz.Resource_Evaluation efEntity = new ResBiz.Resource_Evaluation();
            ResBiz.Resource_EvaluationSection section = new ResBiz.Resource_EvaluationSection();

            try
            {
                //will page 'know' evaluation id?

                using ( var context = new ResBiz.ResourceContext() )
                {
                    efEntity.CreatedById = evaluation.CreatedById;
                    efEntity.ResourceIntId = evaluation.ResourceIntId;
                    efEntity.Created = System.DateTime.Now;
                    efEntity.EvaluationId = evaluation.EvaluationId;

                    //TODO - may need to calculate score??
                    efEntity.Score = evaluation.Score;
                    efEntity.UserHasCertification = evaluation.UserHasCertification;

                    context.Resource_Evaluation.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        statusMessage = "Successful";
                        //save dimensions
                        foreach ( ResourceEvaluationDimension dim in evaluation.Dimensions )
                        {
                            section = new ResBiz.Resource_EvaluationSection();
                            section.ResourceEvalId = efEntity.Id;
                            section.EvalDimensionId = dim.EvalDimensionId;
                            //check handling of null
                            //section.StandardId = dim.StandardId;
                            section.Score = dim.Score;
                            section.CreatedById = evaluation.CreatedById;
                            section.Created = System.DateTime.Now;

                            context.Resource_EvaluationSection.Add( section );

                            count = context.SaveChanges();
                            //error checking?
                        }
                        evaluation.Id = efEntity.Id;
                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error
                        statusMessage = "Error: evaluation was not saved.";
                        return 0;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Evaluations_SaveAchieveEvaluation()" );
                statusMessage = ex.Message;
                return 0;
            }
        }

        public static List<ResourceEvaluationSummaryDTO> GetAllEvaluationsForResource( int resourceId, ThisUser user, ref string statusMessage )
        {

            List<ResourceEvaluationSummaryDTO> resList = new List<ResourceEvaluationSummaryDTO>();
            ResourceEvaluationSummaryDTO eval = new ResourceEvaluationSummaryDTO();
            ResourceEvaluationDimensionSummary dimension = new ResourceEvaluationDimensionSummary();

            List<ResBiz.Resource_Evaluation> userEvals = new List<ResBiz.Resource_Evaluation>();

            //get all evals for resource
            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    if ( user != null && user.Id > 0 )
                        userEvals = GetUserResourceEvaluations( user.Id, resourceId, context );

                    List<ResBiz.Resource_EvaluationsSummary> list = context.Resource_EvaluationsSummary
                            .Where( s => s.ResourceIntId == resourceId )
                            .OrderBy( s => s.EvaluationTitle )
                            .ToList();
                    foreach ( ResBiz.Resource_EvaluationsSummary re in list )
                    {
                        eval = new ResourceEvaluationSummaryDTO();

                        eval.ResourceIntId = re.ResourceIntId;
                        eval.EvaluationId =  re.EvaluationId;
                        eval.EvaluationTitle = re.EvaluationTitle;
                        eval.RequiresCertification = re.RequiresCertification;
                        eval.PrivilegeCode = re.PrivilegeCode == null ? "" : re.PrivilegeCode;

                        eval.HasCertificationTotal = re.HasCertificationTotal == null ? 0 : (int) re.HasCertificationTotal;
                        eval.HasCertificationTotalScore = re.HasCertificationTotalScore == null ? 0 : ( int ) re.HasCertificationTotalScore;
                        if ( eval.HasCertificationTotal > 0 )
                            eval.CertifiedAverageScore = CalculateAverageWithRoundUp( eval.HasCertificationTotalScore, eval.HasCertificationTotal);

                        eval.NonCertificationTotal = re.HasNOTCertificationTotal == null ? 0 : ( int ) re.HasNOTCertificationTotal;
                        eval.NonCertificationTotalScore = re.HasNOTCertificationTotalScore == null ? 0 : ( int ) re.HasNOTCertificationTotalScore;
                        if ( eval.NonCertificationTotal > 0 )
                          eval.NonCertifiedAverageScore = CalculateAverageWithRoundUp( eval.NonCertificationTotalScore, eval.NonCertificationTotal );

                        if ( user.Id > 0 )
                        {
                            if ( userEvals.Count > 0 )
                            {
                                foreach ( ResBiz.Resource_Evaluation ueval in userEvals )
                                {
                                    if ( ueval.EvaluationId == eval.EvaluationId )
                                    {
                                        eval.UserHasCompletedEvaluation = true;
                                        break;
                                    }
                                }
                            }
                            eval.CanUserDoEvaluation = true;
                            if ( eval.RequiresCertification && eval.PrivilegeCode.Length > 0)
                            {
                                eval.CanUserDoEvaluation = false;
                                ApplicationRolePrivilege privileges = new ApplicationRolePrivilege();
                                privileges = SecurityManager.GetGroupObjectPrivileges( user, eval.PrivilegeCode );

                                if ( privileges != null && privileges.CanCreate() )
                                    eval.CanUserDoEvaluation = true;
                            }
                        }

                        List<ResBiz.Resource_EvalDimensionsSummary> rubrics = context.Resource_EvalDimensionsSummary
                            .Where( s => s.ResourceIntId == resourceId && s.EvaluationId == eval.EvaluationId )
                            .OrderBy( s => s.EvalDimensionId )
                            .ToList();
                        foreach ( ResBiz.Resource_EvalDimensionsSummary res in rubrics )
                        {
                            dimension = new ResourceEvaluationDimensionSummary();

                            dimension.EvalDimensionId = res.EvalDimensionId == null ? 0 : ( int ) res.EvalDimensionId;
                            dimension.DimensionTitle = res.DimensionTitle;

                            dimension.HasCertificationTotal = res.HasCertificationTotal == null ? 0 : ( int ) res.HasCertificationTotal;
                            dimension.HasCertificationTotalScore = res.HasCertificationTotalScore == null ? 0 : ( int ) res.HasCertificationTotalScore;
                            if ( dimension.HasCertificationTotal > 0 )
                                dimension.CertifiedAverageScore = CalculateAverageWithRoundUp( dimension.HasCertificationTotalScore, dimension.HasCertificationTotal );

                            dimension.NonCertificationTotal = res.HasNOTCertificationTotal == null ? 0 : ( int ) res.HasNOTCertificationTotal;
                            dimension.NonCertificationTotalScore = res.HasNOTCertificationTotalScore == null ? 0 : ( int ) res.HasNOTCertificationTotalScore;
                            if ( dimension.NonCertificationTotalScore > 0 )
                                dimension.NonCertifiedAverageScore = CalculateAverageWithRoundUp( dimension.NonCertificationTotalScore, dimension.NonCertificationTotal );


                            eval.Dimensions.Add( dimension );
                        }

                        resList.Add( eval );
                    }

                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetAllEvaluationsForResource()" );
                statusMessage = ex.Message;
            }

            return resList;
      }

        /// <summary>
        /// Determine if user has done the evaluation for the resource. If not, is the user allowed to 
        /// </summary>
        /// <param name="evaluationId"></param>
        /// <param name="resourceId"></param>
        /// <param name="user"></param>
        /// <param name="hasUserCompletedEvaluation"></param>
        /// <param name="canUserDoEvaluation"></param>
        public static void Evaluations_UserEvaluationStatus( int evaluationId, int resourceId, ThisUser user
                                                        , ref bool hasUserCompletedEvaluation
                                                        , ref bool canUserDoEvaluation )
        {
            hasUserCompletedEvaluation = false;
            canUserDoEvaluation = false;

            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    //has user taken eval:
                    ResBiz.Resource_Evaluation reval = context.Resource_Evaluation
                            .Where( s => s.ResourceIntId == resourceId && s.EvaluationId == evaluationId && s.CreatedById == user.Id )
                            .FirstOrDefault();

                    if ( reval != null && reval.Id > 0 )
                    {
                        hasUserCompletedEvaluation = true;
                        return;
                    }
                    else
                    {
                        //otherwise, can user take eval
                        EvaluationDTO eval = Evaluation_GetComponents( evaluationId, false );
                        canUserDoEvaluation = true;

                        if ( eval.RequiresCertification && eval.PrivilegeCode.Length > 0 )
                        {
                            canUserDoEvaluation = false;
                            ApplicationRolePrivilege privileges = new ApplicationRolePrivilege();
                            privileges = SecurityManager.GetGroupObjectPrivileges( user, eval.PrivilegeCode );

                            if ( privileges != null && privileges.CanCreate() )
                                canUserDoEvaluation = true;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Evaluations_UserEvaluationStatus()" );
            }

        }

        private static int CalculateAverageWithRoundUp( int totalScore, int totalItems )
        {
            int average = 0;
            if ( totalItems > 0 )
                average = totalScore / totalItems;
            if ( totalScore % totalItems > 0 )
                average++;

            return average;
        }

        public static List<ResBiz.Resource_Evaluation> GetUserResourceEvaluations( int userId, int resourceId, ResBiz.ResourceContext context )
        {

            List<ResBiz.Resource_Evaluation> list = context.Resource_Evaluation
                            .Where( s => s.ResourceIntId == resourceId && s.CreatedById == userId )
                            .OrderBy( s => s.Created )
                            .ToList();

            return list;
        }

        /// <summary>
        /// Retrieve Achieve evaluations for a resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="user">Optional. If provided, check if has certification</param>
        /// <param name="statusMessage">Blank unless an error</param>
        /// <returns></returns>
        public static ResourceEvaluationsSummaryDTO GetAchieveEvaluations( int resourceId, ThisUser user, ref string statusMessage )
        {
            //might be better to store this in evaluation table
            //as well as whether certification is required
            string certificationObject = "isle.achieve.evaluation.certified";

            EvaluationDTO dto = new EvaluationDTO();

            return GetResourceEvaluations( resourceId, AchieveEvaluationId, certificationObject, user, ref statusMessage );
        }

        /// <summary>
        /// Retrieve evaluations of the specified type for a particular resource
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="evaluationId"></param>
        /// <param name="certificationObject"></param>
        /// <param name="user">Optional. If provided, check if has certification</param>
        /// <param name="statusMessage">Blank unless an error</param>
        /// <returns></returns>
        public static ResourceEvaluationsSummaryDTO GetResourceEvaluations( int resourceId, int evaluationId, string certificationObject, ThisUser user, ref string statusMessage )
        {
            ResourceEvaluationsSummaryDTO dto = new ResourceEvaluationsSummaryDTO();
            ResourceEvaluationDTO eval = new ResourceEvaluationDTO();
            ResourceEvaluationDimension dimension;

            ResBiz.Resource_Evaluation efEntity = new ResBiz.Resource_Evaluation();
            //string certificationObject = "isle.achieve.evaluation.certified";

            if ( certificationObject != null && certificationObject.Length > 0 )
            {
                ApplicationRolePrivilege privileges = new ApplicationRolePrivilege();
                if ( user != null && user.Id > 0 )
                    privileges = SecurityManager.GetGroupObjectPrivileges( user, certificationObject );
                else
                    privileges.CreatePrivilege = 0;

                if ( privileges != null && privileges.CanCreate() )
                    dto.CurrentUserHasCertification = true;
            } else 
            {
                //really means user can do eval!
                dto.CurrentUserHasCertification = true;
            }

            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    List<ResBiz.Resource_Evaluation> list = context.Resource_Evaluation
                            .Include( "Resource_EvaluationSection" )
                            .Where( s => s.ResourceIntId == resourceId && s.EvaluationId == evaluationId )
                            .OrderBy( s => s.Created )
                            .ToList();
                    foreach ( ResBiz.Resource_Evaluation re in list )
                    {
                        eval = new ResourceEvaluationDTO();
                        eval.Id = re.Id;
                        eval.ResourceIntId = re.ResourceIntId;
                        eval.EvaluationId = re.EvaluationId == null ? 0 : ( int ) re.EvaluationId;
                        eval.Score = re.Score == null ? 0 : ( int ) re.Score;
                        //eval.Score = re.Score;
                        eval.UserHasCertification = ( bool ) re.UserHasCertification;

                        eval.CreatedById = ( int ) re.CreatedById;
                        if ( eval.CreatedById == user.Id )
                        {
                            if ( eval.UserHasCertification )
                                dto.CurrentUserHasACertifiedEvaluation = true;
                            else
                                dto.CurrentUserHasAnUnCertifiedEvaluation = true;
                        }

                        foreach ( ResBiz.Resource_EvaluationSection res in re.Resource_EvaluationSection )
                        {
                            dimension = new ResourceEvaluationDimension();
                            dimension.Id = res.Id;
                            dimension.ResourceEvalId = res.ResourceEvalId;
                            //dimension.StandardId = res.StandardId == null ? 0 : ( int ) res.StandardId;
                            dimension.EvalDimensionId = res.EvalDimensionId == null ? 0 : ( int ) res.EvalDimensionId;

                            dimension.Score = res.Score;
                            dimension.CreatedById = res.CreatedById == null ? 0 : ( int ) res.CreatedById;
                            dimension.Created = res.Created;

                            eval.Dimensions.Add( dimension );
                        }

                        if ( eval.UserHasCertification )
                            dto.CerifiedEvaluations.Add( eval );
                        else
                            dto.UnCertifiedEvaluations.Add( eval );
                    }

                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetAchieveEvaluations()" );
                statusMessage = ex.Message;
            }

            return dto;
        }


        public static List<EvaluationDTO> Evaluations_GetAllTools()
        {
            ThisUser user = new ThisUser();
            return Evaluations_GetAllTools( user );
        }


        public static List<EvaluationDTO> Evaluations_GetAllTools( ThisUser user )
        {
            List<EvaluationDTO> evalList = new List<EvaluationDTO>();
            if ( user == null )
                user = new ThisUser();

            EvaluationDTO eval = new EvaluationDTO();
            EvaluationDimensionDTO dim = new EvaluationDimensionDTO();
            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    List<ResBiz.Evaluation> list = context.Evaluations
                        .Include( "Evaluation_Dimension" )
                        .Where( s => s.IsActive == true )
                        .OrderBy( s => s.Title)
                        .ToList();

                    if ( list != null && list.Count > 0 )
                    {
                        foreach ( ResBiz.Evaluation evaluation in list )
                        {
                            eval = new EvaluationDTO();

                            eval.Id = evaluation.Id;
                            eval.Title = evaluation.Title;
                            eval.ShortName = evaluation.ShortName;
                            eval.Description = evaluation.Description;
                            eval.RequiresCertification = evaluation.RequiresCertification == null ? false : ( bool ) evaluation.RequiresCertification;
                            eval.PrivilegeCode = evaluation.PrivilegeCode == null ? "" : evaluation.PrivilegeCode;
                            
                            eval.CanUserDoEvaluation = true;

                            if ( eval.RequiresCertification && eval.PrivilegeCode.Length > 0 )
                            {
                                eval.CanUserDoEvaluation = false;
                                ApplicationRolePrivilege privileges = new ApplicationRolePrivilege();
                                if (user.Id > 0)
                                    privileges = SecurityManager.GetGroupObjectPrivileges( user, eval.PrivilegeCode );

                                if ( privileges != null && privileges.CanCreate() )
                                    eval.CanUserDoEvaluation = true;
                            }

                            if ( evaluation.Evaluation_Dimension != null && evaluation.Evaluation_Dimension.Count > 0 )
                            {
                                foreach ( ResBiz.Evaluation_Dimension ed in evaluation.Evaluation_Dimension )
                                {
                                    dim = new EvaluationDimensionDTO();
                                    if ( ed.IsActive )
                                    {
                                        dim.Id = ed.Id;
                                        dim.EvaluationId = ed.EvaluationId;
                                        dim.DimensionId = ed.DimensionId;
                                        dim.Title = ed.Title;
                                        dim.Description = ed.Description;
                                        dim.ShortName = ed.ShortName;
                                        dim.Url = ed.Url;

                                        eval.Dimensions.Add( dim );
                                    }
                                }
                            }

                            evalList.Add( eval );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Evaluations_GetAllTools()" );
            }

            return evalList;
        }
        /// <summary>
        /// Retrieve an evaluation and its components
        /// </summary>
        /// <param name="evaluationId"></param>
        /// <returns></returns>
        public static EvaluationDTO Evaluation_GetComponents( int evaluationId, bool includingDimensions )
        {
            EvaluationDTO dto = new EvaluationDTO();
            EvaluationDimensionDTO dim = new EvaluationDimensionDTO();
            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    ResBiz.Evaluation evaluation = context.Evaluations
                        .Include( "Evaluation_Dimension" )
                        .FirstOrDefault( s => s.Id == evaluationId );
                    if ( evaluation != null && evaluation.Id > 0 )
                    {
                        dto.Id = evaluation.Id;
                        dto.Title = evaluation.Title;
                        dto.ShortName = evaluation.ShortName;
                        dto.Description = evaluation.Description;
                        dto.RequiresCertification = evaluation.RequiresCertification == null ? false : ( bool ) evaluation.RequiresCertification;
                        dto.PrivilegeCode = evaluation.PrivilegeCode == null ? "" : evaluation.PrivilegeCode;

                        if ( includingDimensions )
                        {
                            if ( evaluation.Evaluation_Dimension != null && evaluation.Evaluation_Dimension.Count > 0 )
                            {
                                foreach ( ResBiz.Evaluation_Dimension ed in evaluation.Evaluation_Dimension )
                                {
                                    dim = new EvaluationDimensionDTO();
                                    if ( ed.IsActive )
                                    {
                                        dim.Id = ed.Id;
                                        dim.EvaluationId = ed.EvaluationId;
                                        dim.DimensionId = ed.DimensionId;
                                        dim.Title = ed.Title;
                                        dim.Description = ed.Description;
                                        dim.ShortName = ed.ShortName;
                                        dim.Url = ed.Url;

                                        dto.Dimensions.Add( dim );
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetEvaluationComponents()" );
            }

            return dto;
        }
        #endregion

        #region ResourceStandards methods
        /// <summary>
        /// Select ResourceStandards By Version - called by curriculum display for now
        /// </summary>
        /// <param name="resourceVersionId"></param>
        /// <returns></returns>
        private ResourceStandardCollection SelectResourceStandardsByVersion( int resourceVersionId )
        {
            ResourceVersion entity = new ResourceVersionManager().Get( resourceVersionId );
            if ( entity != null && entity.Id > 0 )
            {
                return ResourceStandards_Select( entity.ResourceIntId );
            }
            else
            {
                return new ResourceStandardCollection();
            }
        }

        /// <summary>
        /// Select ResourceStandards By resource Id - called by curriculum display for now
        /// </summary>
        /// <param name="resourceIntID"></param>
        /// <returns></returns>
        public ResourceStandardCollection ResourceStandards_Select( int resourceIntID )
        {
            ResourceStandardManager standardManager = new ResourceStandardManager();
            ResourceStandardCollection standardsList = standardManager.Select( resourceIntID );
            return standardsList;

        }

        public ResourceStandardCollection ResourceStandards_SelectWithRating( int resourceIntID )
        {
            ResourceStandardManager standardManager = new ResourceStandardManager();
            ResourceStandardCollection standardsList = standardManager.Select( resourceIntID );
            return standardsList;

        }

        /// <summary>
        /// Create a resource standard evaluation, using a resource.Id and StandardNode.Id - the latter refer to an existing  Resource.Standard record
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="standardId"></param>
        /// <param name="createdById"></param>
        /// <param name="score"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int ResourceStandardEvaluation_Create( int resourceId, int standardId, int createdById, int score, ref string status )
        {
            status = "successful";
            int retVal = 0;
            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    ResBiz.Resource_Standard rs = context.Resource_Standard
                        .Where( s => s.ResourceIntId == resourceId && s.StandardId == standardId )
                        .FirstOrDefault();
                    if ( rs != null && rs.Id > 0 )
                    {
                        return ResourceStandardEvaluation_Create( rs.Id, createdById, score, ref status );
                    }
                    else
                    {
                        status = "error - resource standard record not found";
                    }
                }
            }
            catch ( Exception ex )
            {
                LogError( thisClassName + ".ResourceStandardEvaluation_Create(): " + ex.ToString() );
                status = thisClassName + ".ResourceStandardEvaluation_Create(): " + ex.Message;
            }

            return retVal;
        }// Create

        /// <summary>
        /// Create a resource standard evaluation using an existing Resource.Standard.Id
        /// </summary>
        /// <param name="resourceStandardId"></param>
        /// <param name="createdById"></param>
        /// <param name="score"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int ResourceStandardEvaluation_Create( int resourceStandardId, int createdById, int score, ref string status )
        {
            status = "successful";
            int retVal = 0;
            ResBiz.Resource_StandardEvaluation efEntity = new ResBiz.Resource_StandardEvaluation();
            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    efEntity.ResourceStandardId = resourceStandardId;
                    efEntity.Score = score;
                    efEntity.CreatedById = createdById;
                    efEntity.Created = System.DateTime.Now;

                    context.Resource_StandardEvaluation.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                        return efEntity.Id;
                }
            }
            catch ( Exception ex )
            {
                LogError( thisClassName + ".ResourceStandardEvaluation_Create(): " + ex.ToString() );
                status = thisClassName + ".ResourceStandardEvaluation_Create(): " + ex.Message;
            }

            return retVal;
        }// Create

        private static int StandardEvaluation_Create( ResourceEvaluation evaluation, ref string status )
        {
            status = "successful";
            int retVal = 0;
            ResBiz.Resource_StandardEvaluation efEntity = new ResBiz.Resource_StandardEvaluation();
            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    efEntity.ResourceStandardId = evaluation.ResourceStandardId;
                    efEntity.Score = evaluation.Score;
                    efEntity.CreatedById = evaluation.CreatedById;
                    efEntity.Created = System.DateTime.Now;

                    context.Resource_StandardEvaluation.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                }

                //get resour
                #region parameters
                //will look up by resId and standard id, then do add
                SqlParameter[] parameters = new SqlParameter[ 4 ];
                parameters[ 0 ] = new SqlParameter( "@ResourceIntId", evaluation.ResourceIntId );
                parameters[ 1 ] = new SqlParameter( "@StandardId", evaluation.StandardId );
                parameters[ 2 ] = new SqlParameter( "@CreatedById", evaluation.CreatedById );
                parameters[ 3 ] = new SqlParameter( "@Value", evaluation.Value );

                #endregion

                //DataSet ds = SqlHelper.ExecuteDataset( ConnString, CommandType.StoredProcedure, INSERT_PROC, parameters );
                //if ( DoesDataSetHaveRows( ds ) )
                //{
                //    retVal = GetRowColumn( ds.Tables[ 0 ].Rows[ 0 ], "Id", 0 );
                //}
            }
            catch ( Exception ex )
            {
                LogError( thisClassName + ".CreateStandardEvaluation(): " + ex.ToString() );
                status = thisClassName + ".CreateStandardEvaluation(): " + ex.Message;
            }

            return retVal;
        }// Create

        /// <summary>
        /// Retrieve all standards aligned to the resource, as well as all ratings.
        /// Also check if the provided user has done a rating for the standard.
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="user"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static List<ResourceStandardEvaluationSummary> ResourceStandardEvaluation_GetAll( int resourceId, ThisUser user, ref string statusMessage )
        {

            List<ResourceStandardEvaluationSummary> resList = new List<ResourceStandardEvaluationSummary>();
            ResourceStandardEvaluationSummary eval = new ResourceStandardEvaluationSummary();
            ResBiz.Resource_StandardEvaluationSummary resEvalSummary = new ResBiz.Resource_StandardEvaluationSummary();

            List<ResBiz.Resource_StandardEvaluationList> userEvals = new List<ResBiz.Resource_StandardEvaluationList>();

            //get all evals for resource
            try
            {
                using ( var context = new ResBiz.ResourceContext() )
                {
                    if ( user != null && user.Id > 0 )
                    {
                        ///get any user evaluations
                        userEvals = context.Resource_StandardEvaluationList
                            .Where( s => s.CreatedById == user.Id && s.ResourceIntId == resourceId )
                            .OrderBy( s => s.Created )
                            .ToList();
                    }

                    List<ResBiz.Resource_Standard> list = context.Resource_Standard
                            .Include( "StandardBody_Node" )
                            .Where( s => s.ResourceIntId == resourceId )
                            .ToList();
                    foreach ( ResBiz.Resource_Standard re in list )
                    {
                        eval = new ResourceStandardEvaluationSummary();

                        eval.ResourceIntId = re.ResourceIntId;
                        eval.StandardId = re.StandardId;
                        eval.AverageRating = -1;
                        eval.TotalRatings = 0;
                        eval.HasUserRated = false;
                        eval.AlignmentTypeId = re.AlignmentTypeCodeId ?? 0;

                        if ( re.StandardBody_Node != null )
                        {
                            eval.NotationCode = re.StandardBody_Node.NotationCode;
                            eval.Description = re.StandardBody_Node.Description;
                        }

                        //check for an eval summary
                        resEvalSummary = context.Resource_StandardEvaluationSummary
                                   .FirstOrDefault( s => s.StandardId == re.StandardId && s.ResourceIntId == re.ResourceIntId );

                        if ( resEvalSummary != null && resEvalSummary.ResourceIntId > 0 )
                        {
                            eval.AverageRating = resEvalSummary.AverageScorePercent != null ? (int) resEvalSummary.AverageScorePercent : 0;
                            eval.TotalRatings = ( int )resEvalSummary.TotalEvals;
                            if ( user.Id > 0 )
                            {
                                if ( userEvals.Count > 0 )
                                {
                                    foreach ( ResBiz.Resource_StandardEvaluationList ueval in userEvals )
                                    {
                                        if ( ueval.StandardId == eval.StandardId )
                                        {
                                            eval.HasUserRated = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        resList.Add( eval );
                    }

                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetAllStandardEvaluationsForResource()" );
                statusMessage = ex.Message;
            }

            return resList;
        }

        public static void TestJoins()
        {
            int resourceIntId = 444671;
            using ( var context = new ResBiz.ResourceContext() )
            {
                // use DefaultIfEmpty() to do a left join (otherwise it is an inner join.
                var results = ( from rs in context.Resource_Standard
                                join st in context.StandardBody_Node
                                    on rs.StandardId equals st.Id
                                //join rse in context.Resource_StandardEvaluationSummary.DefaultIfEmpty()
                                //    on rs.Id equals rse.ResourceIntId
                                where rs.ResourceIntId == resourceIntId
                                select new { rs.ResourceIntId, rs.StandardId, st.Description, st.NotationCode, rs.CreatedById } ).ToList();
                if ( results != null )
                {
                    //Success code
                }

            }
        }
        #endregion

        #region ResourceVersion methods
        public static ResourceVersion ResourceVersion_Get( int rvId )
        {
            ResourceVersion rv = new ResourceVersionManager().Get( rvId );

            return rv;
        }// 
        public static ResourceVersion ResourceVersion_GetByResourceId( int resourceId )
        {
          return ResourceVersion_GetByResourceId( resourceId, true );
        }// 
        public static ResourceVersion ResourceVersion_GetByResourceId( int resourceId, bool mustBeActive )
        {
          ResourceVersion rv = EfMgr.ResourceVersion_GetByResourceId( resourceId, mustBeActive );
          
          return rv;
        }
        public static string ResourceVersion_SyncContentItemChanges( string title, string summary, int resourceId )
        {
            string result = "";
            //ResourceVersion rv = new ResourceVersionManager().GetByResourceId( resourceId );
            ResourceVersion rv = EfMgr.ResourceVersion_GetByResourceId( resourceId );
            if ( rv.Title != title || rv.Description != summary )
            {
                rv.Title = title;
                rv.Description = summary;

                result = new ResourceVersionManager().UpdateById( rv );
            }

            return result;
        }// 
        public static string ResourceVersion_SyncContentItemChanges( int rvId, string title, string summary )
        {
            string result = "";
            ResourceVersion rv = new ResourceVersionManager().Get( rvId );
            if ( rv.Title != title || rv.Description != summary )
            {
                rv.Title = title;
                rv.Description = summary;

                result = new ResourceVersionManager().UpdateById( rv );
            }

            return result;
        }// 

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

        #region Resource url methods
        /// <summary>
        /// Format a friendly url for resource
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string FormatFriendlyResourceUrl( ResourceVersion entity )
        {
            //TODO - may want to remove the protocol to handle where used on secure pages?
            string title = FormatFriendlyTitle( entity.Title );

            //use site root - in case called from a web service, outside the project!!
            var siteRoot = UtilityManager.GetAppKeyValue( "siteRoot", "http://ioer.ilsharedlearning.org" );

            if (entity.ResourceIntId > 0)
                return UtilityManager.FormatAbsoluteUrl( siteRoot + string.Format( "/Resource/{0}/{1}", entity.ResourceIntId, title ), false );
            else
                return UtilityManager.FormatAbsoluteUrl( siteRoot + string.Format( "/IOER/{0}/{1}", entity.Id, title ), false );
        }

        public static string FormatFriendlyResourceUrlByResId( string resTitle, int resourceId )
        {
            string title = FormatFriendlyTitle( resTitle );
            return UtilityManager.FormatAbsoluteUrl( string.Format( "/Resource/{0}/{1}", resourceId, title ), true );
        }

        public static string FormatFriendlyResourceUrlByRvId( string resTitle, int resVersionId )
        {
            string title = FormatFriendlyTitle( resTitle );
            return UtilityManager.FormatAbsoluteUrl( string.Format( "/IOER/{0}/{1}", resVersionId, title ), true );
        }

        /// <summary>
        /// Format a string for friendly url display
        /// NOTE: with change to use the sort title, a number of the rules can be skipped
        /// ==> note that we need to handle authored content first
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatFriendlyTitle( string text )
        {
            if ( text == null || text.Trim().Length == 0 )
                return "";

            string title = ResourceVersion.UrlFriendlyTitle( text );


            title = HttpUtility.HtmlEncode( title );
            return title;
        }//
        /// <summary>
        /// Get or format the display image for the resource
        /// </summary>
        /// <param name="resourceUrl"></param>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public static string GetResourceImageUrl( string resourceUrl, int resourceIntId )
        {
            string imageUrl = "";
            //need to handle special types
            //also should not hard-code domain - maybe should be at the controller level!
            if ( resourceUrl != null && resourceUrl.Length > 5 )
            {
                string url = resourceUrl.ToLower().Trim();

                if ( url.EndsWith( ".swf" ) ) imageUrl = SwfImageUrl;
                else if ( url.EndsWith( ".ppt" ) ) imageUrl = PPTImageUrl;
                else if ( url.EndsWith( ".pptx" ) ) imageUrl = PPTImageUrl;
                else if ( url.EndsWith( ".xls" ) ) imageUrl = XlxImageUrl;
                else if ( url.EndsWith( ".xlzx" ) ) imageUrl = XlxImageUrl;
                else if ( url.EndsWith( ".doc" ) ) imageUrl = WordImageUrl;
                else if ( url.EndsWith( ".docx" ) ) imageUrl = WordImageUrl;
                //else if ( url.EndsWith( ".pdf" ) ) imageUrl = PdfImageUrl;  //now shows content
                else
                    imageUrl = string.Format( "//ioer.ilsharedlearning.org/OERThumbs/large/{0}-large.png", resourceIntId );
            }

            return imageUrl;
        }
        /// <summary>
        /// Get or format the thumbnail image for the resource
        /// </summary>
        /// <param name="resourceUrl"></param>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public static string GetResourceThumbnailImageUrl( string resourceUrl, int resourceIntId )
        {
            //thumbnail is the same now, just use same method
            return GetResourceImageUrl( resourceUrl, resourceIntId );

            //string imageUrl = "";
            ////need to handle special types
            ////also should not hard-code domain - maybe should be at the controller level!
            //if ( resourceUrl != null && resourceUrl.Length > 5 )
            //{
            //    string url = resourceUrl.ToLower().Trim();

            //    if ( url.EndsWith( ".swf" ) )  imageUrl = SwfImageUrl;
            //    else if ( url.EndsWith( ".ppt" ) )  imageUrl = PPTImageUrl;
            //    else if ( url.EndsWith( ".pptx" ) ) imageUrl = PPTImageUrl;
            //    else if ( url.EndsWith( ".xls" ) )  imageUrl = XlxImageUrl;
            //    else if ( url.EndsWith( ".xlzx" ) ) imageUrl = XlxImageUrl;
            //    else if ( url.EndsWith( ".doc" ) )  imageUrl = WordImageUrl;
            //    else if ( url.EndsWith( ".docx" ) ) imageUrl = WordImageUrl;
            //    //else if ( url.EndsWith( ".pdf" ) ) imageUrl = PdfImageUrl;
            //    else
            //        imageUrl = string.Format( "//ioer.ilsharedlearning.org/OERThumbs/large/{0}-large.png", resourceIntId );
            //}

            //return imageUrl;
        }
        #endregion


        #region === resource.comment methods ===
        public int Resource_AddComment( int resourceId, string comment, int pCreatedById, ref string statusMessage )
        {
            ResourceCommentManager mgr = new ResourceCommentManager();
            ResourceComment entity = new ResourceComment();
            entity.ResourceIntId = resourceId;
            entity.Comment = comment;
            entity.CreatedById = pCreatedById;
            //????????????????????
            entity.CreatedBy = new PatronManager().Get( pCreatedById ).FullName();
            //entity.IsActive = true;

            var commentID = mgr.Create( entity, ref statusMessage );
            if ( commentID > 0 )
            {
                //??why
                new ElasticSearchManager().RefreshResource( resourceId );
                
            }
            else
            {
                statusMessage = "Error: comment was not created:<br/> " + statusMessage;
            }
            return commentID;
        }

        public List<ResourceComment> Resource_GetComments( int resourceId )
        {
            var list = new List<ResourceComment>();
            list = new ResourceCommentManager().SelectList( resourceId );
           
            return list;
        }
        #endregion
        #region === resource.like methods ===
        public int AddLikeDislike( string userGUID, int resourceId, bool isLike )
        {
            int id = 0;
            string statusMessage = "";
            Patron user = new AccountServices().GetByRowId( userGUID );

            if ( user.Id == 0 )
            {
                return 0;
            } else 
                return AddLikeDislike( user.Id, resourceId, isLike );

        }
        public int AddLikeDislike( int userId, int resourceId, bool isLike )
        {
            int id = 0;
            string statusMessage = "";
            
            var manager = new ResourceLikeManager();

            //check to ensure doesn't exist?
            //ResourceLikeSummary likes = Resource_GetLikeSummmary( resourceId, user.Id, ref statusMessage );

            ResourceLike like = new ResourceLike();

            like.IsLike = isLike;
            like.CreatedById = userId;
            like.ResourceIntId = resourceId;


            id = manager.Create( like, ref statusMessage );

            return id;
        }

        public static ResourceLikeSummary Resource_GetLikeSummmary( int resourceIntId, int userId, ref string status )
        {
            ResourceLikeSummary entity = new ResourceLikeSummary();
            entity = new ResourceLikeSummaryManager().GetForDisplay( resourceIntId, userId, ref status );

            return entity;

        }

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
