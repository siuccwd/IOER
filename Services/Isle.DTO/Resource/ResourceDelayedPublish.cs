using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
	public class ResourceDelayedPublish
	{
		public ResourceDelayedPublish()
		{
			ThumbnailsStatusId = -1;
			ElasticStatusId = -1;
			LRPublishStatusId = -1;
			ResourceUrl = "";
		}

		public int Id { get; set; }
		public int ParentContentId { get; set; }
		public int ContentId { get; set; }
		public int ContentTypeId { get; set; }
		public int ThumbnailsStatusId { get; set; }
		public int ElasticStatusId { get; set; }
		public int LRPublishStatusId { get; set; }
		public int StatusId { get; set; }
		public int ResourceIntId { get; set; }
		public int ResourceVersionIntId { get; set; }
		public System.DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public string DocId { get; set; }
		public System.DateTime PublishedDate { get; set; }

		public string ResourceUrl { get; set; }
	}
}
