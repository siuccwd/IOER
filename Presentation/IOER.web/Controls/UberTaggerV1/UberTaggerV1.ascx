<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UberTaggerV1.ascx.cs" Inherits="ILPathways.Controls.UberTaggerV1.UberTaggerV1" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/Controls/StandardsBrowser7.ascx" %>

<link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<script type="text/javascript">
  /* Global Variables */
  var keywords = [];
  var preselectedKeywords = <%=LoadedKeywordsJSON %>;
  var valids = {};
  var timers = {};
  var SB7mode = "tag";
  var myLibraries = <%=MyLibColData %>;
  var preselectedStandards = <%=LoadedStandardsJSON %>;
</script>
<script type="text/javascript">
  /* Initialization */
  //Startup
  $(document).ready(function () {
    setupKeywords();
    setupUsageRights();
    setupValidation();
    setupCBXLs();
    setupLibColDDLs();
    $("#SB7").on("afterContentChange", function() { tallyRequired(); });
    renderKeywords(true);
    setupPreselectedStandards();
    triggerValidations();
    addSelectAlls();
  });

  //Setup Keywords
  function setupKeywords() {
    var box = $(".field[data-schema=keywords]");
    var added = box.find("#addedKeywords");
    var text = box.find(".txtKeyword");
    var minLength = parseInt(box.attr("data-minlength"));
    var message = box.find(".validMessage").attr("id");
    text.on("keyup", function (e) {
      var value = text.val().trim();

      //Help prevent duplicates
      if(checkDuplicateKeyword()){
        setValid(message, "red", "That keyword was already entered.");
        return;
      }

      //If validated and user presses enter
      if ((e.which == 13 || e.keyCode == 13) && valids[message] == true) {
        keywords.push(value);
        setValid(message, "gray", "");
        text.val("");
        renderKeywords();
        return;
      }

      //Do validation
      validateText("keywords", false, successValidateKeyword);
    });
  }

  /*function addCurrentKeyword() {
    var box = $(".field[data-schema=keywords]");
    var text = box.find(".txtKeyword");
    var message = box.find(".validMessage").attr("id");
    var value = text.val().trim();

    keywords.push(value);
    setValid(message, "gray", "");
    text.val("");
    renderKeywords();
    return;
  }*/

  //Setup Usage Rights
  function setupUsageRights() {
    var box = $(".field[data-schema=usageRights]");
    var ddl = box.find("[name=ddlUsageRights]");
    var link = box.find("#usageRightsLink");
    var imageBox = box.find("#standardRights");
    var urlBox = box.find("#txtUsageRightsURL");
    ddl.on("change", function () {
      var data = ddl.find("option:selected");
      link.attr("href", data.attr("data-url"));
      link.html(data.attr("data-description"));
      imageBox.css("background-image", "url('" + data.attr("data-icon") + "')");
      urlBox.val(data.attr("data-url"));
      if (data.attr("data-iscustom") == "true") {
        urlBox.show();
      }
      else {
        urlBox.hide();
      }
      urlBox.trigger("change");
      tallyRequired();
    });
    ddl.trigger("change");
  }

  //Setup Validation
  function setupValidation() {
    $("#basicInfo .field[data-required=true][data-type=text], #basicInfo .field[data-required=true][data-type=url]").each(function () {
      var schema = $(this).attr("data-schema");
      $(this).find(".mainInput").on("keyup change", function () {
        validateText(schema, false, successValidateItem);
      });
      valids[$(this).find(".validMessage").attr("id")] = false;
    });
    valids[$("#basicInfo .field[data-schema=keywords] .validMessage").attr("id")] = false;
    $("#basicInfo .field[data-schema=usageRights] .mainInput").trigger("change");
  }

  //Setup CBXLs
  function setupCBXLs() {
    $("#tags .field[data-required=true] input").on("change", function() {
      tallyRequired();
    });
  }
  
  //Setup Library/Collection DDLs
  function setupLibColDDLs() {
    var ddlLibrary = $("#ddlLibrary");
    var ddlCollection = $("#ddlCollection");

    for (i in myLibraries) {
      ddlLibrary.append("<option value=\"" + myLibraries[i].id + "\">" + myLibraries[i].title + "</option>");
    }

    ddlLibrary.on("change", function () {
      var libID = parseInt($(this).find("option:selected").attr("value"));
      ddlCollection.html("<option selected=\"selected\" value=\"0\">Select a Collection...</option>");

      for (i in myLibraries) {
        if (myLibraries[i].id == libID) {
          for (j in myLibraries[i].collections) {
            ddlCollection.append("<option value=\"" + myLibraries[i].collections[j].id + "\">" + myLibraries[i].collections[j].title + "</option>");
          }
        }
      }
    });
  }

  //Add selected standards
  function setupPreselectedStandards() {
    for(i in preselectedStandards){
      var current = preselectedStandards[i];
      //Add standard
      addSelectedStandard(current.StandardId);
      var box = $(".selectedStandard[data-standardid=" + current.StandardId + "]");
      //Set alignment type
      box.find(".alignmentType option[value=" + current.AlignmentTypeId + "]").prop("selected", true);
      //Set usage type
      box.find(".usageType option[value=" + current.AlignmentDegreeId + "]").prop("selected", true);
    }
  }

  function triggerValidations() {
    if(window.location.search.toLowerCase().indexOf("resourceid") > -1){
      $("[name=txtURL], [name=txtTitle], [name=txtDescription], [name=txtUsageRightsURL]").trigger("change");
    }
  }
