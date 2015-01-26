<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Widgets.ResourceSelector.Default" %>
<%@ Register Src="/Widgets/ResourceSelector/Selector1.ascx" TagPrefix="uc1" TagName="ResourceSelector" %>

<!DOCTYPE html>
<html>
<head runat="server">
  <title>IOER Resource Selector</title>
</head>
<body>
  <form id="form1" runat="server">
    <uc1:ResourceSelector id="selector" runat="server" />
  </form>
</body>
</html>
