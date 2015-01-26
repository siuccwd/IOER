<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header6.ascx.cs" Inherits="ILPathways.Controls.Includes.Header6" %>
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
  var navCount = 0;
  $(document).ready(function () {
    //Apply tabindex to each top level nav item
    $(".navTop").each(function () {
      $(this).attr("tabindex", 0);
      navCount++;
    });

    //Fix for Safari
    if (navigator.userAgent.indexOf("5.1.7 Safari") > -1 || navigator.userAgent.indexOf("5.0 Safari") > -1 || navigator.userAgent.indexOf("4.0 Mobile Safari") > -1) {
      $(".navTop").css("margin-right", "-4px");
    }

    //Apply width to top level nav, and apply behavioral functionality
    $(".navTop").css("width", ((1 / navCount) * 100) + "%")
      .on("mouseout", function () { $(this).trigger("blur") });

    $(".navTop a").on("blur", function () { $(this).parent().removeClass("showing"); })
      .on("focus", function () { $(this).parent().addClass("showing"); });

    //Add menu button functionality
    $("#mobileMenuButton").on("click", function () {
      var nav = $("#navigation");
      nav.hasClass("mobileHidden") ? nav.removeClass("mobileHidden") : nav.addClass("mobileHidden");
      return false;
    });
  });

  function manualLogout(id) {
    theForm.onsubmit = null; //The onsubmit is normally overridden to return false to prevent submission by hitting "enter" in a text field. This resets that behavior to normal.
  }

  function skipNav() {
    console.log("Skipping Navigation");
    $("#mainContentTarget").focus();
    $("body a").not("#header a").first().focus();
  }
</script>

