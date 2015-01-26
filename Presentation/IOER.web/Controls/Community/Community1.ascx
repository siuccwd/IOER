<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Community1.ascx.cs" Inherits="ILPathways.Controls.Community.Community1" %>

<script type="text/javascript">
  //From Server
  <%=communityData %>
  <%=userGUID %>
  <%=minimumLength %>
  <%=userID %>
  <%=userIsAdmin %>
</script>
<script type="text/javascript">
  //Page Initialization
  $(document).ready(function () {
    //Load the community data
    renderCommunity(communityData);

    //Load the postings
    renderPostings(communityData.postings);

    //Replies
    setupReplyBoxes();
  });

  function setupReplyBoxes() {
    $(".postTextBox").each(function() {
      setupBox($(this));
    });
  }

  function setupBox(box){
    box.unbind().on("keydown", function(event) {
      if(event.which == 13 || event.keyCode == 13){
        event.preventDefault();
      }
    }).on("keyup change", function() {
      var box = $(this);
      var val = box.val();
      var status = box.parent().find(".replyStatus");
      if(val.length < minimumLength){
        status.html("Please enter " + (minimumLength - val.length) + " more characters.");
      }
      else {
        status.html("");
      }
    });
  }

  //Rendering Functions
  function renderCommunity(data) {
    $("#communityTitle").html(data.title);
    $("#communityDescription").html(data.description);
    $("#communityAvatar").css("background-image", "url('" + data.image + "')");
    
    if(userGUID == ""){
      $("#btnFollow").remove();
    }
    else {
      if(communityData.userIsFollowing){
        $("#btnFollow").attr("value", "Unfollow");
      }
      else {
        $("#btnFollow").attr("value", "Follow");
      }
    }
  }

  function renderPostings(data) {
    var list = $("#communityPostingList");
    var template = $("#template_posting").html();
    var dateSeparator = $("#template_dateSeparator").html();
    var currentDate = "";
    list.html("");

    for (i in data) { //For each posting
      var current = data[i];

      //Add a date separator if needed
      if (current.created != currentDate) {
        list.append(dateSeparator.replace(/{date}/g, current.created));
        currentDate = current.created;
      }

      //Add the posting the the list
      list.append(
        buildPost(template, current)
      );

      //Render responses to this posting
      for (j in current.responses) {
        console.log("adding response");
        console.log(current.responses[j]);
        $(".replies[data-parentID=" + current.id + "]").append(
          buildPost(template, current.responses[j])
        );
        console.log($(".replies[data-parentID=" + current.id + "]").length);
      }
    }

    //Remove the unneeded replies div from responses
    $(".replies .replies").remove();
    $(".replies .replyBox").remove();
    if (userGUID == "") {
      $(".replyBox").remove();
    }

    $("#communityPostingList .posting.locked").each(function() {
      $(this).find(".replyBox").remove();
      $(this).append("<div class=\"lockMessage\">Locked</div>");
    });

    $("#communityPostingList .posting").each(function() {
      var box = $(this);
      if(box.find(".replyLink").length == 0 || box.find(".lockLink").length == 0){
        box.find(".delimiter").remove();
      }
    });
  }

  function buildPost(template, data) {
    var output = template
      .replace(/{postingID}/g, data.id)
      .replace(/{parentID}/g, data.parentID)
      .replace(/{name}/g, data.poster)
      .replace(/{userid}/g, data.createdByID)
      .replace(/{date}/g, data.created)
      .replace(/{avatar}/g, data.posterAvatar)
      .replace(/{text}/g, data.text);

    if(data.createdByID == userID || userIsAdmin){
      output = output.replace(/{deleteLink}/g, $("#template_deleteLink").html().replace(/{postingID}/g, data.id));
    }
    else {
      output = output.replace(/{deleteLink}/g, "");
    }

    return output;
  }
</script>
<script>
  //Page functions
  function postMessage(id, button) {
    var val = $(".postTextBox[data-id=" + id + "]").val();
    if (val.length > 0) {
      doAjax("PostMessage", { userGUID: userGUID, communityID: communityData.id, parentID: id, text: val }, successPostMessage, $(button), id);
    }
    else {
      $("[data-replyStatusID=" + id + "]").html("You cannot post an empty message!");
    }
  }

  function openReplyBox(id) {
    var box = $(".replyBox[data-postingID=" + id + "]");
    var template = $("#template_replyControls").html();
    box.addClass("hasControls");
    box.html("");
    box.append(
      template.replace(/{id}/g, id)
    );
    setupBox(box.find(".replyTextBox"));
  }

  function deletePost(id){
    $(".posting[data-postingID=" + id + "], .replies[data-parentID=" + id + "] .posting").addClass("preDelete");
    if(confirm("Are you sure you want to delete this post and any replies to it? This action cannot be undone.")){
      //Delete the post(s)
      doAjax("DeletePost", { userGUID: userGUID, postID: id, communityID: communityData.id }, successDeletePost, null); 
    }
    else {
      $(".posting[data-postingID=" + id + "], .replies[data-parentID=" + id + "] .posting").removeClass("preDelete");
    }
  }

  function getPost(id){
    for(i in communityData.postings){
      if(communityData.postings[i].id == id){
        return communityData.postings[i];
      }
      for(j in communityData.postings[i].responses){
        if(communityData.postings[i].responses[j].id == id){
          return communityData.postings[i].responses[j];
        }
      }
    }
  }

  function lock(id){
    if(confirm("Are you sure you want to lock this thread?")){
      doAjax("LockThread", { userGUID: userGUID, postID: id, communityID: communityData.id }, successLockThread, null);
    }
  }

  function toggleFollowing() {
    doAjax("ToggleFollowing", { userGUID: userGUID, communityID: communityData.id }, successToggleFollowing, null);
  }
