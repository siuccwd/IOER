<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContentSearch.ascx.cs" Inherits="ILPathways.LRW.controls.ContentSearch" %>


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


<script type="text/javascript" language="javascript">  var defaultFlyoutWidth = 280;</script>
<script src="/Scripts/flyout.js" type="text/javascript"></script>
<link rel="Stylesheet" href="/Styles/Flyout.css" />

<!-- Page Setup -->
<script type="text/javascript" language="javascript">
  $(document).ready(function () {
    //Inject missing attribute
    $(".txtKeyword").attr("placeholder", "Search in Resources...");

    //Fix table setup
    $("table").css("border-collapse", "separate")
    $("table th").addClass("isleH2");

  });
</script>


<!--Major page elements -->
<style type="text/css">
.column {
  display: inline-block;
  *display: inline;
  zoom: 1;
  vertical-align: top;
  box-sizing: border-box;
}
.column.left {
  width: 250px;
}
.column.right {
  width: 70%;
  margin-top: 10px;
}
    .searchSection { display: inline-block; min-width: 150px; }
</style>

<!-- Flyouts -->
<style type="text/css">
.flyoutList .flyoutTrigger {
    display: block;
    background-color: <%=css_lightblue %>;
    border-radius: 5px;
    color: #FFF;
    margin: 2px 0;
    padding: 2px 5px;
}
.flyoutList .flyoutTrigger:hover, .flyoutList .flyoutTrigger:focus {
  background-color: <%=css_orange %>;
}

.flyoutContent ul li {
  display: inline-block;
  *display: inline;
  zoom: 1;
  width: 200px; 
  cursor: pointer;
  border-radius: 5px;
  position: static;
  margin-left: 0;
}
.flyoutContent ul li input {
  margin-right: 5px;
  margin: 3px 5px 0px 5px;
  cursor: pointer;
}
.flyoutContent ul li label {
  display: inline-block;
  *display: inline;
  zoom: 1;
  height:18px;
  line-height:18px;
  border: 5px;
  cursor: pointer;
}
.flyoutContent ul li:hover, .flyoutContent ul li:focus {
  color: #FFF;
  background-color: <%=css_orange %>;
}
.flyoutContent ul.wide li {
  width: 200px;
}
.flyoutContent ul.wide li label {
  width: 200px; 
}
.leftColumn {
  width: 200px;
  display: inline-block;
  *display: inline;
  zoom: 1;
  vertical-align: top;
}
.flyoutContainer {
  top: 5px;
}
.flyoutContent h2 .cbxlReset {
  float: right;
  color: #FFF;
}
.flyoutContent h2 .cbxlReset:hover, .flyoutContent h2 .cbxlReset:focus {
  text-decoration: underline;
}
</style>

<!-- header stuff -->
<style type="text/css">
.txtKeyword {
  width: 670px;
  font-size: 130%;
}
.searchBoxHolder {
  position: relative;
  margin-bottom: 5px;
}
.searchImgBtn {
  margin-bottom: -5px;
}
.defaultButton {
  color: #FFF;
  width: auto;
  padding: 2px 10px;
  cursor: pointer;
  background-color: <%=css_black %>;
  margin: 0 1px;
}
.defaultButton.clearButton {
  height: 27px;
vertical-align: bottom;
}
.column.right .pager table tr {
  text-align: center;
  padding-right: 100px;
}
.pager {
  margin-left: 0;
  margin-right: 0;
  min-height: 29px;
}
.pageSizeController {
  float: right;
}
</style>

<!-- Table stuff -->
<style type="text/css">
table.mainDisplay {
  border-collapse: separate;
  border-spacing: 2px 10px;
}
table.mainDisplay, .column.right table tbody, table tr {
  box-sizing: border-box;
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  border: none;
}
table.mainDisplay th {
  border: none;
}
table.mainDisplay td {
  border: none;
  border-top: 1px solid <%=css_white %>;
  padding: 2px;
  /*background: transparent url('/images/grayVerticalGradient50.png') repeat-x left bottom;*/
}
table.mainDisplay tr:nth-child(2) td {
  border: none;
}
table.mainDisplay tr.gridItem, table tr.gridItem:hover {
  background-color: transparent;
}
table tr.gridItem td {
  padding: 0 3px;
}
</style>


