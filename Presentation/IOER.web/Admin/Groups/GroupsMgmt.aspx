<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" Async="true"  CodeBehind="GroupsMgmt.aspx.cs" Inherits="ILPathways.Admin.Groups.GroupsMgmt" %>
<%@ Register TagPrefix="uc1" TagName="groupDetail" Src="~/Controls/GroupsManagment/GroupDetail.ascx" %>
<%@ Register TagPrefix="uc1" TagName="GroupMembers" Src="~/Controls/GroupsManagment/GroupMembers.ascx" %>
<%@ Register TagPrefix="uc1" TagName="privilegeDetail" Src="~/Controls/GroupsManagment/GroupPrivileges.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
	<ajaxToolkit:TabContainer ID="formContainer" runat="server" ActiveTabIndex="0">			
		<ajaxToolkit:TabPanel ID="tabGroups" Visible="false" runat="server" HeaderText="Groups Search" >		
			<ContentTemplate>
				<asp:Panel ID="searchPanel" runat="server">
					<h3>Select a group type and group</h3>
					<asp:Panel ID="groupTypePanel" runat="server" Visible="false">
					<div class="clear"></div>			
					<div class="labelcolumn" >Select a Group Type</div>	
					<div class="dataColumn">   		
						<asp:DropDownList		 id="ddlGroupTypeList" runat="server" Width="300px" AutoPostBack="True" 
								 ></asp:DropDownList>
					</div>	
					<div class="clear"></div>		
					</asp:Panel>	

					<asp:Panel ID="keywordPanel" runat="server">
						<div class="labelcolumn" >Enter keyword</div>	
						<div class="dataColumn">   		
						<asp:Textbox 		 id="txtKeyword" runat="server" Width="400px"></asp:Textbox>
						</div>
						<div class="clear"></div> 
					</asp:Panel> 

									
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
						<wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="25" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
					</div>
					<br class="clearFloat" />	
				<asp:GridView ID="formGrid" runat="server" autogeneratecolumns="False"
						allowpaging="true" PageSize="15" allowsorting="false" DataKeyNames="ID" 
						BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
						caption="<h3>User List</h3>" captionalign="Top" useaccessibleheader="true" 
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
							<asp:BoundField DataField="Group" HeaderText="Group"></asp:BoundField>

	  					<asp:BoundField DataField="GroupCode" HeaderText="GroupCode"></asp:BoundField>
				          
	  					</columns>
						</asp:GridView>	
					
					
					<div style="float:left;">
						<wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
					</div>	
				<br class="clearFloat" />
				</div>			
				</asp:Panel>

				</asp:Panel>
			</ContentTemplate>
		</ajaxToolkit:TabPanel>		

		<ajaxToolkit:TabPanel ID="tabGroupDetail" Visible="false" runat="server" HeaderText="Group Detail">		
			<ContentTemplate>
			<uc1:groupDetail id="groupDetail1" runat="server" UsingDefaultGroupCode="false"></uc1:groupDetail>
			</ContentTemplate>
		</ajaxToolkit:TabPanel>		

		<ajaxToolkit:TabPanel ID="tabMembers" Visible="false" runat="server" HeaderText="Members">			
			<ContentTemplate>			
				<uc1:GroupMembers id="teamDetail1" runat="server"></uc1:GroupMembers>
			</ContentTemplate>
		</ajaxToolkit:TabPanel>		

		<ajaxToolkit:TabPanel ID="tabPrivileges" Visible="false" runat="server" HeaderText="Privileges">			
			<ContentTemplate>
			<asp:panel ID="actionsPanel1" runat="server" >
				<uc1:privilegeDetail id="privilegeDetail2" runat="server"></uc1:privilegeDetail>
			</asp:panel>				
			</ContentTemplate>
		</ajaxToolkit:TabPanel>	

	</ajaxToolkit:TabContainer>
	<asp:label ID="txtCurrentParentId" runat="server" Visible="false"></asp:label>

<asp:Panel ID="hiddenPanel1" runat="server" Visible="false">
<asp:Literal ID="formSecurityName" runat="server" Visible="false">ILPathways.Controls.GroupsManagment.GroupsManager</asp:Literal>
<asp:Literal ID="defaultGroupType" runat="server">2</asp:Literal>
<asp:Literal ID="preSelectIfOneGroup" runat="server">no</asp:Literal>
<asp:Literal ID="txtGroupFilter" runat="server">GroupTypeId = 2</asp:Literal>
</asp:Panel>	
</asp:Content>
