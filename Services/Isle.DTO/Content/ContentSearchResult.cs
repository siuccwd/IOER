using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Isle.DTO
{
	public class ContentSearchQuery
	{
		public ContentSearchQuery()
		{
			Filters = new List<ContentSearchFilter>();
			StandardIds = new List<int>();
			IsMyAuthoredView = false;
			IsApproversView = false;
			Message = "";
			CustomSearch = "";
		}
		public bool IsMyAuthoredView { get; set; }
		public bool IsApproversView { get; set; }
		public string Text { get; set; } //Will need to sanitize
		/// <summary>
		/// The CustomSearch is used when all the needed filters are set by the calling party. All other filters will be ignore.
		/// </summary>
		public string CustomSearch { get; set; } 
		public List<ContentSearchFilter> Filters { get; set; }
		public List<int> StandardIds { get; set; }
		public int PageStart { get; set; }
		public int PageSize { get; set; }
		public string SortOrder { get; set; } //Will need to sanitize
		public bool SortReversed { get; set; }
		public string Message { get; set; } 

	}
	public class ContentSearchFilter
	{
		public ContentSearchFilter()
		{
			Tags = new List<string>();
		}
		public string Category { get; set; }
		public List<string> Tags { get; set; } //Might work better as List<int> ?  Will need to sanitize string input.
	}
	public class ContentSearchResult
	{
		public ContentSearchResult()
		{
			Partners = new List<int>();
		}
		public int Id { get; set; }
		public string Guid { get; set; }
		public bool Editable { get; set; }
		public int TypeId { get; set; }
		public string Type { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public string EditUrl { get; set; }
		public string ImageUrl { get; set; }
		public string Author { get; set; }
		public string Privilege { get; set; }
		public string Status { get; set; }
		public string OrganizationTitle { get; set; }
		public string Updated { get; set; }
		public int ParentId { get; set; }
		public List<int> Partners { get; set; }
	}
}
