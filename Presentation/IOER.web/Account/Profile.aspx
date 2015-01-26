<%@ Page Title="Profile" Language="C#" MasterPageFile="/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="ILPathways.Account.Profile" %>
<%@ Register Src="~/Account/controls/UserProfile2.ascx" TagPrefix="uc1" TagName="UserProfile" %>


<asp:Content ID="LoginContent" ContentPlaceHolderID="BodyContent" runat="server">
    <uc1:UserProfile runat="server" id="UserProfile2a" />
</asp:Content>