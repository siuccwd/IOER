<%@ Page Title="IOER - Gooru Search" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="GooruSearch.aspx.cs" Inherits="IOER.Pages.GooruSearch" %>
<%@ Register TagPrefix="UC1" TagName="StandardsBrowser" Src="/Controls/StandardsBrowser7.ascx"%>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%--<script src="//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js"></script>--%>
    
    <link href="/Styles/common2.css" rel="stylesheet" />
<style>
    /*padding-left: 50px;*/
   #content { min-height: 500px; transition: padding 1s; }

    @media (max-width:500px) {

        .isleH1 {
            font-size: 20pt; 
        }

        img {
            float:right;
            width:85px; 
        }

        #options {
            width:100%; 
            display:inline-block; 
            margin: 1px 0; 
        }

        .filterBox {
            position: absolute;
            top: 0;
            background-color: #EEE;
            padding: 5px;
            left: 60px; 
        }

        #welcomeTXT {
            text-align:center;
            padding-top:75px; 
            width:90%;
            margin:auto; 
        }
    }

    @media (min-width:501px) {

        .isleH1 {
            font-size:228%; 
        }

        img {
            width: 170px;
            float: right;
        }

        #options {
            width:24%; 
            display:inline-block; 
            margin: 1px 0; 
        }

        .filterBox {
            position: absolute;
            top: 0;
            background-color: #EEE;
            padding: 5px;
            width: 310px;
            left: 160px;
        }

        #welcomeTXT {
            text-align:center;
            padding-top:75px; 
            width:500px;
            margin:auto; 
        }
    }
  
    h2 {
        font-size:16pt; 
        padding-right:15px; 
        display:inline; 
    }

    h4 {font-size:16pt}

    @media (max-width:850px) {
        #textBox {
            width:100%;
            font-size: 1.5em;
        }

        #searchButton {
            height: 33px;
            width: 100px;
            margin-right: 8px;
            border-radius: 5px;
            border: 1px solid #FFF;
        }
    }

    @media (min-width:851px) {
        #textBox {
            width: calc(100% - 120px);
            display: inline-block;
            margin-right: 5px;
            font-size: 1.5em;
        }

        #searchButton {
            height: 33px;
            width: 100px;
            float: right;
            margin-right: 8px;
            border-radius: 5px;
            border: 1px solid #FFF;
        }
    }

    #searchBarX {
        position: absolute;
        background-color: #D55;
        color: #FFF;
        font-weight: bold;
        text-align: center;
        padding: 2px 10px;
        font-size: 1.5em;
        border-top-right-radius: 5px;
        border-bottom-right-radius: 5px;
    }

    .pageButtons{ 
        text-align:center; 
        background-color: #EEE;  
        border-radius:5px;
        padding:5px 0px;  
        margin:5px 0px;  
        display:none; 
    }

    .pageButtons a {
        border-radius:5px;
        display:inline-block;
        line-height:30px;
        min-width:30px; 
        height:30px; 
        background-color:#3572B8; 
        color: #FFF; 
        padding:0 5px; 
        margin:2px; 
    }

    .pageButtons a.currentPage{background-color:#9984BD}

    #resultData {  
        padding:5px 5px 35px 5px;   
        box-shadow:0 10px 80px -30px #CCC inset; 
        border-radius:5px; 
        clear:both;   
    }

    #loader {
        display:none; 
        position:fixed;   
        z-index:1000; 
        height:100%; 
        width:100%; 
        top:0; 
        left:0; 
        background: rgba(255, 255, 255, .8) url('/images/icons/progress.gif') 50% 50% no-repeat; 
    }

    #gooruResults {transition: padding 0.2s}

    #activeFilterList {display:inline}

    #activeFilterItem {
        display:inline-block;
        margin: 1px 5px 0px 0px;
        border: 1px solid #D55;
        min-height:28px;
        padding:2px;
        overflow:hidden;
        border-radius:5px;
        vertical-align:top;
    }

    #filters {
        position: absolute; 
        z-index: 1000; 
        width: 160px; 
        background-color: #EEE; 
        padding:5px; 
    }

    #filters > h4, .filterBox > h4 {
        background-color: #4AA394; 
        color: #FFF; 
        margin: -5px -5px 10px -5px; 
        padding: 2px 5px;
        font-size: 1.2em; 
    }

   #filterLinks, .filterBox .list { 
       border-radius: 5px; 
       overflow: hidden; 
   }

   #filterLinks a, .filterBox .list a {
        margin-bottom: 1px; 
        background-color: #3572B8; 
        color: #FFF; 
        display:block; 
        font-size: 1.1em; 
   }

    .filterBox .list a {
        padding:5px;
        border-left:15px solid #3572B8; 
    }

    .filterBox .list a[data-selected=true]{
        background-color:#9984BD;
        border-left:15px solid #FF5707; 
    }

    #filterLinks > a{
        background-color: #3572B8;
        color: #FFF; 
        text-align:right; 
        display:block; 
        font-size: 1.1em;
        padding:5px;  
    }

    #filterLinks a:hover, #btnShowFilterLinks:hover, .filterBox .list a:hover, .pageButtons a:hover, #btnResetFilters:hover {
        background-color:#FF5707; 
        cursor:pointer;
    }

    .filterBox .closeBox, .closeBox{
        float:right;
        margin: 0 0 2px 2px; 
        display:inline-block; 
        width:20px; 
        height:20px; 
        line-height:20px;
        font-weight:bold; 
        background-color:#D55;
        color:#FFF;
        border-radius:5px; 
        box-shadow: 0 5px 10px -5px #FFF inset; 
        text-align:center; 
    }

    .filterBox .closeBox:hover, .closeBox:hover, #searchBarX:hover{
        background-color:#F00; 
        color:#FFF;
    }

    #btnShowFilterLinks {
        background-color: #3572B8; 
        color: #FFF; 
        border-radius:5px; 
        font-size:20px; 
        font-weight:bold; 
        margin:2px 0; 
        width:100%; 
        height:30px; 
        line-height: 30px; 
        text-align:center; 
        display:inline-block; 
    }

    #resetFilters {display:inline}

    #btnResetFilters {
        background-color:#3572B8;
        color:#FFF;
        border-radius:5px;
        box-shadow:0 5px 25px -5px #FFF inset;
        padding:5px;
        font-weight:bold;
        border:1px solid #FFF;
        height:30px;
        display:inline-block;
    }
  @media screen and (max-width: 975px) {
    #content { padding-left: 0; }
  }
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

      <div id="content">  
          <h1 class="isleH1"">IOER gooru Search (beta)</h1>


      <div id="searchBarBox">

          <input type="text" id="textBox" placeholder="Start typing to Search..."/><input type="button" value="Search" id="searchButton" class="isleButton bgGreen" onclick="newSearch()"/>
          <a href="#" onclick="clearSearchBar(); return false;" id="searchBarX" style="display: none;">X</a>
          <div id="activeFilters">
              <div id="activeFilterList">
                  
              </div>
              <div id="resetFilters"><a href="#" id="btnResetFilters"  style="display:none" onclick="resetFilters()">Clear Filters</a></div>
          </div>

            <div id="options">
                <a href="#" id="btnShowFilterLinks" onclick="showFilterLinks()">Filters...</a>
                <div id="filters" data-showing="true" style="display:none">
                    <h4>Filters...</h4>
                    <div id="filterLinks" style="display:block">
                        <a href="#" data-name="searchType"onclick="showSearchTypeFilter()" >Search Type</a>
                        <a href="#" data-name="standardsBrowser" id="standardsFilter" onclick="showStandardBrowser()" >Standards Browser</a>
                        <a href="#" data-name="gradeLevel" id="gradeBTN" onclick="showGradeFilter()">Grade Level</a>
                        <a href="#" data-name="subject" id="subjectBTN" onclick="showSubjectFilter()">K-12 Subjects</a>
                        <a href="#" data-name="mediaType" id="mediaBTN" onclick="showMediaFilter()">Media Type</a>
                    </div>

                    <div id="filterContents">
                        <div class="filterBox" id="searchTypeFilter" data-filtername="searchType" data-show="false" style="display:none">
                            <a href ="#" class="closeBox" onclick="hideFilterBox()">X</a>
                                <h4>Search Type</h4>
                            <div class="list" data-title="Search Type" data-es="searchTypeIDs">
                                <input type="radio" id="resourceSearch" name="type" value="resource"  checked="checked"/> Resource
                                <input type="radio" id="collectionSearch" name="type" value="collection"/> Collection
                            </div>
                        </div>
                        <div class="filterBox" id="standardBrowser" data-filtername="standardsBrowser" data-maxwidth="true" data-showing="false" style="display:none">
                            <a href ="#" class="closeBox" onclick="hideFilterBox()">X</a>
                            <h4>Standards Browser</h4>
                            <script>var SB7mode = "search";</script>
                            <UC1:StandardsBrowser ID="browserControl" runat="server" />
                        </div>

                        <div class="filterBox" id="gradeFilter" data-filtername="gradeLevel" data-showing="false" style="display:none">
                           <a href ="#" class="closeBox" onclick="hideFilterBox()">X</a>
                             <h4>Grade Level</h4>
                            <div class="list" data-gooruID="grade" data-type="id" data-title="Grade Level" data-es="gradeLevelIDs">
                                <a href="#" data-selected="false" data-display="Kindergarten" data-name="Kindergarten,K-4">Kindergarten</a>
                                <a href="#" data-selected="false" data-display="Grade 1" data-name="1">Grade 1</a>
                                <a href="#" data-selected="false" data-display="Grade 2" data-name="2">Grade 2</a>
                                <a href="#" data-selected="false" data-display="Grade 3" data-name="3">Grade 3</a>
                                <a href="#" data-selected="false" data-display="Grade 4" data-name="4">Grade 4</a>
                                <a href="#" data-selected="false" data-display="Grade 5" data-name="5">Grade 5</a>
                                <a href="#" data-selected="false" data-display="Grade 6" data-name="6">Grade 6</a>
                                <a href="#" data-selected="false" data-display="Grade 7" data-name="7">Grade 7</a>
                                <a href="#" data-selected="false" data-display="Grade 8" data-name="8">Grade 8</a>
                                <a href="#" data-selected="false" data-display="Grades 9-10" data-name="9-10">Grades 9-10</a>
                                <a href="#" data-selected="false" data-display="Grades 11-12" data-name="11-12">Grades 11-12</a>
                                <a href="#" data-selected="false" data-display="Higher Education" data-name="H">Higher Education</a>
                            </div>
                        </div>

                        <div class="filterBox" id="subjectFilter" data-filtername="subject" data-showing="false" style="display:none">
                           <a href ="#" class="closeBox" onclick="hideFilterBox()">X</a>
                             <h4>K-12 Subjects</h4>
                            <div class="list" data-gooruID="subject" data-type="id" data-group="subject" data-title="K-12 Subjects">
                                <a href="#" data-selected="false" data-display="Mathematics" data-name="Math">Mathematics</a>
                                <a href="#" data-selected="false" data-display="English Language Arts" data-name="Language Arts">English Language Arts</a>
                                <a href="#" data-selected="false" data-display="Science" data-name="Science">Science</a>
                                <a href="#" data-selected="false" data-display="Social Sciences" data-name="Social Sciences">Social Sciences</a>
                                <a href="#" data-selected="false" data-display="Arts & Humanities" data-name="Arts & Humanities">Arts & Humanities</a>
                                <a href="#" data-selected="false" data-display="Technology & Engineering" data-name="Technology & Engineering">Technology & Engineering</a>
                            </div>
                        </div>

                        <div class="filterBox" id="mediaFilter" data-filtername="mediaType" data-showing="false" style="display:none">
                            <a href ="#" class="closeBox" onclick="hideFilterBox()">X</a>
                            <h4>Media Type</h4>
                            <div class="list" data-gooruID="media" data-type="id" data-title="Media Type" data-es="mediaTypeIDs">
                                <a href="#" data-selected="false" data-display="Audio" data-name="Audio">Audio</a>
                                <a href="#" data-selected="false" data-display="Exam" data-name="Exam">Exam</a>
                                <a href="#" data-selected="false" data-display="Handout" data-name="Handout">Handout</a>
                                <a href="#" data-selected="false" data-display="Image" data-name="Image">Image</a>
                                <a href="#" data-selected="false" data-display="Interactive" data-name="Interactive">Interactive</a>
                                <a href="#" data-selected="false" data-display="Question" data-name="Question">Question</a>
                                <a href="#" data-selected="false" data-display="Slide" data-name="Slide">Slide</a>
                                <a href="#" data-selected="false" data-display="Textbook" data-name="Text">Textbook</a>
                                <a href="#" data-selected="false" data-display="Video" data-name="Video">Video</a>
                                <a href="#" data-selected="false" data-display="Website" data-name="Website">Website</a>
                            </div>
                        </div>
                    </div>
                 </div>
              </div>

          <div id="resultCount" style="text-align:center"></div>
          </div>

       <div id="gooruResults">
            <div class="pageButtons" id="topPageButtons">

            </div>

         <div id="resultList">
             <div id="welcomeTXT">
                <b style="font-size:14pt">Welcome to ISLE Open Educational Resources gooru Search!</b><br>
                This search taps into gooru and the ISLE Community to provide Learning Resources for teachers and students alike.<br>  
                Please take advantage of both the free-text search and the filtering options (use the blue “Filters…” button) to quickly find what you’re looking for, <br />
                in terms of Resource, Collection, Grade Level, K-12 Subjects, Media Type and Keyword filtering options.
             </div>

           <div id="results">


           </div>
        </div>
       </div>
           <div class="pageButtons" id="bottomPageButtons">

           </div>

        <div id="loader"></div>
  </div>
