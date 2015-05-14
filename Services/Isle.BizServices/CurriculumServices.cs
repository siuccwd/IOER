using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

//using Isle.DataContracts;
using Isle.DTO;

using ILPathways.Business;
using ILPathways.Utilities;
using ILPathways.Common;
using MyManager = ILPathways.DAL.ContentManager;
using DocManager = ILPathways.DAL.DocumentStoreManager;
using DBM = ILPathways.DAL.DatabaseManager;

using EFDAL = IoerContentBusinessEntities;
using EFManager = IoerContentBusinessEntities.EFContentManager;
using AcctManager = Isle.BizServices.AccountServices;
using GroupManager = Isle.BizServices.GroupServices;
using dto = Isle.DTO;
using dtoc = Isle.DTO.Content;
using ThisUser = LRWarehouse.Business.Patron;
using LRWarehouse.Business;
using ResourceManager = LRWarehouse.DAL.ResourceManager;

namespace Isle.BizServices
{
    public class CurriculumServices : ContentServices
    {
        #region == methods for groups of curriculum ==

        /// <summary>
        /// use the provided id to return curricula in the group
        ///ex ISBE - the interface may have to have a hard-coded id for this action. 
        ///For example /Curricula/1/ISBE
        ///The id will relate to a Content item with children curricula
        /// </summary>
        /// <param name="curriculumGroupId"></param>
        /// <returns>List of Content items (curricula)</returns>
        public List<ContentItem> Curriculum_GetGroup( int curriculumGroupId )
        {
            List<ContentItem> list = new List<ContentItem>();


            return list;
        }


        #endregion

        #region == curriculum methods  ==

        public static ContentItem CreateIsbeHierarchy( int contentId, int modules )
        {
            return new EFManager().CreateIsbeHierarchy( contentId, modules );
        }

        /// <summary>
        /// Get a curriculum outline/hierarchy
        /// The 
        /// </summary>
        /// <param name="pContentId"></param>
        /// <returns></returns>
        public ContentNode GetPublicCurriculumOutline( int pContentId, bool allowCaching )
        {
            return GetCurriculumOutline( pContentId, true, allowCaching );
        }

        public ContentNode GetPublicCurriculumOutline( int pContentId, string userGuid )
        {
            return GetCurriculumOutline( pContentId, true, true );
        }

        public ContentNode GetPublicCurriculumOutline( int pContentId, string userGuid, bool allowCaching )
        {
            //if user has access, get all
            //note need to check if author!
            ContentPartner cp = ContentPartner_Get( pContentId, userGuid );
            if (cp != null && cp.PartnerTypeId > 1)
                return GetCurriculumOutline( pContentId, false, allowCaching );
            else
                return GetCurriculumOutline( pContentId, true, allowCaching );
        }

        /// <summary>
        /// Get a curriculum outline for edit use - calling will verify user for use
        /// NOTE: USED BY OLD AUTHOR TOOL ONLY, NOT LEARNING LIST EDITOR
        /// </summary>
        /// <param name="pContentId"></param>
        /// <returns></returns>
        public ContentItem GetCurriculumOutlineForEditOld( int pContentId )
        {
            return GetCurriculumOutlineOLD( pContentId, false );
        }
        public ContentNode GetCurriculumOutlineForEdit( int pContentId )
        {
            return GetCurriculumOutline( pContentId, false, false );
        }

        public int Curriculum_AddHistory( int contentId, string message, int createdById )
        {
            MyManager mgr = new MyManager();
            string statusMessage = "";

            return ContentHistory_Create( contentId, "Content News", message, createdById, ref statusMessage );
        }

        public bool Curriculum_UpdateHistory( int id, string message, int createdById )
        {
            MyManager mgr = new MyManager();
            string statusMessage = "";

            return ContentHistory_Update( id, message, createdById, ref statusMessage );
        }

