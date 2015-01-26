<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestSamplePage.aspx.cs" Inherits="ILPathways.Pages.TestSamplePage" %>

<!DOCTYPE html>

<html>
<head runat="server">
  <title>Sample Site Home Page</title>
  <script src="//ajax.googleapis.com/ajax/libs/jquery/2.1.0/jquery.min.js"></script>
  <script type="text/javascript">
      var multiplier = 0;
      $(document).ready(function () {
          $(window).on("resize", function () {
              resize();
          });
      });
      function slide(input) {
          multiplier = input;
          resize();
          $("body").removeClass("page1 page2 page3 page4 page5").addClass("page" + (input + 1));
      }
      function resize() {
          $("#page1").css("margin-left", $(window).outerWidth() * multiplier * -1);
      }
  </script>

  <style type="text/css">
    * { box-sizing: border-box; -moz-box-sizing: border-box; font-family: Calibri, Arial, Helvetica, sans-serif; }

    html, body, form { width: 100%; height: 100%; margin: 0; padding: 0; }
    #content { width: 100%; height: 100%; white-space: nowrap; overflow-x: hidden; }
    .page { width: 100%; height: 100%; display: inline-block; margin-right: -4px; transition: margin 1s; vertical-align: top; }
    .pageContent { padding: 10px; background-color: rgba(255,255,255,0.6); height: 90%; margin: 2% 50px 1% 200px; position: relative; transition: margin 0.5s; }

    /*#page1 { background: linear-gradient(#99B, #AAF); background: -webkit-linear-gradient(#99B, #AAF); }
    #page2 { background: linear-gradient(#9B9, #AFA); background: -webkit-linear-gradient(#9B9, #AFA); }
    #page3 { background: linear-gradient(#B99, #FAA); background: -webkit-linear-gradient(#B99, #FAA); }
    #page4 { background: linear-gradient(#BB9, #FFA); background: -webkit-linear-gradient(#BB9, #FFA); }
    #page5 { background: linear-gradient(#9BB, #AFF); background: -webkit-linear-gradient(#9BB, #AFF); }*/
    /*body.page1 { background: linear-gradient(#99B, #AAF); background: -webkit-linear-gradient(#99B, #AAF); }
    body.page2 { background: linear-gradient(#9B9, #AFA); background: -webkit-linear-gradient(#9B9, #AFA); }
    body.page3 { background: linear-gradient(#B99, #FAA); background: -webkit-linear-gradient(#B99, #FAA); }
    body.page4 { background: linear-gradient(#BB9, #FFA); background: -webkit-linear-gradient(#BB9, #FFA); }
    body.page5 { background: linear-gradient(#9BB, #AFF); background: -webkit-linear-gradient(#9BB, #AFF); }*/
    body.page1 { background-color: #AAF; }
    body.page2 { background-color: #AFA; }
    body.page3 { background-color: #FAA; }
    body.page4 { background-color: #FFA; }
    body.page5 { background-color: #AFF; }
    body { transition: background 1s; -webkit-transition: background 1s; }
    #content { background: linear-gradient(rgba(0,0,0,0.2), transparent); background: -webkit-linear-gradient(rgba(0,0,0,0.2), transparent); }

    #navbar { background-color: rgba(255,255,255,0.5); width: 100%; padding: 10px; position: absolute; top: 50%; transition: top 0.5s; }
    #navbar a { display: block; width: 200px; padding: 5px; }

    iframe { border: 0; width: 100%; height: 100%; }

    @media screen and (max-width: 700px) {
      .pageContent { margin: 175px 5px 5px 5px; height: 70%; }
      #navbar { top: 0; }
    }
  </style>

</head>
<body class="page1">
    <form id="form1" runat="server" onsubmit="return false;">
      <div id="content">
        
        <div id="navbar">
          <a href="#" onclick="slide(0);">Library</a>
          <a href="#" onclick="slide(1);">Calendar</a>
          <a href="#" onclick="slide(2);">Search</a>
          <a href="#" onclick="slide(3);">Standards Browser</a>
          <a href="#" onclick="slide(4);">Curriculum</a>
        </div>
        <div class="page" id="page1"><div class="pageContent"><iframe src="/Widgets/Library/?library=87&collections=272,271,267"></iframe></div></div>
        <div class="page" id="page2"><div class="pageContent"><iframe src="//testapps.il-work-net.com/Calendar/Public/Events/CalendarWidget1.aspx?eventseparator=4AA394&fontFamily=Calibri,Arial,Helvetica,sans-serif"></iframe></div></div>
        <div class="page" id="page3"><div class="pageContent"><iframe src="//ioer.ilsharedlearning.org/widgets/search/"></iframe></div></div>
        <div class="page" id="page4"><div class="pageContent"><iframe src="//ioer.ilsharedlearning.org/widgets/standards/"></iframe></div></div>
        <div class="page" id="page5"><div class="pageContent"><iframe src="//ioer.ilsharedlearning.org/widgets/curriculum?cidx=2197"></iframe></div></div>
      </div>
    </form>
</body>
</html>
