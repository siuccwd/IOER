<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ESLibrary2.ascx.cs" Inherits="ILPathways.LRW.controls.ESLibrary2" %>
<%--<%@ Register TagPrefix="uc1" TagName="Search" Src="/LRW/Controls/ElasticSearch3.ascx" %>
<%@ Register TagPrefix="uc1" TagName="Search" Src="/Controls/ImportedSearch.ascx" %>--%>
<%@ Register TagPrefix="uc1" TagName="ActivityRenderer" Src="/Activity/ActivityRenderer.ascx" %>
<%@ Register TagPrefix="uc1" TagName="Search" Src="/Controls/SearchV6/SearchV6.ascx" %>


<div id="libraryStuff" runat="server">
  <!-- from server -->
  <script type="text/javascript">
    var libraryData = <%=libInfoString %>;
    var userGUID = "<%=userGUID %>";
    var proxyId = "<%=proxyId %>";
    var linkedCollectionID = <%=linkedCollectionID %>;

  </script>
  <script type="text/javascript" src="/Scripts/ESLibrary2.js"></script>
  <!--<script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>-->
  <!--<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />-->
  <script type="text/javascript" src="/Scripts/toolTipV2.js"></script>
  <link rel="stylesheet" type="text/css" href="/Styles/toolTipV2.css" />

  <style type="text/css">
    /* General */
    body #container { min-height: 1200px; }
    #libraryHeader {
      margin-bottom: 5px;
      box-sizing: border-box;
      -moz-box-sizing: border-box;
    }
    #libraryHeader * {
      box-sizing: border-box;
      -moz-box-sizing: border-box;
    }
    #libraryHeaderContent {
      border-radius: 5px;
      background-color: #EFEFEF;
      padding: 5px;
      transition: max-height 0.5s;
      -webkit-transition: max-height 0.5s;
      height: auto;
      max-height: 1000px;
      overflow: hidden;
      box-shadow: 0 0 10px -2px #4AA394, 0 3px 3px -1px #4AA394 inset;
      border-top: 1px solid #4AA394;
      position: relative;
    }
    #libraryHeaderContent[data-collapsed=true] { max-height: 160px; }
    p.middle { text-align: center; padding: 10px; font-style: italic; color: #555; }
    .panel { padding-top: 5px; }
    #libColTitle { padding-bottom: 5px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; padding-right: 70px; }
    .modBox_before { width: 55%; }
    .modBox_before select { width: 100%; margin-bottom: 2px; }
    /*.modBox_before select.halfWidth { width: 50%; }*/
    .modBox_before select option[disabled=disabled] { font-style: italic; color: #999; }

    /* Library/Collection Selector */
    #libColSelector { position: relative; padding-top: 25px; }
    #libColSelector #libraryAvatar { 
      height: 125px; 
      width: 125px; 
      display: block; 
      position: absolute;
      top: 0;
      left: 0;
    }
    #libColSelector a { 
      background-position: center center;
      background-size: contain;
      background-repeat: no-repeat;
    }
      #libColSelector #collectionList {
          overflow-x: auto;
          overflow-y: hidden;
          margin-left: 130px;
          height: 100px;
          white-space: nowrap;
          box-shadow: 75px 0 65px -50px #CCC inset, -75px 0 65px -50px #CCC inset;
          border-radius: 5px;
          border-left: 1px solid #CCC;
          border-right: 1px solid #CCC;
      }

    #libColSelector #collectionsHeader {
        height: 25px;
        font-style: italic;
        font-size: 14px;
        line-height: 25px;
        color: #555;
        position: absolute;
        left: 135px;
        top: 0;
    }
    #libColSelector #collectionsHeader .toolTipLink { 
      border: none;
    }
    #libColSelector #collectionsHeader .toolTipLink:hover, #libColSelector #collectionsHeader .toolTipLink:focus {
      box-shadow: none;
    }
    #libColSelector #collectionsHeader .toolTipDiv { 
      font-weight: normal;
      font-style: normal;
      line-height: initial;
    }
    #libColSelector .collectionIcon {  
      height: 75px; 
      display: inline-block; 
      margin: 5px; 
      background-color: #EEE;
    }
    #libColSelector a {
      border: 1px solid #CCC;
      border-radius: 5px;
      overflow: hidden;
      transition: border 0.2s;
      -webkit-transition: border 0.2s;
    }
    #libColSelector a:focus, #libColSelector a:hover {
      border: 1px solid #FF5707;
      box-shadow: 0 0 10px #FF5707;
    }
    #libColSelector a.selected {
      border: 1px solid #4AA394;
      box-shadow: 0 0 10px #4AA394;
    }
    #libColSelector a.selected:hover, #libColSelector a.selected:focus { 
      border: 1px solid #3572B8;
      box-shadow: 0 0 10px #3572B8;
    }
    #libColSelector a img { 
      height: 100%;
      max-width: 200px;
    }
    #libColSelector .collectionTitle { 
      display: inline-block;
      vertical-align: top;
      width: 100px;
      white-space: normal;
      text-transform: capitalize;
      padding: 1px 2px 1px 0;
      overflow: hidden;
      text-overflow: ellipsis;
      height: 99%;
      color: #000;
      font-size: 14px;
    }

    /* Controls */
    #libraryHeader textarea {
      resize: none;
    }

    /* Buttons */
    #libraryHeader .btn, .resourceActionButton, #btnSendExternal {
      padding: 1px 5px;
      border-radius: 5px;
      color: #FFF;
      background-color: #3572B8;
      width: 100%;
      margin-top: 5px;
    }
    #libraryHeader .btn.green, #btnSendExternal { background-color: #4AA394; }
    #libraryHeader .btn.red { background-color: #B03D25; }
    #libraryHeader .btn:hover, #libraryHeader .btn:focus, #btnSendExternal:hover, #btnSendExternal:focus { background-color: #FF5707; cursor: pointer; }

    /* Tabs */
    #libTabBox { margin: 0 5px; position: relative; }
    #libTabBox a { 
      display: inline-block;
      font-weight: bold; 
      color: #FFF; 
      background-color: #3572B8; 
      padding: 5px 5px;
      border-radius: 5px 5px 0 0;
      vertical-align: bottom;
      margin-right: -2px;
    }
    #libTabBox a.selected { background-color: #4AA394; }
    #libTabBox a:hover, #libTabBox a:focus { background-color: #FF5707; }
    #libraryHeaderContent a[data-id=expandCollapse] { 
      border-radius: 5px; 
      padding: 0 25px 0 3px;
      margin: 1px; 
      position: absolute;
      top: 2px;
      right: 2px;
      height: 25px;
      text-align: center;
      font-size: 14px;
      line-height: 25px;
      color: #FFF;
      background: no-repeat right 3px top 50% #FF5707;
      background-size: 15px 15px;
    }
    #libraryHeaderContent[data-collapsed=true] a[data-id=expandCollapse] { background-image: url('/images/arrow-right-offwhite.png'); }
    #libraryHeaderContent[data-collapsed=false] a[data-id=expandCollapse] { background-image: url('/images/arrow-down-offwhite.png'); }
    #libraryHeaderContent a[data-id=expandCollapse]:hover, #libraryHeaderContent a[data-id=expandCollapse]:focus  {
      color: #FFF;
      background-color: #FF5707;
      box-shadow: 0 0 20px #FF5707;
    }

    /* Details */
    #pnlDetails { position: relative; min-height: 135px; }
    #pnlDetails #detailsAvatar { 
      border-radius: 5px;
      overflow: hidden;
      border: 1px solid #CCC;
      height: 125px;
      width: 125px;
      margin: 5px 5px 5px 0;
      position: absolute;
      top: 5px;
      left: 0;
      background-position: center center;
      background-size: contain;
      background-repeat: no-repeat;
    }
    #pnlDetails h4 { font-weight: normal; font-style: italic; color: #555; }
    #pnlDetails #detailsContent { margin-left: 130px; padding: 8px 0; }
    #pnlDetails #shareFollow, #pnlDetails #opinionBox { display: inline-block; width: 49%; vertical-align: top; }
    #pnlDetails #opinionBox { text-align: center; clear: both; }
    #pnlDetails #opinionRack { border: 1px solid #CCC; padding: 1px; border-radius: 5px; height: 23px; position: relative; font-size: 0; }
    #pnlDetails #likeBar, #pnlDetails #dislikeBar, #pnlDetails #likeText, #pnlDetails #dislikeText { height: 19px; display: inline-block; transition: width 0.5s; -webkit-transition: width 0.5s; }
    #pnlDetails #likeBar, #pnlDetails #likeText { left: 1px; }
    #pnlDetails #likeBar { background-color: #4AA394; border-radius: 5px 0 0 5px; float: left; }
    #pnlDetails #likeText { padding-left: 30px; background: url('/images/icons/icon_likes_white.png') no-repeat left center; }
    #pnlDetails #dislikeBar, #pnlDetails #dislikeText { right: 1px; }
    #pnlDetails #dislikeBar { background-color: #B03D25; border-radius: 0 5px 5px 0; float: right; }
    #pnlDetails #dislikeText { padding-right: 30px; background: url('/images/icons/icon_dislikes_white.png') no-repeat right center; }
    #pnlDetails #btnAddLike, #pnlDetails #btnAddDislike { width: 47%; font-size: 16px; margin: 1px; }
    #pnlDetails #likeText, #pnlDetails #dislikeText { font-size: 16px; position: absolute; top: 1px; color: #FFF; }
    #pnlDetails #likeBar.full, #pnlDetails #dislikeBar.full { border-radius: 5px; }
    #pnlDetails #description, #pnlDetails #opinionBox { display: inline-block; width: 49%; vertical-align: top; }
    /* Possibly temporary */
    #pnlDetails #btnAddDislike, #pnlDetails #dislikeText { display: none; }
    #pnlDetails #btnAddLike { width: 100%; }


    /* Comments */
    #pnlComments { position: relative; min-height: 135px; }
    #pnlComments #postCommentBox, #pnlComments #commentsBox { display: inline-block; vertical-align: top; }
    #pnlComments #postCommentBox { width: 25%; position: absolute; top: 25px; left: 0; }
    #pnlComments #commentsBox { width: 74%; margin-left: 26%; height: 115px; position: relative; font-size: 0; transition: height 1s; -webkit-transition: height 1s; }
    #pnlComments #txtCommentInput { width: 100%; height: 80px; }
    #pnlComments #btnPostComment { width: 100%; }
    #pnlComments #btnShowHideComments { width: 100%; position: absolute; bottom: 0; right: 0; box-shadow: 30px 0 30px 20px #EEE; transition: box-shadow 1s; -webkit-transition: box-shadow 1s; }
    #pnlComments .comment { border: 1px solid #DDD; border-radius: 5px; margin-bottom: 2px; font-size: 16px; position: relative; }
    #pnlComments .comment .owner { background-color: #DDD; padding: 2px 5px 2px 2px; width: 200px; text-align: right; width: 25%; }
    #pnlComments .comment .commentText { padding: 2px; font-size: 14px; line-height: 16px; margin: 0; width: 100%; width: 73%; }
    #pnlComments .comment .owner, #pnlComments .comment .commentText { display: inline-block; vertical-align: top; }
    #pnlComments .comment .owner .date { font-size: 12px; font-style: italic; color: #555; }
    #pnlComments .comment .owner .name { overflow: hidden; text-overflow: ellipsis; }

    /* Settings */
    #pnlSettings { font-size: 0; }
    #pnlSettings .third { 
      width: 33%; 
      font-size: 16px; 
      display: inline-block; 
      vertical-align: top; 
      padding-right: 5px; 
      position: relative; 
      min-height: 150px;
    }
    #pnlSettings .third:last-child { padding-right: 0; }
    #pnlSettings label { display: block; clear: both; margin: 5px 0; padding-left: 5px; }
    #pnlSettings label:hover { cursor: pointer; }
    #pnlSettings input[type=button], input[type=text], #pnlSettings textarea, #pnlSettings iframe, #pnlSettings label { width: 100%; }
    #pnlSettings #btnDeleteCollection { position: absolute; bottom: 0; left: 0; }
    #pnlSettings input[type=checkbox] { margin: 3px 5px 0 0; float: left; }
    #pnlSettings textarea { height: 90px; }
    #pnlSettings iframe { border: none; height: 24px; overflow: hidden; margin: 5px 0; }
    #pnlSettings .third:last-child input { margin-bottom: 5px; }
    #pnlSettings #btnSelectIcon { display: none; }
    #pnlSettings #accessLevelDDLs select { width: 100%; }
    #pnlSettings #accessLevelDDLs p { display: inline-block; }

    /* Add a Resource */
    #pnlAddResources { text-align: center; }
    #pnlAddResources > h3, #pnlAddResources > p { text-align: left; }
    #pnlAddResources .wayToAdd { padding: 0 5px;  text-align: left; display: inline-block; width: 49%; vertical-align: top; position: relative; margin-bottom: 15px; }
    #pnlAddResources .wayToAdd { padding: 0 5px;  text-align: left; display: inline-block; width: 32%; vertical-align: top; position: relative; margin-bottom: 15px; }
    #pnlAddResources h4 { background-color: #4AA394; color: #FFF; border-radius: 5px; }
    #pnlAddResources h4, #pnlAddResources p { padding-left: 20px; margin-left: 10px; }
    #pnlAddResources img { position: absolute; top: 0; left: 0; background-color: #4AA394; border-radius: 50%; }
    #pnlAddResources a { display: block; font-weight: bold; text-align: right; }

    #pnlJoinLibrary { position: relative; min-height: 160px; }
    #pnlJoinLibrary #joinExplanation { position: absolute; top: 25px; left: 0; width: 400px; }
    #pnlJoinLibrary #joinInput { width: 100%; padding-left: 410px; }
    #pnlJoinLibrary textarea { width: 100%; height: 6em; resize: none; }

    /* Share and Follow */
    /*#pnlShareFollow #followBox, #pnlShareFollow #shareBox { margin-bottom: 10px; padding-right: 5px; display: inline-block; vertical-align: top; width: 49%; margin-right: -4px; }*/
    #pnlShareFollow #txtShareBox { margin-bottom: 15px; }
    #pnlShareFollow #followBox select { width: 65%; margin-right: -4px; }
    #pnlShareFollow #followBox input { width: 32%; float: right; }
    #pnlShareFollow #btnFollowingUpdate { margin: 0; }
    #pnlShareFollow #widgetConfigList label { display: inline-block; vertical-align: top; width: 32%; padding: 2px 5px; }
    #pnlShareFollow #widgetConfigList label:hover { cursor: pointer; }

    /* Responsive */
    @media screen and (max-width:550px){
      #pnlDetails #shareFollow, #pnlDetails #opinionBox, #pnlSettings .third { width: 100%; display: block; }
      #pnlAddResources .wayToAdd { width: 100%; }
      #libTabBox a { font-size: 12px; }
      #pnlShareFollow #followBox, #pnlShareFollow #shareBox { width: 100%; display: block; }
      #pnlDetails #description, #pnlDetails #opinionBox { width: 100%; display: block; }
    }
    @media screen and (max-width:650px) {
      #pnlComments #postCommentBox, #pnlComments #commentsBox { width: 100%; position: static; margin: 5px 0; }
      .modBox_before select, .modBox_before select.halfWidth, .modBox_before input { width: 100%; }
      .modBox_before input { margin-top: 10px; }
      #pnlComments .comment .owner, #pnlComments .comment .commentText { width: 100%; display: block; text-align: left; }
      #pnlComments .comment .owner .date { position: absolute; top: 0; right: 2px; }
      #pnlComments .comment .owner .name { margin-right: 55px; }
    }
    @media screen and (max-width: 800px) {
      #pnlJoinLibrary #joinExplanation { position: static; width: auto; }
      #pnlJoinLibrary #joinInput { padding-left: 0; }
      #pnlShareFollow #widgetConfigList label { width: 49%; }
    }
    @media screen and (min-width: 1200px) {
      #pnlAddResources .wayToAdd { width: 24%; }
      #pnlAddResources .wayToAdd { width: 32%; }
    }
    @media screen and (max-width: 450px) {
      #pnlShareFollow #widgetConfigList label { display: block; width: 100%; }
    }

    /* Icons */
    .tab { background: 5px center no-repeat; width: 40px; height: 40px; }
    .tab[data-id=pnlDetails] { background-image: url('/images/icons/icon_library_bg.png'); }
    .tab[data-id=pnlShareFollow] { background-image: url('/images/icons/icon_loginaccount_bg.png'); }
    .tab[data-id=pnlComments] { background-image: url('/images/icons/icon_comments_bg.png'); }
    .tab[data-id=pnlSettings] { background-image: url('/images/icons/icon_myisle_bg.png'); }
    .tab[data-id=pnlAddResources] { background-image: url('/images/icons/icon_resources_bg.png'); }
    .tab[data-id=pnlJoinLibrary] { background-image: url('/images/icons/icon_swirl_bg.png'); }
    .tab[data-id=pnlActivity] { background-image: url('/images/icons/icon_click-throughs_bg.png'); }

    #libTabBox a.tab { width: auto; padding-left: 40px; }
    @media screen and (max-width: 700px) {
      #libTabBox a.tab { width: 40px; padding-left: 0; }
      #libTabBox a.tab span { display: none; }
    }

    /* Patches */
    #libraryHeader { padding-right: 35px; }
    #content .theme #searchHeader { margin-right: 5px; }
  </style>

  <uc1:ActivityRenderer id="activityRenderer" runat="server" />

