using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;


using Isle.DTO;

using ILPathways.Business;
using ILPathways.Utilities;
using ILPathways.Common;
using MyManager = ILPathways.DAL.ContentManager;
using DocManager = ILPathways.DAL.DocumentStoreManager;
using DBM = ILPathways.DAL.DatabaseManager;

using EFDAL = IoerContentBusinessEntities;
using EFManager = IoerContentBusinessEntities.EFContentManager;
using AcctManager = Isle.BizServices.AccountServices;
using GroupManager = Isle.BizServices.GroupServices;
using ResourceManager = Isle.BizServices.ResourceBizService;

using ThisUser = LRWarehouse.Business.Patron;
using LRWarehouse.Business;
using ResourceDALManager = LRWarehouse.DAL.ResourceManager;

namespace Isle.BizServices
{
    public class ContentServices : ServiceHelper
    {
        static string thisClassName = "ContentServices";
        MyManager myMgr = new MyManager();

        EFDAL.IsleContentContext ctx = new EFDAL.IsleContentContext();
        EFManager myEfManager = new EFManager();
		/// <summary>
		/// Default constructor
		/// </summary>
        public ContentServices()
		{ }//

		#region ====== Core Methods ===============================================
		/// <summary>
		/// Delete a ContentItem record
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
		{
            bool isValid = false;
            //get item in order to handle related actions
            ContentItem entity = Get( pId );
            if ( entity != null && entity.Id > 0 )
            {
                //may need to check if part of a connector relationship
                //also, related doc version is deleted.
                //isValid = myEfManager.Content_Delete( pId, ref statusMessage );
                //15-01-28 MP - moved up delete to here to enable use of other services at this level
                isValid = Content_Delete( pId, ref statusMessage );
                //assumes if failed, the status message will have details

                //if resource exists, need to set inactive
                if ( entity.HasResourceId() )
                {
                    new ResourceBizService().Resource_SetInactive( entity.ResourceIntId, ref statusMessage );
                }
            }
            return isValid;
		}//
        private bool Content_Delete( int id, ref string statusMessage )
        {
            bool isSuccessful = false;
            try
            {
                statusMessage = "";
                using ( var context = new EFDAL.IsleContentContext() )
                {
                    EFDAL.Content item = context.Contents.SingleOrDefault( s => s.Id == id );

                    if ( item != null && item.Id > 0 )
                    {
                        //will need to delete resource
                        //14-12-04 MP - added code at the services level to check this!
                        //15-01-28 MP - note in a cascade delete, the resources will need to be handled!!
                        int resourceId = item.ResourceIntId == null ? 0 : ( int ) item.ResourceIntId;

                        context.Contents.Remove( item );
                        context.SaveChanges();
                        isSuccessful = true;

                        //TODO - need to check for and delete a related resource
                        //delete doc version
                        if ( item.DocumentRowId != null
                            && item.DocumentRowId.ToString().Length == 36
                            && item.DocumentRowId.ToString() != ContentItem.DEFAULT_GUID )
                        {
                            myEfManager.Document_Version_Delete( item.DocumentRowId, ref statusMessage );
                        }
                       

                        // =========== delete child nodes ==========================
                        List<EFDAL.Content> eflist = context.Contents
                            .Where( s => s.ParentId == id )
                            .OrderBy( s => s.Id )
                            .ToList();

                        if ( eflist != null && eflist.Count > 0 )
                        {
                            foreach ( EFDAL.Content efom in eflist )
                            {
                                Content_Delete( efom.Id, ref statusMessage );
                                //if resource exists, need to set inactive
                                if ( efom.ResourceIntId != null && efom.ResourceIntId > 0 )
                                {
                                    new ResourceBizService().Resource_SetInactive( ( int ) efom.ResourceIntId, ref statusMessage );
                                }
                            }
                            statusMessage = string.Format( "Also removed all child items ({0})", eflist.Count );
                        }

                    }
                }
            }
            catch ( Exception ex )
            {
                isSuccessful = false;
                statusMessage = ex.Message;
            }
            return isSuccessful;
        }


		/// <summary>
		/// Add a ContentItem record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public int Create( ContentItem entity, ref string statusMessage )
		{
            //return myMgr.Create( entity, ref statusMessage );
            int contentId = Create_ef( entity, ref statusMessage );

            //until a better process is designed, always add creator of a learning list as an admin partner
            if (contentId > 0 && entity.TypeId == ContentItem.CURRICULUM_CONTENT_ID)
            {
                Content_AddCreatorPartner(contentId, entity.CreatedById, ref statusMessage);
            }
            return contentId;
		}
        public int Create_ef( ContentItem entity, ref string statusMessage )
        {
            int contentId = myEfManager.ContentAdd(entity, ref statusMessage);

            //until a better process is designed, always add creator of a learning list as an admin partner
            if (contentId > 0 && entity.TypeId == ContentItem.CURRICULUM_CONTENT_ID)
            {
                Content_AddCreatorPartner(contentId, entity.CreatedById, ref statusMessage);
            }
            return contentId;
        }
		/// <summary>
		/// Update an Content record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
        public string Update( ContentItem entity )
		{
			return Update( entity, true );
		}//

				public string Update( ContentItem entity, bool updateResourceVersion )
				{
            string result = "";
            //if ( 1 == 1 )
            //result = myMgr.ContentUpdate( entity );
            //else
            //{
                if ( myEfManager.ContentUpdate( entity ) )
                    result = "successful";
            //}
            if ( entity.HasResourceId() && updateResourceVersion )
            {
                //may want to check on sync of RV content!
                //should only do if published status?
                new ResourceBizService().ResourceVersion_SyncContentItemChanges( entity.Title, entity.Summary, entity.ResourceIntId );
            }
            return result;

				}

        /// <summary>
        /// Update ContentItem with related ResourceVersionId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resourceVersionId"></param>
        /// <returns></returns>
        [Obsolete]
        private string UpdateResourceVersionId( int id, int resourceVersionId )
        {
            //return myMgr.UpdateResourceVersionId( id, resourceVersionId );
            return "obsolete";
        }//

        /// <summary>
        /// Do a quick update to ensure image is generated correctly
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public string UpdateAfterPublish( int contentId)
        {
            ContentItem item = Get( contentId );
            item.StatusId = ContentItem.PUBLISHED_STATUS;
            //item.IsPublished = true;

            return UpdateAfterQuickPub( item );
        }//
        
        /// <summary>
        /// Do a quick update to ensure image is generated correctly
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string UpdateAfterQuickPub( ContentItem entity )
        {
            //check on usage rights
            if ( entity.UseRightsUrl != null && entity.UseRightsUrl.Length > 5 && entity.ConditionsOfUseId == 0 )
            {
                //call method to look up url
                //if found set 1, else use 4. Read the Fine Print
            }
            string result = ""; //myMgr.Update( entity );
            if ( myEfManager.ContentUpdate( entity ) )
                result = "successful";
            return result;  // myMgr.Update( entity );

        }//