<style type="text/css">
  #header { margin: 0px 0 10px 0; position: relative; height: 100px; border-bottom: 1px solid #DDD; min-width: 300px; }
  #header, #header * { box-sizing: border-box; -moz-box-sizing: border-box; -webkit-box-sizing: border-box; }
  #logoLink { 
    display: inline-block; 
    width: 120px; 
    height: 100px; 
    background: transparent url('/images/ioer_med_bigtext.png') no-repeat center center; 
    background-size: cover;
    position: absolute;
    top: 0;
    left: 5px;
  }
  #siteHeader { 
    text-align: left;
    font-size: 20px;
    margin-left: 135px;
    height: 40px;
    line-height: 40px;
    text-transform: uppercase;
    max-height: 40px;
    text-overflow: ellipsis;
    white-space: nowrap;
    overflow: hidden;
    color: #000;
    background-color: transparent;
    border-radius: none;
    box-shadow: none;
    margin-bottom: 0;
    padding: 0;
  }
  #navigation {
    padding: 5px 0 0 125px;
    width: 100%;
    font-size: 0;
  }
  #navigation h2 {
    font-size: 16px;
    text-transform: uppercase;
    color: inherit;
  }
  #navigation .navTop {
    border-right: 1px solid #DDD;
    padding: 0 5px;
    background: #FFF url('/images/icons/icon_search.png') no-repeat 5px center;
    padding: 0 5px 0 35px;
    position: relative;
  }
  #navigation .navTop .navLinks {
    background-color: #FFF;
    position: absolute;
    right: 0;
    top: 53px;
    height: 0;
    overflow: hidden;
    z-index: 1000000;
    opacity: 0;
    transition: opacity 0.2s;
    -webkit-transition: opacity 0.2s;
    border-radius: 0 0 5px 5px;
    border: 1px solid #CCC;
    border-top: none;
  }
  #navigation .navTop a {
    font-size: 14px;
    line-height: 18px;
    padding: 10px 5px 5px 5px;
    min-height: 40px;
    color: #000;
    width: 100%;
    text-align: center;
    font-weight: bold;
    display: block;
  }
  #navigation .navTop a:last-child {
    border-radius: 0 0 5px 5px;
  }
  #navigation .navTop a:hover, #navigation .navTop a:focus {
    color: #FFF;
  }
  #navigation .navTop:hover .navLinks, #navigation .navTop:focus .navLinks, #navigation .navLinks.showing {
    height: auto;
    opacity: 1;
  }
  #navigation .navTop:hover, #navigation .navTop:focus {
    color: #FFF;
    cursor: pointer;
  }
  #headerContent #loggedInName {
    position: absolute; 
    top: 5px;
    right: 10px;
    text-align: right;
  }
  #mobileMenuButton {
    display: block;
    color: #FFF;
    font-weight: bold;
    text-transform: uppercase;
    border-radius: 5px;
    margin: 0 15px 0 140px;
    height: 50px;
    line-height: 50px;
    text-align: center;
    font-size: 20px;
  }
  
  
  @media screen and (max-width: 899px) {
    #navigation { position: absolute; top: 100%; right: 0; z-index: 1000000; padding: 0; }
    #navigation .navTop { display: block; height: 54px; line-height: 54px; min-width: 100%; border: 1px solid #CCC; border-top: none; }
    #navigation .navTop:first-child { border-top: 1px solid #CCC; }
    #headerContent #loggedInName { display: none; }
    #mobileMenuButton { display: block; }
    #navigation.mobileHidden { display: none; }
    #navigation .navTop .navLinks { width: 65%; }
  }
  @media screen and (min-width: 900px) {
    #navigation, #navigation.mobileHidden { display: block; padding-left: 125px; }
    #navigation .navTop { display: inline-block; vertical-align: bottom; height: 54px; line-height: 54px; }
    #headerContent #loggedInName { display: block; }
    #navigation .navTop:last-child { border-right: none; }
    #mobileMenuButton { display: none; }
    #navigation .navTop .navLinks { width: 100%; }
  }
  
  #navigation .navTop[data-icon=search] { background-image: url('/images/icons/icon_search.png'); }
  #navigation .navTop[data-icon=contribute] { background-image: url('/images/icons/icon_tag.png'); }
  #navigation .navTop[data-icon=help] { background-image: url('/images/icons/icon_help.png'); }
  #navigation .navTop[data-icon=my] { background-image: url('/images/icons/icon_myisle.png'); }
  #navigation .navTop[data-icon=admin] { background-image: url('/images/icons/icon_swirl.png'); }
  #navigation .navTop[data-icon=login] { background-image: url('/images/icons/icon_loginaccount.png'); }
  #navigation .navTop[data-icon=search]:hover, #navigation .navTop[data-icon=search]:focus { background-image: url('/images/icons/icon_search_bg.png'); }
  #navigation .navTop[data-icon=contribute]:hover, #navigation .navTop[data-icon=contribute]:focus { background-image: url('/images/icons/icon_tag_bg.png'); }
  #navigation .navTop[data-icon=help]:hover, #navigation .navTop[data-icon=help]:focus { background-image: url('/images/icons/icon_help_bg.png'); }
  #navigation .navTop[data-icon=my]:hover, #navigation .navTop[data-icon=my]:focus { background-image: url('/images/icons/icon_myisle_bg.png'); }
  #navigation .navTop[data-icon=admin]:hover, #navigation .navTop[data-icon=admin]:focus { background-image: url('/images/icons/icon_swirl_bg.png'); }
  #navigation .navTop[data-icon=login]:hover, #navigation .navTop[data-icon=login]:focus { background-image: url('/images/icons/icon_loginaccount_bg.png'); }

  #skipNav { position: absolute; top: 3px; left: 60%; opacity: 0; background-color: #EEE; display: inline-block; padding: 2px 5px; }
  #skipNav:focus { opacity: 1; }
</style>
<style>
  /* Dynamic colors - VS2012 breaks formatting on these */
  #navigation .navTop a:hover, #navigation .navTop a:focus { background-color: <%=css_orange %>; }
  #navigation .navTop:hover, #navigation .navTop:focus { background-color: <%=css_orange %>; }
  #mobileMenuButton { background-color: <%=css_blue %>; }
  #mobileMenuButton:hover, #mobileMenuButton:focus { background-color: <%=css_orange %>; }
</style>