</script>
<script>
  //AJAX stuff
  function doAjax(method, data, success, button, replyStatusID) {
    disableButton(button);
    $.ajax({
      url: "/Services/CommunityService.asmx/" + method,
      async: true,
      success: function (msg) {
        try {
          success($.parseJSON(msg.d), replyStatusID);
        }
        catch (e) {
          success(msg.d, replyStatusID);
        }
        enableButton(button);
      },
      type: "POST",
      data: JSON.stringify(data),
      dataType: "json",
      contentType: "application/json; charset=utf-8"
    });
  }

  function disableButton(button) {
    if(button == null){ return; }
    button.attr("originalVal", button.attr("value"));
    button.attr("value", "...");
    button.attr("disabled", "disabled");
  }
  function enableButton(button) {
    if(button == null){ return; }
    button.attr("value", button.attr("originalVal"));
    button.removeAttr("disabled");
  }

  function successPostMessage(data, replyStatusID) {
    if (data.isValid) {
      communityData = data.data;
      $("[data-replyStatusID=" + replyStatusID + "]").html("Posted!");
      renderCommunity(communityData);
      renderPostings(communityData.postings);
      $(".postTextBox[data-id=" + replyStatusID + "]").val("");
    }
    else {
      $("[data-replyStatusID=" + replyStatusID + "]").html(data.status);
    }
  }

  function successDeletePost(data){
    if(data.isValid){
      communityData = data.data;
      renderCommunity(communityData);
      renderPostings(communityData.postings);
      alert("Post(s) deleted.");
    }
    else {
      alert(data.status);
    }
  }

  function successLockThread(data){
    if(data.isValid){
      communityData = data.data;
      renderCommunity(communityData);
      renderPostings(communityData.postings);
      alert("Thread locked.");
    }
    else {
      alert(data.status);
    }
  }

  function successToggleFollowing(data){
    if(data.isValid){
      communityData.userIsFollowing = data.data;
      renderCommunity(communityData);
      if(data.data){
        alert("You are now following this Community.");
      }
      else {
        alert("You are no longer following this Community.");
      }
    }
    else {
      alert(data.status);
    }
  }
</script>

