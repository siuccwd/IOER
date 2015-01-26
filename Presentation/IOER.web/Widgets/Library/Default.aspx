<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Widgets.Library.Default" %>
<%@ Register TagPrefix="uc1" TagName="WidgetLibrary1" Src="/Widgets/Library/Library1.ascx" %>

<!DOCTYPE HTML>
<html>
<head runat="server">
    <title>IOER Library</title>
</head>
<body>
    <form id="form1" runat="server">
      <uc1:WidgetLibrary1 ID="Library" runat="server" />
    </form>

    <script src="//ajax.googleapis.com/ajax/libs/jquery/2.1.0/jquery.min.js" type="text/javascript"></script>
    <script src="/scripts/widgets/resizer.js" type="text/javascript"></script>
</body>
</html>
