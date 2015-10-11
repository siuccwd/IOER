<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Footer4.ascx.cs" Inherits="IOER.Controls.Includes.Footer4" %>

<style type="text/css">
	#mainSiteFooter { margin: 25px -5px 0 -5px; background-color: #4AA394; background-image: linear-gradient(#59E6E0, #4AA394); }
	#mainSiteFooterBanner { color: #FFF; text-align: center; padding: 25px 5px 10px 5px; }
	#mainSiteFooter .footerItem { display: inline-block; vertical-align: top; min-height: 225px; border-radius: 5px; padding: 0 5px 10px 5px; width: 30%; margin: 0 1.5%; background-color: #4AA394; background-image: linear-gradient(#49C6B0, #4AA394); box-shadow: 0 0 6px -1px #4AA394; }
	#mainSiteFooter .footerItem h3 { font-size: 20px; background-color: rgba(74, 163, 148, 0.7); margin: 0 -5px 5px -5px; border-radius: 5px 5px 0 0; padding: 5px; }
	#mainSiteFooter .footerItem a { color: #FFF; display: block; border-radius: 5px; padding: 2px 5px; font-size: 18px; transition: background 0.2s; }
	#mainSiteFooter .footerItem a:hover, #mainSiteFooter .footerItem a:focus { background-color: #50C7B1; background-color: rgba(89, 230, 224, 0.2); }
	#mainSiteFooterCopyright { text-align: center; padding: 5px; color: #FFF; }
	#currentServer { opacity: 0; transition: opacity 1s; }
	#currentServer:hover { opacity: 1; }
	@media (max-width: 650px) {
		#mainSiteFooter .footerItem { width: 90%; min-height: 0; margin-bottom: 10px; }
	}
</style>

<div id="mainSiteFooter">
	<h2 class="offScreen">IOER Site Footer</h2>
	<div id="mainSiteFooterBanner">
		<div class="footerItem">
			<h3>Information</h3>
			<a href="/help/guide.aspx">User Guide</a>
			<a href="/widgets">Widgets for your Site</a>
			<a href="/ioer_timeline">Timeline</a>
			<a href="/activity/stats.aspx">Activity &amp; Statistics</a>
			<a href="/Pages/NationalInitiatives.aspx">National Initiatives</a>
			<a href="//www2.illinoisworknet.com/Pages/Contact-Us.aspx" target="isleSite">Contact Us</a>

		</div><!--

		--><div class="footerItem">
			<h3>Policies</h3>
			<a href="/pages/privacypolicy.aspx">Privacy Policy</a>
			<a href="/pages/termsofuse.aspx">Terms of Use</a>
			<a href="/pages/Accessibility.aspx">Accessibility Statement</a>

		</div><!--

		--><div class="footerItem">
			<h3>Developers</h3>
			<a href="/developers/">Developer Documentation</a>

		</div>
	</div>
	<div id="mainSiteFooterCopyright">
		<p>Copyright &copy; 2012 - <%=DateTime.Today.Year %> Illinois Department of Commerce and Economic Opportunity and Illinois State Board of Education</p>
		<p id="currentServer"><%=ILPathways.Utilities.UtilityManager.GetAppKeyValue( "serverName", "" ) %> | <%=ILPathways.Utilities.UtilityManager.GetAppKeyValue( "siteVersion", "" ) %></p>	
	</div>
</div>

<script>
	(function (i, s, o, g, r, a, m) {
		i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
			(i[r].q = i[r].q || []).push(arguments)
		}, i[r].l = 1 * new Date(); a = s.createElement(o),
		m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
	})(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

	ga('create', 'UA-42065465-1', 'ilsharedlearning.org');
	ga('send', 'pageview');

</script>
