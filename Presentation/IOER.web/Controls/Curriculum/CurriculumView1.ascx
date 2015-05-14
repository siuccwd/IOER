<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CurriculumView1.ascx.cs" Inherits="ILPathways.Controls.Curriculum.CurriculumView1" %>
<%@ Register TagPrefix="uc1" TagName="ActivityRenderer" Src="/Activity/ActivityRenderer.ascx" %>

<% if(curriculumContent.Visible) { %>
<div id="curriculumContent" runat="server">

  <script type="text/javascript">
    //From server
    var files = <%=filesList %>;
    var hasFeaturedItem = <%=(hasFeaturedItem ? "true" : "false") %>;
    var curriculumID = <%=curriculumNode.Id %>;
    var nodeID = <%=currentNode.Id %>;
    var comments = <%=commentsList %>;
    var isWidget = <%=( isWidget ? "true" : "false" ) %>;

    //Functions that need to be done here
    function goToSibling(id){
      window.location.href = "<% Response.Write( GetUrl(curriculumNode.Id, -99) ); %>".replace("-99", id); //hack
    }

    //Detect IE
    var isIEBrowser = window.navigator.userAgent.indexOf("Trident") > -1;
  </script>
  <script type="text/javascript" src="/scripts/jscommon.js"></script>
  <script type="text/javascript" src="/Controls/Curriculum/curriculumview1.js"></script>
  <link rel="stylesheet" href="/controls/curriculum/curriculumview1.css" type="text/css" />

  <uc1:ActivityRenderer id="activityRenderer" runat="server" />

  <!-- The header -->
  <div id="curriculumHeader">
    <% var avatar = curriculumNode.ImageUrl.Replace( @"\", "" );
       if ( string.IsNullOrWhiteSpace( avatar ) )
       {
         avatar = string.IsNullOrWhiteSpace( curriculumNode.ResourceThumbnailImageUrl ) ? "/images/ioer_med.png" : curriculumNode.ResourceThumbnailImageUrl.Replace( @"\", "" ); ;
       }
    %>
    <div style="background-image: url('<%=avatar %>');" id="curriculumAvatar"></div>
    <h1 class="isleH1" id="curriculumTitle"><%=curriculumNode.Title %></h1>
    <input type="button" id="btnShowTools" class="isleButton bgBlue" onclick="toggleTools()" value="Learning List Menu" />
    <div id="breadcrumbs">
      <% foreach ( var item in currentNodeParents ) {%>
        <a href="<% Response.Write( GetUrl( curriculumNode.Id, item.Item2 ) ); %>"><%=item.Item1 %></a> <span class="separator">&rarr;</span>
      <% } %>
    </div>
  </div>

  <!-- The toolbox -->
  <div id="tools" class="grayBox">
    <h2 class="header"><input type="button" value="x" class="closeButton" onclick="hideTools();" />Learning List Menu <% if(isWidget && !IsUserAuthenticated()){ %><a class="mainLoginLink" href="//ioer.ilsharedlearning.org/Account/Login.aspx?hidechrome=1&nextUrl=<%=Request.Url.PathAndQuery %>">Login with your IOER Account</a><% } %></h2>
    <div id="buttons">
      <input type="button" class="isleButton bgBlue selected" value="Learning List Map" data-id="tab_curriculumMap" onclick="toggleTab('tab_curriculumMap')" />
      <input type="button" class="isleButton bgBlue" value="Help & Info" data-id="tab_helpInfo" onclick="toggleTab('tab_helpInfo')" />
      <input type="button" class="isleButton bgBlue" value="Timeline & Follow Updates" data-id="tab_timeline" onclick="toggleTab('tab_timeline')" />
      <input type="button" class="isleButton bgBlue" value="Activity & Statistics" data-id="tab_activity" onclick="toggleTab('tab_activity')" />
      <input type="button" class="isleButton bgBlue" value="Embed Widget" data-id="tab_embed" onclick="toggleTab('tab_embed')" />
      <input type="button" class="isleButton bgBlue" value="Like & Comment" data-id="tab_community" onclick="toggleTab('tab_community')" />
    </div>
    <div id="tabs">
      <!-- Help and Information -->
      <div class="tab" id="tab_helpInfo">
        <h2 class="mid">IOER Information</h2>
        <p>The <a href="http://ilsharedlearning.org/" target="_blank">Illinois Shared Learning Envrionment (ISLE)</a> hosts the <a href="http://ioer.ilsharedlearning.org/" target="_blank">ISLE Open Education Resources (IOER)</a> system with the goal of helping teachers connect with learning resources and each other.</p>
        <h2 class="mid">IOER Learning List Help &amp; Information</h2>
        <p>The IOER Learning List Tool provides quick and easy access to a Learning List.  A learning list is any organized group of resources and/or files, often in a sequential order. It can be as small as a lesson or as large as a curriculum. It can be constructed by a user or team of users, and has several features, including:</p>
        <ul>
          <li>Allowing users to browse every detail of the Learning List</li>
          <li>Restricting access to assessments and answer keys</li>
          <li>Sharing the learning list directly or as a widget on your website</li>
        </ul>
        <h2 class="mid">Understanding the Learning List Tool</h2>
        <h3>Learning List Map</h3>
        <p>The Learning List Map gives you an overview of the entire Learning List and lets you jump directly to any node in it.</p>
        <h3>Timeline &amp; Follow Updates</h3>
        <p>This section allows you to view and follow updates to this Learning List.</p>
        <h3>Embed Widget</h3>
        <p>The Learning List can be displayed as a widget on your site. For more information, click on the "Embed Widget" button above.</p>
        <h3>Like &amp; Comment</h3>
        <p>Clicking on the "Like &amp; Comment" button above will allow you to like and comment on the node you are currently viewing.</p>
      </div>
      <!-- Curriculum Map -->
      <div class="tab showing" id="tab_curriculumMap">
        <h2 class="mid">Learning List Map <input type="button" id="btnCurriculumExpandCollapseAll" class="isleButton bgBlue" value="Collapse All" onclick="toggleCurriculumExpandCollapseAll(this)" /></h2>
        <%=curriculumMapHTML %>
      </div>
      <!-- Timeline -->
      <div class="tab" id="tab_timeline">
        <h2 class="mid">Timeline for this Learning List</h2>
        <div class="columns">
          <% if(userGUID != "") { %>
          <select id="ddlTimelineSubscribe" class="column">
            <option value="0" selected>Not Following this Learning List</option>
            <option value="1">Follow this Learning List without Email Updates</option>
            <option value="2">Follow this Learning List with Daily Email</option>
            <option value="3">Follow this Learning List with Weekly Email</option>
          </select>
          <input type="button" class="isleButton bgBlue column" value="Save" id="btnFollow" onclick="updateFollowing(<%=curriculumNode.Id %>);" />
          <% } %>
          <% else { %>
          <p class="grayMessage">Please login to follow this Learning List.</p>
          <% } %>
        </div>
        <div id="timelineContent">
          <% if ( history.Count() > 0 ) { %>
            <% foreach(var item in history) { %>
            <div class="event">
              <div class="date">Update <%=item.Date %></div>
              <div class="text"><%=item.Text %></div>
            </div>
            <% } %>  
          <% } else { %>
          <p class="grayMessage">No Timeline Events found for this Learning List</p>
          <% } %>
        </div>
      </div>
      <!-- Activity -->
      <div class="tab" id="tab_activity">
        <h2 class="mid">Activity for this Learning List in the last <%=activityDaysAgo.Text %> days</h2>
        <script type="text/javascript">
          var activityData = <%=activityJSON %>;

          $(document).ready(function() {
            $("#activityBlock").html(getActivity("totals", activityData.Activity, {timespan: "", timescale: "day(s)", object_views: "Views (This layer):", resource_views: null, total_views: "Views (This layer and below):", parent_downloads: "Downloads (This layer):", total_downloads: "Downloads (This layer and below):" }));
            for(i in activityData.ChildrenActivity){
              $("#activityChildrenBlock").append(getActivity("totals", activityData.ChildrenActivity[i], { timespan: "", timescale: "day(s)", child_views: "Views:", child_downloads: "Downloads:" }));
            }
          });
        </script>
        <style type="text/css">
          #tab_activity .activity.totals { width: 48%; min-width: 300px; }
        </style>
        <div id="activityBlock"></div>
        <h2 class="mid">Applicable activity directly below this Layer in the last <%=activityDaysAgo.Text %> days</h2>
        <div id="activityChildrenBlock"></div>
      </div>
      <!-- Embed -->
      <div class="tab" id="tab_embed">
        <h2 class="mid">Embed This</h2>
        <p>To embed this <b>Learning List</b> as a widget in your page, copy this into your site's HTML:</p>
        <input type="text" id="widgetLink_curriculum" class="widgetLink" readonly="readonly" value="<iframe src='//ioer.ilsharedlearning.org/widgets/learninglist/?node=<%=curriculumNode.Id %>'></iframe>" />
        <% if(currentNode.Id != curriculumNode.Id) { %>
        <p>To embed <b>this node</b> as a widget in your page, use this instead:</p>
        <input type="text" id="widgetLink_node" class="widgetLink" readonly="readonly" value="<iframe src='//ioer.ilsharedlearning.org/widgets/learninglist/?node=<%=currentNode.Id %>'></iframe>" />
        <% } %>
        <p>If your site uses <b><a href="http://jquery.com/" target="_blank">jQuery</a></b>, you can also add the following line to enable a more seamless self-resizing widget:</p>
        <input type="text" id="widgetLink_resizer" class="widgetLink" readonly="readonly" value="<script src='//ioer.ilsharedlearning.org/scripts/widgets/postmessagereceiver.js'></script>" />
        <p>For more information, check the <a href="/widgets" <%=( isWidget ? "target='_blank'" : "" ) %>>IOER Widgets page</a>.</p>
      </div>
      <!-- Community -->
      <div class="tab" id="tab_community">
        <h2 class="mid">Community Opinion</h2>
        <div id="communityColumns" class="columns">
          <div id="communityLeftColumn" class="column">
            <h3>Comments on this Node</h3>
            <% if(userGUID != "") { %>
            <textarea id="txtComment"></textarea>
            <input type="button" id="btnComment" class="isleButton bgBlue" onclick="comment();" value="Comment" />
            <% } %>
            <% else { %>
            <p class="grayMessage">Please login to comment.</p>
            <% } %>
            <div id="commentsList">
              <% //Comments go here %>
              <p class="grayMessage">No comments found for this Node</p>
            </div>
          </div>
          <div id="communityRightColumn" class="column">
            <h3>Likes</h3>
            <div class="column" id="likes">
              <div class="likeBox columns" id="likeBox_curriculum">
                <span class="column" id="likeCount_curriculum"><span><%=curriculumLikes.LikeCount %></span> Learning List Likes</span>
                <% if(curriculumLikes.YouLikeThis){ %>
                  <span class="grayMessage">You like this.</span>
                <% } else if(userGUID != "") { %>
                  <input type="button" class="isleButton bgBlue" id="likeButton_curriculum" value="Like" onclick="like(<%=curriculumNode.Id %>, this, 'curriculum');" />
                <% } %>
              </div>
              <% if( curriculumNode.Id != currentNode.Id) { %>
              <div class="likeBox columns" id="likeBox_node">
                <span class="column" id="likeCount_node"><span><%=nodeLikes.LikeCount %></span> Node Likes</span>
                <% if(nodeLikes.YouLikeThis){ %>
                  <span class="grayMessage">You like this.</span>
                <% } else if(userGUID != "") { %>
                  <input type="button" class="isleButton bgBlue"id="likeButton_node" value="Like" onclick="like(<%=currentNode.Id %>, this, 'node');" />
                <% } %>
              </div>
              <% } %>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- Node information -->
  <div id="node">
    <div id="leftColumn">
      <h2 id="nodeTitle"><%=currentNode.Title %></h2>
      <div id="nodeDescription">
        <%=(string.IsNullOrWhiteSpace( currentNode.Description ) ? currentNode.Summary : currentNode.Description) %>
      </div>

    <% if( allStandards.Count() > 0){ %>
      <% if( true ) { //if usingContentStandards %>
        <div id="alignedStandards">
          <h3><input type="button" class="isleButton bgBlue" id="btnToggleAllStandards" onclick="toggleAllStandards();" value="Expand All" />Aligned Standards </h3>
          <div id="standardsLevelButtons">
            <input type="button" class="isleButton stdMajor" id="btnStandardsMajor" value="Major" onclick="showStandardsType('major');" />
            <input type="button" class="isleButton stdSupporting" id="btnStandardsSupporting" value="Supporting" onclick="showStandardsType('supporting');" />
            <input type="button" class="isleButton stdAdditional" id="btnStandardsAdditional" value="Additional" onclick="showStandardsType('additional');" />
          </div>
          <div class="standardsLevel grayBox showing" data-levelID="major">
            <h4 class="header stdMajor">Major Standards</h4>
            <ul class="standardsList">
            <% foreach( var item in allStandards.Where(s => s.usageID == 1).ToList() ) { %>
              <li><input type="button" value="<%=item.code %>" onclick="showStandard('major_<%=item.recordID %>');" /><div class="standardText" data-standardID="major_<%=item.recordID %>"><%=item.text %></div></li>
            <% } %>
            </ul>
          </div>
          <div class="standardsLevel grayBox" data-levelID="supporting">
            <h4 class="header stdSupporting">Supporting Standards</h4>
            <ul class="standardsList">
            <% foreach ( var item in allStandards.Where( s => s.usageID == 2 ).ToList() )
               { %>
              <li><input type="button" value="<%=item.code %>" onclick="showStandard('supporting_<%=item.recordID %>');" /><div class="standardText" data-standardID="supporting_<%=item.recordID %>"><%=item.text %></div></li>
            <% } %>
            </ul>
          </div>
          <div class="standardsLevel grayBox" data-levelID="additional">
            <h4 class="header stdAdditional">Additional Standards</h4>
            <ul class="standardsList">
            <% foreach ( var item in allStandards.Where( s => s.usageID == 3 ).ToList() )
               { %>
              <li><input type="button" value="<%=item.code %>" onclick="showStandard('additional_<%=item.recordID %>');" /><div class="standardText" data-standardID="additional_<%=item.recordID %>"><%=item.text %></div></li>
            <% } %>
            </ul>
          </div>
        </div>
      <% } else { %>
        <div id="alignedStandards">
          <h3><input type="button" class="isleButton bgBlue" id="btnToggleAllStandards" onclick="toggleAllStandards();" value="Expand All" />Aligned Standards </h3>
          <ul class="standardsList">
          <% foreach( var item in allStandards ) { %>
            <li><input type="button" value="<%=item.code %>" onclick="showStandard(<%=item.recordID %>);" /><div class="standardText" data-standardID="<%=item.standardID %>"><%=item.text %></div></li>
          <% } %>
          </ul>
        </div>
      <% } %>
    <% } %>


    <% if(currentNode.ChildItems.Count() > 0) { %>
      <div id="filesBox" class="<%=(currentNode.ChildItems.Count() >= 3 ? "centered" : "") %> <%=( hasFeaturedItem ? "scrolling" : "grid" ) %>">
        <h3>Files <% if( currentNode.ChildItems.Where(m => m.CanViewDocument).Count() > 0 ) { %> <a href="/Repository/DownloadFiles.aspx?all=false&nid=<%=currentNode.Id %>" class="downloadFilesLink"><img src="/images/icons/download-orange.png" /> Download These Files</a><% } %></h3>
        <% if( currentNode.ChildItems.Where( m => m.CanViewDocument == false ).Count() > 0 ) { %>
          <p class="grayMessage">Private documents below can only be accessed by authorized users. <a href="//ioer.ilsharedlearning.org/Account/Login.aspx?<%=( isWidget ? "hidechrome=1&" : "" ) %>nextUrl=<%=Request.Url.PathAndQuery %>">Login</a></p>
        <% } %>
        <div id="resources">
          <% foreach( var item in currentNode.ChildItems.OrderBy( m => m.SortOrder ).ThenBy( m => m.Id ) ) { %>
            <% if(item.CanViewDocument){ %>
            <div class="file grayBox">
              <a href="<%=item.DocumentUrl %>">
                <% 
                  var thumbnailURL = item.ResourceThumbnailImageUrl;
                  var isArchive = item.DocumentUrl.Contains( ".zip" ) || item.DocumentUrl.Contains( ".rar" );
                  if ( string.IsNullOrEmpty( item.ResourceThumbnailImageUrl ) || string.IsNullOrEmpty( item.ResourceFriendlyUrl ) )
                  {
                    if ( System.IO.File.Exists( @"\\OERDATASTORE\OerThumbs\large\content-" + item.Id + "-large.png" ) )
                    {
                      thumbnailURL = "/OERThumbs/large/content-" + item.Id + "-large.png";
                    }
                    else 
                    {
                      thumbnailURL = "/images/icons/icon_upload_400x300.png";
                    }
                  }
                  else if ( isArchive )
                  {
                    thumbnailURL = "/images/icons/icon_zip_400x300.png";
                  }
                %>
                <img src="/images/ThumbnailResizer.png" style="background-image:url('<%=thumbnailURL %>')" />
                <input type="button" class="iconButton preview" onclick="preview(<%=item.Id %>, <%=(isArchive ? "true" : "false") %>);" title="Preview" />
                <div><%=item.Title %></div>
              </a>
              <% if(!string.IsNullOrWhiteSpace(item.ResourceFriendlyUrl)){ %>
              <a href="<%=item.ResourceFriendlyUrl %>" target="_blank" title="View Tags" class="iconButton tags"></a>
              <% } %>
            </div>
            <% } else { %>
            <div class="file grayBox private">
              <p class="grayMessage"><%=(string.IsNullOrWhiteSpace(item.DocumentPrivacyMessage) ? "You do not have permission to view this document." : item.DocumentPrivacyMessage ) %></p>
            </div>
            <% } %>
          <% } %>
        </div>
      </div>
    <% } %>

    <% if( hasFeaturedItem ) { //If there is a featured item %>
      <div id="featuredPreview">
        <h3>Featured File</h3>
        <%  //Temporary? fix for auto preview URL
            if ( 
              currentNode.AutoPreviewUrl.Length > 0 //if there is a url
              && currentNode.AutoPreviewUrl.IndexOf("/") == 0 //and it starts with "/" and is thus relative
              && currentNode.AutoPreviewUrl.IndexOf( "ilsharedlearning.org" ) == -1 //and it doesn't already contain our site URL
              ) 
            { 
                currentNode.AutoPreviewUrl = "//ioer.ilsharedlearning.org" + currentNode.AutoPreviewUrl; 
            }

            var url = "";
            if ( currentNode.AutoPreviewUrl.ToLower().IndexOf( "contentdocs" ) > -1 )
            {
              //Detect IE
              var isIEBrowser = Request.UserAgent.IndexOf( "Trident" ) > -1;
              //Default to google previewer
              url = "http://docs.google.com/viewer?embedded=true&url=" + Uri.EscapeUriString( currentNode.AutoPreviewUrl.IndexOf( "http" ) == 0 ? currentNode.AutoPreviewUrl : "http:" + currentNode.AutoPreviewUrl );
              //If PDF and not IE, show directly
              if ( currentNode.AutoPreviewUrl.ToLower().IndexOf( ".pdf" ) > -1 && !isIEBrowser )
              {
                url = currentNode.AutoPreviewUrl;
              }
              //Use office previewer only if relevant
              var officeTypes = new List<string>() { ".doc", ".ppt", ".xls" };
              foreach ( var item in officeTypes )
              {
                if ( currentNode.AutoPreviewUrl.ToLower().IndexOf( item ) > -1 )
                {
                  url = "http://view.officeapps.live.com/op/view.aspx?src=" + Uri.EscapeUriString( currentNode.AutoPreviewUrl.IndexOf( "http" ) == 0 ? currentNode.AutoPreviewUrl : "http:" + currentNode.AutoPreviewUrl );
                }
              }
            }
            else
            {
              url = currentNode.AutoPreviewUrl;
            }
        %>
        <iframe id="featuredPreviewFrame" src=""></iframe>
        <script type="text/javascript">
          $(document).ready(function() {
            if($(window).width() > 500){
              $("#featuredPreviewFrame").attr("src", "<%=url %>");
            }
            else {
              $("#featuredPreview").remove();
            }
          });
        </script>
      </div>
    <% } %>

    </div><div id="rightColumn">
      <div class="grayBox">
        <h3 class="header">Explore Content</h3>
        <% if(currentNode.Id != curriculumNode.Id){  %>
          <h3 class="mid">Download Files</h3>
          <a href="/Repository/DownloadFiles.aspx?nid=<%=currentNode.Id %>" class="downloadFilesLink"><img src="/images/icons/download-orange.png" /> Download all Files in and below this layer</a>
        <% } %>

        <% var children = new List<ILPathways.Services.CurriculumService.JSONNode>();  %>
        <% var treeNode = GetFromTree( tree, currentNode.Id ); %>
        <% if( treeNode != null && treeNode.children.Count > 0)
               children = treeNode.children; %>
        <% if(children.Count() > 0){ %>
        <h3 class="mid">Deeper Content</h3>
        <p class="grayMessage">Explore the next layer of this branch of <input type="button" class="btnOpenMap" value="the Learning List." onclick="toggleTools(); toggleTab('tab_curriculumMap');" /></p>
          <div class="subNodes">
          <% foreach(var item in children ) { %>
          <div class="subNode">
            <a href="<% Response.Write( GetUrl( curriculumNode.Id, item.id ) ); %>"><%=item.title %></a>
            <p><%=item.description.Length > 75 ? item.description.Substring(0,72) + "..." : item.description %></p>
          </div>
          <% } %>
        </div>
        <% } else { %>
        <%--<p class="grayMessage">You have reached the deepest layer of this branch of <input type="button" class="btnOpenMap" value="the Curriculum." onclick="toggleTools(); toggleTab('tab_curriculumMap');" /></p>--%>
        <% } %>


        <% var siblings = currentNode.Id == curriculumNode.Id ? null : GetFromTree( tree, currentNode.ParentId ).children; %>
        <% if(siblings != null && children.Count() == 0){ %>
        <h3 class="mid">More Content</h3>
        <p class="grayMessage">Explore other items in this layer of this branch of <input type="button" class="btnOpenMap" value="the Learning List." onclick="toggleTools(); toggleTab('tab_curriculumMap');" /></p>
        <div class="subNodes">
          <% foreach(var item in siblings ) { %>
          <div class="subNode <% if ( item.id == currentNode.Id ) { Response.Write( "current" ); } %>">
            <a href="<% Response.Write( GetUrl( curriculumNode.Id, item.id ) ); %>"><%=item.title %></a>
            <p><%=item.description.Length > 75 ? item.description.Substring(0,72) + "..." : item.description %></p>
          </div>
          <% } %>
        </div>
        <% } %>
        <div id="siblingButtons">
          <input type="button" class="isleButton bgBlue" id="btnPrevNode" value="Previous" />
          <input type="button" class="isleButton bgBlue" id="btnNextNode" value="Next" />
        </div>
      </div>
    </div>
  </div>

  <div id="previewerOverlay">
    <div id="previewer" class="grayBox">
      <h2 class="header"><input type="button" value="x" class="closeButton" onclick="hidePreview();" /><span id="googlePreviewerLink" class="previewerLink">(Not working? Try the <input type="button" class="isleButton bgGreen" onclick="previewWithGoogle();" value="Google Previewer" />) </span><span id="officePreviewerLink" class="previewerLink">(Not working? Try the <input type="button" class="isleButton bgGreen" onclick="previewWithOffice();" value="MS Office Previewer" />) </span>Resource <span id="previewerTracker"></span>: <a href="#" id="previewerTitle"></a> <img src="/images/icons/download-orange.png" /> </h2>
      <iframe id="previewerFrame" src="" ></iframe>
      <input type="button" class="isleButton bgBlue" id="btnPreviewerPrevious" value="←" title="Previous Document" onclick="previewerPrevious();" />
      <input type="button" class="isleButton bgBlue" id="btnPreviewerNext" value="→" title="Next Document" onclick="previewerNext();" />
    </div>
  </div>

  <div id="cssDetector" style="display:none;"></div>

  <div id="templates" style="display:none;">
    <div id="template_comment">
      <div class="comment" id="{id}">
        <div class="name">{user} <div class="date">{date}</div></div>
        <div class="text">{text}</div>
      </div>
    </div>
  </div>

</div><!-- /curriculumContent -->
<% } %>
<div id="error" runat="server">
  <p style="padding: 50px; text-align: center;" id="errorMessage" runat="server"></p>
</div>

<asp:Literal ID="template_mapNode" runat="server" Visible="false">
  <li {current}><a href="{url}" data-id="{id}">{title}</a></li>
</asp:Literal>
<asp:Literal ID="urlPattern" runat="server" Visible="false">/learninglist/{nodeID}/{shortTitle}</asp:Literal>
<asp:Literal ID="widgetUrlPattern" runat="server" Visible="false">/widgets/learninglist?node={nodeID}</asp:Literal>
<asp:Literal ID="activityDaysAgo" runat="server" Visible="false">90</asp:Literal>
