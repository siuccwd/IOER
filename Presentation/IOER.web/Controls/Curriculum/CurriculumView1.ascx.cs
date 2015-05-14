using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Ionic.Zip;
using Isle.BizServices;
using AcctManager = Isle.BizServices.AccountServices;
using OrgManager = Isle.BizServices.OrganizationBizService;
using LRWarehouse.Business;
using ILPathways.Business;
using ILPathways.Services;
using ILPathways.Library;
using JSONNode = ILPathways.Services.CurriculumService.JSONNode;
using CurriculumService = ILPathways.Services.CurriculumService;
using System.Web.Script.Serialization;
using Isle.DTO;
using System.Text.RegularExpressions;
using StandardDTO = ILPathways.Services.CurriculumService1.StandardDTO;

namespace ILPathways.Controls.Curriculum
{
  public partial class CurriculumView1 : BaseUserControl
  {
    public CurriculumView1()
    {
      currentNode = new ContentItem();
      curriculumNode = new ContentItem();
      tree = new JSONNode();
      currentNodeParents = new List<Tuple<string, int>>();
      allStandards = new List<StandardDTO>();
      curriculumMapHTML = "";
      userGUID = "";
      filesList = "[]";
      allStandardsList = "[]";
      usingContentStandards = false;
      isWidget = false;
      history = new List<CommentDTO>();
    }

    //Stuff used by this class
    private ContentServices conService = new ContentServices();
    public JavaScriptSerializer serializer = new JavaScriptSerializer();
    public CurriculumServices curriculumService = new CurriculumServices();
    public CurriculumService1 curriculumWebService = new CurriculumService1();

    //Variables sent to client
    public ContentItem currentNode { get; set; }
    public ContentItem curriculumNode { get; set; }
    public JSONNode tree { get; set; }
    public List<Tuple<string, int>> currentNodeParents { get; set; }
    public string curriculumMapHTML { get; set; }
    public string userGUID { get; set; }
    public string filesList { get; set; }
    public List<StandardDTO> allStandards { get; set; }
    public string allStandardsList { get; set; }
    public bool usingContentStandards { get; set; }
    public bool isWidget { get; set; }
    public List<CommentDTO> history { get; set; }
    public ResourceLikeSummary curriculumLikes { get; set; }
    public ResourceLikeSummary nodeLikes { get; set; }
    public object comments { get; set; }
    public string commentsList { get; set; }
    public bool hasFeaturedItem { get; set; }
    public string activityJSON { get; set; }

    protected void Page_Load( object sender, EventArgs e )
    {
      var userID = 0;
      var status = "";
      Patron currentUser = new Patron();

      if ( IsUserAuthenticated() )
      {
        userGUID = WebUser.RowId.ToString();
        userID = WebUser.Id;
        currentUser = (Patron) WebUser;
      }

      curriculumContent.Visible = true;
      error.Visible = false;
      int curriculumID = 0;
      int currentNodeID = 0;
      //Get node ID and derive curriculum ID from it
      try
      {
        currentNodeID = int.Parse( Request.Params[ "node" ] ?? Page.RouteData.Values[ "node" ].ToString() );
        if ( currentNodeID == 0 )
        {
          throw new ArgumentException( "Invalid Node ID. Please double check and try again." );
        }

        //check for caching option, defaults to true (only applicable for curriculum node)
        bool allowCaching = this.GetRequestKeyValue( "allowCaching", true );
        //Get the current node
        currentNode = curriculumService.GetACurriculumNode( currentNodeID, ( Patron ) WebUser, allowCaching );
        if ( currentNode == null || currentNode.Id == 0 )
        {
            throw new ArgumentException( "Invalid Node ID. Please double check and try again." );
        }
        //Get curriculum id
        curriculumID = curriculumService.GetCurriculumIDForNode( currentNode );
        curriculumNode = ( currentNodeID == curriculumID || curriculumID == 0 ) ? currentNode : curriculumService.GetTheCurriculumNode( curriculumID );
        if ( curriculumID == 0 || currentNodeID == curriculumID )
        {
          curriculumNode = currentNode;
          curriculumID = currentNodeID;
        }

        if ( currentNodeID == curriculumID )
        {
            new ActivityBizServices().ContentHit( curriculumNode, currentUser );
        }
        else
        {
            new ActivityBizServices().NodeHit( curriculumNode, currentNode, currentUser );
        }
        //Get tree
        tree = new CurriculumService().GetTreeJSON( curriculumID, userGUID, allowCaching );
      }
      catch( Exception ex )
      {
        curriculumContent.Visible = false;
        error.Visible = true;
        if ( ex is ArgumentException )
        {
          errorMessage.InnerHtml = ex.Message;
        }
        else
        {
          errorMessage.InnerHtml = "Error. Please double check the URL and try again.";
        }
        return;
      }

      //Determine widget status
      isWidget = Request.Path.Contains( "/widget" );

      //Assemble the breadcrumb trail
      AssembleBreadcrumbs( currentNodeID );
      
      //Assemble the HTML for the tree overlay
      curriculumMapHTML = "<ul class=\"layer\">";
      GetMapHTML( tree );
      curriculumMapHTML += "</ul>";

      //Assemble the files list
      AssembleFilesList();

      //Assemble the standards list
      AssembleStandardsList();

      //Get History
      history = curriculumService.Curriculum_GetHistory( curriculumID );

      //Get Likes
      curriculumLikes = curriculumService.Content_GetLikeSummmary( curriculumID, userID, ref status );
      nodeLikes = curriculumID == currentNodeID ? curriculumLikes : curriculumService.Content_GetLikeSummmary( currentNodeID, userID, ref status );

      //Get Comments
      comments = new ILPathways.Services.CurriculumService1().GetComments( currentNodeID, true ).data;
      commentsList = serializer.Serialize( comments );

      //Has featured item
      hasFeaturedItem = !string.IsNullOrWhiteSpace( currentNode.AutoPreviewUrl );

      //Get Activity
      activityJSON = serializer.Serialize(
        new ActivityBizServices().ActivityTotals_LearningLists(
          currentNodeID,
          DateTime.Now.AddDays( -1 * int.Parse( activityDaysAgo.Text ) ),
          DateTime.Now
        ).FirstOrDefault() ?? new Isle.DTO.HierarchyActivityRecord()
      );
    }

