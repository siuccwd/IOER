using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using IOER.Library;
using LRWarehouse.Business.ResourceV2;
using Isle.BizServices;
using System.Web.Script.Serialization;
using LRWarehouse.Business;
using ILPathways.Utilities;

namespace IOER.Controls.UberTaggerV2
{
  public partial class UberTaggerV2 : BaseUserControl
  {
    //Managers
    ResourceV2Services resService = new ResourceV2Services();
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    //Global Variables
		public List<Theme> Themes { get; set; }
		public Theme CurrentTheme { get; set; }
		public ResourceDTO Resource { get; set; }
		public List<UsageRights> UsageRights { get; set; }
		public List<ResourceV2Services.DDLItem> ContentPrivileges { get; set; }
		public string LibColData { get; set; }
		public List<OrganizationMember> OrganizationData { get; set; }
		public string GooruResourceId { get; set; }
		public string GooruSessionToken { get; set; }
		public bool GooruSuccessful { get; set; }
		//public int ContentId { get; set; } //Only for use with content being tagged for the first time (e.g., learning list), NOT for updates to previously-tagged content
		public bool CanUpdateSpecialFields { get; set; }
		public bool CanReplaceContent { get; set; }
	  //TBD
		public string LRPublishAction = "";
		public string RequiresApproval = "";