<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<style type="text/css">
  #communityPostMaker, #communityHeader, #communityTitle, #communityDescription { transition: margin 0.8s, padding 0.8s; -webkit-transition: margin 0.8s, padding 0.8s; }
  #communityPostMaker { max-width: 600px; margin: 5px auto -50px auto;  }
  #postBox { width: 100%; resize: none; height: 4em; }
  #mainReplyBox { text-align: right; }
  #btnPostMessage { width: 75px; }
  #headerContainer { margin: 0px -5px 50px -55px; padding-left: 50px; background-color: #4AA394; background: linear-gradient(#4AA394, #5AB3A4); background: -webkit-linear-gradient(#4AA394, #5AB3A4); }
  #communityHeader { min-height: 160px; position: relative; margin: 0 auto; padding: 10px;  }
  #communityHeader.login { margin-bottom: 10px; }
  #communityTitle { margin: -5px -5px 10px -5px; box-shadow: none; text-align: center; padding: 3px 160px; }
  #communityDescription { font-size: 20px; text-align: center; padding: 5px 150px; color: #FFF; }
  #communityAvatar { width: 150px; height: 150px; position: absolute; top: 5px; left: 5px; border-radius: 5px; background: url('') center center #999; background-size: cover; border: 1px solid #333; transition: width 0.8s, left 0.8s, height 0.8s; -webkit-transition: width 0.8s, left 0.8s, height 0.8s; }
  #communityPostMaker h2 { color: #4AA394; font-size: 20px; margin-top: -10px; padding: 5px; }
    #communityPostingList, #communitiesList { display: inline-block; vertical-align: top; }
    #communitiesList { width: 300px; margin: 0 50px 0 100px; min-height: 300px; }
  .posting, .replies, .dateSeparator { margin: 5px auto; max-width: 600px; }
  .dateSeparator { text-align: center; padding: 2px 5px; max-width: 200px; margin-bottom: 30px; font-weight: bold; }
  .replies { margin-bottom: 20px; padding-left: 50px; }
  .posting { position: relative; }
  .posting p { padding-left: 55px; }
  .posting .date { position: absolute; top: 5px; right: 5px; color: #333; font-size: 12px; }
  .grayBox .name { font-weight: bold; color: #4AA394; margin: -5px -5px 10px -5px; border-bottom: 1px solid #4AA394; font-size: 20px; padding-left: 60px; }
  .posting.locked { opacity: 0.8; }

  .lockMessage { text-align: right; font-style: italic; }
  
  .replyBox { position: relative; text-align: right; color: #999; }
  .replyBox.hasControls { padding-right: 80px; }
  .replyButton { position: absolute; top: 0; right: 0; width: 75px; font-weight: normal; font-size: 16px; line-height: 16px;  }
  .replyTextBox { width: 100%; height: 24px; }
  .replyStatus { padding: 2px 5px; text-align: right; font-style: italic; }

  .deleteLink { display: inline-block; background-color: #B03D25; color: #FFF; font-weight: bold; border-radius: 5px; height: 18px; width: 18px; line-height: 18px; font-size: 18px; text-align: center; opacity: 0.2; }
  .deleteLink:hover, .deleteLink:focus { background-color: #F00; color: #FFF; opacity: 1; }
  .preDelete { box-shadow: 0 0 50px -10px #F00 inset; }
  
  .middle { text-align: center; font-style: italic; color: #555; }
  .avatar { position: absolute; left: 5px; top: 5px; width: 50px; height: 50px; background: url('') no-repeat center center; background-size: cover; border-radius: 5px; border: 1px solid #4AA394; background-color: #EFEFEF; }

  #btnFollow { position: absolute; width: 90%; top: 135px; left: 5%; min-width: 80px; transition: left 0.8s, top 0.8s;  }

  @media screen and (max-width: 1044px) {
      #communitiesList { width: 400px;  min-height: 50px; }
  }
  @media screen and (max-width: 650px) {
    #communityTitle, #communityDescription { padding-right: 5px; padding-left: 85px; }
    #communityAvatar { width: 75px; height: 75px; }
    #btnFollow { top: 65px; left: -3px; }
    #communitiesList { margin-left: 10px; min-height: 50px; }
  }
</style>


<div id="content">
  <div id="headerContainer">
    <div id="communityHeader" class="<%=loginClass %>">
      <h1 class="isleH1" id="communityTitle"></h1>
      <div id="communityAvatar"><input type="button" class="isleButton bgBlue" id="btnFollow" onclick="toggleFollowing()" value="Follow" /></div>
      <p id="communityDescription"></p>
      <div class="grayBox" id="communityPostMaker">
        <div id="PostMakerContent" runat="server">
          <h2>Post to this Community</h2>
          <textarea id="postBox" class="postTextBox" data-id="0"></textarea>
          <div id="mainReplyBox"><span data-replyStatusID="0" class="replyStatus"></span> <input type="button" class="isleButton bgGreen" id="btnPostMessage" value="Post" onclick="postMessage(0, this)" /></div>
        </div>
        <p id="PostMakerLoginMessage" runat="server" class="middle">Please login to post to this Community!  </p>
      </div>
    </div>
  </div>
  <div id="communitiesList" class="grayBox">
      <h2 class="header">Communities</h2>
      <asp:Literal ID="txtCommunities" runat="server"></asp:Literal>
  </div>
  <div id="communityPostingList">

  </div>
</div>
<div id="templates" style="display:none;">
  <div id="template_posting">
    <div class="posting grayBox" data-postingID="{postingID}">
      <div class="avatar" style="background-image: url('{avatar}');"></div>
      <h2 class="name"><a href="/Profile/{userid}/{name}">{name}</a></h2><span class="date">{date} {deleteLink}</span>

      <p>{text}</p>
      <div class="replyBox" data-postingID="{postingID}"><a href="#" class="textLink replyLink" onclick="openReplyBox({postingID}); return false;">Reply</a><!--<span class="delimiter"> | </span><a href="#" class="textLink lockLink" onclick="lock({postingID}); return false;">Lock</a>--></div>
    </div>
    <div class="replies" data-parentID="{postingID}"></div>
  </div>
  <div id="template_dateSeparator">
    <div class="grayBox dateSeparator">Postings on {date}:</div>
  </div>
  <div id="template_replyControls">
    <input type="text" data-id="{id}" class="replyTextBox postTextBox" /><input type="button" class="isleButton bgGreen replyButton" onclick="postMessage({id}, this);" value="Post" />
    <div data-replyStatusID="{id}" class="replyStatus"></div>
  </div>
  <div id="template_deleteLink">
    <a class="deleteLink" href="#" onclick="deletePost({postingID}); return false;">X</a>
  </div>
</div>

<asp:Literal ID="ltlMinimumMessageLength" runat="server" Visible="false">25</asp:Literal>