<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PublishResource_Narrow2.ascx.cs" Inherits="ILPathways.LRW.controls.PublishResource_Narrow2" %>
<%@ Register TagPrefix="uc1" TagName="LoginBox" Src="/Account/Controls/LoginController.ascx" %>
<%@ Register TagPrefix="uc1" TagName="ConditionsOfUseSelector" Src="/LRW/controls/ConditionsOfUseSelector.ascx" %>
<%@ Register TagPrefix="uc1" TagName="HeaderControl" Src="/Controls/Includes/Header6.ascx" %>
<%@ Register TagPrefix="uc1" TagName="ListPanel" Src="/LRW/controls/ListPanel.ascx" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowserV5" Src="/LRW/controls/StandardsBrowser5.ascx" %>

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

<script type="text/javascript" language="javascript" src="//ajax.googleapis.com/ajax/libs/jquery/1.9.0/jquery.min.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ISLE.css" />
<script language="javascript" type="text/javascript" src="/Scripts/tagList.js"></script>
<link rel="Stylesheet" href="/Styles/TagList.css" />
<script src="//code.jquery.com/ui/1.10.1/jquery-ui.js"></script>
<link rel="stylesheet" href="//code.jquery.com/ui/1.10.1/themes/base/jquery-ui.css" />
<script language="javascript" type="text/javascript" src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" href="/Styles/ToolTip.css" />

<style type="text/css">
  /* Main Page stuff */
  #header, #headerContent { background-color: #FFF; }
  #header { padding-top: 5px; }
  html, body, form, .mainPageContainer {
    margin: 0;
    padding: 0;
    width: 100%;
    height: 100%;
    overflow: hidden;
    background-color: <%=css_white %>;
  }
  .blockLink {
    color: #FFF;
    background-color: <%=css_blue %>;
    font-weight: bold;
    display: block;
    text-align: center;
    height: 30px;
    line-height: 30px; 
  }
  .blockLink:hover, a.blockLink:focus {
    background-color: <%=css_orange %>; 
  }
  .tabNav .blockLink {
    background-image: url('/images/whiteMiddleGradient50.png');
    background-repeat: repeat-x;
    background-position: 0 -40px;
  }
  #previewFrame {
    position: absolute;
    height: 100%;
    left: 400px;
    top: 80px;
    border: none;
    background-color: <%=css_white %>;
    min-width: 450px;
  }
  
  /* Tool Container */
  .toolContainer {
    height: 100%;
    width: 400px;
    background-color: #FFF;
    position: absolute;
    top: 80px;
    left: 0;
    overflow-y: scroll;
    overflow-x: hidden;
    z-index: 1;
  }
  
  /* Navigation Tabs */
  .tabNav {
    list-style-type: none; 
    border-radius: 10px;
    overflow: hidden;
  }
  .tabNav li a {
    width: 200px;
    height: 25px;
    line-height: 25px;
  }
  .tabNav li a:hover, .tabNav li a:focus {
    background-color: <%=css_orange %>;
    color: #FFF;
  }
  
  /* Custom Flyouts */
  .flyout {
    position: fixed;
    right: 50px;
    top: 80px;
    min-width: 200px;
    
  }
  .isleBox_closeButton {
    background-image: url('/images/icons/isleBox_closeButton.png');
    background-position: 0 0;
    float: right;
    display: block;
    height: 20px;
    width: 20px;
    margin: -1px -7px 0 0;
  }
  .isleBox_closeButton:hover, .isleBox_closeButton:focus {
    background-position: 20px 0;
  }
  
  /* Progress Bar */
  #progressBarContainer {
    width: 340px;
    margin: 2px auto;
    border-radius: 10px;
    border: 1px solid <%=css_teal %>; 
    padding: 1px;
  }
  #progressBar {
    background-color: <%=css_orange %>; 
    border-radius: 10px;
    color: #FFF;
    min-width: 20px;
    padding: 0 5px;
    text-align: center;
    background-image: url('/images/whiteMiddleGradient50.png');
    background-repeat: repeat-x;
    background-position: 0 -25px;
  }
  
  /* Pages */
  .page, .pagetopper {
    padding: 5px; 
  }
  .page {
    display: none;
  }
  .pageItem {
    margin-bottom: 25px;
  }
  
  /* Form stuff */
  input[type=text], select, textarea, input[type=password] {
     border: 1px solid #DDD;
     border-radius: 5px;
     padding: 2px;
     margin-bottom: 15px;
     width: 100%;
     -moz-box-sizing: border-box;
     -webkit-box-sizing: border-box;
     box-sizing: border-box;
  }
  textarea {
    max-width: 100%;
    min-width: 100%;
    height: 100px;
    min-height: 75px;
    max-height: 200px;
  }
  .required, .optional {
    border-radius: 5px;
    padding-left: 5px;
  }
  .required {
    border-left: 10px solid <%=css_red %>;
  }
  .optional {
    border-left: 10px solid <%=css_black %>;
  }
  .buttons {
    text-align: right;
  }
  .buttons .left {
    float: left;
  }
  .progressButton {
    background-color: <%=css_blue %>;
    color: #FFF;
    padding: 2px 10px;
    border-radius: 10px;
    font-weight: bold;
    cursor: pointer;
    min-width: 150px;
    background-image: url('/images/whiteMiddleGradient50.png');
    background-repeat: repeat-x;
    background-position: 0 -40px;
    margin-bottom: 10px;
  }
  .progressButton:hover, .progressButton:focus {
    background-color: <%=css_orange %>;
  }
  
  /* Checkboxes */
  .cbxl input[type=checkbox] {
    margin: 2px 0 2px 2px;
    cursor: pointer;
  }
  .cbxl label {
    display: inline-block;
    *display: inline;
    zoom: 1;
    width: 348px;
    padding: 1px 0 1px 5px;
    cursor: pointer;
  }
  .cbxl li:hover, cbxl li:focus {
    background-color: <%=css_orange %>; 
    color: #FFF;
  }
  
  /* Miscellaneous */
  .navigationLink {
    text-align: right;
    padding: 0 20px;
    border-radius: 10px;
    font-size: 120%;
    line-height: 30px;
    height: 32px;
    width: 100%;
    -moz-box-sizing: border-box;
    -webkit-box-sizing: border-box;
    box-sizing: border-box;
  }
  .quickSubject {
    display: inline-block;
    *display: inline;
    zoom: 1;
    background-color: <%=css_blue %>;
    padding: 1px 3px;
    margin: 1px 2px;
    border-radius: 5px;
    color: #FFF;
  }
  .quickSubject:hover, .quickSubject:focus {
    background-color: <%=css_orange %>;
    color: #FFF;
  }
  div.toolTipOuter {
    position:fixed;
  }
  .toolTipLink {
    color: inherit;
  }
  .reviewStuff ul.review_standards {
    list-style-type: none;
    padding-left: 25px;
  }
  .reviewStuff div, .reviewStuff ul {
    padding-left: 25px;
  }
  #page_review input.publishButton {
    background-color: <%=css_orange %>;
  }
  #page_review input.publishButton:hover, .publishButton:focus {  
    background-color: <%=css_teal %>;
  }
  #url_linksBox {
    text-align: right;
  }
  
  /* Usage Rights */
  /*#useRightsTable {
    width: 100%;
    border-collapse: collapse;
    -moz-box-sizing: border-box;
    -webkit-box-sizing: border-box;
    box-sizing: border-box;
  }
  #useRightsTable td.ccRightsImage {
    width: 90px;
    padding-right: 5px;
  }
  #useRightsTable .ddlConditionsOfUse {
    margin-bottom: 1px;
  }*/
  
  /* Time Taken */
  input.timeTaken, select.timeTaken {
    width: 60px;
    margin-left: 10px;
    text-align: right;
  }
  
  /* Login Stuff */
  #loginTable {
    display: table;
  }
  
  /* Date Picker */
  .ui-datepicker-trigger {
    height: 20px;
    display: inline-block;
    *display: inline;
    zoom: 1;
    margin: 0 0 -5px 5px;
    cursor: pointer;
  }

  /* Standards Selector */
  #appliedStandardsContainer {
    top: 25px;
    position: static;
  }
  .appliedStandards {
    height: 300px;
    /* width: 400px; */
    overflow-x: hidden;
    overflow-y: scroll;
  }
  .appliedStandards .isleBox h2 a, .bottomLevel.isleBox h2 a, .reviewStuff .isleBox h2 a {
    color: #FFF; 
  }
  .appliedStandards .isleBox h2 a:hover, .appliedStandards .isleBox h2 a:focus, .reviewStuff .isleBox h2 a:hover, .reviewStuff .isleBox h2 a:focus {
    text-decoration: underline;
  }
  .alignmentType.ddl {
    width: 125px;
    display: inline;
    margin-bottom: 2px;
  }
  
  /* Overlay Stuff */
  .overlayContainer {
    position: fixed;
    height: 100%;
    width: 100%;
    z-index: 9999;
  }
  .overlayCover {
    position: fixed;
    width: 100%;
    height: 100%;
    background-color: #000;
    opacity: 0.4;
    filter:alpha(opacity=40);
  }
  .overlayWrapper {
    position: relative;
    width: 100%;
    height: 100%; 
  }
  .overlayBox {
    width: 800px;
    height: 300px;
    margin: 50px auto;
  }
  .overlayBox a {
    display: block;
    text-align: center;
    font-size: 200%;
    color: #FFF;
    font-weight: bold;
    border-radius: 20px;
    margin: 5px 10px;
    background: <%=css_teal %> url('/images/whiteMiddleGradient50.png') repeat-x left top;
    background-position: 0 -20px;
  }
  .overlayBox a:hover, .overlayBox a:focus {
    background-color: <%=css_orange %>;
  }
  
  /* Page Message Panel */
  #pageMessagePanel {
    top: 80px;
    right: 50px;
    max-width: 300px;
    z-index: 10000;
  }
  .messageError {
    background-color: <%=css_red %>;
    color: #FFF;
    font-weight: bold;
    text-align: center;
    padding: 2px 10px;
    border-radius: 5px;
    margin: 2px;
  }
  .messageError a {
    color: #FFF;
    text-decoration: underline;
  }
  .messageError a:hover, .messageError a:focus {
    text-decoration: underline;
  }
  
  /* Assessment Type */
  #flyout_additionalInfo .cbxl label {
    width: 310px;
  }
  #flyout_additionalInfo ul {
    list-style-type: none;
  }
  .previewLink {
    display: block;
    text-align: right;
  }
  .previewLink span {
    display: inline-block;
    height: 30px;
    padding: 2px;
  }
  .previewLink .icon {
    height: 25px;
    width: 25px;
    background: transparent url('/images/icons/preview-popup.png') no-repeat top left;
  }
  .previewLink .text {
    vertical-align: middle;
  }
  .previewLink .frame {
    background-position: 0 0;
  }
  .previewLink .popup {
    background-position: -30px 0;
  }
  .previewLink:hover .frame, .previewLink:focus .frame {
    background-position: 0 -30px;
  }
  .previewLink:hover .popup, .previewLink:focus .popup {
    background-position: -30px -30px;
  }

