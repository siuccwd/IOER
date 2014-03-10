<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PortalSplash.aspx.cs" Inherits="ILPathways.PortalSplash" %>
<%@ Register TagName="MiniSplash" TagPrefix="uc1" Src="/Controls/Splash2.ascx" %>
<%@ Register TagName="Console" TagPrefix="uc1" Src="/Controls/Includes/SystemMessageLine.ascx" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>IOER Home</title>
  <link href="/Styles/common.css" rel="stylesheet" />
  <link rel="Stylesheet" type="text/css" href="/Styles/ISLE.css" />
  <script src="//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js" type="text/javascript"></script>
  <style type="text/css">
    html { margin-top: -10px; }
  </style>
  <!--<style type="text/css">
    html, body { height: 100%; }
    body { 
      background-color: #4AA394;
      background: linear-gradient(#4AA394, #5AB3A4);
      background: -webkit-linear-gradient(#4AA394, #5AB3A4);
    }
    #splash .splashItem img { box-shadow: none; }
    #splash .splashItem:hover img { background-color: #3572B8; }
    #splash .splashItem a { box-shadow: none; }
    #splash .splashItem a:hover, .splashItem a:focus { box-shadow: 0 0 15px #3572B8; background-color: #3572B8; }
    #splash .splashItem h2 { background-color: #333; color: #FFF; box-shadow: none; font-size: 22px; }
  </style>-->
  <script type="text/javascript">
    $("document").ready(function () {
      setTimeout(function () { $("a").attr("target", "_blank"); }, 500);
    });
  </script>
</head>
<body>
  <form id="form1" runat="server">
    <div style="margin: 10px auto"><uc1:Console runat="server" ID="messageLine" /></div>
    <uc1:MiniSplash runat="server" id="MiniSplash" useNewWindow="true" />
    <style type="text/css">
      .contentBox #links { padding: 5px 5%; }
      .contentBox { margin-left: 0; }
      h1 { box-shadow: none; } 
    </style>
  </form>
</body>
</html>
