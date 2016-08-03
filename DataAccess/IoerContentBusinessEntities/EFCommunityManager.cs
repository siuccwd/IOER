using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AutoMapper;

using IB =  ILPathways.Business;
using ILPathways.Utilities;
using DTO = Isle.DTO;

namespace IoerContentBusinessEntities
{
    public class EFCommunityManager
    {
        static string thisClassName = "EFCommunityManager";
        static IsleContentContext ctx = new IsleContentContext();
        static int MAX_POSTINGS = 100;
        public EFCommunityManager() { }

        #region Community =======================

        public int CommunityAdd( IB.Community entity, ref string message )
        {
            Community com = new Community();

            try
            {
                com = Community_FromMap( entity );
                com.Created = System.DateTime.Now;
                com.LastUpdated = System.DateTime.Now;
                ctx.Communities.Add( com );

                // submit the change to database
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return com.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CommunityAdd()" );
				message = ex.Message;
                return 0;
            }
        }

		/// <summary>
		/// Update a community
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public bool CommunityUpdate( IB.Community entity, ref string message )
        {
            bool isValid = false;
            try
            {
                Community com = ctx.Communities.SingleOrDefault( s => s.Id == entity.Id );
                if ( com != null && com.Id > 0 )
                {
                    com = Community_FromMap( entity );
                    com.LastUpdated = System.DateTime.Now;

                    ctx.SaveChanges();

                    isValid = true;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CommunityUpdate()" );
				message = ex.Message;
                return false;
            }
            return isValid;
        }

		/// <summary>
		/// Delete a community
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
        public bool CommunityDelete( int id, ref string message )
        {
            bool isSuccessful = false;
			try
			{
				Community item = ctx.Communities.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ctx.Communities.Remove( item );
					ctx.SaveChanges();
					isSuccessful = true;
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Community_Delete()" );
				message = ex.Message;
			}
            return isSuccessful;
 }

        /// <summary>
        /// Get a community
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IB.Community Community_Get( int id )
        {
            int maxPosts = UtilityManager.GetAppKeyValue( "maxCommunityPosts", MAX_POSTINGS );
            return Community_Get( id, maxPosts );
        }

        /// <summary>
        /// Get a community
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IB.Community Community_Get( int id, int recentPosts )
        {
            IB.Community entity = new IB.Community();
            using ( var context = new IsleContentContext() )
            {

                Community item = context.Communities.SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    entity = Community_ToMap( item );
                    //MP- temp: all postitems are returned asc, either have to resort or just, do the explicit select
                    entity.Postings = PostingView_Select( id, recentPosts );
                    
                }
            }
            return entity;
        }

        /// <summary>
        /// Get all communities - may want to add paging later!
        /// </summary>
        /// <returns></returns>
        public static List<IB.Community> Community_GetAll( int recentPosts )
        {

            List<IB.Community> list = new List<IB.Community>();
            IB.Community entity = new IB.Community();
            using ( var context = new IsleContentContext() )
            {
                List<Community> eflist = context.Communities
                            .Where( s => s.IsActive == true)
                            .OrderBy( s => s.Title ).ToList();

                if ( eflist != null && eflist.Count > 0 )
                {
                    foreach ( Community efom in eflist )
                    {
                        entity = Community_ToMap( efom );
                        //additions like total posts, last post, or n most recent

                        entity.Postings = PostingView_Select( efom.Id, recentPosts );
                       

                        list.Add( entity );
                    }
                }
            }
            return list;
        }

        public static Community Community_FromMap( IB.Community fromEntity )
        {
            Community to = new Community();
            to.Id = fromEntity.Id;
            to.Title = fromEntity.Title;
            to.Description = fromEntity.Description;
            if (fromEntity.OrgId > 0)
                to.OrgId = fromEntity.OrgId;
            if ( fromEntity.ContactId > 0 )
                to.ContactId = fromEntity.ContactId;
            to.PublicAccessLevel = fromEntity.PublicAccessLevelInt;
            to.OrgAccessLevel = fromEntity.OrgAccessLevelInt;

            to.ImageUrl = fromEntity.ImageUrl;
            to.IsActive = fromEntity.IsActive;
			to.IsModerated = fromEntity.IsModerated;

            to.Created = fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = fromEntity.LastUpdatedById;
            return to;
        }

