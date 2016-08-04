<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibrariesSearch.ascx.cs" Inherits="ILPathways.LRW.controls.LibrariesSearch" %>


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


<script type="text/javascript" language="javascript">  var defaultFlyoutWidth = 200;</script>
<script src="/Scripts/flyout.js" type="text/javascript"></script>
<link rel="Stylesheet" href="/Styles/Flyout.css" />

<style type="text/css">
.collectionMtcePanel { float: left;}
</style>
<a href="#" id="backToTopRight" title="back to top"></a>

<script type="text/javascript">
  var highest = 0;
  $(document).ready(function () {
    //Inject missing attribute
    $(".txtKeyword").attr("placeholder", "Search in Libraries...");

    //Fix table setup
    $("table").css("border-collapse", "separate")
    $("table th").addClass("isleH2").css("font-size", "18px");

    $(".cbxlReset").click(function () {
      var targetID = this.id.split("_")[1];
      $(".cbxl" + targetID + " input").each(function () {
        $(this).prop("checked", false);
      });
    });

    $("input[type=reset]").click(function () {
      setTimeout(resetSearchFields, 100);
    });

    $(window).scroll(function () {
      if ($(this).scrollTop() > 50) {
        $('#backToTopRight').fadeIn('slow');
      } else {
        $('#backToTopRight').fadeOut('slow');
      }
    });
    $('#backToTopRight').click(function () {
      $("html, body").animate({ scrollTop: 0 }, 500);
      return false;
    });

    //Page size controller change was only firing once, ever. so...
    //setInterval(calculateLibraryBackground, 1000);

  });
  // end ready

  function resetSearchFields() {
    $(".flyoutList input").each(function () {
      $(this).prop("checked", false);
    });
    $(".txtKeyword").val("");
    $(".searchTagsBox").slideUp("fast", function () { $(this).html(""); });
  }

  function calculateLibraryBackground() {
    highest = 0;
    //Library backgrounds
    $(".gridItem .resCount").each(function () {
      var item = parseInt($(this).text());
      if (item > highest) { highest = item; }
    });

    //console.log("highest: " + highest);
    $(".gridItem").each(function () {
      var resultBox = $(this).find(".resultData");
      var totalBox = $(this).find(".resCount");
      var showPercent = parseInt(totalBox.text()) / highest;
      var showAmount = 500 - (showPercent * 500);
      var height = totalBox.height() - 65;
      var offset = resultBox.width() - 500;
      console.log(offset);
      //if (showAmount < 30) { showAmount = 30; }
      resultBox.css("background-position", showAmount + offset + "px bottom");
    });

    $(".gridResultsHeader th").addClass("isleH2");

    //Add sorting functionality
    $("th:contains(Total)").html("<a href=\"javascript:sortRows()\">Total</a>");

  }

  //Sorting
  var sortMode = "desc";
  function sortRows() {
    if (sortMode == "asc") {
      $("tr.gridItem").sort(sortingAsc).appendTo(".gridTable");
      sortMode = "desc";
    }
    else {
      $("tr.gridItem").sort(sortingDesc).appendTo(".gridTable");
      sortMode = "asc";
    }
    $(".resultData").css("background-position", "600px bottom");
    calculateLibraryBackground();
  }
  function sortingAsc(a, b) {
    return parseInt($(a).find(".resCount").text()) > parseInt($(b).find(".resCount").text()) ? 1 : -1;
  }
  function sortingDesc(a, b) {
    return parseInt($(a).find(".resCount").text()) < parseInt($(b).find(".resCount").text()) ? 1 : -1;
  }

</script>


