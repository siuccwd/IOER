using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Isle.DataContracts;
using ILPathways.Business;
using ILPathways.Utilities;
using MyManager = ILPathways.DAL.ContentManager;
using DocManager = ILPathways.DAL.DocumentStoreManager;
using DBM = ILPathways.DAL.DatabaseManager;
using EFDAL = IoerContentBusinessEntities;

namespace Isle.BizServices
{
    public class ContentServices : ServiceHelper
    {
        static string thisClassName = "ContentServices";
        MyManager myMgr = new MyManager();

        EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();

		/// <summary>
		/// Default constructor
		/// </summary>
        public ContentServices()
		{ }//

		#region ====== Core Methods ===============================================
		/// <summary>
		/// Delete a ContentItem record
		/// </summary>
		/// <param name="pId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public bool Delete( int pId, ref string statusMessage )
		{
            return myMgr.Delete( pId, ref statusMessage );
		}//


		/// <summary>
		/// Add a ContentItem record
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
        public int Create( ContentItem entity, ref string statusMessage )
		{
            return myMgr.Create( entity, ref statusMessage );
		}

		/// <summary>
		/// Update an Content record
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
        public string Update( ContentItem entity )
		{
            return myMgr.Update( entity );

		}//

        /// <summary>
        /// Update ContentItem with related ResourceVersionId
        /// </summary>
        /// <param name="id"></param>
        /// <param name="resourceVersionId"></param>
        /// <returns></returns>
        public string UpdateResourceVersionId( int id, int resourceVersionId )
        {
            return myMgr.UpdateResourceVersionId( id, resourceVersionId );

        }//

        public string UpdateAfterQuickPub( ContentItem entity )
        {
            //check on usage rights
            if ( entity.UseRightsUrl != null && entity.UseRightsUrl.Length > 5 && entity.ConditionsOfUseId == 0 )
            {
                //call method to look up url
                //if found set 1, else use 4. Read the Fine Print
            }
            return myMgr.Update( entity );

        }//
		#endregion

		#region ====== Retrieval Methods ===============================================
		/// <summary>
		/// Get Content record
		/// </summary>
		/// <param name="pId"></param>
		/// <returns></returns>
        public ContentItem Get( int pId )
		{
            if ( pId > 0 )
                return myMgr.Get( pId );
            else
                return new ContentItem();

		}//

        public ContentItem GetByRowId( string pRowId )
        {
            return myMgr.GetByRowId( pRowId );

        }//

        /// <summary>
        /// retrieve list of org/district templates
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        public DataSet SelectOrgTemplates( int orgId )
        {
            return myMgr.SelectOrgTemplates( orgId );
        }

		/// <summary>
		/// Search for Content related data using passed parameters
		/// - uses custom paging
		/// - only requested range of rows will be returned
		/// </summary>
		/// <param name="pFilter"></param>
		/// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
		/// <param name="pStartPageIndex"></param>
		/// <param name="pMaximumRows"></param>
		/// <param name="pTotalRows"></param>
		/// <returns></returns>
        public DataSet Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
		{
            //TODO - create a List<> version
            return myMgr.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
		}

        #endregion

        #region ====== ContentSupplement Section ===============================================

        /// <summary>
        /// Add an Content record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentSupplementCreate( ContentSupplement entity, ref string statusMessage )
        {
            return myMgr.ContentSupplementCreate( entity, ref statusMessage );
        }

        public string ContentSupplementUpdate( ContentSupplement entity )
		{
            return myMgr.ContentSupplementUpdate( entity );

		}//

        public bool ContentSupplementDelete( int id, ref string statusMessage )
        {
            return myMgr.ContentSupplementDelete( id, ref statusMessage );
        }
        public ContentSupplement ContentSupplementGet( int id )
        {
            return myMgr.ContentSupplementGet( id );
        }

        public ContentSupplement ContentSupplementGetByRowId( string id )
        {
            return myMgr.ContentSupplementGetByRowId( id );
        }
        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet ContentSupplementsSelect( int parentId )
        {
            return myMgr.ContentSupplementsSelect( parentId );
        }
        public List<ContentSupplement> ContentSupplementsSelectList( int parentId )
        {
            return myMgr.ContentSupplementsSelectList( parentId );
        }
        #endregion