<!--Miscellaneous -->
<style type="text/css">
  .description { margin-left:10px; }
.rightAlign {
  text-align: right;
  padding: 5px 3px;
}
.rightAlign a {
  display: inline-block;
  background-color: <%=css_orange %>;
  color: #FFF;
  font-weight: bold;
  padding: 3px 10px;
  border-radius: 5px;
  font-size: 110%;
}
.rightAlign a:hover, .rightAlign a:focus {
  background-color: <%=css_lightblue %>;
}
</style>
<script type="text/javascript">
	<!--
    function ResetForm() {
        //alert('ResetForm');

        document.getElementById("ctl00_BodyContent_search1_txtKeyword").value = "";
        $("#ctl00_BodyContent_search1_searchSummaryDesc").html("");
       // alert('buttons');
        clearRadioButtonList('<%=listCreatedBy.ClientID %>', false)
        clearRadioButtonList('<%=rblIDateCreated.ClientID %>', false)

        
        return false;
    } //end function
    function CheckBoxListSelect(cbControl, state) {
        // alert("CheckBoxListSelect for " + cbControl);
        var chkBoxList = document.getElementById(cbControl);
        var chkBoxCount = chkBoxList.getElementsByTagName("input");
        for (var i = 0; i < chkBoxCount.length; i++) {
            chkBoxCount[i].checked = state;
        }

        return false;
    }
    function clearRadioButtonList(cbControl, state) {

        var elementRef = document.getElementById(cbControl);
        var inputElementArray = elementRef.getElementsByTagName('input');

        for (var i = 0; i < inputElementArray.length; i++) {
            var inputElement = inputElementArray[i];

            inputElement.checked = false;
        }
        return false;
    }

    //-->
</script>
<asp:Panel ID="authoringPanel" runat="server" Visible="true">
  <asp:Panel ID="CustomHdrPanel" runat="server" Visible="false">
	  <h3 class="resultsHeader"><asp:Label ID="lblCustomTitle" runat="server" /></h3>
  </asp:Panel>

  <div class="column left ">
    <asp:Panel ID="approversPanel" runat="server" Visible="false">
      <asp:RadioButtonList ID="approversOptionsList" AutoPostBack="true" RepeatDirection="Horizontal" OnSelectedIndexChanged="approversOptionsList_SelectedIndexChanged" runat="server" >
        <asp:ListItem Text="Author View" Selected="True" ></asp:ListItem>
        <asp:ListItem Text="Approver View" ></asp:ListItem>
      </asp:RadioButtonList>
    </asp:Panel>
    <asp:Panel ID="dateFiltersPanel" CssClass="isleBox" runat="server" Visible="true">
      <div class="flyoutContent">
              <h2 class="isleBox_H2">Date Filters <a href="javascript:void('')" class="cbxlReset" id="reset_Dates">Reset</a></h2>
                Date last updated:
                <asp:RadioButtonList id="rblIDateCreated" causesvalidation="false" RepeatLayout="UnorderedList"   runat="server" tooltip="Date ranges" RepeatDirection="Vertical">
	                <asp:ListItem Text="Last 7 days" Value="1"></asp:ListItem>
                  <asp:ListItem Text="Last 30 days" Value="2"></asp:ListItem>
                  <asp:ListItem Text="Last 6 months" Value="3" Selected="True"></asp:ListItem>
                  <asp:ListItem Text="All" Value="4"></asp:ListItem>
              </asp:RadioButtonList>
            </div>
    </asp:Panel>
              
    <asp:Panel ID="filtersPanel" CssClass="isleBox" runat="server" Visible="false">
      <div class="flyoutContent">
              <h2 class="isleBox_H2">Created By <a href="javascript:void('')" class="cbxlReset" id="reset_Language">Reset</a></h2>
              <asp:Label ID="createdByMessage" runat="server" Visible="false"></asp:Label>
              <asp:RadioButtonList id="listCreatedBy" causesvalidation="false" RepeatLayout="UnorderedList"   runat="server" tooltip="Created By options" RepeatDirection="Vertical">
	                <asp:ListItem Text="Created By Me" Value="1" Selected="True" ></asp:ListItem>
                  <asp:ListItem Text="My Organzation" Value="2"></asp:ListItem>
                  <asp:ListItem Text="My District" Value="3"></asp:ListItem>
                  <asp:ListItem Text="All" Value="4"></asp:ListItem>
              </asp:RadioButtonList>
            </div>
    </asp:Panel>

  </div><!-- end left column -->

  <div class="column right">
    <asp:Panel runat="server" ID="createResourceLinkPanel" Visible="false" Cssclass="rightAlign">
      <a href="/My/Author.aspx">Create a New Educational Resource</a>
    </asp:Panel>

    <div class="searchBoxHolder">
      <asp:TextBox ID="txtKeyword" runat="server" CssClass="txtKeyword" />
      <asp:ImageButton ID="searchImgBtn" runat="server" CssClass="searchImgBtn" ImageUrl="~/images/icons/magnifyingBtn1.png" CommandName="Search" OnCommand="FormButton_Click" />
      <input type="button" value="Clear" title="Reset all search criteria" class="defaultButton clearButton reset" onclick="ResetForm()" />
      
      <br />
      <asp:Label ID="searchSummaryDesc" CssClass="searchSummaryDesc"  runat="server" ></asp:Label>

    </div>