        public static IB.Community Community_ToMap( Community fromEntity )
        {
            IB.Community to = new IB.Community();
            to.Id = fromEntity.Id;
            to.Title = fromEntity.Title;
            to.Description = fromEntity.Description;
            to.OrgId = fromEntity.OrgId != null ? ( int ) fromEntity.OrgId : 0;
            if ( to.OrgId > 0 )
            {
                //prob get org, at least a title
                //or minimize the overhead - or use a view!
            }
            to.ContactId = fromEntity.ContactId != null ? ( int ) fromEntity.ContactId : 0;
            to.PublicAccessLevel = ( IB.EObjectAccessLevel ) fromEntity.PublicAccessLevel;
            to.OrgAccessLevel = ( IB.EObjectAccessLevel ) fromEntity.OrgAccessLevel;

            to.ImageUrl = fromEntity.ImageUrl;
            to.IsActive = fromEntity.IsActive == null ? false : ( bool )fromEntity.IsActive;
			to.IsModerated = fromEntity.IsModerated == null ? false : ( bool )fromEntity.IsModerated;

            to.Created = (System.DateTime) fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = ( System.DateTime ) fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            return to;
        }

        #endregion

        #region Community Community_PostItem =======================
        //14-03-10 mparsons - change to use PostItem as join tzble to allow posting one message to multiple communities
        public static int Community_AddPostings( List<int> communities, string message, int pCreatedById )
        {
            if ( communities != null && communities.Count > 0 )
            {
                //create posting
                int postId = PostingAdd( message, pCreatedById, 0 );

                foreach ( int id in communities )
                {
                    Community_PostItem_Add( id, postId, pCreatedById );
                }
                return 1;
            }
            else
            {
                return 1;
            }
   }

        public static int Community_AddPosting( int communityId, string message, int pCreatedById )
        {
            return  Community_AddPosting( communityId, message, pCreatedById, 0 );
        }

        public static int Community_AddPosting( int communityId, string message, int pCreatedById, int relatedPostingId )
        {
            if ( communityId > 0 )
            {
				DTO.ObjectMember mbr = new DTO.ObjectMember();
				bool isApproved = true;

				//get community and detemine if moderation needed
				//if true, will add posting but not the postItem
				IB.Community c = Community_Get( communityId );
				if ( c.IsModerated )
				{
					//check user privileges
					mbr = CommunityMember_Get( communityId, pCreatedById );
					if (mbr == null || mbr.Id == 0 || mbr.MemberTypeId < 3)
						isApproved = false;
				}
                //create posting
				int postId = PostingAdd( message, pCreatedById, relatedPostingId, isApproved );

                Community_PostItem_Add( communityId, postId, pCreatedById );
                return postId;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Delete posting from all communities
        /// </summary>
        /// <param name="postingId"></param>
        /// <returns></returns>
        public static bool Posting_Delete( int postingId )
        {
            return Posting_Delete( postingId, 0 );
        }

        /// <summary>
        /// not yet sure if deleting all, or single postItem - if last, then delete the posting as well
        /// ===> will need a check if posting exists in other communities, if none, then do the delete
        /// </summary>
        /// <param name="postingId"></param>
        /// <param name="communityId"></param>
        /// <returns></returns>
        public static bool Posting_Delete( int postingId, int communityId )
        {
            bool isSuccessful = false;
            //first get all pitems. If only one, then the posting will be deleted regardless
            List<Community_PostItem> eflist = ctx.Community_PostItem
                 .Where( s => s.PostingId == postingId )
                 .ToList();
            if ( eflist != null && eflist.Count > 0 )
            {
                //need to delete pitems first (or the postingId is set to null and can't delte
                foreach ( Community_PostItem item in eflist )
                {
                    if ( communityId == 0 || communityId == item.CommunityId )
                        Community_PostItem_Delete( (int) item.PostingId, item.CommunityId );
                }


                if ( communityId == 0 || eflist.Count == 1 )
                {
                    //now delete the posting
                    isSuccessful = Community_Posting_Delete( postingId );
                }
            }

            return isSuccessful;
        }

        private static bool Community_PostItem_Delete( int postingId, int communityId )
        {
            bool isSuccessful = false;

            Community_PostItem item = ctx.Community_PostItem.SingleOrDefault( s => s.PostingId == postingId && s.CommunityId == communityId );

            if ( item != null && item.Id > 0 )
            {
                ctx.Community_PostItem.Remove( item );
                ctx.SaveChanges();
                isSuccessful = true;
            }

            return isSuccessful;
        }

        /// <summary>
        /// delete all post_items for posting 
        /// NOTE: typically only called when deleting a related post, as should only be one post item
        /// </summary>
        /// <param name="postingId"></param>
        /// <returns></returns>
        private static bool Community_PostItem_Delete( int postingId )
        {
            bool isSuccessful = false;
            List<Community_PostItem> eflist = ctx.Community_PostItem
               .Where( s => s.PostingId == postingId )
               .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Community_PostItem item in eflist )
                {
                    ctx.Community_PostItem.Remove( item );
                    ctx.SaveChanges();
                }
                isSuccessful = true;
            }
  

            return isSuccessful;
        }

