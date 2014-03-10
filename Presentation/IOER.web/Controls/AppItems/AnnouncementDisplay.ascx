<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AnnouncementDisplay.ascx.cs" Inherits="ILPathways.Controls.AppItems.AnnouncementDisplay" %>

<asp:panel id="detailsPanel" runat="server" visible="false">
<div class="section">

  <div><asp:HyperLink ID="lnkSubscribe" runat="server" Visible="false" Text="Subscribe" /></div>
	<div><asp:Literal ID="ReadAnotherItem1" runat="server"></asp:Literal></div>
	<div style="margin: 10px 0px;"><%= txtDescr1 %></div>
	<asp:Panel ID="History" runat="server" Visible="false">
	  Published: <%= published %>  Updated: <%= updated %>
	</asp:Panel>
	
	<p style="margin-top: 25px;">
	<asp:HyperLink ID="lblDocumentLink" runat="server" Target="_blank" Visible="false">View File</asp:HyperLink>
	</p>
	<div><asp:Literal ID="ReadAnotherItem2" runat="server"></asp:Literal></div>
</div>
</asp:panel>	

<asp:Panel ID="HiddenFields" runat="server" Visible="false">
  <asp:Literal ID="txtNewsItemCode" runat="server"></asp:Literal>
  <asp:Literal ID="usingWebService" runat="server">yes</asp:Literal>

  <asp:Literal ID="dateTemplate" runat="server"><span style="margin-right: 10px;">{0}</span></asp:Literal>
  <asp:Literal ID="dateFormat" runat="server">MMM d, yyyy</asp:Literal>
</asp:Panel>