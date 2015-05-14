<%@ Page Title="Organzation Management" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Organizations.aspx.cs" Inherits="ILPathways.Admin.Org.Organizations" %>

<%@ Register Src="~/Controls/OrgMgmt/OrganizationMgmt.ascx" TagPrefix="uc1" TagName="OrganizationMgmt" %>
<%@ Register Src="~/Controls/OrgMgmt/Import.ascx" TagPrefix="uc1" TagName="Import" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
<style type="text/css">
    .longTxt { width: 300px;}
    .shortTxt { width: 100px;}
.memberPanel { width: 800px; margin: 5px auto; min-height: 500px;}    

  @media screen and (max-width: 700px){

.memberPanel { width: 95%; margin: 5px auto; min-height: 100px;}
  }
</style>    
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
      $("form").removeAttr("onsubmit");
  </script>

 <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
<div class="container-fluid">
    <h1 class="isleH1">Organization Administration</h1>


    <div class="span11" style="padding-left: 50px;">
        <h2 style="text-align: center;font-size: 150%;"><asp:Literal ID="litOrgTitle" runat="server"></asp:Literal></h2>
<ajaxToolkit:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
<ajaxToolkit:TabPanel runat="server" ID="tabSummary" HeaderText="Search">
		<ContentTemplate>            
<asp:Panel ID="searchPanel" runat="server">
<div id="containerSearch">
		<h3>Search</h3>
	
<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" >Select an Org. Type</div>
  <div class="dataColumn"> 
		<asp:DropDownList		 id="ddlOrgType" runat="server" ></asp:DropDownList>
	</div>
<!-- --> 
  <div class="labelColumn " > 
    <asp:label id="Label8"  associatedcontrolid="rblIsIsleMember" runat="server">Is ISLE Member</asp:label> 
  </div>
    <div class="dataColumn">
        <asp:RadioButtonList ID="rblIsIsleMember" CausesValidation="false" runat="server" RepeatDirection="Vertical">
            <asp:ListItem Text="All Types" Value="0"></asp:ListItem>
            <asp:ListItem Text="Only Isle Members" Value="1"></asp:ListItem>
            <asp:ListItem Text="Only Non-Isle Members" Value="2"></asp:ListItem>
        </asp:RadioButtonList>
    </div>

	<!-- keyword -->
	<div class="clearFloat">
	</div>
	<div class="labelColumn">
		<asp:Label ID="lblKeyword" runat="server" AssociatedControlID="txtKeyword" Text="Keyword"></asp:Label></div>
	<div class="dataColumn">
		<asp:TextBox ID="txtKeyword" runat="server" CssClass="longTxt" enabled="true"></asp:TextBox>
		<asp:Label ID="lblKeyword_Help" runat="server" Visible="false" >Enter a partial organization name, or city. 
		</asp:Label>
	</div>	
		
		<!-- -->
		<div  class="labelColumn">&nbsp;</div >
		<div class="dataColumn">
				<asp:button id="SearchButton" runat="server" text="Search" cssclass="defaultButton" onclick="SearchButton_Click" CausesValidation="false"></asp:button>&nbsp;&nbsp;&nbsp; 
        				&nbsp;&nbsp;&nbsp;
<%--            <asp:button id="Button1" runat="server" CssClass="defaultButton"  CommandName="New" 
				OnCommand="FormButton_Click" Text="New Query" causesvalidation="false"></asp:button>--%>
		</div>
		
</div>
</asp:Panel>

<asp:Panel ID="resultsPanel" runat="server">
<br class="clearFloat" />		
<div style="width:95%">
	<div class="clearFloat" style="float:right;">	
		<div style="display:inline-block;">Page Size:</div>	
		<div style="display:inline-block; text-align: right; font-size: 100%;">   
				<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
		</div>  
	</div>

	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
	</div>
	<br class="clearFloat" />	
	<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
		allowpaging="true" PageSize="25" allowsorting="True"  
		OnRowCommand="formGrid_RowCommand"
		OnPageIndexChanging="formGrid_PageIndexChanging"
		onsorting="formGrid_Sorting"				
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
		captionalign="Top" 
		useaccessibleheader="true"  >
		<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
		<columns>
			<asp:TemplateField HeaderText="Select">
				<ItemTemplate>
				 <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("Id") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
					 Select</asp:LinkButton>
				</ItemTemplate>
			</asp:TemplateField>
		    <asp:boundfield datafield="Name" headertext="Organization" sortexpression="Name"></asp:boundfield>	
			<asp:boundfield datafield="OrgType" headertext="Org. Type" sortexpression="OrgType"></asp:boundfield>	
            <asp:boundfield datafield="IsIsleMember" headertext="Is Isle Member" sortexpression="IsIsleMember"></asp:boundfield>	
			<asp:TemplateField HeaderText="Address"  sortexpression="">
				<ItemTemplate>
					<%# Eval( "AddressToString" )%>  
				</ItemTemplate>
			</asp:TemplateField>					
			<asp:boundfield datafield="MainPhoneFormatted" headertext="Main Phone" sortexpression=""></asp:boundfield>			
			<asp:TemplateField HeaderText="Last Updated"  sortexpression="LastUpdated">
				<ItemTemplate>
					<asp:Label ID="lblLastUpdated" Text='<%# Eval( "LastUpdated" )%>' runat="server"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>				
											
		</columns>
		<pagersettings  visible="false" 
						mode="NumericFirstLast"
            firstpagetext="First"
            lastpagetext="Last"
            pagebuttoncount="5"  
            position="TopAndBottom"/> 
	
	</asp:gridview>

	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="25" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
	</div>	
