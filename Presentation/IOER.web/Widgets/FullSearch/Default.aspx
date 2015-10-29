<%@ Page Title="Illinois Open Educational Resources - Search Widget" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IOER.Widgets.FullSearch.Default" MasterPageFile="/Masters/Plain.Master" %>
<%@ Register TagPrefix="uc1" TagName="Search" Src="/Controls/SearchV7/SearchV7.ascx" %>

<asp:Content ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>
<asp:Content ContentPlaceHolderID="BodyContent" runat="server">
  <style type="text/css">
  <% if(Request.Params["scrollbars"] == "false") { %>
    body { overflow: hidden; }
  <% } %>
		#content { padding: 0; }
  </style>
  <uc1:Search ID="search" runat="server" />
  <script type="text/javascript">
    //Preselect tags passed in via parent URL
    $(document).ready(function () {
      $(window).on("message", function (msg) {
        try {
          var message = JSON.parse(msg.originalEvent.data);
          if (message.action == "handleQueryString") {
            var tags = message.queryString.tagIDs.split(",");
            $(window).trigger("selectTags", { tags: tags });
          }
        }
        catch (e) { }
      });
    });
    //Request tags
    window.parent.postMessage(JSON.stringify({ action: "getQueryString" }), "*");
  </script>
	<asp:Literal ID="defaultTheme" runat="server" Visible="false">ioer_library</asp:Literal>
</asp:Content>