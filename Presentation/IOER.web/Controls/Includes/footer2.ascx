<%@ Control Language="C#" AutoEventWireup="true" CodeFile="footer2.ascx.cs" Inherits="ILPathways.Includes.footer2" %>

<div id="FooterSection">
    <div id="pathwaysBottomGraphic">
        <img src="http://illinoisworknet.com/vos_portal/STEM/images/body_FootBottomRight-corner2.jpg" style="float:right;" alt="" />
        <img src="http://illinoisworknet.com/vos_portal/STEM/images/body_FootBottomLeft-corner2.jpg" style="float:left; margin-left:-1px; margin-top:-1px;" alt="" />
    <br class="clearFloat" />
    </div>

	<div id="FooterDiv">
		<div class="footerItem">
			<h3>About This Site</h3>
			<ul>
				<li><asp:HyperLink id="linkSiteMap" navigateurl="http://www.illinoisworknet.com/vos_portal/STEM/en/admin/SiteMap/SiteMap.htm" runat="server">Site Map</asp:HyperLink></li>
			</ul>
		</div>
		<div class="footerItem">
			<h3>Website Policies</h3>
			<ul>
				<li><asp:HyperLink id="linkPrivacyPolicy" navigateurl="http://www.illinoisworknet.com/vos_portal/residents/en/admin/privacyPolicy" runat="server">Privacy Policy</asp:HyperLink></li>
				<li><asp:HyperLink id="linkTermsConditions" navigateurl="http://www.illinoisworknet.com/vos_portal/residents/en/admin/terms/termsconditions.htm" runat="server">Terms of Use</asp:HyperLink></li>
				<li><asp:HyperLink id="linkModerationPolicy" navigateurl="http://www.illinoisworknet.com/vos_portal/residents/en/admin/ModerationPolicy/" runat="server">Moderation Policy</asp:HyperLink></li>
			</ul>
		</div>
		<div class="footerItem">
			<h3>Partners</h3>
			<ul>
				<li><asp:HyperLink id="linkLearningExchangeInfo" navigateurl="http://www.illinoisworknet.com/vos_portal/STEM/en/LearningExchanges/learnExchgInfo" runat="server">STEM Learning Exchange Information</asp:HyperLink></li>
				<li><asp:HyperLink id="linkLearningResourceSearch" navigateUrl="http://209.7.195.215:90/LRW/Pages/LRSearch.aspx" runat="server">Learning Registry Pre-Alpha Resource Search</asp:HyperLink></li>
			</ul>
		</div>
		<div class="footerItem">
			<h3>Communications Center</h3>
			<ul>
				<li><asp:HyperLink id="linkNews" navigateurl="http://www.illinoisworknet.com/vos_portal/Stem/en/News" runat="server">llinois Pathways News</asp:HyperLink></li>
				<li><asp:HyperLink id="linkNewsSubscribe" navigateurl="http://www.illinoisworknet.com/vos_portal/STEM/en/News/" runat="server">Subscribe to Illinois Pathways News</asp:HyperLink></li>
				<li><asp:HyperLink id="linkFacebook" navigateurl="http://www.facebook.com/illinois.worknet" Target="_blank" runat="server">Facebook</asp:HyperLink></li>
				<li><asp:HyperLink id="linkYoutube" navigateurl="http://www.youtube.com/illinoisworknet" Target="_blank" runat="server">Youtube</asp:HyperLink></li>
				<li><asp:HyperLink id="linkTwitter" navigateurl="http://twitter.com/ILworknet" Target="_blank" runat="server">Twitter</asp:HyperLink></li>
				<li><asp:HyperLink id="linkContactUs" navigateurl="http://www.illinoisworknet.com/vos_portal/Stem/en/About/page_ContactUs.htm" Target="_blank" runat="server">Contact Us</asp:HyperLink></li>
			</ul>
		</div>
	</div>
	</div>
<div id="CopyWriteDiv">
	
	<p>Copyright &copy; 2006 - <%= thisYear %> Illinois Department of Commerce and Economic Opportunity</p>
	<p><asp:label ID="eo_Statement" runat="server"></asp:label></p>	
</div>
