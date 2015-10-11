using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using EmailHelper = ILPathways.Utilities.EmailManager;
using MyManager = IOER.Controllers.FaqController;
using ILPathways.Business;
using IOER.classes;
using BDM = ILPathways.Common.BaseDataManager;
using IOER.Controllers;
using ILPathways.DAL;
using IOER.Library;
using ILPathways.Utilities;
using LRWarehouse.Business;
using LRDAL = LRWarehouse.DAL;

namespace IOER.Controls.FAQs
{
    public partial class FaqList : BaseUserControl
    {
    MyManager myManager = new MyManager();

	/// <summary>
	/// Set constant for this control to be used in log messages, etc
	/// </summary>
	const string thisClassName = "controls_FAQ_FaqList";

	/// <summary>
	/// Output format for the FAQ list
	/// </summary>
	public enum EFaqViewType
	{
		FaqSheet,
		FaqAccordions,
		GridView
	}

	#region Properties
	/// <summary>
	/// Get/Set default category. If blank, may want to show category list
	/// </summary>
	public string DefaultCategory
	{
		get { return defaultCategory.Text; }
		set { defaultCategory.Text = value; }
	}

	public string DefaultTargetPathways
	{
		get { return defaultTargetPathways.Text; }
		set { defaultTargetPathways.Text = value; }
	}

	public string SubcategoryTitle
	{
		get { return subcategoryTitle.Text; }
		set { subcategoryTitle.Text = value; }
	}
    public string ConfirmLink
    {
        get { return confirmLink.Text; }
        set { confirmLink.Text = value; }
    }
    public string FaqCategoryTitle
    {
        get { return faqCategoryTitle.Text; }
        set { faqCategoryTitle.Text = value; }
    }

	private EFaqViewType _faqViewType = EFaqViewType.FaqSheet;
	/// <summary>
	/// Gets/Sets FaqViewType - controls display type
	/// List
	/// Grid
	/// FaqAccordions- not implemented yet!
	/// </summary>
	public EFaqViewType FaqViewType
	{
		get { return this._faqViewType; }
		set { this._faqViewType = value; }
	}

	public string AllowingQuestions
	{
        get { return txtAllowingQuestions.Text; }
        set { txtAllowingQuestions.Text = value; }
	}

	private bool _mustBeAuthenticated = false;
	public bool MustBeAuthenticated
	{
		get { return _mustBeAuthenticated; }
		set { _mustBeAuthenticated = value; }
	}
	/// <summary>
	/// Store retrieve the last page number - used after updates to attempt to show the same page
	/// </summary>
	protected int LastPageNumber
	{
		get
		{
			if ( ViewState[ "LastPageNumber" ] == null )
				ViewState[ "LastPageNumber" ] = 0;

			if ( IsInteger( ViewState[ "LastPageNumber" ].ToString() ) )
				return Int32.Parse( ViewState[ "LastPageNumber" ].ToString() );
			else
				return 0;
		}
		set { ViewState[ "LastPageNumber" ] = value; }
	}//
	/// <summary>
	/// Store last retrieved total rows. Need to use to properly reset pager item count after none search postbacks
	/// </summary>
	protected int LastTotalRows
	{
		get
		{
			if ( ViewState[ "LastTotalRows" ] == null )
				ViewState[ "LastTotalRows" ] = 0;

			if ( IsInteger( ViewState[ "LastTotalRows" ].ToString() ) )
				return Int32.Parse( ViewState[ "LastTotalRows" ].ToString() );
			else
				return 0;
		}
		set { ViewState[ "LastTotalRows" ] = value; }
	}//
	#endregion
	/// <summary>
	/// Handle Page Load
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="ex"></param>
	protected void Page_Load( object sender, EventArgs e )
	{

		if ( Page.IsPostBack )
		{

		} else
		{
			this.InitializeForm();
		}
	}//