<script type="text/javascript">
    //From server
    <%=gooruResourcesUrl %>
    <%=gooruCollectionsUrl %>
    
    var currentPage = 1;
    var filtersSelected = 0;
    var position = $("#searchBarBox").offset();
    var countDown; 

    $(document).ready(function () {

        setupCountDown(); 
        if ($(window).width() <= 500) {
            $(".filterBox").width($("#searchBarBox").outerWidth() - 70);
        } else {
            $("#standardBrowser").width($("#searchBarBox").outerWidth() - 187);
        }

        if ($(window).width() > 975) {
            $("#searchBarX").css("top", position.top);
            $("#searchBarX").css("right", position.left + 71);
        } else if ($(window).width() >= 850 && $(window).width() <= 975) {
            $("#searchBarX").css("top", position.top);
            $("#searchBarX").css("right", position.left + 121);
        } else {
            $("#searchBarX").css("top", position.top);
            $("#searchBarX").css("right", position.left);
        }

        jQuery.support.cors = true;

        $(window).on("SB7search", function (s1, s2, s3, standards) { 
            var  chosenStandards = "";
            for (var i = 0; i < standards.length; i++) {

                var item = standards[i]; 
                if (item.children.length == 0) {
                    chosenStandards += "," + item.fullCode; 
                }
            }
            if (chosenStandards.length > 0) {
                chosenStandards = chosenStandards.replace(",", ""); 
            } 
            getValues(chosenStandards);  
        });
        hideILStandards();
    });

    $(window).resize(function () {

        position = $("#searchBarBox").offset();
        if ($(window).width() <= 500) {
            $(".filterBox").css("left", "60px");
            $(".filterBox").width($("#searchBarBox").outerWidth() - 70);
            $("#welcomeTXT").width($("#searchBarBox").outerWidth());
        } else {
            $(".filterBox").css("left", "160px");
            $(".filterBox").width(300);
            $("#standardBrowser").width($("#searchBarBox").outerWidth() - 170);
        }

        if ($(window).width() > 975) {
            $("#searchBarX").css("top", position.top);
            $("#searchBarX").css("right", position.left + 71);
        } else if ($(window).width() >= 850 && $(window).width() <= 975) {
            $("#searchBarX").css("top", position.top);
            $("#searchBarX").css("right", position.left + 121);
        } else{
            $("#searchBarX").css("top", position.top);
            $("#searchBarX").css("right", position.left);
        }
    });

    function hideFilterBox() {
        $(".filterBox").hide(); 
    }

    function showFilterLinks() {

        if ($("#filters").is(':visible')) {
            $("#filters").hide();
            $("#gooruResults").css("padding-left", "0px");
        } else {
            $("#filters").show();
            if ($(window).width() > 800) {
                $("#gooruResults").css("padding-left", "160px");
            }
        }

        if ($("#collectionSearch").is(':checked')) {
            $("#mediaBTN").hide();
        } else {
            $("#mediaBTN").show();
        }

        $("#standardBrowser").hide();
        $("#searchTypeFilter").hide();
        $("#gradeFilter").hide();
        $("#subjectFilter").hide();
        $("#mediaFilter").hide()
    }

    function showSearchTypeFilter() {

        $("#searchTypeFilter").fadeToggle("fast");
        $("#standardBrowser").hide();
        $("#gradeFilter").hide();
        $("#subjectFilter").hide();
        $("#mediaFilter").hide();
    }

    function showStandardBrowser() {

        $("#standardBrowser").fadeToggle("fast");
        $("#searchTypeFilter").hide();
        $("#gradeFilter").hide();
        $("#subjectFilter").hide();
        $("#mediaFilter").hide();
    }

    function showGradeFilter() {

        $("#gradeFilter").fadeToggle("fast");
        $("#standardBrowser").hide();
        $("#searchTypeFilter").hide();
        $("#subjectFilter").hide();
        $("#mediaFilter").hide();
    }

    function showSubjectFilter() {

        $("#subjectFilter").fadeToggle("fast");
        $("#gradeFilter").hide();
        $("#standardBrowser").hide();
        $("#searchTypeFilter").hide();
        $("#mediaFilter").hide();
    }

    function showMediaFilter() {

        $("#mediaFilter").fadeToggle("fast");
        $("#subjectFilter").hide();
        $("#gradeFilter").hide();
        $("#standardBrowser").hide();
        $("#searchTypeFilter").hide();
    }

    $(".filterBox .list a").on("click", function () {

        if ($(this).attr("data-selected") == "true") {

            $(this).attr("data-selected", "false");
            removeFilter($(this).attr("data-display")); 
 
        } else {
            $(this).attr("data-selected", "true");

            $("#activeFilterList").append("<div id='activeFilterItem' class='filterItems' data-name='" + $(this).attr("data-display")
                + "'>" + $(this).attr("data-display") + "<a href ='#' class='closeBox' data-subjectID='" + $(this).attr("data-display")
                + "' onclick='removeFilter($(this).attr(\"data-subjectID\"))'>X</a></div>");

            filtersSelected++;
            $("#btnResetFilters").show();
        }
        resetCountDown();
    });

    function removeFilter(filter) {
            
        $(".filterItems").each(function () {

            if ($(this).attr("data-name") == filter) {
                $(this).remove();
                filtersSelected--;
            } 
        });

        $(".filterBox .list a").each(function () {

            if ($(this).attr("data-display") == filter) {
                $(this).attr("data-selected", "false");
            }
        });

        if (filtersSelected == 0) {
            $("#btnResetFilters").hide();
        }
        resetCountDown();
    }

    function resetFilters() {

        $(".filterBox .list a").attr("data-selected", "false");
        filtersSelected = 0;
        $("#btnResetFilters").hide();
        $(".filterBox").hide();
        $("#activeFilterList").empty();
        resetCountDown(); 
    }

    $("#textBox").on("keyup", function () {

        if ($(this).val() != "") {
            $("#searchBarX").fadeIn("fast");
        } else {
            $("#searchBarX").fadeOut("fast");
        }
    }); 

    function clearSearchBar() {

        $("#textBox").val("");
        $("#searchBarX").fadeOut("fast");
        resetCountDown(); 
    }

    function resetCountDown() {
        countDown = 10; 
    }

    function tickCountDown() {
        if (countDown > 0) {
            countDown--;
            if (countDown == 0) {
                newSearch(); 
            }
        }
    }

    function setupCountDown() {
        setInterval(tickCountDown, 100); 
    }

    $("#textBox").on("keyup", function (event) {

        if (event.which) {
            if (event.which == 16 || event.which == 9) { }
            else if (event.which == 13) {
                resetCountDown();
                event.stopPropagation();
            } else {
                resetCountDown();
            }
        } else {
            resetCountDown();
        }
    }); 

    function newSearch() {
        currentPage = 1;
        getValues();
    }

    function getValues(standards) {

        var searchParameter = {

            media: [],
            grade: [], 
            subject: [], 
        }; 

    $(".filterBox .list").each(function () {

        var currentList = $(this).attr("data-gooruID");

            var items = ""; 

            $(this).find("a[data-selected=true]").each(function () {

                if (currentList == "subject") {
                    items += "~~" + ($(this).attr("data-name"));
                } else {
                    items += "," + ($(this).attr("data-name"));
                }
            });
            items = items.replace("~~", ""); 
            items = items.replace(",", ""); 
            searchParameter[currentList] = items; 
        }); 
        doSearch(searchParameter.media, searchParameter.grade, searchParameter.subject, standards);
    }

    function doSearch(category, grade, subject, standard) {

        $("#welcomeTXT").hide();
        $("#loader").show();
        var query = $("#textBox").val();
        var data = {}; 
        if (query == "") {
            data = { 'pageNum': currentPage, 'category': category, 'flt.grade': grade, 'flt.subjectName': subject, 'flt.standard': standard };
        } else {
            data = { 'pageNum': currentPage, 'category': category, 'flt.grade': grade, 'flt.subjectName': subject, 'flt.standard': standard, 'query': query };
        }
        doAjax(data);
    }


    function doAjax(input) {
        if ($("#resourceSearch").is(':checked')) {
            $.ajax({
                type: 'GET',
                url: gooruResourcesUrl,
                data: input,
                dataType: "jsonp",
                crossDomain: true,
                cache: false,
                success: function (data) { displayResults(data); updatePageButtons(data.totalHitCount); updateResultCount(data.totalHitCount); }
            });
        } else {
            $.ajax({
                type: 'GET',
                url: gooruCollectionsUrl,
                data: input,
                dataType: "jsonp",
                crossDomain: true,
                cache: false,
                success: function (data) { displayResults(data); updatePageButtons(data.totalHitCount); updateResultCount(data.totalHitCount); }
            });
        }
    }

    /*function doAjaxDev(input) {
        if ($("#resourceSearch").is(':checked')) {
            $.ajax({
                type: 'GET',
                url: "//concept.goorulearning.org/gooruapi/rest/search/resource?sessionToken=d9a38a1a-88d0-444f-8e4b-a15120308b29&pageSize=20",
                data: input,
                dataType: "jsonp",
                crossDomain: true,
                cache: false,
                success: function (data) { displayResults(data); updatePageButtons(data.totalHitCount); updateResultCount(data.totalHitCount); }
            });
        } else {
            $.ajax({
                type: 'GET',
                url: "//concept.goorulearning.org/gooruapi/rest/search/collection?sessionToken=d9a38a1a-88d0-444f-8e4b-a15120308b29&pageSize=20",
                data: input,
                dataType: "jsonp",
                crossDomain: true,
                cache: false,
                success: function (data) { displayResults(data); updatePageButtons(data.totalHitCount); updateResultCount(data.totalHitCount); }
            });
        }
    }*/

    function updatePageButtons(hitCount) {

        var total = hitCount;
        var pageSize = 20; 
        var totalPages = Math.ceil(total / pageSize);

        if (total == 0 || totalPages == 1) {
            $(".pageButtons").hide();
            return;
        }else {
            $(".pageButtons").show();
        }

        $(".pageButtons").html("Jump to Page: ");
        var skips = [1, 10, 50, 100, 500, 1000, 5000, 10000, totalPages];
        for (var i = 1; i <= totalPages; i++) {
            var printed = false;
            if (i == skips[j] || (i >= currentPage - 3 && i <= currentPage + 3)) {
                if (i == currentPage) {
                    addPage(i, true);
                } else {
                    addPage(i, false);
                }
                printed = true;
            }
            for (var j in skips) {
                if (i == skips[j] && !printed) {
                    addPage(i, false);
                }
            }
        }
    }

    function addPage(pageNumber, isCurrent) {
        $(".pageButtons").append("<a href=\"#\" onclick=\"changePage(" + pageNumber + ")\"" + (isCurrent ? " class=\"currentPage\"" : "") + ">" + pageNumber + "</a>");
    }

    function changePage(target) {
        currentPage = target;
        getValues();
    }

    function fixThumbnail(image){
        $(image).attr("src", "/images/BlankImage.png");
    }

    function updateResultCount(hitCount) {
        $("#resultCount").append("Found " + hitCount + " results."); 
    }

    function displayResults(data) {
        //gooruOid
        $(".pageButtons").show();
        var results = data.searchResults;
        console.log("Results:", results);

        $("#resultCount").empty(); 
        $("#results").empty();

        if ($("#resourceSearch").is(':checked')) {
            for (var i = 0; i < results.length; i++) {
                var playerUrl = "/gooruResource?t=r&id=" + results[i].gooruOid;

            	try {
            		$("#results").append("<div id = 'resultData' class='resources'><a href ='" + playerUrl + "'  target='gooruRes'><img class='thumbnail' src ='" + results[i].thumbnails.url +
                    "' onerror='fixThumbnail(this)'/></a><a href = '" + playerUrl + "' target='gooruRes'><h2>" + results[i].title + "</h2></a><br><p>" + results[i].description +
                    "</p><br> Media Type: " + results[i].category + "<br> Views: " + results[i].viewCount + "<br>Subscribers: " + results[i].subscriptionCount + "</div>");
            	}
            	catch (e) { }
            }
        } else {
            for (var i = 0; i < results.length; i++) { 
                //player is for resources on
                var playerUrl = "/gooruResource?t=c&id=" + results[i].id;
                //var playerUrl = results[i].url;
                if (playerUrl == undefined) playerUrl = "javascript:void(0)";
                var goals = results[i].goals;
                if (goals == undefined) goals = "";

            	try {
            		$("#results").append("<div id = 'resultData' class='resources'><a href ='" + playerUrl + "'  target='gooruRes'><img class='thumbnail' src ='" + results[i].thumbnails.url +
                    "'onerror='fixThumbnail(this)'/></a><a href = '" + playerUrl + "' target='gooruRes'><h2>" + results[i].title + "</h2></a><br><p>" +
                    goals + "</p><br> Views: " + results[i].viewCount + "<br>Subscribers: " + results[i].subscriptionCount + "</div>");
            	}
            	catch (e) { }
            }
        }
        $("#loader").hide();
    }
