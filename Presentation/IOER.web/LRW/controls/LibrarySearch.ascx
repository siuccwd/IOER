<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibrarySearch.ascx.cs" Inherits="ILPathways.LRW.controls.LibrarySearch" %>
<%@ Register Namespace="workNetCustomGridView" TagPrefix="wclGrid" Assembly="workNetCustomGridView" %>
<%@ Register TagPrefix="uc1" TagName="LibraryMtce" Src="/LRW/controls/LibraryMtce.ascx" %>
<%@ Register TagPrefix="uc1" TagName="LibraryCollectionMtce" Src="/LRW/controls/LibraryCollectionMtce.ascx" %>


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

<script type="text/javascript">
<!--
	function confirmUnsubscribe(recordTitle, id) {
		var bresult
		bresult = confirm("Are you sure you want to unfollow this library? \n\n"
            + "Click OK to unfollow this library or click Cancel.");

		if (bresult) {
			return true;
		} else {
			return false;
		}


	} //  
//-->
</script>
<script type="text/javascript" language="javascript">  var defaultFlyoutWidth = 250; </script>
<script type="text/javascript" language="javascript" src="/Scripts/flyout.js"></script>
<link rel="Stylesheet" href="/Styles/Flyout.css" />
<script type="text/javascript" language="javascript" src="/Scripts/fadeCollapse.js"></script>
<link rel="Stylesheet" href="/Styles/fadeCollapse.css" />

<!-- Page Setup -->
<script type="text/javascript" language="javascript">
  $(document).ready(function () {

    $(".cbxlReset").click(function () {
      var targetID = this.id.split("_")[1];
      $(".cbxl" + targetID + " input").each(function () {
        $(this).prop("checked", false);
      });
    }); //cbxlReset

    $("input[type=reset]").click(function () {
      setTimeout(resetSearchFields, 100);
    });
    //Hide unwanted filters
    $(".filters .flyoutContent ul").each(function () {
      if ($(this).find("li").length == 0) {
        $(this).parentsUntil(".flyoutList").remove();
      }
    }); //filters

    //Inject missing attribute
    $(".txtKeyword").attr("placeholder", "Search in Library...");

    //Back to Top
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
  });       // end ready


  // ==========================================================
  function resetSearchFields() {
    $(".flyoutList input").each(function () {
      $(this).prop("checked", false);
    });

    $(".txtKeyword").val("");
    $(".searchTagsBox").slideUp("fast", function () { $(this).html(""); });
  } //resetSearchFields

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
  width: 200px;
}
.column.right {
  width: 800px;
  margin-top: 10px;
}
</style>
<!-- Table stuff -->
<style type="text/css">
.column.right table, .column.right table tbody, .column.right table tr {
  display: block;
  width: 800px;
  max-width: 800px;
  min-width: 800px;
  box-sizing: border-box;
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  border: none;
}
.column.right table th {
  border: none;
}
.column.right table th input {
  margin: 5px 5px 5px 0;
}
.column.right table td {
  display: inline-block;
  *display: inline;
  zoom: 1;
  border: none;
}
.column.right table tr.gridItem, .column.right table tr.gridItem:hover {
  background-color: transparent;
  position: relative;
}
table tr.gridItem td {
  padding: 0 3px;
}
</style>
<!-- Search Results -->
<style type="text/css">
.column.right table tr.gridItem td:nth-child(1) {
  /* contains the checkbox */
  width: 16px;
  padding-top: 10px;
}
.column.right table tr.gridItem td:nth-child(2) {
  width: 760px;
}
.column.right table tr.gridItem td .result {
  width: 760px;
  margin-bottom: 50px;
}
.column.right table tr.gridItem td .resultData {
  width: 560px;
  padding: 0 2px 0 5px;
  display: inline-block;
  *display: inline;
  zoom: 1;
  box-sizing: border-box;
  -webkit-box-sizing: border-box;
  -moz-box-sizing: border-box;
  vertical-align: top;
}
.column.right table tr.gridItem td .thumbnailHolder {
  width: 200px;
  display: inline-block;
  *display: inline;
  zoom: 1;
  box-sizing: border-box;
  vertical-align: top;
  border-radius: 5px;
  overflow: hidden;
  border: 1px solid <%=css_white %>;
  width: 200px;
  height: 150px;
  margin-right: -10px;
}
.result ul {
  list-style-type: none;
}
.result ul.metadata {
  font-size: 80%;
  color: #999;
  text-align: right;
  padding-right: 8px;
}
.result ul.paradata, .result ul li, .result .description, .result h3 {
  display: inline-block;
  *display: inline;
  zoom: 1;
  box-sizing: border-box;
  vertical-align: top;
}
.result ul.metadata li {
  padding: 1px 3px;
  margin-left: 10px;
}
.result .description {
  width: 445px;
  margin-left:10px;
}
.result ul.paradata {
  width: 80px;
  margin-top: -32px;
}
.result h3 {
  width: 95%;
  margin: inherit 0 0 0;
}
.result ul.paradata li {
  width: 80px;
  text-align: right;
  margin: 2.5px 0;
  padding: 0;
  height: 25px;
  line-height: 25px;
}
</style>
<!-- header stuff -->
<style type="text/css">
.txtKeyword {
  width: 708px;
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
  margin-right: 0;
  min-height: 30px;
}
.pageSizeController {
  float: right;
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
<!-- Subscription Items -->
<style type="text/css">
.subscriptionDDL, .subscriptionButton {
  width: 100%;
  margin: 3px 0;
}
</style>
<!-- Miscellaneous -->
<style type="text/css">
a#backToTopRight {
	width:64px;
	height:64px;
	opacity:0.5;
	position:fixed;
	bottom:15px;
  right: 10px;
	display:none;
	text-indent:-10000px;
	outline:none !important;
	background-image: url('/images/icons/Top.png');
	background-repeat: no-repeat;
	z-index:500;
}

