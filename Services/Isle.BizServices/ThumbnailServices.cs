using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Caching;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.IO;

using ILPathways.Utilities;

namespace Isle.BizServices
{
	//A self-managing queue system to handle thumbnailing in a synchronous, serial manner from asynchronous calls from multiple threads
	public class ThumbnailServices
	{
		//This gets called from other methods
		public static void CreateThumbnail( string filename, string url, bool overwriteIfExists )
		{
			//Create the round
			var bullet = new ThumbnailItem()
			{
				Filename = filename,
				Url = url,
				OverwriteIfExists = overwriteIfExists,
				Firing = false
			};

			//Load the round into the end of the chain
			var chain = GetChain();
			chain.Enqueue( bullet );

			//Pull the trigger
			ThreadPool.QueueUserWorkItem( delegate
			{
				Fire();
				//chain.First().Fire();
			} );

		}//

		//Examine the chain without creating one if it doesn't exist
		public static List<ThumbnailItem> ExamineChain()
		{
			var result = new List<ThumbnailItem>();
			var cache = MemoryCache.Default;
			var chain = cache[ "ThumbnailChain" ] as ConcurrentQueue<ThumbnailItem>;

			if ( chain != null )
			{
				foreach ( var item in chain )
				{
					result.Add( new ThumbnailItem() //Ensure we do not pass by reference
					{
						Filename = item.Filename,
						Url = item.Url,
						OverwriteIfExists = item.OverwriteIfExists,
						Firing = item.Firing
					} );
				}
			}

			return result;
		}

		//Get the chain of ammo using thread-safe FIFO list
		static private ConcurrentQueue<ThumbnailItem> GetChain()
		{
			var cache = MemoryCache.Default;
			var chain = cache[ "ThumbnailChain" ] as ConcurrentQueue<ThumbnailItem>;

			if ( chain == null )
			{
				DumpChain();
				chain = new ConcurrentQueue<ThumbnailItem>();
				string thumbnailerLog = UtilityManager.GetAppKeyValue( "thumbnailerLog", "C:\\Thumbnail Generator 4\\lastrun.txt" );
				Log( thumbnailerLog, "", true ); //Refresh the log file
				var item = new CacheItem( "ThumbnailChain", chain );
				var policy = new CacheItemPolicy();
				policy.SlidingExpiration = new TimeSpan( 0, 15, 0 );
				cache.Add( item, policy );
			}

			return chain;
		}

		//Get rid of the chain
		static private void DumpChain() {
			var cache = MemoryCache.Default;
			cache.Remove( "ThumbnailChain" );
		}

		//Write to thumbnail log file
		static private void Log( string fileName, string text, bool overWrite )
		{
			var success = false;
			while ( !success )
			{
				try
				{
					if ( overWrite )
					{
						File.WriteAllText( fileName, text + System.Environment.NewLine );
					}
					else
					{
						File.AppendAllText( fileName, text + System.Environment.NewLine );
					}

					success = true;
				}
				catch ( IOException ex )
				{
					Thread.Sleep( 50 );
				}
			}
		}//

		//Make a thumbnail
		static private void Fire()
		{
			//Aim correctly
			string thumbnailerLog = UtilityManager.GetAppKeyValue( "thumbnailerLog", "C:\\Thumbnail Generator 4\\lastrun.txt" );
			string thumbnailerWorkingDirectory = UtilityManager.GetAppKeyValue( "thumbnailGeneratorV2Folder", "C:\\Thumbnail Generator 4" );
			string thumbnailGenerator = UtilityManager.GetAppKeyValue( "thumbnailGenerator", "C:\\Thumbnail Generator 4\\ThumbnailerV4User.exe" );

			//Feed the ammo
			var chain = GetChain();

			//Start shooting
			while ( chain != null && chain.Count() > 0 )
			{
				if ( chain.Where( m => m.Firing ).Count() == 0 )
				{
					//Load the first round
					var chambered = chain.First();

					//Prevent other rounds from firing
					chambered.Firing = true;

					//Only use live ammo on production
					if ( ServiceHelper.GetAppKeyValue( "envType", "dev" ) == "prod" )
					{
						//Fire
						try
						{
							var arguments = chambered.Filename + " \"" + chambered.Url + "\" " + ( chambered.OverwriteIfExists ? "true" : "false" );

							var processInfo = new ProcessStartInfo( thumbnailGenerator, arguments );
							processInfo.WorkingDirectory = thumbnailerWorkingDirectory;
							processInfo.CreateNoWindow = false;
							processInfo.UseShellExecute = false;

							var process = Process.Start( processInfo );
							process.WaitForExit( 30000 );
							process.Close();
							Log( thumbnailerLog, "Thumbnail " + chambered.Filename + " should now exist.", false );
						}
						catch ( Exception ex )
						{
							Log( thumbnailerLog, "Project Level Error: " + System.Environment.NewLine + ex.Message.ToString(), false );
						}
					}
					else
					{
						//Shoot blank
						Thread.Sleep( 5000 );
						Log( thumbnailerLog, "Simulated thumbnail created successfully: " + chambered.Filename, false );
					}

					//Eject the spent casing
					try
					{
						var spent = new ThumbnailItem();
						chain.TryDequeue( out spent );
					}
					catch ( Exception ex )
					{
						//Prevent rogue threads - better to miss thumbnails than overload the server
						Log( thumbnailerLog, "Thumbnail chain jammed! Dumping entries..." + System.Environment.NewLine + ex.Message.ToString(), false );
						DumpChain();
					}
				}
				else
				{
					//Ensure only one thread is working on the entire queue at a time
					Log( thumbnailerLog, "Another thread is handling this. Current thread is exiting.", false );
					return;
					//Wait and try firing again
					//Thread.Sleep( 1000 );
				}

			}

			//Eject empty magazine
			Log( thumbnailerLog, "All thumbnails should be finished", false );
			DumpChain();
		}

		//Ammunition
		public class ThumbnailItem
		{
			public bool Firing { get; set; }
			public string Filename { get; set; }
			public string Url { get; set; }
			public bool OverwriteIfExists { get; set; }
		}

	}  //End ThumbnailServices
}
