<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GroupMembers.ascx.cs" Inherits="ILPathways.Controls.GroupsManagment.GroupMembers" %>


<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" ValidationGroup="mbrsValGroup" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>

<asp:panel id="membersPanel" runat="server" cssclass="menuBranch" visible="true" width="100%">		
<br class="clearFloat" />	
<div style="float:right;">
			<asp:label id="Label8"  associatedcontrolid="txtGroupId" runat="server">Group Id</asp:label> 
			<asp:label id="txtGroupId"  runat="server">0</asp:label> 
</div>	
<div style="float:left;">	
<asp:Panel ID="menuPanel" runat="server" Visible="true">
	<br class="clearFloat" />
	<asp:button id="btnAddMember" runat="server" CssClass="defaultButton" width="150px" CommandName="AddMember" 
	OnCommand="FormButton_Click" Text="Add Individual"	CausesValidation="False" visible="true"></asp:button>
	&nbsp;&nbsp;

	<asp:button id="btnRefresh" runat="server"  CssClass="defaultButton" width="150px" CommandName="RefreshTeam" 
	OnCommand="FormButton_Click" Text="Refresh"	CausesValidation="False" visible="true"></asp:button>	
	&nbsp;&nbsp;
	<asp:button id="btnNotifyInComplete" runat="server"  CssClass="defaultButton" width="150px" CommandName="NotifyInComplete" 
	OnCommand="FormButton_Click" Text="Notify InComplete????"	CausesValidation="False" visible="False"></asp:button>	
	<br />
	Filter By Last Name:&nbsp;<asp:TextBox id="txtFilterBy" width="150px" runat="server"  ></asp:TextBox>
	<asp:button id="btnfilter" runat="server" CssClass="defaultButton" width="50px" CommandName="Filter" 
	OnCommand="FormButton_Click" Text="Go"	CausesValidation="False" ></asp:button>
	
	</asp:Panel>
</div>
<div>
<asp:Panel ID="resultsPanel" runat="server" Visible="true">
	<br class="clearFloat" />
	<asp:Label ID = "lblRecordCount" runat = "server" ></asp:Label>	
<!--  ============================================ -->
		<br class="clearFloat"/>
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
					
<!--  ============================================ -->
		<br class="clearFloat"/>	
	<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
	  allowpaging="true" PageSize="15" allowsorting="True"   
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
		OnRowDataBound="formGrid_RowDataBound" 
		OnRowCommand="formGrid_RowCommand" 
		OnPageIndexChanging="formGrid_PageIndexChanging"
		onsorting="formGrid_Sorting"
		caption="<h4>Group Members</h4>" captionalign="Top" useaccessibleheader="true"   >
		<HeaderStyle CssClass="gridResultsHeader" horizontalalign="left"  />
		<columns>
			<asp:TemplateField HeaderText="Select" >	
				<ItemTemplate>
				 <asp:LinkButton ID="mbrSelectBtn" CommandArgument='<%# Eval("UserId") %>' CommandName="SelectMember" CausesValidation="false" runat="server">
					 Select</asp:LinkButton>
				</ItemTemplate>
			</asp:TemplateField>	
					
			<asp:TemplateField HeaderText="Remove" >
			   <ItemTemplate>
				 <asp:LinkButton ID="btnDelete" CommandArgument='<%# Eval("ID") %>' CommandName="DeleteRow" CausesValidation="false" runat="server">
					 Remove</asp:LinkButton>
				</ItemTemplate>				
			</asp:TemplateField>
                
			<asp:TemplateField HeaderText="UserId" >
				<ItemTemplate>
				<asp:Label ID="lblUserId" runat="server" text='<%# Eval("UserId") %>'></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>
					
			<asp:boundfield datafield="SortName" headertext="Full Name"  sortexpression="SortName" ControlStyle-Width = "10%" ></asp:boundfield>
      <asp:boundfield datafield="Email" headertext="Email"  sortexpression="Email" ControlStyle-Width = "10%" ></asp:boundfield>
      <asp:boundfield datafield="Organization" headertext="Organization"  sortexpression="Organization" ControlStyle-Width = "10%" ></asp:boundfield>
			<asp:TemplateField HeaderText="Contact Info" ControlStyle-Width = "25%" Visible="false" >
				<ItemTemplate>
				<strong><%# Eval( "Email" )%></strong><br /><%# Eval( "Organization" )%>
				</ItemTemplate>
			</asp:TemplateField>	
			<asp:boundfield datafield="Status" headertext="Status" Visible="false" sortexpression="Status" ControlStyle-Width = "10%" ></asp:boundfield>							
		</columns>
		<pagersettings position="TopAndBottom" visible="False" />
	
	</asp:gridview>

	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
	</div>	
		<br class="clearFloat" />
</div>	
	</asp:Panel>
</div>
</asp:panel>
<asp:panel id="hiddenPanel" runat="server"  visible="false">	
	<asp:button id="btnAddIWDSMember" runat="server" CssClass="defaultButton" width="200px" CommandName="AddIWDSMember" 
	OnCommand="FormButton_Click" Text="Import IWDS Customer"	CausesValidation="False" visible="false"></asp:button>
	
<asp:Literal ID="formSecurityName" runat="server" Visible="false">ILPathways.Controls.GroupsManagment.GroupMembers</asp:Literal>


</asp:Panel>	