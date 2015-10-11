<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="footer3.ascx.cs" Inherits="IOER.Controls.Includes.footer3" %>

<style type="text/css">
  #FooterSection, #FooterSection * { box-sizing: border-box; -moz-box-sizing: border-box; }
	#FooterSection { padding: 0 10px; }
  #FooterDiv { border: solid 1px #4F4E4F; margin: 10px auto; padding-bottom: 20px; text-align: center; width: 100%; border-radius: 5px; }
  #FooterDiv ul, .footerItem ul { list-style-type: none; margin: 0; padding: 0; }
  #CopyWriteDiv { font-size: 12px; text-align: center; color: #000; margin-bottom: 50px; }
  #FooterDiv h3 { font-size: 20px; background-color: #4F4E4F; color:#FFF; }
  #FooterDiv a { font-size: 16px; color:#000 }
  #FooterDiv a:hover, #FooterDiv a:focus { text-decoration: underline; }
  #FooterDiv .footerItem { display: inline-block; vertical-align: top; width: 33.333%;  } 
  #CopyWriteDiv .txtServer { display: none; } 

  @media screen and (max-width: 600px) {
		#FooterDiv { padding-bottom: 5px; }
    #FooterDiv .footerItem { display: block; width: 100%; margin: 0 0 15px 0; } 
  }
</style>
<div id="FooterSection">
    <div id="FooterDiv">
        <h2 class="offScreen">footer</h2>
				<div class="footerItem">
            <h3 style="margin-left:0;">Information</h3>
            <ul>
				<li><a href="/help/guide.aspx">User Guide</a></li>
                <li><a href="/widgets">Configure IOER Widgets for your Site</a></li>
				<li><a href="/ioer_timeline">Timeline</a></li>
				<li><a href="/activity/stats.aspx">Activity &amp; Statistics</a></li>
                <li><a href="/Pages/NationalInitiatives.aspx">National Initiatives</a></li>
                <li><a href="//www2.illinoisworknet.com/Pages/Contact-Us.aspx" target="isleSite">Contact Us</a></li>
            </ul>
        </div><!--
        --><div class="footerItem">
            <h3>Policies</h3>
            <ul>
                <li><a href="/pages/privacypolicy.aspx">Privacy Policy</a></li>
                <li><a href="/pages/termsofuse.aspx">Terms of Use</a></li>
                <li><a href="/pages/Accessibility.aspx">Accessibility Statement</a></li>
            </ul>
        </div><!--
        --><div class="footerItem">
            <h3>Developers</h3>
            <ul>
                <li><a href="/developers/">Developer Documentation</a></li>
            </ul>
        </div>
    </div>
</div>
<div id="CopyWriteDiv">
	<p>Copyright &copy; 2012 - <%= thisYear %> Illinois Department of Commerce and Economic Opportunity and Illinois State Board of Education</p>
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
