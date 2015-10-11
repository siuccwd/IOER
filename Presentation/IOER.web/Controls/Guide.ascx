<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Guide.ascx.cs" Inherits="IOER.Controls.Guide" %>

<script type="text/javascript">
  var videosLoaded = false;
  var responsiveTimer = {};
  $(document).ready(function () {
    $(window).on("resize", function () {
      redrawLines();
      clearTimeout(responsiveTimer);
      responsiveTimer = setTimeout(function () {
        $(window).trigger("resize");
      }, 1100);
    }).trigger("resize");

  });

  function loadVideos() {
    if (videosLoaded) {
      return;
    }
    $("#overviewBox .youtube").attr("src", "//www.youtube.com/embed/SLxD8ECjQhU?list=PLlkuU1Id_xm_wnuoFjqe4p7mK-cZdTusZ&listType=playlist&wmode=transparent&rel=0");
    $("#overviewBox .youtubeLink").attr("href", "http://www.youtube.com/watch?v=SLxD8ECjQhU&list=PLlkuU1Id_xm_wnuoFjqe4p7mK-cZdTusZ");
    $("#searchBox .youtube").attr("src", "//www.youtube.com/embed/6jaS81hUcwk?wmode=transparent&rel=0");
    $("#searchBox .youtubeLink").attr("href", "http://www.youtube.com/watch?v=6jaS81hUcwk");
    $("#contributeBox .youtube").attr("src", "//www.youtube.com/embed/Jo_tvwv8voU?wmode=transparent&rel=0");
    $("#contributeBox .youtubeLink").attr("href", "http://www.youtube.com/watch?v=Jo_tvwv8voU");
    $("#resourceBox .youtube").attr("src", "//www.youtube.com/embed/n-Iu4_gToDk?wmode=transparent&rel=0");
    $("#resourceBox .youtubeLink").attr("href", "http://www.youtube.com/watch?v=n-Iu4_gToDk");
    $("#librariesBox .youtube").attr("src", "//www.youtube.com/embed/7VgpIoUx5Fs?wmode=transparent&rel=0");
    $("#librariesBox .youtubeLink").attr("href", "http://www.youtube.com/watch?v=7VgpIoUx5Fs");
    $("#shareBox .youtube").attr("src", "//www.youtube.com/embed/pWJ9o7nGxTM?wmode=transparent&rel=0");
    $("#shareBox .youtubeLink").attr("href", "http://www.youtube.com/watch?v=pWJ9o7nGxTM");
    videosLoaded = true;
  }

  function redrawLines() {
    //Search box to...
    var overviewBox = $("#overviewBox");
    var searchBox = $("#searchBox");
    var contributeBox = $("#contributeBox");
    var resourceBox = $("#resourceBox");
    var librariesBox = $("#librariesBox");
    var isSmall = contributeBox.css("display") == "block";
    var shareBox = $("#shareBox");

    //if (!isSmall && !videosLoaded) {
    if(true){
      loadVideos();
    }

    overviewBox.sPosition = overviewBox.position();
    overviewBox.sPosition.outerWidth = overviewBox.outerWidth() + parseInt(overviewBox.css("margin-left").replace("px", ""));
    overviewBox.sPosition.bottom = overviewBox.sPosition.top + parseInt(overviewBox.css("margin-top").replace("px", ""));
    overviewBox.sPosition.left1 = overviewBox.sPosition.left + (overviewBox.sPosition.outerWidth * 0.2);
    overviewBox.sPosition.left2 = overviewBox.sPosition.left + (overviewBox.sPosition.outerWidth * (isSmall ? 0.95 : 0.8));

    searchBox.sPosition = searchBox.position();
    searchBox.sPosition.outerWidth = searchBox.outerWidth() + parseInt(searchBox.css("margin-left").replace("px",""));
    searchBox.sPosition.bottom = searchBox.sPosition.top + searchBox.outerHeight() + parseInt(searchBox.css("margin-top").replace("px", ""));
    searchBox.sPosition.left1 = searchBox.sPosition.left + (searchBox.sPosition.outerWidth * (isSmall ? 0.08 : 0.25));
    searchBox.sPosition.left2 = searchBox.sPosition.left + (searchBox.sPosition.outerWidth * (isSmall ? 0.18 : 0.85));

    contributeBox.sPosition = contributeBox.position();
    contributeBox.sPosition.bottom = contributeBox.sPosition.top + contributeBox.outerHeight() + parseInt(contributeBox.css("margin-top").replace("px", ""));
    contributeBox.sPosition.left1 = contributeBox.sPosition.left + (contributeBox.outerWidth() * (isSmall ? 0.8 : 0.6));

    resourceBox.sPosition = resourceBox.position();
    resourceBox.sPosition.outerWidth = resourceBox.outerWidth();
    resourceBox.sPosition.top = resourceBox.sPosition.top + parseInt(resourceBox.css("margin-top").replace("px", ""));
    resourceBox.sPosition.bottom = resourceBox.sPosition.top + resourceBox.outerHeight();
    resourceBox.sPosition.left = resourceBox.sPosition.left + parseInt(resourceBox.css("margin-left").replace("px", ""));
    resourceBox.sPosition.left1 = resourceBox.sPosition.left + (resourceBox.sPosition.outerWidth * 0.5);
    resourceBox.sPosition.left2 = resourceBox.sPosition.left + (resourceBox.sPosition.outerWidth * 0.9);

    librariesBox.sPosition = librariesBox.position();
    librariesBox.sPosition.top = librariesBox.sPosition.top + parseInt(librariesBox.css("margin-top").replace("px", ""));
    librariesBox.sPosition.bottom = librariesBox.sPosition.top + librariesBox.outerHeight();
    librariesBox.sPosition.left1 = librariesBox.sPosition.left + (librariesBox.outerWidth() * 0.5);

    shareBox.sPosition = shareBox.position();
    shareBox.sPosition.top = shareBox.sPosition.top + parseInt(shareBox.css("margin-top").replace("px", ""));

    $("line#overviewToSearch").attr("x1", overviewBox.sPosition.left1).attr("y1", overviewBox.sPosition.bottom).attr("x2", overviewBox.sPosition.left1).attr("y2", searchBox.sPosition.top);
    $("line#overviewToContribute").attr("x1", overviewBox.sPosition.left2).attr("y1", overviewBox.sPosition.bottom).attr("x2", overviewBox.sPosition.left2).attr("y2", contributeBox.sPosition.top);
    $("line#searchToLibraries").attr("x1", searchBox.sPosition.left1).attr("y1", searchBox.sPosition.bottom).attr("x2", searchBox.sPosition.left1).attr("y2", librariesBox.sPosition.top);
    $("line#searchToResource").attr("x1", searchBox.sPosition.left2).attr("y1", searchBox.sPosition.bottom).attr("x2", searchBox.sPosition.left2).attr("y2", resourceBox.sPosition.top);
    $("line#contributeToResource").attr("x1", contributeBox.sPosition.left1).attr("y1", contributeBox.sPosition.bottom).attr("x2", contributeBox.sPosition.left1).attr("y2", resourceBox.sPosition.top);
    $("line#resourceToLibraries").attr("x1", resourceBox.sPosition.left1).attr("y1", resourceBox.sPosition.bottom).attr("x2", resourceBox.sPosition.left1).attr("y2", librariesBox.sPosition.top);
    $("line#librariesToShare").attr("x1", librariesBox.sPosition.left1).attr("y1", librariesBox.sPosition.bottom).attr("x2", librariesBox.sPosition.left1).attr("y2", shareBox.sPosition.top);
    $("line#resourceToShare").attr("x1", resourceBox.sPosition.left2).attr("y1", resourceBox.sPosition.bottom).attr("x2", resourceBox.sPosition.left2).attr("y2", shareBox.sPosition.top);
  }
