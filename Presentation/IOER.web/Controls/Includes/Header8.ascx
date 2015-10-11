<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Header8.ascx.cs" Inherits="IOER.Controls.Includes.Header8" %>

<script type="text/javascript">
	$("document").ready(function () {
		//Handle iOS
		var userAgent = window.navigator.userAgent.toLowerCase();
		if (userAgent.indexOf("ipad") > -1 || userAgent.indexOf("iphone") > -1) {
			//Toggle on touch
			$(".navigationItem input").on("click", function () { header.toggle($(this).attr("data-headerMenu")); });
		}
		else {
			//Add additional functionality
			$(".navigationItem").on("mouseover", function () { header.showMenu($(this).attr("data-headerMenu")); });
			$(".navigationItem input").on("mouseover focus", function () { header.showMenu($(this).attr("data-headerMenu")); });
			$("#mainSiteNavigation").on("mouseout", function () { header.hideOtherMenus(); });
		}

		$("html").not($(".navigationItem")).on("click", function () { header.hideOtherMenus(); });
		$(".navigationItem").on("click", function (e) { e.stopPropagation(); });
		$(".navigationItem a").on("focus", function () { header.showMenu($(this).parentsUntil(".navigationItem").parent().attr("data-headerMenu")); });
		$(".navigationItem").last().find("a").last().on("blur", function () { header.hideOtherMenus(); });

	});
	var header = {
		toggle: function (target) {
			header.hideOtherMenus(target);
			if ($(".navigationItem input[data-headerMenu=" + target + "]").hasClass("collapsed")) {
				header.showMenu(target);
			}
			else {
				if ($(".navigationItem a:hover").length == 0) {
					header.hideOtherMenus();
				}
			}
		}
		,
		hideOtherMenus: function (target) {
			$(".navigationItem input").not(".navigationItem input[data-headerMenu=" + target + "]").removeClass("expanded").addClass("collapsed");
			$(".navigationItem .navigationLinks").not(".navigationItem .navigationLinks[data-headerMenu=" + target + "]").removeClass("expanded").addClass("collapsed");
		}
		,
		showMenu: function (target) {
			header.hideOtherMenus(target);
			$(".navigationItem input[data-headerMenu=" + target + "]").removeClass("collapsed").addClass("expanded");
			$(".navigationItem .navigationLinks[data-headerMenu=" + target + "]").removeClass("collapsed").addClass("expanded");
		}
		,
		skipNav: function () {
			$("h1").first().attr("tabindex", "0").focus(); //fallback
			$("#skipLinkTarget").focus();
		}
	};
