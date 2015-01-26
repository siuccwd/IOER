using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILP = ILPathways.Business;
using ILPathways.Common;
using ILPathways.Library;
using ILPathways.Utilities;
using Isle.BizServices;

namespace ILPathways.Controls
{
  public partial class Activity1 : BaseUserControl
  {
    public List<EventList> eventRanges = new List<EventList>();
    public string ranges { get; set; }
    public string userGUID { get; set; }

    /// <summary>
    /// Default number of days for timeline events
    /// </summary>
    public int historyDays = 30;
    /// <summary>
    /// max number of posts to return regardless of historyDays
    /// </summary>
    public int maxPosts = 100;

    protected void Page_Load( object sender, EventArgs e )
    {
      //Do usual init stuff
      InitPage();

      //Get the list of events to display
      GetEvents();

      //Serialize the EventList
      ranges = "var ranges = " + new JavaScriptSerializer().Serialize( eventRanges ) + ";";
    }

    public void InitPage()
    {
      if ( IsUserAuthenticated() )
      {
        userGUID = "var userGUID = \"" + WebUser.RowId + "\";";
        contribute.Visible = true;
        contributeTabs.Visible = true;
        LoadCommunities();
      }
      else
      {
        userGUID = "var userGUID = \"\";";
        contribute.Visible = false;
        contributeTabs.Visible = false;
      }
    }
    private void LoadCommunities()
    {
        List<CodeItem> list = CommunityServices.Community_SelectList();
        BaseDataManager.PopulateList( communitiesList, list, "id", "Title", "Select a Community" );
        if ( list.Count > 0 )
            communitiesList.SelectedIndex = 1;
    }

    /// <summary>
    /// Retrieve timeline events
    /// </summary>
    public void GetEvents()
    {
        List<ObjectActivity> activities = new List<ObjectActivity>();
        int userId = 0;
        Organization org = new Organization();

        if ( IsUserAuthenticated() )
            userId = WebUser.Id;

        maxPosts = UtilityManager.GetAppKeyValue( "maxTimelinePosts", maxPosts );
        historyDays = UtilityManager.GetAppKeyValue( "timelineHistoryDays", historyDays );
        int orgId = FormHelper.GetRouteKeyValue( Page, "orgId", 0 );
        if ( orgId > 0 )
            org = OrganizationBizService.EFGet( orgId );

        //Determine what to show
        if ( Request.RawUrl.ToLower() == "/my/timeline" && IsUserAuthenticated() )  //User wants their network
        {
            activities = ActivityBizServices.ObjectActivity_MyFollowingSummary( historyDays, WebUser.Id, maxPosts );
            myTimelineMessage.Visible = true;
            pageHeading.Text = myIoerHeader.Text;
        }
        else if ( Request.RawUrl.ToLower() == string.Format("/org/{0}/timeline", orgId)  )  
        {
            //not sure of user implications
            //&& IsUserAuthenticated()
            activities = ActivityBizServices.ObjectActivity_OrgSummary( historyDays, orgId, userId, maxPosts );
            orgTimelineMessage.Visible = true;
            pageHeading.Text = string.Format(orgIoerHeader.Text, org.Name);
        }
        else
        {
            if ( IsUserAuthenticated() )
                userId = WebUser.Id;

            activities = ActivityBizServices.ObjectActivity_RecentList( historyDays, userId, maxPosts );
            ioerTimelineMessage.Visible = true;
        }

        LoadActivities( activities );
      //activity.Actor  activity.Activity  
    }

    public void LoadActivities( List<ObjectActivity> activities )
    {
      var currentDay = DateTime.Today;
      var currentRange = new EventList() { startDate = currentDay.ToShortDateString() };
      if ( activities != null && activities.Count > 0 )
      {
        foreach ( Business.ObjectActivity activity in activities )
        {
          if ( activity.ObjectText.IndexOf( "testdata: " ) > -1 ) //Skip test resources
          {
            continue;
          }
          if ( activity.ActivityDay != currentDay ) //If the next activity happened on a different day...
          {
            //Update the current day
            if ( currentRange.events.Count > 0 )
            {
              eventRanges.Add( currentRange );
            }
            currentDay = activity.ActivityDay;
            currentRange = new EventList() { startDate = currentDay.ToShortDateString() };
          }

          //Add the activity to the current range's list of events
          var currentEvent = new Event();

          currentEvent.date = activity.ActivityDay.ToShortDateString();
          currentEvent.actionType = activity.Action.ToLower();
          currentEvent.actionTitle = activity.Activity;
          currentEvent.likes = activity.ObjectCount;
          currentEvent.dislikes = activity.ObjectCount2;
          currentEvent.hasRated = activity.HasObject;

          currentEvent.actor.id = activity.ActorId;
          currentEvent.actor.type = activity.ActorTypeId.ToString().ToLower();
          currentEvent.actor.title = activity.Actor;
          currentEvent.actor.description = "";
          currentEvent.actor.link = activity.ActorUrl;
          currentEvent.actor.thumbnail = activity.ActorImageUrl;

          currentEvent.item.id = activity.ObjectId;
          currentEvent.item.type = activity.ObjectType.ToLower();
          currentEvent.item.title = activity.ObjectTitle;
          currentEvent.item.description = activity.ObjectText;
          currentEvent.item.link = activity.ObjectUrl;
          currentEvent.item.thumbnail = activity.ObjectImageUrl;

          currentEvent.location.id = activity.TargetObjectId;
          currentEvent.location.type = activity.TargetType.ToLower();
          currentEvent.location.title = activity.TargetTitle;
          currentEvent.location.description = activity.TargetText;
          currentEvent.location.link = activity.TargetUrl;
          currentEvent.location.thumbnail = activity.TargetImageUrl;
          currentEvent.location.extraClass = "";
          if ( activity.TargetImageUrl == null || activity.TargetImageUrl.Trim().Length == 0 )
          {
              //hide it!!
              currentEvent.location.extraClass = "hideSection";
          }
          currentRange.events.Add( currentEvent );
        }
      }
      if ( currentRange.events.Count > 0 )
      {
        eventRanges.Add( currentRange );
      }
    }

    public class EventList
    {
      public EventList()
      {
        events = new List<Event>();
      }
      public string startDate { get; set; }
      public List<Event> events { get; set; }
    }

    public class Event
    {
      public Event()
      {
        item = new EventItem();
        actor = new EventItem();
        location = new EventItem();
      }

      public EventItem actor { get; set; }
      public string actionTitle { get; set; }
      public string actionType { get; set; }
      public EventItem item { get; set; }
      public EventItem location { get; set; }
      public string date { get; set; }
      public int likes { get; set; }
      public int dislikes { get; set; }
      public bool hasRated { get; set; }
    }

    public class EventItem
    {
      public int id { get; set; }
      public string type { get; set; }
      public string title { get; set; }
      public string description { get; set; }
      public string thumbnail { get; set; }
      public string link { get; set; }
      public string extraClass { get; set; }
    }

  }
}