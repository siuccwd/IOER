using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Resources;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.UI;
using Microsoft.ApplicationBlocks.Data;

using ILPathways.Business;
using IOER.Library;
using ILPathways.Utilities;
using IOER.classes;
using DAL=ILPathways.DAL;

namespace IOER.classes
{
    public class ApplicationManager
    {
        
        //public ApplicationManager()
        //{ } //
        string thisClassName = "ApplicationManager";
        #region  caching methods
        /// <summary>
        /// Retrieve a string item from the current cache
        /// - assumes a default value of blank
        /// </summary>
        /// <param name="cacheKeyName"></param>
        /// <returns></returns>
        public static string GetCacheItem( string cacheKeyName )
        {
            string defaultValue = "";
            return GetCacheItem( cacheKeyName, defaultValue );

        }//

        /// <summary>
        /// Retrieve a string item from the current cache
        /// </summary>
        /// <param name="cacheKeyName">Is assumed to be the same as the filename (and filename to have an ext of txt)</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetCacheItem( string cacheKeyName, string defaultValue )
        {
            string filename = cacheKeyName + ".txt";
            return GetCacheItem( cacheKeyName, filename, defaultValue );
        }//

        /// <summary>
        /// Retrieve a string item from the current cache
        /// </summary>
        /// <param name="cacheKeyName"></param>
        /// <param name="filename"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetCacheItem( string cacheKeyName, string filename, string defaultValue )
        {
            string cacheItem = defaultValue;
            try
            {
                cacheItem = HttpContext.Current.Cache[ cacheKeyName ] as string;

                if ( string.IsNullOrEmpty( cacheItem ) )
                {
                    //assuming keyname is same as file name in app_Data - or should the ext also be part of the key?
                    string dataLoc = String.Format( "~/App_Data/{0}", filename );
                    string file = System.Web.HttpContext.Current.Server.MapPath( dataLoc );

                    cacheItem = File.ReadAllText( file );
                    //save in cache for future
                    HttpContext.Current.Cache[ cacheKeyName ] = cacheItem;
                }

            }
            catch ( Exception ex )
            {
                //LoggingHelper.LogError( ex, thisClassName + ".GetCacheItem( string cacheKeyName, string defaultValue ). Error retrieving item key: " + cacheKeyName );
                cacheItem = defaultValue;
            }
            return cacheItem;
        }//

        /// <summary>
        /// not sure if a stream can be cached, or if we want to actually use this
        /// </summary>
        /// <param name="cacheKeyName"></param>
        /// <param name="filename"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        //public static MemoryStream GetCacheItemAsStream( string cacheKeyName, string filename, string defaultValue )
        //{
        //    string cacheItem = defaultValue;
        //    MemoryStream ms = new MemoryStream();
        //    try
        //    {
        //        cacheItem = HttpContext.Current.Cache[ cacheKeyName ] as string;
                

        //        if ( string.IsNullOrEmpty( cacheItem ) )
        //        {
        //            //assuming keyname is same as file name in app_Data - or should the ext also be part of the key?
        //            string dataLoc = String.Format( "~/App_Data/{0}", filename );
        //            string item = System.Web.HttpContext.Current.Server.MapPath( dataLoc );

        //            cacheItem = File.ReadAllText( item );
        //            //save in cache for future
        //            HttpContext.Current.Cache[ cacheKeyName ] = cacheItem;

                    
        //            //either read all the time
        //            FileStream file = new FileStream( filename, FileMode.Create, FileAccess.Read );
        //            byte[] bytes = new byte[ file.Length ];
        //            file.Read( bytes, 0, ( int ) file.Length );
        //            ms.Write( bytes, 0, ( int ) file.Length );
        //            file.Close();
        //            ms.Close();
        //            //can a stream be cached?
        //            HttpContext.Current.Cache[ cacheKeyName + "Stream" ] = ms;
        //        }

        //        TextWriter tw = new StreamWriter( ms );

        //        tw.WriteLine( cacheItem );
        //        tw.Close();
        //        ms.Close();
                    

        //    }
        //    catch ( Exception ex )
        //    {
        //        //LoggingHelper.LogError( ex, thisClassName + ".GetCacheItem( string cacheKeyName, string defaultValue ). Error retrieving item key: " + cacheKeyName );
        //        cacheItem = defaultValue;
        //        ms = new MemoryStream();
        //    }
        //    return ms;
        //}//

        //public static AppItem GetCacheAppItem( string appItemCode )
        //{
        //    AppItem cacheItem;
        //    try
        //    {
        //        if ( HttpContext.Current.Cache[ appItemCode ] == null )
        //        {
        //            //retrieve from database ...
        //            cacheItem = new DAL.AppItemManager().GetByCode( appItemCode );
        //            if ( cacheItem == null )
        //            {
        //                cacheItem = new AppItem();
        //                cacheItem.Title = appItemCode + " was not found";
        //                cacheItem.Description = "System administration has been notified";
        //                cacheItem.IsValid = false;
        //                cacheItem.Message = "Missing appItemCode";
        //                //probably should send a message

        //            }
        //            else if ( !cacheItem.IsValid )
        //            {
        //                //invalid item return, probably has a reason in the Message

        //                //probably should send a message
        //            }

        //            //save in cache for future
        //            HttpContext.Current.Cache[ appItemCode ] = cacheItem;
        //        }
        //        else
        //        {
        //            cacheItem = HttpContext.Current.Cache[ appItemCode ] as AppItem;
        //        }

        //    }
        //    catch ( Exception ex )
        //    {
        //        LoggingHelper.LogError( ex, thisClassName + ".GetCacheAppItem( string appItemCode ). Error retrieving application item code: " + appItemCode );
        //        cacheItem = new AppItem();
        //        cacheItem.Title = "Error retrieving: " + appItemCode;
        //        cacheItem.Description = "System administration has been notified";
        //        cacheItem.IsValid = false;
        //        cacheItem.Message = ex.Message;
        //    }
        //    return cacheItem;
        //}//
        #endregion
    }
}