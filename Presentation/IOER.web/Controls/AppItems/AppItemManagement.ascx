<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AppItemManagement.ascx.cs" Inherits="ILPathways.Controls.AppItems.AppItemManagement" %>
<%@ Register
    Assembly="Obout.Ajax.UI"
    Namespace="Obout.Ajax.UI.HTMLEditor"
    TagPrefix="obout" %>
<%@ Register
    Assembly="Obout.Ajax.UI"
    Namespace="Obout.Ajax.UI.HTMLEditor.ContextMenu"
    TagPrefix="obout" %>
<%@ Register
    Assembly="Obout.Ajax.UI"
    TagPrefix="obout"
    Namespace="Obout.Ajax.UI.HTMLEditor.ToolbarButton" %>
<%@ Register
    TagPrefix="custom"
    Namespace="CustomToolbarButton" %>
<%@ Register assembly="Obout.Ajax.UI" namespace="Obout.Ajax.UI.HTMLEditor.Popups" tagprefix="obout" %>
<%@ Register namespace="CustomPopups" tagprefix="custom" %>
<%@ Register Assembly="Obout.Ajax.UI" Namespace="Obout.Ajax.UI.FileUpload" TagPrefix="obout" %>

<style type="text/css">
/*Textbox Watermark*/

.unwatermarked {
	height:18px;
	width:148px;
}

.watermarked {
	height:20px;
	width:150px;
	padding:2px 0 0 2px;
	border:1px solid #BEBEBE;
	background-color:#F0F8FF;
	color:gray;
}
.approveButton {
	width:150px;
	background-color:green; color: white;
}	
.unapproveButton {
	width:200px;
	background-color:whitesmoke; color: black;
}	
.dialogContent1 {
	border: solid 2px #fff; 
	width: 500px;
	height: 250px;
	margin-left:auto; margin-right:auto; margin-top:100px; 
	padding: 20px;
	background-color:#efefef;
	filter: progid:DXImageTransform.Microsoft.Gradient (GradientType=0,StartColorStr='#efefefef',EndColorStr='#ffffffff');
}

.dialogContent {
	border: solid 2px #fff; 
	width: 500px;
	height: 250px;
	margin-left:auto; margin-right:auto; margin-top:100px; 
	padding: 20px;
	background-color:blue;
	filter: alpha(opacity=100);
}
</style>