</script>
<link rel="stylesheet" type="text/css" href="/styles/common2.css" />
<style type="text/css">
	#mainSiteHeader { margin: 0 -5px; position: relative; height: 100px; padding: 0 300px 0 140px; }
	#mainSiteHeaderLogo, #mainSiteHeaderContent, #mainSiteHeaderAccount { display: inline-block; vertical-align: top; }

	/* Logo */
	#mainSiteHeaderLogo { width: 135px; height: 100px; padding: 5px; position: absolute; top: 0; left: 0; }
	#mainSiteHeaderLogo img { max-width: 100%; max-height: 100%; }
	#skipLink { position: absolute; bottom: 5px; right: -100px; text-align: center; border-radius: 5px; padding: 5px; opacity: 0; height: 0; padding: 0; }
	#skipLink:focus { opacity: 1; height: auto; padding: 5px; }

	/* Links */
	#mainSiteHeaderContent { width: 100%; text-align: right; }
	#mainSiteNavigation { display: inline-block; }
	#mainSiteNavigation .navigationItem { display: inline-block; vertical-align: top; position: relative; }
	#mainSiteNavigation .navigationItem input { width: 100%; border: 0; background-color: transparent; font-size: 20px; text-transform: uppercase; font-weight: bold; height: 50px; line-height: 50px; padding: 0 45px 0 5px; background: right 5px center no-repeat; text-align: right; margin-right: 10px; transition: color 0.2s, background 0.2s; }
	#mainSiteNavigation .navigationItem input:hover, #mainSiteNavigation .navigationItem input:focus { background-color: #FF6A00; color: #FFF; }
	#mainSiteNavigation .navigationLinks { position: absolute; background-color: rgba(255,255,255,0.9); width: 300px; right: 0; z-index: 1000000; border-radius: 5px 0 5px 5px; transition: transform 0.3s, -webkit-transform 0.3s, -ms-transform 0.3s, opacity 0.3s; height: 0; transform-origin: top center; -webkit-transform-origin: top center; transform: scaleY(0); -webkit-transform: scaleY(0); -ms-transform: scaleY(0); opacity: 0; }
	#mainSiteNavigation .navigationItem a { display: block; padding: 5px 10px; font-size: 18px; color: #000; font-weight: bold; transition: color 0.2s, background-color 0.2s;  }
	#mainSiteNavigation .navigationItem a:hover, #mainSiteNavigation .navigationItem a:focus { background-color: #FF6A00; color: #FFF; }
	#mainSiteNavigation .navigationItem a:first-child { border-radius: 5px 0 0 0; }
	#mainSiteNavigation .navigationItem a:last-child { border-radius: 0 0 5px 5px; }
	#mainSiteNavigation input[data-headerMenu=search] { background-image: url('/images/icons/icon_search.png'); }
	#mainSiteNavigation input[data-headerMenu=share] { background-image: url('/images/icons/icon_tag.png'); }
	#mainSiteNavigation input[data-headerMenu=dashboard] { background-image: url('/images/icons/icon_myisle.png'); }
	#mainSiteNavigation input[data-headerMenu=admin] { background-image: url('/images/icons/icon_swirl.png'); }
	#mainSiteNavigation input[data-headerMenu=search].expanded { background-image: url('/images/icons/icon_search_bg.png'); }
	#mainSiteNavigation input[data-headerMenu=share].expanded { background-image: url('/images/icons/icon_tag_bg.png'); }
	#mainSiteNavigation input[data-headerMenu=dashboard].expanded { background-image: url('/images/icons/icon_myisle_bg.png'); }
	#mainSiteNavigation input[data-headerMenu=admin].expanded { background-image: url('/images/icons/icon_swirl_bg.png'); }
	#mainSiteNavigation .navigationLinks.expanded { height: auto; transform: scaleY(1); -webkit-transform: scaleY(1); -ms-transform: scaleY(1); opacity: 1; }

	/* Account */
	#mainSiteHeaderAccount { width: 300px; position: absolute; top: 0; right: 0; }
	#mainLoginLinkBox { padding: 8px 5px; }
	#mainLoginLink { display: block; text-align: right; padding: 5px; font-size: 20px; text-align: center; }
	#mainProfileCard { padding: 5px; position: relative; }
	#mainProfileCard #userName { font-size: 18px; background-color: #4F4E4F; color: #FFF; padding: 2px 95px 2px 5px; text-align: right; border-radius: 5px; }
	#mainProfileCard #mainProfileImage { position: absolute; top: 5px; right: 5px; width: 90px; height: 90px; background: #EEE center center no-repeat; background-size: cover; border-radius: 5px; border: 1px solid #4F4E4F; }
	#mainProfileCard #options { text-align: right; padding: 2px 95px 2px 5px; white-space: nowrap; }
	#mainProfileCard #options a { display: inline-block; vertical-align: top; padding: 0 5px; }
	#mainProfileCard #options a:first-child { border-right: 1px solid #CCC; }

	/* Responsive */
	@media (max-width: 875px) {
		#mainSiteNavigation .navigationItem input { font-size: 0; padding: 0 35px 0 0; background-position: center center; }
	}
	@media (max-width: 600px) {
		#mainSiteHeader { padding: 50px 0 0 30%; }
		#mainSiteHeaderLogo { width: 30%; }
		#mainSiteHeaderAccount { width: 70%; }
		#mainSiteNavigation .navigationItem:nth-last-child(2) .navigationLinks { right: -45px; }
		#mainSiteNavigation .navigationItem:nth-last-child(3) .navigationLinks { right: -90px; }
		#mainSiteNavigation .navigationItem:nth-last-child(4) .navigationLinks { right: -135px; }
		#mainProfileCard { padding: 2px; }
		#mainProfileCard #mainProfileImage { width: 45px; height: 45px; top: 2px; right: 2px; }
		#mainProfileCard #userName { padding-right: 50px; font-size: 14px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
		#mainProfileCard #options { padding-right: 50px; font-size: 12px; }
	}
</style>

