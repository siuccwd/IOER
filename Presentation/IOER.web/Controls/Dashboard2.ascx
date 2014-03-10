<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Dashboard2.ascx.cs" Inherits="ILPathways.Controls.Dashboard2" %>
<%@ Register TagName="FeedControl" TagPrefix="uc1" Src="/controls/Activity1.ascx" %>

<script type="text/javascript">
  //From server
  <%=profile %>
  <%=libraryThumbs %>
  <%=resourceThumbs %>
</script>
<script type="text/javascript">
  $(document).ready(function () {
    renderUserProfile(profile);
  });

  /* Rendering Functions */
  function renderUserProfile(data) {
    $("#myName").html(profile.name);
    $("#myAvatar").css("background-image", "url('" + profile.avatarURL + "')");
    $("#myDescription").html(profile.description);
  }
</script>
<style type="text/css">
  /* Big Stuff */
  * { box-sizing: border-box; -moz-box-sizing: border-box; }
  .mainContent { transition: margin-left 1s; -webkit-transition: margin-left 1s; font-size: 0; }

  /* Section stuff */
  #my, #myFeed { display: inline-block; vertical-align: top; }
  #my { padding-right: 10px; width: 25%; min-width: 300px; }
  #myFeed { width: 75%; max-width: calc(100% - 300px); }
  #my .section { background-color: #EEE; border-radius: 5px; padding: 5px; margin-bottom: 10px; }

  /* Profile stuff */
  #myProfile { font-size: 0; }
  #myProfile #myAvatar, #myProfile #profileDetails { display: inline-block; vertical-align: top; }
  #myProfile #myAvatar { width: 100px; height: 100px; border-radius: 5px; background-size: contain; }
  #myProfile #profileDetails { width: calc(100% - 100px); padding-left: 5px; }

  /* Feed stuff */
  #myFeed #content { padding-left: 0; }
  #myFeed h1 { display: none; }


  @media screen and (min-width: 950px) {
    .mainContent { padding-left: 50px; }
  }
</style>

<div id="mainContent" class="mainContent" runat="server">
  <h1 class="isleH1">My Dashboard</h1>

  <div id="my">
    <div class="section gray" id="myProfile">
      <div id="myAvatar"></div>
      <div id="profileDetails">
        <h2 id="myName"></h2>
        <p id="myDescription"></p>
      </div>
    </div><!-- /myProfile -->

    <div class="section gray" id="myStuff">
      <div id="myLibrary" class="myStuffBox">
        <h2 id="myLibraryHeader">My Library</h2>
        <div id="myLibraryContent"></div>
      </div>
      <div id="myResources" class="myStuffBox">
        <h2 id="myResourcesHeader">My Resources</h2>
        <div id="myResourcesContent"></div>
      </div>
    </div><!-- /myStuff -->
  </div>

  <div id="myFeed">
    <uc1:FeedControl ID="myFeedControl" runat="server" />
  </div><!-- /myFeed -->

</div>

<div id="errorMessage" runat="server">
  <p style="padding: 50px; text-align: center;">Please login to access your Dashboard.</p>
</div>