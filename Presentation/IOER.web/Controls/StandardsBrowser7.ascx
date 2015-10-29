<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="StandardsBrowser7.ascx.cs" Inherits="IOER.Controls.StandardsBrowser7" %>


<script type="text/javascript" src="/Scripts/standards/jsonMath-trimmed.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonELA-trimmed.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonNGSS-trimmed.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonNHES.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILFineArts.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILPhysicalDevelopment.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILSocialScience.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILSocialEmotional.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILAdultEdMath.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILAdultEdELAReading.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILAdultEdELAWriting.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILAdultEdELASpeakingListening.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonILForeignLanguage.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonFinance_k12pfe.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonFinance_vncse.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonFinance_nsfl.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonFramework21stCenturyLearning.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonRICivics.js"></script>
<script type="text/javascript" src="/Scripts/standards/jsonRIHistory.js"></script>

<script type="text/javascript">
  var SB7mode = (typeof (SB7mode) == "undefined" ? "tbd" : SB7mode); //search, tag, browse, widget, dev
  var useSecureURL = (typeof (useSecureURL) == "undefined" ? false : useSecureURL);
  var curriculumMode = (typeof (curriculumMode) == "undefined" ? false : curriculumMode); //Determine whether or not to show/use the "major/supporting/additional" standards stuff
  var inputBodies = {
  	math: "jsonMath",
  	ela: "jsonELA",
  	ngss: "jsonNGSS",
  	nhes: "jsonNHES",
  	ilfinearts: "jsonILFineArts",
  	ilphysicaldevelopment: "jsonILPhysicalDevelopment",
  	ilsocialscience: "jsonILSocialScience",
  	ilsocialemotional: "jsonILSocialEmotional",
  	//iladultedreading: "jsonILAdultEdReading",
  	//iladultedwriting: "jsonILAdultEdWriting",
  	iladultedmath: "jsonILAdultEdMath",
  	iladultedelareading: "jsonILAdultEdELAReading",
  	iladultedelawriting: "jsonILAdultEdELAWriting",
  	iladultedelaspeakinglistening: "jsonILAdultEdELASpeakingListening",
  	ilforeignlanguage: "jsonILForeignLanguage",
  	finance_k12pfe: "jsonFinance_k12pfe",
  	finance_vncse: "jsonFinance_vncse",
  	finance_nsfl: "jsonFinance_nsfl",
  	f21cl: "jsonF21CL",
  	riCivics: "jsonRICivics",
		riHistory: "jsonRIHistory"
  };
  var prefixes = {
  	jsonMath: "CCSS.Math.Content.",
  	jsonELA: "CCSS.ELA-Literacy.",
  	jsonNHES: "NHES.",
  	jsonNGSS: "NGSS-",
  	jsonILFineArts: "IL.FA.",
  	jsonILPhysicalDevelopment: "IL.PD.",
  	jsonSocialScience: "IL.SS.",
  	jsonILSocialEmotional: "IL.SED.",
  	//jsonILAdultEdReading: "IL.ABE.Reading.",
  	//jsonILAdultEdWriting: "IL.ABE.Writing.",
  	jsonILAdultEdMath: "IL.ABE.Math.",
  	jsonILAdultEdELAReading: "IL.ABE.ELA.",
  	jsonILAdultEdELAWriting: "IL.ABE.ELA.",
  	jsonILAdultEdELASpeakingListening: "IL.ABE.ELA.",
  	jsonILForeignLanguage: "IL.FL.",
  	jsonFinance_k12pfe: "Finance.K12PFE.",
  	jsonFinance_vncse: "Finance.VNCSE.",
  	jsonFinance_nsfl: "Finance.NSFL",
  	jsonF21CL: "",
  	jsonRICivics: "jsonRICivics",
		jsonRIHistory: "jsonRIHistory"
  };
  var bodies = {
  	none: null,
  	jsonMath: jsonMath,
  	jsonELA: jsonELA,
  	jsonNHES: jsonNHES,
  	jsonNGSS: jsonNGSS,
  	jsonILFineArts: jsonILFineArts,
  	jsonILPhysicalDevelopment: jsonILPhysicalDevelopment,
  	jsonILSocialScience: jsonILSocialScience,
  	jsonILSocialEmotional: jsonILSocialEmotional,
  	//jsonILAdultEdReading: jsonILAdultEdReading,
  	//jsonILAdultEdWriting: jsonILAdultEdWriting,
  	jsonILAdultEdMath: jsonILAdultEdMath,
  	jsonILAdultEdELAReading: jsonILAdultEdELAReading,
  	jsonILAdultEdELAWriting: jsonILAdultEdELAWriting,
  	jsonILAdultEdELASpeakingListening: jsonILAdultEdELASpeakingListening,
  	jsonILForeignLanguage: jsonILForeignLanguage,
  	jsonFinance_k12pfe: jsonFinance_k12pfe,
  	jsonFinance_vncse: jsonFinance_vncse,
  	jsonFinance_nsfl: jsonFinance_nsfl,
  	jsonF21CL: jsonF21CL,
  	jsonRICivics: jsonRICivics,
		jsonRIHistory: jsonRIHistory
  };
  var grades = [ "none", "K", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "Adult" ];
  var currentBody = [];
  var currentGrade = "none";
  var currentDomain = null;
  var treeItemTemplate = "";
  var selectedStandards = [];
  var allSelectedIDs = [];
  var allSelectedStandards = [];
  var showButtonOnNextResults = false;
  var helperURL = "";
