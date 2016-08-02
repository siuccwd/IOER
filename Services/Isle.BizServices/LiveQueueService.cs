using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Caching;
using System.Collections.Concurrent;

namespace Isle.BizServices
{
	public class LiveQueueService
	{
		/// <summary>
		/// Get the cache's global LiveQueueItems, or if they have expired, create a new one, insert them into the cache, and return them.
		/// </summary>
		/// <returns></returns>
		public static ConcurrentBag<LiveQueueItem> GetCacheItems()
		{
			var cache = MemoryCache.Default;
			var items = cache[ "LiveQueueItems" ] as ConcurrentBag<LiveQueueItem>;
			if ( items == null )
			{
				items = new ConcurrentBag<LiveQueueItem>();
				var policy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan( 1, 0, 0 ) };
				var cacheItem = new CacheItem( "LiveQueueItems", items );
				cache.Add( cacheItem, policy );
			}

			return items;
		}
		//

		/// <summary>
		/// Add an item to the cache and return the random GUID assigned to it
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string AddItem( LiveQueueItem input )
		{
			var items = GetCacheItems();

			var randomID = Guid.NewGuid().ToString();
			input.MyCacheId = randomID;

			items.Add( input );

			return randomID;
		}
		//

		public static string AddItem( object content, string owner, string status, LiveQueueItem.ContentPurpose purpose )
		{
			var item = new LiveQueueItem()
			{
				Content = content,
				Owner = owner,
				IsInProgress = false,
				Status = status,
				Purpose = purpose
			};

			return AddItem( item );
		}
		//

		/// <summary>
		/// Remove an item from the cache and return it
		/// </summary>
		/// <param name="targetCacheId"></param>
		/// <param name="ignoreIsInProgress"></param>
		/// <param name="itemWasFound"></param>
		/// <param name="itemWasRemoved"></param>
		public static LiveQueueItem TakeItem( string targetCacheID, bool ignoreIsInProgress, ref bool itemWasFound, ref bool itemWasRemoved )
		{
			//Get cache items
			var items = GetCacheItems();

			//Set defaults
			itemWasFound = false;
			itemWasRemoved = false;

			//Find target item by GUID
			var toRemove = items.Where( m => m.MyCacheId == targetCacheID ).FirstOrDefault();
			//if found...
			if ( toRemove != null )
			{
				itemWasFound = true;
				//If the item is not in use (or if we don't care), remove it
				if ( ignoreIsInProgress || !toRemove.IsInProgress )
				{
					var holder = new LiveQueueItem();
					items.TryTake( out holder );
					if ( holder != null )
					{
						itemWasRemoved = true;
						return holder;
					}
				}
			}

			return null;
		}
		//

		public static LiveQueueItem TakeItem( string targetCacheID, bool ignoreIsInProgress )
		{
			var found = false;
			var removed = false;
			return TakeItem( targetCacheID, ignoreIsInProgress, ref found, ref removed );
		}

	}
	//

	public class LiveQueueItem
	{
		public enum ContentPurpose
		{
			GENERAL_PURPOSE,
			IMAGE_UPLOAD,
			FILE_UPLOAD,
			FILE_SIDELOAD_GOOGLEDRIVE,
			THUMBNAIL,
			RESOURCE_PUBLISH,
			RESOURCE_REINDEX
		};

		public Type ContentType
		{
			get { return Content.GetType(); }
		}

		public string MyCacheId { get; set; }
		public string Status { get; set; }
		public bool IsInProgress { get; set; }
		public string Owner { get; set; }
		public object Content { get; set; }
		public ContentPurpose Purpose { get; set; }
	}
	//

}
