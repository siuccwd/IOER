using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

using ILPathways.Business;
using IOER.Library;
using LRWarehouse.Business.ResourceV2;
using Isle.BizServices;
using LRWarehouse.Business;
using ILPathways.Utilities;


namespace IOER.Controls.UberTaggerV3
{
	public partial class UberTaggerV3 : BaseUserControl
	{
		//Managers
		ResourceV2Services resService = new ResourceV2Services();
		public JavaScriptSerializer serializer = new JavaScriptSerializer();

		//Global Variables
		public List<Theme> Themes { get; set; }
		public Theme CurrentTheme { get; set; }
		public ResourceDTO Resource { get; set; }
		public List<UsageRights> UsageRights { get; set; }
		public List<ResourceV2Services.DDLItem> ContentPrivileges { get; set; }
		public List<ResourceV2Services.DDLLibrary> UserLibraries { get; set; }
		public string LibColData { get; set; }
		public List<OrganizationMember> OrganizationData { get; set; }
		public string GooruResourceId { get; set; }
		public string GooruSessionToken { get; set; }
		public bool GooruSuccessful { get; set; }
		public bool CanUpdateSpecialFields { get; set; }
		public bool CanReplaceContent { get; set; }
		//TBD
		public string LRPublishAction = "";
		public string RequiresApproval = "";


		protected void Page_Load( object sender, EventArgs e )
		{
			//Load Theme
			LoadTheme();

			//Check for postback
			if ( IsPostBack )
			{

			}
			else
			{
				//Get User
				Patron user = new Patron();
				if ( !IsUserAuthenticated() )
				{
					Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
					return;
				}
				else
				{
					user = ( Patron ) WebUser;
				}

				//Load Usage Rights
				UsageRights = resService.GetUsageRightsList();

				//Load ContentPrivileges
				ContentPrivileges = resService.GetContentPrivilegesList();

				//Load Library and Collection data
				UserLibraries = resService.GetUserLibraryAndCollectionList( WebUser.Id );
				LibColData = serializer.Serialize( UserLibraries );

				//Load Organization data
				OrganizationData = OrganizationBizService.OrganizationMember_GetUserOrgs( WebUser.Id );

				//Initialize for a new Resource (or to handle an erroneous fetch)
				Resource = new ResourceDTO();

				//Try to load Resource data
				GooruSuccessful = false;
				CanReplaceContent = false;
				try
				{
					//Existing resource
					if ( !string.IsNullOrWhiteSpace( Request.Params[ "resourceID" ] ) )
					{
						LoadResource( user );
					}

					//Gooru resource
					else if ( !string.IsNullOrWhiteSpace( Request.Params[ "gooruID" ] ) )
					{
						LoadGooruResource( user );
					}

					//Content resource
					//should not use Id, unless we have a authorization check! -MP
					//Authorization happens in the load method and again later in the SaveResource method in the service -NA
					else if ( !string.IsNullOrWhiteSpace( Request.Params[ "contentID" ] ) )
					{
						LoadContentResource( user );
					}

					//New resource
					else
					{
						//Just load the tagger
					}

				}
				catch ( Exception ex )
				{
					SetConsoleErrorMessage( "There was an error loading the requested Resource:<br />" + ex.Message );
					return;
				}

				//Determine special field (title, description) update privileges
				CanUpdateSpecialFields = CanUserUpdateSpecialFields( Resource.ResourceId, user );

			} // End Resource check
		} // End Page_Load

		//Load normal resource
		private void LoadResource( Patron user )
		{
			var requestedResourceID = 0;

			int.TryParse( Request.Params[ "resourceID" ], out requestedResourceID );
			if ( requestedResourceID != 0 )
			{
				//Load the resource
				Resource = resService.GetResourceDTO( int.Parse( Request.Params[ "resourceID" ] ) );
				if ( Resource.ResourceId == 0 )
				{
					throw new Exception( "That ID does not match any Resource record" );
				}

				//Get existing access privilege ID - might be a better way to do this
				var canView = true;
				var content = new ContentServices().GetForResourceDetail( Resource.ResourceId, user, ref canView );
				if ( content != null && content.Id > 0 )
				{
					Resource.PrivilegeId = content.PrivilegeTypeId;
					Resource.ContentId = content.Id;
					ContentPartner cp = ContentServices.ContentPartner_Get( content.Id, user.Id );
					//Allow replacing content if the resource being updated has content and that content was created by the user
					//or is a partner, or is an admin with org
					if ( cp != null && cp.PartnerTypeId > ContentPartner.PARTNER_TYPE_ID_CONTRIBUTOR )
					{
						CanReplaceContent = true;
					}
				}

			}
		} // End LoadResource

