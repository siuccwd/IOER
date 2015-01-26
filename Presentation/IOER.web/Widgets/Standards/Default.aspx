<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Widgets.Standards.Default" %>
<%@ Register TagPrefix="uc1" TagName="browser" Src="/controls/StandardsBrowser7.ascx" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title></title>
    <link rel="Stylesheet" href="/Styles/ISLE.css" />
    <script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
    <style type="text/css">
      body { background-color: transparent; }
    </style>
    <script type="text/javascript">
      var SB7mode = "widget";
      var useSecureURL = <%=Request.Params[ "secure" ] == "true" ? "true" : "false" %>;
    </script>
</head>
<body>
    <uc1:browser ID="sBrowser" runat="server" />
</body>
</html>
