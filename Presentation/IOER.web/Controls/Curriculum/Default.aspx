<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Controls.Curriculum.Default" MasterPageFile="/Masters/Responsive.Master" %>
<%@ Register TagPrefix="uc1" TagName="CurriculumControl" Src="/Controls/Curriculum/CurriculumView1.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
  <link rel="Stylesheet" type="text/css" href="/Styles/common2.css" />
</asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <div id="content">
    <uc1:CurriculumControl ID="Curriculum" runat="server" />
  </div>
</asp:Content>