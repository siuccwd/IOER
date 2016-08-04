using System;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Business;
using ILPathways.classes;
using ILPathways.Controllers;
using ILPLibrary = ILPathways.Library;
using ILPathways.Utilities;
using GDAL = Isle.BizServices;

using LRWarehouse.Business;
using LLibrary = ILPathways.Business.Library;
using MyManager = Isle.BizServices.LibraryBizService;
using LRWarehouse.DAL;
using MyEventController = ILPathways.Controllers.CodeTableController;
using CodesContract = ILPathways.CodeTableServiceReference.CodesDataContract;
using DataBaseHelper = LRWarehouse.DAL.BaseDataManager;
using bizService = ILPathways.CodeTableServiceReference;
using AjaxControlToolkit;

namespace ILPathways.LRW.controls
{
    public partial class LibrarySearch : ILPLibrary.BaseUserControl
    {

        MyManager myManager = new MyManager();
        string filterDesc = "";
        MyEventController mec = new MyEventController();

        /// <summary>
        /// Set constant for this control to be used in log messages, etc
        /// </summary>
        const string thisClassName = "LibrarySearch";


        #region Properties
        /// <summary>
        /// Set value used when check form privileges
        /// </summary>
        public string FormSecurityName
        {
            get { return this.txtFormSecurityName.Text; }
            set { this.txtFormSecurityName.Text = value; }
        }
        public int CurrentLibraryId
        {
            get
            {
                int id = 0;
                Int32.TryParse( this.txtLibraryId.Text, out id );
                return id;
            }
            set { this.txtLibraryId.Text = value.ToString(); }
        } //
        /// <summary>
        /// INTERNAL PROPERTY: CurrentRecord
        /// Set initially and store in ViewState
        /// </summary>
        protected LLibrary CurrentRecord
        {
            get { return ViewState[ "CurrentRecord" ] as LLibrary; }
            set { ViewState[ "CurrentRecord" ] = value; }
        }
        protected string CustomFilter
        {
            get
            {
                if ( ViewState[ "CustomFilter" ] == null )
                    ViewState[ "CustomFilter" ] = "";

                return ViewState[ "CustomFilter" ].ToString();
            }
            set { ViewState[ "CustomFilter" ] = value; }
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
        public string DisplayURL
        {
            get
            {
                return this.txtDisplayUrl.Text;
            }
            set { this.txtDisplayUrl.Text = value; }
        }//
        private bool _isPersonalLibrary = false;
        /// <summary>
        /// If is a personal view, then user must be owner
        /// </summary>
        public bool IsPersonalLibraryView
        {
            get
            {
                return _isPersonalLibrary;
            }
            set { _isPersonalLibrary = value; }
        }//

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

        /// <summary>
        /// Store last retrieved total rows. Need to use to properly reset pager item count after none search postbacks
        /// </summary>
        protected bool UsingFullTextOption
        {
            get
            {
                return _usingFullTextOption;
            }
            set { this._usingFullTextOption = value; }
        }//
        private bool _usingFullTextOption = true;

        //protected bool IsUser
        //{
        //    get
        //    {
        //        return _usingFullTextOption;
        //    }
        //    set { this._usingFullTextOption = value; }
        //}//
        //private bool _usingFullTextOption = true;
        #endregion

        /// <summary>
        /// Handle Page Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        protected void Page_Load( object sender, EventArgs e )
        {

            if ( IsUserAuthenticated() )
            {
                CurrentUser = GetAppUser();
            }
            if ( !Page.IsPostBack )
            {
                this.InitializeForm();
                if ( GetRequestKeyValue( "sId", 0 ) == 0 && CurrentLibraryId > 0 )
                    DoSearch();
            }
        }

        /// <summary>
        /// Perform form initialization activities
        /// </summary>
        public void InitializeForm()
        {

            this.mec.GetCodesSuccess += new EventHandler( mec_GetCodesSuccess );
            this.mec.GetCodesFailure += new EventFailureEventHandler( mec_GetCodesFailure );
            this.mec.GetCodesNoComponents += new EventHandler( mec_GetCodesNoComponents );

            FormPrivileges = new ApplicationRolePrivilege();
            FormPrivileges.SetReadOnly();
            FormPrivileges.ReadPrivilege = 1;

            RecordPrivileges = new ApplicationRolePrivilege();
            RecordPrivileges.SetReadOnly();
            RecordPrivileges.ReadPrivilege = 1;
			btnUnSubscribe.Attributes.Add( "onClick", "return confirmUnsubscribe(this);" );

            if ( this.IsUserAuthenticated() == false )
            {

                if ( IsPersonalLibraryView )
                {
                    //can't access personal if not auth
                    SetConsoleErrorMessage( "Error - you must sign in to access personal libraries." );
                    pnlContainer.Visible = false;
                    return;

                } else 
                {


                    //or may always have to be authenticated for libs - TBD
                    //13-05-30 MP- OK, can nav here from the general library search
                    //just disable tools
                    actionsPanel.Visible = false;
                    //SetConsoleErrorMessage( "Error - you must sign in to access libraries." );
                    //pnlContainer.Visible = false;
                    //return;
                }

            }
            else
            {
                if ( showingLibMtceToAll.Text.Equals( "no" ) )
                {
                    //don't allow lib mtce from search for now
                    libMtceFlyoutPanel.Visible = false;
                }
                // get form privileges TODO are there any for basic search?, if not don't call
                FormPrivileges = GDAL.SecurityManager.GetGroupObjectPrivileges( WebUser, FormSecurityName );
                
                //only allow checkboxes if personal, or admin person
			    //may need later check for public access to copy
			    //maybe check later for non-personal
                if ( IsPersonalLibraryView )
                {
                    formGrid.AutoGenerateCheckBoxColumn = true;
                    //show header tools
                    actionsPanel.Visible = true;
                    btnSelect.Visible = true;
                }
                else
                {
                    //default to hidden and configure later
                    formGrid.AutoGenerateCheckBoxColumn = false;
                    //actionsPanel.Visible = false;

                    btnSelect.Visible = false;
                }
            }

            //set grid defaults (variables are in base control)
            //set sort to blank to default to results from database
            GridViewSortExpression = "";
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            LastPageNumber = 0;

            resultsPanel.Visible = false;
            LibraryCollectionMtcePanel.Visible = false;
            // Set source for form lists
            this.PopulateControls();
          

            CheckRecordRequest();
           
            if ( CurrentLibraryId > 0 )
            {
                //get sections
                bool hasSections = GetSections( CurrentLibraryId );
                //override if auth and not owner
                if ( IsUserAuthenticated() == true && WebUser.Id != CurrentRecord.CreatedById )
                {
                    GetCurrentUserSections();
                }
                if ( hasSections )
                {
                    PopulateFilters( CurrentLibraryId );
                    detailsPanel.Visible = true;
                    collectionsPanel.Visible = true;

                    if ( new MyManager().IsLibraryEmpty( CurrentLibraryId ) == false )
                    {
                        //if a sectionId was provided, a search was already done
                        
                        //if ( IsPersonalLibraryView && this.GetRequestKeyValue( "sId", 0 ) == 0 )
                        //    DoSearch();
                        //else if ( IsPersonalLibraryView == false && this.GetRequestKeyValue( "s", "n" ) == "y" )
                        //    DoSearch();
                    }
                    else
                    {
                        SetConsoleInfoMessage( "The library is empty, no resources. Use the search tool to find resources, select and add to collections" );
                        pnlSearch.Visible = false;
                        detailsPanel.Visible = false;
                    }
                }
                else
                {
                    SetConsoleInfoMessage( "The library is empty, no collections or resources. Click the Collections section, to add collections" );
                    collectionsPanel.Visible = true;
                    pnlSearch.Visible = false;
                    detailsPanel.Visible = false;
                }
            }
           
        }	// End 

        /// <summary>
        /// Check the request type for the form
        /// </summary>
        private void CheckRecordRequest()
        {
            CurrentLibraryId = 0;
            // Check if a request was made for a specific section
            int sid = this.GetRequestKeyValue( "sId", 0 );
            int lid = this.GetRequestKeyValue( "id", 0 );
            if ( sid > 0 )
            {
                LibrarySection ls = myManager.LibrarySectionGet( sid );
                if ( ls != null && ls.Id > 0 )
                {
                    SetCurrentLibrary( ls.LibraryId );
                    //may set a perm property for use with the search?
                    CustomFilter = string.Format( " (LibrarySectionId = {0})", sid );
                    DoCustomSearch( "Collection", ls.Title );

					if ( IsUserAuthenticated() && CurrentUser.Id != ls.CreatedById )
					{
						if (this.showingCollectionSubscriptions.Text == "yes")
                        collectionSubscriptionPanel.Visible = true;
					}

                    //TODO - check for access, and determine if should hide, or at least change the title
                    //- new method: get library privileges - use RecordPrivileges
                    if ( RecordPrivileges.CanUpdate() == true )
                    {
                        lblUpdateCollection.Text = "Update: " + ls.Title;
                    }
                    else
                    {
                        lblUpdateCollection.Text = "Collection: " + ls.Title + " details";
                        lblCollectionSummary.Visible = true;
                        lblCollectionSummary.Text = ls.SectionSummaryFormatted( "isleBox_H2" );
                        LibraryCollectionMtce1.Visible = false;
                    }
                }
                return;
            }
            else if ( lid > 0 )
            {
                SetCurrentLibrary( lid );

                //may set a perm property for use with the search?
                CustomFilter = string.Format( " (LibraryId = {0})", lid );
                //DoCustomSearch( "Library", lib.Title );
                return;
            }
            else if ( IsPersonalLibraryView )
            {
                Patron user = ( Patron ) WebUser;
                LLibrary lib = myManager.GetMyLibrary( user );
                if ( lib != null && lib.IsValid )
                {
                    SetCurrentLibrary( lib );
                    //CurrentLibraryId = lib.Id;
                    //RecordPrivileges.SetAdminPrivileges();
                    //detailsPanel.Visible = true;
                }
                else
                {
                    detailsPanel.Visible = false;
                    //libraryStateMsg.Text = "Your personal library has not been initialized. To initialze, add resources.";
                   
                }
            }
      }	// End 


        protected bool SetCurrentLibrary( int libraryId )
        {
            LLibrary lib = myManager.Get( libraryId );
            return SetCurrentLibrary( lib );
        }	// End 


        protected bool SetCurrentLibrary( LLibrary lib )
        {
            bool isValid = true;
            CurrentRecord = lib;

            libHeading.Text = lib.Title;
            CurrentLibraryId = lib.Id;
            int libraryImageWidth = UtilityManager.GetAppKeyValue( "libraryImageWidth", 150 );
            if ( lib.ImageUrl.Length > 0 )
            {
                int sec = DateTime.Now.Second;
                string v = "?v=" + sec.ToString();
                imageTag.Text = string.Format( txtLibraryImageTemplate.Text, lib.ImageUrl + v, libraryImageWidth );
            }
            else
            {
                //maybe a default placeholder
                string defaultLibraryImage = UtilityManager.GetAppKeyValue( "defaultLibraryImage", "" );
                if ( defaultLibraryImage.Length > 10)
                    imageTag.Text = string.Format( txtLibraryImageTemplate.Text, defaultLibraryImage, libraryImageWidth );
                
            }

            if ( IsUserAuthenticated() == true && WebUser.Id == lib.CreatedById )
            {
                RecordPrivileges.SetAdminPrivileges();
            }
            else
            {
                //check access
                //just in case
                if ( lib.IsDiscoverable == false)
                {
                    //reject
                    SetConsoleErrorMessage( "Access to this library is not allowed!" );
                    isValid = false;

                } else if ( lib.IsPublic == false)
                {
                    //check if a subscriber
                    SetConsoleErrorMessage( "Access to this library is by invitation only (coming soon)!" );
                    accessPanel.Visible = true;
                    isValid = false;
                }

            }

            if ( isValid == false )
            {
                pnlContainer.Visible = false;
                CurrentLibraryId = 0;
            }
            else
            {
                if ( IsUserAuthenticated() == true && WebUser.Id != lib.CreatedById )
                {
                    libSubscriptionPanel.Visible = true;
                    SetCollectionActions( false );
                    formGrid.AutoGenerateCheckBoxColumn = true;
                    btnSelect.Visible = true;

                    //already a subscriber
                    ObjectSubscription entity = myManager.LibrarySubscriptionGet( CurrentLibraryId, WebUser.Id );
					if ( entity != null && entity.Id > 0 )
					//if (myManager.IsSubcribedtoLibrary( CurrentLibraryId, WebUser.Id))
					{
						this.btnUnSubscribe.Visible = true;
						this.btnUpdateSubscribe.Visible = true;
						this.btnSubscribeToThisLibrary.Visible = false;
						this.SetListSelection( this.ddlLibrarySubscriptions, entity.SubscriptionTypeId.ToString() );
					}
					else
					{
						this.btnUnSubscribe.Visible = false; 
					}
                }
				libraryStateMsg.Text = lib.LibrarySummaryFormatted( "isleBox_H2" );
                if ( RecordPrivileges.CanUpdate() == false )
                {
                    SetCollectionActions( false );
                    libMtcePanel.Visible = false;
                    //libraryStateMsg.Text = lib.LibrarySummaryFormatted( "isleBox_H2" );
                }
            }

            return isValid;
        }	// End 


        protected bool GetSections( int libraryId )
        {
            bool hasSections = false;
            //TODO - may want to anticipate check for private sections
            int pShowingAll = 1;
            if ( RecordPrivileges.CanUpdate() )
            {
                pShowingAll = 2;
                addCollectionPanel.Visible = true;
            }

            DataSet ds = myManager.LibrarySectionsSelect( libraryId, pShowingAll );
            string collTitle = "Title";
            if ( usingFormattedCollectionTitle.Text.Equals("yes"))
                collTitle = "FormattedTitle";

            if ( DoesDataSetHaveRows( ds ) )
            {
                hasSections = true;
                string list = "";
                foreach ( DataRow row in ds.Tables[ 0 ].Rows )
                {
                    string title = DataBaseHelper.GetRowColumn( row, collTitle, "Title" );
                    int id = DataBaseHelper.GetRowColumn( row, "Id", 0 );
                    bool isDef = DataBaseHelper.GetRowColumn( row, "IsDefaultSection", false );
                    if ( isDef )
                        title += " (default)";

                    list += string.Format( this.collectionsTemplate.Text, id, title, DisplayURL );
                }
                if ( list.Length > 0 )
                {
                    //add all, need to handle differently if not personal
                    list = string.Format( this.wholeLibraryTemplate.Text, libraryId, allCollectionsLabel.Text, DisplayURL ) + list;
                    //list = string.Format( this.collectionsTemplate.Text, 0, allCollectionsLabel.Text ) + list;
                    collectionsList.Text = string.Format( "<ul>{0}</ul>", list );
                }

                DatabaseManager.PopulateList( this.ddlTargetCollection, ds, "Id", "Title", "Target Collection" );
            }

            return hasSections;
        }	// End 


        protected void GetCurrentUserSections()
        {

            int pShowingAll = 2;
            LLibrary lib = myManager.GetMyLibrary( CurrentUser );
            DataSet ds = myManager.LibrarySectionsSelect( lib.Id, pShowingAll );
            if ( DoesDataSetHaveRows( ds ) )
            {
                DatabaseManager.PopulateList( this.ddlTargetCollection, ds, "Id", "Title", string.Format("{0}'s Target Collection", CurrentUser.FullName()) );
            }

        }	// End 


        protected void Page_PreRender( object sender, EventArgs e )
        {

            try
            {
                if ( !Page.IsPostBack )
                {
                    if ( IsPersonalLibraryView )
                    {
                        if ( IsUserAuthenticated() )
                            SetPersonalLibrary();
                    }
                    else
                    {
                        //may want to set lib panel, or hide from this view
                        //or set control to preview/display
                    }
                    //check if a collection was requested
                    int sid = this.GetRequestKeyValue( "sId", 0 );
                    if ( sid > 0 )
                    {
                        LibraryCollectionMtcePanel.Visible = true;
                        LibraryCollectionMtce1.SetCollection( sid );
                    }
                }
                else
                {
                    //may want to now check if a lib was created
                    if ( CurrentLibraryId == 0 && IsPersonalLibraryView && IsUserAuthenticated() )
                    {

                        CurrentUser = GetAppUser();
                        LLibrary lib = myManager.GetMyLibrary( CurrentUser );
                        if ( lib != null && lib.IsValid )
                        {
                            SetCurrentLibrary( lib.Id );
                            bool hasSections = GetSections( CurrentLibraryId );

                            //TODO - should filters reflect current section??
                            if ( hasSections )
                            {
                                PopulateFilters( CurrentLibraryId );
                                detailsPanel.Visible = true;
                                collectionsPanel.Visible = true;
                            }
                            else
                            {
                                SetConsoleSuccessMessage( "Now that your library has been created, you can add collections. " );
                                collectionsPanel.Visible = true;
                                detailsPanel.Visible = false;
                            }
                            
                        }
                    }
                }
               
                pager1.CurrentIndex = pager2.CurrentIndex = LastPageNumber;
                pager1.ItemCount = pager2.ItemCount = LastTotalRows;
            }
            catch
            {
                //no action
            }
        }//

        /// <summary>
        /// Set up a personal library
        /// </summary>
        private void SetPersonalLibrary()
        {
            if ( CurrentLibraryId == 0 )
            {
                CurrentUser = GetAppUser();
                LLibrary lib = myManager.GetMyLibrary( CurrentUser );
                if ( lib != null && lib.IsValid )
                    SetCurrentLibrary( lib ); 
            }
            if ( CurrentLibraryId > 0 )
            {
                LibraryMtce1.SetLibrary( CurrentLibraryId );
                //libraryStateMsg.Text = "";

                libMtcePanel.Visible = true;
            }
            else
            {
                //libraryStateMsg.Text = "Your personal library has not been initialized. To initialze, add resources.";
                SetConsoleInfoMessage( "Your personal library has not been initialized. To initialze, click on the Library section below." );
                if ( allowingLibCreate.Text.Equals( "yes" ) )
                {
                    libMtcePanel.Visible = true;
                    //ensure the type is locked as personal, or maybe provide a public method
                    LibraryMtce1.InitializePersonalLibary();
                }
            }

        }//

		protected void btnShowMtce_Click( object sender, EventArgs e )
		{
			this.maintenancePanel.Visible = true;
			pnlContainer.Visible = false;

        }//
		protected void btnShowSearch_Click( object sender, EventArgs e )
		{
			this.maintenancePanel.Visible = false;
			pnlContainer.Visible = true;

		}//
        public void btnSubscribeToThisLibrary_Click( object sender, EventArgs e )
        {
            //TODO: need to handle updates
            if ( this.ddlLibrarySubscriptions.SelectedIndex > 0 )
            {
                string statusMessage = "";
                int typeId = Int32.Parse( this.ddlLibrarySubscriptions.SelectedValue);
                int id = myManager.LibrarySubScriptionCreate( CurrentLibraryId, WebUser.Id, typeId, ref statusMessage );
                if ( id > 0 )
                    SetConsoleSuccessMessage( "Successfully subscribed to library" );
                else
                    SetConsoleErrorMessage( "Error attempting to subscribe: " + statusMessage );
            }

        }

        public void btnUpdateSubscribe_Click( object sender, EventArgs e )
        {
            //TODO: need to handle updates
            if ( this.ddlLibrarySubscriptions.SelectedIndex > 0 )
            {
                string statusMessage = "";
                int typeId = Int32.Parse( this.ddlLibrarySubscriptions.SelectedValue);
                statusMessage = myManager.LibrarySubScriptionUpdate( CurrentLibraryId, WebUser.Id, typeId );
                if ( statusMessage == "successful" )
                    SetConsoleSuccessMessage( "Successfully updated subscription" );
                else
                    SetConsoleErrorMessage( "Error attempting to update subscription: " + statusMessage );

            }

        }


        protected void btnUnSubscribe_Click( object sender, EventArgs e )
        {
            //SetConsoleInfoMessage( "Not implemented, Coming soon" );
			string statusMessage = "";
			ObjectSubscription entity = myManager.LibrarySubscriptionGet( CurrentLibraryId, CurrentUser.Id );
			if ( entity != null && entity.Id > 0 )
			{
				if (myManager.LibrarySubscriptionDelete( entity.Id, ref statusMessage ) )
				{
					SetConsoleSuccessMessage("Unsubscribed from library");
					ddlLibrarySubscriptions.SelectedIndex = 0;
					this.btnUnSubscribe.Visible = false; 
				} else 
				{
					SetConsoleErrorMessage("Error encountered attempting to unsubscribe. <br/>System administration has been notified");
					EmailManager.NotifyAdmin("Error on library unsubscribe", string.Format("Error while {0} attempted to unsubscribe from library: {1}<br/>{2}", CurrentUser.FullName(), CurrentLibraryId, statusMessage));
				} 

			} else 
			{
				SetConsoleErrorMessage("Wierd, the subscription record was not found. >br/>Notifying system administration.");
				EmailManager.NotifyAdmin("Error on library unsubscribe", string.Format("Error while {0} attempted to unsubscribe from library: {1}<br/>Could not retrieve the subscription record!", CurrentUser.FullName(), CurrentLibraryId));
			}
        }
        public void btnSubscribeToThisCollection_Click( object sender, EventArgs e )
        {
            //TODO: Add code here
        }

        /// <summary>
        /// Handle a form button clicked 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ex"></param>
        public void FormButton_Click( Object sender, CommandEventArgs ev )
        {

            switch ( ev.CommandName )
            {

                case "Search":
                    if ( this.IsSearchValid() )
                    {
                        //SearchType = 1;
                        this.DoSearch();
                    }
                    break;

                case "AddCollection":
                    if ( this.txtCollectionName.Text.Length > 0 )
                    {
                        this.AddCollection();
                    }
                    break;
                case "SelectChecked":
                    if ( IsRequestValid() == true )
                    {
                        this.HandleCheckedRows();
                       
                    }
                    break;
            }
        } // end 
        protected void AddCollection()
        {
            LibrarySection entity = new LibrarySection();
            string statusMessage = "";
            try
            {
                entity.LibraryId = CurrentLibraryId;
                entity.SectionTypeId = LLibrary.GENERAL_LIBRARY_SECTION_ID;
                entity.Title = txtCollectionName.Text;
                entity.Description = "To be named later";
                entity.AreContentsReadOnly = false;
                entity.IsDefaultSection = false;
                entity.IsPublic = true;
                entity.IsActive = true;
                entity.ParentId = 0;
                entity.CreatedById = WebUser.Id;
                int id = myManager.LibrarySectionCreate( entity, ref statusMessage );
                if ( id > 0 )
                {
                    txtCollectionName.Text = "";
                    addCollectionMsg.Text = "Added collection";
                    GetSections( CurrentLibraryId );
                }
                else
                {
                    addCollectionMsg.Text = "Error encountered adding collection: " + statusMessage;
                }
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".AddCollection() " );
                addCollectionMsg.Text = "Error encountered adding collection: " + statusMessage;
              
            }
        } // end 
        protected bool IsRequestValid()
        {
            bool isValid = true;

            if ( this.ddlCheckOptions.SelectedIndex < 1 )
            {
                SetConsoleErrorMessage( "Please select an action for checked rows" );
                return false;
            }
            else if ( this.ddlCheckOptions.SelectedIndex < 3 && this.ddlTargetCollection.SelectedIndex < 1 )
            {
                string action = ddlCheckOptions.SelectedItem.Text;
                SetConsoleErrorMessage( "Please select a target collection for " + action );
                return false;
            }
            else
            {
                if ( IsTestEnv() == false && areToolsActive.Text.Equals( "no" ) )
                {
                    SetConsoleInfoMessage( "This functionality is under construction, coming real soon." );
                    return false;
                }
            }
            return isValid;
        } // end 
/// <summary>
	/// Iterate thru all checked rows and call method to add to related table
	/// </summary>
	protected void HandleCheckedRows()
	{

		int keysSelected = 0;
		int recordsUpdated = 0;
		string messages = "";
        int LibraryResourceId;

		string txt = "Selected the following indices:<br>";
        //need to get the action type
		try
		{
            int targetCollection = 0;
            if ( ddlTargetCollection.SelectedIndex > 0 )
                targetCollection = Int32.Parse( ddlTargetCollection.SelectedValue );
            int ResourceIntId=0;
			foreach ( int index in formGrid.GetSelectedIndices() )
			{
				keysSelected++;
                LibraryResourceId = Int32.Parse( formGrid.DataKeys[ index ].Values[ "LibraryResourceId" ].ToString() );

                ResourceIntId = Int32.Parse( formGrid.DataKeys[ index ].Values[ "ResourceIntId" ].ToString() );

                txt += LibraryResourceId.ToString() + ",";
				//create training student and add to status report
                recordsUpdated += UpdateCheckedRecord( LibraryResourceId, targetCollection, ref messages );

			}

		} catch ( Exception ex )
		{
			LoggingHelper.LogError( ex, thisClassName + ".HandleCheckedRows() - Unexpected error encountered while attempting the requested action. " );
			this.SetConsoleErrorMessage( "An unexpected error was encountered while attempting to add these records. System administration has been notified:<br/> " + ex.Message );
		}

		if ( keysSelected > 0 )
		{
			if ( recordsUpdated > 0 && recordsUpdated == keysSelected )
			{
				this.SetConsoleSuccessMessage( "Successfully updated library resources. " );
				//should reset selections first
				formGrid.ClearSelection();

			} else
			{
				this.SetConsoleSuccessMessage( String.Format( "Warning not all selections were processed successfully. Requested Count = {0}, Added Count = {1} </br>{2}", keysSelected.ToString(), recordsUpdated.ToString(), messages ) );
			}

			int pageNbr = 0;
			if ( formGrid.PageIndex > 0 )
				pageNbr = formGrid.PageIndex;
			else if ( LastPageNumber > 0 )
				pageNbr = LastPageNumber;

			DoSearch( pageNbr, GetCurrentSortTerm() );

		} else
		{
			this.SetConsoleErrorMessage( "Error - You must select the check box of at least one record." );
			return;
		}
	}
	/// <summary>
	/// handle record update
	/// </summary>
	/// <param name="orgId"></param>
	/// <param name="userId"></param>
	/// <param name="statusMessage"></param>
    protected int UpdateCheckedRecord( int libraryResourceId, int targetCollectionId, ref string statusMessage )
    {
        int count = 0;
        try
        {
            if ( this.ddlCheckOptions.SelectedIndex == 1 )
            {
                //copy
                int id = myManager.ResourceCopyById( libraryResourceId, targetCollectionId, WebUser.Id, ref statusMessage );
                if ( id > 0 )
                    count++;

            }
            else if ( this.ddlCheckOptions.SelectedIndex == 2 )
            {
                //move
                string message = myManager.ResourceMoveById( libraryResourceId, targetCollectionId, WebUser.Id, ref statusMessage );
                if ( message == "successful" )
                    count++;
            }
            else if ( this.ddlCheckOptions.SelectedIndex == 3 )
            {
                //delete
                if ( myManager.LibraryResourceDeleteById( libraryResourceId, ref statusMessage ) )
                    count++;

            }
            else
            {
                //unknkown
            }
        }
        catch ( Exception ex )
        {
            LoggingHelper.LogError( ex, thisClassName + ".UpdateCheckedRecord() - Unexpected error encountered while attempting the requested action. " );
            this.SetConsoleErrorMessage( "An unexpected error was encountered while attempting to add these records. System administration has been notified:<br/> " + ex.Message );
        }
        return count;
    }

