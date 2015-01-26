using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;

using ILPathways.Services;
using Isle.BizServices;
using ILPathways.Business;
using ILPathways.Utilities;
using Library = ILPathways.Business.Library;
using Section = ILPathways.Business.LibrarySection;
using Resource = ILPathways.Business.LibraryResource;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;

namespace ILPathways.Widgets.Collection
{
    public partial class Default : System.Web.UI.Page
    {


        UtilityService utilService = new UtilityService();
        LibraryBizService libService = new LibraryBizService();

        public static string libraryData { get; set; }
        public static string libraryIdString { get; set; }
        public static string collectionIdString { get; set; }
        public static string resourceMaxString { get; set; }
        public static string usingTargetUrlString { get; set; }
        public static string keywordString { get; set; }



        public string SenderId {
            get { return txtSenderId.Text; }
            set {txtSenderId.Text = value;}
        }
        public int libraryID
        {
            get { return Int32.Parse( txtLibraryId.Text ); }
            set { txtLibraryId.Text = value.ToString(); }
        }
        public int colId
        {
            get { return Int32.Parse( txtCollectionId.Text ); }
            set { txtCollectionId.Text = value.ToString(); }
        }
        public int resourceMax
        {
            get { return Int32.Parse( txtResourceMax.Text); }
            set { txtResourceMax.Text = value.ToString(); }
        }
        public int collectionMax
        {
            get { return Int32.Parse( txtCollectionMax.Text ); }
            set { txtCollectionMax.Text = value.ToString(); }
        }

         public bool showingHdr
        {
            get { return bool.Parse( txtShowingHdr.Text); }
            set { txtShowingHdr.Text = value.ToString(); }
        }
         public bool usingTargetUrl
         {
             get { return bool.Parse( txtUsingTargetUrl.Text ); }
             set { txtUsingTargetUrl.Text = value.ToString(); }
         }
         public bool showingCollectionList
         {
             get { return bool.Parse( txtShowingCollectionList.Text ); }
             set { txtShowingCollectionList.Text = value.ToString(); }
         }
         public string keyword
         {
             get { return txtKeyword.Text; }
             set { txtKeyword.Text = value; }
         }
         public string collectionsList
         {
             get { return txtCollectionsList.Text; }
             set { txtCollectionsList.Text = value; }
         }
         public string collectionName
         {
             get { return txtCollectionName.Text; }
             set { txtCollectionName.Text = value; }
         }

        /// <summary>
        /// handle page load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( !IsPostBack )
            {
                InitializeForm();

            }

        }//
        protected void InitializeForm()
        {

            //int libraryID = 0;
          

            //sender id for sharepoint
            SenderId = FormHelper.GetRequestKeyValue( "SenderId", "" );
            ViewState[ "senderId" ] = SenderId;

            //Setup
            libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, "No data", null ) + ";";
            resourceMax = FormHelper.GetRequestKeyValue( "resourceMax", 10 );
            resourceMaxString = "var resourceMax = " + resourceMax.ToString() + ";";
            usingTargetUrl = FormHelper.GetRequestKeyValue( "useTargetUrl", false );
            usingTargetUrlString = "var usingTargetUrl = '" + usingTargetUrl.ToString().ToLower() + "';";
            keyword = FormHelper.GetRequestKeyValue( "keyword", "" );
            keywordString = "var keyword = " + keyword.ToString() + ";";
            colId = FormHelper.GetRequestKeyValue( "cId", 0 );
            collectionIdString = "var colId = " + colId.ToString() + ";";

            collectionMax = FormHelper.GetRequestKeyValue( "collectionMax", 10 );
            showingHdr = FormHelper.GetRequestKeyValue( "showHdr", true );
            showingCollectionList = FormHelper.GetRequestKeyValue( "showCollections", true );


            collectionsList = FormHelper.GetRequestKeyValue( "collections", "" );
            collectionName = FormHelper.GetRequestKeyValue( "collectionName", "" );


