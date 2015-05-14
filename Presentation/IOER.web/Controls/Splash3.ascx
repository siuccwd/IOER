<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Splash3.ascx.cs" Inherits="ILPathways.Controls.Splash3" %>

<script type="text/javascript">
  /* Server Variables */
  var followedLibraries = <%=followedLibraries %>;
  var newestResources = <%=newestResources %>;
  var mostCommentedResources = <%=mostCommentedResources %>;
  var featuredResources = <%=featuredResources %>;
  var featuredLearningLists = <%=featuredLearningLists %>;
  var communityPosts = <%=communityPosts %>;
</script>
<script type="text/javascript">
  var stemLibraries = [
    { title: "Agriculture, Food, & Natural Resources", url: "/Library/211/Agriculture,_Food,_and_Natural_Resources_STEM_Learning_Exchange_Library", id: "stem_agriculture", img: "//ioer.ilsharedlearning.org/ContentDocs/65/2/7c3f376823ce4f6982017678eac353b5.png" },
    { title: "Energy", url: "/Library/213/Energy_Learning_Exchange_Library", id: "stem_energy", img: "//ioer.ilsharedlearning.org/ContentDocs/11/2/9e8dd759b4d24defb620b7710399f692.png" },
    { title: "Finance", url: "/Library/214/Finance_Learning_Exchange_Library", id: "stem_finance", img: "//ioer.ilsharedlearning.org/ContentDocs/66/2/76bd4ecfc29348dfa4151451aebcdb66.png" },
    { title: "Health Science", url: "/Library/69/Health_Sciences", id: "stem_health", img: "//ioer.ilsharedlearning.org/ContentDocs/10/159/3a72d4920c6d4cb1b145459e311da022.png" },
    { title: "Information Technology", url: "/Library/87/Illinois_IT_Learning_Exchange_Recommended_Resources", id: "stem_information", img: "//ioer.ilsharedlearning.org/ContentDocs/57/222/596f067acae54bc1ab1d6d0ed1118d4a.png" },
    { title: "Manufacturing", url: "/Library/2/Discover_Manufacturing_Library", id: "stem_manufacturing", img: "http://ioer.ilsharedlearning.org/ContentDocs/56/22/aff4f623d62b4065a33f5f0b6b236009.png" },
    { title: "Research & Development", url: "/Library/70/Research_Development_STEM_Library", id: "stem_research", img: "http://ioer.ilsharedlearning.org/ContentDocs/49/167/df4fb4198efe401481a0addfb4137464.png" },
  ];
  var featuredLibraries = [
    { title: "Abraham Lincoln", url: "/Library/349/Abraham_Lincoln", id: "featured_lincoln", img: "//ioer.ilsharedlearning.org/ContentDocs/1/303/bfc0d6fac42e429f845181bfc867fdef.jpg" },
    { title: "Illinois workNet", url: "/Library/100/Illinois_workNet", id: "featured_workNet", img: "//ioer.ilsharedlearning.org/ContentDocs/34/2/40bdb21a5df247c0890dc2aa1fc0982e.jpg" },
    { title: "Illinois Pathways", url: "/Library/349/Abraham_Lincoln", id: "featured_pathways", img: "//ioer.ilsharedlearning.org/ContentDocs/61/2/7e7b6563e6274f53965122d8412df9ee.jpg" },
    { title: "Adult Education", url: "/Library/316/Adult_Education", id: "featured_adulted", img: "//ioer.ilsharedlearning.org/ContentDocs/61/303/3afcb2bd944b4e5e934a1eb6b50232fc.jpg" },
  ];

