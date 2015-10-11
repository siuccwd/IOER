using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net.Mail;
using System.Net;
using System.IO;

using ILPathways.Business;
using ILPathways.Utilities;
namespace IOER.AppCode.Classes
{
    /// <summary>
    /// ref: http://www.codeproject.com/Articles/16873/Sending-the-contents-of-a-webpage-with-images-as-a
    /// This class can send a webpage to a list of mail recipients
    /// Images on the webpage are embedded into the mail message
    /// 
    /// There are some restrictions that apply on the webpage
    /// * css is not handled
    /// * pages that use ajax will not work (obviously)
    /// * pages that use javascript to set images do not work
    /// * pages with iframes might trigger virus scanners
    /// * and so on....
    /// </summary>
    /// 
    public class WebpageMailer
    {
        MailMessage _mailMessage = new MailMessage();
        public MailMessage MailMessage
        {
            get
            {
                return _mailMessage;
            }
        }

        string _smptServerName;
        public string SmtpServerName
        {
            get { return _smptServerName; }
            set { _smptServerName = value; }
        }

        int _delay;
        /// <summary>
        /// The delay can be used to relieve stress from a webserver
        /// It is the number of milliseconds that the application waits before retrieving the next image
        /// </summary>
        public int Delay
        {
            get { return _delay; }
            set { _delay = value; }
        }

        /// <summary>
        /// Sends the contents of the uri as a mailmessage
        /// The mail message should be configured first with recipient atc...
        /// </summary>
        /// <param name="uri"></param>
        public void SendMailMessage( Uri uri )
        {
            // Retrieve the contents
            string htmlbody = GetBody( uri );

            // See what images there are to embed
            string modifiedbody;
            List<LinkedResource> foundResources;
            ExtractLinkedResources( uri, htmlbody, out modifiedbody, out foundResources );

            // Write the html to a memory stream
            MemoryStream stream = new MemoryStream();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes( modifiedbody );
            stream.Write( bytes, 0, bytes.Length );
            stream.Position = 0;

            // Configure the mail so it contains the html page
            EmailNotice notice = new EmailNotice();

            _mailMessage.Body = "This is a html mail - use an email client that can read it";
            notice.Message = "This is a html mail - use an email client that can read it";
            AlternateView altView = new AlternateView( stream, System.Net.Mime.MediaTypeNames.Text.Html );

            // Embed the images into the mail
            foreach ( LinkedResource linkedResource in foundResources )
            {
                altView.LinkedResources.Add( linkedResource );
            }
            _mailMessage.AlternateViews.Add( altView );

            // Send the mail
            //SmtpClient client = new SmtpClient( _smptServerName );
            SmtpClient client = new SmtpClient( UtilityManager.GetAppKeyValue( "smtpEmail" ) );
            client.Send( _mailMessage );
        }

        /// <summary>
        /// Gets the body of the mail message
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetBody( Uri uri )
        {
            HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create( uri );

            // Set some reasonable limits on resources used by this request
            request.MaximumAutomaticRedirections = 4;
            request.MaximumResponseHeadersLength = 4;
            // Set credentials to use for this request.
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();

            // Get the stream associated with the response.
            Stream receiveStream = response.GetResponseStream();

            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader( receiveStream, Encoding.UTF8 );

            string body = readStream.ReadToEnd();
            readStream.Close();

            response.Close();
            return body;
        }

        /// <summary>
        /// Retrieves an image to embed
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        Stream GetImageStream( Uri uri, out string contentType )
        {
            System.Threading.Thread.Sleep( _delay );

            HttpWebRequest request = ( HttpWebRequest ) WebRequest.Create( uri );

            // Set some reasonable limits on resources used by this request
            request.MaximumAutomaticRedirections = 4;
            request.MaximumResponseHeadersLength = 4;
            // Set credentials to use for this request.
            request.Credentials = CredentialCache.DefaultCredentials;
            HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();

            // Get the stream associated with the response.
            Stream receiveStream = response.GetResponseStream();

            BinaryReader binaryReader = new BinaryReader( receiveStream );
            MemoryStream memoryStream = new MemoryStream();
            try
            {
                while ( true )
                {
                    byte b = binaryReader.ReadByte(); // Not so efficient but it works...
                    memoryStream.WriteByte( b );
                }
            }
            catch ( EndOfStreamException )
            {
                memoryStream.Position = 0;
            }

            contentType = response.ContentType;
            response.Close();
            return memoryStream;
        }