</script>
<script type="text/javascript">
  /* Page Functions */

  //Check for duplicate keyword
  function checkDuplicateKeyword(){
    for (i in keywords) {
      if (keywords[i].toLowerCase() == $(".field[data-schema=keywords] .txtKeyword").val().toLowerCase()) {
        return true;
      }
    }
    return false;
  }

  //Remove a keyword
  function removeKeyword(target) {
    var replacement = [];
    for (i in keywords) {
      if(i != target){
        replacement.push(keywords[i]);
      }
    }
    keywords = replacement;
    renderKeywords();
  }

  //Set valid
  function setValid(target, color, message, isValid) {
    $("#" + target).attr("data-valid", color).html(message);
    valids[target] = isValid;
  }

  //Validate Field
  function validateText(schema, isFinal, successMethod) {
    var box = $(".field[data-schema=" + schema + "]");
    var text = box.find(".mainInput").val();
    var message = box.find(".validMessage").attr("id");
    var minLength = parseInt(box.attr("data-minlength"));
    var dataType = box.attr("data-type");
    clearTimeout(timers[schema]);
    //Auto-validate disabled
    if(box.find(".mainInput").attr("disabled") == "disabled"){
      setValid(message, "gray", "You don't have permission to edit this field", true);
      return;
    }
    //Check empty
    if (text.length == 0) {
      setValid(message, "gray", "", false);
      return;
    }
    //Check too short
    if (text.length < minLength) {
      setValid(message, "gray", "Please enter at least " + (minLength - text.length) + " more characters", false);
      return;
    }
    setValid(message, "gray", "Checking...", false);
    //Check AJAX
    if (dataType == "url") {
      var data = { text: text, mustBeNew: box.attr("data-unique") == "true" };
      timers[schema] = setTimeout( function() { validateAJAX("ValidateURL", data, successMethod, schema); }, 800);
    }
    else {
      var data = { text: text, minimumLength: minLength, fieldTitle: box.attr("data-title") };
      timers[schema] = setTimeout( function() { validateAJAX("ValidateText", data, successMethod, schema); }, 800);
    }
  }

  //Tally Required
  function tallyRequired(){
    //Basic Items
    var validItems = 0;
    var validTotal = 0;
    for(i in valids){
      if(valids[i] && i != "valid_txtKeyword"){ validItems++; }
      validTotal++;
    }
    if(keywords.length > 0 || preselectedKeywords.length > 0){
      validItems++;
    }
    $("#validFinal_basicInfo .status").html(validItems + " of " + validTotal + " Required Items");

    //Learning Standards
    $("#validFinal_standards .status").html(selectedStandards.length + " Standards Selected");

    //Tagging
    var tags = $("#tags .field[data-required=true]");
    var tagValids = 0;
    tags.each(function(){
      var hasChecked = false;
      if($(this).find("input:checked").length > 0){
        hasChecked = true;
      }
      if(hasChecked){ tagValids++; }
    });
    $("#validFinal_tags .status").html(tagValids + " of " + tags.length + " Required Items");
  }

  //Publish
  function publish() {
    //Validate Page
    var valid = true;
    var messages = [];
    for(i in valids){
      if(!valids[i] && i != "valid_txtKeyword"){
        valid = false;
        messages.push( "Please double-check " + $("#" + i).parent().attr("data-title"));
      }
    }
    if($(".txtKeyword").val().trim() != ""){
      if(confirm("You haven't added the keyword in the keyword entry box yet. Would you like to add this keyword now?")){
        var event = jQuery.Event("keyup");
        event.which = 13;
        event.keyCode = 13;
        $(".txtKeyword").trigger(event);
      }
    }
    if(keywords.length == 0 && preselectedKeywords.length == 0){
      messages.push("You must add at least one keyword");
    }
    $("#tags .field[data-required=true]").each(function() {
      if($(this).find("input:checked").length == 0){
        valid = false;
        messages.push( "You must select at least one " + $(this).find("h3").html());
      }
    });

    if(messages.length > 0){
      var text = "";
      for(i in messages){
        text += messages[i] + "\n";
      }
      alert(text);
      return;
    }

    //Pack Standards
    var standards = [];
    for(i in selectedStandards){
      var current = selectedStandards[i];
      standards.push({
        StandardId: current.id,
        Description: current.description,
        NotationCode: current.code,
        AlignmentTypeId: current.alignmentType,
        AlignmentDegreeId: isNaN(current.usageType) ? 0 : current.usageType
      });
    }
    $(".hdnStandards").val(JSON.stringify(standards));

    //Pack Keywords
    $(".hdnKeywords").val(JSON.stringify(keywords));

    //Final confirmation
    if(confirm("Are you sure you're ready to publish this resource?")){
      //Do Post
      $("#btn_finish").attr("disabled", "disabled").attr("value", "Working...");
      document.forms[0].submit();
    }
    else {
      return;
    }

  }

  function toggleColumns() {
    $("#tags").toggleClass("noColumn");
  }

  function addSelectAlls() {
    $(".field[data-schema=gradeLevel] h3").after(
      $("<input></input>")
        .attr("type", "button")
        .attr("value", "All High School")
        .attr("id", "btn_allHighSchool")
    );
    $(".field[data-schema=gradeLevel] h3").after(
      $("<input></input>")
        .attr("type", "button")
        .attr("value", "All Elementary")
        .attr("id", "btn_allElementary")
    );
    $(".field[data-schema=careerCluster] h3").after(
      $("<input></input>")
        .attr("type", "button")
        .attr("value", "All Career Clusters")
        .attr("id", "btn_allClusters")
    );
    $("#btn_allElementary").on("click", function () {
      for (var i = 133; i <= 142; i++) {
        $(".field[data-schema=gradeLevel] input[data-id=" + i + "]").prop("checked", true).trigger("change");
      }
    });
    $("#btn_allHighSchool").on("click", function () {
      for (var i = 143; i <= 144; i++) {
        $(".field[data-schema=gradeLevel] input[data-id=" + i + "]").prop("checked", true).trigger("change");
      }
    });
    $("#btn_allClusters").on("click", function () {
      for (var i = 74; i <= 92; i++) {
        $(".field[data-schema=careerCluster] input[data-id=" + i + "]").prop("checked", true).trigger("change");
      }
    });
  }

