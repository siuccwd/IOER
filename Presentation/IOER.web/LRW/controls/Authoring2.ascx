<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Authoring2.ascx.cs" Inherits="ILPathways.LRW.controls.Authoring2" %>

<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor" %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.ToolbarButton" %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.ContextMenu"  %>

<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.Popups" %>
<%@ Register TagPrefix="custom"	Namespace="CustomToolbarButton" Assembly="IllinoisPathways"  %>
<%@ Register TagPrefix="custom" Namespace="CustomPopups"  %>

<%@ Register TagPrefix="uc1" TagName="ConditionsOfUseSelector" Src="/LRW/controls/ConditionsOfUseSelector.ascx" %>
	<%--<obout:EditorPopupHolder runat="server" ID="popupHolder" />
  <obout:PopupHolder runat="server" id="popupHolder1" DefaultAddPolicy="Demand" ChangingOnLoad="true">
   <Demand>
       <obout:ImageBrowser runat="server" ID="ib1" />
   </Demand>
</obout:PopupHolder>--%>

<script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />

<script type="text/javascript" language="javascript">
  var supplementID = 0;
  var supplementEditor;
  var supplementContent;

  $(document).ready(function () {

    var preselectedTab = "<%=selectedTab %>";
    if (preselectedTab !== "") {
      showTab(preselectedTab);
    }

    var params = window.location.search.split(/\?|&/);
    for (var i = 0; i < params.length; i++) {
      if (params[i] != "") {
        var param = params[i].split("=");
        if (param[0].toLowerCase() == "tabid") {
          showTab(param[1]);
        }
      }
    }

    //ensure that a tab is showing
    var needFix = true;
    $(".tabBox").each(function () {
      if ($(this).hasClass("selected")) {
        needFix = false;
      }
    });
    if (needFix) {
      showTab("basicInfo");
    }

    //Track textbox lengths
    $(".textTracker").on("keyup change", function() {
      updateTextLengths($(this), $("#" + $(this).attr("trackerName")));
    });
    $(".textTracker").change();

    //Fix display of navtabs
    var tabCount = $(".tabNav a.tab").length;
    var tabCountStatic = 10 + tabCount + 1;
    $(".tabNav a.tab").each(function() {
      $(this).css("z-index", tabCountStatic + tabCount);
      tabCount--;
    });

  }); //End document.ready

  //switch tabs
  function showTab(id) {
    var targetTabNav = $(".tabNav #tabNav_" + id);
    var targetTab = $("#tab_" + id);

    $(".tabNav li a").removeClass("selected");
    $(".tabBox").removeClass("selected");

    targetTabNav.addClass("selected");
    targetTab.addClass("selected");
  }

  //Select attachments
  function editAttachment(attachmentValue){
    var currentIndex = 0;
    $("#<%=ddlAttachment.ClientID %> option").each(function() {
      if($(this).prop("value") == attachmentValue){
        $("#<%=ddlAttachment.ClientID %>").prop("selectedIndex", currentIndex).change();
      }
      currentIndex++;
    });
  }

  //Select references
  function editReference(referenceValue){
    var currentIndex = 0;
    $("#<%=ddlReference.ClientID %> option").each(function() {
      if($(this).prop("value") == referenceValue){
        $("#<%=ddlReference.ClientID %>").prop("selectedIndex", currentIndex).change();
      }
      currentIndex++;
    });
  }

  function doPreview() {
    window.location = $(".tab.last").prop("href");
  }

  //Communicate with the web service
  function queryServer(targetServerMethod, targetData, targetJFunction, targetHTMLContainer, isAsync) {
    $.ajax({
      type: "POST",
      contentType: "application/json; charset=utf-8",
      url: "/Services/WebDALService.asmx/" + targetServerMethod,
      data: targetData == "" ? {} : targetData,
      dataType: "json",
      async: isAsync,
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
  

  function populateUsageRights(dataRaw) {
    data = jQuery.parseJSON(dataRaw);
    for (var i = 0; i < data.values.length; i++) {
      $(".ddlConditionsOfUse").append("<option value=\"" + data.values[i] + "\">" + data.texts[i] + "</option>");
      conditionsOfUse_Titles.push(data.descriptions[i]);
      conditionsOfUse_URLs.push(data.urls[i]);
      conditionsOfUse_ImageURLs.push(data.imageURLs[i]);
    }
  }


  function updateTextLengths(jqTextBox, jqTracker) {
    var minLength = parseInt(jqTracker.attr("minLength"));
    var currentLength = jqTextBox.val().trim().length;
    if(currentLength < minLength){
      jqTracker.html(        "requires at least " + (minLength - currentLength) + " more characters"      );
    }
    else {
      jqTracker.html("Minimum length requirement met.");
    }
  }
function Insert()
{
alert("Insert");
 oboutGetEditor('editor').InsertHTML("<a href='mailto://support@obout.com'>Obout support</a>");
}
</script>

<script type="text/javascript">
<!--
  function confirmDelete(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to delete a record
    // Note - this could be made generic if the url is passed as well

    var bresult
    bresult = confirm("Are you sure you want to delete this content?\n\n"
            + "All related content, including Learning Registry data will also be removed?\n\n"
            + "Click OK to delete this record or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }


  } //  


  function confirmPublish(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to publish a record

    var bresult
    bresult = confirm("TAG THIS RESOURCE\n\nAre you sure you want to Tag this content?\n\n"
            + "Clicking OK will open the tagging page where you can add useful tags, align to standards, etc.\n\n"
            + "Click OK to Tag this record or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }


  } //  

  function confirmSubmit(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to submit a record

    var bresult
    bresult = confirm("SUBMIT THIS RESOURCE\n\nAre you sure you want to Submit this content?\n\n"
            + "Clicking OK will open the publishing page where you can add useful tags, align to standards, etc.\n\n"
            + "Click OK to Publish this record or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }
  } //  

  function confirmRePublish(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to republish a record

    var bresult
    bresult = confirm("Re-Publish This Resource\n\nAre you sure you want to Publish this content?\n\n"
            + "This content was previously published (and tagged). Clicking OK will set the status to published, making the content available to the intended audience.\n\n"
            + "Click OK to Publish this record or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }
  } //  

  function confirmUnPublish(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to unpublish a record

    var bresult
    bresult = confirm("Are you sure you want to unpublish this content?\n\n"
            + "This content will no longer be available to the public, unless it is re-published.\n\n"
            + "Click OK to unpublish this record or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }


  } //  

  function confirmSetInactive(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to set record inactive

    var bresult
    bresult = confirm("Are you sure you want to set this content to be inactive?\n\n"
            + "This content will no longer be available to the public, \nit will be removed from the Learning Registry.\n\nIf you want to reactivate it in the future, it will have to go through a new publishing workflow.\n\n"
            + "Click OK to set this record to Inactive, or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      return true;
    } else {
      return false;
    }


  } //  

  function removeAttachment(id) {

    var bresult
    bresult = confirm("Are you sure you want to remove this attachment?\n\n"
				+ "Click OK to remove this attachment or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      //alert("delete requested for id " + id + "\nlocation = " + self.location);

      __doPostBack('btnRemAtt', id);

      return true;
    } else {
      return false;
    }
  }
  function removeReference(id) {

    var bresult
    bresult = confirm("Are you sure you want to remove this reference?\n\n"
				+ "Click OK to remove this reference or click Cancel.");
    var loc;

    loc = self.location;

    if (bresult) {
      //alert("delete requested for id " + id + "\nlocation = " + self.location);

      __doPostBack('btnRemRef', id);

      return true;
    } else {
      return false;
    }
  }
    //-->
</script>
<style type="text/css">
  .tabPanelStyle {min-height: 450px; }
  .ajax__tab_tab { color: Black;}
  .ajax__tab_xp {visibility:visible; }
  .leftCol { width: 480px;}
  .textBox { width: 100%;}
</style>
<style type="text/css">
  .column {
    display: inline-block;
    *display: inline;
    zoom: 1;
    vertical-align: top;
    width: 480px;
    margin: 10px;
  }
  .column.right {
    min-height: 525px;
  }
  .column input[type=text], .column textarea, .column select, .txtBrowseBox, table input[type=text], table select, table textarea {
    border-radius: 5px;
    border: 1px solid #AAA;
    width: 470px; 
    padding: 2px;
  }
  .column textarea {
    max-width: 470px;
    min-width: 470px;
    height: 150px;
    min-height: 150px;
    max-height: 150px;
  }
   .column textarea shortTextArea {
    height: 150px;
  }
  .metadata #useRightsTable .label {
    width: 150px;
    text-align: right;
    vertical-align: top;
    padding-right: 10px;
  }
  .metadata #useRightsTable input, .metadata #useRightsTable select {
    width: 428px; 
  }
  .metadata #useRightsTable select {
    width: 374px;
  }
  .metadata #useRightsTable .txtConditionsOfUse {
    width: 468px; 
  }
  .metadata #useRightsTable .ccRightsImage {
    vertical-align: middle; 
  }
  .metadata .supplementButton {
    width: 100px; 
  }
  .rightAlign {
    text-align: right; 
  }
  .rblPostOptions {
    list-style-type: none; 
    margin: 10px;
  }
  .rblPostOptions li {
    margin-left: 0; 
  }
  .rblPostOptions li input {
    margin: 3px; 
  }
  .postBox {
    text-align: right;
  }
  .postBox .postButton {
    text-align: center;
    width: 200px;
    padding: 2px 10px; 
  }
  .cbxTermsAgreement input {
    margin: 3px; 
  }
  .isleH3_Block {
    margin-top: 15px;
  }
  select.ddl {
    width: 475px;
  }
  .buttons {
    text-align: center;
    padding: 10px;
  }
  table {
    width: 100%;
  }
  table td.label {
    text-align: right;
    font-weight: bold;
    padding-right: 10px;
    width: 300px; 
    font-size: 16px;
    line-height: 23px;
    color: #4F4E4F;
  }
  table td input[type=text], table td textarea.textBox {
    width: 500px;
  }
  table td textarea.textBox {
    height: 60px;
    max-width: 500px;
    max-height: 60px;
    min-height: 60px;
  }
  table td input[type=file] {
    width: 350px;
  }
  table td input[type=submit] {
    width: 155px;
  }
  table td select, table td select.ddl {
    width: 505px;
  }
  .tabBox {
    display: none;
  }
  .tabBox.selected {
    display: block;
  }
  .isleTabs {
    width: 1000px;
  }
  .isleTabs li a { width: 155px }
  
  .lengthTracker {
    float: right;
    font-weight: normal;
    color: #999; 
  }
  .horizontalList li {
    display: inline-block;
    *display: inline;
    zoom: 1;
    width: 300px;
    vertical-align: top;
  }
  h3.isleH3_Block a, h4.isleH4_Block a {
    border: none;
    float:right;
    font-size: 16px;
    font-weight: normal;
    margin-top: -2px;
  }
  h3.requiredX {
  border-color: #D55;
  color: #600;
}
  .required {
    border-left: 10px solid #B03D25;
    border-radius: 5px;
  }
 .status { 
  border-radius: 5px;
  padding: 0 5px; font-weight: bold;
  margin-left: 2px;
   margin-top: 0px;
    border-left: 10px solid #4F4F4F; border-right: 10px solid #4F4F4F;
    border-top: 2px solid #4F4F4F;   border-bottom: 2px solid #4F4F4F;
  }  
  .inprogressStatus { background-color:#f5f5f5; color: #000;  padding: 0 25px; }
  .submittedStatus {  background-color:YELLOW; color: #000; padding: 0 25px; }
  .publishedStatus { background-color:Lime; color: #000; padding: 0 25px; }
  .declinedStatus { background-color:red; color: #fff; padding: 0 25px; }
  .requiresrevisionStatus { background-color:red; color: #fff; padding: 0 25px; width: 100% }
  .inactiveStatus { background-color:black; color: red; padding: 0 25px; }
  
  .attachmentTitle {
    width: 275px;
    display: inline-block;
    *display: inline;
    zoom: 1;
    vertical-align: top;
  }
  .defaultButton {
    width: 200px;
  }
    h3.isleH3_Block a.toolTipLink, h4.isleH4_Block a.toolTipLink {
      float: none;
      margin-bottom: -2px;
    }
  .conditionsSelector .ddlConditionsOfUse { width: 100%; }
  .conditionsSelector input.txtConditionsOfUse { width: 100%; }
  table.conditionsSelector { width: 475px; }

  /* tab chevrons */
  .tabNav li a.tab {
     background: transparent url('/images/navchevron.png') no-repeat right top;
     border: none;
     width: 220px;
     margin-right: -25px;
     position:relative;
     border-radius: 10px;
  }
  .tabNav li a.tab:hover, .tabNav li a.tab:focus {
    background-color: transparent;
    background-position: -280px -32px;
  }
  .tabNav li a.tab.selected, .tabNav li a.tab.selected.last {
    background-color: transparent;
    background-position: -280px -64px;
  }
  .tabNav li a.tab.selected.last {
    background-position: 0 -64px;
  }
  .tabNav li a.tab.last {
     background-position: 0px -0px;
     width: 210px;
  }
  .tabNav li a.tab.last:hover, .tabNav li a.tab.last:focus {
    background-position: 0px -32px;
  }
  .tabNav li a.tab.preview {
    width: 200px;
    margin-left: 30px;
    background-color: #4C98CC;
    background-image: none;
  }
  .tabNav li a.tab.preview:hover, .tabNav li a.tab.preview:focus {
    background-color: #FF5707;
  }
  .publishButton {
  padding: 5px;
  background-color: #FF5707;
  cursor: pointer;
}
</style>

<h1 class="isleH1">ISLE Resource Authoring Tool</h1>
<asp:panel ID="containerPanel" runat="server">
<div id="statusDiv" style="float:right;" runat="server" visible="false">
<asp:Button ID="btnPublish2" runat="server" Visible="false" CssClass="defaultButton publishButton" OnClick="btnPublish2_Click"  Text="Publish" CausesValidation="false"></asp:Button>
<asp:button id="btnPublish" runat="server" Visible="false" CssClass="defaultButton publishButton" OnCommand="btnPublish_Click" CommandName="PublishUpdate" Text="Publish" causesvalidation="false"></asp:button>

      <span style="float: right;">&nbsp; <a class="toolTipLink" id="tipStatus" title="Status|Current status of this resource:<ul><li>In Progess: content is under construction, the content is not public.</li><li>Submitted: Content authored on behalf of an organization requires approval. Status of Submitted indicates approval is pending.</li><li>Requires Revision: For content submitted for approval, a status of Requires Revision indicates changes have been requested before content will be approved.</li><li>Published: the content is now visible to the public (dependent on the privacy settings)</li><li>Inactive: author has set content to inactive. An inactive item will no longer be available to the searches, libraries, etc.</li></ul>"><img src="/images/icons/infoBubble.gif" alt="" /> </a> </span>

 <asp:Label ID="lblStatus" runat="server" >Draft</asp:Label>
  
</div>
<div class="clearFloat"></div>
<ul class="tabNav isleTabs" id="tabNav" runat="server" visible="false">
  <li><a href="javascript:showTab('basicInfo')" class="tab selected" id="tabNav_basicInfo">1: Basic Info</a></li>
  <li><a href="javascript:showTab('webcontent')" class="tab" id="tabNav_webcontent">2: Web Content</a></li>
  <li><a href="javascript:showTab('attachments')" class="tab" id="tabNav_attachments">3: Attachments</a></li>
  <li><a href="javascript:showTab('references')" class="tab last" id="tabNav_references">4: Resources</a></li>
  <li><a href="/CONTENT/<%=previewID%>/<%=cleanTitle%>" class="tab preview" target="resPrevw">Preview Any Time</a></li>
</ul>

  <%--<li><a href="/Repository/ResourcePage.aspx?rid=<%=previewID %>" class="tab preview offScreen" target="resPrevw">Preview Any Time</a></li>
--%>
<div id="Stage1Items" runat="server">

<div class="tabBox selected" id="tab_basicInfo">
  <h2 class="isleH2">Basic Content Information</h2>
  <p class="required">Required fields are marked with a red border.</p>
  <asp:Panel ID="keysPanel" runat="server" Visible="false">
    <asp:Label ID="txtId" runat="server" >0</asp:Label>
    <br />
    <asp:Label ID="txtRowId" runat="server" ></asp:Label>
  </asp:Panel>
	<div class="tabPanelStyle metadata" >
    <div  class="column" >
    <asp:Panel ID="contentTypePanel" runat="server"  Visible="true">
      <h3 class="isleH3_Block required">Content Type<a class="toolTipLink" id="contentTipLink" title="Content Type|Select the content type. Select template if you want to create an item that others in your organization can use as a building block for new content."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlContentType" runat="server" CssClass="ddl" ></asp:DropDownList><asp:Label ID="lblContentType" Visible="false" runat="server" ></asp:Label>
      </asp:Panel>
      
      <asp:Panel ID="templatesPanel" runat="server"  Visible="false">
      <h3 class="isleH3_Block">Do you want to start from a template<a class="toolTipLink" id="tipTemplate" title="Authoring Template|Where available, select a template as a starting point for new content."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlTemplates" runat="server" CssClass="ddl" ></asp:DropDownList>
      </asp:Panel>

      <h3 class="isleH3_Block required">Title<a class="toolTipLink" id="tipTitle" title="Title|The name or title of the resource"><img src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_Title" minLength="10"></span></h3>
      <asp:TextBox ID="txtTitle" class="textBox textTracker" trackerName="lt_Title" runat="server" />

      <h3 class="isleH3_Block required">Description<a class="toolTipLink" id="tipDescription" title="Description|A brief description of the resource. This field will be used in the Learning Registry metadata if you choose to Publish it."><img src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_Description" minLength="25"></span></h3>
      <asp:TextBox ID="txtDescription" class="textBox textTracker" trackerName="lt_Description" Rows="5" TextMode="MultiLine"  runat="server" />
    

      <h3 class="isleH3_Block required">Select Usage Rights<a class="toolTipLink" id="tipUsageRights" title="Usage Rights|The URL where the owner specifies permissions for using the resource.|<b>Remix and Share:</b> You may adapt, edit, or tweak resource before using and sharing.|<b>Share Only:</b> You may copy, distribute or transmit the resource in its original form only.|<b>No Strings Attached:</b> No restrictions are placed on usage.|<b>Read the Fine Print:</b> Specific restrictions may be in place; read usage rights carefully.|<b>Attribution:</b> You must attribute the work in the manner specified by the author or licensor."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <uc1:ConditionsOfUseSelector ID="conditionsSelector" runat="server" />

      <h3 class="isleH3_Block required">Who can access this resource?<a class="toolTipLink" id="tipPrivilege" title="Access Privilege|You can allow only specific groups of users to access this resource by selecting the appropriate group from the box below."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlPrivacyLevel" CssClass="ddl" runat="server">
        <asp:ListItem Value="1" Selected="True">Anyone can access, including students</asp:ListItem>
        <asp:ListItem Value="2">Only authenticated users</asp:ListItem>
        <asp:ListItem value="3">Only education staff at my school</asp:ListItem>
        <asp:ListItem value="4">Only education staff at schools in Illinois</asp:ListItem>
      </asp:DropDownList>

      <h3 class="isleH3_Block required">This resource is being Authored on behalf of...<a class="toolTipLink" id="tipOrganization" title="Authoring|If you are authoring this resource for your own purposes, select 'Myself'. Otherwise, select the appropriate organization from the box below."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlIsOrganizationContent" runat="server" CssClass="ddl" >
        <asp:ListItem Value="1" Text="Myself"></asp:ListItem>
        <asp:ListItem Value="2" Text="My Organization"></asp:ListItem>
      </asp:DropDownList>

      <div class="buttons">
        <asp:Button ID="btnSave" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSave_Click" Text="Save and Continue" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnCancel" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnCancel_Click"  Text="Cancel ????" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnDelete" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnDelete_Click"  Text="Delete" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnNew" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnNew_Click"  Text="New" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnFinish" runat="server" Visible="false" CssClass="defaultButton btnFinish" OnClick="btnFinish_Click" Text="Finish" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnUnPublish" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnUnPublish_Click" Text="Un-Publish" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnSetInactive" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnSetInactive_Click" Text="Set Inactive" CausesValidation="false"></asp:Button>
      </div>
      <!-- -->
      <asp:Panel ID="historyPanel" runat="server" Visible="false">
		      <div class="clearFloat"></div>	
		      <div class="labelColumn">
		      <asp:label id="Label12" runat="server">History</asp:label>
		      </div>
		      <div class="dataColumn">
			      <asp:label id="lblHistory" runat="server"></asp:label>
		      </div>
      </asp:Panel>
		</div>
    <div class="column right isleBox">
      <h2 class="isleBox_H2">Guidance</h2>
      <h4 class="isleBox_H4_Block">What is This?</h4>
      <p>Use the ISLE Resource Authoring Tool to create an education or career resource web page. You can save the page in your Library and share it with others. It can be found by anyone searching the ISLE OER (unless you indicate otherwise), but only you can edit what you create here.</p>
      <h4 class="isleBox_H4_Block">How do I use this Tool?</h4>
      <p>To use this tool, you'll need a few minutes and the education or career resource you've created in its original format. For example, you may have created a lesson plan or activity using a Word document or PDF.</p>
      <p>You'll have the option to create a web page, upload the file(s) you created, and fill in references. Examples of references include texts to read or videos to watch to complete the lesson or activity.</p>
      <p>Lessons and activities are just examples. Click here to see more.</p>
      <h4 class="isleBox_H4_Block">Getting Started</h4>
      <ol>
        <li>Enter a Title for your resource. Be sure it describes your resource in a way that others will understand.</li>
        <li>Describe your resource. Type in a paragraph or two that tells others about it.</li>
        <li>Select the Usage Rights that best apply to your resource.</li>
        <li>Decide who can access your resource. Selecting anything other than "Anyone" will require users to be logged in and members of the appropriate group in order to access the content.</li>
        <li>If you are authoring resources on your own, select "Myself" in the last box. Otherwise, choose an organization you belong to.</li>
      </ol>

      <h4 class="isleBox_H4_Block">Supported files:</h4>
      <ul>
        <li><b>MS Office</b> (.doc/docx, .ppt/pptx, .xls/xlsx, etc.)</li>
        <li><b>Documents</b> (.pdf, .txt, .rtf, etc.)</li>
        <li><b>Images</b> (.jpg, .png, .gif, etc.)</li>
        <li><b>Audio</b> (.wav, .mp3, .ogg, etc.)</li>
        <li><b>Archives</b> (.zip, .rar, .7z, etc.)</li>
        <li><b>Smart Board</b>(.xbk, .notebook)</li>
      </ul>
        
    </div>
  </div>
</div>

</div>

<div id="Stage2Items" runat="server" visible="false"><%-- To be used to hide the rest of the form until the first section is complete --%>

<div class="tabBox" id="tab_webcontent">
  <h2 class="isleH2">Web Page Content</h2>
 <div class="column" style="width:750px; margin: 5px auto;">
 
    <obout:Editor ID="editor" runat="server" Height="400px" Width="100%">
      <TopToolbar Appearance="Custom" >
        <AddButtons>
          <obout:Undo /><obout:Redo />
          <obout:HorizontalSeparator />
          <obout:Bold /><obout:Italic /><obout:Underline /><obout:StrikeThrough />
          <obout:HorizontalSeparator />
          <obout:ForeColorGroup /><obout:BackColorGroup /><obout:RemoveStyles />
          <obout:HorizontalSeparator />
          <obout:Cut /><obout:Copy /><obout:Paste /><obout:PasteText /><obout:PasteWord />
          <obout:HorizontalSeparator />
          <obout:DecreaseIndent /><obout:IncreaseIndent />
          <obout:HorizontalSeparator />
          <obout:Paragraph /><obout:JustifyLeft /><obout:JustifyCenter /><obout:JustifyRight /><obout:JustifyFull /><obout:RemoveAlignment />
          <obout:HorizontalSeparator />
          <obout:OrderedList /><obout:BulletedList />
          <obout:HorizontalSeparator />
          <obout:InsertLink /><obout:RemoveLink /><obout:InsertAnchor /><obout:AnchorsToggle /><obout:InsertHR /><obout:InsertSpecialCharacter />
          <obout:HorizontalSeparator />
          <obout:InsertTable /><obout:InsertDiv /> <obout:InsertImage />
          <obout:HorizontalSeparator />
          <obout:SpellCheck />
          <obout:HorizontalSeparator />
					<custom:ImmediateImageInsert runat="server" ID="myImmediateImageInsert" />
          <obout:VerticalSeparator />
          <obout:FontName /><obout:HorizontalSeparator /><obout:FontSize /><obout:HorizontalSeparator /><obout:Header />
          <obout:HorizontalSeparator/>
          <obout:Print/>
        </AddButtons>
      </TopToolbar>
      <EditPanel ID="EditPanel1" FullHtml="false"  runat="server"></EditPanel>
      <BottomToolBar ShowDesignButton="true" ShowHtmlTextButton="true" ShowPreviewButton="true" ShowFullScreenButton="true" ></BottomToolBar>
    </obout:Editor>
  </div>
  <ul class="isleBox column right" style="width:200px;min-height:390px;">
    <h2 class="isleBox_H2">Instructions</h2>
    <li>1. Use the text editor to type or copy and paste from another document. Format the text with paragraphs, bullets, tables, and more--just like any other word processor.</li>
    <li>2. The web page will be visible to the public, so you can use it to show your resource, add instructions, or just expand on the description.</li>
    <li>3. Click the "Save and Continue" button.</li>
  </ul>
  <div class="buttons">
    <asp:Button ID="btnSave2" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSaveWeb_Click" Text="Save" CausesValidation="false"></asp:Button>
    <asp:Button ID="btnSaveWebContinue" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSaveWebContinue_Click" Text="Save and Go to Next Step" CausesValidation="false"></asp:Button>

    <asp:Button ID="btnFinish2" runat="server" Visible="false" CssClass="defaultButton btnFinish" OnClick="btnFinish_Click" Text="Finish" CausesValidation="false"></asp:Button>
  </div>
</div>

<div class="tabBox" id="tab_attachments">
  <h2 class="isleH2">Supplementary Attachments</h2>
  <p>You may add one or more supplements/attachments to associate with the resource you are creating.</p>

  <div class="column">
    <asp:Panel ID="docKeyPanel" runat="server" Visible="false" >
    Attachment ID&nbsp;<asp:TextBox ID="txtAttachmentId" runat="server">0</asp:TextBox>
    Document ID&nbsp;<asp:TextBox ID="txtDocumentRowId" runat="server">0</asp:TextBox>
    </asp:Panel>
    
      <h3 class="isleH3_Block required">Select a file to attach<a class="toolTipLink" id="tipFile" title="Attachment|Select a file to upload as an attachment for this resource."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:FileUpload ID="fileUpload" runat="server"  />
      <asp:Button ID="btnUpload" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnUpload_Click"  Text="Upload" CausesValidation="false"></asp:Button>
      <asp:Panel ID="currentFilePanel" runat="server" Visible="false">
      <h3 class="isleH3_Block">Attachment</h3>
      <asp:HyperLink ID="lblDocumentLink" runat="server" Target="_blank" Visible="true">View File</asp:HyperLink>
      <asp:Label ID="currentFileName" runat="server" Visible="false" ></asp:Label>
      </asp:Panel>

      <h3 class="isleH3_Block required">Attachment Title <a class="toolTipLink" id="tipFileTitle" title="Title|The friendly name or title of the attachment"><img src="/images/icons/infoBubble.gif" alt="" /></a><span class="lengthTracker" id="lt_AttachmentTitle" minLength="10"></span></h3>
      <asp:textbox id="txtFileTitle" MaxLength="100" runat="server" CssClass="textTracker" trackerName="lt_AttachmentTitle" />

      <h3 class="isleH3_Block">Attachment Summary <a class="toolTipLink" id="tipAttachmentSummary" title="Attachment Summary|Description for the contents of the attachment"><img src="/images/icons/infoBubble.gif" alt="" /></a><span class="lengthTracker" id="lt_AttachmentSummary" minLength="20"></span></h3>
      <asp:textbox id="txtAttachmentSummary" class="textBox textTracker" trackerName="lt_AttachmentSummary" MaxLength="500" TextMode="MultiLine" Rows="3" runat="server" />

      <h3 class="isleH3_Block required">Who can access this Attachment?<a class="toolTipLink" id="tipAccessPrivilege" title="Access Privilege|You can allow only specific groups of users to access this attachment by selecting the appropriate group from the box below. For example an answer key should NOT be publically available even though the content may be available."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlAttachmentPrivacyLevel" CssClass="ddl" runat="server">
        <asp:ListItem Value="1" Selected="True">Anyone can access, including students</asp:ListItem>
        <asp:ListItem Value="2">Only authenticated users</asp:ListItem>
        <asp:ListItem value="3">Only education staff at my school</asp:ListItem>
        <asp:ListItem value="4">Only education staff at schools in Illinois</asp:ListItem>
      </asp:DropDownList>


    <div class="buttons">
      <asp:Button ID="btnSaveAttachment" runat="server" Visible="true" Width="270px" CssClass="defaultButton" OnClick="btnSaveAttachment_Click" Text="Save and Add Another Attachment" ToolTip="Saves the current attachment and clears the entry fields (for easy addition of another attachment)" CausesValidation="false"></asp:Button>
      <asp:Button ID="btnDeleteAttachment" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnDeleteAttachment_Click" Text="Remove Attachment" CausesValidation="false"></asp:Button>
      <asp:Button ID="btnNewAttachment" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnNewAttachment_Click" Text="Cancel Changes" ToolTip="Clears the entry fields (for easy addition of another attachment), no updates are done" CausesValidation="false"></asp:Button>

      <asp:Button ID="btnSaveAttachmentContinue" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSaveAttachmentContinue_Click" Text="Save and Go to Next Step" ToolTip="Saves the current attachment, clears the entry fields and navigates to resources" CausesValidation="false"></asp:Button>

      <asp:Button ID="Button2" runat="server" Visible="false" CssClass="defaultButton btnFinish" OnClick="btnFinish_Click" Text="Finish" CausesValidation="false"></asp:Button>
    </div>

  </div>

  <div class="column right isleBox">
    <h2 class="isleBox_H2">Applied Attachments</h2>
    <asp:DropDownList id="ddlAttachment" runat="server" AutoPostBack="True" onselectedindexchanged="ddlAttachment_SelectedIndexChanged" style="display:none;" /><%-- The system uses this DDL behind the scenes. Do not remove or set to visible=false. --%>

    <asp:Literal ID="ltlAttachmentsList" runat="server" />
  </div>

</div>

<div class="tabBox" id="tab_references">
  <h2 class="isleH2">Additional Resources</h2>
  <p>Add references to texts or other supporting resources including websites.  You may add one or several resources.  There is no limit to the number of resources you can add.</p>
 
  <div class="column">


        <asp:Panel ID="Panel1" runat="server" Visible="false" >
          Ref ID&nbsp;
          <asp:TextBox ID="txtReferenceId" runat="server">0</asp:TextBox>
        </asp:Panel>
       
        <h3 class="isleH3_Block required">Title <a class="toolTipLink" id="tipPublicationTitle" title="Publication Title|The title of the referenced book, article, etc."><img src="/images/icons/infoBubble.gif" alt="" /></a><span class="lengthTracker" id="lt_referencePublicationTitle" minLength="3"></span></h3>
        <asp:TextBox id="txtPublicationTitle" MaxLength="100" CssClass="textTracker" runat="server" trackerName="lt_referencePublicationTitle" />
        
        <h3 class="isleH3_Block ">Author <a class="toolTipLink" id="tipAuthor" title="Author|The author of this resource."><img src="/images/icons/infoBubble.gif" alt="" /></a><span class="lengthTracker" id="lt_referenceAuthor" minLength="5"></span></h3>
        <asp:TextBox ID="txtAuthor" MaxLength="100" CssClass="textTracker" runat="server" trackerName="lt_referenceAuthor" />

        <h3 class="isleH3_Block ">Publisher <a class="toolTipLink" id="tipPublisher" title="Publisher|The publisher of this resource."><img src="/images/icons/infoBubble.gif" alt="" /></a><span class="lengthTracker" id="lt_referencePublisher" minLength="5"></span></h3>
        <asp:TextBox id="txtPublisher" MaxLength="100" CssClass="textTracker" runat="server" trackerName="lt_referencePublisher" />
        
        <h3 class="isleH3_Block">Additional Information <a class="toolTipLink" id="tipRefAdditionalInfo" title="Additional Information|This is a free form field. A possible use could be to more accurately site the resource to say a chapter in a book, an article in a magazine, or even a page number."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
        <asp:TextBox id="txtAdditionalInfo" MaxLength="500" CssClass="shortTextArea" TextMode="MultiLine" Rows="2" runat="server" />

        <h3 class="isleH3_Block">ISBN <a class="toolTipLink" id="tipISBN" title="ISBN|Optional field to contain the ISBN for a resource such as a book."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
        <asp:TextBox id="txtISBN" MaxLength="100" runat="server" />
        
        <h3 class="isleH3_Block">Resource Url <a class="toolTipLink" id="A3" title="Resource Url|Optional field to contain the web url for the resource if relevent."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
        <asp:TextBox id="txtUrl" MaxLength="200" runat="server" Text="http://" OnClick="this.select()" />

        <div class="buttons">
          <asp:Button ID="btnSaveReference" runat="server" Visible="true" CssClass="defaultButton" Width="270px" OnClick="btnSaveReference_Click" Text="Save and Add Another Resource" ToolTip="Saves the current item and clears the entry fields (for easy addition of another item)" CausesValidation="false"></asp:Button>
          <asp:Button ID="btnDeleteReference" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnDeleteReference_Click" Text="Remove Resource" CausesValidation="false"></asp:Button>
          <asp:Button ID="btnNewReference" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnNewReference_Click" Text="Cancel Changes" ToolTip="Clears the entry fields (for easy addition of another attachment), no updates are done" CausesValidation="false"></asp:Button>
          <input type="button" onclick="doPreview()" class="defaultButton" style="display:none;" value="Preview" />
          <asp:Button ID="Button3" runat="server" Visible="false" CssClass="defaultButton btnFinish" OnClick="btnFinish_Click" Text="Finish Authoring" CausesValidation="false"></asp:Button>
        </div>

      <asp:Label ID="refMessage" runat="server"></asp:Label>

  </div>

  <div class="column right isleBox">
    <h2 class="isleBox_H2">Applied Resources</h2>
    <asp:DropDownList id="ddlReference" runat="server" AutoPostBack="True" onselectedindexchanged="ddlReference_SelectedIndexChanged" style="display:none;" /><%-- The system uses this DDL behind the scenes. Do not remove or set to visible=false. --%>
    <asp:Literal ID="ltlReferencesList" runat="server" />
  </div>


</div>

</div>
</asp:panel>
<div id="loginMessage" runat="server">
  <h2 class="isleH2">You must be logged in and authorized in order to use this feature.</h2>
  <p>Only members of authorized organizations can create resource pages.</p>
</div>


<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
  <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.LRW.controls.Authoring</asp:Literal>
  <asp:Literal ID="ltlTabGetter" runat="server" Visible="false">tabID</asp:Literal>
  <asp:Literal ID="ltlBasicTabName" runat="server" Visible="false">basicInfo</asp:Literal>
  <asp:Literal ID="ltlWebContentTabName" runat="server" Visible="false">webcontent</asp:Literal>
  <asp:Literal ID="ltlAttachmentsTabName" runat="server" Visible="false">attachments</asp:Literal>
  <asp:Literal ID="ltlReferencesTabName" runat="server" Visible="false">references</asp:Literal>
  <asp:Literal ID="ltlMinTxtTitleLength" runat="server" Visible="false">10</asp:Literal>
  <asp:Literal ID="ltlMinTxtDescriptionLength" runat="server" Visible="false">25</asp:Literal>
  <asp:Literal ID="ltlMinUsageRightsURLLength" runat="server" Visible="false">15</asp:Literal>
  <asp:Literal ID="litPreviewUrlTemplate2" runat="server" Visible="false">/Repository/ResourcePage.aspx?rid={0}</asp:Literal>
  <asp:Literal ID="litPreviewUrlTemplate" runat="server" Visible="false">/Content/{0}/{1}</asp:Literal>

  <asp:Literal ID="ltlAppliedAttachmentTemplate" runat="server" Visible="false"><h3 class="isleH3_Block"><span class="attachmentTitle">{0} ({2})</span><a class="textLink" href="javascript:removeAttachment('{3}')">Remove</a><a class="textLink" href="javascript:editAttachment('{3}')">Edit</a> <a class="textLink" href="{4}" target="_blank">View</a></h3><p>{1}</p></asp:Literal>

  <asp:Literal ID="ltlAppliedReferencesHeader" runat="server" Visible="false"><h3 class="isleH3_Block">Resource <a class="textLink" href="javascript:removeReference('{0}')">Remove</a><a class="textLink" href="javascript:editReference('{0}')">Edit</a> </h3></asp:Literal>
  <asp:Literal ID="ltlAppliedReferencesTemplate" runat="server" Visible="false"><p><b>{0}:</b> {1}</p></asp:Literal>
  <asp:Literal ID="ltlReferenceColumns" runat="server" Visible="false">Title,Author,Publisher,AdditionalInfo,ISBN,ReferenceUrl</asp:Literal>
  <asp:Literal ID="ltlReferenceColumnsDisplay" runat="server" Visible="false">Title,Author,Publisher,Additional Information,ISBN,Resource URL</asp:Literal>
  <asp:Literal id="previewLink" runat="server" Visible="false" />
</asp:Panel>
