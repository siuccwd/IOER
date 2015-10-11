<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SessionHints.ascx.cs" Inherits="IOER.SessionHints" %>

<script type="text/javascript">
  var hints = <%=hintsJSON %>;
  var token = "<%=token %>";
  var hideHints = <%=hideHints %>;
</script>
<link rel="Stylesheet" type="text/css" href="/Styles/hint.css" />
<script src="/Scripts/hint.js" ></script>