    protected void DoCustomSearch( string type, string parm )
    {
        lblCustomTitle.Text = CustomTitle = string.Format( "{0}: {1}", type, parm );
        CustomHdrPanel.Visible = true;
        //SearchType = 2;

        DoSearch();
    }	// End 

    /// <summary>
    /// Conduct a search and populate the form grid
    /// </summary>
    private void DoSearch()
    {
        int selectedPageNbr = 0;
        GridViewSortExpression = "";
        string sortTerm = GetCurrentSortTerm();

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
        this.searchSummaryDesc.Text = "";
        DateTime startTime = System.DateTime.Now;
        //??? initially fake out with default dot net paging
        if ( selectedPageNbr == 0 )
        {
            //with custom pager, need to start at 1
            selectedPageNbr = 1;
        }
        LastPageNumber = selectedPageNbr;
        pager1.CurrentIndex = pager2.CurrentIndex = selectedPageNbr;

        // Set the page size for the DataGrid control based on the selection
        CheckForPageSizeChange();

        //if ( ftOptionList.SelectedIndex == 1 )
        UsingFullTextOption = false;
        //else
        //    UsingFullTextOption = true;
        int pTotalRows = 0;
        int currentPageSize = 0;
        string filter = "";

        formGrid.DataSource = null;
        formGrid.DataBind();

        if ( CustomTitle.Length > 0 )
        {
            lblCustomTitle.Text = CustomTitle;
            CustomHdrPanel.Visible = true;
        }

        try
        {
            filter = FormatFilter();

            currentPageSize = formGrid.PageSize;

            ds = myManager.LibraryResourceSearch( filter, sortTerm, selectedPageNbr, pager1.PageSize, ref pTotalRows );

            pager1.ItemCount = pager2.ItemCount = pTotalRows;
            LastTotalRows = pTotalRows;

            searchSummaryDesc.Text = filterDesc;

            if ( DataBaseHelper.DoesDataSetHaveRows( ds ) == false )
            {
                if ( Page.IsPostBack == true )
                {
                    resultsPanel.Visible = false;
                    SetConsoleInfoMessage( "No records were found for the provided search criteria" );
                    ddlPageSizeList.Enabled = false;
                    pager1.Visible = false;
                    pager2.Visible = false;
                    formGrid.DataSource = null;
                    formGrid.DataBind();
                }
            }
            else
            {
                resultsPanel.Visible = true;
                ddlPageSizeList.Enabled = true;

                DataTable dt = ds.Tables[ 0 ];
                //DataView dv = ( ( DataTable ) dt ).DefaultView;
                //if ( sortTerm.Length > 0 )
                //    dv.Sort = sortTerm;

                if ( pTotalRows > currentPageSize )
                {
                    //formGrid.PagerSettings.Visible = true;
                    pager1.Visible = pager2.Visible = true;
                }
                else
                {
                    pager1.Visible = pager2.Visible = false;
                }


                //populate the grid
                formGrid.Visible = true;
                formGrid.DataSource = dt;
                formGrid.DataBind();


            }

        }
        catch ( System.Exception ex )
        {
            //Action??		- display message and close form??	
            LoggingHelper.LogError( ex, thisClassName + ".DoSearch() - Unexpected error encountered while attempting search. " );
            if ( ex.Message.ToLower().IndexOf( "timeout period elapsed prior" ) > -1 )
            {
                this.SetConsoleErrorMessage( "Sorry your query was taking too long and the timeout period has elaspsed<br>You could try again and use some for the checkbox values to improve the search capability.<br>" );
                pager1.Visible = false;
                pager2.Visible = false;
            }
            else
            {
                this.SetConsoleErrorMessage( "Unexpected error encountered<br>Close this form and try again.<br>" + ex.ToString() );
            }
        }
        DateTime endTime = System.DateTime.Now;
        TimeSpan duration = endTime.Subtract( startTime );

        if ( this.IsTestEnv() )
        {
            this.SetConsoleInfoMessage( "duration: " + duration.TotalSeconds.ToString() );
        }

    }	//

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
    /// call for an initial search
    /// </summary>
    /// <returns></returns>
    protected string FormatFilter()
    {

        string filter = "";
        //if ( CustomFilter.Length > 0 )
        //{
        //    //skip for now, checking below==> address with future custom filters
        //    filter = CustomFilter;
        //}
        //else
        //{
            if ( IsPersonalLibraryView )
                filter = string.Format( "(LibraryTypeId = 1 and LibraryCreatedById = {0}) ", WebUser.Id );
            else
            {
                //may be adding option to search against multiple libraries
                filter = string.Format( "(lib.LibraryId = {0}) ", CurrentLibraryId );
            }
        //}
        filterDesc = "";

        string booleanOperator = "AND";
        int sectionId = this.GetRequestKeyValue( "sId", 0 );
        if ( sectionId > 0 )
            filter += DatabaseManager.FormatSearchItem( filter, "lib.LibrarySectionId", sectionId, booleanOperator );
        //
        FormatDatesFilter( booleanOperator, ref filter );
        //
        SearchController.FormatAccessRightsFilter( cbxlAccessRights, booleanOperator, ref filter, ref filterDesc );
        SearchController.FormatSubselectFilter( cbxlAudience, "[Resource.IntendedAudience]", "AudienceId", booleanOperator, ref filter, ref filterDesc );

        SearchController.FormatSubselectFilter( cbxGradeLevel, "[Resource.GradeLevel]", "GradeLevelId", booleanOperator, ref filter, ref filterDesc );

        SearchController.FormatSubselectFilter( cbxlResType, "[Resource.ResourceType]", "ResourceTypeId", booleanOperator, ref filter, ref filterDesc );
        //
        FormatHasStandardsFilter( cbxlCCSS, booleanOperator, ref filter, ref filterDesc );
        //
        //formats
        SearchController.FormatSubselectFilter( cbxlFormats, "[Resource.Format]", "CodeId", booleanOperator, ref filter, ref filterDesc );

        FormatKeyword( txtKeyword, booleanOperator, ref filter );

        if ( this.IsTestEnv() || GetRequestKeyValue( "debug", "" ) == "yes" )
        {
            this.SetConsoleSuccessMessage( "sql: " + filter );
            LoggingHelper.DoTrace( 6, "sql: " + filter );
        }
        return filter;
    }	//

