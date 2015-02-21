using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Isle.BizServices;
using ILPathways.Business;
using LRWarehouse.Business;

using System.Data;

namespace ILPathways.Activity
{
  public partial class Stats1 : System.Web.UI.UserControl
  {
    public List<OrganizationStats> Organizations;
    Random random = new Random();

    protected void Page_Load( object sender, EventArgs e )
    {
      LoadStats();
    }

    private void LoadStats()
    {
      var orgService = new OrganizationBizService();
      var libService = new LibraryBizService();
      var curSurvice = new CurriculumServices();
      var conService = new ContentServices();

      //var orgs = new List<Organization>(); //replace with call to get all orgs

      //mega kludge
      //first get all libraries where orgID isn't null
      //next get all unique orgIDs
      //then get the orgs based on that list of IDs
      //recycle the data where possible later
      var libraries = new List<Business.Library>();
      var librariesDS = Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteDataset( ILPathways.DAL.LibraryManager.ContentConnectionRO(), System.Data.CommandType.Text, "SELECT * FROM Library WHERE OrgId IS NOT NULL" );
      var orgsToGet = new List<int>();
      foreach ( DataRow row in librariesDS.Tables[ 0 ].Rows )
      {
        libraries.Add( libService.Get( int.Parse( ILPathways.DAL.LibraryManager.GetRowColumn( row, "Id" ) ) ) );
        var targetOrg = int.Parse( ILPathways.DAL.LibraryManager.GetRowColumn( row, "OrgId" ) );
        if ( !orgsToGet.Contains( targetOrg ) )
        {
          orgsToGet.Add( targetOrg );
        }
      }

      var LibStatsDS = Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteDataset( ILPathways.DAL.LibraryManager.ContentConnectionRO(), System.Data.CommandType.Text, "SELECT * FROM ActivityLog WHERE Activity = 'Library' AND Event = 'Select' AND CreatedDate > '" + DateTime.Now.AddMonths(-1).ToShortDateString() + "'" );
      var views = new List<int>();
      foreach ( DataRow row in LibStatsDS.Tables[ 0 ].Rows )
      {
        views.Add( int.Parse( ILPathways.DAL.LibraryManager.GetRowColumn( row, "ActivityObjectId" ) ) );
      }

      Organizations = new List<OrganizationStats>();
      foreach ( var id in orgsToGet )
      {
        //Organization Stuff
        var orgDS = Microsoft.ApplicationBlocks.Data.SqlHelper.ExecuteDataset( OrganizationBizService.OrganzationConnection(), System.Data.CommandType.Text, "SELECT * FROM Organization WHERE Id = " + id );
        var dr = orgDS.Tables[0].Rows[0];
        var orgStats = new OrganizationStats()
        {
          Title = ILPathways.DAL.OrganizationManager.GetRowColumn( dr, "Name" ),
          Description = "",
          ImageUrl = ILPathways.DAL.OrganizationManager.GetRowPossibleColumn( dr, "LogoUrl" )
        };

        orgStats.Stats.Add( LoadStat( "Created on", DateTime.Parse( ILPathways.DAL.OrganizationManager.GetRowColumn( dr, "Created" ) ).ToShortDateString() ) );
        //orgStats.Stats.Add( LoadStat( "Members", org.OrgMembers.Count.ToString() ) );
        orgStats.Stats.Add( LoadStat( "Libraries", libraries.Where( m => m.OrgId == id ).Count().ToString() ) );

        //Library Stuff
        foreach( var lib in libraries.Where( m => m.OrgId == id ).ToList() )
        {
          var libStats = new StatObjectDTO()
          {
            Title = lib.Title,
            Description = lib.Description,
            ImageUrl = lib.ImageUrl //lib.imageurl?
          };

          libStats.Stats.Add( LoadStat( "Visits this Month", views.Where( m => m == lib.Id ).Count().ToString() ) );
          libStats.Stats.Add( LoadStat( "Created on", lib.Created.ToShortDateString() ) );
          libStats.Stats.Add( LoadStat( "Members", libService.LibraryMembers_GetAll( lib.Id ).Count.ToString() ) );
          libStats.Stats.Add( LoadStat( "Collections", libService.LibrarySectionsSelectList( lib.Id ).Count.ToString() ) );
          libStats.Stats.Add( LoadStat( "Resources", libService.LibraryResource_SelectAllResourceIdsForLibrary( lib.Id ).Count.ToString() ) );

          //Learning List Stuff
          //ContentServices.get

          orgStats.Libraries.Add( libStats );
        }

        Organizations.Add( orgStats );
      }

      /*
      foreach ( var org in orgs )
      {
        var libs = new List<Business.Library>();
        //libs = libService.

        var orgStats = new OrganizationStats()
        {
          Title = org.Name,
          Description = "", //org.description?
          ImageUrl = "" //org.imageurl?
        };

        orgStats.Stats.Add( LoadStat( "Created on", org.Created.ToShortDateString() ) );
        orgStats.Stats.Add( LoadStat( "Members", org.OrgMembers.Count.ToString() ) );
        orgStats.Stats.Add( LoadStat( "Libraries", libs.Count.ToString() ) );

        foreach ( var lib in libs )
        {
          var libStats = new StatObjectDTO()
          {
            Title = lib.Title,
            Description = lib.Description,
            ImageUrl = lib.ImageUrl //lib.imageurl?
          };

          libStats.Stats.Add( LoadStat( "Created on", lib.Created.ToShortDateString() ) );
          libStats.Stats.Add( LoadStat( "Members", libService.LibraryMembers_GetAll( lib.Id ).Count.ToString() ) );
          libStats.Stats.Add( LoadStat( "Collections", libService.LibrarySectionsSelectList( lib.Id ).Count.ToString() ) );
          libStats.Stats.Add( LoadStat( "Resources", libService.LibraryResource_SelectAllResourceIdsForLibrary( lib.Id ).Count.ToString() ) );
        }
      }

      */

      //Test data

      /*
      for ( var j = 0 ; j < random.Next( 2, 5 ) ; j++ )
      {
        var test = new OrganizationStats()
        {
          Title = "Organization Name Here",
          Description = "This is the organization description. The description of the organization goes here. This space is taken up by the organization's description, which describes this organization.",
          ImageUrl = "/images/icons/icon_community_med.png"
        };
        test.Stats.Add( AddRandomStat() );
        test.Stats.Add( AddRandomStat() );
        test.Stats.Add( AddRandomStat() );

        for ( var i = 0 ; i < random.Next( 4, 10 ) ; i++ )
        {
          var sampleLibrary = new StatObjectDTO()
          {
            Title = "Org Library Title",
            Description = "This is an org library. An org library's description is in this area. This text describes this library.",
            ImageUrl = "/images/icons/icon_library_med.png"
          };
          sampleLibrary.Stats.Add( AddRandomStat() );
          sampleLibrary.Stats.Add( AddRandomStat() );
          sampleLibrary.Stats.Add( AddRandomStat() );
          sampleLibrary.Stats.Add( AddRandomStat() );
          test.Libraries.Add( sampleLibrary );
        }

        for ( var i = 0 ; i < random.Next( 5, 15 ) ; i++ )
        {
          var sampleList = new StatObjectDTO()
          {
            Title = "Learning List Title",
            Description = "Here is some text that describes in moderate detail the learning list contained and referenced herein.",
            ImageUrl = "/images/icons/icon_resources_med.png"
          };
          sampleList.Stats.Add( AddRandomStat() );
          sampleList.Stats.Add( AddRandomStat() );
          sampleList.Stats.Add( AddRandomStat() );
          sampleList.Stats.Add( AddRandomStat() );
          test.LearningLists.Add( sampleList );
        }

        Organizations.Add( test );
      }
      // End Test data
      
      */
    }

