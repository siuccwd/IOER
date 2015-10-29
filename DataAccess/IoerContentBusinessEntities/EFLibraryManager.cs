using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SqlClient;
using System.Linq;
using System.Text;
using AutoMapper;

using EF_LM = IoerContentBusinessEntities.Library_Member;
using EF_LSM = IoerContentBusinessEntities.Library_SectionMember;
using ILP = ILPathways.Business;
using ILPathways.Business;
using ILPathways.Utilities;
using Isle.DTO;
using Isle.DTO.Filters;

namespace IoerContentBusinessEntities
{
    public class EFLibraryManager
    {
        static int     MAX_RESOURCE_BOX_COUNT = 10;
        public EFLibraryManager() { }
        #region === Library ============
        public static ILP.Library Library_Get( int libraryId )
        {
            ILP.Library entity = new ILP.Library();
            using ( var context = new IsleContentContext() )
            {
                Library lib = context.Libraries.SingleOrDefault( s => s.Id == libraryId );
                if ( lib != null && lib.Id > 0 )
                {
                    entity = Library_ToMap( lib );
                }
            }
            return entity;
        }

        public static string GetLibraryFriendlyUrl( ILP.Library entity )
        {
            if ( entity == null || entity.Id == 0 )
                return "";
            else
                return string.Format( "/Library/{0}/{1}", entity.Id, UtilityManager.UrlFriendlyTitle( entity.Title ) );

        }
        public static void Library_FillDashboard( DashboardDTO dashboard, int libraryId, int requestedByUserId )
        {
            
            ILP.Library entity = new ILP.Library();
            using ( var context = new IsleContentContext() )
            {
                Library lib = context.Libraries.SingleOrDefault( s => s.Id == libraryId );
                if ( lib != null && lib.Id > 0 )
                {
                    entity = Library_ToMap( lib );
                    Library_FillDashboard( dashboard, entity, requestedByUserId );
                }
            }
            //return entity;
        }

        public static void Library_FillDashboard( DashboardDTO dashboard, ILP.Library entity, int requestedByUserId )
        {
            if ( dashboard.maxResources == 0 )
                dashboard.maxResources = MAX_RESOURCE_BOX_COUNT;

            using ( var context = new IsleContentContext() )
            {
                dashboard.libraryId = entity.Id;
                dashboard.libraryUrl = GetLibraryFriendlyUrl(entity);
                dashboard.libraryPublicAccessLevel = entity.PublicAccessLevelInt;
                dashboard.libraryOrgAccessLevel = entity.OrgAccessLevelInt;

                dashboard.library = new ResourcesBox();
                dashboard.library.name = "Resources From My Library";
                dashboard.library.resources = RecentResources_ForLibrary( context, dashboard.library, entity.Id, dashboard.maxResources );

               //now get followed libraries
                if ( dashboard.includeMyFollowedLibraries > 0 )
                {
                    dashboard.followedLibraries = new ResourcesBox();
                    dashboard.followedLibraries.name = "My Followed Libraries";
                    dashboard.followedLibraries.resources = GetMyFollowedLibraries( context, dashboard.followedLibraries, dashboard.userId, dashboard.maxResources );
                }
                //now get followed libraries resources
                if ( dashboard.includeMyFollowedLibrariesRecentResources > 0 )
                {
                dashboard.followedLibraryResources = new ResourcesBox();
                    dashboard.followedLibraryResources.name = "Recent Resources from My Followed Libraries";
                dashboard.followedLibraryResources.resources = RecentResources_ForFollowedLibraries( context, dashboard.followedLibraryResources, dashboard.userId, dashboard.maxResources );
                }

                //now get libraries where a member
                if ( dashboard.includeMyMemberLibraries > 0 )
                {
                dashboard.orgLibraries = new ResourcesBox();
                dashboard.orgLibraries.name = "Library Memberships";
                dashboard.orgLibraries.resources = GetMyMemberLibraries( context, dashboard.orgLibraries, dashboard.userId, dashboard.maxResources );
                }
                
                if ( dashboard.includeMyMemberLibrariesRecentResources > 0 )
                {
                    dashboard.orgLibrariesResources = new ResourcesBox();
                    dashboard.orgLibrariesResources.name = "Recent Resources from Library Memberships";
                    dashboard.orgLibrariesResources.resources = RecentResources_ForLibraryMemberships( context, dashboard.followedLibraryResources, dashboard.userId, dashboard.maxResources );
            }
            }
            //return entity;
        }
        public static List<DashboardResourceDTO> RecentResources_ForLibrary( IsleContentContext context, ResourcesBox resourceBox, int libraryId, int maxResources )
        {
            DashboardResourceDTO entity = new DashboardResourceDTO();
            List<DashboardResourceDTO> list = new List<DashboardResourceDTO>();

        
            //??not sure if will get all, and break after max (will have total available then)
            List<Library_SectionResourceSummary> items = context.Library_SectionResourceSummary
                            .Where( s => s.LibraryId == libraryId )
                            .OrderByDescending( s => s.DateAddedToCollection )
                            .Take(maxResources)
                            .ToList();

            if ( items.Count > 0 )
            {
                foreach ( Library_SectionResourceSummary item in items )
                {
                    if ( resourceBox.total == 0 )
                        resourceBox.total = (int) item.LibraryResourceCount;

                    entity = new DashboardResourceDTO();
                    entity.id = item.ResourceIntId;
                    entity.title = item.Title;
                    entity.containerTitle = item.LibrarySection;
                    entity.DateAdded = item.DateAddedToCollection;
                    entity.url = string.Format( "/Resource/{0}/{1}", item.ResourceIntId, UtilityManager.UrlFriendlyTitle( item.Title ) );

                    list.Add( entity );

                }
            }
       
            return list;
        }
        public static List<DashboardResourceDTO> GetMyFollowedLibraries( IsleContentContext context, ResourcesBox resourceBox, int userId, int maxResources )
        {
            DashboardResourceDTO entity = new DashboardResourceDTO();
            List<DashboardResourceDTO> list = new List<DashboardResourceDTO>();

            List<Library_Subscription> items = context.Library_Subscription
                                            .Include("Library")
                                            .Where( s => s.UserId == userId )
                                            .OrderBy( s => s.Library.Title )
                                            .Take( maxResources )
                                            .ToList();

            if ( items.Count > 0 )
            {
                foreach ( Library_Subscription item in items )
                {
                    //if ( resourceBox.total == 0 )
                    //    resourceBox.total = ( int ) item.LibraryResourceCount;

                    entity = new DashboardResourceDTO();
                    entity.id = item.LibraryId;
                    entity.title = item.Library.Title;
                    //???
                    //entity.containerTitle = item.Library.Title;
                    entity.DateAdded = item.Library.Created;
                    entity.url = string.Format( "/Library/{0}/{1}", entity.id, UtilityManager.UrlFriendlyTitle( entity.title ) );
                    entity.imageUrl = item.Library.ImageUrl;

                    list.Add( entity );

                }
                if ( resourceBox.total == 0 )
                    resourceBox.total = ( int ) items.Count;
            }

            return list;
        }
        public static List<DashboardResourceDTO> RecentResources_ForFollowedLibraries( IsleContentContext context, ResourcesBox resourceBox, int userId, int maxResources )
        {
            DashboardResourceDTO entity = new DashboardResourceDTO();
            List<DashboardResourceDTO> list = new List<DashboardResourceDTO>();


            //??not sure if will get all, and break after max (will have total available then)
            List<Library_FollowingSummary> items = context.Library_FollowingSummary
                            .Where( s => s.libraryFollowerId == userId || s.collectionFollowerId == userId )
                            .OrderByDescending( s => s.DateAddedToCollection )
                            .Take( maxResources )
                            .ToList();

            if ( items.Count > 0 )
            {
                foreach ( Library_FollowingSummary item in items )
                {
                    if ( resourceBox.total == 0 )
                        resourceBox.total = ( int )item.LibraryResourceCount;

                    entity = new DashboardResourceDTO();
                    entity.id = item.ResourceIntId;
                    entity.title = item.Title;
                    entity.containerTitle = item.Library + " -<br/> " + item.LibrarySection;
                    entity.DateAdded = item.DateAddedToCollection;
                    entity.url = string.Format( "/Resource/{0}/{1}", item.ResourceIntId, UtilityManager.UrlFriendlyTitle( item.Title) );

                    list.Add( entity );

                }
            }

            return list;
        }