</style>

<!-- Global Variables -->
<script type="text/javascript" language="javascript">
    //Data
    var progressLevels = new Array();
    progressLevels["welcome"] = 1;
    progressLevels["url"] = 1;
    progressLevels["basicInfo"] = 10;
    progressLevels["copyrightInfo"] = 15;
    progressLevels["endUser"] = 25;
    progressLevels["resourceTypes"] = 30;
    progressLevels["mediaTypes"] = 40;
    progressLevels["standards"] = 50;
    progressLevels["gradeLevels"] = 65;
    progressLevels["careerClusters"] = 75;
    progressLevels["groupTypes"] = 85;
    progressLevels["extraInfo"] = 90;
    progressLevels["review"] = 99;

    var hdnKeywords;
    var hdnSubjects;
    var hdnStandards;

    var requiresItemType = false;

</script>

<!-- Document.Ready Functions -->
<script type="text/javascript" language="javascript">
    var jWindow;
    var publishingMode = true;
    $(document).ready(function () {
        jWindow = $(window);
        //Formatting
        jWindow.resize(resizeFrame)
        jWindow.resize();

        //More variables
        hdnKeywords = $("#<%=hdnKeywords.ClientID %>");
    hdnSubjects = $("#<%=hdnSubjects.ClientID %>");
      hdnStandards = $("#<%=hdnStandards.ClientID %>");

      //Initialize Page
      setProgress(0);
      showPage('welcome');
      if(!<%=postbackMode %>){
      $(".ddlItemType").prop("selectedIndex", "-1");
  }

    //Init Tooltips
    $(".cbxl").each(function() {
        var list = $(this);
        var tableName = list.attr("tablename");
        list.find("li > span").each(function() {
            var item = $(this);
            item.addClass("toolTipLink");
            item.attr("title", item.attr("itemname") + "|" + item.attr("itemdescription"));
            item.attr("id", tableName + "_" + item.attr("itemid"));
        });
    });
initToolTips();

//separate assessments
$(".cbxl_resourceTypes li:contains(Assessment)").first().before("<h3 class=\"isleH3_Block\">Assessment Types</h3>");
$(".cbxl_resourceTypes li:contains(Assessment)").last().after("<h3 class=\"isleH3_Block\">Other Types</h3>");

//Restore Keywords/Subjects after postback
var tempSubjects = hdnSubjects.val().split(",");
for (var i = 0; i < tempSubjects.length; i++) {
    if (tempSubjects[i] != "") {
        addTag($("#tagEntry_subjects")[0], "subjects", tempSubjects[i]);
    }
}
hdnSubjects.val("");

var tempKeywords = hdnKeywords.val().split(",");
for (var i = 0; i < tempKeywords.length; i++) {
    if (tempKeywords[i] != "" && tempKeywords[i] != "pathways_testing_mode") {
        addTag($("#tagEntry_keywords")[0], "keywords", tempKeywords[i]);
    }
}
hdnKeywords.val("");

//Restore standards
var tempStandards = hdnStandards.val().split("|%|");
for (var i = 0; i < tempStandards.length; i++) {
    if (tempStandards[i] != "") {
        var standardBits = tempStandards[i].split("|~|");
        //applyThisStandard(standardBits[0], standardBits[3], standardBits[1], standardBits[2], standardBits[5], standardBits[4]);
        
        //hdnStandards.val(hdnStandards.val() + "|%|" + item.code + "|~|" + item.url + "|~|" + item.text + "|~|" + item.alignmentID + "|~|" + item.alignmentText );
    }
}

//Date picker
$(".txtDateCreated").datepicker({
    showOn: "button",
    buttonImage: "images/icons/calendar.gif",
    buttonImageOnly: true,
    maxDate: "+0D",
    changeMonth: true,
    changeYear: true
}).width(340);

//ensure date is valid
$(".txtDateCreated").change(function () {
    try {
        chosenDate = $.datepicker.parseDate("mm/dd/yy", $(this).val());
        today = new Date();
        selectedDate = new Date(chosenDate);
        if (selectedDate > today) {
            throw new Error(); //ensures things are not said to have been created in the future
        }
    }
    catch (error) {
        alert("Invalid Date");
        $(this).val("");
    }
});

//Ensure Day box is numeric
$(".txtTimeTakenDays").keyup(function () {
    var thisBox = $(this);
    if (parseInt(thisBox.val())) {
        var thisNumber = parseInt(thisBox.val());
        if (thisNumber > 365) {
            thisBox.val("365");
        }
        if (thisNumber < 0) {
            thisBox.val("0");
        }
    }
    else {
        thisBox.val("0");
    }
});

//Quick subjects
$(".quickSubject").click(function () {
    addTag($("#tagEntry_subjects")[0], "subjects", $(this).text());
    $(this).css("display", "none");
});

//Just Published
if ($(".overlayContainer").attr("justpublished") == "true") {
    $(".overlayContainer").fadeIn();
}

//Check for existing resource URL
$(".txtResourceURL").change(function () {
    queryServer("CheckExistingURL", "{ 'targetURL' : '" + $(".txtResourceURL").val() + "' }", injectURLResult, "");
});

//Check for login
checkLogin();

//AssessmentType and ItemType and EdUse Assessment
var assessmentObjects = $(".cbxl_resourceTypes li label:contains(Assessment)");
assessmentObjects.each(function () {
    $(this).parent().click(function (event) {

        //Setup variables
        var assessmentObjects = $(".cbxl_resourceTypes li label:contains(Assessment)");
        var hasTest = false;
        var hasObject = false;

        //Detect Assessment Item
        if (assessmentObjects.parent().find("label:contains(Assessment Item)").parent().find("input").prop("checked")) {
            $(".additionalInfo_itemType").show();
            hasObject = true;
            requiresItemType = true;
        }
        else {
            $(".additionalInfo_itemType").hide();
            requiresItemType = false;
        }

        //Scan the other Assessment things
        assessmentObjects.not("label:contains(Assessment Item)").parent().find("input").each(function () {
            if ($(this).prop("checked")) {
                hasTest = true;
                hasObject = true;
            }
        });
        if (hasTest) {
            $(".additionalInfo_assessmentType").show();
        }
        else {
            $(".additionalInfo_assessmentType").hide();
        }

        //Show or hide the panel
        if (hasObject) {
            $("#flyout_additionalInfo").fadeIn("fast");
            $(".cbxl_educationalUse label:contains(Assessment)").parent().find("input").prop("checked", true).prop("disabled", true);
        }
        else {
            $("#flyout_additionalInfo").fadeOut("fast");
            $(".cbxl_educationalUse label:contains(Assessment)").parent().find("input").prop("checked", false).prop("disabled", false);
        }

    });
});

//Pulsing error messages
setInterval(function () { $(".messageError").fadeTo(750, 0.3).fadeTo(750, 1) }, 2000);

//Force mobile device resize triggering
setInterval(function () { jWindow.resize(); }, 1000);

//Interface-ify Educational Use
$(".cbxl_educationalUse label:contains(Curriculum)").append(" (at selected Grade Level)");
$(".cbxl_educationalUse label:contains(Enhancement), .cbxl_educationalUse label:contains(Intervention)").each(function() {
    var item = $(this);
    item.parentsUntil("li").parent().css("margin-left", "25px").css("list-style-type", "none");
    item.width(item.width() - 25);
    item.parent().find("input").click(function() {
        if($(this).prop("checked")){
            $(".cbxl_educationalUse label:contains(Curriculum)").parent().find("input").prop("checked", true).prop("disabled",true);
        }
        else {
            var keepDisabled = false;
            $(".cbxl_educationalUse label:contains(Enhancement), .cbxl_educationalUse label:contains(Intervention)").each(function() {
                if($(this).parent().find("input").prop("checked")){
                    keepDisabled = true;
                }
            });
            $(".cbxl_educationalUse label:contains(Curriculum)").parent().find("input").prop("disabled",keepDisabled);
        }
    });
});

if("<%=sandboxText %>" == "true"){
        $(".buttons").prepend("<div>Sandbox Mode</div>");
        $(".toolContainer").css("background-color", "#F2DCA0");
    }

    scanMessagePanel();
    });           //End document.ready

