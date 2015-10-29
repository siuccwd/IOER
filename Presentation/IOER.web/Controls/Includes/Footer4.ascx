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
	#followLinks a { display: inline-block; vertical-align: middle; width: 30px; height: 30px; background-color: #3572B8; margin: 0 1px; transition: background-color 0.2s; border-radius: 5px; }
	#followLinks a:hover, #followLinks a:focus { background-color: #FF6A00; }
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
		<p id="followLinks">Follow IOER 
			<a href="//twitter.com/ILShared" target="_blank" style="background-image:url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20width%3D%2232%22%20height%3D%2232%22%20viewBox%3D%220%200%2032%2032%22%3E%3Cpath%20fill%3D%22%23fff%22%20d%3D%22M26.67%209.38c-.779.35-1.63.58-2.51.69.9-.54%201.6-1.4%201.92-2.42-.85.5-1.78.87-2.78%201.06-.8-.85-1.94-1.38-3.19-1.38-2.42%200-4.379%201.96-4.379%204.38%200%20.34.04.68.11%201-3.64-.18-6.86-1.93-9.02-4.57-.38.65-.59%201.4-.59%202.2%200%201.52.77%202.86%201.95%203.64-.72-.02-1.39-.22-1.98-.55v.06c0%202.12%201.51%203.89%203.51%204.29-.37.1-.75.149-1.15.149-.28%200-.56-.029-.82-.08.56%201.74%202.17%203%204.09%203.041-1.5%201.17-3.39%201.869-5.44%201.869-.35%200-.7-.02-1.04-.06%201.94%201.239%204.24%201.97%206.71%201.97%208.049%200%2012.45-6.67%2012.45-12.45l-.01-.57c.839-.619%201.579-1.389%202.169-2.269z%22%2F%3E%3C%2Fsvg%3E');" title="Follow IOER on Twitter"></a>
			<a href="//www.facebook.com/ILSharedLearning" target="_blank" style="background-image:url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20width%3D%2232%22%20height%3D%2232%22%20viewBox%3D%220%200%2032%2032%22%3E%3Cpath%20fill%3D%22%23fff%22%20d%3D%22M22.439%2010.95h4v-4.95h-4c-3.311%200-6%202.92-6%206.5v2.5h-4v4.97h4v12.03h5v-12.03h5v-4.97h-5v-2.55c0-.86.532-1.5%201-1.5z%22%2F%3E%3C%2Fsvg%3E');" title="Follow IOER on Facebook"></a>
		</p>
		<p>Copyright &copy; 2012 - <%=DateTime.Today.Year %> Illinois Department of Commerce and Economic Opportunity and Illinois State Board of Education</p>
		<p id="currentServer"><%=Environment.MachineName %> | <%=ILPathways.Utilities.UtilityManager.GetAppKeyValue( "siteVersion", "" ) %> </p>	
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
