<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="WhatsNew.ascx.cs" Inherits="ILPathways.Controls.AppItems.WhatsNew" %>

<asp:Panel ID="detailPanel" runat="server" Visible="true">
	<div id="WhatsNewHeading" >
		<span id="headSpan" runat="server" ><asp:HyperLink ID="Archive" runat="server" style="text-decoration:none;">WHAT'S NEW</asp:HyperLink></span>
	</div>

	<div id="WhatsNewItems">
		<asp:Repeater ID="rptItems" runat="server">
			<ItemTemplate>

				<%# DataBinder.Eval(Container.DataItem,"param1") %>&nbsp;&nbsp;&nbsp;
				<a href='<%# DataBinder.Eval(Container.DataItem,"Url") %>'><%# DataBinder.Eval(Container.DataItem,"Title") %></a>
	      
				<br class="clearFloat" />
			</ItemTemplate>
		</asp:Repeater>
	<br />
	</div>
	<asp:Panel ID="SeeAllUpdates" runat="server" Visible="false">
		<asp:HyperLink ID="Archive1" runat="server">See all Updates</asp:HyperLink>
	</asp:Panel>
	<asp:Panel ID="Footer" runat="server">
	<div id="WhatsNewFooter">
		<span style="font-size:200%;">STAY CONNECTED:</span> 
			<a href="http://twitter.com/ILWorkNet/" title="Twitter" target="_blank"><img src="/vos_portal/STEM/images/twitter.jpg" alt="Twitter" /></a>
			<a href="http://www.facebook.com/illinois.worknet" title="Facebook" target="_blank"><img src="/vos_portal/STEM/images/facebook.jpg" alt="Facebook" /></a>
			<a href="/vos_portal/" title="Illinois workNet"><img src="/vos_portal/STEM/images/workNetIcon27x32.jpg" alt="Illinois workNet" /></a>
		</div>
	</asp:Panel>
</asp:Panel>

<asp:Panel ID="HiddenFields" runat="server" Visible="false">
  <asp:Literal ID="txtNewsItemCode" runat="server"></asp:Literal>
  <asp:Literal ID="wnHeadingText" runat="server"></asp:Literal>
  <asp:Literal ID="usingWebService" runat="server">yes</asp:Literal>

  <asp:Literal ID="dateTemplate" runat="server"><span style="margin-right: 10px;">{0}</span></asp:Literal>
  <asp:Literal ID="dateFormat" runat="server">MMM d, yyyy</asp:Literal>
  <asp:Literal ID="useNewsItemType" runat="server">yes</asp:Literal>
</asp:Panel>