</script>
<script type="text/javascript">
  /* AJAX */
  function validateAJAX(method, data, success, schema){
    $.ajax({
      url: "/Services/UtilityService.asmx/" + method,
      data: JSON.stringify(data),
      success: function(msg){ console.log(msg); success($.parseJSON(msg.d), schema); },
      type: "POST",
      contentType: "application/json; charset=utf-8",
      dataType: "json",
    });
  }

  function successValidateItem(msg, schema){
    var box = $(".field[data-schema=" + schema + "]");
    var message = box.find(".validMessage").attr("id");

    if(msg.isValid){
      setValid(message, "green", "Validated", true);
    }
    else {
      setValid(message, "red", msg.status, false);
    }

    tallyRequired();
  }

  function successValidateKeyword(msg, schema){
    var box = $(".field[data-schema=" + schema + "]");
    var message = box.find(".validMessage").attr("id");

    if(msg.isValid){
      if(checkDuplicateKeyword()){
        setValid(message, "red", "That keyword was already entered.");
        return;
      }
      setValid(message, "green-heavy", "Press Enter to add this keyword", true);
    }
    else {
      setValid(message, "red", msg.status, false);
    }

    tallyRequired();
  }
</script>
<script type="text/javascript">
  /* Rendering */
  //Render Keywords
  function renderKeywords(firstRun) {
    var box = $("#addedKeywords");
    var template = $("#template_keyword").html();
    box.html("");
    for (i in keywords) {
      box.append(template.replace(/{word}/g, keywords[i]).replace(/{id}/g, i).replace(/{isPreselected}/g, ""));
      }
    //if(firstRun){
      for(i in preselectedKeywords){
        box.append(template.replace(/{word}/g, preselectedKeywords[i]).replace(/{id}/g, i).replace(/{isPreselected}/g, "preselected"));
      }
      box.find(".preselected input").remove();
    //}
    tallyRequired();
  }
