using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using AutoMapper;
using LB = LRWarehouse.Business;
using TagValue1 = LRWarehouse.Business.CodesTagValue;
using TagValue = IOERBusinessEntities.Codes_TagValue;
using ILPathways.Common;
using ILPathways.Utilities;
using Isle.DTO;
using LRWarehouse.DAL;

namespace IOERBusinessEntities
{
    public class EFResourceManager
    {
        static string thisClassName = "EFResourceManager"; 
        
        DateTime DefaultDate = new System.DateTime( 1970, 1, 1 );



		#region == core methods
		public static LB.Resource Resource_GetSimple( int resourceId )
		{
			LB.Resource entity = new LB.Resource();

			using ( var context = new ResourceContext() )
			{
				Resource efEntity = context.Resources
							.Include( "Resource_Version" )
							.SingleOrDefault( s => s.Id == resourceId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					entity.Id = efEntity.Id;
					entity.ResourceUrl = efEntity.ResourceUrl;
					entity.ImageUrl = efEntity.ImageUrl;

					entity.ViewCount = efEntity.ViewCount != null ? ( int ) efEntity.ViewCount : 0;
					entity.FavoriteCount = efEntity.FavoriteCount != null ? ( int ) efEntity.FavoriteCount : 0;
					entity.Created = ( DateTime ) efEntity.Created;

					if ( efEntity.Resource_Version != null && efEntity.Resource_Version.Count > 0 )
					{
						//just get first active one
						foreach ( Resource_Version rv in efEntity.Resource_Version )
						{
							if ( rv.IsActive == true )
							{
								entity.Version = ResourceVersion_ToMap( rv );
								break;
							}
						}
					}

				}
			}
			return entity;
		}//

      
		/// <summary>
		/// Retrieve resource summary for display - not a complete resource
		/// </summary>
		/// <param name="resourceId"></param>
		/// <returns></returns>
		public static LB.Resource Resource_GetSummary( int resourceId )
		{
			LB.Resource entity = new LB.Resource();

			using ( var context = new ResourceContext() )
			{
				Resource efEntity = context.Resources
							.Include( "Resource_Version" )
							.Include( "Resource_Tag" )
							.Include( "Resource_Cluster" )
							.Include( "Resource_GradeLevel" )
							.Include( "Resource_Keyword" )
							.Include( "Resource_Subject" )
							.SingleOrDefault( s => s.Id == resourceId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					entity = ResourceSummary_ToMap( efEntity, true );
					//add keywords, subjects, grade level
				}
			}
			return entity;
		}//