</script>

<!-- General Methods -->
<script type="text/javascript" language="javascript">

    //Review Page
    function updateReviewPage(){
        //texts
        $(".review_text").each(function() {
            var thisBox = $(this);
            var target = $(".txt" + this.id.replace("review_",""));
            thisBox.html("");
            thisBox.html(target.val());
        });
        //checkbox lists
        $(".review_cbxl").each(function() {
            var thisBox = $(this);
            var targets = $(".cbxl_" + this.id.replace("review_","") + " li input[type=checkbox]");
            thisBox.html("");
            for(i = 0; i < targets.length; i++){
                var jTarget = $(targets[i]);
                if( jTarget.prop("checked") == true){
                    thisBox.append("<li>" + jTarget.parent().find("label").text() + "</li>");
                }
            }
        });
        //drop-down lists
        $(".review_ddl").each(function() {
            var thisBox = $(this);
            var target = $(".ddl" + this.id.replace("review_","") + " option:selected");
            thisBox.html("");
            thisBox.html(target.text());
        });
        //standards
        //$(".review_standards").html($(".appliedStandards").html());

        /*
        //V4
        $(".review_standards").html("");
        for (i in selectedItems) {
        $(".review_standards").append("<li>" + selectedItems[i].alignmentText + " " + selectedItems[i].code + "</li>");
        }
        */

        //V5
        $(".review_standards").html("");
        $("#selectedStandards .selectedStandard").each(function () {
            $(".review_standards").append(
              "<li><b>" +
              ( $(this).find("select option:selected").attr("value") == "0" ? "Applies To" : $(this).find("select option:selected").text() ) +
              ":</b> " +
              ( $(this).attr("data-code") != "" ? $(this).attr("data-code") : $(this).find("span.text").text().substr(0, 25) ) 
            );
        });

        //miscellaneous
        //time required
        var days = $(".txtTimeTakenDays").val();
        var hours = $(".ddlTimeTakenHours option:selected").text();
        var minutes = $(".ddlTimeTakenMinutes option:selected").text();
        if(days == "0" && hours == "0" && minutes == "0"){ 
            $(".review_time").html("Not Applicable");
        }
        else {
            $(".review_time").html( days + " Days, " + hours + " Hours, " + minutes + " Minutes" );
        }
        //tags
        $(".review_tags").each(function() {
            $(this).html( $("#tags_" + this.id.split("_")[1]).html() ); 
        });
    }


    //Publish
    function publish() {
        //Setup hidden fields
        hdnSubjects.val("");
        hdnKeywords.val("");
        hdnStandards.val("");
        $("#tags_subjects li").each(function() {
            var thisText = $(this).text();
            hdnSubjects.val(hdnSubjects.val() + "," + thisText.substring(0,thisText.length - 2));
        });
        $("#tags_keywords li").each(function() {
            var thisText = $(this).text();
            hdnKeywords.val(hdnKeywords.val() + "," + thisText.substring(0,thisText.length - 2));
        });
        /*
        $(".appliedStandards li").each(function() {
          var thisBox = $(this);
          var thisID = thisBox.prop("id").split("_")[1];
          var thisLink = thisBox.find("#link_appliedStandard_" + thisID);
          hdnStandards.val(hdnStandards.val() + "|%|" + thisID + "|~|" + thisLink.text() + "|~|" + thisLink.prop("href") + "|~|" + thisBox.find(".description").html() + "|~|" + thisBox.find(".alignmentID").text() + "|~|" + thisBox.find(".standardAlignmentTypeText").text() );
        });
        */
        /*
        for (i in selectedItems) {
          var item = selectedItems[i];
          hdnStandards.val(hdnStandards.val() + "|%|" + item.code + "|~|" + item.url + "|~|" + item.text + "|~|" + item.alignmentID + "|~|" + item.alignmentText );
        }
        */
        //hdnStandards.val(JSON.stringify(selectedItems));
        var publishingStandardsList = [];
        $("#selectedStandards .selectedStandard").each(function () {
            var item = getLayer($(this).attr("data-id"));
            var ddlAlignment = $(this).find("select option:selected");
            var itemAlignmentID = ddlAlignment.attr("value");
            var itemAlignmentText = ddlAlignment.attr("text");
            console.log(item);
            publishingStandardsList.push({
                id: item.id,
                parent: 0,
                code: item.code,
                url: item.url,
                grades: item.grades,
                text: item.text,
                alignmentID: itemAlignmentID,
                alignmentText: itemAlignmentText
            });
        });
        hdnStandards.val(JSON.stringify(publishingStandardsList));
        /*$("#selectedStandards li").each(function() {
          var item = $(this);
          hdnStandards.val(hdnStandards.val() + "|%|" + item.attr("standardCode") + "|~|" + item.attr("standardURL") + "|~|" + item.attr("standardDescription") + "|~|" + item.attr("alignID") + "|~|" + item.attr("alignText"));
        });*/

        //Validate
        if(!validatePage("all","")){
            return false;
        }
        else {
            //Do Publish
            $(".publishButton").prop("disabled", true);
            animatePublish();
            __doPostBack("btnPublish","");
        }
    }

    var timer;
    var ticker = 0;
    var tickerMax = 5;
    var publishButton;
    function animatePublish() {
        publishButton = $(".publishButton");
        timer = setInterval(updateAnimation, 500);
        publishButton.val("Publishing Tags, Please Wait");
    }
    function updateAnimation() {
        publishButton.val("Publishing Tags, Please Wait");
        for (var i = 0; i <= tickerMax; i++) {
            if (i <= ticker) {
                publishButton.val(publishButton.val() + ".");
            }
            else {
                publishButton.val(publishButton.val() + " ");
            }
        }
        ticker++;
        if (ticker > tickerMax) {
            ticker = 0;
        }
    }

