using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using IOER.Library;
using Isle.BizServices;
using System.Web.Script.Serialization;
using IOER.Services;
using ILPathways.Business;
using LRWarehouse.DAL;
using LRWarehouse.Business;
using LRWarehouse.Business.ResourceV2;

namespace IOER.Controls.Curriculum
{
  public partial class CurriculumEditor2 : BaseUserControl
  {
    CurriculumService1 curriculumWebService = new CurriculumService1();
    CurriculumServices curriculumServices = new CurriculumServices();
    JavaScriptSerializer serializer = new JavaScriptSerializer();

    #region Properties
    //Set value used when check form privileges
    public string FormSecurityName
    {
      get { return this.txtFormSecurityName.Text; }
      set { this.txtFormSecurityName.Text = value; }
    }

    //Get/Set current Content item
    protected ContentItem currentNode
    {
      get { return ViewState[ "currentNode" ] as ContentItem; }
      set { ViewState[ "currentNode" ] = value; }
    }

    //Preexisting standards
    public List<CurriculumService1.StandardDTO> nodeStandards { get; set; }
    //JSON array of Preexisting standard IDs
    public string nodeStandardsJSON { get; set; }

    //ID of current curriculum
    public int curriculumID { get; set; }

    //ID of current node Parent. 0 if top level node
    public int nodeParentID { get; set; }

    //Attachments for this node
    public string nodeAttachmentsJSON { get; set; }

    //List of siblings of the current node
    public List<Isle.DTO.ContentNode> nodeSiblings { get; set; }

		//Usage Rights list
		public List<UsageRights> UsageRightsList { get; set; }

    //Nearby nodes
    public int previousNodeID { get; set; }
    public int nextNodeID { get; set; }
    public int previousNodeSortOrder { get; set; }
    public int nextNodeSortOrder { get; set; }
    public int outdentNodeID { get; set; }
    public int outdentSortID { get; set; }

    #endregion

    #region Initialization

    public CurriculumEditor2()
    {
      currentNode = new ContentItem();
    }

	protected void Page_Load(object sender, EventArgs e)
	{
		//Get started
		if (IsPostBack)
		{
			CreateNewCurriculum();
			return;
		}

		//Determine mode
		curriculumError.Visible = false;
		var node = Request.Params["node"] ?? Page.RouteData.Values["node"].ToString();

		//Reject if user isn't logged in
		//shouldn't this be done earlier? MP moved 15-09-20
		if (!IsUserAuthenticated())
		{
			Response.Redirect("/Account/Login.aspx?nextUrl=/My/LearningList/" + node);
		}

		if (node == "new")
		{
			//Show the create interface
			curriculumEditor.Visible = false;
			curriculumStarter.Visible = true;
			LoadOrganizationDDL();
		}
		else
		{
			curriculumStarter.Visible = false;
			curriculumEditor.Visible = true;

			//Must load these code tables first
			LoadPermissionDDLs();
			UsageRightsList = new ResourceV2Services().GetUsageRightsList();

			//Load parameters, includes check for requested id
			if (!LoadParameters())
			{
				//kill if this fails
				return;
			}

			LoadStandards();
			LoadAttachments();
			LoadNodeSiblings();
			LoadUserManager();

		}



	}

    //Get Started
    private void CreateNewCurriculum()
    {
      //Get data
      var title = txtStarterTitle.Value;
      var description = txtStarterDescription.Value;
      var organizationID = int.Parse( ddlOrganization.Items[ ddlOrganization.SelectedIndex ].Value );
      var utilityService = new UtilityService();
      var overallValid = true;
      var valid = true;
      var status = "";

      //Validate data
      title = utilityService.ValidateText( title, 5, "Title", ref valid, ref status );
      if ( !valid )
      {
        SetConsoleErrorMessage( status );
        overallValid = false;
      }
      description = utilityService.ValidateText( description, 10, "Description", ref valid, ref status );
      if ( !valid )
      {
        SetConsoleErrorMessage( status );
        overallValid = false;
      }
      if ( !overallValid )
      {
        return;
      }
      
      //Create curriculum
      var data = new CurriculumService1().Curriculum_Create( title, description, organizationID );
      if ( data.valid )
      {
        //NOTE: the Curriculum_Create process results in adding creator as a content.partner administrator

        Response.Redirect( "/my/learninglist/" + data.data );
      }
      else
      {
        SetConsoleErrorMessage( data.status );
        return;
      }
    }

    //Allow the user to choose the org to create the curriculum as a part of
	private void LoadOrganizationDDL()
	{
		if ( IsUserAuthenticated() )
		{
			ddlOrganization.Items.Add( new ListItem()
			{
				Value = "0",
				Text = "No organization"
			} );
			if ( WebUser.OrgMemberships == null )
			{
				addToOrg.Visible = false;
			}
			else
			{
				foreach ( var item in WebUser.OrgMemberships )
				{
					ddlOrganization.Items.Add( new ListItem()
					{
						Value = item.Id.ToString(),
						Text = item.Organization
					} );
				}
			}
		}
	}
	private void LoadOrganizationDDL2()
	{
		if ( IsUserAuthenticated() )
		{
			ddlOrganization2.Items.Add( new ListItem()
			{
				Value = "0",
				Text = "No organization"
			} );
			if ( WebUser.OrgMemberships == null )
			{
				//will there be an issue where a user from diff org could be given access?
				addToOrg.Visible = false;
			}
			else
			{
				foreach ( var item in WebUser.OrgMemberships )
				{
					ddlOrganization2.Items.Add( new ListItem()
					{
						Value = item.Id.ToString(),
						Text = item.Organization
					} );
				}
				if ( currentNode.OrgId > 0 )
				{
					ddlOrganization2.Items.FindByValue( currentNode.OrgId.ToString() ).Selected = true;
				}
			}
		}
	}