        #region ====== ContentFile Section ===============================================

        public int ContentFile_Create( ContentFile entity, ref string statusMessage )
        {
            return EFDAL.EFContentManager.ContentFile_Create( entity, ref statusMessage );
        }

        public bool ContentFile_Updated( int contentFileId, int versionId )
        {
            return EFDAL.EFContentManager.ContentFile_UpdateVersionId( contentFileId, versionId );
        }
        #endregion

        #region ====== DocumentVersion Section ===============================================

        /// <summary>
        /// Add a DocumentVersion record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public string DocumentVersionCreate( DocumentVersion entity, ref string statusMessage )
        {
            return DocManager.Create( entity, ref statusMessage );
        }

        public string DocumentVersionUpdate( DocumentVersion entity )
        {
            return DocManager.Update( entity );

        }//
        /// <summary>
        /// Set the record status to published
        /// Used where the doc version is created before the parent (actually the norm). The status defaults to initial on create.
        /// If something prevented the parent from being saved the record would then be orphaned - depending on how the current process handles the condition.
        /// </summary>
        /// <param name="rowId"></param>
        /// <returns></returns>
        public string DocumentVersion_SetToPublished( string rowId )
        {
            return DocManager.SetToPublished( rowId );
        }
        public DocumentVersion DocumentVersionGet( string rowId )
        {
            return DocManager.Get( rowId );
        }

        public DocumentVersion DocumentVersionGet( Guid rowId )
        {
            return DocManager.Get( rowId );
        }

        #endregion


        #region ====== ContentReference Section ===============================================

        /// <summary>
        /// Add a Content record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentReferenceCreate( ContentReference entity, ref string statusMessage )
        {
            return myMgr.ContentReferenceCreate( entity, ref statusMessage );
        }

        public string ContentReferenceUpdate( ContentReference entity )
        {
           return myMgr.ContentReferenceUpdate( entity );
        }//
        public bool ContentReferenceDelete( int id, ref string statusMessage )
        {
            return myMgr.ContentReferenceDelete( id, ref statusMessage );
        }
        public ContentReference ContentReferenceGet( int id )
        {
            return myMgr.ContentReferenceGet( id );
        }//
        /// <summary>
        /// Select sections for a Content 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet ContentReferencesSelect( int parentId )
        {
            return myMgr.ContentReferencesSelect( parentId );
        }
        public List<ContentReference> ContentReferencesSelectList( int parentId )
        {
            return myMgr.ContentReferencesSelectList( parentId );
        }
        #endregion


        #region ====== ContentHistory Section ===============================================

        /// <summary>
        /// Add a ContentHistory record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ContentHistory_Create( int pContentId, string pAction, string pDescription, int pCreatedById, ref string statusMessage )
        {
            return myMgr.ContentHistory_Create( pContentId, pAction, pDescription, pCreatedById, ref statusMessage );
        }

        public DataSet ContentHistory_Select( int pContentId )
        {
            return myMgr.ContentHistory_Select( pContentId );
        }
        #endregion

        #region ====== person following
        public static int FollowUser( int followingUserId, int followedByUserId )
        {
            EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();

            EFDAL.Person_Following log = new EFDAL.Person_Following();
            log.Created = System.DateTime.Now;
            log.FollowedByUserId = followedByUserId;
            log.FollowingUserId = followingUserId;

            try
            {
                ctx.Person_Following.Add( log );
                int count = ctx.SaveChanges();
                if ( count > 0 )
                {
                    return log.Id;
                }
                else
                {
                    //?no info on error
                    return 0;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".FollowUser()" );
                return 0;
            }
        }
        #endregion
        #region ====== Codes ===============================================
        public DataSet ContentType_Select()
        {
           return myMgr.ContentType_Select();
        }

        public DataSet ContentStatusCodes_Select()
        {
            return myMgr.ContentStatusCodes_Select();
        }
        public DataSet ContentPrivilegeCodes_Select()
        {
           return myMgr.ContentPrivilegeCodes_Select();
        }
        #endregion

        /// <summary>
        /// execute dynamic sql against the content db
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataSet DoQuery( string sql )
        {
            string conn = DBM.ContentConnectionRO();
            return DBM.DoQuery( sql, conn );
        }

    }
}