</script>
<script type="text/javascript">
  $(document).ready(function () {
    //Assign functionality
    $("#SBddlBody, #SBddlGrade").on("change", function () {
      populateDomains();
    });
    $("#SBddlDomain").on("change", function () {
      populateTree();
    });

    $("#SBselected").on("change", ".selectedStandard select", function () {
      SBstoreDDLValues();
    });

    //Enhance efficiency
    treeItemTemplate = $("#template_treeItem").html();

    //Preprocess bodies
    for (i in bodies) {
      for (j in bodies[i]) {
        bodies[i][j].fullCode = prefixes[i] + bodies[i][j].code;
        bodies[i][j].body = i;
      }
    }

    //Use a given mode
    determineSBPreselections();

    //Prompt user to close the browser when results show
    $(window).on("resultsLoaded", function (event, hits) {
      if (selectedStandards.length > 0) {
        $("#SBbtnShowResults").attr("value", "Show " + hits + " Result(s)");
        if (hits > 0 && showButtonOnNextResults) {
          $("#SBresultsEmpty").hide();
          $("#SBbtnShowResults").show();
        }
        else {
          $("#SBbtnShowResults").hide();
          $("#SBresultsEmpty").show().html("<p>Sorry, no results found. Please try a different selection of standards.</p>");
        } 
        showButtonOnNextResults = false;
      }
    });

    //Remove a standard remotely
    $(window).on("SB7removeStandard", function (event, id) {
      removeSelectedStandard(id);
      SBdoSearch();
    });

    //If widget, enable resize thingy
    if (SB7mode == "widget" && helperURL != "") {
      enableResizingParent();
    }
    else {
      $("#resizeFrame").remove();
    }

    //Debugging
    $(window).on("SB7search", function(event, selected, all){
      //console.log(event);
      //console.log(selected);
      //console.log(all);
    });

    //Remove the curriculum stuff if not needed
    if (!curriculumMode) {
      $(".usageType").remove();
    }

  }); //End document.ready


  function determineSBPreselections() {
    var properties = window.location.search.toLowerCase().replace("?", "").split("&");
    //Determine preselected things
    //console.log(properties);
    for (i in properties) {
      var item = properties[i].split("=");
      switch (item[0]) {
        //Standards Browser mode
        case "standardsbrowsermode":
          SB7mode = item[1];
          break;
        //Preselected standard IDs
        case "standardids":
          var ids = item[1].split(",");
          for (var j in ids) {
            addSelectedStandard(parseInt(ids[j]));
          }
          SBdoSearch();
          break;
        //Preselected standard body
        case "body":
          if (inputBodies[item[1]] != undefined) {
            $("#SBddlBody option[value=" + inputBodies[item[1]] + "]").prop("selected", true);
            $("#SBddlBody").trigger("change");
          }
          break;
        //Preselected grade level
        case "grade":
          if (grades.indexOf(item[1]) > -1) {
            $("#SBddlGrade option[value=" + item[1] + "]").prop("selected", true);
            $("#SBddlGrade").trigger("change");
          }
          break;
        case "helperurl":
          helperURL = item[1];
          break;
        default:
          break;
      }
    }

    //Determine mode
    switch (SB7mode) {
      case "browse":
        $(".alignmentType").remove();
        $("#SBbuttons").remove();
        break;
      case "tag":
        $("#SBbuttons").remove();
        break;
      case "search":
        $(".alignmentType").remove();
        break;
      case "widget":
        $(".alignmentType").remove();
        break;
      default:
        SB7mode = "browse";
        $(".alignmentType").remove();
        $("#SBbuttons").remove();
        break;
    }
  }

  //Check the statuses of selected body and grade level. If valid values, populate the domain DDL. Otherwise, empty it
  function populateDomains() {
    var targetBody = $("#SBddlBody option:selected").attr("value");
    var targetGrade = $("#SBddlGrade option:selected").attr("value");
        
    currentBody = bodies[targetBody];
    currentGrade = targetGrade;

    targetBody = targetBody.toLowerCase();

  	//Convenience
		var gradeDDL = $("#SBddlGrade");
    var options = gradeDDL.find("option");
    var validGrades = $("#SBddlBody option:selected").attr("data-grade").split(",");
    options.each(function () {
    	var opt = $(this);
    	opt.hide().removeClass("showing");
    	for (var i in validGrades) {
    		if (opt.hasClass(validGrades[i])) {
    			opt.show().addClass("showing");
    		}
    	}
    });

		//Auto-select the first available choice
    if (!gradeDDL.find("option:selected").hasClass("showing")) {
    	gradeDDL.find("option.showing").first().prop("selected", true);
    	gradeDDL.trigger("change");
    }

    renderDomains();
  }

  //Get the grade-appropriate children for the current domain
  function populateTree() {
    var targetDomain = $("#SBddlDomain option:selected").attr("value");
    currentDomain = targetDomain == "none" ? null : getLayer(parseInt(targetDomain), true);

    renderTree();
  }

  //Toggle a standard being selected
  function toggleSelectedStandard(id, checked) {
    if (checked) {
      addSelectedStandard(id);
    }
    else {
      removeSelectedStandard(id);
    }
  }

  //Add or remove selected standards
  function addSelectedStandard(id) {
    for (i in selectedStandards) {
      if (selectedStandards[i].id == id) { return; }
    }

    var target = getLayer(id, currentGrade != "none");
    if (target != null) {
      selectedStandards.push(target);
    }

    $(window).trigger("standard_added");
    determineAllSelectedStandards();
    renderSelectedStandards();
  }

  function removeSelectedStandard(id) {
    var replacement = [];
    for (i in selectedStandards) {
      if (selectedStandards[i].id != id) {
        replacement.push(selectedStandards[i]);
      }
    }
    selectedStandards = replacement;
    determineAllSelectedStandards();
    renderSelectedStandards();
    $(window).trigger("standard_removed");
  }

  //Do a search with the selected IDs and IDs under them in the hierarchy
  function SBdoSearch() {
    if (SB7mode == "widget") {
      //Send the user to the main search. Send only the selected IDs; the browser will figure out the rest
      var ids = "";
      for (i in selectedStandards) {
        ids += "," + selectedStandards[i].id;
      }
      ids = ids.replace(",", "");
      window.open( (useSecureURL ? "https://ioer.ilsharedlearning.org/secure/IsleSSO.aspx?nextUrl=" : "") + "http://ioer.ilsharedlearning.org/search.aspx?standardIDs=" + ids);
    }
    else {
      determineAllSelectedStandards();

      //Call the external search mechanism
      $(window).trigger("SB7search", [selectedStandards, allSelectedIDs, allSelectedStandards]);
      showButtonOnNextResults = true;
    }
  }

  function determineAllSelectedStandards() {
  	allSelectedIDs = [];
  	for (i in selectedStandards) {
  		allSelectedIDs.push(selectedStandards[i].id);
  		//Recurse through the tree to grab all child IDs
  		getChildIDs(selectedStandards[i]);
  	}
  	//Remove duplicates
  	var replacement = [];
  	for (i in allSelectedIDs) {
  		if (replacement.indexOf(allSelectedIDs[i]) == -1) {
  			replacement.push(allSelectedIDs[i]);
  		}
  	}
  	allSelectedIDs = replacement;


    allSelectedStandards = [].concat(selectedStandards);
    for (i in allSelectedIDs) {
      var item = getLayer(allSelectedIDs[i]);
      if (item != null) {
        allSelectedStandards.push(item);
      }
    }
  }

  //Get all child IDs for a given standard, all the way to the bottom
  function getChildIDs(item) {
    for (i in item.children) {
      var standard = getLayer(item.children[i], currentGrade != "none");
      if (standard != null) {
        allSelectedIDs = allSelectedIDs.concat(standard.id);
        getChildIDs(standard);
      }
    }
  }

  //Send a message to the parent to show the results found
  function SBshowResults() {
    $(window).trigger("SB7showResults");
  }
