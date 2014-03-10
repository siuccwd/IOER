<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.testing.app.standardsbrowser.Default" %>
<%@ Register TagPrefix="uc1" TagName="browser" Src="/LRW/controls/StandardsBrowser5.ascx" %>

<!DOCTYPE html>
<html>
<head runat="server">
  <meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=no" />
  <title>IOER Standards Browser</title>
  <link rel="Stylesheet" href="/Styles/ISLE.css" />
  <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
  <style type="text/css">
    body { background-color: #EEE; }
  </style>
  <script type="text/javascript">
    $(document).ready(function () {
      setTimeout(function () { doResize(); }, 250);
      console.log($(window).width());
      //parent.communicate("test");  //Check this--only protocols need to match?
    });
  </script>
</head>
<body>
    <uc1:browser ID="sBrowser" runat="server" isWidget="true" mode="search" />
</body>
</html>
