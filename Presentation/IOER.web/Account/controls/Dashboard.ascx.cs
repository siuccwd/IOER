using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.DTO;
using Isle.BizServices;
using LibMgr = Isle.BizServices.LibraryBizService;
using IOER.Library;
using ILPathways.Utilities;

using IOER.Controllers;
using LRWarehouse.Business;

namespace IOER.Account.controls
{
  public partial class Dashboard : BaseUserControl
  {
    public DashboardDTO dashboard = new DashboardDTO();
    bool usingPrototype = false;
    private const string CAN_VIEW_ADMIN_MENU = "CAN_VIEW_ADMIN_MENU";
    private const string CAN_VIEW_LIBRARYADMIN_MENU = "CAN_VIEW_LIBRARYADMIN_MENU";

    public string varFollowingUserId { get; set; }
    public string varFollowedByUserId { get; set; }

    bool isValid; 
    string status;

    protected void Page_Load( object sender, EventArgs e )
    {
      //Check for login
      if ( !IsUserAuthenticated() )
      {
        Response.Redirect( "/Account/Login.aspx?nextUrl=" + Request.Url.PathAndQuery );
        return;
      }

        if ( !Page.IsPostBack )
        {
            //Temporary
            if ( usingPrototype )
                LoadPrototype();
            else
                InitializeView();
        }

    }

    protected void InitializeView()
    {
        int maxResources = 10;

        //currently no postbacks, but will be once allowing configuring
        varFollowingUserId = "var followingUserId = \"0\";";
        varFollowedByUserId = "var followedByUserId = \"0\";";
        dashboard = new DashboardDTO();

        int targetUserId = GetTargetUserId();

        //Determine what to show
        //is My dashboard
        if ( IsUserAuthenticated()
            && ( Request.RawUrl.ToLower() == "/my/dashboard" || targetUserId == WebUser.Id) )
        {
            btnFollow.Visible = false;
            //dashboard = AccountServices.GetMyDashboard( GetAppUser(), maxResources );

            AccountServices.FillDashboard( dashboard, GetAppUser() );

            libAdminLink.Visible = CanUserAdminLibraries( WebUser.Id );

            orgAdminLink.Visible = OrganizationBizService.IsAnyOrgAdministrator(WebUser.Id);


            seeAllMyFollowedLink.Visible = true;
            seeAllMyOrgLibrariesLink.Visible = true;
            //this.quickLinks.Visible = true;
            return;
        }
        //check for route or other access types
        int requestedByUserId = 0;

        if ( IsUserAuthenticated() )
            requestedByUserId = WebUser.Id;
        if ( targetUserId == 0 )
            targetUserId = requestedByUserId;

        //if ( targetUserId == requestedByUserId)
            //this.quickLinks.Visible = true;

        if ( targetUserId == requestedByUserId || requestedByUserId == 0 )
            btnFollow.Visible = false;
        else
        {
            //check if already following
            if ( AccountServices.Person_FollowingIsMember( targetUserId, requestedByUserId ) )
            {
                btnFollow.Value = "Unfollow";
                //followingNextAction.Text = "Unfollow";
            }
            else
            {
                //followingNextAction.Text = "Follow";
            }
        }

        if(Request.RawUrl.ToLower().IndexOf("/my/") > -1 && !IsUserAuthenticated()){
          errorMessage.Visible = true;
          entireDashboard.Visible = false;
        }

        //check for my view versus a public view
        varFollowingUserId = string.Format( "var followingUserId = \"{0}\";", targetUserId );
        varFollowedByUserId = string.Format( "var followedByUserId = \"{0}\";", requestedByUserId );

        if ( targetUserId > 0 )
        {
            AccountServices.FillDashboard( dashboard, targetUserId, requestedByUserId );
        }
        else
        {
            SetConsoleErrorMessage( "Invalid request" );
        }
    }
    protected bool CanUserAdminLibraries( int userId )
    {
        bool canView = false;
        //check if previously done
        string auth = IOER.classes.SessionManager.Get( Session, CAN_VIEW_LIBRARYADMIN_MENU, "missing" );
        if ( auth.Equals( "yes" ) )
        {
            canView = true;
        }
        else if ( auth.Equals( "missing" ) )
        {
            canView = new LibMgr().Library_CanUserAdministerLibraries( userId );
            if ( canView )
                IOER.classes.SessionManager.Set( Session, CAN_VIEW_LIBRARYADMIN_MENU, "yes" );
            else
                IOER.classes.SessionManager.Set( Session, CAN_VIEW_LIBRARYADMIN_MENU, "no" );
        }


        return canView;
    }
    protected int GetTargetUserId()
    {
        int userId = 0;

        if ( Page.RouteData.Values.Count > 0
                && Page.RouteData.Values.ContainsKey( "userId" ) )
        {
            userId = int.Parse( Page.RouteData.Values[ "userId" ].ToString() );
        }
        else
        {
            userId = FormHelper.GetRequestKeyValue( "id", 0 );
        }
        if ( userId == 0 && IsUserAuthenticated() )
            userId = WebUser.Id;

        return userId;
    }

