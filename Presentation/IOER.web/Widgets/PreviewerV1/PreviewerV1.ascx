<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PreviewerV1.ascx.cs" Inherits="IOER.Controls.PreviewerV1.PreviewerV1" %>

<div id="rpMainContainer">
  <div id="resourcePreviewer">
    <link rel="stylesheet" type="text/css" href="/Widgets/PreviewerV1/previewerv1.css" />
    <script type="text/javascript" src="/Widgets/PreviewerV1/previewerv1.js"></script>
    <div id="rpToolBar">
      <div id="rpTitle"></div>
      <div id="rpInfo">
        <div id="rpPublisher"></div>
        <div id="rpCreated"></div>
      </div>
      <div id="rpToolButtons">
        <input type="button" class="rpToolButton" id="rpBtnLike" onclick="rp.toggleLikeBox();" title="I Like This!" />
        <input type="button" class="rpToolButton" id="rpBtnComments" onclick="rp.toggleCommentsBox();" title="Comments" />
        <input type="button" class="rpToolButton" id="rpBtnLibraries" onclick="rp.toggleLibrariesBox();" title="Add this to my default Library/Collection" />
        <input type="button" class="rpToolButton" id="rpBtnDetails" onclick="rp.goToDetailPage();" title="View Resource Details" />
        <input type="button" class="rpToolButton" id="rpBtnClose" value="X" onclick="rp.close();" title="Close" />
      </div>
    </div><!-- /rpToolBar -->
    <div id="rpPreviewFrame">
      <iframe id="rpFrame" sandbox="allow-same-origin allow-scripts" src=""></iframe>
    </div>
    <div id="rpCommentsBox">
      <h2>Comments</h2>
      <div id="rpCommentsList"></div>
    </div>
    <div id="rpTemplates" style="display:none;">
      <div id="rpTemplate_comment">
        <div class="rpComment">
          <div class="rpCommentAvatar" style="background-image:url('{avatarURL}');"></div>
          <div class="rpCommentName">{name}</div>
          <div class="rpCommentText">{text}</div>
          <div class="rpCommentDate">{date}</div>
        </div>
      </div>
    </div>
  </div>
</div>
<div id="rpServerSide">
  <script type="text/javascript" src="/scripts/jscommon.js"></script>
  <script type="text/javascript">
    var oerUserGuid = "<%=userGuid %>";
    var events = [
      { type: "addLike", method: "Paradata/AddLike/", success: successAddLike },
      { type: "addToLibrary", method: "Library/AddToDefaultLibCol/", success: successAddToLibrary },
      { type: "getComments", method: "Paradata/GetComments/", success: successGetComments },
      { type: "addComment", method: "Paradata/AddComment/", success: successAddComment }
    ];
    $(document).ready(function () {
      $(window).on("message", function (message) {
        try {
          var data = message.originalEvent.data;
          handleEvent(data);
        }
        catch (e) { }
      });
    });

    function handleEvent(data) {
      var event
      for (i in events) {
        console.log(data.type == events[i].type);
        if(data.type == events[i].type){
          event = events[i];
          console.log(event);
        }
      }

      data.data.userGuid = oerUserGuid;

      $.ajax({
        url: "http://localhost:44110/" + event.method + data.data.intID,
        data: data.data, 
        dataType: "jsonp",
        type: "GET",
        success: event.success
      });
    }
    function genericHandle(data, message) {
      if (data.valid) {
        alert(message);
      }
      else {
        alert(data.status);
      }
    }
    function successAddLike(data) {
      genericHandle(data, "Liked!");
    }
    function successAddToLibrary(data) {
      genericHandle(data, "Resource was added to your default Library's default Collection!");
    }
    function successGetComments(data) {
      console.log(data);
      if (data.valid) {
        renderComments(data.data, data.extra);
      }
      else {
        alert(data.status);
      }
    }
    function successAddComment(data) {
      genericHandle(data, "Posted!");
    }
  </script>
  <script type="text/javascript">
    /* Rendering Functions */
    function renderComments(comments, userCanPost) {
      var template = $("#rpTemplate_comment").html();
      var rawText = ""
      for (i in comments) {
        rawText += jsCommon.fillTemplate(template, comments[i]);
      }
      if (rawText == "") {
        rawText = "<p class=\"noComments\">No comments yet!</p>";
      }
      window.parent.postMessage({ action: "renderComments", data: rawText }, "*");
    }
  </script>
</div>