<script language="javascript" type="text/javascript">
<!--

  function HandleCommonUrlsListClick() {

    //var ddl = document.getElementById("ctl00_MainContent_pageUserControl_TabContainer1_tab3_ddlCommonUrlsList"); 
    var ddl = document.getElementById('<%=ddlCommonUrlsList.ClientID%>');

    var value2 = ddl.options[ddl.selectedIndex].value;

    //alert("HandleCommonUrlsList: " + value2) ;	

    //document.getElementById("ctl00_MainContent_pageUserControl_TabContainer1_tab3_txtRelatedUrl").value = value2; 
    document.getElementById('<%=txtRelatedUrl.ClientID%>').value = value2;

  } //end function   

  function confirmDelete(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to delete a record
    // Note - this could be made generic if the url is passed as well

    var bresult
    bresult = confirm("Are you sure you want to delete this record?\n\n"
            + "Click OK to delete this record or click Cancel to skip the delete.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }


  } //  
  function confirmDeleteImage(id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to delete a record
    // Note - this could be made generic if the url is passed as well

    var bresult
    bresult = confirm("Are you sure you want to delete this image?\n\n"
            + "Click OK to remove the image and delete from the database or click Cancel to skip the delete.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }

  } //  

  function confirmApprove(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to delete a record
    // Note - this could be made generic if the url is passed as well

    var bresult
    bresult = confirm("Are you sure you want to Approve this record?\n\n"
            + "Click OK to Approve this record or click Cancel to well cancel the Approve.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }


  } //  

  var $s = jQuery.noConflict();
  function openPopup() {
    $s("#newItemPopup").css("background-color", "Gray");
    $s("#newItemPopup").css("filter", "alpha(opacity=40)");
    $s("#newItemPopup").css("opacity", "0.95");
    $s("#newItemPopup").css("position", "Fixed");
    $s("#newItemPopup").css("border", "1px solid Silver");
    $s("#newItemPopup").css("z-index", "10000");
    $s("#newItemPopup").css("width", "100%");
    $s("#newItemPopup").css("height", "100%");
    $s("#newItemPopup").css("top", "0px");
    $s("#newItemPopup").css("left", "0px");
    $s("#newItemPopup").css("display", "block");
  }
  function hidePopup() {
    $s("#newItemPopup").css("background-color", "Gray");
    $s("#newItemPopup").css("filter", "alpha(opacity=40)");
    $s("#newItemPopup").css("opacity", "0.9");
    $s("#newItemPopup").css("position", "Fixed");
    $s("#newItemPopup").css("border", "1px solid Silver");
    $s("#newItemPopup").css("z-index", "10000");
    $s("#newItemPopup").css("width", "100%");
    $s("#newItemPopup").css("height", "100%");
    $s("#newItemPopup").css("top", "0px");
    $s("#newItemPopup").css("left", "0px");
    $s("#newItemPopup").css("display", "none");
  }
-->
</script>

<asp:TextBox ID="txtDefaultControl" Visible="false" runat="server"></asp:TextBox>
<br />

<div>
			<ajaxToolkit:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
			<ajaxToolkit:TabPanel ID="tab1" runat="server" HeaderText="Search">
				<ContentTemplate>
					<asp:Panel ID="searchPanel" runat="server">
					<div id="containerSearch">
							<h3>Search</h3>
						<div class="clearFloat"></div>			
						<div class="labelColumn" >Select an Item type</div>	
						<div class="dataColumn">   		
							<!-- Dropdownlist or grid -->
							<asp:DropDownList	id="lstForm" runat="server" Width="300px" AutoPostBack="true" onselectedindexchanged="lstForm_SelectedIndexChanged" ></asp:DropDownList>
						</div>
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn" > 
								<asp:label id="Label6"  associatedcontrolid="ddlSearchCategory" runat="server" >Select a Category</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlSearchCategory" Width="300px" runat="server" AutoPostBack="true" onselectedindexchanged="ddlSearchCategory_SelectedIndexChanged"></asp:dropdownlist>
							</div>						
						<!-- status -->
						<div class="clearFloat"></div>			
						<div class="labelColumn" >Select a Status</div>	
						<div class="dataColumn">   		
							<asp:DropDownList		 id="ddlStatusSearch" runat="server" Width="300px" ></asp:DropDownList>
						</div>  

						
  						<!-- keyword -->  
						<div class="clearFloat"></div>			
						<div class="labelColumn" ><a href="javascript:void(0);" onclick="ShowHideSection('divKeywordHelp');">
								<img alt="Keyword Instructions" src="/vos_portal/images/infoBubble.gif" />Keyword filter</a></div>	
						<div class="dataColumn">   		
							<asp:TextBox ID="txtKeyword" runat="server" Width="50%"></asp:TextBox>
							<br />
							<div id="divKeywordHelp" class="infoMessage" style="display: none; width: 80%;
								text-align: left; margin-top: 0;">
								An entered keyword will be used to search title, description and AppItem code.
							</div>
						</div>

						
						<!-- -->
						<div class="clearFloat"></div>	
						<p class="labelColumn">&nbsp;</p>
						<div class="dataColumn">			
								<asp:button id="btnSearch" runat="server" CssClass="defaultButton" width="100px" CommandName="Search" 
								OnCommand="FormButton_Click" Text="Search" causesvalidation="false"></asp:button>
								<button type="reset" class="defaultButton" title="Reset"  value="Reset" id="resetButton">Reset</button>
								&nbsp;&nbsp;&nbsp;<a href='#' onclick='openPopup();'>Add New Application Item</a>
						</div>		
						<div class="clearFloat"></div>  
					</div>
					</asp:Panel>					
					
				</ContentTemplate>
			</ajaxToolkit:TabPanel>
			<ajaxToolkit:TabPanel ID="tab2" Visible="true" runat="server"  HeaderText="Results" >		
				<ContentTemplate>
					<div class="tabPanelStyle" >
						<asp:Panel ID="resultsPanel" runat="server">
							<div class="clearFloat"></div>		
							<div style="width:100%">
							<div class="clearFloat" style="float:right;">	
								<div class="labelColumn" >Page Size</div>	
								<div class="dataColumn">     
										<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
								</div>  
							</div>
								<div style="float:left;">
									<wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
								</div>
							<div class="clearFloat"></div>	
								<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
									allowpaging="true" PageSize="20" allowsorting="true" 
									OnRowDataBound="formGrid_RowDataBound"
									OnRowCommand="formGrid_RowCommand" 
									OnPageIndexChanging="formGrid_PageIndexChanging"
									onsorting="formGrid_Sorting" 		
									BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="95%" 
									captionalign="Top" 
									useaccessibleheader="true" >
									<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
									<columns>
										<asp:TemplateField HeaderText="Select">
											<ItemTemplate>
											 <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("RowId") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
												 Select</asp:LinkButton>
											</ItemTemplate>
										</asp:TemplateField>
								<asp:TemplateField HeaderText="Delete">
							 <ItemTemplate>
								 <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("RowId") %>' CommandName="DeleteRow" CausesValidation="false" runat="server">
									 Delete</asp:LinkButton>
							 </ItemTemplate>
						 </asp:TemplateField>						
										<asp:boundfield datafield="AppItemTypeTitle" headertext="Type" sortexpression="AppItemTypeTitle"></asp:boundfield>							
										<asp:boundfield datafield="Category" headertext="Category" sortexpression="Category"></asp:boundfield>	
										<asp:boundfield datafield="SequenceNbr" headertext="Order" sortexpression="SequenceNbr" Visible="false"></asp:boundfield>		
												
										<asp:TemplateField HeaderText="Title/Code"  sortexpression="Title">
											<ItemTemplate>
												<%# Eval( "Title" )%><br /><%# Eval( "AppItemCode" )%>
											</ItemTemplate>
										</asp:TemplateField>		
																			
										<asp:boundfield datafield="status" headertext="Status" sortexpression="status"></asp:boundfield>		
										<asp:boundfield datafield="LastUpdated" headertext="Last Updated" sortexpression="LastUpdated"></asp:boundfield>										
									</columns>
									<pagersettings  visible="false" 
													mode="NumericFirstLast"
													firstpagetext="First"
													lastpagetext="Last"
													pagebuttoncount="5"  
													position="TopAndBottom"/> 
								
								</asp:gridview>
								
	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
	</div>									
								</div>			
							</asp:Panel>

					</div>
				</ContentTemplate>
			</ajaxToolkit:TabPanel>
			
			<ajaxToolkit:TabPanel ID="tab3" Visible="true" runat="server" HeaderText="Details">			
				<ContentTemplate>
					<div class="tabPanelStyle" >
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
						<a href='#' onclick='openPopup();'>Add New Application Item</a>
						<asp:Panel ID="detailsPanel" runat="server" Visible="False">

						<div id="containerDetails">
							<div class="clearFloat"></div>
              <div id="queryLinksDiv" runat="server" visible="False" style="float: left">
                  Reports
                  <ul>
                      <li>
                        <img class="linkImg" src="/vos_portal/images/newIcon.jpg" alt="New feature icon"
                            height="20" width="61" />&nbsp;<a id="summaryLink" href="/vos_portal/support/workNetQuery.aspx/workNetQuery.aspx?cd=EA_CustomerSummaryForGroup" runat="server" target="_blank">Success Stories Report - Published.<img class="linkImg"
                                    src="/vos_portal/images/link_NewExternal.gif" alt="External link opens in a new window"
                                    height="16" width="16" /></a> 
                      </li>
                  </ul>
              </div>
              <div class="clearFloat"></div>
              <br />
							<hr />
							<h3>AppItem Details</h3>
							<div class="clearFloat"></div>	
							<asp:button id="btnNew2" runat="server" CssClass="defaultButton" width="100px" CommandName="New" Visible="False" 
										OnCommand="FormButton_Click" Text="New Record" causesvalidation="False"></asp:button>
						<!-- --> 
            <asp:Panel ID="idPanel" runat="server" Visible="false">
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblRowId"  associatedcontrolid="txtRowId" runat="server">RowId</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:label  id="txtRowId" runat="server"></asp:label>
							</div>
              </asp:Panel>
						<!-- --> 
            <asp:Panel ID="versionPanel" runat="server" Visible="false">
						<div class="clearFloat"></div>
							<div class="labelColumn  requiredField" > 
								<asp:label id="lblVersionNbr"  associatedcontrolid="txtVersionNbr" runat="server">Version Nbr</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox id="txtVersionNbr" runat="server"></asp:textbox>
							</div>
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblSequenceNbr"  associatedcontrolid="txtSequenceNbr" runat="server">Order (in lists)</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox id="txtSequenceNbr" runat="server"></asp:textbox>
							</div>
              </asp:Panel>
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn  requiredField" > 
								<asp:label id="lblTypeId"  associatedcontrolid="ddlTypeId" runat="server">Type</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlTypeId" DataValueField="id" runat="server"></asp:dropdownlist>
								&nbsp;&nbsp;<asp:HyperLink ID="hlDisplayStory" runat="server" NavigateUrl="" Visible="false" Target="wnStory">Display Story</asp:HyperLink>
							</div>
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn  requiredField" > 
								<asp:label id="lblCategory"  associatedcontrolid="ddlCategory" runat="server">Category</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlCategory" Width="200px" OnSelectedIndexChanged="Category_OnSelectedIndexChanged" runat="server"></asp:dropdownlist>
							</div>
							<div style="float: left; color: #000; width:125px; text-align:right;">New Category</div>&nbsp;<asp:textbox  id="txtNewCategory" Width="150px" columns="100" MaxLength="50" runat="server"></asp:textbox>
							 
						<asp:Panel ID="subcategoryPanel" runat="server" Visible="false">
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblSubcategory"  associatedcontrolid="ddlSubcategory" runat="server">Subcategory</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlSubcategory" Width="200px" OnSelectedIndexChanged="Subcategory_OnSelectedIndexChanged" AutoPostBack="false" runat="server"></asp:dropdownlist>
							 </div>
							 <div style="float: left; color: #000; width:125px; text-align:right;">New SubCategory</div>&nbsp;<asp:textbox  id="txtNewSubcategory" Width="150px" columns="100" MaxLength="50" runat="server"></asp:textbox> 
</asp:Panel>							 
						<asp:Panel ID="faqSubcategoryPanel" runat="server" Visible="False">
							<!-- --> 
							<div class="clearFloat"></div>
							<div class="labelColumn requiredField" > 
								<asp:label id="Label5" associatedcontrolid="txtSubcategoryDesc" runat="server">Subcategory Description:*</asp:label> 
							</div>
							<div class="dataColumn"> 
							 <asp:TextBox ID="txtSubcategoryDesc" Width="70%" Columns="70" TextMode="MultiLine" Rows="3" MaxLength="500" runat="server"></asp:TextBox>
							</div>
						</asp:Panel>  
						
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn  requiredField" > 
								<asp:label id="lblStatus"  associatedcontrolid="ddlStatus" runat="server">Status</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlStatus" Width="200px" runat="server"></asp:dropdownlist>
							</div>
							<div style="float: left; color: #000; width:125px; text-align:right;">New Status</div>&nbsp;<asp:textbox  id="txtNewStatus" Width="150px" columns="100" MaxLength="50" runat="server"></asp:textbox> 
						  							
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn requiredField">
								<asp:label id="Label1"  associatedcontrolid="txtTitle" runat="server">Title</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox  id="txtTitle" Width="400px" Columns="70" TextMode="MultiLine" Rows="2" MaxLength="100" runat="server"></asp:textbox>  
							</div>

            <!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn  requiredField" > 
								<asp:label id="lblDescription"  associatedcontrolid="txtDescription" runat="server">Description</asp:label> 
							</div>
<div class="dataColumn"> 
                    <obout:EditorPopupHolder runat="server" id="popupHolder"  />
                		<obout:Editor ID="txtDescription" runat="server" Height="500px" Width="95%">
                      <EditPanel FullHtml="true" runat="server"></EditPanel>
                        <TopToolBar Appearance="Lite" >
                    <AddButtons>
                        <obout:HorizontalSeparator ID="HorizontalSeparator1" runat="server" />
               
                    </AddButtons>
                 </TopToolBar>
                       <BottomToolBar ShowDesignButton="true" ShowHtmlTextButton="true" ShowPreviewButton="true" ShowFullScreenButton="true" >
                 </BottomToolBar>
                     </obout:Editor>

								<br /><asp:button id="btnBadWordChecker" runat="server" CssClass="defaultButton" width="150px" CommandName="BadWordCheck" 
												 OnCommand="FormButton_Click" Text="Do BadWord Check" Visible="false"	CausesValidation="False"></asp:button>			
								<br /><div style="float: left"><a style="float: left" onclick="ShowHideSection('divTextDesc');" href="javascript:void(0);"><img alt="Show Html (to verify links etc). " src="/vos_portal/images/infoBubble.gif" />Show actual text version Do a save first.</a> <br /></div>		
								<div class="clearFloat"></div>
								<div id="divTextDesc" style="display: none; width:100%;padding-left:5px;">

								<asp:textbox id="lblDescText" ReadOnly="true" TextMode="MultiLine" Rows="10" Width="95%"  runat="server"></asp:textbox> 
								</div>
							</div>	
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblAppItemCode"  associatedcontrolid="txtAppItemCode" runat="server">Item Code</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox  maxLength="50"  id="txtAppItemCode" Width="400px" runat="server"></asp:textbox>
								<asp:Image ID="locktxtAppItemCode" runat="server" AlternateText="Field locked" ImageUrl="/vos_portal/images/sslicon.gif" Visible="False" />
							</div>
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="Label20"  associatedcontrolid="txtString3" runat="server">String 3</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox  maxLength="200"  id="txtString3" Width="400px" runat="server"></asp:textbox>
							</div>							
<div class="clearFloat"></div>							

			<!-- ********************** document ********************* --> 
			<asp:Panel ID="documentPanel" runat="server" Visible="true">
			<div class="clearFloat"></div>
			<hr />
				<div class="labelColumn  requiredField " > 
					<asp:label id="lblDocumentRowId"  associatedcontrolid="txtDocumentRowId" runat="server">Document</asp:label> 
				</div>
				<div class="dataColumn"> 
				<asp:HyperLink ID="lblDocumentLink" runat="server" Target="_blank" Visible="false">View File</asp:HyperLink>
		<br /><asp:HyperLink ID="lblServerDocumentLink" runat="server" Target="_blank" Visible="false">View File</asp:HyperLink>		
					<asp:textbox id="txtDocumentRowId" Visible="false"  runat="server"></asp:textbox>
				</div>
				<asp:Panel ID="documentTitlePanel" runat="server" Visible="true">  	</asp:Panel>	
				 <!-- --> 
				<div class="clearFloat"></div>
					<div class="labelColumn" ><asp:Label AssociatedControlID="txtFileTitle" runat="server" CssClass="requiredField" ID="Label8">New Document Title*:</asp:Label>
					</div>
					<div class="dataColumn"> 
						<asp:TextBox ID="txtFileTitle" Width="70%" Columns="60" runat="server"></asp:TextBox>
					</div>    
				
				 <!-- --> 
				<div class="clearFloat"></div>
				<div class="labelColumn" >
					<asp:Label AssociatedControlID="fileUpload" runat="server" CssClass="requiredField" ID="Label9">Document File*:</asp:Label>
				</div>
				<div class="dataColumn"> 
								<asp:FileUpload ID="fileUpload" Width="500px" runat="server" />
								<br /><asp:Label ID="currentFileName" runat="server" ></asp:Label>
								
								<br />Pdf documents only
				</div> 
</asp:Panel>				
			<!-- --> 	
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblIsActive"  associatedcontrolid="rblIsActive" runat="server">IsActive</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:RadioButtonList id="rblIsActive"   runat="server" tooltip="True: User is active" RepeatDirection="Horizontal">
							<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>    
 							<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
						</asp:RadioButtonList>
							</div>
						<asp:Panel ID="startDatePanel"  runat="server" Visible="False">
						<!-- assume start and end dates go together --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblStartDate"  associatedcontrolid="txtStartDate" runat="server">Start Date</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox id="txtStartDate" runat="server"></asp:textbox>
								<asp:ImageButton runat="server" ID="calImage1" ImageUrl="/vos_portal/images/Calendar.gif" AlternateText="Click to show calendar" />
		<ajaxToolkit:CalendarExtender ID="CalendarExtender2" runat="server" TargetControlID="txtStartDate" PopupButtonID="calImage1" Enabled="True" /> 								
							</div>
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblEndDate"  associatedcontrolid="txtEndDate" runat="server">End Date</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox id="txtEndDate" runat="server"></asp:textbox>
								<asp:ImageButton runat="server" ID="calImage2" ImageUrl="/vos_portal/images/Calendar.gif" AlternateText="Click to show calendar" />
		<ajaxToolkit:CalendarExtender ID="CalendarExtender1" runat="server" TargetControlID="txtEndDate" PopupButtonID="calImage2" Enabled="True" /> 
							</div>
						 
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblExpiryDate"  associatedcontrolid="txtExpiryDate" runat="server">Expiry Date</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox id="txtExpiryDate" runat="server"></asp:textbox>
							</div>
						</asp:Panel>  
						  
							<asp:Panel ID="approvalPanel"  runat="server" Visible="False">
						<!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn " > 
								<asp:label id="lblApproved"  associatedcontrolid="txtApproved" runat="server">Approved By</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:label id="txtApproved" runat="server"></asp:label>
								<br /><asp:button id="btnApprove" ForeColor="White" runat="server" CssClass="defaultButton"  CommandName="Approve" 
										OnCommand="FormButton_Click" Text="Approve"	CausesValidation="False"></asp:button> 
							</div>
							</asp:Panel>
							
<asp:Panel ID="imagePanel" runat="server">
						 <!-- --> 
						<div class="clearFloat"></div>
						<hr />	
						<h2>Upload an image (optional)</h2>
						<p>You may upload an image by clicking the "Browse" button,
									then selecting the file you want to upload.  Acceptable file types are .gif, .jpg, .png.</p>
							<div class="labelColumn" >&nbsp;</div>
							<div class="dataColumn" >Maximums: Width:&nbsp;<asp:TextBox ID="maxPicWidth" runat="server" Width="50px" style="margin-left: 15px;" ReadOnly="True">135</asp:TextBox>  
									&nbsp; Height:&nbsp;<asp:TextBox ID="maxPicHeight" runat="server" Width="50px" style="margin-left: 11px;" ReadOnly="True">120</asp:TextBox> 
   
							</div>

												    
						 <!-- --> 
						<div class="clearFloat"></div>
							<div class="labelColumn" ><asp:Label AssociatedControlID="txtImageTitle" runat="server" CssClass="requiredField" ID="lblImageTitle">Image Title*:</asp:Label>
							</div>
							<div class="dataColumn"> 
								<asp:TextBox ID="txtImageTitle" Width="70%" Columns="60" runat="server"></asp:TextBox>
							</div>    
						 <!-- --> 
						<div class="clearFloat"></div>
						<div class="labelColumn" >
							<asp:Label AssociatedControlID="imageFileUpload" runat="server" CssClass="requiredField" ID="lblImagePath">File name*:</asp:Label>
						</div>
						<div class="dataColumn"> 
										<asp:FileUpload ID="imageFileUpload" Width="408px" runat="server" />
						</div>  
						 <!-- --> 
						<div class="clearFloat"></div>
						<div class="labelColumn" >
							&nbsp;
						</div>
							<div class="dataColumn" style="background-color: #f5f5f5;"> 
								<a id="UploadPictureField"></a>
								<h3>Current image</h3>
								<asp:Image ID="imgCurrent" runat="server" Visible="False" />
								<br /><asp:label  id="lblImageId" runat="server"></asp:label>&nbsp;<asp:Label ID="txtImageFileName" runat="server" />
								<br /><asp:hyperlink  id="previewImageLink" visible="false" runat="server" Target="_blank">Preview image</asp:hyperlink>	
								<br /><asp:button id="btnDeleteImage" runat="server" CssClass="defaultButton" width="150px" CommandName="DeleteImage" 
												 OnCommand="FormButton_Click" Text="Remove Image"	CausesValidation="False"></asp:button> 
								<br /><asp:label  id="lblImageStatus" runat="server"></asp:label>
								<asp:Label ID="ImageWidthErrorMessage" runat="server" Visible="False">The <a href='#UploadPictureField'>image</a> @FileName you uploaded is too large.  It must be less than or equal to the listed maximums.</asp:Label>
								
							</div>
</asp:Panel>							
						<!-- -->
              <div class="clearFloat">
              </div>
              <div class="labelColumn">
                  <asp:Label ID="Label2" runat="server">History</asp:Label>
              </div>
              <div class="dataColumn">
                  <div style="float: left">
                      <a style="float: left" onclick="ShowHideSection('divCreateHistoryDetails');" href="javascript:void(0);">
                          <asp:Label ID="lblHistory" runat="server"></asp:Label></a>
                  </div>
                  <div class="clearFloat">
                  </div>
                  <div id="divCreateHistoryDetails" class="infoMessage" style="display: none; width: 70%;
                      padding-left: 5px;">
                      <asp:Label ID="lblCreatedHistoryDetails" Width="70%" runat="server"></asp:Label>
                  </div>
              </div>
              <!-- -->
              <div class="clearFloat">
              </div>
              <div class="labelColumn">
                  &nbsp;</div>
              <div class="dataColumn">
                  <div style="float: left">
                      <a style="float: left" onclick="ShowHideSection('divUpdateHistoryDetails');" href="javascript:void(0);">
                          <asp:Label ID="lblUpdatedHistory" runat="server"></asp:Label></a>
                  </div>
                  <div class="clearFloat">
                  </div>
                  <div id="divUpdateHistoryDetails" class="infoMessage" style="display: none; width: 70%;
                      padding-left: 5px;">
                      <asp:Label ID="lblUpdatedHistoryDetails" Width="70%" runat="server"></asp:Label>
                  </div>
              </div>
								<!-- -->
								<div class="clearFloat"></div>	
								<p class="labelColumn">&nbsp;</p>
								<div class="dataColumn">			
										<asp:button id="btnSave" runat="server" CssClass="defaultButton" width="100px" CommandName="Update" 
										OnCommand="FormButton_Click" Text="Save"></asp:button>
										<asp:button id="btnDelete" runat="server" CssClass="defaultButton" width="100px" CommandName="Delete" 
										OnCommand="FormButton_Click" Text="Delete"	CausesValidation="False" visible="False"></asp:button> 
										&nbsp;&nbsp;<asp:button id="btnNew" runat="server" CssClass="defaultButton" width="100px" CommandName="New" 
										OnCommand="FormButton_Click" Text="New Record" causesvalidation="False" Visible="False"></asp:button>
										<asp:Button ID="btnSendTest" runat="server" CssClass="defaultButton" CommandName="SendTest"
										OnCommand="FormButton_Click" Text="Send Test E-mail" CausesValidation="False" Visible="False" />
										<asp:Button ID="btnSend" runat="server" CssClass="defaultButton" CommandName="Send"
										OnCommand="FormButton_Click" Text="Send to Subscribers" CausesValidation="False" Visible="False" />
								</div>		
								<div class="clearFloat"></div>
								<br />
						</div>
						<a href='#' onclick='openPopup();'>Add New Application Item</a>
<!-- validators -->
<asp:requiredfieldvalidator runat="server" Display="None"  id="rfvVersionNbr" ControlToValidate="txtVersionNbr"  ErrorMessage="Version Nbr is required"></asp:requiredfieldvalidator>

<asp:requiredfieldvalidator id="rfvTitle" Display="None" ControlToValidate="txtTitle" ErrorMessage="Title is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvSubcatFaqDesc" Display="None" ControlToValidate="txtSubcategoryDesc" ErrorMessage="Subcategory Description is a required field for an FAQ" runat="server" Enabled="false"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator runat="server" Display="None"  id="rfvDescription" ControlToValidate="txtDescription"  ErrorMessage="Description is required"></asp:requiredfieldvalidator>

<asp:CustomValidator ID="cvlImagePathMime" runat="server" Display="None" EnableClientScript="False" ErrorMessage="<a href='#UploadPictureField'>File name</a> uploaded image must be one of these types: &quot;GIF&quot;, &quot;JPG&quot; or &quot;PNG&quot;." OnServerValidate="cvlImagePathMime_ServerValidate"></asp:CustomValidator>

        

<!-- date validator -->
<asp:RequiredFieldValidator ID="rfvStartDate" runat="server" ControlToValidate="txtStartDate" ErrorMessage="<a href='#StartDateField'>Start Date</a> is required." Display="None"></asp:RequiredFieldValidator>
<asp:CompareValidator ID="cvStartDate" Display="None" ControlToValidate="txtStartDate" Operator="DataTypeCheck" Type="Date" ErrorMessage="Please enter a valid <a href='#StartDateField'>Start Date</a>" runat="server" ></asp:CompareValidator>
<asp:RangeValidator ID="rvStartDate" ControlToValidate="txtStartDate" MinimumValue="2005-01-01" MaximumValue="2015-12-31"
Type="Date" EnableClientScript="False" Text="The <a href='#StartDateField'>Start Date</a> must be greater than 2005-01-01!" runat="server"  Display="None"/>
<!-- integer range validator -->
<asp:RangeValidator ID="RangeValidator1" ControlToValidate="txtStartDate" MinimumValue="1" MaximumValue="10"
Type="Integer" EnableClientScript="False" Text="The <a href='#StartDateField'>Start Date</a> must be an integer from 1 to 10" runat="server"  Display="None"/>						
						
						</asp:Panel>
						
					</div>
				</ContentTemplate>
			</ajaxToolkit:TabPanel>			
			</ajaxToolkit:TabContainer>
    
    </div>
	<div id="newItemPopup" style="display:none">
		<div class="dialogContent">
			<h1 style="text-align:center;color:White; ">Select Item Type for new record</h1>	
			<div style="text-align:center; width:400px; margin-top:15px;">
				<!-- --> 
				<div class="clearFloat"></div>
				<div class="requiredField" > 
					<asp:label id="Label4"  associatedcontrolid="ddlTypeId2" runat="server">Select Type</asp:label> 
					<br/>
					<asp:dropdownlist id="ddlTypeId2" runat="server"></asp:dropdownlist>
				</div>            
		
				<!-- --> 		
				<div class="clearFloat" style="margin-top:25px; margin-left:100px;">
									<asp:button id="Button1" runat="server" CssClass="defaultButton" width="200px" CommandName="Initialize" 
											OnCommand="FormButton_Click" Text="Initialize New Record" causesvalidation="false"></asp:button>
				</div>					
				<!-- --> 		
				<div class="clearFloat" style="margin-top:100px; text-align:center;">
				<a href="#" onclick="hidePopup();" style="color:White;" >Cancel Add Request</a><br />
				</div>
			</div>
		</div>
	</div>
<div>

<asp:panel ID="controlVariables" runat="server" Visible="false">
<asp:Literal ID="appItemTypeRestriction" runat="server" Visible="false"></asp:Literal> 
<!-- list appItems that require approval -->
<asp:Literal ID="itemsRequiringApproval" runat="server" Visible="false">1010,1035,1085</asp:Literal> 
<!-- list appItems that do NOT use images -->
<asp:Literal ID="itemsNOTUsingImages" runat="server" Visible="false">1085</asp:Literal> 

<!-- list appItems that allow versioning-->
<asp:Literal ID="itemsAllowingVersions" runat="server" Visible="false"></asp:Literal> 
<!-- list appItems requiring start and end dates-->
<asp:Literal ID="itemsAllowingStartEndDates" runat="server" Visible="false"></asp:Literal> 

<asp:Literal ID="doingSizeCheck" runat="server" Visible="false">no</asp:Literal> 
<asp:Literal ID="showPictureUrl" runat="server" Visible="false">/vos_portal/showPicture.aspx?imgSrc=is</asp:Literal>  
<asp:Literal ID="showDocumentUrl" runat="server" Visible="false">/vos_portal/support/show.aspx?id=</asp:Literal>  
<asp:Literal ID="lastUpdateDateFormat" runat="server" Visible="false">yyyy-MM-dd hh:mm</asp:Literal> 
<asp:Literal ID="Literal1" runat="server" Visible="false">MMM d, yyyy hh:mm</asp:Literal> 
<asp:Literal ID="annItemPicWidth" runat="server" Visible="false">80</asp:Literal> 
<asp:Literal ID="annItemPicHeight" runat="server" Visible="false">80</asp:Literal> 
<asp:Literal ID="Literal3" runat="server" Visible="false">MMM d, yyyy hh:mm</asp:Literal> 
<!-- Success stories uses string2. Was using description but should use StringValue. Allow temp config here-->
<asp:Literal ID="ssValueColumn" runat="server" Visible="false">StringValue</asp:Literal> 

<asp:Literal ID="LimitAppTypesOnExternalRequest" runat="server" Visible="false">yes</asp:Literal> 
<!-- chages to followng must be sync'd with the Login and UserList controls !time to add to web.config!-->	
<asp:literal id="codes" visible="false" runat="server">W#O)R%K(N&amp;E$T^</asp:literal>
<asp:literal id="dceoStaffRTM" visible="false" runat="server">/vos_portal/advisors/en/DceoStaff/SuccessStoriesMgmt</asp:literal>
<asp:literal id="workNetStaffRTM" visible="false" runat="server">/vos_portal/advisors/en/workNetAdmin/AppItemsMgmt/</asp:literal>
<asp:literal id="defaultRTM" visible="false" runat="server">/vos_portal/advisors/en/Home/</asp:literal>
<asp:literal id="currentLogInRTB" visible="false" runat="server">/</asp:literal>

<asp:literal id="displayStoryUrl" visible="false" runat="server">/vos_portal/controls/SuccessStories/DisplayStory.aspx?rowId={0}</asp:literal>
<asp:Literal ID="industryImageTag" runat="server" Visible="false"><img src='/vos_portal/industries/images/storyIcons/industry{0}.jpg' alt='{1}' align='middle' width='137' height='50'/></asp:Literal>
<asp:Literal ID="industryImageTag2" runat="server" Visible="false"><img src='{0}' alt='{1}' align='middle' width='137' height='50'/></asp:Literal>

<asp:Literal ID="txtUnsubscribeUrl" runat="server" >
<span style="PADDING-RIGHT: 5px; PADDING-LEFT: 5px; BACKGROUND-COLOR: #f69240"><a style="COLOR: white; TEXT-DECORATION: none" href="{0}">Unsubscribe from this list</a></span>
</asp:Literal>

<asp:Literal ID="ddlUrlScript" runat="server" Visible="false" >
<script type="text/javascript" language="javascript">
<!--
  function HandleCommonUrlsListClick() {
    var ddl = document.getElementById("{0}");
    var value2 = ddl.options[ddl.selectedIndex].value;

    //alert("HandleCommonUrlsList: " + value2) ;	

    document.getElementById("{1}").value = value2;

  } //end function   
-->  
</script>  
</asp:Literal>
</asp:panel>
<asp:panel ID="Panel1" runat="server" Visible="false">

<asp:Literal ID="statusSql" runat="server" Visible="false" >
Select distinct Status from AppItem  
where (len(Status) > 0) 
AND (TypeId = {0} or {0} = 0 )
AND (Category = '{1}' or '{1}' = '' )
Order by Status
</asp:Literal>						
</asp:panel>
</div>

