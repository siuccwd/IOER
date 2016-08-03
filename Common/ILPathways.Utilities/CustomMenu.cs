using System;
using System.Collections;
using ILPathways.Business;

namespace ILPathways.Utilities
{
	/// <summary>
	/// Class for formatting a custom menu
	/// </summary>
	[Serializable]
	public class CustomMenu {

		/// <summary>
		/// Menu identifier
		/// </summary>
		public string MenuId
		{
			get { return _menuId; }
			set { _menuId = value; }
		}
		///<exclude/>
		private string _menuId = "";

		/// <summary>
		/// Menu name
		/// </summary>
		public string MenuName
		{
			get { return _menuName; }
			set { _menuName = value; }
		}
		///<exclude/>
		private string _menuName = "";

		/// <summary>
		/// Menu Title
		/// </summary>
		public string MenuTitle {
			get { return _menuTitle; }
			set { _menuTitle = value; }
		}
		///<exclude/>
		private string _menuTitle = "";

		/// <summary>
		/// Set to true to show title for menu
		/// </summary>
		public bool ShowMenuTitle
		{
			get { return _showMenuTitle; }
			set { _showMenuTitle = value; }
		}
		///<exclude/>
		private bool _showMenuTitle = true;		

		/// <summary>
		/// Main URL for the Menu
		/// </summary>
		public string MenuLink{
			get { return _menuLink; }
			set { _menuLink = value; }
		}
		///<exclude/>
		private string _menuLink= "";
		/// <summary>
		/// The parent channel or menu for the current custom menu list
		/// </summary>
		public string ParentMenu {
			get { return _parentMenu; }
			set { _parentMenu = value; }
		}
		///<exclude/>
		private string _parentMenu="";

		/// <summary>
		/// Indicate if the full url is the menu name  
		/// True - use url
		/// False - use parent channel name
		/// </summary>
		public bool UsingUrlAsMenuName
		{
			get { return _usingUrlAsMenuName; }
			set { _usingUrlAsMenuName = value; }
		}
		///<exclude/>
		private bool _usingUrlAsMenuName = false;


		/// <summary>
		/// Get/Set for _addRemoveMenuItem - true means add a remove menu item at bottom of menu
		/// </summary>
		public bool AddRemoveMenuItem
		{
			get { return _addRemoveMenuItem; }
			set { _addRemoveMenuItem = value; }
		}
		///<exclude/>
		private bool _addRemoveMenuItem = true;		


		/// <summary>
		/// Menu Collection
		/// </summary>
		public ArrayList MenuItems{
			get { return _menuItems; }
			set { _menuItems = value; }
		}
		///<exclude/>
		private ArrayList _menuItems = new ArrayList();
		
		/// <summary>
		/// Training Menu Collection - NOT USED - set to private
		/// </summary>
		private ArrayList TrainingMenuList{
			get { return _trainMenuList; }
			set { _trainMenuList = value; }
		}
		///<exclude/>
		private ArrayList _trainMenuList = new ArrayList();

		/// <summary>
		/// Occupation ID
		/// </summary>
		public string OccId{
			get { return _occId; }
			set { _occId = value; }
		}
		///<exclude/>
		private string _occId = "";

		/// <summary>
		/// Job/Career area id
		/// </summary>
		public string JobId{
			get { return _jobId; }
			set { _jobId = value; }
		}
		///<exclude/>
		private string _jobId = "";
		/// <summary>
		/// Program id
		/// </summary>
		public string ProgId{
			get { return _progId; }
			set { _progId = value; }
		}
		///<exclude/>
		private string _progId = "";

		/// <summary>
		/// MenuTag - used to determine if menu item should be styled as the "current" item (by checking against Request Parameter)
		/// </summary>
		public string MenuTag
		{
			get { return _menuTag; }
			set { _menuTag = value; }
		}
		///<exclude/>
		private string _menuTag = "";

		///<exclude/>
		private string _menuTitleStyle = "leftRailNavItem2";
		private string _menuTitleStyleImg = "leftRailNavItem2Img";
		private string _menuStyle2 = "leftRailNavItem3";
		private string _menuStyle2Img = "leftRailNavItem3Img";

		/// <summary>
		/// Default constructor
		/// </summary>
		public CustomMenu() {
			//
			// TODO: Add constructor logic here
			//
		}

		

		/// <summary>
		/// Appends the custom menu to a passed ArrayList
		/// </summary>
		/// <param name="list">ArrayList to contain the custom menu</param>
		/// <param name="itemCntr">Counter used to tag the menu items</param>
		/// <returns>Updated ArrayList</returns>
		public int AppendMenu(ArrayList list, int itemCntr) {
			
			DataItem menuItem = new DataItem();

			try {
				//Add title
				itemCntr ++;
				DataItem titleItem = new DataItem();
                titleItem.Title = this.MenuTitle;
				titleItem.Url = this.MenuLink;
				titleItem.param1 = _menuTitleStyle;
				titleItem.param2 = _menuTitleStyleImg;
				titleItem.param4 = "nav" + itemCntr.ToString();
				titleItem.param3 = "NavDisabled";
				list.Add(titleItem);

				System.Collections.IEnumerator myEnumerator = this.MenuItems.GetEnumerator();

				while ( myEnumerator.MoveNext() ) {
					menuItem = myEnumerator.Current as DataItem;

					itemCntr ++;
					DataItem dataItem = new DataItem();
                    dataItem.Title = menuItem.Title;
					if (menuItem.Url == "#") {
						dataItem.Url = menuItem.Url;
						dataItem.OnClick = menuItem.OnClick;
					} else {
						dataItem.Url = menuItem.Url;
						dataItem.OnClick = "";
					}
					dataItem.param1 = _menuStyle2;
					dataItem.param2 = _menuStyle2Img;
					dataItem.param4 = "nav"+itemCntr.ToString();
					dataItem.param3 = "NavDisabled";

					list.Add(dataItem);
				} // while

			} catch (Exception ex) {
				LoggingHelper.LogError("CustomMenu.AppendMenu:"+ex.Message);
			}
			return itemCntr;
		} //
		
		/// <summary>
		/// Format the menu as a list
		/// </summary>
		/// <returns></returns>
		public string AsList() {
			string menuList="";
			DataItem menuItem = new DataItem();

			try {
				//Add title

				menuList += FormatMenuItem( this.MenuTitle,this.MenuLink, _menuTitleStyle);

				System.Collections.IEnumerator myEnumerator = this.MenuItems.GetEnumerator();

				while ( myEnumerator.MoveNext() ) 
				{
					menuItem = myEnumerator.Current as DataItem;
					
					menuList += FormatMenuItem( menuItem, _menuStyle2);

				} // while

			} catch (Exception ex) {
				LoggingHelper.LogError("CustomMenu.AsList:"+ex.Message);
			}
			return menuList;
		} //

		private string FormatMenuItem(string display, string url, string style) {
			return "<br><a href='" + url + "' class='" + style + "' >" + display + "</a>";

		} //
		private string FormatMenuItem(DataItem menuItem, string style) 
		{
			//string item = "";
			string onClick = "";

            if ( menuItem.Url == "#" )
            {
				if (menuItem.OnClick.Length > 0)
					onClick = "onclick=\"" + menuItem.OnClick + "\"";
			}

			return "<br><a href='" + menuItem.Url + onClick + "' class='" + style + "' >" + menuItem.Title + "</a>";

		} //
	}
}
