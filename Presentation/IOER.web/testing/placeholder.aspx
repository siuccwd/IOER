<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="placeholder.aspx.cs" Inherits="IOER.testing.placeholder" MasterPageFile="~/Masters/Responsive.Master" %>
<%-- <%@ Register TagPrefix="uc1" TagName="TestSubject" Src="/Controls/Curriculum/Curriculum2.ascx" %>--%>
<%-- <%@ Register TagPrefix="uc1" TagName="TestSubject" Src="/Controls/SearchV5/SearchV5.ascx" %>--%>
<%--<%@ Register TagPrefix="uc1" TagName="TestSubject" Src="/Controls/curriculum/curriculumeditor2.ascx" %>--%>
<%-- <%@ Register TagPrefix="uc1" TagName="TestSubject" Src="/Controls/Admin/BigAdmin1.ascx" %>--%>
<%@ Register TagPrefix="uc1" TagName="TestSubject" Src="/Controls/Splash3.ascx" %>


<asp:Content ID="BodyContent" ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:TestSubject runat="server" id="TestController" Visible="true" />
</asp:Content>