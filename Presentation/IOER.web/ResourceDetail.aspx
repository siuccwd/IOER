<%@ Page Language="C#" Title="Illinois Open Educational Resource Detail" AutoEventWireup="true" CodeBehind="ResourceDetail.aspx.cs" Inherits="IOER.ResourceDetail" MasterPageFile="/Masters/Responsive.Master" validateRequest="false" %>
<%@ Register TagPrefix="uc1" TagName="LR_Detail" Src="/LRW/controls/Detail6.ascx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server">
    <meta name="description" content="#IOERResource" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:LR_Detail ID="LR_DetailPanel" runat="server"></uc1:LR_Detail>
  <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">IOER.Pages.ResourceDetail</asp:Literal>
</asp:Content>
