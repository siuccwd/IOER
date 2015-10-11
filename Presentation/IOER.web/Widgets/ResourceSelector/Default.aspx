<%@ Page Title="Illinois Open Educational Resources - Resource Widget" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Widgets.ResourceSelector.Default" %>
<%@ Register Src="/Widgets/ResourceSelector/Selector1.ascx" TagPrefix="uc1" TagName="ResourceSelector" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
  <title>IOER Resource Selector</title>
</head>
<body>
  <form id="form1" runat="server">
    <uc1:ResourceSelector id="selector" runat="server" />
  </form>
</body>
</html>