</script>
<script type="text/javascript">
  //Return the notation code or description
  function getName(item) {
    return (item.code == null || item.code == "") ? item.description : item.code;
  }
  //Return the element if it matches the current grade
  function getLayer(id, matchGrade) {
    if (currentBody.length == 0) {
      var returner = null;
      for (i in bodies) {
        returner = getLayerItem(bodies[i], id, matchGrade);
        if (returner != null) { break; }
      }
      return returner;
    }
    else {
      return getLayerItem(currentBody, id, matchGrade);
    }
  }
  function getLayerItem(body, id, matchGrade) {
    for (i in body) {
      if (body[i].id == id) {
      	if (matchGrade) {
      		//console.log(body[i].grades + ", " + body[i].grades.indexOf(currentGrade) + ", " + currentGrade);
          return body[i].grades.indexOf(currentGrade) > -1 ? body[i] : null;
        }
        else {
          return body[i];
        }
      }
    }
  }

  //Render domains DDL
  function renderDomains() {
    //Get the box
    var ddl = $("#SBddlDomain");
    //Empty out everything but the top level "Select..." item
    ddl.find("option").not("[value=none]").remove();
    clearTree();

    if (currentBody == null || currentGrade == "none") { return; }

    //Dig out the domains for the current grade level and add them
    for (i in currentBody[0].children) {
      var item = getLayer(currentBody[0].children[i], true);
      if (item != null) {
        //ddl.append("<option value=\"" + currentBody[i].id + "\">" + getName(currentBody[i]) + "</option>");
        ddl.append("<option value=\"" + currentBody[i].id + "\">" + currentBody[i].description + "</option>");
      }
    }
    
    $("#SB7").trigger("afterContentChange");
  }

  //Kick off rendering the tree
  function renderTree() {
    var tree = $("#SBtree");
    clearTree();

    if (currentDomain == null || currentGrade == "none") { return; }

    //Render the elements
    treecursion(tree, currentDomain, 0);

    //Add functionality to the checkboxes
    updateCheckboxes();
    tree.find("input[type=checkbox]").on("click", function () {
      var box = $(this);
      toggleSelectedStandard(parseInt(box.attr("data-standardID")), box.prop("checked"));
    });

    $("#SB7").trigger("afterContentChange");
  }

  //Recursively continue rendering the tree
  function treecursion(tree, item, depth) {
    //render the current item
    tree.append(renderItem(item, depth));

    //treecurse the item's children
    for (i in item.children) {
      var next = getLayer(item.children[i], true);
      if (next != null) {
        treecursion(tree, next, depth + 1);
      }
    }
  }

  //Fill out the HTML for a single tree item
  function renderItem(item, depth) {
  	console.log(item);
    return treeItemTemplate
      .replace(/{id}/g, item.id)
      .replace(/{code}/g, item.code == null ? "" : item.code)
			.replace(/{grades}/g, item.grades.join(", "))
      .replace(/{text}/g, item.description)
      .replace(/{depth}/g, depth);
  }

  //Render the selected Standards
  function renderSelectedStandards() {
    var box = $("#SBselected");
    var template = $("#template_selectedStandard").html();

    SBstoreDDLValues();

    //Rerender the list
    box.html("");
    for (i in selectedStandards) {
      box.append(template
        .replace(/{id}/g, selectedStandards[i].id)
        .replace(/{name}/g, getName(selectedStandards[i]))
        .replace(/{code}/g, selectedStandards[i].code == null ? "" : selectedStandards[i].code)
				.replace(/{grades}/g, selectedStandards[i].grades.join(", "))
      );
    }

    SBsetDDLValues();

    updateCheckboxes();
    $("#SBbuttons").css("display", selectedStandards.length > 0 ? "block" : "none");

    $("#SBbtnShowResults, #SBresultsEmpty").hide();

    $("#SB7").trigger("afterContentChange");
  }

  //Get and store DDL values
  function SBstoreDDLValues() {
    $("#SBselected").find(".selectedStandard").each(function () {
      for (i in selectedStandards) {
        var item = $(this);
        if (selectedStandards[i].id == parseInt(item.attr("data-standardID"))) {
          try { selectedStandards[i].alignmentType = parseInt(item.find(".alignmentType option:selected").attr("value")); } catch (e) { }
          try { selectedStandards[i].usageType = parseInt(item.find(".usageType option:selected").attr("value")); } catch (e) { }
        }
      }
    });
  }
  //Set DDL values
  function SBsetDDLValues() {
    for (i in selectedStandards) {
      var item = $("#SBselected").find(".selectedStandard[data-standardID=" + selectedStandards[i].id + "]");
      if (typeof (item) != "undefined") {
        try { item.find(".alignmentType option[value=" + selectedStandards[i].alignmentType + "]").prop("selected", true); } catch (e) { }
        try { item.find(".usageType option[value=" + selectedStandards[i].usageType + "]").prop("selected", true); } catch (e) { }
      }
    }

  }

  //Match the state of checkboxes to the selected items
  function updateCheckboxes() {
    $("#SBtree input[type=checkbox]").each(function () {
      var box = $(this);
      var id = parseInt(box.attr("data-standardID"));
      box.prop("checked", false);
      for (i in selectedStandards) {
        if (id == selectedStandards[i].id) {
          box.prop("checked", true);
          break;
        }
      }
    });
  }

  //Clear the tree
  function clearTree() {
    $("#SBtree").html("");
    $("#SB7").trigger("afterContentChange");
  }