<div id="content">
  <div id="libraryHeader">
    <div id="libTabBox">
      <a href="#" class="tab" data-id="pnlDetails" onclick="showPanel('pnlDetails'); return false;" title="Library/Collection"><span>Details</span></a>
      <a href="#" class="tab" data-id="pnlShareFollow" onclick="showPanel('pnlShareFollow'); return false;" title="Share & Follow"><span>Share &amp; Follow</span></a>
      <a href="#" class="tab" data-id="pnlComments" onclick="showPanel('pnlComments'); return false;" title="Comments"><span>Comments</span></a>
      <a href="#" class="tab" data-id="pnlSettings" onclick="showPanel('pnlSettings'); return false;" id="settingsTab" runat="server" title="Settings"><span>Settings</span></a>
      <a href="#" class="tab" data-id="pnlAddResources" onclick="showPanel('pnlAddResources'); return false;" id="addTab" runat="server" title="Add Resources"><span>Add Resources</span></a>
      <a href="#" class="tab" data-id="pnlJoinLibrary" onclick="showPanel('pnlJoinLibrary'); return false;" id="joinTab" runat="server" title="Become a Member of this Library"><span>Join This Library</span></a>
      <a href="#" class="tab" data-id="pnlActivity" onclick="showPanel('pnlActivity'); return false;" id="activityTab" title="Activity for this Library"><span>Activity</span></a>
    </div>
    <div id="libraryHeaderContent">
      <a href="#" data-id="expandCollapse" onclick="expandCollapsePanels(); return false;">Collapse</a>
      <h2 id="libColTitle"></h2>
      <div id="libColSelector">
        <a href="#" id="libraryAvatar" onclick="pickLibrary(); return false;"></a>
        <h2 id="collectionsHeader">Collections for this Library: <a class="toolTipLink" title="Collections|A Library contains one or more Collections.|A Collection contains one or more Resources.|Click on a Collection icon to view its contents.|Click on the Library icon to the left to view the entire contents of the Library."></a></h2>
        <div id="collectionList"></div>
            
      </div>
      <div id="libPanelHolder">
        <!-- pnlDetails -->
        <div id="pnlDetails" class="panel">
          <div id="detailsAvatar"></div>
          <div id="detailsContent">
            <h3 class="panelHeader"></h3>
            <p id="description"></p>
            <div id="opinionBox">
              <div id="opinionRack">
                <div id="likeBar"></div>
                <div id="dislikeBar"></div>
                <div id="likeText"></div>
                <div id="dislikeText"></div>
              </div>
              <div id="opinionButtons">
                <input type="button" id="btnAddLike" class="btn green" onclick="addLike(); return false;" value="Like" />
                <input type="button" id="btnAddDislike" class="btn red" onclick="addDislike(); return false;" value="Dislike" />
                <p class="middle" id="likeDislikeText"></p>
              </div>
            </div>
          </div>
        </div><!-- /pnlDetails -->
        <!-- pnlComments -->
        <div id="pnlComments" class="panel">
          <h3 class="panelHeader"></h3>
          <div id="postCommentBox">
            <textarea id="txtCommentInput"></textarea>
            <input type="button" class="btn green" id="btnPostComment" onclick="postComment()" value="Submit" />
          </div>
          <div id="commentsBox" data-collapsed="true">
            <div id="comments"></div>
            <input type="button" class="btn" id="btnShowHideComments" onclick="showHideComments()" value="Show/Hide More Comments" />
          </div>
        </div><!-- /pnlComments -->
        <!-- pnlSettings -->
        <div id="settingsPanel" runat="server" visible="false">
          <div id="pnlSettings" class="panel">
            <h3 class="panelHeader"></h3>
            <div class="third">
              <h4>Title</h4>
              <input type="text" id="txtTitle" />
              <h4>Description</h4>
              <textarea id="txtDescription"></textarea>
            </div>
            <div class="third">
              <h4>Upload Icon</h4>
              <input type="button" id="btnSelectIcon" class="btn" onclick="openIconPicker(); return false;" value="Icon Picker..." />
              <iframe id="avatarFrame" allowtransparency="true" scrolling="no" src="/My/Avatar.aspx?guid=<%=userGUID %>&lib=<%=libraryID %>"></iframe>
              <div id="accessLevelDDLs" >
                <p>Set general public access level:</p>
                <a class="toolTipLink" title="Public Access Level|<ul><li><b>None</b> - The library has no default access, and is hidden from searches.</li><li><b>By Request Only</b> - The library has no default access, but will enable requests to access the library. The library can be found by a search.</li><li><b>Read Only</b> - The library can be viewed by anyone and will be displayed in searches.</li><li><b>Contribute with Approval</b> - The library is publically available and any authenticated user may add a resource to the library. The resource will not be visible until it has been approved by a library curator or administrator.</li><li><b>Contribute No Approval</b> - Same as the latter, except no approval is required.</li></ul>"></a>
                <asp:DropDownList ID="ddlPublicAccessLevels" CssClass="ddlPublicAccessLevels" runat="server"></asp:DropDownList>
                 
                <p>Set organization member access level: </p>
                <a class="toolTipLink" title="Organization Access Level|<ul><li><b>None</b> - The library has no default access for members of the related organization, and is hidden from searches. </li><li><b>By Request Only</b> - The library has no default access for members of the related organization, but will enable requests to access the library. The library can be found by a search.</li><li><b>Read Only</b> - The library can be viewed by any member the related organization and will be displayed in searches.</li><li><b>Contribute with Approval</b> - The library is publically available and any member of the related organization may add a resource to the library. The resource will not be visible until it has been approved by a library curator or administrator.</li><li><b>Contribute No Approval</b> - Same as the latter, except no approval is required.</li></ul>"></a>
                <asp:DropDownList ID="ddlOrganizationAccessLevels" CssClass="ddlOrganizationAccessLevels" runat="server"></asp:DropDownList>
              </div>
              <div id="defaulter">
                <p>This is your Default Collection</p>
                <label id="lblMakeDefault" for="makeDefault"><input type="checkbox" id="makeDefault" />Make this my Default Collection</label>
              </div>
            </div>
            <div class="third">
              <input type="button" class="btn green" id="btnSaveSettings" onclick="saveSettings(); return false;" value="Save Changes" />
              <input type="button" class="btn" id="btnCreateCollection" onclick="createNewCollection(); return false;" value="Create New Collection" />
              <input type="button" class="btn red" id="btnDeleteCollection" onclick="deleteCollection(); return false;" value="Delete This Collection" />
            </div>
          </div><!-- /pnlSettings -->
        </div>
        <!-- addResourcesPanel -->
        <div id="addResourcesPanel" runat="server">
          <div id="pnlAddResources" class="panel">
            <h3 class="panelHeader">Add Resources...</h3>
            <p>You can easily add Resources to your Library in several ways:</p>
            <div class="wayToAdd" data-id="fromSearch">
              <img src="/images/icons/icon_search_bg.png" />
              <h4>From the Search</h4>
              <p>Find a wide range of Resources to add to your Library.</p>
              <a href="/Search.aspx">Find Resources &rarr;</a>
            </div>
            <div class="wayToAdd" data-id="fromLibrary">
              <img src="/images/icons/icon_library_bg.png" />
              <h4>From another Library</h4>
              <p>Add from Libraries of Resources hand-picked by users like you.</p>
              <a href="/Libraries/Default.aspx">Browse Libraries &rarr;</a>
            </div>
            <div class="wayToAdd" data-id="fromYou">
              <img src="/images/icons/icon_swirl_bg.png" />
              <h4>From You</h4>
              <p>Quickly tag or upload a Resource you found or created!</p>
              <a href="/Contribute/">Get Started &rarr;</a>
            </div>
            </div> 
          </div>
          <!-- /pnlAddResources -->
          <!-- pnlJoinLibrary -->
          <div id="joinLibraryPanel" runat="server">
            <div id="pnlJoinLibrary" class="panel">
              <h3 class="panelHeader">Become a Member of this Library</h3>
              <div id="joinExplanation">
                <p>Library Members have access to add, move, and remove resources and collections within the Library, at the discretion of Library admin(s).</p>
                <p>To join this Library, please send a brief message to the Library administrator(s) describing why you want to join:</p>
              </div>
              <div id="joinInput">
                <textarea id="txtIJoinBecause"></textarea>
                <input type="button" class="btn green" id="btnRequestJoin" value="Send Request" onclick="requestJoinLibrary()" />
              </div>
            </div>
          </div>
          <!-- /pnlJoinLibrary -->
          <!-- pnlShareFollow -->
          <div id="pnlShareFollow" class="panel">
            <!--<h3 class="panelHeader">Share and Follow</h3>-->
            <div id="shareFollowContent">
              <div id="shareFollow">
                <div id="shareBox">
                  <div class="column">
                    <h4 class="shareLinkHeader">Share:</h4>
                    <input type="text" readonly="readonly" id="txtShareBox" onclick="this.select()" />
                  </div>
                  <div class="column">
                    <h4>Embed this Library:</h4>
                    <p>Select up to 10 publicly-available collections, listed below. Up to 10 of the most recent resources from each will be displayed.</p>
                    <input type="text" readonly="readonly" id="txtWidgetConfig" onclick="this.select()" />
                    <div id="widgetConfigList"></div>
                  </div>
                </div>
                <div id="followBox">
                  <p class="middle" id="followingMessage">You are already following this entire Library.</p>
                  <div id="followingControls">
                    <h4>Follow:</h4>
                    <select id="followingOptions">
                      <option value="0">Not Following</option>
                      <option value="1">Follow without email notifications</option>
                      <option value="2">Follow with weekly email updates</option>
                      <option value="3">Follow with daily email updates</option>
                    </select>
           <%--                             <option value="4">Follow with immediate email updates</option>--%>
                    <input type="button" id="btnFollowingUpdate" onclick="updateFollowingOption(); return false;" value="Save" class="btn green" />
                  </div>
                </div>
              </div>
            </div>
          </div>
          <!-- /pnlShareFollow -->
          <!-- pnlActivity -->
          <div id="pnlActivity" class="panel">
            <script type="text/javascript">
              var activityData = <%=activityJSON %>;

              $(document).ready(function() {
                $("#libraryActivity").html(getActivity("totals", activityData.Activity, { "timespan": "", "timescale": "day(s)", "library_views": "Library Views:", "resource_views": "Resource Views" }));
                if(activityData.ChildrenActivity.length == 0){
                  $("#collectionsActivity").html("<p>No recent activity.</p>");
                }
                else {
                  for(i in activityData.ChildrenActivity){
                    $("#collectionsActivity").append(getActivity("totals", activityData.ChildrenActivity[i], { "timespan": "", "timescale": "day(s)", "collection_views": "Collection Views:", "resource_views": "Resource Views" }));
                  }
                }
              });
            </script>
            <style type="text/css">
              .activity.divbars .activityItem { margin: 5px 0.5%; display: inline-block; vertical-align: top; width: 32%; }
              .activity.divbars .activityItem .title, .activity.divbars .activityItem .activityData { display: inline-block; vertical-align: middle; }
              .activity.divbars .activityItem .title { width: 125px; text-align: right; padding: 5px; }
              .activity.divbars .activityItem .activityData { width: calc(100% - 125px); }
              @media (max-width: 850px) {
                .activity.divbars .activityItem .title { width: 100px; }
                .activity.divbars .activityItem .activityData { width: calc(100% - 100px); }
                .activity.divbars .activityItem { width: 100%; display: block; padding: 0 10px; margin: 10px 0; }
              }
            </style>
            <h3 class="panelHeader">Activity for this Library (Last <%=activityDaysAgo.Text %> days):</h3>
            <div id="libraryActivity"></div>
            <h3>Activity for this Library's Collections (Last <%=activityDaysAgo.Text %> days):</h3>
            <div id="collectionsActivity"></div>
          </div>
          <!-- /pnlActivity -->
        </div> <!-- /libPanelHolder -->
      </div><!-- /libraryHeaderContent -->
    </div> <!-- /libraryHeader -->
  
  <uc1:Search ID="searchControl" runat="server" />
