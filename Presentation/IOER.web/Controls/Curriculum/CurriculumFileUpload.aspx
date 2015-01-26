<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CurriculumFileUpload.aspx.cs" Inherits="ILPathways.Controls.Curriculum.CurriculumFileUpload" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
  <title></title>
  <script src="/Scripts/jquery-2.1.1.min.js"></script>
  <script type="text/javascript">
    $(document).ready(function () {
      var data = <%=dataJSON %>;
      if(data != null){
        window.parent.finishedUpload(data);
      }
      $("form").on("click", function() {
        window.parent.checkFileUploadBox();
      });
    });
  </script>
  <style type="text/css">
    body { background-color: transparent; padding: 0; margin: 0; font-family: Calibri, Arial, Helvetica, sans-serif; }
    *, input { font-size: 14px; }
    input { width: 100%; height: 100%; }
    html:hover, input:hover, input:focus { cursor: pointer; font-size: 14px; }
  </style>
</head>
<body>
    <form id="form1" runat="server">
      <asp:FileUpload ID="fileUpload" runat="server" />
      <input type="hidden" runat="server" id="hdnMetadata" class="hdnMetadata" />
    </form>
</body>
</html>
