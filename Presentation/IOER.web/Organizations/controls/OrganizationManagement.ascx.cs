using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using LRWarehouse.Business;
using Isle.BizServices;
using MyOrg = ILPathways.Business.Organization;
using ILPathways.Business;
using ILPathways.Common;
using ILPathways.Utilities;
using IOER.Services;
using Isle.DTO;

namespace IOER.Organizations
{
	public partial class OrganizationManagement : BaseUserControl
	{
		OrganizationBizService orgService = new OrganizationBizService();
		public UtilityService utilService = new UtilityService();

		public Patron user = new Patron();
		public MyOrg activeOrganization = new MyOrg();
		public List<OrganizationMember> myOrganizations = new List<OrganizationMember>();
		public List<Organization> allOrganizations = new List<Organization>();
		public List<CodeItem> orgTypes = new List<CodeItem>();
		public List<CodeItem> states = new List<CodeItem>();
		public List<CodeItem> libraryCodes = new List<CodeItem>();
		public List<ILPathways.Business.Library> libraries = new List<ILPathways.Business.Library>();
		public List<ContentSearchResult> lists = new List<ContentSearchResult>();
		public int errors = 0;
		public bool isSiteAdmin = false;
		public int currOrgId = 0;

		protected void Page_Load( object sender, EventArgs e )
		{
			//Load required info
			LoadBasics();

			//If postback, get data from the fields and act according to the mode
			if ( IsPostBack )
			{
				ProcessInput();
			}

			//User management
			LoadUserManager();

			//Libraries
			LoadOrgLibraries();

			//Learning Lists
			LoadOrgLearningLists();
		}

		//Load required data
		private void LoadBasics()
		{
			//Get the user
			if ( !IsUserAuthenticated() )
			{
				Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
				return;
			}
			else
			{
				user = (Patron) WebUser;
				isSiteAdmin = new AccountServices().IsUserAdmin( user );
			}

			this.btnDeleteOrg.Visible = false;

			//Get the user's organizations
			myOrganizations = OrganizationBizService.OrganizationMember_GetUserOrgs( user.Id );
			//If admin, get the rest of the orgs
			if ( isSiteAdmin )
			{
				int total = 0;
				allOrganizations = new OrganizationBizService().Organization_Search( "", "", 0, 9999, ref total );
				//Filter out the ones we don't need
				foreach ( var item in myOrganizations )
				{
					var targetOrg = allOrganizations.Where( m => m.Id == item.OrgId ).FirstOrDefault();
					if ( targetOrg != null )
					{
						allOrganizations.Remove( targetOrg );
					}
				}
			}

			var targetOrgID = 0;
			string action = FormHelper.GetRequestKeyValue( "action", "" );
			//check for create request
			
			//var currId = Request.Form[ "CurrentOrgId" ];
			//Int32.TryParse( currId, out currOrgId );

			//Get the organization
			//if ( currOrgId < 0 )
			//{
			//	targetOrgID = currOrgId;
			//	this.txtCurrentOrgId.Text = "-1";
			//}
			//else
			//{	}
				int.TryParse( Request.Params[ "organizationID" ], out targetOrgID );
				if ( targetOrgID == 0 )
					targetOrgID = FormHelper.GetRouteKeyValue( Page, "orgId", 0 );

				if ( targetOrgID == 0 )
					Int32.TryParse( this.txtCurrentOrgId.Text, out targetOrgID );

				if ( targetOrgID > 0 )
				{
					OrganizationMember mbr = myOrganizations.Where( m => m.OrgId == targetOrgID ).FirstOrDefault();

					if ( mbr != null || isSiteAdmin )
					{
						activeOrganization = OrganizationBizService.EFGet( targetOrgID );
						if ( targetOrgID > 0 && ( isSiteAdmin || mbr.OrgMemberTypeId == 1 || mbr.HasAdministratorRole() ) )
						{
							this.btnDeleteOrg.Visible = true;
						}
					}
				}
				else
				{
					if ( action == "new")
						targetOrgID = activeOrganization.Id = -1;
				}

				this.txtCurrentOrgId.Text = targetOrgID.ToString();
		

			//Get organization types
			orgTypes = OrganizationBizService.OrgType_Select();

			//Get states
			states = OrganizationBizService.States_Select();
		}

		//Handle postback'd input depending on mode
		//Might not be necessary
		private void ProcessInput()
		{
			var mode = Request.Form[ "Mode" ];
			if ( string.IsNullOrWhiteSpace( mode ) )
			{
				return;
			}
			switch ( mode )
			{
				case "properties":
					UpdateProperties();
					break;
				case "newOrg":
					break;
				case "delOrg":
					break;

				case "users":

					break;
				case "libraries":
					
					break;
				case "learninglists":

					break;
				default:
					return;

			}
			//
		}

