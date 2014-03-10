/*********************************************************************************
* Author: Michael Parsons
*         Based on an article by Francesco Balena - Code Architects
*					http://www.cs2themax.com/showcontent.aspx?id=5f606582-4626-4c6f-a6e9-6f952f31491a
* Date: Sep. 2, 2004
* Assembly: utilityClasses
* Description: 
* Notes:
* 
* 
* Copyright 2004, DCEO All rights reserved.
*********************************************************************************/

using System;
using System.Web.UI; 
using System.Web.UI.WebControls;

namespace ILPathways.Utilities
{
	/// <summary> 
	/// Utility class for a ASP.NET page 
	/// Used to open a new browser window while keeping the current window open.
	/// Can be attached to a control or to the page load (for example on a post back after an update!
	/// </summary> 
	/// <remarks>
	///  Sample uses:
	/// The constructor of the BrowserWindow class lets you define the target URL and the size of the secondary window. 
	/// All other fields must be set by plain assignments: 
	/// BrowserWindow win = new BrowserWindow("www.vb2themax.com", 500, 400);
	/// win.Resizable = true;
	/// win.ScrollBars = true;
	/// win.StatusBar = true;
	/// 
	/// The BrowserWindow class exposes two methods: AttachToControl lets you attach the window.open method to the 
	/// onClick client-side event of a Button, LinkButton, or any other WebControl-derived control. 
	/// In this case the open method runs exclusively on the client and no postpack occurs. You typically invoke this 
	/// method from inside the Page_Load event handler: 
	/// <code>
	/// 			private void Page_Load(object sender, EventArgs e) {
	/// 				if ( ! this.IsPostBack ) {
	/// 					BrowserWindow win = new BrowserWindow("www.vb2themax.com", 500, 400);
	/// 					win.Resizable = true;
	/// 					win.ScrollBars = true;
	/// 					win.StatusBar = true;
	/// 					win.AttachToControl(btnOpenWindow);
	/// 				}
	/// 			}
	/// </code>
	/// The second method, AttachToPageLoad, lets you open the secondary window after a page postback. 
	/// This method is useful, for example, when you need to update some controls on the main page and then open a secondary window. 
	/// Here's an example of how to use this method: 
	/// <code>
	///				private void btnOpen_Click(object sender, System.EventArgs e) {
	/// 				// display a secondary window when current page is sent to the browser
	/// 				BrowserWindow win = new BrowserWindow("www.vb2themax.com", 500, 300);
	/// 				win.AttachToPageLoad(this);
	/// 			}
	/// </code>
	/// </remarks>

public class BrowserWindow
{
    public string  URL;        // the URL of the page to be displayed
    public string  Name;       // the name of the new window
    public int Height;         // the height of the window
    public int Width;          // the width of the window
    public bool StatusBar;     // true to display a status bar
    public bool Resizable;     // true to make the window resizable
    public bool ScrollBars;    // true to display scrollbars
    public bool ToolBar;       // true to display a toolbar
    public bool Location;      // true to display the address bar
    public bool MenuBar;       // true to display the menu bar
    public bool CopyHistory;   // true to copy current history to the new window

		/// <summary>
		/// BrowserWindow constructor takes url and size
		/// </summary>
		/// <param name="url">Starting URL for window - without the http://</param>
		/// <param name="width">Window Width</param>
		/// <param name="height">Window Height</param>
    public BrowserWindow(string url, int width, int height)
    {
			this.URL = url;
			this.Width = width;
			this.Height = height;
    } //

		/// <summary>
		///  BrowserWindow constructor set url, size, and interface defaults
		/// </summary>
		/// <param name="url">Starting URL for window - without the http://</param>
		/// <param name="width">Window Width</param>
		/// <param name="height">Window Height</param>
		/// <param name="interfaceDefaults">Set to true to show common browser interface items</param>
		public BrowserWindow( string url, int width, int height, bool interfaceDefaults )
		{
			this.URL = url;
			this.Width = width;
			this.Height = height;

			this.SetInterfaceDefaults( interfaceDefaults );
		}

