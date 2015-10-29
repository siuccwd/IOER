using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using ILPathways.Business;
using ILPathways.Utilities;
using Isle.BizServices;
using LRWarehouse.Business;

namespace IOER.Controls.Content
{
	public partial class ContentDisplayV2 : BaseUserControl
	{
		const string thisClassName = "ContentDisplayV2";

		ContentServices contentService = new ContentServices();
		AccountServices accountService = new AccountServices();

		public ContentItem Content { get; set; }
		public ContentItem TopLevelNode { get; set; }
		public List<ContentSupplement> Supplements { get; set; }
		public List<ContentReference> References { get; set; }
		public Patron Owner { get; set; }
		public bool HasError { get; set; }
		public bool ContentIsVisible { get; set; }
		public Patron User { get; set; }
		public string ContentPreviewUrl { get; set; }

		protected void Page_Load( object sender, EventArgs e )
		{

			if ( !IsUserAuthenticated() )
			{
				//get the user
				User = ( Patron ) WebUser;
			}

			LoadContent();
			if ( HasError )
			{
				return;
			}

			DetermineContentVisibility();
			if ( ContentIsVisible == false || CanView( Content ) == false )
			{
				notAllowedPanel.Visible = true;
				return;
			}

			detailPanel.Visible = true;
			new ActivityBizServices().ContentHit( Content, User );

			DeterminePreviewUrl();
			DetermineLearningList();
			//basic content will not use a resource thumbanail as it would simply be an image of itself.
			//so go with any existing content
			//Content.ImageUrl = DetermineThumbnail( Content );
			LoadSupplementsAndReferences();
		}