        public List<CommentDTO> Curriculum_GetHistory( int contentId )
        {

            return ContentHistory_Select( contentId, "Content News" );

        }


        /// <summary>
        /// Return collection of all learning lists to which the user has edit access 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<LearningListNode> Learninglists_SelectUserEditableLists( int userId )
        {
            return EFManager.SelectUserEditableLearningLists( userId );

        }

        /// <summary>
        /// Return collection of all learning lists to which the user has at least read access 
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<LearningListNode> Learninglists_SelectUserLists( int userId )
        {
            return EFManager.SelectUserLearningLists( userId );

        }

        public List<ObjectMember> Learninglist_AllUsers( int listId )
        {
            return EFManager.Learninglist_SelectUsers( listId );
        }

        #endregion


        #region == methods for curriculum node ==
        /// <summary>
        /// Delete a curriculum node.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool DeleteCurriculumNode( int pId, ref string statusMessage )
        {
            return this.Delete( pId, ref statusMessage );
        }//

        public ContentItem GetTheCurriculumNode( int pNodeId )
        {
            ThisUser user = new ThisUser();
            //?????????????????????????
            //why is there not a method including user?
            NodeRequest request = new NodeRequest();
            request.AllowCaching = true;
            request.ContentId = pNodeId;

            //just want privileges, docs, and standards
            //just be proative and make latter properties!
            request.DoCompleteFill = false;//?????
            request.IsEditView = false;
            request.IncludeStandards = true;
            request.IncludeDocumentNodes = true;

            request.IncludeChildNodes = false;
            //use the stored proc version or the EF version??
            return this.Get(pNodeId);

            //return GetACurriculumNode( request, user );
        }//

        /// <summary>
        /// Get node detail 
        /// - assume public view and guest user
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <returns></returns>
        public ContentItem GetACurriculumNode( int pNodeId )
        {
            ThisUser user = new ThisUser();

            return GetACurriculumNode( pNodeId, user, true );
       }//

        /// <summary>
        /// Get node detail 
        /// - assume public view 
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ContentItem GetACurriculumNode( int pNodeId, int userId )
        {
            ThisUser user = new ThisUser();
            if ( userId > 0 )
            {
                user = AccountServices.GetUser( userId );
            }
            return GetACurriculumNode( pNodeId, user, true );
        }//

        /// <summary>
        /// Get node detail 
        /// - assume public view 
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ContentItem GetACurriculumNode( int pNodeId, ThisUser user )
        {
            return GetACurriculumNode( pNodeId, user, true );
        }//

        /// <summary>
        /// Get node detail 
        /// - assume public view 
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="user"></param>
        /// <param name="allowCaching"></param>
        /// <returns></returns>
        public ContentItem GetACurriculumNode( int pNodeId, ThisUser user, bool allowCaching )
        {
            NodeRequest request = new NodeRequest();
            request.AllowCaching = allowCaching;
            request.ContentId = pNodeId;
            //may need more granularity, don't want everything for view
            request.DoCompleteFill = true;  //?????
            request.IsEditView = false;
            request.IncludeStandards = true;

            return GetACurriculumNode( request, user );
        }//

        /// <summary>
        /// Get node detail 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ContentItem GetACurriculumNode( NodeRequest request, ThisUser user )
        {
            return GetCurriculumNode( request, user );
        }//

        /// <summary>
        /// Get node detail 
        /// - assume edit view 
        /// </summary>
        /// <param name="pNodeId"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public ContentItem GetCurriculumNodeForEdit( int pNodeId, ThisUser user )
        {
            NodeRequest request = new NodeRequest();

            request.IsEditView = true;
            request.AllowCaching = false;
            request.ContentId = pNodeId;

            //just want privileges, docs, and standards
            //just be proative and make latter properties!
            request.DoCompleteFill = false;//?????

            //need a variation - only want direct standards - especially for top level
            request.IncludeStandards = true;
            request.IncludeChildNodes = false;
            request.IncludeDocumentNodes = true;
            request.IncludeConnectorNodes = false;

            return GetCurriculumNode( request, user );
        }


