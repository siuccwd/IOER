<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UberTaggerV2.ascx.cs" Inherits="IOER.Controls.UberTaggerV2.UberTaggerV2" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/Controls/StandardsBrowser7.ascx" %>

<% var serializer = new System.Web.Script.Serialization.JavaScriptSerializer(); %>
<link rel="stylesheet" type="text/css" href="/styles/common2.css" />

<script type="text/javascript">
  /* --- Server Variables --- */
  var SB7mode = "tag";
  var curriculumMode = true;
  var preselectedKeywords = <%=serializer.Serialize( Resource.Keywords ) %>;
	var preselectedStandards = <%=serializer.Serialize( Resource.Standards ) %>;
  var addedKeywords = []
  var myLibraries = <%=LibColData %>;
	var resourceID = <%=Resource.ResourceId %>;
	var versionID = <%=Resource.VersionId %>;
	var contentID = <%=Resource.ContentId %>;
	var updateMode = resourceID > 0;
</script>
<script type="text/javascript">
  /* --- Live Global Variables --- */
	var currentFileDetails = { url: "", resourceID: <%=Resource.ResourceId %>, contentID: <%=Resource.ContentId %> };
</script>
<script type="text/javascript">
  /* --- Initialization --- */
  $(document).ready(function () {
    removeAddThis();
    setupUsageRights();
    setupLibColDDLs();
    setupFileUpload();
    setupValidationItems();
    setupValidationCBXLs();
    setupKeywords();
    validateForm();
    setupMode();
    setupQuickTagButtons();
    setupContentTagging();
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
    var box = $(".field[data-field=UsageRightsUrl]");
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
    setTimeout(function() { $("#field_UsageRightsUrl").trigger("change"); }, 500);
  }

  //Setup Library/Collection DDLs
  function setupLibColDDLs() {
    var ddlLibrary = $("#ddlLibrary");
    var ddlCollection = $("#ddlCollection");
    ddlLibrary.append("<option selected=\"selected\" value=\"0\">Select a Library</option>");
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
    //when the window receives a postMessage...
  	$(window).on("message", function(msg) {
  		//console.log("PostMessage received: ", msg);
      try {
        //Attempt to determine if the message is relevant to uploads
        var info = $.parseJSON(msg.originalEvent.data);
        //If it's an upload related message, process it
        if(info.type == "uploadMessage"){
          //If it's valid...
          if(info.valid == true){
            //Page load. Do nothing special, but continue
            if(info.command == "load"){ }
            //If the command was to upload a file
            else if(info.command == "upload") {
            	//Set the Resource URL to be the returned URL of the file. Trigger its change event, then disable it to avoid user tampering. Then enable buttons for file manipulation.
            	var url = info.url.toLowerCase().indexOf("/content/") == 0 ? window.location.origin + info.url : info.url;
              $("#field_Url").val(url).trigger("change").attr("disabled", "disabled").attr("readonly", "readonly");
              enableButton($("#btnUploadFile"), "Change File");
              enableButton($(".btnRemoveFile"), "Remove File");
              $(".btnRemoveFile").show();
              setStatus("Upload", "green", info.status);
              $("#uploadWarning").hide();
            }
            //If the command was to remove the file
            else if(info.command == "remove"){
              //Reset the URL box and trigger its change event. Reenable the file manipulation buttons
              $("#field_Url").val("").trigger("change").attr("disabled", false).attr("readonly", false);
              enableButton($("#btnUploadFile"), "Change File");
              enableButton($(".btnRemoveFile"), "Remove File");
              $(".btnRemoveFile").hide();
              setStatus("Upload", "green", info.status);
              $("#uploadWarning").show();
            }

            //Regardless, set data based on what was returned
            $(".hdnFileDetails").val(JSON.stringify(info));
            currentFileDetails.url = info.url;
            currentFileDetails.contentID = info.contentID;

          }
          //If it wasn't valid, set status and reenable buttons
          else {
            setStatus("Upload", "red", info.status);
            enableButton($("#btnUploadFile"), "Upload");
            enableButton($(".btnRemoveFile"), "Remove File");
            $("#uploadWarning").show();
          }
        }
      }
      catch(e){
        console.log(e);
      }
    });
  }

  //Setup Keywords
  function setupKeywords(){
    $("#field_Keyword").on("keyup", function(e){
      if((e.which == 13 || e.keyCode == 13) && $(".validationMessage[data-field=Keyword]").attr("data-status") == "green" ){
        var word = $("#field_Keyword").val().trim();
        doAJAX("UtilityService", "ValidateText", { text: word, minimumLength: 3, fieldTitle: "keyword" }, function(msg){ success_validateKeyword(msg, word); });
      }
    });
  }

	//Determine which mode to default to
  function setupMode() {
  	var mode = <%=((Request.Params["mode"] ?? "").ToLower() == "file" ? "true" : "false") %>;
  	if(mode){
			setMode("File");
		}
		else {
			setMode("Url");
		}
  }

	//Enable quick tagging of certain groups of checkboxes
	function setupQuickTagButtons() {
		$("#tagFields input[data-grades=elementary]").on("click", function() {
			var grades = ["Grade 1", "Grade 2", "Grade 3", "Grade 4", "Grade 5", "Grade 6", "Grade 7", "Grade 8"];
			autoCheckList($("#tagFields .field[data-schema=gradeLevel]"), grades)
		});
		$("#tagFields input[data-grades=highschool]").on("click", function() {
			var grades = ["Grades 9-10", "Grades 11-12"];
			autoCheckList($("#tagFields .field[data-schema=gradeLevel]"), grades)
		});
	}

	//Trigger validation of relevant fields when tagging content for the first time
	function setupContentTagging() {
		$("#field_Url").trigger("change");
		$("#field_Title").trigger("change");
		$("#field_Description").trigger("change");
		$("#field_Creator").trigger("change");
		$("#field_Publisher").trigger("change");
	}
</script>
<script type="text/javascript">
  /* --- Page Functions --- */
  //Upload a file
  function uploadFile() {
    disableButton($("#btnUploadFile"), "Working...");
    disableButton($(".btnRemoveFile"), "Please Wait...");
    //setValid("Upload", "Uploading file, please wait...", "gray", null);
    setStatus("Upload", "gray", "Uploading file, please wait...");
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

  //Show a preformatted alert
  function showMessage(id){
    alert($("#" + id).html());
  }

  //Remove a keyword
  function removeKeyword(word){
    var newList = [];
    for(var i in addedKeywords){
      if(addedKeywords[i].toLowerCase() != word.toLowerCase()){
        newList.push(addedKeywords[i]);
      }
    }
    addedKeywords = newList;
    renderKeywords();
    validateForm();
  }

	//Auto check a list of checkboxes
  function autoCheckList(jqList, targets){
  	jqList.find("label").each(function() {
  		var label = $(this);
  		if(targets.indexOf(label.text().trim()) > -1){
  			label.find("input").prop("checked", true).trigger("change");
  		}
  	});
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
  function success_validateURL(result, data){
    if(result.isValid){
      setStatus(data, "green", "Validated");
    }
    else {
      setStatus(data, "red", result.status);
    }
    validateForm();
  }

  //Validate Text
  function success_validateText(result, data) {
    if(result.isValid){
      setStatus(data, "green", "Validated");
    }
    else {
      setStatus(data, "red", result.status);
    }
    validateForm();
  }

  //Validate keyword
  function success_validateKeyword(result, word){
    if(result.isValid){
      addedKeywords.push(word);
      renderKeywords();
      $("#field_Keyword").val("").trigger("change");
      setStatus(getValidationData("Keyword"), "green", "");
    }
    else {
      setStatus(getValidationData("Keyword"), "red", result.status);
    }
    validateForm();
  }

</script>
<script type="text/javascript">
  /* --- Rendering --- */

  function renderKeywords() {
    var box = $("#enteredKeywords");
    var template = $("#template_addedKeyword").html();
    box.html("");
    for(var i in addedKeywords){
      box.append(template
        .replace(/{word}/g, addedKeywords[i])
      );
    }
  }

	//Show the URL or Upload box
  function setMode(mode){
  	$(".field[data-item=Url], .field[data-item=File]").hide();
  	$(".field[data-item=" + mode + "]").show();
  	$("#modeButtons input").removeClass("current").filter("[data-mode=" + mode + "]").addClass("current");
  }
</script>
<script type="text/javascript">
  /* --- Validation Engine --- */
  //This holds the data needed to run the engine
  var validationData = [
    { id: "Url", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: true, minLength: 10, validate: function() { validateURL(this, !updateMode); } },
    { id: "Upload", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: false, minLength: 0, validate: function() { } },
    { id: "Title", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: true, minLength: 3, validate: function() { validateText(this); } },
    { id: "Description", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: true, minLength: 25, validate: function() { validateText(this); } },
    { id: "Keyword", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: false, minLength: 3, validate: function() { validateKeyword(this); } },
    { id: "UsageRightsUrl", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: true, minLength: 10, validate: function() { validateUseRightsUrl(this); } },
    { id: "Creator", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: false, minLength: 3, validate: function() { validateText(this); } },
    { id: "Publisher", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: false, minLength: 3, validate: function() { validateText(this); } },
    { id: "Requirements", value: "", previousValue: "", timer: 0, status: "neutral", message: "", required: false, minLength: 0, validate: function() { validateText(this); } },
  ];
  var validationCBXLs = [
    { schema: "learningResourceType", title: "Resource Type", status: "neutral", message: "", required: true, found: false },
    { schema: "mediaType", title: "Media Type", status: "neutral", message: "", required: true, found: false }
  ];
  //This is a list of items to worry about validating
  var activeValidationItems = ["Url", "Title", "Description", "Keyword", "UsageRightsUrl"].concat(<%=(new System.Web.Script.Serialization.JavaScriptSerializer().Serialize( CurrentTheme.VisibleSingleValueFields )) %>);

  //Setup the list of validation items
  function setupValidationItems(){
    for(var i in activeValidationItems){
      try {
        var data = getValidationData(activeValidationItems[i]);
        //Use a closure to make sure the data is passed correctly
        (function(data){
          $("#field_" + data.id).on("keyup change", function() { 
            data.validate(); 
          });
        })(data);
        if(data.required && updateMode){
        	data.validate();
        }
      }
      catch(e) { }
    }
  }

  //Setup required checkbox lists
  function setupValidationCBXLs() {
    for(var i in validationCBXLs){
      var current = validationCBXLs[i];
      var list = $(".field[data-schema=" + current.schema + "]");
      if(list.length == 0){ continue; }
      else { current.found = true; }
      (function(current, list){
        list.find("input[type=checkbox]").on("change", function() {
        	if(list.find("input[type=checkbox]:checked, input[type=checkbox]:disabled").length > 0){
            setStatus(current, "green", "Validated");
          }
          else {
            setStatus(current, "neutral", "");
          }
          validateForm();
        });
        if(updateMode && current.required){
        	list.find("input").first().trigger("change");
        }
      })(current, list);
    }
  }

  //Get a validation item by ID
  function getValidationData(id){
    for(var i in validationData){
      if(validationData[i].id == id){
        return validationData[i];
      }
    }
  }

  //Set validation status
  function setStatus(item, status, message){
    if(typeof(item) == "string"){
      item = getValidationData(item);
    }
    $(".validationMessage[data-field=" + item.id + "]").attr("data-status", status).html(message);
    item.status = status;
    item.message = message;
  }

  //Validate a text box
  function validateText(item){
    //Validate core
    if(validateTextCore(item)){
      //Reset the timeout for validating this item
      clearTimeout(item.timer);
      item.timer = setTimeout(function() { 
        //Once the AJAX begins, tell the user
        setStatus(item, "checking", "Checking...");
        doAJAX("UtilityService", "ValidateText", { text: item.value, minimumLength: item.minLength, fieldTitle: item.id }, function(msg){ success_validateText(msg, item); });
      }, 800);
    }
    validateForm();
  }

  //Validate Resource URL
  function validateURL(item, requireNew){
    //Validate core
    if(validateTextCore(item)){
      clearTimeout(item.timer);
      item.timer = setTimeout(function() { 
        //Once the AJAX begins, tell the user
        setStatus(item, "checking", "Checking...");
        doAJAX("UtilityService", "ValidateURL", { text: item.value, mustBeNew: requireNew }, function(msg) { success_validateURL(msg, item); });
      }, 800);
    }
  }

  //Validate keywords
  function validateKeyword(item){
    item.value = $("#field_Keyword").val();
    for(var i in addedKeywords){
      if(addedKeywords[i].toLowerCase() == item.value.toLowerCase()){
        setStatus(item, "red", "That keyword already exists");
        return;
      }
    }
    if(validateTextCore(item)){
      setStatus(item, "green", "Press Enter to add this keyword");
    }
    validateForm();
  }

	//Usage Rights validation handling
  function validateUseRightsUrl(item){
		//Skip validation for unknown/blank
  	if($("#ddl_UsageRights option:selected").attr("data-isunknown") == "true"){
  		var msg = { isValid: true, status: "" };
  		success_validateURL(msg, item);
  	}
  	else {
  		validateURL(item, false);
  	}
  }

  //Validate text that may or may not just be text
  function validateTextCore(item){
    //Get value
    var value = $("#field_" + item.id).val().trim();
    item.value = value;
    //If same as before, do nothing
    if(value == item.previousValue){ 
      validateForm(); return false; 
    }
    //Otherwise set previous value to current
    item.previousValue = value;
    //If empty, return to initial/neutral state
    if(value == ""){ 
      setStatus(item, "neutral", ""); 
      validateForm(); 
      return false; 
    }
    //Otherwise, do a length check
    if(value.length < item.minLength){ setStatus(item, "gray", "Please enter " + (item.minLength - value.length) + " more characters."); return false; }
    //If it's long enough, return to neutral (in case the user types more than the minimum length)
    setStatus(item, "neutral", "");
    validateForm();
    return true;
  }

  //Check the validation objects for fields and tags
  function validateForm() {
    var requiredItems = 0;
    var completedItems = 0;
    var errors = 0;
    for(var i in activeValidationItems){
      var item = getValidationData(activeValidationItems[i]);
      if(typeof(item) == "undefined"){ continue; } //drop-down lists aren't validated due to forced user choice
      if(item.status == "red"){
        errors++;
      }
      if(item.required){
        requiredItems++;
        if(item.status == "green"){
          completedItems++;
        }
      }
    }
    for(var i in validationCBXLs){
      if(validationCBXLs[i].found == 0){
        continue;
      }
      requiredItems++;
      if(validationCBXLs[i].status == "red"){
        errors++;
      }
      if(validationCBXLs[i].status == "green"){
        completedItems++;
      }
    }

    requiredItems++;
    //var keywordData = getValidationData("Keyword");
    if(addedKeywords.length > 0 || preselectedKeywords.length > 0){
      completedItems++;
    }

    if(errors > 0){
      $("#globalValidationMessage").attr("data-status", "red").html( errors + " item(s) above need your attention");
      return false;
    }
    else if (requiredItems > completedItems) {
      $("#globalValidationMessage").attr("data-status", "neutral").html(completedItems + " of " + requiredItems + " required items completed");
      return false;
    }
    else {
      $("#globalValidationMessage").attr("data-status", "green").html("Ready to publish!");
      return true;
    }
  }
</script>
<script type="text/javascript">
  /* --- Publishing --- */
  //Finish and publish
	function finish() {
    if(validateForm()){
    	if(!confirm("Are you sure you're ready to publish this information?")) { 
    		return; 
    	}
      packStandards();
      packKeywords();
      $("#field_Url").attr("disabled", "disabled").attr("readonly", "readonly");
      $("#btnFinish").attr("disabled", "disabled").attr("value", "Working, please wait...");
      $("#globalValidationMessage").addClass("processing").html("We are processing your request. This may take a few moments.");
    	
    	//AJAX submit so that user choices are not lost in a postback in the event of a problem
      //var ajaxPostData = getAjaxPostData();
      var resourceDTO = getResourceDTO();

      doAJAX("ResourceService", "AjaxSaveResource", { input: resourceDTO, testingMode: $(".cbxTestingMode").prop("checked") }, successSaveResource);

      //$("form").removeAttr("onsubmit")[0].submit();
    }
    else {
    	var problems = "";
			//URL
    	var urlBox = $("#dataSource .field[data-item=Url]");
    	if(urlBox.find("#field_Url").val().length == 0) {
    		problems += "- You must enter a URL or select a File and click the Upload button.\n";
    	}
    	var urlMessage = urlBox.find(".validationMessage");
    	if(urlMessage.attr("data-status") == "red"){
    		problems += urlMessage.text() + "\n";
    	}
			//Basic info
    	$("#basicInfo .field").not("[data-field=Keywords]").each(function() { 
    		var box = $(this);
    		var message = box.find(".validationMessage");
    		var text = message.html();
    		if(message.attr("data-status") == "green"){
					//Do nothing
    		}
    		else if(message.length > 0  && text.length > 0 && ( message.attr("data-status") == "gray" || message.attr("data-status") == "red" )){
    			problems += box.attr("data-field") + ": " + text + "\n";
    		}
    		else if(box.hasClass("required")){
    			problems += "- " + box.attr("data-field") + " must be completed\n";
    		}
    	});
    	//Keywords
    	var kBox = $("#basicInfo .field[data-field=Keywords]");
    	var kMessage = kBox.find(".validationMessage");
    	if(kMessage.attr("data-status") == "red") {
    		problems += kMessage.text();
    	}
    	if(preselectedKeywords.length == 0 && addedKeywords.length == 0){
    		problems += "- You must enter one or more keywords.";
    	}
			//Tag fields
    	$("#tagFields .field.required").each(function() {
    		var box = $(this);
    		if(box.find("input:checked").length == 0){
    			problems += "- You must select at least one " + box.attr("data-title") + "\n";
    		}
    	});
      alert("One or more problems were found with the current information:\n" + problems);
    }
  }

  //put standards into hidden value field
  function packStandards() {
  	var box = $(".hdnStandards");
  	if(box.length == 0){ return; }
  	var list = [];

  	for(var i in selectedStandards){
  		var item = selectedStandards[i];
  		list.push({
  			StandardId: item.id,
  			AlignmentTypeId: item.alignmentType,
				AlignmentDegreeId: item.usageType
  		});
  	}

  	box.val(JSON.stringify(list));

  	return list;
  }

  //put keywords into hidden value field
  function packKeywords() {
    var box = $(".hdnKeywords");
    box.val(JSON.stringify(addedKeywords));

    return addedKeywords;
  }

  function getResourceDTO() {
	//Get a ResourceDTO object from current form data
  var libID = 0;
  var colID = 0;
		if($("#ddlLibrary").length > 0 && $("#ddlCollection").length > 0){
  	libID = parseInt($("#ddlLibrary option:selected").attr("value"));
  	colID = parseInt($("#ddlCollection option:selected").attr("value"));
  }
  	//Setup initial object
  	var data = {
  		ResourceId: resourceID,
  		VersionId: versionID,
  		Url: $("#field_Url").val(),
  		Keywords: addedKeywords,
  		Standards: packStandards(),
  		LibraryId: libID,
  		CollectionId: colID,
  		Fields: [],
  		UsageRights: { Url: $("#field_UsageRightsUrl").val() },
			ContentId: currentFileDetails.contentID
  	};

  	//Single value fields
  	$("#basicInfo .field[data-dto]").not("[data-field=Keywords], [data-field=Standards], [data-field=LibraryAndCollection], [data-field=UsageRightsUrl]").each(function() {
  		var box = $(this);
  		var name = box.attr("data-dto");
  		//Text fields
  		if(box.find("input[type=text]#field_" + name).length > 0 || box.find("textarea#field_" + name).length > 0){
  			data[name] = box.find("#field_" + name).val();
  		}
  		//Drop-down lists
  		else if(box.find("select").length > 0) {
  			var value = box.find("select option:selected").attr("value");
				//IF DDL is actually a tag field...
  			if(box.attr("data-dto") == "tag"){
					//Get schema and ID
  				var schema = box.attr("data-schema");
  				var id = parseInt(box.find("select option:selected").attr("value"));
					//Add to tags
  				data.Fields.push({
  					Schema: schema,
  					Tags: [ {Id: id} ]
  				});
  			}
  			else {
  				if(isNaN(parseInt(value))){
  					data[name] = value;
  				}
  				else {
  					data[name] = parseInt(value);
  				}
  			}
  		}
  	});

  	//Tags
  	$("#tagFields .field").each(function() {
  		var box = $(this);
  		var name = box.attr("data-schema");
  		var selectedItems = box.find("input:checked").not(":disabled");
  		var selectedIDs = [];
  		if(selectedItems.length > 0){
  			selectedItems.each(function() {
  				selectedIDs.push({ Id: parseInt($(this).attr("data-id")) });
  			});
  			data.Fields.push({
  				Schema: name,
  				Tags: selectedIDs
  			});
  		}
  	});

  	return data;
  }

  function successSaveResource(msg){
  	console.log(msg.extra);
  	if(msg.valid){
  		var targetURL = "/resource/" + msg.data.ResourceId + "/" + msg.data.UrlTitle;
			//If doing update, go straight to the resource
  		if(updateMode){
  			window.location.href = targetURL;
  			return;
  		}
			//Otherwise, show message and continue
  		$("#publishSuccess").html("");
  		$("#publishSuccess")
				.append("<p>Your resource was successfully published!</p>")
				.append("<p>View Resource: <a href=\"" + targetURL + "\" target=\"_blank\">" + "http://ioer.ilsharedlearning.org/resource/" + msg.data.ResourceId + "/" + msg.data.UrlTitle + "</a></p>")
				.append("<p>If you would like to tag another resource using the same tags, continue below. If you would like to start fresh, reload the page.</p>")
				[0].scrollIntoView();
			//Reset stuff
  		currentFileDetails = { url: "", resourceID: 0, contentID: 0 };
  		$("#field_Url").attr("disabled", false).attr("readonly", false).val("").trigger("change");
  		$("#iframe_File").attr("src", $("#iframe_File").attr("src"));
  		enableButton($("#btnUploadFile"), "Upload");
  		enableButton($(".btnRemoveFile"), "Remove File");
  		$(".btnRemoveFile").hide();
  		setStatus("Upload", "green", "");
  		$("#field_Title, #field_Description").val("").trigger("change");
  		validateForm();
  	}
  	else {
  		if(currentFileDetails.contentID == 0){
  			$("#field_Url").attr("disabled", false).attr("readonly", false);
  		}
  		alert(msg.status);
  		$("#globalValidationMessage").attr("data-status", "red").html(msg.status);
		}
  	$("#btnFinish").attr("disabled", false).attr("value", "Finish!");
  	$("#globalValidationMessage").removeClass("processing");
  }
</script>
<script type="text/javascript">
  /* --- Miscellaneous --- */

</script>
<script type="text/javascript">
	/* --- Gooru --- */
	var gooruID = "<%=GooruResourceId %>";
	var gooruToken = "<%=GooruSessionToken %>";
	var useGooru = <%=(GooruSuccessful ? "true" : "false") %>;
	var gooruResourceData = {};

	var gradeMap = [
		{ input: "kindergarten", output: "Kindergarten" },
		{ input: "higher education", output: "Postsecondary" },
		{ input: "9", output: "Grades 9-10" },
		{ input: "10", output: "Grades 9-10" },
		{ input: "11", output: "Grades 11-12" },
		{ input: "12", output: "Grades 11-12" }
	];

	$(document).ready(function() {
		if(useGooru){
			getGooruResourceData();
		}
	});

	function getGooruResourceData() {
		$.ajax({
			type: "GET",
			url: "//www.goorulearning.org/gooruapi/rest/v2/resource/" + gooruID + "?sessionToken=" + gooruToken,
			dataType: "jsonp",
			crossDomain: true,
			cache: false,
			success: function(result){ 
				gooruResourceData = result; 
				loadGooruResource(); 
			}
		});
	}

	function loadGooruResource() {
		console.log("Gooru Resource Data: ", gooruResourceData);
		var res = gooruResourceData.resource;

		//Basic info
		$("#field_Title").val(res.title).trigger("change");
		$("#field_Description").val(res.description).trigger("change");
		$("#field_Url").val(res.url).trigger("change");
		$("#field_Creator").val(res.creator.firstName + " " + res.creator.lastName).trigger("change");
		$("#field_UsageRightsUrl").val(res.license.url).trigger("change");
		$("#field_Publisher").val(res.publisher.length > 0 ? res.publisher[0] : "").trigger("change");

		//Grade Levels
		var gradeArray = res.grade.split(",");
		for(var i in gradeArray){
			var item = gradeArray[i].toLowerCase();
			for(var j in gradeMap){
				if(item == gradeMap[j].input){
					checkBoxByTitle(gradeMap[j].output);
				}
			}
			checkBoxByTitle("Grade " + item);
		}

		//Keywords - check matching box if possible; otherwise add to keywords
		var courseData = getNormalizedData(res.course, null)
		for(var i in courseData){
			addGooruKeyword(courseData[i]);
		}
		var edUseData = getNormalizedData(res.educationalUse)
		for(var i in edUseData){
			if(edUseData[i].selected){
				addGooruKeyword(edUseData[i].value);
			}
		}
		var instructionalData = getNormalizedData(res.instructional);
		for(var i in instructionalData){
			addGooruKeyword(instructionalData[i].displayName);
		}
		var mediaData = getNormalizedData(res.mediaType);
		for(var i in mediaData){
			addGooruKeyword(mediaData[i]);
		}
		var momentsData = getNormalizedData(res.momentsOfLearning);
		for(var i in momentsData){
			if(momentsData[i].selected){
				addGooruKeyword(momentsData[i]);
			}
		}
		var formatData = getNormalizedData(res.resourceFormat);
		for(var i in formatData){
			addGooruKeyword(formatData[i].displayName);
		}
		var typeData = getNormalizedData(res.resourceType);
		for(var i in typeData){
			addGooruKeyword(typeData[i].description);
		}
		renderKeywords();

		//Standards (if applicable)
		var codes = [];
		for(var i in res.taxonomySet){
			codes.push(res.taxonomySet[i].code);
		}
		for(var i in gooruResourceData.standards){
			if(codes.indexOf(gooruResourceData.standards[i].code) == -1){
				codes.push(gooruResourceData.standards[i].code);
			}
		}
		try {
			sb7AddStandardsByFullCode(codes);
		}
		catch(e){ }

	}

	function checkBoxByTitle(target){
		var target = $("#tagFields .field label span:contains(" + target + ")").parent().find("input[type=checkbox]");
		if(target.length > 0){
			target.prop("checked", true).trigger("change");
			return true;
		}
		else {
			return false;
		}
	}

	function getNormalizedData(input){
		var type = typeof(input);
		switch(type){
			case "string": return [ input ];
				break;
			case "object": 
				if(typeof(input.length) == "undefined"){
					return [ input ];
				}
				else {
					return input;
				}
				break;
			default:
				return [];
				break;
		}
	}

	function addGooruKeyword(text){
		text = text.replace(/_/g, " ");
		if(!checkBoxByTitle(text)){
			if(addedKeywords.indexOf(text) == -1){
				addedKeywords.push(text);
			}
		}
	}
</script>

<style type="text/css">
  /* Big Items */
  #mainContent input:not([type=checkbox]), #mainContent select, #mainContent textarea, #mainContent #tags label { display: block; width: 100%; }
  .column { display: inline-block; vertical-align: top; width: 50%; padding: 0 5px; }
  #mainContent h2 { font-size: 30px; }
  .step { margin-bottom: 25px; padding: 0 5px; }
  .step > h2 { padding-left: 10px; background-color: #EEE; border-radius: 5px; margin-bottom: 5px; }
	.field > h4 { font-size: 18px; padding: 5px; }

  /* Data Source */
  #iframe_File { width: 100%; height: 25px; border: none; }
  #dataSource ul li { margin-bottom: 5px; }
  #dataSource .field, #basicInfo .field { margin-bottom: 20px; background-image: linear-gradient(rgba(0,0,0,0.05),rgba(0,0,0,0)); padding: 5px; border-radius: 5px 5px 0 0; }
  #dataSource .field h3, #basicInfo .field h3 { font-size: 20px; }
  #dataSource .field h3 i, #basicInfo .field h3 i { float: right; font-weight: normal; font-style: italic; color: #555; margin-left: 10px; }
  .field h3 span { opacity: 0.9; font-weight: normal; color: #E33; }
  #mainContent input.btnRemoveFile { display: none; }

  /* Basic Info */
  #basicInfo #field_Description { resize: vertical; height: 10em; min-height: 8em; max-height: 20em; }
  #basicInfo #usageRightsSelectorBox { padding-left: 100px; background: url('') no-repeat left 5px center; clear: both; }
  #basicInfo #usageRightsLink { padding: 2px 5px 5px 5px; }
  .addedKeyword, .preselectedKeyword { background-color: #F5F5F5; border-radius: 5px; border: 1px solid #CCC; margin: 5px 3px; padding: 2px 30px 2px 5px; display: inline-block; vertical-align: top; position: relative; }
  .addedKeyword::after { content: " "; display: block; clear: both; }
	.preselectedKeyword { padding: 2px 5px; }
  #mainContent .addedKeyword input { position: absolute; width: 25px; height: 100%; line-height: 100%; top: 0; right: 0; font-weight: bold; color: #FFF; background-color: #F00; opacity: 0.7; border-radius: 0 5px 5px 0; border-width: 0; float: right; margin: 0 0 2px 5px; transition: opacity 0.2s; }
  #mainContent .addedKeyword input:hover, #mainContent .addedKeyword input:focus { opacity: 1; }

  /* Tags */
  #tags #tagFields { -moz-column-count: 3; -webkit-column-count: 3; column-count: 3; -webkit-column-gap: 25px; -moz-column-gap: 25px; column-gap: 25px; padding: 5px; }
  #tags .field { display: inline-block; width: 100%; vertical-align: top; margin-bottom: 25px; }
  #tags .field h3 { background-color: #EEE; border-radius: 5px; padding: 2px 5px; margin: 0 0 5px 0; font-size: 18px; }
  #tags .field label { padding: 2px 5px; transition: background-color 0.2s; border-radius: 5px; }
  #tags .field label:hover, #tags .field label:focus { background-color: #F5F5F5; cursor: pointer; }

  /* Footer */
  #footerDiv { margin-bottom: 35px; }
  #footer { position: fixed; background-color: #EEE; bottom: 0; left: 0; right: 0; height: 34px; }
  #footer #btnFinish { width: 25%; height: 30px; position: absolute; top: 2px; right: 2px; font-size: 20px; }
  #footer.green { background-color: #4AA394; box-shadow: 0 0 25px #4AA394; color: #FFF; }
  #footer.red { background-color: #D33; color: #FFF; box-shadow: 0 0 20px #E22; }
	#mainSiteFooter { display: none; }

  /* Validation */
  .validationMessage { padding: 2px 5px; border-radius: 5px; transition: color 0.5s, background-color 0.5s, padding 0.5s; }
  body .validationMessage:empty { padding: 0; }
  .validationMessage[data-status=green] { color: #4AA394; font-style: italic; }
  .validationMessage[data-status=red] { background-color: #B03D25; color: #FFF; font-weight: bold; padding: 5px 10px; margin-top: 2px; }
  .validationMessage[data-status=gray] { color: #999; font-style: italic; }
  .validationMessage[data-status=red] a { color: #FFF; text-decoration: underline; }
  #globalValidationMessage { padding: 5px 25% 5px 5px; transition: background-color 0.5s, color 0.5s; height: 100%; transition: box-shadow 0.3s, background-color 0.3s, color 0.3s; box-shadow: 0 0 30px rgba(0,0,0,0.5); }
  #globalValidationMessage[data-status=neutral] { }
  #globalValidationMessage[data-status=green]{ background-color: #4AA394; color: #FFF; box-shadow: 0 0 30px rgba(0,255,0,0.5); }
  #globalValidationMessage[data-status=red] { background-color: #B03D25; color: #FFF; box-shadow: 0 0 30px rgba(255,0,0,0.5);  }
	#globalValidationMessage.processing { background-image: url('/images/wait.gif'); background-repeat: no-repeat; background-position: right 30% center; }

  /* Miscellaneous */
  .lblTestingMode { float: right; line-height: 32px; }
  .lblTestingMode input { display: inline-block; vertical-align: middle; }
  .lblTestingMode:hover, .lblTestingMode:focus { cursor: pointer; }
  #ddlLibrary { margin-bottom: 5px 25% 5px 5px; }
  .field[data-field=Standards] h3 { padding: 0 5px; }
  #ddlLibrary { margin-bottom: 5px; }
  .alignmentType { margin-bottom: 2px; }
	.preselectedStandard { background-color: #EEE; border-radius: 5px; padding: 2px 5px; margin-bottom: 5px; }
	.preselectedStandard .psNotationCode { font-weight: bold; display: inline-block; vertical-align: top; padding: 2px 15px 2px 5px; min-width: 25%; }
	.preselectedStandard .psAlignment { display: inline-block; vertical-align: top; padding: 2px 15px; float: right; font-style: italic; }
	#publishSuccess { margin: 5px auto; max-width: 1000px; padding: 5px; border-radius: 5px; background-color: #4AA394; color: #FFF; font-weight: bold; box-shadow: 0 0 15px -2px #4AA394; text-align: center; }
	#publishSuccess:empty { display: none; }
	#publishSuccess a { color: #FFF; text-decoration: underline; }
	#mainContent #modeButtons input { display: inline-block; vertical-align: top; width: 50%; }
	#modeButtons input:first-child { border-radius: 5px 0 0 5px; }
	#modeButtons input:last-child { border-radius: 0 5px 5px 0; }
	#modeButtons input.current { background-color: #9984BD; }
	#mainContent #btnCancelUpdate { display: inline-block; width: 25%; margin: 0 5px 10px 5px; }

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
    .column { padding: 0; }
  }
  @media (max-width: 600px) {
    #tags #tagFields { -moz-column-count: 1; -webkit-column-count: 1; column-count: 1; }
    #mainContent #btnCancelUpdate { width: 200px;  }
  }
</style>

<div id="mainContent">
  <h1 class="isleH1"><label id="lblTestingMode" class="lblTestingMode" runat="server">Testing Mode <input type="checkbox" id="cbx_testingMode" runat="server" class="cbxTestingMode" /></label>Resource Tagger </h1>
	<% if(Resource.ResourceId > 0){ %>
	<input type="button" id="btnCancelUpdate" class="isleButton bgRed" onclick="history.go(-1);" value="← Cancel Update" />
	<% } %>
	<div id="publishSuccess"></div>

  <!-- URL (direct) or URL From File -->
  <div id="dataSource" class="step">
    <h2>1. The Resource</h2>
    <div class="column">
      <p>First, provide a URL to the Resource.</p>
      <ul>
        <li>If the Resource is a web page, just paste its URL into the box <span class="fullscreen">to the right</span><span class="mobile">below</span>.</li>
        <li>If the Resource is a file online that you can link to, paste the URL to the file in the box <span class="fullscreen">to the right</span><span class="mobile">below</span>. Make sure it is publicly accessible.</li>
        <li>If the Resource is a file that does <b>not</b> exist online in a publicly-accessible location, use the "Upload a File" option <span class="fullscreen">to the right</span><span class="mobile">below</span> and click the Upload button. We'll host the file and provide the URL for you.</li>
        <li>You may upload most file types (up to 25MB) except for executables and other potentially dangerous files. All files uploaded are subject to a virus scan.</li>
        <li style="color:#D33;">Once you <b>publish</b> a file, you may update the file, but <u>only</u> if the replacement has the same name and extension.</li>
      </ul>
    </div><!--
    --><div class="column">
			<% //If a new resource and new content or replacing existing content %>
			<% if( ( Resource.ContentId == 0 && Resource.ResourceId == 0 ) || CanReplaceContent ) { %>
			<div id="modeButtons">
				<input type="button" onclick="setMode('Url')" data-mode="Url" class="isleButton bgBlue current" value="Tag a URL" /><!--
				--><input type="button" onclick="setMode('File');" data-mode="File" class="isleButton bgBlue" value="Upload a File" />
			</div>
			<% } %>
      <div data-item="Url" class="field" data-dto="Url">
        <h3>Resource URL <span>(required)</span> <i>Direct URL to the Resource</i></h3>
        <input type="text" id="field_Url" placeholder="http://" value="<%=Resource.Url %>" <% if(!string.IsNullOrWhiteSpace(Resource.Url)){ %> disabled="disabled" readonly="readonly" <% } %> />
        <div class="validationMessage" data-field="Url" data-status="neutral"></div>
      </div>
      <div data-item="File" class="field" style="display:none;">
        <h3>Resource Upload <i>Only use this if the Resource doesn't exist online</i></h3>
				<% if ( ( Resource.ContentId == 0 && Resource.ResourceId == 0 ) || CanReplaceContent ) { %>
        <iframe src="/controls/ubertaggerv2/upload.aspx" id="iframe_File"></iframe>
        <input type="hidden" id="hdnFileDetails" class="hdnFileDetails" runat="server" />
        <input type="button" id="btnUploadFile" class="isleButton bgBlue" value="Upload" onclick="uploadFile()" />
        <% if(string.IsNullOrWhiteSpace(Resource.Url)) { %><input type="button" id="btnRemoveFile" class="btnRemoveFile isleButton bgRed" runat="server" value="Remove File" onclick="removeFile()" /> <% } %>
        <div class="validationMessage" data-field="Upload" data-status="neutral"></div>
				<p style="text-align: center; padding: 10px;" id="uploadWarning"><b>Select a file and click the <u>Upload</u> button before continuing.</b></p>
				<% } else { %>
				<p style="padding: 25px; text-align: center; font-style: italic;">You cannot upload a file for this Resource.</p>
				<% } %>
      </div>
      <div class="validationMessage" data-field="DataSource" data-status="neutral"></div>
    </div>
  </div><!-- /dataSource -->


  <!-- Basic Information -->
  <div id="basicInfo" class="step">
    <h2>2. Basic Information</h2>

    <div class="column">
      <!-- Title -->
      <div class="field required" data-field="Title" data-dto="Title">
        <h3>Resource Title <span>(required)</span> <i>The full title of the Resource</i></h3>
        <input type="text" id="field_Title" value="<%=Resource.Title %>" <%=( CanUpdateSpecialFields ? "" : "disabled=\"disabled\"" ) %> />
        <div class="validationMessage" data-field="Title" data-status="neutral"></div>
      </div>

      <!-- Description -->
      <div class="field required" data-field="Description" data-dto="Description">
        <h3>Description <span>(required)</span> <i>A good description of the Resource</i></h3>
        <textarea id="field_Description" <%=( CanUpdateSpecialFields ? "" : "disabled=\"disabled\"" ) %>><%=Resource.Description %></textarea>
        <div class="validationMessage" data-field="Description" data-status="neutral"></div>
      </div>

      <!-- Keywords -->
      <div class="field required" data-field="Keywords" data-dto="Keywords">
        <h3>Keywords <span>(required)</span> <i>Keywords help others find the Resource</i></h3>
        <p>Add keywords (or phrases) by typing them one at a time below <b>and pressing <u>Enter</u> after each word or phrase</b>.</p>
        <input type="text" id="field_Keyword" placeholder="Type a keyword or phrase and press Enter" />
        <div id="enteredKeywords"></div>
				<% if(Resource.Keywords.Count() > 0){ %>
				<h4>These keywords were already added:</h4>
				<% } %>
        <div id="preselectedKeywords">
					<% foreach(var item in Resource.Keywords){ %>
					<div class="preselectedKeyword"><%=item %></div>
					<% } %>
        </div>
        <div class="validationMessage" data-field="Keyword" data-status="neutral"></div>
        <input type="hidden" class="hdnKeywords" id="hdnKeywords" runat="server" />
      </div>

    <% if ( CurrentTheme.Name == "quick" || CurrentTheme.Name == "worknet_public" )
			 { %>
    </div><div class="column">
    <% } %>

      <!-- Usage Rights -->
      <div class="field required" data-field="UsageRightsUrl" data-dto="UsageRights.Url">
        <h3>Usage Rights <span>(required)</span> <i>Restrictions on using, altering, and/or republishing the Resource</i></h3>
        <div id="usageRightsSelectorBox">
          <select id="ddl_UsageRights">
            <% foreach(var item in UsageRights) { %>
              <option value="<%=item.CodeId %>" data-url="<%=item.Url %>" data-description="<%=item.Description %>" data-iscustom="<%=item.Custom ? "true" : "false" %>" data-isunknown="<%=item.Unknown ? "true" : "false" %>" data-icon="<%=item.IconUrl %>" <% if(item.CodeId == Resource.UsageRights.CodeId || (Resource.ResourceId == 0 && item.Unknown) ) { %> selected="selected" <% } %>><%=item.Title %></option>
            <% } %>
          </select>
          <a id="usageRightsLink" href="#"></a>
        </div>
        <input type="text" id="field_UsageRightsUrl" placeholder="Enter the license URL starting with http://" value="<%=Resource.UsageRights.Url %>" />
        <div class="validationMessage" data-field="UsageRightsUrl" data-status="neutral"></div>
      </div>

    <% if( CurrentTheme.Name == "worknet"){ %>
    </div><div class="column">
    <% } %>

      <!-- IOER ContentPrivileges -->
      <div class="field" data-field="ContentPrivileges" data-dto="PrivilegeId">
        <h3>IOER Access Limitations <i>Who can access this Resource?</i></h3>
        <p class="tip">If you select "Anyone can access", the Resource will be published to the Learning Registry and freely available for anyone to access. If you need to restrict access to the Resource (e.g., for tests and answer keys), select the group of users that will be allowed to see it.</p>
        <select id="ddl_ContentPrivileges">
          <% foreach(var item in ContentPrivileges){ %>
          <option value="<%=item.Id %>" <% if( Resource.PrivilegeId == item.Id) { %> selected="selected" <% } %>><%=item.Title %></option>
          <% } %>
        </select>
        <div class="validationMessage" data-field="ContentPrivileges" data-status="neutral"></div>
      </div>

    <% if(CurrentTheme.Name == "ioer" || CurrentTheme.Name == "uber" ){ %>
    </div><div class="column">
    <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Language")){ %>
      <% var targetField = CurrentTheme.VisibleTagData.FirstOrDefault( m => m.Schema == "inLanguage" ); %>
			<% var selectedLanguage = 0; try { selectedLanguage = Resource.Fields.Where( f => f.Schema == "inLanguage" ).FirstOrDefault().Tags.Where( t => t.Selected ).FirstOrDefault().Id; } catch { } %>
      <!-- Language -->
      <div class="field" data-field="Language" data-dto="tag" data-schema="<%=targetField.Schema %>">
        <h3>Language <i>The primary language of the Resource</i></h3>
        <p class="tip">For example: a Resource that teaches Spanish, but is meant for use by English-speaking students, would be considered English here.</p>
        <select id="ddl_Language">
          <% foreach(var item in targetField.Tags){ %>
          <option value="<%=item.Id %>" <% if(item.Id == selectedLanguage) { %> selected="selected" <% } %>><%=item.Title %></option>
          <% } %>
        </select>
        <div class="validationMessage" data-field="Language" data-status="neutral"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("AccessRights")){ %>
      <% var targetField = CurrentTheme.VisibleTagData.FirstOrDefault( m => m.Schema == "accessRights" ); %>
			<% var selectedAccess = 0; try { selectedAccess = Resource.Fields.Where( f => f.Schema == "accessRights" ).FirstOrDefault().Tags.Where( t => t.Selected ).FirstOrDefault().Id; } catch { } %>
      <!-- Access Rights -->
      <div class="field" data-field="AccessRights" data-dto="tag" data-schema="<%=targetField.Schema %>">
        <h3>Access Rights <i>Requirements for accessing the Resource, if any</i></h3>
        <select id="ddl_AccessRights">
          <% foreach(var item in targetField.Tags){ %>
          <option value="<%=item.Id %>" <% if(item.Id == selectedAccess) { %> selected="selected" <% } %>><%=item.Title %></option>
          <% } %>
        </select>
        <div class="validationMessage" data-field="AccessRights" data-status="neutral"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Creator")){ %>
      <!-- Creator -->
      <div class="field" data-field="Creator" data-dto="Creator">
        <h3>Creator <i>The original creator of the Resource</i></h3>
        <input type="text" id="field_Creator" value="<%=Resource.Creator %>" />
        <div class="validationMessage" data-field="Creator" data-status="neutral"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Publisher")){ %>
      <!-- Publisher -->
      <div class="field" data-field="Publisher" data-dto="Publisher">
        <h3>Publisher <i>The person or organization currently making the Resource available</i></h3>
        <p class="tip">For print materials, this would be the publisher. For online Resources, this would be the person or organization that owns the website.</p>
        <input type="text" id="field_Publisher" value="<%=Resource.Publisher %>" />
        <div class="validationMessage" data-field="Publisher" data-status="neutral"></div>
      </div>
      <% } %>

      <% if(CurrentTheme.VisibleSingleValueFields.Contains("Requirements")){ %>
      <!-- Requirements -->
      <div class="field" data-field="Requirements" data-dto="Requirements">
        <h3>Technical/Equipment Requirements <i>Devices, software, equipment, or other noteworthy things needed to use the Resource</i></h3>
        <input type="text" id="field_Requirements" value="<%=Resource.Requirements %>" />
        <div class="validationMessage" data-field="Requirements" data-status="neutral"></div>
      </div>
      <% } %>

			<% if(Resource.ResourceId == 0){ %>
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
				<div class="field" data-field="Organization" data-dto="OrganizationId">
					<h3>Organization <i>You can tag this Resource on behalf of your Organization</i></h3>
					<select id="ddl_Organization">
						<option value="0">None (not tagging on behalf of an organization)</option>
						<% foreach(var item in OrganizationData){ %>
						<option value="<%=item.Id %>" <% if(item.Id == Resource.OrganizationId) { %> selected="selected" <% } %>><%=item.Organization %></option>
						<% } %>
					</select>
				</div>
				<% } %>
			<% } %>
    </div>

    <% if(CurrentTheme.VisibleSingleValueFields.Contains("Standards")){ %>
    <!-- Learning Standards -->
    <div class="field" data-field="Standards" data-dto="Standards">
      <h3>Learning Standards <i>Learning Standards to which the Resource aligns</i></h3>
      <uc1:StandardsBrowser ID="standardsBrowser" runat="server" />
      <input type="hidden" class="hdnStandards" id="hdnStandards" runat="server" />
			<% if(Resource.Standards.Count() > 0) { %>
			<h4>These standards have already been added:</h4>
			<div id="preselectedStandards">
				<% foreach(var item in Resource.Standards ) { %>
				<div class="preselectedStandard" data-id="<%=item.StandardId %>">
					<div class="psAlignment"><%=item.AlignmentDegree %></div><!--
					--><div class="psNotationCode"><%=item.AlignmentType %> <%=item.NotationCode %></div>
					<div class="psDescription"><%=item.Description %></div>
				</div>
				<% } %>
			</div>
			<% } %>
    </div>
    <% } %>

  </div><!-- /basic info --><!--
          Tags
  --><div id="tags" class="step">
    <h2>3. Tags</h2>
    <div id="tagFields">
      <% var requiredFields = new List<string>() { "learningResourceType", "mediaType" }; %>
      <% var skipTags = new List<string>() { "accessRights", "inLanguage", "usageRights" }; %>
			<% var selectedTags = Resource.Fields.SelectMany( t => t.Tags ).Where( t => t.Selected ).Select( t => t.Id ).ToList(); %>
			<% CurrentTheme.VisibleTagData = CurrentTheme.VisibleTagData.OrderBy( m => !requiredFields.Contains( m.Schema ) ).ToList(); %>
      <% foreach(var item in CurrentTheme.VisibleTagData){ %>
      <% if ( skipTags.Contains(item.Schema) ) { continue; } %>
      <div class="field <%=(requiredFields.Contains(item.Schema) ? "required" : "") %>" data-schema="<%=item.Schema %>" data-title="<%=item.Title %>">
        <h3><%=item.Title %><%=(requiredFields.Contains(item.Schema) ? " <span>(required - select at least one.)</span> " : "") %></h3>
				<% if(item.Schema == "gradeLevel"){ %>
				<input type="button" class="isleButton bgBlue" data-grades="elementary" value="All Elementary" />
				<input type="button" class="isleButton bgBlue" data-grades="highschool" value="All High School" />
				<% } %>
        <% foreach(var tag in item.Tags){ %>
        <label>
          <input type="checkbox" data-category="<%=item.Id %>" data-id="<%=tag.Id %>" name="cbx_<%=tag.Id %>" <% if(selectedTags.Contains(tag.Id)) { %> checked="checked" disabled="disabled" <% } %> />
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
  --><input type="button" class="isleButton bgBlue" id="btnFinish" value="Finish!" onclick="finish();" />
</div><!-- /footer -->

<div id="templates" style="display:none;">
  <script type="text/template" id="alert_fileLimits">
    IOER allows the following file types 
    (up to 25 megabytes in size):
      - Microsoft Office (.doc/x, .xls/x, .ppt/x)
      - Images (.jpg, .png, .bmp, .gif, etc.)
      - Adobe PDF
  </script>
  <script type="text/template" id="template_addedKeyword">
    <div class="addedKeyword">
      <input type="button" class="btnRemoveKeyword" value="X" onclick="removeKeyword('{word}');" />{word}
    </div>
  </script>
</div>

<asp:Literal ID="createdContentItemId" runat="server" visible="false">0</asp:Literal>
<asp:Literal ID="txtCreator" runat="server" visible="false"></asp:Literal>
<asp:Literal ID="txtPublisher" runat="server" visible="false"></asp:Literal>