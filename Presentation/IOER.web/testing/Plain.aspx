<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Plain.aspx.cs" Inherits="ILPathways.testing.Plain" %>
<%@ Register TagName="TestSubject" TagPrefix="science" Src="/Controls/Curriculum/CurriculumView1.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common2.css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <div id="content">
    <science:TestSubject id="test" runat="server" />
  </div>
</asp:Content>