        public static bool Community_PostItem_Add( int pCommunityId, int pPostingId, int pCreatedById )
        {
            bool isValid = false;
            Community_PostItem item = new Community_PostItem();
           
            try
            {
                item.CommunityId = pCommunityId;
                item.PostingId = pPostingId;
                item.Created = System.DateTime.Now;
                item.CreatedById = pCreatedById;
                ctx.Community_PostItem.Add( item );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    isValid = true;
                }
               
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Community_PostItem_Add()" );
                return false;
            }

            return isValid;
        }


        #region ============ using Community_PostingSummary================================
		public static List<IB.CommunityPosting> PostingView_Select( int communityId, int maxPostings )
		{
			IB.CommunityPosting entity = new IB.CommunityPosting();
			List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();

			try
			{
				using ( var context = new IsleContentContext() )
				{

					//todo - include documents
					List<Community_PostingSummary> eflist = context.Community_PostingSummary
							.Where( s => s.CommunityId == communityId && s.RelatedPostingId == 0 )
							.Take( maxPostings )
							.OrderByDescending( s => s.Created ).OrderByDescending( s => s.Id )
							.ToList();

					return FillPostItems( eflist );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".PostingView_Select()" );
				return list;
			}
		}
        public static List<IB.CommunityPosting> FillPostItems( List<Community_PostingSummary> eflist )
        {
            bool fillingChildren = true;

            IB.CommunityPosting entity = new IB.CommunityPosting();
            IB.CommunityPosting chEntity = new IB.CommunityPosting();
            List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();

            if ( eflist != null && eflist.Count > 0 )
            {
                int cntr = 0;
                foreach ( Community_PostingSummary efom in eflist )
                {
                    entity = Community_PostingSummary_ToMap( efom );
                    entity.ChildPostings = new List<IB.CommunityPosting>();
                    entity.CommunityId = efom.CommunityId;

                    //fill children
                    if ( fillingChildren && ( int ) efom.ChildPostings > 0 )
                    {
                        List<Community_PostingSummary> chlist = ctx.Community_PostingSummary
                            .Where( s => s.RelatedPostingId == entity.Id )
                            .OrderByDescending( s => s.Id )
                            .ToList();
                        if ( chlist != null && chlist.Count > 0 )
                        {
                            foreach ( Community_PostingSummary chpost in chlist )
                            {
                                chEntity = Community_PostingSummary_ToMap( chpost );
                                chEntity.CommunityId = efom.CommunityId;

                                entity.ChildPostings.Add( chEntity );

                            }
                        }

                    }
                    cntr++;

                    list.Add( entity );

                }
            }

            return list;
        }
        private static IB.CommunityPosting Community_PostingSummary_ToMap( Community_PostingSummary fromEntity )
        {

            IB.CommunityPosting to = new IB.CommunityPosting();
            if ( fromEntity != null && fromEntity.Id > 0 )
            {
                to.Id = fromEntity.Id;
                to.CommunityId = fromEntity.CommunityId;
                to.Message = fromEntity.Message;

                to.PostingTypeId = fromEntity.PostingTypeId == null ? 1 : ( int ) fromEntity.PostingTypeId;
                to.PostingType = fromEntity.PostingType == null ? "General" : fromEntity.PostingType;
				to.IsApproved = fromEntity.IsApproved;
                //to.PostingStatus = fromEntity.PostingStatus == null ? "Open" : fromEntity.PostingStatus;

                to.UserImageUrl = fromEntity.UserImageUrl == null ? "" : fromEntity.UserImageUrl;
                to.UserFullName = fromEntity.UserFullName == null ? "" : fromEntity.UserFullName;

                //to.CommunityId = fromEntity.CommunityId;

                to.Created = fromEntity.Created == null ? System.DateTime.Now : ( System.DateTime ) fromEntity.Created;
                to.CreatedById = ( int ) fromEntity.CreatedById;
                if ( fromEntity.RelatedPostingId > 0 )
                    to.RelatedPostingId = ( int ) fromEntity.RelatedPostingId;
            }
            return to;
        }
        #endregion 
        /// <summary>
        /// Select requested number of postings (postItems + posting) for community
        /// </summary>
        /// <param name="communityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        public static List<IB.CommunityPosting> Posting_Select( int communityId, int maxPostings )
        {
            using ( var context = new IsleContentContext() )
            {
                return Posting_Select( context, communityId, maxPostings ); 
            }
        }

