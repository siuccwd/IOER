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
						entity = new ResourceDelayedPublish();
						entity.Id = item.Id;
						entity.ParentContentId = item.ParentContentId;
						entity.ContentId = item.ContentId;
						entity.ResourceIntId = item.ResourceIntId;
						entity.ResourceVersionIntId = item.ResourceVersionIntId;
						entity.ContentTypeId = item.ContentTypeId;
						entity.StatusId = item.StatusId;
						entity.Created = item.Created;
						entity.CreatedById = item.CreatedById;

						list.Add( entity );
					}
				}

			}
			return list;
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
				item.StatusId = entity.StatusId;
				if ( entity.PublishedDate != null && entity.PublishedDate > entity.Created )
					item.PublishedDate = entity.PublishedDate;

				// submit the change to database
				count = context.SaveChanges();
			}
			return count;
		}
        
	}
}
