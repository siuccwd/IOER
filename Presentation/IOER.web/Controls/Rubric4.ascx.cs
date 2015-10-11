using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using LRWarehouse.Business;
using LRWarehouse.DAL;

namespace IOER.Controls
{
  public partial class Rubric4 : BaseUserControl
  {

    //public Patron user = new Patron();
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
      /*if ( IsUserAuthenticated() )
      {
        user = ( Patron ) WebUser;
      }
      else
      {
        rubric.Visible = false;
        error.Visible = true;
        errorMessage.Text = "You must login to use this tool.";
        return false;
      }*/

      //Get the resource
			resourceVersionID = 456319;

      /*try
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
      }*/

			SetupRatingRBLs();

      return true;
    }

		private void SetupRatingRBLs()
		{
			foreach(RadioButtonList list in rubric.Controls.OfType<RadioButtonList>())
			{
				list.RepeatLayout = RepeatLayout.UnorderedList;
				list.Items.Clear();
				list.Items.Add( new ListItem( "Superior", "3" ) );
				list.Items.Add( new ListItem( "Strong", "2" ) );
				list.Items.Add( new ListItem( "Limited", "1" ) );
				list.Items.Add( new ListItem( "Very Weak/None", "0" ) );
				list.Items.Add( new ListItem( "Not Applicable", "-1" ) );
				list.Items.FindByValue( "-1" ).Selected = true;
			}
		}

		public void btnSubmit_Click( object sender, EventArgs e )
		{
			var score = 0;
			var addedScores = 0;

			foreach ( RadioButtonList list in rubric.Controls.OfType<RadioButtonList>() )
			{
				var val = int.Parse( list.SelectedValue );
				if ( val != -1 )
				{
					score += val;
					addedScores++;
				}
			}

			if ( addedScores == 0 )
			{
				return;
			}
			else
			{
				var average = ( decimal ) score / addedScores;

				//Save the score
			}
		}
  }
}