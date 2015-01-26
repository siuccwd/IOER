using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Newtonsoft.Json;

using ILPathways.Utilities;

namespace ILPathways.Pages
{
    public partial class GooruSearch : System.Web.UI.Page
    {
        private string API_Key = "960a9175-eaa7-453f-ba03-ecd07e1f1afc";
        public string gooruResourcesUrl = "";
        public string gooruCollectionsUrl = "";

        public string gooruSessionId
        {
            get
            {
                if ( Session[ "gooruSessionId" ] == null )
                    Session[ "gooruSessionId" ] = "";
                return Session[ "gooruSessionId" ].ToString();
            }
            set { Session[ "gooruSessionId" ] = value; }
        }

        /*
         * to generate a token:
         * curl -u : -H "Content-Type: application/json" -X POST -d "{}" http://www.goorulearning.org/gooruapi/rest/v2/account/loginas/anonymous?apiKey=960a9175-eaa7-453f-ba03-ecd07e1f1afc
         * output
         * {"createdOn":1418659462811,"dateOfBirth":null,"restEndPoint":"http://www.goorulearning.org/gooruapi/rest/",
         * "token":"a8a0a4aa-555f-4060-bad5-cefdd6690b80",
         * "user":{"accountCreatedType":null,"accountTypeId":3,"active":null,"confirmStatus":1,"c
reatedOn":"2013-03-14 07:37:15.0","customFields":[{"category":"user_meta","class":"org.ednovo.gooru.core.api.model.Party
CustomField","optionalKey":"show_profile_page","optionalValue":"false","partyUid":"ANONYMOUS"}],"emailId":"","firstName"
:"Guest","gooruUId":"ANONYMOUS","lastName":"","loginType":null,"meta":null,"organizationName":"Gooru","partyUid":"ANONYM
OUS","profileImageUrl":"http://profile-demo.s3.amazonaws.com/profile-prod/ANONYMOUS.png","token":null,"userRole":null,"u
serRoleSetString":"ANONYMOUS","username":"Guest4678","usernameDisplay":"Guest4678","viewFlag":12},"userRole":null}
         * 
         */
        protected void Page_Load( object sender, EventArgs e )
        {
            if ( gooruSessionId == "" )
            {
                gooruSessionId = GetSessionToken();
            }

            gooruResourcesUrl = "var gooruResourcesUrl = \"" + string.Format( gooruTemplateResourceUrl.Text, gooruSessionId ) + "\";";
            gooruCollectionsUrl = "var gooruCollectionsUrl = \"" + string.Format( gooruTemplateCollectionsUrl.Text, gooruSessionId ) + "\";";
        }

        public string GetSessionToken()
        {
            string token = "";
            string url = LRWarehouse.DAL.BaseDataManager.GetAppKeyValue( "gooruApiUrl" );  // "http://www.goorulearning.org/gooruapi/rest/v2/account/loginas/anonymous?apiKey=960a9175-eaa7-453f-ba03-ecd07e1f1afc";
            if ( string.IsNullOrWhiteSpace( url ) )
            {
              url = gooruApiUrl.Text; //Couldn't use this as a second argument of the GetAppKeyValue method because it was throwing uninstantiated object reference errors
            }
          
            // Create the web request  
            HttpWebRequest request = WebRequest.Create( url ) as HttpWebRequest;
            request.Method = "POST";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36";
            string postData = "{}";
            byte[] byteArray = Encoding.UTF8.GetBytes( postData );
            
            request.ContentType = "application/json";
            // Set the ContentLength property of the WebRequest.
            request.ContentLength = byteArray.Length;

            try
            {
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write( byteArray, 0, byteArray.Length );
                // Close the Stream object.
                dataStream.Close();

                // Get the response.
                WebResponse response1 = request.GetResponse();
                // Display the status.
                //Console.WriteLine( ( ( HttpWebResponse ) response1 ).StatusDescription );
                // Get the stream containing content returned by the server.
                dataStream = response1.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader1 = new StreamReader( dataStream );
                // Read the content.
                string json = reader1.ReadToEnd();

                //var jo = JObject.Parse( json );
                //extract token
                int pos = json.IndexOf( "\"token\":" );
                if ( pos > 0 )
                {
                    int endPos = json.IndexOf( "\",", pos + 8 );
                    string part1 = json.Substring( pos + 8, endPos - ( pos + 8 ) );
                    string part2 = json.Substring( pos + 8, 38 );

                    token = part1.Replace( "\"", "" );
                    
                }
                // Clean up the streams.
                reader1.Close();
                dataStream.Close();
                response1.Close();

            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "GooruSearch.GetSessionToken" );
            }

            return token;
        }

    }
}