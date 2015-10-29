<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SplashMini.ascx.cs" Inherits="IOER.Controls.SplashMini" %>

<%=scriptLink %>
<script type="text/javascript">
  $(document).ready(function() {
    if(<%=useNewWindow %>){
      $("#splash .splashItem a").each(function() {
        $(this).attr("target", "_blank");
      });
    }
    else {
      $("#splash .splashItem a").each(function() {
        $(this).removeAttr("target");
      });
    }
  });
</script>

<style type="text/css">

	#splash, #splash * { box-sizing: border-box; -moz-box-sizing: border-box; font-family: Calibri, Arial, Helvetica, Sans-serif; }
	#splash { text-align: center; }
	.splashItem { 
		display: inline-block; 
		vertical-align: top; 
		text-align: right; 
		min-height: 155px;
		position: relative;
		padding-left: 75px;
		min-width: 285px;
	}
	.splashItem a, .splashItem h2 {
		display: inline-block; 
		color: #000; 
		text-decoration: none; 
		background-color: #FFF; 
		padding: 2px 5px; 
		width: 100%;
		box-shadow: 0 0 5px #CCC;
		margin: 2px 0;
		border-radius: 5px;
    font-size: 18px;
	}
	.splashItem a:hover, .splashItem a:focus { background-color: #FF5707; box-shadow: 0 0 5px #FF5707; color: #FFF; }
	.splashItem img { 
		position: absolute;
		top: 0px;
		left: 0px;
		height: 155px;
		border-radius: 50%;
		background-color: #FFF;
		box-shadow: 0 0 10px #CCC;
	}
	.splashItem:hover img, .splashItem:focus img {
		box-shadow: 0 0 10px #FF5707, inset 0 0 50px #FF5707;
	}
	.splashItem h2 { background-color: #EEE; }
	
	@media screen and (max-width: 650px) {
		.splashItem { width: 95%; margin: 5px auto; }
	}
	@media screen and (min-width: 651px) {
		.splashItem { width: 45%; margin: 10px 5px; }
	}
	@media screen and (min-width: 1425px) {
		.splashItem { width: 23%; margin: 10px 5px; }
	}
</style>

	<div id="splash">
		
		<div class="splashItem" id="search">
			<h2>Search</h2>
			<a target="_blank" href="/Search" >ISLE Resources</a>
			<a target="_blank" href="/Libraries/Search" >ISLE Libraries</a>
            <a target="_blank" href="/gooruSearch" >gooru Resources</a>
			<!--<a target="_blank" href="/Publishers.aspx" >Publishers</a>-->
			<img src="/images/icons/icon_search_med.png" />
		</div>
		<div class="splashItem" id="contribute">
			<h2>Share</h2>
            <a target="_blank" href="/Contribute/">Share Resources</a>
            <a target="_blank" href="/Community/1/ISLE_Community" class="navLink">Communities</a>
			<!--<a target="_blank" href="/Publish.aspx" >Tag Resource</a>
			<a target="_blank" href="/My/Author.aspx" >Create Resource</a>-->
			<img src="/images/icons/icon_create_med.png" />
		</div>
		<div class="splashItem" id="information">
			<h2>Information</h2>
			<a target="_blank" href="/Help/Guide.aspx" >Guide</a>
			<a target="_blank" href="/News/Default.aspx" >News</a>
      <a target="_blank" href="/IOER_Timeline/">IOER Timeline</a>
			<img alt="" src="/images/icons/icon_resources_med.png" />
		</div>
		<div class="splashItem" id="my">
			<h2>My IOER</h2>
			<a target="_blank" href="/Account/Login.aspx" id="loginLink" runat="server" >Login</a>
      <a target="_blank" href="/Account/Register.aspx" id="registerLink" runat="server" >Register</a>
      <a target="_blank" href="/My/Dashboard" id="myDashboardLink" runat="server">My Dashboard</a>
			<!--<a target="_blank" href="/My/Library" id="myLibraryLink" runat="server" >My Library</a>
			<a target="_blank" href="/My/Favorites.aspx" id="myFollowedLink" runat="server" >Libraries I follow</a>
      <a target="_blank" href="/My/Timeline/" id="myNetworkLink" runat="server">My IOER Timeline</a>
			<a target="_blank" href="/My/Authored" id="myCreatedLink" runat="server" >My Resources</a>-->
			<img src="/images/icons/icon_library_med.png" />
		</div>
		
	</div>
