<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header7.ascx.cs" Inherits="ILPathways.Controls.Includes.Header7" %>

<script type="text/javascript">

  $(document).ready(function () {
    $(".topNav a").on("focus", function () {
      $(this).parent().addClass("active");
    }).on("blur", function () {
      $(this).parent().removeClass("active");
    });


    if (navigator.userAgent.indexOf("iPad") > -1 || navigator.userAgent.indexOf("iPhone") > -1) {
      //$("#header").removeClass("no-ithing");
      /*$(".topNav").on("touchstart", function () {
        $(this).find(".navLinks").addClass("active");
      });*/
      $("html").on("touchstart", function () {
        $(".navLinks").removeClass("active");
        $(".topNav:hover .navLinks").addClass("active");
        $(".navLinks").hide();
        $(".topNav:hover .navLinks").show();
      });
      $(".topNav").on("touchstart", function () {
        var myLinks = $(this).find(".navLinks");
        if (myLinks.first().hasClass("active")) {
          $(".navLinks").removeClass("active");
          $(".navLinks").hide();
        }
        else {
          $(".navLinks").removeClass("active");
          myLinks.addClass("active");
          $(".navLinks").hide();
          myLinks.show();
        }
      });
    }
  });

  function manualLogout(id) {
    theForm.onsubmit = null;
  }