</script>

<style type="text/css">
  /* Big stuff */
  .columnContainer { font-size: 0; }

  /* Shared */
  #basicInfo .field, #tags .field { display: inline-block; width: 100%; }
  #basicInfo .field h3, #tags h3 { font-size: 18px; padding: 2px 5px; }
  .instructions { margin: 5px 10px 0 10px; }
  #basicInfo .field[data-required=true] input:not([type=button]), #basicInfo .field[data-required=true] textarea, #basicInfo .field[data-required=true] select, #tags .field[data-required=true] h3 { border-left: 5px solid #C33; }
  .requiredColor { color: #C33; font-weight: bold; }
  .hidden { font-size: 0; margin: 0; padding: 0; line-height: 0; }
  .validMessage { font-style: italic; padding: 2px 5px; }
  .validMessage[data-valid=green] { color: #3C3; }
  .validMessage[data-valid=red] { color: #C33; }
  .validMessage[data-valid=gray] { color: #777; }
  .validMessage[data-valid=green-heavy] { color: #FFF; background-color: #3C3; border-radius: 5px; margin: 2px; font-style: normal; font-weight: bold; }
  .lblTestingMode { float: right; line-height: 32px; }
  .lblTestingMode input { display: inline-block; vertical-align: middle; }
  .lblTestingMode:hover, .lblTestingMode:focus { cursor: pointer; }

  /* Basic Info */
  #basicInfo { -moz-column-count: 2; -webkit-column-count: 2; column-count: 2; -webkit-column-gap: 25px; -moz-column-gap: 25px; column-gap: 25px; padding: 5px; }
  #basicInfo .field { margin-bottom: 10px; }
  #basicInfo .field h3 i { float: right; font-weight: normal; color: #777; padding-left: 10px; margin-top: 4px; font-size: 14px; }
  #basicInfo textarea { min-height: 4em; max-height: 10em; resize: vertical; }
  #basicInfo input, #basicInfo textarea, #basicInfo select { width: 100%; }
  #basicInfo #standardRights { padding-left: 100px; background: url('') no-repeat left 5px center; clear: both; }
  #basicInfo #usageRightsLink { padding: 2px 5px 5px 5px; }
  #basicInfo .addedKeyword { background-color: #F5F5F5; border-radius: 5px; padding: 2px 5px; display: inline-block; vertical-align: top; margin: 1px 2px; }
  #basicInfo .addedKeyword input { float: right; width: 18px; height: 18px; line-height: 18px; background-color: #C33; color: #FFF; border: 1px solid #D22; border-radius: 5px; font-weight: bold; text-align: center; margin: 0 0 2px 5px; transition: background-color 0.2s; }
  #basicInfo .addedKeyword input:hover, #basicInfo .addedKeyword input:focus { background-color: #F00; }

  /* Standards Browser */

  /* Tags */
  #tags { -moz-column-count: 3; -webkit-column-count: 3; column-count: 3; -webkit-column-gap: 25px; -moz-column-gap: 25px; column-gap: 25px; padding: 5px; }
  #tags .field { margin: 5px 0 25px 0; }
  #tags .field label { display: block; }
  #tags h3 { background-color: #EEE; border-radius: 5px 5px 0 0; }
  #tags .field label span { padding: 2px 5px; }
  #tags label { transition: background-color 0.1s; border-radius: 5px; padding: 1px 5px; }
  #tags label:hover, #tags label:focus { background: #F5F5F5; cursor: pointer; }

  #tags.noColumn { -moz-column-count: initial; -webkit-column-count: initial; column-count: initial; -webkit-column-gap: initial; -moz-column-gap: initial; column-gap: initial; padding: 5px; }
  #tags.noColumn .field label { display: inline-block; vertical-align: top; width: 33%; }
  #tags .field input[type=button] { width: 100%; display: block; }

  /* Finish */
  #finish { position: fixed; height: 50px; bottom: 0; left: 0; right: 0; z-index: 1000; box-shadow: 0 0 50px -1px rgba(0,0,0,0.5); background-color: #EEE; transition: padding 1s, height 1s; }
  #finish .finalBlock { width: 25%; display: inline-block; vertical-align: top; height: 100%; padding: 2px; }
  #finish #btn_finish { width: 100%; height: 100%; font-weight: bold; font-size: 30px; }
  #finish .status { padding: 2px 5px; }

  /* Media */
  @media (min-width: 1300px) {
    #tags { -moz-column-count: 4; -webkit-column-count: 4; column-count: 4; }
    #tags.noColumn { -moz-column-count: initial; -webkit-column-count: initial; column-count: initial; }
    #tags.noColumn .field label { width: 24%; }
  }
  @media (max-width: 979px) {
    #finish { padding-bottom: 45px; height: 95px; }
  }
  @media (max-width: 775px) {
    #basicInfo { -moz-column-count: initial; -webkit-column-count: initial; column-count: initial; }
    #tags { -moz-column-count: 2; -webkit-column-count: 2; column-count: 2; }
    #finish { height: 145px; }
    #finish .finalBlock { width: 50%; height: 50%; }
    #tags.noColumn { -moz-column-count: initial; -webkit-column-count: initial; column-count: initial; }
    #tags.noColumn .field label { width: 49%; }
  }
  @media (max-width: 600px) {
    #tags { -moz-column-count: initial; -webkit-column-count: initial; column-count: initial; }
    #tags.noColumn .field label { width: 100%; }
  }
  @media (max-width: 400px) {
    #finish { height: 95px; }
    #finish .finalBlock:not(#validFinal_finish) { display: none; }
    #finish .finalBlock#validFinal_finish { width: 100%; height: 100%; }
  }
</style>

<div id="content">
  <h1 class="isleH1"><label id="lblTestingMode" class="lblTestingMode" runat="server">Testing Mode <input type="checkbox" id="cbx_testingMode" runat="server" /></label> Resource Tagger </h1>

  <!-- Basic information and single value fields -->
  <div id="basicInfoBox">
    <h2 class="isleH2">Basic Information</h2>
    <p class="instructions">Enter basic information below. Items marked with a <span class="requiredColor">red border</span> are required.</p>
    <div id="basicInfo">

      <div class="field" data-schema="url" data-required="true" data-type="url" data-minlength="12" data-unique="<%=( LoadedResourceData.ResourceId == 0 ? "true" : "false" ) %>" data-title="Resource URL">
        <h3>Resource URL <i>The direct URL to the Resource</i></h3>
        <div class="hidden">This field is Required.</div>
        <input type="url" placeholder="http://" name="txtURL" class="mainInput" value="<%=LoadedResourceData.Url %>" <%=( LoadedResourceData.ResourceId > 0 ? "disabled=\"disabled\"" : "" ) %> />
        <div id="valid_txtURL" class="validMessage"></div>
      </div>

      <div class="field" data-schema="title" data-required="true" data-type="text" data-minlength="10" data-title="Resource Title">
        <h3>Resource Title <i>The full title of the Resource</i></h3>
        <div class="hidden">This field is Required.</div>
        <input type="text" name="txtTitle" class="mainInput" value="<%=LoadedResourceData.Title %>" <%=CanUpdate["title"] || LoadedResourceData.ResourceId == 0 ? "" : "disabled=\"disabled\"" %> />
        <div id="valid_txtTitle" class="validMessage"></div>
      </div>

      <div class="field" data-schema="description" data-required="true" data-type="text" data-minlength="25" data-title="Description">
        <h3>Description <i>A good description of the Resource</i></h3>
        <div class="hidden">This field is Required.</div>
        <textarea name="txtDescription" class="mainInput" <%=CanUpdate["description"] || LoadedResourceData.ResourceId == 0 ? "" : "disabled=\"disabled\"" %>><%=LoadedResourceData.Description %></textarea>
        <div id="valid_txtDescription" class="validMessage"></div>
      </div>

      <div class="field" data-schema="keywords" data-required="true" data-minlength="3" data-title="Keyword">
        <h3>Keywords <i>Keywords help others find the Resource</i></h3>
        <div class="hidden">This field is Required.</div>
        <input maxlength="50" type="text" name="txtKeyword" class="txtKeyword mainInput" placeholder="Type a keyword and press Enter" />
        <div id="valid_txtKeyword" class="validMessage"></div>
        <input type="hidden" id="hdnKeywords" class="hdnKeywords" runat="server" />
        <div id="addedKeywords"></div>
      </div>

      <div class="field" data-schema="usageRights" data-type="url" data-required="true" data-unique="false" data-title="Usage Rights" data-minlength="12">
        <h3>Usage Rights <i>Restrictions on using, altering, and/or republishing the Resource</i></h3>
        <div class="hidden">This field is Required.</div>
        <div id="usageRightsSelector" data-title="Usage Rights">
          <div id="standardRights">
            <select name="ddlUsageRights">
              <% foreach(var item in UsageRights) { %>
              <option value="<%=item.Id %>" data-url="<%=item.Url %>" data-description="<%=item.Description %>" data-iscustom="<%=item.Url == "" ? "true" : "false" %>" data-icon="<%=item.IconUrl %>" <%=( LoadedResourceData.UsageRights.Id == item.Id ? "selected=\"selected\"" : "" ) %>><%=item.Title %></option>
              <% } %>
            </select>
            <a id="usageRightsLink" href="#" target="_blank"></a>
          </div>
          <input type="url" name="txtUsageRightsURL" id="txtUsageRightsURL" placeholder="Enter the license URL starting with http://" class="txtUsageRightsURL mainInput" value="<%=LoadedResourceData.UsageRights.Url %>" />
          <div id="valid_txtUsageRightsURL" class="validMessage"></div>
        </div>
      </div>

      <div class="field" data-schema="inLanguage">
        <h3>Language <i>The primary language the Resource is written in</i></h3>
        <div class="hidden">This field is Required.</div>
        <select name="ddlLanguage">
          <% foreach(var item in Fields.Where(m=> m.Schema == "inLanguage").FirstOrDefault().Tags) { %>
          <option value="<%=item.Id %>" <%=( LoadedResourceData.Fields.Where(m => m.Schema == "inLanguage").FirstOrDefault().Tags.Where(t => t.Selected).FirstOrDefault().Id == item.Id ? "selected=\"selected\"" : "" ) %>><%=item.Title %></option>
          <% } %>
        </select>
      </div>

      <div class="field" data-schema="accessRights">
        <h3>Access Rights <i>Requirements for accessing the Resource, if any</i></h3>
        <div class="hidden">This field is Required.</div>
        <select name="ddlAccessRights">
          <% foreach(var item in Fields.Where(m => m.Schema == "accessRights").FirstOrDefault().Tags) {  %>
          <option value="<%=item.Id %>" <%=( LoadedResourceData.Fields.Where(m => m.Schema == "accessRights").FirstOrDefault().Tags.Where(t => t.Selected).FirstOrDefault().Id == item.Id ? "selected=\"selected\"" : "" ) %>><%=item.Title %></option>
          <% } %>
        </select>
      </div>

      <div class="field" data-schema="creator">
        <h3>Creator <i>The original creator of the Resource</i></h3>
        <input type="text" name="txtCreator" value="<%=LoadedResourceData.Creator %>" />
      </div>

      <div class="field" data-schema="publisher">
        <h3>Publisher <i>The person or entity making the Resource available</i></h3>
        <input type="text" name="txtPublisher" value="<%=LoadedResourceData.Publisher %>" />
      </div>

      <div class="field" data-schema="Requirements">
        <h3>Technical/Equipment Requirements <i>Devices, software, equipment, or other noteworthy things needed to use the Resource</i></h3>
        <input type="text" name="txtRequirements" value="<%=LoadedResourceData.Requirements %>" />
      </div>

      <div class="field" data-schema="Library">
        <h3>Library and Collection <i>You can automatically add this Resource to a Collection in your Library.</i></h3>
        <select id="ddlLibrary" name="ddlLibrary" style="margin-bottom:10px;">
          <option value="0" selected="selected">Select a Library...</option>
        </select>
        <select id="ddlCollection" name="ddlCollection">
          <option value="0" selected="selected">Select a Collection...</option>
        </select>
      </div>

    </div>
  </div><!--/basicInfoBox -->

  <!-- Standards Browser -->
  <div id="standardsBrowser">
    <h2 class="isleH2">Learning Standards</h2>
    <uc1:StandardsBrowser ID="StandardsBrowser" runat="server" />
    <input type="hidden" id="hdnStandards" class="hdnStandards" runat="server" />
  </div><!--/standards browser -->

  <!-- Tags -->
  <div id="tagsBox">
    <h2 class="isleH2">Tagging <input type="button" value="Toggle Columns" onclick="toggleColumns();" style="float:right; width: auto; font-size:14px;" class="isleButton bgBlue" /></h2>
    <p class="instructions">Select all applicable tags below. Lists marked with a <span class="requiredColor">red border</span> require at least one item be selected.</p>
    <div id="tags" class="columnContainer">
      <% var requiredCBXLs = new List<string>() { "mediaType", "learningResourceType" }; %>
      <% var selectedTags = LoadedResourceData.Fields.SelectMany( f => f.Tags ).Where( t => t.Selected ).Select( t => t.Id ).ToList(); %>
      <% foreach(var item in Fields.Where(m => m.Schema != "accessRights" && m.Schema != "inLanguage")) {  %>
      <div class="field" data-schema="<%=item.Schema %>" data-id="<%=item.Id %>" <%=(requiredCBXLs.Contains(item.Schema) ? "data-required=\"true\"" : "") %>>
        <h3><%=item.Title %></h3>
        <% foreach(var tag in item.Tags) { %>
        <label><input type="checkbox" data-category="<%=item.Id %>" data-id="<%=tag.Id %>" name="cbx_<%=tag.Id %>" <%=( selectedTags.Contains(tag.Id) ? "checked=\"checked\" disabled=\"disabled\"" : "" ) %> /><span><%=tag.Title %></span></label>
        <% } %>
      </div>
      <% } %>
    </div>
    <input type="hidden" id="hdnTags" class="hdnTags" runat="server" />
  </div><!--/tagsBox -->

  <!-- Finish -->
  <div id="finish">
    <div id="validFinal_basicInfo" class="finalBlock">
      <h3>Basic Information</h3>
      <div class="status"></div>
    </div><!--
    --><div id="validFinal_standards" class="finalBlock">
      <h3>Learning Standards</h3>
      <div class="status"></div>
    </div><!--
    --><div id="validFinal_tags" class="finalBlock">
      <h3>Tagging</h3>
      <div class="status"></div>
    </div><!--
    --><div id="validFinal_finish" class="finalBlock">
      <input type="button" class="isleButton bgGreen" id="btn_finish" value="Publish!" onclick="publish();" />
      <input type="hidden" id="hdnResourceID" runat="server" value="<%=LoadedResource.Id %>" />
    </div>
  </div><!--/finish -->

</div><!--/content -->

<div id="templates" style="display:none;">
  <div id="template_keyword">
    <div class="addedKeyword {isPreselected}"><input type="button" value="X" onclick="removeKeyword({id});" />{word}</div>
  </div>
</div>