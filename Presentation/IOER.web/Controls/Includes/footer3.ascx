<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="footer3.ascx.cs" Inherits="ILPathways.Controls.Includes.footer3" %>

<style type="text/css">
  #FooterSection, #FooterSection * { box-sizing: border-box; -moz-box-sizing: border-box; }
  #FooterDiv { background-color: #F5F5F5; padding: 5px; margin: 10px auto; text-align: center; border-radius: 5px; width: 100%; }
  #FooterDiv ul, .footerItem ul { list-style-type: none; }
  #CopyWriteDiv { font-size: 8px; text-align: center; color: #AAA; }
  #FooterDiv h3, #FooterDiv a { color: #888; }
  #FooterDiv a:hover, #FooterDiv a:focus { text-decoration: underline; }
  #FooterDiv .footerItem { display: inline-block; vertical-align: top; width: 30%;  } 

  @media screen and (max-width: 500px) {
    #FooterDiv .footerItem { display: block; width: 100%; margin: 10px 5px; } 
  }
</style>
<div id="FooterSection">
	<div id="FooterDiv">
    <div class="footerItem">
      <h3>
        Communication</h3>
      <ul>
        <li><a href="http://www.ilsharedlearning.org/Pages/Contact-Us.aspx">Contact Us & Media</a></li>
      </ul>
    </div>
    <div class="footerItem">
			<h3>Website Policy</h3>
      <ul>
        <li><a href="http://ilsharedlearning.org/Pages/ISLE-Privacy-Policy.aspx">Privacy Policy</a></li>
        <li><a href="http://ilsharedlearning.org/Pages/ISLE-Terms-of-Use.aspx">Terms of Use</a></li>
      </ul>
		</div>

		<div class="footerItem">
			<h3>Communications Center</h3>
			<ul>
				<li><asp:HyperLink id="linkNews" navigateurl="/News/Default.aspx" runat="server">IOER News</asp:HyperLink></li>
				<!--<li><asp:HyperLink id="linkNewsSubscribe" navigateurl="/News/Subscribe.aspx" runat="server">Subscribe to IOER News</asp:HyperLink></li>-->

			</ul>
		</div>
	</div>
	</div>
<div id="CopyWriteDiv">
	
	<p>Copyright &copy; 2012 - <%= thisYear %> Illinois Department of Commerce and Economic Opportunity</p>
	<p><asp:label ID="eo_Statement" runat="server"></asp:label></p>	
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