            libraryID = FormHelper.GetRequestKeyValue( "library", 0 );
            libraryIdString = "var libraryID = " + libraryID.ToString() + ";";

            if ( libraryID < 1 )
            {
                //libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, "Invalid library parameters", null ) + ";";
                return;
            }
            LoadCollection();
            //if ( Request.Form[ "__EVENTTARGET" ] != null && Request.Form[ "__EVENTTARGET" ] == "collectionMsg" )
            //{
            //    string key = Request.Form[ "__EVENTARGUMENT" ].ToString();
            //    if ( key != null && key.Trim().Length > 0 )
            //        collectionName = key;
            //}
        }//
        protected void LoadCollection()
        {
            //LoggingHelper.DoTrace( 6, "############## get library" );
            List<int> collectionIDs = new List<int>();

            //Get target library and collection IDs
            try
            {
                
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

                if ( libraryID == 0 || ( collectionIDs.Count == 0 && colId == 0 ) )
                {
                    throw new Exception();
                }
                

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
                output.link = lib.FriendlyUrl;
                output.showHeader = showingHdr;
                output.showCollections = showingCollectionList;

                foreach ( Section section in allCols )
                {
                    var item = new WidgetResource();
                    item.link = section.FriendlyUrl;
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
                    col.link = section.FriendlyUrl;
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
                    foreach ( Resource resource in resources )
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
        
        [WebMethod]
        public static string GetCollection( int libraryId, string collectionName, int resourceMax, bool usingTargetUrl )
        {
            LoggingHelper.DoTrace( 4, "GetCollection" );
            string resourcesJson = "";
            bool showingHdr = false;
            bool showingCollectionList = false;
            string keyword = "";
            //int resourceMax = 10;
            //bool usingTargetUrl = true;

            UtilityService utilService = new UtilityService();
            LibraryBizService libService = new LibraryBizService();
            List<int> collectionIDs = new List<int>();
            int colId = 0;

            //Setup
            libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, "No data", null ) + ";";
           
            //LoggingHelper.DoTrace( 6, "############## get library" );
            //Get target library and collection IDs
            try
            {

                if ( libraryId < 1 || collectionName.Trim().Length == 0 )
                {
                    libraryData = "var libraryData = " + utilService.ImmediateReturn( "", false, "Invalid library parameters", null ) + ";";
                    return "error";
                }

                LoggingHelper.DoTrace( 4, "GetCollection - get library" );
                var lib = libService.Get( libraryId );
                var allCols = libService.LibrarySectionsSelectList( libraryId, 1 );

                if ( collectionName.Length > 0 )
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

                var cols = new List<Section>();

                var colCount = 1;
                foreach ( int id in collectionIDs )
                {
                    if ( colCount > 10 )
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
                output.link = lib.FriendlyUrl;
                output.showHeader = showingHdr;
                output.showCollections = showingCollectionList;

                foreach ( Section section in allCols )
                {
                    var item = new WidgetResource();
                    item.link = section.FriendlyUrl;
                    item.title = section.Title;
                    item.thumbURL = section.AvatarURL;
                    output.collections.Add( item );
                }

                LoggingHelper.DoTrace( 6, "############## get resources" );
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
                    col.link = section.FriendlyUrl;
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
                    foreach ( Resource resource in resources )
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

                LoggingHelper.DoTrace( 4, "##### GetCollection - done" );

                resourcesJson = utilService.ImmediateReturn( output, true, "okay", null );
                libraryData = "var libraryData = " + resourcesJson + ";";
            }
            catch ( Exception ex )
            {
                resourcesJson = utilService.ImmediateReturn( "", false, ex.Message, null );
                libraryData = "var libraryData = " + resourcesJson + ";";
            }
            return resourcesJson;
        }


        protected static string FormatKeyword( string text )
        {
            string template = "(lr.Title like '{0}'  OR lr.[Description] like '{0}')";
            string keyword = DataBaseHelper.HandleApostrophes( FormHelper.SanitizeUserInput( text.Trim() ) );
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