</script>

<!-- Navigation Methods -->
<script type="text/javascript" language="javascript">

    //Page Toggling
    function showPage(targetPage) {
        $(".page").css("display", "none");
        $("#page_" + targetPage).fadeIn("fast");
        setProgress(progressLevels[targetPage]);
        if (targetPage == "welcome") {
            $(".pagetopper").css("display", "none");
        }
        else {
            $(".pagetopper").fadeIn("fast");
        }
        if (targetPage == "review") {
            updateReviewPage();
        }
        if (targetPage == "standards") {
            //resizeItems();
        }
    }
    function setProgress(percent) {
        var progress = ((percent / 100) * $("#progressBarContainer").width()) - 10;
        $("#progressBar").html(percent + "%");
        $("#progressBar").animate({ width: progress });
    }

    //Mode changing
    function showMeEverything() {
        $(".page").fadeIn("fast");
        $(".buttons").not("#loginTable .buttons").css("display", "none");
        $(".pagetopper").css("display", "none");
        $("#btnBackToGuided").css("display", "inline");
        $("#flyout_navigation").fadeOut("fast");
        $(".reviewStuff").css("display", "none");
        //$("#appliedStandardsContainer").css("position", "static")
        //$(".appliedStandards").css("width", "inherit");
        //$("#flyout_additionalInfo").removeClass("flyout");
        resizeItems();
    }
    function backToGuided() {
        $(".page").css("display", "none");
        $(".buttons").css("display", "block");
        $(".pagetopper").css("display", "block");
        $("#btnBackToGuided").css("display", "none");
        $(".reviewStuff").css("display", "block");
        //$("#appliedStandardsContainer").css("position", "fixed")
        //$(".appliedStandards").css("width", "400px");
        //$("#flyout_additionalInfo").addClass("flyout");
        showPage("welcome");
        //Fixing a rendering bug in jQuery
        $("#loginTable").css("display", "table");
        $("#loginTable tbody").css("display", "table-row-group");
        $("#loginTable tbody tr").css("display", "table-row");
        $("#loginTable tbody tr td").css("display", "table-cell");
    }


</script>

<!-- Utility Methods -->
<script type="text/javascript" language="javascript">
    function external_applyStandardsV4(data) {
        //Reset the array
        var gradesList = [];
        //Apply grades as needed
        for (i in data) {
            for (j in data[i].grades) {
                if($.inArray(data[i].grades[j], gradesList) == -1){
                    gradesList.push(data[i].grades[j]);
                }
            }
        }
        //check the items
        for(i in gradesList){
            if(gradesList[i] == "K"){
                $("ul[tableName=gradeLevel] li span[itemname=Kindergarten] input").prop("checked",true);
            } 
            else if(gradesList[i] == "9" || gradesList[i] == "10"){
                $("ul[tableName=gradeLevel] li span[itemname='Grades 9-10'] input").prop("checked",true);
            }
            else if(gradesList[i] == "11" || gradesList[i] == "12"){
                $("ul[tableName=gradeLevel] li span[itemname='Grades 11-12'] input").prop("checked",true);
            }
            else {
                $("ul[tableName=gradeLevel] li span[itemname='Grade " + gradesList[i] + "'] input").prop("checked",true);
            }
        }
    }

    //Message Panel
    function scanMessagePanel() {
        var messagePanel = $("#pageMessagePanel");
        if(messagePanel.find("#pageMessage").html() != ""){
            messagePanel.fadeIn("slow");
        }
        else{
            messagePanel.fadeOut("slow");
        }
    }

    //Enable/Disable Buttons
    function enableButton( jButton ){
        jButton.prop("disabled",false).fadeTo(250,1);
    }
    function disableButton( jButton ){
        jButton.prop("disabled",true).fadeTo(250,0.5);
    }

    //Handle Error Messages
    function addErrorMessage(messageID, messageClass, messageText){
        removeErrorMessage(messageID);
        $("#pageMessagePanel #pageMessage").append("<div class=\"messageError " + messageClass + "\" id=\"" + messageID + "\">" + messageText + "</div>");
    }
    function removeErrorMessage(messageID){
        $("#" + messageID).remove();
    }

    function checkLogin() {
        if ($(".isNotLoggedIn").html()) {
            addErrorMessage("loggedInError", "pageProblem", "You must <a href=\"javascript:showPage('welcome')\">log in</a> before you can Tag a Resource.<br />WARNING: You will lose your work if you do not log in <i>before</i> filling out the fields in this tool!");
            scanMessagePanel();
        }
        else if ($(".publishButton").length == 0) {
            addErrorMessage("authorizedError", "pageProblem", "Your account is not authorized to Publish. You may preview the tool, but you will not be able to save or publish anything.");
            scanMessagePanel();
        }
    }

    //Restore Quick Subjects
    function customTagListClickHandler() {
        $("li.tag a").click(function () {
            $(".quickSubject:contains(" + $(this).parent().text().substring(0, $(this).parent().text().length - 2) + ")").css("display", "inline-block").css("*display", "inline");
        });
    }

    //Flyouts
    function showFlyout(target) {
        $("#flyout_" + target).fadeIn("fast");
    }
    function hideFlyout(target) {
        $("#flyout_" + target).fadeOut("fast");
    }
    function toggleFlyout(target) {
        var element = $("#flyout_" + target);
        if (element.css("display") == "none") {
            element.fadeIn("fast");
        }
        else {
            element.fadeOut("fast");
        }
    }

    function resizeFrame() {
        var desiredHeight = jWindow.height() - 80;
        $("#previewFrame").width(jWindow.width() - 400).height(desiredHeight);
        $(".toolContainer").height(desiredHeight);

        if (window.innerWidth < 850) {
            $("#flyout_navigation").css("left", "inherit").css("right", "45px");
        }
        else {
            $("#flyout_navigation").css("left", "425px").css("right", "inherit");
        }
    }

    function previewInFrame() {
        $("#previewFrame").prop("src", $(".txtResourceURL").val());
    }
    function previewInPopup() {
        if(typeof resourcePreviewWindow == "object"){
            resourcePreviewWindow = null;
        }
        resourcePreviewWindow = window.open( $(".txtResourceURL").val(), "", "toolbar=1,menubar=1,resizable=1,scrollbars=1,width=" + $("#previewFrame").width() + ",height=" + screen.height );
        resourcePreviewWindow.moveTo(400, 0);
        resourcePreviewWindow.focus();

    }

  </script>