        /// <summary>
        /// Retreive libraries where user is an explicit member.
        /// TODO - probably should include org libraries where user is org member (and not explicity mbr)
        /// - this could be like the list of libraries where can contribute!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="resourceBox"></param>
        /// <param name="userId"></param>
        /// <param name="maxResources"></param>
        /// <returns></returns>
        public static List<DashboardResourceDTO> GetMyMemberLibraries( IsleContentContext context, ResourcesBox resourceBox, int userId, int maxResources )
        {
            DashboardResourceDTO entity = new DashboardResourceDTO();
            List<DashboardResourceDTO> list = new List<DashboardResourceDTO>();

            //get libs, and remove personal
            //var libs = context.Library_SelectCanContribute2( userId, 0).ToList();

            List<Library_Member> items = context.Library_Member
                                           .Include( "Library" )
                                           .Where( s => s.UserId == userId )
                                           .OrderBy( s => s.Library.Title )
                                           .Take( maxResources )
                                           .ToList();

            if ( items.Count > 0 )
            {
                foreach ( Library_Member item in items )
                {


                    entity = new DashboardResourceDTO();
                    entity.id = item.LibraryId;
                    entity.title = item.Library.Title;
                    //???
                    //entity.containerTitle = item.Library.Title;
                    entity.DateAdded = item.Library.Created;
                    entity.url = string.Format( "/Library/{0}/{1}", entity.id, UtilityManager.UrlFriendlyTitle( entity.title) );
                    entity.imageUrl = item.Library.ImageUrl;

                    list.Add( entity );

                }
                if ( resourceBox.total == 0 )
                    resourceBox.total = ( int ) items.Count;
            }

            return list;
        }
        public static List<DashboardResourceDTO> RecentResources_ForLibraryMemberships( IsleContentContext context, ResourcesBox resourceBox, int userId, int maxResources )
        {
            DashboardResourceDTO entity = new DashboardResourceDTO();
            List<DashboardResourceDTO> list = new List<DashboardResourceDTO>();


            //??not sure if will get all, and break after max (will have total available then)
            List<Library_MemberResourceSummary> items = context.Library_MemberResourceSummary
                            .Where( s => s.libraryFollowerId == userId || s.collectionFollowerId == userId )
                            .OrderByDescending( s => s.DateAddedToCollection )
                            .Take( maxResources )
                            .ToList();

            if ( items.Count > 0 )
            {
                foreach ( Library_MemberResourceSummary item in items )
                {
                    if ( resourceBox.total == 0 )
                        resourceBox.total = ( int )item.LibraryResourceCount;

                    entity = new DashboardResourceDTO();
                    entity.id = item.ResourceIntId;
                    entity.title = item.Title;
                    entity.containerTitle = item.Library + " -<br/> " + item.LibrarySection;
                    entity.DateAdded = item.DateAddedToCollection;
                    entity.url = string.Format( "/Resource/{0}/{1}", item.ResourceIntId, UtilityManager.UrlFriendlyTitle( item.Title ) );

                    list.Add( entity );

                }
            }

            return list;
        }


        public static ILP.Library Library_ToMap( Library fromEntity )
        {

            //Mapper.CreateMap<Library_Member, LibraryMember>()
            //    .ForMember( dest => dest.ParentId, opt => opt.MapFrom( src => src.LibraryId ) );
            //LibraryMember to = Mapper.Map<Library_Member, LibraryMember>( fromEntity );

            ILP.Library to = new ILP.Library();
            to.Id = fromEntity.Id;
            to.Title = fromEntity.Title;
			to.FriendlyTitle = UtilityManager.UrlFriendlyTitle( to.Title );

            to.Description = fromEntity.Description;
            to.LibraryTypeId = fromEntity.LibraryTypeId == null ? 1 : ( int )fromEntity.LibraryTypeId;
            //get type
            to.LibraryType = "TBD";

            to.OrgId = fromEntity.OrgId == null ? 0 : ( int )fromEntity.OrgId;
            //get type
            to.Organization = "TBD";

            to.IsActive = fromEntity.IsActive == null ? true : ( bool )fromEntity.IsActive;
            to.IsDiscoverable = fromEntity.IsDiscoverable == null ? true : ( bool )fromEntity.IsDiscoverable;
            to.PublicAccessLevel = ( EObjectAccessLevel )fromEntity.PublicAccessLevel;
            to.OrgAccessLevel = ( EObjectAccessLevel )fromEntity.OrgAccessLevel;

            to.AllowJoinRequest = fromEntity.AllowJoinRequest == null ? true : ( bool )fromEntity.AllowJoinRequest;

            to.ImageUrl = fromEntity.ImageUrl;
            to.Description = fromEntity.Description;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int )fromEntity.CreatedById;
            if ( fromEntity.LastUpdated != null )
                to.LastUpdated = ( System.DateTime )fromEntity.LastUpdated;
            else
                to.LastUpdated = to.Created;
            to.LastUpdatedById = ( int )fromEntity.LastUpdatedById;
            to.RowId = ( Guid )fromEntity.RowId;