</script>

<style type="text/css">

  /* Big stuff */
  * { box-sizing: border-box; -moz-box-sizing: border-box; font-size: 16px; }
  #content { min-width: 300px; transition: padding 1s; -webkit-transition: padding 1s; }
  #presentation { position: relative; max-width: 1200px; margin: 0 auto; }
  #steps { position: relative; z-index: 10; }
  .step { margin-bottom: 75px; padding: 0 10px; }
  .group { padding: 0px 10px 10px 30px; border-radius: 5px; background-color: #EEE; margin: 5px 0; position: relative; margin-left: 25px; }
  .group .icon { position: absolute; top: 0; left: -25px; background-color: #CCC; border-radius: 50%; width: 50px; }
  .group .data { overflow: auto; }
  h2 { font-size: 24px; }
  h3 { font-style: italic; color: #333; }
  .youtubeBox img, .slideshareBox img { width: 100%; }
  .youtubeBox, .slideshareBox { position: relative; max-width: 550px; margin: 5px 0 5px 5px; }
  #searchBox .youtubeBox, #contributeBox .youtubeBox { margin: 5px 0 0 0; }
  .youtube { width: 100%; height: 100%; position: absolute; top: 0; left: 0; }
  .youtubeLink { display: none; font-weight: bold; text-align: right; padding: 2px; }
  .downlinks { margin-top: 5px; font-size: 0; }
  .downlinks a { display: inline-block; background-color: #3572B8; color: #FFF; width: 33%; margin-right: 0.5%; text-align: center; padding: 2px 1px; height: 2.6em; vertical-align: top; }
  .downlinks a:hover, .downlinks a:focus { background-color: #FF6A00 ; }
  .downlinks a:first-child { border-radius: 5px 0 0 5px; }
  .downlinks a:last-child { margin-right: 0; border-radius: 0 5px 5px 0; }
	.downlinks.bigLink a { width: 100%; border-radius: 5px; }
  .pdfLink { display: block; font-weight: bold; text-align: right; padding: 2px; }
	.slideshareBox iframe { border: none; position: absolute; left: 0; top: 0; width: 100%; height: 100%; }

  /* Individualism */
  #step1 { font-size: 0; }
  #searchBox, #contributeBox { width: calc(49% - 25px); display: inline-block; vertical-align: top; height: 100%; }
  #searchBox { margin-right: 1%; }
  #contributeBox { margin-left: calc(1% + 25px); }
  #resourceBox { margin-left: 20%; }
  #librariesBox { margin-right: 20%; }
  .data .youtubeBox, .data .slideshareBox { float: right; }
	.data .mediaBox .youtubeBox, .data .mediaBox .slideshareBox { float: none; display: inline-block; vertical-align: top; width: 31%; margin: 5px 1%; }
	.data .mediaBox { text-align: center; }
	#contributeBox .slideshareBox { margin: 5px 0 0 0; }
	#step3 .data .mediaBox .youtubeBox, #step3 .data .mediaBox .slideshareBox { width: 48%; }

  /* SVG */
  #arrows { width: 100%; height: 100%; position: absolute; }
  #arrows line { stroke: #999; stroke-width: 1%; marker-end: url(#triangle); }
  #arrows marker { fill: #999; }

  /* Responsive */
  @media screen and (min-width: 980px) {
    #content { padding-left: 50px; }
  }
  @media screen and (max-width: 800px) {
    .data span { display: block; }
    .data .youtubeBox, .data .slideshareBox { float: none; margin: 0 auto; display: block; }
  }
  @media screen and (max-width: 650px) {
    /*.data .youtubeBox, .youtubeBox { display: none; }
    .youtubeLink { display: block; }*/
    .downlinks a { display: block; width: 100%; margin-bottom: 1px; height: auto; padding: 5px 2px; }
    .downlinks a:first-child { border-radius: 5px 5px 0 0; }
    .downlinks a:last-child { margin-right: 0; border-radius: 0 0 5px 5px; }
  }
  @media screen and (max-width: 500px) {
    .group { padding: 0 5px 5px 15px; margin-left: 12px; }
    .group .icon { left: -12px; width: 25px; }
    #searchBox, #contributeBox { display: block; }
    #searchBox { width: auto; margin-right: 15%; }
    #contributeBox { width: auto; margin-left: calc(20% + 12px); }
    #arrows line { stroke-width: 1%; }
    #resourceBox { margin-left: 15%; }
    #librariesBox { margin-right: 15%; }
  }

</style>

<div id="content">

  <h1 class="isleH1">Illinois Open Education Resources User Guide</h1>

  <div id="presentation">

    <svg id="arrows">
      <marker id="triangle" viewBox="0 0 8 10" refX="7" refY="5" markerUnits="strokeWidth" markerWidth="3" markerHeight="3" orient="auto">
			  <path d="M 0 0 L 10 5 L 0 10 z" />
			</marker>
      <line id="overviewToSearch" />
      <line id="overviewToContribute" />
      <line id="searchToLibraries" />
      <line id="searchToResource" />
      <line id="contributeToResource" />
      <line id="resourceToLibraries" />
      <line id="librariesToShare" />
      <line id="resourceToShare" />
    </svg>

    <div id="steps">

      <div class="group" id="step-1" style="padding:10px; margin: 10px; display:none;">
        <p style=" font-size: 20px; font-weight:bold; text-align: center;">Thank you for participating in today's Session!  <a href="http://surveymonkey.com/s/VWWQLQ5" target="_blank" style="font-size:inherit; font-weight:inherit;">Please let us know what you think!</a></p>
      </div>

      <div class="step" id="step0">
        <div class="group" id="overviewBox">
          <img alt="" src="/images/icons/icon_help_med.png" class="icon" />
          <h2>Overview</h2>
          <h3>"What is this site all about?"</h3>
          <div class="data">
            <span>IOER provides you with one-click access to open, standards-aligned educational content. Use our tools to find, remix, and comment on resources for your personalized IOER learning library. Hosting more than 200,000 open and available learning resources, IOER provides specific, standards-aligned resources utilizing filters and engaging tools to refine and share quality, peer-reviewed educational collections and resources. </span>
            <a class="pdfLink" href="/OERThumbs/files/QuickStart.pdf" target="_blank">Quick Start Guide (PDF)</a>
						<a class="pdfLink" href="/OERThumbs/files/2015IllinoisECET2Final.pdf" target="_blank">PowerPoint for ISLE OER session at 2015 Illinois ECET2: Teachers Leading the Way! (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/Org_instructions.pdf" target="_blank">Organization Administration Guide (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/Admin.pdf" target="_blank">IOER Administration Guide (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/Overview.pptx" target="_blank">IOER Overview (PPTX)</a>
            <a class="youtubeLink" href="http://www.youtube.com/watch?v=j2wsNSGQQx4" target="_blank">Watch Video &rarr;</a>
						<div class="mediaBox">
							<div class="slideshareBox">
								<img alt="" style="width:89%;" src="/images/ThumbnailResizer.png">
								<iframe src="https://www.slideshare.net/slideshow/embed_code/key/5NUM0MvV2t4w9f?rel=0"></iframe>
							</div><!--
							--><div class="slideshareBox">
								<img alt="" style="width:89%;" src="/images/ThumbnailResizer.png">
								<iframe src="https://www.slideshare.net/slideshow/embed_code/key/ZYZSKdcF28kdT?rel=0"></iframe>
							</div><!--
							--><div class="youtubeBox">
								<img alt="" src="/images/youtube-autoresizer.png" />
								<iframe class="youtube" src="" frameborder="0" allowfullscreen></iframe>
							</div>
						</div>
          </div>
        </div>
      </div>

      <div class="step" id="step1">
        <div class="group" id="searchBox">
          <img alt="" src="/images/icons/icon_search_med.png" class="icon" />
          <h2>Search</h2>
          <h3>"I want to find Resources"</h3>
          <div class="data">
            <span>Providing a wide variety of Filters to refine your search, IOER have developed robust criteria for your search, such as Standards, Grade Level, Subjects and Career Clusters.  Finding quality learning materials is easier than ever with tools to sort and organize by Newest, Most Liked, Most Commented On, and a wide variety of Views and Libraries.</span>
            <a class="pdfLink" href="/OERThumbs/files/Search.pdf" target="_blank">Search Guide (PDF)</a>
          </div>
          <div class="youtubeBox">
            <img alt="" src="/images/youtube-autoresizer.png" />
            <iframe class="youtube" src="" frameborder="0" allowfullscreen></iframe>
          </div>
          <a class="youtubeLink" href="http://www.youtube.com/watch?v=FedkwWdEiio" target="_blank">Watch Video &rarr;</a>
          <div class="downlinks">
            <a href="/Libraries/Default.aspx" target="_blank">Libraries Search</a>
						<a href="/learninglists" target="_blank">Learning Lists Search</a>
            <a href="/Search.aspx" target="_blank">Resources Search</a>
          </div>
        </div>

        <div class="group" id="contributeBox">
          <img alt="" src="/images/icons/icon_tag_med.png" class="icon" />
          <h2>Share</h2>
          <h3>"I want to submit Resources"</h3>
          <div class="data">
            <span>Many options for Contributing in IOER allow you to quickly tag a resource using standards alignment and keywords, create a new resource from your computer directly to the Internet, as well as more detailed tagging and creation tools.  Fast or methodical, IOER has learning resources for everyone.</span>
            <a class="pdfLink" href="/OERThumbs/files/Tagger.pdf" target="_blank">Full Tagger Guide (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/Contribute.pdf" target="_blank">Contribute Guide (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/LearningLists_about.pdf" target="_blank">About Learning Lists (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/LearningLists_create.pdf" target="_blank">How To Create Learning Lists (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/LearningLists_instructions.pdf" target="_blank">Learning List Instructions (PDF)</a>
          </div>
					<div class="slideshareBox">
						<img alt="" style="width:110%;" src="/images/ThumbnailResizer.png">
						<iframe src="https://www.slideshare.net/slideshow/embed_code/key/2VY49zf7k3yAwF?rel=0"></iframe>
					</div>
					<div class="slideshareBox">
						<img alt="" style="width:85%;" src="/images/ThumbnailResizer.png">
						<iframe src="https://www.slideshare.net/slideshow/embed_code/key/8KirWPmMossbbN?rel=0"></iframe>
					</div>
          <div class="youtubeBox">
            <img alt="" src="/images/youtube-autoresizer.png" />
            <iframe class="youtube" src="" frameborder="0" allowfullscreen></iframe>
          </div>
          <a class="youtubeLink" href="http://www.youtube.com/watch?v=AsuiH4bUdKY" target="_blank">Watch Video &rarr;</a>
					<div class="downlinks bigLink">
						<a href="/Contribute">Contribute a Resource</a>
					</div>
          <!--<div class="downlinks">
            <a href="/Publish.aspx" target="_blank">Tagging Tool</a>
            <a href="/My/Author.aspx" target="_blank">Authoring Tool</a>
            <a href="/Contribute" target="_blank">Quick Contribute</a>
          </div>-->
        </div>
      </div>

      <div class="step" id="step2">
        <div class="group" id="resourceBox">
          <img alt="" src="/images/icons/icon_resources_med.png" class="icon" />
          <h2>Resources</h2>
          <h3>"I want to learn about a Resource"</h3>
          <div class="data">
            <div class="youtubeBox">
              <img alt="" src="/images/youtube-autoresizer.png" />
              <iframe class="youtube" src="" frameborder="0" allowfullscreen></iframe>
            </div>
            <span>Each resource has its own Detail Page, providing in-depth information about each resource found in IOER.  In addition to highly detailed standards-alignment tabs, Commenting, Likes and Sharing options are available with just a click for each resource in the Learning Registry, through IOER.</span>
            <a class="pdfLink" href="/OERThumbs/files/Detail.pdf" target="_blank">Detail Page Guide (PDF)</a>
          </div>
          <a class="youtubeLink" href="http://www.youtube.com/watch?v=TSvmUeVdGuQ" target="_blank">Watch Video &rarr;</a>
        </div>
      </div>

      <div class="step" id="step3">
        <div class="group" id="librariesBox">
          <img alt="" src="/images/icons/icon_library_med.png" class="icon" />
          <h2>Libraries</h2>
          <h3>"I want to catalog and organize Resources"</h3>
          <div class="data">
						<div class="mediaBox">
							<div class="slideshareBox">
								<img alt="" style="width:85%;" src="/images/ThumbnailResizer.png">
								<iframe src="https://www.slideshare.net/slideshow/embed_code/key/nzF9ILMPS4z05e?rel=0"></iframe>
							</div><!--
							--><div class="youtubeBox">
								<img alt="" src="/images/youtube-autoresizer.png" />
								<iframe class="youtube" src="" frameborder="0" allowfullscreen></iframe>
							</div>
						</div>
            <span>IOER Libraries provide many ways for you to tag, contribute, create, organize and share your learning resources with fast and easy-to-use tools that allow for public and private settings.  User and Organizational Libraries allow individuals and groups to quickly categorize their learning resources in so many ways.</span>
            <a class="pdfLink" href="/OERThumbs/files/Libraries.pdf" target="_blank">Libraries Guide (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/Libraries_howto.pdf" target="_blank">How To Create Libraries (PDF)</a>
            <a class="pdfLink" href="/OERThumbs/files/Libraries_instructions.pdf" target="_blank">Libraries Instructions (PDF)</a>
          </div>
          <a class="youtubeLink" href="http://www.youtube.com/watch?v=bpUqQR0YZTA" target="_blank">Watch Video &rarr;</a>
        </div>
      </div>

      <div class="step" id="step4">
        <div class="group" id="shareBox">
          <iimg alt=""mg src="/images/icons/icon_swirl_med.png" class="icon" />
          <h2>Community</h2>
          <h3>"I want to share Resources with my colleagues"</h3>
          <div class="data">
            <div class="youtubeBox">
              <img alt="" src="/images/youtube-autoresizer.png" />
              <iframe class="youtube" src="" frameborder="0" allowfullscreen></iframe>
            </div>
            <span>Community-building is what IOER is all about!  As you begin building your selected library of chosen collections of learning resources, you will find a continued focus on adding responsive design tools to assist you in the development of your learning environment.</span>
            <a class="pdfLink" href="/OERThumbs/files/Sharing.pdf" target="_blank">Sharing Guide (PDF)</a>
          </div>
          <a class="youtubeLink" href="#" target="_blank">Watch Video &rarr;</a>
        </div>
      </div>
    </div>
  </div>

</div>