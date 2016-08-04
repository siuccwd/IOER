<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PublishersSearch2.ascx.cs" Inherits="IOER.LRW.controls.PublishersSearch2" %>
<%@ Register Namespace="ASPnetControls" Assembly="ILPathways.Utilities" TagPrefix="wcl" %>

<% 
  //Easy CSS colors
  string css_black      = "#4F4E4F";
  string css_red        = "#B03D25";
  string css_orange     = "#FF5707";
  string css_purple     = "#9984BD";
  string css_teal       = "#4AA394";
  string css_gray       = "#909297";
  string css_blue       = "#3572B8";
  string css_white      = "#E6E6E6";
  string css_lightblue  = "#4C98CC";
%>

<!-- Page setup -->
<script type="text/javascript" lang="javascript">
    $(document).ready(function () {
        $(".txtKeyword").attr("placeholder", "Search for Publishers...");
        $(".resultsTable").attr("border", 0).attr("border-spacing", 1).attr("border-collapse", "separate").attr("cellspacing", 2).css("border-collapse", "separate");
        $(".resultsTable th").addClass("isleH2");
    });
</script>

<!--Major page elements -->
<style type="text/css">
#container {
  margin-left: auto; margin-right: auto; width: 100%; max-width: 800px;
}
.pager {
  min-height: 20px;
}
.pager .pageSizeSelector {
  float: right;
}
.txtKeyword {
  display: block;
  width: 100%;
  margin: 10px auto;
  font-size: 130%;
}
  .PagerContainerTable td { display: inline-block; margin: 2px -4px 2px 0; }
</style>
<style type="text/css">

@media screen and (max-width: 900px) {
#container { width: 95%; margin: 5px; }

}
@media screen and (max-width: 800px){
  .PagerContainerTable { clear: both; margin: 2px auto; }
}
</style>

<!-- Table -->
<style type="text/css">
.resultsTable {
  border-collapse: separate;
  border: 0;
  width: 100%;
}
.resultsTable th, .resultsTable td {
  border: 0;
}
.resultsTable td {
  border-top: 1px solid <%=css_white %>;
  padding: 5px 1px;
}
.resultsTable tr:nth-child(2) td {
  border: 0;
}
.resultsTable tr:hover td, .resultsTable tr:focus td {
  background-color: <%=css_white %>;
}
</style>
<asp:Panel ID="searchOptionsPanel" runat="server" Visible="false">
    <div style="display:inline-block; padding: 2px 5px; vertical-align:middle;">Options</div>

    <div style="display:inline-block; ">
        <asp:RadioButtonList ID="searchOptions" runat="server" RepeatDirection="Horizontal" RepeatColumns="2" >
            <asp:ListItem Text="Publishers" Selected="True"></asp:ListItem>
            <asp:ListItem Text="Creators" ></asp:ListItem>
        </asp:RadioButtonList>
    </div>
<div style="clear:both;"        ></div>
</asp:Panel>
<h1 class="isleH1"> ISLE OER Publisher Search</h1>
<div id="container">
<asp:TextBox ID="txtKeyword" runat="server" class="txtKeyword"></asp:TextBox>
<asp:button id="btnSearch" runat="server" CssClass="defaultButton offScreen" Text="Search" causesvalidation="false" onclick="btnSearch_Click" />

<asp:Panel ID="resultsPanel" runat="server">

  <div class="pager isleBox">
    <asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged" CssClass="pageSizeSelector" />
    <wcl:PagerV2_8 ID="pager1" 
      runat="server" 
      Visible="false" 
      OnCommand="pager_Command" 
      GenerateFirstLastSection="true"
      FirstClause="First Page" 
      PreviousClause="Prev." 
      NextClause="Next" 
      LastClause="Last Page" 
      PageSize="15" 
      CompactModePageCount="4" 
      NormalModePageCount="5"  
      GenerateGoToSection="false" 
      GeneratePagerInfoSection="true"
    />

  </div>

  <asp:gridview 
    id="formGrid" 
    runat="server" 
    autogeneratecolumns="False"
    allowpaging="true" 
    PageSize="15" 
    allowsorting="True" 
    OnRowDataBound="formGrid_RowDataBound" 
    OnPageIndexChanging="formGrid_PageIndexChanging"
    onsorting="formGrid_Sorting"			
    captionalign="Top" 
    useaccessibleheader="true"  
    CssClass="resultsTable"
  >
	  <HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
	  <columns>
		  <asp:TemplateField HeaderText="Publisher"  sortexpression="Publisher">
			  <ItemTemplate>
          <div class="resultData">
					  <asp:Label ID="lblPublisher" runat="server" Visible="false"><%# DataBinder.Eval( Container.DataItem, "Publisher" )%></asp:Label>
          </div>
			  </ItemTemplate>
		  </asp:TemplateField>	
		  <asp:TemplateField HeaderText="Resources"  sortexpression="ResourceTotal" ItemStyle-HorizontalAlign="Right">
			  <ItemTemplate>
				  <%# Eval( "ResourceTotal" )%>
			  </ItemTemplate>
		  </asp:TemplateField>	
	  </columns>
	  <pagersettings 
      Visible="false" 
		  mode="NumericFirstLast"
      firstpagetext="First"
      lastpagetext="Last"
      pagebuttoncount="5"  
      position="TopAndBottom"
    /> 
  </asp:gridview>

  <div class="pager isleBox">
    <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="4" NormalModePageCount="5"  GenerateGoToSection="false" GeneratePagerInfoSection="true"   />
  </div>

</asp:Panel>
</div>

<asp:Panel ID="Panel4" runat="server" Visible="false">
<asp:Literal ID="doingAutoSearch" runat="server" Visible="false">yes</asp:Literal>
    <asp:Literal ID="includeCreator" runat="server" Visible="false">no</asp:Literal>
	<asp:Literal ID="publisherDisplayTemplate" runat="server" Visible="false">
  <strong><a href="/Search.aspx?pub={0}" target="result">{0}</a></strong>
  
  </asp:Literal>
  	<asp:Literal ID="defaultSearchOrderBy" runat="server" Visible="false">[ResourceTotal] desc, Publisher</asp:Literal>

<asp:Literal ID="topPublisherSearchTemplate" runat="server" Visible="false">SELECT top 500 [Id] ,[Publisher] ,[ResourceTotal]
  FROM [dbo].[PublisherSummary] where [IsActive] = 1 {0}
  Order by {0}</asp:Literal>

	<asp:Literal ID="publisherSearchTemplate" runat="server" Visible="false">SELECT [Id] ,[Publisher] ,[ResourceTotal]
  FROM [dbo].[PublisherSummary] where [IsActive] = 1 {0}
  Order by {0}</asp:Literal>

</asp:Panel>