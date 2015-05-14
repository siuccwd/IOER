/*---     ---     Variables      ---     ---*/
var timers = {};
var previous = {};
var enteredKeywords = [];
var keywordMinLength = 3;
var SB7mode = "tag";

/*---     ---     Initialization      ---     ---*/
$(document).ready(function () {
  $("form").attr("onsubmit", "return false;");
  setupValidation();
  setupKeywords();
  setupOrgDDLs();
  setupLibColDDLs();

  //Clear form (kludge)
  if ($(".successMessage").length > 0) {
    $("input[type=text], textarea").val("");
    $("input[type=checkbox]").prop("checked", false);
    $("select").each(function () { $(this).find("option").first().prop("selected", true); });
  }

  //If a box is prefilled, check it
  $("input[type=text], textarea").each(function () {
    if ($(this).val() != "") {
      $(this).trigger("change");
    }
  });

  addSelectAlls();
}); //document.ready


/*---     ---     Page Functions      ---     ---*/
//Keywords
function setupKeywords() {
  $("#txtKeyword").on("keyup", function (event) {
    var word = $(this).val().trim();
    var vm = $(this).parent().find(".vm");

    if (word.length < keywordMinLength) {
      vm.attr("class", "vm gray").html("Please enter at least " + (keywordMinLength - word.length) + " more characters.");
    }
    else {
      vm.attr("class", "vm gray").html("Press Enter to add this Keyword.");
    }

    if (event.which == 13 || event.keyCode == 13) {
      event.stopPropagation();
      //Validate entered keyword
      if (word.length < keywordMinLength) { //Check minimum length
        vm.attr("class", "vm red").html("Keywords must be at least " + keywordMinLength + " characters long.");
        return;
      }

      vm.attr("class", "vm gray").html("Checking...");
      //AJAX validation
      clearTimeout(timers["keyword"]);
      timers["keyword"] = setTimeout(function () {
        utilityAjax("ValidateText", { text: word, minimumLength: keywordMinLength, fieldTitle: "Keyword" }, successValidateKeyword, { word: word, vm: vm } )
      }, 800);
    }
  });
  $("#txtKeyword").on("change blur", function () {
    if (enteredKeywords.length > 0 && $(this).val() == "") {
      $(this).parent().find(".vm").attr("class", "vm green").html("");
    }
  });
}
function removeKeyword(id) {
  var replacement = [];
  for (i in enteredKeywords) {
    if (i != id) {
      replacement.push(enteredKeywords[i]);
    }
  }
  enteredKeywords = replacement;
  renderKeywords();
}

//Library/Collection DDLs
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

function setupOrgDDLs() {
    var box = $("#ddlOrg");
    //Reset the list
    box.html("");
    for (i in orgData) {
        var current = orgData[i];
        addOption(box, current.Id, current.Title);
    }

    if (selectedOrgID > 0) {
        box.find("option[value=" + selectedOrgID + "]").attr("selected", "selected");
        //??
        //box.trigger("change");
    }
}
function addOption(box, value, html) {
    box.append(
      $("<option></option>")
        .attr("value", value)
        .html(html)
    );
}
//Validate and Publish
function validateAndPublish() {
  //Confirm user intent
  if (!confirm("Are you sure you want to Publish this Resource with these tags?")) {
    return false;
  }

  var validationMessage = "";

  //Validate
  $(".required").each(function () {
    if ($(this).find(".vm.green").length < 1) {
      validationMessage += $(this).attr("data-name") + " is not complete!\r\n";
    }
  });

  if (validationMessage != "") {
    alert(validationMessage);
    return false;
  }

  packStandards();
  packKeywords();

  $("#btnFinish").attr("disabled", "disabled").attr("value", "Processing, please wait...");
  $("form").removeAttr("onsubmit");
  $("form")[0].submit();
}

function packStandards() {
  var box = $(".hdnStandards");
  var data = [];
  SBstoreDDLValues();
  for (i in selectedStandards) {
    data.push({
      id: selectedStandards[i].id,
      code: selectedStandards[i].code,
      alignment: selectedStandards[i].alignmentType
    });
  }
  /*$("#SBselected .selectedStandard").each(function () {
    data.push({
      id: parseInt($(this).attr("data-standardID")),
      code: $(this).attr("data-code"),
      alignment: parseInt($(this).find("select option:selected").attr("value"))
    });
  });*/
  box.val(JSON.stringify(data));
}

function packKeywords() {
  $("#cbxlWorknetQualify input:checked").each(function () {
    enteredKeywords.push($(this).attr("value"));
  });
  var box = $(".hdnKeywords");
  box.val(JSON.stringify(enteredKeywords));
}

function addSelectAlls() {
  $("ul[tablename=gradeLevel]").prepend(
    $("<li></li>")
    .append(
      $("<input></input>")
      .attr("type", "button")
      .attr("value", "All High School")
      .attr("id", "btn_allHighSchool")
    )
  );
  $("ul[tablename=gradeLevel]").prepend(
    $("<li></li>")
    .append(
      $("<input></input>")
      .attr("type", "button")
      .attr("value", "All Elementary")
      .attr("id", "btn_allElementary")
    )
  );
  $("ul[tablename=careerCluster]").prepend(
    $("<li></li>")
    .append(
      $("<input></input>")
      .attr("type", "button")
      .attr("value", "All Career Clusters")
      .attr("id", "btn_allClusters")
    )
  );
  $("#btn_allElementary").on("click", function () {
    for (var i = 1; i <= 10; i++) {
      $("ul[tablename=gradeLevel] span[itemid=" + i + "] input").prop("checked", true).trigger("change");
    }
  });
  $("#btn_allHighSchool").on("click", function () {
    for (var i = 11; i <= 14; i++) {
      $("ul[tablename=gradeLevel] span[itemid=" + i + "] input").prop("checked", true).trigger("change");
    }
  });
  $("#btn_allClusters").on("click", function () {
    $("ul[tablename=careerCluster] span input:not([value=0])").prop("checked", true).trigger("change");
  });
}