		//Load user manager
		private void LoadUserManager()
		{
			var manager = userManagerContainer.FindControl( "userManager" ) as Controls.ManageUsers;
			manager.ObjectId = activeOrganization.Id;
			manager.ObjectTitle = activeOrganization.Name;
			manager.ObjectTypeTitle = "Organization";
			manager.ObjectType = "organization";
		}

		//Load org libraries
		private void LoadOrgLibraries()
		{
			libraryCodes = LibraryBizService.GetCodes_LibraryAccessLevel();
			if ( activeOrganization.Id > 0 )
			{
				libraries = OrganizationBizService.Organization_GetLibraries( activeOrganization.Id );
			}
		}

		//Load org learning lists
		private void LoadOrgLearningLists()
		{
			if ( activeOrganization.Id > 0 )
			{
				string message = "";
				//Do search
				lists = new ContentSearchServices().GetLearningListsForOrganization( activeOrganization.Id, CurrentUser, ref message );
			}
		}

		protected void btnNewOrg_Click( object sender, EventArgs e )
		{
			activeOrganization = new MyOrg();
			activeOrganization.Id = -1;
			this.btnDeleteOrg.Visible = false;
		}
		//Update org properties
		private void UpdateProperties()
		{
			var valid = true;
			int orgId = 0;
			string statusMessage = "";
			try
			{
				Int32.TryParse( this.txtCurrentOrgId.Text, out orgId);
				
				//Get form data
				activeOrganization.IsActive = Request.Form[ "IsActive" ] == "on";
				activeOrganization.Name = CheckTextField( Request.Form[ "Name" ] ?? "", 3, ref valid, "Name" );
				activeOrganization.OrgTypeId = int.Parse( Request.Form[ "OrgTypeId" ] ?? "0" );
				if ( isSiteAdmin )
					activeOrganization.IsIsleMember = Request.Form[ "IsIsleMember" ] == "on";

				if ( ValidateEmailDomain() == false)
					errors++;
				activeOrganization.EmailDomain = CheckTextField( Request.Form[ "EmailDomain" ] ?? "", 0, ref valid, "Email Domain" );
				activeOrganization.WebSite = CheckUrlField( Request.Form[ "WebSite" ] ?? "", ref valid, "Website" );
				activeOrganization.MainPhone = CheckTextField( Request.Form[ "MainPhone" ] ?? "", 0, ref valid, "Main Phone Number" );
				activeOrganization.MainExtension = CheckTextField( Request.Form[ "MainExtension" ] ?? "", 0, ref valid, "Main Phone Extension" );
				activeOrganization.Fax = CheckTextField( Request.Form[ "Fax" ] ?? "", 0, ref valid, "Fax" );
				activeOrganization.Address1 = CheckTextField( Request.Form[ "Address1" ] ?? "", 5, ref valid, "Address Line 1" );
				activeOrganization.Address2 = CheckTextField( Request.Form[ "Address2" ] ?? "", 0, ref valid, "Address Line 2" );
				activeOrganization.City = CheckTextField( Request.Form[ "City" ] ?? "", 3, ref valid, "City" );
				activeOrganization.State = CheckTextField( Request.Form[ "State" ] ?? "", 2, ref valid, "State" );
				activeOrganization.Zipcode = CheckTextField( Request.Form[ "Zipcode" ] ?? "", 5, ref valid, "Zip Code" );
				activeOrganization.ZipCode4 = CheckTextField( Request.Form[ "ZipCode4" ] ?? "", 0, ref valid, "Zip Code Extension" );

				//Set additional values
				activeOrganization.LastUpdated = DateTime.Now;
				activeOrganization.LastUpdatedById = user.Id;
			}
			catch ( Exception ex )
			{
				SetConsoleErrorMessage( "Sorry, there was a problem with the data you entered. Please try again." );
				return;
			}

			//If there are any errors to report, don't continue
			if ( errors > 0 )
			{
				//Set active org to be input org so user input is preserved
				SetConsoleErrorMessage( "Your changes have <u>not</u> been saved." );

				//Status messages are already set
				return;
			}

			if ( orgId <= 0 )
			{
				activeOrganization.CreatedById = WebUser.Id;
				if ( isSiteAdmin == false )
					activeOrganization.IsActive = false;

				int id = orgService.Organization_Create( activeOrganization, ref statusMessage );
				//need to add current user as administrator
				//or do this after approval
				if ( isSiteAdmin == false )
				{
					SetConsoleSuccessMessage( "Your organization has been created but must be approved by site administration. You will be notified within one business day" );
					//don't add, or will appear in the list ==> check if retrieve only gets active!!
					orgService.AddAdminUserForNewOrg( id, WebUser.Id, ref statusMessage );

					SendApprovalRequest( activeOrganization, user );

					//NOTE: not setting activeOrganization.Id as user cannot update it until approved
					//would be complicated with the inactive state
				}
				else
				{
					//need to add current user as administrator
					SetConsoleSuccessMessage( "Your organization has been created and you have been added as the administrator" );
					if ( orgService.AddAdminUserForNewOrg( id, WebUser.Id, ref statusMessage ) == false )
					{
						SetConsoleErrorMessage( statusMessage );
					}
				}//

				activeOrganization.Id = id;
				this.txtCurrentOrgId.Text = id.ToString();
			}
			else
			{
				//Otherwise, do the update
				orgService.Organization_Update( activeOrganization );

				SetConsoleSuccessMessage( "Your changes have been saved." );
			}

			//Refresh the active org to make sure any other changes are accounted for
			//NOTE for new org (and not admin),id will be zero, so nothing displays
			activeOrganization = OrganizationBizService.EFGet( activeOrganization.Id );

		} //End Update Org Properties
		