    public JSONNode GetFromTree( JSONNode tree, int id )
    {
      if ( tree.id == id ) { return tree; }
      foreach ( var item in tree.children )
      {
        var temp = GetFromTree( item, id );
        if ( temp != null ) { return temp; }
      }

      return null;
    }

    private void AssembleBreadcrumbs( int currentNodeID )
    {
      var current = GetFromTree( tree, currentNodeID );
      if ( current == null || current.id == 0 )
          return;

      bool isTop = false;
      while ( !isTop )
      {
        currentNodeParents.Add( new Tuple<string, int>( current.title, current.id ) );
        if ( current.parentID != 0 )
        {
          current = GetFromTree( tree, current.parentID );
        }
        else
        {
          isTop = true;
        }
      }
      currentNodeParents.Reverse();
    }

    private void AssembleFilesList()
    {
      var filesToSend = new List<object>();
      foreach ( var item in currentNode.ChildItems )
      {
        filesToSend.Add( new { id = item.Id, title = item.Title, url = item.DocumentUrl } );
      }
      filesList = serializer.Serialize( filesToSend );
    }

    private void AssembleStandardsList()
    {
      currentNode.UsingContentStandards = ContentServices.IsUsingContentStandards();
      allStandards = curriculumWebService.GetNodeStandardsData( currentNode, true );
      allStandardsList = serializer.Serialize( allStandards );
    }

    public void GetMapHTML( JSONNode tree )
    {
      //Render the current item
      curriculumMapHTML += template_mapNode.Text
        .Replace( "{id}", tree.id.ToString() )
        .Replace( "{title}", tree.title )
        .Replace( "{url}", GetUrl( curriculumNode.Id, tree.id ) )
        .Replace( "{current}", ( currentNode.Id == tree.id ? "class=\"current\"" : "" ) );
      if ( tree.children.Count() > 0 )
      {
        curriculumMapHTML += "<ul class=\"layer\">";
        //Recurse through children
        foreach ( var item in tree.children )
        {
          GetMapHTML( item );
        }
        curriculumMapHTML += "</ul>";
      }
    }

    public string GetUrl( int curriculumID, int nodeID )
    {
      var pattern = "";
      if ( isWidget )
      {
        pattern = widgetUrlPattern.Text;
      }
      else
      {
        pattern = urlPattern.Text;
      }
      var shortTitle = "";
      try { shortTitle = Regex.Replace( GetFromTree( tree, nodeID ).title, "[^a-zA-Z0-9-()]", "_" ); }
      catch { }
      return pattern
        .Replace( "{curriculumID}", curriculumID.ToString() )
        .Replace( "{nodeID}", nodeID.ToString() )
        .Replace( "{shortTitle}", shortTitle ); //Can't get friendly title without getting every node from the database individually under current circumstances
    }

  }

  
}