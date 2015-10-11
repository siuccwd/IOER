using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;
using IOER.Library;
using Isle.BizServices;

namespace IOER
{
  public partial class SessionHints : BaseUserControl
  {
    //List of hints to send to the client
    public string hintsJSON { get; set; }
    public string token { get; set; }
    public string hideHints { get; set; }
    private List<Hint> hints { get; set; }

    //Stuff used by this class
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    //Preloading the class
    public SessionHints()
    {
      hints = new List<Hint>();
      hintsJSON = "[]";
      token = "";
      hideHints = "false";
    }
    
    //Main page load
    protected void Page_Load( object sender, EventArgs e )
    {
      if ( IsUserAuthenticated() )
      {
        LoadHints();
        token = WebUser.ProxyId;
      }
    }

    private void LoadHints()
    {
      //Get any hints from the session
      hints = ( List<Hint> ) Session[ "hints" ] ?? new List<Hint>();

      //Add the hints
      AddHints();

      //Add/Replace the Session's hints object
      Session.Remove("hints");
      Session.Add("hints", hints);

      //Load the JSON with the hints
      hintsJSON = serializer.Serialize( hints );

      //Don't over-annoy the user
      hideHints = (string) Session[ "hideHints" ] ?? "false";
    }

    private void AddHints()
    {
      return; //Temporary, until we're ready to use this

      var user = WebUser;
      var profile = AccountServices.GetUser( user.Id );

      //Nag the user to upload a profile image if they haven't done so AND if this hint hasn't already been added
      if ( ( profile.ImageUrl == null || profile.ImageUrl == "" ) && hints.Where( m => m.name == "profileimage" ).Count() == 0 )
      {
        hints.Add( new Hint() { name = "profileimage", title = "Profile Image", text = "You haven't uploaded a <a href=\"/Account/Profile.aspx\">Profile Image</a> yet!" } );
      }

      //Nag the user to create a library if they haven't done so AND if this hint hasn't already been added
      if ( new LibraryBizService().GetMyLibrary( user ).Id == 0 && hints.Where( m => m.name == "haslibrary" ).Count() == 0 )
      {
        hints.Add( new Hint() { name = "haslibrary", title = "Your Library", text = "You can <a href=\"/My/Library\">open your own virtual Library</a> to collect and share Resources!" } );
      }

      //Test 2
      if ( hints.Where( m => m.name == "test2" ).Count() == 0 )
      {
        hints.Add( new Hint() { name = "test2", title = "Test 2", text = "This item is Test #2." } );
      }

      //Test 3
      if ( hints.Where( m => m.name == "test3" ).Count() == 0 )
      {
        hints.Add( new Hint() { name = "test3", title = "Test 3", text = "This item is Test #3. It should always be hidden to test hiding.", showing = false } );
      }

    }

    //JSON sent to client
      [Serializable]
    public class Hint
    {
      public Hint()
      {
        title = "";
        text = "";
        showing = true;
        name = "";
      }
      public string title { get; set; }
      public string text { get; set; }
      public bool showing { get; set; }
      public string name { get; set; }
    }
  }
}