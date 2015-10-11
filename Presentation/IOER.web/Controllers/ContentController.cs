using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AppUser = ILPathways.Business.Patron; //ILPathways.Business.AppUser;
using MyManager = Isle.BizServices.ContentServices;
using AcctManager = Isle.BizServices.AccountServices;
using OrgManager = Isle.BizServices.OrganizationBizService;
using GroupManager = Isle.BizServices.GroupServices;

using ILPathways.Business;
using IOER.Library;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;
using ResourceManager = LRWarehouse.DAL.ResourceManager;

namespace IOER.Controllers
{
    /// <summary>
    /// Methods related to Content items
    /// </summary>
    public class ContentController
    {
        static string thisClassName = "ContentController";


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

		//public void CreateContentHistory_Approve( ContentItem entity )
		//{
		//	//string statusMessage = "";
		//	//string msg = string.Format( "The content item: {0} by author: {1} was approved by {2}", entity.Title, entity.Author, WebUser.FullName() );

		//	//myManager.ContentHistory_Create( entity.Id, "Content Approved", msg, WebUser.Id, ref statusMessage );
		//}



    }
}