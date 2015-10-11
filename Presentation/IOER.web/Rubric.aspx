<%@ Page Language="C#" Title="IOER Rubrics" AutoEventWireup="true" CodeBehind="Rubric.aspx.cs" Inherits="IOER.Rubric" MasterPageFile="/Masters/Previewer.Master" %>
<%@ Register TagPrefix="uc1" TagName="Rubric" Src="/Controls/Rubric4.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:Rubric ID="IOERRubric" runat="server" />
</asp:Content>