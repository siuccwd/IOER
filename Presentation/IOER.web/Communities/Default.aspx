<%@ Page Title="Illinois Open Educational Resources - Communities" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Communities.Default" %>
<%@ Register TagPrefix="uc1" TagName="CommunityHome" Src="/controls/Community/Home.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:CommunityHome id="community" runat="server" />
</asp:Content>
