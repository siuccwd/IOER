using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;
using Isle.BizServices;
using Isle.DTO;

namespace IOER.Activity
{
  public partial class Stats2 : System.Web.UI.UserControl
  {
    ActivityBizServices activityService = new ActivityBizServices();
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    public string accountDataJSON { get; set; }
    public string accountSummaryJSON { get; set; }
    public string organizationDataJSON { get; set; }
    public string libraryDataJSON { get; set; }
    public string datesJSON { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      //Fetch data
      var dates = new List<string>();

			activityRenderer.GetDateRange( ref txtStartDate, ref txtEndDate );

      //Setup date range
			for ( int i = 0 ; i < activityRenderer.timeSpan ; i++ )
      {
				var loopDate = activityRenderer.startDate.AddDays( i );
        dates.Add( loopDate.ToShortDateString() );
      }
      //dates.Reverse();
      datesJSON = serializer.Serialize( dates );

      //Accounts
			var accountData = activityService.ActivityTotals_Accounts( activityRenderer.startDate, activityRenderer.endDate ).Select( m => m.Activity ).ToList();
      accountData.Reverse();
      accountDataJSON = serializer.Serialize( accountData );
      //Accounts data summary
      var accountSummary = new ActivityCount() { Id = 0, Title = "Summary" };
      try
      {
        //For each type of activity, add it to the summary
        foreach ( var item in accountData.First().Activities )
        {
          accountSummary.Activities.Add( item.Key, new List<int>() );
        }
        accountSummary.Activities.Add( "activity_date", new List<int>() );
        //For each day in the timespan... 
        for ( int i = 0 ; i < dates.Count() ; i++ )
        {
          //Attempt to find an activity item with this date
          var dataItem = accountData.SingleOrDefault( m => DateTime.Parse( m.Title ).ToShortDateString() == dates[ i ] );
          //Regardless, for each activity in the summary...
          foreach ( var item in accountSummary.Activities )
          {
            //Relate this item to a date via index later on in the interface, and skip further processing for this item
            if ( item.Key == "activity_date" )
            {
              item.Value.Add( i );
              continue;
            }
            //If no activity item was found for this date, add 0
            if ( dataItem == null )
            {
              item.Value.Add( 0 );
            }
            //Otherwise, add that day's total to the list
            else
            {
              try
              {
                item.Value.Add( dataItem.Activities.Single( m => m.Key == item.Key ).Value.First() );
              }
              catch
              {
                item.Value.Add( -1 );
              }
            }
          }
        }
      }
      catch { }
      accountSummaryJSON = serializer.Serialize( accountSummary );

      //Organizations
      var organizationData = OrganizationBizService.OrganizationMember_Crosstab( true ).Select( m => m.Activity );
      organizationDataJSON = serializer.Serialize( organizationData );

      //Libraries
			var libraryData = activityService.ActivityTotals_Library( 0, activityRenderer.startDate, activityRenderer.endDate );
      libraryDataJSON = serializer.Serialize( libraryData );
    }

  }
}