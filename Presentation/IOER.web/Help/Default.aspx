<%@ Page Title="Illinois Open Educational Resources - Getting Started" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Help.Default" %>
<%@ Register TagPrefix="uc1" TagName="GuideControl" Src="/Help/HelpList.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:GuideControl id="guidance" runat="server" />
</asp:Content>