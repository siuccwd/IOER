<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FaqMaintenance.ascx.cs" Inherits="ILPathways.Controls.FAQs.FaqMaintenance" %>
<%@ Register
    Assembly="AjaxControlToolkit"
    Namespace="AjaxControlToolkit.HTMLEditor"
    TagPrefix="HTMLEditor" %>

<script language="javascript" type="text/javascript">
<!--

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


  function ResetForm() {

    var ddlSearchPathway = document.getElementById("ctl00_MainContent_ctl00_TabContainer1_tab1_ddlSearchPathway");
    var ddlSearchCategory = document.getElementById("ctl00_MainContent_ctl00_TabContainer1_tab1_ddlSearchCategory");
    var ddlStatusSearch = document.getElementById("ctl00_MainContent_ctl00_TabContainer1_tab1_ddlStatusSearch");

    ddlSearchPathway.selectedIndex = 0;
    ddlSearchCategory.selectedIndex = 0;
    ddlStatusSearch.selectedIndex = 0;


    document.getElementById("ctl00_MainContent_ctl00_TabContainer1_tab1_txtKeyword").value = "";

  } //end function
-->
</script>

<asp:TextBox ID="txtDefaultControl" Visible="false" runat="server"></asp:TextBox>
<br />

<div style="width: 90%;">
			<ajaxToolkit:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
			<ajaxToolkit:TabPanel ID="tab1" runat="server" >
			<HeaderTemplate>
				<a href="javascript:void(0);" >Search</a>
			</HeaderTemplate>
				<ContentTemplate>
					<asp:Panel ID="searchPanel" runat="server">
					<div id="containerSearch">
							<h3>Search</h3>
					<asp:panel ID="pathwaysPanel" runat="server">
						<div class="clear"></div>			
						<div class="labelcolumn" >Select a Pathway</div>	
						<div class="dataColumn">   		
							<!-- Dropdownlist or grid -->
							<asp:DropDownList	id="ddlSearchPathway" runat="server" Width="300px" AutoPostBack="true" onselectedindexchanged="SearchPathway_SelectedIndexChanged" ></asp:DropDownList>
						</div>
						</asp:panel>
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn" > 
								<asp:label id="Label6"  associatedcontrolid="ddlSearchCategory" runat="server">Select a Category</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlSearchCategory" Width="300px" runat="server"></asp:dropdownlist>
							</div>
						<!-- status -->
						<div class="clear"></div>			
						<div class="labelcolumn" >Select a Status</div>	
						<div class="dataColumn">   		
							<asp:DropDownList		 id="ddlStatusSearch" runat="server" Width="300px" ></asp:DropDownList>
						</div>  						
  						<!-- keyword -->  
						<div class="clear"></div>			
						<div class="labelcolumn" ><a href="javascript:void(0);" onclick="ShowHideSection('divKeywordHelp');">
								<img alt="Keyword Instructions" src="/vos_portal/images/infoBubble.gif" />Keyword filter</a></div>	
						<div class="dataColumn">   		
							<asp:TextBox ID="txtKeyword" runat="server" Width="50%"></asp:TextBox>
							<br />
							<div id="divKeywordHelp" class="infoMessage" style="display: none; width: 80%;
								text-align: left; margin-top: 0;">
								An entered keyword will be used to search title, and description.
							</div>
						</div>

						
						<!-- -->
						<div class="clear"></div>	
						<p class="labelcolumn">&nbsp;</p>
						<div class="dataColumn">			
								<asp:button id="btnSearch" runat="server" CssClass="defaultButton" width="100px" CommandName="Search" 
								OnCommand="FormButton_Click" Text="Search" causesvalidation="false"></asp:button>
								&nbsp;&nbsp;<input type="button" value="Reset" onclick="ResetForm()" class="defaultButton"/>
								&nbsp;&nbsp;<asp:button id="Button1" runat="server" CssClass="defaultButton" width="100px" CommandName="New" 
								OnCommand="FormButton_Click" Text="New FAQ" causesvalidation="false"></asp:button>								
						</div>		
						<div class="clear"></div>  
					</div>
					</asp:Panel>					
					
				</ContentTemplate>
			</ajaxToolkit:TabPanel>
			<ajaxToolkit:TabPanel ID="tab2" Visible="true" runat="server">
			<HeaderTemplate>
				<a href="javascript:void(0);" >Search Results</a>
			</HeaderTemplate>			
				<ContentTemplate>
					<div class="tabPanelStyle" >
						<asp:Panel ID="resultsPanel" runat="server">
							<br class="clearFloat" />		
							<div style="width:100%">
								<div class="clear" style="float:right;">	
									<div class="labelcolumn" >Page Size</div>	
									<div class="dataColumn">     
											<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
									</div>  
								</div>
								<div style="float:left;">
									<wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
								</div>
								<br class="clearFloat" />	
								<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
									allowpaging="true" PageSize="20" allowsorting="true" 
									OnRowDataBound="formGrid_RowDataBound"
									OnRowCommand="formGrid_RowCommand" 
									OnPageIndexChanging="formGrid_PageIndexChanging"
									onsorting="formGrid_Sorting" 		
									BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
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
										<asp:boundfield datafield="Category" headertext="Category" sortexpression="Category"></asp:boundfield>	
										<asp:boundfield datafield="SubCategory" headertext="SubCategory" sortexpression="Category"></asp:boundfield>													
										<asp:TemplateField HeaderText="Title"  sortexpression="Title">
											<ItemTemplate>
												<%# Eval( "Title" )%>
											</ItemTemplate>
										</asp:TemplateField>		
																			
										<asp:boundfield datafield="status" headertext="Status" sortexpression="status"></asp:boundfield>		
										<asp:boundfield datafield="LastUpdated" headertext="Last Updated" sortexpression="base.LastUpdated"  DataFormatString="{0:MMM dd, yyyy}" ></asp:boundfield>										
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
<br class="clearFloat" />
								
								</div>			
							</asp:Panel>

					</div>
				</ContentTemplate>
			</ajaxToolkit:TabPanel>
			
			<ajaxToolkit:TabPanel ID="tab3" Visible="true" runat="server">
			<HeaderTemplate>
				<a href="javascript:void(0);" >Details</a>
			</HeaderTemplate>				
				<ContentTemplate>
					<div class="tabPanelStyle" >
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
						
						<asp:Panel ID="detailsPanel" runat="server" Visible="true">

						<div id="containerDetails">
              <div class="clear"></div>
              <br />
							<hr />
							<h3>Details</h3>
							<div class="clear"></div>	
							<asp:button id="btnNew2" runat="server" CssClass="defaultButton" width="100px" CommandName="New" Visible="True" 
										OnCommand="FormButton_Click" Text="New FAQ" causesvalidation="False"></asp:button>
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn " > 
								<asp:label id="lblRowId"  associatedcontrolid="txtRowId" runat="server">RowId</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:label  id="txtRowId" runat="server"></asp:label>
							</div>