/*---     ---     Validation      ---     ---*/
function setupValidation() {
  //text
  $(".validation_text").each(function () {
    var box = $(this);
    box.find("input[type=text], textarea").on("keyup change", function () {
      validateText($(this), parseInt(box.attr("data-minLength")), box);
    });
  });

  //url
  $(".validation_url").each(function () {
    var box = $(this);
    box.find("input[type=text]").on("keyup change", function () {
      validateURL($(this), (box.attr("data-unique") == "true"), parseInt(box.attr("data-minLength")), box);
    });
  });
  $(".ddlConditionsOfUse").on("change", function () {
    if ($(this).find("option:selected").attr("value") == "4") { //Read the fine print
      setTimeout(function () {
        $(".conditionsSelector").find("input[type=text]").trigger("change");
      }, 10);
    }
    else {
      $(this).parentsUntil(".required").parent().find(".vm").attr("class", "vm green").html("");
    }
  }).trigger("change");

  //cbxl
  $(".validation_cbxl").each(function () {
    var box = $(this);
    box.find("input[type=checkbox]").on("change", function () {
      validateCBXL(box.find("input[type=checkbox]"), parseInt(box.attr("data-minLength")), box);
    });
  });

  //keywords
  $(".validation_keywords").on("updatedKeywords", function() {
    var box = $(this);
    validateKeywords(parseInt(box.attr("data-minLength")), box);
  });
}

//Validate Text
function validateText(box, minLength, container) {
  var name = container.attr("data-name");
  var vm = container.find(".vm");
  var value = box.val();
  //Don't re-check if the value didn't change
  if (value == previous[name]) {
    return;
  }
  //Update stored value
  previous[name] = value;
  //Validate
  if (value.length == minLength && minLength == 0) {
    vm.attr("class", "vm gray").html("");
    return;
  }
  if (validateTextLength(value, minLength, vm)) {
    vm.attr("class", "vm gray").html("Checking...");

    //AJAX text validation
    clearTimeout(timers[name]);
    timers[name] = setTimeout(function () {
      utilityAjax("ValidateText", { text: value, minimumLength: minLength, fieldTitle: name }, successValidateItem, vm);
    }, 800);
  }
  else {
    return false;
  }
}

//Validate URL
function validateURL(box, requireUnique, minLength, container) {
  var name = container.attr("data-name");
  var vm = container.find(".vm");
  var value = box.val();
  //Don't re-check if the value didn't change
  if (value == previous[name]) {
    return;
  }
  //Update stored value
  previous[name] = value;
  //Validate
  if (validateTextLength(value, minLength, vm)) {
    vm.attr("class", "vm gray").html("Checking...");
    //AJAX URL validation
    clearTimeout(timers[name]);
    timers[name] = setTimeout(function () {
      utilityAjax("ValidateURL", { text: value, mustBeNew: requireUnique }, successValidateItem, vm);
    }, 800);
  }
  else {
    return false;
  }
}

//Validate Checkbox List
function validateCBXL(list, minLength, container) {
  if (list.filter("input[type=checkbox]:checked").length < minLength) {
    container.find(".vm").attr("class", "vm gray").html("Please select at least " + minLength + " item" + (minLength == 1 ? "." : "s."));
  }
  else {
    container.find(".vm").attr("class", "vm green").html("");
  }
}

//Validate Keywords
function validateKeywords(minLength, container) {
  if (enteredKeywords.length >= minLength) {
    container.find(".vm").attr("class", "vm green").html("");
  }
  else {
    container.find(".vm").attr("class", "vm gray").html("Please enter at least " + (minLength - enteredKeywords.length) + " more keyword" + (minLength == 1 ? "." : "s."));
  }
}

//Validate text length
function validateTextLength(value, minLength, vm) {
  if (value.length < minLength) {
    vm.attr("class", "vm gray").html("Please enter at least " + (minLength - value.length) + " more characters.");
    return false;
  }
  else {
    vm.attr("class", "vm").html("");
    return true;
  }
}

/*---     ---     AJAX      ---     ---*/
function utilityAjax(method, data, success, passThru) {
  $.ajax({
    url: "/Services/UtilityService.asmx/" + method,
    async: true,
    success: function (msg) { success($.parseJSON(msg.d), passThru); },
    type: "POST",
    data: JSON.stringify(data),
    dataType: "json",
    contentType: "application/json; charset=utf-8"
  });
}

//Success Methods
function successValidateItem(data, vm) {
  if (data.isValid) {
    vm.attr("class", "vm green").html("");
    return true;
  }
  else {
    vm.attr("class", "vm red").html(data.status.replace("Resource URL", "URL"));
    return false;
  }
}
function successValidateKeyword(data, extra) {
  if (successValidateItem(data, extra.vm)) {
    enteredKeywords.push(extra.word);
    renderKeywords();
    $("#txtKeyword").val("");
  }
}


/*---     ---     Rendering      ---     ---*/
function renderKeywords() {
  var box = $("#enteredKeywords");
  var template = $("#template_addedKeyword").html();
  box.html("");
  for (i in enteredKeywords) {
    box.append(template
      .replace(/{word}/g, enteredKeywords[i])
      .replace(/{id}/g, i)
    );
  }
  $(".validation_keywords").trigger("updatedKeywords");
}