<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Detail6.ascx.cs" Inherits="ILPathways.LRW.controls.Detail6" %>
<%@ Register TagPrefix="uc1" TagName="UsageRightsSelector" Src="/LRW/controls/ConditionsOfUseSelector.ascx" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/controls/StandardsBrowser7.ascx" %>

<% 
  //Easy CSS colors
  string css_black      = "#4F4E4F";
  string css_red        = "#B03D25";
  string css_orange     = "#FF5707";
  string css_purple     = "#9984BD";
  string css_teal       = "#4AA394";
  string css_gray       = "#909297";
  string css_blue       = "#3572B8";
  string css_white      = "#E6E6E6";
  string css_lightblue  = "#4C98CC";
%>


  <!--Data from server -->
  <script type="text/javascript">
      var userGUID = "<%=userGUID %>";
      var resourceVersionID = <%=resourceVersionID %>;
      var resourceID = <%=resourceIntID %>;
      var userIsAdmin = "<%=isUserAdmin %>";
      var userIsAuthor = "<%=isUserAuthor %>";
      <%=resourceText %>
      <%=codeTables %>
      var SB7mode = "tag";
  </script>
  <script type="text/javascript" language="javascript" src="/Scripts/detail6.js"></script>
  <%--<link rel="Stylesheet" href="/Styles/detail6.css" />--%>

  <style type="text/css">
    /* Big stuff */
    html, body { min-height: 100%; width: 100%; margin: 0; padding: 0; }
    .content { font-size: 0; text-align: center; min-width: 300px; }
    .content * { box-sizing: border-box; -moz-box-sizing: border-box; margin: 0; padding: 0; }
    .column { display: inline-block; vertical-align: top; padding: 5px; min-width: 300px; text-align: left; }
    .column * { font-size: 16px; }
    .left.column { width: 64%; }
    .right.column { width: 34%; }
    
    /* Common stuff */
    a.blockLink { display: inline-block; margin: 2px 3px; padding: 1px 2px; background-color: #DDF; }
    a.blockLink:hover, a.blockLink:focus { color: #FFF; }
    p.pleaseLogin { font-style: italic; text-align: center; padding: 10px; }

    /* Specific items */
    #title { font-size: 30px;  margin: 5px; border-radius: 5px; padding: 1px 10px; }
    #title * { font-size: inherit; }
    #resourceURL { font-size: 20px; display: block; text-align: center; margin-bottom: 5px; text-overflow: ellipsis; overflow: hidden; }
    #resourceURL:hover, #resourceURL:focus { text-decoration: underline; }
    #standardsRatings h2.title input { float: right; margin-top: 3px; }
    .thumbnailDiv { width: 400px; height: 300px; background-color: #DDD; text-align: center; padding-top: 50px; font-weight: bold; color: #333; font-size: 20px; }
    #standardsBrowser { padding: 5px; }
    #standardsBrowser #selectStandardGrade h2 { margin-bottom: 5px; background-color: transparent; color: #333; }
    #subject .edit, #subject .view, #keyword .edit, #keyword .view { padding: 5px; }
    #subject input[type=text], #keyword input[type=text] { width: 100%; margin-bottom: 5px; }
    .addedTextItem { display: inline-block;  border: 1px solid #CCC; border-radius: 5px; }
    .addedTextItem span { padding: 2px 5px; }
    .addedTextItem a { font-weight: bold; text-align: center; width: 20px;  color: #FFF; display: inline-block; border-radius: 0 5px 5px 0; }
    .addedTextItem a:hover, .addedTextItem a:focus { background-color: #F00; }
    #timeRequired .edit { text-align: center; }
    #timeRequired .edit div { display: inline-block; }
    #timeRequired input, #timeRequired select { width: 45px; }
    #itemType select, #accessRights select, #language select { width: 100%; }
    #requires { clear: both; }
    #requires input { width: 100%; }
    .resourceLikeThis { overflow: auto; }
    .resourceLikeThis a.mltThumbnail { float: right; width: 50%; max-width: 150px; display: block; margin: 0 0 5px 5px; }
    .resourceLikeThis a.mltThumbnail img { width: 100%; }
    .reportProblemContainer { width: 100%; padding: 5px; }
    #txtReportProblem { width: 100%; resize: none; display: block; height: 50px; padding: 5px; text-align: left; }
    #report .reportProblemContainer input[type=button].report.btn { width: 100%; margin: 1px 0; }
    #lrDocLink { text-align: center; }

    /* Thumbnail and critical info */
    #thumbAndCritical { width: 100%; max-width: 400px; position: relative;  color: #FFF; }
    #criticalInfo { position: absolute; bottom: 0; width: 100%; max-width: 400px; padding: 5px 150px 5px 5px; box-shadow: 0 0 225px 0px #333 inset; min-height: 50px; border-radius: 0 0 5px 5px; }
    #thumbnail img, #thumbnail div { width: 100%; background-color: #EEE; border-radius: 5px; }
    #thumbnail { width: 100%; max-width: 400px; display: block; }
    #usageRights { position: absolute; right: 5px; bottom: 5px; }
    
    /* Tab Box stuff */
    #infoTabBox { clear: both; margin-top: 10px; }
    .tabBox .tab { background-color: #EEE; min-height: 75px; border-radius: 5px; padding-bottom: 10px; }
    .tabBox .tab[data-selected=false] { display: none; }
    .tabBox .tab h2 { font-size: 20px; line-height: 25px; color: #FFF; padding: 0 5px; border-top-left-radius: 5px; border-top-right-radius: 5px; }
    p.center { text-align: center; padding: 10px; }
    
    /* Tab Navigation */
    .tabNavigator { margin-left: 5px; }
    .tabNavigator a { 
      display: inline-block; 
      height: 65px; 
      min-width: 40px; 
      border-radius: 10px 10px 0 0;
      text-align: center; 
      font-weight: bold; 
      color: #FFF; 
      font-size: 30px; 
      box-shadow: inset 0px -20px 60px -20px #26A;
      padding: 30px 3px 5px 3px;
    }
    .tabNavigator a[data-selected=true] { box-shadow: none; }
    .tabNavigator a:hover, .tabNavigator a:focus { color: #FFF; box-shadow: none; }
    .tabNavigator a[data-id=comments] { background-image: url('/images/icons/icon_comments_bg.png'); }
    .tabNavigator a[data-id=tags] { background-image: url('/images/icons/icon_tag_bg.png'); }
    .tabNavigator a[data-id=keyword] { background-image: url('/images/icons/icon_keywords_bg.png'); }
    .tabNavigator a[data-id=subject] { background-image: url('/images/icons/icon_subjects_bg.png'); }
    .tabNavigator a[data-id=moreLikeThis] { background-image: url('/images/icons/icon_morelikethis_bg.png'); }
    .tabNavigator a[data-id=likedislike] { background-image: url('/images/icons/icon_likes_bg.png'); }
    .tabNavigator a[data-id=library] { background-image: url('/images/icons/icon_library_bg.png'); }
    .tabNavigator a[data-id=rubrics] { background-image: url('/images/icons/icon_ratings_bg.png'); }
    .tabNavigator a[data-id=report] { background-image: url('/images/icons/icon_report_bg.png'); }
    .tabNavigator a[data-id=alignedStandards] { background-image: url('/images/icons/icon_standards_bg.png'); }
    .tabNavigator a[data-id=standardsRatings] { background-image: url('/images/icons/icon_standards_rated_bg.png'); }
    .tabNavigator a[data-id=modifyThis] { background-image: url('/images/icons/icon_resources_bg.png'); }
    
    /* Tab Box Tags */
    #infoTabBox .tab#tags > div { padding: 5px; vertical-align: top; }
    #tags div[data-hasContent=false], div[data-hasContent=false] { display: none; }

    /* Tab Box Comments */
    #comments .comment h3 .date { float: right; margin-left: 15px; }
    #comments textarea { text-align: left; padding: 5px;  width: 100%; resize: none; display: block; height: 100px; }
    #comments .edit { text-align: right; padding: 5px; }
    #comments input[type=button] { width: 25%; min-width: 75px; }
    
    .lightbox { border: 1px solid #DDD; padding: 5px; margin: 5px; border-radius: 5px; }
    .lightbox > h3 { background-color: #DDD; color: #333; text-align: left; padding: 1px 5px; margin: 5px -5px 5px -5px; }
    .lightbox > h3:first-child { margin: -5px -5px 5px -5px; border-top-left-radius: 3px; border-top-right-radius: 3px; }
    
    /* Likes and Dislikes */
    #likedislike { text-align: center; margin-bottom: 10px; }
    #likedislike h2 { text-align: left; }
    .opinionbar { position: relative; height: 22px; padding: 1px 2px; border-radius: 5px; border: 1px solid #999; margin: 5px; text-align: left; }
    .opinionbar #likebar, .opinionbar #dislikebar { height: 100%; display: inline-block; vertical-align: top; transition: width 0.5s; -webkit-transition: width 0.5s; }
    .opinionbar #likebar { border-top-left-radius: 5px; border-bottom-left-radius: 5px; }
    .opinionbar #dislikebar { border-top-right-radius: 5px; border-bottom-right-radius: 5px; float: right; }
    .opinionbar #text { position: absolute; top: 0; left: 0; width: 100%; color: #FFF; }
    .opinionbar .spanlikes { float: left; width: 40%; padding-left: 30px; background: transparent url('/images/icons/icon_likes_white.png') no-repeat left -6px; }
    .opinionbar .spandislikes { float: right; width: 40%; text-align: right; padding-right: 30px; background: transparent url('/images/icons/icon_dislikes_white.png') no-repeat right -6px; }
    #likedislike input { width: 48%; }
    
    
    /* Paradata Stuff */
    #paradataIcons { padding: 5px 0; text-align: center; }
    .paradataIcon { display: inline-block; vertical-align: top; text-align: center; background-color: #EEE; border-radius: 10px; font-weight: bold; font-size: 18px; margin: 2px; min-width: 50px; }
    .badge { display: inline-block; max-height: 50px; height: 50px; margin: 5px; vertical-align: top; }
    .badge img { height: 100%; max-width: 150px; }
    #clickthroughs { text-align: center; font-size: 14px; margin-bottom: 10px; }
    #paradataIcons { display: none; /* Temporary ? */ }
    
    /* Library Box */
    #myLibrary { text-align: center; }
    #myLibrary h3 { text-align: left; }
    #myLibrary select { width: 100%; margin: 5px 0; }
    #myLibrary input { width: 100%; color: #FFF; border-radius: 5px; }
    #myLibrary input:hover, #myLibrary input:focus { cursor: pointer; }
      #submissionMessage { color: green;}
    /* Scoreometer */
    /*.scoreometer { border: 1px solid #CCC; padding: 2px; margin: 5px; border-top: 5px solid #CCC; }*/
    .scoreometer h5 { font-weight: normal; font-style: italic; }
    .scoreometer .scorebar { width: 100%; }
    .scoreometer .barholder, .scoreometer .percent { display: inline-block; vertical-align: top; height: 18px; line-height: 17px; font-size: 12px; }
    .scoreometer .barholder { width: 100%; border: 1px solid #999; border-radius: 5px; padding: 1px 2px; position: relative; }
    .scoreometer .percent { width: 100%; position: absolute; top: 0; left: 0; text-align: center; }
    .scoreometer .bar { height: 100%; background-color: #CBD; border-radius: 5px; box-shadow: 0 0 5px #CBD; min-width: 5px; }
    .scoreometer .myScore { font-size: 0; text-align: center; margin: 2px 0; }
    /*.scoreometer .myScore a, .scoreometer .myScore input { display: inline-block; vertical-align: top; width: 20%; font-size: 12px; height: 20px; line-height: 20px; }
    .scoreometer .myScore a { background-color: #CCC; color: #000; }
    .scoreometer .myScore a:hover, .scoreometer .myScore a:focus { color: #FFF; }*/
    .scoreometer .myScore input[type=button] { display: inline-block; vertical-align: top; color: #FFF; border-radius: 5px; line-height: 15px; width: 20%; margin-top: -1px; }
    .scoreometer .myScore select { height: 22px; font-size: 12px; width: 80%; padding: 1px 2px; margin-bottom: -1px; }
    /*.scoreometer .myScore .ratethis { font-weight: bold; font-size: 12px; }*/
    .scoreometer .bar.notrated { background-color: #DDD; box-shadow: none; }
    .scoreometer .percent i { font-size: 12px; padding: 0 2px; }
    .scoreometer .percent .weak { float: left; }
    .scoreometer .percent .superior { float: right; }
    .scoreometer .description { font-size: 14px; margin-bottom: 5px; }
    .scoreometer .percent { opacity: 0; }
    .scoreometer:hover .percent { opacity: 1; }
    .scoreometer h3 { font-weight: normal; }
    
    /* Buttons */
    input[type=button].btn { 
      text-align: center;
      font-weight: normal;
      padding: 1px 5px;
      margin: 1px;
      border-radius: 5px; 
      color: #FFF; 
    }
    input[type=button].btn:hover, input[type=button].btn:focus { cursor: pointer; }
    input[type=button].btn.tiny { font-size: 12px; margin: 0; }
    
    #modifyThis { 
      text-align: center;
      margin-bottom: 10px;
    }
    #modifyThis input[type=button] {
      width: 48%;
      display: inline-block;
      vertical-align: top;
      padding: 3px;
    }
    #modifyThis h2 { text-align: left; }
    #description {
          margin-left: 400px;
      }

    #resourceNote { display: inline-block; width: 50%;     }
    #resourceMsg { margin-top:25px; padding: 10px; text-align: center;      }
      #resourceMsg .isleBox {
          padding-bottom: 15px;
      }
      
    #compDescription { text-align: left; padding:12px;   }
    /* Edit Stuff */
    #title .edit input[type=text] { width: 80%; }
    #description .edit textarea { width: 75%; height: 280px; padding: 5px; resize: none; }
    .cbxl .edit a { display: block; padding: 2px 5px; border-left: 10px solid #CCC; color: #333; margin-bottom: 1px; border-radius: 5px; }
    .cbxl .edit a[data-preselected=false]:hover, .cbxl .edit a[data-preselected=false]:focus { color: #FFF; }
    .cbxl .edit a[data-selected=true] { color: #FFF; }
    .cbxl .edit a[data-preselected=true], .cbxl .edit a[data-preselected=true][data-selected=true] { background-color: #CCC; color: #000; }

    /* Evaluations */
    .evaluation { margin-bottom: 20px; }
    .evaluationBarContainer { border: 1px solid #BBB; border-radius: 5px; padding: 1px; background-color: #DDD; background: linear-gradient(#DADADA, #EFEFEF); }
    .evaluationBar, .evaluationBarContainer { position: relative; }
    .evaluationBarFill { background-color: #CBD; height: 20px; }
    .dimensions h4 { font-weight: normal; }
    .dimensions .evaluationBarFill, .ratedStandard .evaluationBarFill { height: 14px; }
    .evaluationBarFill.noRatings, .evaluationBarFill.notApplicable { background-color: #CCC; }
    .evaluationBarText { position: absolute; top: 0; left: 0; text-align: center; width: 100%; font-size: 16px; line-height: 20px; opacity: 0; }
    .dimensions .evaluationBarText, .ratedStandard .evaluationBarText { font-size: 14px; line-height: 14px; }
    .evaluationBarContainer:hover .evaluationBarText, .evaluationBarContainer:focus .evaluationBarText { opacity: 1; cursor: default; }
    .evaluationBar.trained .evaluationBarFill { border-radius: 5px 5px 0 0; }
    .evaluationBar.untrained .evaluationBarFill { border-radius: 0 0 5px 5px; }
    .evaluation[data-requiresCertification=true] .evaluationBar.untrained { display: none; }
    .evaluation[data-requiresCertification=true] .evaluationBar.trained .evaluationBarFill { border-radius: 5px; }

    #standardsRatings .evaluationBarFill { border-radius: 5px; }
    .ratedStandard h4 { margin-bottom: 5px; }
    .ratedStandard .doRating { white-space: nowrap; display: none; }
    .ratedStandard .doRating select { width: 85%; margin-right: -4px; }
    .ratedStandard .doRating input, .ratedStandard .expandCollapseStandard { background-color: #4C98CC; color: #FFF; border-radius: 0 5px 5px 0; width: 15%; border-width: 1px; }
    .ratedStandard .doRating input:hover, .ratedStandard .doRating input:focus, .ratedStandard .expandCollapseStandard:hover, .ratedStandard .expandCollapseStandard:focus { background-color: #FF5707; cursor: pointer; }
    .ratedStandard .expandCollapseStandard { width: 25px; float: right; border-radius: 5px; }
    .ratedStandard .description, .ratedStandard .message { display: none; }
    .ratedStandard.expanded .doRating, .ratedStandard.expanded .description, .ratedStandard.expanded .message { display: block; }

    /* Responsive */
    @media screen and (min-width: 0) and (max-width: 450px) {
      #infoTabBox .tab#comments .comment h3 .date { float: none; text-align: right; } 
      #resourceNote { float: left; width: 90%; }
    }
    @media screen and (min-width: 0) and (max-width: 600px){
      .left.column, .right.column { width: 100%; }
      #thumbAndCritical { margin: 5px auto; float: none; }
      .scoreometer .myScore * { width: 100%; display: block; }
      #modifyThis input[type=button] { width: 100%; }
      #description {margin-left: 20px;} 
    }
    @media screen and (min-width: 900px) and (max-width: 1160px){
        #description {margin-left: 10px; display: inline-block; width: 80%;} 
    }
    @media screen and (min-width: 601px) and (max-width: 900px){
      .left.column, .right.column { width: 100%; } 
    }
    @media screen and (min-width: 951px) {

    }
    @media screen and (min-width: 601px) {
      #thumbAndCritical { margin: 0 10px 10px 0; float: left; }
      #tags > div { display: inline-block; width: 48%; }
      
    }
    /* AddThis customizations */
    .content { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width:980px) {
      .content { padding-left: 50px; }
    }
  </style>
  <style type="text/css">
    /* colors */
    a.blockLink:hover, a.blokcLink:focus { background-color: <%=css_orange %>; }
    #title { background-color: <%=css_black %>; color: <%=css_white %>; }
    .addedTextItem a { background-color: <%=css_red %>; }
    .resourceLikeThis a { color: <%=css_blue %>; }
    .resourceLikeThis a:hover, .resourcesLikeThis a:focus { color: <%=css_orange %>; }
    .tabBox .tab h2 { background-color: <%=css_teal %>; }
    .tabNavigator a { background: <%=css_lightblue %> no-repeat center 5px; }
    .tabNavigator a[data-selected=true] { background-color: <%=css_teal %>; }
    .tabNavigator a:hover, .tabNavigator a:focus { background-color: <%=css_orange %>; }
    .opinionbar #likebar { background-color: <%=css_teal %>; }
    .opinionbar #dislikebar { background-color: <%=css_red %>; }
    #myLibrary input { background-color: <%=css_teal %>; }
    #myLibrary input:hover, #myLibrary input:focus {  background-color: <%=css_orange %>; }
    .scoreometer h4 { color: <%=css_teal %>; }
    .scoreometer .myScore a:hover, .scoreometer .myScore a:focus { background-color: <%=css_orange %>; }
    .scoreometer .myScore a[data-selected=true] { background-color: <%=css_purple %>; }
    .scoreometer .myScore input[type=button] { background-color: <%=css_teal %>; }
    input[type=button].btn.green { background-color: <%=css_teal %>; }
    input[type=button].btn.blue { background-color: <%=css_blue %>; }
    input[type=button].btn.red { background-color: <%=css_red %>; }
    input[type=button].btn:hover, input[type=button].btn:focus { background-color: <%=css_orange %>; }
    .cbxl .edit a[data-preselected=false]:hover, .cbxl .edit a[data-preselected=false]:focus { background-color: <%=css_orange %>; }
    .cbxl .edit a[data-selected=true] { background-color: <%=css_teal %>; }
    .error { padding: 50px 5px; text-align: center; }
  </style>
  
  <div id="error" class="error" runat="server" visible="false">
    <p>Sorry, that is an invalid Resource.</p>

      <asp:Button ID="btnReActivateResource" runat="server" Visible="false"  CssClass="defaultButton" Text="Reactivate Resource"  OnClick="btnReActivateResource_Click" OnClientClick="document.forms[0].onsubmit='';" />
      <br /><asp:Literal ID="litResourceId" runat="server" Visible="false">0</asp:Literal>
    
  </div>

  <div id="content" class="content" runat="server" visible="true">
    <h1 id="title"><span class="view admin" itemprop="name"></span><span class="edit admin">Title: <input type="text" /></span></h1>

    <div class="left column">
      <a href="#" id="resourceURL" target="_blank" itemprop="targetUrl"></a>
      <div id="lrDocLink"></div>
      <div id="clickthroughs"></div>
      <div id="thumbAndCritical">
        <a id="thumbnail" href="#" target="_blank"><img src="/images/ThumbnailResizer.png" /></a>
        <div id="criticalInfo">
          <div id="usageRights"><img class="view" src="/images/icons/rightsreserved.png" /></div>
          <div id="created" itemprop="dateCreated"><b>Created:</b> <span></span></div>
        </div>
      </div>
      <div id="description"><h2>Description</h2><p class="view admin" itemprop="description"></p><span class="edit admin"><textarea></textarea></span></div>
        <div id="resourceNote"><p class="view admin" itemprop="description"></p></div>

      <div id="requires"><h2>Technology and Equipment Requirements</h2><p class="view admin"></p><span class="edit admin"><input type="text" /></span></div>
      <div id="infoTabBox" class="tabBox">
       <div id="modifyThis">
          <input type="button" value="Update This Data" runat="server" id="btnStartUpdateMode" class="update btn green view" onclick="switchToUpdateMode()" />
          <input type="button" value="Save Changes" runat="server" id="btnFinishUpdate" class="finish btn green edit" onclick="saveUpdates()" />
          <input type="button" value="Cancel Changes" runat="server" id="btnCancelChanges" class="cancel btn red edit" onclick="cancelChanges()" />
          <input type="button" value="Deactivate Resource" runat="server" id="btnDeactivateResource" class="deactivate btn red" onclick="deactivate()" />
          <input type="button" value="Regenerate Thumbnail" runat="server" id="btnRegenerateThumbnail" class="regenerate btn green" onclick="regenerateThumbnail()" />
          <!--<input type="button" value="Send Resource to External Site" class="btn green" id="btnSendResource" onclick="sendResource()" />-->
          <input type="button" value="Send Resource to External Site" class="btn green" id="btnMsgResource" onclick="sendResourceMsg()" />
       </div>
        <div class="tabNavigator">
          <a href="#" data-id="tags" title="Metadata Tags"></a>
          <a href="#" data-id="alignedStandards" title="Aligned Standards"></a>
          <a href="#" data-id="keyword" title="Keywords"></a>
          <!--<a href="#" data-id="subject" title="Subjects"></a>-->
          <a href="#" data-id="moreLikeThis" title="More Like This"></a>
       </div>
       <div class="tab" id="tags">
          <h2>Tags</h2>
          <div id="gradeLevel" class="cbxl"></div>
          <div id="careerCluster" class="cbxl"></div>
          <div id="endUser" class="cbxl" itemprop="endUser"></div>
          <div id="groupType" class="cbxl"></div>
          <div id="resourceType" class="cbxl" itemprop="learningResourceType"></div>
          <div id="mediaType" class="cbxl" itemprop="mediaType"></div>
          <div id="educationalUse" class="cbxl" itemprop="educationalUse"></div>
          <div id="k12subject" class="cbxl"></div>
          <div id="pickUsageRights" class="edit">
            <h3>Usage Rights</h3>
            <uc1:UsageRightsSelector ID="usageRightsSelector" runat="server" />
          </div>
          <div id="accessRights" class="ddl"></div>
          <div id="language" class="ddl" itemprop="inLanguage"></div>
          <div id="timeRequired">
            <h3>Time Required</h3>
            <div class="view" itemprop="timeRequired"></div>
            <div class="edit">
              <div>Days: <input type="text" id="timeRequiredDays" /></div>
              <div>Hours: <select id="timeRequiredHours"></select></div>
              <div>Minutes: <select id="timeRequiredMinutes"></select></div>
            </div>
          </div>
          <div id="itemType" class="ddl"></div>
          <div id="creator" itemprop="author"><h3>Creator:</h3> <span></span></div>
          <div id="publisher" itemprop="publisher"><h3>Publisher:</h3> <span></span></div> 
          <div id="submitter"><h3>Submitter:</h3> <span></span></div>       
          <div id="accessibilityControl" class="cbxl"></div>
          <div id="accessibilityFeature" class="cbxl"></div>
          <div id="accessibilityHazard" class="cbxl"></div>
        </div>
        <div class="tab" id="keyword">
          <h2>Keywords</h2>
          <div class="edit">
            <h3>Add keywords</h3>
            <input type="text" placeholder="Type a keyword and press Enter..." />
            <div class="addedFreeText" data-id="keyword"></div>
          </div>
          <div class="view"></div>
        </div>
        <div class="tab" id="subject">
          <h2>Subjects</h2>
          <div class="edit">
            <h3>Suggested subjects</h3>
            <div id="suggestedSubjects"></div>
            <h3>Add subjects</h3>
            <input type="text" placeholder="Type a subject and press Enter..." />
            <div class="addedFreeText" data-id="subject"></div>
          </div>
          <div class="view" itemprop="about"></div>
        </div>
        <div class="tab" id="alignedStandards">
          <h2>Aligned Standards</h2>
          <div class="edit">
            <uc1:StandardsBrowser ID="sBrowser" runat="server" />
          </div>
          <div class="view"></div>
        </div>
        <div class="tab" id="moreLikeThis">
          <h2>More Like This</h2>
          <div class="resourcesLikeThis"></div>
        </div>
      </div>
    </div><!-- /left column -->
    <div class="right column">
      <div class="tabBox">
        <div id="likedislike" class="tab" data-selected="true">
          <h2>Community Opinion</h2>
          <div class="opinionbar">
            <div id="likebar"></div><div id="dislikebar"></div>
            <div id="text">
              <span class="spanlikes" title="Likes"></span>
              <span class="spandislikes" title="Dislikes"></span>
            </div>
          </div>
          <input id="btnAddLike" type="button" class="like btn green" value="Like" />
          <input id="btnAddDislike" type="button" class="dislike btn red" value="Dislike" />
          <p class="myopinion"></p>
        </div>
      </div>
      <div id="opinionTabBox" class="tabBox">
        <div class="tabNavigator">
          <a href="#" data-id="comments" title="Comments"></a>
          <a href="#" data-id="library" title="Library Info"></a>
          <a href="#" data-id="standardsRatings" title="Standards Alignment Ratings"></a>
          <!--<a href="#" data-id="rubrics" title="Rubric Evaluations"></a>-->
          <a href="#" data-id="report" title="Report an Issue">...</a>
        </div>
        <div class="tab" id="comments">
          <h2>Comments</h2>
          <div class="edit"><textarea></textarea><input type="button" class="btn green" value="Submit" onclick="postComment()" /></div>
          <div class="view"></div>
        </div>
        <div class="tab" id="library">
          <h2>Library Info</h2>
          <div id="badges" class="lightbox"></div>
          <div id="myLibrary" class="lightbox">
            <h3>Add this to my Library/Other Library:</h3>
            <select id="myLibraries"></select>
            <select id="myCollections"></select>
            <input type="button" class="btn green" value="Add" onclick="addToCollection()" />
              <div id="submissionMessage" ></div>
          </div>
        </div>
        <div id="standardsRatings" class="tab"></div>
        <div id="rubrics" class="tab">
          <h2>Resource Evaluations</h2>
          <div id="rubricsData"></div>
        </div>
        <div id="report" class="tab">
          <h2>Report an Issue</h2>
          <div class="reportProblemContainer" id="reportProblemContainer" runat="server">
            <textarea id="txtReportProblem"></textarea>
            <input type="button" value="Report a Problem" runat="server" id="btnReportProblem" class="report btn red" onclick="reportIssue()" />
          </div>
          <p></p>
        </div>
      </div>
    </div><!-- /right column -->

  </div><!-- /content -->

  <div id="templates" style="display:none;">
    <div id="template_CBXL">
      <h3>{title}</h3>
      <div class="view">{list}</div>
      <div class="edit"></div>
    </div>
    
    <div id="template_comment">
      <div class="comment lightbox">
        <h3><div class="date">{date}</div>{name} commented:</h3>
        <p>{text}</p>
      </div>
    </div>

    <div id="template_subjectkeyword">
      <a target="_blank" class="blockLink" href='/Search.aspx?q="{text}"'>{text}</a>
    </div>

    <div id="template_suggestedsubject">
      <a href="#" onclick="addFreeText('{text}', 'subject'); return false;" class="blockLink">{text}</a>
    </div>

    <div id="template_addedTextItem">
      <div class="addedTextItem" data-text="{text}"><span>{text}</span><a href="#" onclick="removeAddedText(this); return false;">X</a></div>
    </div>

    <div id="template_paradataIcon">
      <div class="paradataIcon" title="{title}">
        <img {img} />
        <div>{text}</div>
      </div>
    </div>

    <div id="template_badge">
      <div class="badge">
        <a href="/Libraries/Library.aspx?id={id}" title="{title}" target="_blank"><img {img} /></a>
      </div>
    </div>

    <div id="template_rubricScoreometer">
      <div class="scoreometer lightbox" data-score="{rawscore}">
        <h3>{title}</h3>
        <div class="scorebar">
          <div class="barholder">
            <div class="bar"></div>
            <div class="percent"><i class="weak">Weak</i> {count} rating{plural} <i class="superior">Superior</i></div>
          </div>
        </div>
      </div>
    </div>

    <div id="template_standardScoreometer">
      <div class="scoreometer lightbox" data-score="{rawscore}">
        <h3>{alignment}: {title}</h3>
        <p class="description">{description}</p>
        <div class="scorebar">
          <div class="barholder">
            <div class="bar"></div>
            <div class="percent"><i class="weak">Weak</i> {count} rating{plural} <i class="superior">Superior</i></div>
          </div>
        </div>
        <div class="myScore" data-standard="{standardID}">
          <select class="standardRatingDDL">
            <option value="null" selected="selected">This Resource's alignment to this Standard is...</option>
            <option value="1">Very Weak</option>
            <option value="2">Limited</option>
            <option value="3">Strong</option>
            <option value="4">Superior</option>
          </select>
          <input type="button" value="Save" class="btn green tiny" />
        </div>
      </div>
    </div>

    <div id="template_listedStandard">
      <div class="listedStandard lightbox" data-standardid="{standardID}">
        <h3>{alignment}: {title}</h3>
        <p>{description}</p>
      </div>
    </div>

    <div id="template_moreLikeThis">
      <div class="resourceLikeThis lightbox">
        <h3><a href="/Resource/{rid}/{urlTitle}" target="_blank">{title}</a></h3>
        <a class="mltThumbnail" href="/Resource/{rid}/{urlTitle}" target="_blank"><img src="//ioer.ilsharedlearning.org/OERThumbs/large/{intID}-large.png" /></a>
        <p>{description}</p>
      </div>
    </div>

    <div id="template_evaluation_rubric">
      <div class="evaluation rubric lightbox" data-requiresCertification="{requiresCert}">
        <h3>{title}</h3>
        <p class="pleaseLogin">{evalText}</p>
        <h3>Overall Scores</h3>
        {overallRatings}
        <h3>Score Breakdown</h3>
        <div class="dimensions">
          {dimensions}
        </div>
      </div>
    </div>

    <div id="template_evaluation_dimension">
      <div class="dimension">
        <h4>{title}</h4>
        {ratings}
      </div>
    </div>

    <div id="template_evaluationBars">
      <div class="evaluationBarContainer">
        <div class="evaluationBar trained">
          <div class="evaluationBarText">{trainedRatingsCount}</div>
          <div class="evaluationBarFill {noTrainedRatings} {trainedNotApplicable}" style="width:0px"></div>
        </div>
        <div class="evaluationBar untrained">
          <div class="evaluationBarText">{untrainedRatingsCount}</div>
          <div class="evaluationBarFill {noUntrainedRatings} {untrainedNotApplicable}" style="width:9999px"></div>
        </div>
      </div>
    </div>

    <div id="template_standard_ratings">
      <div class="ratedStandard lightbox" data-standardID="{standardID}">
        <h4>{title} <input type="button" class="expandCollapseStandard" value="+" onclick="expandCollapseStandard({standardID}, this);" /></h4>
        <div class="description">{description}</div>
        <div class="evaluationBarContainer">
          <div class="evaluationBar">
            <div class="evaluationBarText">99px - {ratingsCount} User Ratings</div>
            <div class="evaluationBarFill {noRatings} {notApplicable}"  style="width:99px"></div>
          </div>
        </div>
        {doRating}
      </div>
    </div>

    <div id="template_evaluation_bar">
      <div class="evaluationBarContainer" id="{id}">
        <div class="evaluationBar">
          <div class="evaluationBarText"></div>
          <div class="evaluationBarFill"></div>
        </div>
      </div>
    </div>

    <div id="template_doRating">
      <div class="doRating">
        <select id="standardRating_{standardID}">
          <option value="-1" selected="selected">This Resource's alignment to this Standard is...</option>
          <option value="0">Very Weak</option>
          <option value="34">Limited</option>
          <option value="67">Strong</option>
          <option value="100">Superior</option>
        </select>
        <input type="button" onclick="doStandardRating({standardID});" value="Rate" />
      </div>
    </div>

  </div>


<asp:Panel ID="Panel1" runat="server" Visible="false">
    <!-- set to blank to allow any one to update -->
  <asp:Literal ID="txtGeneralSecurity" runat="server" Visible="false"></asp:Literal>
    <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.LRW.Pages.ResourceDetail</asp:Literal>
</asp:Panel>