	/// <summary>
	/// Perform form initialization activities
	/// </summary>
	private void InitializeForm()
	{
		//set grid defaults (variables are in base control)
		//set sort to blank to default to results from database
		GridViewSortExpression = "base.Created";
		GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Descending;
		LastPageNumber = 0;
		formGrid.DataSource = null;
		formGrid.DataBind();

		if ( AllowingQuestions == "yes")
		{
			if ( MustBeAuthenticated )
			{
				//assuming only for asking q
				if ( IsUserAuthenticated() == false )
				{
					questionPanel.Visible = false;
				}
			}
		} else
		{
			questionPanel.Visible = false;
		}

		//string pageControl = "";    //McmsHelper.GetPageControl();
		//check for mcms parms
        //string defCategory = UtilityManager.ExtractNameValue( pageControl, "DefaultCategory", ";" );
        //if ( defCategory.Length > 0 )
        //{
        //    DefaultCategory = defCategory;
        //    //!!control not loaded yet, can't reference
        //    //AskQuestion1.DefaultCategory = defCategory;
        //}
        //string defTargetPathways = UtilityManager.ExtractNameValue( pageControl, "DefaultTargetPathways", ";" );
        //if ( defTargetPathways.Length > 0 )
        //    DefaultTargetPathways = defTargetPathways;
        //string defFaqViewType = UtilityManager.ExtractNameValue( pageControl, "FaqViewType", ";" );
        //if ( defFaqViewType.Length > 0 )
        //{
        //    if (defFaqViewType.ToLower().Equals(EFaqViewType.FaqSheet.ToString().ToLower()) )
        //        FaqViewType = EFaqViewType.FaqSheet;
        //    else if ( defFaqViewType.ToLower().Equals( EFaqViewType.FaqAccordions.ToString().ToLower() ) )
        //        FaqViewType = EFaqViewType.FaqAccordions;
        //    else 
        //        FaqViewType = EFaqViewType.GridView;
        //}
        //string url = CmsHttpContext.Current.Posting.Url;
        string url = Request.RawUrl;
        if ( ConfirmLink.ToLower().IndexOf( url.ToLower() ) == -1 )
        {
            ConfirmLink = url + "?id={0}";
        }
		if ( IsTestEnv()
			|| ( IsUserAuthenticated() && WebUser.TopAuthorization < 4 ) )
		{
			//searchPanel.Visible = true;
		}

		// Set source for form lists
		this.PopulateControls();

		DoSearch();
	}	// End 

	protected void Page_PreRender( object sender, EventArgs e )
	{

		try
		{
			pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
			pager1.ItemCount = pager2.ItemCount = LastTotalRows;

			if ( !Page.IsPostBack )
			{
				//if ( AskQuestion1 != null )
				//{
					//If post question cannot find a Default category in the placeholder, need to pass here and redo the initialize ==> or actually always do here
                    //if ( AskQuestion1.DefaultCategory != DefaultCategory )
                    //{
                    //    //AskQuestion1.DefaultCategory = DefaultCategory;
                    //    AskQuestion1.ResetDefaultCategory( DefaultCategory );
                    //}
                    //if ( AskQuestion1.ConfirmLink != ConfirmLink )
                    //{
                    //    AskQuestion1.ConfirmLink = ConfirmLink;
                    //}
                    //if ( AskQuestion1.FaqCategoryTitle != FaqCategoryTitle )
                    //{
                    //    AskQuestion1.FaqCategoryTitle = FaqCategoryTitle;
                    //}
				//}
			}
		} catch
		{
			//no action
		}

	}//
	#region Events
	/// <summary>
	/// Handle a form button clicked 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="ex"></param>
	public void FormButton_Click( Object sender, CommandEventArgs ev )
	{

		switch ( ev.CommandName )
		{
			case "New":
				//this.HandleNewRequest();
				break;

			case "Search":
				if ( this.IsSearchValid() )
				{
					this.DoSearch();
				}
				break;

		}
	} // end 


	#endregion

	#region form grid related methods
	/// <summary>
	/// Verify the search parameters are valid, or complete before continuing
	/// </summary>
	protected bool IsSearchValid()
	{
		bool isValid = true;

		return isValid;
	} //