        /// <summary>
        /// Retrieve the curriculum id for the provided content item
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int GetCurriculumIDForNode( ContentItem node )
        {
            return  GetTopIdForHierarchy( node );
        }

        /// <summary>
        /// sets document associated with the provided content item as the featured document (autopreview)
        /// Currently this is accomplished by setting sort order to zero. Any other items under the parent with sort order equal to zero, will have the sort order set to the default (10).
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="contentId"></param>
        /// <returns>successful is OK, otherwise an error message</returns>
        public string SetAsFeaturedItem( int parentId, int contentId )
        {
            string msg = new MyManager().SetAsFeaturedItem( parentId, contentId );
            return msg;
        }

    
        /// <summary>
        /// Set the sort order to the default for the provided content 
        /// - maybe we should just set all to the default using parent?
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns>successful is OK, otherwise an error message</returns>
        public string RemovedItemAsFeatured( int contentId )
        {
            string msg = new MyManager().RemoveFeaturedItem( contentId );
            return msg;
        }

        #endregion

        #region Content partners

        #endregion

        #region SUBSCRIPTIONS
        /// <summary>
        /// Check if user already has a subscription.
        /// return true if found, else false
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool ContentSubscription_Exists( int contentId, int userId )
        {
            if ( contentId == 0 || userId == 0 )
                return false;

            EFManager mgr = new EFManager();
            ObjectSubscription os = mgr.ContentSubscription_Get( contentId, userId );
            if ( os != null && os.Id > 0 )
                return true;
            else
                return false;
        }

        public ObjectSubscription ContentSubscriptionGet( int contentId, int userId )
        {
            if ( contentId == 0 || userId == 0 )
                return new ObjectSubscription(); 

            EFManager mgr = new EFManager();
            return mgr.ContentSubscription_Get( contentId, userId );
        }

        /// <summary>
        /// Add a new content subscription/membership
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentSubScription_Create( int contentId, int userId, int typeId, ref string statusMessage )
        {
            EFManager mgr = new EFManager();
            int id = mgr.ContentSubscription_Add( contentId, userId, typeId, ref statusMessage );
            return id;
        }

        public bool ContentSubScription_Update( int subscriptionId, int typeId, ref string statusMessage )
        {
            EFManager mgr = new EFManager();

            return mgr.ContentSubscription_Update( subscriptionId, typeId, ref statusMessage ); ;
        }

        public bool ContentSubscription_Delete( int subscriptionId, ref string statusMessage )
        {
            EFManager mgr = new EFManager();
            return mgr.ContentSubscription_Delete( subscriptionId, ref statusMessage );
        }

        #endregion


        #region  === Content.Standard
        public static int ContentStandard_Add( List<ContentStandard> standards )
        {
            int cntr = 0;
            int added = 0;
            foreach ( ContentStandard standard in standards )
            {
                cntr++;
                if ( EFManager.ContentStandard_Add( standard ) > 0 )
                    added++;
            }
            //some check to ensure all were added.
            return added;
        }

        public static int ContentStandard_Add( int contentId, int standardId, int alignmentTypeCodeId, int usageTypeId, int createdById )
        {
            return EFManager.ContentStandard_Add( contentId, standardId, alignmentTypeCodeId, usageTypeId, createdById );
        }
        public static bool ContentStandard_Update( int id, int alignmentTypeCodeId, int usageTypeId, int lastUpdatedById, ref string statusMessage )
        {
            return EFManager.ContentStandard_Update( id, alignmentTypeCodeId, usageTypeId, lastUpdatedById, ref statusMessage );
        }

        /// <summary>
        /// Delete a Content.standard
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool ContentStandard_Delete( int id, ref string statusMessage )
        {
            return EFManager.ContentStandard_Delete( id, ref statusMessage );
        }

