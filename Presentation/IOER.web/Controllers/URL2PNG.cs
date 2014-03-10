using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Text;
using System.Security.Cryptography;

namespace ILPathways.Controllers
{
    public static class URL2PNG
    {
        private static int defaultThumbnailWidth = 400;
        private static int defaultViewportWidth = 1366;
        private static int defaultViewportHeight = 768;

        public static string GetThumbnail( string url )
        {
            return GetThumbnail( url, defaultThumbnailWidth, defaultViewportWidth, defaultViewportHeight, false );
        }

        public static string GetThumbnail( string url, int thumbnailWidth )
        {
            return GetThumbnail( url, thumbnailWidth, defaultViewportWidth, defaultViewportHeight, false );
        }

        public static string GetThumbnail( string url, int viewportWidth, int viewportHeight )
        {
            return GetThumbnail( url, defaultThumbnailWidth, viewportWidth, viewportHeight, false );
        }

        public static string GetThumbnail( string url, int thumbnailWidth, int viewportWidth, int viewportHeight )
        {
            return GetThumbnail( url, thumbnailWidth, viewportWidth, viewportHeight, false );
        }

        public static string GetThumbnail( string url, bool useFullPage )
        {
            return GetThumbnail( url, defaultThumbnailWidth, defaultViewportWidth, defaultViewportHeight, useFullPage );
        }

        public static string GetThumbnail( string url, bool useFullPage, int thumbnailWidth )
        {
            return GetThumbnail( url, thumbnailWidth, defaultViewportWidth, defaultViewportHeight, useFullPage );
        }

        public static string GetThumbnail( string url, int thumbnailWidth, int viewportWidth, int viewportHeight, bool useFullPage )
        {
            string secretKey = "SC41DDD8646C80";
            string apiKey = "P5170300BD362E";

            //Select options
            string options = "";
            if ( useFullPage )
            {
                options = "&fullpage=true&thumbnail_max_width=" + thumbnailWidth;
            }
            else
            {
                options = "&viewport=" + viewportWidth + "x" + viewportHeight + "&thumbnail_max_width=" + thumbnailWidth;
            }

            //Construct the request
            string request = "?url=" + HttpUtility.UrlEncode( url ) + options;

            //Construct the hash
            System.Security.Cryptography.MD5CryptoServiceProvider md5Provider = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hash = md5Provider.ComputeHash( Encoding.Default.GetBytes( request + secretKey ) );
            StringBuilder builder = new StringBuilder();
            foreach ( byte a in hash )
            {
                if ( a < 16 )
                {
                    builder.Append( "0" + a.ToString( "x" ) );
                }
                else
                {
                    builder.AppendFormat( a.ToString( "x" ) );
                }
            }

            string publicToken = builder.ToString();

            //Encode the URL
            string encodedURL = "?url=" + HttpUtility.UrlEncode( url );

            //Create the URL that the client sees
            return "http://beta.url2png.com/v6/" + apiKey + "/" + publicToken + "/" + "png" + "/" + encodedURL + options;
        }

    }
}