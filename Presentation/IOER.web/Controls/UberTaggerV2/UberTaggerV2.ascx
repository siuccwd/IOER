<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UberTaggerV2.ascx.cs" Inherits="ILPathways.Controls.UberTaggerV2.UberTaggerV2" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/Controls/StandardsBrowser7.ascx" %>

<link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<script type="text/javascript">
  /* --- Server Variables --- */
  var SB7mode = "tag";
  var preselectedKeywords = [];
  var myLibraries = <%=LibColData %>;
</script>
<script type="text/javascript">
  /* --- Live Global Variables --- */
  var currentFileDetails = { url: "", resourceID: 0, contentID: 0 };
  var validationTimers = {
    DataSource: { timer: 0, old: "" },
    Url: { timer: 0, old: "" },
    Upload: { timer: 0, old: "" }
  };
  var valids = {
    Url: false
  };
  var requireds = {
    Url: false
  };
</script>
<script type="text/javascript">
  /* --- Initialization --- */
  $(document).ready(function () {
    removeAddThis();
    setupUsageRights();
    setupLibColDDLs();
    setupFileUpload();
    setupValidations();
  });

  //Remove addthis
  function removeAddThis(){
    if($("[class*=at4]").length == 0){
      setTimeout(removeAddThis, 100);
    }
    else {
      $("[class*=at4]").remove();
    }
  }

  //Setup Usage Rights
  function setupUsageRights() {
    var box = $(".field[data-field=UsageRights]");
    var ddl = box.find("#ddl_UsageRights");
    var link = box.find("#usageRightsLink");
    var imageBox = box.find("#usageRightsSelectorBox");
    var urlBox = box.find("#field_UsageRightsUrl");
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
    });
    ddl.trigger("change");
  }

  //Setup Library/Collection DDLs
  function setupLibColDDLs() {
    var ddlLibrary = $("#ddlLibrary");
    var ddlCollection = $("#ddlCollection");
    for (i in myLibraries) {
      ddlLibrary.append("<option value=\"" + myLibraries[i].Id + "\">" + myLibraries[i].Title + "</option>");
    }
    ddlLibrary.on("change", function () {
      var libID = parseInt($(this).find("option:selected").attr("value"));
      ddlCollection.html("<option selected=\"selected\" value=\"0\">Select a Collection...</option>");
      for (i in myLibraries) {
        if (myLibraries[i].Id == libID) {
          for (j in myLibraries[i].Collections) {
            ddlCollection.append("<option value=\"" + myLibraries[i].Collections[j].Id + "\">" + myLibraries[i].Collections[j].Title + "</option>");
          }
        }
      }
    });
    ddlLibrary.trigger("change");
  }

  //Setup file upload control
  function setupFileUpload() {
    $(window).on("message", function(msg) {
      try {
        var info = $.parseJSON(msg.originalEvent.data);
        if(info.type == "uploadMessage"){
          if(info.valid == true){
            if(info.command == "load"){ } //Page load. do nothing
            else if(info.command == "upload") {
              $("#field_Url").val(info.url).trigger("change").attr("disabled", "disabled").attr("readonly", "readonly");
              enableButton($("#btnUploadFile"), "Change File");
              enableButton($(".btnRemoveFile"), "Remove File");
              $(".btnRemoveFile").show();
              setValid("Upload", info.status, "green", null);
            }
            else if(info.command == "remove"){
              $("#field_Url").val("").trigger("change").attr("disabled", false).attr("readonly", false);
              enableButton($("#btnUploadFile"), "Change File");
              enableButton($(".btnRemoveFile"), "Remove File");
              $(".btnRemoveFile").hide();
              setValid("Upload", info.status, "green", null);
            }

            $(".hdnFileDetails").val(JSON.stringify(info));
            currentFileDetails.url = info.url;
            currentFileDetails.contentID = info.contentID;
          }
          else {
            setValid("Upload", info.status, "red", null);
            enableButton($("#btnUploadFile"), "Upload");
            enableButton($(".btnRemoveFile"), "Remove File");
          }
        }
      }
      catch(e){}
    });
  }

  //Setup validations
  function setupValidations() {
    //URL
    $("#field_Url").on("keyup change", function() {
      resetValidationTimer("Url", validateURL, 800, $("#field_Url").val());
    });
  }