<!-- Validation Methods -->
<script type="text/javascript" language="javascript">
    //Individual Page Validation
    function validatePage(targetPageID, nextPageID) {
        var isValid = true;
        //URL page
        if(targetPageID == "url" || targetPageID == "all"){
            //validate URL
            var urlVal = $(".txtResourceURL").val();
            if(urlVal == "" || urlVal == "http://" || urlVal == "https://" || urlVal == "ftp://" || urlVal.length < 12){
                addErrorMessage("errInvalidURL", "pageProblem", "Please enter a valid <a href=\"javascript:showPage('url')\">URL</a>.");
                isValid = false;
            }
            else {
                removeErrorMessage("errInvalidURL");
            }
            //validate Language
            if($(".ddlLanguage").prop("selectedIndex") == "-1"){
                addErrorMessage("errInvalidLanguage", "pageProblem", "Error: You must <a href=\"javascript:showPage('url')\">select a Language</a>.");
                isValid = false;
            }
            else {
                removeErrorMessage("errInvalidLanguage");
            }
            //Check for existing URL
            if($("#pageMessagePanel #errURLExists").html()){
                isValid = false;
            }
            if(isValid & targetPageID != "all"){
                showPage(nextPageID);
            }
        }
        //Basic Info Page
        if (targetPageID == "basicInfo" || targetPageID == "all") {
            //validate Title
            if($(".txtTitle").val().length < 4){
                addErrorMessage("errTitleShort", "pageProblem", "Please enter a <a href=\"javascript:showPage('basicInfo')\">Title</a> of meaningful length.");
                isValid = false;
            }
            else{
                removeErrorMessage("errTitleShort");
            }
            //validate Description
            if($(".txtDescription").val().length < 25){
                addErrorMessage("errDescriptionShort", "pageProblem", "Please enter a <a href=\"javascript:showPage('basicInfo')\">Description</a> of meaningful length.");
                isValid = false;
            }
            else{
                removeErrorMessage("errDescriptionShort");
            }
            if (isValid & targetPageID != "all") {
                showPage(nextPageID);
            }
        }
        //Copyright Page
        if (targetPageID == "copyrightInfo" || targetPageID == "all") {
            //Validate Usage Rights
            var urlVal = $(".txtConditionsOfUse").val();
            if (urlVal == "" || urlVal == "http://" || urlVal == "https://" || urlVal == "ftp://" || urlVal.length < 12) {
                addErrorMessage("errConditionsOfUse", "pageProblem", "Please select or enter appropriate <a href=\"javascript:showPage('copyrightInfo')\">Usage Rights</a>.");
                isValid = false;
            }
            else {
                removeErrorMessage("errConditionsOfUse" || targetPageID == "all");
            }
            //Validate Access Rights
            if ($(".ddlAccessRights").prop("selectedIndex") == "-1") {
                addErrorMessage("errAccessRights", "pageProblem", "Please select appropriate <a href=\"javascript:showPage('copyrightInfo')\">Access Rights</a>.");
                isValid = false;
            }
            else {
                removeErrorMessage("errAccessRights");
            }
            if (isValid & targetPageID != "all") {
                showPage(nextPageID);
            }
        }
        //End User
        if (targetPageID == "endUser" || targetPageID == "all") {
            isValid = validateCBXL($(".cbxl_intendedAudience"), "errAudience", "Please select one or more <a href=\"javascript:showPage('endUser')\">End Users</a>.");
            if (isValid & targetPageID != "all") {
                showPage(nextPageID);
            }
        }
        //Resource Types
        if (targetPageID == "resourceTypes" || targetPageID == "all") {
            isValid = validateCBXL($(".cbxl_resourceTypes"), "errTypes", "Please select one or more <a href=\"javascript:showPage('resourceTypes')\">Resource Types</a>.");
            if (requiresItemType) {
                if ($(".ddlItemType").prop("selectedIndex") == "-1") {
                    addErrorMessage("errInvalidLanguage", "pageProblem", "Error: You must select an <a href=\"javascript:showPage('resourceTypes')\">Item Type</a>");
                    isValid = false;
                }
            }
            if (isValid & targetPageID != "all") {
                showPage(nextPageID);
            }
        }
        //Resource Formats
        if (targetPageID == "mediaTypes" || targetPageID == "all") {
            isValid = validateCBXL($(".cbxl_resourceFormats"), "errFormats", "Please select one or more <a href=\"javascript:showPage('mediaTypes')\">Media Types</a>.");
            if (isValid & targetPageID != "all") {
                showPage(nextPageID);
            }
        }

        scanMessagePanel();
        return isValid;
    }

    function validateCBXL(jCBXL, errorID, errorMessage) {
        var hasValue = false;
        jCBXL.find("li input").each(function () {
            if ($(this).prop("checked")) {
                hasValue = true;
            }
        });
        if (!hasValue) {
            addErrorMessage(errorID, "pageProblem", errorMessage);
            return false;
        }
        else {
            removeErrorMessage(errorID);
            return true;
        }
    }
</script>

