<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GroupsManager.ascx.cs" Inherits="IOER.Controls.GroupsManagment.GroupsManager" %>


<%@ Register TagPrefix="uc1" TagName="groupDetail" Src="GroupDetail.ascx" %>
<%@ Register TagPrefix="uc1" TagName="GroupMembers" Src="GroupMembers.ascx" %>
<%@ Register TagPrefix="uc1" TagName="OrgGroupMembers" Src="OrgGroupMembers.ascx" %>
<%@ Register TagPrefix="uc1" TagName="privilegeDetail" Src="GroupPrivileges.ascx" %>

<h1 class="isleH1" itemprop="name">Groups Management</h1>
<asp:Literal ID="txtCurrentTab" runat="server" Visible="false">0</asp:Literal>
<input type="hidden" name="currentTab" runat="server" value="3"/>
<div id="tabs">
  <ul>
    <li><a href="#tabs-1">Group Search</a></li>
    <li><a href="#tabs-2">Group Detail</a></li>
    <li><a href="#tabs-3">Members</a></li>
    <li><a href="#tabs-4">Organizations</a></li>
    <li><a href="#tabs-5">Privileges</a></li>
  </ul>
  <div id="tabs-1">
  <asp:Panel ID="searchPanel" runat="server">
					<h3>Group Search</h3>
					<asp:Panel ID="groupTypePanel" runat="server" Visible="true">
					<div class="clearFloat"></div>			
					<div class="labelColumn" >Select a Group Type</div>	
					<div class="dataColumn">   		
						<asp:DropDownList		 id="ddlGroupTypeList" runat="server" Width="300px" AutoPostBack="false" 
								 ></asp:DropDownList>
					</div>	
					<div class="clearFloat"></div>		
					</asp:Panel>	

					<asp:Panel ID="keywordPanel" runat="server">
						<div class="labelColumn" >Enter keyword</div>	
						<div class="dataColumn">   		
						<asp:Textbox 		 id="txtKeyword" runat="server" Width="400px"></asp:Textbox>
						</div>
						<div class="clearFloat"></div> 
					</asp:Panel> 
          <div class="clearFloat"></div>			
					<div class="labelColumn" >&nbsp;</div>	
					<div class="dataColumn"> 
           <asp:button ID="btnSearch" runat="server" CssClass="defaultButton"  OnClick="btnSearch_Click" Text="Search" causesvalidation="False" Visible="true" ></asp:button>
           </div>
									
				<asp:Panel ID="resultsPanel" runat="server">
				<br class="clearFloat" />		
				<div style="width:100%">
					<div class="clearFloat" style="float:right;">	
						<div class="labelColumn" >Page Size</div>	
						<div class="dataColumn">     
								<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
						</div>  
					</div>
					<div style="float:left;">
						<wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="25" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
					</div>
					<br class="clearFloat" />	
				<asp:GridView ID="formGrid" runat="server" autogeneratecolumns="False"
						allowpaging="true" PageSize="15" allowsorting="true" DataKeyNames="ID" 
						BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
						captionalign="Top" useaccessibleheader="true" 
						OnRowCommand="formGrid_RowCommand" 
						OnRowDataBound="formGrid_RowDataBound"  >
  						<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
	  					<columns>
							<asp:TemplateField HeaderText="Select">
								<ItemTemplate>
								 <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("Id") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
									 Select</asp:LinkButton>
								</ItemTemplate>
							</asp:TemplateField>	  	
							<asp:BoundField DataField="Id" HeaderText="Group Id"></asp:BoundField>
              
              <asp:BoundField DataField="Title" HeaderText="Group" SortExpression="Title"></asp:BoundField>
              <asp:BoundField DataField="GroupType" HeaderText="GroupType" SortExpression="GroupType"></asp:BoundField>
	  					<asp:BoundField DataField="GroupCode" HeaderText="GroupCode" SortExpression="GroupCode"></asp:BoundField>
				          
	  					</columns>
						</asp:GridView>	
					
					
					<div style="float:left;">
						<wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
					</div>	
				<br class="clearFloat" />
				</div>			
				</asp:Panel>

				</asp:Panel>
  </div>
  <div id="tabs-2">
  			<uc1:groupDetail id="groupDetail1" runat="server" UsingDefaultGroupCode="false"></uc1:groupDetail>
  </div>
  <div id="tabs-3">
      <h2>Individual Members</h2>
  <uc1:GroupMembers id="teamDetail1" runat="server"></uc1:GroupMembers>
  </div>
    <div id="tabs-4">
  <h2>Organization Members</h2>
        
        <uc1:OrgGroupMembers id="OrgGroupMembers1" runat="server"></uc1:OrgGroupMembers>
  </div>
   <div id="tabs-5">
       <h2>Group Privileges</h2>
     <asp:panel ID="actionsPanel1" runat="server" >
				<uc1:privilegeDetail id="privilegeDetail2" runat="server"></uc1:privilegeDetail>
	</asp:panel>	
  </div>
</div>

		<ajaxToolkit:TabContainer ID="formContainer" runat="server" Visible="false" ActiveTabIndex="0">			
		<ajaxToolkit:TabPanel ID="tabGroups" Visible="true" runat="server" HeaderText="Groups Search" >		
			<ContentTemplate>
				
			</ContentTemplate>
		</ajaxToolkit:TabPanel>		

		<ajaxToolkit:TabPanel ID="tabGroupDetail" Visible="true" runat="server" HeaderText="Group Detail">		
			<ContentTemplate>

			</ContentTemplate>
		</ajaxToolkit:TabPanel>		

		<ajaxToolkit:TabPanel ID="tabMembers" Visible="true" runat="server" HeaderText="Members">			
			<ContentTemplate>			
				
			</ContentTemplate>
		</ajaxToolkit:TabPanel>		

		<ajaxToolkit:TabPanel ID="tabPrivileges" Visible="true" runat="server" HeaderText="Privileges">			
			<ContentTemplate>
						
			</ContentTemplate>
		</ajaxToolkit:TabPanel>	

	</ajaxToolkit:TabContainer>
	
  <asp:label ID="txtCurrentParentId" runat="server" Visible="false"></asp:label>

<asp:Panel ID="Panel1" runat="server" Visible="false">
<asp:Literal ID="formSecurityName" runat="server" Visible="false">IOER.Controls.GroupsManagment.GroupsManager</asp:Literal>
<asp:Literal ID="defaultGroupType" runat="server">2</asp:Literal>
<asp:Literal ID="preSelectIfOneGroup" runat="server">no</asp:Literal>
<asp:Literal ID="txtGroupFilter" runat="server"></asp:Literal>



</asp:Panel>	