<asp:Panel ID="seqNbrPanel" runat="server" Visible="false">
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn " > 
								<asp:label id="lblSequenceNbr"  associatedcontrolid="txtSequenceNbr" runat="server">Order (in lists)</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox id="txtSequenceNbr" runat="server">0</asp:textbox>
							</div>
</asp:Panel>
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn  requiredField" > 
								<asp:label id="lblCategory"  associatedcontrolid="ddlCategory" runat="server">Category</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlCategory" Width="300px" OnSelectedIndexChanged="Category_OnSelectedIndexChanged" runat="server" AutoPostBack="true"></asp:dropdownlist>
							</div>
						<!-- --> 
						<div class="clear"></div>
						<div class="labelcolumn requiredField " > 
							<asp:label id="lblSubcategory"  associatedcontrolid="ddlSubcategory" runat="server">Subcategory</asp:label> 
						</div>
						<div class="dataColumn"> 
							<asp:dropdownlist id="ddlSubcategory" Width="300px" AutoPostBack="false" runat="server"></asp:dropdownlist>
						 </div>	
						 <asp:Panel ID="publishPathwaysPanel" runat="server" Visible="true">
					<!-- --> 
					<div class="clear"></div>
						<div class="labelcolumn  requiredField" > 
							<asp:label id="Label3"  associatedcontrolid="cbxFAQPathway" runat="server">Publish to Pathways</asp:label> 
						</div>
						<div class="dataColumn"> 
							<asp:CheckBoxList ID="cbxFAQPathway" runat="server"  RepeatColumns="1"></asp:CheckBoxList>
					</div> 
					</asp:Panel>	  
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn  requiredField" > 
								<asp:label id="lblStatus"  associatedcontrolid="ddlStatus" runat="server">Status</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:dropdownlist id="ddlStatus" Width="150px" runat="server"></asp:dropdownlist>
							</div>
							<asp:Panel ID="newStatusPanel" runat="server" Visible="false">
							<div style="float: left; color: #000; width:125px; text-align:right;">New Status</div>&nbsp;<asp:textbox  id="txtNewStatus" Width="150px" MaxLength="50" runat="server"></asp:textbox> 
						  </asp:Panel>	
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn " > 
								<asp:label id="lblIsActive"  associatedcontrolid="rblIsActive" runat="server">IsActive</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:RadioButtonList id="rblIsActive"   runat="server" tooltip="True: User is active" RepeatDirection="Horizontal">
							<asp:ListItem Text="Yes"  value="Yes" Selected="True"></asp:ListItem>    
 							<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
						</asp:RadioButtonList>
							</div>						  
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn requiredField">
								<asp:label id="Label1"  associatedcontrolid="txtTitle" runat="server">Title</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox  id="txtTitle" Width="400px" TextMode="MultiLine" Rows="3"  columns="90" MaxLength="300" runat="server"></asp:textbox>  
							</div>
					      
            <!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn  requiredField" > 
								<asp:label id="lblDescription"  associatedcontrolid="radeDescription" runat="server">Description</asp:label> 
							</div>
							<div class="dataColumn"> 


                <ajaxToolkit:HtmlEditorExtender ID="htmlEditorExtender1" TargetControlID="                <ajaxToolkit:HtmlEditorExtender ID="htmlEditorExtender1" TargetControlID="txtDescription" runat="server" DisplaySourceTab="false" >
                    <Toolbar>                        
                        <ajaxToolkit:Bold />
                        <ajaxToolkit:Italic />
                        <ajaxToolkit:Underline />
                        <ajaxToolkit:HorizontalSeparator />
                        <ajaxToolkit:JustifyLeft />
                        <ajaxToolkit:JustifyCenter />
                        <ajaxToolkit:JustifyRight />
                        <ajaxToolkit:JustifyFull />                      
                    </Toolbar>
                </ajaxToolkit:HtmlEditorExtender>
<asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" Width="300px"></asp:TextBox>
        <asp:UpdatePanel ID="commentsUpdatePanel" runat="server">
            <ContentTemplate>
                <HTMLEditor:Editor runat="server" OnContentChanged="ContentChanged" Id="editor1" Height="300px" AutoFocus="true" Width="100%" />
                <asp:Label runat="server" ID="ContentChangedLabel" />
                <br />
   
            </ContentTemplate>
			
								<asp:button id="btnBadWordChecker" runat="server" CssClass="defaultButton" width="150px" CommandName="BadWordCheck" 
												 OnCommand="FormButton_Click" Text="Do BadWord Check"	CausesValidation="False"></asp:button> 
							</div>	
							<asp:Panel ID="faqCodePanel" runat="server" Visible="false">
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn " > 
								<asp:label id="lblAppFaqCode"  associatedcontrolid="txtFaqCode" runat="server">FAQ Code</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox  maxLength="50"  id="txtFaqCode" Width="300px" runat="server"></asp:textbox>
								<asp:Image ID="locktxtAppFaqCode" runat="server" AlternateText="Field locked" ImageUrl="/vos_portal/images/sslicon.gif" Visible="False" />
							</div>	
							</asp:Panel>			

						<asp:Panel ID="startDatePanel"  runat="server" Visible="False">
					 
						<!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn " > 
								<asp:label id="lblExpiryDate"  associatedcontrolid="txtExpiryDate" runat="server">Expiry Date</asp:label> 
							</div>
							<div class="dataColumn"> 
								<asp:textbox id="txtExpiryDate" runat="server"></asp:textbox>
							</div>
						</asp:Panel>  
						  
						<asp:Panel ID="imagePanel" runat="server" Visible="false">
						 <!-- --> 
						<div class="clear"></div>
			<div style="width:70%; float:left">						
							<div class="labelcolumn" >&nbsp;</div>
							<div class="dataColumn" > 
									<strong>Upload an image (optional)</strong>
									<br />You may upload an image by clicking the "Browse" button,
									then selecting the file you want to upload.  Acceptable file types are .gif, .jpg, .png.
									<br /> Maximum Width:&nbsp;<asp:TextBox ID="maxPicWidth" runat="server" Width="50px" style="margin-left: 15px;" ReadOnly="True">135</asp:TextBox>  
									<br />Maximum Height:&nbsp;<asp:TextBox ID="maxPicHeight" runat="server" Width="50px" style="margin-left: 11px;" ReadOnly="True">120</asp:TextBox> 
   
							</div>

												    
						 <!-- --> 
						<div class="clear"></div>
							<div class="labelcolumn" ><asp:Label AssociatedControlID="txtImageTitle" runat="server" CssClass="requiredField" ID="lblImageTitle">Image Title*:</asp:Label>
							</div>
							<div class="dataColumn"> 
								<asp:TextBox ID="txtImageTitle" Width="300px" runat="server"></asp:TextBox>
							</div>    
						 <!-- --> 
						<div class="clear"></div>
						<div class="labelcolumn" >
							<asp:Label AssociatedControlID="imageFileUpload" runat="server" CssClass="requiredField" ID="lblImagePath">File name*:</asp:Label>
						</div>
						<div class="dataColumn"> 
										<asp:FileUpload ID="imageFileUpload" Width="300px" runat="server" />
						</div>  
