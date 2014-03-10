<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="placeholder.aspx.cs" Inherits="ILPathways.testing.placeholder" MasterPageFile="~/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="TestSubject" Src="/LRW/Controls/ESLibrary2.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:TestSubject runat="server" id="TestController" Visible="true" />
</asp:Content>