    protected void FormatKeyword( TextBox textBox, string booleanOperator, ref string filter )
    {
        string keyword = DataBaseHelper.HandleApostrophes( FormHelper.SanitizeUserInput( textBox.Text.Trim() ) );
        string keywordFilter = "";

        if ( keyword.Length > 0 )
        {
            filterDesc = filterDesc + "<div class='searchSection isleBox'>" + keyword + "</div>";
            keyword = keyword.Replace( "*", "%" );
            if ( keyword.IndexOf( "," ) > -1 )
            {
                string[] phrases = keyword.Split( new char[] { ',' } );
                foreach ( string phrase in phrases )
                {
                    string next = phrase.Trim();
                    if ( next.IndexOf( "%" ) == -1 )
                        next = "%" + next + "%";
                    string where = string.Format( this.keywordTemplate.Text, next );
                    keywordFilter += DataBaseHelper.FormatSearchItem( keywordFilter, where, "OR" );
                }
            }
            else
            {
                if ( keyword.IndexOf( "%" ) == -1 )
                    keyword = "%" + keyword + "%";

                keywordFilter = string.Format( keywordTemplate.Text, keyword );

            }

            if ( keywordFilter.Length > 0 )
                filter += DataBaseHelper.FormatSearchItem( filter, keywordFilter, booleanOperator );
        }
    }	//
    private void FormatDatesFilter( string booleanOperator, ref string filter )
    {


        DateTime endDate;
        if ( rblIDateCreated.SelectedIndex == 0 )
        {

            endDate = System.DateTime.Now.AddDays( -7 );
        }
        else if ( rblIDateCreated.SelectedIndex == 1 )
        {
            endDate = System.DateTime.Now.AddDays( -30 );
        }
        else if ( rblIDateCreated.SelectedIndex == 2 )
        {
            endDate = System.DateTime.Now.AddMonths( -6 );
        }
        else if ( rblIDateCreated.SelectedIndex == 3 )
        {
            endDate = System.DateTime.Now.AddYears( -1 );
        }
        else
        {
            return;
        }

        string where = string.Format( " lib.ResourceCreated > '{0}'", endDate.ToString( "yyyy-MM-dd" ) );
        filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
        string selDesc = string.Format( " Added after {0}", endDate.ToString( "yyyy-MM-dd" ) );
        filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
    }