        /// <summary>
        /// Select requested number of postings (postItems + posting) for community
        /// Will first select top level posts, and then on fill, retrieve any child posts
        /// </summary>
        /// <param name="communityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        public static List<IB.CommunityPosting> Posting_Select( IsleContentContext context, int communityId, int maxPostings )
        {
            IB.CommunityPosting entity = new IB.CommunityPosting();
            List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();
			try
			{
				//todo - include documents
				List<Community_PostItem> eflist = context.Community_PostItem
						.Where( s => s.CommunityId == communityId && s.Community_Posting.RelatedPostingId == 0 )
						.Take( maxPostings )
						.OrderByDescending( s => s.Created ).OrderByDescending( s => s.Id )
						.ToList();

				return FillPostItems( eflist );
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Posting_Select()" );
				return list;
			}

        }

        /// <summary>
        /// mostly example, will use proc due to issues with patron ref
        /// </summary>
        /// <param name="communityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        public static List<IB.CommunityPosting> Posting_Search( int pCommunityId, int pSelectedPageNbr, int pPageSize, ref int pTotalRows )
        {
            IB.CommunityPosting entity = new IB.CommunityPosting();
            List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();
            int skip = 0;
            if ( pSelectedPageNbr > 1 )
                skip = ( pSelectedPageNbr - 1 ) * pPageSize;
			try
			{
				//todo - include documents
				List<Community_PostItem> eflist = ctx.Community_PostItem
						.Where( s => s.CommunityId == pCommunityId )
						.Skip( skip )
						.Take( pPageSize )
						.OrderByDescending( s => s.Created ).OrderByDescending( s => s.Id )
						.ToList();

				if ( eflist != null && eflist.Count > 0 )
				{
					foreach ( Community_PostItem efom in eflist )
					{
						entity = Community_PostItem_ToMap( efom );
						//additions like total posts, last post, or n most recent
						list.Add( entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Posting_Search()" );
			
			}
            return list;
        }
        public static IB.CommunityPosting Community_PostItem_ToMap( Community_PostItem fromEntity )
        {

            IB.CommunityPosting to = new IB.CommunityPosting();
            to.Id = (int) fromEntity.PostingId;
            to.Message = fromEntity.Community_Posting.Message;
            to.CommunityId = fromEntity.CommunityId;

            to.Created = ( System.DateTime ) fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;

            to.PostingTypeId = fromEntity.Community_Posting.PostingTypeId == null ? 1 : (int) fromEntity.Community_Posting.PostingTypeId;
			to.IsApproved = fromEntity.Community_Posting.IsApproved;
            //to.PostingStatus = fromEntity.Community_Posting.PostingStatus;

            if ( fromEntity.Community_Posting.RelatedPostingId != null )
                to.RelatedPostingId = ( int ) fromEntity.Community_Posting.RelatedPostingId;

            return to;
        }
        public static Community_PostItem Community_PostItem_FromMap( IB.CommunityPosting fromEntity )
        {

            Community_PostItem to = new Community_PostItem();
            to.Community_Posting = new Community_Posting();

            //Hmmmmmmmmmmmm don't have a post item id
            //to.Id = fromEntity.Id;
            to.PostingId = fromEntity.Id;
            to.CommunityId = fromEntity.CommunityId;
            to.Created = ( System.DateTime ) fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;


            to.Community_Posting.Id = fromEntity.Id;
            to.Community_Posting.Message = fromEntity.Message;
            to.Community_Posting.RelatedPostingId = ( int ) fromEntity.RelatedPostingId;
            to.Community_Posting.Created = ( System.DateTime ) fromEntity.Created;
            to.Community_Posting.CreatedById = ( int ) fromEntity.CreatedById;

            to.Community_Posting.PostingTypeId = fromEntity.PostingTypeId < 1 ? 1 : ( int ) fromEntity.PostingTypeId;
			to.Community_Posting.IsApproved = fromEntity.IsApproved;
            return to;
        }

        public static List<IB.CommunityPosting> FillPostItems( List<Community_PostItem> eflist )
        {
            return FillPostItems( eflist, true );
        }

        public static List<IB.CommunityPosting> FillPostItems( List<Community_PostItem> eflist, bool fillingChildren )
        {
            IB.CommunityPosting entity = new IB.CommunityPosting();
            IB.CommunityPosting chEntity = new IB.CommunityPosting();
            List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();

            if ( eflist != null && eflist.Count > 0 )
            {
                int cntr = 0;
                foreach ( Community_PostItem efom in eflist )
                {
                    entity = CommunityPosting_ToMap( efom.Community_Posting );
                    entity.ChildPostings = new List<IB.CommunityPosting>();
                    entity.CommunityId = efom.CommunityId;

                    //fill children
                    if ( fillingChildren && entity.RelatedPostingId == 0 )
                    {
                        List<Community_Posting> chlist = ctx.Community_Posting
                            .Where( s => s.RelatedPostingId == entity.Id )
                            .OrderByDescending( s => s.Id )
                            .ToList();
                        if ( chlist != null && chlist.Count > 0 )
                        {
                            foreach ( Community_Posting chpost in chlist )
                            {
                                chEntity = CommunityPosting_ToMap( chpost );
                                chEntity.CommunityId = efom.CommunityId;

                                entity.ChildPostings.Add( chEntity );

                            }
                        }

                    }
                    cntr++;
                    
                    list.Add( entity );
      
                }
            }

            return list;
        }
        #endregion

        #region Community Posting =======================

		private static int PostingAdd( string message, int pCreatedById )
		{
			return PostingAdd( message, pCreatedById, 0, true );
		}

		private static int PostingAdd( string message, int pCreatedById, int relatedPostingId )
		{
			return PostingAdd( message, pCreatedById, relatedPostingId, true );
		}
		private static int PostingAdd( string message, int pCreatedById, int relatedPostingId, bool isApproved )
        {
            Community_Posting msg = new Community_Posting();

            //msg.CommunityId = pCommunityId;
            msg.Message = message;
            msg.Created = System.DateTime.Now;
            msg.CreatedById = pCreatedById;
			msg.IsApproved = isApproved;
            msg.PostingTypeId = 1;

            if ( relatedPostingId > 0 )
                msg.RelatedPostingId = relatedPostingId;

            try
            {
                ctx.Community_Posting.Add( msg );

                // submit the change to database
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return msg.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".PostingAdd()" );
                return 0;
            }
        }

		/// <summary>
		/// Delete a posting
		/// NOTE: if allowing post to multiple communities, then possible to delete from one and not another. Which would lead to asking user if wishes to delete from all
		/// ==> for now, do the all of none
		/// </summary>
		/// <param name="postingId"></param>
		/// <returns></returns>
        private static bool Community_Posting_Delete( int postingId )
        {
            bool isSuccessful = false;

            Community_Posting item = ctx.Community_Posting.SingleOrDefault( s => s.Id == postingId );

            if ( item != null && item.Id > 0 )
            {
                //get all related posts and delete
                Posting_DeleteRelated( postingId );

                ctx.Community_Posting.Remove(item);
                ctx.SaveChanges();
                isSuccessful = true;
            }

            return isSuccessful;
        }

        public static void Posting_DeleteRelated( int postingId )
        {
            //get all related posts and delete
            //==================> need to get post items here and delete first
            Community_PostItem_Delete( postingId );

            List<Community_Posting> eflist = ctx.Community_Posting
                .Where( s => s.RelatedPostingId == postingId )
                .ToList();
            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Community_Posting item in eflist )
                {
                    ctx.Community_Posting.Remove( item );
                    ctx.SaveChanges();
                }
            }
        }

