using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using System.Web.Script.Serialization;
using ILPathways.Services;

namespace ILPathways.Controls.Libraries
{
    public partial class LibrariesSearch : BaseUserControl
    {
        List<jsonLibraryFilter> currentFilters = new List<jsonLibraryFilter>();
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        public string useSubscribedLibraries { get; set; }
        public string filters { get; set; }
        public string userGUID { get; set; }

        /// <summary>
        /// If yes, only show libraries to which user is subscribed
        /// </summary>
        public string SubscribedLibrariesView
        {
            get
            {
                return txtSubscribedLibsView.Text;
            }
            set { txtSubscribedLibsView.Text = value; }
        }//

        protected void Page_Load( object sender, EventArgs e )
        {
            InitPage();
        }

        protected void InitPage()
        {
            filters = "var filters = [];";
            userGUID = "var userGUID = \"\";";
            useSubscribedLibraries = "var useSubscribedLibraries = false;";

            if ( IsUserAuthenticated() )
            {
                userGUID = "var userGUID = \"" + WebUser.RowId.ToString() + "\";";
                if ( SubscribedLibrariesView == "yes" )
                {
                    useSubscribedLibraries = "var useSubscribedLibraries = true;";
                }
            }

            ConstructFilters();
        }

        protected void ConstructFilters()
        {
            //These should probably be generated from database data or some similar source, but for now this should do
            currentFilters.Clear();

            //Date Range
            var dateRange = new jsonLibraryFilter();
            dateRange.header = "Date Range";
            dateRange.name = "dateRange";
            dateRange.type = "rbl";
            dateRange.tip = "Resource(s) added to the Library in:";
            dateRange.items.Add( new jsonLibraryFilterItem() { selected = false, text = "The last 7 Days", value = 1 } );
            dateRange.items.Add( new jsonLibraryFilterItem() { selected = false, text = "The last 30 Days", value = 2 } );
            dateRange.items.Add( new jsonLibraryFilterItem() { selected = false, text = "The last 6 Months", value = 3 } );
            dateRange.items.Add( new jsonLibraryFilterItem() { selected = false, text = "The last Year", value = 4 } );
            dateRange.items.Add( new jsonLibraryFilterItem() { selected = true, text = "Any timeframe", value = 5 } );
            currentFilters.Add( dateRange );

            //Library Types
            var libraryType = new jsonLibraryFilter();
            libraryType.header = "Library Type";
            libraryType.name = "libraryType";
            libraryType.type = "cbxl";
            libraryType.tip = "Show:";
            libraryType.items.Add( new jsonLibraryFilterItem() { selected = false, text = "Organization Libraries", value = 2 } );
            libraryType.items.Add( new jsonLibraryFilterItem() { selected = false, text = "User Libraries", value = 1 } );
            currentFilters.Add( libraryType );

            if ( IsUserAuthenticated() )
            {
                //Views
                var viewType = new jsonLibraryFilter();
                viewType.header = "Privacy";
                viewType.name = "view";
                viewType.type = "rbl";
                viewType.tip = "Show:";
                //viewType.items.Add( new jsonLibraryFilterItem() { selected = true, text = "All Libraries", value = 1 } );
                viewType.items.Add( new jsonLibraryFilterItem() { selected = true, text = "All Public Libraries", value = 2 } );
                //viewType.items.Add( new jsonLibraryFilterItem() { selected = false, text = "Only Private Libraries", value = 3 } );
                viewType.items.Add( new jsonLibraryFilterItem() { selected = false, text = "Only Libraries I'm a member of", value = 4 } );
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