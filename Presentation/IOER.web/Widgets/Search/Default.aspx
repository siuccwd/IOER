<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Widgets.Search.Default" %>

<!DOCTYPE html>

<html>
<head runat="server">
  <title>IOER Search Widget</title>
  <script src="//ajax.googleapis.com/ajax/libs/jquery/2.1.0/jquery.min.js" type="text/javascript"></script>
  <link href="/styles/isle.css" rel="stylesheet" type="text/css" />
  <link href="/styles/common2.css" rel="stylesheet" type="text/css" />
  
  <script type="text/javascript">
    var useSecureURL = false;
    $(document).ready(function () {
      /*var params = window.location.search.replace("?", "").split("&");
      for (i in params) {
        var item = params[i].split("=");
        switch (item[0]) {
          case "button":
            if (item[1] == "no") { $("#btnSearch").remove(); $("#txtSearch").css("width", "100%"); }
            break;
          case "css":
            if (item[1].substring(item[1].length - 4) == ".css") {
              $("head").append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + item[1] + "\" />");
            }
          default:
            break;
        }
      }*/

      useSecureURL = <%=Request.Params[ "secure" ] == "true" ? "true" : "false" %>;
      var hideButton = <%=Request.Params[ "button" ] == "no" ? "true" : "false" %>;
      var css = "<%=Request.Params[ "css" ] %>";

      if(hideButton){ $("#btnSearch").remove(); }
      if(css.length > 5 && css.substring(css.length -4) == ".css"){
        $("head").append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + css + "\" />");
      }

      $("#txtSearch").on("keyup", function (event) {
        console.log("listening");
        if (event.which == 13 || event.keyCode == 13) {
          event.stopPropagation();
          doSearch();
          return false;
        }
      });
      $("#btnSearch").on("click", doSearch);
    });

    function doSearch() {
      window.open( (useSecureURL ? "https://ioer.ilsharedlearning.org/secure/IsleSSO.aspx?nextUrl=" : "") + "http://ioer.ilsharedlearning.org/search.aspx?text=" + $("#txtSearch").val(), "_blank");
    }
  </script>
  <style type="text/css">
    body { margin: 0; padding: 0; background-color: transparent; }
    form { white-space: nowrap; }
    #txtSearch { width: 75%; }
    #btnSearch { width: 25%; }
  </style>
</head>
<body>
  <form id="form1" runat="server" onsubmit="return false;">
    <input type="text" id="txtSearch" placeholder="Search..." /><input type="button" id="btnSearch" value="Search" />
  </form>
</body>
</html>
