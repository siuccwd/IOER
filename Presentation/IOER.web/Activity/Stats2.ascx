<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Stats2.ascx.cs" Inherits="IOER.Activity.Stats2" %>
<%@ Register TagPrefix="uc1" TagName="ActivityRenderer" Src="/Activity/ActivityRenderer.ascx" %>

<uc1:ActivityRenderer ID="activityRenderer" runat="server" />

<link rel="stylesheet" href="/styles/common2.css" type="text/css" />

<script type="text/javascript">
  var dates = <%=datesJSON %>;
  var accountData = <%=accountDataJSON %>;
  var accountSummary = <%=accountSummaryJSON %>;
  var organizationData = <%=organizationDataJSON %>;
  var libraryData = <%=libraryDataJSON %>;

  //Load data
  $(document).ready(function(){ 
    loadActivitySummary();
    loadActivityTable();
    loadOrganizations();
    loadLibraries();
    setupDatePickers();
  });

  function loadActivitySummary(){
    var summaryInfo = getActivity("divbars", accountSummary, { timespan: "", timescale: "day(s)", account_confirmation: "Account Confirmations", activity_date: null, auto_login: "Auto-Logins", login: "Total Logins", portal_sso: "Logins via Portal (SSO)", portal_sso_registration: "Registrations via Portal (SSO)", registration: "Total Registrations", sessions: "User Sessions" });
    $("#siteActivitySummary").html(summaryInfo);
  }

  function loadActivityTable() {
    var box = $("#siteActivityTable");
    box.append(getActivityTable("summary", "Date", accountData, { sessions: "User Sessions", auto_login: "Auto-Logins", login: "Total Logins", portal_sso: "Logins via Portal (SSO)", portal_sso_registration: "Registrations via Portal (SSO)", registration: "Total Registrations", account_confirmation: "Account Confirmations" }));
    var columnCount = $(".activity[data-name=summary] tr:first-child th").length;
    for(var i = 2; i <= columnCount; i++){
      var biggest = 0;
      var columnItems = $(".activity[data-name=summary] tr td:nth-child(" + i + ")");
      columnItems.each(function() {
        var thisVal = $(this).hasClass("noActivity") ? 0 : parseInt($(this).text());
        biggest = biggest < thisVal ? thisVal : biggest;
      });
      if(biggest > 0){
        columnItems.each(function() {
          if(!$(this).hasClass("noActivity")){
            var thisVal = parseInt($(this).text());
            var percent = 100 * (thisVal / biggest);
            $(this).css("background-image", "linear-gradient(90deg, rgba(74, 163, 148, 0.8) 0%, rgba(74, 163, 148, 0.5) " + percent + "%, #EFEFEF " + percent + "%)");
          }
        });
      }
    }
  }

  function loadOrganizations() {
    var box = $("#organizationList");
    /*for(i in organizationData){
      var current = organizationData[i];
      if(i == 0){ continue; }
      box.append(getActivity("totals", current, { timespan: "", timescale: "day(s)", administrator: "Administrators:", employee: "Employees:", student: "Students:", external: "External Members:", other: "Other Members:", line_total: "Total Users:"  }));
    }*/
    var cleanedOrgData = [];
    for(i in organizationData){
      if(i == 0){ continue; }
      cleanedOrgData.push(organizationData[i]);
    }
    box.append(getActivityTable("organizations", "Organization", cleanedOrgData, { administrator: "Administrators", employee: "Employees", student: "Students", external: "External Members", other: "Other Members", line_total: "Total Users" }));
    box.find(".noActivity").text("None");
  }

  function loadLibraries() {
    var template = $("#template_library").html();
    var list = $("#libraryList");
    for(i in libraryData){
      var current = libraryData[i];
      var libInfo = getActivity("totals", current.Activity, { timespan: "", timescale: "day(s)", library_views: "Library Views", resource_views: "Resource Views" });
      var colInfo = "";
      for(j in current.ChildrenActivity){
        colInfo += getActivity("totals", current.ChildrenActivity[j], { timespan: "", timescale: "day(s)", collection_views: "Collection Views" });
      }
      if(current.ChildrenActivity.length == 0){
        colInfo = "<p>No recent activity.</p>";
      }
      list.append(
        template.replace(/{id}/g, current.Activity.Id).replace(/{title}/g, current.Activity.Title).replace(/{libraryData}/g, libInfo).replace(/{collectionData}/g, colInfo)
      );
    }
  }

  function submit() {
  	$("form").removeAttr("onsubmit");
  	$("form")[0].submit();
  }
