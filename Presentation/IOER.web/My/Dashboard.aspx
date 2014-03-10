<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="ILPathways.My.Dashboard" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="Dashboard" Src="/controls/Dashboard2.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:Dashboard ID="DashboardControl" runat="server" />
</asp:Content>