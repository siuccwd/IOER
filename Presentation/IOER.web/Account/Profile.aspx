<%@ Page Title="Illinois Open Educational Resources - Profile" Language="C#" MasterPageFile="/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="IOER.Account.Profile" %>
<%@ Register Src="~/Account/controls/UserProfile2.ascx" TagPrefix="uc1" TagName="UserProfile" %>


<asp:Content ID="LoginContent" ContentPlaceHolderID="BodyContent" runat="server">
    <uc1:UserProfile runat="server" id="UserProfile2a" />
</asp:Content>