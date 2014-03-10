<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Feed.Default" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Feed" Src="/Feed/Feed1.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:Feed id="FeedBox" runat="server" />
</asp:Content>