            return to;
        }

        #endregion

		#region === Library Utilities ============
		/// <summary>
		/// Set all libraries for org to be inactive
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="updatedById"></param>
		/// <param name="activeState">set to true or false</param>
		/// <returns></returns>
		public bool Library_SetOrgLibsActiveState( int orgId, int updatedById, bool activeState )
		{
			bool action = false;
			using ( var context = new IsleContentContext() )
			{
				List<Library> list = context.Libraries
					.Where( s => s.OrgId == orgId )
					.ToList();
				foreach ( Library item in list )
				{
					item.IsActive = activeState;
					item.LastUpdated = System.DateTime.Now;
					item.LastUpdatedById = updatedById;

					context.SaveChanges();
				}

			}
			return action;
		}


		#endregion
		#region === Library Collection============
		public static ILP.LibrarySection LibrarySection_Get( int collectionId )
        {
            ILP.LibrarySection entity = new ILP.LibrarySection();
            using ( var context = new IsleContentContext() )
            {
                Library_Section efEntity = context.Library_Section
                        .Include( "Library" )
                        .SingleOrDefault( s => s.Id == collectionId );
                if ( efEntity != null && efEntity.Id > 0 )
                {
                    entity = LibrarySection_ToMap( efEntity );
                }
            }
            return entity;
        }
        public static ILP.LibrarySection LibrarySection_ToMap( Library_Section fromEntity )
        {

            ILP.LibrarySection to = new ILP.LibrarySection();
            to.Id = fromEntity.Id;
            to.LibraryId = fromEntity.LibraryId;
            to.SectionTypeId = fromEntity.SectionTypeId < 1 ? 1 : ( int ) fromEntity.SectionTypeId;
            //get type
            if (fromEntity.Library_SectionType != null && fromEntity.Library_SectionType.Title != null)
                to.SectionType = fromEntity.Library_SectionType.Title;

            to.Title = fromEntity.Title;
			to.FriendlyTitle = UtilityManager.UrlFriendlyTitle( to.Title );

            to.Description = fromEntity.Description;
            to.ParentId = fromEntity.ParentId == null ? 0 : ( int ) fromEntity.ParentId;
            to.IsDefaultSection = fromEntity.IsDefaultSection == null ? false : ( bool ) fromEntity.IsDefaultSection;
            to.AreContentsReadOnly = fromEntity.AreContentsReadOnly == null ? false : ( bool ) fromEntity.AreContentsReadOnly;

            to.ImageUrl = fromEntity.ImageUrl;
            //get library
            if ( fromEntity.Library != null && fromEntity.Library.Id > 0 )
                to.ParentLibrary = Library_ToMap( fromEntity.Library );

            to.PublicAccessLevel = fromEntity.PublicAccessLevel == 0 ? EObjectAccessLevel.ReadOnly : ( EObjectAccessLevel ) fromEntity.PublicAccessLevel;
            to.OrgAccessLevel = fromEntity.OrgAccessLevel == 0 ? EObjectAccessLevel.ReadOnly : ( EObjectAccessLevel ) fromEntity.OrgAccessLevel;

            to.Created = fromEntity.Created == null || ( System.DateTime ) fromEntity.Created <= to.DefaultDate ? to.DefaultDate 
                    : ( System.DateTime ) fromEntity.Created;
            to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;
;
            if ( fromEntity.LastUpdated != null )
                to.LastUpdated = ( System.DateTime ) fromEntity.LastUpdated;
            else
                to.LastUpdated = to.Created;
            to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;
            if (fromEntity.RowId != null)
                to.RowId = ( Guid ) fromEntity.RowId;

            return to;
        }
        #endregion

        #region     Library resources
        public int LibraryResource_Create( int librarySectionId, int resourceIntId, int createdById, ref string statusMessage )
        {

            return LibraryResource_Create( librarySectionId, resourceIntId, createdById, true, ref statusMessage);
        }