	/// <summary>
	/// Conduct a search and populate the form grid
	/// </summary>
	private void DoSearch()
	{
		if ( searchPanel.Visible )
		{
			if ( rblFaqView.SelectedIndex > -1 )
			{
				if ( rblFaqView.SelectedIndex == 0 )
					FaqViewType = EFaqViewType.FaqSheet;
				else if ( rblFaqView.SelectedIndex == 1 )
					FaqViewType = EFaqViewType.FaqAccordions;
				else
					FaqViewType = EFaqViewType.GridView;
			}
		}
		int selectedPageNbr = 0;
		string sortTerm = GetCurrentSortTerm();
		pager1.ItemCount = 0;
		pager2.ItemCount = 0;

		DoSearch( selectedPageNbr, sortTerm );
	} //


	/// <summary>
	/// Conduct a search while addressing current page nbr and a sort term
	/// </summary>
	/// <param name="selectedPageNbr"></param>
	/// <param name="sortTerm"></param>
	private void DoSearch( int selectedPageNbr, string sortTerm )
	{
		DataSet ds = null;
		resultsPanel.Visible = false;
		faqSheetPanel.Visible = false;
		if ( selectedPageNbr == 0 )
		{
			//with custom pager, need to start at 1
			selectedPageNbr = 1;
		}
		LastPageNumber = selectedPageNbr;
		pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;

		// Set the page size for the DataGrid control based on the selection
		CheckForPageSizeChange();

		int pageSize = pager1.PageSize;
		if ( FaqViewType == EFaqViewType.FaqSheet
			|| FaqViewType == EFaqViewType.FaqAccordions )
		{
			pageSize = 10000;
			sortTerm = "cat.Category, scat.SubCategory, base.SequenceNbr, base.Title";
		}

		int pTotalRows = 0; 
		string filter = FormatFilter();

		try
		{
            //ds = myManager.SelectByCategory( category );

			ds = myManager.Search( filter, sortTerm, selectedPageNbr, pageSize, ref pTotalRows );
			//assumes min rows are returned, so pager needs to have the total rows possible
			pager1.ItemCount = pager2.ItemCount = pTotalRows;

			LastTotalRows = pTotalRows;

			if ( DoesDataSetHaveRows( ds ) == false )
			{
				resultsPanel.Visible = false;
				if (Page.IsPostBack)
					SetConsoleInfoMessage( "No records were found for the provided search criteria" );
				ddlPageSizeList.Enabled = false;
				pager1.Visible = false;
				pager2.Visible = false;
			} else
			{
				if ( FaqViewType == EFaqViewType.FaqSheet
					|| FaqViewType == EFaqViewType.FaqAccordions )
				{
					PopulateFaqSheet( ds );
				} else
				{
					PopulateGrid( ds, pTotalRows );
				}

			}
		} catch ( System.Exception ex )
		{
			//Action??		- display message and close form??	
			LoggingHelper.LogError( ex, thisClassName + ".DoSearch() - Unexpected error encountered while attempting search."  );

			this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
		}
	}	//


	protected void PopulateFaqSheet( DataSet ds )
	{
		faqSheetPanel.Visible = true;
		string toc = "";
		string results = "";
		//future use, initial assumption is one page per category
		string prevCategory = "";
		//string prevSubcat = "";
		string hdg = "";
		int sectionNbr = 0;
		int qNbr = 0;
		string sectionHdrTemplate = "";
		string footerTemplate = "";
		if ( FaqViewType == EFaqViewType.FaqAccordions )
		{
			sectionHdrTemplate = accordionHeader.Text;
			footerTemplate = accordionSectionFooter.Text;
		} else
		{
			sectionHdrTemplate = sectionHeader.Text;
			footerTemplate = sectionFooter.Text;
		}
		foreach ( DataRow dr in ds.Tables[ 0 ].DefaultView.Table.Rows )
		{
			string subcat = BDM.GetRowColumn( dr, "SubCategory", "" );

			if ( subcat != prevCategory )
			{
				sectionNbr++;
				string anchor = "sect" + sectionNbr.ToString();
				//append to toc
				toc += string.Format( this.sectionTocTemplate.Text, anchor, subcat );

				if ( prevCategory.Length > 0 )
				{
					//add footer
					results += footerTemplate;
				}
				//Todo - could have a description
				string subCatDesc = BDM.GetRowColumn( dr, "SubCatDesc", "" );

				//add section header 
				hdg = string.Format( sectionHdrTemplate, anchor, subcat );
				results += hdg;
				qNbr = 0;

				prevCategory = subcat;
			}

			string question = dr[ "Title" ].ToString().Trim();
			string answer = dr[ "Description" ].ToString().Trim();
			qNbr++;
			results += string.Format( this.faqTemplate.Text, qNbr, question, answer );

		} //end foreach

		if ( prevCategory.Length > 0 )
		{
			//add footer
			results += footerTemplate;
		}

		tocSection.Text = toc;
		subjectSection.Text = results;
	}	//