		/// <summary>
		/// verify email domain is of proper format and unique.
		/// </summary>
		/// <returns></returns>
		private bool ValidateEmailDomain()
		{
			bool isValid = true;
			string emailDomain = Request.Form[ "EmailDomain" ] ?? "";
			if ( emailDomain == null || emailDomain.Trim().Length == 0 )
				return true;

			if ( emailDomain.StartsWith( "@" ) )
			{
				SetConsoleErrorMessage( "The email domain must not start with an @ sign" );
				return false;
			}

			//validate reasonable length and format
			if ( emailDomain.Length < 5 )
			{
				SetConsoleErrorMessage( "Please enter an email domain of a reasonable length" );
				return false;
			}
			//skip common email providers
			if ( litCommonDomains.Text.IndexOf( emailDomain.ToLower() ) > -1 )
			{
				SetConsoleErrorMessage( "Please only use an organization email domain, not a public email domain" );
				return false;
			}

			//ensure email domain is unique
			Organization org = OrganizationBizService.GetByEmailDomain( emailDomain );
			if ( org != null && org.Id > 0 )
			{
				if ( activeOrganization.Id > 0 && activeOrganization.Id == org.Id )
					return true;

				//if not this org, then error
				SetConsoleErrorMessage( "Error: this email domain is already associated with another organization" );
				return false;
			}

			return isValid;
		} //

		private void SendApprovalRequest( Organization org, Patron user )
		{

			string statusMessage = "";

			string toEmail = UtilityManager.GetAppKeyValue( "contactUsMailTo", "mparsons@siuccwd.com" );
			string bcc = UtilityManager.GetAppKeyValue( "systemAdminEmail", "mparsons@siuccwd.com" );
			string fromEmail = user.Email;
			
			//
			bool isSecure = false;
			string msg = org.Name + "<br/>" +  org.LocationSummary();

			string subject = string.Format( "IOER - new organization request: {0}", org.Name );

			string body = string.Format( this.txtNewOrgRequest.Text, user.FullName(), msg );


			EmailManager.SendEmail( toEmail, fromEmail, subject, body, "", bcc );

		}
		protected void btnDeleteOrg_Click( object sender, EventArgs e )
		{
			//SetConsoleInfoMessage( "Not implemented" );
			string statusMessage = "";

			if ( orgService.Organization_SetInactive( activeOrganization.Id, WebUser.Id ) == false )
			{
				SetConsoleErrorMessage( "This organization has been set inactive. " );
				this.txtCurrentOrgId.Text = "0";
			}
			else
			{
				SetConsoleSuccessMessage( "This organization has been set inactive. All related components will no longer be available." );
				activeOrganization = new MyOrg();
			}
		}

		#region Helper Methods
		private string CheckTextField( string text, int minLength, ref bool valid, string name )
		{
			var status = "";
			text = utilService.ValidateText( text, minLength, name, ref valid, ref status );
			if ( !valid )
			{
				SetConsoleErrorMessage( status );
				errors++;
			}
			return text;
		}

		private string CheckUrlField( string text, ref bool valid, string name )
		{
			if ( text.Length == 0 )
			{
				return text;
			}

			var status = "";
			text = utilService.ValidateURL( text, false, ref valid, ref status );
			if ( !valid )
			{
				SetConsoleErrorMessage( status );
				errors++;
			}
			return text;
		}

		#endregion 



	
	}
}