<!-- AJAX Methods -->
<script type="text/javascript" language="javascript">
    //AJAX stuff
    function queryServer(targetServerMethod, targetData, targetJFunction, targetHTMLContainer) {
        $.ajax({
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: "/Services/ResourceStandardsService.asmx/" + targetServerMethod,
            data: targetData == "" ? {} : targetData,
            dataType: "json",
            async: false,
            success: function (msg) {
                if (targetHTMLContainer == "") {
                    targetJFunction(msg.d);
                }
                else {
                    targetJFunction(targetHTMLContainer, msg.d);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("error: " + jqXHR + " : " + textStatus + " : " + errorThrown);
            }
        });
    }

    function injectURLResult( data ){
        var info = jQuery.parseJSON(data);
        if(info.checkedURL == "true"){
            addErrorMessage("errURLExists","pageProblem", "That URL was already tagged. <a target=\"_blank\" href=\"/ResourceDetail.aspx?vid=" + info.existingPageVID + "\">Click Here to view its Detail page.</a>");
        }
        else {
            removeErrorMessage("errURLExists");
        }
        scanMessagePanel();
    }

</script>

<uc1:HeaderControl ID="header" runat="server" />

<div class="mainPageContainer">

  <div class="flyout isleBox" id="flyout_navigation" style="display:none;z-index:100;">
    <h2 class="isleBox_H2">Navigation <a href="javascript:hideFlyout('navigation')" class="isleBox_closeButton" title="Close Navigation window">&nbsp;</a></h2>
    <ul class="tabNav">
      <li id="nav_userInfo"><a href="javascript:showPage('welcome')" class="blockLink">Welcome</a></li>
      <li id="nav_resourceURL"><a href="javascript:showPage('url')" class="blockLink">Getting Started</a></li>
      <li id="nav_basicInfo"><a href="javascript:showPage('basicInfo')" class="blockLink">Basic Information</a></li>
      <li id="nav_copyrightInfo"><a href="javascript:showPage('copyrightInfo')" class="blockLink">Rights Management</a></li>
      <li id="nav_intendedAudiences"><a href="javascript:showPage('endUser')" class="blockLink">End User</a></li>
      <li id="nav_resourceTypes"><a href="javascript:showPage('resourceTypes')" class="blockLink">Resource Type</a></li>
      <li id="nav_mediaTypes"><a href="javascript:showPage('mediaTypes')" class="blockLink">Media Types/Educational Use</a></li>
      <li id="nav_learningStandards"><a href="javascript:showPage('standards')" class="blockLink">Learning Standard</a></li>
      <li id="nav_gradeLevels"><a href="javascript:showPage('gradeLevels')" class="blockLink">Grade Level</a></li>
      <li id="nav_careerClusters"><a href="javascript:showPage('careerClusters')" class="blockLink">Career Cluster</a></li>
      <li id="nav_groupTypes"><a href="javascript:showPage('groupTypes')" class="blockLink">Group Type</a></li>
      <li id="nav_extraInfo"><a href="javascript:showPage('extraInfo')" class="blockLink">Extra Information</a></li>
      <li id="nav_review"><a href="javascript:showPage('review')" class="blockLink">Review</a></li>
    </ul>
  </div>

  <div class="toolContainer">
    
    <div class="pagetopper">
      <input type="button" class="progressButton navigationLink" onclick="toggleFlyout('navigation')" value="Jump to a Page &rarr;" />

      <div class="isleBox">
        <h2 class="isleBox_H2">Progress</h2>
        <div id="progressBarContainer"><div id="progressBar">&nbsp;</div></div>
      </div>
    </div>

    <div class="page" id="page_welcome">
      <h2 class="isleH2">Welcome!</h2>
      <p>The <a href="/" class="textLink">ISLE</a> Resource Tagging Tool is here to help you tag Resources and publish the tags to the <a href="http://www.learningregistry.org/" target="_blank" class="textLink">Learning Registry</a>.</p>
      <p class="required">Required items are shown with a Red border. You must fill these out in order to publish your tags.</p>
      <p class="optional">Optional items are shown with a plain border. These will make it easier for others to find your Resource.</p>
      
      <div id="loginContainer" runat="server">
        <h3 class="isleH3_Block required isNotLoggedIn">You must log in to Tag.</h3>
        <uc1:LoginBox ID="loginBox" runat="server" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton" value="Sounds good. Walk me through it &rarr;" onclick="showPage('url')" />
        <input type="button" class="progressButton" value="I'm experienced. Show me everything at once &rarr;" onclick="showMeEverything()" />
      </div>
        <input type="button" class="progressButton navigationLink" value="I'd like to go back to guided mode &rarr;" id="btnBackToGuided" style="display:none" onclick="backToGuided()" />
    </div>

    <div class="page" id="page_url">

      <div class="pageItem">
        <h2 class="isleH2">Getting Started</h2>
        <h3 class="isleH3_Block required">First, we need the Resource's URL:</h3>
        <input type="text" placeholder="Resource URL" runat="server" class="txtResourceURL" id="txtResourceURL" />
        <div id="url_linksBox">
          <p>Would you like to preview this Resource?</p>
          <a href="javascript:previewInFrame()" class="previewLink"><span class="text">Preview in the frame to the right </span><span class="icon frame"></span></a>
          <p>Resource not working properly in the frame?</p>
          <a href="javascript:previewInPopup()" class="previewLink"><span class="text">Preview in a popup window </span><span class="icon popup"></span></a>
        </div>
      </div>

      <div class="pageItem" id="url_language">
        <h3 class="isleH3_Block required">What language is the Resource in?</h3>
        <uc1:ListPanel ID="lpLanguage" CssClass="ddlLanguage" runat="server" TargetTable="language" ListMode="dropdown" />
      </div>

      <div class="pageItem" id="url_creatorPublisher">
        <h3 class="isleH3_Block">If you can, tell us a bit more:</h3>
        <p>Who created this Resource?</p>
        <input type="text" placeholder="Resource Creator" runat="server" class="txtCreator" id="txtCreator" />
        <p>Who publishes, hosts, or otherwise makes available this Resource?</p>
        <input type="text" placeholder="Resource Publisher" runat="server" class="txtPublisher" id="txtPublisher" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('welcome')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="validatePage('url','basicInfo')" id="btnProgress_url" />
      </div>
    </div>

    <div class="page" id="page_basicInfo">
      
      <div class="pageItem">
        <h2 class="isleH2">Basic Information</h2>
        <h3 class="isleH3_Block required">Title</h3>
        <p>What's the name of this Resource?</p>
        <input type="text" placeholder="Title" runat="server" class="txtTitle" id="txtTitle" />

        <h3 class="isleH3_Block required">Description</h3>
        <p>Please give us a few paragraphs about the Resource.</p>
        <textarea placeholder="Description" runat="server" class="txtDescription" id="txtDescription"></textarea>

        <h3 class="isleH3_Block">Subjects</h3>
        <p>What kinds of educational subjects does this Resource relate to?<br />Select from the list below and/or add your own.<br />Press Enter or use commas to separate subjects:</p>
        <a class="quickSubject" href="javascript:void('')">Mathematics</a>
        <a class="quickSubject" href="javascript:void('')">English Language Arts</a>
        <a class="quickSubject" href="javascript:void('')">Science</a>
        <a class="quickSubject" href="javascript:void('')">Social Studies</a>
        <a class="quickSubject" href="javascript:void('')">Arts</a>
        <a class="quickSubject" href="javascript:void('')">Technology</a>
        <a class="quickSubject" href="javascript:void('')">World Languages</a>
        <a class="quickSubject" href="javascript:void('')">Health</a>
        <a class="quickSubject" href="javascript:void('')">Physical Education</a>
        <input type="text" id="tagEntry_subjects" maxlength="50" class="txtBox tagEntry" placeholder="Enter Subjects separated by pressing Enter or with a comma" />
        <ul class="tagList" id="tags_subjects"></ul>

        <h3 class="isleH3_Block">Keywords</h3>
        <p>Enter keywords that will help people find this Resource.<br />Press Enter or use commas to separate keywords:</p>
        <input type="text" id="tagEntry_keywords" maxlength="50" class="txtBox tagEntry" placeholder="Enter Keywords separated by pressing Enter or with a comma" />
        <ul class="tagList" id="tags_keywords"></ul>
      </div>

      <div class="buttons">
      <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('url')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="validatePage('basicInfo','copyrightInfo')" />
      </div>

    </div>

    <div class="page" id="page_copyrightInfo">
      <div class="pageItem">
        <h2 class="isleH2">Copyright Information</h2>
        <p>What are the licensing and legal constraints tied to this Resource?</p>
        <h3 class="isleH3_Block required">Usage Rights</h3>
        <p>Usage Rights define how someone else can use or re-use the Resource:</p>
        <uc1:ConditionsOfUseSelector id="conditionsSelector" runat="server" />

        <h3 class="isleH3_Block required">Access Rights</h3>
        <p>Access Rights indicate whether or not a cost is associated with the Resource:</p>
        <uc1:ListPanel ID="lpAccessRights" runat="server" CssClass="ddlAccessRights" TargetTable="accessRights" ListMode="dropdown" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('basicInfo')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="validatePage('copyrightInfo','endUser')" />
      </div>

    </div>

    <div class="page" id="page_endUser">

      <div class="pageItem">
        <h2 class="isleH2 required">End User</h2>
        <p>What is the intended audience of this Resource?</p>
        <uc1:ListPanel ID="lpEndUser" runat="server" TargetTable="endUser" CssClass="cbxl_intendedAudience cbxl" UpdateMode="raw" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('copyrightInfo')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="validatePage('endUser','resourceTypes')" />
      </div>

    </div>

    <div class="page" id="page_resourceTypes">

      <div class="pageItem">
        <h2 class="isleH2 required">Resource Type</h2>
        <p>What type(s) of Resource are we tagging?</p>
        <uc1:ListPanel ID="lpResourceType" runat="server" TargetTable="resourceType" CssClass="cbxl_resourceTypes cbxl" UpdateMode="raw" />
      </div>

      <div class="isleBox" id="flyout_additionalInfo" style="display:none;">
        <h2 class="isleBox_H2">Additional Information</h2>

        <div class="additionalInfo_itemType">
          <h2 class="isleH2 required">Item Type</h2>
          <p>You must select the type of Assessment Item:</p>
          <uc1:ListPanel ID="lpItemType" CssClass="ddlItemType" runat="server" ListMode="dropdown" TargetTable="itemType" UpdateMode="raw" />
        </div>

        <div class="additionalInfo_assessmentType">
          <h2 class="isleH2">Assessment Type</h2>
          <p>Can you tell us what type of assessment the Resource is?</p>
          <%-- Leaving these in here for now since we need to expand the length of the varchar in the table to hold the full length descriptions --%>
          <asp:RadioButtonList ID="rblAssessmentType" runat="server" RepeatLayout="UnorderedList" CssClass="cbxl_assessmentType cbxl" Visible="false">
            <asp:ListItem Text="Not Applicable" Value="0" Selected="True" id="assessmentTypeNotApplicable"></asp:ListItem>
            <asp:ListItem Text="Type I" Value="1" class="toolTipLink" id="aTypeI2" title="Type I|A reliable assessment that measures a certain group or subset of students in the same manner with the same potential assessment items, is scored by a non-district entity, and is administered either statewide or beyond Illinois.|Examples include assessments available from the Northwest Evaluation Association (NWEA), Scantron Performance Series, Star Reading Enterprise, College Board’s SAT, Advanced Placement or International Baccalaureate examinations, or ACT’s EPAS® (i.e., Educational Planning and Assessment System)."></asp:ListItem>
            <asp:ListItem Text="Type II" Value="2" class="toolTipLink" id="aTypeII2" title="Type II|Any assessment developed or adopted and approved for use by the school district and used on a district- wide basis by all teachers in a given grade or subject area.|Examples include collaboratively developed common assessments, curriculum tests and assessments designed by textbook publishers."></asp:ListItem>
            <asp:ListItem Text="Type III" Value="3" class="toolTipLink" id="aTypeIII2" title="Type III|Any assessment that is rigorous, that is aligned to the course’s curriculum, and that the qualified evaluator and teacher determine measures student learning in that course.|Examples include teacher-created assessments, assessments designed by textbook publishers, student work samples or portfolios, assessments of student performance, and assessments designed by staff who are subject or grade-level experts that are administered commonly across a given grade or subject.|A Type I or Type II assessment may qualify as a Type III assessment if it aligned to the curriculum being taught and measures student learning in that subject area."></asp:ListItem>
          </asp:RadioButtonList>
          <uc1:ListPanel ID="lpAssessmentType" runat="server" CssClass="cbxl_assessmentType cbxl" ListMode="radio" UseBlankDefault="true" TargetTable="assessmentType" UpdateMode="raw" />
        </div>
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('endUser')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="validatePage('resourceTypes','mediaTypes')" />
      </div>

    </div>

    <div class="page" id="page_mediaTypes">

      <div class="pageItem">
        <h2 class="isleH2 required">Media Type</h2>
        <p>What kind(s) of file or object does this Resource primarily come in?</p>
        <uc1:ListPanel ID="lpMediaType" runat="server" TargetTable="mediaType" CssClass="cbxl_resourceFormats cbxl" UpdateMode="raw" />
      </div>

      <div class="pageItem">
        <h2 class="isleH2">Educational Use</h2>
        <p>How is this Resource meant to be used?</p>
        <uc1:ListPanel ID="lpEducationalUse" runat="server" TargetTable="educationalUse" CssClass="cbxl_educationalUse cbxl" UpdateMode="raw" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('resourceTypes')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="validatePage('mediaTypes','standards')" />
      </div>

    </div>

    <div class="page" id="page_standards">

      <div class="pageItem">
        <h2 class="isleH2">Learning Standard</h2>
        <p>Does this Resource align to any Learning Standards? Select all that apply:</p>
        <uc1:StandardsBrowserV5 ID="standardsBrowserNew" runat="server" mode="tag" isWidget="false" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('mediaTypes')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="showPage('gradeLevels')" />
      </div>

    </div>

    <div class="page" id="page_gradeLevels">

      <div class="pageItem">
        <h2 class="isleH2">Grade Level</h2>
        <p>What grade level is this Resource meant for?</p>
        <uc1:ListPanel ID="lpGradeLevels" runat="server" TargetTable="gradeLevel" CssClass="cbxl_gradeLevels cbxl" UpdateMode="raw" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('standards')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="showPage('careerClusters')" />
      </div>

    </div>

    <div class="page" id="page_careerClusters">

      <div class="pageItem">
        <h2 class="isleH2">Career Cluster</h2>
        <p>Does this Resource apply to any major Career Clusters?</p>
        <uc1:ListPanel ID="lpCareerClusters" runat="server" TargetTable="careerCluster" CssClass="cbxl_careerClusters cbxl" UpdateMode="raw" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('gradeLevels')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="showPage('groupTypes')" />
      </div>

    </div>

    <div class="page" id="page_groupTypes">

      <div class="pageItem">
        <h2 class="isleH2">Group Type</h2>
        <p>How many people should use this Resource at a time?</p>
        <uc1:ListPanel ID="lpGroupType" runat="server" TargetTable="groupType" CssClass="cbxl_groupTypes cbxl" UpdateMode="raw" />
      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('careerClusters')" />
        <input type="button" class="progressButton" value="What's next? &rarr;" onclick="showPage('extraInfo')" />
      </div>

    </div>

    <div class="page" id="page_extraInfo">

      <div class="pageItem">
        <h2 class="isleH2">Extra Information</h2>
        <p>We're almost done! If there's anything else you can tell us about the Resource, please do so below. Remember, the more information you give us, the easier it will be for others to find your Resource!</p>

        <h3 class="isleH3_Block">Date Created</h3>
        <p>When was the Resource originally created?</p>
        <input type="text" placeholder="MM/DD/YYYY" id="txtDateCreated" runat="server" class="txtDateCreated" />

        <h3 class="isleH3_Block">Original Version URL</h3>
        <p>If this Resource is a derivative of another Resource, where can others find the original Resource?</p>
        <input type="text" placeholder="Original Version URL" id="txtRelatedURL" class="txtRelatedURL" runat="server" />

        <h3 class="isleH3_Block">Instructions &amp; Equipment Requirements</h3>
        <p>Please describe any unique hardware, equipment, tools, goods, or instructions needed to use this Resource:</p>
        <input type="text" placeholder="Instructions & Equipment Requirements" class="txtRequirements" id="txtRequirements" runat="server" />

        <h3 class="isleH3_Block">Time Required</h3>
        <p>How long does it take, on average, to use this Resource?</p>
        <div style="text-align: center;">
          <input type="text" id="txtTimeTakenDays" class="timeTaken txtTimeTakenDays" runat="server" value="0" /> Days
          <asp:DropDownList ID="ddlTimeTakenHours" CssClass="timeTaken ddlTimeTakenHours" runat="server"></asp:DropDownList> Hours
          <asp:DropDownList ID="ddlTimeTakenMinutes" CssClass="timeTaken ddlTimeTakenMinutes" runat="server"></asp:DropDownList> Minutes
        </div>

      </div>

      <div class="buttons">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('groupTypes')" />
        <input type="button" class="progressButton" value="Let's Review &rarr;" onclick="showPage('review')" />
      </div>

    </div>

    <div class="page" id="page_review">
      <div class="buttons" style="height: 35px;">
        <input type="button" class="progressButton left" value="&larr; Go Back" onclick="showPage('extraInfo')" />
      </div>

      <h2 class="isleH2">Review</h2>
      <p>Please double-check your work. You can jump back to a previous page via the button above. When you are happy with your tags, click the Publish Tags button. Publishing Tags takes a just a few moments, and then you will be able to see your finished, tagged Resource.</p>
      <p class="required">Please note: Once you finish tagging your Resource, many of the tags you have entered will be permanently associated with it! Be sure you're happy with your work.</p>
      <input type="button" class="progressButton navigationLink publishButton" value="I'm completely finished. Tag my Resource!" onclick="setProgress(100);publish()" runat="server" id="publishButton" />
      <div class="pageItem reviewStuff">
        <h3 class="isleH3_Block required">Resource URL</h3>
        <div class="review_text" id="review_ResourceURL"></div>

        <h3 class="isleH3_Block required">Language</h3>
        <div class="review_ddl" id="review_Language"></div>

        <h3 class="isleH3_Block">Resource Creator</h3>
        <div class="review_text" id="review_Creator"></div>

        <h3 class="isleH3_Block">Resource Publisher</h3>
        <div class="review_text" id="review_Publisher"></div>

        <h3 class="isleH3_Block required">Title</h3>
        <div class="review_text" id="review_Title"></div>

        <h3 class="isleH3_Block required">Description</h3>
        <div class="review_text" id="review_Description"></div>

        <h3 class="isleH3_Block">Subjects</h3>
        <ul class="review_tags tagList" id="review_subjects"></ul>

        <h3 class="isleH3_Block">Keywords</h3>
        <ul class="review_tags tagList" id="review_keywords"></ul>

        <h3 class="isleH3_Block required">Usage Rights</h3>
        <div class="review_text" id="review_ConditionsOfUse"></div>

        <h3 class="isleH3_Block required">Access Rights</h3>
        <div class="review_ddl" id="review_AccessRights"></div>

        <h3 class="isleH3_Block required">End User</h3>
        <ul class="review_cbxl" id="review_intendedAudience"></ul>

        <h3 class="isleH3_Block">Learning Standards</h3>
        <ul class="review_standards" id="review_standards"></ul>

        <h3 class="isleH3_Block">Grade Levels</h3>
        <ul class="review_cbxl" id="review_gradeLevels"></ul>

        <h3 class="isleH3_Block">Career Clusters</h3>
        <ul class="review_cbxl" id="review_careerClusters"></ul>

        <h3 class="isleH3_Block required">Resource Types</h3>
        <ul class="review_cbxl" id="review_resourceTypes"></ul>

        <h3 class="isleH3_Block required">Media Types</h3>
        <ul class="review_cbxl" id="review_resourceFormats"></ul>

        <h3 class="isleH3_Block">Group Types</h3>
        <ul class="review_cbxl" id="review_groupTypes"></ul>

        <h3 class="isleH3_Block">Date Created</h3>
        <div class="review_text" id="review_DateCreated"></div>

        <h3 class="isleH3_Block">Original Version URL</h3>
        <div class="review_text" id="review_RelatedURL"></div>

        <h3 class="isleH3_Block">Instructions &amp; Equipment Requirements</h3>
        <div class="review_text" id="review_Requirements"></div>

        <h3 class="isleH3_Block">Time Required</h3>
        <div class="review_time" id="review_TimeRequired"></div>
      </div>
    </div>


  </div><!-- end tool container -->

  <iframe src="" id="previewFrame" frameborder="0"></iframe>

  <div class="isleBox flyout" id="pageMessagePanel" style="display: none;">
    <h2 class="isleBox_H2">Message</h2>
    <div id="pageMessage"><%=returnError %></div>
    <%-- <div id="temp"></div>--%>
  </div>

  <div class="overlayContainer" id="overlayContainer" style="display:none;" justpublished="<%=justPublished %>">
    <div class="overlayCover"></div>
    <div class="overlayWrapper">
      <div class="overlayBox isleBox">
        <h1 class="isleH1" style="margin-top:5px;"><%=publishedMessage%></h1>
        <%=additionalMessage%>
        <p>Your Resource has been published. Select an action:</p>
        <a href="/Publish.aspx">Tag another Resource</a>
        <a href="/ResourceDetail.aspx?vid=<%=returnMessage %>">View your newly-tagged Resource</a>
      </div>
    </div>
  </div>

</div><!-- end main page container -->

<input type="hidden" id="hdnKeywords" runat="server" />
<input type="hidden" id="hdnSubjects" runat="server" />
<input type="hidden" id="hdnStandards" runat="server" />

<script>
    (function (i, s, o, g, r, a, m) {
        i['GoogleAnalyticsObject'] = r; i[r] = i[r] || function () {
            (i[r].q = i[r].q || []).push(arguments)
        }, i[r].l = 1 * new Date(); a = s.createElement(o),
      m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m)
    })(window, document, 'script', '//www.google-analytics.com/analytics.js', 'ga');

    ga('create', 'UA-42065465-1', 'ilsharedlearning.org');
    ga('send', 'pageview');