</script>
<script type="text/javascript">
  /* --- Page Functions --- */
  //Upload a file
  function uploadFile() {
    disableButton($("#btnUploadFile"), "Working...");
    disableButton($(".btnRemoveFile"), "Please Wait...");
    setValid("Upload", "Uploading file, please wait...", "gray", null);
    $("#iframe_File")[0].contentWindow.postMessage(JSON.stringify({command: "upload", info: currentFileDetails}), "*");
  }
  //Remove a file
  function removeFile() {
    if(confirm("Are you sure you want to remove this file?")){
      disableButton($("#btnUploadFile"), "Please Wait...");
      disableButton($(".btnRemoveFile"), "Working...");
      $("#iframe_File")[0].contentWindow.postMessage(JSON.stringify({command: "remove", info: currentFileDetails}), "*");
    }
  }

  //Disable a button
  function disableButton(button, message){
    button.attr("data-text", button.attr("value"));
    button.attr("value", message);
    button.attr("disabled", "disabled");
  }
  //Enable a button
  function enableButton(button, message){
    button.attr("value", typeof(message) == undefined ? button.attr("data-text") : message);
    button.attr("disabled", false);
  }
</script>
<script type="text/javascript">
  /* --- AJAX --- */
  //Do AJAX
  function doAJAX(service, method, data, success){
    $.ajax({
      url: "/Services/" + service + ".asmx/" + method,
      data: JSON.stringify(data),
      success: function(msg){ console.log(msg); success($.parseJSON(msg.d)); },
      async: true,
      headers: { "Accept": "application/json", "Content-type": "application/json; charset=utf-8" },
      dataType: "json",
      type: "POST"
    });
  }

  //Validate URL
  function success_validateURL(result){
    if(result.isValid){
      setValid("Url", "URL is valid", "green", true);
    }
    else {
      setValid("Url", result.status, "red", false);
    }
  }

</script>
<script type="text/javascript">
  /* --- Rendering --- */

</script>
<script type="text/javascript">
  /* --- Validation --- */
  //Do validation
  function validateURL() {
    var text = $("#field_Url").val().trim();
    if(text == ""){
      setValid("Url", "", "", false);
    }
    else {
      setValid("Url", "Checking URL...", "gray", false);
      doAJAX("UtilityService", "ValidateUrl", { text: text, mustBeNew: true }, success_validateURL);
    }
  }

  //Set validation message
  function setValid(field, message, css, valid){
    $(".validationMessage[data-field=" + field + "]").attr("class", "validationMessage " + css).html(message);
    if(valid != null && typeof(valid) != "undefined"){
      valids[field] = valid;
    }
    //checkPublishValidations();
  }

  //Set status bar
  function setMainStatus(text, css){
    $("#globalValidationMessage").html(text);
    $("#footer").attr("class", css);
  }

  //Reset validation timer
  function resetValidationTimer(field, method, time, oldValue){
    if(validationTimers[field].old != oldValue){
      clearTimeout(validationTimers[field].timer);
      validationTimers[field].timer = setTimeout(method, time);
      validationTimers[field].old = oldValue;
    }
  }

  //Validate before publishing
  function checkPublishValidations(){



    /*  ---  ---  TODO: rewrite this ---  ---  */


    var errors = [];
    //URL
    if($("#field_Url").val().trim().length == 0){
      errors.push("Resource URL cannot be empty.");
    }

    //Update status bar
    var allValid = true;
    var invalids = 0;
    var needRequireds = 0;
    for(var i in valids){
      if(typeof(requireds[valids[i]]) != "undefined"){
        requireds[valids[i]] = valids[i];
        if(!valids[i]){
          needRequireds++;
        }
      }
      if(!valids[i]){
        allValid = false;
        invalids++;
      }
    }
    if(!allValid){
      setMainStatus(invalids + " field(s) are invalid", "red");
    }
    else if(needRequireds > 0) { //TODO: determine this
      setMainStatus("Ready to publish!", "green");
    }
    else {
      setMainStatus("", "gray");
    }

    return errors;
  }

  //Publish
  function doPublish() {
    var errors = checkPublishValidations();
    if(errors.length > 0){
      //Count and display
      var text = "";
      for(var i in errors){
        text += errors[i] + "\n";
      }
      alert(text);
      return;
    }
    else {
      if(confirm("Are you sure you're ready to publish this Resource?")){
        //Do publish
      }
    }
  }
</script>
<script type="text/javascript">
  /* --- Miscellaneous --- */

</script>

