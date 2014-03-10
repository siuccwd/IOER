<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Rubric.aspx.cs" Inherits="ILPathways.Rubric" MasterPageFile="/Masters/Previewer.Master" %>
<%@ Register TagPrefix="uc1" TagName="Rubric" Src="/Controls/Rubric4.ascx" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <uc1:Rubric ID="IOERRubric" runat="server" />
</asp:Content>