	protected void PopulateGrid( DataSet ds, int pTotalRows )
	{

		resultsPanel.Visible = true;
		ddlPageSizeList.Enabled = true;

		//searchPanel.Visible = false;

		DataTable dt = ds.Tables[ 0 ];
		DataView dv = ( ( DataTable ) dt ).DefaultView;
		//sort is done in the proc
		//if ( sortTerm.Length > 0 )
		//  dv.Sort = sortTerm;

		if ( pTotalRows > formGrid.PageSize )
		{
			//formGrid.PagerSettings.Visible = true;
			pager1.Visible = true;
			if ( pTotalRows > 10 )
				pager2.Visible = true;
		} else
		{
			pager1.Visible = false;
			pager2.Visible = false;
		}


		//populate the grid
		formGrid.DataSource = dv;
		//formGrid.PageIndex = selectedPageNbr;

		formGrid.DataBind();

	}	//


	protected string FormatFilter()
	{
		string filter = "";

		string booleanOperator = "AND";
		filter += BaseDataManager.FormatSearchItem( filter, "Status", "Published", booleanOperator );

		if (DefaultCategory.Length > 0)
			filter += BaseDataManager.FormatSearchItem( filter, "cat.Category", DefaultCategory, booleanOperator );

		if (DefaultTargetPathways.Length > 0) 
		{
			string where = string.Format( "(base.RowId in (select FaqRowId from [Faq.FaqPathway] where PathwayId in ({0}))) ", DefaultTargetPathways );

			//string where = string.Format(" (fqpath.PathwayId in ({0})) ",DefaultTargetPathways);
			filter += BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
		}

		if ( txtKeyword.Text.Trim().Length > 0 )
		{
			string keyword = BDM.HandleApostrophes( FormHelper.CleanText( txtKeyword.Text.Trim() ) );

			if ( keyword.IndexOf( "%" ) == -1 )
				keyword = "%" + keyword + "%";

			string where = " (base.Title like '" + keyword + "'	OR base.[Description] like '" + keyword + "') ";
			filter += BaseDataManager.FormatSearchItem( filter, where, booleanOperator );
		}


		if ( this.IsTestEnv() )
			this.SetConsoleSuccessMessage( "sql: " + filter );

		return filter;
	}	//
	protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
	{
		string url = Request.RawUrl;

		if ( e.Row.RowType == DataControlRowType.DataRow )
		{
			//select
			if ( openingDetailInNewWindow.Text.Equals( "yes" ) )
			{
				LinkButton slb = ( LinkButton ) e.Row.FindControl( "selectButton" );
				slb.PostBackUrl = url + "?id=" + DataBinder.Eval( e.Row.DataItem, "rowId" );

				//slb.Attributes.Add( "onclick", "javascript:return " +
				//		"confirm('Are you sure you want to select this project " +
				//		DataBinder.Eval( e.Row.DataItem, "projectId" ) + "')" );
			}

		}
	}//

	protected void formGrid_RowCommand( object sender, GridViewCommandEventArgs e )
	{
		if ( e.CommandName == "SelectRow" )
		{
			pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
			pager1.ItemCount = pager2.ItemCount = LastTotalRows;

			// get the ID of the clicked row
			int ID = Convert.ToInt32( e.CommandArgument );

			// show the record 
			//ShowRecord( ID );
		}
	}