        public static IB.CommunityPosting Posting_Get( int id )
        {
            IB.CommunityPosting entity = new IB.CommunityPosting();
            IB.CommunityPostItem cpi = new IB.CommunityPostItem();
            entity.PostItems = new List<IB.CommunityPostItem>();
            //todo - include documents
            Community_Posting item = ctx.Community_Posting.SingleOrDefault( s => s.Id == id );
            if ( item != null && item.Id > 0 )
            {
                entity = CommunityPosting_ToMap( item );
                if ( item.Community_PostItem != null & item.Community_PostItem.Count > 0 )
                {
                    entity.PostItems = new List<IB.CommunityPostItem>();
                    foreach ( Community_PostItem cp in item.Community_PostItem )
                    {
                        cpi = new IB.CommunityPostItem();
                        cpi.CommunityId = cp.CommunityId;
                        //just incase:
                        if ( entity.CommunityId == 0 )
                            entity.CommunityId = cp.CommunityId;

                        cpi.PostingIdId = (int) cp.PostingId;
                        cpi.Created = (System.DateTime) cp.Created;
                        cpi.CreatedById = cp.CreatedById;
                        entity.PostItems.Add( cpi );
                    }
                }
            }

            return entity;
        }

        /// <summary>
        /// mostly example, will use proc due to issues with patron ref
        /// </summary>
        /// <param name="communityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        //private static List<IB.CommunityPosting> CommPosting_Search( int pCommunityId, int pSelectedPageNbr, int pPageSize, ref int pTotalRows)
        //{
        //    IB.CommunityPosting entity = new IB.CommunityPosting();
        //    List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();
        //    int skip = 0;
        //    if ( pSelectedPageNbr > 1 )
        //        skip = (pSelectedPageNbr - 1) * pPageSize;