		private static LB.Resource ResourceSummary_ToMap( Resource fromEntity, bool doingEagerLoad )
		{
			LB.Resource to = new LB.Resource();

			try
			{
				to.IsValid = true;
				to.Id = fromEntity.Id;
				to.RowId = fromEntity.RowId;

				to.ResourceUrl = fromEntity.ResourceUrl != null ? fromEntity.ResourceUrl : "";
				to.ImageUrl = fromEntity.ImageUrl != null ? fromEntity.ImageUrl : "";
				to.FavoriteCount = fromEntity.FavoriteCount != null ? ( int ) fromEntity.FavoriteCount : 10;
				to.ViewCount = fromEntity.ViewCount != null ? ( int ) fromEntity.ViewCount : 0;

				to.IsActive = ( bool ) fromEntity.IsActive;
				//to.HasPathwayGradeLevel = fromEntity.HasPathwayGradeLevel != null ? ( bool ) fromEntity.HasPathwayGradeLevel : false;

				to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
				to.LastUpdated = fromEntity.LastUpdated != null ? ( System.DateTime ) fromEntity.LastUpdated : to.DefaultDate;
				if ( doingEagerLoad == false )
					return to;

				if ( fromEntity.Resource_Version != null && fromEntity.Resource_Version.Count > 0 )
				{
					//just get first active one
					foreach ( Resource_Version rv in fromEntity.Resource_Version )
					{
						if ( rv.IsActive == true )
						{
							to.Version = ResourceVersion_ToMap( rv );
							break;
						}
					}
				}

				if ( fromEntity.Resource_Cluster != null && fromEntity.Resource_Cluster.Count > 0 )
				{
					to.ClusterMap = new List<LB.ResourceChildItem>();

					foreach ( Resource_Cluster rc in fromEntity.Resource_Cluster )
					{
						to.ClusterMap.Add( Resource_Cluster_ToMap( rc ) );
					}
				}
				if ( fromEntity.Resource_GradeLevel != null && fromEntity.Resource_GradeLevel.Count > 0 )
				{
					to.Gradelevel = new List<LB.ResourceChildItem>();

					foreach ( Resource_GradeLevel gl in fromEntity.Resource_GradeLevel )
					{
						to.Gradelevel.Add( Resource_GradeLevel_ToMap( gl ) );
					}
				}

				if ( fromEntity.Resource_Subject != null && fromEntity.Resource_Subject.Count > 0 )
				{
					to.SubjectMap = new List<LB.ResourceChildItem>();

					foreach ( Resource_Subject rs in fromEntity.Resource_Subject )
					{
						to.SubjectMap.Add( Resource_Subject_ToMap( rs ) );
					}
				}


				if ( fromEntity.Resource_Keyword != null && fromEntity.Resource_Keyword.Count > 0 )
				{
					to.Keyword = new List<LB.ResourceChildItem>();

					foreach ( Resource_Keyword rk in fromEntity.Resource_Keyword )
					{
						to.Keyword.Add( Resource_Keyword_ToMap( rk ) );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Resource_ToMap( Resource fromEntity )" );
			}
			return to;
		}//
		private static LB.ResourceVersion ResourceVersion_ToMap( Resource_Version fromEntity )
		{
			LB.ResourceVersion to = new LB.ResourceVersion();

			Mapper.CreateMap<Resource_Version, LB.ResourceVersion>()
				  .ForMember( d => d.LRDocId, o => o.Ignore() )
				  .ForMember( d => d.ResourceUrl, o => o.Ignore() )
				  .ForMember( d => d.Subjects, o => o.Ignore() )
				  .ForMember( d => d.EducationLevels, o => o.Ignore() )
				  .ForMember( d => d.Keywords, o => o.Ignore() )
				  .ForMember( d => d.LanguageList, o => o.Ignore() )
				  .ForMember( d => d.AudienceList, o => o.Ignore() )
				  .ForMember( d => d.ResourceTypesList, o => o.Ignore() )
				  .ForMember( d => d.HasChanged, o => o.Ignore() )
				  .ForMember( d => d.IsValid, o => o.Ignore() )
				  .ForMember( d => d.LastUpdatedById, o => o.Ignore() )
				  .ForMember( d => d.LastUpdatedBy, o => o.Ignore() )
				  .ForMember( d => d.LastUpdated, o => o.Ignore() )
				  .ForMember( d => d.CreatedById, o => o.Ignore() )
				  .ForMember( d => d.CreatedBy, o => o.Ignore() )
				  .ForMember( d => d.Message, o => o.Ignore() )
				  .ForMember( d => d.CanEdit, o => o.Ignore() )
				  .ForMember( d => d.CanView, o => o.Ignore() )
				  .ForMember( d => d.TempProperty1, o => o.Ignore() )
				  .ForMember( d => d.TempProperty2, o => o.Ignore() )
				  .ForMember( d => d.ChangeLog, o => o.Ignore() )
				  .ForMember( d => d.DEFAULT_GUID, o => o.Ignore() )
				  .ForMember( d => d.DefaultDate, o => o.Ignore() );

			Mapper.AssertConfigurationIsValid();

			to = Mapper.Map<Resource_Version, LB.ResourceVersion>( fromEntity );

			if ( to.ResourceUrl == null )
			{
				if ( fromEntity.Resource != null && fromEntity.Resource.Id > 0 )
				{
					to.ResourceUrl = fromEntity.Resource.ResourceUrl;
					to.ResourceImageUrl = fromEntity.Resource.ImageUrl;
				}
				else
				{
					LB.Resource entity = Resource_GetSimple( to.ResourceIntId );
					to.ResourceUrl = entity.ResourceUrl;
					to.ResourceImageUrl = fromEntity.Resource.ImageUrl;
				}
			}

			return to;
		}

		private static LB.ResourceChildItem Resource_Cluster_ToMap( Resource_Cluster fromEntity )
		{
			LB.ResourceChildItem to = new LB.ResourceChildItem();

			to.Id = fromEntity.Id;
			to.ResourceIntId = fromEntity.ResourceIntId;
			to.CodeId = fromEntity.ClusterId;
			to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;

			if ( fromEntity.Codes_CareerCluster != null && fromEntity.Codes_CareerCluster.Title != null )
				to.MappedValue = fromEntity.Codes_CareerCluster.Title;
			else
			{
				to.MappedValue = GetCluster( to.CodeId );
			}
			return to;
		}

		private static string GetCluster( int codeId )
		{
			string title = "";
			using ( var context = new ResourceContext() )
			{
				Codes_CareerCluster efEntity = context.Codes_CareerCluster
							.SingleOrDefault( s => s.Id == codeId );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					title = efEntity.Title;
				}

			}
			return title;
		}
		private static LB.ResourceChildItem Resource_GradeLevel_ToMap( Resource_GradeLevel fromEntity )
		{
			LB.ResourceChildItem to = new LB.ResourceChildItem();

			to.Id = fromEntity.Id;
			to.ResourceIntId = fromEntity.ResourceIntId == null ? 0 : ( int ) fromEntity.ResourceIntId;
			to.CodeId = fromEntity.GradeLevelId == null ? 0 : ( int ) fromEntity.GradeLevelId;
			to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;

			if ( fromEntity.Codes_GradeLevel != null && fromEntity.Codes_GradeLevel.Title != null )
				to.MappedValue = fromEntity.Codes_GradeLevel.Title;
			else
			{
				to.MappedValue = GetGradeLevel( to.CodeId );
			}

			return to;
		}

		private static string GetGradeLevel( int codeId )
		{
			string title = "";
			using ( var context = new ResourceContext() )
			{
				Codes_GradeLevel efEntity = context.Codes_GradeLevel
							.SingleOrDefault( s => s.Id == codeId );
				if ( efEntity != null && efEntity.Id > 0 )
				{
					title = efEntity.Title;
				}

			}
			return title;
		}

		private static LB.ResourceChildItem Resource_Subject_ToMap( Resource_Subject fromEntity )
		{
			LB.ResourceChildItem to = new LB.ResourceChildItem();

			to.Id = fromEntity.Id;
			to.ResourceIntId = fromEntity.ResourceIntId == null ? 0 : ( int ) fromEntity.ResourceIntId;
			to.CodeId = fromEntity.CodeId == null ? 0 : ( int ) fromEntity.CodeId;
			to.MappedValue = fromEntity.Subject;
			to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;

			if ( ( to.MappedValue == null || to.MappedValue == "" ) &&
				( fromEntity.Codes_Subject != null && fromEntity.Codes_Subject.Title != null ) )
				to.MappedValue = fromEntity.Codes_Subject.Title;

			return to;
		}

		private static LB.ResourceChildItem Resource_Keyword_ToMap( Resource_Keyword fromEntity )
		{
			LB.ResourceChildItem to = new LB.ResourceChildItem();

			to.Id = fromEntity.Id;
			to.ResourceIntId = fromEntity.ResourceIntId == null ? 0 : ( int ) fromEntity.ResourceIntId;
			to.MappedValue = fromEntity.Keyword;
			to.Created = fromEntity.Created != null ? ( System.DateTime ) fromEntity.Created : to.DefaultDate;
			to.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;

			return to;
		}

		/// <summary>
		/// Update image url for resource
		/// </summary>
		/// <param name="resourceId"></param>
		/// <param name="imageUrl"></param>
		/// <returns>-1 - error; 0 - images aleady match; >=1 - update made</returns>
		public int Resource_UpdateImageUrl( int resourceId, string imageUrl )
		{

			using ( var context = new ResourceContext() )
			{
				Resource e = context.Resources
					.SingleOrDefault( s => s.Id == resourceId );
				if ( e.ImageUrl != null 
					&& e.ImageUrl.ToLower() == imageUrl.ToLower() )
				{
					return 0;
				}
				e.ImageUrl = imageUrl;
				e.LastUpdated = System.DateTime.Now;

				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					return e.Id;
				}
				else
				{
					//?no info on error
					return -1;
				}
			}
		}
       
