<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Search.ascx.cs" Inherits="IOER.Controls._Templates.Search" %>


<asp:Panel ID="searchPanel" runat="server">
<div id="containerSearch">
		<h3>Search for ????</h3>
		<br />
		<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />
	<!-- status -->
	<br class="clearFloat" />			
  <div class="labelColumn" >Select a Status</div>	
  <div class="dataColumn">   		
		<asp:DropDownList		 id="ddlStatus" runat="server" Width="80%" ></asp:DropDownList>
  </div>
  	<!-- keyword -->  
	<br class="clearFloat" />			
  <div class="labelColumn" >Keyword filter</div>	
  <div class="dataColumn">   		
		<asp:TextBox ID="txtKeyword" runat="server"></asp:TextBox>
  </div>    
  <!-- organization list or allow selecting my projects - based on user's orgid -->  
	<br class="clearFloat" />			
  <div class="labelColumn" >Select an owning Organization</div>	
  <div class="dataColumn">   		
		<asp:DropDownList		 id="ddlOrgList" runat="server" Width="80%" ></asp:DropDownList>
  </div>  
			
	<!-- -->
	<br class="clearFloat" />	
	<div class="labelColumn">&nbsp;</div>
	<div class="dataColumn">			
			<asp:button id="btnSearch" runat="server" CssClass="defaultButton" width="100px" CommandName="Search" 
			OnCommand="FormButton_Click" Text="Search" causesvalidation="false"></asp:button>
			<button type="reset" class="defaultButton" title="Reset"  value="Reset" id="resetButton">Reset</button>
	</div>		
	<br class="clearFloat" />
</div>
</asp:Panel>
<asp:Panel ID="resultsPanel" runat="server">
<br class="clearFloat" />		
<div style="width:100%">
	<div class="clear" style="float:right;">	
		<div class="labelColumn" >Page Size</div>	
		<div class="dataColumn">     
				<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
		</div>  
	</div>
	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
	</div>
	<br class="clearFloat" />	
	<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
		allowpaging="true" PageSize="15" allowsorting="True"  
		OnRowCommand="formGrid_RowCommand"
		OnRowDataBound="formGrid_RowDataBound"
		OnPageIndexChanging="formGrid_PageIndexChanging"
		onsorting="formGrid_Sorting"			
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
		captionalign="Top" 
		useaccessibleheader="true"  >
		<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
		<columns>
			<asp:TemplateField HeaderText="Select">
				<ItemTemplate>
				 <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("ProjectId") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
					 Select</asp:LinkButton>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField HeaderText="Delete">
     <ItemTemplate>
       <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("ProjectId") %>' CommandName="DeleteRow" CausesValidation="false" runat="server">
         Delete</asp:LinkButton>
     </ItemTemplate>
   </asp:TemplateField>
			<asp:TemplateField HeaderText="Grant #"  sortexpression="IwdsGrantNbr">
				<ItemTemplate>
					<%# Eval( "IwdsGrantNbr" )%>
				</ItemTemplate>
			</asp:TemplateField>			
			<asp:TemplateField HeaderText="Project #"  sortexpression="ProjectId">
				<ItemTemplate>
					<%# Eval( "ProjectId" )%>
				</ItemTemplate>
			</asp:TemplateField>								
			<asp:boundfield datafield="Title" headertext="Project Title" sortexpression="Title"></asp:boundfield>
			<asp:TemplateField HeaderText="Status"  sortexpression="ProjectStatus">
				<ItemTemplate>
					<asp:Label ID="lblRowStatus" Text='<%# Eval( "ProjectStatus" )%>' runat="server"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>	
			<asp:TemplateField HeaderText="Last Updated"  sortexpression="LastUpdated">
				<ItemTemplate>
					<asp:Label ID="lblLastUpdated" Text='<%# Eval( "LastUpdated" )%>' runat="server"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>				
											
		</columns>
		<pagersettings  visible="true" 
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
<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="formSecurityName" runat="server" Visible="false">/vos_portal/advisor/...@template</asp:Literal>

<!-- control variables -->
<asp:Literal ID="someStringValue" runat="server" Visible="false">tbd</asp:Literal>

<asp:Literal ID="openingDetailInNewWindow" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="projectDetailUrl" runat="server" Visible="false">/vos_portal/advisors/en/home/IncumbentWorker/projectPlan</asp:Literal>
<!-- message variables -->

</asp:Panel>