    private void FormatHasStandardsFilter( CheckBoxList cbxl, string booleanOperator, ref string filter, ref string filterDesc )
    {
        //future: any math or any lit
        //also could build the cbxl with totals
        string selDesc = "Has any CCSS standard";

        //should only be one for now
        bool isSelected = false;
        foreach ( ListItem li in cbxl.Items )
        {
            if ( li.Selected )
            {
                isSelected = true;
            }
        }
        if ( isSelected )
        {
            string where = resStandardFilter.Text;
            filter += DataBaseHelper.FormatSearchItem( filter, where, booleanOperator );
            filterDesc = filterDesc + "<div class='searchSection isleBox'>" + selDesc + "</div>";
        }
    }
    /// <summary>
    /// no longer used
    /// </summary>
    /// <returns></returns>
    protected void formGrid_RowDataBound( object sender, GridViewRowEventArgs e )
    {

        if ( e.Row.RowType == DataControlRowType.DataRow )
        {

        }
   }//

    public string CleanTitle( string text )
    {
        if ( string.IsNullOrEmpty( text ) )
            return  "";
        else
            return new PublishController().FormatFriendlyTitle( text );
    }//

    public string HandleResourceCounts( string LikeCount, string DisLikeCount, string NbrEvaluations, string NbrComments, string NbrStandards )
    {
        string list = "";    
        string template = "<li>{0} <img src=\"/images/icons/{1}.png\" alt='{2}' title='{2}' /></li>";

        int ctr = 0;
        if ( int.TryParse( LikeCount, out ctr ) )
        {
            if ( ctr > 0)
                list += string.Format( template, LikeCount, "icon_likes", "Likes" );
        }
        if ( int.TryParse( DisLikeCount, out ctr ) )
        {
            if ( ctr > 0 )
                list += string.Format( template, DisLikeCount, "icon_dislikes", "Likes" );
        }
        if ( int.TryParse( NbrEvaluations, out ctr ) )
        {
            if ( ctr > 0 )
                list += string.Format( template, NbrEvaluations, "icon_ratings", "Evaluations" );
        }
        if ( int.TryParse( NbrComments, out ctr ) )
        {
            if ( ctr > 0 )
                list += string.Format( template, NbrComments, "icon_comments", "Comments" );
        }
        if ( int.TryParse( NbrStandards, out ctr ) )
        {
            if ( ctr > 0 )
                list += string.Format( template, NbrStandards, "icon_standards", "Standards" );
        }

        if ( list.Length == 0 )
            list = "<li>&nbsp;</li>";

        return list;
    }//