		private void LoadContent()
		{
			var targetID = 0;

			//legacy
			string rid = this.GetRequestKeyValue( "rid", "" );

			//Get content ID

			if ( rid.Trim().Length == 36 )
			{
				//this.Get( rid );
				Content = contentService.GetByRowId( rid );
				VerifyContent();
				//if ( Content == null || Content.Id == 0 )
				//{
				//	throw new NullReferenceException( "Unable to load requested Content" );
				//}
			}
			else
			{
				int.TryParse( Request.Params[ "RouteID" ], out targetID );
				if ( targetID == 0 )
				{
					targetID = FormHelper.GetRouteKeyValue( Page, "RouteID", 0 );
					if ( targetID == 0 )
						targetID = this.GetRequestKeyValue( "cidx", 0 );
				}
				if ( targetID == 0 )
				{
					ShowError( "Invalid Content ID" );
					return;
				}

				//Load content
				try
				{
					Content = contentService.Get( targetID );
					VerifyContent();
					//if ( Content == null || Content.Id == 0 )
					//{
					//	throw new NullReferenceException( "Unable to load Content" );
					//}

					//Owner = accountService.Get( Content.CreatedById );
					//Owner.OrgMemberships = OrganizationBizService.OrganizationMember_GetUserOrgs( Owner.Id );
				}
				catch ( System.Threading.ThreadAbortException tex )
				{
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".LoadContent() - Unexpected error encountered" );
					this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );
					return;
				}
			}

		} //

		private void VerifyContent()
		{
			if ( Content == null || Content.Id == 0 )
			{
				//throw new NullReferenceException( "Unable to load Content" );
				SetConsoleErrorMessage( "Unable to load requested Content" );
			}
			try
			{
				string stay = this.GetRequestKeyValue( "stay", "no" );
				if ( Content.IsHierarchyType
					&& redirecting50ToLearningList.Text.Equals( "yes" )
					&& stay == "no" )
				{
					Response.Redirect( string.Format( learningListUrlTemplate.Text, Content.Id, ResourceBizService.FormatFriendlyTitle( Content.Title ) ), true );
				}

				Owner = accountService.Get( Content.CreatedById );
				Owner.OrgMemberships = OrganizationBizService.OrganizationMember_GetUserOrgs( Owner.Id );

				//hide summary. If no description, use summary
				//but showing in the summary location for now
				if ( !string.IsNullOrWhiteSpace( Content.Description ) )
				{
					Content.Summary = Content.Description;
					Content.Description = "";
				}
				else if ( !string.IsNullOrWhiteSpace( Content.Summary ) )
				{
					//Content.Description = Content.Summary;
					Content.Description = "";
				}

			}
			catch ( System.Threading.ThreadAbortException tex )
			{
			}
			catch ( System.Exception ex )
			{
				//Action??		- display message and close form??	
				LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
				this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

			}

		} //

		private void DetermineContentVisibility()
		{
			//If public, show regardless
			if ( Content.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
			{
				ContentIsVisible = true;
				//need to check if pubished, active
				return;
			}
			//Otherwise, figure it out

			//If the user isn't logged in, and the content isn't public, don't show it
			if ( !IsUserAuthenticated() )
			{
				ContentIsVisible = false;
				return;
			}

			//If the user is the owner or is site admin, show it
			if ( Content.CreatedById == User.Id || accountService.IsUserAdmin( User ) )
			{
				ContentIsVisible = true;
				if ( Content.CreatedById != User.Id )
				{
					SetConsoleInfoMessage( "Admin access granted - you would not normally see this content." );
				}
				return;
			}

			//If the owner belongs to the same org as the user and appropriate permissions are set (MAY not want students seeing teacher resources even if same org), show it
			User.OrgMemberships = OrganizationBizService.OrganizationMember_GetUserOrgs( User.Id );
			var matchingMembership = User.OrgMemberships.Where( m => m.Id == Content.OrgId ).FirstOrDefault() ?? User.OrgMemberships.Where( m => m.Id == Content.ParentOrgId ).FirstOrDefault();
			if ( matchingMembership != null )
			{ //Content and User belong to the same org, but does the user have appropriate access?
				
				//TODO: Finish This

			}

		} //

		private bool CanView( ContentItem entity )
		{
			bool isValid = false;
			if ( IsUserAuthenticated() && WebUser.Id == entity.CreatedById )
				return true;

			//allow approvers
			if ( entity.StatusId > ContentItem.INPROGRESS_STATUS && UserCanApproveThisResource( entity ) )
				return true;

			if ( IsUserAuthenticated() && WebUser.TopAuthorization > 0 && WebUser.TopAuthorization < 5 )
			{
				//notAllowedPanel.Visible = true;
				//SetConsoleInfoMessage( "OVERRIDE - Normally MAY not be allowed" );
				//return true;
			}

			if ( entity.StatusId < ContentItem.PUBLISHED_STATUS )
				return false;

			//TODO - only caveat may be to allow a reviewer to see this view?
			//       they would have to be authenticated with org admin status???
			if ( entity.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE )
				return true;

			//else must be authenticated
			if ( IsUserAuthenticated() == false )
				return false;

			CurrentUser = GetAppUser();
			//must have an orgId, although not for the state
			if ( CurrentUser.OrgId == 0 )
				return false;


			if ( entity.PrivilegeTypeId == ContentItem.MY_ORG_PRIVILEGE
			   && entity.OrgId == CurrentUser.OrgId )
				return true;


			//hmm what for district??
			//==> need to include parent with content, and also get user parent - should persist, or maybe an oppurtunity for cookies???
			//-should have a state level access, or admin check
			//start with having an org - since 

			if ( entity.PrivilegeTypeId == ContentItem.MY_REGION_PRIVILEGE )
			{
				if ( entity.OrgId == CurrentUser.OrgId
				  || entity.OrgId == CurrentUser.ParentOrgId
				  || entity.ParentOrgId == CurrentUser.ParentOrgId
				  || entity.ParentOrgId == CurrentUser.OrgId
				 )
					return true;
			}

			return isValid;
		}//

		private void DeterminePreviewUrl()
		{
			//Determine what URL to put in the iframe
			if ( !string.IsNullOrWhiteSpace( Content.DocumentUrl ) )
			{
				var googleURL = googlePreviewer.Text;
				var officeURL = officePreviewer.Text;
				var baseURL = UtilityManager.GetAppKeyValue( "siteRoot", siteBaseUrl.Text );
				var nativeFileTypes = nativeTypes.Text.Split( ' ' ).ToList();
				var officeFileTypes = officeTypes.Text.Split( ' ' ).ToList();

				//If the browser natively supports it, just show it
				foreach(var item in nativeFileTypes)
				{
					if ( Content.DocumentUrl.IndexOf( item ) > -1 )
					{
						ContentPreviewUrl = Content.DocumentUrl;
						return;
					}
				}

				//If Office, use Microsoft's viewer
				foreach ( var item in officeFileTypes )
				{
					if ( Content.DocumentUrl.IndexOf( item ) > -1 )
					{
						ContentPreviewUrl = officeURL + baseURL + Content.DocumentUrl;
						return;
					}
				}

				//Otherwise, use the google previewer and hope for the best
				ContentPreviewUrl = googleURL + baseURL + Content.DocumentUrl;
			}

		} //

		/// <summary>
		/// should not be used for a learning list?
		/// </summary>
		private void DetermineLearningList()
		{
			if ( contentService.IsNodePartOfCurriculum( Content ) || Content.ParentId > 0 )
			{
				//TopLevelNode = contentService.GetTopNodeForHierarchy( Content ); //Does not work - retrieves new/empty node
				TopLevelNode = new CurriculumServices().GetTheCurriculumNode( Content.Id );
				TopLevelNode.ImageUrl = DetermineThumbnail( TopLevelNode );
			}
			else
			{
				TopLevelNode = null;
			}
		} //

		private string DetermineThumbnail( ContentItem item )
		{
			var candidates = new List<string>() {
				item.ImageUrl,
				item.ResourceImageUrl,
				item.ResourceThumbnailImageUrl
			};

			foreach ( var thing in candidates )
			{
				if ( !string.IsNullOrWhiteSpace( thing ) )
				{
					return thing;
				}
			}

			return "";
		} //

		private void LoadSupplementsAndReferences()
		{
			Supplements = contentService.ContentSupplementsSelectList( Content.Id );
			References = contentService.ContentReferencesSelectList( Content.Id );
		}

		private void ShowError( string message )
		{
			HasError = true;
			contentWrapper.Visible = false;
			//generally need to be careful with arbirarily showing messages - could be exceptions that are not to be shown to user (example abort of a thread on a redirect
			SetConsoleErrorMessage( message );
		} //


		#region approval related

		protected bool UserCanPublishThisResource()
		{

			if ( !IsUserAuthenticated() )
			{
				return false;
			}

			//TODO - need to handle with rowId
			ContentItem entity = new ContentItem();
			//if ( CurrentRecordID > 0 )
			//{
			//	entity = myManager.Get( CurrentRecordID );
			//}
			//else
			//{
			//	entity = Get();
			//}

			return UserCanPublishThisResource( entity );

		}
		protected bool UserCanPublishThisResource( ContentItem entity )
		{

			if ( !IsUserAuthenticated() )
			{
				return false;
			}

			if ( entity.CreatedById == WebUser.Id )
			{
				return true;
			}
			else
			{
				return false;
			}

		}
		protected bool UserCanApproveThisResource()
		{

			if ( !IsUserAuthenticated() )
			{
				return false;
			}

			//TODO - need to handle with rowId
			//???already have record, so why get again?
			ContentItem entity = new ContentItem();
			//if ( CurrentRecordID > 0 )
			//{
			//	entity = myManager.Get( CurrentRecordID );
			//}
			//else
			//{
			//	entity = Get();
			//}

			return UserCanApproveThisResource( entity );

		}

		protected bool UserCanApproveThisResource( ContentItem entity )
		{

			if ( !IsUserAuthenticated() )
			{
				return false;
			}

			CurrentUser = GetAppUser();

			//for now, don't allow author to approve,even if an approver??
			if ( entity.CreatedById == CurrentUser.Id && canAuthorApproveOwnContent.Text == "no" )
			{
				return false;
			}
			/* 
			 * get orgId, and parentOrgId, or specific method
			 * 
			 * 
			 */

			return ContentServices.IsUserOrgApprover( entity, CurrentUser.Id );
		
		}
		#endregion
	}
}