<br class="clearFloat" />
</div>			
</asp:Panel> 	
		</ContentTemplate>
	</ajaxToolkit:TabPanel>
    <ajaxToolkit:TabPanel ID="tabResults"  HeaderText="Organization" runat="server"  >
		<ContentTemplate>
            <asp:Panel ID="detailsPanel" runat="server">

            <div id="containerDetails">
	            <h3 style="text-align: center">Details</h3>
                <uc1:OrganizationMgmt runat="server" ID="OrganizationMgmt" />
            </div>

            </asp:Panel>

		</ContentTemplate>
	</ajaxToolkit:TabPanel>
    <ajaxToolkit:TabPanel ID="TabPanel1"  HeaderText="Members" runat="server"  >
		<ContentTemplate>
            <asp:Panel ID="memberPanel" CssClass="memberPanel" Visible="false" runat="server">
                <asp:Button ID="btnImportMembers" runat="server" CausesValidation="false" Text="Import People" OnClick="btnImportMembers_Click" CssClass="defaultButton" />
                <asp:Button ID="btnAddMember" runat="server" CausesValidation="false" Text="Add New User" OnClick="btnAddMember_Click" CssClass="defaultButton" />
                <asp:Button ID="btnAddExitingMbr" runat="server" CausesValidation="false" Text="Add Existing User" OnClick="btnAddExitingMbr_Click" CssClass="defaultButton" />
                <br />
                <div class="labelColumn"><asp:Label ID="Label6" AssociatedControlID="ddlFilterMemberType" runat="server">Member Type </asp:Label></div>
                <div class="dataColumn">
                    <asp:DropDownList ID="ddlFilterMemberType" runat="server"></asp:DropDownList>
                </div>
                    <div class="labelColumn" >
                        <asp:Label ID="Label1" runat="server">Keyword:</asp:Label></div>
                    <div class="dataColumn">
                        <asp:TextBox ID="txtMemberKeyword" runat="server" ></asp:TextBox>
                    </div>
                    <br />
                    <div class="labelColumn">&nbsp;</div>
                    <div class="dataColumn">
                        <asp:Button ID="searchLink" runat="server" Text="Search" CausesValidation="false" OnClick="searchLink_Click" CssClass="defaultButton" ></asp:Button>
                    </div>