<div id="header">
  <a href="/" id="logoLink"></a>
  <div id="headerContent">
    <h1 id="siteHeader">Illinois Shared Learning Environment <asp:Literal ID="environmentText" runat="server" /></h1>
    <a id="skipNav" href="#mainContentTarget" onclick="skipNav();">Skip to Content</a>
    <div id="loggedInName"><asp:label ID="lblLoggedInAs" runat="server" /></div>
    <a href="#" id="mobileMenuButton">Menu</a>
    <div id="navigation" class="mobileHidden">
      <div data-icon="search" class="navTop" data-href="/Search.aspx" aria-haspopup="true">
        <h2>Search</h2>
        <div class="navLinks">
          <a href="/Search.aspx" class="navLink">Resources</a>
          <a href="/Libraries/Default.aspx" class="navLink">Libraries</a>
          <a href="/Publishers.aspx" class="navLink">Publishers</a>
        </div>
      </div>
      <div data-icon="contribute" class="navTop" data-href="" aria-haspopup="true">
        <h2>Share</h2>
        <div class="navLinks">
          <a href="/Contribute" class="navLink">Contribute Resources</a>
            <a href="/Community/1/ISLE_Community" class="navLink">Communities</a>
          <!--<a href="/Publish.aspx" class="navLink">Tag Online Resource</a>
          <a href="/My/Author.aspx" class="navLink">Create New Resource</a>-->
        </div>
      </div>
      <div data-icon="help" class="navTop" data-href="/Guide.aspx" aria-haspopup="true">
        <h2>Information</h2>
        <div class="navLinks">
          <!--<a href="/Help" class="navLink">Getting Started</a>-->
          <!--<a href="/Help/FAQs.aspx" class="navLink">Frequently Asked Questions</a>-->
          <a href="/IOER_Timeline/" class="navLink">IOER Timeline</a>
          <a href="/Help/Guide.aspx" class="navLink">User Guide</a>
          <a href="/News/Default.aspx" class="navLink">OER News</a>
        </div>
      </div>
      <div data-icon="my" class="navTop" id="myIsle" runat="server" data-href="" aria-haspopup="true">
        <h2>My IOER</h2>
        <div class="navLinks">
          <a href="/My/Library" class="navLink">My Library and Collections</a>
          <a href="/My/Authored.aspx" class="navLink">Resources I Created</a>
          <a href="/My/Favorites.aspx" class="navLink">Libraries I Follow</a>
          <a href="/My/Timeline" class="navLink">My IOER Timeline</a>
          <a href="/Libraries/Admin.aspx" class="navLink" runat="server" id="libAdminLink" visible="false">Library Administration</a>
        </div>
      </div>
      <div data-icon="admin" class="navTop" id="adminMenu" visible="false" runat="server" data-href="" aria-haspopup="true">
        <h2>Admin</h2>
        <div class="navLinks">
            <a href="/Admin/Groups/default.aspx" class="navLink">Groups Management</a>
            <a href="/Admin/Org/Organizations.aspx" class="navLink">Organizations Management</a>
            <a href="/Admin/Queries/QueryMgmt.aspx" class="navLink">Query Maintenance</a>
            <a href="/Admin/mapping/MapCareerClusters.aspx">Map Career Clusters</a>
            <a href="/Admin/mapping/MapK12Subjects.aspx">Map K12 Subjects</a>
            <a href="/Admin/AdminStuff.aspx">Copy Collections prototype</a>
            <a href="/Admin/mapping/MapResourceType.aspx">Map Resource Types</a>
            <a href="/Admin/mapping/MapResourceFormat.aspx">Map Resource Format</a>
            <a href="/Pages/MapResourceNSDL.aspx">Map Resource NSDL Schema</a>
            <a href="/Activity/">Site Activity</a>
            <a href="/Repository/All" class="navLink">IOER Content</a>
        </div>
      </div>
        
      <div data-icon="login" class="navTop" id="loginNavTop" runat="server" aria-haspopup="true">
        <h2><asp:Literal ID="loginTitle" runat="server" /></h2>
        <div class="navLinks">
          <a href="" id="loginLink" runat="server" class="navLink">Login</a>
          <a href="/Account/Register.aspx" id="registerLink" runat="server" class="navLink">Register</a>
          <a href="/Account/Profile.aspx" id="profileLink" runat="server" class="navLink">My Profile</a>
          <asp:LinkButton ID="logoutLink" runat="server" Text="Logout" OnClientClick="manualLogout(this.id);" OnClick="logoutButton_Click" />
        </div>
      </div>
    </div>
  </div>
</div>

<div id="hiddenStuff" runat="server" visible="false">
  <asp:Literal ID="txtAdminSecurityName" runat="server" Visible="false">ILPathways.Admin.QueryMgmt</asp:Literal>

<div data-icon="admin" class="navTop" id="mpMenu" visible="false" runat="server" data-href="">
        <h2>MP Admin</h2>
        <div class="navLinks">
            <a href="/Admin/Org/organizations.aspx" class="navLink">Org Management</a>
            <a href="/Admin/AdminStuff.aspx" class="navLink">Admin Stuff</a>
            <a href="/Libraries/Admin.aspx">Library Admin</a>

        </div>
      </div>
</div>

<a id="mainContentTarget" href="#"></a>