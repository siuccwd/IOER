using System;
using System.Collections.Generic;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Common;
using ILPathways.Utilities;
using Isle.BizServices;

using IOER.Library;
//using DBM = LRWarehouse.DAL.DatabaseManager;
using Isle.DTO;
using LRWarehouse.DAL;


namespace IOER.Admin
{
	public partial class PublishHierarchy : BaseAppPage
	{
		protected void Page_Load( object sender, EventArgs e )
		{
			try
			{
				if ( IsUserAuthenticated() == false )
				{
					SetConsoleErrorMessage( "Error: you must be authenticated in order to use this page.<br/>Please login and try again." );
					Response.Redirect( "/", true );
					return;
				}

				if ( !Page.IsPostBack )
				{
					this.InitializeForm();
				}
			}
			catch ( ThreadAbortException taex )
			{
				// Do nothing, this is okay
			}
			catch ( Exception ex )
			{
				SetConsoleErrorMessage( "Sorry: unexpected error occurred." );
			}
		}

		protected void InitializeForm( )
		{
			if ( new AccountServices().IsUserAdmin( GetAppUser() ) == false )
			{
				SetConsoleErrorMessage( "Error: you are not authorized use this function." );
				Response.Redirect( "/", true );
			}
			//eventually populate list
			List<LearningListNode> list =  new CurriculumServices().Learninglists_SelectUserLists(WebUser.Id);
			PopulateList( ddlList, list, "Id", "Title", "Select a learning list" );
		}
		public static void PopulateList( DropDownList list, List<LearningListNode> items, string dataValueField, string dataTextField, string selectTitle )
		{
			try
			{
				//clear current entries
				list.Items.Clear();

				if ( items.Count > 0 )
				{
					int count = items.Count;
					if ( selectTitle.Length > 0 )
					{
						// add select row
						LearningListNode hdr = new LearningListNode();
						hdr.Id = 0;
						hdr.Title = selectTitle;
						items.Insert( 0, hdr );
					}
					list.DataSource = items;
					list.DataValueField = dataValueField;
					list.DataTextField = dataTextField;
					list.DataBind();
					list.Enabled = true;
					if ( selectTitle.Length > 0 )
						list.SelectedIndex = 0;
				}
				else
				{
					list.Items.Add( new ListItem( "No Selections Available", "" ) );
					list.Enabled = false;
				}

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "PublishHierarchy.PopulateList( list, string " + dataValueField + ", string " + dataTextField + ", string selectTitle )" );
			}
		}

		protected void btnStart_Click( object sender, EventArgs e )
		{
			int contentId = 0;
			int resourceId = 0;
			string statusMessage = "";
			string resourceList = "";
			ResourceManager mgr = new ResourceManager();

			if ( !string.IsNullOrWhiteSpace( txtCurriculumId.Text ) )
			{
				if ( Int32.TryParse( txtCurriculumId.Text, out contentId ) == false )
				{
					SetConsoleErrorMessage( "please enter a valid curriculum Id" );
					return;
				}
			}
			
			else if ( ddlList.SelectedIndex > 0 )
			{
				contentId = Int32.Parse( ddlList.SelectedValue );
			}

			else
			{
				SetConsoleErrorMessage( "please enter a curriculum Id or select item from list" );
				return;
			}

			if ( contentId > 0 )
			{
				//get
				var contentItem = new ContentServices().Get( contentId );
				if ( contentItem.Id == 0 )
				{
					SetConsoleErrorMessage( "That ID does not match valid content record" );
					return;
				}

				if ( contentItem.TypeId < 50 )
				{
					SetConsoleErrorMessage( "That ID does not match valid curriculum record" );
					return;
				}
				if ( contentItem.ResourceIntId == 0 )
				{
					SetConsoleErrorMessage( "The content item must not have been published (no resource id is present)" );
					return;
				}
				bool successful = mgr.InitiateDelayedPublishing( contentId, contentItem.ResourceIntId, WebUser.Id, ref resourceList, ref statusMessage );
				if ( successful )
				{
					ddlList.SelectedIndex = 0;

					//now add to ESI
					ResourceV2Services mgr2 = new ResourceV2Services();
					//mgr2.ImportRefreshResources( resourceList );
					statusMessage = "";
					//do the thumbs
					int thumbCntr = mgr2.AddThumbsForDelayedResources( contentId, ref statusMessage );
					//now update elastic
					int cntr = mgr2.AddDelayedResourcesToElastic( contentId, ref statusMessage );


					SetConsoleSuccessMessage( "Done, need to check logs. <br/>" + statusMessage );
				}
				else
				{
					SetConsoleErrorMessage( "InitiateDelayedPublishing failed, or didn't return any resources.<br> : resourceList" );
					return;
				}
			}
		}

		protected void btnNext_Click( object sender, EventArgs e )
		{
			//ResourceManager mgr = new ResourceManager();
			//int cntr = mgr2.AddThumbsForDelayedResources( contentId, ref statusMessage );
		}

		protected void btnDoElastic_Click( object sender, EventArgs e )
		{
			ResourceV2Services mgr2 = new ResourceV2Services();

			mgr2.AddDelayedResourcesToElastic();

		} //
	}
}