</div>
							<div style="float:left; background-color: #f5f5f5;"> 
								<a id="UploadPictureField"></a>
								<h3>Current image</h3>
								<asp:Image ID="imgCurrent" runat="server" Visible="False" />
								<br /><asp:label  id="lblImageId" runat="server"></asp:label>&nbsp;<asp:Label ID="txtImageFileName" runat="server" />
								<br /><asp:button id="btnDeleteImage" runat="server" CssClass="defaultButton" width="150px" CommandName="DeleteImage" 
												 OnCommand="FormButton_Click" Text="Remove Image"	CausesValidation="False"></asp:button> 
								<br /><asp:label  id="lblImageStatus" runat="server"></asp:label>
								<asp:Label ID="ImageWidthErrorMessage" runat="server" Visible="False">The <a href='#UploadPictureField'>image</a> @FileName you uploaded is too large.  It must be less than 135 pixels wide and less than 120 pixels in height.</asp:Label>
								
							</div>
							</asp:Panel>
						<!-- -->
              <div class="clear">
              </div>
              <div class="labelcolumn">
                  <asp:Label ID="Label2" runat="server">History</asp:Label>
              </div>
              <div class="dataColumn">
                  <div style="float: left">
                      <a style="float: left" onclick="ShowHideSection('divCreateHistoryDetails');" href="javascript:void(0);">
                          <asp:Label ID="lblHistory" runat="server"></asp:Label></a>
                      <br />
                  </div>
                  <div class="clear">
                  </div>
                  <div id="divCreateHistoryDetails" class="infoMessage" style="display: none; width: 90%;
                      padding-left: 5px;">
                      <asp:Label ID="lblCreatedHistoryDetails" Width="90%" runat="server"></asp:Label>
                  </div>
              </div>
              <!-- -->
              <div class="clear">
              </div>
              <div class="labelcolumn">
                  &nbsp;</div>
              <div class="dataColumn">
                  <div style="float: left">
                      <a style="float: left" onclick="ShowHideSection('divUpdateHistoryDetails');" href="javascript:void(0);">
                          <asp:Label ID="lblUpdatedHistory" runat="server"></asp:Label></a>
                      <br />
                  </div>
                  <div class="clear">
                  </div>
                  <div id="divUpdateHistoryDetails" class="infoMessage" style="display: none; width: 95%;
                      padding-left: 5px;">
                      <asp:Label ID="lblUpdatedHistoryDetails" Width="90%" runat="server"></asp:Label>
                  </div>
              </div>
								<!-- -->
								<div class="clear"></div>	
								<p class="labelcolumn">&nbsp;</p>
								<div class="dataColumn">			
										<asp:button id="btnSave" runat="server" CssClass="defaultButton" width="100px" CommandName="Update" 
										OnCommand="FormButton_Click" Text="Save"></asp:button>
										<asp:button id="btnDelete" runat="server" CssClass="defaultButton" width="100px" CommandName="Delete" 
										OnCommand="FormButton_Click" Text="Delete"	CausesValidation="False" visible="False"></asp:button> 
										&nbsp;&nbsp;<asp:button id="btnNew" runat="server" CssClass="defaultButton" width="100px" CommandName="New" 
										OnCommand="FormButton_Click" Text="New FAQ" causesvalidation="False" Visible="False"></asp:button>
								</div>		
								<div class="clear"></div>
								<br />
						</div>
						
