using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

namespace ILPathways.Feed
{
  public partial class Feed1 : System.Web.UI.UserControl
  {

    public List<EventList> eventRanges { get; set; }
    public string ranges { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      //Do usual init stuff
      InitPage();

      //Get the list of events to display
      GetEvents();

      //Serialize the EventList
      ranges = "var ranges = " + new JavaScriptSerializer().Serialize( eventRanges ) + ";";
    }

    protected void InitPage()
    {
      eventRanges = new List<EventList>();
    }

    protected void GetEvents()
    {
      var userNames = new string[] { "Mike Parsons", "Jerome Grimmer", "Herb Williams", "Nathan Argo", "Jeanne Kitchens", "Deb Young" };
      var libraryNames = new string[] { "STEM Health Science Library", "Bees", "STEM Manufacturing Library", "Math Resources", "Cans of Energy", "Medical Science", "New honeycomb advancements" };
      var resourceNames = new string[] { "Resource Title Here", "Many Bees", "Counting Figures", "The Brain and You: An Introspective Look", "Trees" };
      var titleNames = new string[] { "Sample Title", "Name of an Item", "This is a Title", "Here's a Fancy Title with a lot of extra words. Here's a Fancy Title with a lot of extra words.", "A Title here" };
      var lorem = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
      var randomizer = new Random();

      for ( var i = 0 ; i < randomizer.Next( 3, 7 ) ; i++ ) //up to 7 eventLists
      {
        var currentList = new EventList();
        currentList.startDate = DateTime.Now.AddDays( ( i + randomizer.Next( i, i + 10 ) ) * -1 ).ToShortDateString();
        currentList.endDate = currentList.startDate;

        for ( var j = 0 ; j < randomizer.Next( 1, 10 ) ; j++ ) //events to go in the list
        {
          var current = new Event();
          //Type
          var randType = randomizer.Next( 1, 5 );
          switch ( randType )
          {
            case 1:
              current.item.type = "news";
              current.title = "News Update";
              current.item.title = titleNames[ randomizer.Next( 0, titleNames.Length - 1 ) ];
              current.item.description = lorem.Substring( randomizer.Next( 0, lorem.Length - 1 ) );
              current.action = "added";
              current.actor.title = "IOER";
              current.location.title = "IOER News";
              break;
            case 2:
              current.item.type = "comment";
              current.title = "Comment";
              current.item.title = titleNames[ randomizer.Next( 0, titleNames.Length - 1 ) ];
              current.item.description = lorem.Substring( randomizer.Next( 0, lorem.Length - 1 ) );
              current.action = "added";
              current.actor.title = userNames[ randomizer.Next( 0, userNames.Length - 1 ) ];
              current.location.title = resourceNames[ randomizer.Next( 0, resourceNames.Length - 1 ) ];
              break;
            case 3:
              current.item.type = "resource";
              current.title = "Resource";
              current.item.title = titleNames[ randomizer.Next( 0, titleNames.Length - 1 ) ];
              current.item.description = lorem.Substring( randomizer.Next( 0, lorem.Length - 1 ) );
              current.action = "added";
              current.actor.title = userNames[ randomizer.Next( 0, userNames.Length - 1 ) ];
              current.location.title = resourceNames[ randomizer.Next( 0, resourceNames.Length - 1 ) ];
              break;
            case 4:
              current.item.type = "collection";
              current.title = "Collection";
              current.item.title = titleNames[ randomizer.Next( 0, titleNames.Length - 1 ) ];
              current.item.description = lorem.Substring( randomizer.Next( 0, lorem.Length - 1 ) );
              current.action = "added";
              current.actor.title = userNames[ randomizer.Next( 0, userNames.Length - 1 ) ];
              current.location.title = libraryNames[ randomizer.Next( 0, libraryNames.Length - 1 ) ];
              break;
            default: break;
          }
          current.item.link = "http://www.ioer.ilsharedlearning.org";
          current.item.thumbnail = "/images/ioer_med.png";
          current.location.thumbnail = "/images/ioer_med.png";
          current.date = currentList.startDate;

          currentList.events.Add( current );
        }

        eventRanges.Add( currentList );
      }
    }
  }

  public class EventList
  {
    public EventList()
    {
      events = new List<Event>();
    }
    public string startDate { get; set; }
    public string endDate { get; set; }
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

    public string title { get; set; }
    public EventItem item { get; set; }
    public string action { get; set; }
    public EventItem actor { get; set; }
    public EventItem location { get; set; }
    public string date { get; set; }
  }

  public class EventItem
  {
    public string type { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string thumbnail { get; set; }
    public string link { get; set; }
  }

}