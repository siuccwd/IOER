using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using LRWarehouse.Business;
using Isle.BizServices;
using ILPathways.Business;
using JSON = IOER.Services.UtilityService.GenericReturn;
using System.IO;
using ILPathways.Utilities;

namespace IOER.Services
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
					user = AccountServices.GetUser( item.CreatedById ).FullName(), //optimize this later
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
			if ( user == null )
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
			try
			{
				output.Sort( delegate( StandardDTO x, StandardDTO y ) { return string.Compare( x.code, y.code ) > 0 ? 1 : -1; } ); //Not sure this has any effect
			}
			catch { }

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
		[WebMethod( EnableSession = true )]
		public JSON GetJSTree( int nodeID )
		{
			var raw = curriculumService.GetCurriculumOutlineForEdit( nodeID );
			var list = new List<JSTreeNode>();
			FlattenJSTreeNodes( raw, ref list );
			return Reply( list, true, "okay", null );
		}
		private void FlattenJSTreeNodes( Isle.DTO.ContentNode node, ref List<JSTreeNode> runningList )
		{
			runningList.Add( new JSTreeNode()
			{
				id = node.Id,
				text = node.Title,
				parent = node.ParentId,
				//a_attr = new { href = "/testing/placeholder.aspx?node=" + node.Id } 
				a_attr = new { href = "/my/learninglist/" + node.Id },
				li_attr = new { sortOrder = node.SortOrder }
			} );
			foreach ( var item in node.ChildNodes )
			{
				FlattenJSTreeNodes( item, ref runningList );
			}
		}

		//Get attachments for a node
		[WebMethod( EnableSession = true )]
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
					summary = item.Summary,
					accessID = item.PrivilegeTypeId,
					url = item.DocumentUrl,
					featured = item.SortOrder <= 0,
					attachmentType = ( ( ContentTypes ) item.TypeId ).ToString(),
					standards = GetContentStandardsData( item.Id ),
					usageRightsId = item.ConditionsOfUseId,
					usageRights = item.ConditionsOfUse,
					usageRightsUrl = item.UsageRightsUrl
				} );
			}
			return Reply( list, true, "okay", null );
		}
		#endregion

		#region Post Methods
		//Create a learning set
		public JSON LearningSet_Create( string title, string description, int organizationID )
		{
			return Curriculum_Create( title, description, organizationID, ContentItem.LEARNING_SET_CONTENT_ID );
		}

		//Create a curriculum
		public JSON Curriculum_Create( string title, string description, int organizationID )
		{
			return Curriculum_Create( title, description, organizationID, ContentItem.CURRICULUM_CONTENT_ID );
		}

		//Create a curriculum or a learning set
		public JSON Curriculum_Create( string title, string description, int organizationID, int contentTypeID )
		{
			//Validate the user
			ValidationPermissions vp = GetValidatedUser(0, 0);
			//var user = GetValidatedUser( 0, 0 ).user;
			if ( vp == null || vp.user == null )
			{
				return Fail( "You must login to create a Learning List or a Learning Set." );
			}
			var user = vp.user;
			//Create the node
			var topNode = new ContentItem()
			{
				Title = title,
				//Description = description,
				Summary = description,
				Created = DateTime.Now,
				CreatedById = user.Id,
				LastUpdated = DateTime.Now,
				LastUpdatedById = user.Id,
				IsActive = true,
				ConditionsOfUseId = ContentItem.UNKNOWN_CCOU,
				StatusId = ContentItem.DRAFT_STATUS,

				TypeId = contentTypeID,
				PrivilegeTypeId = ContentItem.PUBLIC_PRIVILEGE,
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
			var unknownRightsID = 0;
			try
			{
				unknownRightsID = new ResourceV2Services().GetUsageRightsList().Where( m => m.CodeId == 8 ).FirstOrDefault().CodeId;
			}
			catch
			{
				unknownRightsID = 0;
			}
			LoggingHelper.DoTrace(7, "IOER.Services.CurriculumService1.Node_Create(). curriculumID: {0}");
			return Save_Properties( curriculumID, 0, nodeID, "New Level", "", "", 1, null, null, unknownRightsID, "", 0, "" );
		}

		//Delete a node
		[WebMethod( EnableSession = true )]
		public JSON Node_Delete( int nodeID )
		{
			//Get the node
			var node = curriculumService.Get( nodeID );

			//Validate the user
			var permissions = GetValidatedUser( node, node.CreatedById );
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
			valid = curriculumService.Delete( node.Id, user.Id, ref status );

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
			var user = GetValidatedUser( nodeID, 0 ).user;
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
				text = new UtilityService().ValidateText( text, 10, "Comment", ref valid, ref status );
				if ( !valid ) { return Fail( status ); }

				status = "";
				//Post the comment
				try
				{
					curriculumService.Content_AddComment( nodeID, text, user, ref status );
				}
				catch ( Exception ex )
				{
					return Reply( null, false, status, ex.Message );
				}

				//Fetch the updated comments list
				return GetComments( nodeID, true );
			}
			catch ( Exception ex )
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
			var user = GetValidatedUser( nodeID, 0 ).user;
			if ( user == null )
			{
				return Reply( "", false, "You must login to subscribe.", null );
			}

			//Update the option
			string status = "";
			try
			{
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
						var success = curriculumService.ContentSubscription_Delete( subscription.Id, user, ref status );
						return Reply( success, success, status, null );
					}
				}
				else
				{
					if ( type > 0 )
					{
						var success = curriculumService.ContentSubScription_Create( nodeID, user, type, ref status );
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

		/// <summary>
		/// Save a node (create or update)'s properties
		/// </summary>
		/// <param name="curriculumID"></param>
		/// <param name="nodeID"></param>
		/// <param name="parentID"></param>
		/// <param name="title"></param>
		/// <param name="summary"></param>
		/// <param name="timeframe"></param>
		/// <param name="accessID"></param>
		/// <param name="k12SubjectIDs"></param>
		/// <param name="gradeLevelIDs"></param>
		/// <param name="usageRightsId"></param>
		/// <param name="usageRightsUrl"></param>
		/// <returns></returns>
		[WebMethod( EnableSession = true )]
		public JSON Save_Properties( int curriculumID, int nodeID, int parentID, string title, string summary, string timeframe, int accessID, List<int> k12SubjectIDs, List<int> gradeLevelIDs, int usageRightsId, string usageRightsUrl, int orgID, string richDescription )
		{
			//TODO - add keywords - should use an object now
			// or pass the following and then populate the node.ContentKeywords
			// Actually, a TODO is whether only new keywords will be passed
			List<string> keywords = new List<string>();
			var valid = true;
			var status = "";
			//Fetch the curriculum
			var curriculumNode = curriculumService.GetACurriculumNode( curriculumID );
			var existingNode = curriculumService.GetACurriculumNode( nodeID );

			//Validate the user
			//current user may not be the creator???
			//also needs to be at the curriculum level
			var nodePermissions = GetValidatedUser( existingNode, existingNode == null ? 0 : existingNode.CreatedById );
			var curriculumPermissions = GetValidatedUser( curriculumNode, curriculumNode == null ? 0 : curriculumNode.CreatedById );

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
				string message = "";
				//-should not return one error at a time
				//Title
				title = util.ValidateText( title, 3, "Title", ref valid, ref status );
				if ( !valid ) {
					message += status + "\r\n";
					//return Fail( status ); 
				}
				//Summary
				summary = util.ValidateText( summary, 10, "Summary", ref valid, ref status );
				if ( !valid )
				{
					message += status + "\r\n";
					//return Fail( status ); 
				}
				//Timeframe
				timeframe = util.ValidateText( timeframe, 0, "Timeframe", ref valid, ref status );
				if ( !valid )
				{
					message += status + "\r\n";
					//return Fail( status ); 
				}
				//Usage Rights
				if ( usageRightsId == ContentItem.READ_THE_FINE_PRINT_CCOU )
				{
					//url required
					//Usage Rights URL
					if ( usageRightsUrl.Length == 0 )
					{
						status = "A URL must be entered when selecting Usage Rights of 'Read the Fine Print'";
						message += status + "\r\n";
						//return Fail( status );
					} else 
					{
						usageRightsUrl = util.ValidateURL( usageRightsUrl, false, ref valid, ref status );
						if ( !valid )
						{
							message += status + "\r\n";
							//return Fail( status ); 
						}
					}
				}
				//Rich Description
				richDescription = util.ValidateText( richDescription, 0, "Rich Description", true, ref valid, ref status );
				if ( !valid )
				{
					message += status + "\r\n";
				}

				//Return if there are errors
				if ( message.Length > 0 ) { return Fail( message ); }
			}

			if ( existingNode == null || existingNode.Id == 0 )
			{
				//Ensure the parent ID belongs to the curriculum
				if ( parentID != curriculumID && curriculumService.GetCurriculumIDForNode( curriculumService.GetACurriculumNode( parentID ) ) == 0 )
				{
					return Fail( "Invalid Level Parent ID." );
				}

				//Determine sort order
				var lastSibling = curriculumService.GetCurriculumOutlineForEdit( parentID ).ChildNodes.OrderBy( m => m.SortOrder ).LastOrDefault();
				var sortOrder = 10;
				if ( lastSibling != null )
				{
					sortOrder = lastSibling.SortOrder + 10;
				}

				//TODO: not sure this should be hard coded!
				//perhaps if parent not curriculum, add 2?/ Actually, it probably doesn't matter if there is no code specific to node type
				int typeId = ContentItem.MODULE_CONTENT_ID;

				//Create node
				//TODO - not sure about the indirect means to get org. We have already retrieved the curriculum node:
				//15-11-20 mparsons - change to just use the orgId from topNode
				//curriculumNode.OrgId
				var node = new ContentItem()
				{
					Id = nodeID,
					Title = title,
					Description = richDescription,
					Summary = summary,
					Timeframe = timeframe,
					PrivilegeTypeId = accessID,
					ParentId = parentID,
					CreatedById = user.Id,
					LastUpdatedById = user.Id,
					StatusId = ContentItem.INPROGRESS_STATUS,
					TypeId = typeId,
					OrgId = curriculumNode.OrgId,
					SortOrder = sortOrder,
					UsageRightsUrl = usageRightsUrl,
					ConditionsOfUseId = usageRightsId
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
				existingNode.Description = richDescription;
				existingNode.Summary = summary;
				//16-02-25 mparsons - leave status as is if published (until we have any sort of approvals process)
				if ( existingNode.StatusId != ContentItem.PUBLISHED_STATUS)
					existingNode.StatusId = ContentItem.INPROGRESS_STATUS;
				existingNode.Timeframe = timeframe;
				existingNode.PrivilegeTypeId = accessID;
				existingNode.ConditionsOfUseUrl = usageRightsUrl;
				existingNode.ConditionsOfUseId = usageRightsId;
				existingNode.LastUpdatedById = user.Id;
				if ( nodeID == curriculumID && orgID > 0 && existingNode.OrgId != orgID )
				{
					//org can only be updated at top level
					existingNode.OrgId = orgID;
				}
				//grade level
				//k12 subject
				status = curriculumService.Update( existingNode );
				if ( status == "successful" )
				{
					return Reply( existingNode.Id, true, "okay", new { title = existingNode.Title, summary = existingNode.Summary, description = existingNode.Description, timeframe = existingNode.Timeframe } );
				}
				else
				{
					return Fail( status );
				}
			}
		}


		/// <summary>
		/// For a published node, check for and publish any new child resources
		/// </summary>
		/// <param name="curriculumID"></param>
		/// <param name="nodeID"></param>
		/// <returns></returns>
		[WebMethod( EnableSession = true )]
		public JSON PublishChildResources( int curriculumID, int nodeID )
		{
			var valid = true;
			var status = "";
			if ( nodeID == 0 ) { return Fail( "A valid identifier must be provided." ); }
			if ( curriculumID == 0 ) { return Fail( "A valid parent identifier must be provided." ); }
			//Fetch the curriculum
			var curriculumNode = curriculumService.GetACurriculumNode( curriculumID );
			var existingNode = curriculumService.GetACurriculumNode( nodeID );

			//Validate the user
			var user = GetValidatedUser( existingNode, 0 ).user;
			if ( user == null ) { return Fail( "You must login to create or edit a level" ); }

			if ( existingNode == null || existingNode.Id == 0 )
			{
				return Fail( "Invalid Level Parent ID." );
			}

			new ResourceV2Services().PublishRelatedChildContent( existingNode, user );

			return new JSON() { data = true, valid = true, status = "Publishing all new related resources", extra = new { title = "Refreshing" } };

		}

		#endregion
		#region standards
		//Add standards
		[WebMethod( EnableSession = true )]
		public JSON Standards_Add( int nodeID, int targetID, string contentItemType, List<StandardDTO> standards )
		{
			//Ge the node
			var content = curriculumService.Get( targetID );

			//Validate the user
			var permissions = GetValidatedUser( content, content.CreatedById );
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

			int curriculumID = curriculumService.GetCurriculumIDForNode( content );
			//Save standards
			curriculumService.ContentStandard_Add( curriculumID, nodeID, user.Id, addedStandards );

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
			var permissions = GetValidatedUser( content, content.CreatedById );
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
			var valid = new CurriculumServices().ContentStandard_Update( standard.recordID, standard.alignmentID, standard.usageID, user.Id, ref status );

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
			var permissions = GetValidatedUser( content, content.CreatedById );
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
			new CurriculumServices().ContentStandard_Delete( parentID, user.Id, recordID, ref status );

			//Return JSON
			return GetContentStandards( parentID );
		}

		/// <summary>
		/// List all nodes with standard within list
		/// </summary>
		/// <param name="parentID"></param>
		/// <param name="standardId"></param>
		/// <returns></returns>
		[WebMethod( EnableSession = true )]
		public JSON Standard_List( int parentID, int standardId )
		{
			var status = "";

			//Get node

			//Update standard
			var valid = true;
			if ( !valid )
			{
				return Fail( status );
			}

			//Return JSON
			return GetContentStandards( parentID );
		}

		#endregion
		#region attachments
		/// <summary>
		/// Save URL attachment 
		/// (File attachment is handled in /Controls/Curriculum/CurriculumFileUpload.aspx--maybe that should be moved here?)
		/// </summary>
		/// <param name="nodeID"></param>
		/// <param name="attachmentID"></param>
		/// <param name="title"></param>
		/// <param name="accessID"></param>
		/// <param name="url"></param>
		/// <param name="featured"></param>
		/// <returns></returns>
		[WebMethod( EnableSession = true )]
		public JSON Attachment_SaveURL( int nodeID, int attachmentID, string title, string summary, int accessID, string url, int usageRightsId, string usageRightsUrl, bool featured )
		{
			return ManageAttachment( "saveurl", nodeID, attachmentID, title, summary, accessID, url, usageRightsId, usageRightsUrl, featured );
		}

		//Delete attachment
		[WebMethod( EnableSession = true )]
		public JSON Attachment_Delete( int nodeID, int attachmentID )
		{
			//Validate user
			//if passing 0, 0, then no real validation is done, so why not just get user??
			var user = GetValidatedUser( 0, 0 ).user;
			if ( user == null || user.Id == 0)
			{
				return Fail( "You must log in to use this feature." );
			}

			var status = "";
			var attachment = contentService.Get( attachmentID );
			if ( attachment == null || attachment.Id == 0 )
			{
				return Fail( "Error attachment was not found" );
			}

			//should not 
			//return ManageAttachment( "delete", nodeID, attachmentID, "", 0, "", 0, "", false );
			//need to check return from delete, and use status if not successful
			bool valid1 = contentService.Delete( attachment.Id, user.Id, ref status );
			//????????????
			//var valid2 = true;
			//var status1 = "";
			//var status2 = "";

			//Not sure which of these is appropriate
			//??RI should handle these??
			//valid1 = curriculumService.ContentReferenceDelete( attachmentID, ref status1 );
			//valid2 = curriculumService.ContentSupplementDelete( attachmentID, ref status2 );

			if ( valid1 )
			{
				//Return confirmation
				return GetAttachments( nodeID );
			}
			else
			{
				return Fail( status );
			}

		}

		//Update attachment without touching document/URL
		[WebMethod( EnableSession = true )]
		public JSON Attachment_UpdateData( int nodeID, int attachmentID, string title, string summary, int accessID, int usageRightsId, string usageRightsUrl, bool featured )
		{
			return ManageAttachment( "updatedata", nodeID, attachmentID, title, summary, accessID, "", usageRightsId, usageRightsUrl, featured );
		}

		//Alter sort order of documents
		[WebMethod( EnableSession = true )]
		public JSON Attachment_Reorder( int nodeID, int attachmentID, string direction )
		{
			//seems unnecessary to manage generally
			//return ManageAttachment( "reorder", nodeID, attachmentID, direction, 0, "", false );
			//Validate user
			var user = GetValidatedUser( 0, 0 ).user;
			if ( user == null )
			{
				return Fail( "You must log in to use this feature." );
			}

			var attachment = contentService.Get( attachmentID );
			if ( attachment == null || attachment.Id == 0 )
			{
				return Fail( "Error loading attachment" );
			}

			var node = curriculumService.GetACurriculumNode( nodeID );
			if ( node == null || node.Id == 0 )
			{
				return Fail( "Error: Invalid level ID" );
			}
			//Get siblings
			var siblings = node.ChildItems;

			//If there are any duplicate sort orders among siblings, reorder them
			if ( siblings.Select( m => m.SortOrder ).Distinct().ToList().Count() < siblings.Count() )
			{
				//Do the reordering
				ReorderAttachments( null, null, nodeID, user );
				//Then refresh
				siblings = curriculumService.GetACurriculumNode( nodeID ).ChildItems;
			}

			//Get item to be moved
			var swapee = new ContentItem() { Id = 0 };
			if ( direction == "up" )
			{
				swapee = siblings.OrderBy( m => m.SortOrder ).Where( m => m.SortOrder < attachment.SortOrder ).LastOrDefault();
			}
			else if ( direction == "down" )
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

		//Manage attachment
		private JSON ManageAttachment( string method, int nodeID, int attachmentID, string title, string summary, int accessID, string url, int usageRightsId, string usageRightsUrl, bool featured )
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
					var nodePermissions = GetValidatedUser( node, node.CreatedById );
					if ( !nodePermissions.write )
					{
						return Fail( "You don't have permission to edit that level." );
					}

					//Validate data
					//title = util.ValidateText( title, 3, "Title", ref valid, ref status );
					//if ( !valid ) { return Fail( status ); }

					url = util.ValidateURL( url, false, ref valid, ref status );
					if ( !valid ) { return Fail( status ); }
					string message = "";
					if ( ValidateAttachment( title, usageRightsId, usageRightsUrl, ref message ) == false )
					{
						return Reply( null, false, message, null );
					}
					//Fetch item if available - returns new if not found
					var item = contentService.Get( attachmentID );

					//Update fields
					item.Id = attachmentID;
					item.Title = title;
					item.Summary = summary;
					item.DocumentUrl = url;
					item.PrivilegeTypeId = accessID;
					item.LastUpdatedById = user.Id;
					item.StatusId = ContentItem.INPROGRESS_STATUS;
					item.SortOrder = featured ? -1 : ( node.ChildItems.Count() * 10 ) + 10;
					item.TypeId = 41; //URL
					item.ConditionsOfUseId = usageRightsId;
					item.UsageRightsUrl = usageRightsUrl;

					//Save to database
					if ( attachmentID == 0 )
					{
						item.ParentId = nodeID;
						item.CreatedById = user.Id;
						contentService.Create_ef( item, ref status );

						//if node is published, then auto publish this item
						if ( node.StatusId == 5 && node.ResourceIntId > 0 )
						{
							//should be able to just do the call for the parent
							new ResourceV2Services().PublishRelatedChildContent( node, user );
						}
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
							return ManageAttachment( "updatedata", nodeID, item.Id, title, summary, accessID, url, usageRightsId, usageRightsUrl, featured ); //several things need to happen to make featuring work
						}
						else
						{
							//Create thumbnail
							//new LRWarehouse.DAL.ResourceThumbnailManager().CreateThumbnailAsync( "content-" + item.Id, item.DocumentUrl, true, 3 );
							ThumbnailServices.CreateThumbnail( "content-" + item.Id, item.DocumentUrl, true );

							//Return confirmation
							return GetAttachments( nodeID );
						}
					}
					else
					{
						return Fail( status );
					}
				}
				//Modify ==================================
				else
				{
					var attachment = contentService.Get( attachmentID );
					if ( attachment == null || attachment.Id == 0 )
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
					var nodePermissions = GetValidatedUser( node, node.CreatedById );
					if ( !nodePermissions.write )
					{
						return Fail( "You don't have permission to edit that level." );
					}

					//Update data 
					if ( method == "updatedata" )
					{
						//Validate input
						string message = "";
						//title = util.ValidateText( title, 3, "Title", ref valid, ref status );

						//if ( !valid ) { return Reply( null, false, status, null ); }

						if ( ValidateAttachment( title, usageRightsId, usageRightsUrl, ref message ) == false )
						{
							return Reply( null, false, message, null );
						}

						//Update data
						attachment.Title = title;
						attachment.Summary = summary;
						//update to inprogress (could have been draft from multi-upload
						attachment.StatusId = ContentItem.INPROGRESS_STATUS;
						attachment.PrivilegeTypeId = accessID;
						attachment.UsageRightsId = usageRightsId;
						attachment.UsageRightsUrl = usageRightsUrl;
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
						else if ( !featured && attachment.SortOrder == -1 )
						{
							curriculumService.RemovedItemAsFeatured( attachmentID );
							attachment.SortOrder = 0;
							//ReorderAttachments( attachment, node.ChildItems.Where( m => m.SortOrder != -1 && m.Id != attachment.Id ).FirstOrDefault(), node.Id, user );
							ReorderAttachments( null, null, node.Id, user );
						}

						//Recreate thumbnail
						//new LRWarehouse.DAL.ResourceThumbnailManager().CreateThumbnailAsync( "content-" + attachment.Id, attachment.DocumentUrl, true, 3 );
						ThumbnailServices.CreateThumbnail( "content-" + attachment.Id, attachment.DocumentUrl, true );

						//Return data
						return GetAttachments( nodeID );
					}

				}

				//Return confirmation
				return Fail( "Error: Invalid operation" );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "There was an error managing attachments for learning list node " + nodeID + " and attachment " + attachmentID );
				return Reply( ex.Message, false, "There was an error processing your request.", ex.ToString() );
			}
		}

		private bool ValidateAttachment( string title, int usageRightsId, string usageRightsUrl, ref string message )
		{
			var valid = true;
			var status = "";
			//Validate inputs
			var util = new UtilityService();
			message = "";
			//-should not return one error at a time
			//Title
			title = util.ValidateText( title, 3, "Title", ref valid, ref status );
			if ( !valid )
			{
				message += status + "\r\n";
				//return Fail( status ); 
			}
			//Description
			//summary = util.ValidateText( summary, 10, "Summary", ref valid, ref status );
			//if ( !valid )
			//{
			//	message += status + "\r\n";
			//	//return Fail( status ); 
			//}
		
			if ( usageRightsId == ContentItem.READ_THE_FINE_PRINT_CCOU )
			{
				//url required
				//Usage Rights URL
				if ( usageRightsUrl.Length == 0 )
				{
					status = "A URL must be entered when selecting Usage Rights of 'Read the Fine Print'";
					message += status + "\r\n";
					//return Fail( status );
				}
				else
				{
					usageRightsUrl = util.ValidateURL( usageRightsUrl, false, ref valid, ref status );
					if ( !valid )
					{
						message += status + "\r\n";
						//return Fail( status ); 
					}
				}
			}
			if ( message.Length > 0 )
				return false;
			else
				return true;
			
		}
		#endregion
		#region news
		//Post a news item
		[WebMethod( EnableSession = true )]
		public JSON Save_News( int nodeID, string text )
		{
			return Save_NewsItem( nodeID, text, 0 );
		}

		[WebMethod( EnableSession = true )]
		public JSON Save_NewsItem( int nodeID, string text, int newsID )
		{
			bool valid = true;
			string status = "";
			//Validate the user
			var user = GetValidatedUser( nodeID, 0 ).user;
			if ( user == null ) { return Fail( "You must login to post a news item." ); }

			//Validate the input (May need to use a different method due to HTML-ey nature of the input
			text = new UtilityService().ValidateText( text, 10, "News", ref valid, ref status );
			if ( !valid ) { return Fail( status ); }

			//Add/Update the news item
			try
			{
				if ( newsID == 0 )
				{
					var newNewsID = curriculumService.Curriculum_AddHistory( nodeID, text, user.Id );
					return Reply( newNewsID, true, "okay", null );
				}
				else
				{
					var success = curriculumService.Curriculum_UpdateHistory( newsID, text, user.Id );
					return Reply( success, true, "okay", newsID );
				}
			}
			catch ( Exception ex )
			{
				return Reply( "", false, "Sorry, there was a problem saving your news item.", ex.Message );
			}
		}

		//Get news items
		[WebMethod]
		public JSON Get_News( int nodeID )
		{
			try
			{
				var newsItems = curriculumService.Curriculum_GetHistory( nodeID );
				return Reply( newsItems, true, "okay", null );
			}
			catch
			{
				return Reply( null, false, "No news items found.", null );
			}
		}

		//Delete a news item
		[WebMethod( EnableSession = true )]
		public JSON Delete_News( int nodeID, int newsID )
		{
			bool valid = true;
			string status = "";
			//Validate the user
			var user = GetValidatedUser( nodeID, 0 ).user;
			if ( user == null ) { return Fail( "You must login to delete a news item." ); }

			//Need a way to ensure the user has rights to delete the news item?

			//Do the delete
			try
			{
				//This method doesn't exist
				//var success = curriculumService.Curriculum_DeleteHistory( newsID );
				return Reply( null, false, "Not implemented yet", null );
			}
			catch ( Exception ex )
			{
				return Reply( "", false, "Sorry, there was a problem deleting the news item.", ex.Message );
			}
		}

		#endregion

		[WebMethod( EnableSession = true )]
		public JSON RegenerateThumbnail( int nodeID, int contentID, string contentURL )
		{
			var user = GetValidatedUser( nodeID, 0 );
			if ( user.write )
			{
				if ( contentID > 0 && !string.IsNullOrWhiteSpace( contentURL ) )
				{
					var title = "content-" + contentID;
					//new LRWarehouse.DAL.ResourceThumbnailManager().CreateThumbnailAsync( title, contentURL, true, 1 );
					ThumbnailServices.CreateThumbnail( title, contentURL, true );
					return new JSON() { data = true, valid = true, status = "Regenerating...", extra = new { title = title } };
				}
				else
				{
					return new JSON() { data = null, valid = false, status = "Incorrect parameters - please double-check.", extra = new { nodeID = nodeID, contentID = contentID, contentURL = contentURL } };
				}
			}
			else
			{
				return new JSON() { data = null, valid = false, status = "You are not authorized to do that.", extra = null };
			}
		}

		#region Helper Methods
		//Get user and permissions
		public ValidationPermissions GetValidatedUser( int entityID, int createdByID )
		{
			ContentItem entity = new ContentItem();
			if (entityID > 0)
				entity = curriculumService.GetACurriculumNode(entityID);
			return GetValidatedUser( entity, createdByID );
		}
		public ValidationPermissions GetValidatedUser( ContentItem entity, int createdByID )
		{
			//Get the user
			var user = AccountServices.GetUserFromSession( Session );
			if ( user == null || user.Id == 0 )
			{
				return new ValidationPermissions() { user = null, read = false, write = false };
			}
			//If no inputs, skip permission check
			if ( entity == null || entity.Id == 0 || createdByID == 0 )
			{
				return new ValidationPermissions() { user = user, read = true, write = true };
			}

			//Check global admin permissions
			//Temporary hack
			if (user.Id == 2 || user.Id == 22 || user.TopAuthorization < (int)EUserRole.StateAdministrator)
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
				var privileges = SecurityManager.GetGroupObjectPrivileges( user, "IOER.controls.Authoring" );
				//ContentPartner partner = ContentServices.ContentPartner_Get( entityID, user.Id );

				//Check for partner association with top level node
				var topNodeID = 0;
				if ( entity.ParentId > 0 )
					topNodeID = curriculumService.GetCurriculumIDForNode( entity );
				else
					topNodeID = entity.Id;
				var topPartner = ContentServices.ContentPartner_Get( topNodeID, user.Id );

				if ( topPartner == null || topPartner.PartnerTypeId == 0 )
				{
					return new ValidationPermissions() { user = user, read = false, write = false };
				}
				else
				{
					var canRead = topPartner.PartnerTypeId > 0;
					var canWrite = topPartner.PartnerTypeId >= 2 || privileges.WritePrivilege > ( int ) ILPathways.Business.EPrivilegeDepth.Region;
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
			var code = "";
			try
			{
				code = string.IsNullOrWhiteSpace( item.NotationCode ) ? item.Description.Substring( 0, 20 ) + "..." : item.NotationCode;
			}
			catch { }
			return new StandardDTO()
			{
				recordID = item.StandardRecordId,
				standardID = item.StandardId,
				contentID = item.ContentId,
				code = code,
				text = item.Description,
				isContentStandard = isContentStandard,
				usageID = 1,
				alignmentID = item.AlignmentTypeCodeId,
			};
		}
		public StandardDTO GetStandardDTO( Content_StandardSummary item, bool isContentStandard )
		{
			var code = "";
			try
			{
				code = string.IsNullOrWhiteSpace( item.NotationCode ) ? item.Description.Substring( 0, 20 ) + "..." : item.NotationCode;
			}
			catch { }
			return new StandardDTO()
			{
				recordID = item.StandardRecordId,
				standardID = item.StandardId,
				contentID = item.ContentId,
				code = code,
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
				usageRights = "";
			}
			public int attachmentID { get; set; }
			public string title { get; set; }
			public string summary { get; set; }
			public int accessID { get; set; }
			public string url { get; set; }
			public bool featured { get; set; }
			public string attachmentType { get; set; }
			public int usageRightsId { get; set; }
			public string usageRights { get; set; }
			public string usageRightsUrl { get; set; }
			public List<StandardDTO> standards { get; set; }
		}

		#endregion
	}
}
