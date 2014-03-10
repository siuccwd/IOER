<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StandardsBrowser5.ascx.cs" Inherits="ILPathways.LRW.controls.StandardsBrowser5" %>

<!-- Standards Browser v5 JS -->
<!-- From Server -->
<script type="text/javascript">
    var mode = "<%=mode %>";
    //var mode = "tag";
    var preselectedGrade = "<%=preselectedGrade %>";
    var preselectedBody = "<%=preselectedBody %>";
    var isWidget = "<%=isWidget %>";
</script>
<!-- variables -->
<script type="text/javascript" src="/Scripts/standards/jsonMath-trimmed.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonELA-trimmed.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonNGSS-trimmed.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILFineArts.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILPhysicalDevelopment.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILSocialScience.js"></script>
<!--<script type="text/javascript" src="/Scripts/standards/jsonAB.js"></script>-->
<script type="text/javascript" src="/Scripts/standards/xframeCommunication.js"></script>
<script type="text/javascript">
    var standardBodies = [jsonMath, jsonELA, jsonNGSS, jsonILFineArts, jsonILPhysicalDevelopment, jsonILSocialScience];
    var gradeLevels = ["K", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12"];
    var currentBody = [];
    var currentGrade = null;
    var activeLayer = {};
    var sizeMode = "large";
    var sizeModeCheck = null;
    var sizeCutoffLarge = 750;
    var sizeCutoffMedium = 525;
    var widgetQuery = "";
</script>
<!-- initialization -->
<script type="text/javascript">
    $(document).ready(function () {
        //Report preselected items
        console.log("Mode: " + mode);
        console.log("Preselected Body: " + preselectedBody);
        console.log("Preselected Grade: " + preselectedGrade);
        console.log("Widget: " + isWidget);

        //Load grade levels
        loadGradeLevels();
        //Select body
        $("#ddlStandardBody").on("change", function () { selectStandardBody(); });
        //Select domain
        $("#ddlDomain").on("change", function () { populateTree($(this).find("option:selected").attr("value")); renderSearchSelections(); });
        //hide alignment in non-tag mode
        if (mode != "tag") {
            $(".appliedAlignmentType").remove();
        }
        //hide searching/selections in browse mode
        if (mode == "browse") {
            $("#searchAt").remove();
            $("#selectedStandards").remove();
        }

        //Handle resizing
        $(window).resize(function () { resizeStandardsBrowser($("#standardsBrowser").width()); });

        //Do initial hiding
        $("#grades").attr("data-hidden", "true");
        $("#sbLeftColumn").attr("data-hidden", "true");
        $("#sbRightColumn").attr("data-hidden", "true");
        $("#selectedStandards").attr("data-hidden", "true");

        //Preselect stuff
        if (preselectedBody != "") {
            $("#ddlStandardBody option").attr("selected", "false");
            $("#ddlStandardBody option[value=" + preselectedBody + "]").attr("selected", "selected");
            $("#ddlStandardBody").trigger("change");
        }
        if (preselectedGrade != "") {
            $("#grades a").attr("data-selected", "false");
            $("#grades a[data-grade=" + preselectedGrade + "]").attr("selected", "selected").trigger("click");
        }

        setTimeout(function () { $(window).trigger("resize"); }, 1000);

    });
</script>
<!-- Browsing -->
<script type="text/javascript">
    //Get an item from the current body
    function getLayer(target) {
        if (target == "first") {
            return currentBody[0];
        }
        else {
            for (i in currentBody) {
                if (currentBody[i].id == target) {
                    return currentBody[i];
                }
            }
            //Not found. search all of the standards
            for (i in standardBodies) {
                var body = standardBodies[i];
                for (j in body) {
                    if (body[j].id == target) {
                        return body[j];
                    }
                }
            }
        }
    }

    //Get an array of items from the current body
    function getLayers(ids) {
        var returner = [];
        for (i in ids) {
            returner.push(getLayer(ids[i]));
        }
        return returner;
    }

    //Get an array of items from the current body at the current grade level
    function getLayersAtGradeLevel(ids, grade) {
        var temp = getLayers(ids);
        var returner = [];
        for (i in temp) {
            var item = temp[i];
            if (arrayContains(item.grades, grade)) {
                returner.push(item);
            }
        }
        return returner;
    }

    //Check an array for an item
    function arrayContains(array, target) {
        for (i in array) {
            if (target == array[i]) {
                return true;
            }
        }
        return false;
    }

    //Change which standard body is currently in use
    function selectStandardBody() {
        var target = $("#ddlStandardBody option:selected").attr("value")
        currentBody = window[target];
        activeLayer = getLayer("first");
        $("span#sbBodyTitle").html(getText(activeLayer));
        populateTree(activeLayer.id);
        renderSearchSelections();
        if (currentGrade == null) {
            $("#grades").attr("data-hidden", "false");
            $("#sbLeftColumn").attr("data-hidden", "true");
            $("#sbRightColumn").attr("data-hidden", "true");
            $("#selectedStandards").attr("data-hidden", $("#selectedStandards .selectedStandard").length > 0 ? "false" : "true");
            doResize();
        }
        else {
            var selectedItem = $("#ddlDomain option:not(:first()):selected");
            var oldDomain = selectedItem.attr("value");
            var oldDomainText = selectedItem.text();
            loadDomainDDL(activeLayer.children);
            $("#grades").attr("data-hidden", "false");
            $("#sbLeftColumn").attr("data-hidden", "false");
            $("#sbRightColumn").attr("data-hidden", "true");
            $("#selectedStandards").attr("data-hidden", $("#selectedStandards .selectedStandard").length > 0 ? "false" : "true");
            //Attempt to keep the same domain for a different grade level
            var findTarget = $("#ddlDomain").find("option[value=" + oldDomain + "]");
            var findAltTarget = $("#ddlDomain").find("option:contains(" + oldDomainText + ")");
            if (findTarget.length > 0) {
                $("#ddlDomain option").attr("selected", false);
                findTarget.attr("selected", "selected");
                $("#ddlDomain").trigger("change");
            }
            else if (findAltTarget.length > 0 && oldDomainText.length > 0) {
                $("#ddlDomain option").attr("selected", false);
                findAltTarget.attr("selected", "selected");
                $("#ddlDomain").trigger("change");
            }
            doResize();
        }
    }

    //Change which grade level is currently in use
    function chooseGrade(grade) {
        $("#grades a").attr("data-selected", "false");
        $("#grades a[data-grade=" + grade + "]").attr("data-selected", "true");
        currentGrade = grade;
        selectStandardBody();
        return false;
    }

    //Populate the tree for a given domain
    function populateTree(targetID) {
        if (targetID == "none" || targetID == null) {
            $("#treeBrowser").html("");
            return;
        }
        var numID = parseInt(targetID);
        fillOutDetails(getLayer(numID));
        $("#treeBrowser").html("");
        $("#treeBrowser").html(doTreecursion(numID, 0, []));
        $("#treeBrowser .sbTreeItem:contains('Students who demonstrate understanding can')").unbind().hide();

        //Clicking on things
        $(".sbTreeItem").on("click keyup", function (event) { selectTreeItems($(this), event); });
    }
    //Fill out the tree
    function doTreecursion(id, depth) {
        var current = getLayer(id);
        if (!arrayContains(current.grades, currentGrade)) { return ""; }
        if (depth > 1 && getCode(current) == "") { return ""; }
        var tempCode = getCode(current);
        var filteredCode = depth > 2 ? tempCode.substr(tempCode.length - 1) : tempCode;
        var layerText = "";
        for (i in current.children) {
            layerText += doTreecursion(current.children[i], depth + 1);
        }
        return $("#sbTemplate_treeItem").html()
          .replace(/{indent}/g, depth)
          .replace(/{id}/g, id)
          .replace(/{code}/g, filteredCode)
          .replace(/{text}/g, depth == 0 ? current.description : getText(current))
          .replace(/{content}/g, layerText)
          .replace(/{tab}/g, "tabindex=\"0\"")
    }

    //React to user selections in the tree
    function selectTreeItems(item, event) {
        if (event.type == "click" || (event.type == "keyup" && event.which == 13)) {
            event.stopPropagation();
            $(".sbTreeItem").attr("data-selected", "false");
            item.attr("data-selected", "true");
            var temp = item;
            while (true) {
                temp.parent().attr("data-selected", "true");
                if (temp.parent().is("#treeBrowser")) {
                    break;
                }
                else {
                    temp = temp.parent();
                }
            }
            //item.parentsUntil("#treeBrowser").attr("data-selected","true");
            renderSearchSelections();
            fillOutDetails(getLayer(parseInt(item.attr("data-id"))));
        }
    }
</script>
<!-- Rendering -->
<script type="text/javascript">
    //Grade Levels
    function loadGradeLevels() {
        $("#grades").html("");
        for (i in gradeLevels) {
            $("#grades").append(
              $("#sbTemplate_gradeLink").html()
                .replace(/{grade}/g, gradeLevels[i])
            );
        }
    }

    //Domain DDL
    function loadDomainDDL(domains) {
        $("#ddlDomain").html("<option selected=\"selected\" value=\"none\">Select Domain...</option>");
        var temp = getLayersAtGradeLevel(domains, currentGrade);
        for (i in temp) {
            $("#ddlDomain").append("<option value=\"" + temp[i].id + "\">" + temp[i].description + "</option>");
        }
    }

    //Search Selections
    function renderSearchSelections() {
        //disable this
        return;
        if (mode == "search" || mode == "tag") {
            $("#searchAtOptions").html("");

            //Body
            if (currentBody != []) {
                //var temp = getLayer("first");
                //addSearchAtLink(temp, false);
            }
            else { return; }

            //Body with Grade
            /*if (currentGrade != null) {
              var temp = getLayer("first");
              addSearchAtLink(temp, true);
            }
            else { return; }*/

            //Domain
            /*var selectedDomain = $("#ddlDomain option:selected");
            if (selectedDomain.length > 0) {
              var temp = getLayer(parseInt(selectedDomain.attr("value")));
              addSearchAtLink(temp, false);
            }
            else { return; }*/

            //Tree
            /*$("#treeBrowser div.sbTreeItem[data-selected=true]").each(function() {
              var temp = getLayer(parseInt($(this).attr("data-id")));
              addSearchAtLink(temp, false);
            });*/

            //Tree
            try {
                var starter = $("#treeBrowser div.sbTreeItem[data-selected=true]").last();
                var depth = parseInt(starter.attr("data-depth").substr(1));
                var items = [];
                items.push(getLayer(parseInt(starter.attr("data-id"))));
                while (depth > 0) {
                    depth--;
                    var next = starter.prevAll("[data-depth=d" + depth + "]").first();
                    var toInsert = getLayer(parseInt(next.attr("data-id")));
                    if (toInsert.description == "Students who demonstrate understanding can:") { continue; }
                    items.push(toInsert);
                }
                items.reverse();
                for (i in items) {
                    addSearchAtLink(items[i], false);
                }
            }
            catch (e) { }

            //Indentation
            var indent = 8;
            $("#searchAt #searchAtOptions a").each(function () {
                $(this).css("padding-left", indent + "px");
                indent = indent + 8;
            });

            if ($("#searchAtOptions a").length == 0) {
                $("#searchAt").hide();
            }
            else {
                $("#searchAt").show();
            }

        }
        else {
            return;
        }
    }

    //Add a "search for/at" link
    function addSearchAtLink(item, plusGrade) {
        $("#searchAtOptions").append(
          $("#sbTemplate_searchForLink").html()
            .replace(/{id}/g, item.id)
            .replace(/{text}/g, getText(item) + (plusGrade ? " at Grade " + currentGrade : ""))
        );
    }

    //Get short text/title to display
    function getText(item) {
        return item.description;
        if (item.description.length > 50) { return item.description.substring(0, 47) + "..."; }
        else { return item.description; }
    }

    //Get code to display, if any
    function getCode(item) {
        return (item.code == "null" || item.code == null) ? "" : item.code;
    }

    //Fill out the details for a given item
    function fillOutDetails(item) {
        activeLayer = item;
        $("#sbRightColumn").attr("data-hidden", "false");
        var ddlSelection = $("#ddlDomain option:selected").html();
        var domainText = (ddlSelection == undefined ? "" : ddlSelection);
        $("#standardDetails").html(
          $("#sbTemplate_details").html()
            .replace(/{standardCode}/g, getCode(item))
            .replace(/{standardDomain}/g, ddlSelection)
            .replace(/{gradesList}/g, getGradesText(item.grades))
            //.replace(/{treeInfo}/g, getTreeInfo())
            .replace(/{treeInfo}/g, "")
            .replace(/{description}/g, item.description)
            .replace(/{url}/g, item.url)
        );
        doResize();
    }

    //Get the grades list in a textual format
    function getGradesText(list) {
        var returner = "";
        for (i in list) {
            returner += ", " + list[i];
        }
        return returner.substring(2);
    }

    //Get critical display info for the current tree
    function getTreeInfo() {
        var returner = "";
        var counter = 0;
        var list = $(".sbTreeItem[data-selected=true]");
        list.each(function () {
            if (counter < list.length - 1) {
                returner += $(this).find("> .sbTitle .text").text() + "<br /><br />";
                counter++;
            }
        });
        return returner;
    }

    //Apply a selected standard
    function applyStandard(id, alignmentID, alignmentText) {
        $("#selectedStandards").attr("data-hidden", "false").css("display", "block");
        var item = getLayer(id);
        if ($("#selectedStandards .selectedStandard[data-id=" + id + "]").length == 0) {
            $("#selectedStandards").append(
              $("#sbTemplate_selectedStandard").html()
                .replace(/{id}/g, item.id)
                .replace(/{code}/g, getCode(item))
                .replace(/{alignmentID}/g, alignmentID)
                .replace(/{alignmentText}/g, alignmentText)
                .replace(/{text}/g, getText(item))
            );
        }
        doExternalApplication();
        setTimeout(function () { doResize(); }, 250);
        return false;
    }

    //Remove a selected standard
    function removeStandard(id) {
        $("#selectedStandards .selectedStandard[data-id=" + id + "]").remove();
        doExternalApplication();
        return false;
    }

    //Trigger external stuff
    function doExternalApplication() {
        //Get the data for all of the children of each selected item
        var appliedStandardIDs = [];
        var moreIDs = [];
        $("#selectedStandards .selectedStandard").each(function () {
            var item = getLayer(parseInt($(this).attr("data-id")));
            if (mode != "tag") { moreIDs = getChildrenIDs(item.id, []); }
          try {
                appliedStandardIDs.push({
                    code: ((item.code == "" || item.code == null) ? item.description : item.code),
                    id: item.id
                });
            }
            catch (e) { }
        });
        //If running as a widget, send only the IDs to the search
        if (isWidget.toLowerCase() == "true") {
            widgetQuery = "";
            for (i in moreIDs) {
                widgetQuery += "," + moreIDs[i].id;
            }
            widgetQuery = (widgetQuery.length > 0 ? widgetQuery.substr(1) : "");

            //Linking
            $("#searchLink").html("");
            if (widgetQuery.length > 0) {
                $("#searchLink").html("<a href=\"http://ioer.ilsharedlearning.org/Search.aspx?sids=" + widgetQuery + "\" target=\"_blank\">Click Here to search for the items selected below</a>");
            }

        }
            //Otherwise, call the function on the search page
        else {
            if (typeof (external_applyStandardsV5) == "function") {
                external_applyStandardsV5(appliedStandardIDs, moreIDs);
            }
        }
    }

    function getChildrenIDs(itemID, list) {
      var item = getLayer(itemID);
      for (i in item.grades) {
        if (item.grades[i] == currentGrade) {
          list.push({ text: "", id: item.id });
          for (j in item.children) {
              list = getChildrenIDs(item.children[j], list);
          }
        }
      }
        return list;
    }

    function selectCurrentItem() {
        applyStandard(activeLayer.id, 0, "");
    }

</script>
<!-- Responsive JS -->
<script type="text/javascript">
    function resizeStandardsBrowser(containerWidth) {
        sizeMode = (containerWidth >= sizeCutoffLarge ? "large" : containerWidth >= sizeCutoffMedium ? "medium" : "small");
        if (sizeModeCheck != sizeMode) {
            sizeModeCheck = sizeMode;
            doResize();
        }
    }

    function doResize() {
        if (isWidget == true || isWidget == "True" || isWidget == "true") {
            $("body").trigger("afterContentChange");
            setTimeout(function () {
                resizeIframePipe(null, null);
            }, 400);
        }
        switch (sizeMode) {
            case "large":
                $("#ddlStandardBody").css({ "width": "29%", "display": "inline-block" });
                $("#grades").css({ "width": "69%", "display": "inline-block", "margin-top": "0px" });
                $("#grades a").css({ "width": "7.5%", "margin-bottom": "1px" });
                $("#grades a:first-child").css({ "border-top-left-radius": "5px", "border-bottom-left-radius": "5px" });
                $("#grades a:last-child").css({ "border-top-right-radius": "5px", "border-bottom-right-radius": "5px" });
                $("#sbLeftColumn, #sbRightColumn").css({ "display": "inline-block", "width": "49%" });
                $("#treeBrowser").css({ "max-height": "450px" });
                //$("#selectedStandards .selectedStandard .text").css({ "margin-top": "0px", "padding-left": (mode == "tag" ? "135px" : "5px"), "padding-right": "200px" });

                break;
            case "medium":
                $("#ddlStandardBody").css({ "width": "100%", "display": "block" });
                $("#grades").css({ "width": "100%", "display": "block", "margin-top": "5px" });
                $("#grades a").css({ "width": "7.5%", "margin-bottom": "1px" });
                $("#grades a:first-child").css({ "border-top-left-radius": "5px", "border-bottom-left-radius": "5px" });
                $("#grades a:last-child").css({ "border-top-right-radius": "5px", "border-bottom-right-radius": "5px" });
                $("#sbLeftColumn, #sbRightColumn").css({ "display": "inline-block", "width": "49%" });
                $("#treeBrowser").css({ "max-height": "450px" });
                //$("#selectedStandards .selectedStandard .text").css({ "margin-top": "1.75em", "padding-left": "5px", "padding-right": "0" });

                break;
            case "small":
                $("#ddlStandardBody").css({ "width": "100%", "display": "block" });
                $("#grades").css({ "width": "100%", "display": "block", "margin-top": "5px" });
                $("#grades a").css({ "width": "13.5%", "margin-bottom": "1px" });
                $("#grades a:first-child").css({ "border-top-left-radius": "0px", "border-bottom-left-radius": "0px" });
                $("#grades a:last-child").css({ "border-top-right-radius": "0px", "border-bottom-right-radius": "0px" });
                $("#sbLeftColumn, #sbRightColumn").css({ "display": "block", "width": "100%" });
                $("#treeBrowser").css({ "max-height": "300px" });
                //$("#selectedStandards .selectedStandard .text").css({ "margin-top": "1.75em", "padding-left": "5px", "width": "100%", "padding-right": "0" });

                break;
            default:
                break;
        }

        $("#standardsBrowser *[data-hidden=true]").hide();
        $("#standardsBrowser *[data-hidden=false]").show();
    }
</script>

<!-- Standards Browser v5 CSS -->
<style type="text/css">
  #standardsBrowser { min-width: 275px; padding-bottom: 50px; }
  #standardsBrowser * { box-sizing: border-box; -moz-box-sizing: border-box; }
  #standardsBrowser h2 { font-size: 22px }
  #standardsBrowser h3 { font-size: 18px; }
  #standardsBrowser h2 *, #standardsBrowser h3 * { font-size: inherit; }
  #searchAt #searchAtOptions a { display: block; }
  #sbLeftColumn, #sbRightColumn { display: inline-block; vertical-align: top; width: 49%; min-width: 200px; }
  #sbRightColumn { padding: 0 5px 5px 5px; }
  #sbRightColumn a:hover, #sbRightColumn a:focus { text-decoration: underline; }
  #treeBrowser { max-height: 450px; overflow-x: hidden; overflow-y: scroll; background-color: #DFDFDF; }
  #ddlDomain { width: 100%; }
  #ddlStandardBody, #sbTopBar #grades { display: inline-block; vertical-align: top; }
  #ddlStandardBody { width: 29%; min-width: 200px; }
  #sbTopBar #grades { width: 69%; min-width: 200px; font-size: 0; }
  #sbTopBar #grades a { display: inline-block; vertical-align: top; font-size: 18px; line-height: 25px; height: 25px; font-weight: bold; min-width: 30px; text-align: center; background-color:  #3572B8; color: #FFF; margin-right: 1px; width: 7.5%; }
  #sbTopBar #grades a:first-child { border-top-left-radius: 5px; border-bottom-left-radius: 5px; }
  #sbTopbar #grades a:last-child { border-top-right-radius: 5px; border-bottom-right-radius: 5px; }
  #sbTopBar #grades a:hover, #sbTopBar #grades a:focus { background-color: #FF5707; }
  #sbTopBar #grades a[data-selected=true] { background-color: #4AA394; }
  .selectedStandard { border: 1px solid #C00; border-radius: 5px; margin: 1px; padding: 2px; position: relative; min-height: 30px; }
  .selectedStandard a { background-color: #C00; border-radius: 5px; display: block; color: #EEE; font-weight: bold; min-width: 20px; margin-left: 5px; text-align: center; position: absolute; top: 2px; right: 2px; }
  .selectedStandard .code { float: right; display: inline-block; width: 35%; text-align: right; position: absolute; top: 2px; right: 25px; }
  .selectedStandard .text { display: inline-block; width: 100%; margin-top: 1.75em; padding-left: 5px; padding-right: 5px }
  .selectedStandard select.appliedAlignmentType { position: absolute; top: 2px; left: 2px; width: 130px; }
  #treeBrowser .sbTreeItem { }
  #treeBrowser .sbTreeItem[data-depth=d0] { padding: 5px 5px 10px 5px; }
  #treeBrowser .sbTreeItem[data-depth=d1] { padding: 5px 5px 5px 15px; margin-top: 15px; color: #4AA394; }
  #treeBrowser .sbTreeItem[data-depth=d2] { padding: 5px 5px 5px 25px; }
  #treeBrowser .sbTreeItem[data-depth=d3] { padding: 5px 5px 5px 45px; }
  #treeBrowser .sbTreeItem[data-depth=d4] { padding: 5px 5px 5px 65px; }
  #treeBrowser .sbTreeItem[data-depth=d0] > .sbTitle * { font-size: 22px; font-weight: bold; }
  #treeBrowser .sbTreeItem[data-depth=d1] > .sbTitle * { font-size: 18px; font-weight: bold; }
  #treeBrowser .sbTreeItem[data-depth=d2] > .sbTitle * { font-size: 16px; }
  #treeBrowser .sbTreeItem[data-depth=d2] .sbTitle, #treeBrowser .sbTreeItem[data-depth=d3] .sbTitle, #treeBrowser .sbTreeItem[data-depth=d4] .sbTitle,  #searchAtOptions a, #selectedStandards .selectedStandard .text { 
    text-overflow: ellipsis; 
    max-height: 1.5em; 
    overflow: hidden; 
    white-space: nowrap; 
  }
  #treeBrowser .sbTreeItem[data-depth=d2] .text, #treeBrowser .sbTreeItem[data-depth=d3] .text, #treeBrowser .sbTreeItem[data-depth=d4] .text { padding-left: 5px; }
  #treeBrowser .sbTreeItem[data-depth=d1]:nth-child(2) { margin-top: 0; }
  #treeBrowser .sbTreeItem .code { font-weight: bold; }
  #treeBrowser .sbTreeItem .text {  }
  #treeBrowser .sbTreeItem[data-selected=true] { background-color: #4AA394; color: #FFF; }
  #treeBrowser .sbTreeItem[data-selected=false] {  }
  #treeBrowser .sbTreeItem:hover, #treeBrowser .sbTreeItem:focus { cursor: pointer; background-color: #FF5707; color: #FFF; }
  #standardDetails #sbStandardTitle { font-size: 24px; }
  #standardDetails #sbStandardGrades { margin: 5px; font-style: italic; }
  #standardDetails #sbStandardDescription: { margin: 5px; padding: 5px; }
  #standardDetails #sbStandardLink { margin: 20px 5px 10px 5px; display: none; }
  #standardDetails #searchAt { margin: 5px; }
  
  #searchLink a { display: block; text-align: center; padding: 3px; background-color: #4AA394; color: #FFF; margin-top: 10px; }
  #searchLink a:hover, #searchLink a:focus { background-color: #FF5707; text-decoration: none; }
  
  #selectItem { display: block; text-align: center; padding: 5px; border-radius: 5px; background-color: #3572B8; color: #FFF; margin: 5px 0; }
  #searchAt #selectItem:hover, #selectItem:focus { background-color: #FF5707; color: #FFF; text-decoration: none; }
</style>

<!-- Standards Browser v5 HTML -->
<div id="standardsBrowser">

  <div id="sbTopBar">
    <div id="selectStandardGrade">
      <h2>Select Standard Body and Grade Level</h2>
      <select id="ddlStandardBody">
        <option selected="selected" value="none">Select...</option>
        <option value="jsonMath">Common Core Math Standards</option>
        <option value="jsonELA">Common Core ELA/Literacy Standards</option>
        <option value="jsonNGSS">Next Generation Science Standards</option>
        <option value="jsonILFineArts">Illinois Fine Arts Standards</option>
        <option value="jsonILPhysicalDevelopment">Illinois Physical Development and Health Standards</option>
        <option value="jsonILSocialScience">Illinois Social Science Standards</option>
        <!--<option value="jsonAB">Academic Benchmark Standards</option>-->
      </select>
      <div id="grades">
      </div>
    </div>
  </div>

  <div id="sbLeftColumn">
    <div id="selectDomain">
      <!--<h2>Explore <span class="inheritor" id="sbBodyTitle"></span></h2>-->
      <select id="ddlDomain">
      </select>
    </div>
    <div id="treeBrowser">
    </div><!-- /treeBrowser -->
  </div>

  <div id="sbRightColumn">
    <div id="standardDetails">
    </div>

    <div id="searchAt">
      <a href="#" id="selectItem" onclick="selectCurrentItem(); return false;">Select This Item</a>
      <div id="searchLink"></div>
    </div>

    <div id="standardsBrowserExternalMessage"></div>

    <div id="selectedStandards">
      <h3>Selected Standards</h3>
    </div>

  </div>


</div><!-- /standardsBrowser -->

<!-- Standards Browser Templates -->
<div id="standardsBrowserTemplates" style="display: none;">
  <div id="sbTemplate_gradeLink">
    <a href="#" data-grade="{grade}" onclick="chooseGrade('{grade}');return false;">{grade}</a>
  </div>

  <div id="sbTemplate_treeItem">
    <div class="sbTreeItem" data-depth="d{indent}" data-selected="false" data-id="{id}" {tab}>
      <div class="sbTitle">
        <span class="code">{code}</span> <span class="text">{text}</span>
      </div>
    </div>
    {content}
  </div>

  <div id="sbTemplate_details">
    <!--<h2 id="sbStandardTitle">{standardCode}<br />{standardDomain}</h2>-->
    <div id="sbStandardGrades">Applies to Grade(s): {gradesList}</div>
    <div id="sbTreeInfo">{treeInfo}</div>
    <div id="sbStandardDescription">{description}</div>
    <a href="{url}" id="sbStandardLink" target="_blank">View on ASN</a>
  </div>
  

  <div id="sbTemplate_selectedStandard">
    <div class="selectedStandard" data-id="{id}" data-code="{code}" data-alignment="{alignmentID}">
      <a href="#" onclick="removeStandard({id}); return false;">X</a>
      <select class="appliedAlignmentType">
        <option selected="selected" value="0">The Resource...</option>
        <option value="2">Teaches</option>
        <option value="1">Assesses</option>
        <option value="3">Requires</option>
      </select>
      <span class="text">{text}</span>
      <span class="code">{code}</span>
    </div>
  </div>

  <div id="sbTemplate_searchForLink">
    <a href="#" data-id="{id}" onclick="applyStandard({id}, 0, '');return false;">{text}</a>
  </div>
</div>