</script>

<asp:Panel ID="hiddenPanel" runat="server" >
<asp:Literal ID="gooruTemplateResourceUrl" runat="server" Visible="false">//www.goorulearning.org/gooruapi/rest/search/resource?sessionToken={0}&pageSize=20</asp:Literal>
<asp:Literal ID="gooruTemplateCollectionsUrl" runat="server" Visible="false">//www.goorulearning.org/gooruapi/rest/search/scollection?sessionToken={0}&pageSize=20</asp:Literal>

    <%-- NOTE: This value is now superseded by the gooruApiUrl value in the Web.Config  --%>
    <asp:Literal ID="gooruApiUrl" runat="server" Visible="false">//www.goorulearning.org/gooruapi/rest/v2/account/loginas/anonymous?apiKey=960a9175-eaa7-453f-ba03-ecd07e1f1afc</asp:Literal>

    <%--//via dev key: 
    //  //concept.goorulearning.org/gooruapi/rest/search/collection?sessionToken=d9a38a1a-88d0-444f-8e4b-a15120308b29&pageSize=20
    //  //concept.goorulearning.org/gooruapi/rest/search/resource?sessionToken=d9a38a1a-88d0-444f-8e4b-a15120308b29&pageSize=20
    //prod: 
    
    //  //www.goorulearning.org/gooruapi/rest/search/resource?sessionToken=&pageSize=20",
    //  //www.goorulearning.org/gooruapi/rest/search/collection?sessionToken=960a9175-eaa7-453f-ba03-ecd07e1f1afc&pageSize=20
    // old 14-12-15: 783186ec-256e-41e9-a9e5-2cc7cf285f0--%>



</asp:Panel>
</asp:Content>