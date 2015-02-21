<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CurriculumEditor1.ascx.cs" Inherits="ILPathways.Controls.Curriculum.CurriculumEditor1" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/controls/standardsbrowser7.ascx" %>
<%@ Register TagPrefix="custom" TagName="CheckBoxList" Src="/LRW/Controls/ListPanel.ascx" %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor" %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.ToolbarButton" %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.ContextMenu"  %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.Popups" %>

<!-- JSTree plugin -->
<script type="text/javascript" src="/scripts/jstree/jstree.min.js"></script>
<link rel="stylesheet" href="/scripts/jstree/themes/ioer/style.css" type="text/css" />

<script type="text/javascript" src="/scripts/widgets/postmessagereceiver.js"></script>
<script type="text/javascript" src="/scripts/jscommon.js"></script>
<script type="text/javascript">
  //From Server
  var curriculumID = <%=curriculumID %>;
  var currentNodeID = <%=CurrentRecord.Id %>;
  var nodeParentID = <%=nodeParentID %>;
  var currentStandards = <%=currentStandardsJSON %>;
  var currentAttachments = <%=currentAttachmentsJSON %>;

  //Page properties
  var curriculumMode = true;
  var SB7mode = "tag";
</script>
<script type="text/javascript">
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

    //Initialize the tree with the data
    $("#tree").jstree({
      core: {
        check_callback: true,
        data: function(node, callback){
          callback.call(this, data.data);
        }
      },
      plugins: ["contextmenu", "dnd", "unique", "wholerow"],
      dnd: { copy: false },
      //contextmenu: { items: { ccp: false } }
    });

    //Add tree function hooks
    $("#tree").jstree("open_all");
    $(".jstree-leaf:first-child").each(function() { $("#tree").jstree("close_node", $(this).parent()) });
    $("#tree").on("click", "a", function(e, d) {
      window.location.href = $(this).attr("href");
    });
    setTimeout(function() {
      $("#tree").jstree("select_node", currentNodeID, false, false);
      $("#tree, #actions").fadeIn();
      $("#loadingTree").remove();
    }, 1000);
  }
