using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Utilities;
namespace IOER.Activity
{
  public partial class ActivityRenderer : System.Web.UI.UserControl
  {
		public DateTime startDate { get; set; }
		public DateTime endDate { get; set; }
		public int timeSpan { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {

    }

		public void GetDateRange( ref System.Web.UI.HtmlControls.HtmlInputText txtStartDate, ref System.Web.UI.HtmlControls.HtmlInputText txtEndDate )
		{
			//Get Activity
			startDate = new DateTime();
			endDate = new DateTime();
			timeSpan = UtilityManager.GetAppKeyValue("activityDefaultTimespan",30);
			try
			{
				startDate = DateTime.Parse( txtStartDate.Value );
				endDate = DateTime.Parse( txtEndDate.Value );
				if ( endDate == DateTime.Today ) //Show realtime(ish) changes
				{
					endDate = DateTime.Now; 
				}
				if ( startDate > endDate ) { throw new Exception(); }
				timeSpan = ( int ) ( endDate - startDate ).TotalDays;
			}
			catch
			{
				startDate = DateTime.Today.AddDays( -1 * timeSpan );
				endDate = DateTime.Today;
			}
			txtStartDate.Value = startDate.ToShortDateString();
			txtEndDate.Value = endDate.ToShortDateString();
		}
  }
}