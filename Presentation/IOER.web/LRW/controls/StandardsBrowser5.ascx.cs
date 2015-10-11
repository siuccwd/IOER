using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IOER.LRW.controls
{
    public partial class StandardsBrowser5 : System.Web.UI.UserControl
    {
        public string mode { get; set; }
        public string preselectedBody { get; set; }
        public string preselectedGrade { get; set; }
        public bool isWidget { get; set; }

        protected void Page_Load( object sender, EventArgs e )
        {
            //If available, use the mode provided by the request
            if ( ( mode == null || mode == "" ) && Request.QueryString[ "standardsbrowsermode" ] != null )
            {
                switch ( Request.QueryString[ "standardsbrowsermode" ].ToLower() )
                {
                    case "search":
                        mode = "search";
                        break;
                    case "browse":
                        mode = "browse";
                        break;
                    default:
                        mode = "browse";
                        break;
                }
            }
            //Otherwise default to browse
            if ( mode == null || mode == "" ) { mode = "browse"; }

            //If available, preselect the standard body
            if ( ( preselectedBody == null || preselectedBody == "" ) && Request.QueryString[ "preselectedbody" ] != null )
            {
                switch ( Request.QueryString[ "preselectedBody" ].ToLower() )
                {
                    case "ccssmath":
                        preselectedBody = "jsonMath";
                        break;
                    case "ccssela":
                        preselectedBody = "jsonELA";
                        break;
                    case "ngss":
                        preselectedBody = "jsonNGSS";
                        break;
                    case "ilsocialscience":
                        preselectedBody = "jsonILSocialScience";
                        break;
                    case "ilfinearts":
                        preselectedBody = "jsonILFineArts";
                        break;
                    case "ilphysicaldevelopment":
                        preselectedBody = "jsonILPhysicalDevelopment";
                        break;
                    default:
                        break;
                }
            }

            //If available, preselect the grade
            if ( ( preselectedGrade == null || preselectedGrade == "" ) && Request.QueryString[ "preselectedgrade" ] != null )
            {
                var tester = Request.QueryString[ "preselectedGrade" ].ToLower();
                var validOptions = new string[] { "K", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
                if ( validOptions.Contains<string>( tester ) )
                {
                    preselectedGrade = tester;
                }
            }
        }
    }
}