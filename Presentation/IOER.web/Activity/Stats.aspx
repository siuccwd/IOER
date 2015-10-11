<%@ Page Title="Illinois Open Educational Resources - Site Statistics" Language="C#" AutoEventWireup="true" CodeBehind="Stats.aspx.cs" Inherits="IOER.Activity.Stats"  MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Stats" Src="/Activity/Stats2.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:Stats id="StatsList" runat="server" />
</asp:Content>
