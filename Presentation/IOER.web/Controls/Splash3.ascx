<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Splash3.ascx.cs" Inherits="IOER.Controls.Splash3" %>

<link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<script type="text/javascript">
  /* Server Variables */
  var followedLibraries = <%=followedLibraries %>;
  var newestResources = <%=newestResources %>;
  var mostCommentedResources = <%=mostCommentedResources %>;
  var featuredResources = <%=featuredResources %>;
  var featuredLearningLists = <%=featuredLearningLists %>;
	var featuredLibrariesNew = <%=featuredLibraries %>;
	var communityPosts = <%=communityPosts %>;
</script>
<script type="text/javascript">
  var stemLibraries = [
    
  ];
  var featuredLibraries = [
    /*{ title: "Agriculture, Food, & Natural Resources", url: "/Library/211/Agriculture,_Food,_and_Natural_Resources_STEM_Learning_Exchange_Library", id: "stem_agriculture", img: "//ioer.ilsharedlearning.org/ContentDocs/65/2/7c3f376823ce4f6982017678eac353b5.png" },*/
    { title: "Energy", url: "/Library/213/Energy_Learning_Exchange_Library", id: "stem_energy", img: "//ioer.ilsharedlearning.org/ContentDocs/11/2/9e8dd759b4d24defb620b7710399f692.png" },
    /*{ title: "Finance", url: "/Library/214/Finance_Learning_Exchange_Library", id: "stem_finance", img: "//ioer.ilsharedlearning.org/ContentDocs/66/2/76bd4ecfc29348dfa4151451aebcdb66.png" },*/
    { title: "Health Science", url: "/Library/69/Health_Sciences", id: "stem_health", img: "//ioer.ilsharedlearning.org/ContentDocs/10/159/3a72d4920c6d4cb1b145459e311da022.png" },
    { title: "Information Technology", url: "/Library/87/Illinois_IT_Learning_Exchange_Recommended_Resources", id: "stem_information", img: "//ioer.ilsharedlearning.org/ContentDocs/57/222/596f067acae54bc1ab1d6d0ed1118d4a.png" },
    /*{ title: "Manufacturing", url: "/Library/2/Discover_Manufacturing_Library", id: "stem_manufacturing", img: "//ioer.ilsharedlearning.org/ContentDocs/56/22/aff4f623d62b4065a33f5f0b6b236009.png" },*/
    /*{ title: "Research & Development", url: "/Library/70/Research_Development_STEM_Library", id: "stem_research", img: "//ioer.ilsharedlearning.org/ContentDocs/49/167/df4fb4198efe401481a0addfb4137464.png" },*/
		{ title: "Abraham Lincoln", url: "/Library/349/Abraham_Lincoln", id: "featured_lincoln", img: "//ioer.ilsharedlearning.org/ContentDocs/1/303/bfc0d6fac42e429f845181bfc867fdef.jpg" },
    { title: "Illinois workNet", url: "/Library/100/Illinois_workNet", id: "featured_workNet", img: "//ioer.ilsharedlearning.org/ContentDocs/34/2/40bdb21a5df247c0890dc2aa1fc0982e.jpg" },
    { title: "Illinois Pathways", url: "/Library/318/Illinois_Pathways", id: "featured_pathways", img: "//ioer.ilsharedlearning.org/ContentDocs/61/2/7e7b6563e6274f53965122d8412df9ee.jpg" },
    { title: "Adult Education", url: "/Library/316/Adult_Education", id: "featured_adulted", img: "//ioer.ilsharedlearning.org/ContentDocs/61/303/3afcb2bd944b4e5e934a1eb6b50232fc.jpg" },
    { title: "Financial Literacy", url: "/Library/312/Financial_Literacy_for_Education_Success", id: "featured_financialliteracy", img: "//ioer.ilsharedlearning.org/ContentDocs/76/1965/a24c7127f4654fe79992336f6d2c47f4.jpg" },
		{ title: "Williamsfield Schools", url: "/Library/456/Williamsfield_Schools_@BilltownBombers", id: "featured_williamsfield", img: "//ioer.ilsharedlearning.org/ContentDocs/83/27/49e07923a3ab4fa38ec9cc58b902d2ba.jpg" },
  ];

