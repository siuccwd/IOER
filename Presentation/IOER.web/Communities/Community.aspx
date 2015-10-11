<%@ Page Title="Illinois Open Educational Resources - Communities" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Community.aspx.cs" Inherits="IOER.Communities.Community" %>

<%@ Register TagPrefix="uc1" TagName="CommunityController" Src="/controls/Community/Community1.ascx" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:CommunityController id="community" runat="server" />
</asp:Content>
