<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.ascx.cs" Inherits="IOER.Account.controls.Dashboard" %>

<div id="errorMessage" runat="server" visible="false">
  <p style="margin: 20px auto; display:block; text-align: center;">Please login to access your dashboard.</p>
</div>
<div id="entireDashboard" runat="server">
<script type="text/javascript">
    //From Server
    <%=varFollowingUserId %>
    <%=varFollowedByUserId%>
  
</script>
<script type="text/javascript">

    function toggleFollowing() {
        doAjax("ToggleFollowing", { followingUserId: followingUserId, followedByUserId: followedByUserId }, successToggleFollowing, null);
    }

    function loadAll(target) {
      doAjax("LoadAll", { userGUID: userGUID, target: target }, successLoadAll, null);
    }
</script>
<script>
    //AJAX stuff
    function doAjax(method, data, success, button, replyStatusID) {
        disableButton(button);
        $.ajax({
            url: "/Services/DashboardService.asmx/" + method,
            async: true,
            success: function (msg) {
                try {
                    success($.parseJSON(msg.d), replyStatusID);
                }
                catch (e) {
                    success(msg.d, replyStatusID);
                }
                enableButton(button);
            },
            type: "POST",
            data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json; charset=utf-8"
        });
    }

    function disableButton(button) {
        if (button == null) { return; }
        button.attr("originalVal", button.attr("value"));
        button.attr("value", "...");
        button.attr("disabled", "disabled");
    }
    function enableButton(button) {
        if (button == null) { return; }
        button.attr("value", button.attr("originalVal"));
        button.removeAttr("disabled");
    }

    function successToggleFollowing(data) {
        if (data.isValid) {
            
            if (data.data) {
                alert("You are now following this Person.");
                $(".btnFollow").attr("value", "Unfollow");
            }
            else {
                alert("You are no longer following this Person.");
                $(".btnFollow").attr("value", "Follow");
            }
        }
        else {
            alert(data.status);
        }
    }

    function loadAll(data) {
        console.log(data);
      if (data.isValid) {
      }
      else {
        alert(data.status);
      }
    }
</script>

