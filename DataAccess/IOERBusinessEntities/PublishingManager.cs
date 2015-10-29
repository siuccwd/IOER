using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using AutoMapper;
using LB = LRWarehouse.Business;
using ILPathways.Utilities;
using Isle.DTO;

namespace IOERBusinessEntities
{
	public class PublishingManager
	{
		static string thisClassName = "PublishingManager";


		#region select delayed publishing records
		/// <summary>
		/// Retrieve all pending thumbnail requests
		/// </summary>
		/// <returns></returns>
		public static List<ResourceDelayedPublish> ResourceDelayedPublish_SelectPendingThumbnails()
		{
			ResourceDelayedPublish entity = new ResourceDelayedPublish();
			List<ResourceDelayedPublish> list = new List<ResourceDelayedPublish>();

			using ( var context = new ResourceContext() )
			{
				List<Resource_DelayedPublish> items = context.Resource_DelayedPublish
								.Where( s => s.ThumbnailsStatusId == 1 )
								.OrderBy( s => s.ContentId ).ThenBy( s => s.ResourceIntId )
								.ToList();

				if ( items.Count > 0 )
				{
					foreach ( Resource_DelayedPublish item in items )
					{
						entity = ResourceDelayedPublish_ToMap( item );
						list.Add( entity );
					}
				}

			}
			return list;
		}//

		public static List<ResourceDelayedPublish> ResourceDelayedPublish_SelectPendingElasticUpdates()
		{
			ResourceDelayedPublish entity = new ResourceDelayedPublish();
			List<ResourceDelayedPublish> list = new List<ResourceDelayedPublish>();

			using ( var context = new ResourceContext() )
			{
				List<Resource_DelayedPublish> items = context.Resource_DelayedPublish
								.Where( s => s.ElasticStatusId == 1 )
								.OrderBy( s => s.ContentId ).ThenBy( s => s.ResourceIntId )
								.ToList();

				if ( items.Count > 0 )
				{
					foreach ( Resource_DelayedPublish item in items )
					{
						entity = ResourceDelayedPublish_ToMap( item );
						list.Add( entity );
					}
				}

			}
			return list;
		}//
		public static List<ResourceDelayedPublish> ResourceDelayedPublish_SelectPendingLRUpdates()
		{
			ResourceDelayedPublish entity = new ResourceDelayedPublish();
			List<ResourceDelayedPublish> list = new List<ResourceDelayedPublish>();

			using ( var context = new ResourceContext() )
			{
				List<Resource_DelayedPublish> items = context.Resource_DelayedPublish
								.Where( s => s.LRPublishStatusId == 1 )
								.OrderBy( s => s.ContentId ).ThenBy( s => s.ResourceIntId )
								.ToList();

				if ( items.Count > 0 )
				{
					foreach ( Resource_DelayedPublish item in items )
					{
						entity = ResourceDelayedPublish_ToMap( item );
						list.Add( entity );
					}
				}

			}
			return list;
		}//
		private static List<ResourceDelayedPublish> ResourceDelayedPublish_SelectTypes( int statusId)
		{

			ResourceDelayedPublish entity = new ResourceDelayedPublish();
			List<ResourceDelayedPublish> list = new List<ResourceDelayedPublish>();

			using (var context = new ResourceContext())
			{
				List<Resource_DelayedPublish> items = context.Resource_DelayedPublish
								.Where(s => s.StatusId == statusId)
								.OrderBy(s => s.ContentId).ThenBy(s => s.ResourceIntId)
								.ToList();

				if (items.Count > 0)
				{
					foreach (Resource_DelayedPublish item in items)
					{
						entity = ResourceDelayedPublish_ToMap( item );

						list.Add(entity);
					}
				}

			}
			return list;
	}//

