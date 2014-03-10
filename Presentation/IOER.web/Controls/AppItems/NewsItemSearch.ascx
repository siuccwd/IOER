<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="NewsItemSearch.ascx.cs" Inherits="ILPathways.Controls.AppItems.NewsItemSearch" %>
<%@ Register TagName="Subscribe" TagPrefix="uc1" Src="~/News/Controls/AnnouncementSubscribe2.ascx" %>

<style type="text/css">
/*Textbox Watermark*/

.unwatermarked {	height:18px;	width:148px;}

.watermarked {	height:20px;	width:150px;	padding:2px 0 0 2px;	border:1px solid #BEBEBE;	background-color:#F0F8FF;	color:gray;}	
</style>
<asp:Panel ID="searchSection" runat="server" >
<asp:Panel ID="topPanel" runat="server" Visible="true">
<div class="infoBox">
  <asp:Panel ID="pnlError" runat="server" Visible="false">
    <table cellpadding="0" cellspacing="0" class="errorMessage">
      <tr>
        <td></td>
        <td>
          Error! You did not successfully complete the following information:
          <asp:ValidationSummary ID="vsError" runat="server" ForeColor="" />
          Please try re-entering these fields to continue.
        </td>
      </tr>
    </table>
  </asp:Panel>
<asp:Panel ID="pnlSubscribe" runat="server" Visible="true">
  <uc1:Subscribe ID="NewsSubscribe" runat="server" NewsItemTemplateCode="IOERS"  /><br />
  
</asp:Panel>
<asp:Panel ID="searchPanel" runat="server" Visible="true">
<asp:Panel ID="categoriesPanel" runat="server" Visible="false">
  <div class="labelcolumn">
    <asp:Label ID="Label1" runat="server" AssociatedControlID="CBLCategory" Text="IOERS:"></asp:Label>
  </div>
  <div class="dataColumn">
  <asp:CheckBoxList ID="CBLCategory" runat="server" RepeatDirection="Horizontal">    
  </asp:CheckBoxList>
	<asp:Button ID="BtnSelectAll" runat="server" Text="Select All" OnClick="BtnSelectAll_Click" Visible="false" />
	<asp:Button ID="BtnReset" runat="server" Text="Reset" OnClick="BtnReset_Click" Visible="false" />
	</div>
	  <br class="clearFloat" />
</asp:Panel>

  <div class="labelcolumn">
    <asp:Label ID="lblSearch" runat="server" AssociatedControlID="txtSearch" Text="Search for:"></asp:Label>
  </div>
  <div class="dataColumn">
    <asp:TextBox ID="txtSearch" runat="server" Width="300px"></asp:TextBox>
        	    <ajaxToolkit:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender2" runat="server"
        TargetControlID="txtSearch"
        WatermarkText="Enter Keyword(s)"
        WatermarkCssClass="watermarked" Enabled="True" />
    <asp:Button ID="btnSearch" runat="server" CommandName="Search" CssClass="defaultButton" Text="Search" Width="100px"
      OnCommand="FormButton_Click" />
      <asp:Panel ID="localPanel1" runat="server" Visible="false">
				<asp:RadioButtonList ID="searchOptions" runat="server">
					<asp:ListItem Text="Use Webservice" Selected="True"></asp:ListItem>
					<asp:ListItem Text="Use local DB" ></asp:ListItem>
				</asp:RadioButtonList>
  
			</asp:Panel>
  </div>
  <br class="clearFloat" />
</asp:Panel>
</div>

  </asp:Panel>
<asp:Label ID="lblResults" runat="server"></asp:Label>
<asp:Panel ID="pnlResults" runat="server">
  <h2 id="SearchResults" runat="server">Search Results</h2>
  <asp:Label ID="lblResultsPerPage" runat="server" AssociatedControlID="ddlPageSizeList" Text="Results per Page: "></asp:Label>
  <asp:DropDownList ID="ddlPageSizeList" runat="server"></asp:DropDownList>
  <asp:Button ID="btnResults" runat="server" CommandName="ResultsPerPage" CssClass="defaultButton" Text="Go"
    OnCommand="FormButton_Click" />
    
<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
		allowpaging="true" PageSize="15" allowsorting="True"  
		OnPageIndexChanging="formGrid_PageIndexChanging"
		onsorting="formGrid_Sorting"			
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
		captionalign="Top" 
		useaccessibleheader="true"  >
		<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
		<columns>

			<asp:TemplateField HeaderText="Published" ItemStyle-Width="100px"  sortexpression="Approved"  ItemStyle-HorizontalAlign="Right">
				<ItemTemplate>
					<%# FormatDate( DataBinder.Eval( Container.DataItem, "Approved" ).ToString() )%>
				</ItemTemplate>
			</asp:TemplateField>			
			<asp:TemplateField HeaderText="Title"  sortexpression="Title">
				<ItemTemplate>
				<%# FormatTitle( DataBinder.Eval( Container.DataItem, "Title" ).ToString(), DataBinder.Eval( Container.DataItem, "RowId" ).ToString() )%>
				<br />

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
  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="Pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
  
</asp:Panel>
</asp:Panel>
<asp:panel id="detailsPanel" runat="server" visible="false">
<div class="section">

  <div><asp:HyperLink ID="lnkSubscribe" runat="server" Visible="false" Text="Subscribe" /></div>
	<div><asp:Literal ID="ReadAnotherItem1" runat="server"></asp:Literal></div>
  <h1><%=txtTitle %></h1>
	<div style="margin: 10px 0px;"><%= txtDescr1 %></div>
	<asp:Panel ID="History" runat="server" Visible="false">
	  Published: <%= published %>  Updated: <%= updated %>
	</asp:Panel>
	
	<p style="margin-top: 25px;">
	<asp:HyperLink ID="lblDocumentLink" runat="server" Target="_blank" Visible="false">View File</asp:HyperLink>
	</p>
	<div><asp:Literal ID="ReadAnotherItem2" runat="server"></asp:Literal></div>
</div>
</asp:panel>	

<asp:Panel ID="HiddenFields" runat="server" Visible="false">
<asp:Literal ID="txtNewsItemCode" runat="server"></asp:Literal>
<asp:Literal ID="categoryHeading" runat="server"></asp:Literal>
<asp:Literal ID="resultsHeading" runat="server"></asp:Literal>
<asp:Literal ID="usingWebService" runat="server">yes</asp:Literal>
<asp:Literal ID="preventUnauthWFPNewsAccess" runat="server">yes</asp:Literal>

<asp:Literal ID="webOnlyPublishedStatus" runat="server">Published-WebOnly</asp:Literal>
<asp:Literal ID="defaultDisplayUrl" runat="server">/News/Default.aspx</asp:Literal>
<asp:Literal ID="DataItemTemplate" runat="server" Text='<a href="{0}?id={1}">{2}</a>'></asp:Literal>
<asp:Literal ID="dataItemTemplate2" runat="server" Text='<a href="{0}?id={1}">{2}</a>'></asp:Literal>
<asp:Literal ID="dateTemplate" runat="server"><span style="margin-right: 10px;">{0}</span></asp:Literal>
<asp:Literal ID="dateFormat" runat="server">MMM d, yyyy</asp:Literal>
<asp:Literal ID="useNewsItemType" runat="server">yes</asp:Literal>
<asp:Literal ID="txtSortTerm" runat="server">Approved DESC</asp:Literal>
<asp:Literal ID="allowCategoriesPanelDisplay" runat="server">no</asp:Literal> 		
</asp:Panel>