</script>
<script type="text/javascript">
  //Resize parent frame
  function enableResizingParent() {
    $("#resizeFrame").attr("src", helperURL);
    $(window).on("resize", updateHelperFrame );
    $("#SB7").on("afterContentChange", updateHelperFrame );
  }
  function updateHelperFrame() {
    $("#resizeFrame").attr("src", helperURL + "?height=" + ($("#SB7").height() + 40) + "&cachb=" + Math.random());
  }
</script>
<script>
  //Gooru things
  function hideILStandards() {
    $("#SBddlBody option[value*=jsonIL]").remove();
  }

  function sb7AddStandardsByFullCode(codes) {
    var tempGrade = currentGrade;
    currentGrade = "none";
    for (i in codes) {
      var codeBody = "";
      for (j in prefixes) {
        if (codes[i].indexOf(prefixes[j] > -1)) {
          codeBody = j;
          codes[i] = codes[i].replace(prefixes[j], "");
          break;
        }
      }
      //console.log(codes[i]);
      //console.log(codeBody);
      for (j in bodies[codeBody]) {
        if (bodies[codeBody][j].code == codes[i]) {
          //console.log("adding standard #" + bodies[codeBody][j].id);
          addSelectedStandard(bodies[codeBody][j].id);
          break;
        }
      }
    }
    currentGrade = tempGrade;
  }