</script>

<asp:Panel ID="HiddenStuff" runat="server" Visible="false">
  <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">Isle.Controls.CanPublish</asp:Literal>
  <asp:Literal ID="txtContentSourceId" runat="server" Visible="false">0</asp:Literal>
  <asp:Literal ID="txtDoingLRPublish" runat="server" Visible="false">yes</asp:Literal>
  <asp:Literal ID="txtRequiresApproval" runat="server" Visible="false">no</asp:Literal>
  <asp:Literal ID="ltl_ddlLanguagesQuery" runat="server">SELECT [Id], [Title] FROM [Codes.Language] WHERE [IsPathwaysLanguage] = 1</asp:Literal>
  <asp:Literal ID="ltl_ddlAccessRightsQuery" runat="server">SELECT [Id], [Title] FROM [Codes.AccessRights] WHERE [IsActive] = 1</asp:Literal>
  <asp:Literal ID="ltl_ddlItemTypeQuery" runat="server">SELECT [Id], [Title] FROM [Codes.ItemType] WHERE [IsActive] = 1</asp:Literal>
  <asp:Literal ID="ltlStandardSeparator" runat="server">|%|</asp:Literal>
  <asp:Literal ID="ltlStandardDataSeparator" runat="server">|~|</asp:Literal>
  <%--<asp:Literal ID="ltlConditionsOfUseDefaultValue" runat="server">4</asp:Literal>--%>
  <asp:Literal ID="ltl_minimumLengths" runat="server">4,25,12</asp:Literal>
  <asp:Literal ID="ltl_errorTemplate" runat="server"><div class="messageError pageProblem">{0}</div></asp:Literal>
</asp:Panel>
