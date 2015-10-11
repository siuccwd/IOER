using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Serialization;
using Hint = IOER.SessionHints.Hint;

namespace IOER.Services
{
  /// <summary>
  /// Summary description for HintsService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class HintsService : System.Web.Services.WebService
  {
    public UtilityService utils = new UtilityService();

    //Hide an individual hint
    [WebMethod( EnableSession=true )]
    public string HideHint( string name )
    {
      //Get the hint data from the session
      var hints = ( List<Hint> ) Session[ "hints" ] ?? new List<Hint>();

      //Flag the target hint to be hidden
      try {
        hints.Where( m => m.name == name ).FirstOrDefault().showing = false;
      }
      catch {
        return utils.ImmediateReturn(null, false, "Invalid Hint Name", null);
      }

      //Save data back to the session
      Session.Remove( "hints" );
      Session.Add( "hints", hints );

      //Return result
      return utils.ImmediateReturn( hints, true, "okay", name );
    }

    //Toggle keeping the hint box hidden between page loads
    [WebMethod( EnableSession = true )]
    public string HideHints(bool hide)
    {
      Session.Remove( "hideHints" );
      Session.Add( "hideHints", (hide ? "true" : "false") );
      return utils.ImmediateReturn( hide, true, "okay", null );
    }

  }
}
