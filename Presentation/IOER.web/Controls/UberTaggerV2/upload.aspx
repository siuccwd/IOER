<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="upload.aspx.cs" Inherits="IOER.Controls.UberTaggerV2.upload" MasterPageFile="/Masters/Plain.Master" %>

<asp:Content ContentPlaceHolderID="BodyContent" runat="server">

  <script type="text/javascript">
    var loadMessage = <%=LoadMessage %>;

    //When the page loads...
    $(document).ready(function () {
      window.parent.postMessage(JSON.stringify(loadMessage), "*");
    });

    //When the upload command is received...
    $(window).on("message", function (msg) {
      var message = $.parseJSON( msg.originalEvent.data );
      console.log("Message", message);
      //Flatten
      message.info.command = message.command;
      switch (message.command) {
        case "upload":
          doUpload(message.info);
          break;
        case "remove":
          doRemove(message.info);
      }
    });

    function doUpload(info) {
      console.log(info);
      $(".hdnData").val(JSON.stringify(info));
      $("form").submit();
    }

    function doRemove(info) {
      $(".hdnData").val(JSON.stringify(info));
      $("form").submit();
    }
  </script>

  <style type="text/css">
    body { overflow: hidden; height: 100%; width: 100%; padding: 0; margin: 0; }
    .fileUpload { width: 100%; height: 100%; }
  </style>
  
  <div id="uploadBox" runat="server">
    <asp:FileUpload ID="fileUpload" CssClass="fileUpload" runat="server" />
    <input type="hidden" id="hdnData" class="hdnData" runat="server" />
  </div>
  <div id="messageBox" runat="server">

  </div>

</asp:Content>