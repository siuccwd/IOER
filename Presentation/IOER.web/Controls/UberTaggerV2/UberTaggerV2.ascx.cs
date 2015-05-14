using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using LRWarehouse.Business.ResourceV2;
using Isle.BizServices;
using System.Web.Script.Serialization;

namespace ILPathways.Controls.UberTaggerV2
{
  public partial class UberTaggerV2 : BaseUserControl
  {
    //Managers
    ResourceV2Services resService = new ResourceV2Services();
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    //Global Variables
    public List<Theme> Themes { get; set; }
    public Theme CurrentTheme { get; set; }
    public ResourceDB Resource { get; set; }
    public List<UsageRights> UsageRights { get; set; }
    public List<ResourceV2Services.DDLItem> ContentPrivileges { get; set; }
    public string LibColData { get; set; }
    public List<Business.OrganizationMember> OrganizationData { get; set; }

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
        //Check for login
        if ( !IsUserAuthenticated() )
        {
          Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
          return;
        }

        //Load Usage Rights
        UsageRights = resService.GetUsageRightsList();

        //Load ContentPrivileges
        ContentPrivileges = resService.GetContentPrivilegesList();

        //Load Library and Collection data
        var lib = resService.GetUserLibraryAndCollectionList( WebUser.Id );
        LibColData = serializer.Serialize( lib );

        //Load Organization data
        OrganizationData = OrganizationBizService.OrganizationMember_GetUserOrgs( WebUser.Id );

        //Initialize for a new Resource (or to handle an erroneous fetch)
        Resource = new ResourceDB();

        //Check to see if this is an update or a new publish
        if ( string.IsNullOrEmpty( Request.Params[ "resourceID" ] ) )
        {
          //Nothing to do but load the page
        }
        else
        {
          //Load Resource data
          try
          {
            Resource = resService.GetResourceDB( int.Parse( Request.Params[ "resourceID" ] ) );
            if ( Resource.ResourceId == 0 )
            {
              throw new Exception( "That ID does not match any Resource record" );
            }
          }
          catch ( Exception ex )
          {
            SetConsoleErrorMessage( "There was an error loading the requested Resource:<br />" + ex.Message );
            return;
          }

        } // End Resource check
      } // End IsPostBack check

    } // End Page_Load

    private void LoadTheme()
    {
      //Get all of the tags once, then select subsets of them by schema for each of the themes. Ideally these would come from a database table
      var allTags = resService.GetFieldAndTagCodeData();
      var ioerTags = new List<string>() { "accessRights", "inLanguage", "resourceType", "mediaType", "assessmentType", "careerCluster", "educationalUse", "educationalRole", "gradeLevel", "groupType", "k12Subject", "accessibilityControl", "accessibilityFeature", "accessibilityHazard" };
      var quickTags = new List<string>() { "careerCluster", "gradeLevel", "k12Subject" };
      var wnTags = new List<string>() { "accessRights", "inLanguage", "resourceType", "mediaType", "careerCluser", "demandDrivenIT", "disabilityTopic", "educationalRole", "explore", "gradeLevel", "guidanceScenario", "ilPathway", "jobs", "k12Subject", "layoffAssistance", "networking", "qualify", "region", "resources", "training", "wdqi", "wioaWorks", "wfePartner", "workNetarea" };
      //Assemble the list of theme objects - these would also ideally come from some kind of database call
      Themes = new List<Theme>()
      {
        new Theme() 
        { 
          Name = "uber", 
          VisibleSingleValueFields = new List<string>() { "Language", "AccessRights", "Creator", "Publisher", "Requirements", "LibraryAndCollection", "Organization", "Standards" }, 
          VisibleTagData = allTags.Where( i => i.Id > 0 ).ToList() //Select all
        },
        new Theme() 
        { 
          Name = "ioer", 
          VisibleSingleValueFields = new List<string>() { "Language", "AccessRights", "Creator", "Publisher", "Requirements", "LibraryAndCollection", "Organization", "Standards" }, 
          VisibleTagData = allTags.Where( i => ioerTags.Contains( i.Schema ) ).ToList()
        },
        new Theme() 
        { 
          Name = "quick", 
          VisibleSingleValueFields = new List<string>() { "LibraryAndCollection", "Organization", "Standards" }, 
          VisibleTagData = allTags.Where( i => quickTags.Contains( i.Schema ) ).ToList()
        },
        new Theme() 
        { 
          Name = "worknet", 
          VisibleSingleValueFields = new List<string>() { "Language", "Creator", "Publisher", "Requirements" }, 
          VisibleTagData = allTags.Where( i => wnTags.Contains( i.Schema ) ).ToList()
        },
      };

      //Now determine which of the above themes to use, or default to uber
      CurrentTheme = Themes.FirstOrDefault( t => t.Name == Request.Params[ "theme" ] ) ?? Themes.FirstOrDefault( t => t.Name == "uber" );

    }

    #region Helper Classes
    public class Theme {
      public string Name { get; set; }
      public List<string> VisibleSingleValueFields { get; set; }
      public List<FieldDB> VisibleTagData { get; set; }
    }
    #endregion
  }
}