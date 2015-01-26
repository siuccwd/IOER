<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Curriculum.aspx.cs" Inherits="ILPathways.My.Curriculum" MasterPageFile="~/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="TestSubject" Src="/Controls/curriculum/curriculumeditor2.ascx" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:TestSubject runat="server" id="TestController" Visible="true" />
</asp:Content>