</script>
<script type="text/javascript">
    $(document).ready(function() {
        setupTextSearch();

    //Remove addthis
    var removeAddThis = setInterval(function() {
      addthis.layers({'share' : {}}, function(item){ item.destroy(); });
    }, 10);
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
  	//Detect links in community posts
    detectLinks();
  });

	//Text Search
	function setupTextSearch(){
	    $("#txtSearchMain").on("keypress", function(event){
			if(event.which == 13 || event.keyCode == 13){
				doSearch();
			}
		});
	}

	function doSearch() {
	    var query = $("#txtSearchMain").val();
		if(query.length > 0){
			window.location.href = "/search?text=" + encodeURIComponent(query);
		}
	}

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

  function detectLinks(){
  	var posts = $(".communityPost p:contains(http)"); //Get all posts that have a link
  	posts.each(function() { //Find the links and linkify them
  		var words = $(this).text().split(" ");
  		for(var i in words){
  			if(words[i].indexOf("http") == 0 && words[i].length > 12){
  				words[i] = "<a href=\"" + words[i] + "\" target=\"_blank\">" + words[i] + "</a>";
  			}
  		}
  		$(this).html(words.join(" ")); //Reassemble the text
  	});
  }

</script>
<%-- 
<script type="text/javascript">
  /* Rotator */
  var rotatorContents = [ 
    { id: 1, title: "Share", content: "Tag online resources or upload your own to share.", img: "/images/icons/icon_tag_white_large.png", link: "Start Sharing", url: "/contribute" },  //contribute
    { id: 2, title: "Learning Lists", content: "Build and share lessons, activities, or an entire curriculum.", img: "/images/icons/icon_upload_white_large.png", link: "Browse Learning Lists", url: "/search?tagIDs=361" }, //search learning lists
    { id: 3, title: "Search", content: "Quickly locate education and career resources.", img: "/images/icons/icon_search_white_large.png", link: "Search Resources", url: "/search" }, //search
    { id: 4, title: "Libraries", content: "Explore online resource libraries, and create your own.", img: "/images/icons/icon_library_white_large.png", link: "Explore Libraries", url: "/libraries/search" }, //library search
    { id: 5, title: "Community", content: "Connect with other educators.", img: "/images/icons/icon_community_white_large.png", link: "IOER Community", url: "/community/1" }, //ioer community
    { id: 6, title: "Personalize", content: "Setup your profile and get your own dashboard.", img: "/images/icons/icon_swirl_white_large.png", link: "Go To Your Dashboard", url: "/my/dashboard" }, //dashboard
    { id: 7, title: "Widgets", content: "You don't have to leave your website with IOER widgets.", img: "/images/icons/icon_resources_white_large.png", link: "Get Widgets", url: "/widgets" }, //widgets
    { id: 8, title: "Getting Started", content: "Get information about using all of the IOER tools.", img: "/images/icons/icon_help_white_large.png", link: "Learn More", url: "/help" } //help guides
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
      .replace(/{link}/g, content.link)
      .replace(/{url}/g, content.url)
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


</script>
--%>
<style type="text/css">
  /* Big Stuff */
	#content { padding: 0; margin: 0 -5px; }
	#columns { background-color: #F5F5F5; margin-top: -10px; }
  #main { padding-right: 250px; position: relative; }
  #sidebar { width: 250px; position: absolute; right: 0; top: 0; height: 100%; background-color: #F5F5F5; background-image: linear-gradient(rgba(53, 114, 184, 0.2), rgba(255,255,255,0)); padding-bottom: 50px; }
  .section { background-image: linear-gradient(rgba(74, 163, 148, 0.2), rgba(74, 163, 148, 0)); margin-bottom: 25px; }
  #main h2 { font-size: 40px; margin: 10px; color: #4AA394; }

  <%--
	/* Rotator */
  #rotator { position: relative; background-color: #4AA394; background: linear-gradient(#59E6E0, #4AA394); background: -webkit-linear-gradient(#59E6E0, #4AA394); overflow: hidden; }
  #rotator #rotatorResizer { width: 100%; min-height: 250px; max-height: 350px; }
  #rotatorContent { position: absolute; white-space: nowrap; top: 0; left: 0; width: 100%; height: 100%; font-size: 0; }
  #rotatorContent.animated { transition: margin 1s; -webkit-transition: margin 1s; }
  #rotator h2, #rotator p { color: #FFF; }
  #rotator h2 { background-color: #333; padding: 0; text-align: center; font-size: 40px; margin: 0 0 25px 0; }
  #rotator p { text-shadow: 1px 1px #4AA394; font-size: 34px; padding: 10px 10px 10px 5%; max-width: 50%; }
  #rotator .slide { height: 100%; white-space: normal; position: relative; display: inline-block; vertical-align: top; width: 100%; }
  #rotator .slide .background { position: absolute; top: 0; left: 0; opacity: 0.3; width: 100%; height: 100%; background: url('') right 5% top 15% no-repeat; background-size: 75%; }
  #rotator .slide .slideContent { position: relative; z-index: 10; height: 100%; }
  .rotator_btn { position: absolute; bottom: 25px; display: block; background-color: rgba(0,0,0,0.4); color: #FFF; font-weight: bold; text-align: center; width: 45px; height: 45px; font-size: 30px; line-height: 45px; border-radius: 30px; transition: background-color 0.2s; -webkit-transition: background-color 0.2s; -webkit-appearance: none; border: none; z-index: 1000; }
  #rotator_btnLeft { left: 25px; }
  #rotator_btnRight { right: 25px; }
  .rotator_btn:hover, .rotator_btn:focus { background-color: #FF5707; color: #FFF; cursor: pointer; }
  #searchBox { transition: padding 1s; padding-bottom: 20px; }
  #searchBox input { display: block; margin: 5px auto; width: 75%; font-size: 32px; padding: 2px 10px; z-index: 1000; position: relative; }
  #rotator .link { position: absolute; bottom: 25%; right: 5%; color: #FFF; font-weight: bold; font-size: 30px; display: block; width: 80%; text-align: right; padding: 10px 40px 10px 10px; background-image: url('/images/arrow-right-white.png'), linear-gradient(90deg,rgba(0,75,60,0),rgba(0,75,60,0.3)); border-radius: 0 25px 25px 0; background-repeat: no-repeat; background-position: right 5px center, center center; background-size: auto 50%, auto; transition: box-shadow 0.2s; }
  #rotator .link::after { content: attr(title); position: absolute; top: 0; right: 0; width: 100%; background-image: url('/images/arrow-right-white.png'), linear-gradient(90deg,rgba(255,106,0,0),rgba(255,106,0,0.9)); border-radius: inherit; background-repeat: inherit; background-position: right 5px center, center center; background-size: inherit; padding: inherit; transition: opacity 0.2s; opacity: 0; }
  #rotator .link:hover, #rotator .link:focus { box-shadow: 10px 0 10px #FF6A00; }
  #rotator .link:hover::after, #rotator .link:focus::after { opacity: 1; }
	--%>

	/* Headlines */
	#headlineBox h1 { background-color: #333; color: #FFF; font-size: 45px; text-align: center; padding: 5px; margin-bottom: 10px; display: none; }
	#txtSearchBox { padding: 50px 8%; position: relative; text-align: center; }
	#txtSearchMain, #btnSearchMain { display: inline-block; vertical-align: top; padding: 0 5px; }
	#txtSearchMain { font-size: 32px; padding: 2px 10px; width: 80%; max-width: 1200px; margin: 0 auto; height: 45px; border-radius: 5px 0 0 5px; background-image: linear-gradient(#EEE, #FFF 50%, #FFF 85%, #EFEFEF); }
	#btnSearchMain { height: 45px; width: 20%; border-radius: 0 5px 5px 0; font-size: 32px; border-width: 1px; }
	#headlineBox { background-color: #4AA394; background-image: linear-gradient(#59E6E0, #4AA394); }
	#headlines { margin: 0 auto; max-width: 1200px; padding: 5px 5px 15px 5px; text-align: center; }
	#headlines .headline { background-color: #4AA394; background-image: linear-gradient(#49C6B0, #4AA394); border-radius: 5px; padding: 5px; display: inline-block; vertical-align: top; width: 30%; margin: 10px 1.5%; min-height: 350px; position: relative; box-shadow: 0 0 6px -1px #4AA394; }
	#headlines .headline .headlineBackground { position: absolute; top: 0; left: 0; width: 100%; height: 100%; background-repeat: no-repeat; background-size: 275px auto; background-position: left -50px bottom -100px; opacity: 0.2; border-radius: 5px; }
	#headlines .headline .headlineContent { position: relative; z-index: 100; }
	#headlines .headline h2 { color: #FFF; font-size: 40px; min-height: 1.3em; text-align: center; background-color: rgba(74, 163, 148, 0.7); margin: -5px -5px 5px -5px; border-radius: 5px 5px 0 0; padding: 0 5px; }
	#headlines .headline a { display: block; color: #FFF; font-size: 24px; min-height: 3em; padding: 5px 40px 5px 5px; text-align: right; background: url('/images/arrow-right-white.png') no-repeat center right 5px; background-size: 30px 30px; margin-bottom: 5px; transition: background 0.2s, text-shadow 0.2s; border-radius: 10px; margin-bottom: 15px; text-shadow: 1px 1px #4AA394; }
	#headlines .headline a:hover, #headlines .headline a:focus { background-color: #FF6A00; text-shadow: 1px 1px #FF6A00; }

	/* Secondary items */
	#secondary { padding: 25px 5px; }
	#secondary .secondaryItem { display: inline-block; vertical-align: top; width: 24%; margin: 10px 0.5%; text-align: center; color: #4AA394; background-size: 0; background-repeat: no-repeat; background-position: left 10px center; transition: background 0.2s; border-radius: 5px; }
	#secondary .secondaryItem h2 { font-size: 24px; margin: 0; }
	#secondary .secondaryItem img { width: 50%; max-width: 150px; display: inline-block; margin: 10px; }
	#secondary .secondaryItem p { min-height: 3em; margin: 0; font-size: 16px; }
	#secondary .secondaryItem:hover, #secondary .secondaryItem:focus { background-color: #EEE; }

  /* Sections and Resources */
  .resourcesBox { padding: 10px; text-align: center; }
  .resourcesBox .resource { display: inline-block; vertical-align: top; text-align: center; white-space: normal; margin: 5px 1% 20px 1%; width: 17%; max-width: 250px; }
  .resourcesBox .resource img { width: 100%; background: url('') top center no-repeat; background-size: contain; }
  .resourcesBox .resource .title { font-weight: normal; min-height: 4em; word-wrap: break-word; }

  /* Sidebar */
  #sidebar h3 { color: #333; font-size: 20px; padding: 5px 10px 10px 5px; text-align: center; }
	#sidebar h3 a { font-size: inherit; }
  .communityPost { padding: 5px; border-radius: 5px; margin: 5px 0 20px 0; position: relative; min-height: 60px; word-wrap: break-word; }
  .communityPost .avatar { background-size: cover; width: 50px; height: 50px; border: 1px solid #99D; border-radius: 5px; position: absolute; top: 5px; left: 5px; background-color: #99D; }
  .communityPost .name { padding-left: 55px; border-bottom: 1px solid #99D; font-weight: bold; }
  .communityPost .text { padding-left: 55px; }
  .communityPost .date { position: absolute; left: 8px; top: 55px; font-size: 10px; }
	#sidebar #communityBox { overflow-y: auto; overflow-x: hidden; max-height: calc(100% - 125px); }

  @media (min-width: 1500px) {
    .resourcesBox .resource .title { font-size: 18px; }
  }

  @media (max-width: 1100px) {
    .resourcesBox .resource { width: 22%; }
  }
  @media (max-width: 900px) {
    .resourcesBox .resource { width: 30%; }
		#secondary .secondaryItem h2 { font-size: 22px; }
		#headlines .headline .headlineBackground {  }
  }
	@media (max-width: 800px) {
		#headlines .headline h2 { font-size: 22px; }
		#headlines .headline a { font-size: 18px; }
		#txtSearchMain, #btnSearchMain { font-size: 26px; height: 35px; }
	}
  @media (max-width: 750px) {
    <%--
		#rotator h2 { margin-bottom: 10px; }
    #rotator p { font-size: 28px; max-width: 75%; }
		--%>
    #main { display: block; width: 100%; padding-right: 0; }
    #sidebar { display: block; width: 100%; position: static; height: auto; }
    .resourcesBox .resource .title { font-size: 14px; }
  }
	<%--
  @media (max-width: 675px) {
    .rotator_btn { width: 35px; height: 35px; font-size: 25px; line-height: 25px; }
    #rotator_btnLeft { left: 10px; }
    #rotator_btnRight { right: 10px; }
    #searchBox input { font-size: 26px; padding: 2px 5px; width: 60%; }
    #rotator .link { font-size: 22px; padding: 5px 35px 5px 5px }
  }
	--%>
	@media (max-width: 575px) {
		#headlines .headline { display: block; width: 100%; margin: 0 0 10px 0; min-height: 0; }
		#headlines .headline .headlineBackground {  }
		#secondary .secondaryItem { width: 100%; background-size: 65px 65px; padding-left: 85px; text-align: left; margin: 5px 0; }
		#secondary .secondaryItem img { display: none; }
	}
  @media (max-width: 500px) {
    .resourcesBox .resource { width: 47%; }
		#txtSearchMain, #btnSearchMain { font-size: 24px; height: 35px; }
		#txtSearchMain { width: 70%; }
		#btnSearchMain { width: 30%; }
  }
</style>

<div id="content">
  <%--<!-- Rotator -->
  <div id="rotator">

    <img alt="" src="/images/rotatorResizer.png" id="rotatorResizer" />
    <div id="rotatorContent" class="animated"></div>
    <div id="rotatorNavigation">
      <input type="button" onclick="slideLeft();" class="rotator_btn" id="rotator_btnLeft" value="←" />
      <input type="button" onclick="slideRight();" class="rotator_btn" id="rotator_btnRight" value="→" />
      <div id="searchBox">
        <label for="txtSearch" class="offScreen">Search</label>
        <input type="text" id="txtSearch" name="txtSearch" placeholder="Search IOER..." />
      </div>
    </div>
  </div><!-- /rotator -->--%>
	<!-- Headline Boxes -->
	<div id="headlineBox">
		<h1>Illinois Open Educational Resources</h1>
		<div id="txtSearchBox">
      <input type="text" id="txtSearchMain" name="txtSearchMain" title="Search IOER" placeholder="Search IOER..." /><!--
			--><input type="button" class="isleButton bgBlue" id="btnSearchMain" name="btnSearchMain" value="Search" onclick="doSearch();" />
    </div>
		<div id="headlines">
			<div class="headline" id="headline_resources">
				<div class="headlineBackground" style="background-image: url('/images/icons/icon_search_white_large.png');"></div>
				<div class="headlineContent">
					<h2>Browse</h2>
					<a href="/search">Education and career resources</a>
					<a href="/content/search">Member-created content</a>
					<!--<a href="/community/1">Connect with other educators</a>-->
				</div>
			</div><!--
			--><div class="headline" id="headline_containers">
				<div class="headlineBackground" style="background-image: url('/images/icons/icon_library_white_large.png');"></div>
				<div class="headlineContent">
					<h2>Explore</h2>
					<a href="/libraries/search">Online libraries of curated resources</a>
					<a href="/learninglists">Learning Lists of lessons, activities, and curricula</a>
				</div>
			</div><!--
			--><div class="headline" id="headline_share">
				<div class="headlineBackground" style="background-image: url('/images/icons/icon_resources_white_large.png');"></div>
				<div class="headlineContent">
					<h2>Share</h2>
					<a href="/my/library">Open your own online resource library</a>
					<a href="/tagger">Tag, upload, or create a resource</a>
					<a href="/My/LearningList/new">Create a learning list</a>
				</div>
			</div>
		</div>
	</div><!-- /headlineBox -->
	<!-- Secondary -->
	<div id="secondary">
		<a class="secondaryItem" id="userGuide" href="/Help/Guide.aspx" style="background-image: url('/images/icons/icon_morelikethis_med.png');">
			<h2>User Guide</h2>
			<p>Quickly get acquainted</p>
			<img src="/images/icons/icon_morelikethis_med.png" title="User Guide" />
		</a><!--
		--><a class="secondaryItem" id="community" href="/Community/1/" style="background-image: url('/images/icons/icon_comments_med.png');">
			<h2>Community</h2>
			<p>Build your resource network</p>
			<img src="/images/icons/icon_comments_med.png" title="Community" />
		</a><!--
		--><a class="secondaryItem" id="widgets" href="/widgets/" style="background-image: url('/images/icons/icon_app_med.png');">
			<h2>Widgets</h2>
			<p>Share with seamless access</p>
			<img src="/images/icons/icon_app_med.png" title="Widgets" />
		</a><!--
		--><a class="secondaryItem" id="rate" href="/Help/Guide.aspx?tab=ratings" style="background-image: url('/images/icons/icon_ratings_med.png');">
			<h2>Rate</h2>
			<p>Help others find good resources</p>
			<img src="/images/icons/icon_ratings_med.png" title="Rate" />
		</a>
	</div><!-- /secondary -->
  <!-- Columns -->
  <div id="columns">
    <!-- Main Column -->
    <div id="main">

      <div class="section elastic" id="followedLibraries">
        <h2>Newest resources from libraries I follow</h2>
        <div class="resourcesBox"></div>
      </div>
      
      <div class="section elastic" id="newestResources">
        <h2>Newest resources from IOER</h2>
        <div class="resourcesBox"></div>
      </div>

      <%--<div class="section" id="stemLibraries">
        <h2>Featured STEM Libraries</h2>
        <div class="resourcesBox"></div>
      </div>--%>

      <div class="section elastic" id="featuredLearningLists" data-directLink="true">
        <h2>Featured learning lists</h2>
        <div class="resourcesBox"></div>
      </div>

      <div class="section elastic" id="featuredResources">
        <h2>Featured resources</h2>
        <div class="resourcesBox"></div>
      </div>

      <div class="section elastic" id="featuredLibrariesNew">
        <h2>Featured libraries</h2>
        <div class="resourcesBox"></div>
      </div>

			<%--
      <div class="section elastic" id="mostCommentedResources">
        <h2>Most Commented-on Resources</h2>
        <div class="resourcesBox"></div>
      </div>--%>

      <div id="sidebar">
        <script type="text/javascript">
          var addthis_config = {
          	services_expanded: "'twitter,facebook,google_plusone_share,pinterest,flipboard,linkedin,email,print,more'"
          }
        </script>
        <style type="text/css">
          .addthis_toolbox { white-space: nowrap; text-align: center; padding: 10px 0; }
          .addthis_toolbox a.at300b, .addthis_toolbox a.at300m { display: inline-block; vertical-align: top; margin: 0; padding: 0 1px; float: none; }
        	a.at300b .at4-icon, a.at300m .at4-icon { width: 25px; height: 25px; background-size: 25px 25px !important; background-color: #3572B8 !important; transition: background-color 0.2s; }
        	a.at300b .at4-icon:hover, a.at300m .at4-icon:hover, a.at300b .at4-icon:focus, a.at300m .at4-icon:focus { background-color: #FF6A00 !important; }
        </style>
        <div class="addthis_toolbox addthis_default_style addthis_32x32_style">
          <a class="addthis_button_twitter"></a><!--
          --><a class="addthis_button_facebook"></a><!--
					--><a class="addthis_button_google_plusone_share"></a><!--
					--><a class="addthis_button_pinterest_share"></a><!--
					--><a class="addthis_button_flipboard"></a><!--
					--><a class="addthis_button_linkedin"></a><!--
          --><a class="addthis_button_email"></a><!--
          --><a class="addthis_button_print"></a><!--
          --><a class="addthis_button_compact"></a>
        </div>        
        <h3>Latest posts from the<br /><a href="/Community/1/ISLE_Community">IOER Community</a></h3>
        <div id="communityBox"></div>
        <div id="offScreen" class="offScreen">
            <a href="/LearningLists"  title="#IOER, IOER Learning lists">IOER Learning Lists</a>
            <a href="/Publishers.aspx" title="#IOER, IOER publishers">IOER publishers</a>

        </div>
      </div><!-- /sidebar -->
    </div><!-- /main -->
  </div><!-- /columns -->
</div>

<div id="templates" style="display:none;">
  <script type="text/template" id="template_resource">
    <a class="resource" data-id="{id}" href="{url}">
      <img alt="" src="/images/ThumbnailResizer.png" style="background-image: url('{src}');" />
      <div class="title">{title}</div>
    </a>
  </script>
  <script type="text/template" id="template_communityPost">
    <div class="communityPost">
      <div class="avatar" style="background-image: url('{avatar}');"></div>
      <div class="name">{name}</div><span class="date">{date}</span>
			<div class="text">
				<p>{text}</p>
			</div>
    </div>
  </script>
  <script type="text/template" id="template_slide">
    <div class="slide">
      <div class="background" style="background-image: url('{img}');"></div>
      <div class="slideContent">
        <h2>{title}</h2>
        <p>{content}</p>
        <a class="link" href="{url}" title="{link}">{link}</a>
      </div>
    </div>
  </script>

</div>