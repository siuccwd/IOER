using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AutoMapper;

using IB =  ILPathways.Business;
using ILPathways.Utilities;

namespace IoerContentBusinessEntities
{
    public class EFCommunityManager
    {
        static string thisClassName = "EFCommunityManager";
        static IsleContentEntities ctx = new IsleContentEntities();

        public EFCommunityManager() { }

        #region Community =======================

        /// <summary>
        /// Get a community
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IB.Community Community_Get( int id )
        {
            IB.Community entity = new IB.Community();
            
            Community item = ctx.Communities.SingleOrDefault( s => s.Id == id );

            if ( item != null && item.Id > 0 )
            {
                entity = Community_ToMap( item );
                entity.Postings = Posting_Select( id, 25 );
            }

            return entity;
        }

        /// <summary>
        /// Get all communities - may want to add paging later!
        /// </summary>
        /// <returns></returns>
        public static List<IB.Community> Community_GetAll()
        {

            List<IB.Community> list = new List<IB.Community>();
            IB.Community entity = new IB.Community();
            List<Community> eflist = ctx.Communities.OrderBy( s => s.Title).ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Community efom in eflist )
                {
                    entity = Community_ToMap( efom );
                    //additions like total posts, last post, or n most recent
                    entity.Postings = Posting_Select( entity.Id, 25 );

                    list.Add( entity );
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
            to.ImageUrl = fromEntity.ImageUrl;

            to.Created = fromEntity.Created;
            //to.CreatedById = ( int ) fromEntity.CreatedById;

            return to;
        }

        public static IB.Community Community_ToMap( Community fromEntity )
        {

            IB.Community to = new IB.Community();
            to.Id = fromEntity.Id;
            to.Title = fromEntity.Title;
            to.Description = fromEntity.Description;
            to.ImageUrl = fromEntity.ImageUrl;

            to.Created = (System.DateTime) fromEntity.Created;
            //to.CreatedById = ( int ) fromEntity.CreatedById;

            return to;
        }

        #endregion

        #region Community Posting =======================

        public static int PostingAdd( int pCommunityId, string message, int pCreatedById )
        {
            return PostingAdd( pCommunityId, message, pCreatedById, 0 );
        }

        public static int PostingAdd( int pCommunityId, string message, int pCreatedById, int relatedPostingId )
        {
            //EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();
            Community_Posting msg = new Community_Posting();

            msg.CommunityId = pCommunityId;
            msg.Message = message;
            msg.Created = System.DateTime.Now;
            msg.CreatedById = pCreatedById;
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
                LoggingHelper.LogError( ex, thisClassName + ".MessageAdd()" );
                return 0;
            }
        }

        public static bool Posting_Delete( int id )
        {
            bool isSuccessful = false;

            Community_Posting item = ctx.Community_Posting.SingleOrDefault( s => s.Id == id );

            if ( item != null && item.Id > 0 )
            {
                //get all related posts and delete
                Posting_DeleteRelated( id );

                ctx.Community_Posting.Remove(item);
                ctx.SaveChanges();
                isSuccessful = true;
            }

            return isSuccessful;
        }

        public static void Posting_DeleteRelated( int postingId )
        {
            //get all related posts a
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

            //todo - include documents
            Community_Posting item = ctx.Community_Posting.SingleOrDefault( s => s.Id == id );
            if ( item != null && item.Id > 0 )
            {
                entity = CommunityPosting_ToMap( item );
            }

            return entity;
        }

        /// <summary>
        /// mostly example, will use proc due to issues with patron ref
        /// </summary>
        /// <param name="communityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        public static List<IB.CommunityPosting> Posting_Search( int pCommunityId, int pSelectedPageNbr, int pPageSize, ref int pTotalRows)
        {
            IB.CommunityPosting entity = new IB.CommunityPosting();
            List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();
            int skip = 0;
            if ( pSelectedPageNbr > 1 )
                skip = (pSelectedPageNbr - 1) * pPageSize;

            //todo - include documents
            List<Community_Posting> eflist = ctx.Community_Posting
                    .Where( s => s.CommunityId == pCommunityId )
                    .Skip(skip)
                    .Take( pPageSize )
                    .OrderByDescending( s => s.Created ).OrderByDescending( s => s.Id )
                    .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Community_Posting efom in eflist )
                {
                    entity = CommunityPosting_ToMap( efom );
                    //additions like total posts, last post, or n most recent
                    list.Add( entity );
                }
            }

            return list;
        }

        /// <summary>
        /// mostly example, will use proc due to issues with patron ref
        /// </summary>
        /// <param name="communityId"></param>
        /// <param name="recordsMax"></param>
        /// <returns></returns>
        public static List<IB.CommunityPosting> Posting_Select( int communityId, int recordsMax )
        {
            IB.CommunityPosting entity = new IB.CommunityPosting();
            List<IB.CommunityPosting> list = new List<IB.CommunityPosting>();

            //todo - include documents
            List<Community_Posting> eflist = ctx.Community_Posting
                    .Where( s => s.CommunityId == communityId )
                    .Take( recordsMax)
                    .OrderByDescending( s=> s.Created).OrderByDescending(s=> s.Id)
                    .ToList();

            if ( eflist != null && eflist.Count > 0 )
            {
                foreach ( Community_Posting efom in eflist )
                {
                    entity = CommunityPosting_ToMap( efom );
                    //additions like total posts, last post, or n most recent
                    list.Add( entity );
                }
            }

            return list;
        }

        public static Community_Posting CommunityPosting_FromMap( IB.CommunityPosting fromEntity )
        {
            Community_Posting to = new Community_Posting();
            to.Id = fromEntity.Id;
            to.Message = fromEntity.Message;
            to.CommunityId = fromEntity.CommunityId;

            to.Created = ( System.DateTime ) fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            if ( fromEntity.RelatedPostingId != null )
                to.RelatedPostingId = ( int ) fromEntity.RelatedPostingId;

            return to;
        }

        public static IB.CommunityPosting CommunityPosting_ToMap( Community_Posting fromEntity )
        {

            IB.CommunityPosting to = new IB.CommunityPosting();
            to.Id = fromEntity.Id;
            to.Message = fromEntity.Message;
            to.CommunityId = fromEntity.CommunityId;

            to.Created =  (System.DateTime) fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            if (fromEntity.RelatedPostingId != null)
                to.RelatedPostingId = ( int ) fromEntity.RelatedPostingId;

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