<style type="text/css">
  /* Big Items */
  #mainContent input:not([type=checkbox]), #mainContent select, #mainContent textarea, #mainContent #tags label { display: block; width: 100%; }
  .column { display: inline-block; vertical-align: top; width: 50%; padding: 0 10px; }
  #mainContent h2 { font-size: 30px; }
  .step { margin-bottom: 25px; padding: 0 5px; }
  .step > h2 { padding-left: 10px; background-color: #EEE; border-radius: 5px; margin-bottom: 5px; }

  /* Data Source */
  #iframe_File { width: 100%; height: 25px; border: none; }
  #dataSource ul li { margin-bottom: 5px; }
  #dataSource .field, #basicInfo .field { margin-bottom: 25px; }
  #dataSource .field h3, #basicInfo .field h3 { font-size: 20px; }
  #dataSource .field h3 i, #basicInfo .field h3 i { float: right; font-weight: normal; font-style: italic; color: #555; margin-left: 10px; }
  #mainContent input.btnRemoveFile { display: none; }

  /* Basic Info */
  #basicInfo #field_Description { resize: vertical; height: 10em; min-height: 8em; max-height: 20em; }
  #basicInfo #usageRightsSelectorBox { padding-left: 100px; background: url('') no-repeat left 5px center; clear: both; }
  #basicInfo #usageRightsLink { padding: 2px 5px 5px 5px; }

  /* Tags */
  #tags #tagFields { -moz-column-count: 3; -webkit-column-count: 3; column-count: 3; -webkit-column-gap: 25px; -moz-column-gap: 25px; column-gap: 25px; padding: 5px; }
  #tags .field { display: inline-block; width: 100%; vertical-align: top; margin-bottom: 25px; }
  #tags .field h3 { background-color: #EEE; border-radius: 5px; padding: 2px 5px; margin: 0 0 5px 0; font-size: 18px; }
  #tags .field label { padding: 2px 5px; transition: background-color 0.2s; border-radius: 5px; }
  #tags .field label:hover, #tags .field label:focus { background-color: #F5F5F5; cursor: pointer; }

  /* Footer */
  #footerDiv { margin-bottom: 35px; }
  #footer { position: fixed; background-color: #EEE; box-shadow: 0 0 15px #CCC; transition: box-shadow 0.3s, background-color 0.3s, color 0.3s; bottom: 0; left: 0; right: 0; height: 34px; padding: 2px 5px; }
  #footer #btnFinish { width: 25%; height: 30px; float: right; }
  #footer.green { background-color: #4AA394; box-shadow: 0 0 25px #4AA394; color: #FFF; }
  #footer.red { background-color: #D33; color: #FFF; box-shadow: 0 0 20px #E22; }

  /* Validation */
  .validationMessage { padding: 2px 5px; border-radius: 5px; transition: color 0.5s, background-color 0.5s, padding 0.5s; }
  body .validationMessage:empty { padding: 0; }
  .validationMessage.green { color: #4AA394; font-style: italic; }
  .validationMessage.red { background-color: #B03D25; color: #FFF; font-weight: bold; padding: 5px 10px; }
  .validationMessage.gray { color: #999; font-style: italic; }
  .validationMessage.red a { color: #FFF; text-decoration: underline; }

  /* Miscellaneous */
  .lblTestingMode { float: right; line-height: 32px; }
  .lblTestingMode input { display: inline-block; vertical-align: middle; }
  .lblTestingMode:hover, .lblTestingMode:focus { cursor: pointer; }
  #ddlLibrary { margin-bottom: 5px; }
  .field[data-field=Standards] h3 { padding: 0 5px; }

  /* Responsive */
  .fullscreen { display: inline; }
  .mobile { display: none; }

  @media (min-width: 1300px) {
    #tags #tagFields { -moz-column-count: 4; -webkit-column-count: 4; column-count: 4; }
  }
  @media (max-width: 700px) {
    .fullscreen { display: none; }
    .mobile { display: inline; }
    .column { display: block; width: 100%; }
    #tags #tagFields { -moz-column-count: 2; -webkit-column-count: 2; column-count: 2; }
  }
  @media (max-width: 600px) {
    #tags #tagFields { -moz-column-count: initial; -webkit-column-count: initial; column-count: initial; }
  }
</style>

<div id="mainContent">
  <h1 class="isleH1"><label id="lblTestingMode" class="lblTestingMode" runat="server">Testing Mode <input type="checkbox" id="cbx_testingMode" runat="server" /></label>Resource Tagger </h1>

  <!-- URL (direct) or URL From File -->
  <div id="dataSource" class="step">
    <h2>1. The Resource</h2>
    <div class="column">
      <p>First, provide a URL to the Resource.</p>
      <ul>
        <li>If the Resource is a web page, just paste its URL into the box <span class="fullscreen">to the right</span><span class="mobile">below</span>.</li>
        <li>If the Resource is a file online that you can link to, paste the URL to the file in the box <span class="fullscreen">to the right</span><span class="mobile">below</span>. Make sure this is a link others can use to access the Resource.</li>
        <li>If the Resource is a file that does <b>not</b> exist online in a publicly-accessible location, upload it with the option <span class="fullscreen">to the right</span><span class="mobile">below</span>. We'll host the file and provide the URL for you.</li>
        <li style="color:#D33;">Once you <b>publish</b> a file, you may change the file later, but you <u>will not</u> be able to use a URL instead of a file for this Resource.</li>
      </ul>
      <p><input type="button" value="What kind of file can I upload?" onclick="showFileLimits()" /></p>
    </div><!--
    --><div class="column">
      <div data-item="Url" class="field">
        <h3>Resource URL <i>Direct URL to the Resource</i></h3>
        <input type="text" id="field_Url" placeholder="http://" />
        <div class="validationMessage" data-field="Url"></div>
      </div>
      <div data-item="File" class="field">
        <h3>Resource Upload <i>Only use this if the Resource doesn't exist online</i></h3>
        <iframe src="/controls/ubertaggerv2/upload.aspx" id="iframe_File"></iframe>
        <input type="hidden" id="hdnFileDetails" class="hdnFileDetails" runat="server" />
        <input type="button" id="btnUploadFile" value="Upload" onclick="uploadFile()" />
        <input type="button" id="btnRemoveFile" class="btnRemoveFile" runat="server" value="Remove File" onclick="removeFile()" />
        <div class="validationMessage" data-field="Upload"></div>
      </div>
      <div class="validationMessage" data-field="DataSource"></div>
    </div>
  </div><!-- /dataSource -->


  <!-- Basic Information -->
  <div id="basicInfo" class="step">
    <h2>2. Basic Information</h2>

    <div class="column">
      <!-- Title -->
      <div class="field required" data-field="Title">
        <h3>Resource Title <i>The full title of the Resource</i></h3>
        <input type="text" id="field_Title" placeholder="http://" />
        <div class="validationMessage" data-field="Title"></div>
      </div>

      <!-- Description -->
      <div class="field required" data-field="Description">
        <h3>Description <i>A good description of the Resource</i></h3>
        <textarea id="field_Description"></textarea>
        <div class="validationMessage" data-field="Description"></div>
      </div>

      <!-- Keywords -->
      <div class="field required" data-field="Keywords">
        <h3>Keywords <i>Keywords help others find the Resource</i></h3>
        <p>Add keywords (or phrases) by typing them one at a time below <b>and pressing <u>Enter</u> after each word or phrase</b>.</p>
        <input type="text" id="field_Keyword" placeholder="Type a keyword or phrase and press Enter" />
        <div id="preexistingKeywords"></div>
        <div id="enteredKeywords"></div>
        <div class="validationMessage" data-field="Keywords"></div>
      </div>

    <% if(CurrentTheme.Name == "quick"){ %>
    </div><div class="column">
    <% } %>

      <!-- Usage Rights -->
      <div class="field required" data-field="UsageRights">
        <h3>Usage Rights <i>Restrictions on using, altering, and/or republishing the Resource</i></h3>
        <div id="usageRightsSelectorBox">
          <select id="ddl_UsageRights">
            <% foreach(var item in UsageRights) { %>
              <option value="<%=item.Id %>" data-url="<%=item.Url %>" data-description="<%=item.Description %>" data-iscustom="<%=item.Url == "" ? "true" : "false" %>" data-icon="<%=item.IconUrl %>"><%=item.Title %></option>
            <% } %>
          </select>
          <a id="usageRightsLink" href="#"></a>
        </div>
        <input type="text" id="field_UsageRightsUrl" placeholder="Enter the license URL starting with http://" />
        <div class="validationMessage" data-field="UsageRights"></div>
      </div>

    <% if( CurrentTheme.Name == "worknet"){ %>
    </div><div class="column">
    <% } %>

      <!-- IOER ContentPrivileges -->
      <div class="field required" data-field="ContentPrivileges">
        <h3>IOER Access Limitations <i>Who can access this Resource?</i></h3>
        <p class="tip">If you select "Anyone can Access", the Resource will be published to the Learning Registry and freely available for anyone to access. If you need to restrict access to the Resource (e.g., for tests and answer keys), select the group of users that will be allowed to see it.</p>
        <select id="ddl_ContentPrivileges">
          <% foreach(var item in ContentPrivileges){ %>
          <option value="<%=item.Id %>"><%=item.Title %></option>
          <% } %>
        </select>
        <div class="validationMessage" data-field="ContentPrivileges"></div>
      </div>

    <% if(CurrentTheme.Name == "ioer" || CurrentTheme.Name == "uber" ){ %>
    </div><div class="column">
    <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Language")){ %>
      <% var targetField = CurrentTheme.VisibleTagData.FirstOrDefault( m => m.Schema == "inLanguage" ); %>
      <!-- Language -->
      <div class="field" data-field="Language">
        <h3>Language <i>The primary language the Resource is written in</i></h3>
        <select id="ddl_Language">
          <% foreach(var item in targetField.Tags){ %>
          <option value="<%=item.Id %>"><%=item.Title %></option>
          <% } %>
        </select>
        <div class="validationMessage" data-field="Language"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("AccessRights")){ %>
      <% var targetField = CurrentTheme.VisibleTagData.FirstOrDefault( m => m.Schema == "accessRights" ); %>
      <!-- Access Rights -->
      <div class="field" data-field="AccessRights">
        <h3>Access Rights <i>Requirements for accessing the Resource, if any</i></h3>
        <select id="ddl_AccessRights">
          <% foreach(var item in targetField.Tags){ %>
          <option value="<%=item.Id %>"><%=item.Title %></option>
          <% } %>
        </select>
        <div class="validationMessage" data-field="AccessRights"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Creator")){ %>
      <!-- Creator -->
      <div class="field" data-field="Creator">
        <h3>Creator <i>The original creator of the Resource</i></h3>
        <input type="text" id="field_Creator" />
        <div class="validationMessage" data-field="Creator"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Publisher")){ %>
      <!-- Publisher -->
      <div class="field" data-field="Publisher">
        <h3>Publisher <i>The person or entity making the Resource available</i></h3>
        <input type="text" id="field_Publisher" />
        <div class="validationMessage" data-field="Publisher"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Requirements")){ %>
      <!-- Requirements -->
      <div class="field" data-field="Requirements">
        <h3>Technical/Equipment Requirements <i>Devices, software, equipment, or other noteworthy things needed to use the Resource</i></h3>
        <input type="text" id="field_Requirements" />
        <div class="validationMessage" data-field="Requirements"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("LibraryAndCollection")){ %>
      <!-- Library and Collection -->
      <div class="field" data-field="LibraryAndCollection">
        <h3>Library and Collection <i>You can automatically add this Resource to a Collection in your Library</i></h3>
        <select id="ddlLibrary"></select>
        <select id="ddlCollection"></select>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Organization")){ %>
      <!-- Organization -->
      <div class="field" data-field="Organization">
        <h3>Organization <i>You can tag this Resource on behalf of your Organization</i></h3>
        <select id="ddlOrganization">
          <option value="0">None (not tagging on behalf of an organization)</option>
          <% foreach(var item in OrganizationData){ %>
          <option value="<%=item.Id %>"><%=item.Organization %></option>
          <% } %>
        </select>
      </div>
      <% } %>

    </div>

    <% if(CurrentTheme.VisibleSingleValueFields.Contains("Standards")){ %>
    <!-- Learning Standards -->
    <div class="field" data-field="Standards">
      <h3>Learning Standards <i>Learning Standards to which the Resource aligns</i></h3>
      <uc1:StandardsBrowser ID="standardsBrowser" runat="server" />
    </div>
    <% } %>

  </div><!-- /basic info --><!--
          Tags
  --><div id="tags" class="step">
    <h2>3. Tags</h2>
    <div id="tagFields">
      <% foreach(var item in CurrentTheme.VisibleTagData){ %>
      <% if ( item.Schema == "inLanguage" || item.Schema == "accessRights" ) { continue; } %>
      <div class="field" data-schema="<%=item.Schema %>">
        <h3><%=item.Title %></h3>
        <% foreach(var tag in item.Tags){ %>
        <label>
          <input type="checkbox" data-category="<%=item.Id %>" data-id="<%=tag.Id %>" name="cbx_<%=tag.Id %>" />
          <span> <%=tag.Title %></span>
        </label>
        <% } %>
      </div>
      <% } %>
    </div>
  </div><!-- /tags -->
</div><!-- /content -->

<div id="footer">
  <div id="globalValidationMessage">

  </div><!--
  --><input type="button" id="btnFinish" value="Finish!" />
</div><!-- /footer -->

<div id="templates" style="display:none;">


</div>