        #endregion

        #region === dashboard methods
        /// <summary>
        /// Fill dashboard with resources published by this user
        /// </summary>
        /// <param name="dashboard"></param>
        public static void Resource_FillDashboard( DashboardDTO dashboard  )
        {
            //just in case...
            if ( dashboard.maxResources == 0 )
                dashboard.maxResources = 10;

			string imgUrl = ContentHelper.GetAppKeyValue( "cachedImagesUrl", "/OERThumbs/" );

            DashboardResourceDTO entity = new DashboardResourceDTO();

            using ( var context = new ResourceContext() )
            {
                dashboard.myResources = new ResourcesBox();
                dashboard.myResources.name = "Resources I Created or Tagged";
                dashboard.myResources.resources = new List<DashboardResourceDTO>();

//                dashboard.myResources.resources = MyRecentResources( context, dashboard.myResources, dashboard.userId, dashboard.maxResources );


                int totalResources = context.Resource_Version_Summary
                              .Where( s => s.PublishedById == dashboard.userId )
                              .Count();

                if ( totalResources > 0 )
                {
                    dashboard.myResources.total = totalResources;

                    List<Resource_Version_Summary> items = context.Resource_Version_Summary
                                   .Where( s => s.PublishedById == dashboard.userId )
                                   .OrderByDescending( s => s.Imported )
                                   .Take( dashboard.maxResources ).ToList();

                    if ( items.Count > 0 )
                    {
                        foreach ( Resource_Version_Summary item in items )
                        {
                            entity = new DashboardResourceDTO();
                            entity.id = item.ResourceIntId;
                            entity.title = item.Title;
                            entity.containerTitle = "Resources I Published";
							if ( item.Imported != null)
								entity.DateAdded = ( DateTime )item.Imported;
							else if ( item.Created != null )
								entity.DateAdded = ( DateTime ) item.Created;
							//
                            entity.url = string.Format( "/Resource/{0}/{1}", item.ResourceIntId, LB.ResourceVersion.UrlFriendlyTitle( item.Title ) );
							if ( string.IsNullOrEmpty( item.ImageUrl ) == false )
								entity.imageUrl = item.ImageUrl;
							else
								entity.imageUrl = imgUrl + string.Format( "large/{0}-large.png", entity.id );

                            dashboard.myResources.resources.Add( entity );
                        }
                    }
                }
            }
            //return entity;
        }
        
