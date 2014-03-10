<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Comments.ascx.cs" Inherits="ILPathways.LRW.controls.Comments" %>

<script type="text/javascript">
  $(document).ready(function () {
    loadComments("<%=commentsBox.ClientID %>");
  }); 

function postComment(targetElementID, commentTextSelector, button) {
  var commentText = $(commentTextSelector).val();
  queryWebDAL("PostComment", "{ 'resourceIntID' : '<%=currentResourceIntID %>', 'commentText' : '" + commentText + "', 'userID' : '<%=currentUserGUID %>' }", loadComments, targetElementID, true);
  $(".responseMessage").html("Processing Request. Please wait...");
  $(button).prop("disabled", true).css("opacity","0.5");
}

function loadComments(targetElementID, dataRaw) {
  queryWebDAL("GetComments", "{ 'resourceIntID' : '<%=currentResourceIntID %>' }", fillComments, targetElementID, true);
  if (dataRaw) {
    data = jQuery.parseJSON(dataRaw);
    $(".responseMessage").html(data.returnMessage);
  }
}

function fillComments(targetElementID, dataRaw) {
  data = jQuery.parseJSON(dataRaw);
  var targetElement = $("#" + targetElementID);
  //$(targetElement).html("");
  $(".commentsBox").html("");
  if (data.values == null) { return; }
  for (var i = 0; i < data.values.length; i++) {
    //$(targetElement).append(
    $(".commentsBox").prepend(
      $("#template_comment").html()
      .replace(/{id}/g, data.values[i])
      .replace(/{name}/g, data.texts[i])
      .replace(/{commentText}/g, data.descriptions[i])
      .replace(/{date}/g, data.dates[i])
    );
  }
  $(".txtAddComment").val("");
  $(".comments input.addComment").prop("disabled", false).css("opacity", "1");
}

//Communicate with the web service
function queryWebDAL(targetServerMethod, targetData, targetJFunction, targetHTMLContainer, isAsync) {
  $.ajax({
    type: "POST",
    contentType: "application/json; charset=utf-8",
    url: "/Services/WebDALService.asmx/" + targetServerMethod,
    data: targetData == "" ? {} : targetData,
    dataType: "json",
    async: isAsync,
    success: function (msg) {
      if (targetHTMLContainer == "") {
        targetJFunction(msg.d);
      }
      else {
        targetJFunction(targetHTMLContainer, msg.d);
      }
    },
    error: function (jqXHR, textStatus, errorThrown) {
      alert("error: " + jqXHR + " : " + textStatus + " : " + errorThrown);
    }
  });
}


</script>

<style type="text/css">
.addCommentBox {
  text-align: right;
}
.addCommentBox h3 {
  background-color: #FF5707;
  color: #FFF;
  border: none;
  margin-right: 55px;
  margin-top: 30px;
}
.addCommentBox img.headerImage {
  margin-top: -40px;
  margin-bottom: 10px;
}

</style>
<div class="comments">
  <div class="addCommentBox">
    <h3 class="isleH3_Block">Add a comment:</h3><img src="/images/icons/comments-small.png" class="headerImage" />
    <asp:Label ID="lblAddCommentMessage" runat="server"></asp:Label>
    <asp:UpdatePanel ID="commentsUpdatePanel" runat="server">
      <ContentTemplate>
        <asp:TextBox TextMode="MultiLine" MaxLength="5000" CssClass="txtAddComment" ID="txtComments" runat="server"></asp:TextBox>
        <asp:Label ID="commentMessage" runat="server" Visible="false"></asp:Label>
        <span class="responseMessage"></span>
        <asp:Button ID="btnAddComment" OnClick="btnAddComment_Click" OnClientClick="" Visible="false" runat="server" Text="Submit" CssClass="defaultButton addComment" />
      </ContentTemplate>
    </asp:UpdatePanel>
  </div>
  <div class="commentsBox" id="commentsBox" runat="server">
    <asp:Literal ID="ltlCommentDisplay" runat="server"></asp:literal>
  </div>
</div>