        /// <summary>
        /// Select all Content_StandardSummary for a content item
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static List<Content_StandardSummary> ContentStandard_Select( int contentId )
        {
            return EFManager.Fill_ContentStandards( contentId );
        }
        #endregion


        #region  === Content.Tag

        public int ContentTag_Add( int contentId, int tagValueId, int createdById )
        {
            return EFManager.ContentTag_Add( contentId, tagValueId, createdById );
        }

        /// <summary>
        /// Delete a Content.tag by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool ContentTag_Delete( int id, ref string statusMessage )
        {
            return EFManager.ContentTag_Delete( id, ref statusMessage );
        }
        /// <summary>
        /// Delete a Content.tag by contentId and tagValueId
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="tagValueId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public static bool ContentTag_Delete( int contentId, int tagValueId, ref string statusMessage )
        {
            return EFManager.ContentTag_Delete( contentId, tagValueId, ref statusMessage );
        }
        /// <summary>
        /// Select all Content_TagSummary for a content item
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public static List<dtoc.Content_TagSummary> ContentTags_Select( int contentId )
        {
            return EFManager.Fill_ContentTags( contentId );
        }
        #endregion
        #region  === downloading nodes
        public ContentItem DownloadCurriculumNode( int pNodeId, ThisUser user, bool includingChildren, bool doCompleteFill )
        {
            return DownloadCurriculumNode2( pNodeId, user, includingChildren, doCompleteFill );
        }//



        #endregion

        #region WIP methods
        //These methods aren't truly implemented yet, but serve as placeholders
        //They don't necessarily need to be static if it helps to do them normally
        //I'm also not committed to the method/variable names below, so feel free to adjust them to fit your desired pattern/schema

        //List pending members for a learning list
        public List<ObjectMember> LearningListMembers_ListPending( int learningListId )
        {
          throw new NotImplementedException( "Sorry, listing pending members is not implemented yet." );
        }

        //Deny a pending membership
        //Note: userId is the ID of the user performing the denial
        //Note: customMessage is sent to the denied member, presumably to indicate why they were denied. We don't -have- to implement this part.
        //Should return a bool indicating whether or not the denial was successful, and a status message explaining any failure. 
        //The status message will be hidden from the user but findable to us for debugging purposes.
        public bool LearningListMember_DenyPending( int learningListId, int userId, int pendingMemberId, string customMessage, ref string status )
        {
          throw new NotImplementedException( "Sorry, denying memberships is not implemented yet." );
        }

        //Invite an existing IOER user
        //Again, userId is the user performing the action
        //roleId is the role to be assigned to the invited person once they are approved for membership
        //roleId should correspond to the organization's roles. If this is an issue, let me know.
        //customMessage is ideally sent to the invitee
        //status should explain any failure and will be hidden from the user
        public bool InviteExistingUser( int learningListId, int userId, int inviteeId, int roleId, string customMessage, ref string status )
        {
          throw new NotImplementedException( "Sorry, inviting existing users is not implemented yet." );
        }

        //Invite a non-existing user by email
        //userId is the performing user
        //The email should already be validated by this point but feel free to validate it further
        //roleId is an organization role for the member to have once they're all finished
        //customMessage would make good email filler text
        //status is for us, not the users
        //should return a bool indicating successful invitation or failure to invite
        public bool InviteNewUser( int learningListId, int userId, string inviteeEmail, int roleId, string customMessage, ref string status )
        {
          throw new NotImplementedException( "Sorry, inviting new users is not implemented yet." );
        }

        //Gets all organization members of a certain role
        //The current hack below accomplishes this, but is not very efficient
        public List<ObjectMember> LearningList_AllUsers( int learningListId, int roleId )
        {
          //May need to capitalize the L in the method below?
          return Learninglist_AllUsers( learningListId ).Where( m => m.MemberTypeId == roleId ).ToList();
        }

        #endregion

    }
}
