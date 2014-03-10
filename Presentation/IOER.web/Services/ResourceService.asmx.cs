using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.IO;
using System.Web.Script.Services;

using LRWarehouse.DAL;
using LRWarehouse.Business;
using ILPathways.DAL;
using System.Data;

using DatabaseManager = LRWarehouse.DAL.DatabaseManager;
using PatronManager = LRWarehouse.DAL.PatronManager;

namespace ILPathways.Services
{
    /// <summary>
    /// Summary description for ResourceService
    /// </summary>
    [WebService( Namespace = "http://tempuri.org/" )]
    [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
    [System.ComponentModel.ToolboxItem( false )]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ResourceService : System.Web.Services.WebService
    {
        JavaScriptSerializer serializer;

        public ResourceService()
        {
            serializer = new JavaScriptSerializer();
        }

        #region Communication Methods
        [WebMethod]
        public void Fetch( string widgetName, int vid )
        {
            switch ( widgetName ) //Uses Response.Write to return any kind of widget without needing separate methods
            {
                case "basicInfo":
                    //Return data for Basic Info widget
                    //Title, URL, Description, Requirements
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_BasicInfo( vid ) ) );
                    break;
                case "criticalInfo":
                    //Return data for Critical info widget
                    //Publisher, Creator, Created Date, Usage Rights
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_CriticalInfo( vid ) ) );
                    break;
                case "details":
                    //Return details
                    //checkbox lists and single value items
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_Details( vid ).lists ) );
                    break;
                case "keywords":
                    //Return keywords
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_Keywords( vid ) ) );
                    break;
                case "subjects":
                    //Return subjects
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_Subjects( vid ) ) );
                    break;
                case "morelikethis":
                    //Return More Like This
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_MoreLikeThis( vid ) ) );
                    break;
                case "comments":
                    //Return comment list
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_Comments( vid ) ) );
                    break;
                default:
                    HttpContext.Current.Response.Write( serializer.Serialize( new { d = "Error: Invalid Widget Name" } ) );
                    break;
            }
        }

        [WebMethod]
        public void FetchForUser( string widgetName, int vid, string userGUID )
        {
            switch ( widgetName )
            {
                case "paradata":
                    //Return Likes, Dislikes, and icons
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_Paradata( vid, userGUID ) ) );
                    break;
                case "libraries":
                    //Return a list of libraries for the resource, as well as info about the user's library
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_LibraryInfo( vid, userGUID ) ) );
                    break;
                default:
                    HttpContext.Current.Response.Write( serializer.Serialize( new { d = "Error: Invalid Widget Name" } ) );
                    break;
                case "standards":
                    //Return learning standards
                    HttpContext.Current.Response.Write( serializer.Serialize( Get_LearningStandards( vid, userGUID ) ) );
                    break;

            }
        }

        #endregion

        #region Intra-Server methods
        public bool CanUserEdit( string userGUID )
        {
            Patron user = new PatronManager().GetByRowId( userGUID );
            return SecurityManager.GetGroupObjectPrivileges( user, "ILPathways.LRW.Pages.ResourceDetail" ).CanUpdate();
        }

        public bool IsUserAdmin( Patron user )
        {
            return SecurityManager.GetGroupObjectPrivileges( user, "ILPathways.LRW.Pages.ResourceDetail" ).CreatePrivilege > ( int )ILPathways.Business.EPrivilegeDepth.State;
        }

        public int GetIntIDFromVersionID( int versionID )
        {
            return new ResourceVersionManager().Get( versionID ).ResourceIntId;
        }

        public Widget_FreeWords GetFreeWords( List<ResourceChildItem> data )
        {
            var output = new Widget_FreeWords();
            foreach ( ResourceChildItem item in data )
            {
                output.words.Add( item.OriginalValue );
            }
            return output;
        }
        #endregion

        #region Widget-Specific Methods
        /*  *   *   *   *   *   Widgets   * *   *   *   *   */
        protected Widget_BasicInfo Get_BasicInfo( int versionID )
        {
            var widget = new Widget_BasicInfo();

            ResourceVersion source = new ResourceVersionManager().Get( versionID );
            if ( source != null && source.IsValid )
            {
                widget.title = source.Title;
                widget.url = source.ResourceUrl;
                widget.description = source.Description;
                widget.requirements = source.Requirements;
            }

            return widget;
        }

        protected Widget_CriticalInfo Get_CriticalInfo( int versionID )
        {
            var widget = new Widget_CriticalInfo();
            ResourceVersion source = new ResourceVersionManager().Get( versionID );
            if ( source != null && source.IsValid )
            {
                widget.creator = source.Creator;
                widget.publisher = source.Publisher;
                widget.created = source.Created.ToShortDateString();
                widget.rightsURL = source.Rights;
            }

            return widget;
        }

        public Widget_Details Get_Details( int versionID )
        {
            var output = new Widget_Details();

            List<jsonCheckboxList> lists = new List<jsonCheckboxList>();

            string[] tables = new string[] { "assessmentType", "accessRights", "careerCluster", "educationalUse", "mediaType", "gradeLevel", "groupType", "endUser", "itemType", "language", "resourceType" };
            string[] titles = new string[] { "Assessment Type", "Access Rights", "Career Cluster", "Educational Use", "Media Type", "Grade Level", "Group Type", "End User", "Item Type", "Language", "Resource Type" };

            //Get the Resource Int ID
            int intID = GetIntIDFromVersionID( versionID );

            //Load the data for that ID
            var dataManager = new ResourceDataManager();
            for ( int i = 0; i < tables.Length; i++ )
            {
                DataSet ds = dataManager.SelectedCodes( ResourceDataManager.ResourceDataSubclassFinder.getSubclassByName( tables[ i ] ), intID );

                //Convert it into the JSON object
                var list = new jsonCheckboxList();
                list.title = titles[ i ];
                list.name = tables[ i ];
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    var item = new jsonCBXLItem();
                    item.id = int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) );
                    item.selected = bool.Parse( DatabaseManager.GetRowColumn( dr, "IsSelected" ) );
                    item.title = DatabaseManager.GetRowColumn( dr, "Title" );
                    list.items.Add( item );
                }

                //Return the JSON object
                lists.Add( list );
            }

            output.lists = lists;

            return output;
        }

        protected Widget_FreeWords Get_Keywords( int versionID )
        {
            List<ResourceChildItem> data = new ResourceKeywordManager().Select( GetIntIDFromVersionID( versionID ) );
            return GetFreeWords( data );
        }

        protected Widget_FreeWords Get_Subjects( int versionID )
        {
            List<ResourceChildItem> data = new ResourceSubjectManager().Select( GetIntIDFromVersionID( versionID ) );
            return GetFreeWords( data );
        }

        protected Widget_MoreLikeThis Get_MoreLikeThis( int versionID )
        {
            return new Widget_MoreLikeThis() { result = new ElasticSearchManager().GetByVersionID( versionID ) };
        }

        protected Widget_Comments Get_Comments( int versionID )
        {
            var output = new Widget_Comments();
            var manager = new ResourceCommentManager();
            var intID = GetIntIDFromVersionID( versionID );
            DataSet ds = manager.Select( intID );
            if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
            {
                foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                {
                    var comment = new jsonWidgetComment();
                    comment.name = DatabaseManager.GetRowColumn( dr, "CreatedBy" );
                    comment.commentID = int.Parse( DatabaseManager.GetRowColumn( dr, "Id" ) );
                    comment.commentDate = DateTime.Parse( DatabaseManager.GetRowColumn( dr, "Created" ) ).ToShortDateString();
                    comment.commentText = DatabaseManager.GetRowColumn( dr, "Comment" );
                    comment.avatarURL = ""; //One day, this will be useful
                    output.comments.Add( comment );
                }
            }
            return output;
        }

        protected Widget_Paradata Get_Paradata( int versionID, string userGUID )
        {
            var output = new Widget_Paradata();
            var intID = GetIntIDFromVersionID( versionID );
            string status = "";
            var user = new PatronManager().GetByRowId( userGUID );
            if ( user == null ) { return null; }
            var summary = new ResourceLikeSummaryManager().GetForDisplay( intID, user.Id, ref status );
            if ( summary.YouLikeThis )
            {
                output.iLikeThis = true;
                output.iDislikeThis = false;
            }
            else if ( summary.YouDislikeThis )
            {
                output.iLikeThis = false;
                output.iDislikeThis = true;
            }
            else
            {
                output.iLikeThis = false;
                output.iDislikeThis = false;
            }

            output.likes = summary.LikeCount;
            output.dislikes = summary.DislikeCount;

            return output;
        }

        protected Widget_LibraryInfo Get_LibraryInfo( int versionID, string userGUID )
        {
            //Setup
            var output = new Widget_LibraryInfo();
            var intID = GetIntIDFromVersionID( versionID );
            string status = "";
            var libraryManager = new Isle.BizServices.LibraryBizService();
            var user = new PatronManager().GetByRowId( userGUID );

            //Check to see if the resource is in your library
            output.isInMyLibrary = libraryManager.IsResourceInLibrary( user, intID );

            //Get info about libraries that contain the resource
            var flats = new ResourceJSONManager().GetJSONFlatByIntID( intID );
            foreach ( ResourceJSONFlat flat in flats ) //Should only be one
            {
                foreach ( int item in flat.collectionIDs )
                {
                    var info = new jsonLibraryInfo();
                    var tempCollection = libraryManager.LibrarySectionGet( item );
                    var tempLibrary = libraryManager.Get( tempCollection.LibraryId );
                    if ( tempCollection.IsPublic && tempLibrary.IsPublic )
                    {
                        info.libraryID = tempLibrary.Id;
                        info.collectionID = tempCollection.Id;
                        info.libraryName = tempLibrary.Title;
                        info.collectionName = tempCollection.Title;
                        info.libraryAvatarURL = tempLibrary.ImageUrl;
                        output.inLibraries.Add( info );
                    }

                }
            }

            //Get user's collection info
            if ( user.IsValid && user.Id != 0 )
            {
                var myCollections = libraryManager.LibrarySectionsSelectList( libraryManager.GetMyLibrary( user ).Id, 2 );
                foreach ( Business.LibrarySection collection in myCollections )
                {
                    var item = new jsonMyCollection();
                    item.id = collection.Id;
                    item.name = collection.Title;
                    output.myCollections.Add( item );
                }
            }

            return output;
        }

        protected Widget_LearningStandards Get_LearningStandards( int versionID, string userGUID )
        {
            var output = new Widget_LearningStandards();
            var standardManager = new ResourceStandardManager();
            var evaluationManager = new ResourceEvaluationManager();
            var intID = GetIntIDFromVersionID( versionID );
            var ratings = standardManager.Select( intID );
            var user = new PatronManager().GetByRowId( userGUID );

            ResourceStandardCollection collection = standardManager.Select( intID );
            foreach ( ResourceStandard standard in collection )
            {
                //Setup
                string status = "";
                LearningStandard item = new LearningStandard();

                //Basic, standalone data
                item.alignmentType = ( standard.AlignmentTypeValue == "" ? "Aligns to" : standard.AlignmentTypeValue );
                item.standardNotationCode = standard.StandardNotationCode;
                item.standardURL = standard.StandardUrl;
                item.standardID = standard.StandardId;
                item.description = standard.StandardDescription;
                item.myRating = -1;
                item.ratingCount = 0;

                //Paradata
                DataSet ds = evaluationManager.Select( intID, 0, standard.StandardId, 0, ref status );
                double tempCount = 0;
                if ( DatabaseManager.DoesDataSetHaveRows( ds ) )
                {
                    item.ratingCount = ds.Tables[ 0 ].Rows.Count;
                    foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
                    {
                        var value = DatabaseManager.GetRowPossibleColumn( dr, "Value" );
                        if ( value == "" ) { value = "0"; }
                        tempCount += int.Parse( value );
                        if ( DatabaseManager.GetRowPossibleColumn( dr, "CreatedById" ) == user.Id.ToString() )
                        {
                            item.myRating = ( int )tempCount;
                        }
                    }
                }
                if ( item.ratingCount > 0 )
                {
                    double tempRatingCount = ( double )item.ratingCount;
                    double average = tempCount / item.ratingCount; //Get the average
                    double percentage = average / 3; //Get the percentage of max score (0-3)
                    item.communityRating = percentage;
                }
                else
                {
                    item.communityRating = -1;
                }

                output.standards.Add( item );

            }

            return output;
        }
    }
        #endregion

    //Basic Info
    public class Widget_BasicInfo
    {
        public string title { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string requirements { get; set; }
    }

    //Critical Info
    public class Widget_CriticalInfo
    {
        public string publisher { get; set; }
        public string creator { get; set; }
        public string created { get; set; }
        public string rightsURL { get; set; }
    }

    //Details
    public class Widget_Details
    {
        public List<jsonCheckboxList> lists { get; set; }
    }
    public class jsonCheckboxList
    {
        public jsonCheckboxList()
        {
            items = new List<jsonCBXLItem>();
        }
        public string title { get; set; }
        public string name { get; set; }
        public List<jsonCBXLItem> items { get; set; }
    }
    public class jsonCBXLItem
    {
        public string title { get; set; }
        public int id { get; set; }
        public bool selected { get; set; }
    }

    //Keywords and Subjects
    public class Widget_FreeWords
    {
        public Widget_FreeWords()
        {
            words = new List<string>();
        }
        public List<string> words;
    }

    //More Like This
    public class Widget_MoreLikeThis
    {
        public string result { get; set; }
    }

    //Comments
    public class Widget_Comments
    {
        public Widget_Comments()
        {
            comments = new List<jsonWidgetComment>();
        }
        public List<jsonWidgetComment> comments;
    }

    //Paradata
    public class Widget_Paradata
    {
        public bool iLikeThis { get; set; }
        public bool iDislikeThis { get; set; }
        public int likes { get; set; }
        public int dislikes { get; set; }
        //Other paradata items are handled by the external system, at least for now 
    }
    public class jsonWidgetComment
    {
        public string name { get; set; }
        public int commentID { get; set; }
        public string avatarURL { get; set; }
        public string commentDate { get; set; }
        public string commentText { get; set; }
    }

    //Libraries
    public class Widget_LibraryInfo
    {
        public Widget_LibraryInfo()
        {
            inLibraries = new List<jsonLibraryInfo>();
            myCollections = new List<jsonMyCollection>();
        }
        public List<jsonLibraryInfo> inLibraries { get; set; }
        public List<jsonMyCollection> myCollections { get; set; }
        public bool isInMyLibrary { get; set; }
    }
    public class jsonLibraryInfo
    {
        public string libraryName { get; set; }
        public string collectionName { get; set; }
        public int libraryID { get; set; }
        public int collectionID { get; set; }
        public string libraryAvatarURL { get; set; }
    }
    public class jsonMyCollection
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    //Learning Standards
    public class Widget_LearningStandards
    {
        public Widget_LearningStandards()
        {
            standards = new List<LearningStandard>();
        }
        public List<LearningStandard> standards { get; set; }
    }
    public class LearningStandard
    {
        public int standardID { get; set; }
        public string standardNotationCode { get; set; }
        public string standardURL { get; set; }
        public double communityRating { get; set; }
        public int ratingCount { get; set; }
        public int myRating { get; set; }
        public string alignmentType { get; set; }
        public string description { get; set; }
    }
}