        private void ExtractLinkedResources( Uri uri, string html, out string modifiedhtml, out List<LinkedResource> linkedResources )
        {
            modifiedhtml = html;
            linkedResources = new List<LinkedResource>();

            List<string> imageNames = ExtractImageNames( html );

            int imageID = 1;

            foreach ( string imageName in imageNames )
            {
                try
                {
                    // Deal with some escape characters that can occur in dynamic image url's
                    string workaroundImageName = imageName.Replace( "&amp;", "&" );

                    // Generate the uri to retrieve the image - usually an image path is relative to the page uri
                    Uri imageUri = new Uri( uri, workaroundImageName );

                    // Retrieve the image
                    string contentType;
                    Stream imageStream = GetImageStream( imageUri, out contentType );

                    // Fill the linked resource
                    LinkedResource data = new LinkedResource( imageStream );

                    // Determine a name and set the media type of the linked resource
                    string generatedName = null;
                    if ( contentType.ToLower().IndexOf( "image/gif" ) >= 0 )
                    {
                        data.ContentType.MediaType = System.Net.Mime.MediaTypeNames.Image.Gif;
                        generatedName = "image" + imageID.ToString() + ".gif";
                    }
                    else if ( contentType.ToLower().IndexOf( "image/jpeg" ) >= 0 )
                    {
                        data.ContentType.MediaType = System.Net.Mime.MediaTypeNames.Image.Jpeg;
                        generatedName = "image" + imageID.ToString() + ".jpeg";
                    }

                    // it is something that I don't handle yet
                    if ( generatedName == null )
                        continue;

                    // Generate the linked resource for the image being embedded
                    string generatedSrc = "cid:" + generatedName;
                    data.ContentType.Name = generatedName;
                    data.ContentId = generatedName;
                    data.ContentLink = new Uri( generatedSrc );
                    linkedResources.Add( data );

                    // Let the html refer to the linked resource
                    modifiedhtml = modifiedhtml.Replace( imageName, generatedSrc );
                }
                catch
                {
                    modifiedhtml = modifiedhtml.Replace( imageName, "#" );
                }

                imageID++;
            }
        }

        /// <summary>
        /// longest length first
        /// </summary>
        private class LengthComparer : IComparer<string>
        {
            public int Compare( string x, string y )
            {
                return -x.Length.CompareTo( y.Length );
            }
        }

        /// <summary>
        /// Sorts the list of strings so the longest names are in the start of the list
        /// When there is an image named "a.aspx?id=1" and another named "a.aspx?id=12", the list should start with "a.aspx?id=12"
        /// This guarantees that string replacement will not damage the names
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static private List<string> SortNoDuplicate( List<string> input )
        {
            List<string> result = new List<string>( input );
            IComparer<string> comparer = new LengthComparer();
            result.Sort( comparer );
            return result;
        }

        /// <summary>
        /// Optimistic search for image names in an html document
        /// The images found here will be embedded into the email
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private static List<string> ExtractImageNames( string html )
        {
            List<string> imagenames = new List<string>();
            string[] imageattributes = new string[] { "src=", "background=" };
            foreach ( string imageattribute in imageattributes )
            {
                int position = 0;
                while ( position < html.Length )
                {
                    int foundIndex = html.ToLower().IndexOf( imageattribute, position );
                    if ( foundIndex < 0 )
                    {
                        position = html.Length;
                    }
                    else
                    {
                        int valueStartIndex = foundIndex + imageattribute.Length + 1;
                        int foundIndexEnd = html.IndexOfAny( new char[] { '\"', ' ', '\'', '>' }, valueStartIndex );
                        if ( foundIndexEnd < 0 )
                        {
                            position = html.Length;
                        }
                        else
                        {
                            string relativeimagename = html.Substring( valueStartIndex, foundIndexEnd - valueStartIndex );
                            relativeimagename = relativeimagename.Trim( new char[] { '\"', ' ', '\'', '>' } );
                            if ( !imagenames.Contains( relativeimagename ) )
                            {
                                imagenames.Add( relativeimagename );
                            }
                            position = foundIndexEnd;
                        }
                    }
                }
            }
            return SortNoDuplicate( imagenames );
        }
    }
}
