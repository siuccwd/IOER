using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Services;
using LRWarehouse.Business;
using Isle.BizServices;

namespace ILPathways.Controls.Admin
{
  public partial class BigAdmin1 : BaseUserControl
  {
    public List<ManagementList> MyManagementLists { get; set; }
    public ManagementList MyOrganizations { get; set; }
    public ManagementList MyLibraries { get; set; }
    public ManagementList MyCommunities { get; set; }
    public ManagementList MyLearningLists { get; set; }
    public ManagementList ActiveList { get; set; }
    public ManagementObject ActiveObject { get; set; }
    public ManagementObject ActiveOrganization { get; set; }
    public Patron user { get; set; }
    public string manage { get; set; }
    public int manageID { get; set; }
    public bool isOrganizationMode { get; set; }
    public int organizationModeID { get; set; }
    public string orgURL { get; set; }

    public BigAdmin1()
    {
      MyManagementLists = new List<ManagementList>();
      MyOrganizations = new ManagementList() { title = "My Organizations", objectType ="organization" };
      MyLibraries = new ManagementList() { title = "My Libraries", objectType = "library" };
      MyCommunities = new ManagementList() { title = "My Communities", objectType = "community" };
      MyLearningLists = new ManagementList() { title = "My Learning Lists", objectType = "learninglist" };
      MyManagementLists.Add( MyOrganizations );
      MyManagementLists.Add( MyLibraries );
      MyManagementLists.Add( MyCommunities );
      MyManagementLists.Add( MyLearningLists );
      ActiveList = new ManagementList() { title = "", objectType = "" };
      ActiveObject = new ManagementObject();
    }

    protected void Page_Load( object sender, EventArgs e )
    {
      //Validate login
      ValidateUser();
      
      //Determine what to show
      LoadPageData();
    }

    //Make sure the user is logged in
    private void ValidateUser()
    {
      if ( IsUserAuthenticated() )
      {
        user = ( Patron ) WebUser;
      }
      else
      {
        Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
      }
    }

    //Figure out what to show on the page
    private void LoadPageData()
    {
      //Determine basic mode information
      ActiveList.title = "My IOER Administration";

      //Determine which mode we're in
      DetermineManagementMode();

      //Get an overview of the data available for this user
      LoadManagementData();

      //Get detailed data about the current object being managed
      manage = manage.ToLower();
      ActiveList = MyManagementLists.Where( m => m.objectType == manage ).FirstOrDefault();
      ActiveObject = ActiveList.Where( m => m.id == manageID ).FirstOrDefault(); //Will be null if id = 0 or id not found, which is good

      //Load data into the Privilege DDL
      PopulatePrivilegeDDL();
    }

    //Determine whether or not we're doing things on behalf of an organization
    private void DetermineManagementMode()
    {
      //Determine org mode
      isOrganizationMode = Request.Params[ "mode" ] == "organization";
      if ( isOrganizationMode )
      {
        try
        {
          organizationModeID = int.Parse( Request.Params[ "orgID" ] );
        }
        catch
        {
          MainEditor.Visible = false;
          MainError.Visible = true;
          MainError.InnerHtml = "Error: Invalid Organization ID";
          organizationModeID = 0;
          isOrganizationMode = false;
        }
      }

      //Set the org URL for use in the ascx
      orgURL = ( isOrganizationMode ? "&mode=organization&orgID=" + organizationModeID : "" );

      //Determine what is being managed
      try
      {
        manage = Request.Params[ "manage" ];
        manageID = int.Parse( Request.Params[ "id" ] );
        if ( string.IsNullOrWhiteSpace( manage ) )
        {
          throw new ArgumentException();
        }
      }
      catch
      {
        manage = "";
        manageID = 0;
      }

    }