        public void HandleSyncResourceVersionChgs( ResourceVersion rv )
        {
            try
            {
                //ContentItem entity = EFManager.Content_GetByResourceVersionId( rv.Id );
                ContentItem entity = EFManager.Content_GetByResourceId( rv.ResourceIntId );
                if ( entity != null && entity.Id > 0 )
                {
                    entity.HasChanged = false;

                    entity.Title = rv.Title;
                    entity.Description = rv.Description;
                    if ( entity.HasChanged )
                    {
                        //may want a date check, just in case??
                        myEfManager.ContentUpdate( entity );
                        //myMgr.Update( entity );
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + "HandleSyncResourceVersionChgs()" );
            }

        }//
		#endregion
		#region === content approval ===
		/// <summary>
		/// Handle request to approve a content record
		/// </summary>
		/// <param name="resource"></param>
		/// <param name="contentId"></param>
		/// <param name="author"></param>
		/// /// <param name="hasApproval">return true if approval was required</param>
		/// <param name="statusMessage"></param>
		/// <returns>true if ok, false if errors</returns>
		public bool HandleContentApproval(Resource resource, int contentId, ThisUser author, ref bool hasApproval, ref string statusMessage)
		{
			// - get record, check if on behalf is org
			//     if not, set to published?
			// - 
			bool isValid = true;
			hasApproval = false;
			ContentItem entity = Get(contentId);
			if (entity == null || entity.Id == 0)
			{
				//invalid, return, log error?
				statusMessage = "Invalid request, record was not found.";
				return false;
			}

			//entity.ResourceVersionId = resource.Version.Id;
			entity.ResourceIntId = resource.Id;
			entity.UseRightsUrl = resource.Version.Rights;

			if (entity.IsOrgContent() == false)
			{
				entity.StatusId = ContentItem.PUBLISHED_STATUS;
				//entity.IsPublished = true;
				Update(entity);

				//TODO - anything else??
				return true;
			}
			hasApproval = true;
			//get approvers for org
			SubmitApprovalRequest(entity);

			//update status
			entity.StatusId = ContentItem.SUBMITTED_STATUS;
			Update(entity);
			//set resource to inactive
			new ResourceBizService().Resource_SetInactive(resource.Id, ref statusMessage);

			//add record to audit table
			string msg = string.Format("Request by {0} for approval of resource id: {1}", author.FullName(), entity.Id);

			ContentHistory_Create(contentId, "Approval Request - new", msg, author.Id, ref statusMessage);

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
		public bool RequestApproval(int contentId, ThisUser author, ref string statusMessage)
		{
			//TODO  - should resource be set inactive

			bool isValid = true;

			try
			{
				ContentItem entity = Get(contentId);
				if (entity == null || entity.Id == 0)
				{
					//invalid, return, log error?
					statusMessage = "Invalid request, record was not found.";
					return false;
				}
				//get approvers for org
				SubmitApprovalRequest(entity);

				//update status
				entity.StatusId = ContentItem.SUBMITTED_STATUS;
				Update(entity);
				//set resource to inactive
				new ResourceBizService().Resource_SetInactive(entity.ResourceIntId, ref statusMessage);

				//add record to audit table
				string msg = string.Format("Request by {0} for approval of resource id: {1}", author.FullName(), entity.Id);

				ContentHistory_Create(entity.Id, "Approval Request - existing", msg, author.Id, ref statusMessage);
			}
			catch (Exception ex)
			{
				LoggingHelper.LogError(ex, thisClassName + string.Format(".RequestApproval(contentId: {0}, author: {1})", contentId, author.FullName()));
				isValid = false;
				statusMessage = ex.Message;
			}
			return isValid;
		}


		public static void SubmitApprovalRequest(ContentItem entity)
		{
			//get administrators for org, and parent
			//format and send emails
			string statusMessage = "";
			AcctManager mgr = new AcctManager();
			ThisUser author = mgr.Get(entity.CreatedById);
			string note = "";
			string toEmail = "";
			string bccEmail = "";
			//if valid
			Organization org = OrganizationBizService.GetOrganization(author, ref statusMessage);
			if (org != null && org.Id > 0)
			{
				//get list of administrators
				List<GroupMember> list = GroupManager.OrgApproversSelect(org.Id);
				if (list != null && list.Count > 0)
				{
					foreach (GroupMember item in list)
					{
						if (item.UserEmail.Length > 5)
							toEmail += item.UserEmail + ",";
					}
				}
				else
				{
					//if no approvers, send to info
					toEmail = UtilityManager.GetAppKeyValue("contactUsMailTo", "DoNotReply@ilsharedlearning.org");
					note = "<br/>NOTE: no organization approvers were found for this organization: " + org.Name;
				}
				string friendlyTitle = ResourceBizService.FormatFriendlyTitle(entity.Title);
				string url = UtilityManager.FormatAbsoluteUrl(string.Format("/Content/{0}/{1}", entity.Id.ToString(), friendlyTitle), true);
				string urlTitle = string.Format("<a href='{0}'>{1}</a>", url, entity.Title);
				if (System.DateTime.Now < new System.DateTime(2013, 7, 1))
					bccEmail = UtilityManager.GetAppKeyValue("appAdminEmail", "info@illinoisworknet.com");

				string subject = string.Format("Isle request to approve an education resource from: {0}", author.FullName());
				string body = string.Format("<p>{0} from {1} is requesting approval on an education resource.</p>", author.FullName(), org.Name);

				//could include the override parm in the link, and just go to the display?
				//or, approver will need to see all content, even private?


				body += "<br/>url:&nbsp;" + urlTitle;
				body += "<br/><br/>From: " + author.EmailSignature();
				string from = author.Email;
				EmailManager.SendEmail(toEmail, from, subject, body, author.Email, bccEmail);
			}
		}
		/// <summary>
		/// Handle action of content denied
		/// </summary>
		/// <param name="contentId"></param>
		/// <param name="reason"></param>
		/// <param name="approver"></param>
		/// <param name="statusMessage"></param>
		public bool HandleDeclinedAction(int contentId, string reason, ThisUser approver, ref string statusMessage)
		{
			ContentItem entity = new MyManager().Get(contentId);
			if (entity != null && entity.Id > 0)
				return HandleDeclinedAction(entity, reason, approver, ref statusMessage);
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
		public bool HandleDeclinedAction(ContentItem entity, string reason, ThisUser approver, ref string statusMessage)
		{
			bool isValid = true;

			statusMessage = string.Empty;
			entity.StatusId = ContentItem.REVISIONS_REQUIRED_STATUS;
			//not actually used yet
			entity.ApprovedById = 0;
			entity.Approved = entity.DefaultDate;
			Update(entity);

			//set resource to inactive
			new ResourceBizService().Resource_SetInactive(entity.ResourceIntId, ref statusMessage);

			//add record to audit table
			string msg = string.Format("Resource: {0}, author: {1}, was declined by {2}", entity.Title, entity.Author, approver.FullName());

			ContentHistory_Create(entity.Id, "Approval Request Denied", msg, approver.Id, ref statusMessage);

			//send email
			AcctManager amgr = new AcctManager();
			ThisUser author = amgr.Get(entity.CreatedById);

			string friendlyTitle = ResourceBizService.FormatFriendlyTitle(entity.Title);
			string url = UtilityManager.FormatAbsoluteUrl(string.Format("/Content/{0}/{1}", entity.Id.ToString(), friendlyTitle), true);
			string urlTitle = string.Format("<a href='{0}'>{1}</a>", url, entity.Title);
			string toEmail = author.Email;
			string bccEmail = UtilityManager.GetAppKeyValue("systemAdminEmail", "info@illinoisworknet.com");

			string subject = "Isle: Require updates in order to approve education resource";

			string body = string.Format("<p>{0} has reviewed your education resource. The resource has not been approved. Please refer to the following for reasons/instructions.<br/>{1}</p>", approver.FullName());
			body += "<br/>url:&nbsp;" + urlTitle;
			body += "<br/>From: " + approver.EmailSignature();
			string from = approver.Email;
			EmailManager.SendEmail(toEmail, from, subject, body, approver.Email, bccEmail);


			return isValid;
		}

		public static bool IsUserOrgApprover(ContentItem entity, int checkUserId)
		{
			return IsUserOrgApprover(entity.OrgId, entity.CreatedById, checkUserId);
		}


		public static bool IsUserOrgApprover(int contentOrgId, int contentCreatedById, int checkUserId)
		{
			string statusMessage = "";
			if (contentOrgId > 0)
			{
				if (GroupManager.IsUserAnOrgApprover(contentOrgId, checkUserId))
					return true;
				else
					return false;
			}
			AcctManager mgr = new AcctManager();
			ThisUser author = mgr.Get(contentCreatedById);
			//if valid
			Organization org = OrganizationBizService.GetOrganization(author, ref statusMessage);
			if (org != null && org.Id > 0)
			{
				if (GroupManager.IsUserAnOrgApprover(org.Id, checkUserId))
					return true;
			}
			return false;

		}
		#endregion
		#region ====== Retrieval Methods ===============================================
		/// <summary>
		/// Get Content record
		/// </summary>
        /// <param name="pContentId"></param>
		/// <returns></returns>
        public ContentItem Get( int pContentId )
		{

            ContentItem entity = new ContentItem();

            if ( pContentId > 0 )
            {
                entity = myMgr.Get( pContentId );
                FormatResourceContent(entity);
            }
            
            return entity;

		}//
		public void GetContentStandards( ContentItem entity )
		{

			if ( entity != null && entity.Id > 0 )
			{
				entity.ContentStandards = EFManager.Fill_ContentStandards( entity.Id );
				
			}

		}//
		public ContentItem GetContentTags( ContentItem entity )
		{


			if ( entity != null && entity.Id > 0 )
			{
				entity.ContentStandards = EFManager.Fill_ContentStandards( entity.Id );
				
			}

			return entity;

		}//
        public ContentItem GetByRowId( string pRowId )
        {
            if ( pRowId.Length == 36 && pRowId.Substring(0,4) != "0000")
            {
                return myMgr.GetByRowId( pRowId );
            }
            else
                return new ContentItem();
            

        }//

        /// <summary>
        /// Get Content record using resourceId
        /// The implied context is for use on the detail page or to support the resource
		/// TODO - should we arbitrarily check and include a Content.Partner object???
        /// </summary>
        /// <param name="pContentId"></param>
        /// <param name="user"></param>
        /// <param name="canViewItem">Should default to true, if a related to a document, a privilege check will be done</param>
        /// <returns></returns>
        public ContentItem GetForResourceDetail(int resourceId, Patron user, ref bool canViewItem )
        {

            ContentItem entity = new ContentItem();
            string statusMessage = "";
            string extraMessage = "";
            string formattedUrl = "";
            canViewItem = true;

            if (resourceId > 0)
            {
                entity = EFManager.Content_GetByResourceId(resourceId);

                //if found, what else would we want
                if (entity != null && entity.Id > 0)
                {
                    if (entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID)
                    {
                        canViewItem = CanViewDocument( entity, user, ref statusMessage, ref formattedUrl );
                        if ( canViewItem == false )
                        {
                            //for consistancy, just use formattedUrl always - except can't assume the H level.
                            //MP- in this case, let caller do the formatting. It will have access to ResourceFriendlyUrl and DocumentUrl
                            //in this case, the caller (detail page), needs to handle
                            extraMessage = "<p class='restrictedMsg'>" + statusMessage + "</p>";
                        }
                        //method first checks if part of a curriculum, then formats message with link to item under the curriculum
                        //for docs, should link to parent node!
                        if ( FormatCurriculumMessage( entity, extraMessage ) == false )
                        {
							//HACK WARNING - using standAloneContent to indicate not part of curriculum, so detail page can show actual file url
                            string message = " <div id='compDescription' class='standAloneContent'>This resource has a related page:<div style='margin-left:20px;'><br/><a href='{0}'>{1}</a></div></div>";
                            //
                            string content = string.Format( message, FormatContentFriendlyUrl( entity ), entity.Title );
                            content += extraMessage;

                            entity.Message = string.Format( "<div class='isleBox'><h2 class='isleBox_H2'>Resource Note</h2>{0}</div>", content );
                        }

					}
					
                    else if (entity.TypeId == ContentItem.CURRICULUM_CONTENT_ID)
                    {
                        //could get all standards, but would take a lot of time - unless cached
                        string content = "<div id='compDescription'>Note: Only standards directly aligned to the curriculum are displayed here. The curriculum resource page will display all standards that have been aligned to any of the curriculum components.</div>";
                        entity.Message = string.Format( "<div class='isleBox'><h2 class='isleBox_H2'>Curriculum Information</h2>{0}</div>", content );
                    }
					else if (entity.IsHierarchyType 
						|| entity.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID)
                    {
                        FormatCurriculumMessage( entity, extraMessage );
                    }

                }
            }

            return entity;

        }//

        /// <summary>
        /// Check if content item is part of a curriculum.
        /// If it is, format a link to curriculum page
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="extraMessage"></param>
        /// <returns></returns>
        private bool FormatCurriculumMessage( ContentItem entity, string extraMessage )
        {
            bool foundTopNode = false;
            
            string message = "<div id='compDescription'>This resource is a component of:<div style='margin-left:20px;'><br/><a href='{0}'>{1}</a><br/>{2}</div></div>";

            ContentItem topNode = EFManager.GetTopNode( entity );
            if ( topNode == null || topNode.Id == 0 )
                return false;

            foundTopNode = true;
            string noteBody = "";   

            //if top code is a curriculum
            if (topNode.TypeId == ContentItem.CURRICULUM_CONTENT_ID)
            {
                string content = string.Format( message, FormatCurriculumFriendlyUrl( topNode, entity ), topNode.Title, noteBody );
                content += extraMessage;
                entity.Message = string.Format( "<div class='isleBox'><h2 class='isleBox_H2'>Curriculum Component</h2>{0}</div>", content );
            }
            else if ( topNode.IsHierarchyType )
            {
                //not sure if non-curriculum hierarchy will use curriculum - probably could????
                //string content = string.Format( message, FormatContentFriendlyUrl( topNode ), topNode.Title, noteBody );
                string content = string.Format( message, FormatCurriculumFriendlyUrl( topNode, entity ), topNode.Title, noteBody );
                content += extraMessage;
                entity.Message = string.Format( "<div class='isleBox'><h2 class='isleBox_H2'>IOER Content</h2>{0}</div>", content );
            }
            else if ( topNode.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
            {
                //??? not sure why there would be document - maybe a future related docs?
                string content = string.Format( message, FormatContentFriendlyUrl( topNode ), topNode.Title, noteBody );
                content += extraMessage;
                entity.Message = string.Format( "<div class='isleBox'><h2 class='isleBox_H2'>IOER Content</h2>{0}</div>", content );
            }
       

            return foundTopNode;
        }

        /// <summary>
        /// retrieve list of org/district templates
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public DataSet SelectOrgTemplates( int orgId )
        {
            return myMgr.SelectOrgTemplates( orgId );
        }

		#endregion

		#region === content search ====
		/// <summary>
		/// Search for Content related data using passed parameters
		/// - uses custom paging
		/// - only requested range of rows will be returned
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>


		public DataSet SearchOLD( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
			//TODO - create a List<> version
			return myMgr.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
		}
        #endregion

        #region === hierarchical content ====
        public static bool IsUsingContentStandards()
        {
            bool usingContentStandards = ServiceHelper.GetAppKeyValue( "usingContentStandards", false );

            return usingContentStandards;
        }
        public ContentItem CreateHierarchy( int contentId, int modules, int units, int lessons, int activities )
        {
            return  new EFManager().CreateHierarchy( contentId, modules, units, lessons, activities );
        }

        public int ContentConnectorAdd( int parentId, int childId, int createdById )
        {
            return new EFManager().ContentConnectorAdd( parentId, childId, createdById );
        }
        public bool ContentConnectorDelete( int id, ref string statusMessage )
        {
            return new EFManager().ContentConnector_Delete( id, ref statusMessage );
        }

        public bool ContentConnectorDelete( int parentId, int childId, ref string statusMessage )
        {
            return myEfManager.ContentConnector_Delete( parentId, childId, ref statusMessage );
 }

        protected ContentNode GetCurriculumOutline( int pContentId, bool publishedOnly )
        {
            return GetCurriculumOutline( pContentId, publishedOnly, true );
        }

        protected ContentNode GetCurriculumOutline( int pContentId, bool publishedOnly, bool allowCaching )
        {
            ContentNode entity = new ContentNode();
            try
            {
                if ( pContentId > 0 )
                {
                    LoggingHelper.DoTrace( 6, string.Format("###### GetCurriculumOutline started for ", pContentId) );
                    entity = EFManager.Content_GetHierarchyOutline( pContentId, publishedOnly, allowCaching );
                    LoggingHelper.DoTrace( 6, string.Format( "###### GetCurriculumOutline end for ", pContentId ) );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetCurriculumOutline()" );
            }

            return entity;
        }//

        /// <summary>
        /// Get curriculum outline (shallow hierarchy - typically for author view)
        /// </summary>
        /// <param name="pContentId"></param>
        /// <returns></returns>
        public ContentItem GetCurriculumOutlineOLD( int pContentId, bool publishedOnly )
        {
            ContentItem entity = new ContentItem();
            try
            {
                if ( pContentId > 0 )
                {
                    entity = EFManager.Content_GetHierarchyOutlineFAT( pContentId, 
                                    publishedOnly );
                    //should have been done, just in case, check
                    if ( entity.OrgId > 0 && ( entity.Organization == null || entity.Organization == "" ) )
                    {
                        Organization org = Isle.BizServices.OrganizationBizService.EFGet( entity.OrgId );
                        entity.Organization = org.Name;

                        //don't need parentOrgId in this context (???)
                        if ( org.ParentId > 0 )
                        {
                            Organization parentOrg = Isle.BizServices.OrganizationBizService.EFGet( org.ParentId );
                            entity.ParentOrganization = parentOrg.Name;
                        }
                    }

                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetCurriculum()" );
            }

            return entity;
        }//

        /// <summary>
        /// Check if the passed node is part of a hierarchy
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool IsNodePartOfCurriculum( ContentItem node )
        {
            return EFManager.IsNodePartOfHierarchy( node );
        }

        /// <summary>
        /// Get top nodeId for passes node. This is often the curriculum but could be any hierarchy
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetTopIdForHierarchy( ContentItem node )
        {
            ContentItem entity = EFManager.GetTopNode(node);

            return entity.Id;
        }
        public ContentItem GetTopNodeForHierarchy(ContentItem node)
        {
            ContentItem entity = EFManager.GetTopNode(node);

            return entity;
        }

        /// <summary>
        /// Get node detail - assume guest user
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <returns></returns>
        //public ContentItem GetCurriculumNode( int pNodeId )
        //{
        //    ThisUser user = new ThisUser();

        //    return GetCurriculumNode( pNodeId, user, true, true );
        //}//

        /// <summary>
        /// Get node detail
        /// - retrieve user info
        /// - handle user privileges
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        //public ContentItem GetCurriculumNode( int pNodeId, int userId )
        //{

        //    ThisUser user = new ThisUser();
        //    if ( userId > 0 )
        //    {
        //        user = AccountServices.GetUser( userId );
        //    }

        //    return GetCurriculumNode( pNodeId, user, true, true );
        //}//

        /// <summary>
        /// Get node detail 
        /// - currently oriented to lession level: just returns doc children, not additional hierarchy nodes
        /// - then handle user privileges
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="user"></param>
        /// <param name="doCompleteFill"></param>
        /// <returns></returns>
        protected ContentItem GetCurriculumNode( NodeRequest request, ThisUser user )
        {
            ContentItem entity = new ContentItem();
            string formattedUrl = "";
            string statusMessage = "";
            string noViewDocLinkTemplate = "<a href=\"#\" >{0}</a><br/>{1}";
            //string docLinkTemplate = "<a href=\"{0}\" target=\"_blank\">{1}</a>";
            try
            {
                if ( request.ContentId > 0 )
                {
                    entity = EFManager.Content_GetHierarchyNode( request );
                    FormatResourceContent( entity );
                    if ( entity == null )
                        return entity;

                    if ( entity.HasChildItems )
                    {
                        //check each child CI and determine if user can view
                        foreach ( ContentItem item in entity.ChildItems )
                        {
                            //a node will only display docs, not any child nodes, so ignore if not typeId = 40
                            if ( item.TypeId == ContentItem.DOCUMENT_CONTENT_ID 
                              || item.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID )
                            {
                                FormatResourceContent( item );

                                bool canView = CanViewDocument( item, user, ref statusMessage, ref formattedUrl );
                                if ( canView )
                                {
                                    //for consistancy, just use formattedUrl always - except can't assume the H level.
                                    //MP- in this case, let caller do the formatting. It will have access to ResourceFriendlyUrl and DocumentUrl
                                    //item.ResourceFriendlyUrl = formattedUrl;   // string.Format( docLinkTemplate, entity.DocumentUrl, entity.Title );
                                    //or just set the doc url
                                    // item.DocumentUrl = formattedUrl; 
                                }
                                else
                                {
                                    if ( entity.AutoPreviewUrl.ToLower().Equals( item.DocumentUrl.ToLower() ) )
                                        entity.AutoPreviewUrl = "";

                                    //item.ResourceFriendlyUrl = string.Format( noViewDocLinkTemplate, entity.Title, statusMessage );
                                    item.DocumentUrl = "#";
                                    //the statusMessage should have been added to DocumentPrivacyMessage
                                    //item.Title += "<br/>" + statusMessage;
                                }
                            }
                        }
                    }
                    else
                    {
                        if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID
                          || entity.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID )
                        {
                            bool canView = CanViewDocument( entity, user, ref statusMessage, ref formattedUrl );
                            if ( canView == false )
                            {
                                entity.DocumentUrl = "#";
                            }
                        }
                    }
                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".GetCurriculumNode()" );
            }

            return entity;

        }//

        public bool RenumberNodeChildren( int pNodeId )
        {
            return EFManager.Content_RenumberHierarchy( pNodeId );
        }//

        public bool CloneNodeTags( ThisUser user, int fromNodeId, ContentItem toNode, ref string statusMessage )
        {
            ContentItem fromNode = Get(fromNodeId);

            return CloneNodeTags( user, fromNode, toNode, ref statusMessage );
        }//

        public bool CloneNodeTags( ThisUser user, ContentItem fromNode, ContentItem toNode, ref string statusMessage )
        {
            bool isValid = false;

            if ( fromNode.ResourceIntId == 0 )
            {
                statusMessage = "Error: the base node doesn't have a resourceId - so will not have tags";
                return false;
            }

            if ( toNode.ResourceIntId > 0 )
            {
                //not attempting a merge yet, just new
                statusMessage = "Error: the target node already has a resourceId - not allowing a merge or appending of tags";
                return false;
            }
            string sortTitle = "";
            int versionID = 0;
            int intID= 0;

            Resource res = ResourceBizService.Resource_FillSummary( fromNode.ResourceIntId );
            if ( res != null && res.Id > 0 )
            {
                ResourceReplace( res, user, toNode );
                //re: publish for org, should be same as source. But don't have explicit question
                PublishingServices.PublishToDatabase( res
                    ,user.OrgId
                        , ref isValid
                        , ref statusMessage
                        , ref versionID
                        , ref intID
                        , ref sortTitle );

                if ( intID > 0 )
                {
                    toNode.ResourceIntId = intID;
                    //toNode.ResourceVersionId = versionID;
                    Update( toNode );
                }
            }
            return isValid;

        }//

        public void ResourceReplace( Resource resource, Patron user, ContentItem toNode )
        {
            resource.Id = 0;
            resource.IsActive = true;
            resource.CreatedById = user.Id;
            resource.Created = DateTime.Now;
            resource.LastUpdated = DateTime.Now;

            //URL
            resource.ResourceUrl = FormatContentFriendlyUrl(toNode);
            resource.Version.ResourceUrl = resource.ResourceUrl;

            //Version Data
            resource.Version.Id = 0;
            resource.Version.ResourceIntId = 0;
            resource.Version.Title = toNode.Title;
            resource.Version.Description = toNode.Summary;
            resource.Version.LRDocId = "";
            resource.Version.IsActive = true;

            resource.Version.Submitter = user.FullName();
            resource.Version.Creator = user.FullName();
            resource.Version.Created = DateTime.Now;
            resource.Version.CreatedById = user.Id;
            resource.Version.Imported = DateTime.Now;
            resource.Version.Modified = DateTime.Now;
            resource.Version.SortTitle = ResourceBizService.FormatFriendlyTitle( toNode.Title );
            //Keywords
            foreach ( ResourceChildItem k in resource.Keyword )
            {
                k.ResourceIntId = 0;
                k.CreatedById = user.Id;
            }

            foreach ( ResourceChildItem s in resource.SubjectMap )
            {
                s.ResourceIntId = 0;
                s.CreatedById = user.Id;
            }

            foreach ( ResourceChildItem g in resource.Gradelevel )
            {
                g.ResourceIntId = 0;
                g.CreatedById = user.Id;
            }
            foreach ( ResourceChildItem c in resource.ClusterMap )
            {
                c.ResourceIntId = 0;
                c.CreatedById = user.Id;
            }
            foreach ( ResourceChildItem l in resource.Language )
            {
                l.ResourceIntId = 0;
                l.CreatedById = user.Id;
            }
           
        } //

        public Resource GetNodeTags( int resourceId )
        {
            return ResourceBizService.Resource_FillSummary( resourceId );
        }//

        /// <summary>
        /// Get node detail for download
        /// - currently oriented to lession level: just returns doc children, not additional hierarchy nodes
        /// - then handle user privileges
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="user"></param>
        /// <param name="doCompleteFill">False - no standards, org info</param>
        /// <returns></returns>
    //    [Obsolete]
    //    public ContentItem DownloadCurriculumNode( int pNodeId, ThisUser user, bool doCompleteFill )
    //    {
    //        NodeRequest request = new NodeRequest();
    //        request.AllowCaching = true;
    //        request.ContentId = pNodeId;
    //        //may need more granularity, don't want everything for view
    //        request.DoCompleteFill = false;  //?????
    //        request.IsEditView = false;

    //        ContentItem entity = new ContentServices().GetCurriculumNode( request, user );
    //        if ( entity.TypeId == ContentItem.MODULE_CONTENT_ID
    //          || entity.TypeId == ContentItem.UNIT_CONTENT_ID )
    //        {
    //            //fill children
    //            //if ( entity.HasChildItems )
    //            //{
    //            //    foreach ( ContentItem item in entity.ChildItems )
    //            //    {

    //            //    }

    //            //}

    //        }

    //        return entity;
    //}//
        public ContentItem DownloadCurriculumNode2( int pNodeId, ThisUser user, bool doCompleteFill )
        {
            return DownloadCurriculumNode2( pNodeId, user, true, doCompleteFill );
        }//
        
        /// <summary>
        /// Get node detail for download
        /// - then handle user privileges
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="user"></param>
        /// <param name="includingChildren"></param>
        /// <param name="doCompleteFill"></param>
        /// <returns></returns>
        public ContentItem DownloadCurriculumNode2( int pNodeId, ThisUser user, bool includingChildren, bool doCompleteFill )
        {
            ContentItem entity = new ContentItem();
            string formattedUrl = "";
            string statusMessage = "";

            try
            {
                if ( pNodeId > 0 )
                {
                    entity = EFManager.GetHierarchyForDownload( pNodeId, includingChildren, doCompleteFill );
                    if ( entity == null || entity.Id == 0 )
                        return entity;

                    FormatResourceContent( entity );

                    if ( entity.HasChildItems || entity.HasNodeResources )
                    {
                        Download_ManageNodeChildren( entity, user );
                    }
                    else
                    {
                        if ( entity.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                        {
                            bool canView = CanViewDocument( entity, user, ref statusMessage, ref formattedUrl );
                            if ( canView == false )
                            {
                                entity.DocumentUrl = "#";
                            }
                        }
                    }
                }

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".DownloadCurriculumNode2()" );
            }

            return entity;

        }//

        public void Download_ManageNodeChildren( ContentItem entity, ThisUser user )
        {
            string statusMessage = "";
            string formattedUrl = "";

            //check each child CI and determine if user can view
            //TODO - handling url resources
            foreach ( ContentItem item in entity.ChildItems )
            {
                //a node will only display docs, not any child nodes, so ignore if not typeId = 40
                if ( item.TypeId == ContentItem.DOCUMENT_CONTENT_ID )
                {
                    FormatResourceContent( item );

                    bool canView = CanViewDocument( item, user, ref statusMessage, ref formattedUrl );
                    if ( canView == false)
                    {
                        item.DocumentUrl = "#";
                        //the statusMessage should have been added to DocumentPrivacyMessage
                    }
                }
                else if ( item.TypeId == ContentItem.EXTERNAL_URL_CONTENT_ID )
                {
                        //????????????? - what to do for urls??
                        entity.ResourceFriendlyTitle = ResourceBizService.FormatFriendlyTitle( item.Title );
                        //actual url should be ok

                }         
                else
                {
                    //handle child nodes
                    if ( entity.HasChildItems )
                    {
                        foreach ( ContentItem child in entity.ChildItems )
                        {
                            Download_ManageNodeChildren( child, user );
                        }
                    }
                }
            }
        }//
      
        #endregion
        #region --- helper methods
        /// <summary>
        /// Format content friendly url
        /// </summary>
        /// <param name="entity"></param>
        public static string FormatContentFriendlyUrl( ContentItem entity )
        {
            if ( entity == null || entity.Id == 0 )
                return "";

            string friendlyTitle = ResourceBizService.FormatFriendlyTitle( entity.Title );
            var siteRoot = UtilityManager.GetAppKeyValue( "siteRoot", "http://ioer.ilsharedlearning.org" );

            if (entity.TypeId >= ContentItem.CURRICULUM_CONTENT_ID)
                return UtilityManager.FormatAbsoluteUrl( siteRoot + string.Format( "/learningList/{0}/{1}", entity.Id.ToString(), friendlyTitle ), false );
            else 
                return UtilityManager.FormatAbsoluteUrl( siteRoot + string.Format( "/Content/{0}/{1}", entity.Id.ToString(), friendlyTitle ), false );
        }
        /// <summary>
        /// Format friendly url for a curriculum item
        /// </summary>
        /// <param name="entity"></param>
        public static string FormatCurriculumFriendlyUrl( ContentItem topEntity, ContentItem childItem )
        {
            if ( topEntity == null || topEntity.Id == 0 )
                return "";

            string friendlyTitle = ResourceBizService.FormatFriendlyTitle( topEntity.Title );
            var siteRoot = UtilityManager.GetAppKeyValue( "siteRoot", "http://ioer.ilsharedlearning.org" );
            int childId = childItem.Id;
            //if a doc, link to parent node
            if ( childItem.IsDocumentType || childItem.IsReferenceUrlType )
            {
                if ( childItem.ParentId > 0 )
                    childId = childItem.ParentId;
                else
                {
                    //don't think that can link directly to a document?
                    //so use top
                    childId = topEntity.Id;
                }
            }
            //chg
            string template1 = "/curriculum/{0}/{1}";
            string template2 = "/curriculum/{0}/{1}/{2}";

            return UtilityManager.FormatAbsoluteUrl( siteRoot + string.Format( template1, childId, friendlyTitle ), false );
            //return UtilityManager.FormatAbsoluteUrl( siteRoot + string.Format( "/curriculum/{0}/{1}/{2}", topEntity.Id, childId, friendlyTitle ), false );
        }

        /// <summary>
        /// Format resource related data (friendly name and url initially)
        /// </summary>
        /// <param name="entity"></param>
        public void FormatResourceContent( ContentItem entity )
        {
            if ( entity == null || entity.Id == 0 || entity.HasResourceId() == false )
                return;

            entity.ResourceFriendlyTitle = ResourceBizService.FormatFriendlyTitle( entity.Title );
            if ( entity.ResourceIntId > 0 )
                entity.ResourceFriendlyUrl = ResourceBizService.FormatFriendlyResourceUrlByResId( entity.Title, entity.ResourceIntId );
            //else
            //    entity.ResourceFriendlyUrl = ResourceBizService.FormatFriendlyResourceUrlByRvId( entity.Title, entity.ResourceVersionId );
            entity.ResourceThumbnailImageUrl = ResourceBizService.GetResourceThumbnailImageUrl( entity.ResourceFriendlyUrl, entity.ResourceIntId );
        }

        #endregion
        
        #region ====== ContentSupplement Section ===============================================

        /// <summary>
        /// Add an Content record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentSupplementCreate( ContentSupplement entity, ref string statusMessage )
        {
            return myMgr.ContentSupplementCreate( entity, ref statusMessage );
        }

        public string ContentSupplementUpdate( ContentSupplement entity )
		{
            return myMgr.ContentSupplementUpdate( entity );

		}//

        public bool ContentSupplementDelete( int id, ref string statusMessage )
        {
            return myMgr.ContentSupplementDelete( id, ref statusMessage );
        }
        public ContentSupplement ContentSupplementGet( int id )
        {
            return myMgr.ContentSupplementGet( id );
        }

        public ContentSupplement ContentSupplementGetByRowId( string id )
        {
            return myMgr.ContentSupplementGetByRowId( id );
        }
        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet ContentSupplementsSelect( int parentId )
        {
            return myMgr.ContentSupplementsSelect( parentId );
        }
        public List<ContentSupplement> ContentSupplementsSelectList( int parentId )
        {
            return myMgr.ContentSupplementsSelectList( parentId );
        }
        #endregion

       

        #region ====== DocumentVersion Section ===============================================

        /// <summary>
        /// Add a DocumentVersion record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string DocumentVersionCreate( DocumentVersion entity, ref string statusMessage )
        {
            return DocManager.Create( entity, ref statusMessage );
        }

        public string DocumentVersionUpdate( DocumentVersion entity )
        {
            return new DocManager().Update( entity );

        }//
        /// <summary>
        /// Set the record status to published
        /// Used where the doc version is created before the parent (actually the norm). The status defaults to initial on create.
        /// If something prevented the parent from being saved the record would then be orphaned - depending on how the current process handles the condition.
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public string DocumentVersion_SetToPublished( string rowId )
        {
            return DocManager.SetToPublished( rowId );
        }
        public DocumentVersion DocumentVersionGet( string rowId )
        {
            return DocManager.Get( rowId );
        }

        public DocumentVersion DocumentVersionGet( Guid rowId )
        {
            return DocManager.Get( rowId );
        }

        /// <summary>
        /// Determine if user can view the ContentItem document
        /// If not, the reason is returned in statusMessage.
        /// If can, return a formatted url
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        /// <param name="statusMessage"></param>
        /// <param name="formattedUrl"></param>
        /// <returns></returns>
        public bool CanViewDocument( ContentItem entity, ThisUser user, ref string statusMessage, ref string formattedUrl )
        {
            string docLinkTemplate = "<a href=\"{0}\" target=\"_blank\">{1}</a>";
            formattedUrl = "";
            statusMessage = "";
            bool canViewDoc = false;

            if ( user == null )
                user = new ThisUser();

            //hmmm, assuming doc has been sync'd already
            if ( entity.DocumentUrl  != null && entity.DocumentUrl.Length > 5 )
            {
				//need org member checks
                bool isOwner = ( user.Id > 0 && user.Id == entity.CreatedById );

                if ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE || isOwner )
                {
                    formattedUrl = string.Format( docLinkTemplate, entity.DocumentUrl, entity.Title );
                    canViewDoc = true;
                }
                else if ( user == null || user.Id == 0 )
                {
                    statusMessage = "Private document, viewing is not allowed for users who are not logged in.";
                }
                else
                {
                    //check if user is same org (direct or via orgMbr)
                    OrganizationMember ombr = OrganizationBizService.OrganizationMember_Get( entity.OrgId, user.Id );

                    if ( entity.PrivilegeTypeId == ContentItem.MY_ORG_PRIVILEGE )
                    {
                        if ( "1 2 4".IndexOf( ombr.OrgMemberTypeId.ToString() ) > -1 )
                            canViewDoc = true;
                        else
                            statusMessage = string.Format( "Private document, viewing is only allowed for staff of this organization: <strong>{0}</strong>.", entity.Organization );

                    }
                    else if ( entity.PrivilegeTypeId == ContentItem.MY_ORG_AND_STUDENTS_PRIVILEGE )
                    {
                        if ( "1 2 3 4".IndexOf( ombr.OrgMemberTypeId.ToString() ) > -1 )
                            canViewDoc = true;
                        else
                            statusMessage = string.Format( "Private document, viewing is only allowed for staff and students of this organization: <strong>{0}</strong>.", entity.Organization );
                    }

                    else if ( entity.PrivilegeTypeId == ContentItem.ISLE_ORG_MEMBER_PRIVILEGE )
                    {
                        if ( OrganizationBizService.IsStaffMemberOfAnIsleOrg( user.Id ) )
                            canViewDoc = true;
                        else
                            statusMessage = "Private document, viewing is only allowed for staff of an ISLE affiliated organization.";
                    }


                    if ( canViewDoc == false )
                    {
                        //statusMessage = string.Format( "Private document, viewing is only allowed for members of this organization: <strong>{0}</strong>.", entity.Organization );
                    }
                    else
                    {
                        formattedUrl = string.Format( docLinkTemplate, entity.DocumentUrl, entity.Title );
                        canViewDoc = true;
                    }
                }
            }
            else
            {
                statusMessage = "Sorry issue encountered locating the related document.";
				DoTrace(4, string.Format("@@@@ CanViewDocument - issue encountered locating the related document. ContentId: {0}, type: {1}, rId: {2}", entity.Id, entity.TypeId, entity.ResourceIntId));
            }

            entity.CanViewDocument = canViewDoc;
            entity.DocumentPrivacyMessage = statusMessage;

            return canViewDoc;
        }
        #endregion


        #region ====== ContentReference Section ===============================================

        /// <summary>
        /// Add a Content record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentReferenceCreate( ContentReference entity, ref string statusMessage )
        {
            return myMgr.ContentReferenceCreate( entity, ref statusMessage );
        }

        public string ContentReferenceUpdate( ContentReference entity )
        {
           return myMgr.ContentReferenceUpdate( entity );
        }//
        public bool ContentReferenceDelete( int id, ref string statusMessage )
        {
            return myMgr.ContentReferenceDelete( id, ref statusMessage );
        }
        public ContentReference ContentReferenceGet( int id )
        {
            return myMgr.ContentReferenceGet( id );
        }//
        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet ContentReferencesSelect( int parentId )
        {
            return myMgr.ContentReferencesSelect( parentId );
        }
        public List<ContentReference> ContentReferencesSelectList( int parentId )
        {
            return myMgr.ContentReferencesSelectList( parentId );
        }

        #endregion


        #region ====== Content.Partner Section ===============================================
        public int ContentPartner_Add( ContentPartner entity, ref string statusMessage )
        {
            return new EFManager().ContentPartner_Add( entity, ref statusMessage );
        }
        /// <summary>
        /// the creator of an applicable content item will be the administrator by default
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="createdbyId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Content_AddCreatorPartner(int contentId, int createdbyId, ref string statusMessage)
        {
            ContentPartner entity = new ContentPartner();
            int newId = 0;

            entity.ContentId = contentId;
            entity.UserId = createdbyId;
            entity.PartnerTypeId = LibraryMember.LIBRARY_MEMBER_TYPE_ID_ADMIN;
            entity.CreatedById = entity.LastUpdatedById = createdbyId;
            entity.Created = entity.LastUpdated = DateTime.Now;

            newId = ContentPartner_Add(entity, ref statusMessage);

            return newId;
        }

        /// <summary>
        /// Update a content partner
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="userId"></param>
        /// <param name="partnerTypeId"></param>
        /// <param name="updatedById"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool ContentPartner_Update(int contentId, int userId, int partnerTypeId, int updatedById, ref string status)
        {

            ContentPartner entity = ContentServices.ContentPartner_Get(contentId, userId);
            if (entity == null || entity.Id == 0)
            {
                status = "Content partner was not found.";
                return false;
            }

            entity.PartnerTypeId = partnerTypeId;
            entity.LastUpdatedById = updatedById;

            return new EFManager().ContentPartner_Update(entity);
        }

        /// <summary>
        /// /// Update a content partner NOTE: MSUT HAVE AN ID
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool ContentPartner_Update( ContentPartner entity, ref string status )
        {
            if (entity.Id == 0)
            {
                status = "Error: The ContentPartner object must have an Id";
                return false;
            }
            return new EFManager().ContentPartner_Update( entity );
        }

        public bool ContentPartner_Delete(int contentId, int userId, ref string statusMessage)
        {
            ContentPartner entity = ContentPartner_Get(contentId, userId);
            if (entity != null && entity.Id > 0)
                return new EFManager().ContentPartner_Delete(entity.Id, ref statusMessage);
            else
            {
                statusMessage = "The requested partner record was not found.";
                return false;
            }
        }

        public bool ContentPartner_Delete( int id, ref string statusMessage )
        {
            return new EFManager().ContentPartner_Delete( id, ref statusMessage );
        }

        public static bool IsContentPartner(int contentId, int userId)
        {
            ContentPartner entity = ContentPartner_Get(contentId, userId);
            if (entity != null && entity.Id > 0)
                return true;
            else
                return false;
        }

        public static ContentPartner ContentPartner_Get( int contentId, int userId )
        {
            return EFManager.Content_GetContentPartner( contentId, userId);
        }

        public static ContentPartner ContentPartner_Get( int contentId, string userGuid )
        {
            ThisUser user = new AccountServices().GetByRowId( userGuid );
            if ( user == null || user.Id == 0 )
                return new ContentPartner();
            else
                return EFManager.Content_GetContentPartner( contentId, user.Id );
        }

        /// <summary>
        /// get all partners for content
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static List<ContentPartner> ContentPartner_GetAll(int contentId)
        {

            return EFManager.Content_GetContentPartners(contentId);
        }
        public static List<ContentPartner> ContentPartner_GetAll(int contentId, int minimumType)
        {

            return EFManager.Content_GetContentPartners(contentId, minimumType);
        }
        public static List<CodeItem> GetCodes_ContentPartnerType()
        {
            return GetCodes_ContentPartnerType(10);
        }
        public static List<CodeItem> GetCodes_ContentPartnerType(int topTypeId)
        {
            CodeItem ci = new CodeItem();
            //skip zero - pending
            List<EFDAL.Codes_ContentPartnerType> eflist = new EFDAL.IsleContentEntities().Codes_ContentPartnerType
                            .Where(s => s.Id > 0 && s.Id <= topTypeId)
                            .OrderBy(s => s.Id)
                            .ToList();
            List<CodeItem> list = new List<CodeItem>();
            if (eflist.Count > 0)
            {
                foreach (EFDAL.Codes_ContentPartnerType item in eflist)
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.Title;
                    list.Add(ci);
                }
            }
            return list;
        }

        public static CodeItem GetCode_ContentPartnerType(int typeId)
        {
            CodeItem ci = new CodeItem();
            List<EFDAL.Codes_ContentPartnerType> eflist = new EFDAL.IsleContentEntities().Codes_ContentPartnerType
                            .Where(s => s.Id == typeId)
                            .OrderBy(s => s.Id)
                            .ToList();
            
            if (eflist.Count > 0)
            {
                foreach (EFDAL.Codes_ContentPartnerType item in eflist)
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.Title;
                    break;
                }
            }
            return ci;
        }
        #endregion


        #region ====== Content approval related ===========================================


        #endregion 

        #region ====== ContentHistory Section ===========================================

        /// <summary>
        /// Add a ContentHistory record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentHistory_Create( int pContentId, string pAction, string pDescription, int pCreatedById, ref string statusMessage )
        {
            return myMgr.ContentHistory_Create( pContentId, pAction, pDescription, pCreatedById, ref statusMessage );
        }

        public bool ContentHistory_Update( int id, string pDescription, int pCreatedById, ref string statusMessage )
        {
            EFManager mgr = new EFManager();
            return mgr.ContentHistory_Update( id, pDescription, ref statusMessage );
        }

        public DataSet ContentHistory_Select( int pContentId )
        {
            return myMgr.ContentHistory_Select( pContentId );
        }


        public List<CommentDTO> ContentHistory_Select( int pContentId, string pAction )
        {
            EFManager mgr = new EFManager();

            return mgr.ContentHistory_Select( pContentId, pAction );
        }
        #endregion

        #region ====== Content Likes/Comments Section ===========================================

        /// <summary>
        /// Add a Content Comment - done through resource
        /// </summary>
        /// <param name="pContentId"></param>
        /// <param name="pDescription"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Content_AddComment( int pContentId, string comment, int pCreatedById, ref string statusMessage )
        {
            //get content, if has resourceId, check for like
            ContentItem entity = Get( pContentId );
            if ( entity != null && entity.ResourceIntId > 0 )
            {
                return new ResourceManager().Resource_AddComment( entity.ResourceIntId, comment, pCreatedById, ref statusMessage );
            }
            else
            {
                statusMessage = "Error: no related resource was found";
                return 0;
            }
        }

        /// <summary>
        /// Get comments for content item - uses Content.ResourceIntId to return resource comments
        /// </summary>
        /// <param name="pContentId"></param>
        /// <returns></returns>
        public List<ResourceComment> Content_GetComments( int pContentId )
        {

            var list = new List<ResourceComment>();

            //get content, if has resourceId, check for like
            ContentItem entity = Get( pContentId );
            if ( entity != null && entity.ResourceIntId > 0 )
            {
                list = new ResourceManager().Resource_GetComments( entity.ResourceIntId );
            }
            return list;
        }

        /// <summary>
        /// Retrieve like summary for content item
        /// The likes are associated with the related resource record.
        /// For the returned entity (ResourceLikeSummary), check HasRating. If latter is true, user has already rated the item
        /// </summary>
        /// <param name="pContentId"></param>
        /// <param name="pUserId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public ResourceLikeSummary Content_GetLikeSummmary( int pContentId, int pUserId, ref string statusMessage )
        {
            ResourceLikeSummary likes = new ResourceLikeSummary();

            //get content, if has resourceId, check for like
            ContentItem entity = Get( pContentId );
            if ( entity != null && entity.ResourceIntId > 0 )
            {
                likes = ResourceManager.Resource_GetLikeSummmary( entity.ResourceIntId, pUserId, ref statusMessage );
            }

            return likes;
        }

        public int Content_AddLike( int pContentId, int userId, bool isLike)
        {
            //get content, if has resourceId, check for like
            ContentItem entity = Get( pContentId );
            if ( entity != null && entity.ResourceIntId > 0 )
            {
                return new ResourceManager().AddLikeDislike( userId, entity.ResourceIntId, isLike );
            }
            else
                return 0;
        }
        
        #endregion

        #region ====== person following
        public int FollowUser( int followingUserId, int followedByUserId )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();

            EFDAL.Person_Following log = new EFDAL.Person_Following();
            log.Created = System.DateTime.Now;
            log.FollowedByUserId = followedByUserId;
            log.FollowingUserId = followingUserId;

            try
            {
                ctx.Person_Following.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return log.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".FollowUser()" );
                return 0;
            }
        }
        #endregion
        #region ====== Codes ===============================================
        public List<CodeItem> ContentType_ActiveList()
        {
            List<CodeItem> list = EFManager.ContentType_GetActive();
            return list;
        }
        public List<CodeItem> ContentType_TopLevelActiveList()
        {
            List<CodeItem> list = EFManager.ContentType_GetAllowedTopLevel();
            return list;
        }
        public DataSet ContentType_Select()
        {
            return myMgr.ContentType_Select();
        }

        public DataSet ContentStatusCodes_Select()
        {
            return myMgr.ContentStatusCodes_Select();
        }
        public DataSet ContentPrivilegeCodes_Select()
        {
           return myMgr.ContentPrivilegeCodes_Select();
        }
        #endregion

        /// <summary>
        /// execute dynamic sql against the content db
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataSet DoQuery( string sql )
        {
            string conn = DBM.ContentConnectionRO();
            return DBM.DoQuery( sql, conn );
        }

    }
}
