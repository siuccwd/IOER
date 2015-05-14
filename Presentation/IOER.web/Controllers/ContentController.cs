using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AppUser = LRWarehouse.Business.Patron; //ILPathways.Business.AppUser;
using MyManager = Isle.BizServices.ContentServices;
using AcctManager = Isle.BizServices.AccountServices;
using OrgManager = Isle.BizServices.OrganizationBizService;
using GroupManager = Isle.BizServices.GroupServices;

using ILPathways.Business;
using ILPathways.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;
using ResourceManager = LRWarehouse.DAL.ResourceManager;

namespace ILPathways.Controllers
{
    /// <summary>
    /// Methods related to Content items
    /// </summary>
    public class ContentController
    {
        static string thisClassName = "ContentController";
        /// <summary>
        /// Handle request to approve a content record
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="contentId"></param>
        /// <param name="author"></param>
        /// /// <param name="hasApproval">return true if approval was required</param>
        /// <param name="statusMessage"></param>
        /// <returns>true if ok, false if errors</returns>
        public static bool HandleContentApproval( Resource resource, int contentId, AppUser author, ref bool hasApproval, ref string statusMessage )
        {
            // - get record, check if on behalf is org
            //     if not, set to published?
            // - 
            bool isValid = true;
            hasApproval = false;
            MyManager mgr = new MyManager();
            ContentItem entity = mgr.Get( contentId );
            if ( entity == null || entity.Id == 0 )
            {
                //invalid, return, log error?
                statusMessage = "Invalid request, record was not found.";
                return false;
            }

            //entity.ResourceVersionId = resource.Version.Id;
            entity.ResourceIntId = resource.Id;
            entity.UseRightsUrl = resource.Version.Rights;

            if ( entity.IsOrgContent() == false )
            {
                entity.StatusId = ContentItem.PUBLISHED_STATUS;
                //entity.IsPublished = true;
                mgr.Update( entity );

                //TODO - anything else??
                return true;
            }
            hasApproval = true;
            //get approvers for org
            SubmitApprovalRequest( entity );

            //update status
            entity.StatusId = ContentItem.SUBMITTED_STATUS;
            mgr.Update( entity );
            //set resource to inactive
            string status = new ResourceManager().SetResourceActiveState( resource.Id, false );

            //add record to audit table
            string msg = string.Format("Request by {0} for approval of resource id: {1}", author.FullName(), entity.Id);

            mgr.ContentHistory_Create( contentId, "Approval Request - new", msg, author.Id, ref statusMessage );

            return isValid;
        }

        /// <summary>
        /// Request approval for a content item
        /// - resource tagging, and LR publishing (or caching) would have already occured
        /// - typically used where a previous submission was denied or author has made updates to an previously approved content item
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="author"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool RequestApproval( int contentId, AppUser author, ref string statusMessage )
        {
            //TODO  - should resource be set inactive
           
            bool isValid = true;
            
            MyManager mgr = new MyManager();
            try
            {
                ContentItem entity = mgr.Get( contentId );
                if ( entity == null || entity.Id == 0 )
                {
                    //invalid, return, log error?
                    statusMessage = "Invalid request, record was not found.";
                    return false;
                }
                //get approvers for org
                SubmitApprovalRequest( entity );

                //update status
                entity.StatusId = ContentItem.SUBMITTED_STATUS;
                mgr.Update( entity );
                //set resource to inactive
                string status = new ResourceManager().SetResourceActiveState( entity.ResourceIntId, false );

                //add record to audit table
                string msg = string.Format( "Request by {0} for approval of resource id: {1}", author.FullName(), entity.Id );

                mgr.ContentHistory_Create( entity.Id, "Approval Request - existing", msg, author.Id, ref statusMessage );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format(".RequestApproval(contentId: {0}, author: {1})", contentId, author.FullName()) );
                isValid = false;
                statusMessage = ex.Message;
            }
            return isValid;
        }