.collectionContent ul li {
  display: inline-block;
  *display: inline;
  zoom: 1;
  width: 170px; 
  cursor: pointer;
  border-radius: 5px;
  position: static;
  margin-left: 0;
}
.collectionList li {
  padding: 0;
}
.collectionList li a {
  display: block;
  padding: 1px 5px;
}
.collectionList li a:hover, .collectionList li a:focus {
  color: #FFF;
  background-color: <%=css_orange %>;
}
.addCollection {
  display: block;
  color: #FFF;
  background-color: <%=css_lightblue %>;
  border-radius: 5px;
  text-align: center;
  padding: 1px 5px;
  margin: 2px 0;
}
.addCollection:hover, .addCollection:focus {
  color: #FFF;
  background-color: <%=css_orange %>;
}
.toggleVictim {
  text-align: center;
}
.searchSection.isleBox {
  display: inline-block;
  *display: inline;
  zoom: 1;
  margin: 2px 5px 10px 5px;
}
.libraryAvatar {
  text-align: center;
  max-width: 100%;
  max-height: 150px;
  overflow: hidden;
}
.libraryAvatar img {
  border-radius: 5px;
}
.btnShowMTCE {
  display:block;
  margin: 5px auto;
}
</style>

<div id="enableFollowToggler" runat="server">
<script type="text/javascript" language="javascript">
$(document).ready(function () {

    enableFollowOptions();
  });       // end ready  

  // ==========================================================
  function enableFollowOptions() {
    $(".followOptions").each(function () {
      var box = $(this);
      box.html(
    });
  }     
</script>
</div>
<h1 class="isleH1"><asp:label ID="libHeading" runat="server">Library</asp:label></h1>
<asp:Panel ID="accessPanel" runat="server" Visible="false">
<h2>Private Library</h2>
<p>Access to this library is by request only.</p>
<p>Coming soon ability to request access.</p>
</asp:Panel>

<asp:Panel ID="confirmationPanel" runat="server" Visible="false">
<h2>Access Request Library</h2>
<asp:label ID="lblResponse" runat="server"></asp:label>
</asp:Panel>
<asp:Panel ID="maintenancePanel" runat="server" Visible="false">
	<asp:Button ID="btnShowSearch" runat="server" Visible="true" CssClass="defaultButton" Text="&larr; Back to Library"	OnClick="btnShowSearch_Click" />
	<uc1:LibraryMtce ID="LibraryMtce1" DefaultLibraryTypeId="1" runat="server"></uc1:LibraryMtce>
</asp:Panel>

<asp:Panel ID="pnlContainer" runat="server">
  <!-- Header -->
  

  <!-- Left Column -->
  <div class="column left">
    <div class="isleBox libraryAvatar">
      <asp:Literal ID="imageTag" runat="server" ></asp:Literal>
    </div>
    <div class="isleBox">
      <h2 class="isleBox_H2">Management</h2>

      <asp:Panel ID="CustomHdrPanel" runat="server" Visible="false">
        <h2 class="isleH4_Block"><asp:Label ID="lblCustomTitle" runat="server" /></h2>
        <asp:Panel ID="LibraryCollectionMtcePanel" CssClass="collectionMtcePanel" runat="server" Visible="false">
          <ul class="flyoutList">
            <li>
              <a href="javascript:void('')" class="flyoutTrigger" id="flyout_libraryColl" flyoutwidth="500"><asp:Label ID="lblUpdateCollection" runat="server"></asp:Label> &gt;</a>
              <div class="flyoutContainer isleBox" id="flyoutContainer_libraryColl">
                <div class="flyoutContent" style="width: 500px">
                  <asp:label ID="lblCollectionSummary" runat="server" Visible="false" ></asp:label>
                  <uc1:LibraryCollectionMtce ID="LibraryCollectionMtce1" DefaultSectionTypeId="3" runat="server" />
                </div>
              </div> 
            </li>
          </ul>
        </asp:Panel>
        <asp:Panel ID="collectionSubscriptionPanel" runat="server" Visible="false">
          <ul class="flyoutList">
            <li>
              <a href="javascript:void('')" class="flyoutTrigger" id="flyout_collSub" flyoutwidth="500">Follow This Collection &gt;</a>
              <div class="flyoutContainer isleBox" id="flyoutContainer_collSub">
                <div class="flyoutContent" style="width: 500px">
                  <h2 class="isleBox_H2">Follow This Collection</h2>
                  <p>Tell me about updates to this Collection:</p>
                  <asp:DropDownList ID="ddlCollectionSubscriptions" runat="server" CssClass="subscriptionDDL">
                    <asp:ListItem Value="0" Text="Never (Not Following)" />
                    <asp:ListItem Value="1" Text="Never (No Notifications)" />
                    <asp:ListItem Value="2" Text="Up to once a week" />
                    <asp:ListItem Value="3" Text="Up to once a day" />
                    <asp:ListItem Value="4" Text="Every time this Library is updated" />
                  </asp:DropDownList>
                  <asp:Button ID="btnSubscribeToThisCollection" OnClick="btnSubscribeToThisCollection_Click" Text="Update Following" CssClass="defaultButton subscriptionButton" runat="server" />
                </div>
              </div> 
            </li>
          </ul>
        </asp:Panel>
      </asp:Panel>

      <asp:Panel ID="libMtceFlyoutPanel" runat="server">
        <ul class="flyoutList">
          <li>
            <a href="javascript:void('')" class="flyoutTrigger" id="flyout_library" flyoutWidth="500">Library &gt;</a>
            <div class="flyoutContainer isleBox" id="flyoutContainer_library">
              <div class="flyoutContent" style="width: 500px;">
                <asp:label ID="libraryStateMsg" runat="server" ></asp:label>
                <asp:Panel ID="libMtcePanel" runat="server" Visible="false">
									<asp:Button ID="btnShowMtce" runat="server" CssClass="defaultButton btnShowMTCE" Visible="true" Text="Edit Library" OnClick="btnShowMtce_Click" />
                </asp:Panel>
              </div>
            </div>
          </li>
       </ul>
      </asp:Panel>
      <asp:Panel ID="libSubscriptionPanel" runat="server" Visible="false">
        <ul class="flyoutList">
          <li>
            <a href="javascript:void('')" class="flyoutTrigger" id="flyout_librarySub" flyoutWidth="300">Follow This Library &gt;</a>
            <div class="flyoutContainer isleBox" id="flyoutContainer_librarySub">
              <div class="flyoutContent" style="width: 300px">
                <h2 class="isleBox_H2">Follow This Library</h2>
                <p>Tell me about updates to this Library:</p>
                <asp:DropDownList ID="ddlLibrarySubscriptionNotificationFrequency" visible="false" runat="server" CssClass="subscriptionDDL">                </asp:DropDownList>

                <asp:DropDownList ID="ddlLibrarySubscriptions" runat="server"  CssClass="subscriptionDDL">
                  <asp:ListItem Value="0" Text="Never (Not Following)" />
                  <asp:ListItem Value="1" Text="Never (No Notifications)" />
                  <asp:ListItem Value="2" Text="Up to once a week" />
                  <asp:ListItem Value="3" Text="Up to once a day" />
                  <asp:ListItem Value="4" Text="Every time this Library is updated" />
                </asp:DropDownList>
                <asp:Button ID="btnSubscribeToThisLibrary" OnClick="btnSubscribeToThisLibrary_Click" Text="Follow" CssClass="defaultButton subscriptionButton" runat="server" />
                <asp:Button ID="btnUpdateSubscribe" OnClick="btnUpdateSubscribe_Click" Text="Update Following" visible="false" CssClass="defaultButton subscriptionButton" runat="server" />
                <asp:Button ID="btnUnSubscribe" OnClick="btnUnSubscribe_Click" Text="Unfollow" Visible="false" CssClass="defaultButton subscriptionButton" OnClientClick="" runat="server" />
              </div>
            </div>
          </li>
       </ul>
      </asp:Panel>

    
    </div>
    <asp:Panel ID="collectionsPanel" runat="server" Visible="false" CssClass="isleBox">
      <h2 class="isleBox_H2">Collections</h2>
        <div class="collectionContent" style="width: 170px">
                
          <asp:Label ID="collectionsList" CssClass="collectionList" runat="server" ></asp:Label>
                
          <asp:Panel ID="addCollectionPanel" runat="server" Visible="false">
            <a href="javascript:void('')" class="toggleTrigger addCollection" id="trigger_2">Add Collection</a>
            <div class="toggleVictim hidden" id="victim_2">
              Name&nbsp;<asp:TextBox ID="txtCollectionName" runat="server" Width="165px" MaxLength="50" ></asp:TextBox>
              <asp:LinkButton id="addCollectionBtn" runat="server" CommandName="AddCollection" OnCommand="FormButton_Click" Text="Add" CssClass="addCollection" ></asp:LinkButton>
              <asp:Label ID="addCollectionMsg" Visible="false" runat="server" ></asp:Label>
            </div>
          </asp:Panel>
        </div>
           
  
      </asp:Panel>
    
    <asp:Panel ID="detailsPanel" runat="server" Visible="false" CssClass="isleBox filters">
    
      <h2 class="isleBox_H2">Library Filters</h2>

      <ul class="flyoutList">
        <li>
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_dates">Date Filters &gt;</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_dates">
            <div class="flyoutContent">
              <h2 class="isleBox_H2">Date Filters <a href="javascript:void('')" class="cbxlReset" id="reset_Dates">Reset</a></h2>
                Date added to library:
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
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_accessRights">Access Rights &gt;</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_accessRights">
            <div class="flyoutContent">
              <h2 class="isleBox_H2">Access Rights <a href="javascript:void('')" class="cbxlReset" id="reset_AccessRights">Reset</a></h2>
			        <wcl:skmCheckBoxList ID="cbxlAccessRights" CssClass="cbxlAccessRights" runat="server" RepeatLayout="UnorderedList" AccessiblePrefix="" Visible="true" Enabled="false"></wcl:skmCheckBoxList>	
            </div>
          </div>
        </li>
        <li>
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_targetAudience">Target Audience &gt;</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_targetAudience">
            <div class="flyoutContent">
              <h2 class="isleBox_H2">Target Audience <a href="javascript:void('')" class="cbxlReset" id="reset_TargetAudience">Reset</a></h2>
		          <wcl:skmCheckBoxList ID="cbxlAudience" CssClass="cbxlTargetAudience" runat="server" RepeatLayout="UnorderedList" AccessiblePrefix="" AutoPostBack="false" Enabled="false"></wcl:skmCheckBoxList>
            </div>
          </div>
        </li>
        <li>
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_gradeLevels">Grade Levels &gt;</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_gradeLevels">
            <div class="flyoutContent">
              <h2 class="isleBox_H2">Grade Levels <a href="javascript:void('')" class="cbxlReset" id="reset_GradeLevel">Reset</a></h2>
		          <wcl:skmCheckBoxList ID="cbxGradeLevel" CssClass="cbxlGradeLevel" runat="server" RepeatLayout="UnorderedList" AccessiblePrefix=""  Visible="true" Enabled="false"></wcl:skmCheckBoxList>
            </div>
          </div>
        </li>
        <li>
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_ccss">CCSS &gt;</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_ccss">
            <div class="flyoutContent">
              <h2 class="isleBox_H2">Has Any Common Core Standards <a href="javascript:void('')" class="cbxlReset" id="reset_ccss">Reset</a></h2>
		          <asp:CheckBoxList ID="cbxlCCSS" CssClass="cbxlCCSS" runat="server" RepeatLayout="UnorderedList" AccessiblePrefix=""  Visible="true" Enabled="true">
                <asp:ListItem Text="Has Any CCSS Standard" Value="1"></asp:ListItem>
              </asp:CheckBoxList>
            </div>
          </div>
        </li>
        <li>
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_resourceType" flyoutWidth="510">Resource Type &gt;</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_resourceType">
            <div class="flyoutContent" style="width:510px">
              <h2 class="isleBox_H2">Resource Type <a href="javascript:void('')" class="cbxlReset" id="reset_ResourceType">Reset</a></h2>
		          <wcl:skmCheckBoxList ID="cbxlResType" CssClass="cbxlResourceType" runat="server" RepeatLayout="UnorderedList" AccessiblePrefix=""  Enabled="false"></wcl:skmCheckBoxList>
            </div>
          </div>
        </li>
        <li>
          <a href="javascript:void('')" class="flyoutTrigger" id="flyout_resourceFormat">Media Type &gt;</a>
          <div class="flyoutContainer isleBox" id="flyoutContainer_resourceFormat">
            <div class="flyoutContent">
              <h2 class="isleBox_H2">Media Type <a href="javascript:void('')" class="cbxlReset" id="reset_ResourceFormat">Reset</a></h2>
		          <wcl:skmCheckBoxList ID="cbxlFormats" CssClass="cbxlResourceFormat" runat="server" RepeatLayout="UnorderedList" AccessiblePrefix="" AutoPostBack="false" Visible="true"></wcl:skmCheckBoxList>
            </div>
          </div>
        </li>
      </ul>
    </asp:Panel>

  </div><!-- End Left Column -->

  <!-- right column -->
  <div class="column right">

    <asp:Panel ID="pnlSearch" runat="server" CssClass="searchBoxHolder">
      <asp:TextBox ID="txtKeyword" runat="server" CssClass="txtKeyword"></asp:TextBox>
      <%-- <input type="text" placeholder="Search in Library..." id="txtKeyword" runat="server" class="txtKeyword" />--%>
      <asp:ImageButton ID="searchImgBtn" runat="server" CssClass="searchImgBtn" ImageUrl="~/images/icons/magnifyingBtn1.png" CommandName="Search" OnCommand="FormButton_Click"  />
      <input type="reset" class="defaultButton resultsApply clearButton" value="Clear"  />
      <asp:button ID="btnSearch" runat="server" CssClass="defaultButton resultsApply offScreen" CommandName="Search" OnCommand="FormButton_Click" Text="Search" causesvalidation="False" Visible="true" />
    </asp:Panel>

    <div class="searchTagsBox">
      <asp:Label ID="searchSummaryDesc" runat="server" />
    </div>

    <asp:Panel ID="actionsPanel" runat="server" Visible="true" CssClass="collectionActions">
      <asp:DropDownList ID="ddlCheckOptions" runat="server">
        <asp:ListItem Text="Action:" Value="0"></asp:ListItem>
        <asp:ListItem Text="Copy" Value="1"></asp:ListItem>
        <asp:ListItem Text="Move" Value="2"></asp:ListItem>
        <asp:ListItem Text="Delete" Value="3"></asp:ListItem>
      </asp:DropDownList>
              
      <asp:DropDownList ID="ddlTargetCollection" runat="server">
        <asp:ListItem Text="To Collection - TBD" Value="0"></asp:ListItem>
      </asp:DropDownList>
      <asp:Button ID="btnSelect" runat="server" Visible="false" CssClass="defaultButton actionApply" CommandName="SelectChecked" OnCommand="FormButton_Click" Text="Update" CausesValidation="false" />
      </asp:Panel>
      
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
          CompactModePageCount="4" 
          NormalModePageCount="5"  
          GenerateGoToSection="false" 
          GeneratePagerInfoSection="true"
        />
      </div>
 
                   <%--<h3 class="isleH3"><a href="/ResourceDetail.aspx?vid=<%#Eval("ResourceVersionIntId")%>" class="title"><%#Eval("Title")%></a></h3>--%>

    <asp:Panel ID="resultsPanel" runat="server" CssClass="resultsBox">
	    <wclGrid:gridview id="formGrid" runat="server" 
        autogeneratecolumns="False"
        allowpaging="false" 
        DataKeyNames="LibraryResourceId, ResourceIntId"
        PageSize="15" 
        RowStyle-CssClass="gridItem"
        allowsorting="True"  
        OnRowDataBound="formGrid_RowDataBound"
        OnPageIndexChanging="formGrid_PageIndexChanging"
        onsorting="formGrid_Sorting"	
        OnSorted="formGrid_Sorted"				
        captionalign="Top"
      >
	      <HeaderStyle CssClass="gridResultsHeader isleH2" />
	      <columns>
		      <asp:TemplateField HeaderText="Title/Description" SortExpression="SortTitle">
			      <ItemTemplate>
              <div class="result"><!-- begin result -->
                <div class="resultData">
                  <h3 class="isleH3"><a href="/IOER/<%#Eval("ResourceVersionIntId")%>/<%# CleanTitle(DataBinder.Eval( Container.DataItem, "SortTitle" ).ToString())%>" class="title"><%#Eval("Title")%></a></h3>
                  <div class="description expandCollapseBox">
                    <%# CleanDescription( DataBinder.Eval( Container.DataItem, "Description" ).ToString(), 0 )%>
                    
                  </div>
                  <ul class="paradata">
                  <%# HandleResourceCounts( DataBinder.Eval( Container.DataItem, "LikeCount" ).ToString(), DataBinder.Eval( Container.DataItem, "DislikeCount" ).ToString(), DataBinder.Eval( Container.DataItem, "NbrEvaluations" ).ToString(), DataBinder.Eval( Container.DataItem, "NbrComments" ).ToString(), DataBinder.Eval( Container.DataItem, "NbrStandards" ).ToString() )%>
                  </ul>
                  <ul class="paradata" style="display: none;">
                    <li><%#Eval( "LikeCount" )%> <img src="/images/icons/icon_likes.png" alt="Likes" title="Likes" /></li>
                    <li><%#Eval( "DislikeCount" )%> <img src="/images/icons/icon_dislikes.png" alt="Dislikes" title="Dislikes" /></li>
                    <li><%#Eval( "NbrEvaluations" )%> <img src="/images/icons/icon_ratings.png" alt="Evaluations" title="Evaluations" /></li>
                    <li><%#Eval( "NbrComments" )%> <img src="/images/icons/icon_comments.png" alt="Dislikes" title="Comments" /></li>
                    <li><%#Eval( "NbrStandards" )%> <img src="/images/icons/icon_standards.png" alt="Dislikes" title="Standards" /></li>
                  </ul>
                </div>
                <div class="thumbnailHolder">
                  <a href="<%#Eval( "resourceURL" )%>"><%# RenderThumbnailURL( DataBinder.Eval( Container.DataItem, "ResourceUrl" ).ToString() )%></a>
                </div>
                <ul class="metadata">
                <li><span>Created:</span> <%# DataBinder.Eval( Container.DataItem, "ResourceCreated" )%></li>
                  <li><span>Publisher:</span> <a href="/Search.aspx?pub=<%# DataBinder.Eval( Container.DataItem, "Publisher" )%>" target="_blank"><%# DataBinder.Eval( Container.DataItem, "Publisher" )%></a></li>
                  <li><span>Collection:</span> <%# DataBinder.Eval( Container.DataItem, "LibrarySection" )%></li>
                  <li><span>Added:</span> <%# DataBinder.Eval( Container.DataItem, "DateAddedToCollection" )%></li>
                  <li><%# FormatRightsUrl( DataBinder.Eval( Container.DataItem, "Rights" ).ToString() )%></li>
                </ul>
              </div><!-- end result -->
			      </ItemTemplate>
		      </asp:TemplateField>
        </columns>
	      <pagersettings mode="NumericFirstLast" firstpagetext="First" lastpagetext="Last" pagebuttoncount="5" position="TopAndBottom" /> 
	    </wclGrid:gridview>

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
          CompactModePageCount="2"  GenerateToolTips="true" GenerateSmartShortCuts="true" MaxSmartShortCutCount="3"
          NormalModePageCount="5" 
          GenerateGoToSection="false" 
          GeneratePagerInfoSection="true" 
        />
      </div>
    </asp:Panel>
  </div><!-- end Right Column -->

  <a href="#" id="backToTopRight" title="back to top"></a>

</asp:Panel><!-- end pnlContainer -->

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.LRW.controls.LibrarySearch</asp:Literal>
<asp:Literal ID="txtLibraryId" runat="server" Visible="false">0</asp:Literal>
<asp:Literal ID="usingFullTextOption" runat="server" Visible="false">false</asp:Literal>
<asp:Literal ID="txtDisplayUrl" runat="server" Visible="false">/My/Default.aspx</asp:Literal>
<asp:Literal ID="showingLibMtceToAll" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="allowingLibCreate" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="usingFormattedCollectionTitle" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="areToolsActive" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="usingWSToPopulateFilters" runat="server" Visible="false">no</asp:Literal>
<asp:Literal ID="showingCollectionSubscriptions" runat="server" >no</asp:Literal>

<asp:Literal ID="recommendationsTemplate" runat="server" Visible="false"><br /><i>{0}</i></asp:Literal>
<asp:Literal ID="formattedTitleTemplate" runat="server" Visible="false"><a style="color:#000;  cursor:hand;" href="{0}" target="_blank" title="Website link opens in a new window">{1}</a></asp:Literal>

</asp:Panel>

<asp:Panel ID="Panel1" runat="server" Visible="false">

<asp:Literal ID="collectionsTemplate" runat="server" Visible="false"><li><a href="{2}?sId={0}">{1}</a></li></asp:Literal>
<asp:Literal ID="wholeLibraryTemplate" runat="server" Visible="false"><li><a href="{2}?Id={0}&s=y">{1}</a></li></asp:Literal>
<asp:Literal ID="allCollectionsLabel" runat="server" Visible="false">All Collections (whole library)</asp:Literal>
</asp:Panel>

<asp:Panel ID="Panel3" runat="server" Visible="false">
<asp:Literal ID="readMoreTemplate" runat="server" Visible="false">&nbsp;...&nbsp;<p class="readMore"><a href="javascript:void()">Show More &gt;&gt;</a></p></asp:Literal>
<asp:Literal ID="readMoreTemplateSystran" runat="server" Visible="false">&nbsp;...&nbsp;<p class="readMore"><a href="javascript:void()">Show More</a></p></asp:Literal>

</asp:Panel>
<asp:Panel ID="Panel4" runat="server" Visible="false">
<asp:Literal ID="formattedSourceTemplate" runat="server" Visible="false"><a class="textLink" href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>
<asp:Literal ID="formattedCCRightsUrl2" runat="server" Visible="false"><a class="textLink" href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>
<!-- code table Filters 
NOTE: libraries were moved to the content db. Views were created to match the table names from the IOER db
==> actually the code table search should go thru the content connnection!
-->
<asp:Literal ID="accessRightsFilter" runat="server" Visible="false">
[Codes.AccessRights].Id in (select distinct rv.AccessRightsId from [IsleContent].[dbo].[Library.Resource] lres
INNER JOIN [IsleContent].[dbo].[Library.Section] lsec ON lres.LibrarySectionId = lsec.Id 
inner join dbo.[Resource.Version] rv on lres.ResourceIntId = rv.ResourceIntId
where (lsec.LibraryId = {0}) )</asp:Literal>

<asp:Literal ID="audienceFilter" runat="server" Visible="false">
[Codes.AudienceType].Id in (select distinct res.AudienceId from [IsleContent].[dbo].[Library.Resource] lres
INNER JOIN [IsleContent].[dbo].[Library.Section] lsec ON lres.LibrarySectionId = lsec.Id 
inner join dbo.[Resource.IntendedAudience] res on lres.ResourceIntId = res.ResourceIntId
where (lsec.LibraryId = {0}) ) </asp:Literal>


<asp:Literal ID="gradeLevelFilter" runat="server" Visible="false">
[Codes.GradeLevel].Id in (select distinct rgl.GradeLevelId from [IsleContent].[dbo].[Library.Resource] lres
INNER JOIN [IsleContent].[dbo].[Library.Section] lsec ON lres.LibrarySectionId = lsec.Id 
inner join dbo.[Resource.GradeLevel] rgl on lres.ResourceIntId = rgl.ResourceIntId
where (lsec.LibraryId = {0}) ) </asp:Literal>

<asp:Literal ID="resTypeFilter" runat="server" Visible="false">
[Codes.ResourceType].Id in (select distinct res.ResourceTypeId from [IsleContent].[dbo].[Library.Resource] lres
INNER JOIN [IsleContent].[dbo].[Library.Section] lsec ON lres.LibrarySectionId = lsec.Id 
inner join dbo.[Resource.ResourceType] res on lres.ResourceIntId = res.ResourceIntId
where (lsec.LibraryId = {0}) ) </asp:Literal>


<asp:Literal ID="resFormatFilter" runat="server" Visible="false">
[Codes.ResourceFormat].Id in (select distinct res.CodeId from [IsleContent].[dbo].[Library.Resource] lres
INNER JOIN [IsleContent].[dbo].[Library.Section] lsec ON lres.LibrarySectionId = lsec.Id 
inner join dbo.[Resource.Format] res on lres.ResourceIntId = res.ResourceIntId
where (lsec.LibraryId = {0}) ) </asp:Literal>

<asp:Literal ID="resStandardFilter" runat="server" Visible="false">
lr.id in (select ResourceIntId from  (SELECT [ResourceIntId], count(*) As RecCount FROM [dbo].[Resource.Standard] group by [ResourceIntId] ) As ResStandards )</asp:Literal>


 <asp:Literal ID="keywordTemplate" runat="server" Visible="false"> (lr.Title like '{0}'  OR lr.[Description] like '{0}') </asp:Literal>

<asp:Literal ID="hasStandardsTemplate" runat="server" Visible="false"></asp:Literal>
<asp:Literal ID="txtLibraryImageTemplate" runat="server" Visible="false"><img src='{0}'  alt='libary icon {1}'/></asp:Literal>

<!-- width='{1}px'  height='{1}px'-->
</asp:Panel>