<style type="text/css">
  #content { min-height: 500px; }
  #columns { white-space: nowrap; position: relative; }
  #columns .column { white-space: normal; display: inline-block; vertical-align: top; margin-right: -4px; padding: 5px; }

  #columns #leftColumn { width: 200px; position: absolute; top: 0; left: 0; }
  #columns #rightColumn { width: 100%; padding-left: 205px; }

  #myProfile img { width: 100%; border: 1px solid #CCC; border-radius: 5px; overflow: hidden; }

  #leftColumn .grayBox { margin-bottom: 10px; }
  #quickLinks a { display: block; background-color: #3572B8; color: #FFF; text-align: left; padding: 3px 5px; margin-bottom: 1px; }
  #quickLinks a:hover, #quickLinks a:focus { background-color: #FF6A00; color: #FFF; }

  .resourcesBox { margin-bottom: 20px; }
  .resourcesBox h2 { font-size: 120%; }
  .resourcesBox .resources { white-space: nowrap; overflow-x: auto; overflow-y: hidden; }
  .resourcesBox .resources .resource { width: 150px; background-color: #EEE; border-radius: 5px; display: inline-block; vertical-align: top; padding: 2px; margin-bottom: 5px; text-align: left; position: relative; white-space: normal; }
  .resourcesBox .resources .resource img { width: 100%; }
  .resourcesBox .resources .resource .libImg { width: 145px; height: 145px; text-align: center;}
  .resourcesBox .resources .resource .title { font-weight: bold; height: 2.5em; overflow: hidden; text-overflow: ellipsis; }
   .resourcesBox .resources .resource .createdDate { font-size: 90%; overflow: hidden; text-overflow: ellipsis; padding: 1px; position: absolute; top: 2px; right: 2px; left: 2px; color: #FFF; background-color: rgba(0,0,0,0.3); text-align: right; }
  .resourcesBox .resources .resource .extra { font-style: italic; font-size: 85%; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
  .resourcesBox .resources .emptyMessage { text-align: center; margin: 50px auto; font-style: italic; }

  .showAllLink { float: right; }
  .showAllLink:hover, .showAllLink:focus { text-decoration: underline; }

  @media screen and (max-width: 550px) {
    #columns #leftColumn, #columns #rightColumn { width: 100%; display: block; margin: 0; padding: 5px; position: static; }
    #myProfile img { width: 50px; float: left; margin: 0 10px 10px 0; }
    .resourcesBox .resources { text-align: center; }
  }
</style>

<div id="content">
  <h1 class="isleH1"><%=dashboard.name %>'s Dashboard</h1>

  <div id="columns">
  
    <div class="column" id="leftColumn">

      <div id="myProfile" class="grayBox">
        <h2 class="header"><%=dashboard.name %></h2>
        <img alt="" src="<%=dashboard.avatarUrl %>" />
        <%if( dashboard.organization != null) { %><p><%=dashboard.organization %></p><% } %>
        <p><%=dashboard.jobTitle %></p>
        <p><%=dashboard.description %></p>
        <%if( dashboard.isMyDashboard){  %> <a href="/Account/Profile.aspx" class="textLink">Update My Profile</a> <% } %>
          <input type="button" class="isleButton bgBlue btnFollow" id="btnFollow" onclick="toggleFollowing()" value="Follow" runat="server" />
      </div>

      <%if(dashboard.isMyDashboard){ %>
      <div id="quickLinks" class="grayBox">
        <h2 class="header">My Stuff</h2>
        <a href="/My/Library">My Library/Collections</a>
        <a href="/My/Authored">Resources I Created</a>
        <a href="/My/Favorites.aspx">Libraries I Follow</a>
        <a href="/My/Timeline">My IOER Timeline</a>
        <a href="/Organizations" runat="server" id="orgAdminLink" visible="false">Organization Administration</a>
        <a href="/Libraries/Admin.aspx" runat="server" id="libAdminLink" visible="false">Library Administration</a>

      </div>
      <% } %>

    </div><!-- /leftColumn -->
  
    <div class="column" id="rightColumn">

      <div class="resourcesBox" id="myLibrary">
        <%if(dashboard.isMyDashboard && dashboard.library == null){ %><p class="emptyMessage">You haven't setup your Library yet!</p><% } %>
        <%else if(!dashboard.isMyDashboard && dashboard.library == null){ %><p class="emptyMessage">This user has not setup their Library yet.</p><% } %>
        <%else if(!dashboard.isMyDashboard && dashboard.libraryPublicAccessLevel < 2){ %><p class="emptyMessage">This library is marked as private.</p><% } %>
        <%else { %>
          <h2 class="isleH2"><a href="<%=dashboard.libraryUrl %>">Recent Resources from <%=dashboard.isMyDashboard ? " My Library" : " the Library of " +  dashboard.name %></a> <a class="showAllLink" href="#" onclick="loadAll('library'); return false;" style="display:none;">See All</a></h2>
          <div class="resources">
              <%foreach ( var item in dashboard.library.resources ) {%>
                <div class="resource" data-resourceID="<%=item.id %>">
                    <a href="<%=item.url %>" ><img alt="" src="//ioer.ilsharedlearning.org/OERThumbs/large/<%=item.id %>-large.png" /></a>
                    <div class="createdDate"><%=item.DateAdded.ToShortDateString() %></div>
                  <div class="title"><a href="<%=item.url %>" ><%=item.title %></a></div>
                  <div class="extra"><%=item.containerTitle %></div>
                </div>
              <% } %>
            <%if(dashboard.library.resources.Count() == 0){ %>
                <%if(dashboard.isMyDashboard){ %><p class="emptyMessage">Your Library doesn't have any Resources yet!</p><% } %>
                <%else { %><p class="emptyMessage"><%=dashboard.name %>'s Library doesn't have any Resources yet.</p><% } %>
            <% } %>
          </div>
        <% } %>
      </div><!-- /myLibrary -->


      <div class="resourcesBox" id="myOrganizationLibraries">
        <%if( dashboard.isMyDashboard && dashboard.orgLibraries == null){ %><p class="emptyMessage">You aren't a part of any Organization Libraries.</p><% } %>
        <%else if( !dashboard.isMyDashboard && dashboard.orgLibraries == null) { %><p class="emptyMessage">This user isn't a part of any Organization Libraries.</p> <% } %>
        <% else { %>
        <h2 class="isleH2">Libraries where  <%=dashboard.isMyDashboard ? "I am" : dashboard.name + " is" %>  a Member <a class="showAllLink" href="#" onclick="loadAll('orgLibraries'); return false;" style="display:none;">See All</a>&nbsp;<a id="seeAllMyOrgLibrariesLink" visible="false" runat="server" class="showAllLink" href="/Libraries/Search?showMyOrgLibraries=yes" >See All</a></h2>
        <div class="resources">
            <%foreach ( var item in dashboard.orgLibraries.resources ) {%>
              <div class="resource" data-resourceID="<%=item.id %>">
                <a href="<%=item.url %>" ><img alt="" class="libImg" src="<%=item.imageUrl %>" /></a>
                  <div class="createdDate"><%=item.DateAdded.ToShortDateString() %></div>
                <div class="title"><a href="<%=item.url %>" ><%=item.title %></a></div>
                <div class="extra"><%=item.containerTitle %></div>
              </div>
            <%}%>
        </div>
        <% } %>
      </div><!-- /org Libraries -->


      <div class="resourcesBox" id="myResources">
        <h2 class="isleH2"><%=dashboard.myResources.name %> <a class="showAllLink" href="#" onclick="loadAll('myResources'); return false;" style="display:none;">See All</a></h2>
        <div class="resources">
            <%foreach ( var item in dashboard.myResources.resources ) {%>
              <div class="resource" data-resourceID="<%=item.id %>">
                <a href="<%=item.url %>" ><img alt="" src="//ioer.ilsharedlearning.org/OERThumbs/large/<%=item.id %>-large.png" /></a>
                <div class="createdDate"><%=item.DateAdded.ToShortDateString() %></div>
                <div class="title"><a href="<%=item.url %>" ><%=item.title %></a></div>
                <div class="extra"><%=item.containerTitle %></div>
              </div>
            <% } %>
            <%if(dashboard.myResources.resources.Count() == 0){ %>
                <%if(dashboard.isMyDashboard){ %><p class="emptyMessage">You haven't created any Resources yet!</p><% } %>
                <%else { %><p class="emptyMessage"><%=dashboard.name %> hasn't created any Resources yet.</p><% } %>
            <% } %>
        </div>
      </div><!-- /myResources -->
      
      <div class="resourcesBox" id="myFollowed">
        <%if( dashboard.isMyDashboard && dashboard.followedLibraries == null){ %><p class="emptyMessage">You aren't following any Libraries yet!</p><% } %>
        <%else if( !dashboard.isMyDashboard && dashboard.followedLibraries == null) { %><p class="emptyMessage">This user isn't following any Libraries yet.</p> <% } %>
        <% else { %>
          <h2 class="isleH2">Libraries <%=dashboard.isMyDashboard ? "I am" : dashboard.name + " is" %>  Following &nbsp;<a id="seeAllMyFollowedLink" visible="false" runat="server" class="showAllLink" href="/Libraries/Search?showFollowed=yes" >See All</a></h2>
          <div class="resources">
              <%foreach ( var item in dashboard.followedLibraries.resources ) {%>
                <div class="resource" data-resourceID="<%=item.id %>">
                  <a href="<%=item.url %>" ><img alt="" class="libImg"  src="<%=item.imageUrl %>" /></a>
                    <div class="createdDate"><%=item.DateAdded.ToShortDateString() %></div>
                  <div class="title"><a href="<%=item.url %>" ><%=item.title %></a></div>
                  <div class="extra"><%=item.containerTitle %></div>
                </div>
              <% } %>
            <%if(dashboard.followedLibraries.resources.Count() == 0){ %>
                <%if(dashboard.isMyDashboard){ %><p class="emptyMessage">No Libraries you follow have added any Resources lately.</p><% } %>
                <%else { %><p class="emptyMessage">No Libraries <%=dashboard.name %> follows have added any Resources lately.</p><% } %>
            <% } %>
          </div>
        <% } %>
      </div><!-- /myFollowed -->

    </div><!-- /rightColumn -->

  </div><!-- /columns -->
</div><!-- /content -->

</div>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="litFollowingUserString" runat="server"></asp:Literal>
<asp:Literal ID="litFollowedByUserString" runat="server"></asp:Literal>
</asp:Panel>