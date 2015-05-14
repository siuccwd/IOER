<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Splash2.ascx.cs" Inherits="ILPathways.Controls.Splash2" %>
<%@ Register TagPrefix="uc1" TagName="SplashMini" Src="/Controls/SplashMini.ascx" %>

<script type="text/javascript">
  var newestResourcesRaw = <%=newestResources %>;
  var followedResourcesRaw = <%=followedResources %>;
  var mostCommentedResourcesRaw = <%=mostCommentedResources %>;
  var communityPosts = <%=communityPosts %>;
  //var featuredResourcesData = [{ title: "HSLE - Health Science Curriculum", intID: 543476, url: "http://ioer.ilsharedlearning.org/curriculum/2197/2197/HSLE_-_Health_Science_Curriculum" }];
  var featuredResourcesRaw = <%=featuredResources %>;
</script>
<script type="text/javascript">
  var newestResources = [];
  var fileTypes = [
    /*{ match: ".doc", text: ".doc File" },
    { match: ".ppt", text: ".ppt File" },
    { match: ".xls", text: ".xls File" },
    { match: ".docx", text: ".docx File" },
    { match: ".pptx", text: ".pptx File" },
    { match: ".xlsx", text: ".xlsx File" },
    //{ match: ".pdf", text: ".pdf File" },*/
    //{ match: ".swf", text: ".swf File" },
    { match: "localhost", text: "Test Data" }
  ];
  var thumbIconTypes = [
    //{ match: ".pdf", header: "Adobe PDF", file: "/images/icons/filethumbs/filethumb_pdf_200x150.png" },
    { match: ".doc", header: "Microsoft Word Document", file: "/images/icons/filethumbs/filethumb_docx_200x150.png" },
    { match: ".docx", header: "Microsoft Word Document", file: "/images/icons/filethumbs/filethumb_docx_200x150.png" },
    { match: ".ppt", header: "Microsoft PowerPoint Document", file: "/images/icons/filethumbs/filethumb_pptx_200x150.png" },
    { match: ".pptx", header: "Microsoft PowerPoint Document", file: "/images/icons/filethumbs/filethumb_pptx_200x150.png" },
    { match: ".xls", header: "Microsoft Excel Spreadsheet", file: "/images/icons/filethumbs/filethumb_xlsx_200x150.png" },
    { match: ".xlsx", header: "Microsoft Excel Spreadsheet", file: "/images/icons/filethumbs/filethumb_xlsx_200x150.png" },
  ];
  $(document).ready(function () {
    renderResources(newestResourcesRaw, "newestResources");
    renderResources(followedResourcesRaw, "followedResources");
    renderResources(mostCommentedResourcesRaw, "commentedResources");
    renderCommunityPosts(communityPosts.data);
    renderResources(featuredResourcesRaw, "featuredResources", true);
    setupTextSearch();
  });

  function setupTextSearch(){
    $("#txtSearch").on("keypress", function(event){
      if(event.which == 13 || event.keyCode == 13){
        var query = $("#txtSearch").val();
        if(query.length > 0){
          window.location.href = "/search?text=" + encodeURIComponent(query);
        }
      }
    });
  }

  function renderResources(data, id, useResourceURL) {
    try {
      var resources = data.hits.hits;
      var items = [];
      for(i in resources){
        items.push(resources[i]._source);
      }

      renderList(id, items, useResourceURL);
    }
    catch(e){
      console.log("ERROR", e);
      console.log(data);
      console.log(id);
      $("div[data-id=" + id + "]").hide();
    }
  }

  function renderList(target, list, useResourceURL){
    var div = $("#" + target);
    if(list.length == 0){ 
      $("div[data-id=" + target + "]").hide();
    }
    else {
      div.html("");
      var template = $("#template_resource").html();
      var valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ _-";
      for(i in list){
        var fixedTitle = "";
        for(j in list[i].Title){
          if(valid.indexOf(list[i].Title[j]) >= 0){
            fixedTitle += list[i].Title[j];
          }
        }
        fixedTitle = fixedTitle.replace(/ /g, "_");
        var useThumb = "//ioer.ilsharedlearning.org/OERThumbs/large/" + list[i].ResourceId + "-large.png";
        for(j in thumbIconTypes){
          if(list[i].Url.indexOf(thumbIconTypes[j].match) > -1){
            useThumb = thumbIconTypes[j].file;
          }
        }
        div.append(
          template.replace(/{detailURL}/g, (useResourceURL ? list[i].Url : "/Resource/" + list[i].ResourceId + "/" + fixedTitle))
          .replace(/{thumbURL}/g, useThumb )
          .replace(/{thumbSRC}/g, "img src=\"" + useThumb + "\"" )
          .replace(/{text}/g, list[i].Title)
          .replace(/{id}/g, list[i].ResourceId)
        );
        for(j in fileTypes){
          if(list[i].Url.indexOf(fileTypes[j].match) > -1){
            div.find(".resource").last().find(".message").html(fileTypes[j].text);
          }
        }
      }
    }
  }

  function handleBrokenThumbnail(id){
    var target = $("#message_" + id);
    if(target.html().length == 0){
      $("#message_" + id).html("Generating Thumbnail");
    }
  }

  function renderCommunityPosts(data){
    var template = $("#template_communityPost").html();
    var box = $("#communityBox");
    for(i in data){
      box.append(
        template
          .replace(/{avatar}/g, data[i].posterAvatar)
          .replace(/{name}/g, data[i].poster)
          .replace(/{date}/g, data[i].created)
          .replace(/{text}/g, data[i].text)
      );
    }
  }