        //    //todo - include documents
        //    List<Community_PostItem> eflist = ctx.Community_PostItem
        //            .Where( s => s.CommunityId == pCommunityId )
        //            .Skip(skip)
        //            .Take( pPageSize )
        //            .OrderByDescending( s => s.Created ).OrderByDescending( s => s.Id )
        //            .ToList();

        //    if ( eflist != null && eflist.Count > 0 )
        //    {
        //        foreach ( Community_Posting efom in eflist )
        //        {
        //            entity = CommunityPosting_ToMap( efom );
        //            //additions like total posts, last post, or n most recent
        //            list.Add( entity );
        //        }
        //    }

        //    return list;
        //}

        /// <summary>
        /// mostly example, will use proc due to issues with patron ref
        /// </summary>
        /// <param name="communityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        //private static List<IB.CommunityPosting> CommPosting_Select( int communityId, int recordsMax )
        //{
        //    IB.CommunityPosting entity = new IB.CommunityPosting();
        //    List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();

        //    //todo - include documents
        //    List<Community_Posting> eflist = ctx.Community_Posting
        //            .Where( s => s.CommunityId == communityId )
        //            .Take( recordsMax)
        //            .OrderByDescending( s=> s.Created).OrderByDescending(s=> s.Id)
        //            .ToList();

