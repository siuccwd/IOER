using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Common;
using ILPathways.Utilities;
using IOER.Library;
using Ibus = ILPathways.Business;
using MyManager = Isle.BizServices.OrganizationBizService;
using AcctManager = Isle.BizServices.AccountServices;

namespace IOER.Organizations
{
	public partial class Import : BaseAppPage
	{
		const string thisClassName = "OrganizationsImport";
		MyManager myManager = new MyManager();
		AcctManager acctMgr = new AcctManager();

		#region Properties

		public int LastOrgId
		{
			get
			{
				if ( Session[ "LastOrgId" ] == null )
					Session[ "LastOrgId" ] = "0";

				return Int32.Parse( Session[ "LastOrgId" ].ToString() );
			}
			set { Session[ "LastOrgId" ] = value.ToString(); }
		}

		#endregion

		protected void Page_Load( object sender, EventArgs e )
		{
			if ( IsUserAuthenticated() == false )
			{
				this.SetConsoleErrorMessage( "Error - you must be logged in and authorized to use this function." );
				return;
			}
			if (Page.IsPostBack == false)
				Initialize();
		}

		private void Initialize()
		{
			LastOrgId = 0;
			Ibus.Organization org = new Ibus.Organization();
			//TODO - add checks to ensure user has access to the entered orgId
			//actually add guid version
			int recordId = GetRequestKeyValue( "orgId", 0 );
			string rowId = GetRequestKeyValue( "rid", "" );
			if ( recordId > 0 )
			{
				org = this.Get( recordId );
			}
			else if ( rowId.Length == 36 )
			{
				org = Get( rowId );
			}
			else
			{
				this.SetConsoleErrorMessage( "Error - a valid organization identifier has not been provided." );
				return;
			}

			if ( org != null || org.Id > 0 )
			{
				//ensure user has rights

				importPanel.Visible = true;
				memberImport.LastOrgId = org.Id;
				memberImport.InitializeImport();
			}
		}
		public Ibus.Organization Get( int recId )
		{
			Ibus.Organization entity = new Ibus.Organization();
			try
			{
				//get record
				entity = MyManager.EFGet( recId );

				if ( entity == null || entity.IsValid == false )
				{
					this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );

				}
				else
				{
					litOrgTitle.Text = entity.Name;
					this.memberImport.LastOrgId = entity.Id;
				}

			}
			catch ( System.Exception ex )
			{
				//Action??		- display message and close form??	
				LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
				this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

			}
			return entity;
		}	// End method

		public Ibus.Organization Get( string recId )
		{
			Ibus.Organization entity = new Ibus.Organization();
			try
			{
				//get record
				entity = MyManager.EFGetByRowId( recId );

				if ( entity == null || entity.IsValid == false )
				{
					this.SetConsoleErrorMessage( "Sorry the requested record does not exist" );

				}
				else
				{
					litOrgTitle.Text = entity.Name;
					this.memberImport.LastOrgId = entity.Id;
				}

			}
			catch ( System.Exception ex )
			{
				//Action??		- display message and close form??	
				LoggingHelper.LogError( ex, thisClassName + ".Get() - Unexpected error encountered" );
				this.SetConsoleErrorMessage( "Unexpected error encountered - Close this form and try again. (System Admin has been notified)<br/>" + ex.ToString() );

			}
			return entity;
		}	// End method

		protected void Page_PreRender( object sender, EventArgs e )
		{

			try
			{
		

				//check for change in orgId
				if ( LastOrgId == 0 )
				{
					SetConsoleErrorMessage( "Error - an organization has not be provided. Import is not possible." );
				}
				else if ( LastOrgId > 0 )
				{
					

				}
			}
			catch
			{
				//no action
			}

		}//
	}
}