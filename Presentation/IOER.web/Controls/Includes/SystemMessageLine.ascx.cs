using System;
using System.Globalization;
using System.Configuration;
using System.Resources;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using IOER.Library;
using ILPathways.Utilities;
using IOER.classes;
using IPB = ILPathways.Business;

namespace IOER.Includes
{

    public partial class SystemMessageLine : BaseUserControl
    {
        public string SystemMessageCss = "";
        public string SystemMessageText = "";


        /// <summary>
        /// Reset the system message
        /// </summary>	
        protected void Page_Load( object sender, EventArgs e )
        {
            //systemMessageDiv.Style[ "display" ] = "none";
            //systemMessageDiv.InnerHtml = "";
            SystemMessageCss = "invisible";
        }

        protected void Page_PreRender( object sender, EventArgs e )
        {
            //check for form messages
            IPB.FormMessage message = new IPB.FormMessage();
            message = ( IPB.FormMessage ) Session[ SessionManager.SYSTEM_CONSOLE_MESSAGE ];
            if ( message != null )
            {
                //systemMessageDiv.InnerHtml = message.Text;
                if ( message.Text.IndexOf( "System.Threading.ThreadAbortException" ) > -1 )
                {
                    //skip
                    LoggingHelper.DoTrace( "includes_SystemMessageLine.Page_PreRender - Skipping Message: System.Threading.ThreadAbortException !!\r\n " + message.Text );
                }
                else
                {
                    SystemMessageText = message.Text;
                    SystemMessageCss = message.CssClass;
                }

                //now clear out message
                Session.Remove( SessionManager.SYSTEM_CONSOLE_MESSAGE );
            }

        }
    }
}