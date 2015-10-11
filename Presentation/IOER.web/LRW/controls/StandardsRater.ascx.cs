using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;

namespace IOER.LRW.controls
{
    public partial class StandardsRater : System.Web.UI.UserControl
    {
        public string userGUID { get; set; }
        public int resourceIntID { get; set; }

        public string contextData { get; set; }

        protected void Page_Load( object sender, EventArgs e )
        {
            ContextData data = new ContextData();
            data.guid = userGUID;
            data.intID = resourceIntID;

            contextData = new JavaScriptSerializer().Serialize( data );

            //TODO: handle a user that isn't logged in
        }

        public class ContextData
        {
            public string guid;
            public int intID;
        }
    }
}