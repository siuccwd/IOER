<%@ Page Title="Illinois Open Educational Resources - Organizations" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Organizations.Default" %>

<%@ Register Src="~/Organizations/controls/OrganizationManagement.ascx" TagPrefix="uc1" TagName="OrganizationManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    <uc1:OrganizationManagement runat="server" id="orgManager" />
</asp:Content>