<asp:Panel ID="pnlSearch" runat="server">
    <asp:panel ID="resultsPanel" runat="server">
      <div class="isleBox pager">
        <div class="pageSizeController">
          Page Size <asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" visible="true" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
        </div>
        <wcl:PagerV2_8 ID="pager1" runat="server" 
          Visible="false" 
          OnCommand="pager_Command" 
          GenerateFirstLastSection="true"
          FirstClause="First Page" 
          PreviousClause="Prev." 
          NextClause="Next" 
          LastClause="Last Page" 
          PageSize="15" 
          CompactModePageCount="8" 
          NormalModePageCount="10"  
          GenerateGoToSection="false" 
          GeneratePagerInfoSection="true"
        />
      </div>
    
      <asp:gridview id="formGrid" runat="server" 
        autogeneratecolumns="False"
        allowpaging="false" 
        PageSize="15" 
        RowStyle-CssClass="gridItem"
        allowsorting="true"  
        OnRowDataBound="formGrid_RowDataBound"
        OnPageIndexChanging="formGrid_PageIndexChanging"
        onsorting="formGrid_Sorting"	
        OnSorted="formGrid_Sorted"	
        CssClass="mainDisplay"			
      >
	      <HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
	      <columns>
        <asp:TemplateField HeaderText=""  >
          <ItemTemplate>
            <asp:HyperLink ID="editLink" runat="server" Visible="false" Target="_blank" NavigateUrl='<%# Eval("ContentRowId", "/My/Author.aspx?rid={0}") %>' CssClass="editLink textLink"> Edit <img src="/images/icons/Edit-Document-icon24.png" alt="Edit item" /></asp:HyperLink>
          </ItemTemplate>
        </asp:TemplateField>

		      <asp:TemplateField HeaderText="Title/Description" SortExpression="Title" ItemStyle-Width="50%" >
			      <ItemTemplate>

              <div class="result"><!-- begin result -->
                <div class="resultData">
                <h3 class="isleH3"><a href="/CONTENT/<%#Eval("ContentId")%>/<%# CleanTitle(DataBinder.Eval( Container.DataItem, "Title" ).ToString())%>" class="title"><%#Eval("Title")%></a></h3>
                <h3 class="isleH3" style="display:none;" ><a href="/CONTENT/<%#Eval("ContentRowId")%>/<%# CleanTitle(DataBinder.Eval( Container.DataItem, "Title" ).ToString())%>" class="title"><%#Eval("Title")%></a></h3>
                  <h3 class="isleH3" style="display:none;" >
                    <a href="/Repository/ResourcePage.aspx?rid=<%#Eval("ContentRowId")%>"  class="title textLink" >
                      <asp:Literal ID="Label1" Text='<%#Eval("Title")%>' runat="server" />
                    </a> 
                   		
                  </h3>
                  <div class="description expandCollapseBox">
                    <%# CleanDescription( DataBinder.Eval( Container.DataItem, "Summary" ).ToString(), 0 )%>
                  </div>
                  <asp:Label ID="lblCreatedById" runat="server" visible="false" Text='<%#Eval("CreatedById")%>'></asp:Label>
                </div>
              </div><!-- end result -->

			      </ItemTemplate>
		      </asp:TemplateField>
          <asp:boundfield datafield="ContentPrivilege" headertext="Privilege" ItemStyle-CssClass="resultsPrivilege" SortExpression="ContentPrivilege" Visible="true"></asp:boundfield>	
          <asp:boundfield datafield="ContentStatus" headertext="Status" SortExpression="ContentStatus" ItemStyle-Width="75px" Visible="true"></asp:boundfield>	
		      <asp:boundfield datafield="Author" headertext="Author" SortExpression="auth.SortName" Visible="true"></asp:boundfield>	
          <asp:boundfield datafield="Organization" headertext="Organization" ItemStyle-Width="100px" SortExpression="base.Organization" Visible="true"></asp:boundfield>	
          <asp:boundfield datafield="LastUpdatedDisplay" ItemStyle-Width="75px"  SortExpression="LastUpdated" headertext="Updated"></asp:boundfield>	
          <asp:boundfield datafield="OrgId" headertext="Organization" Visible="false"></asp:boundfield>
          <%--<asp:TemplateField ><ItemTemplate> <span style="margin-left:10px;">&nbsp;</span></ItemTemplate></asp:TemplateField>--%>
        </columns>
	      <pagersettings 
		      mode="NumericFirstLast"
		      firstpagetext="First"
		      lastpagetext="Last"
		      pagebuttoncount="5"  
		      position="TopAndBottom"
        /> 
	    </asp:gridview>
      <div class="isleBox pager">
        <wcl:PagerV2_8 ID="pager2" runat="server" 
          Visible="false" 
          OnCommand="pager_Command" 
          GenerateFirstLastSection="true" 
          FirstClause="First Page" 
          PreviousClause="Prev." 
          NextClause="Next" 
          LastClause="Last Page" 
          PageSize="15" 
          CompactModePageCount="8"  GenerateToolTips="true" GenerateSmartShortCuts="true" MaxSmartShortCutCount="3"
          NormalModePageCount="10" 
          GenerateGoToSection="false" 
          GeneratePagerInfoSection="true" 
        />
      </div>
    </asp:panel>
  </asp:Panel>
  </div><!-- end right column -->

  
