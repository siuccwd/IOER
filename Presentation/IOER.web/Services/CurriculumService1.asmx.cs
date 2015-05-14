using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using LRWarehouse.Business;
using Isle.BizServices;
using ILPathways.Business;
using JSON = ILPathways.Services.UtilityService.GenericReturn;
using System.IO;
using ILPathways.Utilities;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for Curriculum1
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class CurriculumService1 : System.Web.Services.WebService
  {
    CurriculumServices curriculumService = new CurriculumServices();
    ContentServices contentService = new ContentServices();

    #region Get Methods
    //Get comments for a node
    [WebMethod]
    public JSON GetComments( int nodeID, bool reverse )
    {
      //Return comments
      var data = curriculumService.Content_GetComments( nodeID );
      if ( !reverse )
      {
        data.Reverse();
      }
      var returner = new List<object>();
      foreach ( var item in data )
      {
        returner.Add( new
        {
          id = item.Id,
          user = AccountServices.GetUser(item.CreatedById).FullName(), //optimize this later
          text = item.Comment,
          date = item.Created.ToShortDateString(),
        } );
      }
      return Reply( returner, true, "okay", null );
    }

    //Get likes for a node
    [WebMethod]
    public JSON GetLikes( int nodeID )
    {
      //Get user if available
      var user = GetValidatedUser( nodeID, 0 ).user;
      if ( user == null )
      {
        user = new Patron();
      }

      var status = "";

      //Return likes
      try
      {
        var data = curriculumService.Content_GetLikeSummmary( nodeID, user.Id, ref status );
        if ( data.YouLikeThis )
        {
          return Reply( null, false, "You already like this.", "alreadyLike" );
        }
        else
        {
          return Reply( data.LikeCount, true, "okay", null );
        }
      }
      catch ( Exception ex )
      {
        return Reply( null, false, status, ex.Message );
      }
    }
    
    //Get Subscriptions for a node
    [WebMethod( EnableSession = true )]
    public JSON GetSubscription( int nodeID )
    {
      //Validate user
      var user = GetValidatedUser( nodeID, 0 ).user;
      if(user == null)
      {
        return Reply( null, false, "Invalid User.", null );
      }

      //Return subscription
      var data = curriculumService.ContentSubscriptionGet( nodeID, user.Id );
      var toReturn = new
      {
        type = data.SubscriptionTypeId
      };
      return Reply( toReturn, true, "okay", null );
    }

    //Get Standards for a content item
    [WebMethod]
    public JSON GetContentStandards( int contentID )
    {
      return Reply( GetContentStandardsData( contentID ), true, "okay", null );
    }
    public List<StandardDTO> GetContentStandardsData( int contentID )
    {
      var data = CurriculumServices.ContentStandard_Select( contentID );
      var output = new List<StandardDTO>();
      foreach ( var item in data )
      {
        output.Add( GetStandardDTO( item, true ) );
      }
      output.Sort( delegate( StandardDTO x, StandardDTO y ) { return string.Compare(x.code, y.code) > 0 ? 1 : -1; } ); //Not sure this has any effect
      return output;
    }
    //Get standards for a node -- hope to phase this out, only used in the curriculum viewer
    [WebMethod]
    public JSON GetNodeStandards( int nodeID, bool includeChildStandards )
    {
      var data = curriculumService.GetACurriculumNode( nodeID );
      return Reply( GetNodeStandardsData( data, includeChildStandards ), true, "", null );
    }
    public List<StandardDTO> GetNodeStandardsData( ContentItem node, bool includeChildStandards )
    {
      var output = new List<StandardDTO>();
      if ( node.UsingContentStandards )
      {
        foreach ( var item in ( includeChildStandards ? node.ContentStandards.Concat( node.ContentChildrenStandards ) : node.ContentStandards ) )
        {
          output.Add( GetStandardDTO( item, true ) );
        }
      }
      else
      {
        foreach ( var item in ( includeChildStandards ? node.Standards.Concat( node.ChildrenStandards ) : node.Standards ) )
        {
          output.Add( GetStandardDTO( item, false ) );
        }
      }
      
      return output;
    }

    //Get a tree for a curriculum in JSTree format
    [WebMethod(EnableSession=true)]
    public JSON GetJSTree( int nodeID )
    {
      var raw = curriculumService.GetCurriculumOutlineForEdit( nodeID );
      var list = new List<JSTreeNode>();
      FlattenJSTreeNodes( raw, ref list );
      return Reply( list, true, "okay", null );
    }
    private void FlattenJSTreeNodes( Isle.DTO.ContentNode node, ref List<JSTreeNode> runningList )
    {
      runningList.Add( new JSTreeNode() { 
        id = node.Id, 
        text = node.Title, 
        parent = node.ParentId, 
        //a_attr = new { href = "/testing/placeholder.aspx?node=" + node.Id } 
        a_attr = new { href = "/my/learninglist/" + node.Id },
        li_attr = new { sortOrder = node.SortOrder }
      });
      foreach ( var item in node.ChildNodes )
      {
        FlattenJSTreeNodes( item, ref runningList );
      }
    }

    //Get attachments for a node
    [WebMethod(EnableSession=true)]
    public JSON GetAttachments( int nodeID )
    {
      //Get user if available
      var user = GetValidatedUser( nodeID, 0 ).user;
      var userID = user == null ? 0 : user.Id;
      //var node = curriculumService.GetACurriculumNode( nodeID, userID );
      var node = curriculumService.GetCurriculumNodeForEdit( nodeID, user );
      return GetAttachments( node );
    }
    enum ContentTypes { document = 40, url = 41 };
    public JSON GetAttachments( ContentItem node )
    {
      var list = new List<AttachmentDTO>();
      foreach ( var item in node.ChildItems.OrderBy( m => m.SortOrder ).ThenBy( m => m.Id ).ToList() )
      {
        list.Add( new AttachmentDTO()
        {
          attachmentID = item.Id,
          title = item.Title,
          accessID = item.PrivilegeTypeId,
          url = item.DocumentUrl,
          featured = item.SortOrder <= 0,
          attachmentType = ( ( ContentTypes ) item.TypeId ).ToString(),
          standards = GetContentStandardsData( item.Id )
        } );
      }
      return Reply( list, true, "okay", null );
    }
    #endregion

    #region Post Methods
    //Create a curriculum
    public JSON Curriculum_Create( string title, string description, int organizationID )
    {
      //Validate the user
      var user = GetValidatedUser( 0, 0 ).user;
      if ( user == null )
      {
        return Fail( "You must login to create a Learning List." );
      }

      //Create the node
      var topNode = new ContentItem()
      {
        Title = title,
        Description = description,
        Summary = description,
        Created = DateTime.Now,
        CreatedById = user.Id,
        LastUpdated = DateTime.Now,
        LastUpdatedById = user.Id,
        IsActive = true,
        //TODO - ??should NOT default to published?????
        StatusId = ContentItem.PUBLISHED_STATUS,
        TypeId = 50,
        PrivilegeTypeId = 1,
        OrgId = organizationID
      };

      //Save the changes
      var status = "";
      var newID = curriculumService.Create_ef( topNode, ref status );

      if ( newID == 0 )
      {
        return Fail( status );
      }
      else
      {
        return Reply( newID, true, status, null );
      }

    }

    //Publish the curriculum
    [WebMethod( EnableSession = true )]
    public JSON Curriculum_Publish( int curriculumID )
    {
      //Get the top level node
      var topNode = curriculumService.Get( curriculumID );

      //Validate the user
      var permissions = GetValidatedUser( curriculumID, topNode.CreatedById );
      var user = permissions.user;
      if ( user == null )
      {
        return Fail( "You must login to publish a Learning List." );
      }

      //Check permissions
      if ( !permissions.write ) //Not sure how best to check this
      {
        return Fail( "You don't have permission to do that." );
      }

      //Create the resource
      var newRes = new Resource()
      {
        ResourceUrl = "http://ioer.ilsharedlearning.org/learninglist/" + curriculumID + "/" + ResourceBizService.FormatFriendlyTitle( topNode.Title ),
        CreatedById = user.Id,
        LastUpdatedById = user.Id
      };
      var newVersion = new ResourceVersion()
      {
        ResourceUrl = newRes.ResourceUrl,
        CreatedById = newRes.CreatedById,
        LastUpdatedById = newRes.LastUpdatedById,
        Title = topNode.Title,
        Description = topNode.Description,
        Creator = user.FullName(),
        Publisher = "ISLE OER",
        Submitter = user.FullName(),
        IsActive = true
      };

      //Add English
      //TODO: Test this
      //newRes.Language = new List<ResourceChildItem>() { new ResourceChildItem() { Id = 1, CreatedById = user.Id } };

      newRes.Version = newVersion;
      
      var status = "";
      var valid = true;

      var versionID = 0;
      var intID = 0;
      var sortTitle = "";

      //Do the publish
        //15-04-13 mparsons - added use of publish to org (established when creating a new LL)
      PublishingServices.PublishToAll( newRes
                        , ref valid
                        , ref status
                        , ref versionID
                        , ref intID
                        , ref sortTitle
                        , true
                        , false
                        , user
                        , topNode.OrgId );

      if ( valid )
      {
        //Update the node
        topNode.StatusId = ContentItem.PUBLISHED_STATUS;
        topNode.ResourceIntId = intID;
        curriculumService.Update( topNode );

        //publish the sub-nodes
        var tree = curriculumService.GetCurriculumOutlineForEdit( curriculumID );
        foreach ( var item in tree.ChildNodes )
        {
          SetNodePublished( item );
        }

        //Replace the thumbnail
        if ( !string.IsNullOrWhiteSpace( topNode.ImageUrl ) && UtilityManager.GetAppKeyValue("envType") != "dev" )
        {
          var thumbnailFolder = UtilityManager.GetAppKeyValue( "serverThumbnailFolder", @"\\OERDATASTORE\OerThumbs\large\" );
          File.WriteAllBytes( thumbnailFolder + intID + "-large.png", File.ReadAllBytes( thumbnailFolder + topNode.RowId + ".png" ) );
        }

        return Reply( intID, true, "okay", null );
      }
      else
      {
        return Fail( status );
      }
    }
    private void SetNodePublished( Isle.DTO.ContentNode item )
    {
      var node = curriculumService.Get( item.Id );

      node.StatusId = ContentItem.PUBLISHED_STATUS;
      curriculumService.Update( node );
      foreach ( var subItem in item.ChildNodes )
      {
        SetNodePublished( subItem );
      }
    }

    //Create a node
    [WebMethod( EnableSession = true )]
    public JSON Node_Create( int curriculumID, int nodeID )
    {
      return Save_Properties( curriculumID, 0, nodeID, "New Level", "", "", 1, null, null );
    }

    //Delete a node
    [WebMethod( EnableSession = true )]
    public JSON Node_Delete( int nodeID )
    {
      //Get the node
      var node = curriculumService.Get( nodeID );

      //Validate the user
      var permissions = GetValidatedUser( nodeID, node.CreatedById );
      var user = permissions.user;
      if ( user == null )
      {
        return Fail( "You must login to do that." );
      }

      //Validate permissions
      if ( !permissions.write ) //Not sure how best to do this
      {
        return Fail( "You don't have permission to do that." );
      }

      //Get the node's parent ID
      var returnID = node.ParentId;
      var deletingTopLevelNode = false;
      if ( returnID == 0 || node.Id == curriculumService.GetCurriculumIDForNode( node ) )
      {
        //Handle deleting curriculum node
        returnID = 0;
        deletingTopLevelNode = true;

        //Remove from search index
        if ( node.HasResourceId() && node.ResourceIntId != 0 )
        {
          new LRWarehouse.DAL.ElasticSearchManager().DeleteResource( node.ResourceIntId );
        }
        //return Fail( "You can't delete that level from here." ); //Now allowing delete of top level node
      }

      //Delete the node
      var valid = true;
      var status = "";
      valid = curriculumService.Delete( node.Id, ref status );

      return Reply( returnID, valid, status, deletingTopLevelNode );
    }

    //Reposition a node
    [WebMethod( EnableSession = true )]
    public JSON Node_Move( int nodeID, int targetParentID, int targetSortOrder, int targetSwapNodeID )
    {
      //Validate the user
      var user = GetValidatedUser( nodeID, 0 ).user;
      if ( user == null )
      {
        return Fail( "You must login to do that." );
      }

      //Move the node
      try
      {
        //Get the node to be moved
        var node = curriculumService.Get( nodeID );

        //Move things around
        var oldSortOrder = node.SortOrder;
        node.ParentId = targetParentID == -1 ? node.ParentId : targetParentID;
        node.SortOrder = targetSortOrder == -1 ? node.SortOrder : targetSortOrder;
        node.LastUpdatedById = user.Id;
        curriculumService.Update( node );

        //Swap the other node's position if needed
        if ( targetSwapNodeID != -1 )
        {
          var swapNode = curriculumService.Get( targetSwapNodeID );
          swapNode.SortOrder = oldSortOrder;
          swapNode.LastUpdatedById = user.Id;
          curriculumService.Update( swapNode );
        }

        //Reorder sort orders //if needed
        //if ( targetSortOrder == -1 )
        //{
          var siblings = curriculumService.GetCurriculumOutlineForEdit( node.ParentId ).ChildNodes.OrderBy( m => m.SortOrder ).ToList();
          var newOrder = 10;
          foreach ( var item in siblings )
          {
            //Not sure if lastupdatedbyid should be set here
            var temp = curriculumService.Get( item.Id );
            temp.SortOrder = newOrder;
            curriculumService.Update( temp );
            newOrder = newOrder + 10;
          }
        //}

        return Reply( node.Id, true, "okay", targetSwapNodeID );
      }
      catch ( Exception ex )
      {
        return Fail( ex.Message );
      }
    }

    //Post a Comment
    [WebMethod( EnableSession = true )]
    public JSON Comment( string text, int nodeID )
    {
      //Validate the user
      var user = GetValidatedUser(nodeID, 0).user;
      if ( user == null )
      {
        return Fail( "You must login to comment." );
      }

      //Post the comment
      try
      {
        var status = "";
        var valid = true;
        //Validate the comment
        text = new UtilityService().ValidateText(text, 10, "Comment", ref valid, ref status);
        if ( !valid ) { return Fail( status ); }

        status = "";
        //Post the comment
        try
        {
          curriculumService.Content_AddComment( nodeID, text, user.Id, ref status );
        }
        catch ( Exception ex )
        {
          return Reply( null, false, status, ex.Message );
        }

        //Fetch the updated comments list
        return GetComments( nodeID, true );
      }
      catch( Exception ex )
      {
        return Reply( null, false, "There was an error while posting your comment.", ex.Message );
      }
    }

    //Add a like
    [WebMethod( EnableSession = true )]
    public JSON Like( int nodeID )
    {
      //Validate the user
      var user = GetValidatedUser( nodeID, 0 ).user;
      if ( user == null ) 
      { 
        return Fail( "You must login to like this." ); 
      }

      //Add the like if it hasn't already been added
      try
      {
        curriculumService.Content_AddLike( nodeID, user.Id, true );
        return GetLikes( nodeID );
      }
      catch ( Exception ex )
      {
        return Reply( null, false, "There was an error while posting your Like.", ex.Message );
      }

    }

    //Create or update a subscription
    [WebMethod( EnableSession = true )]
    public JSON UpdateSubscription( int nodeID, int type )
    {
      //Validate the user
      var user = GetValidatedUser(nodeID, 0).user;
      if ( user == null ) 
      { 
        return Reply( "", false, "You must login to subscribe.", null ); 
      }

      //Update the option
      string status = "";
      try {
        var subscription = curriculumService.ContentSubscriptionGet( nodeID, user.Id );
        if ( subscription != null && subscription.Id > 0 )
        {
          if ( type > 0 )
          {
            var success = curriculumService.ContentSubScription_Update( subscription.Id, type, ref status );
            return Reply( success, success, status, null );
          }
          else
          {
            var success = curriculumService.ContentSubscription_Delete( subscription.Id, ref status );
            return Reply( success, success, status, null );
          }
        }
        else
        {
          if ( type > 0 )
          {
            var success = curriculumService.ContentSubScription_Create( nodeID, user.Id, type, ref status );
            return Reply( success, success > 0, status, null );
          }
          else
          {
            return Reply( true, true, "", null );
          }
        }
      }
      catch ( Exception ex )
      {
        return Reply( null, false, status, ex.Message );
      }
    }

    //Save a node (create or update)'s properties
    [WebMethod( EnableSession = true )]
    public JSON Save_Properties( int curriculumID, int nodeID, int parentID, string title, string summary, string timeframe, int accessID, List<int> k12SubjectIDs, List<int> gradeLevelIDs )
    {
      var valid = true;
      var status = "";
      //Fetch the curriculum
      var curriculumNode = curriculumService.GetACurriculumNode( curriculumID );
      var existingNode = curriculumService.GetACurriculumNode( nodeID );

      //Validate the user
      //current user may not be the creator???
      //also needs to be at the curriculum level
      var nodePermissions = GetValidatedUser( nodeID, existingNode == null ? 0 : existingNode.CreatedById );
      var curriculumPermissions = GetValidatedUser( curriculumID, curriculumNode == null ? 0 : curriculumNode.CreatedById );

      var user = nodePermissions.user;
      if ( user == null ) { return Fail( "You must login to create or edit a level" ); }
      
      //Validate edit permissions
      if ( !nodePermissions.write && !curriculumPermissions.write ) 
      { 
        return Fail( "You you do not have permission to create or edit a level in this Learning List." ); 
      }

      if ( nodeID != 0 )
      {
        //Validate inputs
        var util = new UtilityService();
        //Title
        title = util.ValidateText( title, 3, "Title", ref valid, ref status );
        if ( !valid ) { return Fail( status ); }
        //Description
        summary = util.ValidateText( summary, 10, "Summary", ref valid, ref status );
        if ( !valid ) { return Fail( status ); }
        //Timeframe
        timeframe = util.ValidateText( timeframe, 0, "Timeframe", ref valid, ref status );
        if ( !valid ) { return Fail( status ); }
      }

      if ( existingNode == null || existingNode.Id == 0 )
      {
        //Ensure the parent ID belongs to the curriculum
        if ( parentID != curriculumID && curriculumService.GetCurriculumIDForNode( curriculumService.GetACurriculumNode( parentID ) ) == 0 )
        {
          return Fail( "Invalid Level Parent ID." );
        }

        //Determine sort order
        var lastSibling = curriculumService.GetCurriculumOutlineForEdit( parentID ).ChildNodes.OrderBy(m => m.SortOrder).LastOrDefault();
        var sortOrder = 10;
        if(lastSibling != null){
          sortOrder = lastSibling.SortOrder + 10;
        }

        //TODO: not sure this should be hard coded!
        //perhaps if parent not curriculum, add 2?/ Actually, it probably doesn't matter if there is no code specific to node type
        int typeId = ContentItem.MODULE_CONTENT_ID;

        //Create node
        //TODO - not sure about the indirect means to get org. We have already retrieved the curriculum node:
        //curriculumNode.OrgId
        var node = new ContentItem()
        {
          Id = nodeID,
          Title = title,
          Description = summary,
          Summary = summary,
          Timeframe = timeframe,
          PrivilegeTypeId = accessID,
          ParentId = parentID,
          CreatedById = user.Id,
          LastUpdatedById = user.Id,
          StatusId = ContentItem.INPROGRESS_STATUS,
          TypeId = typeId,
          OrgId = curriculumService.GetCurriculumNodeForEdit( curriculumID, user ).OrgId,
          SortOrder = sortOrder
          //grade level
          //k12 subject
        };

        var newID = curriculumService.Create_ef( node, ref status );
        if ( newID != 0 )
        {
          return Reply( newID, true, "okay", null );
        }
        else
        {
          return Fail( status );
        }
      }
      else
      {
        //Modify node
        existingNode.Title = title;
        existingNode.Description = summary;
        existingNode.Summary = summary;
        existingNode.Timeframe = timeframe;
        existingNode.PrivilegeTypeId = accessID;

        existingNode.LastUpdatedById = user.Id;
        //grade level
        //k12 subject
        status = curriculumService.Update( existingNode );
        if ( status == "successful" )
        {
          return Reply( existingNode.Id, true, "okay", new { title = existingNode.Title, description = existingNode.Description, timeframe = existingNode.Timeframe } );
        }
        else
        {
          return Fail( status );
        }
      }
    }

    //Add standards
    [WebMethod( EnableSession = true )]
    public JSON Standards_Add( int nodeID, int targetID, string contentItemType, List<StandardDTO> standards )
    {
      //Ge the node
      var content = curriculumService.Get( targetID );

      //Validate the user
      var permissions = GetValidatedUser( targetID, content.CreatedById );
      var user = permissions.user;
      if ( user == null ) 
      { 
        return Fail( "You must login to update standards." ); 
      }
      if ( !permissions.write )
      {
        return Fail( "You don't have permission to add standards to that." );
      }

      //Add standards
      List<ContentStandard> addedStandards = new List<ContentStandard>();
      foreach ( var item in standards )
      {
        item.recordID = 0;
        addedStandards.Add( GetContentStandardFromStandardDTO( targetID, item, user ) );
      }

      //Save standards
      CurriculumServices.ContentStandard_Add( addedStandards );

      //Return JSON
      if ( contentItemType == "attachment" )
      {
        return GetAttachments( nodeID );
      }
      else
      {
        return GetContentStandards( nodeID );
      }
    }

    //Update a standard
    [WebMethod( EnableSession = true )]
    public JSON Standard_Update( int parentID, StandardDTO standard )
    {
      var status = "";

      //Get node
      var content = curriculumService.Get( parentID );

      //Validate the user
      var permissions = GetValidatedUser( parentID, content.CreatedById );
      var user = permissions.user;
      if ( user == null ) 
      { 
        return Fail( "You must login to update standards." ); 
      }
      if ( !permissions.write )
      {
        return Fail( "You don't have permission to do that." );
      }

      //Update standard
      var valid = CurriculumServices.ContentStandard_Update( standard.recordID, standard.alignmentID, standard.usageID, user.Id, ref status );

      if ( !valid )
      {
        return Fail( status );
      }

      //Return JSON
      return GetContentStandards( parentID );
    }

    //Delete a standard
    [WebMethod( EnableSession = true )]
    public JSON Standard_Delete( int parentID, int recordID )
    {
      var status = "";

      //Get the contentitem
      var content = curriculumService.Get( parentID );

      //Validate the user
      var permissions = GetValidatedUser( parentID, content.CreatedById );
      var user = permissions.user;
      if ( user == null ) 
      { 
        return Fail( "You must login to update standards." ); 
      }
      if ( !permissions.write )
      {
        return Fail( "You don't have permission to do that." );
      }

      //Delete standard
      CurriculumServices.ContentStandard_Delete( recordID, ref status );

      //Return JSON
      return GetContentStandards( parentID );
    }

    //Save URL attachment (File attachment is handled in /Controls/Curriculum/CurriculumFileUpload.aspx--maybe that should be moved here?)
    [WebMethod( EnableSession = true )]
    public JSON Attachment_SaveURL(int nodeID, int attachmentID, string title, int accessID, string url, bool featured )
    {
      return ManageAttachment( "saveurl", nodeID, attachmentID, title, accessID, url, featured );
    }

    //Delete attachment
    [WebMethod( EnableSession = true )]
    public JSON Attachment_Delete( int nodeID, int attachmentID )
    {
      return ManageAttachment( "delete", nodeID, attachmentID, "", 0, "", false );
    }

    //Update attachment without touching document/URL
    [WebMethod( EnableSession = true )]
    public JSON Attachment_UpdateData( int nodeID, int attachmentID, string title, int accessID, bool featured )
    {
      return ManageAttachment( "updatedata", nodeID, attachmentID, title, accessID, "", featured );
    }

    //Alter sort order of documents
    [WebMethod( EnableSession = true )]
    public JSON Attachment_Reorder( int nodeID, int attachmentID, string direction )
    {
      return ManageAttachment( "reorder", nodeID, attachmentID, direction, 0, "", false );
    }

    //Manage attachment
    private JSON ManageAttachment( string method, int nodeID, int attachmentID, string title, int accessID, string url, bool featured )
    {
      try
      {
        //Validate user
        var user = GetValidatedUser( 0, 0 ).user;
        if ( user == null )
        {
          return Fail( "You must log in to use this feature." );
        }

        var util = new UtilityService();
        var status = "";
        var valid = true;

        //Create
        if ( method == "saveurl" )
        {
          //Validate permissions
          var node = curriculumService.GetACurriculumNode( nodeID );
          var nodePermissions = GetValidatedUser( nodeID, node.CreatedById );
          if ( !nodePermissions.write )
          {
            return Fail( "You don't have permission to edit that level." );
          }

          //Validate data
          title = util.ValidateText( title, 3, "Title", ref valid, ref status );
          if ( !valid ) { return Fail( status ); }

          url = util.ValidateURL( url, false, ref valid, ref status );
          if ( !valid ) { return Fail( status ); }

          //Fetch item if available - returns new if not found
          var item = contentService.Get( attachmentID );

          //Update fields
          item.Id = attachmentID;
          item.Title = title;
          item.DocumentUrl = url;
          item.PrivilegeTypeId = accessID;
          item.LastUpdatedById = user.Id;
          item.StatusId = ContentItem.PUBLISHED_STATUS;
          item.SortOrder = featured ? -1 : ( node.ChildItems.Count() * 10 ) + 10;
          item.TypeId = 41; //URL

          //Save to database
          if ( attachmentID == 0 )
          {
            item.ParentId = nodeID;
            item.CreatedById = user.Id;
            contentService.Create_ef( item, ref status );
          }
          else
          {
            /*var attachmentPermissions = GetValidatedUser( item.Id, item.CreatedById );
            if ( !attachmentPermissions.write )
            {
              return Fail( "You don't have permission to edit that attachment." );
            }*/
            //Only need to check node permissions, which was already done
            status = contentService.Update( item );
          }

          if ( status == "successful" || status == "okay" || status == "" ) //Really need a more consistent way of knowing a transaction worked
          {
            if ( featured )
            {
              return ManageAttachment( "updatedata", nodeID, item.Id, title, accessID, url, featured ); //several things need to happen to make featuring work
            }
            else
            {
              //Create thumbnail
              new LRWarehouse.DAL.ResourceThumbnailManager().CreateThumbnailAsync( "content-" + item.Id, item.DocumentUrl, true, 3 );
              
              //Return confirmation
              return GetAttachments( nodeID );
            }
          }
          else
          {
            return Fail( status );
          }
        }
        //Modify
        else
        {
          var attachment = contentService.Get( attachmentID );
        if ( attachment == null || attachment.Id == 0) 
          {
            return Fail( "Error loading attachment" );
          }
          /*var attachmentPermissions = GetValidatedUser( attachment.Id, attachment.CreatedById );
          if ( !attachmentPermissions.write )
          {
            return Fail( "You don't have permission to edit that attachment." );
          }*/
          var node = curriculumService.GetACurriculumNode( nodeID );
          if ( node == null || node.Id == 0 )
          {
            return Fail( "Error: Invalid level ID" );
          }
          var nodePermissions = GetValidatedUser( node.Id, node.CreatedById );
          if ( !nodePermissions.write )
          {
            return Fail( "You don't have permission to edit that level." );
          }

          //Update data 
          if ( method == "updatedata" )
          {
            //Validate input
            title = util.ValidateText( title, 3, "Title", ref valid, ref status );
            if ( !valid ) { return Reply( null, false, status, null ); }

            //Update data
            attachment.Title = title;
            attachment.PrivilegeTypeId = accessID;
            contentService.Update( attachment );
            //Call this last since procs reset this parameter
            if ( featured )
            {
              var featuredItem = node.ChildItems.Where( m => m.SortOrder == -1 ).FirstOrDefault();
              if ( featuredItem != null )
              {
                curriculumService.RemovedItemAsFeatured( featuredItem.Id );
              }
              curriculumService.SetAsFeaturedItem( nodeID, attachmentID );
              ReorderAttachments( null, null, node.Id, user );
            }
            else if( !featured && attachment.SortOrder == -1 )
            {
              curriculumService.RemovedItemAsFeatured( attachmentID );
              attachment.SortOrder = 0;
              //ReorderAttachments( attachment, node.ChildItems.Where( m => m.SortOrder != -1 && m.Id != attachment.Id ).FirstOrDefault(), node.Id, user );
              ReorderAttachments( null, null, node.Id, user );
            }

            //Recreate thumbnail
            new LRWarehouse.DAL.ResourceThumbnailManager().CreateThumbnailAsync( "content-" + attachment.Id, attachment.DocumentUrl, true, 3 );

            //Return data
            return GetAttachments( nodeID );
          }

          //Delete
          if ( method == "delete" )
          {
          //need to check return from delete, and use status if not successful
            contentService.Delete( attachment.Id, ref status );

            var valid1 = true;
            var valid2 = true;
            var status1 = "";
            var status2 = "";

            //Not sure which of these is appropriate
          //valid1 = curriculumService.ContentReferenceDelete( attachmentID, ref status1 );
          //valid2 = curriculumService.ContentSupplementDelete( attachmentID, ref status2 );

            if ( valid1 || valid2 )
            {
              //Return confirmation
              return GetAttachments( nodeID );
            }
            else
            {
              return Fail( valid1 ? status2 : status1 );
            }
          }

          //Alter sort order
          if ( method == "reorder" )
          {
            //Get siblings
            var siblings = node.ChildItems;

            //Get item to be moved
            var swapee = new ContentItem() { Id = 0 };
            if ( title == "up" )
            {
              swapee = siblings.OrderBy( m => m.SortOrder ).Where( m => m.SortOrder < attachment.SortOrder ).LastOrDefault();
            }
            else if ( title == "down" )
            {
              swapee = siblings.OrderBy( m => m.SortOrder ).Where( m => m.SortOrder > attachment.SortOrder ).FirstOrDefault();
            }

            //Do the move
            if ( swapee.Id != 0 )
            {
              ReorderAttachments( attachment, swapee, nodeID, user );
            }

            //Return data
            return GetAttachments( nodeID );
          }

        }

        //Return confirmation
        return Fail( "Error: Invalid operation" );
      }
      catch ( Exception ex )
      {
        return Reply( ex.Message, false, "There was an error processing your request.", ex.ToString() );
      }
    }

    //Post a news item
    [WebMethod( EnableSession = true )]
    public JSON Save_News( int nodeID, string text )
    {
      bool valid = true;
      string status = "";
      //Validate the user
      var user = GetValidatedUser( nodeID, 0 ).user;
      if ( user == null ) { return Fail( "You must login to post a news item." ); }

      //Validate the input (May need to use a different method due to HTML-ey nature of the input
      text = new UtilityService().ValidateText( text, 10, "News", ref valid, ref status );
      if ( !valid ) { return Fail( status ); }

      //Add the news item
      try
      {
        var newsID = curriculumService.Curriculum_AddHistory( nodeID, text, user.Id );
        return Reply( newsID, true, "okay", null );
      }
      catch( Exception ex )
      {
        return Reply( "", false, "Sorry, there was a problem posting your news item.", ex.Message );
      }
    }
    #endregion

    #region Helper Methods
    //Get user and permissions
    public ValidationPermissions GetValidatedUser( int entityID, int createdByID )
    {
      //Get the user
        var user = ( Patron )Session[ Constants.USER_REGISTER ];
      if ( user == null || user.Id == 0 )
      {
        return new ValidationPermissions() { user = null, read = false, write = false };
      }
      //If no inputs, skip permission check
      if ( entityID == 0 || createdByID == 0 )
      {
        return new ValidationPermissions() { user = user, read = true, write = true };
      }

      //Check global admin permissions
      //Temporary hack
      if ( user.Id == 2 || user.Id == 22 )
      { // If Mike or Nate
        return new ValidationPermissions() { user = user, read = true, write = true };
      }

      //Check user permissions
      //If user created the item, they have full control
      //would need to handle where creator is no longer authorized - ex. left org
      if ( user.Id == createdByID )
      {
        return new ValidationPermissions() { user = user, read = true, write = true };
      }
      else
      {
        var privileges = SecurityManager.GetGroupObjectPrivileges( user, "ILPathways.LRW.controls.Authoring" );
        ContentPartner partner = ContentServices.ContentPartner_Get( entityID, user.Id );

        if ( partner == null || partner.PartnerTypeId == 0 )
        {
          return new ValidationPermissions() { user = user, read = false, write = false };
        }
        else
        {
          var canRead = partner.PartnerTypeId > 0;
          var canWrite = partner.PartnerTypeId >= 2 || privileges.WritePrivilege > (int) ILPathways.Business.EPrivilegeDepth.Region;
          return new ValidationPermissions() { user = user, read = canRead, write = canWrite };
        }
      }

    }
    public class ValidationPermissions
    {
      public Patron user { get; set; }
      public bool write { get; set; }
      public bool read { get; set; }
    }

    //Reply with standardized JSON response message
    private UtilityService.GenericReturn Reply( object data, bool valid, string status, object extra )
    {
      return UtilityService.DoReturn( data, valid, status, extra );
    }
    private UtilityService.GenericReturn Fail( string status )
    {
      return Reply( null, false, status, null );
    }

    //Overloads for standardizing standards
    public StandardDTO GetStandardDTO( ContentResourceStandard item, bool isContentStandard )
    {
      return new StandardDTO()
      {
        recordID = item.StandardRecordId,
        standardID = item.StandardId,
        contentID = item.ContentId,
        code = ( string.IsNullOrWhiteSpace( item.NotationCode ) ? item.Description.Substring( 0, 20 ) + "..." : item.NotationCode ),
        text = item.Description,
        isContentStandard = isContentStandard,
        usageID = 1,
        alignmentID = item.AlignmentTypeCodeId,
      };
    }
    public StandardDTO GetStandardDTO( Content_StandardSummary item, bool isContentStandard )
    {
      return new StandardDTO()
      {
        recordID = item.StandardRecordId,
        standardID = item.StandardId,
        contentID = item.ContentId,
        code = ( string.IsNullOrWhiteSpace( item.NotationCode ) ? item.Description.Substring( 0, 20 ) + "..." : item.NotationCode ),
        text = item.Description,
        isContentStandard = isContentStandard,
        usageID = item.UsageTypeId,
        alignmentID = item.AlignmentTypeCodeId,
      };
    }

    //Conversion
    public ContentStandard GetContentStandardFromStandardDTO( int parentID, StandardDTO input, Patron user )
    {
      return new ContentStandard()
      {
        Id = input.recordID,
        StandardId = input.standardID,
        ContentId = parentID,
        AlignmentTypeCodeId = input.alignmentID,
        UsageTypeId = input.usageID,
        Created = DateTime.Now,
        CreatedById = user.Id,
        LastUpdated = DateTime.Now,
        LastUpdatedById = user.Id
      };
    }

    //Reorder attachments
    public void ReorderAttachments( ContentItem mover, ContentItem moved, int nodeID, Patron user )
    {
      if ( mover != null && moved != null )
      {
        //Swap order
        var temp = mover.SortOrder;
        mover.SortOrder = moved.SortOrder;
        moved.SortOrder = temp;

        //Save changes
        curriculumService.Update( mover );
        curriculumService.Update( moved );
      }

      //Refresh siblings
      var siblings = curriculumService.GetCurriculumNodeForEdit( nodeID, user ).ChildItems.OrderBy( m => m.SortOrder ).ThenBy( m => m.Id ).ToList();

      //Rewrite sort orders as needed
      var order = 10;
      foreach ( var item in siblings )
      {
        if ( item.SortOrder == -1 ) { continue; } //Don't break featured
        if ( item.SortOrder != order )
        {
          //Only update if necessary
          item.SortOrder = order;
          curriculumService.Update( item );
        }
        order = order + 10;
      }
    }

    #endregion


    #region Subclasses

    public class StandardDTOInput
    {
      public StandardDTOInput()
      {
        standards = new List<StandardDTO>();
      }
      public List<StandardDTO> standards { get; set; }
    }

    public class StandardDTO
    {
      public int recordID { get; set; } //database row int ID
      public int standardID { get; set; } //ID of the standard in the standards table
      public int contentID { get; set; } //ID of the content item
      public string code { get; set; }
      public string text { get; set; }
      public bool isContentStandard { get; set; }
      public int usageID { get; set; }
      public int alignmentID { get; set; }
    }

    public class JSTreeNode
    {
      public int id { get; set; }
      public int parent { get; set; }
      public string text { get; set; } //Title
      public object a_attr { get; set; }
      public object li_attr { get; set; }
    }

    public class AttachmentDTO
    {
      public AttachmentDTO()
      {
        standards = new List<StandardDTO>();
      }
      public int attachmentID { get; set; }
      public string title { get; set; }
      public int accessID { get; set; }
      public string url { get; set; }
      public bool featured { get; set; }
      public string attachmentType { get; set; }
      public List<StandardDTO> standards { get; set; }
    }

    #endregion
  }
}
