<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Widgets.Curriculum.Default" %>
<%@ Register TagPrefix="uc1" TagName="Curriculum" Src="/Controls/Curriculum/CurriculumView1.ascx" %>

<!DOCTYPE html>

<html>
  <head runat="server">
    <title>IOER Curriculum Widget</title>
    <script src="//ajax.googleapis.com/ajax/libs/jquery/2.1.0/jquery.min.js" type="text/javascript"></script>
    <link href="/styles/isle.css" rel="stylesheet" type="text/css" />
    <link href="/styles/common2.css" rel="stylesheet" type="text/css" />
    <script src="/scripts/widgets/resizer.js" type="text/javascript"></script>

    <style type="text/css">
      body { padding: 5px; background-color: transparent; overflow-x: hidden; }
    </style>
  </head>
  <body>
    <form id="form1" runat="server">
      <uc1:Curriculum ID="CurriculumControl" runat="server" />
    </form>
  </body>
</html>