    //Populate the drop-down lists of user permissions
    private void LoadPermissionDDLs()
    {
      DataSet ds = new ContentServices().ContentPrivilegeCodes_Select();
      BaseDataManager.PopulateList( ddlNodePermissions, ds, "Id", "Title" );
      BaseDataManager.PopulateList( ddlAttachmentPermissions, ds, "Id", "Title" );
      BaseDataManager.PopulateList( ddlAttachmentTemplatePermissions, ds, "Id", "Title" );
      if ( currentNode.Id > 0 )
      {
        ddlNodePermissions.Items.FindByValue( currentNode.PrivilegeTypeId.ToString() ).Selected = true;
      }
    }

	//Get parameters from the request
	private bool LoadParameters()
	{
		var user = new Patron();

		if ( IsUserAuthenticated() )
		{
			user = ( Patron ) WebUser;
		}

		try
		{
			int requestedId = int.Parse( Request.Params[ "node" ] ?? Page.RouteData.Values[ "node" ].ToString() );
			currentNode = curriculumServices.GetCurriculumNodeForEdit( requestedId, user );
			if ( currentNode.Id == 0 )
			{
				throw new ArgumentException( "Invalid Level ID" );
			}
			if ( currentNode.ImageUrl.ToLower().IndexOf( "ioer.ilsharedlearning.org/oerthumbs" ) > -1 )
			{
				//appears to be the default image for a published item. Remove for editing
				currentNode.ImageUrl = "";
			}
			curriculumID = curriculumServices.GetCurriculumIDForNode( currentNode );
		}
		catch ( Exception ex )
		{
			curriculumError.Visible = true;
			curriculumStarter.Visible = false;
			curriculumEditor.Visible = false;
			curriculumErrorMessage.InnerHtml = "Error: Invalid Level ID. Please check the URL and try again.";
			curriculumErrorMessageHidden.InnerHtml = ex.Message;
			return false;
		}

		if ( curriculumID == 0 )
		{
			curriculumID = currentNode.Id;
		}
		nodeParentID = currentNode.ParentId;

		txtTitle.Value = currentNode.Title;
		txtDescription.Value = currentNode.Summary;
		txtTimeframe.Value = currentNode.Timeframe;
		ddlNodePermissions.Items.FindByValue( currentNode.PrivilegeTypeId.ToString() ).Selected = true;

		if ( curriculumID == currentNode.Id )
		{
			LoadOrganizationDDL2();
		}

		lblHistory.Text = currentNode.HistoryTitle();
		//cbxlK12Subject
		//cbxlGradeLevel

		return true;
	}

    //Get the current Standards
    private void LoadStandards()
    {
      //nodeStandards = curriculumWebService.GetNodeStandards( currentNode, false );
      nodeStandards = curriculumWebService.GetContentStandardsData( currentNode.Id );
      nodeStandardsJSON = serializer.Serialize( nodeStandards );
    }

    //Get attachments 
    private void LoadAttachments()
    {
      var attachments = curriculumWebService.GetAttachments( currentNode );
      nodeAttachmentsJSON = serializer.Serialize( attachments.data );
    }

    //Get sibling nodes
    private void LoadNodeSiblings()
    {
      //Get siblings
      nodeSiblings = curriculumServices.GetCurriculumOutlineForEdit( currentNode.ParentId ).ChildNodes.OrderBy(m => m.SortOrder).ToList();
      //Get stuff for previous node
      var previousNode = nodeSiblings.Where( m => m.SortOrder < currentNode.SortOrder ).LastOrDefault();
      previousNodeID = previousNode == null ? -1 : previousNode.Id;
      previousNodeSortOrder = previousNode == null ? -1 : previousNode.SortOrder;
      //Get stuff for next node
      var nextNode = nodeSiblings.Where( m => m.SortOrder > currentNode.SortOrder ).FirstOrDefault();
      nextNodeID = nextNode == null ? -1 : nextNode.Id;
      nextNodeSortOrder = nextNode == null ? -1 : nextNode.SortOrder;
      //Get parent of parent node
      var parentNode = curriculumServices.Get( currentNode.ParentId );
      outdentNodeID = parentNode.ParentId;
      //Handle outdenting better
      outdentSortID = parentNode.SortOrder + 5;
    }

	//Load user manager
	private void LoadUserManager()
	{
		var manager = userManagerContainer.FindControl( "userManager" ) as Controls.ManageUsers;
		manager.ObjectId = currentNode.Id;
		manager.ObjectTitle = currentNode.Title;
		manager.ObjectTypeTitle = "Learning List";
		manager.ObjectType = "learninglist";
	}

    #endregion

  }
}