		protected void Page_Load(object sender, EventArgs e)
		{

			//Load Theme
			LoadTheme();

			//Check for postback
			if (IsPostBack)
			{

			}
			else
			{
				Patron user = new Patron();
				//Check for login
				if (!IsUserAuthenticated())
				{
					Response.Redirect("/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery);
					return;
				}
				else
				{
					user = (Patron)WebUser;
				}

				//Load Usage Rights
				UsageRights = resService.GetUsageRightsList();

				//Load ContentPrivileges
				ContentPrivileges = resService.GetContentPrivilegesList();

				//Load Library and Collection data
				var lib = resService.GetUserLibraryAndCollectionList(WebUser.Id);
				LibColData = serializer.Serialize(lib);

				//Load Organization data
				OrganizationData = OrganizationBizService.OrganizationMember_GetUserOrgs(WebUser.Id);

				//Initialize for a new Resource (or to handle an erroneous fetch)
				Resource = new ResourceDTO();

				//Try to load Resource data
				GooruSuccessful = false;
				CanReplaceContent = false;
				try
				{
					//Existing resource
					if (!string.IsNullOrWhiteSpace(Request.Params["resourceID"]))
					{
						LoadResource( user );
					}

					//Gooru resource
					else if (!string.IsNullOrWhiteSpace(Request.Params["gooruID"]))
					{
						LoadGooruResource( user );
					}

					//Content resource
					//should not use Id, unless we have a authorization check! -MP
					//Authorization happens in the load method and again later in the SaveResource method in the service -NA
					else if (!string.IsNullOrWhiteSpace(Request.Params["contentID"]))
					{
						LoadContentResource( user );
					}

					//New resource
					else
					{
						//Just load the tagger
					}

				}
				catch (Exception ex)
				{
					SetConsoleErrorMessage("There was an error loading the requested Resource:<br />" + ex.Message);
					return;
				}

				//Determine special field (title, description) update privileges
				CanUpdateSpecialFields = CanUserUpdateSpecialFields(Resource.ResourceId, user);

			} // End Resource check
		} // End Page_Load

		private void LoadResource(Patron user)
		{
			var requestedResourceID = 0;

			int.TryParse(Request.Params["resourceID"], out requestedResourceID);
			if (requestedResourceID != 0)
			{
				//Load the resource
				Resource = resService.GetResourceDTO(int.Parse(Request.Params["resourceID"]));
				if (Resource.ResourceId == 0)
				{
					throw new Exception("That ID does not match any Resource record");
				}

				//Get existing access privilege ID - might be a better way to do this
				var canView = true;
				var content = new ContentServices().GetForResourceDetail(Resource.ResourceId, user, ref canView);
				if (content != null && content.Id > 0)
				{
					Resource.PrivilegeId = content.PrivilegeTypeId;
					Resource.ContentId = content.Id;
					ContentPartner cp = ContentServices.ContentPartner_Get(content.Id, user.Id);
					//Allow replacing content if the resource being updated has content and that content was created by the user
					//or is a partner, or is an admin with org
					//if (content.CreatedById == user.Id)
					if (cp != null && cp.PartnerTypeId > ContentPartner.PARTNER_TYPE_ID_CONTRIBUTOR )
					{
						CanReplaceContent = true;
					}
				}

			}
		} // End LoadResource

		//Load Content into a Resource object
		private void LoadContentResource(Patron user)
		{
			var requestedResourceID = 0;
			int tempContentId = 0;

			int.TryParse(Request.Params["contentID"], out tempContentId);
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

			/*
			//valid?
			var contentItem = new ContentServices().Get( Resource.ContentId );
			if (contentItem.Id == 0)
			{
				throw new Exception("That ID does not match valid content record");
			}

			//for now don't handle updates
			if (contentItem.ResourceIntId > 0)
			{
				throw new Exception("The referenced content record has already been published.");
			}
			//authorization
			ContentPartner cp = ContentServices.ContentPartner_Get( Resource.ContentId, user.Id );
			if (cp == null && cp.PartnerTypeId < ContentPartner.PARTNER_TYPE_ID_CONTRIBUTOR)
			{
				throw new Exception("You do not have authorization to publish the requested contentItem");
			}

			//save to initiate auto-publish later
			createdContentItemId.Text = Resource.ContentId.ToString();

			//Load the resource
	
			Resource.Url = SetContentUrl(contentItem);
			Resource.Title = contentItem.Title;
			Resource.Description = contentItem.Summary;
			//????

			Resource.UsageRights = resService.GetUsageRights(contentItem.UseRightsUrl);
			 
			//Creator
			Resource.Creator = contentItem.HasOrg() ? contentItem.ContentOrg.Name : user.FullName();
			//Publisher
			Resource.Publisher = ILPathways.Utilities.UtilityManager.GetAppKeyValue("defaultPublisher", "Illinois Shared Learning Environment");

			if (contentItem.PrivilegeTypeId != ContentItem.PUBLIC_PRIVILEGE)
			{
				LRPublishAction = "no";
			}
			else
			{
				Resource.PrivilegeId = contentItem.PrivilegeTypeId;
			}

			if (contentItem.IsOrgContent())
			{
				RequiresApproval = "yes"; //If it requires approval, we can't publish it yet
				if (contentItem.PrivilegeTypeId == ContentItem.PUBLIC_PRIVILEGE)
				{
					LRPublishAction = "save";
				}
			}

			//standards
			//confirm these will be published, not just ignored as existing
			if (contentItem.ContentStandards.Count > 0)
			{
				foreach (Content_StandardSummary css in contentItem.ContentStandards)
				{
					var standard = new StandardsDTO();
					//issue the StandardsDTO doesn't have aligned by
					//often the same. However, I suppose it is ok for publishing
					//standard.AlignedById = css.AlignedById;
					standard.StandardId = css.StandardId;
					standard.AlignmentTypeId = css.AlignmentTypeCodeId;
					//major/supporting/additional - recycles old object's at/above/below field
					standard.AlignmentDegreeId = css.UsageTypeId;

					Resource.Standards.Add(standard);
				}
			}
			*/

		} // End LoadContentResource

		/*
		//Determine content URL
		private string SetContentUrl(ContentItem contentItem)
		{
			string url = "";
			string contentUrl = ServiceHelper.GetAppKeyValue("contentUrl");
			if (contentItem.TypeId == 50)
			{
				url = ServiceHelper.GetAppKeyValue("learningListUrl");
				if (url.Length > 10)
					url = string.Format(url, contentItem.Id, ResourceBizService.FormatFriendlyTitle(contentItem.Title));
			}
			else if (contentItem.TypeId > 50 && contentItem.TypeId < 58)
			{
				//really need to check the parent, and if an LL, then know the url pattern to use
				url = ServiceHelper.GetAppKeyValue("learningListUrl");
				if (url.Length > 10)
					url = string.Format(url, contentItem.Id, ResourceBizService.FormatFriendlyTitle(contentItem.Title));
			}
			else if (contentItem.TypeId == 10)
			{
				if (contentUrl.Length > 10)
					url = string.Format(contentUrl, contentItem.Id, ResourceBizService.FormatFriendlyTitle(contentItem.Title));
			}
			else if (contentItem.DocumentUrl != null && contentItem.DocumentUrl.Length > 10)
			{
				//Should probably direct to the content page, not the document URL, so that the published URL will always go through security checks in the event that the owner changes permissions later  -NA
					url = UtilityManager.FormatAbsoluteUrl(contentItem.DocumentUrl, false);
			}
			else
			{
				//not sure, so default to content
				if (contentUrl.Length > 10)
					url = string.Format(contentUrl, contentItem.Id, ResourceBizService.FormatFriendlyTitle(contentItem.Title));
			}

			return url;
		} //End SetContentUrl
		*/

		//Load Gooru Resource
		private void LoadGooruResource(Patron user)
		{
			GooruResourceId = Request.Params["gooruID"];
			GooruSessionToken = "";
			//Get gooru token
			try
			{
				//Hack to get gooru session token - should probably move this to a central place
				GooruSessionToken = new IOER.Pages.GooruSearch().GetSessionToken();
				GooruSuccessful = true;
			}
			catch (Exception ex)
			{
				GooruSessionToken = ex.Message;
				GooruSuccessful = false;
			}

			//Client handles the rest
		} //End LoadGooruResource

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
          VisibleSingleValueFields = new List<string>() { "Language", "Creator", "Publisher", "Requirements", "LibraryAndCollection" }, 
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