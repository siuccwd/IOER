<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IT_Home2.aspx.cs" Inherits="ILPathways.Pages.IT_Home2" %>

<!DOCTYPE html>

<html>
  <head runat="server">
    <title>STEM IT Homepage</title>
    <style type="text/css">
    	html, body, form, #content { height: 100%; }
      body { padding: 0; margin: 0; min-width: 300px; }
      * { font-family: Calibri, Arial, Helvetica, sans-serif; box-sizing: border-box; -moz-box-sizing: border-box; white-space: normal; font-size: 16px; }
      iframe { border: 0; display: block; width: 100%; }
      #content { white-space: nowrap; font-size: 0; }
      #featured { width: 100%; height: 100px; text-align: center; padding-top: 40px; background-color: #CCC; }
      #libraryWidget { height: 100%; }
      #calendarWidget { height: 100%; }
      #locatorWidget { height: 50%; background-color: #CCC; padding-top: 20px; text-align: center; display: none; }
      .column { display: inline-block; vertical-align: top; height: calc(100% - 100px); }
      .column.left { width: 65%; }
      .column.right { width: 35%; }

      @media screen and (max-width: 950px) {
        .column, .column.left, .column.right { display: block; height: 600px; width: 100%; }
      }
    </style>
  </head>
  <body>
    <form id="form1" runat="server">
      
      <div id="content">
        <div id="featured">Featured Content Banner</div>
        <div class="column left">
          <iframe src="/Widgets/Library/?library=2&collections=124,2,84,33" id="libraryWidget"></iframe>
        </div>
        <div class="column right">
          <iframe src="http://localhost:38075/Public/Events/CalendarWidget1.aspx?calendarid=2&calendarlist=1,2&interval=20&publicsep=4AA394&fontFamily=Calibri,Arial,Helvetica,sans-serif" id="calendarWidget"></iframe>
          <div id="locatorWidget">Locator Widget</div>
        </div>
      </div>
    
    </form>
  </body>
</html>