</script>

<style type="text/css">
  /* General */
  .majorItem { margin-bottom: 25px; }
  .majorItem h3 a { font-size: 24px; }
  .majorItem h4 { font-size: 20px; }

  /* Site Activity */
  #siteActivitySummary .activityTitle { display: none; }
  #siteActivitySummary .divbars .activityBarZone { padding: 0 0.001%; }
  #siteActivitySummary .activityItem { margin-bottom: 10px; background-color: #F5F5F5; border-radius: 5px; padding: 5px; }
  #siteActivitySummary .activityItem .title, #siteActivitySummary .activityItem .activityData { display: inline-block; vertical-align: top; }
  #siteActivitySummary .activityItem .title { width: 100px; }
  #siteActivitySummary .activityItem .activityData { width: calc(100% - 105px); }
  .noActivity { font-style: italic; color: #999; }
  #siteActivityTable .noActivity { background-color: #EEE; }
  #siteActivityTable .activity { width: 100%; white-space: nowrap; }
  #siteActivityTable .header { white-space: nowrap; font-size: 18px; }
  #siteActivityTable th, #siteActivityTable td { width: calc(100% / 8); text-align: center; white-space: normal; }

  /* Organizations */
  #organizationList th, #organizationList td { width: calc(100% / 7); text-align: center; padding: 2px; }
  #organizationList tr:nth-child(2n + 1) { background-color: #EEE; }

  /* Libraries */
  .libraryData .activityTitle { display: none; }
  .libraryData .noActivity { display: inline; }

</style>

<div id="content">

  <h1 class="isleH1">IOER Activity (<%=(activityRenderer.startDate.ToShortDateString() + " - " + activityRenderer.endDate.ToShortDateString()) %>)</h1>

	<div class="grayBox" id="dateSelector">
		Show activity from <input title="FromDate" type="text" class="date startDate" id="txtStartDate" runat="server" /> to <input  title="ToDate" type="text" class="date endDate" id="txtEndDate" runat="server" /> <input type="button" class="isleButton bgBlue" value="Show" onclick="submit();" />
	</div>
  <h2 class="isleH2">Sitewide Summary Graphs</h2>
  <div id="siteActivitySummary"></div>
  <h2 class="isleH2">Sitewide Summary Table (Only Days With Activity)</h2>
  <div id="siteActivityTable"></div>

  <h2 class="isleH2">Organization User Totals</h2>
  <div id="organizationList"></div>

  <h2 class="isleH2">Libraries</h2>
  <div id="libraryList"></div>

</div>

<div id="templates" style="display:none;">

  <script type="text/template" id="template_library">
    <div class="majorItem library grayBox" data-libraryID="{id}">
      <h3><a href="/Library/{id}">{title}</a></h3>
      <h4>Library Information (<%=(activityRenderer.startDate.ToShortDateString() + " - " + activityRenderer.endDate.ToShortDateString()) %>):</h4>
      <div class="libraryData">{libraryData}</div>
      <h4>Collections (<%=(activityRenderer.startDate.ToShortDateString() + " - " + activityRenderer.endDate.ToShortDateString()) %>):</h4>
      <div class="collectionData">{collectionData}</div>
    </div>
  </script>

</div>
