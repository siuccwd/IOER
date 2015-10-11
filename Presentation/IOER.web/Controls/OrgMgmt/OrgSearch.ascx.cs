using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Common;
using IOER.Library;
using IOER.Services;
using Isle.BizServices;

namespace IOER.Controls.OrgMgmt
{
    /// <summary>
    /// Search for organizations
    /// </summary>
    public partial class OrgSearch : BaseUserControl
    {

        List<jsonLibraryFilter> currentFilters = new List<jsonLibraryFilter>();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        
        public string filters { get; set; }
        public string userGUID { get; set; }

        /// <summary>
        /// handle page load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load( object sender, EventArgs e )
        {
            InitPage();
        }

        protected void InitPage()
        {
            filters = "var filters = [];";
            userGUID = "var userGUID = \"\";";
            
            if ( IsUserAuthenticated() )
            {
                userGUID = "var userGUID = \"" + WebUser.RowId.ToString() + "\";";
            }

            ConstructFilters();
        }

        protected void ConstructFilters()
        {
            //These should probably be generated from database data or some similar source, but for now this should do
            currentFilters.Clear();


            //Library Types
            var orgType = new jsonLibraryFilter();
            orgType.header = "Organization Type";
            orgType.name = "organizationType";
            orgType.type = "cbxl";
            orgType.tip = "Show:";

            List<CodeItem> list = OrganizationBizService.OrgType_Select();
            foreach ( CodeItem item in list )
            {
                orgType.items.Add( new jsonLibraryFilterItem() { selected = false, text = item.Title, value = item.Id} );
            }
            currentFilters.Add( orgType );

            if ( IsUserAuthenticated() )
            {
                //Views
                var viewType = new jsonLibraryFilter();
                viewType.header = "Privacy";
                viewType.name = "view";
                viewType.type = "rbl";
                viewType.tip = "Show:";
                viewType.items.Add( new jsonLibraryFilterItem() { selected = true, text = "All Public Organizations", value = 2 } );

                viewType.items.Add( new jsonLibraryFilterItem() { selected = false, text = "Only Organizations I'm a member of", value = 4 } );
                currentFilters.Add( viewType );
            }

            filters = "var filters = " + serializer.Serialize( currentFilters ) + ";";
        }

        #region subclasses

        public class jsonLibraryFilter
        {
            public jsonLibraryFilter()
            {
                items = new List<jsonLibraryFilterItem>();
            }
            public string tip { get; set; } //brief text describing the filter
            public string header { get; set; } //presentation title
            public string name { get; set; } //code-readable name
            public string type { get; set; } //Tells the code to render as a radio button list or checkbox list
            public List<jsonLibraryFilterItem> items { get; set; }
        }
        public class jsonLibraryFilterItem
        {
            public string text { get; set; }
            public int value { get; set; }
            public bool selected { get; set; }
        }

        #endregion
    }
}