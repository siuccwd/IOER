using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Services;
using Isle.BizServices;
using ILPathways.Business;
using ILPathways.Utilities;
using Library = ILPathways.Business.Library;
using Section = ILPathways.Business.LibrarySection;
using LResource = ILPathways.Business.LibraryResource;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;

namespace IOER.Widgets.Library
{
  public partial class Library1 : System.Web.UI.UserControl
  {
    public string libraryData { get; set; }
    UtilityService utilService = new UtilityService();
    LibraryBizService libService = new LibraryBizService();

    /// <summary>
    /// handle page load
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load( object sender, EventArgs e )
    {

      int libraryID = 0;
      List<int> collectionIDs = new List<int>();

      //sender id for sharepoint
      string senderId = FormHelper.GetRequestKeyValue( "SenderId", "" );
      ViewState[ "senderId" ] = senderId;

      //Setup
      libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, "No data", null ) + ";";
      int resourceMax = FormHelper.GetRequestKeyValue( "resourceMax", 10 );
      int collectionMax = FormHelper.GetRequestKeyValue( "collectionMax", 10 );
      bool showingHdr = FormHelper.GetRequestKeyValue( "showHdr", true );
      //bool showingHdr = FormHelper.GetRequestKeyValue( "sh", true );
      bool usingTargetUrl = FormHelper.GetRequestKeyValue( "useTargetUrl", false);
      bool showingCollectionList = FormHelper.GetRequestKeyValue( "showCollections", true);
      string keyword = FormHelper.GetRequestKeyValue( "keyword", "" );

      string collectionsList = FormHelper.GetRequestKeyValue( "collections", "" );
      string collectionName = FormHelper.GetRequestKeyValue( "collectionName", "" );
      int colId = FormHelper.GetRequestKeyValue( "cId", 0 );

      //LoggingHelper.DoTrace( 6, "############## get library" );
      //Get target library and collection IDs
      try
      {
        libraryID = int.Parse( Request.Params[ "library" ] );

        if ( libraryID < 1 )
        {
            libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, "Invalid library parameters", null ) + ";";
            return;
        }

        var lib = libService.Get( libraryID );
        var allCols = libService.LibrarySectionsSelectList( libraryID, 1 );

        if ( colId == 0 && collectionName.Length == 0 )
        {
            var collectionIDsRaw = Request.Params[ "collections" ].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            foreach ( string input in collectionIDsRaw )
            {
                collectionIDs.Add( int.Parse( input ) );
            }
        }
        else if ( collectionName.Length > 0 )
        {
            foreach ( var col in allCols )
            {
                if ( col.Title.ToLower() == collectionName.ToLower() )
                {
                    collectionIDs.Add( col.Id );
                    break;
                }
            }
        }
        else
        {
            collectionIDs.Add( colId );
        }

        if ( libraryID == 0 || (collectionIDs.Count == 0 && colId ==0) )
        {
          throw new Exception();
        }
      //}
      //catch ( Exception ex )
      //{
      //  libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, "Invalid URL parameters", null ) + ";";
      //  return;
      //}

      ////Get data
      //try
      //{

        var cols = new List<Section>();

        var colCount = 1;
        foreach ( int id in collectionIDs )
        {
          if ( colCount > collectionMax )
          {
            break;
          }
          colCount++;

          foreach ( var item in allCols )
          {
            if ( item.Id == id )
            {
              cols.Add( item );
            }
          }
        }

        var output = new WidgetLibCol();
        output.title = lib.Title;
        output.link = LibraryBizService.GetLibraryFriendlyUrl( lib ); ;
        output.showHeader = showingHdr;
        output.showCollections = showingCollectionList;

        foreach ( Section section in allCols )
        {
          var item = new WidgetResource();
		  item.link = string.Format( "/Library/Collection/{0}/{1}/{2}", lib.Id, section.Id, ResourceBizService.FormatFriendlyTitle( section.Title ));
		  //section.FriendlyUrl;
          item.title = section.Title;
          item.thumbURL = section.AvatarURL;
          output.collections.Add( item );
        }