<div style="width: 100%">
                    <div class="clear" style="float: right;margin: 15px 0;">
                        <div style="display:inline-block;">Page Size:</div>
                        <div style="display:inline-block; text-align: right; font-size: 100%;">
                            <asp:DropDownList ID="ddlMembersPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="MembersPageSizeList_OnSelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>

                    <br class="clearFloat" />
                    <asp:GridView ID="membersGrid" runat="server" AutoGenerateColumns="False"
                        AllowPaging="true" PageSize="25" AllowSorting="false"
                        AutoGenerateEditButton="false" DataKeyNames="Id"
                        OnRowCommand="membersGrid_RowCommand"
                        OnPageIndexChanging="membersGrid_PageIndexChanging"
                        OnSorting="membersGrid_Sorting"
                        BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="Horizontal" Width="100%"
                        CaptionAlign="Top"
                        UseAccessibleHeader="true">
                        <HeaderStyle CssClass="gridResultsHeader" HorizontalAlign="Left" />
                        <Columns>
                            <asp:TemplateField Visible="true" HeaderText="Select">
                                <ItemTemplate>
                                    <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("UserId") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
				Select</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Remove"  ItemStyle-CssClass="removeCol">
                                <ItemTemplate>
                                    <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("Id") %>' CommandName="DeleteRow" CausesValidation="false" OnClientClick="return confirm('Are you certain that you want to remove this member?');" runat="server">Remove</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="UserId" HeaderText="Id" Visible="true"></asp:BoundField>
                            <asp:TemplateField HeaderText="First Name" SortExpression="FirstName">
                                <ItemTemplate>
                                    <asp:Label ID="gridlblFirstName" runat="server" Text='<%# Bind("FirstName") %>'></asp:Label>

                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Last Name" SortExpression="LastName">
                                <ItemTemplate>
                                    <asp:Label ID="gridlblLastName" runat="server" Text='<%# Bind("LastName") %>'></asp:Label>

                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Org. Member Type"  >
                                <EditItemTemplate>
                                    <asp:DropDownList ID="gridDdlOrgMbrType" Visible="false" runat="server"></asp:DropDownList>
                                   
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="gridlblOrgMbrType" runat="server" Text='<%# Bind("OrgMemberType") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                           
                            <asp:BoundField DataField="OrgMemberTypeId" HeaderText="Member Type Id" Visible="false"></asp:BoundField>

                            <asp:TemplateField HeaderText="Last Updated" SortExpression="LastUpdated">
                                <EditItemTemplate>
                                    <asp:Label ID="gridlblLastUpdated" runat="server" Text='<%# Bind("LastUpdated", "{0:d}") %>'></asp:Label>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="gridlblLastUpdated2" runat="server" Text='<%# Bind("LastUpdated","{0:d}") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Created" Visible="true" HeaderText="Added" SortExpression="Created" DataFormatString="{0:d}"></asp:BoundField>

                        </Columns>
                        <PagerSettings Visible="false"
                            Mode="NumericFirstLast"
                            FirstPageText="First"
                            LastPageText="Last"
                            PageButtonCount="5"
                            Position="TopAndBottom" />

                    </asp:GridView>
                    <div style="float: left; margin-top: 10px;">
                        <wcl:PagerV2_8 ID="membersPager2" runat="server" Visible="false" OnCommand="memberPager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next" LastClause="Last Page" PageSize="25" CompactModePageCount="10" NormalModePageCount="10" GenerateGoToSection="false" GeneratePagerInfoSection="true" />
                    </div>
                    <br class="clearFloat" />
                </div>

            </asp:Panel>
            
            <asp:Panel ID="addMemberPanel" CssClass="memberPanel" Visible="false" runat="server">
                 <asp:Button ID="btnCloseAddPanel" CssClass="defaultButton" runat="server" CausesValidation="false" Text="Back to Search" OnClick="btnCloseAddPanel_Click" />
                
                <h3>Account:</h3>
    <div  class="labelColumn">Userid</div>
    <div  class="dataColumn"><asp:Label ID="lblUserId" runat="server" >0</asp:Label></div>
    
                <asp:Panel ID="existingMbrOptionPanel" Visible="false" runat="server">
                    <div style="display: none;"><asp:Label ID="lblAddMbrUserId" runat="server" >0</asp:Label></div>
                    <div class="labelTop">The entered user aleady has an account and is NOT associated with this organization.<br />If you just want to add this person to your organization, click the <strong>Add to Organization</strong> button below. Otherwise, change the entered information.</div>
                    <div class="labelColumn">&nbsp;</div>
                    <div class="dataColumn" style="margin-top:10px;">
                <asp:Button ID="btnAddToOrganization" runat="server" Text="Add User to organization" OnClick="btnAddToOrganization_Click" CssClass="defaultButton" ></asp:Button> 
                <asp:Button ID="brnResetUsr" runat="server" Text="Reset User" OnClick="btnAddMember_Click" CssClass="defaultButton" ></asp:Button> 
                    </div>

                    </asp:Panel>

                 <asp:Panel ID="mbrDetailPanel"  Visible="true" runat="server">
                     <asp:Panel ID="mbrNamePanel"  Visible="true" runat="server">
                    <div class="labelColumn"><asp:Label ID="Label4" AssociatedControlID="txtFirstName"  runat="server" >First Name</asp:Label></div>
                    <div class="dataColumn"><asp:TextBox ID="txtFirstName" runat="server" CssClass="longTxt"></asp:TextBox> </div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn"><asp:Label ID="Label5" AssociatedControlID="txtLastName"  runat="server" >Last Name</asp:Label></div>
                    <div class="dataColumn"><asp:TextBox ID="txtLastName" runat="server" CssClass="longTxt"></asp:TextBox> </div>
                    <div class="clearFloat"></div>
                         </asp:Panel>
                    <div class="labelColumn"><asp:Label ID="Label2" AssociatedControlID="txtEmail"  runat="server" >Email</asp:Label></div>
                    <div class="dataColumn"><asp:TextBox ID="txtEmail" runat="server" CssClass="longTxt"></asp:TextBox> </div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn"><asp:Label ID="Label7" AssociatedControlID="txtConfirmEmail"  runat="server" >Confirm Email</asp:Label></div>
                    <div class="dataColumn"><asp:TextBox ID="txtConfirmEmail" runat="server" CssClass="longTxt"></asp:TextBox> </div>
                     </asp:Panel>

                    <div class="clearFloat"></div>
                    <div class="labelColumn"><asp:Label ID="Label3" AssociatedControlID="rblMemberType"  runat="server" >Member Type</asp:Label></div>
                    <div class="dataColumn">
                        <asp:RadioButtonList ID="rblMemberType" runat="server">
                            <asp:ListItem Text="Add as Administration"            Value="1"></asp:ListItem>
                            <asp:ListItem Text="Add as Staff member/Employee" Selected="True"     Value="2"></asp:ListItem>
                            <asp:ListItem Text="Add as Student"                   Value="3"></asp:ListItem>
                            <asp:ListItem Text="Add as Contrator/External"        Value="4"></asp:ListItem>
                        </asp:RadioButtonList>
                    </div>

                    <div class="clearFloat"></div>
                            <!-- User should have some min org admin role to assign these.
                                - also should probably not be able to assign a role above their own? 
                                -->
                            <div class="labelTop">You may optionally assign organization roles for this person. If applicable, select one or more roles to be assigned to this person.</div>
                            <div class="labelColumn">&nbsp;</div>
                            <div class="dataColumn">
                                <asp:CheckBoxList ID="cblOrgRoles" runat="server"></asp:CheckBoxList>
                            </div>
                            <br />
                      
                    <div class="labelColumn">&nbsp;</div>
                    <div class="dataColumn" style="margin-top:10px;"><asp:Button ID="btnAddUser" runat="server" Text="Save" OnClick="btnAddUser_Click" CssClass="defaultButton" ></asp:Button> 
                <asp:Button ID="btnAddAdnother" runat="server" Text="Add New User" OnClick="btnAddMember_Click" CssClass="defaultButton" ></asp:Button> 

                    </div>
                    
            </asp:Panel>

            <asp:Panel ID="importPanel" Visible="false" runat="server">
                 <asp:Button ID="btnCloseImport" runat="server" CausesValidation="false" Text="Close Import" OnClick="btnCloseImport_Click" CssClass="defaultButton" />
                <uc1:Import runat="server" ID="memberImport" />
            </asp:Panel>
		</ContentTemplate>
	</ajaxToolkit:TabPanel>
    <ajaxToolkit:TabPanel ID="RequestsPanel"  HeaderText="Requests" runat="server"  >
		<ContentTemplate>
            <asp:Panel ID="Panel1" CssClass="memberPanel" Visible="true" runat="server">
            <p>Future use - to handle requests to join the current organization</p>
                </asp:Panel>
		</ContentTemplate>
	</ajaxToolkit:TabPanel>

