<%@ Page Title="Illinois Open Educational Resources - Rubrics" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Controls.Rubrics.Default" MasterPageFile="/Masters/Previewer2.Master" %>
<%@ Register TagPrefix="uc1" TagName="RubricCoreControl" Src="/Controls/Rubrics/RubricCore.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:RubricCoreControl id="RubricCore" runat="server" />
</asp:Content>