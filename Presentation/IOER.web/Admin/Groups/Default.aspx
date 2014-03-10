<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Pathway.Master" Async="true"  AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ILPathways.Admin.Groups.Default" %>
<%@ Register TagPrefix="uc1" TagName="groupMgr" Src="~/Controls/GroupsManagment/GroupsManager.ascx" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
 <link rel="stylesheet" href="http://code.jquery.com/ui/1.10.3/themes/smoothness/jquery-ui.css" />
  <script src="http://code.jquery.com/jquery-1.9.1.js"></script>
  <script src="http://code.jquery.com/ui/1.10.3/jquery-ui.js"></script>
  <script src="/Scripts/jquery.cookie.js"></script>
   <script type="text/javascript">
   $(document).ready(function () {
//     $("#tabs").tabs({ active: document.tabTest.currentTab.value });

     $(function () {
       var tabs = $("#tabs").tabs();
       tabs.find(".ui-tabs-nav").sortable({
         axis: "x",
         stop: function () {
           tabs.tabs("refresh");
         }
       });
     });

     $("#tabs").tabs({
       activate: function (event, ui) {
         $.cookie("tabs_selected", $("#tabs").tabs("option", "active"));
       },
       active: $("#tabs").tabs({ active: $.cookie("tabs_selected") })
     });
   });
</script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<uc1:groupMgr id="groupMgr1" runat="server" UsingDefaultGroupCode="false"></uc1:groupMgr>

</asp:Content>
