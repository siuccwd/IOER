using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EFDAL = IoerContentBusinessEntities;
using ILPathways.Common;
using ILPathways.Utilities;
using ILPathways.DAL;
using ILPathways.Business;
using ThisUser = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
    public class CommunityServices : ServiceHelper
    {
		//private static string thisClassName = "CommunityServices";

		EFDAL.EFCommunityManager myManager = new EFDAL.EFCommunityManager();

        public CommunityServices()
		{ }//

        #region == Community ==
		/// <summary>
		/// Delete a community
		/// </summary>
		/// <param name="id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int id, ref string statusMessage )
		{
			return myManager.CommunityDelete( id, ref statusMessage );
		}

		/// <summary>
		/// Create a community
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Create( Community entity, ref string statusMessage )
		{
			return myManager.CommunityAdd( entity, ref statusMessage );
		}

		/// <summary>
		/// Update a community
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="message"></param>
		/// <returns></returns>
		public bool Update( Community entity, ref string message )
		{
			return myManager.CommunityUpdate( entity, ref message );
		}

        /// <summary>
        /// Get a community
        /// Will return community including the most recent n posts (from web.config)
        /// </summary>
        /// <param name="communityId"></param>
        /// <returns></returns>
        public static Community Community_Get( int communityId)
        {
            return EFDAL.EFCommunityManager.Community_Get( communityId );
        }
        public static Community Community_Get( int communityId, int recentPosts )
        {
            return EFDAL.EFCommunityManager.Community_Get( communityId, recentPosts );
        }

        /// <summary>
        /// Select all communities
        /// </summary>
        /// <returns>Community and the default nbr of recent posts</returns>
        public List<Community> Community_SelectAll()
        {
            return EFDAL.EFCommunityManager.Community_GetAll(5);
        }

        public static List<CodeItem> Community_SelectList()
        {
            CodeItem ci = new CodeItem();
            var eflist = EFDAL.EFCommunityManager.Community_GetAll( 5 );
            List<CodeItem> list = new List<CodeItem>();
            if ( eflist.Count > 0 )
            {
                foreach ( Community item in eflist )
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.Title;
                    list.Add( ci );
                }
            }
            return list;
        }

        /// <summary>
        /// Select all communities
        /// </summary>
        /// <param name="recentPosts"></param>
        /// <returns>Community and the requested nbr of recent posts</returns>
        public List<Community> Community_SelectAll( int recentPosts )
        {
            return EFDAL.EFCommunityManager.Community_GetAll( recentPosts );
        }
        #endregion


        #region === Community_Member =====
        public int Community_MemberAdd( int pCommunityId, int pUserId )
        {
            return new EFDAL.EFCommunityManager().CommunityMember_Add( pCommunityId, pUserId );
        }//
        public bool Community_MemberDelete( int pCommunityId, int pUserId )
        {
            return new EFDAL.EFCommunityManager().CommunityMember_Delete( pCommunityId, pUserId );
        }//
        public static bool Community_MemberIsMember( int pCommunityId, int pUserId )
        {
            return EFDAL.EFCommunityManager.CommunityMember_IsMember( pCommunityId, pUserId );
        }//
        #endregion


        #region === Postings =====
        /// <summary>
        /// Add posting to selected list of communities
        /// ==> assumes not used with replys
        /// </summary>
        /// <param name="communities"></param>
        /// <param name="comment"></param>
        /// <param name="pCreatedById"></param>
        /// <returns></returns>
        public static int PostingAdd( List<int> communities, string comment, int pCreatedById )
        {
            return EFDAL.EFCommunityManager.Community_AddPostings( communities, comment, pCreatedById );
        }//

        /// <summary>
        /// add a posting
        /// </summary>
        /// <param name="pCommunityId"></param>
        /// <param name="comment"></param>
        /// <param name="pCreatedById"></param>
        /// <returns></returns>
        public static int PostingAdd( int pCommunityId, string comment, int pCreatedById )
        {
            return EFDAL.EFCommunityManager.Community_AddPosting( pCommunityId, comment, pCreatedById, 0 );
        }

        /// <summary>
        /// add a posting with a related postingId
        /// </summary>
        /// <param name="pCommunityId"></param>
        /// <param name="comment"></param>
        /// <param name="pCreatedById"></param>
        /// <param name="relatedPostingId"></param>
        /// <returns></returns>
        public static int PostingAdd( int pCommunityId, string comment, int pCreatedById, int relatedPostingId )
        {
            return EFDAL.EFCommunityManager.Community_AddPosting( pCommunityId, comment, pCreatedById, relatedPostingId );
        }

        public static bool Posting_Delete( int id )
        {
            return EFDAL.EFCommunityManager.Posting_Delete( id );
        }

        public static CommunityPosting Posting_Get( int id )
        {
            return EFDAL.EFCommunityManager.Posting_Get( id );
        }

        /// <summary>
        /// Posting search, used with paging
        /// </summary>
        /// <param name="pCommunityId"></param>
        /// <param name="selectedPageNbr"></param>
        /// <param name="pageSize"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public static List<CommunityPosting> PostingSearch( int pCommunityId, int selectedPageNbr, int pageSize, ref int pTotalRows )
        {
            return EFDAL.EFCommunityManager.Posting_Search( pCommunityId, selectedPageNbr, pageSize, ref pTotalRows );
        }

        /// <summary>
        /// Select postings for community up to the value for recordsMax
        /// </summary>
        /// <param name="pCommunityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        public static List<CommunityPosting> Posting_Select( int pCommunityId, int recordsMax )
        {
            return EFDAL.EFCommunityManager.PostingView_Select( pCommunityId, recordsMax );
        }

        /// <summary>
        /// Add a document for a posting/community
        /// </summary>
        /// <param name="pPostingId"></param>
        /// <param name="docId"></param>
        /// <param name="pCreatedById"></param>
        /// <returns></returns>
        public static int PostingDocumentAdd( int pPostingId, Guid docId, int pCreatedById )
        {
            return EFDAL.EFCommunityManager.PostingDocumentAdd( pPostingId, docId, pCreatedById );
        }

        #endregion

    }
}
