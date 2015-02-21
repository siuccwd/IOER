using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LRWarehouse.Business;
using LRWarehouse.DAL;
using System.Data;
using Isle.DTO;

namespace Isle.BizServices
{
  public class ParadataService 
  {
    public void AddLikeDislike( int resourceID, string userGuid, bool isLike, ref bool successful, ref string status )
    {
      //Get and check user
      var user = new AccountServices().GetByRowId( userGuid );
      if ( user == null || user.Id == 0 )
      {
        successful = false;
        status = "You must be logged in to like things!";
        return;
      }

      //Make sure the user hasn't already liked it -- still need a proc for this
      DataSet ds = ResourceBizService.DoQuery( "SELECT * FROM [Resource.Like] WHERE [ResourceIntId] = " + resourceID + " AND [CreatedById] = " + user.Id );
      if ( ResourceBizService.DoesDataSetHaveRows( ds ) )
      {
        successful = false;
        status = "You already like this!";
        return;
      }

      //Return success
      new ResourceLikeManager().Create( new ResourceLike() { ResourceIntId = resourceID, IsLike = isLike, CreatedById = user.Id }, ref status );
      successful = true;
      status = "okay";

      return;
    }

    public List<CommentDTO> GetComments( int resourceID, string userGuid, ref bool userCanPost, ref bool success, ref string status )
    {
      var user = new AccountServices().GetByRowId( userGuid );
      userCanPost = !( user == null || user.Id == 0 );

      var comments = new List<CommentDTO>();

      DataSet ds = new ResourceCommentManager().Select( resourceID );
      var accountServices = new AccountServices();
      if ( ResourceCommentManager.DoesDataSetHaveRows( ds ) )
      {
        foreach ( DataRow dr in ds.Tables[ 0 ].Rows )
        {
          var comment = new CommentDTO();
          var createdByIdText = ResourceCommentManager.GetRowColumn( dr, "CreatedById" );
          comment.AvatarURL = createdByIdText == "" ? "" : accountServices.Get( int.Parse( createdByIdText ) ).ImageUrl;
          comment.Date = DateTime.Parse( ResourceCommentManager.GetRowColumn( dr, "Created" ) ).ToShortDateString();
          comment.Id = int.Parse( ResourceCommentManager.GetRowColumn( dr, "Id" ) );
          comment.Text = ResourceCommentManager.GetRowColumn( dr, "Comment" );
          comment.Name = ResourceCommentManager.GetRowColumn( dr, "CreatedBy" );
          comments.Add( comment );
        }
      }

      success = true;
      status = "okay";
      return comments;
    }
  }
}