</script>
<script type="text/javascript">
  /* ---   ---   ---   Initialization   ---   ---   --- */
  $(document).ready(function () {
    //Show tab
    showInitialTab();
    //Render existing standards
    renderCurrentStandards();
    //Render existing attachments
    renderAttachments();
    //Fix MS formatting
    fixCBXLFormatting();
    //Setup the tree
    setupTree();
    //Listen for added Standards
    $(window).on("standard_added", function() { preventDuplicateStandards(); });
    //Listen for changes in attachments
    $("#currentFiles").on("change", ".attachment select", function() { $(this).parent().find(".buttons .update").show(); });
  });

  //Show initial tab
  function showInitialTab() {
    var id = window.location.href.split("#");
    var target = (id.length > 1 ? id[1] : "properties");
    showTab(target);
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

  /* ---   ---   ---   Page Functions   ---   ---   --- */
  //Show a tab
  function showTab(target) {
    $(".tab").addClass("hidden");
    $("#tab_" + target).removeClass("hidden");
    $("#tabNavigation input").removeClass("current");
    $("#tabNavigation input[data-id=" + target + "]").addClass("current");
  }

  //Toggle expanded interface at large screen resolutions
  function toggleExpandedInterface() {
    $("#node").toggleClass("expanded");
  }

  //Save Node properties
  function save_properties() {
    var data = {
      curriculumID: curriculumID,
      nodeID: currentNodeID,
      parentID: nodeParentID,
      title: $(".txtTitle").val(),
      timeframe: $(".txtTimeframe").val(),
      accessID: parseInt($(".ddlNodePermissions option:selected").attr("value")),
      description: $(".txtDescription").val(),
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
  
  //Save standards
  function standards_add() {
    var standards = [];
    SBstoreDDLValues();
    for(i in selectedStandards){
      var current = selectedStandards[i];
      standards.push({
        id: current.id,
        code: current.fullCode,
        alignmentID: current.alignmentType,
        usageID: current.usageType,
        isContentStandard: true,
        text: ""
      });
      data = {
        nodeID: currentNodeID,
        standards: standards
      };
    }
    doAjax("CurriculumService1", "Standards_Add", data, success_standards_save, $("#btnSaveStandards"), null);
  }

  //Update existing standard
  function standard_update(id, button){
    var box = $(".existingStandard[data-standardID=" + id + "]");
    var alignmentID = parseInt(box.find(".ddlAlignentType option:selected").attr("value"));
    var usageID = parseInt(box.find(".ddlUsageType option:selected").attr("value"));
    var data = { nodeID: currentNodeID, standard: { id: id, alignmentID: alignmentID, usageID: usageID } };
    doAjax("CurriculumService1", "Standard_Update", data, success_standards_save, $(button), null);
  }

  //Delete standard
  function standard_delete(id, button){
    var data = { nodeID: currentNodeID, contentStandardID: id };
    doAjax("CurriculumService1", "Standard_Delete", data, success_standards_save, $(button), null);
  }

  //Save attachment
  function attachment_save() {
    var title = $(".txtAttachmentTitle").val();
    var accessID = $(".ddlMainAttachmentPermissions option:selected").attr("value");
    if(title == ""){ return; }
    var data = { nodeID: currentNodeID, title: title, accessID: accessID };
    if($("#rblAttachmentFile").prop("checked")){
      //Upload file
      var message = JSON.stringify(data);
      var contents = $("#fileAttachmentFile").contents();
      if(contents.find("input[type=file]").val() == "") { return; }
      contents.find(".hdnMetadata").val(message);
      contents.find("form")[0].submit();
    }
    else {
      //Post URL
      data.url = $(".txtAttachmentUrl").val();
      if(data.url == ""){ return; }
      doAjax("CurriculumService1", "Attachment_SaveURL", data, success_loadAttachments, $("#btn_attachmentSave"), null);
    }
  }
  //Update an attachment
  function attachment_update(id, button) {
    //Get access rights ID
    var box = $("#currentFiles .attachment[data-attachmentID=" + id + "]");
    var accessID = parseInt(box.find(".ddlAttachmentPermissions option:selected").attr("value"));
    var data = { nodeID: currentNodeID, attachmentID: id, accessID: accessID };
    //AJAX
    doAjax("CurriculumService1", "Attachment_Update", data, success_loadAttachments, $(button), null);
  }
  //Rename attachment
  function attachment_rename(id, button) {
    //Set new title
    var title = prompt("Enter a new title for this attachment.", "");
    if(title == null) { return; }
    var data = { nodeID: currentNodeID, attachmentID: id, title: title };

    //AJAX
    doAjax("CurriculumService1", "Attachment_Rename", data, success_loadAttachments, $(button), null);
  }
  //Delete attachment
  function attachment_delete(id, button) {
    //Get attachment ID
    if(!confirm("Are you sure you want to delete this attachment? This action cannot be undone.")){ return; }
    var data = { nodeID: currentNodeID, attachmentID: id };

    //AJAX
    doAjax("CurriculumService1", "Attachment_Delete", data, success_loadAttachments, $(button), null);
  }
  //Feature attachment
  function attachment_feature(id, button) {
    //Get attachment ID
    var data = { nodeID: currentNodeID, attachmentID: id };

    //AJAX
    doAjax("CurriculumService1", "Attachment_Feature", data, success_loadAttachments, $(button), null);
  }

  //Save a news item
  function save_news() {
    var data = { nodeID: currentNodeID, text: $(".oae_editor_editpanel iframe").first().contents().find("body").html() };
    doAjax("CurriculumService1", "Save_News", data, success_save_news, $("#btnSaveNews"), null);
  }

  //Load attachments
  function loadAttachments() {
    doAjax("CurriculumService1", "GetAttachments", { nodeID: currentNodeID }, success_loadAttachments, null, null);
  }

  function doNodeAction() {
    var action = $("#treeActions option:selected").attr("value");
    switch(action){
      case "addAfterCurrent":
        var newNode = $("#tree").jstree("create_node", $("#tree").jstree("get_selected"), { text: "New Node" }, "after", false, false);
        $("#tree").jstree("deselect_all");
        $("#tree").jstree("select_node", newNode);
        break;
      case "addBelowCurrent":
        var newNode = $("#tree").jstree("create_node", $("#tree").jstree("get_selected"), { text: "New Node" }, "inside", false, false);
        $("#tree").jstree("deselect_all");
        $("#tree").jstree("select_node", newNode);
        break;
      case "moveNodeUp":
        var node = $("#" + $("#tree").jstree("get_selected")[0]);
        var index = node.parent().children().index(node);
        if(index == 0){ return; }
        else { $("#tree").jstree("move_node", node, node.parent(), index - 1); }
        break;
      case "moveNodeDown":
        var node = $("#" + $("#tree").jstree("get_selected")[0]);
        var children = node.parent().children();
        console.log(children);
        var index = children.index(node);
        console.log(index);
        if(index == children.length - 1){ return; }
        else { $("#tree").jstree("move_node", node, node.parent(), index + 2); }
        break;
      case "deleteNode":
        if($("#tree").jstree("get_selected")[0] == "#"){ return; }
        if(!confirm("Are you sure you want to delete this node? This action cannot be undone.")){ return; }
        var temp = $("#" + $("#tree").jstree("get_selected")[0]).parent();
        $("#tree").jstree("delete_node", $("#tree").jstree("get_selected"));
        $("#tree").jstree("select_node", temp);
        break;
      default:
        break;
    }
  }
  
  //Prevent duplicate standards
  function preventDuplicateStandards() {
    for(i in selectedStandards){
      if($(".existingStandard[data-standardID=" + selectedStandards[i].id + "]").length > 0){
        removeSelectedStandard(selectedStandards[i].id);
        alert("That standard was already added.");
      }
    }
  }

  /* ---   ---   ---   AJAX   ---   ---   --- */
  function doAjax(service, method, data, success, button, extra) {
    if (button) { button.attr("disabled", "disabled").attr("originalText", button.attr("value")).attr("value","..."); }
    $.ajax({
      url: "/Services/" + service + ".asmx/" + method,
      contentType: "application/json; charset=utf-8",
      type: "POST",
      data: JSON.stringify(data),
      success: function (msg) { success((msg.d ? msg.d : msg), extra); }
    }).always(function () {
      if (button) { button.attr("value", button.attr("originalText")).removeAttr("disabled"); }
    });
  }

  //From file iframes
  function finishedUpload(data){
    if(data.valid){
      if(data.extra == "attachment"){
        currentAttachments = data.data;
        $(".txtAttachmentTitle").val("");
        $(".ddlMainAttachmentPermissions option[value=1]").prop("selected",true);
        loadAttachments();
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
      if($("#curriculumImage").length > 0 && $("#curriculumImage").contents().find("input[type=file]").val() != ""){
        $("#curriculumImage").contents().find("form")[0].submit();
        alert("Updated Node Properties. Your Custom Curriculum Image is currently being processed, please wait.");
      }
      else {
        alert("Updated Node Properties.");
      }
    }
    else {
      alert(data.status);
    }
  }

  function success_standards_save(data){
    if(data.valid){
      currentStandards = data.data;
      renderCurrentStandards();
      selectedStandards = [];
      $("#SBselected").html("");
      alert("Successfully updated Standards.");
    }
    else {
      alert(data.status);
    }
  }

  function success_save_news(data) {
    if (data.valid) {
      $("#newsID").html(data.data);
      $(".oae_editor_editpanel iframe").first().contents().find("body").html("");
      alert("Your news item was successfully posted!");
    }
    else {
      alert(data.status);
    }
  }

  function success_loadAttachments(data){
    if(data.valid){
      currentAttachments = data.data;
      renderAttachments();
    }
    else {
      alert(data.status);
    }
  }

  /* ---   ---   ---   Rendering   ---   ---   --- */
  function renderCurrentStandards() {
    var box = $("#currentStandards");
    var template = $("#template_currentStandard").html();
    box.html("");
    for(i in currentStandards){
      var current = currentStandards[i];
      box.append(jsCommon.fillTemplate(template, current));
      box.find(".existingStandard[data-standardid=" + current.id + "] .ddlAlignmentType option[value=" + current.alignmentID + "]").prop("selected", true);
      box.find(".existingStandard[data-standardid=" + current.id + "] .ddlUsageType option[value=" + current.usageID + "]").prop("selected", true);
    }
  }

  function renderAttachments() {
    var box = $("#currentFiles");
    var template = $("#template_attachment").html();
    box.html(currentAttachments.length == 0 ? "<p class='grayMessage'>No Files or References have been added yet.</p>" : "");
    for(i in currentAttachments){
      var current = currentAttachments[i];
      current.featureText = current.featured ? "Featured" : "";
      box.append(jsCommon.fillTemplate(template, current));
      box.find(".attachment[data-attachmentID=" + current.id + "] .ddlAttachmentPermissions option[value=" + current.accessID + "]").prop("selected", true);
    }
  }
</script>
<link rel="stylesheet" href="/styles/common2.css" type="text/css" />
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
  #tabNavigation { padding: 0 0 0 5px; }
  #tabNavigation input { border-radius: 5px 5px 0 0; width: auto; }
  #tabNavigation input.current { background-color: #9984BD; }
  .tab { margin-bottom: 10px; }
  .column { display: inline-block; width: 50%; vertical-align: top; padding: 0 5px; }
  input[type=text], select, textarea { width: 100%; margin: 5px 0; min-height: 25px; }
  iframe.fileIframe { height: 25px; border: none; width: 100%; margin: 5px 0; overflow: hidden; }
  textarea { min-height: 5em; height: 8em; max-height: 20em; resize: vertical; }
  .buttons { padding: 5px; }
  .buttons input { width: auto; }
  .buttons.right { text-align: right; }
  .rblBox { padding-left: 25px; position: relative; }
  .rblBox input[type=radio] { position: absolute; top: 5px; left: 0; margin: 5px; }
  #contributeToolBox { width: 100%; border-radius: 5px; border: 1px solid #CCC; background-color: #FFF; }
  #contributeTool { width: 100%; height: 100%; border: none; border-radius: 5px; }
  .tab h2 { margin-bottom: 5px; }
  .lightBox { border: 1px solid #CCC; border-radius: 5px; padding: 5px; margin-bottom: 10px; }
  .lightBox .header { background-color: #CCC; color: #000; font-size: 16px; margin: -5px -5px 5px -5px; font-weight: normal; }
  .existingStandard { position: relative; min-height: 95px; }
  .existingStandard .data { padding-right: 145px; }
  .existingStandard .data .buttons { position: absolute; right: 5px; width: 150px; padding: 0; }
  .existingStandard .data .buttons input { width: 100%; margin-bottom: 5px; }
  .existingStandard .data .ddls { white-space: nowrap; text-align: center; }
  .existingStandard .data .ddls select { width: 48%; display: inline-block; }
  .attachment .featured { font-weight: bold; float: right; }

  /* Miscellaneous */
  div[class*='at4-'] { display: none; }
  .grayMessage { text-align: center; padding: 10px; font-style: italic; color: #999; }
  .tab > .mid:first-child { margin-top: -5px; }
  .grayBox .mid.columnLeft { margin: -5px -15px 5px -15px; }
  .grayBox .mid.columnRight { margin: -5px -15px 5px -15px; }
  .buttons.attachment .update { display: none; }
  .bottomMargin { margin-bottom: 10px; }
  .listPanel input { margin-right: 5px; }
  #btnShowExpandedInterface { display: none; }
  .hidden { display: none; }

  @media (max-width: 1075px) {
    #tabNavigation { text-align: center; }
    #tabNavigation input { border-radius: 5px; margin: 2px 0.5%; width: 48%; }
    .column { width: 100%; display: block; }
  }
  @media (max-width: 775px) {
    #tabNavigation input { width: 100%; margin: 2px 0; }
  }
  @media (max-width: 675px) {
    #treeBox { width: 100%; float: none; margin-bottom: 10px; }
    #tabNavigation input { border-radius: 5px; margin: 2px 0.5%; width: 48%; }
    #node { margin-left: 0; }
  }
  @media (max-width: 475px) {
    #tabNavigation input { width: 100%; margin: 2px 0; }
  }
  @media (min-width: 1650px) {
    .expanded .hidden { display: block; }
    .expanded #tab_contribute { float: right; width: 49%; clear: right; }
    .expanded .tab:not(#tab_contribute) { margin-right: 50%; }
    .expanded #tabNavigation { display: none; }
    #btnShowExpandedInterface { display: inline; float: right; width: auto; margin: 5px; }
  }
</style>


<div id="content">
  <h1 class="isleH1">IOER Curriculum Editor <input type="button" id="btnShowExpandedInterface" class="isleButton bgGreen" value="Toggle Expanded Interface" onclick="toggleExpandedInterface();" /></h1>
  
  <div id="editor">
    <div id="treeBox" class="grayBox">
      <h2 class="header">Curriculum Navigation</h2>
      <div id="loadingTree" class="grayMessage">Loading...</div>
      <div id="tree"></div>
      <div id="actions">
        <select id="treeActions">
          <option value="addAfterCurrent" selected="selected">Add a node after current node</option>
          <option value="addBelowCurrent">Add a node below current node</option>
          <option value="moveNodeUp">Move node up</option>
          <option value="moveNodeDown">Move node down</option>
          <option value="deleteNode">Delete this node</option>
        </select>
        <input type="button" class="isleButton bgBlue" value="Select" id="btnDoNodeAction" onclick="doNodeAction();" />
      </div>
    </div>
    <div id="node">
      <div id="tabNavigation">
        <input type="button" class="isleButton bgBlue current" value="Node Properties" data-id="properties" onclick="showTab('properties');" />
        <input type="button" class="isleButton bgBlue" value="Learning Standards" data-id="standards" onclick="showTab('standards');" />
        <input type="button" class="isleButton bgBlue" value="Files & References" data-id="attachments" onclick="showTab('attachments');" />
        <input type="button" class="isleButton bgBlue" value="Tag/Upload a Resource" data-id="contribute" onclick="showTab('contribute');" />
        <span id="newsItemsTabContainer" runat="server"><input type="button" class="isleButton bgBlue" value="News Update" data-id="news" onclick="showTab('news');" /></span>
      </div>
      <div id="tabs">
        <!-- Contribute Tool -->
        <div class="tab grayBox hidden" id="tab_contribute">
          <h2 class="header">Tag/Upload a Resource</h2>
          <div id="contributeToolBox">
            <iframe id="contributeTool" src="/Contribute/?mode=tag&hidechrome=1&contentId=<%=CurrentRecord.Id %>&nodeId=<%=CurrentRecord.Id %>"></iframe>
          </div>
        </div>
        <!-- Node Properties -->
        <div class="tab grayBox" id="tab_properties">
          <h2 class="header">Node Properties</h2>
          <div class="column">
            <input type="text" class="txtTitle" id="txtTitle" placeholder="Title" runat="server" />
            <div class="validation" data-control="txtTitle"></div>
            <input type="text" class="txtTimeframe" id="txtTimeframe" placeholder="Approximate timeframe to complete" runat="server" />
            <div class="validation" data-control="txtTimeframe"></div>
            <asp:DropDownList ID="ddlNodePermissions" CssClass="ddlNodePermissions" runat="server"></asp:DropDownList>
            <% if( CurrentRecord.Id == curriculumID ) { %>
            <label>Custom Curriculum Image</label>
            <iframe id="curriculumImage" class="fileIframe" src="/controls/curriculum/curriculumfileupload.aspx?usage=curriculumimage"></iframe>
            <% } %>
          </div><div class="column">
            <textarea class="txtDescription" id="txtDescription" placeholder="Description" runat="server"></textarea>
          </div>
          <% if( CurrentRecord.Id == curriculumID ){ %>
          <div id="topLevelOptions">
            <p class="grayMessage">Selections below will apply to <b><u>all</u></b> items that are a part of this Curriculum.</p>
            <div class="column bottomMargin k12subject">
              <h3 class="mid columnLeft">K-12 Subjects</h3>
              <custom:CheckBoxList ID="cbxlK12Subject" runat="server" TargetTable="subject" UpdateMode="raw" />
            </div><div class="column gradelevel">
              <h3 class="mid columnRight">Grade Level</h3>
              <custom:CheckBoxList ID="cbxlGradeLevel" runat="server" TargetTable="gradeLevel" UpdateMode="raw" />
            </div>
          </div>
          <% } %>
          <div class="buttons right">
            <input type="button" class="isleButton bgBlue" id="btnSaveProperties" value="Save Changes" onclick="save_properties();" />
          </div>
        </div>
        <!-- Learning Standards -->
        <div class="tab grayBox hidden" id="tab_standards">
          <h2 class="header">Learning Standards</h2>
          <h2 class="mid" style="margin-top:-5px;">Current Learning Standards</h2>
          <div id="currentStandards">
            <% if(currentStandards.Count() == 0){  %>
            <p class="grayMessage">No Learning Standards have been added yet.</p>
            <% } else { %>
              <% foreach(var item in currentStandards){ %>
            <div class="existingStandard lightBox" data-standardID="<%=item.standardID %>">
              <h3 class="header"><%=item.code %></h3>
              <div class="data">
                <div class="buttons">
                  <input type="button" class="isleButton bgBlue" value="Save Changes" onclick="standard_update(<%=item.recordID %>);" />
                  <input type="button" class="isleButton bgRed" value="Remove Standard" onclick="standard_delete(<%=item.recordID %>);" />
                </div>
                <p><%=item.text %></p>
                <div class="ddls">
                  <select class="ddlAlignmentType">
                    <option value="0" <%=item.alignmentID == 0 ? "selected='selected'" : "" %>>Node aligns to this standard</option>
                    <option value="1" <%=item.alignmentID == 1 ? "selected='selected'" : "" %>>Node assesses this standard</option>
                    <option value="2" <%=item.alignmentID == 2 ? "selected='selected'" : "" %>>Node teaches this standard</option>
                    <option value="3" <%=item.alignmentID == 3 ? "selected='selected'" : "" %>>Node requires this standard</option>
                  </select>
                  <select class="ddlUsageType">
                    <option value="1" <%=item.alignmentID == 1 ? "selected='selected'" : "" %>>Major alignment</option>
                    <option value="2" <%=item.alignmentID == 2 ? "selected='selected'" : "" %>>Supporting alignment</option>
                    <option value="3" <%=item.alignmentID == 3 ? "selected='selected'" : "" %>>Additional alignment</option>
                  </select>
                </div>
              </div>
            </div>
              <% } %>
            <% } %>
          </div>
          <h2 class="mid">Add Learning Standards</h2>
          <uc1:StandardsBrowser ID="StandardsBrowser" runat="server" />
          <input type="hidden" id="hdn_standards" class="hdn_standards" runat="server" />
          <div class="buttons right">
            <input type="button" class="isleButton bgBlue" value="Add These Standards" id="btnSaveStandards" onclick="standards_add();" />
          </div>
        </div>
        <!-- Files and References -->
        <div class="tab grayBox hidden" id="tab_attachments">
          <h2 class="header">Files &amp; References</h2>
          <div class="column">
            <h2 class="mid columnLeft">Current Files &amp; References</h2>
            <div id="currentFiles">
              <p class="grayMessage">No Files or References have been added yet.</p>
            </div>
            <!--<div class="buttons attachment">
              <input type="button" class="isleButton bgBlue" id="btn_attachmentDelete" onclick="attachment_delete();" value="Delete" />
              <input type="button" class="isleButton bgBlue" id="btn_attachmentRename" onclick="attachment_rename();" value="Rename" />
              <input type="button" class="isleButton bgBlue" id="btn_attachmentFeature" onclick="attachment_feature();" value="Make Featured" />
            </div>-->
          </div><div class="column">
            <h2 class="mid columnRight">Attach a File or Reference</h2>
            <input type="text" class="txtAttachmentTitle" id="txtAttachmentTitle" placeholder="Title" runat="server" />
            <div class="validation" data-control="txtAttachmentTitle"></div>
            <div class="rblBox">
              <input type="radio" name="rblAttachment" id="rblAttachmentFile" value="attachmentFile" checked="checked" />
              <iframe id="fileAttachmentFile" class="fileIframe" src="/controls/curriculum/curriculumfileupload.aspx?usage=attachment"></iframe>
            </div>
            <div class="rblBox">
              <input type="radio" name="rblAttachment" id="rblAttachmentUrl" value="attachmentUrl" />
              <input type="text" class="txtAttachmentUrl" id="txtAttachmentUrl" runat="server" placeholder="Webpage URL" />
            </div>
            <div class="validation" data-control="txtAttachmentUrl"></div>
            <asp:DropDownList ID="ddlAttachmentPermissions" CssClass="ddlMainAttachmentPermissions" runat="server"></asp:DropDownList>
            <div class="buttons right">
              <input type="button" class="isleButton bgBlue" id="btn_attachmentSave" onclick="attachment_save();" value="Save Attachment" />
            </div>
          </div>
        </div>
        <div id="newsItemsContainer" runat="server">
          <!-- News Items -->
          <div class="tab grayBox hidden" id="tab_news">
            <h2 class="header">News Update</h2>
            <p>You may alert followers of updates to this Curriculum when you have made a major change:</p>
            <p>Current News ID: <asp:label id="newsId" CssClass="newsID" runat="server">0</asp:label></p>
            <style type="text/css">
              .oae_editor_default .oae_editor_container { border: none; }
              .oae_editor_base .oae_editor_container, .oae_editor_default .oae_editor_toptoolbar, .oae_editor_default .oae_editor_bottomtoolbar { background-color: transparent; }
              .oae_editor_toptoolbar > div { padding-bottom: 4px; }
              .oae_editor_default .oae_editor_editpanel_container { border: 1px solid #CCC; overflow: hidden; border-radius: 5px; }
              .oae_editor_bottomtoolbar > div { padding-top: 2px; }
              div.oae_contextmenu { background-color: #F5F5F5; border-radius: 5px; overflow: hidden; border-color: #CCC; }
              .oae_contextmenu .oae_contextmenu_item th, .oae_contextmenu .oae_contextmenu_item td { background-image: linear-gradient(#EEF, transparent); }
              .oae_contextmenu .oae_contextmenu_item th img { background-color: #CCC; }
              .oae_contextmenu .oae_contextmenu_item_hover th, .oae_contextmenu .oae_contextmenu_item_hover td { background-image: none; background-color: #FF6A00; }
              .oae_editor_default .oae_editor_toptoolbar .oae_toolbar_button { margin-bottom: 4px; }
            </style>
            <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
            <obout:Editor ID="newsEditor" runat="server" Height="400px" Width="100%">
              <TopToolbar Appearance="Custom">
                <AddButtons>
                  <obout:Undo /><obout:Redo /><obout:HorizontalSeparator />
                  <obout:Bold /><obout:Italic /><obout:Underline /><obout:StrikeThrough /><obout:HorizontalSeparator />
                  <obout:ForeColorGroup /><obout:BackColorGroup /><obout:RemoveStyles /><obout:HorizontalSeparator />
                  <obout:Cut /><obout:Copy /><obout:Paste /><obout:PasteText /><obout:PasteWord /><obout:HorizontalSeparator />
                  <obout:DecreaseIndent /><obout:IncreaseIndent /><obout:HorizontalSeparator />
                  <obout:Paragraph /><obout:JustifyLeft /><obout:JustifyCenter /><obout:JustifyRight /><obout:JustifyFull /><obout:RemoveAlignment /><obout:HorizontalSeparator />
                  <obout:OrderedList /><obout:BulletedList /><obout:HorizontalSeparator />
                  <obout:InsertLink /><obout:RemoveLink /><obout:InsertAnchor /><obout:AnchorsToggle /><obout:InsertHR /><obout:InsertSpecialCharacter /><obout:HorizontalSeparator />
                  <obout:InsertTable /><obout:InsertDiv /><obout:InsertImage /><obout:HorizontalSeparator />
                  <obout:SpellCheck />
                  <obout:VerticalSeparator />
                  <obout:FontName /><obout:HorizontalSeparator />
                  <obout:FontSize /><obout:HorizontalSeparator />
                  <obout:Header /><obout:HorizontalSeparator />
                  <obout:Print />
                </AddButtons>
              </TopToolbar>
              <EditPanel ID="newsEditPanel" FullHtml="false" runat="server"></EditPanel>
              <BottomToolbar ShowDesignButton="true" ShowHtmlTextButton="true" ShowPreviewButton="true" ShowFullScreenButton="true"></BottomToolbar>
            </obout:Editor>
            <div class="buttons right">
              <input type="button" class="isleButton bgBlue" id="btnSaveNews" onclick="save_news()" value="Post News" />
            </div>
          </div>
        </div>

      </div><!-- /tabs -->
    </div>
  </div>

</div>

<div id="templates" style="display:none;">
  <div id="template_currentStandard">
    <div class="existingStandard lightBox" data-standardID="{id}" data-contentStandardID="{contentStandardID}">
      <h3 class="header">{code}</h3>
      <div class="data">
        <div class="buttons">
          <input type="button" class="isleButton bgBlue" value="Save Changes" onclick="standard_update({id}, this);" />
          <input type="button" class="isleButton bgRed" value="Remove Standard" onclick="standard_delete({contentStandardID}, this);" />
        </div>
        <p>{text}</p>
        <div class="ddls">
          <select class="ddlAlignmentType">
            <option value="0">Node aligns to this standard</option>
            <option value="1">Node assesses this standard</option>
            <option value="2">Node teaches this standard</option>
            <option value="3">Node requires this standard</option>
          </select>
          <select class="ddlUsageType">
            <option value="1">Major alignment</option>
            <option value="2">Supporting alignment</option>
            <option value="3">Additional alignment</option>
          </select>
        </div>
      </div>
    </div>
  </div>
  <div id="template_attachment">
    <div class="attachment lightBox" data-attachmentID="{id}">
      <div class="header">{title} <span class="featured">{featureText}</span></div>
      <a href="{url}" target="_blank">{url}</a>
      <asp:DropDownList ID="ddlAttachmentTemplatePermissions" CssClass="ddlAttachmentPermissions" runat="server"></asp:DropDownList>
      <div class="buttons attachment right">
        <input type="button" class="isleButton bgBlue update" onclick="attachment_update({id}, this);" value="Save" />
        <input type="button" class="isleButton bgBlue" onclick="attachment_rename({id}, this);" value="Rename" />
        <input type="button" class="isleButton bgBlue feature" onclick="attachment_feature({id}, this);" value="Feature" />
        <input type="button" class="isleButton bgRed" onclick="attachment_delete({id}, this);" value="Delete" />
      </div>
    </div>
  </div>
</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
  <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false"></asp:Literal>
</asp:Panel>