        //LoggingHelper.DoTrace( 6, "############## get resources" );
        string filter = "";
        string keywordFilter = FormatKeyword( keyword );
        if ( keywordFilter.Length > 0 )
            filter += DataBaseHelper.FormatSearchItem( filter, keywordFilter, " AND " );

        List<LibraryResource> resources;
        int pTotalRows = 0;

        foreach ( Section section in cols )
        {
          var col = new WidgetLibCol();
          col.title = section.Title;
          //col.link = section.FriendlyUrl;
		  col.link = string.Format( "/Library/Collection/{0}/{1}/{2}", lib.Id, section.Id, ResourceBizService.FormatFriendlyTitle( section.Title ) );
          if ( filter.Length > 0 )
          {
              // 
              filter += DataBaseHelper.FormatSearchItem( filter, "lib.LibrarySectionId", section.Id, "AND" );
              resources = libService.LibraryResource_SearchList( filter, "", 1, resourceMax, ref pTotalRows );

          }
          else
          {
              resources = libService.LibraryResource_SelectAllResourcesForSection( section.Id );
          }

          int count = 1;
          foreach ( LResource resource in resources )
          {
              if ( count > resourceMax )
            {
              break;
            }
            count++;
            var res = new WidgetResource();

            res.title = resource.Title;
            if ( usingTargetUrl == false )
            {
                if ( resource.ResourceIntId > 0 )
                    res.link = ResourceBizService.FormatFriendlyResourceUrlByResId( resource.SortTitle, resource.ResourceIntId );
                else
                    res.link = ResourceBizService.FormatFriendlyResourceUrlByRvId( resource.SortTitle, resource.ResourceVersionIntId );
            }
            else
            {
                res.link = resource.ResourceUrl;
            }

            res.thumbURL = ResourceBizService.GetResourceThumbnailImageUrl( resource.ResourceUrl, resource.ResourceIntId );  // "//ioer.ilsharedlearning.org/OERThumbs/thumb/" + resource.ResourceIntId + "-thumb.png";
            col.items.Add( res );
          }

          output.items.Add( col );
        }


        libraryData = "var libraryData = " + utilService.ImmediateReturn( output, true, "okay", null ) + ";";
      }
      catch ( Exception ex )
      {
        libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, ex.Message, null ) + ";";
      }
    }

    protected string FormatKeyword( string text )
    {
        string template = "(lr.Title like '{0}'  OR lr.[Description] like '{0}')";
        string keyword = DataBaseHelper.HandleApostrophes( FormHelper.CleanText( text.Trim() ) );
        string keywordFilter = "";

        if ( keyword.Length > 0 )
        {
            keyword = keyword.Replace( "*", "%" );
            if ( keyword.IndexOf( "," ) > -1 )
            {
                string[] phrases = keyword.Split( new char[] { ',' } );
                foreach ( string phrase in phrases )
                {
                    string next = phrase.Trim();
                    if ( next.IndexOf( "%" ) == -1 )
                        next = "%" + next + "%";
                    string where = string.Format( template, next );
                    keywordFilter += DataBaseHelper.FormatSearchItem( keywordFilter, where, "OR" );
                }
            }
            else
            {
                if ( keyword.IndexOf( "%" ) == -1 )
                    keyword = "%" + keyword + "%";

                keywordFilter = string.Format( template, keyword );

            }

            
        }
        return keywordFilter;

    }	//

    //Subclasses
    public class WidgetLibraryBase
    {
      public string title { get; set; }
      public string link { get; set; }
    }
    public class WidgetLibCol : WidgetLibraryBase
    {
      public WidgetLibCol()
      {
        items = new List<object>();
        collections = new List<WidgetResource>();
      }
      public bool showHeader { get; set; }
      public bool showCollections { get; set; }
      public List<object> items { get; set; }
      public List<WidgetResource> collections { get; set; }
    }

    public class WidgetResource : WidgetLibraryBase
    {
      public string thumbURL { get; set; }
    }

  }
}