		//Load Gooru Resource
		private void LoadGooruResource( Patron user )
		{
			GooruResourceId = Request.Params[ "gooruID" ];
			GooruSessionToken = "";
			//Get gooru token
			try
			{
				//Hack to get gooru session token - should probably move this to a central place
				GooruSessionToken = new IOER.Pages.GooruSearch().GetSessionToken();
				GooruSuccessful = true;
			}
			catch ( Exception ex )
			{
				GooruSessionToken = ex.Message;
				GooruSuccessful = false;
			}

			//Client handles the rest
		} //End LoadGooruResource

		//Load Content Resource
		private void LoadContentResource( Patron user )
		{
			var requestedResourceID = 0;
			int tempContentId = 0;

			int.TryParse( Request.Params[ "contentID" ], out tempContentId );
			if ( tempContentId == 0 )
			{
				return;
			}
			else
			{
				Resource.ContentId = tempContentId;
			}

			var status = "";
			var valid = true;
			Resource = resService.GetResourceDTOFromContent( tempContentId, user, ref valid, ref status, ref RequiresApproval, ref LRPublishAction );

			if ( !valid )
			{
				throw new Exception( status );
			}

		} //End LoadContentResource

		private bool CanUserUpdateSpecialFields( int resourceID, Patron user )
		{
			if ( resourceID == 0 )
			{
				return true;
			}
			else
			{
				var canEdit = ResourceBizService.CanUserEditResource( resourceID, user.Id );
				var permissions = SecurityManager.GetGroupObjectPrivileges( user, "IOER.Pages.ResourceDetail" );
				return canEdit || permissions.CreatePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.State;
			}
		}

		private void LoadTheme()
		{
			//Assemble the list of theme objects - these would ideally come from some kind of database call
			Themes = new List<Theme>()
      {
        new Theme() 
        { 
          Name = "uber", 
          VisibleSingleValueFields = new List<string>() { "Language", "AccessRights", "Creator", "Publisher", "Requirements", "LibraryAndCollection", "Organization", "Standards" }, 
					VisibleTagFields = new List<string>()
        },
        new Theme() 
        { 
          Name = "ioer", 
          VisibleSingleValueFields = new List<string>() { "Language", "AccessRights", "Creator", "Publisher", "Requirements", "LibraryAndCollection", "Organization", "Standards" }, 
					VisibleTagFields = new List<string>() { "accessRights", "inLanguage", "learningResourceType", "mediaType", "assessmentType", "careerCluster", "educationalUse", "educationalRole", "gradeLevel", "groupType", "k12Subject", "accessibilityControl", "accessibilityFeature", "accessibilityHazard", "nrsEducationalFunctioningLevel" }
        },
        new Theme() 
        { 
          Name = "quick", 
          VisibleSingleValueFields = new List<string>() { "LibraryAndCollection", "Organization", "Standards" }, 
					VisibleTagFields = new List<string>() { "careerCluster", "gradeLevel", "k12Subject" }
        },
        new Theme() 
        { 
          Name = "worknet", 
          VisibleSingleValueFields = new List<string>() { "Language", "Creator", "Publisher", "Requirements" }, 
					VisibleTagFields = new List<string>() { "accessRights", "inLanguage", "learningResourceType", "mediaType", "careerCluster", "demandDrivenIT", "disabilityTopic", "educationalRole", "explore", "gradeLevel", "guidanceScenario", "ilPathway", "jobs", "k12Subject", "layoffAssistance", "networking", "qualify", "region", "resources", "training", "wdqi", "wioaWorks", "wfePartner", "workNetarea", "nrsEducationalFunctioningLevel" }
        },
        new Theme() 
        { 
          Name = "worknet_public", 
          VisibleSingleValueFields = new List<string>() { "LibraryAndCollection" }, 
					VisibleTagFields = new List<string>() { "learningResourceType", "mediaType", "careerCluster", "educationalRole" }
        },
      };

			//Now determine which of the above themes to use, or default to uber
			CurrentTheme = Themes.FirstOrDefault( t => t.Name == Request.Params[ "theme" ] ) ?? Themes.FirstOrDefault( t => t.Name == "ioer" );

			//Get all of the tags
			var allTags = resService.GetFieldAndTagCodeData();

			//Assign the appropriate tags for the selected theme
			CurrentTheme.LoadTags( allTags );
		}

		#region Helper Classes
    public class Theme 
		{
			public Theme()
			{
				VisibleSingleValueFields = new List<string>();
				VisibleTagFields = new List<string>();
				VisibleTagData = new List<FieldDB>();
			}
      public string Name { get; set; }
      public List<string> VisibleSingleValueFields { get; set; }
			public List<string> VisibleTagFields { get; set; }
      public List<FieldDB> VisibleTagData { get; set; }
			public void LoadTags( List<FieldDB> allTags )
			{
				if ( VisibleTagFields.Count() == 0 )
				{
					VisibleTagData = allTags;
				}
				else
				{
					VisibleTagData = allTags.Where( i => VisibleTagFields.Contains( i.Schema ) ).ToList();
				}
			}
    }
		#endregion
	}
}