    public string RenderThumbnailURL( string url )
    {
        string url2pngResult = URL2PNG.GetThumbnail( url, 200, 1024, 768 );
        if ( url2pngResult != null && url2pngResult.Length > 0 )
            return string.Format("<img src='{0}' id='thumbnail' />", url2pngResult);
        else
            return "Image Not Available";
    }//



    public string FormatRightsUrl( string url )
    {
        string formatted = "";
        /*
        if ( url == null || url.Trim().Length < 15 )
            return "";
        if ( url.Trim().ToLower().IndexOf( "http://" ) != 0 )
            return "See details";
        */

        //TODO - extract domain name from url
        if ( url.ToLower().IndexOf( "http://creativecommons.org/licenses/" ) > -1 )
        {
            formatted = FormatCCRightsUrl( url.Trim() );
        }
        else
        {
            string sqlGetRights = "SELECT TOP 1 [MiniIconUrl] FROM [ConditionOfUse] WHERE [Url] IS NULL";
            DataSet ds = DatabaseManager.DoQuery( string.Format( sqlGetRights, url ) );
            formatted = string.Format( formattedCCRightsUrl2.Text, url, "<img src=\"" + DatabaseManager.GetRowPossibleColumn( ds.Tables[ 0 ].Rows[ 0 ], "MiniIconUrl" ) + "\" />" );
            //formatted = string.Format( formattedSourceTemplate.Text, url.Trim(), "Custom Rights" );
        }

        return formatted;
    }
    protected string FormatCCRightsUrl( string url )
    {
        string sqlGetRights = "SELECT TOP 1 [MiniIconUrl] FROM [ConditionOfUse] WHERE [Url] = '{0}'";
        DataSet ds = DatabaseManager.DoQuery( string.Format( sqlGetRights, url ) );
        if ( DoesDataSetHaveRows( ds ) )
        {
            return string.Format( formattedCCRightsUrl2.Text, url, "<img src=\"" + DatabaseManager.GetRowPossibleColumn( ds.Tables[ 0 ].Rows[ 0 ], "MiniIconUrl" ) + "\" />" );
        }
        else
        {
            if ( url.Trim().Length > 0 )
                return string.Format( formattedCCRightsUrl2.Text, url, "CC Rights" );
            else
                return url;
        }
        //string formatted = string.Format( formattedCCRightsUrl2.Text, url, "Creative Commons" );
        //return formatted;
    }

