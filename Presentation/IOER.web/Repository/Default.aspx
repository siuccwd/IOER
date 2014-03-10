<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Content.Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<h1>Authoring Content</h1>
<div class="isleMainSection">
<p>
<asp:Label ID="notAuthenticatedMesssage" runat="server" Visible="false" >You must sign in and be authorized in order to author new resources.</asp:Label>
<asp:Label ID="noContentMesssage" runat="server" Visible="false" >You are not authorized to author new content at this time.</asp:Label>
<asp:Panel ID="authoringPanel" runat="server" Visible="false">
  <a href="/My/Author.aspx">Add new content</a>
  </asp:Panel>
</p>

</div>

<asp:Literal ID="txtAuthorSecurityName" runat="server" Visible="false">ILPathways.LRW.controls.Authoring</asp:Literal>
</asp:Content>
