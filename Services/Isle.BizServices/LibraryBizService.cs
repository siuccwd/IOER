using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Isle.DataContracts;
using ILPathways.Business;
using ILPathways.Common;
using Library = ILPathways.Business.Library;

using ILPathways.DAL;
using LibManager = ILPathways.DAL.LibraryManager;
using LibResourceManager = ILPathways.DAL.LibraryResourceManager;
using SubscriptionManager = ILPathways.DAL.LibrarySubScriptionManager;
using ILPathways.Utilities;
//.use alias for easy change to Gateway
//using ThisUser = ILPathways.Business.AppUser;
//using ThisUserProfile = ILPathways.Business.AppUserProfile;
//using PatronMgr = ILPathways.DAL.PatronManager;
using ThisUser = LRWarehouse.Business.Patron;
using LBiz=LRWarehouse.Business;
using LRDAL = LRWarehouse.DAL;
using EFDAL = IoerContentBusinessEntities;
using IoerContentBusinessEntities;
namespace Isle.BizServices
{
    public class LibraryBizService : ServiceHelper
    {
        LibManager myMgr = new LibManager();
        LibResourceManager libResManager = new LibResourceManager();

        EFDAL.IsleContentEntities ctx = new EFDAL.IsleContentEntities();

        const string thisClassName = "LibraryService";

        #region ====== Core Methods ===============================================
        /// <summary>
        /// Delete an Library record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool LibraryDelete( int pId, ref string statusMessage )
        {

            //first get list of all resource ids, so can be removed from elastic search
            List<int> resIdsList = LibraryResource_SelectAllResourceIdsForLibrary( pId );

            bool result = myMgr.Delete( pId, ref statusMessage );

            if ( result && resIdsList.Count > 0 )
            {
                foreach ( int resourceIntId in resIdsList )
                {
                    LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );
                }
            }

            return result;
        }//
        /// <summary>
        /// Set a library to inactive - a virtual delete, pending delete of resources from ES
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool Library_SetInactive( int pId, ref string statusMessage )
        {
            bool result = true;
            Library entity = Get( pId );
            if ( entity != null && entity.SeemsPopulated )
            {
                entity.IsActive = false;
                statusMessage = Update( entity );
            }

            return result;
        }//

        /// <summary>
        /// Add a personal Library 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public Library CreateMyLibrary( ThisUser user, ref string statusMessage )
        {
            return myMgr.CreateMyLibrary( user, ref statusMessage );
        }//

        /// <summary>
        /// Add an Library record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Create( Library entity, ref string statusMessage )
        {
            return myMgr.Create( entity, ref statusMessage );
        }

        /// <summary>
        /// Update an Library record
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public string Update( Library entity )
        {
            return myMgr.Update( entity );
        }//
        #endregion

        #region ====== Library Privileges/Retrieval Methods ===============================================
        /// <summary>
        /// Determine if user can access the library admin functions. Either
        /// - must have a personal lib, or be a library member with a role of editor or above
        /// - or must be a member of an org with a member role of administrator or library admin
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool Library_CanUserAdministerLibraries( int userId )
        {
            List<Library> libs = Library_SelectListWithEditAccess( userId );
            if ( libs != null && libs.Count > 0 )
                return true;

            List<OrganizationMember> list = OrganizationBizService.OrganizationMember_GetAdminOrgs( userId );
            if ( list != null && list.Count > 0 )
                return true;
            else 
                return false;
        }
        /// <summary>
        /// Get Library record
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public Library Get( int pId )
        {
            return myMgr.Get( pId );
        }//
        public Library GetByRowId( string pRowId )
        {
            return myMgr.GetByRowId( pRowId );

        }//


        public Library GetMyLibrary( IWebUser user )
        {
            return myMgr.GetMyLibrary( user );
        }//

        public Library GetMyLibrary( IWebUser user, bool createIfMissing )
        {
            return myMgr.GetMyLibrary( user, createIfMissing );
        }//

        /// <summary>
        /// Search for Library related data using passed parameters
        /// - returns List
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<Library> LibrarySearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return myMgr.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        /// <summary>
        /// Search for Library related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet LibrarySearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return myMgr.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        /// <summary>
        /// Return list of libraries where user has curator access (at least above contribute)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Library> Library_SelectListWithEditAccess( int userId )
        {
            return myMgr.Library_SelectListWithEditAccess( userId );
        }
        public List<Library> Library_SelectListWithEditAccess( int userId, int libraryId )
        {
            return myMgr.Library_SelectListWithEditAccess( userId, libraryId );
        }
        /// <summary>
        /// Return dataset of libraries where user has curator access (at least above reader)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DataSet Libraries_SelectWithEditAccess( int userId )
        {
            return myMgr.Library_SelectWithEditAccess( userId, 0 );
        }

        /// <summary>
        /// Determine if user has edit access to the library
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool Library_DoesUserHaveEditAccess( int libraryId, int userId )
        {
            return myMgr.DoesUserHaveEditAccess( libraryId, userId );
        }

        /// <summary>
        /// Return list of libraries where user has contribute access (at least above reader)
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<Library> Library_SelectListWithContributeAccess( int userId )
        {
            return myMgr.SelectListWithContributeAccess( userId );
        }
        public List<Library> Library_SelectListWithContributeAccess( int userId, int libraryId )
        {
            return myMgr.SelectListWithContributeAccess( userId, libraryId );
        }

        /// <summary>
        /// Determine if user has contribute access to the library
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool Library_DoesUserHaveContributeAccess( int libraryId, int userId )
        {
            return myMgr.DoesUserHaveContributeAccess( libraryId, userId );
        }


        /// <summary>
        /// Retrieve all libraries containing the resource
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public List<Library> GetAllLibrariesWithResource( int resourceIntId )
        {
            return myMgr.AllLibrariesWithResource( resourceIntId );
        }

