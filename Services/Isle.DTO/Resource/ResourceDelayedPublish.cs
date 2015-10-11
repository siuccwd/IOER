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

		}

		public int Id { get; set; }
		public int ParentContentId { get; set; }
		public int ContentId { get; set; }
		public int ContentTypeId { get; set; }
		public int StatusId { get; set; }
		public int ResourceIntId { get; set; }
		public int ResourceVersionIntId { get; set; }
		public System.DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public string DocId { get; set; }
		public System.DateTime PublishedDate { get; set; }
	}
}