        public int LibraryResource_Create( int librarySectionId, int resourceIntId, int createdById, bool isActive, ref string statusMessage )
        {

            Library_Resource entity = new Library_Resource();
            try
            {
                entity.LibrarySectionId = librarySectionId;
                entity.ResourceIntId = resourceIntId;
                entity.IsActive = isActive;
                entity.Created = DateTime.Now;
                entity.CreatedById = createdById;
                entity.Created = System.DateTime.Now;


                using ( var context = new IsleContentContext() )
                {
                    context.Library_Resource.Add( entity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        statusMessage = "Successful";
                        return entity.Id;
                    }
                    else
                    {
                        statusMessage = "Error - Library_Resource failed";
                        //?no info on error
                        return 0;
                    }
                }
            }
            catch ( Exception ex )
            {
				LoggingHelper.LogError( ex, string.Format( "LibraryResource_Create( librarySectionId: {0}, resourceIntId, {1},userId: {2})", librarySectionId, resourceIntId, createdById ) );
                statusMessage = ex.Message;
                return 0;
            }

        }
        public static bool LibraryResource_Activate( int libraryResourceId, ref string statusMessage )
        {
            bool action = false;
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    Library_Resource libRes = context.Library_Resource.SingleOrDefault( s => s.Id == libraryResourceId );
                    if ( libRes != null && libRes.Id > 0 )
                    {
                        libRes.IsActive = true;
                        //do we need a lastupdated/approved

                        context.SaveChanges();
                        action = true;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, string.Format( "LibraryResource_Activate( libraryResourceId: {0})", libraryResourceId) );
                statusMessage = ex.Message;
                return false; ;
            }
            return action;
        }
        public static LibraryResource LibraryResource_Get( int libraryResourceId, ref string statusMessage )
        {
            return LibraryResource_Get( libraryResourceId, false, ref statusMessage );
        }

        public static LibraryResource LibraryResource_Get( int libraryResourceId, bool includingParents, ref string statusMessage )
        {
            LibraryResource entity = new LibraryResource();
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    Library_Resource libRes = context.Library_Resource
                                .Include( "Library_Section" )
                                .SingleOrDefault( s => s.Id == libraryResourceId );
                    if ( libRes != null && libRes.Id > 0 )
                    {
                        entity.Id = libRes.Id;
                        entity.ResourceIntId = libRes.ResourceIntId;
                        entity.LibrarySectionId = libRes.LibrarySectionId;
                        entity.IsValid = ( bool )libRes.IsActive;
                        entity.Created = libRes.Created;
                        entity.CreatedById = ( int )libRes.CreatedById;

                        if ( entity.ResourceIntId > 0 )
                        {
                            //get resource
                            LR_ResourceVersion_Summary lr = ResourceSummary_Get( entity.ResourceIntId );
                            if ( lr != null && lr.Id > 0 )
                            {
                                entity.Title = lr.Title;
                                entity.ResourceUrl = lr.ResourceUrl;
                                entity.SortTitle = lr.SortTitle;
                                entity.Description = lr.Description;
                            }
                            else
                            {
                                LoggingHelper.LogError( string.Format( "EFLibraryManager.LibraryResource_Get(libraryResourceId: {0}) the resource was not found for resourceId: {1}", libraryResourceId, entity.ResourceIntId ), true );
                                return entity;
                            } 
                        }
                        if ( includingParents )
                        {
                            if ( libRes.Library_Section != null && libRes.Library_Section.Id > 0 )
                            {
                                entity.CollectionTitle = libRes.Library_Section.Title;
                                entity.LibraryId = libRes.Library_Section.LibraryId;
                                entity.ResourceSection = LibrarySection_ToMap( libRes.Library_Section );
                            }
                            else
                            {
                                entity.ResourceSection = LibrarySection_Get( entity.LibrarySectionId );
                                entity.CollectionTitle = entity.ResourceSection.Title;
                                entity.LibraryId = entity.ResourceSection.LibraryId;
                            }

                            entity.ResourceLibrary = Library_Get( entity.LibraryId );

                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, string.Format( "LibraryResource_Get( libraryResourceId: {0})", libraryResourceId ) );
                entity.IsValid = false;
                entity.Message = ex.Message;
                return entity; ;
            }
            return entity;
        }
        public static LibraryResource LibraryResource_Get( int collectionId, int resourceId )
        {
            LibraryResource entity = new LibraryResource();
            try
            {
                using ( var context = new IsleContentContext() )
                {
                    Library_Resource libRes = context.Library_Resource.SingleOrDefault( s => s.LibrarySectionId == collectionId && s.ResourceIntId == resourceId );
                    if ( libRes != null && libRes.Id > 0 )
                    {
                        entity.Id = libRes.Id;
                        entity.ResourceIntId = libRes.ResourceIntId;
                        entity.LibrarySectionId = libRes.LibrarySectionId;
                        entity.IsValid = ( bool ) libRes.IsActive;
                        entity.Created = libRes.Created;
                        entity.CreatedById = ( int ) libRes.CreatedById;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, string.Format( "LibraryResource_Get( resourceId: {0}, resourceId: {1})", collectionId, resourceId ) );
                entity.IsValid = false;
                entity.Message = ex.Message;
                return entity; ;
            }
            return entity;
        }

        public static bool IsResourceInLibrary( int libraryId, int resourceIntId )
        {
            using ( var context = new IsleContentContext() )
            {
                List<Library_SectionResourceSummary> items = context.Library_SectionResourceSummary
                                .Where( s => s.LibraryId == libraryId && s.ResourceIntId == resourceIntId )
                                .ToList();
                if ( items != null && items.Count > 0 )
                    return true;
                else 
                    return false;
            }
        }//

		/// <summary>
		/// Return first collection id associated with library resource
		/// </summary>
		/// <param name="libraryId"></param>
		/// <param name="resourceIntId"></param>
		/// <returns></returns>
		public static int GetCollectionForLibraryResource( int libraryId, int resourceIntId )
		{
			int collId = 0;
			using ( var context = new IsleContentContext() )
			{
				List<Library_SectionResourceSummary> items = context.Library_SectionResourceSummary
								.Where( s => s.LibraryId == libraryId && s.ResourceIntId == resourceIntId )
								.ToList();
				if ( items != null && items.Count > 0 ) {
					foreach ( Library_SectionResourceSummary item in items )
					{
						collId = item.LibrarySectionId;
						break;
					}
				}
			}

			return collId;
		}//
        public static bool IsResourceInMyLibrary( int userId, int resourceIntId )
        {
            //int libraryId, 
            using ( var context = new IsleContentContext() )
            {
                List<Library_SectionResourceSummary> items = context.Library_SectionResourceSummary
                                .Where( s => s.LibraryTypeId == ILP.Library.PERSONAL_LIBRARY_ID
                                    && s.LibraryCreatedById == userId
                                    && s.ResourceIntId == resourceIntId )
                                    .ToList();
                if ( items != null && items.Count > 0 )
                    return true;
                else
                    return false;
            }
        }//

        public static bool IsResourceInCollection( int collectionId, int resourceIntId )
        {
            using ( var context = new IsleContentContext() )
            {
                List<Library_SectionResourceSummary> items = context.Library_SectionResourceSummary
                                .Where( s => s.LibrarySectionId == collectionId && s.ResourceIntId == resourceIntId )
                                .ToList();
                if ( items != null && items.Count > 0 )
                    return true;
                else
                    return false;
            }
        }//

        public static LR_ResourceVersion_Summary ResourceSummary_Get( int resourceId )
        {
            LR_ResourceVersion_Summary lrSummary = new LR_ResourceVersion_Summary();
            using ( var context = new IsleContentContext() )
            {
                lrSummary = context.LR_ResourceVersion_Summary
                            .SingleOrDefault( s => s.Id == resourceId );
                //if ( lrSummary != null && lrSummary.Id > 0 )
                //{
                //    entity = Library_ToMap( lib );
                //}
            }
            return lrSummary;
        }

                /// <summary>
        /// Get unique tags for categoryId used in the passed library or collection
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="libraryId"></param>
        /// <param name="sectionId">Id of a collection</param>
        /// <returns></returns>
        public static List<TagFilterBase> LibraryCategoryTags_GetUsed( int categoryId, int libraryId )
        {
            return LibraryTags_GetUsed( categoryId, libraryId, 0 );
        }

        /// <summary>
        /// Get unique tags for categoryId used in the passed collection
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        public static List<TagFilterBase> CollectionCategoryTags_GetUsed( int categoryId, int collectionId )
        {
            return LibraryTags_GetUsed( categoryId, 0, collectionId );
        }

        /// <summary>
        /// Get unique tags for categoryId used in the passed library or collection
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="libraryId"></param>
        /// <param name="collectionId">Id of a collection</param>
        /// <returns></returns>
        private static List<TagFilterBase> LibraryTags_GetUsed( int categoryId, int libraryId, int collectionId )
        {

            List<TagFilterBase> list = new List<TagFilterBase>();
            TagFilterBase entity = new TagFilterBase();

            using ( var context = new IsleContentContext() )
            {
                using ( context )
                {
                    //get raw, which can have duplicated
                    var categories = context.Library_ResourceTagUniqueFilters
                                        .Where( s => s.CategoryId == categoryId
                                           && ( s.LibraryId == libraryId || s.SectionId == collectionId ) )
                                        .OrderBy( s => s.Title )
                                        .ToList();

                    //distinct doesn't work as the list includes lib, and section
                    //List<Library_ResourceTagUniqueFilters> categories = results.Distinct().ToList();
                     //.Where( s => s.CategoryId == categoryId
                     //   && ( s.LibraryId == libraryId || s.SectionId == sectionId ) )
                     //.OrderBy( s => s.Title ).Distinct()
                     //.ToList();
                    int prevTagId = 0;
                    if ( categories != null && categories.Count > 0 )
                    {
                        foreach ( Library_ResourceTagUniqueFilters tag in categories )
                        {
                            entity = new TagFilterBase();
                            //workaround for library dups - where distinct not working
                            if ( tag.Id != prevTagId )
                            {
                                prevTagId = tag.Id;

                                entity.id = tag.Id; //absolute id
                                entity.codeID = ( int ) tag.CodeId;
                                entity.title = tag.Title;

                                list.Add( entity );
                            }
                            else
                            {
                               //skip duplicate
                            }
                        }
                    }
                }
            }
            return list;
        }

        #endregion

        #region *** Library Member
        public int LibraryMember_Create( int libraryId, int userId, int memberTypeId, int createdById, ref string statusMessage )
        {

            Library_Member entity = new Library_Member();
            try
            {
                entity.LibraryId = libraryId;
                entity.UserId = userId;
                entity.MemberTypeId = memberTypeId;
                entity.CreatedById = createdById;
                entity.Created = System.DateTime.Now;
                entity.LastUpdatedById = createdById;
                entity.LastUpdated = System.DateTime.Now;
                entity.RowId = Guid.NewGuid();

                using ( var context = new IsleContentContext() )
                {
                    context.Library_Member.Add( entity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if ( count > 0 )
                    {
                        statusMessage = "Successful";
                        return entity.Id;
                    }
                    else
                    {
                        statusMessage = "Error - LibraryMember_Create failed";
                        //?no info on error
                        return 0;
                    }
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, string.Format( "LibraryMember_Create( libraryId: {0}, userId: {1})", libraryId, userId ) );
                statusMessage = ex.Message;
                return 0;
            }

        }
        public bool LibraryMember_Update( int id, int memberTypeId, int updatedById )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_Member entity = context.Library_Member.SingleOrDefault( s => s.Id == id );
                if ( entity != null )
                {
                    entity.MemberTypeId = memberTypeId;
                    entity.LastUpdatedById = updatedById;
                    entity.LastUpdated = System.DateTime.Now;
                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }
        public bool LibraryMember_Update( int libraryId, int userId, int memberTypeId, int updatedById )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_Member entity = context.Library_Member.SingleOrDefault( s => s.LibraryId == libraryId && s.UserId == userId );
                if ( entity != null )
                {
                    entity.MemberTypeId = memberTypeId;
                    entity.LastUpdatedById = updatedById;
                    entity.LastUpdated = System.DateTime.Now;
                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }
        public bool LibraryMember_Delete( int id )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_Member entity = context.Library_Member.SingleOrDefault( s => s.Id == id );
                if ( entity != null )
                {
                    context.Library_Member.Remove( entity );
                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }

        /// <summary>
        /// Get a library member, using Library_MemberSummary
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LibraryMember LibraryMember_Get( int id )
        {
            LibraryMember entity = new LibraryMember();
            using ( var context = new IsleContentContext() )
            {
                Library_MemberSummary mbr = context.Library_MemberSummary
                                .SingleOrDefault( s => s.Id == id );
                if ( mbr != null && mbr.Id > 0 )
                {
                    entity = LibraryMember_ToMap( mbr );
                }
            }
            return entity;
        }

        /// <summary>
        /// Get a library member, using Library_MemberSummary
        /// </summary>
        /// <param name="libraryId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static LibraryMember LibraryMember_Get( int libraryId, int userId )
        {
            LibraryMember entity = new LibraryMember();
            using ( var context = new IsleContentContext() )
            {
                Library_MemberSummary mbr = context.Library_MemberSummary
                        .SingleOrDefault( s => s.LibraryId == libraryId && s.UserId == userId );
                    //.Include( "Codes_LibraryMemberType" )
                if ( mbr != null && mbr.Id > 0 )
                {
                    entity = LibraryMember_ToMap( mbr );
                }
            }
            return entity;
        }

        /// <summary>
        /// Get approvers: editors or administrators
        /// NOTE: using Library_MemberSummary
        /// may want to arbrarily added the creator, as may not be in the list of LMbrs
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        public static List<LibraryMember> LibraryMembers_GetApprovers( int libraryId )
        {
            LibraryMember entity = new LibraryMember();
            List<LibraryMember> list = new List<LibraryMember>();
            ILP.Library lib = Library_Get( libraryId );

            using ( var context = new IsleContentContext() )
            {
                List<Library_MemberSummary> mbrs = context.Library_MemberSummary
                                    .Where( s => s.LibraryId == libraryId && s.MemberTypeId >= LibraryMember.LIBRARY_MEMBER_TYPE_ID_EDITOR )
                                    .ToList();
                    //.Include( "Codes_LibraryMemberType" )
            bool addCreator = true;

                
            if ( mbrs.Count > 0 )
            {
                foreach ( Library_MemberSummary item in mbrs )
                {
                    entity = new LibraryMember();
                    entity = LibraryMember_ToMap( item );
                    entity.Member = Person_Get( entity.UserId );

                    list.Add( entity );
                    if ( entity.UserId == lib.CreatedById )
                        addCreator = false;

                }
            }
            if ( addCreator == true )
            {
                LibraryMember mbr = new LibraryMember();
                mbr.UserId = lib.CreatedById;
                mbr.ParentId = libraryId;
                mbr.MemberTypeId = LibraryMember.LIBRARY_MEMBER_TYPE_ID_ADMIN;
                    mbr.SetMemberType();
                mbr.Created = mbr.LastUpdated = lib.Created;
                mbr.CreatedById = lib.CreatedById;
                mbr.LastUpdatedById = mbr.CreatedById;

                mbr.Member = Person_Get( mbr.UserId );

                list.Add( mbr );
            }
            }
            return list;
        }

        /// <summary>
        /// Get Type Members - all members of a library of the indicated type
        /// NOTE: using Library_MemberSummary
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        public static List<LibraryMember> LibraryMembers_GetTypeMembers( int libraryId, int memberTypeId )
        {
            LibraryMember entity = new LibraryMember();
            List<LibraryMember> list = new List<LibraryMember>();

            using ( var context = new IsleContentContext() )
            {
                List<Library_MemberSummary> mbrs = context.Library_MemberSummary
                                    .Where( s => s.LibraryId == libraryId && s.MemberTypeId == memberTypeId )
                                    .ToList();
                //.Include( "Codes_LibraryMemberType" )
                
                if ( mbrs.Count > 0 )
                {
                    foreach ( Library_MemberSummary item in mbrs )
                    {
                        entity = new LibraryMember();
                        entity = LibraryMember_ToMap( item );
                        entity.Member = Person_Get( entity.UserId );

                        list.Add( entity );
                    }
                }
            }
            return list;
        }


        /// <summary>
        /// get all members for the library, using Library_MemberSummary
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        public static List<LibraryMember> LibraryMembers_ForLibrary( int libraryId )
        {
            LibraryMember entity = new LibraryMember();
            List<LibraryMember> list = new List<LibraryMember>();

            using ( var context = new IsleContentContext() )
            {
                List<Library_MemberSummary> mbrs = context.Library_MemberSummary
                                    .Where( s => s.LibraryId == libraryId )
                                    .ToList();
                //                                    .Include( "Codes_LibraryMemberType" )

            if ( mbrs.Count > 0 )
            {
                foreach ( Library_MemberSummary item in mbrs )
                {
                    entity = new LibraryMember();
                    entity = LibraryMember_ToMap( item );
                    list.Add( entity );
                }
            }
            }
            return list;
        }

        public static Library_Member LibraryMember_FromMap( LibraryMember fromEntity )
        {
            //Mapper.CreateMap<LibraryMember, Library_Member>()
            //    .ForMember( dest => dest.LibraryId, opt => opt.MapFrom( src => src.ParentId ) );

            //Library_Member toEntity = Mapper.Map<LibraryMember, Library_Member>( fromEntity );

            Library_Member to = new Library_Member();
            to.Id = fromEntity.Id;
            to.LibraryId = fromEntity.ParentId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;
            to.RowId = ( Guid ) fromEntity.RowId;

            return to;
        }

        public static LibraryMember LibraryMember_ToMap( Library_Member fromEntity )
        {
            
            //Mapper.CreateMap<Library_Member, LibraryMember>()
            //    .ForMember( dest => dest.ParentId, opt => opt.MapFrom( src => src.LibraryId ) );
            //LibraryMember to = Mapper.Map<Library_Member, LibraryMember>( fromEntity );

            LibraryMember to = new LibraryMember();
            to.Id = fromEntity.Id;
            to.ParentId = fromEntity.LibraryId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;
            //do following if not properly geting related data
            if ( fromEntity.Codes_LibraryMemberType != null && fromEntity.Codes_LibraryMemberType.Id > 0 )
                to.MemberType = fromEntity.Codes_LibraryMemberType.Title;
            else 
            to.SetMemberType();

            to.Created = fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;
            to.RowId = ( Guid ) fromEntity.RowId;

            return to;
        }
        public static LibraryMember LibraryMember_ToMap( Library_MemberSummary fromEntity )
        {

            LibraryMember to = new LibraryMember();
            to.Id = fromEntity.Id;
            to.ParentId = fromEntity.LibraryId;
            to.Library = fromEntity.Library;
            to.UserId = fromEntity.UserId;

            to.MemberName = fromEntity.FullName;
            to.MemberSortName = fromEntity.SortName;
            to.MemberEmail = fromEntity.Email;
            to.MemberTypeId = fromEntity.MemberTypeId;
            to.MemberType = fromEntity.MemberType;
            to.IsAnOrgMbr = fromEntity.IsAnOrgMbr == 1;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            return to;
        }

        #endregion

        #region     Library Invitation
        public int LibraryInvitationCreate( LibraryInvitation entity, ref string statusMessage )
        {
            using ( var context = new IsleContentContext() )
            {
                Library_Invitation ef = new Library_Invitation();
                LibraryInvitation_FromMap( entity, ef );

                context.Library_Invitation.Add( ef );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    statusMessage = "Successful";
                    return ef.Id;
                }
                else
                {
                    statusMessage = "Error - LibraryInvitation_Create failed";
                    //?no info on error
                    return 0;
                }
            }
        }
        public bool LibraryInvitation_Update( LibraryInvitation entity )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_Invitation toEntity = context.Library_Invitation.SingleOrDefault( s => s.Id == entity.Id );
                if ( toEntity != null && toEntity.Id > 0 )
                {
                    //hmmm - NOTE cannot use the mapping, as must be the entity retrieved from ef in the save
                   // ef = LibraryInvitation_FromMap( entity );
                    //jsut update likely suspects
                    toEntity.IsActive = entity.IsActive;
                    toEntity.ResponseDate = entity.ResponseDate;

                    toEntity.LastUpdatedById = entity.LastUpdatedById;
                    toEntity.LastUpdated = System.DateTime.Now;

                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }

        public bool LibraryInvitation_Delete( int id )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_Invitation entity = context.Library_Invitation.SingleOrDefault( s => s.Id == id );
                if ( entity != null )
                {
                    context.Library_Invitation.Remove( entity );
                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }

        public static LibraryInvitation LibraryInvitation_Get( int id )
        {
            LibraryInvitation entity = new LibraryInvitation();
            using ( var context = new IsleContentContext() )
            {
                Library_Invitation mbr = context.Library_Invitation.SingleOrDefault( s => s.Id == id );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }
            }
            return entity;
        }

        public static LibraryInvitation LibraryInvitation_Get( int libraryId, int userId )
        {
            LibraryInvitation entity = new LibraryInvitation();
            using ( var context = new IsleContentContext() )
            {
                Library_Invitation mbr = context.Library_Invitation.SingleOrDefault( s => s.LibraryId == libraryId && s.TargetUserId == userId );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }
            }
            return entity;
        }
        public static LibraryInvitation LibraryInvitation_Get( Guid rowId )
        {
            LibraryInvitation entity = new LibraryInvitation();
            using ( var context = new IsleContentContext() )
            {
                Library_Invitation mbr = context.Library_Invitation.SingleOrDefault( s => s.RowId == rowId );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }
            }
            return entity;
        }
        public static LibraryInvitation LibraryInvitation_GetByPasscode( string passcode )
        {
            LibraryInvitation entity = new LibraryInvitation();
            using ( var context = new IsleContentContext() )
            {
                Library_Invitation mbr = context.Library_Invitation.SingleOrDefault( s => s.PassCode == passcode );
            if ( entity != null && entity.Id > 0 )
            {
                entity = LibraryInvitation_ToMap( mbr );
            }
            }
            return entity;
        }
        public static void LibraryInvitation_FromMap( LibraryInvitation fromEntity, Library_Invitation toEntity )
        {
            //Mapper.CreateMap<LibraryInvitation, Library_Invitation>();
            //Library_Invitation toEntity = Mapper.Map<LibraryInvitation, Library_Invitation>( fromEntity );

            //Library_Invitation toEntity = new Library_Invitation();
            toEntity.Id = fromEntity.Id;
            toEntity.LibraryId = fromEntity.ParentId;
            toEntity.LibMemberTypeId = ( int ) fromEntity.LibMemberTypeId;
            toEntity.InvitationType = fromEntity.InvitationType;
            toEntity.PassCode = fromEntity.PassCode;

            toEntity.TargetEmail = fromEntity.TargetEmail;
            toEntity.AddToOrgId = fromEntity.AddToOrgId;
            toEntity.AddAsOrgMemberTypeId = fromEntity.AddAsOrgMemberTypeId;
            toEntity.OrgMbrRoles = fromEntity.OrgMbrRoles;

            toEntity.StartingUrl = fromEntity.StartingUrl;
            toEntity.IsActive = fromEntity.IsActive;
            toEntity.TargetUserId = fromEntity.TargetUserId;
            toEntity.Subject = fromEntity.Subject;
            toEntity.ExpiryDate = fromEntity.ExpiryDate;
            if ( fromEntity.IsValidRowId( fromEntity.RowId ) )
                toEntity.RowId = fromEntity.RowId;
            else
                toEntity.RowId = Guid.NewGuid();

            toEntity.CreatedById = fromEntity.CreatedById;
            toEntity.Created = fromEntity.Created;
            toEntity.LastUpdated = fromEntity.LastUpdated;
            toEntity.LastUpdatedById = fromEntity.LastUpdatedById;

            //return toEntity;
        }

        public static LibraryInvitation LibraryInvitation_ToMap( Library_Invitation fromEntity )
        {

            //Mapper.CreateMap<Library_Invitation, LibraryInvitation>();
            //LibraryInvitation toEntity = Mapper.Map<Library_Invitation, LibraryInvitation>( fromEntity );

            LibraryInvitation toEntity = new LibraryInvitation();
            toEntity.Id = fromEntity.Id;
            toEntity.ParentId = ( int ) fromEntity.LibraryId;
            toEntity.LibMemberTypeId = ( int ) fromEntity.LibMemberTypeId;
            toEntity.InvitationType = toEntity.InvitationType;
            toEntity.TargetEmail = fromEntity.TargetEmail;
            toEntity.AddToOrgId = ( int ) fromEntity.AddToOrgId;
            toEntity.AddAsOrgMemberTypeId = ( int ) fromEntity.AddAsOrgMemberTypeId;
            toEntity.OrgMbrRoles = fromEntity.OrgMbrRoles;
            toEntity.PassCode = fromEntity.PassCode;

            toEntity.StartingUrl = fromEntity.StartingUrl;
            toEntity.IsActive = ( bool ) fromEntity.IsActive;
            toEntity.TargetUserId = ( int ) fromEntity.TargetUserId;
            toEntity.Subject = fromEntity.Subject;
            toEntity.ExpiryDate = ( System.DateTime ) fromEntity.ExpiryDate;
            if ( toEntity.IsValidRowId( fromEntity.RowId ) )
                toEntity.RowId = fromEntity.RowId;
            else
                toEntity.RowId = Guid.NewGuid();

            toEntity.CreatedById = fromEntity.CreatedById;
            toEntity.Created = ( System.DateTime ) fromEntity.Created;
            toEntity.LastUpdated = ( System.DateTime ) fromEntity.LastUpdated;
            toEntity.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;

            return toEntity;
        }

        #endregion

        #region     Library SectionMember
        public int LibrarySectionMember_Create( int parentId, int userId, int memberTypeId, int createdById, ref string statusMessage )
        {

            Library_SectionMember entity = new Library_SectionMember();
            using ( var context = new IsleContentContext() )
            {
                entity.LibrarySectionId = parentId;
                entity.UserId = userId;
                entity.MemberTypeId = memberTypeId;
                entity.CreatedById = createdById;
                entity.Created = System.DateTime.Now;
                entity.LastUpdatedById = createdById;
                entity.LastUpdated = System.DateTime.Now;
                entity.RowId = Guid.NewGuid();

                context.Library_SectionMember.Add( entity );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return entity.Id;
                }
                else
                {
                    statusMessage = "Error - LibrarySectionMember_Create failed";
                    //?no info on error
                    return 0;
                }
            }
        }
        public bool LibrarySectionMember_Update( int id, int memberTypeId, int updatedById )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_SectionMember entity = context.Library_SectionMember.SingleOrDefault( s => s.Id == id );
                if ( entity != null )
                {
                    entity.MemberTypeId = memberTypeId;
                    entity.LastUpdatedById = updatedById;
                    entity.LastUpdated = System.DateTime.Now;
                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }
        public bool LibrarySectionMember_Update( int parentId, int userId, int memberTypeId, int updatedById )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_SectionMember entity = context.Library_SectionMember.SingleOrDefault( s => s.LibrarySectionId == parentId && s.UserId == userId );
                if ( entity != null )
                {
                    entity.MemberTypeId = memberTypeId;
                    entity.LastUpdatedById = updatedById;
                    entity.LastUpdated = System.DateTime.Now;
                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }
        public bool LibrarySectionMember_Delete( int id )
        {
            bool action = false;
            using ( var context = new IsleContentContext() )
            {
                Library_SectionMember entity = context.Library_SectionMember.SingleOrDefault( s => s.Id == id );
                if ( entity != null )
                {
                    context.Library_SectionMember.Remove( entity );
                    context.SaveChanges();

                    action = true;
                }
            }
            return action;
        }

        public static LibraryMember LibrarySectionMember_Get( int id )
        {
            LibraryMember entity = new LibraryMember();
            using ( var context = new IsleContentContext() )
            {
                EF_LSM mbr = context.Library_SectionMember.SingleOrDefault( s => s.Id == id );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibrarySectionMember_ToMap( mbr );
            }
            }
            return entity;
        }

        public static LibraryMember LibrarySectionMember_Get( int libraryId, int userId )
        {
            // Load one blogs and its related posts 
            //var blog1 = context.Library_SectionMembers
            //                    .Where( b => b.UserId == userId )
            //                    .Include( b => b. )
            //                    .FirstOrDefault(); 

            LibraryMember entity = new LibraryMember();
            using ( var context = new IsleContentContext() )
            {
                Library_SectionMember mbr = context.Library_SectionMember
                            .Include( "Codes_LibraryMemberType" )
                        .Where( s => s.LibrarySectionId == libraryId && s.UserId == userId )
                        .SingleOrDefault( s => s.LibrarySectionId == libraryId && s.UserId == userId );
            if ( mbr != null && mbr.Id > 0 )
            {
                entity = LibrarySectionMember_ToMap( mbr );
            }
            }
            return entity;
        }
        public static Library_SectionMember LibrarySectionMember_FromMap( LibraryMember fromEntity )
        {
            //Mapper.CreateMap<LibraryMember, Library_SectionMember>()
            //    .ForMember(dest => dest.LibrarySectionId, opt => opt.MapFrom(src => src.ParentId));

            //Library_SectionMember toEntity = Mapper.Map<LibraryMember, Library_SectionMember>( fromEntity );

            Library_SectionMember to = new Library_SectionMember();
            to.Id = fromEntity.Id;
            to.LibrarySectionId = fromEntity.ParentId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;

            to.Created = fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;
            to.RowId = ( Guid ) fromEntity.RowId;
            return to;
        }

        public static LibraryMember LibrarySectionMember_ToMap( Library_SectionMember fromEntity )
        {

            //Mapper.CreateMap<Library_SectionMember, LibraryMember>()
            //    .ForMember( dest => dest.ParentId, opt => opt.MapFrom( src => src.LibrarySectionId ) );

            //LibraryMember toEntity = Mapper.Map<Library_SectionMember, LibraryMember>( fromEntity );

            LibraryMember to = new LibraryMember();
            to.Id = fromEntity.Id;
            to.ParentId = fromEntity.LibrarySectionId;
            to.UserId = fromEntity.UserId;
            to.MemberTypeId = fromEntity.MemberTypeId;
            //do following if not properly geting related data
            if ( fromEntity.Codes_LibraryMemberType != null && fromEntity.Codes_LibraryMemberType.Id > 0 )
                to.MemberType = fromEntity.Codes_LibraryMemberType.Title;
            else
                to.SetMemberType();

            to.Created = fromEntity.Created;
            to.CreatedById = ( int ) fromEntity.CreatedById;
            to.LastUpdated = fromEntity.LastUpdated;
            to.LastUpdatedById = ( int ) fromEntity.LastUpdatedById;
            to.RowId = ( Guid ) fromEntity.RowId;
            return to;
        }

        #endregion

        #region     Library Following
        /// <summary>
        /// get list of library subscriptions for user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<ObjectSubscription> LibraryFollowing_ForUser( int userId )
        {
            ObjectSubscription entity = new ObjectSubscription();
            List<ObjectSubscription> list = new List<ObjectSubscription>();

            using ( var context = new IsleContentContext() )
            {
                List<Library_Subscription> mbrs = context.Library_Subscription
                                .Where( s => s.UserId == userId )
                                .ToList();

            if ( mbrs.Count > 0 )
            {
                foreach ( Library_Subscription item in mbrs )
                {
                    entity = new ObjectSubscription();
                    entity.Id = item.Id;
                    entity.ParentId = item.LibraryId;
                    entity.UserId = item.UserId;
                        entity.SubscriptionTypeId = ( int ) item.SubscriptionTypeId;

                        entity.Created = ( System.DateTime ) item.Created;
                        entity.LastUpdated = ( System.DateTime ) item.LastUpdated;

                    //entity = LibraryMember_ToMap( item );
                    list.Add( entity );

                }
            }
            }

            return list;
        }

        /// <summary>
        /// get list of collection subscriptions for user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static List<ObjectSubscription> CollectionFollowing_ForUser( int userId )
        {
            ObjectSubscription entity = new ObjectSubscription();
            List<ObjectSubscription> list = new List<ObjectSubscription>();

            using ( var context = new IsleContentContext() )
            {
                List<Library_SectionSubscription> mbrs = context.Library_SectionSubscription
                                .Where( s => s.UserId == userId )
                                .ToList();

            if ( mbrs.Count > 0 )
            {
                foreach ( Library_SectionSubscription item in mbrs )
                {
                    entity = new ObjectSubscription();
                    entity.Id = item.Id;
                    entity.ParentId = item.SectionId;
                    entity.UserId = item.UserId;
                        entity.SubscriptionTypeId = ( int )item.SubscriptionTypeId;

                        entity.Created = ( System.DateTime )item.Created;
                    entity.LastUpdated = item.LastUpdated == null 
                                       ? ( System.DateTime )entity.DefaultDate
                                       : ( System.DateTime )item.LastUpdated;
                    list.Add( entity );

                }
            }
            }
            return list;
        }
        #endregion

        public int LibraryCommentCreate( int libraryId, string comment, int createdById )
        {
            using ( var context = new IsleContentContext() )
            {
                Library_Comment c = new Library_Comment();
                c.Comment = comment;
                c.CreatedById = createdById;
                c.Created = System.DateTime.Now;
                c.LibraryId = libraryId;

                context.Library_Comment.Add( c );

                // submit the change to database
                int count = context.SaveChanges();
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
        }

        #region == person, org
        public static AppUser Person_Get( int userId )
        {
            AppUser entity = new AppUser();
            using ( var context = new IsleContentContext() )
            {
                LR_PatronOrgSummary user = context.LR_PatronOrgSummary.SingleOrDefault( s => s.UserId == userId );
            if ( user != null && user.UserId > 0 )
            {
                entity.Id = user.UserId;
                entity.UserName = user.UserName;
                entity.FirstName = user.FirstName;
                entity.LastName = user.LastName;
                entity.Email = user.Email;
                entity.OrganizationName = user.Organization;
                    entity.OrgId = user.OrganizationId == null ? 0 : ( int ) user.OrganizationId;
                entity.UserProfile.ImageUrl = user.ImageUrl;
            }
            }
            return entity;
        }

        #endregion
    }
}