	/// <summary>
	/// Checks selected sort column and determines if new sort or a change in the direction of the sort
	/// </summary>
	protected void formGrid_Sorting( object sender, GridViewSortEventArgs e )
	{
		string newSortExpression = e.SortExpression;
		string sortTerm = "";

		//check if the same field as previous sort
		if ( GridViewSortExpression.ToLower().Equals( newSortExpression.ToLower() ) )
		{
			// This sort is being applied to the same field for the second time so Reverse it.
			if ( GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Ascending )
			{
				GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Descending;
				sortTerm = newSortExpression + " DESC";
			} else
			{
				GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
				sortTerm = newSortExpression + " ASC";
			}
		} else
		{
			GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
			GridViewSortExpression = newSortExpression;
			sortTerm = newSortExpression + " ASC";
		}


		DoSearch( 1, sortTerm );

	}//

	///<summary>
	///Add pagination capabilities
	///</summary>
	public void formGrid_PageIndexChanging( object sender, GridViewPageEventArgs e )
	{
		formGrid.PageIndex = e.NewPageIndex;
		//get current sort term
		string sortTerm = GetCurrentSortTerm();

		DoSearch( formGrid.PageIndex, sortTerm );

	}//
	private string GetCurrentSortTerm()
	{
		string sortTerm = GridViewSortExpression;
		if ( sortTerm.Length > 0 )
		{
			if ( GridViewSortDirection == System.Web.UI.WebControls.SortDirection.Ascending )
				sortTerm = sortTerm + " ASC";
			else
				sortTerm = sortTerm + " DESC";
		}
		return sortTerm;

	}
	#endregion
	#region Paging related methods
	public void pager_Command( object sender, CommandEventArgs e )
	{

		int currentPageIndx = Convert.ToInt32( e.CommandArgument );
		pager1.CurrentIndex = currentPageIndx;
		pager2.CurrentIndex = pager1.CurrentIndex;
		string sortTerm = GetCurrentSortTerm();

		DoSearch( currentPageIndx, sortTerm );

	}
	/// <summary>
	/// Initialize page size list and check for a previously set size
	/// </summary>
	private void InitializePageSizeList()
	{
		SetPageSizeList();

		//Set page size based on user preferences
		int defaultPageSize = SessionManager.Get( Session, SessionManager.SYSTEM_GRID_PAGESIZE, 25 );
		this.formGrid.PageSize = defaultPageSize;
		pager1.PageSize = defaultPageSize;
		pager2.PageSize = defaultPageSize;

		this.SetListSelection( this.ddlPageSizeList, defaultPageSize.ToString() );

	} //
	private void SetPageSizeList()
	{
		DataSet ds1 = LRDAL.DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
		BDM.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

	} //
	/// <summary>
	/// Check if page size preferrence has changed and update session variable if appropriate
	/// </summary>
	private void CheckForPageSizeChange()
	{
		int index = ddlPageSizeList.SelectedIndex;
		if ( index > 0 )
		{
			int size = Convert.ToInt32( ddlPageSizeList.SelectedItem.Text );
			if ( formGrid.PageSize != size )
			{
				formGrid.PageSize = size;
				pager1.PageSize = size;
				pager2.PageSize = size;
				//Update user preference
				Session[ SessionManager.SYSTEM_GRID_PAGESIZE ] = ddlPageSizeList.SelectedItem.Text;
			}
		}

	} //

	/// <summary>
	/// Handle change to page size
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="ex"></param>
	protected void PageSizeList_OnSelectedIndexChanged( object sender, System.EventArgs ea )
	{
		// Set the page size for the DataGrid control based on the selection
		int index = ddlPageSizeList.SelectedIndex;
		if ( index > 0 )
		{
			//need to reset to first page as current pg nbr may be out of range
			formGrid.PageIndex = 0;
			//retain curent sort though
			string sortTerm = GetCurrentSortTerm();

			DoSearch( formGrid.PageIndex, sortTerm );
			//DoSearch();
		}
	} //
	#endregion
	#region Housekeeping
	/// <summary>
	/// Populate form controls
	/// </summary>
	private void PopulateControls()
	{


		//
		InitializePageSizeList();
	} //

	#endregion


    }
}
