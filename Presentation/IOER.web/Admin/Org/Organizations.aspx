<%@ Page Title="Organzation Management" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Organizations.aspx.cs" Inherits="ILPathways.Admin.Org.Organizations" %>

<%@ Register Src="~/Controls/OrgMgmt/OrganizationMgmt.ascx" TagPrefix="uc1" TagName="OrganizationMgmt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
      $("form").removeAttr("onsubmit");
  </script>

 <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
<div class="container-fluid">
    <h1 class="isleH1">Organization Administration</h1>
<div class="row-fluid span12">
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
</div>

    <div class="span11" style="padding-left: 50px;">
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
	<!-- keyword -->
	<div class="clearFloat">
	</div>
	<div class="labelColumn">
		<asp:Label ID="lblKeyword" runat="server" AssociatedControlID="txtKeyword" Text="Keyword"></asp:Label></div>
	<div class="dataColumn">
		<asp:TextBox ID="txtKeyword" runat="server" Width="300px" enabled="true"></asp:TextBox>
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
		
			<asp:boundfield datafield="OrgType" headertext="Org. Type" sortexpression="OrgType"></asp:boundfield>	
            <asp:boundfield datafield="Name" headertext="Organization" sortexpression="Name"></asp:boundfield>	
			<asp:TemplateField HeaderText="Address"  sortexpression="">
				<ItemTemplate>
					<%# Eval( "Address1" )%>  <%# Eval( "Address2" )%>, <%# Eval( "City" )%>, <%# Eval( "ZipCode" )%>             , <%# Eval( "State" )%>
				</ItemTemplate>
			</asp:TemplateField>					
			<asp:boundfield datafield="MainPhone" headertext="Main Phone" sortexpression=""></asp:boundfield>			

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
	  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
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
	            <h3>Details</h3>
                <uc1:OrganizationMgmt runat="server" ID="OrganizationMgmt" />
            </div>

            </asp:Panel>

		</ContentTemplate>
	</ajaxToolkit:TabPanel>
<ajaxToolkit:TabPanel ID="TabPanel1"  HeaderText="Members" runat="server"  >
		<ContentTemplate>
            <asp:Panel ID="Panel1" runat="server">



            </asp:Panel>
            
		</ContentTemplate>
	</ajaxToolkit:TabPanel>
</ajaxToolkit:TabContainer>

    </div>
</div>


</asp:Content>
