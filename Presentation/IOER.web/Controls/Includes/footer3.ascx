<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="footer3.ascx.cs" Inherits="ILPathways.Controls.Includes.footer3" %>

<style type="text/css">
  #FooterSection, #FooterSection * { box-sizing: border-box; -moz-box-sizing: border-box; }
  #FooterDiv { background-color: #F5F5F5; padding: 5px; margin: 10px auto; text-align: center; border-radius: 5px; width: 100%; }
  #FooterDiv ul, .footerItem ul { list-style-type: none; }
  #CopyWriteDiv { font-size: 8px; text-align: center; color: #AAA; }
  #FooterDiv h3, #FooterDiv a { color: #888; }
  #FooterDiv a:hover, #FooterDiv a:focus { text-decoration: underline; }
  #FooterDiv .footerItem { display: inline-block; vertical-align: top; width: 23%;  } 
  #CopyWriteDiv .txtServer { display: none;  } 

  @media screen and (max-width: 500px) {
    #FooterDiv .footerItem { display: block; width: 100%; margin: 10px 5px; } 
    #CopyWriteDiv .txtServer { display: inline-block; margin: 5px 10px; } 
  }
</style>
<div id="FooterSection">
	<div id="FooterDiv">
    <div class="footerItem">
      <h3>
        Communication</h3>
      <ul>
        <li><a href="//ilsharedlearning.org/Pages/Contact-Us.aspx" target="isleSite">Contact Us & Media</a></li>
        <!--<li><a href="http://oerevaluationteam.weebly.com/" target="_blank">ISBE Open Education Resource Team</a></li>-->
        <li><a href="//ilsharedlearning.org/Pages/default.aspx" target="isleSite">Illinois Shared Learning Environment</a></li>
        <li><a href="//ioer.ilsharedlearning.org/Pages/NationalInitiatives.aspx" target="isleSite">National Initiatives</a></li>
				<!--<li><asp:HyperLink id="linkNewsSubscribe" navigateurl="/News/Subscribe.aspx" runat="server">Subscribe to IOER News</asp:HyperLink></li>-->
      </ul>
    </div>
    <div class="footerItem">
			<h3>Website Policy</h3>
      <ul>
        <li><a href="//ilsharedlearning.org/Pages/ISLE-Privacy-Policy.aspx" target="isleSite">Privacy Policy</a></li>
        <li><a href="//ilsharedlearning.org/Pages/ISLE-Terms-of-Use.aspx" target="isleSite">Terms of Use</a></li>
      </ul>
		</div>

		<div class="footerItem">
			<h3>Widgets</h3>
			<ul>
        <li><a href="/widgets">Configure IOER Widgets for your Site</a></li>
        <li><a href="/Pages/SamplePage.aspx">Widgets Sample Page</a></li>
        
			</ul>
		</div>
		<div class="footerItem">
			<h3>Developers</h3>
			<ul>
                <li><a href="http://ilsharedlearning.org/DevDoc/SitePages/Home.aspx" target="_blank">Developer Documentation</a></li>
        
			</ul>
		</div>
	</div>
	</div>
<div id="CopyWriteDiv">
	
	<p>Copyright &copy; 2012 - <%= thisYear %> Illinois Department of Commerce and Economic Opportunity</p>
	<p><asp:label ID="eo_Statement" runat="server"></asp:label></p>	
    <p><asp:label ID="txtServer" CssClass="txtServer" runat="server"></asp:label></p>	
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
