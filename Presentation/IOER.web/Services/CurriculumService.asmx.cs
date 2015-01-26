using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using LRWarehouse.Business;
using Isle.BizServices;
using Isle.DTO;

using Node = ILPathways.Business.ContentItem;

namespace ILPathways.Services
{
  /// <summary>
  /// Summary description for CurriculumService
  /// </summary>
  [WebService( Namespace = "http://tempuri.org/" )]
  [WebServiceBinding( ConformsTo = WsiProfiles.BasicProfile1_1 )]
  [System.ComponentModel.ToolboxItem( false )]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class CurriculumService : System.Web.Services.WebService
  {
    UtilityService utilService = new UtilityService();
    ContentServices contentServices = new ContentServices();
    CurriculumServices curriculumServices = new CurriculumServices();

    string downloadPage = "/Repository/DownloadFiles.aspx?nid=";

    #region Get methods
    [WebMethod]
    public string GetTree( int id )
    {
      return utilService.ImmediateReturn( GetTreeJSON( id ), true, "okay", null );
    }

    public JSONNode GetTreeJSON( int id, string userGUID )
    {
      var data = GetTreeOutline( id, userGUID );
      var output = GetChildNodes( data, 0 );
      return output;
    }
    public JSONNode GetTreeJSON( int id )
    {
        var data = GetTreeOutline( id );
      var output = GetChildNodes( data, 0 );

      return output;
    }
    private JSONNode GetChildNodes( ContentNode item, int parentID )
    {
      var output = new JSONNode();
      output.id = item.Id;
      output.parentID = parentID;
      output.title = item.Title;
      output.description = item.Description;

      foreach ( ContentNode child in item.ChildNodes )
      {
        output.children.Add( GetChildNodes( child, item.Id ) );
      }

      return output;
    }
    public ContentNode GetTreeOutline( int id, string userGUID )
    {
        //retrieve public view of curriculum
        //if user has access, all nodes will be returned
        return curriculumServices.GetPublicCurriculumOutline( id, userGUID );
    }
    public ContentNode GetTreeOutline( int id )
    {
        //retrieve public view of curriculum
        //will need user if nodes can be privileged
        return curriculumServices.GetPublicCurriculumOutline( id );
    }
    [WebMethod]
    public string DisplayNode( string userGUID, int id )
    {
        return utilService.ImmediateReturn( DisplayJSONNode( userGUID, id, true ), true, "okay", null );
    }

    public JSONNode DisplayJSONNode( string userGUID, int id, bool allowCaching )
    {
       var user = new AccountServices().GetByRowId( userGUID );
       bool usingContentStandards = ContentServices.IsUsingContentStandards();
      var output = new JSONNode();
      var data = GetTreeNode( user, id, allowCaching );

      output.id = data.Id;
      output.parentID = data.ParentId;
      output.title = data.Title;
      output.description = (data.Description == "" ? data.Summary : data.Description );
      output.contentType = data.ContentType;
      output.downloadUrl = downloadPage + data.Id.ToString();
      if ( data.ContentType == null || data.ContentType == "" )
      {
        output.contentType = "item";
      }
      output.resourceUrl = data.ResourceFriendlyUrl;

      if ( data.TypeId == Node.DOCUMENT_CONTENT_ID )
      {
          //TODO - should get parent node for reference
          if ( data.CanViewDocument )
          {
              output.documentUrl = data.DocumentUrl;
          }
          else
          {
              output.documentUrl = "#";
              //may be OK to see metadata, as not currently hidden from public
              output.message = data.DocumentPrivacyMessage;
          }
      }

      if ( usingContentStandards )
      {
          if ( data.HasStandards && data.Standards != null )
          {
              foreach ( Business.Content_StandardSummary standard in data.ContentStandards )
              {
                  output.standards.Add( new Standard() { code = standard.NotationCode, description = standard.Description } );
              }
          }
          foreach ( var item in data.ContentChildrenStandards )
          {
              output.childStandards.Add( new Standard() { code = item.NotationCode, description = item.Description } );
          }
      }
      else
      {
          if ( data.HasStandards && data.Standards != null )
          {
              foreach ( Business.ContentResourceStandard standard in data.Standards )
              {
                  output.standards.Add( new Standard() { code = standard.NotationCode, description = standard.Description } );
              }
          }
          foreach ( var item in data.ChildrenStandards )
          {
              output.childStandards.Add( new Standard() { code = item.NotationCode, description = item.Description } );
          }
      }

      foreach ( Node item in data.ChildItems )
      {

        //a node will only display docs, not any child nodes, so ignore if not typeId = 40
        if ( item.TypeId != Node.DOCUMENT_CONTENT_ID )
            continue;

        var kid = new JSONNode();
        kid.title = item.Title;
        kid.description = ( item.Description == "" ? item.Summary : item.Description );
        if ( item.CanViewDocument )
        {
            kid.documentUrl = item.DocumentUrl;
            kid.resourceUrl = item.ResourceFriendlyUrl;
        }
        else
        {
            kid.documentUrl = "#";
            //may be OK to see metadata, as not currently hidden from public
            kid.resourceUrl = item.ResourceFriendlyUrl;
            kid.message = item.DocumentPrivacyMessage;
        }
        
        if ( item.HasStandards )
        {
            if ( usingContentStandards )
            {
                foreach ( Business.Content_StandardSummary standard in item.ContentStandards )
                {
                    kid.standards.Add( new Standard() { code = standard.NotationCode, description = standard.Description } );
                }
            }
            else
            {
                foreach ( Business.ContentResourceStandard standard in item.Standards )
                {
                    kid.standards.Add( new Standard() { code = standard.NotationCode, description = standard.Description } );
                }
            }
        }
        output.children.Add( kid );
      }

      return output;
    }
    public Node GetTreeNode( Patron user, int id, bool allowCaching )
    {
      //get node with complete fill

        NodeRequest request = new NodeRequest();
        request.AllowCaching = allowCaching;
        request.ContentId = id;
        //may need more granularity, don't want everything for view
        request.DoCompleteFill = true;  //?????
        request.IsEditView = false;
        return curriculumServices.GetACurriculumNode( request, user);
    }

    [WebMethod]
    public string RefreshNodeDocuments( string userProxyId, int nodeId )
    {
        return utilService.ImmediateReturn( RefreshJsonNodeDocuments( userProxyId, nodeId ), true, "okay", null );
    }
    public JSONNode RefreshJsonNodeDocuments( string userProxyId, int nodeId )
    {
        string statusMessage = "";
        var output = new JSONNode();

        //TODO - switch to use of proxyId
        var user = new AccountServices().GetByProxyRowId( userProxyId, ref statusMessage );

        output.message = "Error";
        var data = GetTreeNode( user, nodeId, true );

        output.id = data.Id;
        output.parentID = data.ParentId;
        output.title = data.Title;
        output.description = ( data.Description == "" ? data.Summary : data.Description );
        output.contentType = data.ContentType;

        if ( data.ContentType == null || data.ContentType == "" )
        {
            output.contentType = "item";
        }
        output.resourceUrl = data.ResourceFriendlyUrl;

        foreach ( Node item in data.ChildItems )
        {
            var kid = new JSONNode();
            kid.title = item.Title;
            kid.description = ( item.Description == "" ? item.Summary : item.Description );
            if ( item.CanViewDocument )
            {
                kid.documentUrl = item.DocumentUrl;
                kid.resourceUrl = item.ResourceFriendlyUrl;
            }
            else
            {
                kid.documentUrl = "#";
                //may be OK to see metadata, as not currently hidden from public
                kid.resourceUrl = item.ResourceFriendlyUrl;
                kid.message = item.DocumentPrivacyMessage;
            }

            output.children.Add( kid );
        }

        return output;
    }


    #endregion
   
    #region Subclasses
    public class JSONNode
    {
      public JSONNode()
      {
        resourceIDs = new List<int>();
        children = new List<JSONNode>();
        standards = new List<Standard>();
        childStandards = new List<Standard>();
      }
      public int id { get; set; }
      public int parentID { get; set; }
      public string resourceUrl { get; set; }
      public string documentUrl { get; set; }
        /// <summary>
        /// contains link to download files for this node
        /// </summary>
      public string downloadUrl { get; set; }

      public string title { get; set; }
      public string description { get; set; }
      public string message { get; set; }
      public string contentType { get; set; }
      public List<int> resourceIDs { get; set; }
      public List<JSONNode> children { get; set; }
      public List<Standard> standards { get; set; }
      public List<Standard> childStandards { get; set; }
    }
    public class Standard
    {
      public string code { get; set; }
      public string description { get; set; }
    }

    #endregion
  }
}