</script>
<style type="text/css">
  #header, #header * { box-sizing: border-box; -moz-box-sizing: border-box; }
  #header { height: 125px; position: relative; background-color: #FFF; z-index: 100000; min-width: 300px; }
  #logo { background: url('/images/ioer_med.png') no-repeat center center; background-size: cover; height: 110px; width: 150px; display: inline-block; position: absolute; top: 5px; left: 5px; z-index: 10; }
  #headerContent { width: 100%; padding-left: 160px; }
  #navbar { height: 50px; position: absolute; bottom: 0; left: 0px; width: 100%; padding-left: 160px; }
  .topNav { color: #000; white-space: nowrap; background: url('') no-repeat 5px 50%; display: inline-block; vertical-align: bottom; margin-right: -4px; border-right: 1px solid #CCC; position: relative; padding-left: 40px; height: 100%; transition: padding 0.5s, background-position 0.5s; -webkit-transition: padding 0.5s, background-position 0.5s; }
  .topNav:last-child { border: none; }
  .topNav h2, a.topNav { font-size: 18px; line-height: 50px; text-transform: uppercase; font-weight: bold; }
  #accountStuff { position: absolute; top: 5px; right: 5px; text-align: right; }
  #accountStuff * { display: inline; }
  .navLinks { box-shadow: 0 0 20px -1px #FF5707; white-space: normal; position: absolute; top: 99.5%; left: 0; width: 100%; background-color: #FFF; border-radius: 0 0 5px 5px; overflow: hidden; max-height: 0; transition: max-height 0.3s, opacity 0.3s; -webkit-transition: max-height 0.3s, opacity 0.3s; opacity: 0; }
  .navLinks a, .navLinks a:visited { color: #000; display: block; padding: 10px 5px 10px 5px; font-weight: bold; text-align: center; color: #000; }
  .topNav:hover, .topNav:focus, .navLinks a:hover, .navLinks a:focus { background-color: #FF5707; color: #FFF; cursor: pointer; }
  #navbar.three .topNav { width: 33.32%; }
  #navbar.four .topNav { width: 25%; }
  #navbar.five .topNav { width: 25%; }

  .no-ithing .topNav:hover .navLinks, .topNav:focus .navLinks, .topNav .navLinks.active { max-height: 300px; opacity: 1; }
  .no-ithing .topNav.adminNav:hover .navLinks, .topNav.adminNav:focus .navLinks, .topNav.adminNav .navLinks.active { max-height: 900px; opacity: 1; }
  .no-ithing .navLinks a { max-height: 0; padding: 0; transition: max-height 0.5s, padding 0.5s; -webkit-transition: max-height 0.5s, padding 0.5s; }
  .no-ithing .topNav:hover .navLinks a, .topNav:focus .navLinks a, .navLinks.active a { max-height: 100px; padding: 10px 5px; }

  #skipLink { opacity: 0; position: absolute; top: 10px; left: 50%; font-weight: bold; background-color: #EEE; border-radius: 5px; display: inline-block; height: 0; }
  #skipLink:focus { opacity: 1; height: auto; padding: 5px; }

  #accountStuff .adminNav { display: block; text-align: left; z-index: 100; min-width: 200px; border-radius: 5px 5px 0 0; }
  #accountStuff .adminNav * { display: block; }
  #altLogin {display: none;}

  @media screen and (max-width: 580px) {
    #mainLogin {display: none;}
    #altLogin {display: block;}
  }
  @media screen and (max-width: 699px) {
    #navbar .topNav { position: static; }
  }
  @media screen and (max-width: 850px) {
    #navbar .topNav { background-position: center center; padding: 0; }
    #navbar .topNav h2 { display: none; font-size: 0; }
    .myIOERNavTitle {display: none;}
    #accountStuff .profileStuff span { display: block; }
    #accountStuff .adminNav { height: 25px; min-width: 150px; }
    #accountStuff .adminNav h2 { line-height: 25px; }
  }

  .adminNav, .adminNav { background-image: url('/images/icons/icon_swirl.png'); }
  .searchNav, .searchNav { background-image: url('/images/icons/icon_search.png'); }
  .shareNav, .shareNav { background-image: url('/images/icons/icon_tag.png'); }
  .infoNav, .infoNav { background-image: url('/images/icons/icon_help.png'); }
  .myIOERNav, .myIOERNav { background-image: url('/images/icons/icon_myisle.png'); }

  .adminNav:hover, .adminNav:focus { background-image: url('/images/icons/icon_swirl_bg.png'); }
  .searchNav:hover, .searchNav:focus { background-image: url('/images/icons/icon_search_bg.png'); }
  .shareNav:hover, .shareNav:focus { background-image: url('/images/icons/icon_tag_bg.png'); }
  .infoNav:hover, .infoNav:focus { background-image: url('/images/icons/icon_help_bg.png'); }
  .myIOERNav:hover, .myIOERNav:focus { background-image: url('/images/icons/icon_myisle_bg.png'); }
</style>

<div id="header" class="no-ithing">
  <a href="/" id="logo" title="Open Educational Resources"></a>
  <div id="headerContent">
    <a id="skipLink" href="#skipLinkTarget" onclick="skipNav();" title="Skip to Content">Skip to Content</a>
    <div id="accountStuff">
      <div id="mainLogin"><a href="" id="loginLink" runat="server" class="loginLink textLink">Login/Register</a></div>
        <div id="altLogin"><a href="" id="loginLink2" runat="server" class="loginLink textLink">Login/Register</a></div>
      <div id="profileStuff" class="profileStuff" runat="server">
        <span>Logged in as </span>
        <a href="/Account/Profile.aspx" id="profileLink" runat="server" class="textLink" title="My Profile"></a> | 
        <asp:LinkButton ID="logoutLink" runat="server" Text="Logout" OnClientClick="manualLogout(this.id);" OnClick="logoutButton_Click" CssClass="textLink" />

        <div class="topNav adminNav" tabindex="0" aria-haspopup="true" id="adminMenu" runat="server">
          <h2>Admin</h2>
            <div class="navLinks">
                <a href="/Admin/Groups/default.aspx" class="navLink">Groups Management</a>
                <a href="/Libraries/Admin.aspx">Library Admin</a>
                <a href="/Admin/Org/Organizations.aspx" class="navLink">Organizations Management</a>
                <a href="/Admin/Queries/QueryMgmt.aspx" class="navLink">Query Maintenance</a>
                <a href="/Publishers.aspx">Publishers Search</a>
                <a href="/Admin/mapping/MapCareerClusters.aspx">Map Career Clusters</a>
                <a href="/Admin/mapping/MapK12Subjects.aspx">Map K12 Subjects</a>
                <a href="/Admin/mapping/MapResourceType.aspx">Map Resource Types</a>
                <a href="/Admin/mapping/MapResourceFormat.aspx">Map Resource Format</a>
                <a href="/Pages/MapResourceNSDL.aspx">Map Resource NSDL Schema</a>
                <a href="/Repository/All" class="navLink">IOER Content Search</a>
                <div id="mpMenu" runat="server" visible="false">
                    <a href="#" class="navLink">=============================================</a>
                    <a href="/My/Library" class="navLink">My Library and Collections</a>
                    <a href="/My/Authored.aspx" class="navLink">Resources I Created</a>
                    <a href="/My/Favorites.aspx" class="navLink">Libraries I Follow</a>
                    <a href="/My/Timeline" class="navLink">My IOER Timeline</a>
                    <a href="/Admin/AdminStuff.aspx">Copy Collections prototype</a>
                    <a href="/Admin/AdminStuff.aspx" class="navLink">Admin Stuff</a>
                </div>
          </div>
        </div>

      </div>

    </div>
    <div id="navbar" class="<%=topNavCount %>">

      <div class="topNav searchNav" tabindex="0" aria-haspopup="true">
        <h2>Search</h2>
        <div class="navLinks">
          <a href="/Search">ISLE Resources</a>
          <a href="/Libraries/Search">ISLE Libraries</a>
          <a href="/gooruSearch">gooru Resources (beta)</a>
          <!--<a href="/Publishers.aspx">Publishers</a>-->
        </div>
      </div>

      <div class="topNav shareNav" tabindex="0" aria-haspopup="true">
        <h2>Share</h2>
        <div class="navLinks">
          <a href="/Contribute">Contribute Resources</a>
          <a href="/Community/1/ISLE_Community">Communities</a>
        </div>
      </div>

      <div class="topNav infoNav" tabindex="0" aria-haspopup="true">
        <h2>Info</h2>
        <div class="navLinks">
          <a href="/IOER_Timeline">IOER Timeline</a>
          <a href="/Help/Guide.aspx">User Guide</a>
          <a href="/News/Default.aspx">IOER News</a>
        </div>
      </div>

      <!--<div class="topNav myIOERNav" tabindex="0" aria-haspopup="true" id="myIOERMenu" runat="server">
        <h2>My Dashboard</h2>
        <div class="navLinks">
          <a href="/My/Dashboard" class="navLink">My Dashboard</a>
          <!--<a href="/My/Library" class="navLink">My Library and Collections</a>
          <a href="/My/Authored.aspx" class="navLink">Resources I Created</a>
          <a href="/My/Favorites.aspx" class="navLink">Libraries I Follow</a>
          <a href="/My/Timeline" class="navLink">My IOER Timeline</a>
          <a href="/Libraries/Admin.aspx" class="navLink" runat="server" id="libAdminLink" visible="false">Library Administration</a>-->
        <!--</div>
      </div>-->

      <a href="/My/Dashboard" class="topNav myIOERNav" tabindex="0" aria-haspopup="true" id="myIOERMenuLink" runat="server"><span class="myIOERNavTitle">My Dashboard</span></a>


    </div><!--/navbar-->
  </div><!--/headerContent-->
  <a id="skipLinkTarget" href="#"></a>
  <div id="hiddenStuff" runat="server" visible="false">
    <asp:Literal ID="txtAdminSecurityName" runat="server" Visible="false">ILPathways.Admin.QueryMgmt</asp:Literal>
      <asp:Literal ID="skipRedirectCheck" runat="server" Visible="false">no</asp:Literal>
      <asp:Literal ID="doingSecureCheck" runat="server" Visible="false">no</asp:Literal>
      <asp:Literal ID="loginUrl1" runat="server" Visible="false">/Account/Login.aspx?nextUrl=</asp:Literal>
       <asp:Literal ID="loginUrl2" runat="server" Visible="false">/Pages/Login.aspx?nextUrl=</asp:Literal>
  </div>
</div>