    protected void LoadPrototype()
    {
        //Temporary
        dashboard.name = "Test User";
        dashboard.avatarUrl = "https://ioer.ilsharedlearning.org/images/ioer_med.png";
        dashboard.description = "A member of the development team";
        dashboard.role = "Tester";
        dashboard.organization = "IOER";
        dashboard.library.name = "Test User's Personal Library";
        dashboard.myResources.name = dashboard.name + "'s Resources";
        dashboard.followedLibraryResources.name = "Resources from Libraries " + dashboard.name + " follows";
        dashboard.isMyDashboard = true;

        dashboard.myResources.total = 32;
        dashboard.myResources.resources.Add( new DashboardResourceDTO() { title = "Some Resource", id = 450293, containerTitle = "Some Collection" } );
        dashboard.myResources.resources.Add( new DashboardResourceDTO() { title = "Some Other Resource", id = 450258, containerTitle = "Some Collection" } );
        dashboard.myResources.resources.Add( new DashboardResourceDTO() { title = "Another Resource", id = 450267, containerTitle = "Some Other Collection" } );
        dashboard.myResources.resources.Add( new DashboardResourceDTO() { title = "One More Resource", id = 450287, containerTitle = "Some Other Collection" } );
        dashboard.myResources.resources.Add( new DashboardResourceDTO() { title = "An containerTitle Resource", id = 450260, containerTitle = "Some Collection" } );

        dashboard.orgLibraries.resources.Add( new DashboardResourceDTO() { title = "Another Resource Title Here", id = 450298, containerTitle = "My Organization" } );
        dashboard.orgLibraries.resources.Add( new DashboardResourceDTO() { title = "Yet Another Resource", id = 450253, containerTitle = "Other Organization" } );
        dashboard.orgLibraries.resources.Add( new DashboardResourceDTO() { title = "More Resource", id = 450233, containerTitle = "Other Organization" } );
    }

    public void btnUpdateAvatar_Click(object sender, EventArgs e)
    {
        if (IsUserAuthenticated())
        {
            var user = (Patron)WebUser;

            try
            {
                if (fileAvatar.HasFile == false || fileAvatar.FileName == "")
                {
                    SetConsoleErrorMessage("An image was not provided.<br/>Please select a valid image and try again!");
					//terrible but??
					InitializeView();
                    return;

                }

                int imageWidth = UtilityManager.GetAppKeyValue("libraryImageWidth", 150);
                string ext = System.IO.Path.GetExtension(fileAvatar.FileName);
                if (ext == null || ext.Trim().Length == 0)
                {
                    SetConsoleErrorMessage("The selected file does not have an extention type. Please select a valid image and try again!");
                    isValid = false;
                    return;
                }
                string savingName = user.RowId.ToString().Replace("-", "") + System.IO.Path.GetExtension(fileAvatar.FileName);
                string savingFolder = FileResourceController.DetermineDocumentPath(user.Id, 0);
                string savingURL = FileResourceController.DetermineMyImageUrl(user.Id, savingName);
                var img = new ILPathways.Business.ImageStore();
                img.FileName = savingName;
                img.FileDate = DateTime.Now;

                FileResourceController.HandleImageResizingToWidth(img, fileAvatar, imageWidth, imageWidth, false, true);
                FileSystemHelper.HandleImageCaching(savingFolder, img, true);

                bool isNewProfile = false;
                //get the current profile
                PatronProfile profile = new AccountServices().PatronProfile_Get(user.Id);
                if (profile == null)
                {
                    profile = new PatronProfile();
                    profile.UserId = user.Id;
                    profile.ImageUrl = "";
                    isNewProfile = true;
                }

                if (profile.ImageUrl != savingURL)
                {
                    string statusMessage = "";
                    profile.ImageUrl = savingURL;

                    if (profile.UserId == 0)
                        profile.UserId = user.Id;
                    if (isNewProfile)
                        new AccountServices().PatronProfile_Create(profile, ref statusMessage);
                    else
                        new AccountServices().PatronProfile_Update(profile);
                }

            }
            catch (Exception ex)
            {
                status = ex.Message;
                LoggingHelper.LogError(ex, "Profile().HandleUpload");
                SetConsoleErrorMessage("There was an error processing your request. Please try again later.");
                isValid = false;
            }
            InitializeView();
            //InitPage();
        }
    }
  }
}