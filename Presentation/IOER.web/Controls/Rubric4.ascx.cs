using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace ILPathways.Controls
{
  public partial class Rubric4 : BaseUserControl
  {
    /* Notes */
    /*
     * 1. Get the user ID
     * 2. Get the resource
     * 3. Has math standards? activate math rubric
     * 4. Has ELA standards and grades K-2? activate ELA K-2 rubric
     * 5. Has ELA standards and grades 3-12? activate ELA 3-12 rubric
     * 6. Has NGSS standards? activate NGSS rubric
     * 7. Has any "Assessment" resource type? foreach rubric, activate assessment dimension
     * 8. Assemble object
     * 
     * 1. Read object
     * 2. Assemble interface
     * 
     */

    public Patron user = new Patron();
    public ResourceEvaluationManager rManager = new ResourceEvaluationManager();
    public int resourceVersionID = 0;

    protected void Page_Load( object sender, EventArgs e )
    {
      if ( !InitPage() )
      {
        return;
      }


    }

    protected bool InitPage()
    {
      //Get the user
      if ( IsUserAuthenticated() )
      {
        user = ( Patron ) WebUser;
      }
      else
      {
        rubric.Visible = false;
        error.Visible = true;
        errorMessage.Text = "You must login to use this tool.";
        return false;
      }

      //Get the resource
      try
      {
        resourceVersionID = int.Parse( Session[ "versionID" ].ToString() );
        if ( resourceVersionID == 0 )
        {
          throw new Exception();
        }
      }
      catch ( Exception ex )
      {
        rubric.Visible = false;
        error.Visible = true;
        errorMessage.Text = "No Resource identified.";
        return false;
      }

      return true;
    }
  }
}