        //    return FillPostings( eflist );

        //}

        public static List<IB.CommunityPosting> FillPostings( List<Community_Posting> eflist, int maxPostings )
        {
            IB.CommunityPosting entity = new IB.CommunityPosting();
            List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();

            if ( eflist != null && eflist.Count > 0 )
            {
                int cntr = 0;
                foreach ( Community_Posting efom in eflist )
                {
                    entity = CommunityPosting_ToMap( efom );
                    cntr++;
                    //additions like total posts, last post, or n most recent
                    list.Add( entity );
                    if ( cntr == maxPostings )
                        break;
                }
            }

            return list;
        }

        public static Community_Posting CommunityPosting_FromMap( IB.CommunityPosting fromEntity )
        {
            Community_Posting to = new Community_Posting();
            to.Id = fromEntity.Id;
            to.Message = fromEntity.Message;
            //to.CommunityId = fromEntity.CommunityId;
            to.PostingTypeId = fromEntity.PostingTypeId;
			to.IsApproved = fromEntity.IsApproved;

            to.Created = ( System.DateTime ) fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById;
            to.RelatedPostingId = fromEntity.RelatedPostingId;

            return to;
        }

        public static IB.CommunityPosting CommunityPosting_ToMap( Community_Posting fromEntity )
        {

            IB.CommunityPosting to = new IB.CommunityPosting();
            if ( fromEntity != null && fromEntity.Id > 0 )
            {
                to.Id = fromEntity.Id;
                to.Message = fromEntity.Message;

                to.PostingTypeId = fromEntity.PostingTypeId == null ? 1 : ( int ) fromEntity.PostingTypeId;
				to.IsApproved = fromEntity.IsApproved;

                //to.CommunityId = fromEntity.CommunityId;

                to.Created = fromEntity.Created == null ? System.DateTime.Now : ( System.DateTime ) fromEntity.Created;
                to.CreatedById = ( int ) fromEntity.CreatedById;
                if ( fromEntity.RelatedPostingId != null )
                    to.RelatedPostingId = ( int ) fromEntity.RelatedPostingId;
            }
            return to;
        }

        #endregion