</ajaxToolkit:TabContainer>

    </div>
</div>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="currentOrgId" runat="server" Visible="false">0</asp:Literal>
<asp:Literal ID="doingBccOnRegistration" runat="server" Visible="false">yes</asp:Literal>

<asp:Label ID="userAddConfirmation" runat="server">The user account for {0} was created and an email was sent with a link to activate the account.</asp:Label>
<asp:Label ID="noticeSubject" runat="server">{0} added you to an "Illinois Open Educational Resources" organization</asp:Label>
<asp:Label ID="noticeEmail" runat="server">
Welcome<br />An IOER account has been created for you:
    <br />Organization: <strong>{0}</strong> 
    <br />Member Type: <strong>{1}</strong>.
<p>Use the link below to activate your account. You will then have access to the latter organization.</p>
    <p>Upon activation a personal library will be created where you can store the resources that you find interesting.</p>
</asp:Label>
<asp:Label ID="existingUserNoticeEmail" runat="server">
Note<br />You have been added to the following organization:
    <br />Organization: <strong>{0}</strong> 
    <br />Member Type: <strong>{1}</strong>.
<p>Use the link below to login to the website. You will then have access to the latter organization.</p>
    <a href="{2}">Login to IOER</a>
</asp:Label>
<asp:Literal ID="loginLink" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Libraries</asp:Literal>
    <asp:Literal ID="Literal1" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Organizations</asp:Literal>
<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?pg={0}&a=activate&nextUrl=/Account/Profile.aspx</asp:Literal>
<asp:Label ID="acctCreatedMsg" runat="server">
<p>An account has been created for you:</p>
User Name (email): {0}
<br />Password (temporary): {1} 
<p>Click on the following link to confirm your account and update your profile:</p>
    <a href="{2}">Confirm your IOER registration</a>
<p>Use the Profile page to:</p>
    <ul>
        <li>Set your password</li>
        <li>Upload a personal avatar</li>
        <li>Assign other optional properties</li>
    </ul>
</asp:Label>

    <asp:Literal ID="userExistsMessage" runat="server" >"Error: an account already exist for the entered email address.<br/>Do you want to just add this person to your organization using the entered member type and role(s)?: <br/>{0}, <br/>Registered: {1}.<p>Click the <strong>Add User to organization</strong> button below to add user to this organization.</p></asp:Literal>
</asp:Panel>
</asp:Content>