</script>

<style type="text/css">
  /* Big Stuff */
  body { padding: 0; }
  #header { margin-bottom: 0; }
  #content { min-width: 300px; background-color: #EEE; }
  #content, #content * { box-sizing: border-box; -moz-box-sizing: border-box; }
  .contentBox { transition: margin-left 1s; -webkit-transition: margin-left 1s; }
  .tealBox { background-color: #4AA394; background: linear-gradient(#4AA394, #5AB3A4); background: -webkit-linear-gradient(#4AA394, #5AB3A4); padding: 10px 0; }
  .interestBox { padding: 10px; }

  /* Specific Items */
  #content h1 { padding: 5px; font-size: 25px; text-align: center; margin: 5px 5px; color: #F5F5F5; background-color: #333; border-radius: 5px; }
  .contentBox h2 { font-size: 20px; color: #444; }
  #content p.bigText { padding: 15px; text-align: center; font-size: 24px; }
  p.bigText.white { color: #FFF; }
  #content h2 a { font-size: inherit; }
  #content h2 a:hover, #content h2 a:focus { text-decoration: underline; }

  /* Mini Splash Items */
  #links { transition: padding 1s; -webkit-transition: padding 1s; }
  #links .splashItem img { box-shadow: none; }
  #links .splashItem:hover img { background-color: #3572B8; }
  #links .splashItem a { box-shadow: none; }
  #links .splashItem a:hover, #links .splashItem a:focus { box-shadow: 0 0 15px #3572B8; background-color: #3572B8; }
  #links .splashItem h2 { background-color: #333; color: #FFF; box-shadow: none; font-size: 22px; }

  /* Resource Items */
  .resourcesBox { text-align: center; height: 230px; overflow: hidden; }
  .resource { 
    text-align: left; 
    border-radius: 5px; 
    border: 1px solid #CCC; 
    display: inline-block; 
    width: 22%; 
    vertical-align: top; 
    margin: 10px 1% 200px 0; 
    max-width: 200px; 
    white-space: nowrap; 
    overflow: hidden;
  }
  .resource .title { 
    height: 60px; 
    overflow: hidden; 
    text-align: center; 
    padding: 2px; 
    white-space: initial; 
    background-color: #FFF; 
    text-overflow: ellipsis; 
  }
  .resource .message { 
    text-align: center; 
    padding: 10px; 
    font-weight: bold; 
    font-size: 26px; 
    height: calc(100% - 56px);
    width: 100%;
    position: absolute;
    top: 0;
    left: 0;
    white-space: initial;
  }
  .resource a { display: block; width: 100%; height: 100%; position: relative; background: linear-gradient(#DFDFDF, #EFEFEF); background: -webkit-linear-gradient(#DFDFDF, #EFEFEF); }
  .resource img { width: 100%; margin-bottom: -4px; }
  .resource img.resizer { background-size: contain;  }
  .resource img.tester { display: none; }
  .resource:hover { box-shadow: 0 0 15px #FF5707; }

  #stuffBox { position: relative; min-height: 600px; }
  #resourcesBox { padding-left: 250px; }
  #communityBox { width: 250px; position: absolute; top: 0; left: 0; padding: 5px; overflow-y: scroll; height: 100%;  }
  .communityPost { box-shadow: 0 0 10px -2px #CCC; padding: 5px; border-radius: 5px; margin: 5px 0; position: relative; min-height: 60px; word-break: break-word; }
  .communityPost .avatar { background-size: cover; width: 50px; height: 50px; border: 1px solid #DDD; border-radius: 5px; position: absolute; top: 5px; left: 5px; background-color: #DDD; }
  .communityPost .name { padding-left: 55px; border-bottom: 1px solid #DDD; }
  .communityPost p { padding-left: 55px; }
  .communityPost .date { position: absolute; left: 8px; top: 55px; font-size: 10px; }

  .libraryBox { text-align: center; white-space: nowrap; }
  .libraryBox h2 { text-align: left; }
  .libraryBox a { position: relative; display: inline-block; vertical-align: top; width: 100%; max-width: 175px; margin-right: 1%; height: 200px; background: url('') no-repeat center center; background-size: 90%; }
  .libraryBox a div { background-color: #FFF; display: none; }
  #stemLibraries a { width: 13%; }
  #featuredLibraries a div { display: block; position: absolute; bottom: 0; left: 0; right: 0; text-align: center; background-color: rgba(0,0,0,0.6); color: #FFF; padding: 2px; }
  #featuredLibraries a { background-color: #FFF; border-radius: 5px; overflow: hidden; background-size: 95%; margin-bottom: 15px; background-position: top 10% center; }

  #searchBox { transition: padding 1s; padding-bottom: 20px; }
  #searchBox input { display: block; margin: 5px auto; width: 75%; font-size: 32px; padding: 2px 10px; }

  /* Featured Resources */
  #featuredResources { height: auto; }
  #featuredResources .resource { width: 17%; margin-bottom: 20px; }

  /* Responsive */
  @media screen and (min-width: 980px) {
    #searchBox { padding-left: 65px; }
    .contentBox #links { padding: 5px 5%; }
    .contentBox { margin-left: 65px; }
  }
  @media screen and (max-width: 950px) {
    #searchBox { padding-bottom: 5px; }
    #searchBox input { font-size: 20px; }
  }
  @media screen and (max-width: 1350px) {
    #links .splashItem { width: 47%; }
  }
  @media screen and (max-width: 800px) {
    .resource, #featuredResources .resource { width: 30%; margin: 10px 2% 200px 0; }
    #featuredResources .resource { margin-bottom: 20px; }
  }
  @media screen and (max-width: 700px) {
    #links .splashItem { width: 98%; }
    #content h1 { font-size: 18px; }
    #resourcesBox { padding-left: 0; }
    #communityBox { position: static; width: 100%; padding: 5px; }
  }
  @media screen and (max-width: 500px){
    .resource, #featuredResources .resource { width: 48%; margin: 10px 1% 200px 0; }
    #featuredResources .resource { margin-bottom: 20px; }
    .contentBox h1, .contentBox .bigText.white { display: none; }
    #links .splashItem img { width: 100px; height: 100px; }
    #splash .splashItem { padding-left: 50px; min-height: 100px; }
  }
  @media screen and (max-width: 1175px) {
    .libraryBox { white-space: normal; }
    #stemLibraries a { margin: 10px; width: 100px; height: 100px; }
  }
</style>

<div id="content">
  <div id="rotator">
    <script type="text/javascript">
      /* Rotator JS */
      var rotatorContents = [ 
        { id: 1, title: "Share", content: "Tag online resources or upload your own to share.", img: "/images/icons/icon_tag_white_large.png" }, 
        { id: 2, title: "Curriculum", content: "Build and share lessons, activities, or an entire curriculum.", img: "/images/icons/icon_upload_white_large.png" },
        { id: 3, title: "Search", content: "Quickly locate education and career resources.", img: "/images/icons/icon_search_white_large.png" },
        { id: 4, title: "Libraries", content: "Open your own resource library, and explore other libraries.", img: "/images/icons/icon_library_white_large.png" },
        { id: 5, title: "Community", content: "Connect with other educators.", img: "/images/icons/icon_community_white_large.png" },
        { id: 6, title: "Personalize", content: "Setup your profile and get your own dashboard.", img: "/images/icons/icon_swirl_white_large.png" }, 
        { id: 7, title: "Widgets", content: "You don't have to leave your website with OER widgets.", img: "/images/icons/icon_resources_white_large.png" },
        { id: 8, title: "Getting Started", content: "Get information about using all of the IOER tools.", img: "/images/icons/icon_help_white_large.png" }
      ];
      var rotator;
      var rotatorContent;
      var rotatorMultiplier = 0;
      var rotatorTimer;

      $(document).ready(function() {
        setupRotator();
        resetRotatorTimer();
      });

      function resetRotatorTimer() {
        clearTimeout(rotatorTimer);
        rotatorTimer = setTimeout(autoSlide, 7000);
      }

      function autoSlide() {
        if($("#rotator").is(":hover")){ resetRotatorTimer(); }
        else { slideRight(); }
      }

      function setupRotator() {
        rotator = $("#rotator");
        rotatorContent = $("#rotatorContent");
        rotatorContent.html("");
        for(i in rotatorContents){
          appendRotator(rotatorContents[i]);
        }
        appendRotator(rotatorContents[0]); //double the endpiece
      }
      function appendRotator(content){
        var rotatorContentItemTemplate = '<div class="rotatorContentItem" data-id="{id}"><div class="rotatorContentItemBackground" style="background-image: url(\'{img}\')"></div><div class="rotatorContentItemContentContainer"><h1>{title}</h1><div class="rotatorContentItemContent">{content}</div></div></div>';
        rotatorContent.append(rotatorContentItemTemplate
          .replace(/{title}/g, content.title)
          .replace(/{id}/g, content.id)
          .replace(/{content}/g, content.content)
          .replace(/{img}/g, content.img)
        );
      }

      function slideRotator(multiplier){
        rotatorMultiplier = multiplier;
        rotatorContent.css("right", (multiplier * 100) + "%");
        resetRotatorTimer();
      }

      function slideRight(){
        if(rotatorMultiplier == rotatorContents.length){ 
          rotatorContent.removeClass("animated");
          setTimeout(function() { rotatorContent.css("right", "0%"); }, 100 );
          setTimeout(function() { rotatorContent.addClass("animated"); slideRotator(1); }, 200 );
        }
        else {
          slideRotator(rotatorMultiplier + 1);
        }
      }
      function slideLeft() {
        if(rotatorMultiplier == 0){
          rotatorContent.removeClass("animated");
          setTimeout(function() { rotatorContent.css("right", (rotatorContents.length * 100) + "%"); }, 100 );
          setTimeout(function() { rotatorContent.addClass("animated"); slideRotator( rotatorContents.length - 1 ); }, 200 );
        }
        else {
          slideRotator(rotatorMultiplier - 1);
        }
      }
    </script>
    <style type="text/css">
      /* Rotator Styles */
      #rotator { position: relative; background-color: #4AA394; background: linear-gradient(#59E6E0, #4AA394); background: -webkit-linear-gradient(#59E6E0, #4AA394); overflow: hidden; min-height: 225px; max-height: 400px; }
      #rotator #rotatorResizer { width: 100%; }
      #rotatorContent { position: absolute; white-space: nowrap; top: 0; right: 0; width: 100%; height: 100%; }
      #rotatorContent.animated { transition: right 1s; -webkit-transition: right 1s; }
      .rotatorContentItem { width: 100%; height: 100%; display: inline-block; vertical-align: top; transition: padding 1s; -webkit-transition: padding 1s; position:relative; }
      .rotatorContentItemBackground { width: 100%; height: 100%; position: absolute; top: 0; right: 0; background: transparent url('') right 5% top 25% no-repeat; background-size: auto 150%; z-index: 1; opacity: 0.5; }
      .rotatorContentItemContentContainer { z-index: 100; position:relative; }
      .rotatorContentItemContent { margin: 10px 20px; font-size: 36px; color: #FFF; white-space: normal; text-shadow: 1px 1px 1px #4AA394; }
      #content .rotatorContentItemContentContainer h1 { font-size: 35px; }
      .rotatorContentItemContent a { color: #FFF; text-decoration: underline; font-size: inherit; }
      .rotatorContentItemContent a:hover, .rotatorContentItemContent a:focus { text-shadow: 0 0 3px #FFF; }
      #rotatorNavigation { z-index: 10000; position: absolute; bottom: 0; right: 0; width: 100%; }
      .rotator_btn { position: absolute; bottom: 25px; display: block; background-color: rgba(0,0,0,0.4); color: #FFF; font-weight: bold; text-align: center; width: 45px; height: 45px; font-size: 30px; line-height: 45px; border-radius: 30px; transition: background-color 0.2s; -webkit-transition: background-color 0.2s; }
      #rotator_btnLeft { left: 25px; }
      #rotator_btnRight { right: 25px; }
      .rotator_btn:hover, .rotator_btn:focus { background-color: #FF5707; color: #FFF; }

      @media screen and (min-width: 1300px) {
        .rotatorContentItemContent { font-size: 56px; }
      }
      @media screen and (min-width: 950px) {
        .rotatorContentItem { padding-left: 55px; }
        #rotator_btnLeft { left: 85px; }
        .rotatorContentItemContent { max-width: 40%; }
      }
      @media screen and (max-width: 949px){
        .rotator_btn { bottom: 5px; width: 25px; height: 25px; font-size: 20px; line-height: 25px; border-radius: 25px; }
        #content .rotatorContentItemContentContainer h1 { font-size: 26px; }
        .rotatorContentItemContent { font-size: 25px; margin: 10px 35px; }
        #rotator_btnRight { right: 5px; }
        #rotator_btnLeft { left: 5px; }
      }
    </style>
    <img src="/images/rotatorResizer.png" id="rotatorResizer" />
    <div id="rotatorContent" class="animated"></div>
    <div id="rotatorNavigation">
      <a href="#" onclick="slideLeft(); return false;" class="rotator_btn" id="rotator_btnLeft">&larr;</a>
      <a href="#" onclick="slideRight(); return false;" class="rotator_btn" id="rotator_btnRight">&rarr;</a>
      <div id="searchBox">
        <input type="text" id="txtSearch" placeholder="Search IOER..." />
      </div>
    </div>
  </div>  
  <div class="tealBox" style="display: none;">
    <div class="contentBox">
      <h1>Open Educational Resources</h1>
      <p class="bigText white">IOER provides you with one-click access to open, standards-aligned educational content. Use our tools to find, remix, and comment on resources for your personalized IOER learning library.</p>
      <div id="links">
        <uc1:SplashMini runat="server" ID="splashMini" useNewWindow="false" Visible="false" />
      </div>
    </div>
  </div>
  <div id="stuffBox">
    <div class="contentBox" id="communityBox">
      <h2>Latest Posts from <br />The <a href="/Community/1/IOER_Community">IOER Community</a></h2>
    </div>
    <div id="resourcesBox">
      <div class="contentBox interestBox" data-id="followedResources">
        <h2>Latest Resources from Libraries I Follow:</h2>
        <div id="followedResources" class="resourcesBox"></div>
      </div>
      <div class="contentBox interestBox" data-id="newestResources">
        <h2><a href="/Search.aspx?sort=newest">Newest Resources:</a></h2>
        <div id="newestResources" class="resourcesBox"></div>
      </div>
      <div id="stemLibraries" class="contentBox interestBox libraryBox">
        <h2>Illinois Pathways STEM Libraries:</h2>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/211/Agriculture,_Food,_and_Natural_Resources_STEM_Learning_Exchange_Library" style="background-image:url(http://ioer.ilsharedlearning.org/ContentDocs/65/2/7c3f376823ce4f6982017678eac353b5.png);"><div>Agriculture</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/213/Energy_Learning_Exchange_Library" style="background-image: url(http://ioer.ilsharedlearning.org/ContentDocs/11/2/9e8dd759b4d24defb620b7710399f692.png);"><div>Energy</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/214/Finance_Learning_Exchange_Library" style="background-image: url(http://ioer.ilsharedlearning.org/ContentDocs/66/2/76bd4ecfc29348dfa4151451aebcdb66.png);"><div>Finance</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/69/Health_Sciences" style="background-image: url(http://ioer.ilsharedlearning.org/ContentDocs/10/159/3a72d4920c6d4cb1b145459e311da022.png);"><div>Health Sciences</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/87/Illinois_IT_Learning_Exchange_Recommended_Resources" style="background-image: url(http://ioer.ilsharedlearning.org/ContentDocs/57/222/596f067acae54bc1ab1d6d0ed1118d4a.png);"><div>IT Learning Exchange</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/2/Discover_Manufacturing_Library" style="background-image: url(http://ioer.ilsharedlearning.org/ContentDocs/56/22/aff4f623d62b4065a33f5f0b6b236009.png);"><div>Discover Manufacturing</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/70/Research_Development_STEM_Library" style="background-image: url(http://ioer.ilsharedlearning.org/ContentDocs/49/167/df4fb4198efe401481a0addfb4137464.png);"><div>Research and Development</div></a>
      </div>
      <div class="contentBox interestBox" data-id="commentedResources" style="display:none;">
        <h2><a href="/Search.aspx?sort=comments">Most Talked About Resources:</a></h2>
        <div id="commentedResources" class="resourcesBox"></div>
      </div>
      <div class="contentBox interestBox" data-id="featuredResources">
        <h2>Featured Resources</h2>
        <div id="featuredResources" class="resourcesBox"></div>
      </div>
      <div id="featuredLibraries" class="contentBox interestBox libraryBox">
        <h2>Featured Libraries:</h2>
        <a class="stemLibrary" href="https://ioer.ilsharedlearning.org/Library/349/Abraham_Lincoln" style="background-image:url('https://ioer.ilsharedlearning.org/ContentDocs/1/303/bfc0d6fac42e429f845181bfc867fdef.jpg');"><div>Abraham Lincoln</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/100/Illinois_workNet" style="background-image:url('http://ioer.ilsharedlearning.org/ContentDocs/34/2/40bdb21a5df247c0890dc2aa1fc0982e.jpg');"><div>Illinois workNet</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/318/Illinois_Pathways" style="background-image:url('http://ioer.ilsharedlearning.org/ContentDocs/61/2/7e7b6563e6274f53965122d8412df9ee.jpg');"><div>Illinois Pathways</div></a>
        <a class="stemLibrary" href="http://ioer.ilsharedlearning.org/Library/316/Adult_Education" style="background-image:url('http://ioer.ilsharedlearning.org/ContentDocs/61/303/3afcb2bd944b4e5e934a1eb6b50232fc.jpg');"><div>Adult Education</div></a>
      </div>
    </div>
  </div>
</div>

<div id="templates" style="display:none;">
  <div id="template_resource">
    <div class="resource">
      <a href="{detailURL}">
        <img src="/images/ThumbnailResizer.png" class="resizer" style="background-image:url('{thumbURL}');" />
        <img {thumbSRC} class="tester" onerror="handleBrokenThumbnail({id});" />
        <div class="message" id="message_{id}"></div>
        <div class="title">{text}</div>
      </a>
    </div>
  </div>

  <div id="template_communityPost">
    <div class="communityPost">
      <div class="avatar" style="background-image: url('{avatar}');"></div>
      <h3 class="name">{name}</h3><span class="date">{date}</span>
      <p>{text}</p>
    </div>
  </div>
</div>