		public static List<ResourceDelayedPublish> ResourceDelayedPublish_Select( int parentContentId )
		{

			ResourceDelayedPublish entity = new ResourceDelayedPublish();
			List<ResourceDelayedPublish> list = new List<ResourceDelayedPublish>();

			using ( var context = new ResourceContext() )
			{

				List<Resource_DelayedPublish> items = context.Resource_DelayedPublish
								.Where( s => s.ParentContentId == parentContentId )
								.OrderBy( s => s.ResourceIntId )
								.ToList();

				if ( items.Count > 0 )
				{
					foreach ( Resource_DelayedPublish item in items )
					{
						entity = ResourceDelayedPublish_ToMap( item );

						list.Add( entity );
					}
				}

			}
			return list;
		}//


		private static ResourceDelayedPublish ResourceDelayedPublish_ToMap( Resource_DelayedPublish item )
		{
			ResourceDelayedPublish entity = new ResourceDelayedPublish();

			entity.Id = item.Id;
			entity.ParentContentId = item.ParentContentId;
			entity.ContentId = item.ContentId;
			entity.ResourceIntId = item.ResourceIntId;
			//entity.ResourceVersionIntId = item.ResourceVersionIntId;
			entity.ResourceUrl = item.ResourceUrl;
			entity.ContentTypeId = item.ContentTypeId;

			entity.ThumbnailsStatusId = ( int ) item.ThumbnailsStatusId;
			entity.ElasticStatusId = ( int ) item.ElasticStatusId;
			entity.LRPublishStatusId = ( int ) item.LRPublishStatusId;
			entity.StatusId = item.StatusId;

			entity.Created = item.Created;
			entity.CreatedById = item.CreatedById;
			return entity;
		}//

		public int ResourceDelayedPublish_Update( ResourceDelayedPublish entity )
		{
			int count = 0;
			using ( var context = new ResourceContext() )
			{

				Resource_DelayedPublish item = context.Resource_DelayedPublish
						.SingleOrDefault( s => s.Id == entity.Id );

				//only reason to update will be statusId, and perhaps published.
				//we may want to be more granular, and have status for es, thumbnails, and LR publish
				item.ThumbnailsStatusId = entity.ThumbnailsStatusId;
				item.ElasticStatusId = entity.ElasticStatusId;
				item.LRPublishStatusId = entity.LRPublishStatusId;

				item.StatusId = entity.StatusId;
				if ( entity.PublishedDate != null && entity.PublishedDate > entity.Created )
					item.PublishedDate = entity.PublishedDate;

				// submit the change to database
				count = context.SaveChanges();
			}
			return count;
		}
		#endregion

		#region

		/// <summary>
		/// Add Resource_DelayedPublish
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		public int ResourceDelayedPublish_Add( ResourceDelayedPublish entity )
		{
			
			int newId = 0;
			using ( var context = new ResourceContext() )
			{
				try
				{
					Resource_DelayedPublish efEntity = new Resource_DelayedPublish();
					efEntity.ParentContentId = entity.ParentContentId;
					efEntity.ContentId = entity.ContentId;
					efEntity.ContentTypeId = entity.ContentTypeId;
					efEntity.ResourceIntId = entity.ResourceIntId;
					efEntity.ElasticStatusId = entity.ElasticStatusId;
					efEntity.LRPublishStatusId = entity.LRPublishStatusId;
					efEntity.ThumbnailsStatusId = entity.ThumbnailsStatusId;
					efEntity.ResourceUrl = entity.ResourceUrl;

					efEntity.Created = System.DateTime.Now;
					efEntity.CreatedById = entity.CreatedById;

					context.Resource_DelayedPublish.Add( efEntity );

					int count = context.SaveChanges();
					if ( count > 0 )
					{
						newId = efEntity.Id;
						//add related standard
						//may need at a higher level so that can reference elastic update code
						//ContentRelatedStandard_Add( contentId, newId );
					}
					else
					{
						//?no info on error
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".ContentStandard_Add()" );
				}
			}

			return newId;
		}

		#endregion
	}
}