</script>
<style type="text/css">
  #SB7 { padding: 5px; }
  #SB7, #SB7 * { box-sizing: border-box; -moz-box-sizing: border-box; }

  #SB7 #SBleftColumn, #SB7 #SBrightColumn { display: inline-block; vertical-align: top; margin-right: -4px; }
  #SB7 #SBleftColumn { width: 75%; padding-right: 5px; }
  #SB7 #SBrightColumn { width: 24.5%; }

  #SB7 #SBddlBox { white-space: nowrap; margin-bottom: 5px; text-align: center; }
  #SB7 .SBddl { width: 32.8%; display: inline-block; vertical-align: top; }
  #SB7 #SBtree { padding: 5px; max-height: 500px; overflow-x: hidden; overflow-y: scroll; border: 1px solid #CCC; border-radius: 5px; background-color: #DFDFDF; }
  #SB7 #SBtree:empty { padding: 0; border: none; border-radius: 0; }
  #SB7 #SBtree .treeItem { display: block; padding: 5px; border-radius: 5px; }
  #SB7 #SBtree .treeItem:focus, #SB7 #SBtree .treeItem:hover { background-color: #FF5707; color: #FFF; cursor: pointer; }
  #SB7 #SBtree .treeItem input { float: left; margin: 4px 5px;  }
  #SB7 #SBtree .treeItem .code { font-weight: bold; }
	#SB7 #SBtree .treeItem .grades, .selectedStandard .grades { font-size: 16px; font-style: italic; font-weight: normal; }
	.selectedStandard .grades { border-top: 1px solid #CCC; }
  #SB7 #SBtree .treeItem[data-depth=d0] { padding-left: 5px; }
  #SB7 #SBtree .treeItem[data-depth=d1] { padding-left: 15px; }
  #SB7 #SBtree .treeItem[data-depth=d2] { padding-left: 30px; }
  #SB7 #SBtree .treeItem[data-depth=d3] { padding-left: 45px; }
  #SB7 #SBtree .treeItem[data-depth=d4] { padding-left: 60px; }
  #SB7 #SBtree .treeItem[data-depth=d5] { padding-left: 75px; }
  #SB7 #SBtree .treeItem[data-depth=d0] .code, #SB7 #SBtree .treeItem[data-depth=d0] .text { font-size: 30px; font-weight: bold; }
  #SB7 #SBtree .treeItem[data-depth=d1] .code, #SB7 #SBtree .treeItem[data-depth=d1] .text { font-size: 24px; font-weight: bold; color: #4AA394; }
  #SB7 #SBtree .treeItem[data-depth=d2] .code, #SB7 #SBtree .treeItem[data-depth=d2] .text { font-size: 20px; }
  #SB7 #SBtree .treeItem[data-depth=d0] input[type=checkbox] { margin: 9px 5px; }
  #SB7 #SBtree .treeItem[data-depth=d1] input[type=checkbox] { margin: 6px 5px }
  #SB7 #SBtree .treeItem[data-depth=d2] input[type=checkbox] { margin: 5px 5px; }
  #SB7 #SBtree .treeItem[data-depth=d1]:hover *, #SB7 #SBtree .treeItem[data-depth=d1]:focus * { color: #FFF; }
  #SB7 #SBtree .treeItem ol, #SB7 #SBtree .treeItem ul { margin-left: 25px; }

  #SB7 #SBselected { margin-bottom: 10px; }
  #SB7 #SBselected .selectedStandard { padding: 2px 25px 3px 2px; position: relative; border: 1px solid #CCC; border-radius: 5px; margin-bottom: 5px; }
  #SB7 #SBselected .selectedStandard a { position: absolute; top: 2px; right: 2px; display: block; width: 20px; height: 20px; line-height: 20px; margin: 0 0 2px 5px; color: #FFF; background-color: #D33; border-radius: 5px; text-align: center; font-weight: bold; }
  #SB7 #SBselected .selectedStandard a:hover, #SB7 #SBselected .selectedStandard a:focus { background-color: #F00; }

  #SB7 #SBbuttons input { margin-bottom: 10px; width: 100%; border-radius: 5px; border: 1px solid #999; padding: 5px; background-color: #3572B8; color: #FFF; font-weight: bold; }
  #SB7 #SBbuttons input:hover, #SB7 #SBbuttons input:focus { cursor: pointer; background-color: #FF6A00; }
  #SB7 .usageType select { width: 100%; }

  #resizeFrame { display: none; }

  @media screen and (max-width: 600px) {
    #SB7 #SBleftColumn { width: 66%; }
    #SB7 #SBrightColumn { width: 32.5%; }
    #SB7 #SBddlBox { white-space: normal; }
    #SB7 .SBddl { display: block; width: 100%; margin-bottom: 5px; }
    #SB7 #SBtree { max-height: none; overflow-x: visible; overflow-y: visible; border: none; border-radius: none; }
  }
</style>

<div id="SB7">

  <div id="SBddlBox">
    <select id="SBddlBody" class="SBddl">
      <option selected="selected" value="none">Select a Standard Body...</option>
      <optgroup label="K-12 Education">
        <option value="jsonMath" data-grade="grade">Common Core Math Standards</option>
        <option value="jsonELA" data-grade="grade">Common Core ELA/Literacy Standards</option>
        <option value="jsonILFineArts" data-grade="grade">Illinois Fine Arts Standards</option>
        <option value="jsonILForeignLanguage" data-grade="grade">Illinois Foreign Language Standards</option>
        <option value="jsonILPhysicalDevelopment" data-grade="grade">Illinois Physical Development and Health Standards</option>
        <option value="jsonILSocialScience" data-grade="grade">Illinois Social Science Standards</option>
        <option value="jsonILSocialEmotional" data-grade="grade">Illinois Social/Emotional Development Standards</option>
        <option value="jsonNGSS" data-grade="grade">Next Generation Science Standards</option>
				<!--<option value="jsonRICivics" data-grade="grade">Rhode Island GSEs for Civics &amp; Government</option>
				<option value="jsonRIHistory" data-grade="grade">Rhode Island GSEs for Historical Perspectives/Rhode Island History</option>-->
      </optgroup>
      <optgroup label="Adult Education">
        <option value="jsonILAdultEdELAReading" data-grade="adult,nrs">Illinois Adult Education (ABE/ASE) English Language Arts Standards for Reading</option>
        <option value="jsonILAdultEdELAWriting" data-grade="adult,nrs">Illinois Adult Education (ABE/ASE) English Language Arts Standards for Writing</option>
        <option value="jsonILAdultEdELASpeakingListening" data-grade="adult,nrs">Illinois Adult Education (ABE/ASE) English Language Arts Standards for Speaking and Listening</option>
        <option value="jsonILAdultEdMath" data-grade="adult,nrs">Illinois Adult Education (ABE/ASE) Mathematics Standards</option>
        <!--<option value="jsonILAdultEdReading">Illinois Adult Education (ABE/ASE) Reading Standards</option>
        <option value="jsonILAdultEdWriting">Illinois Adult Education (ABE/ASE) Writing Standards</option>-->
      </optgroup>
      <optgroup label="National Standards">
				<option value="jsonF21CL" data-grade="grade">Framework for 21st Century Learning</option>
        <option value="jsonNHES" data-grade="grade">National Health Education Standards</option>
        <option value="jsonFinance_k12pfe" data-grade="grade">National Standards in K-12 Personal Finance Education</option>
        <option value="jsonFinance_nsfl" data-grade="grade">National Standards for Financial Literacy</option>
        <option value="jsonFinance_vncse" data-grade="grade">Voluntary National Content Standards in Economics</option>
      </optgroup>
    </select>
    <select id="SBddlGrade" class="SBddl">
      <option selected="selected" class="select" value="none">Select an Education Level...</option>
      <option value="K" class="grade">Kindergarten</option>
      <option value="1" class="grade">1st Grade</option>
      <option value="2" class="grade">2nd Grade</option>
      <option value="3" class="grade">3rd Grade</option>
      <option value="4" class="grade">4th Grade</option>
      <option value="5" class="grade">5th Grade</option>
      <option value="6" class="grade">6th Grade</option>
      <option value="7" class="grade">7th Grade</option>
      <option value="8" class="grade">8th Grade</option>
      <option value="9" class="grade">9th Grade</option>
      <option value="10" class="grade">10th Grade</option>
      <option value="11" class="grade">11th Grade</option>
      <option value="12" class="grade">12th Grade</option>
      <option value="Adult" class="adult">General Adult Education</option>
			<option value="NRS1" class="nrs">NRS 1 - Beginning Adult Basic Education Literacy</option>
			<option value="NRS2" class="nrs">NRS 2 - Beginning Basic Education</option>
			<option value="NRS3" class="nrs">NRS 3 - Low Intermediate Basic Education</option>
			<option value="NRS4" class="nrs">NRS 4 - High Intermediate Basic Education</option>
			<option value="NRS5" class="nrs">NRS 5 - Low Adult Secondary Education</option>
			<option value="NRS6" class="nrs">NRS 6 - High Adult Secondary Education</option>
    </select>
    <select id="SBddlDomain" class="SBddl">
      <option selected="selected" value="none">Select a Content Area...</option>
    </select>
  </div>
  <div id="SBleftColumn">
    <div id="SBtree"></div>
  </div>
  <div id="SBrightColumn">
    <div id="SBselected"></div>

    <div id="SBbuttons" style="display: none;">
      <input type="button" id="SBbtnSearch" onclick="SBdoSearch()" class="isleButton bgBlue" value="Search" />
      <input type="button" id="SBbtnShowResults" style="display: none;" onclick="SBshowResults()" value="Show Results" />
      <div id="SBresultsEmpty"></div>
    </div>
  </div>

  <iframe id="resizeFrame"></iframe>

  <div id="template_treeItem" style="display:none;">
    <label for="standard_{id}" class="treeItem" data-id="{id}" data-depth="d{depth}">
      <input type="checkbox" id="standard_{id}" data-standardID="{id}" />
      <div class="code">{code}</div>
      <div class="text">{text}</div>
			<div class="grades">Applies to Grade(s): <span>{grades}</span></div>
    </label>
  </div>
  <div id="template_selectedStandard" style="display:none;">
    <div class="selectedStandard" data-standardID="{id}" data-code="{code}">
      <a href="#" onclick="removeSelectedStandard({id}); return false;">X</a>
      <select class="alignmentType">
        <option value="0">Aligns To</option>
        <option value="1">Assesses</option>
        <option value="2">Teaches</option>
        <option value="3">Requires</option>
      </select>
      <select class="usageType">
        <option value="1">Major</option>
        <option value="2">Supporting</option>
        <option value="3">Additional</option>
      </select>
      <span>{name}</span>
			<div class="grades">Grades {grades}</div>
    </div>
  </div>
</div>