        /// <summary>
        /// Return list of libraries, and collections user is following
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public FollowingCollection Library_SelectLibrariesFollowing( int userId )
        {
            FollowingCollection fc = new FollowingCollection();
            List<ObjectSubscription> libList = EFLibraryManager.LibraryFollowing_ForUser( userId );
            if ( libList != null && libList.Count > 0 )
            {
                foreach ( ObjectSubscription res in libList )
                {
                    fc.LibraryIds.Add( res.ParentId );
                }
            }

            List<ObjectSubscription> colList = EFLibraryManager.CollectionFollowing_ForUser( userId );

            if ( colList != null && colList.Count > 0 )
            {
                foreach ( ObjectSubscription res in colList )
                {
                    fc.CollectionIds.Add( res.ParentId );
                }
            }
            return fc;
        }
        #endregion
        #region ====== Library Section ===============================================
        /// <summary>
        /// Add a personal Library 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public LibrarySection LibrarySection_CreateDefault( int libraryId, ref string statusMessage )
        {
            return LibrarySectionManager.CreateDefault( libraryId, ref statusMessage );
        }//

        public LibrarySection LibrarySection_CreateMyAuthored( int libraryId, ref string statusMessage )
        {
            return LibrarySectionManager.CreateMyAuthored( libraryId, ref statusMessage );

        }//

        public bool LibrarySection_Delete( int sectionId, ref string statusMessage )
        {
            //first get list of all resource ids, so can be removed from elastic search
            List<int> resIdsList = LibraryResource_SelectAllResourceIdsForCollection( sectionId );

            bool result = LibrarySectionManager.Delete( sectionId, ref statusMessage );
            if ( result && resIdsList.Count > 0)
            {
                foreach ( int resourceIntId in resIdsList )
                {
                    LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );
                }
            }