<!--Major page elements -->
<style type="text/css">
#container {
   width: 100%;
}
.column {
  display: inline-block;
  *display: inline;
  zoom: 1;
  vertical-align: top;
  box-sizing: border-box;
}
.column.left {
  width: 300px;
}
.column.right {
  width: 75%;
  margin-top: 10px;
}
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
  width: 250px; 
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
  width: 225px;
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
  width: 300px;
}
.flyoutContent ul.wide li label {
  width: 275px; 
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
    .keywordSection {text-align:center}
.txtKeyword {
  width: 65%;
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
  position: absolute;
  top: -2px;
}
.column.right .pager table tr {
  text-align: center;
  padding-right: 100px;
}
.pager {
  margin-left: 0;
  margin-right: 5px;
  min-height: 29px;
}
.pageSizeController {
  float: right;
}
</style>

<!-- Table stuff -->
<style type="text/css">
table {
  border-collapse: separate;
  border-spacing: 2px;
  border: none;
}
table tr.gridItem {
  background-color: transparent;
}
table tr.gridItem:hover, table tr.gridItem:focus {
  background-color: <%=css_white %>;
}
table tr.gridItem td  {
  padding: 0 5px;
  border-top: 1px solid <%=css_white %>;
}
table tr.gridItem:nth-child(2) td {
  border-top: none;
}
.resCount { font-size: 120%; color: <%=css_blue %>;}
.resultData {
  background: transparent url('') no-repeat bottom right;
  background-position: 600px bottom;
  transition: background-position: 0.8s;
  -webkit-transition: background-position 0.8s;
  -ms-transition: background-position 0.8s;
  -moz-transition: background-position 0.8s;
  -o-transition: background-position: 0.8s;
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
.libraryImg {
  float: left;
  display: block;
  width: 100px;
  height: 100px;
  margin: 5px;
  border-radius: 5px;
  overflow: hidden;
}

</style>

<style type="text/css">

@media screen and (max-width: 550px) {
.column.left {  width: 200px; }
.column.right {  width: 400px; }
.keywordSection {text-align:left}
}

#container { min-width: 300px; }
tr.gridItem td:nth-child(3), tr.gridItem td:nth-child(4), tr.gridItem td:nth-child(5), tr.gridItem td:nth-child(6) { width: 75px; }

@media screen and (max-width: 750px) {
  tr.gridItem, table tr.gridItem td { display: block; border: none; }
  tr.gridItem { margin-bottom: 10px; border-top: 1px solid #AAA; border-bottom: 1px solid #AAA; }
  tr.gridResultsHeader { display: none; }
  tr.gridItem td:before { font-weight: bold; }
  tr.gridItem td:nth-child(2):before { content: 'Library Type: '; }
  tr.gridItem td:nth-child(3):before { content: 'Contact: '; }
  tr.gridItem td:nth-child(4):before { content: 'Organization: '; }
  tr.gridItem td:nth-child(5):before { content: 'Total: '; }
  tr.gridItem td:nth-child(6):before { content: 'Updated: '; }
  tr.gridItem td:nth-child(3), tr.gridItem td:nth-child(4) ,tr.gridItem td:nth-child(5), tr.gridItem td:nth-child(6) { width: auto; }

}
</style>
<div ID="container" >

<!-- -->
    <div class="searchTagsBox" style="margin: 5px 50px; display:none;">
    <asp:Label ID="searchSummaryDesc" runat="server" ></asp:Label>
    </div>

 <div class="column left isleBox">
 <h2 class="isleBox_H2">Filtering Options</h2>
    <ul class="flyoutList">
     <li>
      <a href="javascript:void('')" class="flyoutTrigger" id="flyout_dates">Date Filters &gt;</a>
      <div class="flyoutContainer isleBox" id="flyoutContainer_dates">
        <div class="flyoutContent">
          <h2 class="isleBox_H2">Date Filters <a href="javascript:void('')" class="cbxlReset" id="reset_Dates">Reset</a></h2>
            Date resources added to library:
            <asp:RadioButtonList id="rblIDateCreated" causesvalidation="false"   runat="server" tooltip="Date ranges"  RepeatLayout="UnorderedList" RepeatDirection="Vertical">
	            <asp:ListItem Text="Last 7 days" Value="1"></asp:ListItem>
              <asp:ListItem Text="Last 30 days" Value="2"></asp:ListItem>
              <asp:ListItem Text="Last 6 months" Value="3"></asp:ListItem>
              <asp:ListItem Text="Last year" Value="4"></asp:ListItem>
              <asp:ListItem Text="All" Value="5"></asp:ListItem>
          </asp:RadioButtonList>
        </div>
      </div>
    </li>

    <li>
      <a href="javascript:void('')" class="flyoutTrigger" id="flyout_libraryType">Types &gt;</a>
      <div class="flyoutContainer isleBox" id="flyoutContainer_libraryType">
        <div class="flyoutContent">
          <h2 class="isleBox_H2">Library Type<a href="javascript:void('')" class="cbxlReset" id="reset_libraryType">Reset</a></h2>
			    <asp:CheckBoxList ID="cbxlLibraryType" runat="server" CssClass="cbxlLibraryType" RepeatLayout="UnorderedList"></asp:CheckBoxList>
        </div>
      </div>
    </li>
    <li>
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_viewable">Views</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_viewable">
            <div class="flyoutContent">
              <h2 class="isleBox_H2">View Type <a href="javascript:void('')" class="cbxlReset" id="reset_viewable">Reset</a></h2>
              <asp:RadioButtonList id="rblViewable" causesvalidation="false" RepeatLayout="UnorderedList"   runat="server" tooltip="Viewable options" RepeatDirection="Vertical">
                  <asp:ListItem Text="All" Value="1" ></asp:ListItem>
                  <asp:ListItem Text="Public Only" Value="2" Selected="True"></asp:ListItem>
                  <asp:ListItem Text="Private Only" Value="3" Enabled="false"></asp:ListItem>
                  <asp:ListItem Text="Where Library Member" Value="4"></asp:ListItem>
              </asp:RadioButtonList>
            </div>
          </div>
        </li>
    </ul>
    </div>

  <!-- right column -->
  <div class="column right">
       
	<div class="keywordSection" >   
	  <asp:TextBox ID="txtKeyword" runat="server"  CssClass="txtKeyword"></asp:TextBox>&nbsp;<asp:ImageButton ID="searchImgBtn" runat="server" CssClass="searchImgBtn" ImageUrl="~/images/icons/magnifyingBtn1.png" CommandName="Search" OnCommand="FormButton_Click"  />  <input type="reset" class="defaultButton resultsApply" value="Clear"  /><br />
    <asp:button ID="btnSearch" runat="server" CssClass="defaultButton resultsApply offScreen" CommandName="Search" OnCommand="FormButton_Click" Text="Search" causesvalidation="False" Visible="true" ></asp:button>
  

    </div>

  </div>
    <div id="resultsMainColumn" class="columns"  >
     <asp:UpdatePanel ID="resultsUPanel" runat="server" UpdateMode="Conditional">
    <ContentTemplate>
    
  
  <asp:Panel ID="resultsPanel" runat="server" >
     <div class="isleBox pager">
        <div class="pageSizeController">
          Page Size <asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" visible="true" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged" />
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
          CompactModePageCount="4" GenerateToolTips="true" 
          NormalModePageCount="5"  
          GenerateGoToSection="false" 
          GeneratePagerInfoSection="true"
        />
      </div>

	  <asp:gridview id="formGrid" runat="server" 
          autogeneratecolumns="False"
          allowpaging="false" 
          DataKeyNames="LibraryId"
          PageSize="15" 
          RowStyle-CssClass="gridItem"
          allowsorting="True"  
          OnRowDataBound="formGrid_RowDataBound"
          OnPageIndexChanging="formGrid_PageIndexChanging"
          onsorting="formGrid_Sorting"	
          OnSorted="formGrid_Sorted"				
          captionalign="Top"
          width="100%" BorderWidth="0" GridLines="none"
          style="border-collapse: separate" CssClass="gridTable"
      >
	<HeaderStyle CssClass="gridResultsHeader isleH2" horizontalalign="Left" />
	<columns>
		<asp:TemplateField HeaderText="Title/Description" SortExpression="lib.Title">
			<ItemTemplate>

        <div class="result"><!-- begin result -->
          <div class="resultData">
            <h3 class="isleH3"><a href="/Libraries/Library.aspx?id=<%#Eval("LibraryId")%>" class="title" ><asp:Literal ID="Label1" Text='<%#Eval("Title")%>' runat="server"></asp:Literal></a></h3>

            <div class="libraryImg">
            <a href="/Libraries/Library.aspx?id=<%#Eval("LibraryId")%>" title="library icon and link" > <%# FormatLibraryImage( DataBinder.Eval( Container.DataItem, "ImageUrl" ).ToString() )%></a>
            </div>
            <div class="description expandCollapseBox" style="display:inline-block; width:250px;">
              <%# DataBinder.Eval( Container.DataItem, "Description" ).ToString()%>
              <br /><strong>Collections</strong>:<br /><%# DataBinder.Eval( Container.DataItem, "Collections" ).ToString()%>
            </div>
            <div class="clearFloat"></div>
          </div>
        </div><!-- end result -->

			</ItemTemplate>
		</asp:TemplateField>
      <asp:boundfield datafield="LibraryType" headertext="Type" SortExpression="libt.Title" Visible="true"></asp:boundfield>	
			<asp:boundfield datafield="ViewType" InsertVisible="true" headertext="View" SortExpression="lib.IsPublic" Visible="false"></asp:boundfield>	
      <asp:boundfield datafield="SortName" headertext="Contact" SortExpression="owner.SortName" Visible="true"></asp:boundfield>
      <asp:boundfield datafield="Organization" headertext="Organization" SortExpression="Organization" Visible="true"></asp:boundfield>	
      <asp:boundfield datafield="TotalSections"  headertext="Collections" ItemStyle-HorizontalAlign="Right" Visible="false"  ></asp:boundfield>	
      <asp:boundfield datafield="TotalResources" SortExpression="TotalResources"  headertext="Total" ItemStyle-HorizontalAlign="Right" ItemStyle-CssClass="resCount" ></asp:boundfield>	
      <asp:boundfield datafield="ResourceLastAddedDate" SortExpression="ResourceLastAddedDate" headertext="Updated" DataFormatString="{0:d}" ItemStyle-HorizontalAlign="Right" ></asp:boundfield>	
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
          CompactModePageCount="4"  GenerateToolTips="true" 
          NormalModePageCount="5" 
          GenerateGoToSection="false" 
          GeneratePagerInfoSection="true" 
        />
      </div>


  </asp:Panel>

  </ContentTemplate>
    </asp:UpdatePanel>

  </div><!-- end results column -->

</div>


<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.LRW.controls.LibrarySearch</asp:Literal>
<asp:Literal ID="txtLibraryId" runat="server" Visible="false">0</asp:Literal>
<asp:Literal ID="usingFullTextOption" runat="server" Visible="false">false</asp:Literal>
<asp:Literal ID="txtSubscribedLibsView" runat="server" Visible="false">no</asp:Literal>

<asp:Literal ID="usingFormattedCollectionTitle" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="areToolsActive" runat="server" Visible="false">yes</asp:Literal>

<asp:Literal ID="recommendationsTemplate" runat="server" Visible="false"><br /><i>{0}</i></asp:Literal>
<asp:Literal ID="formattedTitleTemplate" runat="server" Visible="false"><a style="color:#000;  cursor:hand;" href="{0}" target="_blank" title="Website link opens in a new window">{1}</a></asp:Literal>

</asp:Panel>

<asp:Panel ID="Panel1" runat="server" Visible="false">

<asp:Literal ID="collectionsTemplate" runat="server" Visible="false"><li><a href="/My/Default.aspx?sId={0}">{1}</a></li></asp:Literal>
<asp:Literal ID="allCollectionsLabel" runat="server" Visible="false">All Collections (whole library)</asp:Literal>
</asp:Panel>

<asp:Panel ID="Panel3" runat="server" Visible="false">
<asp:Literal ID="txtNoFavsMessage" runat="server" Visible="false">Note: you currently do not have any followed libraries. COMING SOON:<br/>To follow a library:<ul><li>Go to <a href="/Libraries/Default.aspx">Libraries search</a></li><li> do a search, select a library and,</li><li> follow from the library detail page</li></ul>.<br/><br/>Defaulting to the public view of libraries.</asp:Literal>

<asp:Literal ID="readMoreTemplate" runat="server" Visible="false">&nbsp;...&nbsp;<p class="readMore"><a href="javascript:void()">Show More &gt;&gt;</a></p></asp:Literal>
<asp:Literal ID="readMoreTemplateSystran" runat="server" Visible="false">&nbsp;...&nbsp;<p class="readMore"><a href="javascript:void()">Show More</a></p></asp:Literal>

</asp:Panel>
<asp:Panel ID="Panel4" runat="server" Visible="false">
<asp:Literal ID="formattedSourceTemplate" runat="server" Visible="false"><a class="textLink" href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>
<asp:Literal ID="formattedCCRightsUrl2" runat="server" Visible="false"><a class="textLink" href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>
<!-- code table Filters 
NOTE: libraries were moved to the content db. Views were created to match the table names from the IOER db
-->
<asp:Literal ID="subscribedLibrariesFilter" runat="server" Visible="false">
( lib.Id in (SELECT  LibraryId FROM [Library.Subscription] where UserId = {0}) )</asp:Literal>

 <asp:Literal ID="keywordTemplate" runat="server" Visible="false"> (lib.Title like '{0}'  OR lib.[Description] like '{0}' OR owner.[SortName] like '{0}'  OR owner.[Organization] like '{0}' OR org.[Name] like '{0}') </asp:Literal>

<asp:Literal ID="hasStandardsTemplate" runat="server" Visible="false"></asp:Literal>

<asp:Literal ID="txtLibraryImageTemplate" runat="server" Visible="false"><img src='{0}' width='{1}px' data-height='{1}px' {2} alt='library icon'/></asp:Literal>
<asp:Literal ID="txtDefaultLibraryImage" runat="server" Visible="false"></asp:Literal>
<asp:Literal ID="txtImgageWidth" runat="server" Visible="false">100</asp:Literal>
<asp:Literal ID="txtLowOpacity" runat="server" Visible="false">style="opacity:0.2;filter:alpha(opacity=20)"</asp:Literal>
</asp:Panel>


