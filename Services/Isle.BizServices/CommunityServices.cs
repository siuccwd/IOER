using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EFDAL = IoerContentBusinessEntities;
using ILPathways.Utilities;
using ILPathways.DAL;
using ILPathways.Business;
using ThisUser = LRWarehouse.Business.Patron;

namespace Isle.BizServices
{
    public class CommunityServices : ServiceHelper
    {
        private static string thisClassName = "CommunityServices";
        EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();

        public CommunityServices()
		{ }//

        #region == Community ==
        /// <summary>
        /// Get a community
        /// </summary>
        /// <param name="communityId"></param>
        /// <returns></returns>
        public static Community Community_Get( int communityId)
        {
            return EFDAL.EFCommunityManager.Community_Get( communityId );
        }

        /// <summary>
        /// Select all communities
        /// </summary>
        /// <returns></returns>
        public List<Community> Community_SelectAll()
        {
            return EFDAL.EFCommunityManager.Community_GetAll();
        }
        #endregion


        #region === Postings =====
        /// <summary>
        /// add a posting
        /// </summary>
        /// <param name="pCommunityId"></param>
        /// <param name="comment"></param>
        /// <param name="pCreatedById"></param>
        /// <returns></returns>
        public static int PostingAdd( int pCommunityId, string comment, int pCreatedById )
        {
            return EFDAL.EFCommunityManager.PostingAdd( pCommunityId, comment, pCreatedById, 0 );
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
            return EFDAL.EFCommunityManager.PostingAdd( pCommunityId, comment, pCreatedById, relatedPostingId );
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
            return EFDAL.EFCommunityManager.Posting_Select( pCommunityId, recordsMax );
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