<!-- validators -->
<asp:requiredfieldvalidator id="rfvTitle" Display="None" ControlToValidate="txtTitle" ErrorMessage="Title is a required field" runat="server"></asp:requiredfieldvalidator>

<asp:requiredfieldvalidator runat="server" Display="None"  id="rfvDescription" ControlToValidate="radeDescription"  ErrorMessage="Description is required"></asp:requiredfieldvalidator>

					
						
						</asp:Panel>
						
					</div>
				</ContentTemplate>
			</ajaxToolkit:TabPanel>			
			</ajaxToolkit:TabContainer>
    
    </div>

<div>

<asp:panel ID="controlVariables" runat="server" Visible="false">

<asp:Literal ID="defaultCategories" runat="server" Visible="false"></asp:Literal>
<asp:Literal ID="defaultTargetPathways" runat="server" Visible="false"></asp:Literal>
<asp:Literal ID="usingMultiplePath" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="searchingOnPathwayChange" runat="server" Visible="false">no</asp:Literal>
<asp:Literal ID="doingSizeCheck" runat="server" Visible="false">no</asp:Literal> 
<asp:Literal ID="showPictureUrl" runat="server" Visible="false">/vos_portal/showPicture.aspx?imgSrc=is</asp:Literal>  
<asp:Literal ID="lastUpdateDateFormat" runat="server" Visible="false">yyyy-MM-dd hh:mm</asp:Literal> 
</asp:panel>

<asp:panel ID="sqlPanel1" runat="server" Visible="false">
<asp:Literal ID="sqlSelectDistinctPaths" runat="server" Visible="false">
SELECT Distinct [PathwayId] As Id, [SitePathName] As Title, [name] As DisplayName
FROM [dbo].[Faq.FaqPathway] fp
inner join [dbo].[AppPathway] ap on fp.PathwayId = ap.Id Order by 2</asp:Literal>
<asp:Literal ID="Literal2" runat="server" Visible="false">SELECT id, [SitePathName] As Title, [name] As DisplayName FROM [dbo].[AppPathway] where [isActive] = 1 order by 2</asp:Literal>
 

<asp:Literal ID="sqlCategoriesList" runat="server" Visible="false">
SELECT id, [Category]  FROM [dbo].[FAQ.Category] 
where ([dbo].[FAQ.Category].IsActive = 1 )
{0}
Order by Category
</asp:Literal>

<asp:Literal ID="sqlSelectSearchCategories" runat="server" Visible="false">
SELECT distinct [CategoryId] As Id, [Category] FROM [dbo].[FAQ] inner join [dbo].[FAQ.Category] cat on faq.CategoryId = cat.Id Left Join [Faq.FaqPathway] fp on faq.RowId = fp.FaqRowId 
where ([dbo].[FAQ].IsActive = 1 )
{0}
Order by Category
</asp:Literal>
</asp:panel>
</div>