        #endregion
       

        #region === resource access ===

        /// <summary>
        /// Determine if user is the resource author 
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ObjectMember GetResourceAccess( int resourceId, int userId ) 
        {
            ObjectMember mbr = new ObjectMember();
            Patron_ResourceSummary res = new Patron_ResourceSummary();

            LB.Resource entity = new LB.Resource();

            using ( var context = new ResourceContext() )
            {
                Patron_ResourceSummary efEntity = context.Patron_ResourceSummary
                            .SingleOrDefault( s => (s.ResourceId == resourceId && s.UserId == userId));

                if ( efEntity != null && efEntity.ResourceId > 0 )
                {
                    mbr.FirstName = efEntity.FirstName;
                    mbr.LastName = efEntity.LastName;
                    mbr.Email = efEntity.Email;

                    mbr.OrgId = efEntity.OrganizationId != null ? ( int ) efEntity.OrganizationId : 0;
                    mbr.Organization = efEntity.Organization;

                    mbr.MemberTypeId = 4;
                    mbr.MemberType = "Administrator";
                    mbr.Created = ( DateTime ) efEntity.Published;
                    mbr.MemberImageUrl = efEntity.ImageUrl;

                }
                else
                {
                    //check org level access
                    //where res was published for an org
                    //and current user has an appropriate content role for the org
                }

            }

            return mbr;
        }