</script>
<script type="text/javascript">
  $(document).ready(function() {
    //Remove addthis
    var removeAddThis = setInterval(function() {
      addthis.layers({'share' : {}}, function(item){ item.destroy(); });
    }, 500);
    setTimeout(function() {
      clearTimeout(removeAddThis);
    }, 3000);
    //Load resources from elasticsearch
    $(".section.elastic").each(function() {
      loadResources($(this));
    });
    //Load featured resources
    var box = $("#stemLibraries .resourcesBox");
    for(var i in stemLibraries){
      appendResource(box, stemLibraries[i]);
    }
    //Load featured libraries
    var box = $("#featuredLibraries .resourcesBox");
    for(var i in featuredLibraries){
      appendResource(box, featuredLibraries[i]);
    }
    //Load special pseudo-resources
    $("#followedLibraries .resource").last().remove();
    appendResource($("#followedLibraries").find(".resourcesBox"), { title: "<b>See More from Libraries I follow...</b>", url: "/search?libraryIDs=<%=followedLibraryIDs.Replace("[","").Replace("]","") %>", id: "all_followedLibraries", img: "/images/icons/icon_search_med.png" });
    $("#newestResources .resource").last().remove();
    appendResource($("#newestResources").find(".resourcesBox"), { title: "<b>Search all Resources...</b>", url: "/search?sort=ResourceId|desc", id: "all_newestResources", img: "/images/icons/icon_search_med.png" });
    $("#mostCommentedResources .resource").last().remove();
    appendResource($("#mostCommentedResources").find(".resourcesBox"), { title: "<b>See what people are talking about...</b>", url: "/search?sort=Paradata.comments|desc", id: "all_mostCommentedResources", img: "/images/icons/icon_search_med.png" });
    //Render community posts
    renderCommunityPosts(communityPosts.data);
  });

  function loadResources(section){
    var id = section.attr("id");
    if(window[id] == null){
      section.hide();
      return;
    }
    else {
      var resources = window[id].hits.hits;
      var list = [];
      for(var i in resources){
        list.push(resources[i]._source);
      }
      renderResources(section, list);
    }
  }

  function renderResources(section, list){
    var directLink = section.attr("data-directLink") == "true";
    var box = section.find(".resourcesBox");
    if(list.length == 0){
      section.hide();
      return;
    }
    else {
      for(var i in list){
        var current = list[i];
        appendResource(box, 
          { title: current.Title,
            url: (directLink ? current.Url : "/resource/" + current.ResourceId + "/" + current.Title.replace(/ /g, "_").replace(/&/g, "_").replace(/:/g, "_").replace(/\?/g, "_")),
            id: current.ResourceId,
            img: "//ioer.ilsharedlearning.org/OERThumbs/large/" + current.ResourceId + "-large.png"
          }          
        );
      }
    }
  }

  function appendResource(box, resource){
    var template = $("#template_resource").html()
    box.append(template
      .replace(/{title}/g, resource.title)
      .replace(/{url}/g, resource.url)
      .replace(/{id}/g, resource.id)
      .replace(/{src}/g, resource.img)
    );
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
<script type="text/javascript">
  /* Rotator */
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
  var rotatorContent;
  var rotatorTimer = 0;
  var rotatorOffset = 0;

  //Initialize
  $(document).ready(function() {
    setupRotator();
    resetRotatorTimer();
    setupTextSearch();
  });

  //Setup
  function setupRotator() {
    rotatorContent = $("#rotatorContent");
    rotatorContent.html("");
    for(var i in rotatorContents){
      appendSlide(rotatorContents[i]);
    }
    appendSlide(rotatorContents[0]);
  }
  function appendSlide(content){
    var template = $("#template_slide").html();
    rotatorContent.append(template
      .replace(/{title}/, content.title)
      .replace(/{content}/g, content.content)
      .replace(/{img}/g, content.img)
    );
  }

  //Auto slide if not hover
  function resetRotatorTimer() {
    clearInterval(rotatorTimer);
    rotatorTimer = setInterval(autoSlide, 7000);
  }
  function autoSlide() {
    if($("#rotator").is(":hover")){ resetRotatorTimer(); }
    else { slideRight(); }
  }

  //Move slides
  function slideRight() {
    if(rotatorOffset == rotatorContents.length){
      rotatorOffset = 0;
      $("#rotatorContent").removeClass("animated").css("margin-left", rotatorOffset);
    }
    setTimeout(function() {
      rotatorOffset++;
      $("#rotatorContent").addClass("animated").css("margin-left", (rotatorOffset * -100) + "%");
    }, 100);
  }

  function slideLeft() {
    if(rotatorOffset == 0){
      rotatorOffset = rotatorContents.length;
      $("#rotatorContent").removeClass("animated").css("margin-left", (rotatorOffset * -100) + "%");
    }
    setTimeout(function() {
      rotatorOffset--;
      $("#rotatorContent").addClass("animated").css("margin-left", (rotatorOffset * -100) + "%");
    }, 100);
  }

  //Text Search
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

</script>

<style type="text/css">
  #columns { margin: -10px -5px 0 -5px; }
  #content { margin: 0 -5px 0 -5px; background-color: #F5F5F5; }

  /* Big Stuff */
  #columns { background-color: #F5F5F5; }
  #main { padding-right: 250px; position: relative; }
  #sidebar { width: 250px; position: absolute; right: 0; top: 0; height: 100%; background-color: #F5F5F5; background-image: linear-gradient(rgba(53, 114, 184, 0.2), rgba(255,255,255,0)); overflow-y: auto; padding-bottom: 50px; }
  .section { background-image: linear-gradient(rgba(74, 163, 148, 0.2), rgba(74, 163, 148, 0)); margin-bottom: 25px; }
  #main h2 { font-size: 40px; margin: 10px; color: #4AA394; }

  /* Rotator */
  #rotator { position: relative; background-color: #4AA394; background: linear-gradient(#59E6E0, #4AA394); background: -webkit-linear-gradient(#59E6E0, #4AA394); overflow: hidden; }
  #rotator #rotatorResizer { width: 100%; min-height: 225px; max-height: 400px; }
  #rotatorContent { position: absolute; white-space: nowrap; top: 0; left: 0; width: 100%; height: 100%; font-size: 0; }
  #rotatorContent.animated { transition: margin 1s; -webkit-transition: margin 1s; }
  #rotator h2, #rotator p { color: #FFF; }
  #rotator h2 { background-color: #333; padding: 0; text-align: center; font-size: 40px; margin: 0 0 25px 0; }
  #rotator p { text-shadow: 1px 1px #4AA394; font-size: 34px; padding: 10px 10px 10px 5%; max-width: 50%; }
  #rotator .slide { height: 100%; white-space: normal; position: relative; display: inline-block; vertical-align: top; width: 100%; }
  #rotator .slide .background { position: absolute; top: 0; left: 0; opacity: 0.3; width: 100%; height: 100%; background: url('') right 5% top 15% no-repeat; background-size: 75%; }
  #rotator .slide .slideContent { position: relative; z-index: 10; }
  .rotator_btn { position: absolute; bottom: 25px; display: block; background-color: rgba(0,0,0,0.4); color: #FFF; font-weight: bold; text-align: center; width: 45px; height: 45px; font-size: 30px; line-height: 45px; border-radius: 30px; transition: background-color 0.2s; -webkit-transition: background-color 0.2s; -webkit-appearance: none; border: none; z-index: 1000; }
  #rotator_btnLeft { left: 25px; }
  #rotator_btnRight { right: 25px; }
  .rotator_btn:hover, .rotator_btn:focus { background-color: #FF5707; color: #FFF; cursor: pointer; }
  #searchBox { transition: padding 1s; padding-bottom: 20px; }
  #searchBox input { display: block; margin: 5px auto; width: 75%; font-size: 32px; padding: 2px 10px; z-index: 1000; position: relative; }

  /* Sections and Resources */
  .resourcesBox { padding: 10px; text-align: center; }
  .resourcesBox .resource { display: inline-block; vertical-align: top; text-align: center; white-space: normal; margin: 5px 1% 20px 1%; width: 17%; max-width: 250px; }
  .resourcesBox .resource img { width: 100%; background: url('') top center no-repeat; background-size: contain; }
  .resourcesBox .resource h4 { font-weight: normal; min-height: 4em; word-wrap: break-word; }

  /* Sidebar */
  #sidebar h3 { color: #333; font-size: 20px; padding: 5px 10px 10px 5px; text-align: right; }
  .communityPost { padding: 5px; border-radius: 5px; margin: 5px 0 20px 0; position: relative; min-height: 60px; word-wrap: break-word; }
  .communityPost .avatar { background-size: cover; width: 50px; height: 50px; border: 1px solid #99D; border-radius: 5px; position: absolute; top: 5px; left: 5px; background-color: #99D; }
  .communityPost .name { padding-left: 55px; border-bottom: 1px solid #99D; }
  .communityPost p { padding-left: 55px; }
  .communityPost .date { position: absolute; left: 8px; top: 55px; font-size: 10px; }

  @media (min-width: 1500px) {
    .resourcesBox .resource h4 { font-size: 18px; }
  }

  @media (max-width: 1100px) {
    .resourcesBox .resource { width: 22%; }
  }
  @media (max-width: 900px) {
    .resourcesBox .resource { width: 30%; }
  }
  @media (max-width: 750px) {
    #rotator h2 { margin-bottom: 10px; }
    #rotator p { font-size: 28px; max-width: 75%; }
    #main { display: block; width: 100%; }
    #sidebar { display: block; width: 100%; position: static; height: auto; }
    .resourcesBox .resource h4 { font-size: 14px; }
  }
  @media (max-width: 675px) {
    .rotator_btn { width: 35px; height: 35px; font-size: 25px; line-height: 25px; }
    #rotator_btnLeft { left: 10px; }
    #rotator_btnRight { right: 10px; }
    #searchBox input { font-size: 26px; padding: 2px 5px; width: 60%; }
  }
  @media (max-width: 500px) {
    .resourcesBox .resource { width: 47%; }
  }
</style>

<div id="content">
  <!-- Rotator -->
  <div id="rotator">

    <img src="/images/rotatorResizer.png" id="rotatorResizer" />
    <div id="rotatorContent" class="animated"></div>
    <div id="rotatorNavigation">
      <input type="button" onclick="slideLeft();" class="rotator_btn" id="rotator_btnLeft" value="←" />
      <input type="button" onclick="slideRight();" class="rotator_btn" id="rotator_btnRight" value="→" />
      <div id="searchBox">
        <input type="text" id="txtSearch" placeholder="Search IOER..." />
      </div>
    </div>
  </div>  

  </div><!-- /rotator -->
  <!-- Columns -->
  <div id="columns">
    <!-- Main Column -->
    <div id="main">

      <div class="section elastic" id="followedLibraries">
        <h2>Newest Resources from libraries I follow</h2>
        <div class="resourcesBox"></div>
      </div>
      
      <div class="section elastic" id="newestResources">
        <h2>Newest Resources from IOER</h2>
        <div class="resourcesBox"></div>
      </div>

      <div class="section" id="stemLibraries">
        <h2>Featured STEM Libraries</h2>
        <div class="resourcesBox"></div>
      </div>

      <div class="section elastic" id="featuredLearningLists" data-directLink="true">
        <h2>Featured Learning Lists</h2>
        <div class="resourcesBox"></div>
      </div>

      <div class="section elastic" id="featuredResources">
        <h2>Featured Resources</h2>
        <div class="resourcesBox"></div>
      </div>

      <div class="section" id="featuredLibraries">
        <h2>Featured Libraries</h2>
        <div class="resourcesBox"></div>
      </div>

      <div class="section elastic" id="mostCommentedResources">
        <h2>Most Commented-on Resources</h2>
        <div class="resourcesBox"></div>
      </div>

      <div id="sidebar">
        <script type="text/javascript">
          var addthis_config = {
            services_expanded: "facebook,twitter,pinterest,delicious,email,print,more"
          }
        </script>
        <style type="text/css">
          .addthis_toolbox { white-space: nowrap; text-align: center; padding: 10px 0; }
          .addthis_toolbox a.at300b, .addthis_toolbox a.at300m { display: inline-block; vertical-align: top; margin: 0; padding: 0 1px; float: none; }
        </style>
        <div class="addthis_toolbox addthis_default_style addthis_32x32_style">
          <a class="addthis_button_facebook"></a><!--
          --><a class="addthis_button_twitter"></a><!--
          --><a class="addthis_button_pinterest_share"></a><!--
          --><a class="addthis_button_delicious"></a><!--
          --><a class="addthis_button_email"></a><!--
          --><a class="addthis_button_print"></a><!--
          --><a class="addthis_button_compact"></a>
        </div>        
        <h3>Latest posts from the IOER Community</h3>
        <div id="communityBox"></div>
      </div><!-- /sidebar -->
    </div><!-- /main -->
  </div>
</div>

<div id="templates" style="display:none;">
  <script type="text/template" id="template_resource">
    <a class="resource" data-id="{id}" href="{url}">
      <img src="/images/ThumbnailResizer.png" style="background-image: url('{src}');" />
      <h4>{title}</h4>
    </a>
  </script>
  <script type="text/template" id="template_communityPost">
    <div class="communityPost">
      <div class="avatar" style="background-image: url('{avatar}');"></div>
      <h4 class="name">{name}</h4><span class="date">{date}</span>
      <p>{text}</p>
    </div>
  </script>
  <script type="text/template" id="template_slide">
    <div class="slide">
      <div class="background" style="background-image: url('{img}');"></div>
      <div class="slideContent">
        <h2>{title}</h2>
        <p>{content}</p>
      </div>
    </div>
  </script>

</div>