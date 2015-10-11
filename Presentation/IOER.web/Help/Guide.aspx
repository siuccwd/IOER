<%@ Page Title="Illinois Open Educational Resources - Guide" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Guide.aspx.cs" Inherits="IOER.Help.Guide" %>
<%@ Register TagPrefix="uc1" TagName="GuideControl" Src="/Help/HelpList.ascx" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:GuideControl id="guidance" runat="server" />
</asp:Content>