    public string CleanDescription( string description, int characters )
    {
        if ( description.Trim().Length < characters )
            return description;

        string sentence = "";
        string suffix = readMoreTemplate.Text;
        if ( characters > 0 )
        {
            sentence = description.Substring( 0, ( description.Length >= characters ? characters : description.Length ) );
            sentence = HandlePartialDesc( sentence );
        }
        else
        {
            sentence = description;
            suffix = "";
        }


        if ( sentence.StartsWith( "&nbsp;" ) )
            sentence = sentence.Substring( 5, sentence.Length - 6 );
        sentence = sentence.Replace( "&nbsp;", " " );
        sentence = sentence.Replace( "&lt;p&gt;", "" );
        sentence = sentence.Replace( "&lt;/p&gt;", "" );
        sentence = sentence.Replace( "&amp;nbsp;", " " );

        return sentence + suffix;
        // }
    }
    private string HandlePartialDesc( string sentence )
    {
        //need to handle special tags to ensure end tags are included
        sentence = sentence.Replace( "<br>", " " );
        sentence = sentence.Replace( "<br/>", " " );

        sentence = StripTag( sentence, "em" );
        sentence = StripTag( sentence, "span" );
        sentence = StripTag( sentence, "b" );
        sentence = StripTag( sentence, "p" );
        sentence = StripTag( sentence, "div" );
        sentence = StripTag( sentence, "font" );

        //TODO - handle embedded anchors - may have a method in utilityManager already - also XHTML page
        // - also consider doing all stripping of tags before getting the first 100 chars


        if ( sentence.ToLower().IndexOf( "<img" ) > -1 )
        {
            //remove img
            int pos1 = sentence.ToLower().IndexOf( "<img" );
            int pos2 = sentence.ToLower().IndexOf( ">", pos1 );
            if ( pos2 > pos1 )
            {
                //extract image
                sentence = sentence.Substring( 0, pos1 - 1 ) + " " + sentence.Substring( pos2 );
            }
            else
            {
                //tag is incomplete, only include up to tag start 
                sentence = sentence.Substring( 0, pos1 - 1 );
            }
        }

        // back up to last blank char
        int pos3 = sentence.LastIndexOf( " " );
        if ( pos3 > 50 )
            sentence = sentence.Substring( 0, pos3 );
        return sentence;
    }
    private string StripTag( string text, string tag )
    {
        string result = text.Replace( "<" + tag + ">", "" );
        result = result.Replace( "</" + tag + ">", "" );

        return result;
    }