        #region Community Members =======================
        public int CommunityMember_Add( int pCommunityId, int pCreatedById )
        {

            Community_Member entity = new Community_Member();
            entity.CommunityId = pCommunityId;
            entity.UserId = pCreatedById;
            entity.MemberTypeId = 1;
            entity.Created = System.DateTime.Now;

            try
            {
                ctx.Community_Member.Add( entity );

                // submit the change to database
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return entity.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".CommunityMember_Add()" );
                return 0;
            }
        }

        public bool CommunityMember_Delete( int communityId, int userId )
        {
            bool isSuccessful = false;

            Community_Member item = ctx.Community_Member.SingleOrDefault( s => s.CommunityId == communityId && s.UserId == userId);

            if ( item != null && item.Id > 0 )
            {
                ctx.Community_Member.Remove( item );
                ctx.SaveChanges();
                isSuccessful = true;
            }

            return isSuccessful;
        }
        public static bool CommunityMember_IsMember( int pCommunityId, int pUserId )
        {

            bool isMember = false;

            Community_Member item = ctx.Community_Member.SingleOrDefault( s => s.CommunityId == pCommunityId && s.UserId == pUserId );

            if ( item != null && item.Id > 0 )
            {
                isMember = true;
            }

            return isMember;
        }

		public static DTO.ObjectMember CommunityMember_Get( int pCommunityId, int pUserId )
		{
			DTO.ObjectMember mbr = new DTO.ObjectMember();
			Community_MemberSummary item = ctx.Community_MemberSummary
							.SingleOrDefault( s => s.CommunityId == pCommunityId && s.UserId == pUserId );

			if ( item != null && item.Id > 0 )
			{
				mbr = CommunityMember_ToMap( item );
			}

			return mbr;
		}
		/// <summary>
		/// GEt all community members for the org
		/// ??not sure of the value - if an org community, then all members are org members.
		/// </summary>
		/// <param name="orgId"></param>
		/// <returns></returns>
        public static List<DTO.ObjectMember> CommunityMember_OrgGetAll( int orgId )
        {
            DTO.ObjectMember mbr = new DTO.ObjectMember();
            List<DTO.ObjectMember> mbrs = new List<DTO.ObjectMember>();
            using ( var context = new IsleContentContext() )
            {
                List<Community_MemberSummary> list = context.Community_MemberSummary
                        .Where( s => s.CommunityOrgId == orgId )
                        .ToList();

                if ( list != null && list.Count > 0 )
                {
                    foreach ( Community_MemberSummary efom in list )
                    {
                        mbr = CommunityMember_ToMap( efom );
                        mbrs.Add( mbr );
                    }
                }
            }

            return mbrs;
        }//

		/// <summary>
		/// Get list of members in pending status
		/// </summary>
		/// <param name="communityId"></param>
		/// <returns></returns>
		public static List<DTO.ObjectMember> CommunityMember_GetPending( int communityId )
		{
			DTO.ObjectMember mbr = new DTO.ObjectMember();
			List<DTO.ObjectMember> mbrs = new List<DTO.ObjectMember>();
			using ( var context = new IsleContentContext() )
			{

				List<Community_MemberSummary> list = context.Community_MemberSummary
						.Where( s => s.CommunityId == communityId && s.MemberTypeId == 0 )
						.ToList();

				if ( list != null && list.Count > 0 )
				{
					foreach ( Community_MemberSummary efom in list )
					{
						mbr = CommunityMember_ToMap( efom );
						mbrs.Add( mbr );
					}
				}
			}

			return mbrs;
		}//

        public static DTO.ObjectMember CommunityMember_ToMap( Community_MemberSummary fromEntity )
        {
            DTO.ObjectMember to = new DTO.ObjectMember();
            to.Id = fromEntity.Id;
            to.ObjectId = fromEntity.CommunityId;
            to.ParentOrgId = fromEntity.CommunityOrgId;
            to.ParentOrganization = fromEntity.CommunityOrganization;

            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId == null ? 0 : ( int ) fromEntity.MemberTypeId;
            to.MemberType = fromEntity.MemberType;

            to.FirstName = fromEntity.FirstName;
            to.LastName = fromEntity.LastName;
            to.MemberImageUrl = fromEntity.ImageUrl;
            to.MemberHomeUrl = fromEntity.UserProfileUrl;

            //to.OrgMemberTypeId = ( int ) fromEntity.OrgMemberTypeId;

            to.Created = fromEntity.Created == null ? DateTime.Now : ( DateTime ) fromEntity.Created; ;
            //if ( fromEntity.CreatedById != null )
            //    to.CreatedById = ( int ) fromEntity.CreatedById;

            //to.LastUpdated = fromEntity.LastUpdated == null ? to.DefaultDate : ( System.DateTime ) fromEntity.LastUpdated;
            //if ( fromEntity.LastUpdatedById != null )
            //    to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            return to;
        }

        #endregion

        #region Community Posting Document =======================


        public static int PostingDocumentAdd( int pPostingId, Guid docId, int pCreatedById )
        {
            Community_PostingDocument entity = new Community_PostingDocument();

            entity.PostingId = pPostingId;
            entity.DocumentId = docId;
            entity.Created = System.DateTime.Now;
            entity.CreatedById = pCreatedById;

            try
            {
                ctx.Community_PostingDocument.Add( entity );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return entity.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".PostingDocumentAdd()" );
                return 0;
            }
        }

        #endregion
    }
}