        /// <summary>
        /// NOTE - not ready for use
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="userId"></param>
        /// <param name="orgId"></param>
        /// <returns></returns>
        private static ObjectMember IsUserResourceAuthor( int resourceId, int userId, int orgId )
        {
            ObjectMember mbr = new ObjectMember();
            Patron_ResourceSummary res = new Patron_ResourceSummary();

            LB.Resource entity = new LB.Resource();

            using ( var context = new ResourceContext() )
            {
                Patron_ResourceSummary efEntity = context.Patron_ResourceSummary
                            .SingleOrDefault( s => ( s.ResourceId == resourceId && s.UserId == userId )
                                                 || s.ResourceId == resourceId && s.OrganizationId == orgId );

                if ( efEntity != null && efEntity.ResourceId > 0 )
                {
                    mbr.ObjectId = efEntity.ResourceId;
                    //actual author
                    mbr.UserId = efEntity.UserId;
                    mbr.FirstName = efEntity.FirstName;
                    mbr.LastName = efEntity.LastName;
                    mbr.Email = efEntity.Email;

                    mbr.OrgId = efEntity.OrganizationId != null ? ( int ) efEntity.OrganizationId : 0;
                    mbr.Organization = efEntity.Organization;

                    mbr.MemberTypeId = 4;
                    mbr.MemberType = "Administrator";
                    mbr.Created = ( DateTime ) efEntity.Published;
                    mbr.MemberImageUrl = efEntity.ImageUrl;

                }

            }

            return mbr;
        }
        #endregion
        #region === resource tags
		public void Resource_CreateTags( List<int> tags, int resourceID, int createdById )
		{
			LoggingHelper.DoTrace( 4, "+++++EFResourceManager.Resource_CreateTags. resourceID: " + resourceID.ToString() );
			using ( var context = new ResourceContext() )
			{
				if ( tags != null && tags.Count() > 0 )
				{
					ResourceTag_Create( context, tags, resourceID, createdById );
				}
			}
		}
        public int ResourceTag_Create( ResourceContext context, List<int> tags, int resourceID, int createdById )
        {
            int createCount = 0;
			LoggingHelper.DoTrace( 4, string.Format( "_____EFResourceManager.ResourceTag_Create. entry rId: {0}", resourceID ) );

			foreach ( int tagId in tags )
            {
				try
				{
					LoggingHelper.DoTrace( 6, "			ResourceTag_Create. tagId: " + tagId.ToString() );
					//check if exists
					Resource_Tag tag = new Resource_Tag();

					//15-11-20 mp - Changed from SingleOrDefault. The latter makes more sense but can result in an error (Sequence contains more than one element). Will check if dups are being added 
					Resource_Tag efEntity = context.Resource_Tag
						   .FirstOrDefault( s => s.ResourceIntId == resourceID && s.TagValueId == tagId );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						//skip
						LoggingHelper.DoTrace( 6, "		ResourceTag_Create. Skip existing tagId: " + tagId.ToString() );
						continue;
					}

					tag.ResourceIntId = resourceID;
					tag.TagValueId = tagId;
					tag.OriginalValue = "";
					if ( createdById > 0 )
						tag.CreatedById = createdById;
					tag.Created = System.DateTime.Now;

					context.Resource_Tag.Add( tag );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						SyncToLegacyCodeTable( context, tag );
						createCount++;
						//return tag.Id;
					}
					else
					{
						//?no info on error
						LoggingHelper.LogError( thisClassName + ".ResourceTag_Create()", true );
					}
				}
				catch ( Exception ex )
				{
					//catch here to allow continuing
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".ResourceTag_Create(). rId: {0}, tagId: {1}", resourceID, tagId.ToString() ), true );
				}

            }
			LoggingHelper.DoTrace( 4, "_____EFResourceManager.ResourceTag_Create. exit " );
            return createCount;
        }


        /// <summary>
        /// Sync updates to old resource tables based on new Resource.Tag values
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tag"></param>
        public static void SyncToLegacyCodeTable( ResourceContext context, Resource_Tag tag ) 
        {
			try 
			{
				TagValue tv = EFCodesManager.Codes_TagValue_Get( context, tag.TagValueId );
				//TagValue tv = CodeTableManager.CodesTagValue_Get( tag.TagValueId );


				LoggingHelper.DoTrace(6, "EFResourceManager.SyncToLegacyCodeTable. categoryId: " + tv.CategoryId.ToString());
				if ( tv.CategoryId == LB.CodesSiteTagCategory.AUDIENCE_TYPE_CATEGORY_Id )
				{
					Resource_IntendedAudience_Create( context, tag, tv );
				}
				else if ( tv.CategoryId == LB.CodesSiteTagCategory.ACCESSIBILITY_CONTROL_CATEGORY_Id )
					ResourceAcsControl_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.ACCESSIBILITY_FEATURE_CATEGORY_Id )
					ResourceAcsFeature_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.ACCESSIBILITY_HAZARD_CATEGORY_Id )
					ResourceAcsHazard_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.CAREER_CLUSTER_CATEGORY_Id )
					ResourceCluster_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.EDUCATIONAL_USE_CATEGORY_Id )
					ResourceEdUse_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.GRADE_LEVEL_CATEGORY_Id )
					Resource_GradeLevel_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.GROUP_TYPE_CATEGORY_Id )
					Resource_GroupType_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.LANGUAGE_CATEGORY_Id )
					Resource_Language_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.MEDIA_TYPE_CATEGORY_Id )
					Resource_Format_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.RESOURCE_TYPE_CATEGORY_Id )
					Resource_Type_Create( context, tag, tv );

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.K12_SUBJECT_CATEGORY_Id )
				{
					ResourceSubject_Create( context, tag, tv );
				}

				else if ( tv.CategoryId == LB.CodesSiteTagCategory.TARGET_SITE_CATEGORY_Id )
				{
					// special for site category:
					// - the tag values are 255-278, the codes.site are 1-4
				}
			}
			catch ( Exception ex )
			{
				//catch here to allow continuing
				LoggingHelper.DoTrace( 2, "@@@@@@ " +  thisClassName + string.Format( ".SyncToLegacyCodeTable(). rId: {0}, tagId: {1} ", tag.ResourceIntId, tag.TagValueId ) + ex.Message );
			}
        }

        #endregion

        #region === resource standards

		/// <summary>
		/// Not currently used, had gone back to using the stored procs!!!
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int ResourceStandard_Create( LB.ResourceStandard entity )
		{
			int newId = 0;
			using ( var context = new ResourceContext() )
			{
				Resource_Standard e = new Resource_Standard();
				//interface doesn't prevent duplicate adds, so check here
				e = context.Resource_Standard.SingleOrDefault( s => s.ResourceIntId == entity.ResourceIntId && s.StandardId == entity.StandardId );
				if ( e != null && e.Id > 0 )
				{
					//exists, so skip
					return 0;
				}

				e = new Resource_Standard();
				e.ResourceIntId = entity.ResourceIntId;
				e.StandardId = entity.StandardId;
				if ( entity.CreatedById > 0 )
					e.AlignedById = entity.CreatedById;
				else
					e.AlignedById = entity.AlignedById;
				e.Created = System.DateTime.Now;
				//AlignmentDegreeId is actually usage (major, supporting, additional )
				e.UsageTypeId = entity.UsageTypeId;
				e.AlignmentTypeCodeId = entity.AlignmentTypeCodeId;
				e.StandardUrl = null;

				context.Resource_Standard.Add( e );

				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					newId = e.Id;
					return e.Id;
				}
				else
				{
					//?no info on error
					LoggingHelper.LogError( thisClassName + ".ResourceStandard_Create()", true );
				}

			}
			return newId;
		}

        #endregion

        #region === resource child tables
        public static int Resource_IntendedAudience_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;
            Resource_IntendedAudience e = new Resource_IntendedAudience();
            try
            {
                e.RowID = Guid.NewGuid();
                e.ResourceIntId = tag.ResourceIntId;
                e.AudienceId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_IntendedAudience.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return count;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".Resource_IntendedAudience_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Resource_IntendedAudience_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
            }
            return id;
        }

		public static int ResourceAcsControl_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
		{
			int id = 0;
			Resource_AccessibilityControl e = new Resource_AccessibilityControl();
			try
			{
				e.ResourceIntId = tag.ResourceIntId;
				e.AccessibilityControlId = tv.CodeId;

				if ( tag.CreatedById > 0 )
					e.CreatedById = tag.CreatedById;
				e.Created = System.DateTime.Now;
				context.Resource_AccessibilityControl.Add( e );

				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					return count;
				}
				else
				{
					//?no info on error
					LoggingHelper.LogError( thisClassName + ".ResourceAcsControl_Create() - failed to add!", true );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".ResourceAcsControl_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
			}
			return id;
		}

		public static int ResourceAcsFeature_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
		{
			int id = 0;
			Resource_AccessibilityFeature e = new Resource_AccessibilityFeature();
			try
			{
				e.ResourceIntId = tag.ResourceIntId;
				e.AccessibilityFeatureId = tv.CodeId;

				if ( tag.CreatedById > 0 )
					e.CreatedById = tag.CreatedById;
				e.Created = System.DateTime.Now;
				context.Resource_AccessibilityFeature.Add( e );

				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					return count;
				}
				else
				{
					//?no info on error
					LoggingHelper.LogError( thisClassName + ".ResourceAcsFeature_Create() - failed to add!", true );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".ResourceAcsFeature_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
			}
			return id;
		}

		public static int ResourceAcsHazard_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
		{
			int id = 0;
			Resource_AccessibilityHazard e = new Resource_AccessibilityHazard();
			try
			{
				e.ResourceIntId = tag.ResourceIntId;
				e.AccessibilityHazardId = tv.CodeId;

				if ( tag.CreatedById > 0 )
					e.CreatedById = tag.CreatedById;
				e.Created = System.DateTime.Now;
				context.Resource_AccessibilityHazard.Add( e );

				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					return count;
				}
				else
				{
					//?no info on error
					LoggingHelper.LogError( thisClassName + ".ResourceAcsHazard_Create() - failed to add!", true );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".ResourceAcsHazard_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
			}
			return id;
		}

        public static int ResourceCluster_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;
            Resource_Cluster e = new Resource_Cluster();
            try
            {
                e.ResourceIntId = tag.ResourceIntId;
                e.ClusterId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_Cluster.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return e.Id;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".ResourceCluster_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".ResourceCluster_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
            }
            return id;
        }

        public static int ResourceEdUse_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;

            Resource_EducationUse e = new Resource_EducationUse();
            try
            {
                e.ResourceIntId = tag.ResourceIntId;
                e.EducationUseId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_EducationUse.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return e.Id;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".ResourceEdUse_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".ResourceEdUse_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
            }
            return id;
        }

        public static int Resource_GradeLevel_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;
            Resource_GradeLevel e = new Resource_GradeLevel();
            try
            {
                e.ResourceIntId = tag.ResourceIntId;
                e.GradeLevelId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_GradeLevel.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return e.Id;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".Resource_GradeLevel_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Resource_GradeLevel_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
            }
            return id;
        }

        public static int Resource_GroupType_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;

            Resource_GroupType e = new Resource_GroupType();
            try
            {
                e.ResourceIntId = tag.ResourceIntId;
                e.GroupTypeId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_GroupType.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return e.Id;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".Resource_GroupType_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Resource_GroupType_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
            }
            return id;
        }

        public static int Resource_Language_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;

            Resource_Language e = new Resource_Language();
            try
            {
                e.RowId = Guid.NewGuid();
                e.ResourceIntId = tag.ResourceIntId;
                e.LanguageId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_Language.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return count;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".Resource_Language_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + string.Format( ".Resource_Language_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId ) );
            }
            return id;
        }

        public static int Resource_Format_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;
            Resource_Format e = new Resource_Format();
            try
            {
                e.RowId = Guid.NewGuid();
                e.ResourceIntId = tag.ResourceIntId;
                e.CodeId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_Format.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return count;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".Resource_Format_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
				string message = "";
				if (ex.InnerException != null && ex.InnerException.Message != null)
					message = "<br/>InnerException: " + ex.InnerException.Message;

				LoggingHelper.LogError(ex, thisClassName + string.Format(".Resource_Format_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId) + message);
            }
            return id;
        }

        public static int Resource_Type_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;
            Resource_ResourceType e = new Resource_ResourceType();
            try
            {
                e.RowId = Guid.NewGuid();
                e.ResourceIntId = tag.ResourceIntId;
                e.ResourceTypeId = tv.CodeId;

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_ResourceType.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return count;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".Resource_Type_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
				string message = "";
				if (ex.InnerException != null && ex.InnerException.Message != null)
					message = "<br/>InnerException: " + ex.InnerException.Message;

				LoggingHelper.LogError(ex, thisClassName + string.Format(".Resource_Type_Create() Category: {0}, abs tagId: {1}", tv.Title, tag.TagValueId) + message);
            }
            return id;
        }


        public static int ResourceSubject_Create( ResourceContext context, Resource_Tag tag, TagValue tv )
        {
            int id = 0;
            Resource_Subject e = new Resource_Subject();
            try
            {
                e.ResourceIntId = tag.ResourceIntId;
				if ( tv.CodeId > 0 )
				{
					//major kludge as a result of merging subjects and career clusters
					//otherwise we will get an FK error on resource.subject
					if ( tv.CodeId < 20 )
					{
						LoggingHelper.DoTrace( 2, "redirecting from ResourceSubject_Create to ResourceCluster_Create" );
						return ResourceCluster_Create( context, tag, tv );
					}
					e.CodeId = tv.CodeId;
				}
                if ( tv.Codes_TagCategory != null && tv.Codes_TagCategory.Id > 0 )
                    e.Subject = tv.Title;
                else
                    e.Subject = "Missing";

                if ( tag.CreatedById > 0 )
                    e.CreatedById = tag.CreatedById;
                e.Created = System.DateTime.Now;

                context.Resource_Subject.Add( e );

                // submit the change to database
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    return e.Id;
                }
                else
                {
                    //?no info on error
                    LoggingHelper.LogError( thisClassName + ".ResourceSubject_Create() - failed to add!", true );
                }
            }
            catch ( Exception ex )
            {
				LoggingHelper.LogError( ex, thisClassName + string.Format( ".ResourceSubject_Create() Category: {0}, abs tagId: {1}, relative CodeId: {2}", tv.Title, tag.TagValueId, tv.CodeId ) );
            }
            return id;
        }

        #endregion

        #region likes

        public static bool HasLikeDislike( int resourceId, int userId )
        {
            bool hasLike = false;
   
            using ( var context = new ResourceContext() )
          {

                Resource_Like item = context.Resource_Like
                        .SingleOrDefault( s => s.ResourceIntId == resourceId && s.CreatedById == userId);
                if ( item != null && item.Id > 0 )
                    hasLike = true;
            }

            return hasLike;
        }

        #endregion

        #region analytics

        public static int ResourceViewCount( int resourceId )
        {
            int count = 0;

            using ( var context = new ResourceContext() )
            {
                count = context.Resource_View
                .Where( s => s.ResourceIntId == resourceId )
                .Count();
            }

            return count;
        }
        
        public static int ResourceLikeGroupCount( int resourceId )
        {
            int count = 0;

            using ( var context = new ResourceContext() )
            {
                var query = context.Resource_Like
                    .Where( s => s.ResourceIntId == resourceId )
                   .GroupBy( p => p.IsLike )
                   .Select( g => new { name = g.Key, count = g.Count() } );

             
                
            }

            return count;
        }
        #endregion

		#region Clean up stuff


		/// <summary>
		/// Get list of resources that need reindexing
		/// Max is 2000
		/// </summary>
		/// <returns></returns>
		public static List<int> Resource_ReindexList_Pending( ref DateTime maxDate )
		{
			List<int> list = new List<int>();
			maxDate = DateTime.Now.AddDays(-10);
			//could make this configurable?
			int maxItems = 2000;
			using ( var context = new ResourceContext() )
			{
				List<Resource_ReindexList> items = context.Resource_ReindexList
					.Where( s => s.StatusId == 1 )
					.OrderBy( s => s.ResourceId)
				    .Take( maxItems ).ToList();
				foreach ( Resource_ReindexList item in items )
				{
					list.Add( (int)item.ResourceId );
					//do we cheat and update status here?
					if ( item.Created > maxDate )
						maxDate = (DateTime) item.Created;
				}
			}

			return list;
		}

		//public bool Resource_ReindexList_UpdatePending( DateTime maxDate )
		//{

		//	using ( var context = new ResourceContext() )
		//	{
		//		context.Database.ExecuteSqlCommand(
		//			"UPDATE [dbo].[Resource.ReindexList] " + 
		//				"SET [StatusId] = 2, [LastUpdated] = getdate() " + 
		//				"WHERE [StatusId] = 1 AND Created <= " + maxDate.ToString("yyyy-MM-dd")
		//				); 
		//	}

		//	return true;
		//}
		//public bool Resource_ReindexList_UpdatePending2( DateTime maxDate )
		//{
		//	string date = maxDate.ToString( "yyyy-MM-dd" );
		//	using ( var context = new ResourceContext() )
		//	{
		//		context.Resource_ReindexList.SqlQuery(
		//			"dbo.[Resource.ReindexListUpdate] @p0", date
		//		); 
		//	}

		//	return true;
		//}
		#endregion
        
    }
}