    //Load management data
    public void LoadManagementData()
    {
      //Load organizations
      if ( user.OrgMemberships.Count() > 0 )
      {
        foreach ( var item in user.OrgMemberships.OrderBy( m => m.Organization ) )
        {
          //If not acting on behalf of an organzation OR if we are acting on behalf of the organization that matches the item...
          if ( !isOrganizationMode || ( isOrganizationMode && item.OrgId == organizationModeID ) )
          {
            MyOrganizations.Add( new ManagementObject()
            {
              id = item.OrgId,
              title = item.Organization,
              privilege = item.OrgMemberType,
              description = item.Message,
              //imageURL = item.ImageUrl,
              manageLink = "?manage=organization&id=" + item.OrgId + orgURL,
              metadata = new List<MetadataDTO>() 
            {
              new MetadataDTO() { key = "Joined" , value = item.Created.ToShortDateString() },
              new MetadataDTO() { key = "Monthly Visits", value =  "99" },
              new MetadataDTO() { key = "Resources", value =  "999" }
            }
            } );
            //If acting on behalf of an organization that matches the item, set the org to be the active organization
            if ( isOrganizationMode && item.OrgId == organizationModeID )
            {
              ActiveOrganization = MyOrganizations.FirstOrDefault();
            }
          }
        }
      }

      //Load libraries
      var libs = new LibraryBizService().Library_SelectListWithEditAccess( user.Id );
      if ( libs.Count() > 0 )
      {
        foreach ( var item in libs.OrderBy( m => m.Title ) )
        {
          if ( !isOrganizationMode || ( isOrganizationMode && item.OrgId == organizationModeID ) )
          {
            MyLibraries.Add( new ManagementObject()
            {
              id = item.Id,
              title = item.Title,
              privilege = item.OrgAccessLevel.ToString(),
              description = item.Description,
              imageURL = item.ImageUrl,
              manageLink = "?manage=library&id=" + item.Id + orgURL,
              metadata = new List<MetadataDTO>() 
            {
              new MetadataDTO() { key = "Joined" , value = item.Created.ToShortDateString() },
              new MetadataDTO() { key = "Monthly Visits", value =  "99" },
              new MetadataDTO() { key = "Resources", value =  "999" }
            }
            } );
          }
        }
      }

      //Load communities
      var coms = new CommunityServices().Community_SelectAll();
      if ( coms.Count() > 0 )
      {
        foreach ( var item in coms.OrderBy( m => m.Title ) ) //Need a method to select communities that the user controls
        {
          if ( !isOrganizationMode || ( isOrganizationMode && item.OrgId == organizationModeID ) )
          {
            MyCommunities.Add( new ManagementObject()
            {
              id = item.Id,
              title = item.Title,
              privilege = item.OrgAccessLevel.ToString(),
              description = item.Description,
              imageURL = item.ImageUrl,
              manageLink = "?manage=community&id=" + item.Id + orgURL,
              metadata = new List<MetadataDTO>() 
            {
              new MetadataDTO() { key = "Created" , value = item.Created.ToShortDateString() }
            }
            } );
          }
        }
      }

      //Load learning lists
      var learns = new CurriculumServices().Learninglists_SelectUserEditableLists( user.Id );
      if ( learns.Count() > 0 )
      {
        foreach ( var item in learns.OrderBy( m => m.Title ) )
        {
          MyLearningLists.Add( new ManagementObject()
          {
            id = item.Id,
            title = item.Title,
            privilege = item.PartnerType,
            description = item.Description,
            imageURL = item.ImageUrl,
            url = "/learninglist/" + item.Id,
            manageLink = "?manage=learninglist&id=" + item.Id + orgURL,
            metadata = new List<MetadataDTO>() 
          {
            new MetadataDTO() { key = "Created" , value = item.Created.ToShortDateString() },
            new MetadataDTO() { key = "Published", value =  item.IsPublished ? "Yes" : "No" },
          }
          } );
        }
      }

    }

    //Populate the privileges DDL
    private void PopulatePrivilegeDDL()
    {
      try
      {
        var data = new AdminService1().ListPrivileges( ActiveList.objectType ).data as List<ILPathways.Services.AdminService1Components.Privilege>;
        foreach ( var item in data )
        {
          ddlPrivilege.Items.Add( new ListItem()
          {
            Value = item.id.ToString(),
            Text = item.text
          } );
        }
      }
      catch { }
    }
  }

  public class ManagementList : List<ManagementObject>
  {
    public string title { get; set; }
    public string objectType { get; set; }
  }
  public class ManagementObject
  {
    public ManagementObject()
    {
      metadata = new List<MetadataDTO>();
    }
    public int id { get; set; }
    public string title { get; set; }
    public string url { get; set; }
    public string description { get; set; }
    public string imageURL { get; set; }
    public string privilege { get; set; }
    public string manageLink { get; set; }
    public List<MetadataDTO> metadata { get; set; }
  }
  public class MetadataDTO
  {
    public string key { get; set; }
    public string value { get; set; }
  }

}