</div><!-- /libraryStuff -->


</div>
<div id="templates" style="display:none;">
  <script type="text/template" id="template_collectionIcon">
    <a href="#" onclick="pickCollection({id}); return false;" data-collectionID="{id}" class="collectionIcon" title="{title}">
      <img alt="{title}" class="iconImg" />
      <div class="collectionTitle">{title}</div>
    </a>
  </script>
  <script type="text/template" id="template_comment">
    <div class="comment" data-id="{id}">
      <div class="owner">
        <div class="name" title="{name}">{name}</div>
        <div class="date">{date}</div>
      </div>
      <p class="commentText">{text}</p>
    </div>
  </script>
  <script type="text/template" id="template_ddl">
    <select data-vid="{vid}" data-intID="{intID}"></select>
  </script>
</div>


<div id="error" runat="server" style="padding: 50px 5px; text-align: center" visible="false"></div>
<div id="noLibraryYet" runat="server" style="padding: 25px;" visible="false">
  <style type="text/css">
    p { text-align: center; }
    .newContent { max-width: 600px; margin: 0 auto; }
    b { display: block; }
    input, textarea { display: block; width: 100%; margin-bottom: 20px; max-width: 100%; min-width: 100%; resize: none; }
    .btnCreateLibrary { font-weight: bold; font-size: 20px; color: #FFF; background-color: #4AA394; border-radius: 5px; }
    .btnCreateLibrary:hover, .btnCreateLibrary:focus { background-color: #FF5707; cursor: pointer; }
  </style>
  <script type="text/javascript">
    function validateNewLibrary() {
      var title = $(".txtNewTitle");
      var description = $(".txtNewDescription");
      var image = $(".fileNewImage");

      var sendTitle = validateText(title, 10, "Please enter a title 10 characters or longer.");
      var sendDescription = validateText(description, 20, "Please enter a description 20 characters or longer.");
      var sendImage = validateText(image, 12, "You must select an image for your Library.");

      if(sendTitle == null || sendDescription == null || sendImage == null){
        return false;
      }

      $("form").removeAttr("onsubmit");
      return true;
    }

    function validateText(box, minimum, message){
      var val = box.val().replace(/</g, "").replace(/>/g, "");
      if(val.length < minimum){
        alert(message);
        return null;
      }
      return val;
    }
  </script>
  <p>You haven't created your Library yet. an IOER Library is a great way to store and organize Resources, and share them with others.</p>
  <p>To create your Library, we need some basic information. You can change all of this later.</p>
    <p>Just want the basics? Click the following button to have the system create your library and a default collection
        <asp:Button ID="quickLibrary" runat="server" Text="Automatically Create My Library" OnClick="quickLibrary_Click" CssClass="defaultButton" />

    </p>
  <div class="newContent">
    <b>Title:</b> 
    <asp:TextBox ID="txtTitleNew" runat="server" CssClass="txtNewTitle" />
    <b>Brief Description:</b> 
    <asp:TextBox TextMode="MultiLine" ID="txtDescriptionNew" runat="server" CssClass="txtNewDescription" />
    <b>Public Access Level</b>
    <asp:dropdownlist id="ddlPublicAccessLevel" runat="server" Width="250px"></asp:dropdownlist>
   <b>Organization Access Level</b> 
   <asp:dropdownlist id="ddlOrgAccessLevel" runat="server" Width="250px"></asp:dropdownlist>

    <b>Image (large images will be resized to about 100 x 100 pixels):</b>
    <asp:FileUpload ID="fileNewImage" runat="server" CssClass="fileNewImage" />
    <asp:Button ID="BtnCreateLibrary" runat="server" CssClass="btnCreateLibrary" OnClick="BtnCreateLibrary_Click" Text="Create My Library Now!" OnClientClick="validateNewLibrary()" />
  </div>
    <asp:Label ID="completionMessage" runat="server" Visible="false">
        <p>Your library was created for you!  <br />Your library is public, if you would like to change access to your library, use the Settings tab.</p><p>Within your library is a default collection; you may add collections to your library and share with others.  Use the Settings tab to add titles, descriptions and images.</p>  </asp:Label>

<asp:Literal ID="libraryCreateMsg" runat="server" Visible="false">Your personal library was created. <br />Be sure to review the getting started guide for information on libraries</asp:Literal>
  <asp:Literal ID="activityDaysAgo" runat="server" Visible="false">90</asp:Literal>
</div>