<div id="mainSiteHeader">
	<!-- Logo -->
	<div id="mainSiteHeaderLogo">
		<a href="/" title="Home">
			<img alt="IOER Logo" src="/images/ioer_med_wide.png" />
		</a>
		<a href="#skipLinkTarget" class="isleButton bgBlue" id="skipLink" onclick="header.skipNav();">Skip to Content</a>
	</div><!--
	Navigation 
	--><div id="mainSiteHeaderContent">
		<div id="mainSiteNavigation">
			<div class="navigationItem" data-headerMenu="search">
				<input type="button" value="Search" data-headerMenu="search" class="collapsed" />
				<div class="navigationLinks collapsed" data-headerMenu="search">
					<a href="/Search">Resources</a>
					<a href="/Content/Search">Created Resources</a>
					<a href="/Libraries/Search">Libraries</a>
					<a href="/LearningLists">Learning Lists</a>
					<a href="/gooruSearch">gooru Resources (beta)</a>
				</div>
			</div><!--
			--><div class="navigationItem" data-headerMenu="share">
				<input type="button" value="Share" data-headerMenu="share" class="collapsed" />
				<div class="navigationLinks collapsed" data-headerMenu="share">
					<a href="/Contribute">Contribute Resources</a>
          <a href="/Community/1/ISLE_Community">Communities</a>
				</div>
			</div><!--
			<% if(IsLoggedIn) { %>
			--><div class="navigationItem" data-headerMenu="dashboard">
				<input type="button" value="Dashboard" data-headerMenu="dashboard" class="collapsed" />
				<div class="navigationLinks collapsed" data-headerMenu="dashboard">
					<a href="/My/Dashboard">My Dashboard</a>
					<a href="/My/Library" >My Library and Collections</a>
					<a href="/My/Authored">Resources I Created</a>
					<a href="/My/Favorites.aspx">Libraries I Follow</a>
					<a href="/My/Timeline">My IOER Timeline</a>
				</div>
			</div><!--
			<% } %>
			<% if(ShowAdminMenu) { %>
			--><div class="navigationItem" data-headerMenu="admin">
				<input type="button" value="Admin" data-headerMenu="admin" class="collapsed" />
				<div class="navigationLinks collapsed" data-headerMenu="admin">
					<a href="/Admin/Groups/default.aspx">Groups Management</a>
					<a href="/Libraries/Admin.aspx">Library Admin</a>
					<a href="/Organizations/">Organizations Management - NEW</a>
                    <a href="/Organizations/Organizations.aspx">Organizations Management - OLD</a>
					<a href="/Admin/Queries/QueryMgmt.aspx">Query Maintenance</a>
					<a href="/Publishers.aspx">Publishers Search</a>
					<a href="/Admin/mapping/MapCareerClusters.aspx">Map Career Clusters</a>
					<a href="/Admin/mapping/MapK12Subjects.aspx">Map K12 Subjects</a>
					<a href="/Admin/mapping/MapResourceType.aspx">Map Resource Types</a>
					<a href="/Admin/mapping/MapResourceFormat.aspx">Map Resource Format</a>
					<a href="/Pages/MapResourceNSDL.aspx">Map Resource NSDL Schema</a>
					<a href="/Content/Search">IOER Content Search</a>
					<% if(ShowAdminExtras){  %>
					<%--<a href="/My/Library" class="navLink">My Library and Collections</a>
					<a href="/My/Authored" class="navLink">Resources I Created</a>
					<a href="/My/Favorites.aspx" class="navLink">Libraries I Follow</a>--%>
					<a href="/My/Timeline">My IOER Timeline</a>
					<a href="/Admin/AdminStuff.aspx">Copy Collections prototype</a>
					<a href="/Admin/AdminStuff.aspx">Admin Stuff</a>
					<% } %>
				</div>
			</div><!--
			<% } %>
			-->
		</div>
	</div><!--
	Account Stuff
	--><div id="mainSiteHeaderAccount">
		<% if(IsLoggedIn) { %>
		<!-- CurrentUser logged in -->
		<div id="mainProfileCard">
			<div id="userName"><%=CurrentUser.FullName() %></div>
			<div id="options">
				<a href="/Account/Profile.aspx">My Profile</a><a href="/?logout=true">Logout</a>
			</div>
			<a href="/Account/Profile.aspx" title="Profile Image" id="mainProfileImage" style="background-image:url('<%=string.IsNullOrEmpty(CurrentUser.ImageUrl) ? "/images/defaultProfileImg.jpg" : CurrentUser.ImageUrl  %>');"></a>
		</div>
		<% } else { %>
		<!-- User not logged in -->
		<div id="mainLoginLinkBox">
			<a id="mainLoginLink" class="isleButton bgGreen" href="/Account/Login.aspx?nextUrl=<%=Request.Url.PathAndQuery %>">Login/Register</a>
		</div>
		<% } %>
	</div>
	<a id="skipLinkTarget"></a>
</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
	<asp:Literal ID="skipRedirectCheck" runat="server">no</asp:Literal>
	<asp:Literal ID="txtAdminSecurityName" runat="server">Site.Admin</asp:Literal>
</asp:Panel>