</asp:Panel>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.LRW.controls.ContentSearch</asp:Literal>
<asp:Literal ID="txtAuthorSecurityName" runat="server" Visible="false">ILPathways.LRW.controls.Authoring</asp:Literal>
<asp:Literal ID="txtLibraryId" runat="server" Visible="false">0</asp:Literal>

<asp:Literal ID="formattedTitleTemplate" runat="server" Visible="false"><a style="color:#000;  cursor:hand;" href="{0}" target="_blank" title="Website link opens in a new window">{1}</a></asp:Literal>

</asp:Panel>

<asp:Panel ID="Panel3" runat="server" Visible="false">
<asp:Literal ID="readMoreTemplate" runat="server" Visible="false">&nbsp;...&nbsp;<p class="readMore"><a href="javascript:void()">Show More &gt;&gt;</a></p></asp:Literal>
<asp:Literal ID="readMoreTemplateSystran" runat="server" Visible="false">&nbsp;...&nbsp;<p class="readMore"><a href="javascript:void()">Show More</a></p></asp:Literal>

</asp:Panel>
<asp:Panel ID="Panel4" runat="server" Visible="false">

<asp:Literal ID="keywordTemplate" runat="server" Visible="false"> (base.Title like '{0}' OR base.[Summary] like '{0}' OR base.[Description] like '{0}' OR auth.SortName like '{0}') </asp:Literal>
<asp:Literal ID="txtMyOrgFilter" runat="server" Visible="false">(base.OrgId = {0} OR auth.OrganizationId = {0}) </asp:Literal>

<asp:Literal ID="txtCustomFilter" runat="server" Visible="false"></asp:Literal>
<asp:Literal ID="txtPublicFilter" runat="server" Visible="false">(base.PrivilegeTypeId = 1 AND base.IsActive = 1  AND base.StatusId = 5 ) </asp:Literal>
<!-- only:
(created by = me and any status) 
or public 
or ( published and (my org, or my district) )
-->
<asp:Literal ID="txtAuthFilter" runat="server" Visible="false">(base.PrivilegeTypeId = 1 AND base.StatusId = 5 ) </asp:Literal>
</asp:Panel>


