using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ILPathways.Library;
using ILPathways.Business;
using Isle.BizServices;
using LRWarehouse.DAL;
using ILPathways.Services;
using System.Web.Script.Serialization;
using LRWarehouse.Business;

using System.Data;

namespace ILPathways.Controls.Curriculum
{
  public partial class CurriculumEditor1 : BaseUserControl
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
    protected ContentItem CurrentRecord
    {
      get { return ViewState[ "CurrentRecord" ] as ContentItem; }
      set { ViewState[ "CurrentRecord" ] = value; }
    }

    //ID of current curriculum
    public int curriculumID { get; set; }

    //Indicates whether current node is curriculum node
    public bool isTopLevelNode { get; set; }

    //Preexisting standards
    public List<CurriculumService1.StandardDTO> currentStandards { get; set; }
    //JSON array of Preexisting standard IDs
    public string currentStandardsJSON { get; set; }

    //ID of current node Parent. 0 if top level node
    public int nodeParentID { get; set; }

    //Attachments for this node
    public string currentAttachmentsJSON { get; set; }

    #endregion

    #region Initialization
    public CurriculumEditor1()
    {
      CurrentRecord = new ContentItem();
    }

    protected void Page_Load( object sender, EventArgs e )
    {
      //Must load these code tables first
      LoadPermissionDDLs();
      LoadParameters();
      LoadStandards();
      LoadAttachments();
      HandleTopLevelItems();
    }

    //Get parameters from the request
    private void LoadParameters()
    {
      var user = new Patron();

      if ( IsUserAuthenticated() )
      {
        user = (Patron) WebUser;
      }

      CurrentRecord = curriculumServices.GetACurriculumNode( int.Parse( Request.Params[ "node" ] ?? Page.RouteData.Values[ "node" ].ToString() ), user.Id );
      curriculumID = curriculumServices.GetCurriculumIDForNode( CurrentRecord );
      if ( curriculumID == 0 )
      {
        curriculumID = CurrentRecord.Id;
      }
      nodeParentID = CurrentRecord.ParentId;

      txtTitle.Value = CurrentRecord.Title;
      txtDescription.Value = CurrentRecord.Description;
      txtTimeframe.Value = CurrentRecord.Timeframe;
      ddlNodePermissions.Items.FindByValue( CurrentRecord.PrivilegeTypeId.ToString() ).Selected = true;
      //cbxlK12Subject
      //cbxlGradeLevel
    }

    //Populate the drop-down lists of user permissions
    private void LoadPermissionDDLs()
    {
      DataSet ds = new ContentServices().ContentPrivilegeCodes_Select();
      BaseDataManager.PopulateList( ddlNodePermissions, ds, "Id", "Title" );
      BaseDataManager.PopulateList( ddlAttachmentPermissions, ds, "Id", "Title" );
      BaseDataManager.PopulateList( ddlAttachmentTemplatePermissions, ds, "Id", "Title" );
      if ( CurrentRecord.Id > 0 )
      {
        ddlNodePermissions.Items.FindByValue( CurrentRecord.PrivilegeTypeId.ToString() ).Selected = true;
      }
    }

    //Get the current Standards
    private void LoadStandards()
    {
      //currentStandards = curriculumWebService.GetNodeStandards( CurrentRecord, false );
      currentStandards = curriculumWebService.GetContentStandardsData( CurrentRecord.Id );
      currentStandardsJSON = serializer.Serialize( currentStandards );
    }

    //Get attachments 
    private void LoadAttachments()
    {
      var attachments = curriculumWebService.GetAttachments( CurrentRecord );
      currentAttachmentsJSON = serializer.Serialize( attachments.data );
    }

    //Do stuff only at curriculum level
    private void HandleTopLevelItems()
    {
      if ( curriculumID == CurrentRecord.Id )
      {
        newsItemsContainer.Visible = true;
        newsItemsTabContainer.Visible = true;
      }
      else
      {
        newsItemsContainer.Visible = false;
        newsItemsTabContainer.Visible = false;
      }
    }

    #endregion

    #region Click Handlers

    #endregion
  }
}