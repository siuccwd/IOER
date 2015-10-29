using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPLibrary = IOER.Library;

namespace IOER.Controls.Content
{
	public partial class ContentSearch2 : ILPLibrary.BaseUserControl
	{
		#region Properties

		protected string CustomFilter
		{
			get { return this.txtCustomFilter.Text; }
			set { this.txtCustomFilter.Text = value; }
		}//

		protected string CustomTitle
		{
			get
			{
				if ( ViewState[ "CustomTitle" ] == null )
					ViewState[ "CustomTitle" ] = "";

				return ViewState[ "CustomTitle" ].ToString();
			}
			set { ViewState[ "CustomTitle" ] = value; }
		}//
		
		/// <summary>
		/// Get/Set if is my authored view
		/// </summary>
		public bool IsMyAuthoredView
		{
			get
			{
				return _isMyAuthoredView;
			}
			set { this._isMyAuthoredView = value; }
		}//
		private bool _isMyAuthoredView = false;

		public bool IsBlindSearch
		{
			get
			{
				return _isBlindSearch;
			}
			set { this._isBlindSearch = value; }
		}//
		private bool _isBlindSearch = false;

		private string _district = null;
		protected string District
		{
			get
			{
				try
				{
					if ( _district == null )
					{
						if ( Page.RouteData.Values.Count > 0 )
							_district = Page.RouteData.Values[ "DistrictName" ].ToString();
						else
							_district = "";
					}
				}
				catch
				{
					_district = "";
				}

				return _district;
			}
		}

		private string _authorSearch = null;
		protected string AuthorSearch
		{
			get
			{
				try
				{
					if ( _authorSearch == null )
					{
						if ( Page.RouteData.Values.Count > 0 )
							_authorSearch = Page.RouteData.Values[ "Author" ].ToString();
						else
							_authorSearch = "";
					}
				}
				catch
				{
					_authorSearch = "";
				}
				return _authorSearch;
			}
		}

		public bool IsSiteAdmin
		{
			get
			{
				return _isSiteAdmin;
			}
			set { this._isSiteAdmin = value; }
		}//
		private bool _isSiteAdmin = false;
		#endregion
		protected void Page_Load( object sender, EventArgs e )
		{

			if ( IsMyAuthoredView )
			{
				if ( this.IsUserAuthenticated() == false )
				{
					//error
					SetConsoleErrorMessage( "Invalid request, must be signed in to use this function.<br/>Either that or be sure to hide stuff" );
					//pnlSearch.Visible = false;
					return;
				}
				else
				{
					//set default filters for "My" view
				}

			}
			else
			{
				if (!Page.IsPostBack)
					IsBlindSearch = true;
			}

			if ( this.IsUserAuthenticated() && WebUser.TopAuthorization == 2)
			{
				IsSiteAdmin = true;
			}
		}
	}
}