		/// <summary>
		///  BrowserWindow constructor set host, url, and size
		/// </summary>
		/// <param name="host">Web Server Host</param>
		/// <param name="url">Starting URL for window - without the http://</param>
		/// <param name="width">Window Width</param>
		/// <param name="height">Window Height</param>
		public BrowserWindow(string host, string url, int width, int height) {
			char [] arr = {'/', '\\'};

			//check for terminating slashes
			string address = host.Trim();
			address = address.TrimEnd(arr);

			//if (address.EndsWith("/") > 0  || address.EndsWith("\\") > 0) address = address.TrimEnd("/", "\\");
			this.URL = address + "/" + url.TrimStart(arr);

			this.Width = width;
			this.Height = height;
		}

		/// <summary>
		/// Set the default value for common browser interface elements
		/// 		- Resizable, Menubar, Location, Toolbar, Scrollbars, Status Bar
		/// </summary>
		/// <param name="interfaceDefaults">Use true to show items or false to hide</param>
		public void SetInterfaceDefaults( bool interfaceDefaults )
		{
			Resizable = interfaceDefaults;
			MenuBar = interfaceDefaults;
			Location = interfaceDefaults;
			ToolBar = interfaceDefaults;
			ScrollBars = interfaceDefaults;
			StatusBar = interfaceDefaults;
		}

		/// <summary>
		/// attach the window.open method to a control
		/// </summary>
		/// <param name="ctrl"></param>
    public void AttachToControl(WebControl ctrl)
    {
        // add the attribute to the control
        ctrl.Attributes.Add("onClick", "javascript:" + GetScriptCode());
    }

		/// <summary>
		/// attach to a Page's Load event
		/// </summary>
		/// <param name="page"></param>
    public void AttachToPageLoad( Page page)
    {
        // register as the startup script, using a unique key name
        string  key = this.GetHashCode().ToString();
        page.RegisterStartupScript("open", "<script language=javascript>" +
           this.GetScriptCode() + "</script>");
    }


		/// <summary>
		/// attach to a Control's Load event
		/// </summary>
		/// <param name="control"></param>
		public void AttachToPageLoad( Control control) {
			// register as the startup script, using a unique key name
			string  key = this.GetHashCode().ToString();

			control.Page.RegisterStartupScript("open", "<script language=javascript>" +
			this.GetScriptCode() + "</script>");
		}

		/// <summary>
		/// get the javascript code that opens the window
		/// </summary>
		/// <returns>Javascript to open a window</returns>
		public string GetScriptCode()
    {
        // the URL must be preceded by HTTP://
        string url = this.URL;
				if ( !url.ToLower().StartsWith( "http" ) )
				{
					//TODO guess if the domain is missing

					url = "http://" + url;

				}
        // the window name can be null or must be encloses between single quotes
        string  winName = "null";
        if ( this.Name != null )
            winName = "'" + this.Name + "'";
        // build the windows feature argument
        string features = string .Format("height={0}, width={1}, status={2}, resizable={3"
           + "}, scrollbars={4}, menubar={5}, toolbar={6}, location={7}, copyhistory={8}",
            this.Height, this.Width, YesNo(this.StatusBar), YesNo(this.Resizable), YesNo(
            this.ScrollBars), YesNo(this.MenuBar), YesNo(this.ToolBar), YesNo(
            this.Location), YesNo(this.CopyHistory));
        // build and return the script code
        return string .Format("window.open('{0}', {1}, '{2}');", url, winName, features);
    }

		/// <summary>
		/// helper routine that returns "no" or "yes"
		/// </summary>
		/// <param name="val">Boolean value</param>
		/// <returns>String yes/no</returns>
    private string YesNo( bool val )
    {
        return val ? "yes" : "no" ;
    }
    
	} //class
} //namespace
