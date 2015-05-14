<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CurriculumEditor2.ascx.cs" Inherits="ILPathways.Controls.Curriculum.CurriculumEditor2" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/controls/standardsbrowser7.ascx" %>
<%@ Register TagPrefix="custom" TagName="CheckBoxList" Src="/LRW/Controls/ListPanel.ascx" %>

<!-- JSTree plugin -->
<script type="text/javascript" src="/scripts/jstree/jstree.min.js"></script>
<link rel="stylesheet" href="/scripts/jstree/themes/ioer/style.css" type="text/css" />
<!-- IOER scripts -->
<script type="text/javascript" src="/scripts/widgets/postmessagereceiver.js"></script>
<script type="text/javascript" src="/scripts/jscommon.js"></script>
<link rel="stylesheet" href="/styles/common2.css" type="text/css" />

<div id="curriculumEditor" runat="server">
  <!-- Page scripts -->
  <script type="text/javascript">
    //From Server
    var curriculumID = <%=curriculumID %>;
    var nodeID = <%=currentNode.Id %>;
    var nodeParentID = <%=nodeParentID %>;
    var nodeStandards = <%=nodeStandardsJSON %>;
    var nodeAttachments = <%=nodeAttachmentsJSON %>;
    var previousNodeID = <%=previousNodeID %>;
    var previousNodeSortOrder = <%=previousNodeSortOrder %>;
    var nextNodeID = <%=nextNodeID %>;
    var nextNodeSortOrder = <%=nextNodeSortOrder %>;
    var outdentNodeID = <%=outdentNodeID %>;
    var outdentSortID = <%=outdentSortID %>;

    //Page Properties for standards browser
    var curriculumMode = true;
    var SB7mode = "tag";
  </script>
  <script type="text/javascript">
    /* ---   ---   ---   JSTree Stuff   ---   ---   --- */
    function setupTree(data) {
      //Get the tree data (via AJAX or from the server directly)
      if(!data){
        console.log("no data. making ajax call");
        doAjax("CurriculumService1", "GetJSTree", { nodeID: curriculumID }, setupTree, null, null);
        return;
      }
      console.log("data loaded: ");
      console.log(data.data);
      //Final formatting fix
      var mainID = 0;
      for(i in data.data){
        if(data.data[i].parent == 0){ 
          data.data[i].parent = "#"; 
          data.data[i].li_attr = {"data-root": "root"};
          mainID = data.data[i].id;
        }
        if(data.data[i].parent == mainID){
          data.data[i].li_attr = {"data-firstlayer": "first" };
        }
      }

      //Reset the tree
      $("#loadingTree").fadeIn();
      $("#tree").hide().html("");
      $.jstree.destroy();

      //Initialize the tree with the data
      $("#tree").jstree({
        core: {
          check_callback: true,
          data: function(node, callback){
            callback.call(this, data.data);
          }
        },
        plugins: ["dnd", "unique", "wholerow"],
        dnd: { copy: false },
        //contextmenu: { items: { ccp: false } }
      });

      //Add tree function hooks
      $("#tree").on("click", "a", function(e, d) {
        window.location.href = $(this).attr("href");
      });
      setTimeout(function() {
        $("#tree").jstree("open_all");
        $("#tree").jstree("select_node", nodeID, false, false);
        $("#tree, #actions").fadeIn();
        $("#loadingTree").hide();
      }, 1000);
    }

    //Tree interaction
    function doNodeAction() {
      var action = $("#treeActions option:selected").attr("value");
      switch(action){
        case "addSubNode":
          addNewNode($("#btnDoNodeAction"));
          break;
        case "moveNodeUp":
          moveNode(nodeParentID, previousNodeSortOrder, previousNodeID, $("#btnDoNodeAction"));
          break;
        case "moveNodeDown":
          moveNode(nodeParentID, nextNodeSortOrder, nextNodeID, $("#btnDoNodeAction"));
          break;
        case "indentNode":
          moveNode(previousNodeID, -1, -1, $("#btnDoNodeAction"));
          break;
        case "outdentNode":
          moveNode(outdentNodeID, outdentSortID, -1, $("#btnDoNodeAction"));
          break;
        case "deleteNode":
          deleteNode($("#btnDoNodeAction"));
          break;
        default:
          break;
      }
    }

  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Initialization   ---   ---   --- */
    $(document).ready(function () {
      //Show tab
      showInitialTab();
      //Render existing standards
      renderNodeStandards();
      //Render existing attachments
      renderAttachments();
      //Fix MS formatting
      fixCBXLFormatting();
      //Setup the tree
      setupTree();
      //Listen for changes in standards
      $("#nodeStandards, #attachmentStandards").on("change", ".contentStandard select", function() { $(this).parentsUntil(".contentStandard").parent().find(".buttons .btnSave").fadeIn(); });
      //Select the relevant radio button on click of its main element
      setupAttachmentRadioButtons();
      //Setup help topic selection
      setupHelp();
    });

    //Show initial tab
    function showInitialTab() {
      var target = "properties";
      showTab(target);
    }

    //Setup help
    function setupHelp() {
      $("#ddlHelp").on("change", function() {
        showHelp($(this).find("option:selected").attr("value"));
      });
      showHelp("introduction");
      $("#helpBox").removeClass("visible");
    }

    //Fix microsoft's formatting
    function fixCBXLFormatting() {
      $(".listPanel").addClass("inputList").each(function () {
        var box = $(this);
        box.find("span").each(function () {
          var span = $(this);
          span.find("input").prependTo(span.find("label"));
          span.find("label").appendTo(box);
        });
        box.find("ul").remove();
      });
    }

    //Make radio buttons behave better
    function setupAttachmentRadioButtons() {
      $(".rblBox").each(function() {
        var box = $(this);
        box.on("click", function() {
          checkRadio(box);
        });
        box.find("input[type=text]").on("change", function() {
          checkRadio(box);
        });
      });
    }
    function checkRadio(box){
      box.find("input[type=radio]").prop("checked", true);
    }
    function checkFileUploadBox() {
      $(".rblBox#rblAttachmentFileBox").find("input[type=radio]").prop("checked", true);
    }

  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Page Functions   ---   ---   --- */
    //Show a tab
    function showTab(target) {
      $(".tab").addClass("hidden");
      $("#tab_" + target).removeClass("hidden");
      $("#tabNavigation input").removeClass("current");
      $("#tabNavigation input[data-id=" + target + "]").addClass("current");
    }

    //Show help box
    function toggleHelpBox(){
      $("#helpBox").toggleClass("visible");
    }
    function showHelp(id){
      $("#helpBox").addClass("visible");
      $("#helpBox .topic").hide();
      $("#helpBox .topic[data-topicID=" + id + "]").show();
      $("#ddlHelp option[value=" + id + "]").prop("selected", true);
      return false;
    }

    //Save Node properties
    function save_properties() {
      var data = {
        curriculumID: curriculumID,
        nodeID: nodeID,
        parentID: nodeParentID,
        title: $(".txtTitle").val(),
        timeframe: $(".txtTimeframe").val(),
        accessID: parseInt($(".ddlNodePermissions option:selected").attr("value")),
        summary: $(".txtDescription").val(),
        k12SubjectIDs: readCBXL("k12subject"),
        gradeLevelIDs: readCBXL("gradelevel")
      };
      //Save the node first, then the image
      doAjax("CurriculumService1", "Save_Properties", data, success_save_properties, $("#btnSaveProperties"), null);
    }
    function readCBXL(target){
      var result = [];
      var list = $(".column." + target + " .inputList input:checked");
      if(list.length > 0){
        list.each(function() {
          var ids = $(this).attr("id").split("_");
          result.push(ids[ids.length - 1]);
        });
      }
      return result;
    }

    //Toggle expanded interface at large screen resolutions
    function toggleExpandedInterface() {
      $("#node").toggleClass("expanded");
    }

    //Show the standards browser
    function showStandardsBrowser(type){
      //Determine what to attach the standard to
      var parentID = nodeID;
      if(type == "node"){
        $("#standardsBrowserInstructions").html("Select one or more Standards to add to this node.");
      }
      else if(type == "attachment"){ 
        parentID = parseInt($("#attachmentManager").attr("data-attachmentID")); 
        $("#standardsBrowserInstructions").html("Select one or more Standards to add to this attachment.");
      }
      else { return; }

      $("#standardsBrowserContainer").attr("data-targetParentID", parentID);
      $("#standardsBrowserContainer, #standardsBrowserOverlay").fadeIn();
      $("#standardsBrowserContainer")[0].scrollIntoView();
      window.scrollTo(0,0);
    }

    //Cancel adding standards
    function cancelAddingStandardsToItem() {
      selectedStandards = [];
      $("#SBselected").html("");
      $("#standardsBrowserContainer, #standardsBrowserOverlay").fadeOut();
    }

    //Apply added standards to target object
    function addStandardsToItem(){
      //Prepare data
      var targetParent = parseInt($("#standardsBrowserContainer").attr("data-targetParentID"));
      SBstoreDDLValues();
      var data = {
        nodeID: nodeID,
        targetID: targetParent,
        contentItemType: targetParent == nodeID ? "node" : "attachment",
        standards: []
      };
      for(i in selectedStandards){
        var current = selectedStandards[i];
        data.standards.push({
          standardID: current.id,
          code: current.code,
          text: current.description,
          isContentStandard: true,
          usageID: current.usageType,
          alignmentID: current.alignmentType
        });
      }

      if(data.standards.length > 0){
        //Make request
        doAjax("CurriculumService1", "Standards_Add", data, targetParent == nodeID ? success_addStandardsToNode : success_addStandardsToAttachment, $("#btnFinishAddingStandards"), targetParent);
      }
    }

    //Update a standard
    function standard_update(parentID, recordID, button){
      var box = $(".contentStandard[data-recordID=" + recordID + "]");
      var alignmentID = parseInt(box.find(".ddlAlignmentType option:selected").attr("value"));
      var usageID = parseInt(box.find(".ddlUsageType option:selected").attr("value"));
      var data = { parentID: parentID, standard: { recordID: recordID, alignmentID: alignmentID, usageID: usageID } };
      doAjax("CurriculumService1", "Standard_Update", data, success_standard_update, $(button), { recordID: recordID, parentID: parentID });
    }
    //Remove a standard
    function standard_delete(parentID, recordID, button){
      if(confirm("Are you sure you want to remove this standard?")){
        var data = { parentID: parentID, recordID: recordID };
        doAjax("CurriculumService1", "Standard_Delete", data, success_standard_delete, $(button), parentID);
      }
    }

    //Create a new node
    function addNewNode(button){
      doAjax("CurriculumService1", "Node_Create", { curriculumID: curriculumID, nodeID: nodeID }, success_node_create, $(button), null);
    }

    //Delete current node
    function deleteNode(button){
      doAjax("CurriculumService1", "Node_Delete", { nodeID: nodeID }, success_node_delete, $(button), null);
    }

    //Move node
    function moveNode(targetParent, targetSortOrder, targetSwapNodeID, button){
      doAjax("CurriculumService1", "Node_Move", { nodeID: nodeID, targetParentID: targetParent, targetSortOrder: targetSortOrder, targetSwapNodeID: targetSwapNodeID }, success_node_move, $(button), null);
    }

    //Publish curriculum
    function publishCurriculum(){
      if(confirm("Are you sure you want to publish this Learning List to the IOER System?")){
        doAjax("CurriculumService1", "Curriculum_Publish", { curriculumID: curriculumID }, success_curriculum_publish, $("#btnPublishCurriculum"), null);
      }
    }

    //View curriculum tags
    function viewCurriculumTags() {
      window.location.href = "/Resource/" + <%=currentNode.ResourceIntId %>;
    }
  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Attachment Functions   ---   ---   --- */

    //Load attachments
    function loadAttachments() {
      doAjax("CurriculumService1", "GetAttachments", { nodeID: nodeID }, success_loadAttachments, null, null);
    }

    //Save attachment
    function attachment_save() {
      //Detect "new" or "edit" mode
      var mode = $("#attachmentManager").attr("data-mode");
      var attachmentID = 0;
      if(mode == "edit"){
        attachmentID = parseInt($("#attachmentManager").attr("data-attachmentID"));
      }

      //Grab data
      var title = $(".txtAttachmentTitle").val();
      var accessID = $(".ddlMainAttachmentPermissions option:selected").attr("value");
      var featured = $("#cbxAttachmentFeature").prop("checked");
      if(title == ""){ return; }
      var data = { attachmentID: attachmentID, nodeID: nodeID, title: title, accessID: accessID, featured: featured };

      //Act based on type and whether or not file/url is being added/replaced
      if(!$("#rblAttachmentNoAction").prop("checked")){
        //Add/replace file or URL
        if($("#rblAttachmentFile").prop("checked")){
          //Upload file
          data.attachmentType = "document";
          var message = JSON.stringify(data);
          var contents = $("#fileAttachmentFile").contents();
          if(contents.find("input[type=file]").val() == "") { return; }
          contents.find(".hdnMetadata").val(message);
          contents.find("form")[0].submit();
          $("#btn_attachmentSave").prop("disabled", true).attr("value", "...");
        }
        else {
          //Post URL
          data.url = $(".txtAttachmentUrl").val();
          data.attachmentType = "url";
          if(data.url == ""){ return; }
          doAjax("CurriculumService1", "Attachment_SaveURL", data, success_loadAttachments, $("#btn_attachmentSave"), "Successfully created attachment");
        }
      }
      else {
        //Just update ContentItem data, leaving document/url intact
        doAjax("CurriculumService1", "Attachment_UpdateData", data, success_loadAttachments, $("#btn_attachmentSave"), "Successfully updated attachment");
      }
    }

    //Edit an attachment
    function attachment_edit(id){
      //Cancel any previous editing
      attachment_cancel();

      //Find the relevant data
      var data = null;
      for(i in nodeAttachments){
        if(nodeAttachments[i].attachmentID == id){ 
          nodeAttachments[i].activeEditMode = true;
          data = nodeAttachments[i]; 
          //Show/hide move buttons as needed
          $("#btn_attachmentMoveUp, #btn_attachmentMoveDown").show();
          if(i == 0 || (i == 1 && nodeAttachments[0].featured)){
            $("#btn_attachmentMoveUp").hide();
          }
          if(i == nodeAttachments.length - 1 || nodeAttachments[i].featured){
            $("#btn_attachmentMoveDown").hide();
          }
        }
        else {
          nodeAttachments[i].activeEditMode = false;
        }
      }
      if(data == null){ return; }

      //Load the data into attachment manager
      $(".txtAttachmentTitle").val(data.title);
      $(".ddlMainAttachmentPermissions option[value=" + data.accessID + "]").prop("selected", true);
      $(".txtAttachmentUrl").val(data.url);
      $("#cbxAttachmentFeature").prop("checked", data.featured);
      $("#attachmentManager").attr("data-attachmentID", data.attachmentID);

      //Shift attachment manager into edit mode
      $("#attachmentManager").attr("data-mode", "edit");
      $("#rblAttachmentFile, #rblAttachmentUrl").prop("disabled", true);
      $("#rblAttachmentFileBox, #rblAttachmentUrlBox").hide();
      $("#rblAttachmentNoAction").prop("disabled", false).prop("checked", true);
      $("#rblAttachmentNoActionBox, #btn_attachmentCancel, #btnAddAttachmentStandards").show();
      $("#btn_attachmentSave").attr("value", "Save Changes");

      if(data.attachmentType == "document"){
        $("#rblAttachmentFile").prop("disabled", false);
        $("#rblAttachmentFileBox").show();
      }
      else if(data.attachmentType == "url"){
        $("#rblAttachmentUrl").prop("disabled", false);
        $("#rblAttachmentUrlBox").show();
      }
    
      //Add a class to the attachment's div
      $(".attachment[data-attachmentID=" + data.attachmentID + "]").addClass("beingEdited").find(".buttons.right").fadeOut();

      //Render standards for this attachment
      renderAttachmentStandards(id);

    }

    //Delete attachment
    function attachment_delete(id, button) {
      //Get attachment ID
      if(!confirm("Are you sure you want to delete this attachment? This action cannot be undone.")){ return; }
      var data = { nodeID: nodeID, attachmentID: id };

      //AJAX
      doAjax("CurriculumService1", "Attachment_Delete", data, success_loadAttachments, $(button), "Attachment deleted.");
    }

    //Move attachment
    function attachment_move(direction, button) {
      var id = 0;
      for(i in nodeAttachments){
        if(nodeAttachments[i].activeEditMode){
          id = nodeAttachments[i].attachmentID;
        }
      }
      var data = { nodeID: nodeID, attachmentID: id, direction: direction };
      doAjax("CurriculumService1", "Attachment_Reorder", data, success_moveAttachments, $(button), id);
    }

    //Cancel editing an attachment
    function attachment_cancel() {
      //Return list of attachments to normal
      for(i in nodeAttachments){
        nodeAttachments[i].activeEditMode = false;
      }

      //Reset the data in attachment manager
      $(".txtAttachmentTitle").val("");
      $(".ddlMainAttachmentPermissions option[value=1]").prop("selected", true);
      $(".txtAttachmentUrl").val("");
      $("#cbxAttachmentFeature").prop("checked", false);
      $("#attachmentManager").attr("data-attachmentID", 0);

      //Shift attachment manager into new mode
      $("#attachmentManager").attr("data-mode", "new");
      $("#rblAttachmentFile, #rblAttachmentUrl").prop("disabled", false);
      $("#rblAttachmentFileBox, #rblAttachmentUrlBox").show();
      $("#rblAttachmentNoAction").prop("disabled", true);
      $("#rblAttachmentFile").prop("checked", true);
      $("#rblAttachmentNoActionBox, #btn_attachmentCancel, #btnAddAttachmentStandards").hide();
      $("#btn_attachmentMoveUp, #btn_attachmentMoveDown").hide();
      $("#btn_attachmentSave").attr("value", "Save Attachment");

      //Remove "beingEdited" class from attachment divs
      $(".attachment").removeClass("beingEdited").find(".buttons.right").fadeIn();

      //Clear standards list
      renderAttachmentStandards(0);
    }

  </script>
  <script type="text/javascript">
    /* ---   ---   ---   AJAX Functions   ---   ---   --- */
    function doAjax(service, method, data, success, button, extra) {
      if (button) { 
        button.prop("disabled", true).attr("originalText", button.attr("value")).attr("value","..."); 
        button.parent().find("input[type=button]:visible").not(button).not(".btnHideOverride").addClass("tempHide").hide(); 
      }
      $.ajax({
        url: "/Services/" + service + ".asmx/" + method,
        contentType: "application/json; charset=utf-8",
        type: "POST",
        data: JSON.stringify(data)
      }).always(function (msg) {
        if (button) { 
          button.attr("value", button.attr("originalText")).prop("disabled", false); 
          button.parent().find(".tempHide").not(".btnHideOverride").removeClass("tempHide").show(); 
        }
        success((msg.d ? msg.d : msg), extra);
      });
    }

    //From file iframes
    function finishedUpload(data){
      if(data.valid){
        if(data.extra == "attachment"){
          nodeAttachments = data.data;
          $(".txtAttachmentTitle").val("");
          $(".ddlMainAttachmentPermissions option[value=1]").prop("selected",true);
          loadAttachments();
          alert("File upload successful. Please wait while the attachment list refreshes...");
        }
        else if(data.extra == "curriculumimage"){
          $("#curriculumImageDisplay").attr("src", data.data + "?rand=" + Math.random().toString().replace(".",""));
          alert("File upload successful.");
        }
        else {
          alert("File upload successful.");
        }
      }
      else {
        alert("File upload error: " + data.status);
      }
    }

    function success_save_properties(data){
      if(data.valid){
        //Save the image if there is one
        setupTree();
        var contents = $("#curriculumImage").contents();
        if($("#curriculumImage").length > 0 && contents.find("input[type=file]").val() != ""){
          contents.find(".hdnMetadata").val(JSON.stringify({ nodeID: nodeID }));
          contents.find("form")[0].submit();
          alert("Updated Level Properties. Your Custom Learning List Image is currently being processed, please wait.");
        }
        else {
          alert("Updated Level Properties.");
          $(".txtTitle").val(data.extra.title);
          $(".txtDescription").val(data.extra.description);
          $(".txtTimeframe").val(data.extra.timeframe);
        }
      }
      else {
        alert(data.status);
      }
    }

    function success_loadAttachments(data, message){
      if(data.valid){
        nodeAttachments = data.data;
        renderAttachments();
        $("#btn_attachmentSave").prop("disabled", false).attr("value", "Save Attachment");
        attachment_cancel();
        if(message){
          alert(message);
        }
      }
      else {
        alert(data.status);
      }
    }

    function success_moveAttachments(data, extra){
      if(data.valid){
        nodeAttachments = data.data;
        renderAttachments();
        attachment_edit(extra);
      }
      else {
        alert(data.status);
      }
    }

    function success_addStandardsToNode(data){
      if(data.valid){
        nodeStandards = data.data;
        cancelAddingStandardsToItem();
        renderNodeStandards();
      }
      else {
        alert(data.status);
      }
    }

    function success_addStandardsToAttachment(data, attachmentID){
      if(data.valid){
        nodeAttachments = data.data;
        cancelAddingStandardsToItem();
        renderAttachments();
        attachment_edit(attachmentID);
      }
      else {
        alert(data.status);
      }
    }

    function success_standard_update(data, targets){
      if(data.valid){
        refreshStandardsDisplay(data.data, targets.parentID);
        alert("Updated standards");
      }
      else {
        alert(data.status);
      }
    }

    function success_standard_delete(data, parentID){
      if(data.valid){
        refreshStandardsDisplay(data.data, parentID);
      }
      else {
        alert(data.status);
      }
    }

    function refreshStandardsDisplay(list, parentID){
      if(parentID == nodeID){ 
        nodeStandards = list;
        renderNodeStandards();
      }
      else {
        for(i in nodeAttachments){
          if(nodeAttachments[i].id == parentID){
            nodeAttachments[i].standards = list;
            renderAttachmentStandards(parentID);
          }
        }
      }
    }

    function success_node_create(data){
      if(data.valid){
        window.location.href = "/my/learninglist/" + data.data;
      }
      else {
        alert(data.status);
      }
    }

    function success_node_delete(data){
      if(data.valid){
        if(data.extra){ //deleted top level node
          alert("The learning list has been deleted.");
          window.location.href = "/My/Authored.aspx";
        }
        else {
          window.location.href = "/my/learninglist/" + data.data;
        }
      }
      else {
        alert(data.status);
      }
    }

    function success_node_move(data){
      if(data.valid){
        window.location.href = "/my/learninglist/" + data.data;
      }
      else {
        alert(data.status);
      }
    }

    function success_curriculum_publish(data){
      if(data.valid){
        window.location.href = "/resource/" + data.data;
      }
      else {
        alert(data.status);
      }
    }
  </script>
  <script type="text/javascript">
    /* ---   ---   ---   Rendering   ---   ---   --- */

    function renderAttachments() {
      var box = $("#currentFiles");
      var template = $("#template_attachment").html();
      box.html(nodeAttachments.length == 0 ? "<p class='grayMessage'>No Attachments have been added yet.</p>" : "");
      for(i in nodeAttachments){
        var current = nodeAttachments[i];
        current.featureText = current.featured ? "Featured" : "";
        current.featureClass = current.featured ? "featured" : "";
        current.featureButtonText = current.featured ? "Unfeature" : "Feature";
        current.standardsCount = current.standards.length;
        current.standardsCountS = current.standardsCount == 1 ? "" : "s";
        current.accessText = $(".ddlAttachmentPermissions option[value=" + current.accessID + "]").text();
        box.append(jsCommon.fillTemplate(template, current));
      }
    }

    function renderNodeStandards() {
      var box = $("#nodeStandards");
      var template = $("#template_currentStandard").html();
      box.html("");
      if(nodeStandards.length == 0){
        box.html("<p class='grayMessage'>This node doesn't have any standards aligned to it.</p>");
      }
      for(i in nodeStandards){
        var current = nodeStandards[i];
        current.type = "This node";
        renderStandard(box, current, template);
      }
    }

    function renderAttachmentStandards(id){
      var box = $("#attachmentStandards");
      var template = $("#template_currentStandard").html();
      box.html("");
      if(id == 0){ 
        box.html("<p class='grayMessage'>Select an existing attachment to view or edit its properties and standard alignments.</p>");
        return; 
      }
      for(i in nodeAttachments){
        if(nodeAttachments[i].attachmentID == id){
          var current = nodeAttachments[i];
          if(current.standards.length == 0){
            box.html("<p class='grayMessage'>This attachment doesn't have any standards aligned to it.</p>");
            return;
          }
          for(i in current.standards){
            var std = current.standards[i];
            std.type = "This attachment";
            renderStandard(box, std, template);
          }
        }
      }
    }

    function renderStandard(box, data, template){
      box.append(jsCommon.fillTemplate(template, data));
      box.find(".contentStandard[data-recordID=" + data.recordID + "] .ddlAlignmentType option[value=" + data.alignmentID + "]").prop("selected", true);
      box.find(".contentStandard[data-recordID=" + data.recordID + "] .ddlUsageType option[value=" + data.usageID + "]").prop("selected", true);
    }
  </script>

  <style type="text/css">
    /* Big Stuff */
    #content { padding: 10px; min-width: 320px; }
    #editor { position: relative; min-height: 550px; }
    #editor:after { content: " "; display: block; clear: both; }
    #treeBox { width: 300px; float: left; min-height: 500px; }
    #node { margin-left: 305px; }

    /* Tree stuff */
    #tree { background-color: #FFF; background-image: linear-gradient(#F5F5F5,transparent); border: 1px solid #CCC; border-radius: 5px; overflow: auto; padding-bottom: 10px; }
    #tree, #actions { display: none; }
    #tree .jstree-wholerow { border-radius: 0; }
    #tree .jstree-node { margin-left: 15px; }
    #tree li[data-root=root] { margin-left: 0; }
    #tree .jstree-themeicon { display: none; }
    .jstree-contextmenu { border-radius: 5px; }
    .jstree-contextmenu li:last-child, .jstree-contextmenu .vakata-context-separator, .vakata-context li > a .vakata-contextmenu-sep { display: none; }
    .jstree-contextmenu li a { padding: 2px 10px; line-height: 1.5em; }
    .vakata-context li > a > i:empty { display: none; }
    .vakata-context li > a:hover, .vakata-context .vakata-context-hover > a { background-color: #EEE; box-shadow: none;  border: none;}

    /* Node stuff */
    .midHeader { background-color: #DDD; margin-bottom: 10px; padding: 3px 5px; margin: 5px -5px 10px -5px; font-size: 18px; }
    .midHeader:first-child { margin-top: -5px; }
    .column.left .midHeader { margin-left: -15px; padding-left: 15px; }
    .column.right .midHeader { margin-right: -15px; padding-right: -15px; }
    #tabNavigation { padding: 0 0 0 5px; }
    #tabNavigation input { border-radius: 5px 5px 0 0; width: auto; }
    #tabNavigation input.current { background-color: #9984BD; }
    .tab { margin-bottom: 10px; }
    .column { display: inline-block; width: 50%; vertical-align: top; padding: 0 5px; }
    input[type=text], select, textarea { width: 100%; margin: 5px 0; min-height: 25px; }
    iframe.fileIframe { height: 25px; border: none; width: 100%; margin: 5px 0; overflow: hidden; }
    textarea { min-height: 5em; height: 8em; max-height: 20em; resize: vertical; }
    .buttons { padding: 5px; }
    .buttons input { width: auto; margin-bottom: 2px; }
    .buttons.spaced input, .buttons.spaced #previewLink { margin: 5px 0 2px 0; }
    .buttons.right { text-align: right; }
    .rblBox, .cbxBox { padding-left: 25px; position: relative; height: 35px; line-height: 35px; border-radius: 5px; display: block; }
    .rblBox:hover, .cbxBox:hover { background-color: #DDD; cursor: pointer; }
    .rblBox input[type=radio], .cbxBox input[type=checkbox] { position: absolute; top: 5px; left: 0; margin: 5px; }
    .rblBox input[type=radio]:hover, .rblBox input[type=radio]:focus, .cbxBox input[type=checkbox]:hover, .cbxBox input[type=checkbox]:focus { cursor: pointer; }
    .lightBox { border: 1px solid #CCC; border-radius: 5px; padding: 5px; margin-bottom: 10px; }
    .lightBox .header { background-color: #DDD; color: #000; font-size: 16px; margin: -5px -5px 5px -5px; font-weight: normal; }
    .attachment.lightBox .header { margin-top: 0; }
    .attachment:before { height: 0; content: "Currently Editing..."; display: block; background-color: #9984BD; color: #FFF; font-weight: bold; margin: -5px -5px 0px -5px; box-sizing: border-box; transition: height 0.5s, border-color 0.5s, padding 0.5s, margin 0.5s; padding: 0; overflow: hidden; }
    .attachment.beingEdited { border-color: #9984BD; }
    .attachment.beingEdited .buttons { top: 46px; }
    .attachment.beingEdited:before { height: 30px; padding: 2px 5px 8px 5px; margin-bottom: -5px; }
    .attachment .header.featured { background-image: linear-gradient(90deg,#DDD,#DDD,#FF6A00); }
    .attachment .header .featured { font-weight: bold; float: right; color: #FFF; }
    .rblBox.attachmentNoAction .grayMessage, .cbxBox .grayMessage { text-align: left; margin: 0; padding: 0; line-height: 35px; }
    .attachment { position: relative; }
    .attachment .info { padding-right: 85px; }
    .attachment .info > * { display: block; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .attachment .buttons { position: absolute; right: 0; top: 23px; transition: top 0.5s; width: 90px; }
    .attachment .buttons input { width: 100%; margin-bottom: 5px; }
    .attachment .info a { font-weight: bold; }

    /* Standards */
    .contentStandard .ddls { white-space: nowrap; font-size: 0; text-align: center; }
    .contentStandard .ddls select { font-size: 16px; width: 49%; text-align: left; }
    .contentStandard .ddls select:first-child { border-radius: 5px 0 0 5px; }
    .contentStandard .ddls select:last-child { border-radius: 0 5px 5px 0; }
    #standardsBrowserOverlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: #000; opacity: 0.5; z-index: 1000000; }
    #standardsBrowserContainer { position: absolute; top: 100px; left: 5%; right: 5%; z-index: 10000000; }

    /* Miscellaneous */
    div[class*='at4-'] { display: none; }
    .grayMessage { text-align: center; padding: 10px; font-style: italic; color: #777; }
    .hidden { display: none; }
    #btnShowExpandedInterface { display: none; }
    #topLevelOptions { margin-left: -5px; }
    .listPanel input[type=checkbox] { margin-right: 5px; }
    #helpBox { margin-bottom: 10px; display: none; }
    #helpBox.visible { display: block; }
    #btnShowHelp { width: auto; display: inline; float: right; margin: 5px; }
    #helpBox p { margin-bottom: 10px; }
    #curriculumImageDisplay { display: block; width: 100%; max-width: 400px; margin: 5px auto; }
    #previewLink { width: auto; display: inline-block; text-align: center; border: 1px solid #CCC; padding: 2px 5px; border-color: #EEE #999 #999 #EEE; }
    #btnSaveProperties { width: 100%; display: block; }

    @media (max-width: 675px) {
      #treeBox { width: 100%; float: none; margin-bottom: 10px; }
      #tabNavigation input { border-radius: 5px; margin: 2px 0.5%; width: 48%; }
      #node { margin-left: 0; }
    }
    @media (max-width: 950px) {
      .tab > .column { width: 100%; display: block; }
      .column.left .midHeader, .column.right .midHeader { margin: 10px -15px; }
      .column.left .midHeader:first-child { margin: -5px -15px 10px -15px; }
    }
    @media (min-width: 1650px) {
      .expanded { white-space: nowrap; }
      .expanded .tab, .expanded tab.hidden { white-space: normal; display: inline-block; width: 49%; margin: 0 0.5%; vertical-align: top; }
      .expanded #tabNavigation { display: none; }
      #btnShowExpandedInterface { display: inline; float: right; width: auto; margin: 5px; }
    }

  </style>

  <div id="content">
    <h1 class="isleH1">IOER Learning List Editor <input type="button" id="btnShowHelp" class="isleButton bgOrange" value="Help" onclick="toggleHelpBox();" /> <input type="button" id="btnShowExpandedInterface" class="isleButton bgGreen" value="Expanded Interface" onclick="toggleExpandedInterface();" /></h1>

    <div id="helpBox" class="grayBox">
      <h2 class="header">Learning List Editor Help &amp; Guidance</h2>
      <select id="ddlHelp">
        <option value="introduction">Select a topic...</option>
        <option value="nodes">Levels</option>
        <option value="attachments">Attachments</option>
        <option value="standards">Learning Standards</option>
        <option value="publish">Publishing</option>
      </select>
      <div class="topics">
        <div class="topic" data-topicID="introduction">
          <p>Welcome to the IOER Learning List Editor. This tool makes it easy to build a heirarchical list of related information and resources. It is suitable for projects as small as a lesson that only needs a few attachments, or an entire school year's curriculum.  The process overall is easy:</p>
          <ol>
            <li>Create a <a href="#" onclick="showHelp('nodes'); return false;">Level</a></li>
            <li>Add <a href="#" onclick="showHelp('attachments'); return false;">Attachments</a></li>
            <li>Add <a href="#" onclick="showHelp('standards'); return false;">Learning Standards</a></li>
            <li>Add or remove <a href="#" onclick="showHelp('nodes'); return false;">child Levels</a> at levels below the top level</li>
            <li><a href="#" onclick="showHelp('publish'); return false;">Publish your Learning List</a></li>
          </ol>
        </div>
        <div class="topic" data-topicID="nodes">
          <p>Levels are the building blocks of a Learning List. Each level represents a concrete unit of information that may be aligned to standards and may have one or more attachments that help define it or help others make use of it. You can think of levels like folders on your computer--You can add files to them, and create more folders inside them. Levels let you add attachments and create child levels underneath them.  This enables a heirarchical structure like a curriculum, just as you might setup a tree of folders and subfolders, or an indented list in an outline.</p>
          <p>Use the navigation list on the left to create, rearrange, delete, and navigate between levels as you build your Learning List. Just as a table of contents defines the organization of chapters in a book (while the chapters themselves contain the actual information), the navigation list defines the organization of levels in a Learning List, while the levels themselves contain the information.</p>
          <p>A finished Learning List's structure might resemble something like this Curriculum:</p>
          <ul>
            <li>My Math Curriculum</li>
            <ul>
              <li>Module 1</li>
              <ul>
                <li>Unit 1</li>
                <li>Unit 2</li>
                <ul>
                  <li>Lesson 1</li>
                  <li>Lesson 2</li>
                </ul>
                <li>Unit 3</li>
                <ul>
                  <li>Lesson 1</li>
                  <li>Lesson 2</li>
                </ul>
              </ul>
              <li>Module 2</li>
              <li>Module 3</li>
            </ul>
          </ul>
        </div>
        <div class="topic" data-topicID="standards">
          <p>Each level can be aligned to one or more Learning Standards. Each level's attachments can also be individually aligned to Learning Standards. You won't see it in this editor, but when you or someone else views your Learning List, the Standards from levels and attachments below which ever level is being viewed will "bubble up" and be visible as being a part of that level. This means that if you create a curriculum and align individual pieces of it to standards, someone else will be able to see all of the standards that the curriculum aligns to by simply visiting the top level level.</p>
          <p>You should align a <b>level</b> to a standard if the entire level as a whole is appropriate for that standard; if an individual <b>attachment</b> on its own meets the criteria below, align that attachment to the standard instead.</p>
          <p>Learning Standards have one of four types of alignments:</p>
          <dl>
            <dt>General Alignment</dt>
            <dd>"This level/attachment aligns to this standard"</dd>
            <dd>This is the default alignment, suitable for when a level/attachment is associated with the standard, but not exclusively or distinctly in one of the ways below.</dd>
            <dt>Assessment Alignment</dt>
            <dd>"This level/attachment assesses this standard"</dd>
            <dd>This alignment is suitable for levels/attachments that contain assessments (or for attachments that <i>are</i> assessments) that test a student's ability to meet the standard.</dd>
            <dt>Teaching Alignment</dt>
            <dd>"This level/attachment teaches this standard"</dd>
            <dd>This alignment is suitable for levels/attachments intended to help students learn the standard.</dd>
            <dt>Requirement Alignment</dt>
            <dd>"This level/attachment requires this standard"</dd>
            <dd>This alignment is suitable for levels/attachments that assume a student already knows and/or meets the standard.</dd>
          </dl>
          <p>Learning Standards also have their alignments ranked by how strongly the level or attachment meets the above alignment:</p>
          <ul>
            <li>Major</li>
            <li>Supporting</li>
            <li>Additional</li>
          </ul>
        </div>
        <div class="topic" data-topicID="attachments">
          <p>Attachments are files or references that can be associated directly with a level. For example, imagine you have a level that represents a single lesson for your class. That lesson requires a page of excerpts from a text, a worksheet, and a quiz. You would add all three as attachments to that level, and select a more restrictive access level for the quiz so students can't find it.</p>
          <p>When you or others are viewing your finished Learning List, you have the option to automatically display one attachment from each level. To do so, just mark the document as "featured".</p>
        </div>
        <div class="topic" data-topicID="publish">
          <p>When your Learning List is finished, visit the top level level. A single click of the Publish button will automatically:</p>
          <ul>
            <li>Make the Learning List publicly visible (access will still be determined by the access options you selected for each level and attachment)</li>
            <li>Enable the Learning List to be added to IOER Libraries</li>
            <li>Publish information about the Learning List to the <a href="http://learningregistry.org/" class="textLink" target="_blank">Learning Registry</a> so others from around the world can find it</li>
          </ul>
        </div>
      </div>
    </div>

    <!-- Editor -->
    <div id="editor">
      <!--Tree Box -->
      <div id="treeBox" class="grayBox">
        <h2 class="header">Learning List Navigation</h2>
        <div id="loadingTree" class="grayMessage">Loading...</div>
        <div id="tree"></div>
        <div id="actions">
          <% var canMoveUp = nodeSiblings.Where( m => m.SortOrder < currentNode.SortOrder ).Count() > 0; %>
          <% var canMoveDown = nodeSiblings.Where( m => m.SortOrder > currentNode.SortOrder ).Count() > 0; %>
          <% var canIndent = canMoveUp; %>
          <% var canOutdent = currentNode.ParentId != curriculumID && currentNode.Id != curriculumID; %>
          <select id="treeActions">
            <option value="addSubNode">Add a child level</option>
            <% if (canMoveUp){ %><option value="moveNodeUp">Move current level up</option><% } %>
            <% if (canMoveDown){ %><option value="moveNodeDown">Move current level down</option><% } %>
            <% if (canMoveUp){ %><option value="indentNode">Indent current level</option><% } %>
            <% if (canOutdent) { %><option value="outdentNode">Outdent current level</option><% } %>
            <option value="deleteNode">Delete current level</option>
          </select>
          <input type="button" class="isleButton bgBlue" value="Select" id="btnDoNodeAction" onclick="doNodeAction();" />
        </div>
      </div><!-- /treeBox -->

      <!-- Node -->
      <div id="node">
        <!-- Tab Navigation -->
        <div id="tabNavigation">
          <input type="button" class="isleButton bgBlue current" value="Level Properties" data-id="properties" onclick="showTab('properties');" />
          <input type="button" class="isleButton bgBlue" value="Attachments" data-id="attachments" onclick="showTab('attachments');" />
        </div>
        <!-- Tabs -->
        <div id="tabs">
          <!-- Node Properties -->
          <div class="tab grayBox" id="tab_properties">
            <h2 class="header">Level Information</h2>
            <div class="column left">
              <h3 class="midHeader">Basic Information</h3>
              <input type="text" class="txtTitle" id="txtTitle" placeholder="Title" runat="server" />
              <textarea class="txtDescription" id="txtDescription" placeholder="Description" runat="server"></textarea>
              <input type="text" class="txtTimeframe" id="txtTimeframe" placeholder="Approximate timeframe to complete" runat="server" />
              <asp:DropDownList ID="ddlNodePermissions" CssClass="ddlNodePermissions" runat="server"></asp:DropDownList>
              <% if( currentNode.Id == curriculumID ) { %>
              <label>Custom Learning List Image (will be resized to 400x300 pixels)</label>
              <img src="<%=(string.IsNullOrWhiteSpace(currentNode.ImageUrl) ? "/images/icons/icon_upload_400x300.png" : currentNode.ImageUrl.Replace( @"\", "" )) %>" id="curriculumImageDisplay" />
              <iframe id="curriculumImage" class="fileIframe" src="/controls/curriculum/curriculumfileupload.aspx?usage=curriculumimage"></iframe>
                <% if (false) { %>
                <div id="topLevelOptions">
                  <p class="grayMessage">Selections below will apply to <b><u>all</u></b> items that are a part of this Learning List.</p>
                  <div class="column left bottomMargin k12subject">
                    <h3 class="midHeader columnLeft">K-12 Subjects</h3>
                    <custom:CheckBoxList ID="cbxlK12Subject" runat="server" TargetTable="subject" UpdateMode="raw" />
                  </div><!-- prevent white space
               --><div class="column gradelevel">
                    <h3 class="midHeader columnRight">Grade Level</h3>
                    <custom:CheckBoxList ID="cbxlGradeLevel" runat="server" TargetTable="gradeLevel" UpdateMode="raw" />
                  </div>
                </div>
                <% } %>
              <% } %>

                <asp:Label ID="lblHistory" runat="server"></asp:Label>
              <div class="buttons spaced right">
                <input type="button" class="isleButton bgBlue" id="btnSaveProperties" value="Save Changes" onclick="save_properties();" />
                <% if( currentNode.Id == curriculumID ) { %>
                  <% if( currentNode.IsPublished ){ %>
                <input type="button" class="isleButton bgGreen" id="btnViewCurriculum" value="View Learning List Tags" onclick="viewCurriculumTags();" />
                  <% } else { %>
                <input type="button" class="isleButton bgGreen" id="btnPublishCurriculum" value="Publish Learning List" onclick="publishCurriculum();" />
                  <% } %>
                <% } %>
                <% if(currentNode.Id != 0) { %>
                <a href="/learninglist/<%=currentNode.Id %>" target="_blank" class="isleButton bgGreen" id="previewLink">Preview this Level</a>
                <% } %>
                <input type="button" class="isleButton bgBlue" id="btnAddNodeStandards" value="Add Standards..." onclick="showStandardsBrowser('node');" />
              </div>
            </div><div class="column right">
              <h3 class="midHeader">Level Standards</h3>
              <div id="nodeStandards"></div>
            </div>
          </div>

          <!-- Attachments -->
          <div class="tab grayBox hidden" id="tab_attachments">
            <h2 class="header">Attachments</h2>
            <div class="column left">
              <h3 class="midHeader">Attach an Attachment</h3>
              <div id="attachmentManager" data-mode="new">
                <input type="text" class="txtAttachmentTitle" id="txtAttachmentTitle" placeholder="Title" runat="server" />
                <label class="rblBox" id="rblAttachmentFileBox">
                  <input type="radio" name="rblAttachment" id="rblAttachmentFile" value="attachmentFile" checked="checked" />
                  <iframe id="fileAttachmentFile" class="fileIframe" src="/controls/curriculum/curriculumfileupload.aspx?usage=attachment"></iframe>
                </label>
                <label for="rblAttachmentUrl" class="rblBox" id="rblAttachmentUrlBox">
                  <input type="radio" name="rblAttachment" id="rblAttachmentUrl" value="attachmentUrl" />
                  <input type="text" class="txtAttachmentUrl" id="txtAttachmentUrl" runat="server" placeholder="Webpage URL" />
                </label>
                <label for="rblAttachmentNoAction" class="rblBox attachmentNoAction" id="rblAttachmentNoActionBox" style="display: none;">
                  <input type="radio" name="rblAttachment" id="rblAttachmentNoAction" value="attachmentNoAction" disabled="disabled" />
                  <p class="grayMessage">Do not replace file or URL</p>
                </label>
                <asp:DropDownList ID="ddlAttachmentPermissions" CssClass="ddlMainAttachmentPermissions" runat="server"></asp:DropDownList>
                <label for="cbxAttachmentFeature" id="cbxAttachmentFeatureBox" class="cbxBox">
                  <input type="checkbox" name="cbxAttachmentFeature" id="cbxAttachmentFeature" />
                  <pre class="grayMessage">Feature this item on the level's main page</pre>
                </label>
                <div class="buttons spaced right">
                  <input type="button" class="isleButton bgBlue btnHideOverride" id="btn_attachmentMoveUp" onclick="attachment_move('up',this);" value="Move ↑" style="display:none;" />
                  <input type="button" class="isleButton bgBlue btnHideOverride" id="btn_attachmentMoveDown" onclick="attachment_move('down',this);" value="Move ↓" style="display:none;" />
                  <input type="button" class="isleButton bgBlue" id="btn_attachmentSave" onclick="attachment_save();" value="Save Attachment" />
                  <input type="button" class="isleButton bgRed" id="btn_attachmentCancel" onclick="attachment_cancel();" value="Cancel" style="display: none;" />
                  <input type="button" class="isleButton bgBlue" id="btnAddAttachmentStandards" value="Add Standards..." onclick="showStandardsBrowser('attachment');" style="display: none;" />
                </div>
              </div>
              <h3 class="midHeader">Current Attachments</h3>
              <div id="currentFiles"></div>
            </div><div class="column right">
              <h2 class="midHeader">Attachment Standards</h2>
              <div id="attachmentStandards"><p class="grayMessage">Select an existing attachment to view or edit its properties and standard alignments.</p></div>
            </div>
          </div>

        </div><!-- /tabs -->
      </div><!-- /node -->
    </div><!-- /editor -->
  </div>

  <div id="templates" style="display:none;">
    <div id="template_currentStandard">
      <div class="contentStandard lightBox" data-recordID="{recordID}" data-standardID="{standardID}">
        <h3 class="header">{code}</h3>
        <div class="data">
          <p>{text}</p>
        </div>
        <div class="ddls">
          <select class="ddlAlignmentType">
            <option value="0">{type} aligns to</option>
            <option value="1">{type} assesses</option>
            <option value="2">{type} teaches</option>
            <option value="3">{type} requires</option>
          </select>
          <select class="ddlUsageType">
            <option value="1">this major standard</option>
            <option value="2">this supporting standard</option>
            <option value="3">this additional standard</option>
          </select>
        </div>
        <div class="buttons right">
          <input type="button" class="isleButton bgBlue btnSave" style="display:none;" value="Save" onclick="standard_update({contentID}, {recordID}, this);" />
          <input type="button" class="isleButton bgRed" value="Remove" onclick="standard_delete({contentID}, {recordID}, this);" />
        </div>
      </div>
    </div>
    <div id="template_attachment">
      <div class="attachment lightBox" data-attachmentID="{attachmentID}">
        <div class="header {featureClass}">{title} <span class="featured">{featureText}</span></div>
        <div class="info">
          <a href="{url}" target="_blank">View Attachment...</a>
          <div class="accessText">{accessText}</div>
          <div class="standardsText">Aligned to {standardsCount} standard{standardsCountS}.</div>
        </div>
        <div class="buttons right">
          <input type="button" class="isleButton bgBlue" onclick="attachment_edit({attachmentID});" value="Select" />
          <input type="button" class="isleButton bgRed" onclick="attachment_delete({attachmentID}, this);" value="Delete" />
        </div>
      </div>
    </div>
    <asp:DropDownList ID="ddlAttachmentTemplatePermissions" CssClass="ddlAttachmentPermissions" runat="server"></asp:DropDownList>
  </div>

  <div id="standardsBrowserOverlay" style="display:none;"></div>
  <div id="standardsBrowserContainer" style="display:none;" class="grayBox">
    <h2 class="header">Standards Browser</h2>
    <p id="standardsBrowserInstructions"></p>
    <uc1:StandardsBrowser ID="standardsBrowser" runat="server" />
    <div class="buttons right">
      <input type="button" id="btnFinishAddingStandards" class="isleButton bgBlue" value="Finish" onclick="addStandardsToItem();" />
      <input type="button" id="btnCancelAddingStandards" class="isleButton bgRed" value="Cancel" onclick="cancelAddingStandardsToItem();" />
    </div>
  </div>

</div><!--/ curriculumEditor -->
<div id="curriculumStarter" runat="server">
  <style type="text/css">
    #starterBox { max-width: 600px; margin: 10px auto; }
    #starterBox input, #starterBox textarea, #starterBox select { width: 100%; margin-bottom: 10px; }
    #starterBox textarea { resize: vertical; height: 5em; min-height: 4em; max-height: 10em; }
  </style>
  <script type="text/javascript">
    function submitForm() {
      $("form").removeAttr("onsubmit");
      $("form")[0].submit();
    }
  </script>
  <h1 class="isleH1">IOER Learning List Editor</h1>
  <div class="grayBox" id="starterBox">
    <h2 class="header">Create a new Learning List</h2>
    <p>Use this tool to group and show relationships between educational resources.  To get started, enter a name and description below. In the next step, you'll upload an image.</p>
    <p>Please enter a title and description that help other users get a good idea of what your Learning List is for.</p>
    <div id="starterForm">
      <h3>Title</h3>
      <input type="text" id="txtStarterTitle" class="txtStarterTitle" runat="server" placeholder="Enter a title" />
      <h3>Description</h3>
      <textarea id="txtStarterDescription" class="txtStarterDescription" runat="server" placeholder="Enter a description"></textarea> 
      <h3>Create learning list on behalf of an organization (optional):</h3>
      <select id="ddlOrganization" class="ddlOrganization" runat="server" visible="true"></select>
      <input type="button" class="isleButton bgBlue" value="Create!" onclick="submitForm();" />
    </div>
  </div>
</div>
<div id="curriculumError" runat="server">
  <p style="text-align: center; margin: 50px auto;" id="curriculumErrorMessage" runat="server"></p>
  <div id="curriculumErrorMessageHidden" runat="server" style="display:none;"></div>
</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
  <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false"></asp:Literal>
</asp:Panel>