    private Stat LoadStat( string title, string value )
    {
      return new Stat()
      {
        Title = title,
        Description = value
      };
    }

    //Test methods
    private Stat AddRandomStat()
    {
      var stats = new List<Stat>()
      {
        new Stat() { Title = "Visitors this month", Description = random.Next(100).ToString() },
        new Stat() { Title = "Other Stat name", Description = "Stat text" },
        new Stat() { Title = "Extra stat", Description = "Longer stat text" },
        new Stat() { Title = "Searches this week", Description = random.Next(1000).ToString() },
        new Stat() { Title = "Other Stat", Description = "Other Text" }
      };

      return stats[ random.Next( stats.Count - 1 ) ];
    }

  }


  //End test methods

  public class OrganizationStats : StatObjectDTO
  {
    public OrganizationStats() {
      Libraries = new List<StatObjectDTO>();
      LearningLists = new List<StatObjectDTO>();
    }
    public List<StatObjectDTO> Libraries { get; set; }
    public List<StatObjectDTO> LearningLists { get; set; }
  }
  public class StatObjectDTO
  {
    public StatObjectDTO()
    {
      Stats = new List<Stat>();
    }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public List<Stat> Stats { get; set; }
  }
  public class Stat
  {
    public string Title { get; set; }
    public string Description { get; set; }
  }
}