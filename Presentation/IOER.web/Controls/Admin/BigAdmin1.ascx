<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BigAdmin1.ascx.cs" Inherits="ILPathways.Controls.Admin.BigAdmin1" %>

<!-- IOER scripts -->
<script type="text/javascript" src="/scripts/widgets/postmessagereceiver.js"></script>
<script type="text/javascript" src="/scripts/jscommon.js"></script>
<link rel="stylesheet" href="/styles/common2.css" type="text/css" />

<div id="MainEditor" runat="server">
  <script type="text/javascript">
    //From server
    var manageType = "<%=ActiveList.objectType %>";
    var manageID = <%=manageID %>;

    //Page variables
    var service = "AdminService1";
    var users = [];
  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Initialization   ---   ---   --- */
    $(document).ready(function () {
      //Setup member finder
      setupMemberFinder();
      //Setup automatic user searches
      setupAutoUserSearches();
    });

    function setupMemberFinder() {
      //Bind visual effects
      $(".memberFinder").on("change", "input[type=checkbox]", function () {
        var cbx = $(this);
        var item = cbx.parentsUntil(".userResult").parent();
        if (cbx.prop("checked")) {
          item.addClass("selected");
        }
        else {
          item.removeClass("selected");
        }
      });

      //Do initial search
      
    }

    function setupAutoUserSearches(){
      $(window).on("showSubTab_currentMembers", function() { loadUsers("ListUsers", true); });
      $(window).on("showSubTab_membershipRequests", function() { loadUsers("ListPendingMembers", true); });
      $(window).on("showSubTab_inviteMembers", function() { loadUsers("ListPendingInvitations", false); });
    }
  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Page Functions   ---   ---   --- */
    //Show a tab
    function showTab(id, button) {
      $(".tab").hide().filter("[data-tabID=" + id + "]").show();
      $("#tabButtons input").removeClass("selected").filter(button).addClass("selected");
      if(id == "members"){
        try { $(".subTabButtons input.selected").trigger("click"); } catch(e) {}
      }
    }
    //Show a subtab
    function showSubTab(id, button) {
      $(".subTab").hide().filter("[data-tabID*=" + id + "]").show();
      $(".subTabButtons input").removeClass("selected").filter(button).addClass("selected");
      $(window).trigger("showSubTab_" + id);
    }
    //Load users into the list
    function loadUsers(method, showCBX){
      $(".memberList").html("<p class='grayMessage loadingImage'><img src='/images/wait.gif' /></p>");
      doAjax("AdminService1",method, { type: manageType, manageID: manageID }, success_listUsers, null, showCBX);
    }
    //Load users into the list after updating
    function updateUsers(method, data, button, showCBX){
      $(".memberList").prepend("<p class='grayMessage loadingImage'><img src='/images/wait.gif' /></p>");
      doAjax("AdminService1", method, data, success_updateUsers, button, showCBX);
    }
    //Save changes to existing members
    function members_updateMemberships(button){
      var data = getBasicMemberData();
      if(data.memberIDs.length == 0){ return; }
      updateUsers("Members_UpdateMemberships", data, $("#btn_members_updateMemberships, #btn_members_revokeMemberships"), true);
    }
    //Revoke memberships
    function members_revokeMemberships(button){
      var data = getBasicMemberData();
      if(data.memberIDs.length == 0){ return; }
      updateUsers("Members_RevokeMemberships", data, $("#btn_members_updateMemberships, #btn_members_revokeMemberships"), true);
    }
    //Approve pending memberships
    function members_approveMemberships(button){
      var data = getBasicMemberData();
      if(data.memberIDs.length == 0){ return; }
      updateUsers("Members_ApproveMemberships", data, $("#btn_members_approveMemberships, #btn_members_denyMemberships"), true);
    }
    //Deny pending memberships
    function members_denyMemberships(button){
      var data = getBasicMemberData();
      if(data.memberIDs.length == 0){ return; }
      updateUsers("Members_DenyMemberships", data, $("#btn_members_approveMemberships, #btn_members_denyMemberships"), true);
    }
    //Invite new members
    function members_invite(button){
      var data = getBasicMemberData();
      doAjax("AdminService1", "Members_Invite", data, success_inviteUsers, $(button), false);
    }
    //Get IDs of currently selected members
    function getSelectedMemberIDs() {
      var ids = [];
      $(".memberList .userResult").each(function() {
        var box = $(this).find("input");
        if(box.prop("checked")){
          ids.push(parseInt($(this).find("input").attr("data-id")));
        }
      });
      return ids;
    }
    //Get data
    function getBasicMemberData() {
      return {
        type: manageType,
        manageID: manageID,
        memberIDs: getSelectedMemberIDs(),
        privilegeID: parseInt($(".ddlPrivilege option:selected").attr("value")),
        emails: $(".txtEmailAddresses").val().split("\n"),
        customMessage: $(".txtCustomMessage").val()
      };
    }
  </script>
  <script type="text/javascript">
    /* ---   ---   ---   AJAX Functions   ---   ---   --- */
    function doAjax(service, method, data, success, button, extra) {
      if (button) { 
        button.each(function() {
          var item = $(this);
          item.prop("disabled", true).attr("originalText", item.attr("value")).attr("value","..."); 
        });
      }
      $.ajax({
        url: "/Services/" + service + ".asmx/" + method,
        contentType: "application/json; charset=utf-8",
        type: "POST",
        data: JSON.stringify(data)
      }).always(function (msg) {
        if (button) { 
          button.each(function() {
            var item = $(this);
            item.attr("value", item.attr("originalText")).prop("disabled", false); 
          });
        }
        success((msg.d ? msg.d : msg), extra);
      });
    }
  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Success Functions   ---   ---   --- */
    //List users
    function success_listUsers(data, info){
      if(data.valid){
        users = data.data;
        renderUsers(info);
      }
      else {
        users = [];
        renderUsers(info);
        alert(data.status);
      }
    }

    //Handle invitation results
    function success_inviteUsers(data){
      if(data.valid){
        users = data.data;
        renderUsers(false);
      }
      else {
        alert(data.status);
      }
      renderInvitees(data.extra);
      //console.log(data);
    }

    //Handle other update results
    function success_updateUsers(data){
      if(data.valid){
        renderUserUpdates(data.data);
      }
      else {
        alert(data.status);
      }
    }

  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Rendering Functions   ---   ---   --- */
    //Render users
    function renderUsers(showCBX){
      var template = $("#template_userResult").html();
      var box = $(".memberList");
      box.html("");
      if(users.length == 0){
        box.html("<p class='grayMessage'>No users found.</p>");
      }
      for(i in users){
        box.append(jsCommon.fillTemplate(template, users[i]));
      }
      if(!showCBX){
        box.find("input").remove();
      }
    }

    //Render invitations that don't have an existing user
    function renderInvitees(data){
      var template = $("#template_invitee").html();
      var box = $(".memberList");
      if(users.length == 0){ box.html(""); }
      box.find(".invitee").remove();
      for(i in data){
        data[i].result = data[i].valid ? "Successful" : "Failed";
        box.append(jsCommon.fillTemplate(template, data[i]));
      }
    }

    //Alter existing HTML to reflect updates
    function renderUserUpdates(data){
      var box = $(".memberList");
      box.find(".loadingImage").remove();
      box.find(".message").attr("class", "message").html("");
      console.log(data);
      for(i in data){
        var item = data[i];
        var target = box.find(".userResult[data-id=" + data[i].id + "]");
        if(target.length > 0){
          if(data[i].extra.title != "") { 
            target.find(".privilege").html(data[i].extra.title); 
          }
          if(data[i].extra.error != ""){
            console.log(data[i].extra.error);
          }
          target.find(".message").attr("class", "message " + data[i].valid).html(data[i].text);
        }
      }
    }
  </script>
  <style type="text/css">
    /* Big stuff */
    #content { min-height: 550px; min-width: 320px; }
    #content:after { content: " "; display: block; clear: both; }
    #leftColumn { width: 300px; float: left; min-height: 500px; }
    #rightColumn { margin-left: 305px; }
    .grayMessage { padding: 10px; font-style: italic; opacity: 0.7; text-align: center; }

    /* Left column */
    #leftColumn a { display: block; padding: 5px 10px; transition: background-color 0.1s, color 0.1s; margin: 0 -10px; }
    #leftColumn a.selected { background-color: #9984BD; color: #FFF; }
    #leftColumn a:hover, #leftColumn a:focus { background-color: #FF6A00; color: #FFF; }
    #leftColumn a.overview { font-weight: bold; }
    
    /* Management item lists */
    .manageItem { position: relative; min-height: 90px; }
    .manageItem:after { content: " "; display: block; clear: right; }
    .manageItem .metadata { float: right; width: 225px; }
    .manageItem .metadata * { font-size: 14px; }
    .manageItem .metadata tr td:first-child { padding-right: 5px; }
    .manageItem .avatar { position: absolute; right: 5px; top: 5px; height: 75px; width: 100px; }
    .manageItem .itemContent { padding-right: 105px; }
    .manageItem .header { padding-right: 105px; }
    .manageItem .header a { font-size: inherit; }
    .manageItem .header a.viewLink { float: right; margin: 0 10px; }
    .manageItem .header a[href=''] { display: none; }

    /* Management item objects */
    .activeItemContent .metadata { float: right; width: 250px; margin: 0 5px 5px 5px; }
    .activeItemContent .metadata * { font-size: 14px; }
    .activeItemContent .metadata tr td:first-child { padding-right: 5px; }
    .activeItemContent .details { position: relative; min-height: 135px; padding-right: 200px; }
    .activeItemContent .avatar { width: 200px; height: 150px; position: absolute; right: 0; top: -25px; background-position: center center; background-size: cover; background-repeat: no-repeat; }
    .header.withPadding { padding-right: 215px; }
    .activeItemContent #tabButtons { clear: right; }
    .activeItemContent #tabButtons input { width: auto; display: inline-block; }
    .activeItemContent input.selected { background-color: #9984BD; }
    .activeItemContent .buttons { margin-top: 15px; }
    .activeItemContent .buttons:after { content: " "; display: block; clear: right; }
    .activeItemContent .buttons input { width: auto; display: inline-block; }
    .activeItemContent .buttons .right { float: right; }
    .memberFinder, .memberSettings, .timelineFinder, .timelineEditor { display: inline-block; vertical-align: top; width: 50%; }
    .memberFinder, .timelineFinder { padding-right: 10px; }
    .memberFinder input[type=text] { width: 100%; }
    .userResult { position: relative; display: block; min-height: 70px; transition: border 0.1s; }
    .userResult .userResultContent { padding-left: 70px; }
    .userResult .header { padding-left: 70px; transition: background-color 0.1s, color 0.1s; }
    .userResult .avatar { position: absolute; top: 5px; left: 5px; height: 60px; width: 60px;  }
    .userResult input[type=checkbox] { float: right; margin: 2px; }
    .userResult .date { position: absolute; top: 20px; right: 0; font-size: 14px; }
    .userResult:hover, .userResult:focus { cursor: pointer; border-color: #FF6A00; }
    .userResult.selected .header { background-color: #9984BD; color: #FFF; }
    .userResult .message:empty { display: none; }
    .userResult .message.true { color: #4AA394; }
    .userResult .message.false { color: #B03D25; }
    .timelinePost { position: relative; }
    .timelinePost .header { padding-right: 50px; }
    .timelinePost .timelinePostMetadata { font-size: 14px; font-style: italic; margin: 5px 0 -3px 0; text-align: right; opacity: 0.8; }
    .timelinePost .btnEditTimelinePost { position: absolute; top: 0; right: 2px; width: auto; padding: 0 5px; }
    .invitee[data-valid=true] .header { color: #4AA394; }
    .invitee[data-valid=false] .header { color: #B03D25; }

    /* Boxes */
    .grayBox .header.withMids { margin-bottom: -5px; }
    .grayBox .mid.close { margin-bottom: 0; }
    .lightBox { border: 1px solid #CCC; margin: 5px 0; padding: 5px; border-radius: 5px; }
    .lightBox .header { font-size: 16px; background-color: #DDD; color: #000; font-weight: normal; margin: -5px -5px 5px -5px; }

    /* Form stuff */
    label { display: block; padding: 5px; margin-bottom: 5px; }
    label input[type=text], label textarea, label select { width: 100%; }
    label textarea { resize: vertical; min-height: 4em; max-height: 12em; height: 5em; }
    label.cbx { border-radius: 5px; transition: background-color .1s; }
    label.cbx:hover, label.cbx:focus { background-color: #DDD; cursor: pointer; }

    /* Miscellaneous */
    div[class*='at4-'] { display: none; }
    #content { padding-left: 0; }
    .bigLink { display: block; text-align: center; }
    .avatar { border: 1px solid #CCC; border-radius: 5px; background-size: contain; background-color: #DDD; }
    .bigMessage { margin: 10px; padding: 10px; text-align: center; font-weight: bold; border-color: #4AA394; }
    .bigMessage h1 { font-size: 30px; }

    /* Media Queries */
    @media (max-width: 1000px) {
      .activeItemContent .metadata, .manageItem .metadata { float: none; }
      .activeItemContent .details { padding-right: 205px; }
      .memberFinder, .memberSettings, .timelineFinder, .timelineEditor { display: block; width: 100%; }
      .memberFinder, .timelineFinder { padding-right: 0; }
    }
    @media (max-width: 800px) {
      #leftColumn { width: auto; float: none; }
      #rightColumn { margin-left: 0; margin-top: 10px; }
      #leftColumn, #rightColumn { display: block; }
    }
    @media (max-width: 500px) {
      .activeItemContent .avatar { width: 100%; width: 300px; height: 225px; position: static; display: block; margin: 5px auto; }
      .activeItemContent .details, .header.withPadding { padding-right: 5px; }
      .manageItem .avatar { width: 75px; height: 56.25px; }
    }
  </style>

  <div id="content">
    
    <h1 class="isleH1">IOER Administration</h1>
    <% if(isOrganizationMode){ %>
      <div class="bigMessage grayBox">
        <p>You are currently administering on behalf of:</p>
        <h1><%=ActiveOrganization.title %></h1>
        <a class="bigLink textLink" href="?manage=organization&id=0">Return to my IOER administration</a>
      </div>
    <% } %>
    <div id="leftColumn" class="mainColumn grayBox">
      <h3 class="header withMids"><%=(isOrganizationMode ? "Organization Administration" : "My IOER Administration") %></h3>
      <% if(MyOrganizations.Count() > 0){ %>
        <% if(isOrganizationMode) { %>
        <h3 class="mid close">Manage Organization</h3>
        <a href="<%=ActiveOrganization.manageLink %>" class="overview <%=(manageID == ActiveOrganization.id && manage == MyOrganizations.objectType ? "selected" : "" ) %>"><%=ActiveOrganization.title %></a>
        <% } else { %>
        <h3 class="mid close">My Organizations</h3>
        <a href="?manage=organization&id=0" class="overview <%=(manageID == 0 && manage == MyOrganizations.objectType ? "selected" : "" ) %>">Organizations Overview</a>
          <% foreach(var item in MyOrganizations) { %>
          <a href="<%=item.manageLink %>" <%=(manageID == item.id && manage == MyOrganizations.objectType ? "class=\"selected\"" : "" ) %>><%=item.title %></a>
          <% } %>
        <% } %>
      <% } %>
      <% if(MyLibraries.Count() > 0){ %>
      <h3 class="mid close">My Libraries</h3>
      <a href="?manage=library&id=0<%=orgURL %>" class="overview <%=(manageID == 0 && manage == MyLibraries.objectType ? "selected" : "" ) %>">Libraries Overview</a>
        <% foreach(var item in MyLibraries) { %>
        <a href="<%=item.manageLink %>" <%=(manageID == item.id && manage == MyLibraries.objectType ? "class=\"selected\"" : "" ) %>><%=item.title %></a>
        <% } %>
      <% } %>
      <% if(MyCommunities.Count() > 0){ %>
      <h3 class="mid close">My Communities</h3>
      <a href="?manage=community&id=0<%=orgURL %>" class="overview <%=(manageID == 0 && manage == MyCommunities.objectType ? "selected" : "" ) %>">Communities Overview</a>
        <% foreach(var item in MyCommunities) { %>
        <a href="<%=item.manageLink %>" <%=(manageID == item.id && manage == MyCommunities.objectType ? "class=\"selected\"" : "" ) %>><%=item.title %></a>
        <% } %>
      <% } %>
      <% if(MyLearningLists.Count() > 0){ %>
      <h3 class="mid close">My Learning Lists</h3>
      <a href="?manage=learninglist&id=0<%=orgURL %>" class="overview <%=(manageID == 0 && manage == MyLearningLists.objectType ? "selected" : "" ) %>">Learning Lists Overview</a>
        <% foreach(var item in MyLearningLists) { %>
        <a href="<%=item.manageLink %>" <%=(manageID == item.id && manage == MyLearningLists.objectType ? "class=\"selected\"" : "" ) %>><%=item.title %></a>
        <% } %>
      <% } %>
    </div><!-- /leftColumn --><!-- Prevent auto-space
    --><div id="rightColumn" class="mainColumn grayBox">
      <h3 class="header <%=(ActiveObject != null ? "withPadding" : "") %>" ><%=(ActiveObject == null ? ( ActiveList == null ? "My IOER Administration" : ActiveList.title ) : ActiveObject.title) %></h3>
      <% if(ActiveList == null){ %>
        <p class="grayMessage">Select an item to manage.</p>
      <% } else { %>
        <% if(ActiveObject == null){ //Show overview %>
          <% foreach(var item in ActiveList) { %>
          <div class="lightBox manageItem" id="<%=item.id %>">
            <div class="header"><a href="<%=item.manageLink %>"><%=item.title %></a> <a class="viewLink" href="<%=item.url %>" target="_blank">View</a></div>
            <div class="itemContent">
              <table class="metadata lightBox">
                <tr><td>Privileges: </td><td><%=item.privilege %></td></tr>
                <% foreach(var data in item.metadata) { %>
                <tr><td><%=data.key %>:</td><td><%=data.value %></td></tr>
                <% } %>
              </table>
              <div class="description"><%=item.description %></div>
            </div>
            <div class="avatar" style="background-image:url('<%=item.imageURL %>')"></div>
          </div>
          <% } %>
        <% } else { //Show active item %>
          <div class="activeItemContent" data-type="<%=ActiveList.objectType %>">
            <div class="details">
              <div class="avatar" style="background-image:url('<%=ActiveObject.imageURL %>')"></div>
              <table class="metadata lightBox">
                <tr><td>Privileges: </td><td><%=ActiveObject.privilege %></td></tr>
                <% foreach(var data in ActiveObject.metadata) { %>
                <tr><td><%=data.key %>:</td><td><%=data.value %></td></tr>
                <% } %>
              </table>
              <p class="description"><%=ActiveObject.description %></p>
            </div>
            <div id="tabButtons">
              <input type="button" class="isleButton bgBlue selected" value="Details" onclick="showTab('details', this);" />
              <input type="button" class="isleButton bgBlue" value="Members" onclick="showTab('members', this);" />
              <% if(ActiveList.objectType == "organization" && !isOrganizationMode) { %>
              <input type="button" class="isleButton bgBlue" value="Administration" onclick="showTab('contents', this);" />
              <% } %>
              <% if(ActiveList.objectType == "learninglist") { %>
              <input type="button" class="isleButton bgBlue" value="Update Timeline" onclick="showTab('timeline', this);" />
              <% } %>
            </div>
            <div id="tabs">
              <div class="tab" data-tabID="details">
                <h3 class="mid">Details</h3>
                <% switch(ActiveList.objectType) { %>
                <% case "organization": %>
                <label>
                  <div>Title <span class="grayMessage">(Please do not modify frequently to avoid confusing your members!)</span></div>
                  <input type="text" id="organization_txtTitle" class="organization_txtTitle" />
                </label>
                <label>
                  <div>Description</div>
                  <textarea id="organization_txtDescription" class="organization_txtDecription"></textarea>
                </label>
                <label class="cbx">
                  <input type="checkbox" id="organization_cbxPubliclyVisible" class="organization_cbxPubliclyVisible" />
                  <span>This organization is publicly visible</span>
                </label>
                <div class="buttons">
                  <input type="button" class="isleButton bgRed" value="Request organization deletion..." />
                  <input type="button" class="isleButton bgBlue right" value="Save Changes" />
                </div>
              <% break; %>
              <% case "community": %>
                <label>
                  <div>Title <span class="grayMessage">(Please do not modify frequently to avoid confusing your members!)</span></div>
                  <input type="text" id="community_txtTitle" class="community_txtTitle" />
                </label>
                <label>
                  <div>Description</div>
                  <textarea id="community_txtDescription" class="community_txtDecription"></textarea>
                </label>
                <label class="cbx">
                  <input type="checkbox" id="community_cbxPubliclyVisible" class="community_cbxPubliclyVisible" />
                  <span>This community is publicly visible</span>
                </label>
                <div class="buttons">
                  <input type="button" class="isleButton bgRed" value="Request community deletion..." />
                  <input type="button" class="isleButton bgBlue right" value="Save Changes" />
                </div>
              <% break; %>
              <% case "library": %>
                <a class="bigLink textLink" href="#">Please visit this Library to change its details.</a>
              <% break; %>
              <% case "learninglist": %>
                <a class="bigLink textLink" href="#">Please use the Learning List editor to change its details.</a>
              <% break; %>
              <% case "resource": //This one shouldn't happen %>
              <% break; %>
              <% default: break; %>
              <% } %>
              </div>
              <div class="tab" data-tabID="members" style="display: none;">
                <h3 class="mid">Members</h3>
                <div class="memberManager">
                  <div class="subTabButtons buttons">
                    <input type="button" class="isleButton bgBlue selected" value="Current Members" onclick="showSubTab('currentMembers', this);" />
                    <% if(ActiveList.objectType != "learninglist") { %>
                    <input type="button" class="isleButton bgBlue" value="Membership Requests" onclick="showSubTab('membershipRequests', this);" />
                    <% } %>
                    <input type="button" class="isleButton bgBlue" value="Invite Members" onclick="showSubTab('inviteMembers', this);" />
                  </div>
                  <div class="memberFinder">
                    <h3 class="mid subTab" data-tabID="currentMembers">Current Members</h3>
                    <h3 class="mid subTab" data-tabID="membershipRequests" style="display: none;">Approve Pending Members</h3>
                    <h3 class="mid subTab" data-tabID="inviteMembers" style="display: none;">Current Pending Invitations</h3>
                    <input type="text" class="txtMemberFinder" placeholder="Find..." />
                    <div class="memberList">
                      <label class="userResult lightBox">
                        <div class="header">Member Name <input type="checkbox" data-id="99" /></div>
                        <div class="avatar" style="background-image:url('');"></div>
                        <div class="userResultContent">
                          <div class="privilege">Contributor</div>
                          <div class="role">Teacher</div>
                          <div class="date grayMessage">Joined: 99/99/9999</div>
                        </div>
                      </label>
                      <label class="userResult lightBox">
                        <div class="header">Member Name <input type="checkbox" data-id="99" /></div>
                        <div class="avatar" style="background-image:url('');"></div>
                        <div class="userResultContent">
                          <div class="privilege">Contributor</div>
                          <div class="role">Teacher</div>
                          <div class="date grayMessage">Joined: 99/99/9999</div>
                        </div>
                      </label>
                      <label class="userResult lightBox">
                        <div class="header">Member Name <input type="checkbox" data-id="99" /></div>
                        <div class="avatar" style="background-image:url('');"></div>
                        <div class="userResultContent">
                          <div class="privilege">Contributor</div>
                          <div class="role">Teacher</div>
                          <div class="date grayMessage">Joined: 99/99/9999</div>
                        </div>
                      </label>
                    </div>
                  </div><!--
                  --><div class="memberSettings">
                    <h3 class="mid subTab" data-tabID="currentMembers">Manage</h3>
                    <h3 class="mid subTab" data-tabID="membershipRequests" style="display: none;">Approve As</h3>
                    <h3 class="mid subTab" data-tabID="inviteMembers" style="display: none;">Invite New Members</h3>
                    <div class="lightBox subTab" data-tabID="currentMembers">
                      <div class="header">To manage users:</div>
                      <ol>
                        <li>Find existing members.</li>
                        <li>Select one or more members to take action upon.</li>
                        <li>Select a privilege or click the revoke membership button. This will be applied to all selected members.</li>
                        <li>Click "Save Changes".</li>
                      </ol>
                    </div>
                    <div class="lightBox subTab" data-tabID="inviteMembers" style="display:none;">
                      <div class="header">To invite users:</div>
                      <ol>
                        <li>Enter their email addresses (one per line) in the box below.</li>
                        <li>Select a privilege. This will be applied to every email you entered.</li>
                        <li>Optionally, enter a custom message to send along with the email. This helps the recipient know the message is legitimate.</li>
                        <li>Double-check that everything is entered correctly, then click "Send Invitations".</li>
                      </ol>
                    </div>
                    <label class="subTab" data-tabID="inviteMembers" style="display: none;">
                      <div>Email Addresses <span class="grayMessage">(one per line)</span></div>
                      <textarea class="txtEmailAddresses" placeholder="Enter one email address per line."></textarea>
                    </label>
                    <label>
                      <div>Set Privileges</div>
                      <asp:DropDownList ID="ddlPrivilege" CssClass="ddlPrivilege" runat="server"></asp:DropDownList>
                    </label>
                    <%-- if(ActiveList.objectType == "organization") { %>
                    <label>
                      <div>Set Role</div>
                      <asp:DropDownList ID="ddlRole" CssClass="ddlRole" runat="server"></asp:DropDownList>
                    </label>
                    <% } --%>
                    <label class="subTab" data-tabID="membershipRequests,inviteMembers" style="display: none;">
                      <div>Custom Message</div>
                      <textarea class="txtCustomMessage" placeholder="Enter a message that will be sent to each recipient as part of the invitation."></textarea>
                    </label>
                    <div class="buttons">
                      <input type="button" class="isleButton bgBlue subTab right" data-tabID="currentMembers" value="Save Changes" id="btn_members_updateMemberships" onclick="members_updateMemberships(this);" />
                      <input type="button" class="isleButton bgRed subTab" data-tabID="currentMembers" value="Revoke Membership..." id="btn_members_revokeMemberships" onclick="members_revokeMemberships(this);" />
                      <input type="button" class="isleButton bgBlue subTab right" data-tabID="membershipRequests" value="Approve..." id="btn_members_approveMemberships" style="display: none;" onclick="members_approveMemberships(this);" />
                      <input type="button" class="isleButton bgRed subTab" data-tabID="membershipRequests" value="Deny..." id="btn_members_denyMemberships" style="display: none;" onclick="members_denyMemberships(this);" />
                      <input type="button" class="isleButton bgBlue subTab right" data-tabID="inviteMembers" value="Send Invitations..." id="btn_members_invite" style="display: none;" onclick="members_invite(this);" />
                    </div>
                  </div>
                </div>
              </div>
              <% if(ActiveList.objectType == "organization"){ %>
              <div class="tab" data-tabID="contents" style="display: none;">
                <h3 class="mid">Administration</h3>
                <p class="grayMessage">An organization has many of the same management features as a user. Click below to administrate for this organization.</p>
                <a class="bigLink textLink" href="?manage=organization&id=<%=ActiveObject.id %>&mode=organization&orgID=<%=ActiveObject.id %>">Click here to administrate for <%=ActiveObject.title %></a>
              </div>
              <% } %>
              <% if(ActiveList.objectType == "learninglist") { %>
              <div class="tab" data-tabID="timeline" style="display:none;">
                <h3 class="mid">Update Timeline</h3>
                <p>Use this section to update existing timeline entries, and create new ones.</p>
                <p>Note that these entries are publicly available immediately upon being saved, so please double check your work!</p>
                <div class="timelineFinder">
                  <h3 class="mid subTab">Current Timeline Posts</h3>
                  <div class="timelinePostList">
                    <div class="timelinePost lightBox">
                      <div class="header">Timeline post #3 item title</div>
                      <div class="timelinePostContent">
                        Timeline post content here in this box. This item shows existing timeline content.
                      </div>
                      <div class="timelinePostMetadata">
                        Posted by User Name on 99/99/9999
                      </div>
                      <input type="button" class="isleButton bgBlue btnEditTimelinePost" value="Edit" onclick="editTimelinePost();" />
                    </div>
                    <div class="timelinePost lightBox">
                      <div class="header">Timeline post #2 item title</div>
                      <div class="timelinePostContent">
                        Timeline post content here in this box. This item shows existing timeline content.
                      </div>
                      <div class="timelinePostMetadata">
                        Posted by User Name on 99/99/9999
                      </div>
                      <input type="button" class="isleButton bgBlue btnEditTimelinePost" value="Edit" onclick="editTimelinePost();" />
                    </div>
                    <div class="timelinePost lightBox">
                      <div class="header">Timeline post #1 item title</div>
                      <div class="timelinePostContent">
                        Timeline post content here in this box. This item shows existing timeline content.
                      </div>
                      <div class="timelinePostMetadata">
                        Posted by User Name on 99/99/9999
                      </div>
                      <input type="button" class="isleButton bgBlue btnEditTimelinePost" value="Edit" onclick="editTimelinePost();" />
                    </div>
                  </div>
                </div><!--
                --><div class="timelineEditor">
                  <h3 class="mid subTab">Update</h3>
                  <label>
                    <div>Title</div>
                    <input type="text" id="txtTimelineTitle" class="txtTimelineTitle" placeholder="Enter a Title" />
                  </label>
                  <label>
                    <div>Description</div>
                    <textarea id="txtTimelineDescription" class="txtTimelineDescription" placeholder="Enter a Description"></textarea>
                  </label>
                  <div class="buttons">
                    <input type="button" class="isleButton bgBlue right" value="Post to Timeline" />
                    <input type="button" class="isleButton bgBlue right btnTimelineSaveChanges" value="Save Changes" style="display:none" />
                    <input type="button" class="isleButton bgRed btnTimelineCancelChanges" value="Cancel Changes" style="display:none" />
                  </div>
                </div>
              </div>
              <% } %>
            </div><!-- /tabs -->
          </div>
        <% } %>
      <% } %>
    </div><!-- /rightColumn -->

    <div id="templates" style="display:none;">
      <div id="template_userResult">
        <label class="userResult lightBox" data-id="{id}">
          <div class="header">{name} <input type="checkbox" data-id="{id}" /></div>
          <div class="avatar" style="background-image:url('{imageURL}');"></div>
          <div class="userResultContent">
            <div class="privilege">{privileges}</div>
            <div class="message"></div>
            <div class="date grayMessage">{date}</div>
          </div>
        </label>
      </div>
      <div id="template_invitee">
        <div class="invitee lightBox" data-valid="{valid}">
          <div class="header">Email Invitation {result}</div>
          {text}
        </div>
      </div>
    </div>
  </div><!-- /content -->
</div>
<div id="MainError" runat="server" visible="false" style="padding: 50px; text-align: center;">

</div>