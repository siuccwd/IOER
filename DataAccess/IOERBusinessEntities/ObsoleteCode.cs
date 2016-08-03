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
	public class ObsoleteCode
	{
		static string thisClassName = "EFResourceManager";
		DateTime DefaultDate = new System.DateTime( 1970, 1, 1 );

		#region == obsolete methods
		/// <summary>
		/// Create a complete resource, including all children and resource tags
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int Resource_CompleteCreate( ResourceTransformDTO entity )
		{
			using ( var context = new ResourceContext() )
			{
				Resource e = new Resource();
				e.ResourceUrl = entity.ResourceUrl;
				e.ImageUrl = entity.ImageUrl;

				e.IsActive = entity.IsActive;
				// e.HasPathwayGradeLevel = false;
				e.FavoriteCount = 0;
				e.ViewCount = 0;
				if ( entity.Created != null && entity.Created > DefaultDate )
				{
					e.Created = entity.Created;
				}
				else
					e.Created = System.DateTime.Now;

				entity.Resource_Version.Created = ( System.DateTime ) e.Created;

				e.LastUpdated = e.Created;
				e.RowId = Guid.NewGuid();
				context.Resources.Add( e );

				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{

					entity.Id = e.Id;
					if ( entity.CreatedById > 0 )
					{
						//Add r.published by
						ResourcePublishedBy_Create( context, entity );
					}
					LoggingHelper.DoTrace( 4, string.Format( thisClassName + ".Resource_CompleteCreate. Created resource witn resId= {0}.", entity.Id ) );
					entity.Resource_Version.ResourceIntId = e.Id;
					ResourceVersion_Create( entity.Resource_Version, context );

					//add children
					//language
					if ( entity.LanguageId > 0 )
					{
						//will be part of tags for now
					}
					//standards
					if ( entity.ResourceStandardIds.Count > 0 )
					{
						ResourceStandard_Create( context, entity );
					}
					//tags
					if ( entity.ResourceTagsIds != null && entity.ResourceTagsIds.Count > 0 )
					{
						ResourceTag_Create( context, entity.ResourceTagsIds, entity.ResourceId, entity.CreatedById );
					}

					//keywords
					if ( entity.Resource_Keywords != null && entity.Resource_Keywords.Count > 0 )
					{
						Resource_Keyword_Create( context, entity );
					}
					//subjects????
					//if ( entity.SubjectMap != null && entity.SubjectMap.Count > 0 )
					//{

					//}
					return e.Id;
				}
				else
				{
					//?no info on error
					return 0;
				}
			}
		}
		private int ResourceVersion_Create( LB.ResourceVersion entity, ResourceContext context )
		{
			//?????
			entity.IsActive = true;
			if ( entity.Created == null || entity.Created < DefaultDate )
				entity.Created = System.DateTime.Now;

			if ( entity.Imported == null || entity.Imported < DefaultDate )
			{
				entity.Imported = System.DateTime.Now;
			}
			if ( entity.Modified == null || entity.Modified < DefaultDate )
			{
				entity.Modified = System.DateTime.Now;
			}

			Resource_Version rv = ResourceVersion_FromMap( entity );
			rv.RowId = Guid.NewGuid();

			context.Resource_Version.Add( rv );

			// submit the change to database
			int count = context.SaveChanges();

			return rv.Id;
		}

			
        public string CreateSortTitleXX( LB.ResourceVersion entity )
        {
            //TODO - create method for sort title
            string title = "";
            if ( entity.SortTitle == null || entity.SortTitle.Trim().Length == 0 )
            {
                entity.SortTitle = entity.Title;
            }

            return title;
        }
        public int ResourceVersion_Update( LB.ResourceVersion entity, ResourceContext context )
        {

            Resource_Version rv = context.Resource_Version
                    .SingleOrDefault( s => s.Id == entity.Id );

            Resource_Version rvUpdated = ResourceVersion_FromMap( entity );

            rv.Title = rvUpdated.Title;
            rv.Description = rvUpdated.Description;
            rv.AccessRightsId = rvUpdated.AccessRightsId;
            rv.Requirements = rvUpdated.Requirements;
            rv.Rights = rvUpdated.Rights;
            
            rv.IsActive = entity.IsActive;
            rv.Modified = System.DateTime.Now;

            // submit the change to database
            int count = context.SaveChanges();

            return count;
        }

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

      
		 #region === resource version
      
        /// <summary>
        /// Get the most recent resource version for the related resource Id
        /// - should do an is active check!
        /// </summary>
        /// <param name="resourceId"></param>
        /// <returns></returns>
        public static LB.ResourceVersion ResourceVersion_GetByResourceIdXX( int resourceId )
        {
          return ResourceVersion_GetByResourceIdXX( resourceId, true );
        }//
        public static LB.ResourceVersion ResourceVersion_GetByResourceIdXX( int resourceId, bool mustBeActive )
        {
          LB.ResourceVersion entity = new LB.ResourceVersion();

          using ( var context = new ResourceContext() )
          {
            List<Resource_Version> list;
            if ( mustBeActive ) //Only get if active
            {
              list = context.Resource_Version
                          .Include( "Resource" )
                          .Where( s => s.ResourceIntId == resourceId && s.IsActive == true )
                          .OrderByDescending( s => s.Id )
                          .ToList();
            }
            else //Get regardless of active state--may be preferable to only get if not active?
            {
              list = context.Resource_Version
                          .Include( "Resource" )
                          .Where( s => s.ResourceIntId == resourceId )
                          .OrderByDescending( s => s.Id )
                          .ToList();
            }
            if ( list != null && list.Count > 0 )
            {
              foreach ( Resource_Version rv in list )
              {
                entity = ResourceVersion_ToMap( rv );
                break;
              }
            }
          }
          return entity;
        }

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

        private static Resource_Version ResourceVersion_FromMap( LB.ResourceVersion fromEntity )
        {
            Resource_Version to = new Resource_Version();

            Mapper.CreateMap<LB.ResourceVersion, Resource_Version>()
                  .ForMember( d => d.Resource, o => o.Ignore() );

            Mapper.AssertConfigurationIsValid();

            to = Mapper.Map<LB.ResourceVersion, Resource_Version>( fromEntity );

            if ( to.Title != null 
                && ( to.SortTitle == null || to.SortTitle.Trim().Length == 0 ))
            {
                to.SortTitle = LB.ResourceVersion.UrlFriendlyTitle( to.Title);

            }
            return to;
        }
        #endregion
		private int ResourceTag_Create( ResourceContext context, List<int> tags, int resourceID, int createdById )
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
						//SyncToLegacyCodeTable( context, tag );
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


		public int Resource_CompleteUpdate( ResourceTransformDTO entity )
		{

			using ( var context = new ResourceContext() )
			{
				Resource e = context.Resources
					.Include( "Resource_Version" )
					.SingleOrDefault( s => s.Id == entity.Id );

				e.ResourceUrl = entity.ResourceUrl;
				//may want a temp check to ensure not being overridden
				if ( string.IsNullOrWhiteSpace( e.ImageUrl ) )
					e.ImageUrl = entity.ImageUrl;

				e.IsActive = entity.IsActive;
				//e.HasPathwayGradeLevel = false;
				e.FavoriteCount = entity.FavoriteCount;
				e.ViewCount = entity.ViewCount;
				// e.Created = System.DateTime.Now;
				e.LastUpdated = System.DateTime.Now;


				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{

					entity.Resource_Version.ResourceIntId = e.Id;
					ResourceVersion_Update( entity.Resource_Version, context );

					//add children
					//language
					if ( entity.LanguageId > 0 )
					{
						//will be part of tags for now
					}
					//standards
					if ( entity.ResourceStandardIds.Count > 0 )
					{
						ResourceStandard_Create( context, entity );
					}
					//tags
					if ( entity.ResourceTagsIds != null && entity.ResourceTagsIds.Count > 0 )
					{
						ResourceTag_Create( context, entity.ResourceTagsIds, entity.ResourceId, entity.CreatedById );
					}

					//keywords
					if ( entity.Resource_Keywords != null && entity.Resource_Keywords.Count > 0 )
					{
						Resource_Keyword_Create( context, entity );
					}
					//subjects????
					//if ( entity.SubjectMap != null && entity.SubjectMap.Count > 0 )
					//{

					//}
					return e.Id;
				}
				else
				{
					//?no info on error
					return 0;
				}
			}
		}

		public static ResourceTransformDTO Resource_CompleteGet( int resourceId )
		{
			ResourceTransformDTO entity = new ResourceTransformDTO();

			using ( var context = new ResourceContext() )
			{
				Resource efEntity = context.Resources
							.Include( "Resource_Version" )
							.Include( "Resource_Tag" )
							.Include( "Resource_Keyword" )
							.Include( "Resource_Standard" )
							.Include( "Resource_Subject" )
							.SingleOrDefault( s => s.Id == resourceId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					entity = Resource_Complete_ToMap( context, efEntity );
					//add keywords, subjects, grade level
				}
			}
			return entity;
		}//
		private static ResourceTransformDTO Resource_Complete_ToMap( ResourceContext context, Resource fromEntity )
		{
			ResourceTransformDTO to = new ResourceTransformDTO();

			try
			{
				to.Id = fromEntity.Id;
				to.RowId = fromEntity.RowId;

				to.ResourceUrl = fromEntity.ResourceUrl != null ? fromEntity.ResourceUrl : "";
				to.ImageUrl = fromEntity.ImageUrl != null ? fromEntity.ImageUrl : "";

				to.FavoriteCount = fromEntity.FavoriteCount != null ? ( int ) fromEntity.FavoriteCount : 10;
				to.ViewCount = fromEntity.ViewCount != null ? ( int ) fromEntity.ViewCount : 0;

				to.IsActive = ( bool ) fromEntity.IsActive;
				//to.HasPathwayGradeLevel = fromEntity.HasPathwayGradeLevel != null ? ( bool ) fromEntity.HasPathwayGradeLevel : false;

				if ( fromEntity.Created != null )
					to.Created = ( System.DateTime ) fromEntity.Created;
				if ( fromEntity.LastUpdated != null )
					to.LastUpdated = ( System.DateTime ) fromEntity.LastUpdated;


				if ( fromEntity.Resource_Version != null && fromEntity.Resource_Version.Count > 0 )
				{
					//just get first active one
					foreach ( Resource_Version rv in fromEntity.Resource_Version )
					{
						if ( rv.IsActive == true )
						{
							to.Resource_Version = ResourceVersion_ToMap( rv );
							break;
						}
					}
				}
				//============ resource tags ============================================
				if ( fromEntity.Resource_Tag != null && fromEntity.Resource_Tag.Count > 0 )
				{
					to.ResourceTagsIds = new List<int>();
					to.ResourceTagsIds = ResourceTaggingManager.Resource_GetTagIds( to.Id );

					to.ResourceTags = new List<LB.ResourceTag>();
					//not sure if can get the related code table, so just call the method
					to.ResourceTags = ResourceTaggingManager.Resource_GetTags( to.Id );
				}

				//=========== subjects - TBD =============================================
				if ( fromEntity.Resource_Subject != null && fromEntity.Resource_Subject.Count > 0 )
				{
					to.Resource_Subjects = new List<string>();

					foreach ( Resource_Subject rs in fromEntity.Resource_Subject )
					{
						to.Resource_Subjects.Add( rs.Subject );
					}
				}

				//=========== keywords =============================================
				if ( fromEntity.Resource_Keyword != null && fromEntity.Resource_Keyword.Count > 0 )
				{
					to.Resource_Keywords = new List<string>();

					foreach ( Resource_Keyword rk in fromEntity.Resource_Keyword )
					{
						to.Resource_Keywords.Add( rk.Keyword );
					}
				}

				//=========== standards =============================================
				if ( fromEntity.Resource_Standard != null && fromEntity.Resource_Standard.Count > 0 )
				{
					to.Standards = new List<LB.ResourceStandard>();
					to.ResourceStandardIds = new List<int>();
					LB.ResourceStandard standard = new LB.ResourceStandard();
					StandardBody_Node standardNode = new StandardBody_Node();

					foreach ( Resource_Standard efEntity in fromEntity.Resource_Standard )
					{
						Mapper.CreateMap<Resource_Standard, LB.ResourceStandard>()
						   .ForMember( d => d.UsageType, o => o.Ignore() )
						   .ForMember( d => d.AlignmentTypeValue, o => o.Ignore() )
						   .ForMember( d => d.StandardDescription, o => o.Ignore() )
						   .ForMember( d => d.StandardNotationCode, o => o.Ignore() );

						Mapper.AssertConfigurationIsValid();

						standard = Mapper.Map<Resource_Standard, LB.ResourceStandard>( efEntity );
						standardNode = GetStandard( context, efEntity.StandardId );
						if ( standardNode != null && standardNode.Description != null )
							standard.StandardDescription = standardNode.Description;

						to.Standards.Add( standard );
						//add just the Id
						to.ResourceStandardIds.Add( efEntity.Id );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Resource_Complete_ToMap( Resource fromEntity )" );
			}
			return to;
		}//
		public static StandardBody_Node GetStandard( ResourceContext context, int id )
		{
			StandardBody_Node node = new StandardBody_Node();

			node = context.StandardBody_Node
				.SingleOrDefault( s => s.Id == id );

			return node;
		}
		#endregion

		#region === resource publishedBy ===

		/// <summary>
		/// NOT USED
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int ResourcePublishedBy_Create( ResourceContext context,
						ResourceTransformDTO entity )
		{
			int createCount = 0;

			Resource_PublishedBy e = new Resource_PublishedBy();
			e.ResourceIntId = entity.Id;
			e.PublishedById = entity.CreatedById;
			if ( entity.PublishedForOrgId > 0 )
				e.PublishedForOrgId = entity.PublishedForOrgId;

			e.Created = System.DateTime.Now;

			context.Resource_PublishedBy.Add( e );

			// submit the change to database
			int count = context.SaveChanges();
			if ( count > 0 )
			{
				createCount++;
				//return e.Id;
			}
			else
			{
				//?no info on error
				LoggingHelper.LogError( thisClassName + ".ResourcePublishedBy_Create()", true );
			}


			return createCount;
		}
		#endregion

		#region === resource keywords
		public int Resource_Keyword_Create( ResourceContext context,
					   ResourceTransformDTO entity )
		{
			int createCount = 0;

			foreach ( string keyword in entity.Resource_Keywords )
			{
				if ( keyword.Trim().Length > 3 )
				{
					Resource_Keyword e = new Resource_Keyword();
					e.ResourceIntId = entity.Id;
					e.Keyword = keyword;

					if ( entity.CreatedById > 0 )
						e.CreatedById = entity.CreatedById;
					e.Created = System.DateTime.Now;

					context.Resource_Keyword.Add( e );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						createCount++;
						//return e.Id;
					}
					else
					{
						//?no info on error
						LoggingHelper.LogError( thisClassName + ".Resource_Keyword_Create()", true );
					}
				}
			}

			return createCount;
		}

		#endregion


		#region === resource standards

		/// <summary>
		/// this method is just using a stanard id, with no checks for alignment and usage ==> it should not be used, and replace with a fuller version - checking
		/// </summary>
		/// <param name="context"></param>
		/// <param name="entity"></param>
		/// <returns></returns>
		private int ResourceStandard_Create( ResourceContext context,
						ResourceTransformDTO entity )
		{
			int createCount = 0;
			Resource_Standard e = new Resource_Standard();
			foreach ( int tagId in entity.ResourceStandardIds )
			{

				//interface doesn't prevent duplicate adds, so check here
				e = context.Resource_Standard.SingleOrDefault( s => s.ResourceIntId == entity.Id && s.StandardId == tagId );
				if ( e != null && e.Id > 0 )
				{
					//exists, so skip
					continue;
				}

				e = new Resource_Standard();
				e.ResourceIntId = entity.Id;
				e.StandardId = tagId;
				if ( entity.CreatedById > 0 )
					e.AlignedById = entity.CreatedById;
				e.Created = System.DateTime.Now;
				//AlignmentDegreeId is actually usage (major, supporting, additional )
				e.UsageTypeId = 0;
				e.AlignmentTypeCodeId = 0;
				e.StandardUrl = null;

				context.Resource_Standard.Add( e );

				// submit the change to database
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					createCount++;
					//return e.Id;
				}
				else
				{
					//?no info on error
					LoggingHelper.LogError( thisClassName + ".ResourceStandard_Create()", true );
				}

			} //foreach

			return createCount;
		}

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

		#region === special resource summary - for web service calls - thinkGate
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

		#endregion
	}
}