            return result;
        }
        /// <summary>
        /// Add an Library section record
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int LibrarySectionCreate( LibrarySection entity, ref string statusMessage )
        {
            return LibrarySectionManager.Create( entity, ref statusMessage );
        }

        public string LibrarySectionUpdate( LibrarySection entity )
        {
            return LibrarySectionManager.Update( entity );
        }


        public int LibrarySectionCopy( int fromSectionId, int toLibraryId, int createdById, ref string statusMessage )
        {
            LibrarySection entity = LibrarySectionGet( fromSectionId );
            entity.Id = 0;
            entity.LibraryId = toLibraryId;
            string message = "";
            if ( entity.ParentLibrary != null && entity.ParentLibrary.Title.Length > 0 )
            {
                message = " (copied from " + entity.ParentLibrary.Title + ")";
            }
            entity.Title = "*** " + entity.Title + message;
            entity.CreatedById = createdById;
            entity.IsDefaultSection = false;
            entity.RowId = Guid.NewGuid();

            int newSectionId = LibrarySectionManager.Create( entity, ref statusMessage );
            if ( newSectionId > 0 )
            {
                int cntr = 0;

                List<int> list = LibraryResource_SelectAllResourceIdsForCollection( fromSectionId );
                if ( list.Count > 0 )
                {
                    foreach ( int rid in list )
                    {
                        cntr++;
                        LibraryResourceCreate( newSectionId, rid, createdById, ref statusMessage );
                    }
                    statusMessage = string.Format("Copied the collection and {0} resources to the requested library.", cntr);
                }
            }

            return newSectionId;
        }

        public LibrarySection GetLibraryDefaultSection( int libraryId )
        {
            return LibrarySectionManager.GetLibrarySection_Default( libraryId, false );
        }//

        public LibrarySection GetLibrarySection_Default( int libraryId, bool createIfMissing )
        {
            return LibrarySectionManager.GetLibraryDefaultSection( libraryId );
        }//

        public LibrarySection GetLibrarySection_MyAuthored( ThisUser user )
        {
            return LibrarySectionManager.GetLibrarySection_MyAuthored( user );
        }//

        public LibrarySection GetLibrarySection_MyAuthored( int libraryId )
        {
            return LibrarySectionManager.GetLibrarySection_MyAuthored( libraryId );
        }//

        public LibrarySection GetLibrarySection_MyAuthored( int libraryId, bool createIfMissing )
        {
            return LibrarySectionManager.GetLibrarySection_MyAuthored( libraryId, createIfMissing );
        }//

        public LibrarySection LibrarySectionGet( int sectionId )
        {
            return LibrarySectionManager.Get( sectionId );
        }//

        public LibrarySection LibrarySectionGetByGuid( string guid )
        {
            return LibrarySectionManager.GetByGuid( guid );
        }//

        /// <summary>
        /// Return collection of public LibrarySections for a library
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        public List<LibrarySection> LibrarySectionsSelectList( int libraryId )
        {
            int pShowingAll = 1;
            return LibrarySectionManager.LibrarySectionsSelectList( libraryId, pShowingAll );
        }
        /// <summary>
        /// Return collection of LibrarySections for a library
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="pShowingAll">0-only non-public; 1-Only public, 2-all</param>
        /// <returns></returns>
        public List<LibrarySection> LibrarySectionsSelectList( int libraryId, int pShowingAll )
        {
            return LibrarySectionManager.LibrarySectionsSelectList( libraryId, pShowingAll );
        }

        /// <summary>
        /// Select sections for a Library 
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        public DataSet LibrarySectionsSelect( int libraryId )
        {
            int pShowingAll = 1;
            return LibrarySectionManager.LibrarySectionsSelect( libraryId, pShowingAll );
        }
        public DataSet LibrarySectionsSelect( int libraryId, int pShowingAll )
        {
            return LibrarySectionManager.LibrarySectionsSelect( libraryId, pShowingAll );
        }
        /// <summary>
        /// Search for LibrarySection related data using passed parameters
        /// - returns List
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy"></param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<LibrarySection> LibrarySections_SearchAsList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return LibrarySectionManager.SearchAsList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        /// <summary>
        /// Search for LibrarySection related data using passed parameters
        /// - uses custom paging
        /// - only requested range of rows will be returned
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet LibrarySections_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return LibrarySectionManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }
        //=================================================================================================
        /// <summary>
        /// Select all LibrarySections as LIST where user has edit access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public List<LibrarySection> LibrarySections_SelectListWithEditAccess( int libraryId, int userId )
        {
            return LibrarySectionManager.SelectListWithEditAccess( libraryId, userId );
        }

        /// <summary>
        /// Select all LibrarySections where user has edit access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public DataSet LibrarySections_SelectWithEditAccess( int libraryId, int userId )
        {
            return LibrarySectionManager.SelectWithEditAccess( libraryId, userId );
        }
        /// <summary>
        /// Determine if user has edit access to the LibrarySection
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool LibrarySection_DoesUserHaveEditAccess( int libraryId, int sectionId, int userId )
        {
            return LibrarySectionManager.DoesUserHaveEditAccess( libraryId, sectionId, userId );
        }

        //=================================================================================================
        /// <summary>
        /// Select all library collections as LIST where user has contribute access. Includes:
        /// - personal
        /// - libraries where has curate access
        /// - collections where has curate access
        /// - org libraries with implicit access
        /// </summary>
        /// <param name="pLibraryId"></param>
        /// <param name="pUserid"></param>
        /// <returns></returns>
        public List<LibrarySection> LibrarySections_SelectListWithContributeAccess( int libraryId, int userId )
        {
            return LibrarySectionManager.SelectListWithContributeAccess( libraryId, userId );
        }

        /// <summary>
        /// Determine if user has contribute access to the collection
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool LibrarySection_DoesUserHaveContributeAccess( int libraryId, int sectionId, int userId )
        {
            return LibrarySectionManager.DoesUserHaveContributeAccess( libraryId, sectionId, userId );
        }

        #endregion


        #region Library and Section Members

        #region ====== Library Members methods ===============================================

        public bool IsLibraryMember( int libraryId, int userId )
        {
            LibraryMember entity = LibraryMember_Get( libraryId, userId );
            if ( entity != null && entity.Id > 0 )
                return true;
            else
                return false;
        }

        public LibraryMember LibraryMember_Get( int id )
        {
            if ( id == 0 )
                return new LibraryMember(); ;

            return EFLibraryManager.LibraryMember_Get( id );

        }

        public LibraryMember LibraryMember_Get( int libraryId, int userId )
        {
            if ( libraryId == 0 || userId == 0 )
                return new LibraryMember(); ;

            return EFLibraryManager.LibraryMember_Get( libraryId, userId );
        }

        public bool LibraryMember_Delete( int id )
        {
            return EFLibraryManager.LibraryMember_Delete( id);

        }

        /// <summary>
        /// Add a new library member
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int LibraryMember_Create( int libraryId, int userId, int memberTypeId, int createdById, ref string statusMessage )
        {
            int id = EFLibraryManager.LibraryMember_Create( libraryId, userId, memberTypeId, createdById, ref statusMessage );

            return id;
        }

        public bool LibraryMember_Update( int id, int memberTypeId, int lastUpdatedById )
        {
            if ( id == 0 )
                return false;
            return EFLibraryManager.LibraryMember_Update( id, memberTypeId, lastUpdatedById );
        }

        public bool LibraryMember_Update( int libraryId, int userId, int memberTypeId, int lastUpdatedById )
        {
            if ( libraryId == 0 || userId == 0 )
                return false;
            return EFLibraryManager.LibraryMember_Update( libraryId, userId, memberTypeId, lastUpdatedById );
        }

        public List<LibraryMember> LibraryMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            return myMgr.LibraryMember_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        #endregion

        #region Library Invitations
        public int LibraryInvitation_Create( LibraryInvitation entity, ref string statusMessage )
        {
            return EFLibraryManager.LibraryInvitationCreate( entity, ref statusMessage );
        }
        public bool LibraryInvitation_Update( LibraryInvitation entity )
        {
            return EFLibraryManager.LibraryInvitation_Update( entity );
        }
        public bool LibraryInvitation_Delete( int id )
        {
            return EFLibraryManager.LibraryInvitation_Delete( id );
        }

        public LibraryInvitation LibraryInvitation_Get( int id )
        {
            if ( id == 0 )
                return new LibraryInvitation(); ;

            return EFLibraryManager.LibraryInvitation_Get( id);
        }
        public LibraryInvitation LibraryInvitation_GetByGuid( string rowId )
        {
            if ( rowId == null || rowId == "" 
                || rowId == "00000000-0000-0000-0000-000000000000" 
                || rowId.Length != 36)
                return new LibraryInvitation(); 

            Guid guid = new Guid( rowId );
            return EFLibraryManager.LibraryInvitation_Get( guid );

        }
        public LibraryInvitation LibraryInvitation_GetByPasscode( string pPassCode )
        {
            if ( pPassCode == null || pPassCode == "" )
                return new LibraryInvitation();

            return EFLibraryManager.LibraryInvitation_GetByPasscode( pPassCode );

        }
        public LibraryInvitation LibraryInvitation_Get( int libraryId, int userId )
        {
            if ( libraryId == 0 || userId == 0 )
                return new LibraryInvitation(); ;

            return EFLibraryManager.LibraryInvitation_Get( libraryId, userId );
        }

        #endregion


        #region ====== Library collection Members methods ==================================

        public bool IsCollectionMember( int parentId, int userId )
        {
            LibraryMember entity = LibraryCollectionMember_Get( parentId, userId );
            if ( entity != null && entity.Id > 0 )
                return true;
            else
                return false;
        }

        public LibraryMember LibraryCollectionMember_Get( int id )
        {
            if ( id == 0 )
                return new LibraryMember(); ;

            return EFLibraryManager.LibrarySectionMember_Get( id );
        }

        public LibraryMember LibraryCollectionMember_Get( int parentId, int userId )
        {
            if ( parentId == 0 || userId == 0 )
                return new LibraryMember(); ;

            return EFLibraryManager.LibrarySectionMember_Get( parentId, userId );

        }

        public bool LibraryCollectionMember_Delete( int id )
        {
            return EFLibraryManager.LibrarySectionMember_Delete( id );

        }

        /// <summary>
        /// Add a new library member
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int LibraryCollectionMember_Create( int parentId, int userId, int memberTypeId, int createdById, ref string statusMessage )
        {
            int id = EFLibraryManager.LibrarySectionMember_Create( parentId, userId, memberTypeId, createdById, ref statusMessage );

            return id;
        }

        public bool LibraryCollectionMember_Update( int id, int memberTypeId, int lastUpdatedById )
        {
            if ( id == 0 )
                return false;
            return EFLibraryManager.LibrarySectionMember_Update( id, memberTypeId, lastUpdatedById );
        }

        public bool LibraryCollectionMember_Update( int parentId, int userId, int memberTypeId, int lastUpdatedById )
        {
            if ( parentId == 0 || userId == 0 )
                return false;
            return EFLibraryManager.LibrarySectionMember_Update( parentId, userId, memberTypeId, lastUpdatedById );
        }

        public List<LibraryMember> LibraryCollectionMember_Search( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {

            return myMgr.LibraryMember_Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }

        #endregion
        #endregion

        #region ====== Library Resource ===============================================
        /// <summary>
        /// Delete an Library record
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public bool LibraryResourceDeleteById( int pId, ref string statusMessage )
        {
            bool result = libResManager.DeleteById( pId, ref statusMessage );
            if ( result == true )
                LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( pId );

            return result;
        }//
        public bool LibraryResourceDelete( int fromCollectionId, int resourceIntId, ref string statusMessage )
        {
            bool result = libResManager.Delete( fromCollectionId, resourceIntId, ref statusMessage );
            if ( result == true )
                LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );

            return result;
        }//
 
        /// <summary>
        ///  Method to create LibraryResource object via resourceVersion Id
        /// </summary>
        /// <param name="resourceVersionId"></param>
        /// <param name="userGUID"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        private int LibraryResourceCreateByRVId_NotUsed( int resourceVersionId, int collectionID, string userGUID, ref string statusMessage )
        {
            int userId = 0;
            ThisUser user = new LRDAL.PatronManager().GetByRowId( userGUID );
            if ( user.IsValid )
            {
                userId = user.Id;
            }
            else
            {
                statusMessage = "Error user account was not found";
                return 0;
            }
            LBiz.ResourceVersion rv = new LRDAL.ResourceVersionManager().Get( resourceVersionId );
            if ( rv != null && rv.Id > 0 )
            {
                LibraryResource entity = new LibraryResource();
                entity.LibrarySectionId = collectionID;
                entity.ResourceIntId = rv.ResourceIntId;
                entity.CreatedById = userId;

                return LibraryResourceCreate( collectionID, rv.ResourceIntId, userId, ref statusMessage );
            }
            else
            {
                statusMessage = "error resource version was not found";
                return 0;
            }
        }//

        /// <summary>
        ///  Method to create LibraryResource object
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public int LibraryResourceCreate( LibraryResource entity, ThisUser user, ref string statusMessage )
        {
            int newId = 0;
            #region if not provided, get from library/default section
            if (entity.LibrarySectionId == 0) 
           {
               LibrarySection section = new LibrarySection();
                if ( entity.LibraryId == 0  )
                {
                    if ( user.CurrentLibraryId > 0 )
                        entity.LibraryId = user.CurrentLibraryId;
                    else
                    {
                        //get user personal library (and maybe create
                        Library lib = myMgr.GetMyLibrary( user, true );
                        if ( lib != null && lib.Id > 0 )
                        {
                            entity.LibraryId = lib.Id;
                            user.CurrentLibraryId = lib.Id;
                        }
                        else
                        {
                            statusMessage = "Error - unable to establish a default user library.";
                            return -1;
                        }
                    }
                    //get default section
                    section = LibrarySectionManager.GetLibrarySection_Default( entity.LibraryId, true );
                    entity.LibrarySectionId = section.Id;
                } else 
                {
                    //get library and default section
                     Library lib = myMgr.Get( entity.LibraryId);
                     if ( lib != null && lib.Id > 0 )
                     {
                         section = LibrarySectionManager.GetLibrarySection_Default( lib.Id, true );
                     }
                     else
                     {
                         statusMessage = "Error - unable to establish a default user library.";
                         return -1;
                     }
                }
               //do we have a section?
               if (section != null && section.Id > 0) {
                    entity.LibrarySectionId = section.Id;
               }
               else {
                   statusMessage = "Error - unable to establish a default user library section.";
                   return -1;
               }
           }
            #endregion
            newId= LibraryResourceCreate( entity.LibrarySectionId, entity.ResourceIntId, user.Id, ref statusMessage );
           
            return newId;
        }//

        public int LibraryResourceCreate( int collectionId, int resourceIntId, int userId, ref string statusMessage )
        {
            LibraryResource entity = new LibraryResource();
            entity.LibrarySectionId = collectionId;
            entity.ResourceIntId = resourceIntId;
            entity.CreatedById = userId;

            int id = libResManager.Create( entity, ref statusMessage );
            if ( id > 0 )
            {
                //update favourites
                ResourceBizService.UpdateFavorite( entity.ResourceIntId );
                //refresh index for resId
                LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );
            }
            else
            {
                string msg = string.Format( "ResourceIntId: {0}; collectionId: {1}; userId: {2}. May need to correct manually.", entity.ResourceIntId, entity.LibrarySectionId, userId.ToString() );
                EmailManager.NotifyAdmin( "LibraryResourceCreate( int collectionId, int resourceIntId, int userId, ref string statusMessage ) - Failed to add collection resource to elastic search", msg );
                statusMessage = "Error resource was not added to the collection.";
                return 0;
            }

            return id;
        }//

        /// <summary>
        /// Copy a resource to another collection using the Id column
        /// - first gets the resource and then invokes the overloaded method
        /// - if successful, the resource favourite count is incremented
        /// </summary>
        /// <param name="libraryResourceId"></param>
        /// <param name="toCollectionId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ResourceCopyById( int libraryResourceId, int toCollectionId, int userId, ref string statusMessage )
        {
            LibraryResource entity = libResManager.Get( libraryResourceId );
            if ( entity != null && entity.Id > 0 )
            {
                return ResourceCopy( entity.ResourceIntId, toCollectionId, userId, ref statusMessage );
            }
            else
            {
                statusMessage = "Error resource was not found for the provided Identifier.";
                return 0;
            }

        }//

        /// <summary>
        /// Copy a resource to another collection
        /// - first gets the resource and then invokes the overloaded method
        /// - if successful, the resource favourite count is incremented
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <param name="toCollectionId"></param>
        /// <param name="userId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int ResourceCopy( int resourceIntId, int toCollectionId, int userId, ref string statusMessage )
        {
            //do check in library - for use with incrementing favourites
            bool isInLibrary = IsResourceInLibraryByCollectionId( toCollectionId, resourceIntId );

            int id = libResManager.ResourceCopy( resourceIntId, toCollectionId, userId, ref statusMessage );
            if ( id > 0 )
            {
                //update favourites 
                if (isInLibrary == false) 
                    ResourceBizService.UpdateFavorite( resourceIntId );

                LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );
            }
            else
            {
                statusMessage = "Error - copy failed.";
                return 0;
            }
            return id;
        }//

        public string ResourceMoveById( int libraryResourceId, int toCollectionId, int userId, ref string statusMessage )
        {
            LibraryResource entity = libResManager.Get( libraryResourceId );
            if ( entity != null && entity.Id > 0 )
            {
                return ResourceMove( entity.LibrarySectionId, entity.ResourceIntId, toCollectionId, userId, ref statusMessage );
            }
            else
            {
                statusMessage = "Error resource was not found for the provided Identifier.";
                return "Error";
            }

        }//

        public string ResourceMove( int fromCollectionId, int resourceIntId, int toCollectionId, int userId, ref string statusMessage )
        {
            string message = libResManager.ResourceMove( fromCollectionId, resourceIntId, toCollectionId, userId, ref statusMessage );
            if ( message == "successful" )
            {
                LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( resourceIntId );
            }
            else
            {
                statusMessage = "Error - move failed.";
                return "Error";
            }
            return message;
        }//
        /// <summary>
        /// determine if resource is in the default library
        /// </summary>
        /// <param name="user"></param>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public bool IsResourceInLibrary( ThisUser user, int resourceIntId )
        {
            return libResManager.IsResourceInLibrary( user, resourceIntId );
        }//
        public bool IsResourceInLibrary( int libraryId, int resourceIntId )
        {
            return libResManager.IsResourceInLibrary( libraryId, resourceIntId );
        }//
        public bool IsResourceInLibraryByCollectionId( int collectionId, int resourceIntId )
        {
            
            //get collection to get library
            LibrarySection entity = LibrarySectionManager.Get( collectionId );
            if ( entity != null && entity.IsValidEntity() )
            {
                return libResManager.IsResourceInLibrary( entity.LibraryId, resourceIntId );
            }
            else return false;
        }//
        public bool IsLibraryEmpty( int libraryId )
        {
            return libResManager.IsLibraryEmpty( libraryId );
        }
        /// <summary>
        /// Return list of all resource ids in a library
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public List<int> LibraryResource_SelectAllResourceIdsForLibrary( int libraryId )
        {
            List<int> resIdsList = new List<int>();
            List<LibraryResource> list = libResManager.SelectAllResourceIdsForLibrary( libraryId );
            if ( list != null && list.Count > 0 )
            {
                foreach ( LibraryResource res in list )
                {
                    resIdsList.Add( res.ResourceIntId );
                }
            }

            return resIdsList;
        }//
        /// <summary>
        /// Return list of all resource ids in a collection
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public List<int> LibraryResource_SelectAllResourceIdsForCollection( int sectionId )
        {
            List<int> resIdsList = new List<int>();
            List<LibraryResource> list = libResManager.SelectAllResourcesForSection( sectionId );
            if ( list != null && list.Count > 0 )
            {
                foreach ( LibraryResource res in list )
                {
                    resIdsList.Add( res.ResourceIntId );
                }
            }

            return resIdsList;
        }//
        /// <summary>
        /// Return list of all resources for a library section
        /// </summary>
        /// <param name="sectionId"></param>
        /// <returns></returns>
        public List<LibraryResource> LibraryResource_SelectAllResourcesForSection( int sectionId )
        {
            return libResManager.SelectAllResourcesForSection( sectionId );
        }//

        /// <summary>
        /// Search for Library Resource related data using passed parameters
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public List<LibraryResource> SearchList( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return libResManager.SearchList( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }


        /// <summary>
        /// Search for Library Resource related data using passed parameters
        /// </summary>
        /// <param name="pFilter"></param>
        /// <param name="pOrderBy">Sort order of results. If blank, the proc should have a default sort order</param>
        /// <param name="pStartPageIndex"></param>
        /// <param name="pMaximumRows"></param>
        /// <param name="pTotalRows"></param>
        /// <returns></returns>
        public DataSet LibraryResourceSearch( string pFilter, string pOrderBy, int pStartPageIndex, int pMaximumRows, ref int pTotalRows )
        {
            return libResManager.Search( pFilter, pOrderBy, pStartPageIndex, pMaximumRows, ref pTotalRows );
        }
        #endregion


        #region ====== OBSOLETE/NOT USED ===============================================

        public LibraryResourceSearchResponse ResourceSearch( LibraryResourceSearchRequest request )
        {
            int totalRows = 0;
            string message = "";
            LibraryResourceSearchResponse searchResponse = new LibraryResourceSearchResponse();

            //search
            DataSet ds = libResManager.Search( request.Filter, request.SortOrder, request.StartingPageNbr, request.PageSize, ref totalRows );

            List<LibraryResourceDataContract> dataContractList = new List<LibraryResourceDataContract>();
            if ( LibManager.DoesDataSetHaveRows( ds ) )
            {
                //check for error message
                if ( ServiceHelper.HasErrorMessage( ds ) )
                {
                    message = ServiceHelper.GetWsMessage( ds );
                    searchResponse.Error.Message += message + "; ";
                    searchResponse.Status = StatusEnumDataContract.Failure;
                }
                else
                {
                    LibraryResourceDataContract dataContract;
                    foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
                    {
                        dataContract = new LibraryResourceDataContract();
                        dataContract = Fill( dr, false );
                        dataContractList.Add( dataContract );

                    } //end foreach
                }
            }
            else
            {
                searchResponse.Error.Message += "No records found for search request; ";
                searchResponse.Status = StatusEnumDataContract.NoData;
            }


            searchResponse = new LibraryResourceSearchResponse { ResourceList = dataContractList };
            searchResponse.ResultCount = dataContractList.Count;
            searchResponse.TotalRows = totalRows;

            return searchResponse;
        }

        private LibraryResourceDataContract Fill( DataRow dr, bool includeRelatedData )
        {
            LibraryResourceDataContract entity = new LibraryResourceDataContract();

            entity.NbrComments = GetRowColumn( dr, "NbrComments", 0 );
            entity.NbrStandards = GetRowColumn( dr, "NbrStandards", 0 );
            entity.LibrarySection = GetRowColumn( dr, "LibrarySection", "" );
            entity.LibrarySectionId = GetRowColumn( dr, "LibrarySectionId", 0 );

            //NEW - get integer version of resource id
            entity.ResourceIntId = GetRowColumn( dr, "ResourceIntId", 0 );
            entity.ResourceVersionIntId = GetRowColumn( dr, "ResourceVersionIntId", 0 );

            entity.Title = GetRowColumn( dr, "Title", "missing" );
            entity.Description = GetRowColumn( dr, "Description", "" );

            //get parent url
            entity.ResourceUrl = GetRowColumn( dr, "ResourceUrl", "" );
            //entity.LRDocId = GetRowColumn( dr, "DocId", "" );
            entity.Publisher = GetRowColumn( dr, "Publisher", "" );
            entity.Creator = GetRowColumn( dr, "Creator", "" );
            //entity.Submitter = GetRowColumn( dr, "Submitter", "" );
            //entity.TypicalLearningTime = GetRowColumn( dr, "TypicalLearningTime", "" );

            entity.LikeCount = GetRowColumn( dr, "LikeCount", 0 );
            entity.DislikeCount = GetRowColumn( dr, "DislikeCount", 0 );
            entity.Rights = GetRowColumn( dr, "Rights", "" );
            entity.AccessRights = GetRowColumn( dr, "AccessRights", "" );
            //entity.AccessRightsId = GetRowColumn( dr, "AccessRightsId", 0 );

            //entity.InteractivityTypeId = GetRowColumn( dr, "InteractivityTypeId", 0 );
            // entity.InteractivityType = GetRowColumn( dr, "InteractivityType", "" );

            entity.Modified = GetRowColumn( dr, "Modified", System.DateTime.MinValue );
            //entity.Created = GetRowColumn( dr, "Imported", System.DateTime.MinValue );
            // entity.SortTitle = GetRowColumn( dr, "SortTitle", "" );
            // entity.Schema = GetRowColumn( dr, "Schema", "" );

            if ( includeRelatedData == true )
            {

                entity.Subjects = GetRowColumn( dr, "Subjects", "" );
                entity.EducationLevels = GetRowColumn( dr, "EducationLevels", "" );
                entity.Keywords = GetRowColumn( dr, "Keywords", "" );
                entity.LanguageList = GetRowColumn( dr, "LanguageList", "" );
                entity.ResourceTypesList = GetRowColumn( dr, "ResourceTypesList", "" );
                // entity.AudienceList = GetRowColumn( dr, "AudienceList", "" );
                if ( entity.ResourceTypesList.Length > 0 )
                {
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&lt;", "<" );
                    entity.ResourceTypesList = entity.ResourceTypesList.Replace( "&gt;", ">" );
                }
            }

            return entity;
        }//
        #endregion


        #region ====== Elastic search methods for Library resources ===============================================

        /// <summary>
        /// Get unique lists of all libraries and collections containing the resource Id
        /// Then call method to refresh the elastic search index
        /// </summary>
        /// <param name="resourceIntId"></param>
        /// <returns></returns>
        public bool LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource( int resourceIntId )
        {
            List<int> libList = new List<int>();
            List<int> collList = new List<int>();

            bool result = libResManager.SelectAllLibrariesAndSectionsForResource( resourceIntId, ref libList, ref collList );
            //call elasticSearch method to update lists

            new LRWarehouse.DAL.ElasticSearchManager().RefreshLibraryCollectionTotals( resourceIntId, libList, collList );

            return result;
        }//

        public bool LibraryResource_UpdateElasticSearchForAllLibrariesAndSectionsForResource2( int resourceIntId )
        {
            List<int> libList = new List<int>();
            List<int> collList = new List<int>();

            bool result = libResManager.SelectAllLibrariesAndSectionsForResource( resourceIntId, ref libList, ref collList );
            //call elasticSearch method to update lists

            new LRWarehouse.DAL.ElasticSearchManager().RefreshLibraryCollectionTotals( resourceIntId, libList, collList );

            return result;
        }//

        #endregion

        #region SUBSCRIPTIONS

        #region ====== Library subscription methods ===============================================
        /// <summary>
        /// Return true if user is subscibed to any libraries
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool HasLibrarySubScriptions( int userId )
        {
            int pTotalRows = 0;
            string pFilter = string.Format( "( lib.Id in (SELECT  LibraryId FROM [Library.Subscription] where UserId = {0}) )", userId );
            DataSet ds = LibrarySearch( pFilter, "", 1, 10, ref pTotalRows );
            if ( LibManager.DoesDataSetHaveRows( ds ) )
                return true;
            else
                return false;
        }

        public bool IsSubcribedtoLibrary( int libraryId, int userId )
        {
            if ( libraryId == 0 || userId == 0 )
                return false;

            int pTotalRows = 0;
            //temp quick and dirty
            string pFilter = string.Format( "( lib.Id in (SELECT  LibraryId FROM [Library.Subscription] where UserId = {0} AND LibraryId = {1}) )", userId, libraryId );
            DataSet ds = LibrarySearch( pFilter, "", 1, 10, ref pTotalRows );
            if ( LibManager.DoesDataSetHaveRows( ds ) )
                return true;
            else
                return false;
        }

        public ObjectSubscription LibrarySubscriptionGet( int libraryId, int userId )
        {
            if ( libraryId == 0 || userId == 0 )
                return new ObjectSubscription(); ;

            SubscriptionManager mgr = new SubscriptionManager();
            return mgr.Get( libraryId, userId );

        }

        public bool LibrarySubscriptionDelete( int subId, ref string statusMessage )
        {
            SubscriptionManager mgr = new SubscriptionManager();
            return mgr.Delete( subId, ref statusMessage );

        }

        /// <summary>
        /// Add a new library subscription/membership
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int LibrarySubScriptionCreate( int libraryId, int userId, int typeId, ref string statusMessage )
        {
            SubscriptionManager mgr = new SubscriptionManager();
            int id = mgr.Create( libraryId, userId, typeId, ref statusMessage );
            return id;
        }

        public string LibrarySubScriptionUpdate( int librarySubId, int typeId )
        {
            SubscriptionManager mgr = new SubscriptionManager();
            ObjectSubscription entity = mgr.Get( librarySubId );

            return mgr.Update( librarySubId, typeId); ;
        }
        public string LibrarySubScriptionUpdate( int libraryId, int userId, int typeId )
        {
            SubscriptionManager mgr = new SubscriptionManager();
            ObjectSubscription entity = mgr.Get( libraryId, userId );
            //get record:

            return mgr.Update( entity.Id, typeId );
        }
        #endregion


        #region ====== Library collection subscription methods ==================================

        public bool IsSubcribedtoCollection( int collectionId, int userId )
        {
            if ( collectionId == 0 || userId == 0 )
                return false;

            //Library_SectionSubscription mbr = CollectionSubscriptionGet( collectionId, userId );
            ObjectSubscription mbr = CollectionSubscriptionGet( collectionId, userId );
            if ( mbr != null && mbr.Id > 0 )
                return true;
            else
                return false;
        }

        public ObjectSubscription CollectionSubscriptionGet( int collectionId, int userId )
        {
            if ( collectionId == 0 || userId == 0 )
                return new ObjectSubscription(); ;

            if ( 1 == 1 )
            {
                return CollectionSubscriptionGetDal( collectionId, userId );
            }
            else
            {
                return CollectionSubscriptionGetEF( collectionId, userId );
            }
            
        }
        private ObjectSubscription CollectionSubscriptionGetDal( int collectionId, int userId )
        {
            ObjectSubscription sub = new ObjectSubscription();

            SubscriptionManager mgr = new SubscriptionManager();
            sub = mgr.CollectionSubcriptionGet( collectionId, userId );
            return sub;
        }
        private ObjectSubscription CollectionSubscriptionGetEF( int collectionId, int userId )
        {
            ObjectSubscription sub = new ObjectSubscription();

            Library_SectionSubscription entity = ctx.Library_SectionSubscription.Single( s => s.SectionId == collectionId && s.UserId == userId );
            if ( entity != null && entity.Id > 0 )
            {
                sub.Id = entity.Id;
                sub.ParentId = entity.SectionId;
                sub.UserId = entity.UserId;
                if (entity.Created != null)
                sub.Created = (System.DateTime) entity.Created;
            }

            return sub;
        }
        /// <summary>
        /// Add a new collection subscription/membership
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int CollectionSubscriptionCreate( int collectionId, int userId, int typeId, ref string statusMessage )
        {
            EFDAL.Library_SectionSubscription entity = new EFDAL.Library_SectionSubscription();
            entity.SectionId = collectionId;
            entity.SubscriptionTypeId = typeId;
            entity.UserId = userId;
            entity.Created = System.DateTime.Now;

            ctx.Library_SectionSubscription.Add( entity );
            // submit the change to database
            // how to get the last id inserted??
            //should be returned/populated in entity
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
        public int CollectionSubscriptionCreateDAL( int collectionId, int userId, int typeId, ref string statusMessage )
        {

            SubscriptionManager mgr = new SubscriptionManager();
            int id = mgr.CollectionSubcriptionCreate( collectionId, userId, typeId, ref statusMessage );
            return id;
        }
        
        /// <summary>
        /// update a collection subscription/membership
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public string CollectionSubScriptionUpdate( int subscriptionId, int typeId )
        {
            Library_SectionSubscription entity = ctx.Library_SectionSubscription.SingleOrDefault( s => s.Id == subscriptionId );
            if ( entity != null && entity.Id > 0 )
            {
                entity.SubscriptionTypeId = typeId;
                entity.LastUpdated = System.DateTime.Now;
                // submit the change to database
                ctx.SaveChanges();

                return "successful";
            }
            else
            {
                return "unsuccessful";
            }
            //SubscriptionManager mgr = new SubscriptionManager();

            //return mgr.CollectionSubcriptionUpdate( subscriptionId, typeId );
        }
        /// <summary>
        /// update a collection subscription/membership
        /// </summary>
        /// <param name="collectionId"></param>
        /// <param name="userId"></param>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public string CollectionSubScriptionUpdate( int collectionId, int userId, int typeId )
        {
            Library_SectionSubscription entity = ctx.Library_SectionSubscription.SingleOrDefault( s => s.SectionId == collectionId && s.UserId == userId );
            if ( entity != null && entity.Id > 0 )
            {
                entity.SubscriptionTypeId = typeId;
                entity.LastUpdated = System.DateTime.Now;
                // submit the change to database
                ctx.SaveChanges();

                return "successful";
            }
            else
            {
                return "unsuccessful";
            }
            //SubscriptionManager mgr = new SubscriptionManager();
            //ObjectSubscription entity = mgr.CollectionSubcriptionGet( collectionId, userId );
            ////get record:

            //return mgr.CollectionSubcriptionUpdate( entity.Id, typeId );
        }

        public bool CollectionSubscriptionDelete( int subscriptionId, ref string statusMessage )
        {
            return CollectionSubscriptionDeleteEF( subscriptionId, ref statusMessage );

        }
        private bool CollectionSubscriptionDeleteEF( int subscriptionId, ref string statusMessage )
        {
            bool action = true;
            Library_SectionSubscription entity = ctx.Library_SectionSubscription.Single( s => s.Id == subscriptionId );
            if ( entity != null )
            {
                ctx.Library_SectionSubscription.Remove( entity );
                ctx.SaveChanges();
            }
            else
            {
                action = false;
                statusMessage = "Error subscription was not found.";
            }
            return action;

        }

        public bool CollectionSubscriptionDelete( int collectionId, int userId, ref string statusMessage )
        {
            return CollectionSubscriptionDeleteEF( collectionId, userId, ref statusMessage );

        }
        private bool CollectionSubscriptionDeleteEF( int collectionId, int userId, ref string statusMessage )
        {
            bool action = true;
            Library_SectionSubscription entity = ctx.Library_SectionSubscription.SingleOrDefault( s => s.SectionId == collectionId && s.UserId == userId );
            if ( entity != null )
            {
                ctx.Library_SectionSubscription.Remove( entity );
                ctx.SaveChanges();
            }
            else
            {
                action = false;
                statusMessage = "Error subscription was not found.";
            }

            return action;

        }
        private bool CollectionSubscriptionDeleteDAL( int subscriptionId, ref string statusMessage )
        {
            SubscriptionManager mgr = new SubscriptionManager();
            return mgr.CollectionSubcriptionDelete( subscriptionId, ref statusMessage );

        }
        #endregion
        #endregion

        #region COMMENTS AND LIKES
        #region ====== Library comments and likes methods ===============================================
        public int LibraryCommentCreate( ObjectComment entity )
        {

            return LibraryCommentCreate( entity.ParentId, entity.Comment, entity.CreatedById );
            
        }
        public int LibraryCommentCreate( int libraryId, string comment, int createdById  )
        {
            Library_Comment c = new Library_Comment();
            c.Comment = comment;
            c.CreatedById = createdById;
            c.Created = System.DateTime.Now;
            c.LibraryId = libraryId;

            ctx.Library_Comment.Add( c );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return c.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }
        public List<ObjectComment> LibraryComment_Select( int libraryId )
        {
            return myMgr.SelectLibraryComments( libraryId );
        }

        public int LibraryLikeCreate( ObjectLike entity )
        {
            Library_Like c = new Library_Like();
            c.IsLike = entity.IsLike;
            c.CreatedById = entity.CreatedById;
            c.Created = System.DateTime.Now;
            c.LibraryId = entity.ParentId;

            ctx.Library_Like.Add( c );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return c.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }
        public int LibraryLikeCreate( int libraryId, bool IsLike, int createdById )
        {
            Library_Like c = new Library_Like();
            c.LibraryId = libraryId;
            c.IsLike = IsLike;
            c.Created = System.DateTime.Now;
            c.CreatedById = createdById;

            ctx.Library_Like.Add( c );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return c.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }
        public ObjectLike Library_GetLike( int libraryId, int userId )
        {
            return myMgr.GetLike( libraryId, userId );
        }
        public List<DataItem> Library_LikeSummary( int libraryId )
        {
            return myMgr.LikeSummary( libraryId );
        }
        #endregion


        #region ====== Library section comments and likes methods ===========================

        public int LibrarySectionCommentCreate( ObjectComment entity )
        {
            return LibrarySectionCommentCreate( entity.ParentId, entity.Comment, entity.CreatedById );
           
        }
        public int LibrarySectionCommentCreate( int sectionId, string comment, int createdById )
        {
            Library_SectionComment c = new Library_SectionComment();
            c.Comment = comment;
            c.CreatedById = createdById;
            c.Created = System.DateTime.Now;
            c.SectionId = sectionId;

            ctx.Library_SectionComment.Add( c );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return c.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }
        public List<ObjectComment> LibrarySectionComment_Select( int collectionId )
        {
            return LibrarySectionManager.SelectCollectionComments( collectionId );
        }

        public int LibrarySectionLikeCreate( ObjectLike entity )
        {
            Library_SectionLike c = new Library_SectionLike();
            c.SectionId = entity.ParentId;
            c.IsLike = entity.IsLike;
            c.CreatedById = entity.CreatedById;

            ctx.Library_SectionLike.Add( c );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return c.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }

        public int LibrarySectionLikeCreate( int sectionId, bool IsLike,int createdById )
        {
            Library_SectionLike c = new Library_SectionLike();
            c.SectionId = sectionId;
            c.IsLike = IsLike;
            c.Created = System.DateTime.Now;
            c.CreatedById = createdById;

            ctx.Library_SectionLike.Add( c );

            // submit the change to database
            int count = ctx.SaveChanges();
            if ( count > 0 )
            {
                return c.Id;
            }
            else
            {
                //?no info on error
                return 0;
            }
        }
        public ObjectLike LibrarySection_GetLike( int collectionId, int userId )
        {
            return LibrarySectionManager.GetLike( collectionId, userId );
        }
        public List<DataItem> LibrarySection_LikeSummary( int collectionId )
        {
            return LibrarySectionManager.LikeSummary( collectionId );
        }
        #endregion
        #endregion

        #region ====== Library codes===============================================
        public List<DataItem> AvailableFiltersForLibrary( int libraryId, string pCodeTableSuffix )
        {
            return myMgr.AvailableFiltersForLibrary( libraryId, pCodeTableSuffix );
        }

        public List<DataItem> AvailableFiltersForCollection( int sectionId, string pCodeTableSuffix )
        {
            return myMgr.AvailableFiltersForSection( sectionId, pCodeTableSuffix );
        }
        public DataSet SelectLibraryTypes()
        {
            return myMgr.SelectLibraryTypes();
        }

        public DataSet SelectLibrarySectionTypes()
        {
            return myMgr.SelectLibrarySectionTypes();
        }
        public DataSet SelectSubscriptionTypes()
        {
            return myMgr.SelectSubscriptionTypes();
        }


        public IQueryable<EFDAL.Codes_GradeLevel> GetCodes_GradeLevel()
        {
            return ctx.Codes_GradeLevel;
        }

        public static List<CodeItem> GetCodes_LibraryAccessLevel()
        {
            CodeItem ci = new CodeItem();
            List<EFDAL.Codes_LibraryAccessLevel> eflist = new EFDAL.IsleContentEntities().Codes_LibraryAccessLevel
                                .ToList();
            List<CodeItem> list = new List<CodeItem>();
            if ( eflist.Count > 0 )
            {
                foreach ( EFDAL.Codes_LibraryAccessLevel item in eflist )
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.Title;
                    list.Add( ci );
                }
            }
            return list;
        }

        public static List<CodeItem> GetCodes_LibraryMemberType()
        {
            CodeItem ci = new CodeItem();
            List<EFDAL.Codes_LibraryMemberType> eflist = new EFDAL.IsleContentEntities().Codes_LibraryMemberType
                            .ToList();
            List<CodeItem> list = new List<CodeItem>();
            if ( eflist.Count > 0 )
            {
                foreach ( EFDAL.Codes_LibraryMemberType item in eflist )
                {
                    ci = new CodeItem();
                    ci.Id = item.Id;
                    ci.Title = item.Title;
                    list.Add( ci );
                }
            }
            return list;
        }
        #endregion
    }
}