    /// <summary>
    /// Reset selected item on sort
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="ex"></param>
    protected void formGrid_Sorted( object sender, EventArgs ex )
    {
        //clear selected index
        formGrid.SelectedIndex = -1;

    }	//
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
            }
            else
            {
                GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
                sortTerm = newSortExpression + " ASC";
            }
        }
        else
        {
            GridViewSortDirection = System.Web.UI.WebControls.SortDirection.Ascending;
            GridViewSortExpression = newSortExpression;
            sortTerm = newSortExpression + " ASC";
        }
        if ( newSortExpression.ToLower() != "sorttitle" )
        {
            sortTerm += ", SortTitle ASC";
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
        DataSet ds1 = DatabaseManager.GetCodeValues( "GridPageSize", "SortOrder" );
        DatabaseManager.PopulateList( this.ddlPageSizeList, ds1, "StringValue", "StringValue", "Select Size" );

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

            DoSearch( 0, sortTerm );

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
        SetSubscriptionsList();
        SetCollectionActions( true );
  } //



    private void SetCollectionActions( bool isEnabled )
    {
//        bool isEnabled = true;

        ddlCheckOptions.Items.Clear();
        ddlCheckOptions.Items.Add( new ListItem( "Action", "" ) );
        ddlCheckOptions.Items.Add( new ListItem( "Copy", "1" ) );

        ddlCheckOptions.Items.Add( new ListItem( "Move", "2", isEnabled ) );
        ddlCheckOptions.Items.Add( new ListItem( "Delete", "3", isEnabled ) );

    } //

   

    private void SetSubscriptionsList()
    {

        DataSet ds = myManager.SelectSubscriptionTypes();
        DataBaseHelper.PopulateList( this.ddlLibrarySubscriptions, ds, "Id", "Title", "Select a subscription type" );
        DatabaseManager.PopulateList (this.ddlCollectionSubscriptions, ds, "Id", "Title", "Select a subscription type");


    }
  

    void PopulateFilters( int libraryId )
    {
        if ( usingWSToPopulateFilters.Text == "yes" )
        {
            string filter = string.Format( accessRightsFilter.Text, libraryId );
            mec.CodeTableSearch( "Codes.AccessRights", "Id", "Title", "SortOrder", filter );

            filter = string.Format( audienceFilter.Text, libraryId );
            mec.CodeTableSearch( "Codes.AudienceType", "Id", "Title", "Title", filter );

            filter = string.Format( gradeLevelFilter.Text, libraryId );
            mec.CodeTableSearch( "Codes.GradeLevel", "Id", "Title", "SortOrder", filter );

            filter = string.Format( resTypeFilter.Text, libraryId );
            mec.CodeTableSearch( "Codes.ResourceType", "Id", "Title", "Title", filter );

            filter = string.Format( resFormatFilter.Text, libraryId );
            mec.CodeTableSearch( "Codes.ResourceFormat", "Id", "Title", "Title", filter );
        }
        else
        {
            string filter = string.Format( accessRightsFilter.Text, libraryId );
            ResourceCheckBoxFiller.FillCheckBoxList( cbxlAccessRights, "Codes.AccessRights", "Title", "SortOrder", filter );

            filter = string.Format( audienceFilter.Text, libraryId );
            ResourceCheckBoxFiller.FillCheckBoxList( cbxlAudience, "Codes.AudienceType", "Title", "Title", filter );

            filter = string.Format( gradeLevelFilter.Text, libraryId );
            ResourceCheckBoxFiller.FillCheckBoxList( cbxGradeLevel, "Codes.GradeLevel", "Title", "SortOrder", filter );

            filter = string.Format( resTypeFilter.Text, libraryId );
            ResourceCheckBoxFiller.FillCheckBoxList( cbxlResType, "Codes.ResourceType", "Title", "Title", filter );

            filter = string.Format( resFormatFilter.Text, libraryId );
            ResourceCheckBoxFiller.FillCheckBoxList( cbxlFormats, "Codes.ResourceFormat", "Title", "Title", filter );
        }
    }
   
    void mec_GetCodesNoComponents( object sender, EventArgs e )
    {
        //SetConsoleErrorMessage( "mec_GetCodesNoComponents - for which" );
    }

    void mec_GetCodesFailure( object sender, EventFailureEventArgs e )
    {
        SetConsoleErrorMessage( "Get Codes Failure - for which" );
    }

    /// <summary>
    /// may need a specific event handler to pass the correct table names back????
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void mec_GetCodesSuccess( object sender, EventArgs e )
    {
        if ( mec.CodesList.Length > 0 )
        {
            if ( mec.CodesList[ 0 ].TableName == "Codes.AccessRights" )
            {
                //pop, just how trustworthy????
                PopulateCheckboxList( mec.CodesList, cbxlAccessRights );

            }
            else if ( mec.CodesList[ 0 ].TableName == "Codes.AudienceType" )
            {
                //
                PopulateCheckboxList( mec.CodesList, cbxlAudience );
            }
            else if ( mec.CodesList[ 0 ].TableName == "Codes.GradeLevel" )
            {
                //
                PopulateCheckboxList( mec.CodesList, cbxGradeLevel );
            }
            else if ( mec.CodesList[ 0 ].TableName == "Codes.ResourceType" )
            {
                //
                PopulateCheckboxList( mec.CodesList, cbxlResType );
            }
            else if ( mec.CodesList[ 0 ].TableName == "Codes.ResourceFormat" )
            {
                //
                PopulateCheckboxList( mec.CodesList, this.cbxlFormats );
            }

        }
        else
        {
            SetConsoleErrorMessage( "No records were found for the provided search criteria" );

        }
    }//

    private void PopulateCheckboxList( bizService.CodesDataContract[] list, CheckBoxList cbxl )
    {
        //ensure cbls has values

        foreach ( CodesContract dr in list )
        {
            ListItem item = new ListItem();
            item.Text = dr.Title;
            item.Value = dr.Id.ToString();
            if ( item.Value != "0" )
                cbxl.Items.Add( item );
        }
        if ( cbxl.Items.Count > 0 )
            cbxl.Enabled = true;

    }//

    #endregion

    }

}
