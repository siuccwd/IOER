<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ElasticSearch3.ascx.cs" Inherits="IOER.LRW.controls.ElasticSearch3" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/LRW/controls/StandardsBrowser5.ascx" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser7" Src="/Controls/StandardsBrowser7.ascx" %>

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

  <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>ISLE Open Educational Resources Search</title>
    <!-- Externally-loaded stuff -->
    <script src="/Scripts/paradataRenderer.js"></script>
    <link rel="Stylesheet" type="text/css" href="/Styles/paradataRenderer.css" />

    <!-- General CSS -->
    <style type="text/css">
      /* Major Page stuff */
      * { box-sizing: border-box; -moz-box-sizing: border-box; font-size: 16px; }
      html, body { min-width: 300px; }
      div#header { min-width: 300px; }
      #container { padding-bottom: 50px; min-height: 1000px; min-width: 300px; }
      #search { min-width: 300px; }

      #searchBar { font-size: 2em; width: 100%; padding-right: 40px; }
      #searchBar::-ms-clear { display: none; }
      #filters { background-color: #FFF; }
      #filterLinks a { display: block; padding: 5px 5px; text-align: right;  }
      .filterBox { display: none; position: absolute; top: 0; z-index: 1000; padding: 5px; overflow: hidden; }
      #filters > h2, .filterBox > h2 { font-size: 1.2em; }
      .filterBox .list a { display: block; width: 100%; padding: 5px; font-size: 1.1em; }
      .thumbnail { background-color: #EEE; width: 100%; }
      .thumbnail h3 { padding: 1px 2px; text-align: right; display: none; }
      .thumbnail p { text-align: center; font-weight: bold; font-size: 24px; padding-top: 25px; }
      #options { padding: 5px; position: relative; }
      #resultCount { text-align: center; padding: 5px; }
      #btnActiveFiltersReset, #btnResetSearch { display: inline-block; font-weight: bold; padding: 5px; vertical-align: top; }
      #display select { margin: 2px 0; padding: 2px; }
      #activeFiltersList { display: inline; }
      #activeFiltersList .activeList { margin-bottom: 10px; display: inline; }
      /*#activeFiltersList .activeList .activeFilterItem:first-child { border-top-left-radius: 5px; border-top-right-radius: 5px; }
      #activeFiltersList .activeList .activeFilterItem:last-child { border-bottom-left-radius: 5px; border-bottom-right-radius: 5px; }*/
      .activeFilterItem { margin: 1px 0; border: 1px solid #D55; min-height: 28px; padding: 2px; overflow: hidden; display: inline-block; border-radius: 5px; vertical-align: top; }
      .activeFilterItem a, .filterBox .closeBox { float: right; margin: 0 0 2px 2px; display: inline-block; width: 20px; height: 20px; line-height: 20px; font-weight: bold; background-color: #D55; color: #FFF; border-radius: 5px; box-shadow: 0 5px 10px -5px #FFF inset; text-align: center; }
      .activeFilterItem a:hover, .activeFilterItem a:focus, .filterBox .closeBox:hover, .filterBox .closeBox:focus { background-color: #F00; }
      
      .paginator { background-color: #EEE; padding: 5px; border-radius: 5px; text-align: center; margin: 5px auto; display: none; }
      .paginator a { display: inline-block; min-width: 30px; height: 30px; line-height: 30px; padding: 0 5px; margin: 2px; border-radius: 5px; }
      
      .result_list .paradata { position: absolute; bottom: 0; box-shadow: 0 0 225px 0px #333 inset; color: #FFF; display: block; margin-left: 0; }
      .result_grid .paradata { height: 60px; margin-left: 0; }
      
      /* Results Rendering */
      .resultList { text-align: center; font-size: 0; }
      .result { text-align: left; font-size: 16px; }
      /*.result .thumbnailLink { display: block; width: 200px; height: 150px; position: relative; }*/
      
      .result_list { margin: 0; padding: 5px 5px 50px 5px; border-radius: 5px; position: relative; }
      .result_list .thumbnailLink { float: right; margin-left: 5px; box-shadow: 0 0 10px #CCC; position: relative; width: 200px; background-color: #EEE; }
      .result_list .miniMetadata { clear: both; color: #555; font-style: italic; }
      .result_list h2 { font-size: 20px; }
      .result_list h2 a { font-size: inherit; }
      .result_list .data p, .result_compact > p { overflow: hidden; }
      .result .thumbnail p { margin: 0; }
      .result .fadeCollapse { position: relative; }
      .result .fadeBox { padding-bottom: 50px; }
      .result .fadeCollapse .collapser { 
        position: absolute; 
        bottom: 0; 
        left: 0; 
        text-align: right; 
        padding: 5px 10px; 
        box-shadow: 0 -25px 40px 0 #FFF inset;
        width: 100%;
        font-weight: bold; 
      }
      .result_list .mainMetadata { text-align: right; height: 150px; padding: 0 5px; float: right; overflow: hidden; }
      .result_grid {
        display: inline-block;
        margin: 10px 2.5px;
        vertical-align: top;
        position: relative;
        width: 300px;
        box-shadow: 0 0 10px #CCC;
      }
      .result_grid h2 {
        /* box-shadow: 0 0 225px 0px #333 inset; */
        padding: 2px;
        font-weight: normal;
        width: 100%;
        overflow: hidden;
        height: 2.6em;
      }
      
      .result_compact { margin: 5px 5px 20px 5px; }
      .result_compact h2 { font-size: 20px; }
      .result_compact h2 a { font-size: inherit; }
      .mainMetadata h3 { color: #777; font-style: italic; font-weight: normal; }
      .created { float: right; display: inline-block; }
      
      .activeFilterItem a[data-group=libraryIDs], .activeFilterItem a[data-group=collectionIDs] { display: none; }

      #ddls { text-align: center; }
      #ddls select { width: 31%; margin: 1px 0.5%; }

      .previewerButton { background-color: #3572B8; color: #FFF; padding: 2px 5px; border-radius: 5px; border-width: 1px; display: block; position: absolute; top: 8px; right: 8px; }
      .previewerButton:hover, .previewerButton:focus { background-color: #FF5707; cursor: pointer; }
    </style>
    <!-- Themes and colors -->
    <style type="text/css">
      #filterLinks a:hover, #filterLinks a:focus { background-color: <%=css_orange %>; color: #FFF; }
      .filterBox { background-color: #EEE; border-top-right-radius: 5px; border-bottom-right-radius: 5px; }
      #filters > h2, .filterBox > h2 { background-color: <%=css_teal %>; color: #FFF; margin: -5px -5px 10px -5px; padding: 2px 5px; }
      .filterBox .list a { border-left: 15px solid <%=css_blue %>; }
      .filterBox .list a[data-selected=true] { border-left: 15px solid <%=css_orange %>; background-color: <%=css_purple %>; }
      .filterBox .list a:hover, .filterBox .list a:focus, #btnShowFilterLinks:hover, #btnShowFilterLinks:focus { background-color: <%=css_orange %>; color: #FFF; }
      #options { background-color: #EEE; }
      #filterLinks, .filterBox .list { border-radius: 5px; overflow: hidden; }
      #filterLinks a, .filterBox .list a, #btnShowFilterLinks { margin-bottom: 1px; background-color: <%=css_blue %>; color: #FFF; }
      #filterLinks a:last-child, .filterBox .list a:last-child { margin-bottom: 0; }
      #btnActiveFiltersReset, #btnResetSearch { background-color: <%=css_blue %>; color: #FFF; border-radius: 5px; border: 1px solid #FFF; box-shadow: 0 5px 25px -5px #FFF inset; }
      #btnActiveFiltersReset:hover, #btnActiveFiltersReset:focus, #btnResetSearch:hover, #btnResetSearch:focus { cursor: pointer; background-color: <%=css_orange %>; }
      .paginator a { background-color: <%=css_blue %>; color: #FFF; }
      .paginator a:hover, .paginator a:focus { background-color: <%=css_orange %>; color: #FFF; }
      .paginator a.currentPage { background-color: <%=css_purple %>; }
      .result_list .fadeCollapse .collapser:hover, .result_list .fadeCollapse .collapser:focus { color: <%=css_orange %>; }
      .result_list { box-shadow: 0 10px 80px -30px #CCC inset; }
      .metadataLink:hover, .metadataLink:focus { text-decoration: underline; }
      .miniMetadata a { display: inline-block; min-width: 250px; }
      .miniMetadata a:first-child { display: block; width: 100%; }
      
      .sbFoundResultsLink, .sbFoundNoResults { border-radius: 5px; padding: 5px; margin: 10px 0; text-align: center; }
      .sbFoundResultsLink { display: block; background-color: <%=css_blue %>; text-decoration: none; color: #FFF; font-weight: bold; }
      #sbRightColumn a.sbFoundResultsLink:hover, #sbRightColumn a.sbFoundResultsLink:focus { background-color: <%=css_orange %>; color: #FFF; text-decoration: none; }
      .sbFoundNoResults { background-color: #D55; color: #FFF; }
      #searchBarBox { position: relative; }
      #searchBarX { position: absolute; background-color: #D55; color: #FFF; font-weight: bold; text-align: center; padding: 2px 10px; right: 0; top: 0; font-size: 2em; border-top-right-radius: 5px; border-bottom-right-radius: 5px; display: none; }
      #searchBarX:hover, #searchBarX:focus { background-color: #F00; }
    </style>
    <!-- Responsive CSS -->
    <style type="text/css">
      @media screen and (min-width: 0px), (min-width: 450px) {
        #filters { position: absolute; z-index: 1000; width: 160px; }
        #filters #filterLinks { display: none; }
        #filters .filterBox { width: 65%; left: 160px; }
        #filters #filterLinks a { font-size: 1.1em; }
        #btnShowFilterLinks { display: inline-block; width: 100%; text-align: center; font-weight: bold; font-size: 20px; height: 30px; line-height: 30px; vertical-align: top; margin: 2px 0; }
        #filters #activeFilters { display: none; }
        #display h2 { display: none; }
        #display { display: inline-block; width: 59%; }
        #display select { width: 48%; }
        #display select#ddlSorting { width: 99%; }
        .mainMetadata { display: none; }
        #options { border-radius: 0; margin: 5px 0; }
        #display { margin: 0; }
        .result_grid { width: 300px; margin: 5px }
        .result_list .thumbnailLink { max-width: 30%; }
        /* Theme and colors */
        #filters { padding: 5px; background-color: #EEE; left: 0; }
        #filters > h2, .filterBox > h2 { margin-top: 0; }
        #btnShowFilterLinks { border-radius: 5px; }
        .filterBox .closeBox { margin-top: 4px; }
        #searchBar, #searchBarX { font-size: 1.5em; }
      }
      @media screen and (max-width: 550px) {
        .result_list .paradata { display: none; }
      }
      @media screen and (min-width: 0px) and (max-width: 530px) {
        .result_grid { width: 48%; margin: 5px 2.5px; }      
      }
      @media screen and (max-width: 650px) {
        .result_list .paradata li { max-width: 25px; text-align: right; transform: scale(.75, .75); -webkit-transform: scale(.75, .75); }
      }
      @media screen and (min-width: 5000px) {
        #filters { position: relative; width: 100%; }
        #filters #filterLinks { display: block; }
        #filters .filterBox { width: 300px; left: 195px; }
        #filters #filterLinks a { font-size: 1.2em; }
        #btnShowFilterLinks { display: none; }
        #filters #activeFilters { display: block; }
        #options { width: 200px; display: inline-block; vertical-align: top; position: absolute; }
        #search { display: inline-block; vertical-align: top; margin-right: -10px; width: 100%; padding-left: 205px; }
        #display { width: 100%; }
        #display select { width: 100%; }
        .mainMetadata { display: none; }
        #display select#ddlSorting { width: 100%; }
        #options { border-radius: 5px; margin: 0; }
        #display: { margin: 5px 0; }
        /* Theme and colors*/
        #filters { padding: 0; }
        #filters > h2, .filterBox > h2 { margin-top: -5px; }
        #filters > h2 { border-top-left-radius: 5px; }
        .filterBox[data-maxwidth=true] { width: 100%; }
        .filterBox .closeBox { margin-top: -1px; }
        #filters .filterBox[data-filtername=standardsBrowser] { left: 190px; }  
        #searchBar, #searchBarX { font-size: 2em; }
      }
      @media screen and (min-width: 1000px) {
        .mainMetadata { display: block; width: 20%; }
      }
      @media screen and (min-width: 1500px) {

      }
      /* AddThis customizations */
      #container { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
      @media screen and (min-width:980px) {
        #container { padding-left: 50px; }
      }
      /* Mobile override fixes */
      @media screen and (min-width: 500px) {
        #options #filters .filterBox { width: 300px; }
        #options { width: 24%; display: inline-block; vertical-align: top; background-color: transparent; padding: 0; margin: 1px 0; }
        #ddls { width: 74%; display: inline-block; vertical-align: top; }
        #ddls select { height: 30px; margin-top: 3px; }
        #ioerResults { transition: padding 0.2s; -webkit-transition: padding 0.2s; }
        #filters > h2, .filterBox > h2 { margin-top: -5px; }
        .filterBox .closeBox { margin-top: 0; }
      }
      @media screen and (max-width: 1024px) {
        .result_grid { width: 31%; }
      }
      @media screen and (max-width: 500px) {
        .result_grid { width: 48%; }
      }
    </style>

    <!-- Global variables -->
    <script type="text/javascript">
      var pageSize = 20;
      var currentPage = 1;
      var renderMode = "list";
      var sortMode = { field: "intID", order: "desc" };
      var requests = [];
      var countDown = -1;
      var responsiveSize = "large";
      var currentResponsiveSize = "";
      var filterLists = [
        { name: "gradeLevel", es: "gradeLevelIDs", esName: "gradeLevels" },
        { name: "accessRights", es: "accessRightsID", esName: "accessRights" },
        { name: "careerCluster", es: "clusterIDs", esName: "clusters" },
        { name: "educationalUse", es: "educationalUseIDs", esName: "educationalUses" },
        { name: "endUser", es: "audienceIDs", esName: "audiences" },
        { name: "groupType", es: "groupTypeIDs", esName: "groupTypes" },
        { name: "language", es: "languageIDs", esName: "languages" },
        { name: "mediaType", es: "mediaTypeIDs", esName: "mediaTypes" },
        { name: "resourceType", es: "resourceTypeIDs", esName: "resourceTypes" }
      ];
      //var filterLists = ["gradeLevel", "accessRights", "careerCluster", "educationalUse", "endUser", "groupType", "language", "mediaType", "resourceType"];
      var activeFiltering = {
        idFilters: [],
        textFilters: []
      };
      var autoTextFilters = [
        { terms: ["freesound", "free sound", "sound"], filter: "-freesound", applied: true },
        { terms: ["delete"], filter: "-delete", applied: true },
        { terms: ["bookshare"], filter: "-bookshare", applied: true },
        { terms: ["Smarter Balanced Assessment Consortium"], filter: "-\"Smarter Balanced Assessment Consortium\"", applied: true }
      ];
      var thumbDivTypes = [
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
        //{ match: ".pdf", header: "Adobe PDF", file: "filethumb_pdf_200x150.png" },
        /*{ match: ".doc", header: "Microsoft Word Document", file: "filethumb_docx_200x150.png" },
        { match: ".docx", header: "Microsoft Word Document", file: "filethumb_docx_200x150.png" },
        { match: ".ppt", header: "Microsoft PowerPoint Document", file: "filethumb_pptx_200x150.png" },
        { match: ".pptx", header: "Microsoft PowerPoint Document", file: "filethumb_pptx_200x150.png" },
        { match: ".xls", header: "Microsoft Excel Spreadsheet", file: "filethumb_xlsx_200x150.png" },
        { match: ".xlsx", header: "Microsoft Excel Spreadsheet", file: "filethumb_xlsx_200x150.png" },*/
        { match: ".zip", header: "Archive File", file: "icon_zip_400x300.png" },
        { match: ".rar", header: "Archive File", file: "icon_zip_400x300.png" },
        { match: ".7z", header: "Archive File", file: "icon_zip_400x300.png" },
        { match: ".tar", header: "Archive File", file: "icon_zip_400x300.png" }
      ];

      var debugResults = {};
      var resultHits = 0;
      var browserBox;
      var standardsFilter = { field: "standardIDs", title: "Learning Standards", es: "standardIDs", items: [] };
      var extraStandards = [];
    </script>
    <!-- Responsive JS -->
    <script type="text/javascript">
      //Window Resizes
      $(window).on("resize", function () { renderContent($(this).width() + ($("body").height() > $(window).height() ? 17 : 0)); });
      //Responsive Handling
      function renderContent(width) {
        //universal
        $("#pageTitle").css("font-size", 100 + (width / 10) + "%");

        responsiveSize =
          //(width >= 1950) ? "beyondHD" :
          (width >= 1500) ? "extraLarge" :
          (width >= 1000) ? "large" :
          (width >= 800) ? "medium" :
          (width >= 500) ? "small" : 
          "tiny";

        initFadeCollapse();

        //Call these constantly while resizing
        if (responsiveSize == "small" || responsiveSize == "tiny") {
          //$("#filters .filterBox").width($("#options").width() - $("#filters").outerWidth());
          //$("#filters .filterBox[data-filtername=standardsBrowser]").width($("#options").outerWidth() - 10);
          if (responsiveSize == "tiny") {
            $("#filters .filterBox").css("left", "60px");
            $("#filters .filterBox").width($("#searchBarBox").outerWidth() - 70);
          }
          else {
            $("#filters .filterBox").css("left", "160px");
            $("#filters .filterBox").width($("#searchBarBox").outerWidth() - $("#filters").outerWidth() - 10);
          }
          $("#filters .filterBox[data-filtername=standardsBrowser]").width($("#searchBarBox").outerWidth() - 10);
          $("#filters .filterBox[data-filtername=standardsBrowser]").css("left", "0");
        }
        else {
          $("#filters .filterBox").width(300);
          $("#filters .filterBox[data-maxwidth=true]").width($("#searchBarBox").outerWidth() - 170);
          $("#filters .filterBox[data-filtername=standardsBrowser]").css("left", "160px");
          $("#filters .filterBox").css("left", "160px");
        }

        var divheight = ($("div.thumbnail").first().width() / 300) * 225;
        $("div.thumbnail").height(divheight);

        if (currentResponsiveSize != responsiveSize) {
          currentResponsiveSize = responsiveSize;
          //Only call these when the size type changes

          //move the search bar on small/tiny mode
          if (responsiveSize == "small" || responsiveSize == "tiny" || responsiveSize == "medium" || responsiveSize == "large" || responsiveSize == "extraLarge") {
            if ($("#searchBarBox").attr("data-responsivemode") == "normal") {
              //$("#searchBarBox").insertBefore("#options"); //moves the bar between the header and filters
              $("#searchBarBox").attr("data-responsivemode", "compact");
              initSearchBar();
              //$("#ddlRendering option").attr("selected", "false");
              //$("#ddlRendering option[value=compact]").attr("selected", "selected");
              //renderMode = "compact";
              //$("#ddlRendering").trigger("change");
            }
            hideFiltering();
          }
          else if (responsiveSize != "small" && responsiveSize != "tiny") {
            if ($("#searchBarBox").attr("data-responsivemode") == "compact") {
              //$("#searchBarBox").prependTo("#search");
              $("#searchBarBox").attr("data-responsivemode", "normal");
              initSearchBar();
              //$("#ddlRendering option").attr("selected", "false");
              //$("#ddlRendering option[value=list]").attr("selected", "selected");
              //renderMode = "list";
              //$("#ddlRendering").trigger("change");
            }
            $("div.thumbnail").height(150);
            showFilterLinks();
          }
        }
      }
    </script>
    <!-- AJAX -->
    <script type="text/javascript">
      function doAjax(service, targetFunction, data, success, clearRequests) {
        (clearRequests ? requests = [] : {});
        var thisRequest = $.ajax({
          url: "/Services/" + service + "/" + targetFunction,
          async: true,
          success: function (msg) { (msg.d ? success(msg.d) : success(msg)); },
          type: "POST",
          data: JSON.stringify(data),
          contentType: "application/json; charset=utf-8",
          dataType: "json"
        });
        requests.push(thisRequest);
      }

      function doAjax2(service, targetFunction, data, success, clearRequests) {
        (clearRequests ? requests = [] : {});
        var thisRequest = $.ajax({
          url: "/Services/" + service + "/" + targetFunction,
          async: true,
          success: function (msg) { (msg.d ? success(msg.d) : success(msg)); },
          type: "POST",
          data: data,
          dataType: "json"
        });
        requests.push(thisRequest);
      }
    </script>
    <!-- Inits and Listeners -->
    <script type="text/javascript">
      $(document).ready(function () {
        /* Initialization */
        //Search bar
        initSearchBar();

        //CountDown
        setupCountDown();

        //Checkboxes
        setupCBXLs();

        //Auto-show standards browser
        if (window.location.href.toLowerCase().indexOf("showstandards") > -1) {
          setTimeout(function () {
            showFilterLinks();
            showFilter("standardsBrowser");
          }, 1000);
        }

        //Remove 3D from IE
        if (/*@cc_on!@*/!1) { //wacky IE detection
          $("#ddlRendering option[value=3d]").remove();
        }

        /* Listeners */
        //Filter clicking
        $("#filterLinks a").on("click", function (event) {
          showFilter($(this).attr("data-name"));
          setTimeout(function () { $(window).trigger("resize"); }, 260);
          return false;
        });

        //Change Display
        $("#ddlSorting").on("change", function () {
          sort($(this).find("option:selected").attr("value"));
        });

        //Change # of items to show
        $("#ddlDisplayCount").on("change", function () {
          changePageSize($(this).find("option:selected").attr("value"));
        });

        //Change page
        $(".paginatorLink").on("click", function () {
          gotoPage($(this).attr("data-page"));
        });

        //Change View Mode
        $("#ddlRendering").on("change", function () {
          changeRenderMode($(this).find("option:selected").attr("value"));
        });

        //Show/hide filters button
        $("#btnShowFilterLinks").on("click", function () {
          if ($("#filters").attr("data-showing") == "true") {
            hideFiltering();
            $("#ioerResults").css("padding-left", "0");
          }
          else {
            showFilterLinks();
            $("#ioerResults").css("padding-left", "165px");
          }
          if (responsiveSize == "small" || responsiveSize == "tiny") {
            $("#ioerResults").css("padding-left", "0");
          }
          $(window).trigger("resize");
          return false;
        });

        //Doesn't apply well without waiting first
        setTimeout(function () {
          $(".filterBox .closeBox").on("click", function () {
            showFilter($(this).parent().attr("data-filtername"));
            setTimeout(function () { $(window).trigger("resize"); }, 260);
            return false;
          });
        }, 1000);

        setTimeout(function () {
          //Default to English
          $("a[data-name=English]").trigger("click");
          //countDown = -1;

          //Handle incoming query
          var params = window.location.search.substr(1).split("&");
          for (i in params) {
            if (params[i].substr(0, 2) == "q=") {
              $("#searchBar").val(decodeURIComponent(params[i].substr(2)));
              resetCountDown();
            }
            else if (params[i].substr(0, 5) == "sids=") {
              var items = params[i].substr(5).split(",");
              for (i in items) {
                extraStandards.push({ text: "", id: parseInt(items[i]) });
              }
              resetCountDown();
            }
            else if (params[i].substr(0, 4) == "pub=") {
              $("#searchBar").val("infields[publisher]:" + decodeURIComponent(params[i].substr(4)));
              resetCountDown();
            }
            else if (params[i].substr(0, 5) == "sort=") {
              var options = params[i].substr(5);
              $("#ddlSorting option[data-val=" + options + "]").prop("selected", true);
              setTimeout(function () {
                $("#ddlSorting").trigger("change");
              }, 500);
            }
          }
        }, 50);

        //Handle external frame
        if (typeof (parent.hasSelector) == "function") {
          $("#template_result_list a").removeAttr("target");
        }

        //Fix weird error
        $("#ddls").appendTo("#searchBarBox");

        resetCountDown();
      });

      function hideFiltering() {
        hideFilterLinks();
        hideFilters();
      }

      function clearSearchBar() { $("#searchBar").val("").trigger("keyup"); return false; }

      function initSearchBar() {
        $("#searchBar").on("keyup change", function () {
          if ($(this).val().length > 0) {
            $("#searchBarX").fadeIn("fast");
          }
          else {
            $("#searchBarX").fadeOut("fast");
          }
        });
        $("#searchBar").on("keyup", function (event) {
          if (event.which) {
            //key press
            if (event.which == 16 || event.which == 9) { } //Ignore Shift or Tab
            else if (event.which == 13) {
              resetCountDown();
              currentPage = 1;
              event.stopPropagation();
            }
            else {
              resetCountDown();
            }
          }
          else {
            resetCountDown();
          }

        });
      }
    </script>
    <!-- Filtering -->
    <script type="text/javascript">
      //Show/hide filters
      function showFilter(target) {
        var jTarget = $(".filterBox[data-filtername=" + target + "]");

        if (jTarget.attr("data-showing") == "true") {
          hideFilters();
        }
        else {
          hideFilters();
          jTarget.fadeIn("fast");
          jTarget.attr("data-showing", "true");
        }
      }

      function hideFilters() {
        $(".filterBox").fadeOut("fast");
        $(".filterBox").attr("data-showing", "false");
      }

      //Reset filters (except english)
      function resetFilters() {
        $(".filterBox .list a").attr("data-selected", "false");
        updateNarrowing();
        $(".activeList .activeFilterItem a").trigger("click");
        $(".selectedStandard a").click();
      }
      function resetSearch() {
        $("#searchBar").val("");
        resetFilters();
      }

      //Toggle displaying the filters for mobile
      function showFilterLinks() {
        $("#filterLinks").show();
        //$("#activeFilters").show();
        $("#filters").show();
        $("#filters").attr("data-showing", "true");
      }
      function hideFilterLinks() {
        $("#filterLinks").hide();
        //$("#activeFilters").hide();
        $("#filters").hide();
        $("#filters").attr("data-showing", "false");
      } 
    </script>
    <!-- CountDown -->
    <script type="text/javascript">
      //Countdown
      function resetCountDown() { countDown = 8; } //800 milliseconds

      function tickCountDown() { 
            if (countDown > 0) {
          countDown--;
                if (countDown == 0) {
            doSearch();
          }
        }
      }

      function setupCountDown() {
        setInterval(tickCountDown, 100);
      }
    </script>
    <!-- Display -->
    <script type="text/javascript">
      //Sorting
      function sort(text) {
        sortParts = text.split("|");
        if (sortParts.length != 2) {
          sortMode.field = null;
          sortMode.order = null;
        }
        else {
          sortMode.field = sortParts[0];
          sortMode.order = sortParts[1];
        }
        resetCountDown();
      }

      //# of items to show
      function changePageSize(size) {
        currentPage = 1;
        pageSize = size;
        resetCountDown();
      }

      //Change the page number
      function gotoPage(pageNumber) {
        pageNumber = pageNumber
        resetCountDown();
      }

      function changeRenderMode(mode) {
        renderMode = mode;
        resetCountDown();
      }
    </script>
    <!-- Pagination -->
    <script type="text/javascript">
      function updatePaginator() {
        var total = resultHits;
        var totalPages = Math.ceil(total / pageSize);
        if (total == 0 || totalPages == 1) {
          $(".paginator").hide();
          return;
        }
        if (responsiveSize == "small" || responsiveSize == "tiny") {
          $("#bottomPaginator").show();
          $("#topPaginator").hide();
        }
        else {
          $(".paginator").show();
        }
        $(".paginator").html("Jump to Page: ");
        var skips = [1, 10, 50, 100, 500, 1000, 5000, 10000, totalPages];
        for (var i = 1; i <= totalPages; i++) {
          var printed = false;
          if (i == skips[j] || (i >= currentPage - 3 && i <= currentPage + 3)) {
            if (i == currentPage) {
              addPage(i, true);
            }
            else {
              addPage(i, false);
            }
            printed = true;
          }
          for (j in skips) {
            if (i == skips[j] && !printed) {
              addPage(i, false);
            }
          }
        }
      }

      function addPage(pageNumber, isCurrent) {
        $(".paginator").append("<a href=\"#\" onclick=\"changePage(" + pageNumber + ")\"" + (isCurrent ? " class=\"currentPage\"" : "") + ">" + pageNumber + "</a>");
      }
    </script>
    <!-- Searching -->
    <script type="text/javascript">
      function doSearch() {
        $(window).resize();
        var searching = getTargetFields($("#searchBar").val());
        var searchText = searching.searchText;
        var targetFields = searching.targetFields;
        searchText = processText(searchText);
        if (extraStandards.length > 0) {
          var standardsFound = false;
          for (i in activeFiltering.idFilters) {
            if (activeFiltering.idFilters[i].field == "standardIDs") {
              activeFiltering.idFilters[i].items = activeFiltering.idFilters[i].items.concat(extraStandards);
              standardsFound = true;
            }
          }
          if (!standardsFound) {
            standardsFilter = { field: "standardIDs", title: "Learning Standards", es: "standardIDs", items: extraStandards };
            activeFiltering.idFilters.push(standardsFilter);
          }
        }
        var queryInfo = { query: { searchText: searchText, narrowingOptions: activeFiltering, sort: sortMode, start: ((currentPage - 1) * pageSize), size: pageSize }, targetFields: targetFields };
        console.log(queryInfo);
        doAjax("ElasticSearchService.asmx", "DoSearch4", queryInfo, loadResults, true);
      }

      //Check for field targets
      function getTargetFields(text) {
        if (text.indexOf("infields[") == 0) {
          var parts = text.split("]:");
          var rest = "";
          for (var i = 1; i < parts.length; i++) {
            rest += parts[i];
          }
          return { targetFields: parts[0].replace("infields[", ""), searchText: rest };
        }
        else {
          return { targetFields: "", searchText: text };
        }

      }

      //Apply automatic text filtering
      function processText(text) {
        if(text == ""){ text = "* "; }
        var appliedTextFilters = "";
        try {
          //Apply auto filters if the terms aren't part of the query
          for (i in autoTextFilters) {
            for (j in autoTextFilters[i].terms) {
              if (text.indexOf(autoTextFilters[i].terms[j]) > -1) {
                autoTextFilters[i].applied = false;
              }
            }
            if (autoTextFilters[i].applied) {
              appliedTextFilters += " " + autoTextFilters[i].filter;
            }
          }
        }
        catch (e) { }
        return text + appliedTextFilters;
      }
      function processTextOLD(text) {
        var output = "";
        var appliedTextFilters = "";
        //Basic replacement
        var temp = text.trim().replace(/"/g, '\\"').replace(/\|/g, " OR ").split(" ");
        for (i in temp) {
          if (temp[i].indexOf("/") > -1) {
            temp[i] = '\\"' + temp[i] + '\\"';
          }
        }
        //Helps with partial matching
        if (temp.length == 1) { //ensure there is data
          console.log(temp[0].indexOf("."));
          if (temp[0].indexOf(".") == -1) { //If the user isn't looking for a standard, add an asterisk
            output = temp[0] + "*";
          }
          else if (temp[0].indexOf(".") > -1 && temp[0].indexOf('\\"') == -1) { //If the user is probably looking for a standard, add quotes if necessary
            output = '\\"' + temp[0] + '\\"';
          }
          else { //Otherwise just pass the text along
            output = temp[0]; 
          }
        }
        else {
          for (i in temp) {
            output += temp[i] + " ";
          }
        }
        try {
          //Apply auto filters if the terms aren't part of the query
                for (i in autoTextFilters) {
                    for (j in autoTextFilters[i].terms) {
                        if (output.indexOf(autoTextFilters[i].terms[j]) > -1) {
                autoTextFilters[i].applied = false;
              }
            }
                    if (autoTextFilters[i].applied) {
              appliedTextFilters += " " + autoTextFilters[i].filter;
            }
          }
        }
            catch (e) { }
        return output + appliedTextFilters;
      }

      function searchFor(text) {
        $("#searchBar").val('"' + text + '"')
        $("#searchBar").trigger("keyup");
      }
    </script>
    <!-- Checkboxes -->
    <script type="text/javascript">
      //Go get the data
      function setupCBXLs() {
        //doAjax2("ResourceService.asmx", "Fetch", { widgetName: "details", vid: 0 }, loadCBXLs, false);
        doAjax("ElasticSearchService.asmx", "FetchCodes", {}, loadCBXLs, false);
      }

      function loadCBXLs(input) {
        console.log(input);
        data = input.lists;
        //For each item in the list of desired items
            for (i in filterLists) {
          var filterName = filterLists[i].name;
          //Find the associated piece of returned data
          for (j in data) {
            var item = data[j];
            //And from it...
            if (item.name == filterName) {

              //Construct the list of items
              var list = "";
              for (k in item.items) {
                var cbx = item.items[k];
                list += $("#template_listItem").html()
                  .replace(/{name}/g, item.name)
                  .replace(/{id}/g, cbx.id)
                  .replace(/{text}/g, cbx.title)
              }

              //Create the filter box
              $("#filterContents").append(
                $("#template_filter").html()
                  .replace(/{name}/g, item.name)
                  .replace(/{title}/g, item.title)
                  .replace(/{list}/g, list)
                  .replace(/{es}/g, filterLists[i].es)
              );
            }
          }
        }

        //Handle clicking
        $("html").not("#filters, #btnShowFilterLinks").on("click", function () {
          ((responsiveSize == "tiny" || responsiveSize == "small") ? hideFiltering() : hideFilters());
        });
        $("#filters, #btnShowFilterLinks").on("click", function (event) { event.stopPropagation(); });
      }

      function toggleCBXLI(box) {
        if ($(box).attr("data-selected") == "false") {
          $(box).attr("data-selected", "true");
        }
        else {
          $(box).attr("data-selected", "false");
        }

        updateNarrowing();
        return false;
      }

      function updateNarrowing() {
        //Clear out the filters
        activeFiltering.idFilters = [];
        activeFiltering.textFilters = [];

        //Reset the page
        currentPage = 1;

        //Handle ID-based filtering
            $(".filterBox .list[data-type=id]").each(function () {
          var list = $(this);
          var temp = { field: list.parent().attr("data-filtername"), title: list.attr("data-title"), es: list.attr("data-es"), items: [] };
          list.find("a[data-selected=true]").each(function () {
            var listItem = $(this);
            temp.items.push({ text: listItem.attr("data-name"), id: parseInt(listItem.attr("data-id")) });
          });
          if (temp.items.length > 0) {
            activeFiltering.idFilters.push(temp);
          }
        });

        if (standardsFilter.items.length > 0) {
          activeFiltering.idFilters.push(standardsFilter);
        }

        //Handle text-based filtering
        $(".filterBox .list[data-type=text]").each(function () {
          var list = $(this);
          var temp = { field: list.attr("data-group"), title: list.attr("data-title"), es: "", items: [] };
          list.find("a[data-selected=true]").each(function () {
            temp.items.push({ text: $(this).attr("data-name"), id: 0 });
          });
          if (temp.items.length > 0) {
            activeFiltering.textFilters.push(temp);
          }
        });

        //Add Library/Collection Filters if available
        if ($("#libraryHeader").length > 0) {
          var active = getActive();
          var temp = { field: "libraryIDs", title: "Library", es: "libraryIDs", items: [] };
          temp.items.push({ text: libraryData.library.title, id: libraryData.library.id });
          activeFiltering.idFilters.push(temp);

          if (!active.isLibrary) {
            var temp2 = { field: "collectionIDs", title: "Collection", es: "collectionIDs", items: [] };
            temp2.items.push({ text: active.data.title, id: active.data.id });
            activeFiltering.idFilters.push(temp2);
          }
        }

        renderActiveFiltering();
        resetCountDown();
      }

      function renderActiveFiltering() {
        $("#activeFiltersList").html("");
        renderFilter(activeFiltering.idFilters);
        renderFilter(activeFiltering.textFilters);
      }

      function renderFilter(input) {
        for (i in input) {
          var group = input[i].field;
          var list = "";

          for (j in input[i].items) {
            var item = input[i].items[j];
            if (item.text != "") {
              list += $("#template_activeFilterListItem").html()
                .replace(/{group}/g, group)
                .replace(/{text}/g, item.text)
                .replace(/{id}/g, item.id)
            }
          }

          $("#activeFiltersList").append(
            $("#template_activeFilterList").html()
              .replace(/{group}/g, input[i].title)
              .replace(/{list}/g, list)
          );
        }
        if ($("#activeFiltersList").text() != "") {
          $("#btnActiveFiltersReset").show();
        }
        else {
          $("#btnActiveFiltersReset").hide();
        }

      }

      function removeFilter(box) {
        var item = $(box);
        $(".filterBox[data-filtername='" + item.attr("data-group") + "'] .list a[data-name='" + item.attr("data-name") + "'][data-id=" + item.attr("data-id") + "]").attr("data-selected", "false");
        if (item.attr("data-group") == "standardIDs") {
          //removeSelectedItem(item.attr("data-id"));
          //removeStandard(parseInt(item.attr("data-id")));
          $(window).trigger("SB7removeStandard", [parseInt(item.attr("data-id"))]);
        }
        updateNarrowing();
        return false;
      }
    </script>
    <!-- Rendering Results -->
    <script type="text/javascript">
      function loadResults(data) {
        var info = JSON.parse(data);
        debugResults = info;

        if (info.hits.hits.length > 0) {
          $("#ioerResults #resultCount").html("Found " + info.hits.total + " results.");
          var sendData = [];
                for (var i = 0; i < info.hits.hits.length; i++) {
            info.hits.hits[i]._source.simpleTitle = getFriendlyTitle(info.hits.hits[i]._source);
            sendData.push(info.hits.hits[i]._source);
          }
          $("#ioerResults .splash").hide();
          $("#ioerResults .resultList").html("");
          switch (renderMode) {
            case "list":
              renderListResults(sendData);
              break;
            case "grid":
              renderGridResults(sendData);
              break;
            case "compact":
              renderCompactResults(sendData);
              break;
            default:
              break;
          }
          $("#ioerResults .resultList").css({ "opacity": 0 }).animate({ "opacity": 1 }, 250);
          resultHits = info.hits.total;
          
        }
        else {
          $("#ioerResults #resultCount").html("Sorry, no results found. We recommend trying different filtering options or search terms.");
          $("#ioerResults .resultList").html("");
          resultHits = 0;
        }

        $(window).trigger("resultsLoaded", [info.hits.total]);

        updatePaginator(info.hits.total);
        //Temporary patch
        setTimeout(function () { $(window).resize(); }, 100);
        setTimeout(function () { $(window).resize(); }, 300);
        setTimeout(function () { $(window).resize(); }, 500);
        setTimeout(function () { $(window).resize(); }, 1000);
        setTimeout(function () { $(window).resize(); }, 2000);
      }

      function renderListResults(data) {
        for (var i = 0; i < data.length; i++) {
          $("#ioerResults .resultList").append(
            $("#template_result_list").html()
              .replace(/{vid}/g, data[i].versionID)
              .replace(/{intID}/g, data[i].intID)
              .replace(/{url}/g, data[i].url)
              .replace(/{thumbnail}/g, pickThumbnail(data[i]))
              .replace(/{simpleTitle}/g, data[i].simpleTitle)
              .replace(/{description}/g, data[i].description)
              .replace(/{metadata}/g, getMetadata(data[i]))
              .replace(/{title}/g, data[i].title)
              .replace(/{cbxls}/g, printMainMetadata(data[i]))
              .replace(/{paradata}/g, getParadataDisplay(data[i]))
              .replace(/{usageRightsIconURL}/g, (data[i].usageRightsMiniIconURL == "" ? "src=\"/images/icons/rightsreserved.png\"" : "src=\"" + data[i].usageRightsMiniIconURL + "\""))
              .replace(/{created}/g, data[i].created)
          );
        }
        initFadeCollapse();
        if (typeof (renderLibraryControls) == "function") { renderLibraryControls(); }
      }

      function renderGridResults(data) {
        for (var i = 0; i < data.length; i++) {
          $("#ioerResults .resultList").append(
            $("#template_result_grid").html()
              .replace(/{vid}/g, data[i].versionID)
              .replace(/{intID}/g, data[i].intID)
              .replace(/{url}/g, data[i].url)
              .replace(/{thumbnail}/g, pickThumbnail(data[i]))
              .replace(/{simpleTitle}/g, data[i].simpleTitle)
              .replace(/{title}/g, data[i].title)
              .replace(/{paradata}/g, getParadataDisplay(data[i]))
          );
        }
      }

      function renderCompactResults(data) {
        for (var i = 0; i < data.length; i++) {
          $("#ioerResults .resultList").append(
            $("#template_result_compact").html()
              .replace(/{vid}/g, data[i].versionID)
              .replace(/{intID}/g, data[i].intID)
              .replace(/{simpleTitle}/g, data[i].simpleTitle)
              .replace(/{description}/g, data[i].description)
              .replace(/{title}/g, data[i].title)
          );
        }
      }

      function getFriendlyTitle(item) {
        var legalChars = "abcdefghijklmnopqrstuvwxyz1234567890_";
        var modTitle = item.title.trim().substring(0, 100).replace(/ /g, "_").toLowerCase().split("");
        var output = "";
        for (i in modTitle) {
          if (legalChars.indexOf(modTitle[i]) > -1) {
            output += modTitle[i];
          }
        }
        return output;
      }

      function getMetadata(item) {
        var output = "";
        output += $("#template_metadataLink").html()
          .replace(/{itemRaw}/g, item.publisher)
          .replace(/{itemText}/g, item.publisher);
        output += "<br />";
        for (i in item.standardNotations) {
          output += $("#template_metadataLink").html().trim()
          .replace(/{itemRaw}/g, item.standardNotations[i])
          .replace(/{itemText}/g, item.standardNotations[i]);
          //output += (i == item.standardNotations.length - 1 ? "" : ", ");
        }
        return output;
      }

      function initFadeCollapse() {
        $(".fadeCollapse").each(function () {
          var box = $(this);
          var threshhold = 75;
          box.css({ "height": "auto" }).removeClass("fadeBox").attr("data-collapsed", "false").find("a.collapser").remove();
          if (box.height() > threshhold) {
            box.css("height", threshhold + "px").addClass("fadeBox").attr("data-collapsed", "true");
            var collapser = box.append("<a href=\"#\" class=\"collapser\">Show/Hide &gt;&gt;</a>").find("a");
            collapser.unbind();
            collapser.on("click", function () {
              if (box.height() > threshhold) {
                box.animate({ "height": threshhold + "px" }, 250);
                box.attr("data-collapsed", "true");
              }
              else {
                box.css({ "height": "auto" });
                var tempHeight = box.height() + 50;
                box.css({ "height": threshhold + "px" });
                box.animate({ "height": tempHeight + "px" }, 250);
                setTimeout(function () { box.css({ "height": "auto" }) }, 300);
                box.attr("data-collapsed", "false");
              }
              return false;
            });
          }
          else {
            box.find("a").remove();
          }
        });
      }

      function printMainMetadata(data) {
        var output = "";
        var limit = 8;
        var count = 0;
        for (i in filterLists) {
          if (count < limit) {
            var item = filterLists[i].esName;
            if (data[item]) {
              if (data[item].length > 0 && typeof (data[item]) == "object") {
                if (data[item].length > 0) {
                  output += "<div class=\"metadataList\">";
                  for (j in data[item]) {
                    var current = data[item][j];
                    if (current.toLowerCase() != "other") {
                      output += $("#template_metadataLink").html()
                    .replace(/{itemRaw}/g, current)
                    .replace(/{itemText}/g, current.replace(/\//g, " / ") + (j == data[item].length - 1 ? "" : ", "));
                      count++;
                    }
                  }
                  output += "</div>";
                }
              }
            }
          }
        }
        return output;
      }

      function pickThumbnail(data) {
        for (i in thumbDivTypes) {
          if (data.url.indexOf(thumbDivTypes[i].match) > 15) {
            return $("#template_thumbdiv").html()
              .replace(/{header}/g, thumbDivTypes[i].header)
              .replace(/{text}/g, thumbDivTypes[i].text)
          }
        }
        for (i in thumbIconTypes) {
            if (data.url.indexOf(thumbIconTypes[i].match) > 15) {
                return $("#template_FileThumbnail").html()
                  .replace(/{header}/g, thumbIconTypes[i].header)
                  .replace(/{file}/g, thumbIconTypes[i].file)
            }
        }

        //
        return $("#template_thumbnail").html()
          .replace(/{intID}/g, data.intID)
          .replace(/{url}/g, data.url)
      }

      function fixThumbnail(intID, url, image) {
        $(image).replaceWith(
          $("#template_thumbdiv").html()
            .replace(/{header}/g, "Error")
            .replace(/{text}/g, "Preview Unavailable")
        );
      }

      function changePage(target) {
        currentPage = target;
        resetCountDown();
      }
    </script>
    <!-- Standards Browser interaction -->
    <script type="text/javascript">
      function external_applyStandardsV4(items) {
        updateAppliedStandards(items);
      }
      function external_removeStandardsV4(items) {
        updateAppliedStandards(items);
      }
      function external_applyStandardsV5(items, extraChildren) {
        extraStandards = extraChildren;
        updateAppliedStandards(items);
      }
      function external_applyStandardsV7(items, extraChildren) {
        extraStandards = [];
        for (i in extraChildren) {
          extraStandards.push({ text: "", id: extraChildren[i] });
        }
        for (i in items) {
          items[i].code = (items[i].code == null || items[i].code == undefined || items[i].code == "" || items[i].code == "null") ? items[i].description : items[i].code;
        }
        updateAppliedStandards(items);
      }
        function updateAppliedStandards(items) {
        console.log(items);
        standardsFilter = { field: "standardIDs", title: "Learning Standards", es: "standardIDs", items: [] };
        for (i in items) {
          standardsFilter.items.push({ text: items[i].code, id: items[i].id });
        }
        updateNarrowing();
        resetCountDown();
      }

    </script>
    <script type="text/javascript">
      //Standards Browser 7 Functionality
      var SB7mode = "search";
      $(window).on("SB7search", function (event, selected, all) {
        external_applyStandardsV7(selected, all);
      });
      $(window).on("SB7showResults", function () {
        hideFilters();
      });
    </script>
    <script type="text/javascript">
      //Experimental
      function preview(id){
        for(i in debugResults.hits.hits){
          var item = debugResults.hits.hits[i];
          if(item._source.intID == id){
            var src = item._source;
            console.log(src);
            rp.open({
              intID: src.intID,
              title: src.title,
              publisher: src.publisher,
              created: src.created,
              url: src.url
            });
            break;
          }
        }
      }
    </script>

    <div id="container">
      <h1 class="isleH1" id="pageTitle">ISLE Open Education Resources Search</h1>
      <!-- /leftColumn -->
      <div id="search">
        <div id="searchBarBox" data-responsivemode="normal">
          <input type="text" id="searchBar" title="Search Keywords" placeholder="Start typing to Search..." />
          <a href="#" onclick="clearSearchBar(); return false;" id="searchBarX">X</a>
          <div id="activeFilters">
            <div id="activeFiltersList"></div>
            <input type="button" id="btnActiveFiltersReset" onclick="resetFilters()" style="display:none;" value="Clear Filters" />
            <!--<input type="button" id="btnResetSearch" onclick="resetSearch()" value="Reset Search" />-->
          </div>
          <div id="options">
            <a href="#" id="btnShowFilterLinks">Filters...</a>
            <!-- Filters -->
            <div id="filters" data-showing="true">
              <h2>Filters...</h2>
              <div id="filterLinks">
                <!--<a href="#" data-name="standardsBrowser">Standards Browser</a>-->
                <a href="#" data-name="standardsBrowser">Standards Browser</a>
                <a href="#" data-name="gradeLevel">Grade Level</a>
                <a href="#" data-name="subject">K-12 Subjects</a>
                <a href="#" data-name="accessRights">Access Rights</a>
                <a href="#" data-name="careerCluster">Career Cluster</a>
                <a href="#" data-name="educationalUse">Educational Use</a>
                <a href="#" data-name="endUser">End User</a>
                <a href="#" data-name="groupType">Group Type</a>
                <a href="#" data-name="language">Language</a>
                <a href="#" data-name="mediaType">Media Type</a>
                <a href="#" data-name="resourceType">Resource Type</a>
                <a href="#" data-name="searchTips">Search Tips</a>
              </div><!-- /filterLinks -->
              <div id="filterContents">
                <div class="filterBox" data-filtername="searchTips" data-maxwidth="true">
                  <a href="#" class="closeBox">X</a>
                  <h2>Search Tips</h2>
                  <p>You can wrap phrases in double quotes ( " ) to search for whole phrases:</p>
                  <ul>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">"3rd grade" literacy</a> <span>(Literacy Resources for Third-Graders)</span></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">"pythagorean theorem"</a></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">"dinosaur footprints"</a></li>
                  </ul>
                  <p>You can use + or - to require/exclude words or phrases:</p>
                  <ul>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">+trees -forest</a> <span>(Finds Resources about trees, avoiding Resources about forests)</span></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">formula -chemistry</a> <span>(Finds Non-Chemistry Resources about Formulas)</span></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">formula +chemistry</a> <span>(Finds Only Chemistry Resources about Formulas)</span></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">geometry +"isosceles triangle"</a> <span>(Finds Geometry Resources focusing on Isosceles Triangles)</span></li>
                  </ul>
                  <p>You can use | to look for one term/phrase or another:</p>
                  <ul>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">math|english</a></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">trees forest|math</a></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">"civil rights"|"civil engineering"</a> <span>(Finds Civil Rights or Civil Engineering Resources)</span></li>
                  </ul>
                  <p>Put them together. Go nuts!</p>
                  <ul>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">"grade 3" geometry -triangles</a> <span>(Finds Grade 3 Geometry Resources about things other than Triangles)</span></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">"charles dickens" +"christmas carol" film|video</a> <span>(Finds film or video Resources about Charles Dickens' <u>A Christmas Carol</u>)</span></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">history|biography +war -ww2 -"world war 2" -"world war II"</a> <span>(Finds History or Biography Resources about war, but not about World War 2)</span></li>
                    <li><a href="#" onclick="searchFor(this.innerHTML)">+free math "grade 3"|"third grade"|"3rd grade" game|activity -test -quiz application/software student</a> <span>(Finds Free Math Resources for Third Graders that are Games or Activities but not Tests or Quizzes, and is software meant for students.)</span></li>
                  </ul>
                </div>
                <div class="filterBox" data-filterName="standardsBrowser" data-maxwidth="true">
                  <a href="#" class="closeBox">X</a>
                  <h2>Standards Browser</h2>
                  <uc1:StandardsBrowser7 ID="sBrowser7" runat="server" mode="search" />
                </div>
                <div class="filterBox" data-filtername="subject">
                  <a href="#" class="closeBox">X</a>
                  <h2>K-12 Subject</h2>
                  <div class="list" data-type="text" data-group="subject" data-title="K-12 Subject">
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="Mathematics">Mathematics</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="English Language Arts">English Language Arts</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="Science">Science</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="Social Studies">Social Studies</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="Arts">Arts</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="World Languages">World Languages</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="Health">Health</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="Physical Education">Physical Education</a>
                    <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="0" data-name="Technology">Technology</a>
                  </div>
                </div>
              </div><!-- /filter Contents -->
            </div><!-- /filters -->
            <!--<div id="display">
              <h2>Display</h2>
              <select id="ddlSorting2">
                <option data-val="relevancy" value="|">Relevancy</option>
                <option data-val="newest" value="timestamp|desc" selected="selected">Newest First</option>
                <option data-val="oldest" value="timestamp|asc">Oldest First</option>
                <option data-val="liked" value="likeCount|desc">Most Liked</option>
                <option data-val="disliked" value="dislikeCount|desc">Most Disliked</option>
                <option data-val="rated" value="evaluationScoreTotal|desc">Best Rated</option>
                <option data-val="viewed" value="viewsCount|desc">Most Visited</option>
                <option data-val="favorites" value="favorites|desc">Most Favorited</option>
                <option data-val="comments" value="commentsCount|desc">Most Commented On</option>
              </select>
              <select id="ddlDisplayCount2">
                <option selected="selected" value="20">Show 20 Items</option>
                <option value="50">Show 50 Items</option>
                <option value="100">Show 100 Items</option>
              </select>
              <select id="ddlRendering2">
                <option value="list" selected="selected">List View</option>
                <option value="grid">Grid View</option>
                <option value="compact">Text-only View</option>
              </select>
            </div>--><!-- /display -->
          </div>          
          <div id="ddls" style="">
              <%--<label for="ddlSorting" class="offScreenX">Sort Order</label>--%>
            <select id="ddlSorting" title="Sort Order">
              <option data-val="relevancy" value="|">Relevancy</option>
              <option data-val="newest" value="intID|desc" selected="selected">Newest First</option>
              <option data-val="oldest" value="intID|asc">Oldest First</option>
              <option data-val="liked" value="likeCount|desc">Most Liked</option>
              <option data-val="disliked" value="dislikeCount|desc">Most Disliked</option>
              <option data-val="rated" value="evaluationScoreTotal|desc">Best Rated</option>
              <option data-val="viewed" value="viewsCount|desc">Most Visited</option>
              <option data-val="favorites" value="favorites|desc">Most Favorited</option>
              <option data-val="comments" value="commentsCount|desc">Most Commented On</option>
            </select>
              <%--<label for="ddlDisplayCount" class="offScreen">Number of Results to Show</label>--%>
            <select id="ddlDisplayCount" title="Number of Results to Show">
              <option selected="selected" value="20">Show 20 Items</option>
              <option value="50">Show 50 Items</option>
              <option value="100">Show 100 Items</option>
            </select>
              <%--<label for="ddlRendering" class="offScreen">Display Format</label>--%>
            <select id="ddlRendering" title="Display Format">
              <option value="list" selected="selected">List View</option>
              <option value="grid">Grid View</option>
              <option value="compact">Text-only View</option>
            </select>
          </div><!-- /ddls -->

        </div>
        <div id="ioerResults">
          <div id="resultCount"></div>
          <div class="paginator" id="topPaginator"></div>
          <div class="resultList"></div>
          <div class="paginator" id="bottomPaginator"></div>
          <div class="splash">
            <h2>Welcome to the ISLE Open Education Resources Search!</h2>
            <p>This search taps into the <a href="http://learningregistry.org/" target="_blank">Learning Registry</a> and the ISLE Community to provide Learning Resources for teachers and students alike.</p>
            <p>Please take advantage of both the free-text search and the filtering options to quickly find what you're looking for.</p>
          </div>
        </div>
      </div><!-- /search -->
      

    </div><!-- /container -->

    <div id="templates" style="display:none;">

      <script type="text/template" id="template_filter">
        <div class="filterBox" data-filtername="{name}" data-showing="false">
          <a href="#" class="closeBox">X</a>
          <h2>{title}</h2>
          <div class="list" data-type="id" data-title="{title}" data-es="{es}">{list}</div>
        </div>
      </script>

      <script type="text/template" id="template_listItem">
        <a href="#" data-selected="false" onclick="toggleCBXLI(this);return false;" data-id="{id}" data-name="{text}">{text}</a>
      </script>

      <script type="text/template" id="template_result_list">
        <div class="result result_list" data-vid="{vid}" data-intid="{intID}">
          <a class="thumbnailLink" href="/Resource/{intID}/{simpleTitle}" target="_self">
            {thumbnail}
            {paradata}
          </a>
          <div class="mainMetadata">
            <img {usageRightsIconURL} />
            <h3>Keywords:</h3>
            {cbxls}
          </div>
          <div class="data">
            <h2><a href="/Resource/{intID}/{simpleTitle}" target="_self">{title}</a></h2>
            <%--<input type="button" class="previewerButton" onclick="preview({intID})" value="Preview" />--%>
            <div class="libraryControls"></div>
            <p class="fadeCollapse">
              {description}
            </p>
            <div class="miniMetadata"><div class="created">Created {created}</div>{metadata}</div>
          </div>
        </div>
      </script>

      <script type="text/template" id="template_result_grid">
        <div class="result result_grid" data-vid="{vid}" data-intid="{intID}">
          <a class="thumbnailLink" href="/Resource/{intID}/{simpleTitle}" target="_self">
            {thumbnail}
            <h2>{title}</h2>
            {paradata}
          </a>
        </div>
      </script>

      <script type="text/template" id="template_result_compact">
        <div class="result result_compact">
          <h2><a href="/Resource/{intID}/{simpleTitle}" target="_self">{title}</a></h2>
          <p class="fadeCollapse">
            {description}
          </p>
        </div>
      </script>

      <script type="text/template" id="template_thumbnail">
        <img id="imgThumbnail" runat="server" class="thumbnail" src="//ioer.ilsharedlearning.org/OERThumbs/large/{intID}-large.png" onerror="fixThumbnail({intID}, '{url}', this)" />
      </script>
    
      <script type="text/template" id="template_FileThumbnail">
        <img class="file thumbnail" src="/images/icons/{file}" alt="{header}" />
      </script>

      <script type="text/template" id="template_thumbdiv">
        <div class="thumbnail">
          <h3>{header}</h3>
          <p>{text}</p>
        </div>
      </script>

      <script type="text/template" id="template_activeFilterList">
        <!--<h3>{group}</h3>-->
        <div class="activeList">{list}</div>
      </script>

      <script type="text/template" id="template_activeFilterListItem">
        <div class="activeFilterItem"><a href="#" onclick="removeFilter(this);return false;" data-group="{group}" data-name="{text}" data-id="{id}">X</a> {text}</div>
      </script>

      <script type="text/template" id="template_metadataLink">
        <a href="#" onclick="searchFor('{itemRaw}')" class="metadataLink">{itemText}</a>
      </script>

    </div>