        public static void SubmitApprovalRequest( ContentItem entity )
        {
            //get administrators for org, and parent
            //format and send emails
            string statusMessage = "";
            AcctManager mgr = new AcctManager();
            AppUser author = mgr.Get( entity.CreatedById );
            string note = "";
            string toEmail = "";
            string bccEmail = "";
            //if valid
            Organization org = OrgManager.GetOrganization( author, ref statusMessage );
            if ( org != null && org.Id > 0 )
            {
                //get list of administrators
                List<GroupMember> list = GroupManager.OrgApproversSelect( org.Id );
                if ( list != null && list.Count > 0 )
                {
                    foreach ( GroupMember item in list )
                    {
                        if ( item.UserEmail.Length > 5 )
                            toEmail += item.UserEmail + ",";
                    }
                }
                else
                {
                    //if no approvers, send to info
                    toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "DoNotReply@ilsharedlearning.org" );
                    note = "<br/>NOTE: no organization approvers were found for this organization: " + org.Name;
                }
                string friendlyTitle = ResourceBizService.FormatFriendlyTitle( entity.Title );
                string url = UtilityManager.FormatAbsoluteUrl( string.Format( "/Content/{0}/{1}", entity.Id.ToString(), friendlyTitle ), true );
                string urlTitle = string.Format( "<a href='{0}'>{1}</a>", url, entity.Title );
                if (System.DateTime.Now < new System.DateTime(2013, 7, 1))
                    bccEmail = UtilityManager.GetAppKeyValue( "appAdminEmail", "info@illinoisworknet.com" );

                string subject = string.Format( "Isle request to approve an education resource from: {0}", author.FullName() );
                string body = string.Format( "<p>{0} from {1} is requesting approval on an education resource.</p>", author.FullName(), org.Name );

                //could include the override parm in the link, and just go to the display?
                //or, approver will need to see all content, even private?


                body += "<br/>url:&nbsp;" + urlTitle;
                body += "<br/><br/>From: " + author.EmailSignature();
                string from = author.Email;
                EmailManager.SendEmail( toEmail, from, subject, body, author.Email, bccEmail );
            }
        }

        /// <summary>
        /// Handle action of content approved
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="approver"></param>
        /// <param name="statusMessage"></param>
        //public static bool HandleApprovedAction( int contentId, AppUser approver, ref string statusMessage )
        //{
        //    ContentItem entity = new MyManager().Get( contentId );
        //    if ( entity != null && entity.Id > 0 )
        //        return HandleApprovedAction( entity, approver, ref statusMessage );
        //    else
        //    {
        //        statusMessage = "Error - unable to retrieve the requested resource";
        //        return false;   
        //    }
        //}

        /// <summary>
        /// Handle action of content approved
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="approver"></param>
        /// <param name="statusMessage"></param>
        //public static bool HandleApprovedAction( ContentItem entity, AppUser approver, ref string statusMessage )
        //{
        //    bool isValid = true;
        //    string bccEmail = "";

        //    statusMessage = string.Empty;
        //    MyManager mgr = new MyManager();
        //    entity.StatusId = ContentItem.PUBLISHED_STATUS;
        //    entity.ApprovedById = approver.Id;
        //    entity.Approved = System.DateTime.Now;
        //    mgr.Update( entity );

        //    //set resource to active
        //    string status = new ResourceManager().SetResourceActiveStateByResVersionId( true, entity.ResourceVersionId );

        //    //======= published cached LR data
        //    if ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
        //    {
        //       //new BaseUserControl().SetConsoleInfoMessage( "WARNING - HAVE NOT IMPLEMENTED CODE TO READ CACHED LR DATA AND DO ACTUAL LR PUBLISH" );
        //        ResourcePublishUpdateController ctr = new ResourcePublishUpdateController();
        //        if ( ctr.PublishSavedEnvelope( entity.ResourceVersionId, ref statusMessage ) == false )
        //        {
        //            new BaseUserControl().SetConsoleErrorMessage( "WARNING - the publish to the learning registry failed. System administration has been notified." );
        //            Utilities.EmailManager.NotifyAdmin( "Publish of pending resource failed", string.Format( "The attempt to publish a saved resource was unsuccessful. Content id: {0}, RV Id: {1}, Message: {2}", entity.Id, entity.ResourceVersionId,  statusMessage ) );
        //        }
        //    }

        //    //add record to audit table
        //    string msg = string.Format( "Resource: {0}, author: {1}, was approved by {2}", entity.Title, entity.Author, approver.FullName() );

        //    mgr.ContentHistory_Create( entity.Id, "Content Approved", msg, approver.Id, ref statusMessage );

        //    //send email
        //    AcctManager amgr = new AcctManager();
        //    AppUser author = amgr.Get( entity.CreatedById );

        //    string friendlyTitle = ResourceBizService.FormatFriendlyTitle( entity.Title );
        //    string url = UtilityManager.FormatAbsoluteUrl( string.Format( "/Content/{0}/{1}", entity.Id.ToString(), friendlyTitle ), true );
        //    string urlTitle = string.Format( "<a href='{0}'>{1}</a>", url, entity.Title );
        //    string toEmail = author.Email;
        //    if ( System.DateTime.Now < new System.DateTime( 2013, 7, 1 ) )
        //        bccEmail = UtilityManager.GetAppKeyValue( "systemAdminEmail", "info@illinoisworknet.com" );
            

        //    string subject = "Isle: APPROVED education resource";

        //    string body = string.Format( "<p>{0} has approved your education resource. It is now available on the website (based on the defined privilege settings).</p>", approver.FullName());
        //    body += "<br/>url:&nbsp;" + urlTitle;
        //    body += "<br/>From: " + approver.EmailSignature();
        //    string from = approver.Email;
        //    EmailManager.SendEmail( toEmail, from, subject, body, approver.Email, bccEmail );


        //    return isValid;
        //}

        public void CreateContentHistory_Approve( ContentItem entity )
        {
            //string statusMessage = "";
            //string msg = string.Format( "The content item: {0} by author: {1} was approved by {2}", entity.Title, entity.Author, WebUser.FullName() );

            //myManager.ContentHistory_Create( entity.Id, "Content Approved", msg, WebUser.Id, ref statusMessage );
        }

        /// <summary>
        /// Handle action of content denied
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="reason"></param>
        /// <param name="approver"></param>
        /// <param name="statusMessage"></param>
        public static bool HandleDeclinedAction( int contentId, string reason, AppUser approver, ref string statusMessage )
        {
            ContentItem entity = new MyManager().Get( contentId );
            if ( entity != null && entity.Id > 0 )
                return HandleDeclinedAction( entity, reason, approver, ref statusMessage );
            else
            {
                //TBD
                return false;
            }
        }

        /// <summary>
        /// Handle action of content denied
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="reason"></param>
        /// <param name="approver"></param>
        /// <param name="statusMessage"></param>
        public static bool HandleDeclinedAction( ContentItem entity, string reason, AppUser approver, ref string statusMessage )
        {
            bool isValid = true;

            statusMessage = string.Empty;
            MyManager mgr = new MyManager();
            entity.StatusId = ContentItem.REVISIONS_REQUIRED_STATUS;
            //not actually used yet
            entity.ApprovedById = 0;
            entity.Approved = entity.DefaultDate;
            mgr.Update( entity );

            //set resource to inactive
            string status = new ResourceManager().SetResourceActiveState( entity.ResourceIntId, true );
            //add record to audit table
            string msg = string.Format( "Resource: {0}, author: {1}, was declined by {2}", entity.Title, entity.Author, approver.FullName() );

            mgr.ContentHistory_Create( entity.Id, "Approval Request Denied", msg, approver.Id, ref statusMessage );

            //send email
            AcctManager amgr = new AcctManager();
            AppUser author = amgr.Get( entity.CreatedById );

            string friendlyTitle = ResourceBizService.FormatFriendlyTitle( entity.Title );
            string url = UtilityManager.FormatAbsoluteUrl( string.Format( "/Content/{0}/{1}", entity.Id.ToString(), friendlyTitle ), true );
            string urlTitle = string.Format( "<a href='{0}'>{1}</a>", url, entity.Title );
            string toEmail = author.Email;
            string bccEmail = UtilityManager.GetAppKeyValue( "systemAdminEmail", "info@illinoisworknet.com" );

            string subject = "Isle: Require updates in order to approve education resource";

            string body = string.Format( "<p>{0} has reviewed your education resource. The resource has not been approved. Please refer to the following for reasons/instructions.<br/>{1}</p>", approver.FullName() );
            body += "<br/>url:&nbsp;" + urlTitle;
            body += "<br/>From: " + approver.EmailSignature();
            string from = approver.Email;
            EmailManager.SendEmail( toEmail, from, subject, body, approver.Email, bccEmail );


            return isValid;
      }


        public static bool IsUserOrgApprover( ContentItem entity, int checkUserId )
        {
            return IsUserOrgApprover( entity.OrgId, entity.CreatedById, checkUserId );
        }


        public static bool IsUserOrgApprover( int contentOrgId, int contentCreatedById, int checkUserId )
        {
            string statusMessage = "";
            if ( contentOrgId > 0 )
            {
                if ( GroupManager.IsUserAnOrgApprover( contentOrgId, checkUserId ) )
                    return true;
                else
                    return false;
            }
            AcctManager mgr = new AcctManager();
            AppUser author = mgr.Get( contentCreatedById );
            //if valid
            Organization org = OrgManager.GetOrganization( author, ref statusMessage );
            if ( org != null && org.Id > 0 )
            {
                if ( GroupManager.IsUserAnOrgApprover( org.Id, checkUserId ) )
                    return true;
            }
            return false;

        }
    }
}