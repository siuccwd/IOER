<%@ Page Title="Illinois Open Educational Resources - Index Refresher" Language="C#" AutoEventWireup="true" CodeBehind="IndexRefresher.aspx.cs" Inherits="IOER.Admin.IndexRefresher" MasterPageFile="/Masters/Responsive.Master" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">

  <link rel="stylesheet" href="/styles/common2.css" type="text/css" />
  <script type="text/javascript">
    function doSubmit() {
      $("form").removeAttr("onsubmit");
      $("#btnSubmit, #btnDelete").attr("value", "Working...").attr("disabled", "disabled");
      $("form")[0].submit();
    }

    function doDelete() {
    	if (confirm("Warning! You are about to delete these resources from the index. Are you sure?")) {
    		$(".hdnDelete").val("true");
    		doSubmit();
    	}
    }
  </script>
  <style type="text/css">
    #content { padding-left: 50px; }
    #content:after { display: block; content: " "; clear: both; }
    input[type=submit] { max-width: 300px; margin: 5px 50px 5px 5px; float: right; }
    .txtIDs { width: 100%; height: 12em; resize: vertical; }
  </style>
  <div id="content">
    <h1 class="isleH1">ElasticSearch Index Resource Refresher</h1>

    <p>Comma-separated list of IDs to Refresh:</p>
    <textarea id="txtIDs" class="txtIDs" runat="server"></textarea>
    <input id="btnSubmit" class="isleButton bgGreen" type="submit" value="Refresh" onclick="doSubmit()" />
		<input id="btnDelete" class="isleButton bgRed" type="submit" value="Delete" onclick="doDelete()" />
		<input type="hidden" id="hdnDelete" class="hdnDelete" runat="server" />
  </div>

</asp:Content>