<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SocialBox1.ascx.cs" Inherits="IOER.Controls.SocialBox.SocialBox1" %>

<div id="socialBoxContainer" runat="server">
  <script type="text/javascript">
    var socialBoxData = <%=socialBoxDataString %>;
  </script>
  <script type="text/javascript">
    $(document).ready(function () {

      socialBoxData.comments.push({ id: 1, name: "User Name", date: "01/01/2001", text: "Comment Text Here", avatar: "http://localhost:2012/images/ioer_med.png" });
      socialBoxData.comments.push({ id: 2, name: "User Name", date: "01/02/2001", text: "More Comment Text Here", avatar: "http://localhost:2012/images/ioer_med.png" });
      socialBoxData.comments.push({ id: 3, name: "User Name", date: "01/03/2001", text: "Other Comment Text Here", avatar: "http://localhost:2012/images/ioer_med.png" });

      loadSocialBoxData();
      showSocialBoxTab("details");
      socialBox_toggle();
    });

    function loadSocialBoxData() {
      //Likes
      $("#socialBox_likeCounter").html(socialBoxData.likeData.likes + " Likes");
      if(socialBoxData.likeData.iLikedThis){
        $("#socialBox_btnLike").attr({ "value": "Liked!", "disabled": "disabled" });
      }
      //Comments
      var list = $("#socialBox_commentsList");
      var template = $("#socialBox_template_comment").html();
      list.html("");
      for(i in socialBoxData.comments){
        var item = socialBoxData.comments[i];
        list.append(template
          .replace(/{id}/g, item.id)
          .replace(/{name}/g, item.name)
          .replace(/{date}/g, item.date)
          .replace(/{text}/g, item.text)
          .replace(/{avatarsrc}/g, "src=\"" + item.avatar + "\"")
        );
      }
    }

    function showSocialBoxTab(target) {
      $(".socialBox_tab").hide();
      $("#socialBox_" + target).show();
      $("#socialBox_tabNavigation a").removeClass("selected");
      $("#socialBox_tabNavigation a[data-name=" + target + "]").addClass("selected");
      $("#socialBox").removeClass("retracted");
      $("#socialBox_showHide").html("-");
      if(target == "details"){ $("#socialBox").addClass("details"); }
      else { $("#socialBox").removeClass("details"); }
    }

    function socialBox_toggle() {
      $("#socialBox").toggleClass("retracted");
      $("#socialBox_showHide").html($("#socialBox").hasClass("retracted") ? "+" : "-");
    }
  </script>
  <style type="text/css">
    #socialBox { margin: 5px 0 10px 0; padding: 0 5px; position: relative; max-height: 1000px; transition: max-height 0.8s; -webkit-transition: max-height 0.8s; box-shadow: none; }
    #socialBox.retracted { max-height: 75px; overflow: hidden; }
    #socialBox.retracted:after { content: " "; display: block; box-shadow: 0 -40px 100px -40px #FFF inset; position: absolute; bottom: 0; left: 5px; right: 5px; height: 40px; }
    #socialBox.details #socialBox_showHide { display: none; }
    #socialBox.retracted.details:after { content: ""; box-shadow: none; height: 0; }
    #socialBox_tabNavigation { white-space: nowrap; }
    #socialBox_tabNavigation a { display: inline-block; margin-right: -3px; padding: 5px; height: 30px; line-height: 30px; text-align: center; border-radius: 5px 5px 0 0; color: #FFF; font-weight: bold; background-color: #3572B8; }
    #socialBox_tabNavigation a.selected { background-color: #4AA394; }
    #socialBox_tabNavigation a:hover, #socialBox_tabNavigation a:focus { background-color: #FF6A00; }

    #socialBox_tabs .socialBox_tab { padding: 5px; background-color: #EEE; border-radius: 0 5px 5px 5px; }

    #socialBox_likeBox { margin-bottom: 5px; }
    #socialBox_btnLike { width: auto; }

    #socialBox_likeComment { white-space: nowrap; }
    #socialBox_likeCommentColumnLeft { width: 25%; }
    #socialBox_commentsList { width: 74.5%; }
    #socialBox_likeCommentColumnLeft, #socialBox_commentsList { display: inline-block; margin-right: -4px; white-space: normal; vertical-align: top; }
    #socialBox_makeCommentBox textarea, #socialBox_btnComment { display: block; width: 100%; }
    #socialBox_makeCommentBox textarea { height: 5em; resize: none; }

    #socialBox_commentsList { padding: 25px 5px 5px 5px; }
    #socialBox_commentsList:empty:after { content: "No comments have been added yet!"; font-style: italic; color: #999; text-align: center; display: block; padding: 50px 10px; }
    .socialBox_comment { border: 1px solid #CCC; border-radius: 5px; padding: 5px 5px 1.2em 5px; position: relative; min-height: 75px; margin-bottom: 5px; }
    .socialBox_commentHeader { margin: -5px -5px 5px -5px; border-radius: 5px 5px 0 0; background-color: #4AA394; color: #FFF; padding: 3px 5px 3px 60px; font-weight: bold; }
    .socialBox_commentText { padding: 0 5px 5px 55px; }
    .socialBox_commentAvatar { position: absolute; top: 3px; left: 5px; width: 50px; }
    .socialBox_commentAvatar img { width: 100%; max-height: 70px; }
    .socialBox_commentDate { position: absolute; bottom: 3px; right: 3px; font-style: italic; color: #999; font-size: 90%; }

    #socialBox_tabs #socialBox_share { padding-top: 20px; }
    #socialBox .txtCopyIframe { width: 100%; }
    #socialBox_showHide { background-color: #FF6A00; color: #FFF; font-weight: bold; width: 22px; height: 22px; line-height: 22px; text-align: center; display: block; position: absolute; top: 25px; right: 8px; font-size: 22px; border-radius: 5px; z-index: 10; }
    #socialBox_showHide:hover, #socialBox_showHide:focus { box-shadow: 0 0 20px #FF5707; }


    @media screen and (max-width: 675px){
      #socialBox_likeCommentColumnLeft, #socialBox_commentsList { display: block; width: 100%; margin-right: auto; }
    }
  </style>
  <div id="socialBox" class="">

    <div id="socialBox_tabNavigation">
      <a href="#" data-name="details" onclick="showSocialBoxTab('details'); return false;">Details</a>
      <a href="#" data-name="likeComment" onclick="showSocialBoxTab('likeComment'); return false;">Like & Comment</a>
      <a href="#" data-name="share" onclick="showSocialBoxTab('share'); return false;">Share</a>
    </div>
    <div id="socialBox_tabs">
      <div class="socialBox_tab" id="socialBox_details"></div>
      <div class="socialBox_tab" id="socialBox_likeComment">
        <div id="socialBox_likeCommentColumnLeft">
          <div id="socialBox_likeBox">
            <input type="button" id="socialBox_btnLike" class="isleButton bgGreen" value="+Like" />
            <span id="socialBox_likeCounter"></span>
          </div>
          <div id="socialBox_makeCommentBox">
            <textarea id="socialBox_txtComment"></textarea>
            <input type="button" id="socialBox_btnComment" class="isleButton bgGreen" value="Comment!" />
          </div>
        </div>
        <div id="socialBox_commentsList"></div>
      </div>
      <div class="socialBox_tab" id="socialBox_share">
        <div id="shareType_curriculum" runat="server" visible="false">
          <script type="text/javascript">
            $(document).ready(function() { 
              $("#socialBox_share .txtCopyIframe").val("<iframe src=\"//ioer.ilsharedlearning.org/widgets/curriculum?cidx=" + socialBoxData.shareData.id + "\"></iframe>"); 
            });
          </script>
          <p>Copy the text below and paste it into your website to display this Curriculum!</p>
          <input class="txtCopyIframe" type="text" readonly="readonly" onclick="this.select()" value="" />
        </div>
      </div>
    </div>
    <a href="#" onclick="socialBox_toggle(); return false;" id="socialBox_showHide">+</a>
  </div>

  <div id="socialBox_templates" style="display:none;">
    <div id="socialBox_template_comment">
      <div class="socialBox_comment" data-commentID="{id}">
        <div class="socialBox_commentHeader">{name}</div>
        <div class="socialBox_commentText">{text}</div>
        <div class="socialBox_commentAvatar">
          <img {avatarsrc} />
        </div>
        <div class="socialBox_commentDate">{date}</div>
      </div>
    </div>
  </div>
</div>
