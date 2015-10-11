<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Authoring.ascx.cs" Inherits="IOER.Controls.Content.Authoring" %>


<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor" %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.ToolbarButton" %>
<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.ContextMenu"  %>

<%@ Register Assembly="Obout.Ajax.UI" TagPrefix="obout" Namespace="Obout.Ajax.UI.HTMLEditor.Popups" %>
<%@ Register TagPrefix="custom"	Namespace="CustomToolbarButton" Assembly="IOER"  %>
<%@ Register TagPrefix="custom" Namespace="CustomPopups"  %>
<%@ Register assembly="Obout.Ajax.UI" namespace="Obout.Ajax.UI.TreeView" tagprefix="obout" %>

<%@ Register TagPrefix="uc1" TagName="ConditionsOfUseSelector" Src="/LRW/controls/ConditionsOfUseSelector.ascx" %>
<%@ Register TagPrefix="uc1" TagName="StandardsBrowser" Src="/Controls/StandardsBrowser7.ascx" %>
	<%--<obout:EditorPopupHolder runat="server" ID="popupHolder" />
  <obout:PopupHolder runat="server" id="popupHolder1" DefaultAddPolicy="Demand" ChangingOnLoad="true">
   <Demand>
       <obout:ImageBrowser runat="server" ID="ib1" />
   </Demand>
</obout:PopupHolder>--%>

<script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />

<script type="text/javascript" language="javascript">
    //From server
    <%=userProxyId %>

    var supplementID = 0;
    var supplementEditor;
    var supplementContent;

    $(document).ready(function () {
        //$(function () {
        //    $("#tabs").tabs();
        //});

        //$('#tabs').tabs({ active: tabId });


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
      $(".textTracker").on("keyup change", function () {
          updateTextLengths($(this), $("#" + $(this).attr("trackerName")));
      });
      $(".textTracker").change();

      //Fix display of navtabs
      var tabCount = $(".tabNav a.tab").length;
      var tabCountStatic = 10 + tabCount + 1;
      $(".tabNav a.tab").each(function () {
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
  function editAttachment(attachmentValue) {
      var currentIndex = 0;
      $("#<%=ddlAttachment.ClientID %> option").each(function () {
        if ($(this).prop("value") == attachmentValue) {
            $("#<%=ddlAttachment.ClientID %>").prop("selectedIndex", currentIndex).change();
      }
        currentIndex++;
    });
}

//Select references
function editReference(referenceValue) {
    var currentIndex = 0;
    $("#<%=ddlReference.ClientID %> option").each(function () {
        if ($(this).prop("value") == referenceValue) {
            $("#<%=ddlReference.ClientID %>").prop("selectedIndex", currentIndex).change();
      }
        currentIndex++;
    });
}

function doPreview() {
    window.location = $(".tab.last").prop("href");
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
    if (currentLength < minLength) {
        jqTracker.html("requires at least " + (minLength - currentLength) + " more characters");
    }
    else {
        jqTracker.html("Minimum length requirement met.");
    }
}
function Insert() {
    alert("Insert");
    oboutGetEditor('editor').InsertHTML("<a href='mailto://support@obout.com'>Obout support</a>");
}

function ToggleTreeview(target) {
    $("#" + target).slideToggle();
    var current = $("#toggleTree").html();
    if (current == "Show Curriculum Content") {
        $("#toggleTree").html('Show Guidance');
        $("#curriculumGuide").hide();
        $("#currContentSection").show();

    }
    else {
        $("#toggleTree").html('Show Curriculum Content');
        $("#curriculumGuide").show();
        $("#currContentSection").hide();
    }
}

</script>

<script type="text/javascript">
<!--

    function refreshList(parentNodeId, postedResourceIntID) {
        //Code to update the list of curriculum items
        //do QnD postback, then add ws client-side call
        //__doPostBack('btnRefreshFileList', postedID);
        if (parentNodeId == null) parentNodeId = "0";
        if (postedResourceIntID == null) postedResourceIntID = "0";

       // $("#nodeItems").html("A new document was uploaded. A refresh method is coming soon");
     //   $("#nodeItems").append("<br/>parent: " + parentNodeId + "<br/>postedResourceIntID: " + postedResourceIntID);

        doAjax("RefreshNodeDocuments", { userProxyId: userProxyId, nodeId: parentNodeId }, successDisplayNode);

        return true;
    }


    function removeNodeDocument(parentNodeId, postedResourceIntID) {
        //Code to update the delete a node attachment and refresh list of curriculum items
        if (parentNodeId == null) parentNodeId = "0";
        if (postedResourceIntID == null) postedResourceIntID = "0";

        doAjax("DeleteNodeDocument", { userProxyId: userProxyId, nodeId: parentNodeId }, successDeleteNodeDoc);

        return true;
    }

//AJAX functions
function doAjax(method, data, success) {
    $.ajax({
        url: "/Services/CurriculumService.asmx/" + method,
        async: true,
        success: function (msg) {
            try {
                success($.parseJSON(msg.d));
            }
            catch (e) {
                success($.parseJSON(msg.d));
            }
        },
        type: "POST",
        data: JSON.stringify(data),
        dataType: "json",
        contentType: "application/json; charset=utf-8"
    });

}

function successDeleteNodeDoc(data) {
    if (data.isValid) {
        renderData(data.data);
    }
    else {
        console.log("error:");
        console.log(data);
        alert(data.status);
    }
}
function successDisplayNode(data) {
    if (data.isValid) {
        renderData(data.data);
    }
    else {
        console.log("error:");
        console.log(data);
        alert(data.status);
    }
}
function renderData(data) {

    $("#nodeItems").html("");
    $(".lblFileList").css("display", "none");

    if (data.children.length == 0) {
        $("#nodeItems").html("<p class=\"message\">There are no Resources associated with this " + data.contentType + "</p>");
    }
    else {

        var template = $("#template_nodeItem").html();
        for (i in data.children) {

            $("#nodeItems").append(
              template
                .replace(/{documentUrl}/g, data.children[i].documentUrl)
                .replace(/{resourceUrl}/g, data.children[i].resourceUrl)
                .replace(/{title}/g, data.children[i].title)
                .replace(/{message}/g, data.children[i].message)
            );
            if (data.children[i].documentUrl == "#") {
                $("#nodeItems .nodeItem").last().find(".resourceLinks").remove();
            } else {
                $("#nodeItems .nodeItem").last().find(".itemMessage").remove();
            }
        }
    }
}

    //-->
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
    function removeNodeAttachment(id) {

        var bresult
        bresult = confirm("Are you sure you want to remove this file?\n\n"
                    + "Click OK to remove this attachment or click Cancel.");
        var loc;

        loc = self.location;

        if (bresult) {
            //alert("delete requested for id " + id + "\nlocation = " + self.location);

            __doPostBack('btnRemNodeAtt', id);

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
  .currIntro { line-height: 200%; font-size: 120%;  }
    .currIntro .labelColumn { width: 200px; }

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
    width: 100%;
  }
  .buttons {
    text-align: center;
    padding: 10px;
  }
  .nodeButton {width: 350px; }

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
    margin: 5px auto;
    width: 75%;
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
  .navlist li {
    display: inline;
    list-style-type: none;
    padding-right: 20px;
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
    min-width: 200px;
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
<style type="text/css">
  .treePanel {width: 30%; background-color: #f5f5f5; display: inline-block; min-height: 500px;}
  .nodePanel {width: 33%; display: inline-block; vertical-align: top; min-height: 600px;}
  .contributePanel {width: 30%; display: inline-block; vertical-align: top;}
  .contributeIframe {width: 100%; height: 600px;}
  .contributePanel iframe { width: 100%; height: 600px; }

    .toolTipLink ul { padding: 2px 5px; }
h2 #toggleTree {
	background: url(/images/icons/open.png) no-repeat 0 11px;
	padding: 10px 0 0 25px;
	cursor: pointer;
}

h3, h4, h5 {
    padding-top: 10px;
}

h4 #toggleTree.close { background-image: url(/images/icons/close.png); }

ul {
    margin-left: 30px;
}
#curriculumGuide     { display: none; }
#toggleSection { background-color: #3572B8; color: #fff; text-align: center; border-radius: 5px; padding: 5px 10px; margin-bottom: 5px; width: 300px;}
#toggleSection a, #toggleSection a:visited, #toggleSection a:focus {color: #fff;}
#toggleSection:hover{background-color: #FF5707; color: #000;}

</style>
<style type="text/css">
@media screen and (max-width: 1450px) {
.tabBox {    width: 100%;  }
.treePanel, .nodePanel {width: 40%; }
.contributePanel {width: 80%; }
}
@media screen and (max-width: 650px) {
.tabBox {    width: 100%;  }
.treePanel, .nodePanel, .contributePanel {width: 75%; }
#toggleSection { width: 300px; }
}

</style>
<h1 class="isleH1">IOER Resource Authoring Tool</h1>
<asp:panel ID="containerPanel" runat="server">
<div id="statusDiv" style="float:right;" runat="server" visible="false">
    <a href="/My/Authored">Back to Search</a>
<asp:Button ID="btnPublish2" runat="server" Visible="false" CssClass="defaultButton publishButton" OnClick="btnPublish2_Click"  Text="Publish" CausesValidation="false"></asp:Button>
<asp:button id="btnPublish" runat="server" Visible="false" CssClass="defaultButton publishButton" OnCommand="btnPublish_Click" CommandName="PublishUpdate" Text="Publish" causesvalidation="false"></asp:button>

      <span style="float: right;">&nbsp; <a class="toolTipLink" id="tipStatus" title="Status|Current status of this resource:<ul><li><strong>In Progess</strong>: content is under construction, the content is not public.</li><li><strong>Submitted</strong>: Content authored on behalf of an organization requires approval. Status of Submitted indicates approval is pending.</li><li><strong>Requires Revision</strong>: For content submitted for approval, a status of Requires Revision indicates changes have been requested before content will be approved.</li><li><strong>Published</strong>: the content is now visible to the public (dependent on the privacy settings)</li><li><strong>Inactive</strong>: author has set content to inactive. An inactive item will no longer be available to the searches, libraries, etc.</li></ul>"><img src="/images/icons/infoBubble.gif" alt="" /> </a> </span>

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
<ul class="tabNav isleTabs" id="tabNavContentFile" runat="server" visible="false">
  <li><a href="javascript:showTab('basicInfo')" class="tab selected" id="tabNav_basicInfo">1: Basic Info</a></li>
  <li><a href="javascript:showTab('fileContent')" class="tab" id="tabNav_fileContent">2: Attachment</a></li>
  <li><a href="/CONTENT/<%=previewID%>/<%=cleanTitle%>" class="tab preview" target="resPrevw">Preview Any Time</a></li>
</ul>
<ul class="tabNav isleTabs" id="tabNavCurriculum" runat="server" visible="false">
  <li><a href="javascript:showTab('basicInfo')" class="tab selected" id="tabNav_basicInfo">1: Basic Info</a></li>
    <li><a href="javascript:showTab('webcontent')" class="tab" id="tabNav_webcontent">2: Description</a></li>
  <li><a href="javascript:showTab('curriculum')" class="tab" id="tabNav_curriculum">3: Curriculum</a></li>
  <li><a href="/CONTENT/<%=previewID%>/<%=cleanTitle%>" class="tab preview" target="resPrevw">Preview Any Time</a></li>
</ul>
 
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

      <h3 class="isleH3_Block required">Title<a class="toolTipLink" id="tipTitle" title="Title|The name or title of the resource"><img alt='' src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_Title" minLength="5"></span></h3>
      <asp:TextBox ID="txtTitle" class="textBox textTracker" trackerName="lt_Title" runat="server" />

      <h3 class="isleH3_Block required">Description<a class="toolTipLink" id="tipDescription" title="Description|A brief description of the resource. This field will be displayed in search results and the resource detail page as well as in the Learning Registry metadata if you choose to Publish it."><img src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_Description" minLength="15"></span></h3>
      <asp:TextBox ID="txtDescription" class="textBox textTracker" trackerName="lt_Description" Rows="5" TextMode="MultiLine"  runat="server" />
    

      <h3 class="isleH3_Block required">Select Usage Rights<a class="toolTipLink" id="tipUsageRights" title="Usage Rights|The URL where the owner specifies permissions for using the resource.|<b>Remix and Share:</b> You may adapt, edit, or tweak resource before using and sharing.|<b>Share Only:</b> You may copy, distribute or transmit the resource in its original form only.|<b>No Strings Attached:</b> No restrictions are placed on usage.|<b>Read the Fine Print:</b> Specific restrictions may be in place; read usage rights carefully.|<b>Attribution:</b> You must attribute the work in the manner specified by the author or licensor."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <uc1:ConditionsOfUseSelector ID="conditionsSelector" runat="server" />

      <h3 class="isleH3_Block required">Who can access this resource?<a class="toolTipLink" id="tipPrivilege" title="Access Privilege|You can allow only specific groups of users to access this resource by selecting the appropriate group from the box below."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlPrivacyLevel" CssClass="ddl" runat="server"></asp:DropDownList>

      <h3 class="isleH3_Block required">This resource is being Authored on behalf of...<a class="toolTipLink" id="tipOrganization" title="Authoring|If you are authoring this resource for your own purposes, select 'Myself'. Otherwise, select the appropriate organization from the box below."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlIsOrganizationContent" runat="server" Enabled="false" CssClass="ddl" >
        <asp:ListItem Value="1" Text="Myself" Selected="True"></asp:ListItem>
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
      <asp:Panel ID="newsPanel" runat="server" Visible="false">
		      <div class="clearFloat"></div>
          <h3>News</h3>	
		      <div class="labelColumn">
		      <asp:label runat="server">NewsId</asp:label>
		      </div>
		      <div class="dataColumn">
			      <asp:label id="newsId" runat="server">0</asp:label>
		      </div>
          <div class="column" style="width: 70%; margin: 5px auto;">

                    <obout:Editor ID="newsEditor" runat="server" Height="400px" Width="100%">
                        <TopToolbar Appearance="Custom">
                            <AddButtons>
                                <obout:Undo /><obout:Redo />
                                <obout:HorizontalSeparator />
                                <obout:Bold /><obout:Italic /><obout:Underline /><obout:StrikeThrough />
                                <obout:HorizontalSeparator />
                                <obout:ForeColorGroup />
                                <obout:BackColorGroup />
                                <obout:RemoveStyles />
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
                                <obout:InsertTable /><obout:InsertDiv />
                                <obout:InsertImage />
                                <obout:HorizontalSeparator />
                                <obout:SpellCheck />
                                <obout:HorizontalSeparator />
                                <%--<custom:ImmediateImageInsert runat="server" ID="myImmediateImageInsert" />--%>
                                <obout:VerticalSeparator />
                                <obout:FontName />
                                <obout:HorizontalSeparator /><obout:FontSize />
                                <obout:HorizontalSeparator /><obout:Header />
                                <obout:HorizontalSeparator />
                                <obout:Print />
                            </AddButtons>
                        </TopToolbar>
                        <EditPanel ID="newsEditPanel" FullHtml="false" runat="server"></EditPanel>
                        <BottomToolbar ShowDesignButton="true" ShowHtmlTextButton="true" ShowPreviewButton="true" ShowFullScreenButton="true"></BottomToolbar>
                    </obout:Editor>
                </div>
          <asp:Button ID="newsSaveButton" runat="server" Visible="true" CssClass="defaultButton" OnClick="newsSaveButton_Click" Text="Save News" CausesValidation="false"></asp:Button>

          <asp:Button ID="addNewNewsButton" runat="server" Visible="true" CssClass="defaultButton" OnClick="addNewNewsButton_Click" Text="Add New News Item" CausesValidation="false"></asp:Button>
      </asp:Panel>
		</div>
    <div class="column right isleBox">
      <h2 class="isleBox_H2">Guidance</h2>
      <h4 class="isleBox_H4_Block">What is This?</h4>
      <p class="MsoNormal">The Authoring Tool provides an interface for you to create an individual Content item  as its own online webpage. <span style="mso-spacerun:yes">&nbsp;</span>Documents, media and other resources that are not currently online can be created as a webpage by using this tool.<span style="mso-spacerun:yes">&nbsp;&nbsp; </span>The created webpage can be found by anyone searching within ISLE OER, unless you indicate otherwise, but only you can edit what you create here.<span style="mso-spacerun:yes">&nbsp; </span>You can save the page in your Library and share it with others.</p>
        <p>
            &nbsp;</p>
      <h4 class="isleBox_H4_Block">How do I use this Tool?</h4>
      <p>You’ll need the resource in its original format (such as Word document or PDF) and a few minutes to properly describe the resource, apply Usage and Access Rights, and upload the files you are associating with the Resource Content.</p>
      
      <h4 class="isleBox_H4_Block">Getting Started</h4>
      <ol style="margin-left:20px;">
        <li>Enter a Title for your resource. Be sure it describes your resource in a way that others will understand.</li>
        <li>Describe your resource. Type in a paragraph or two that tells others about it.</li>
        <li>Select the Usage Rights that best apply to your resource.</li>
        <li>Decide who can access your resource. Selecting anything other than "Anyone" will require users to be logged in and members of the appropriate group in order to access the content.</li>
        <li>If you are authoring resources on your own or for your own purposes, select "Myself" in the last box. Otherwise, choose an organization you belong to.</li>
      </ol>

      <h4 class="isleBox_H4_Block">Supported files:</h4>
      <ul style="margin-left:20px;">
        <li><b>MS Office</b> (.doc/docx, .ppt/pptx, .xls/xlsx, etc.)</li>
        <li><b>Documents</b> (.pdf, .txt, .rtf, .pages, etc.)</li>
        <li><b>Images</b> (.jpg, .png, .gif, etc.)</li>
        <li><b>Audio</b> (.wav, .mp3, .ogg, etc.)</li>
        <li><b>Archives</b> (.zip, .rar, .7z, etc.)</li>
        <li><b>Smart Board</b>(.xbk, .notebook)</li>
      </ul>
        
    </div>
  </div>
</div>

</div>

<div id="Stage2Items" runat="server" visible="false">
    <!-- To be used to hide the rest of the form until the first section is complete -->
    
        <div class="tabBox tab_webcontent" id="tab_webcontent" >
            <h2 class="isleH2">Web Page Content</h2>
            <div style="width: 70%; margin-left: auto; margin-right: auto;">

                <div class="column" style="width: 70%; margin: 5px auto;">

                    <obout:Editor ID="editor" runat="server" Height="400px" Width="100%">
                        <TopToolbar Appearance="Custom">
                            <AddButtons>
                                <obout:Undo /><obout:Redo />
                                <obout:HorizontalSeparator />
                                <obout:Bold /><obout:Italic /><obout:Underline /><obout:StrikeThrough />
                                <obout:HorizontalSeparator />
                                <obout:ForeColorGroup />
                                <obout:BackColorGroup />
                                <obout:RemoveStyles />
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
                                <obout:InsertTable /><obout:InsertDiv />
                                <obout:InsertImage />
                                <obout:HorizontalSeparator />
                                <obout:SpellCheck />
                                <obout:HorizontalSeparator />
                                <%--<custom:ImmediateImageInsert runat="server" ID="myImmediateImageInsert" />--%>
                                <obout:VerticalSeparator />
                                <obout:FontName />
                                <obout:HorizontalSeparator /><obout:FontSize />
                                <obout:HorizontalSeparator /><obout:Header />
                                <obout:HorizontalSeparator />
                                <obout:Print />
                            </AddButtons>
                        </TopToolbar>
                        <EditPanel ID="EditPanel1" FullHtml="false" runat="server"></EditPanel>
                        <BottomToolbar ShowDesignButton="true" ShowHtmlTextButton="true" ShowPreviewButton="true" ShowFullScreenButton="true"></BottomToolbar>
                    </obout:Editor>
                </div>
                <div class="isleBox column right" style="width: 25%; min-height: 390px;">
                    <h2 class="isleBox_H2">Instructions</h2>
                    <ul style="margin-left: 20px;">
                        <li>1. Use the text editor to type or copy and paste from another document. Format the text with paragraphs, bullets, tables, and more--just like any other word processor.</li>
                        <li>2. The web page will be visible to the public, so you can use it to show your resource, add instructions, or just expand on the description.</li>
                        <li>3. Click the "Save and Continue" button.</li>
                    </ul>

                </div>
                <div class="buttons">
                    <asp:Button ID="btnSave2" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSaveWeb_Click" Text="Save" CausesValidation="false"></asp:Button>
                    <asp:Button ID="btnSaveWebContinue" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSaveWebContinue_Click" Text="Save and Go to Next Step" CausesValidation="false"></asp:Button>

                    <asp:Button ID="btnFinish2" runat="server" Visible="false" CssClass="defaultButton btnFinish" OnClick="btnFinish_Click" Text="Finish" CausesValidation="false"></asp:Button>
                </div>
            </div>
        </div>
    
<asp:Panel ID="attachmentsPanel" runat="server" Visible="true">
<div class="tabBox" id="tab_attachments"  >
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

      <h3 class="isleH3_Block required">Attachment Title <a class="toolTipLink" id="tipFileTitle" title="Title|The friendly name or title of the attachment"><img src="/images/icons/infoBubble.gif" alt="" /></a><span class="lengthTracker" id="lt_AttachmentTitle" minLength="5"></span></h3>
      <asp:textbox id="txtFileTitle" MaxLength="100" runat="server" CssClass="textTracker" trackerName="lt_AttachmentTitle" />

      <h3 class="isleH3_Block">Attachment Summary <a class="toolTipLink" id="tipAttachmentSummary" title="Attachment Summary|Description for the contents of the attachment"><img src="/images/icons/infoBubble.gif" alt="" /></a><span class="lengthTracker" id="lt_AttachmentSummary" minLength="15"></span></h3>
      <asp:textbox id="txtAttachmentSummary" class="textBox textTracker" trackerName="lt_AttachmentSummary" MaxLength="500" TextMode="MultiLine" Rows="3" runat="server" />

      <h3 class="isleH3_Block required">Who can access this Attachment?<a class="toolTipLink" id="tipAccessPrivilege" title="Access Privilege|You can allow only specific groups of users to access this attachment by selecting the appropriate group from the box below. For example an answer key should NOT be publically available even though the content may be available."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlAttachmentPrivacyLevel" CssClass="ddl" runat="server">
        <asp:ListItem Value="1" Selected="True">Anyone can access, including students</asp:ListItem>
        <asp:ListItem Value="2">Only authenticated users</asp:ListItem>
        <asp:ListItem value="3">Only education staff at my school</asp:ListItem>
        <asp:ListItem value="4">Only education staff at schools in Illinois</asp:ListItem>
      </asp:DropDownList>


    <div class="buttons">
      <asp:Button ID="btnSaveAttachment" runat="server" Visible="true"  CssClass="defaultButton" OnClick="btnSaveAttachment_Click" Text="Save and Add Another Attachment" ToolTip="Saves the current attachment and clears the entry fields (for easy addition of another attachment)" CausesValidation="false"></asp:Button>
      <asp:Button ID="btnDeleteAttachment" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnDeleteAttachment_Click" Text="Remove Attachment" CausesValidation="false"></asp:Button>
      <asp:Button ID="btnNewAttachment" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnNewAttachment_Click" Text="Cancel Changes" ToolTip="Clears the entry fields (for easy addition of another attachment), no updates are done" CausesValidation="false"></asp:Button>

      <asp:Button ID="btnSaveAttachmentContinue" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSaveAttachmentContinue_Click" Text="Save and Go to Next Step" ToolTip="Saves the current attachment, clears the entry fields and navigates to resources" CausesValidation="false"></asp:Button>

      <asp:Button ID="Button2" runat="server" Visible="false" CssClass="defaultButton btnFinish" OnClick="btnFinish_Click" Text="Finish" CausesValidation="false"></asp:Button>
    </div>

  </div>

  <div class="column right isleBox">
    <h2 class="isleBox_H2">Applied Attachments</h2>
    <asp:DropDownList id="ddlAttachment" runat="server" AutoPostBack="True" onselectedindexchanged="ddlAttachment_SelectedIndexChanged" style="display:none;" /><!-- The system uses this DDL behind the scenes. Do not remove or set to visible=false. -->

    <asp:Literal ID="ltlAttachmentsList" runat="server" />
  </div>

</div>
</asp:Panel>
<asp:Panel ID="referencesPanel" runat="server" Visible="true">
<div class="tabBox" id="tab_references"  >
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
          <asp:Button ID="btnSaveReference" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSaveReference_Click" Text="Save and Add Another Resource" ToolTip="Saves the current item and clears the entry fields (for easy addition of another item)" CausesValidation="false"></asp:Button>
          <asp:Button ID="btnDeleteReference" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnDeleteReference_Click" Text="Remove Resource" CausesValidation="false"></asp:Button>
          <asp:Button ID="btnNewReference" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnNewReference_Click" Text="Cancel Changes" ToolTip="Clears the entry fields (for easy addition of another attachment), no updates are done" CausesValidation="false"></asp:Button>
          <input type="button" onclick="doPreview()" class="defaultButton" style="display:none;" value="Preview" />
          <asp:Button ID="Button3" runat="server" Visible="false" CssClass="defaultButton btnFinish" OnClick="btnFinish_Click" Text="Finish Authoring" CausesValidation="false"></asp:Button>
        </div>

      <asp:Label ID="refMessage" runat="server"></asp:Label>

  </div>

  <div class="column right isleBox">
    <h2 class="isleBox_H2">Applied Resources</h2>
    <asp:DropDownList id="ddlReference" runat="server" AutoPostBack="True" onselectedindexchanged="ddlReference_SelectedIndexChanged" style="display:none;" /><!-- The system uses this DDL behind the scenes. Do not remove or set to visible=false. -->
    <asp:Literal ID="ltlReferencesList" runat="server" />
  </div>

</div>
</asp:Panel>
</div>
    <div id="FileContentItem" runat="server" style="width: 65%; margin: 0px auto;" visible="false">
        <div class="tabBox" id="tab_fileContent">
            <h2 class="isleH2">Content File</h2>
            <p>
                You may only replace the exising file with the same name.
                <br />
                If you want to use a new file, then this content should be set inactive/deleted, and then you will need to create a new file resource <a href="/Contribute/?mode=upload">using the quick contribute tool</a>
            </p>

            <div class="column">
                <asp:Panel ID="docKeyPanel2" runat="server" Visible="false">
                    Document ID&nbsp;<asp:TextBox ID="txtDocumentRowId2" runat="server">0</asp:TextBox>
                    Doc url&nbsp;<asp:Label ID="fileContentUrl" runat="server">0</asp:Label>
                </asp:Panel>

                <asp:Panel ID="contentFilePanel" runat="server" Visible="true">
                    <h3 class="isleH3_Block">
                        <asp:Literal ID="contentFileName" runat="server"></asp:Literal></h3>
                    <p>
                        <asp:Label ID="contentFileDescription" runat="server" Visible="true"></asp:Label>
                    </p>
                    <asp:HyperLink ID="linkContentFile" runat="server" Target="_blank" Visible="true">View File</asp:HyperLink>

                </asp:Panel>


                <h3 class="isleH3_Block required">Select a replacement file to attach<a class="toolTipLink" id="A1" title="Attachment|Select a file to upload to replace the current file."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
                <asp:FileUpload ID="contentFileUpload" runat="server" />

                <div class="buttons">
                    <asp:Button ID="btnUploadNewContentFile" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnContentFileUpload_Click" Text="Save/Replace File" ToolTip="Saves the current attachment and ensures the same file name is used" CausesValidation="false"></asp:Button>

                </div>

            </div>
        </div>

    </div>

    <div id="CurriculumItem" runat="server" visible="false">
        <div class="tabBox" id="tab_curriculum">
            <h2 class="isleH2">Curriculum</h2>

            <div >
                
          <div id="toggleSection"><a href="javascript:void(0);">
                        <h2 id="toggleTree" onclick="ToggleTreeview('currContentSection');" class="message">Show Guidance</h2>
                    </a></div>


                <asp:Panel ID="curriculumIntroPanel" CssClass="currIntro" runat="server" Visible="false">
                    <h3 class="isleH3_Block">Creating a new Curriculum</h3>
                    <p>When starting a new curriculum, you may find it convenient to pre-create the hierarchy of modules, units, lessons, etc. The system will present the hierarchy in a familiar folder or tree structure. This approach would allow you to develop the curriculum in a non-linear fashion. Alternately, if you just want to create as you go, then select a value of 1 for modules, and zero for the rest (or any combination) and click <strong>Create Hierarchy</strong>.</p>
                    <p>Create a default hierarchy by selecting the number of modules and average number of (optionally) units, lessons, and activities.</p>
                    <!-- -->
                    <div class="labelColumn">
                        <asp:Label ID="lblId" AssociatedControlID="ddlModules" runat="server">Level 1 - Modules</asp:Label>
                    </div>
                    <div class="dataColumn">
                        <asp:DropDownList ID="ddlModules" runat="server">
                            <asp:ListItem Text="1" Value="1"></asp:ListItem>
                            <asp:ListItem Text="2" Value="2"></asp:ListItem>
                            <asp:ListItem Text="3" Value="3" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="4" Value="4"></asp:ListItem>
                            <asp:ListItem Text="5" Value="5"></asp:ListItem>
                            <asp:ListItem Text="6" Value="6"></asp:ListItem>
                            <asp:ListItem Text="7" Value="7"></asp:ListItem>
                            <asp:ListItem Text="9" Value="9"></asp:ListItem>
                            <asp:ListItem Text="8" Value="8"></asp:ListItem>
                            <asp:ListItem Text="10" Value="10"></asp:ListItem>
                            <asp:ListItem Text="11" Value="11"></asp:ListItem>
                            <asp:ListItem Text="12" Value="12"></asp:ListItem>
                            <asp:ListItem Text="13" Value="13"></asp:ListItem>
                            <asp:ListItem Text="14" Value="14"></asp:ListItem>
                            <asp:ListItem Text="15" Value="15"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn">
                        <asp:Label ID="Label2" AssociatedControlID="ddlUnits" runat="server">Level 2 - Units</asp:Label>
                    </div>
                    <div class="dataColumn">
                        <asp:DropDownList ID="ddlUnits" runat="server">
                            <asp:ListItem Text="1" Value="1" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="2" Value="2"></asp:ListItem>
                            <asp:ListItem Text="3" Value="3"></asp:ListItem>
                            <asp:ListItem Text="4" Value="4"></asp:ListItem>
                            <asp:ListItem Text="5" Value="5"></asp:ListItem>
                            <asp:ListItem Text="6" Value="6"></asp:ListItem>
                            <asp:ListItem Text="7" Value="7"></asp:ListItem>
                            <asp:ListItem Text="8" Value="8"></asp:ListItem>
                            <asp:ListItem Text="9" Value="9"></asp:ListItem>
                            <asp:ListItem Text="10" Value="10"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn">
                        <asp:Label ID="Label3" AssociatedControlID="ddlLessons" runat="server">Level 3 - Lessons</asp:Label>
                    </div>
                    <div class="dataColumn">
                        <asp:DropDownList ID="ddlLessons" runat="server">
                            <asp:ListItem Text="0" Value="0"></asp:ListItem>
                            <asp:ListItem Text="1" Value="1"></asp:ListItem>
                            <asp:ListItem Text="2" Value="2"  Selected="True"></asp:ListItem>
                            <asp:ListItem Text="3" Value="3"></asp:ListItem>
                            <asp:ListItem Text="4" Value="4"></asp:ListItem>
                            <asp:ListItem Text="5" Value="5"></asp:ListItem>
                            <asp:ListItem Text="6" Value="6"></asp:ListItem>
                            <asp:ListItem Text="7" Value="7"></asp:ListItem>
                            <asp:ListItem Text="8" Value="8"></asp:ListItem>
                            <asp:ListItem Text="9" Value="9"></asp:ListItem>
                            <asp:ListItem Text="10" Value="10"></asp:ListItem>
                            <asp:ListItem Text="12" Value="12"></asp:ListItem>
                            <asp:ListItem Text="14" Value="14"></asp:ListItem>
                            <asp:ListItem Text="16" Value="16"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn">
                        <asp:Label ID="Label4" AssociatedControlID="ddlActivities" runat="server">Level 4 - Activities</asp:Label>
                    </div>
                    <div class="dataColumn">
                        <asp:DropDownList ID="ddlActivities" runat="server">
                            <asp:ListItem Text="0" Value="0" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="1" Value="1"></asp:ListItem>
                            <asp:ListItem Text="2" Value="2"></asp:ListItem>
                            <asp:ListItem Text="3" Value="3"></asp:ListItem>
                            <asp:ListItem Text="4" Value="4"></asp:ListItem>
                            <asp:ListItem Text="5" Value="5"></asp:ListItem>
                            <asp:ListItem Text="6" Value="6"></asp:ListItem>
                            <asp:ListItem Text="7" Value="7"></asp:ListItem>
                            <asp:ListItem Text="8" Value="8"></asp:ListItem>
                            <asp:ListItem Text="9" Value="9"></asp:ListItem>
                            <asp:ListItem Text="10" Value="10"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn">&nbsp;</div>
                    <div class="dataColumn">
                        <asp:Button ID="btnCreateHierarchy" runat="server" Visible="true" CssClass="defaultButton publishButton" OnClick="btnCreateHierarchy_Click" Text="Create Hierarchy" CausesValidation="false"></asp:Button>
                    </div>
                    <div class="clearFloat"></div>
                    <div class="dataColumn">Or create ISBE format using number of modules</div>
                    <div class="clearFloat"></div>
                    <div class="labelColumn">&nbsp;</div>
                    <div class="dataColumn">
                        <asp:Button ID="btnCreateIsbeCurriculum" runat="server" Visible="true" CssClass="defaultButton publishButton" OnClick="btnCreateIsbeCurriculum_Click" Text="Create ISBE Hierarchy" CausesValidation="false"></asp:Button>
                    </div>
                </asp:Panel>

                <div id="currContentSection">

                <div id="treePanel" class="treePanel" runat="server">
          
                    <div id="treeColumn">
                        <h3 class="isleH3_SectionHdr">Curriculum Outline</h3>
                        <obout:Tree ID="OBTreeview" CssClass="vista" runat="server"
                            OnSelectedTreeNodeChanged="OBTreeview_SelectedTreeNodeChanged">
                        </obout:Tree>
                    </div>
                </div>

                <div class="nodePanel">
                    <h3 class="isleH3_SectionHdr">Node Content</h3>
                    <asp:Panel ID="noNodePanel" runat="server" Visible="true">
                        <p>Select a node to update or add files.</p>
                    </asp:Panel>
                    <asp:Panel ID="nodePanel" runat="server" Visible="false">

                        <asp:Panel ID="nodeKeyPanel" runat="server" Visible="false">
                            <asp:Label ID="nodeId" runat="server"></asp:Label>
                            <asp:Label ID="parentNodeId" runat="server"></asp:Label>
                            <asp:Label ID="nodeSortOrder" runat="server"></asp:Label>
                            <asp:Label ID="contentTypeId" Visible="false" runat="server"></asp:Label>
                            <asp:Label ID="lblResourceId" Visible="false" runat="server"></asp:Label>

                        </asp:Panel>
                        <h3 class="isleH3_Block">Content Type</h3>
                        <asp:Label ID="lblNodeContentType" Visible="true" runat="server"></asp:Label>
                        <br />
                        <asp:Label ID="nodeNavSection" runat="server" Visible="false"></asp:Label>
                        <asp:DropDownList ID="ddlNodeContentType" Visible="false" runat="server"></asp:DropDownList>
                        <h3 class="isleH3_Block">Status</h3>
                        <asp:Label ID="lblNodeStatus" Visible="true" runat="server"></asp:Label>

                        <h3 class="isleH3_Block required">Title<a class="toolTipLink" id="tipNodeTitle" title="Title|The name or title of the node"><img src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_NodeTitle" minlength="5"></span></h3>
                        <asp:TextBox ID="txtNodeTitle" class="textBox textTracker" trackerName="lt_NodeTitle" runat="server" />

                        <h3 class="isleH3_Block required">Description<a class="toolTipLink" id="A9" title="Description|A brief description of the node. This field will be displayed in search results and the resource detail page as well as in the Learning Registry metadata if you choose to Publish it."><img src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_NodeDescription" minlength="15"></span></h3>
                        <asp:TextBox ID="txtNodeDescription" class="textBox textTracker" trackerName="lt_NodeDescription" Rows="5" TextMode="MultiLine" runat="server" />
                                                
                        <h3 class="isleH3_Block ">Timeframe<a class="toolTipLink" id="A2" title="Timeframe|The estimated length of time to complete the node"><img src="/images/icons/infoBubble.gif" alt="" /></a> </h3>
                        <asp:TextBox ID="txtTimeframe" class="textBox" runat="server"  minlength="5"/>

                        <h3 class="isleH3_Block required">Who can access this resource?<a class="toolTipLink" id="tipNodePrivilege" title="Access Privilege|You can allow only specific groups of users to access this resource by selecting the appropriate group from the box below."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
                        <asp:DropDownList ID="ddlNodePrivacyLevel" CssClass="ddl" runat="server"></asp:DropDownList>

                                                
                        <h3 class="isleH3_Block ">Auto-Display Document<a class="toolTipLink" id="A4" title="Auto-Display Document|If a document is added here, it will be previewed automatically as soon as the node is clicked (dependent on the curriculum presentation format)."><img src="/images/icons/infoBubble.gif" alt="" /></a> </h3>
                        <asp:FileUpload ID="autodocUpload" runat="server"  />
                         <asp:Button ID="btnAutodocUpload" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnAutodocUpload_Click"  Text="Upload/Replace" CausesValidation="false"></asp:Button>
                          <asp:Panel ID="autodocPanel" runat="server" Visible="false">
                          <h3 class="isleH3_Block">Attachment</h3>
                          <asp:HyperLink ID="autodocLink" runat="server" Target="_blank" Visible="true">View File</asp:HyperLink>
                          <asp:Label ID="autodocFileName" runat="server" Visible="true" ></asp:Label>
                              <asp:Literal ID="txtAutoDocumentRowId" runat="server" Visible="false"></asp:Literal>
                          </asp:Panel>

                      <style type="text/css">
                        #btnShowStandardsBrowser { display: block; margin: 10px auto; }
                        .standardsBox { display: none; position: fixed; top: 100px; left: 10%; right: 10%; height: 650px; z-index: 100; border-radius: 5px; background-color: #EEE; box-shadow: 0 0 15px -5px #000; }
                        .standardsBox .header { background-color: #4AA394; color: #FFF; margin: -5px -5px 10px -5px; padding: 5px; }
                        #btnCloseStandardsBrowser { float: right; background-color: #F00; opacity: 0.8; color: #FFF; text-align: center; font-weight: bold; height: 22px; width: 22px; line-height: 18px; transition: opacity 0.2s; border-radius: 5px; border-width: 1px; }
                        #btnCloseStandardsBrowser:hover, #btnCloseStandardsBrowser:focus { opacity: 1; cursor: pointer; }
                      </style>
                      <input type="button" class="defaultButton nodeButton" id="btnShowStandardsBrowser" value="Show Standards Browser" onclick="showStandardsBrowser();" />

                        <asp:Panel ID="StandardsBrowserContainer" CssClass="standardsBox isleBox" runat="server">
                          <h2 class="header"><input type="button" id="btnCloseStandardsBrowser" onclick="hideStandardsBrowser();" value="X" />Standards Browser</h2>
                          <script type="text/javascript">
                            var SB7mode = "tag";
                            var curriculumMode = true;
                            function loadStandards() {
                              var hdn = $(".hdnStandards");
                              hdn.val("");
                              var selected = [];
                              SBstoreDDLValues();
                              for (i in selectedStandards) {
                                selected.push({
                                  standardID: selectedStandards[i].id,
                                  code: selectedStandards[i].code,
                                  alignmentTypeID: selectedStandards[i].alignmentType,
                                  usageTypeID: selectedStandards[i].usageType
                                });
                              }
                              /*$("#SB7 #SBselected .selectedStandard").each(function () {
                                var box = $(this);
                                var code = box.attr("data-code");
                                if (code == null || typeof (code) == "undefined" || code == "{code}") { code = ""; }
                                selected.push({
                                  standardID: parseInt(box.attr("data-standardID")),
                                  code: code,
                                  alignmentTypeID: box.find(".alignmentType option:selected").attr("value"),
                                  standardTypeID: box.find(".standardType option:selected").attr("value")
                                });
                              });*/
                              hdn.val(JSON.stringify(selected));
                            }
                            function showStandardsBrowser() {
                              $(".standardsBox").fadeIn();
                            }
                            function hideStandardsBrowser() {
                              $(".standardsBox").fadeOut();
                            }
                          </script>
                          <uc1:StandardsBrowser ID="StandardsBrowser" runat="server" />
                          <input type="hidden" id="hdnStandards" class="hdnStandards" value="" runat="server" />
                        </asp:Panel>

                        <div style="text-align: center;">

                            <asp:Button ID="btnUpdateNode" runat="server" Visible="false" CssClass="defaultButton nodeButton" OnClick="btnUpdateNode_Click" OnClientClick="loadStandards();" Text="Save Node" ToolTip="Saves the current item" CausesValidation="false"></asp:Button>
                            <asp:Button ID="btnPublishNode" runat="server" Visible="false" CssClass="defaultButton nodeButton" OnClick="btnPublishNode_Click" Text="Publish Node" ToolTip="Publishes this node to make it searchable, and allow for adding tags" CausesValidation="false"></asp:Button>
                            <br />
                            <asp:Panel ID="nodeButtons" runat="server" Visible="false">
                                <asp:Button ID="btnDeleteNode" runat="server" Visible="false" Enabled="true" CssClass="defaultButton nodeButton" OnClick="btnDeleteNode_Click" OnClientClick="return confirm('Are you certain that you want to delete this node?');" Text="Delete Node" ToolTip="deletes the current item" CausesValidation="false"></asp:Button>
                                <br />
                                <asp:Button ID="btnInsertNode" runat="server" Visible="true" CssClass="defaultButton nodeButton" OnClick="btnInsertNode_Click" Text="Insert a ??? before this Node" ToolTip="Add a node below the current mode" CausesValidation="false"></asp:Button>
                                <asp:Button ID="btnAddChildNode" runat="server" Visible="true" CssClass="defaultButton nodeButton" OnClick="btnAddChildNode_Click" Text="Add Child Node" ToolTip="Adds a node of this same type before this node" CausesValidation="false"></asp:Button>
                                <asp:Button ID="btnAppendNode" runat="server" Visible="true" CssClass="defaultButton nodeButton" OnClick="btnAppendNode_Click" Text="Append a ??? after this Node" ToolTip="Adds a node of this same type after this node" CausesValidation="false"></asp:Button>
                            </asp:Panel>
                        </div>


                        <asp:Panel ID="relatedFiles" runat="server">
                            <h3 class="isleH3_Block">Related Files</h3>
                            <div id="nodeItems"></div>
                            <p>
                                <asp:Label CssClass="lblFileList" ID="lblFileList" Visible="true" runat="server"></asp:Label>
                            </p>


                        </asp:Panel>

                        
                        <asp:Panel ID="alignedStandardsPanel" runat="server">
                            <h3 class="isleH3_Block">Aligned Standards</h3>
                            <div id="standardItems"></div>
                            <p>
                                <asp:Label ID="lblStandardsList" runat="server"  CssClass="lblStandardsList" ></asp:Label>
                            </p>


                        </asp:Panel>


                    </asp:Panel>
                </div>

                <div class="contributePanel">
                    <h3 class="isleH3_SectionHdr">Upload Files</h3>
                    <asp:Literal ID="litFrame" runat="server"></asp:Literal>
                </div>
                </div>

                <div id="curriculumGuide">
                        <h3 class="isleH3_Block">Curriculum Guide</h3>
                        <h4>Overview</h4>
                        <div>
                            A curriculum consists of many 'nodes'. Examples of nodes, include: Module, Unit, Lesson, and Activity. Each node will consists of several attributes (described below). For each new node the author will usually do the following:
                            <ul>
                                <li>Provide a title and summary</li>
                                <li>Set the privacy level</li>
                                <li>Optionally add one or more documents</li>
                                <li><i>Publish</i> the node when all the necessary components have been added.<br />Note: a node will not be visible to the public until it has been <i>published</i>. This step is described later. For your convenience, if a node has not been published yet, the status will be appended to the node title in the curriculum outline, as well as of course being displayed in the content section.</li>
                            </ul>
                        </div>

                        <h4>Interface Components</h4>
                        <div>
                            The author view of the curriculum interface consists of three main sections:
                            <ul>
                                <li>The outline or tree view section</li>
                                <li>The content section</li>
                                <li>The upload files section.</li>
                            </ul>
                            <h5>Curriculum Outline Section</h5>
                            <div>As nodes are added to a curriculum, the outline section gets updated. The curriculum may be navigated by toggling the nodes to view the children nodes.</div>

                            <h5>Content Section</h5>
                            <div>The content section consists of:</div>
                            <ul>
                                <li>Content Type (Module, Unit, etc)</li>
                                <li>Status (indicates if published or in an editing status)</li>
                                <li>Title</li>
                                <li>Short summary of the node. This summary will appear in searches.</li>
                                <!--<li>Optional web content/details. If a node consists of documents such as pdfs, there may be no need to enter rich text that will dispaly as a web page. Alternately, if the preference is to have the main content of the node presented as a web page, then the user may use the Html editor to enter the rich content.</li>-->
                                <li>Privacy level</li>
                                <li>A series of action buttons, including:
                                    <ul>
                                        <li>Save {node} - to update the current node</li>
                                        <li>Delete {node} - to delete the current node</li>
                                        <li>Insert {node} - insert a node of the same type before the current node</li>
                                        <li>Add {child node} - insert a child node below the current node (for example to add a lesson below a unit)</li>
                                        <li>Add {node} - add a node of the same type after the current node</li>
                                    </ul>
                                </li>
                                <li>Related documents<br />	&nbsp;&nbsp;A list of all documents uploaded for the current node.</li>
                            </ul>

                            
                        <h5>Upload Files Section</h5>
                        <div><a id="uploadSection" href="#"></a>
                            <div>A version of the contribute tool is used to add related documents to the current node.</div>
                            <div>You should only use the <i>upload file</i> option (which is conveniently preselected). To contribute a resource, ensure upload a file is selected, then .</div>
                            <ul>
                                <li>Click on the Choose File button (exact title varies between browsers)</li>
                                <li>Navigate and select a file, and</li>
                                <li>Then click the Open button</li>
                            </ul>
                            <div>Next, complete the <b>Describe Your Resource</b> section by providing each of the following for the resource: </div>
                            <ul>
                                <li>Title</li>
                                <li>Description</li>
                                <li>Keyword (enter a keyword in the text box and then press enter to 'add' to the list)</li>
                                <li>Privacy level (Who can access this resource)</li>
                                <li>Optionally align standards to the resource</li>
                                <li>Subject</li>
                                <li>Grade Level</li>
                                <li>Career Cluster</li>
                            </ul>
                            <p>Optionally, you can have the resource automatically added to a library collection.</p>
                            <p>Finally, submit the resource to be published.</p>
                            <div>After publishing the resource, the contribute tool will offer to 'remember' your selections. The selected checkboxes and keywords can be used for the next resource upload - saving you a little time. These values are only remembered for the current node. They are lost when a new node is selected. </div>
                            <div>The system will update the node page with the document details as follows: <br /><img alt='' src="/Help/images/documentOptions.png" /><br />The document section includes four possible actions: 
                                <ul>
                                     <li><b>Edit</b> - will open the document editor in a new tab. Use this option to update the title, description, etc or upload a new version of the file.</li>
                                    <li><b>View</b> - view the physical document</li>
                                    <li><b>Tags</b> - navigate to the resource detail page to view/or update the tags</li>
                                    <li><b>Remove</b> - remove the document from the node</li>
                                </ul></div>
                        </div>
                        </div>
                    <h4>Publishing a Node</h4>
                    <div>In order to prevent work-in-progress from being displayed to the public, only published nodes and documents are displayed. Documents are published through the <a href="#uploadSection" >process discussed above</a>. Once a node is published, it can found via the resources search, added to libraries, etc. Note that only resources with a privilege of public will be available in the search. <br /> The status of a node appears above the title. <br /><img alt='' src="/Help/images/curriculumNodeStatus.png" /><br />Note, don't confuse the status of the node with the status of the curriculum - the curriculum as a whole has its own status which is displayed in the top right corner of the page. The latter image shows the lesson has a status of <b>In Progress</b>. If a node has not been published, the link: <span style="color: blue; font-weight:bold">Tag/publish Lesson</span>, will be displayed. It is displayed above the status and is used to publish the node. On click of this link:
                        <ul><li>The system will open the quick contribute tool in a new tab</li>
                            <li>The system will prefill the public resource URL,title, summary, and privacy level </li>
                            <li>You would then fill in the remaining fields as for a file upload, including:
                                <ul>
                                     <li>Adding Keywords (enter a keyword in the text box and then press enter to 'add' to the list)</li>
                                    <li>Optionally align standards to the resource</li>
                                    <li>Subject</li>
                                    <li>Grade Level</li>
                                    <li>Career Cluster</li>
                                    <li>Optionally have the resource added to a specified library collection</li>
                                </ul>
                            </li>
                        </ul>
                        <div>Press Submit and the resource to be published.</div>
                        <div>As for documents, the system will offer to 'remember' some of the selected values. This option will not be especially useful in this scenario.</div>
                    </div>
                    </div>
                
            </div>
        </div>

    </div>


</asp:panel>

<div id="templates" style="display:none;">
  
  <div id="template_nodeItem">
    <div class="nodeItem grayBox">
      <h3 class="isleH3_Block">{title}</h3>
      <p class="resourceLinks"><a href="{documentUrl}" target="resDetl2">View Resource</a> | <a href="{resourceUrl}" target="resDetl2">View This Resource's Tags</a></p>
        <p class="itemMessage">{message}</p>
    </div>
  </div>
</div>

<div id="loginMessage" runat="server">
  <h2 class="isleH2">You must be logged in and authorized in order to use this feature.</h2>
  <p>Only members of authorized organizations can create resource pages.</p>
</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
  <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false"></asp:Literal>
  <asp:Literal ID="txtFormSecurityName2" runat="server" Visible="false">IOER.controls.Authoring</asp:Literal>
  <asp:Literal ID="ltlTabGetter" runat="server" Visible="false">tabID</asp:Literal>
  <asp:Literal ID="ltlBasicTabName" runat="server" Visible="false">basicInfo</asp:Literal>
  <asp:Literal ID="ltlWebContentTabName" runat="server" Visible="false">webcontent</asp:Literal>
  <asp:Literal ID="ltlAttachmentsTabName" runat="server" Visible="false">attachments</asp:Literal>
  <asp:Literal ID="ltlReferencesTabName" runat="server" Visible="false">references</asp:Literal>
  <asp:Literal ID="ltlFileContentTabName" runat="server" Visible="false">fileContent</asp:Literal>
  <asp:Literal ID="ltlCurriculumTabName" runat="server" Visible="false">curriculum</asp:Literal>

  <asp:Literal ID="ltlMinTxtTitleLength" runat="server" Visible="false">10</asp:Literal>
  <asp:Literal ID="ltlMinTxtDescriptionLength" runat="server" Visible="false">20</asp:Literal>
  <asp:Literal ID="ltlMinUsageRightsURLLength" runat="server" Visible="false">15</asp:Literal>
  <asp:Literal ID="litPreviewUrlTemplate2" runat="server" Visible="false">/Repository/ResourcePage.aspx?rid={0}</asp:Literal>
  <asp:Literal ID="litPreviewUrlTemplate" runat="server" Visible="false">/Content/{0}/{1}</asp:Literal>
    <asp:Literal ID="showingTemplates" runat="server" Visible="false">no</asp:Literal>

  <asp:Literal ID="ltlAppliedAttachmentTemplate" runat="server" Visible="false"><h3 class="isleH3_Block"><span class="attachmentTitle">{0} ({2})</span><a class="textLink" href="javascript:removeAttachment('{3}')">Remove</a><a class="textLink" href="javascript:editAttachment('{3}')">Edit</a> <a class="textLink" href="{4}" target="_blank">View</a></h3><p>{1}</p></asp:Literal>
</asp:Panel>

<asp:Panel ID="nodesHiddenPanel" runat="server" Visible="false">
<asp:Literal ID="litNodeViewTemplate" runat="server" Visible="false"><a class="textLink" style="margin-left:10px;" href="/Content/{1}/{2}" target="resDetl2">View {3} Separately</a></asp:Literal>

<asp:Literal ID="litNodePublishTemplate" runat="server" Visible="false"><a class="textLink" style="margin-left:10px;" href="/Contribute/?mode=tag&contentId={0}&doingLRPublish=no" target="resDetl2">Tag/publish {1}</a></asp:Literal>
<asp:Literal ID="litNodeViewTags" runat="server" Visible="false"><a class="textLink" style="margin-left:10px;" href="/Resource/{0}/{1}" target="resDetl2">View tags for {2}</a></asp:Literal>

<asp:Literal ID="litNodeEditTemplate" runat="server" Visible="false"><a class="textLink" style="margin-left:10px;" href="/My/Author.aspx?rid={0}" target="resDetl2">Edit {3} Separately</a></asp:Literal>

  <asp:Literal ID="ltlNodeAttachmentTemplate" runat="server" Visible="false">
      <div style="border-radius: 5px; border: solid 2px #f5f5f5; background-color: #f6f6f6; padding: 5px; margin-bottom: 8px;" >
      <h3 style="padding-top: 3px; padding-bottom: 5px;" >{0}</h3>
      <div style="margin-left: 10px;">{1}
      <ul class="navlist">
      <li><a class="textLink" href="/My/DocumentEditor.aspx?rid={2}" target="_blank">Edit</a> </li>
      <li><a class="textLink" href="{4}" target="_blank">View</a></li>
      <li><a class="textLink"  href="{5}" target="_blank">Tags</a></li>
      <li><a class="textLink"  href="javascript:removeNodeAttachment('{3}')">Remove</a></li>
    </ul></div></div></asp:Literal>
    
  <asp:Literal ID="ltlAppliedReferencesHeader" runat="server" Visible="false"><h3 class="isleH3_Block">Resource <a class="textLink" href="javascript:removeReference('{0}')">Remove</a><a class="textLink" href="javascript:editReference('{0}')">Edit</a> </h3></asp:Literal>
  <asp:Literal ID="ltlAppliedReferencesTemplate" runat="server" Visible="false"><p><b>{0}:</b> {1}</p></asp:Literal>
  <asp:Literal ID="ltlReferenceColumns" runat="server" Visible="false">Title,Author,Publisher,AdditionalInfo,ISBN,ReferenceUrl</asp:Literal>
  <asp:Literal ID="ltlReferenceColumnsDisplay" runat="server" Visible="false">Title,Author,Publisher,Additional Information,ISBN,Resource URL</asp:Literal>
  <asp:Literal id="previewLink" runat="server" Visible="false" />

<asp:Literal ID="docLinkTemplate" runat="server"><a href="{0}" target="_blank">{1}</a></asp:Literal>


  <asp:Literal ID="standardsTemplate" runat="server" Visible="false">
      <div style="border-radius: 5px; border: solid 2px #f5f5f5; background-color: #f6f6f6; padding: 5px; margin-bottom: 8px;" >
      <h3 style="padding-top: 3px; padding-bottom: 5px;" >{0}</h3>
      <div style="margin-left: 10px;">{1}
      <ul class="navlist">
      <li>Alignment: {2}</li>
      <li>Usage: {3}</li>
      <li><a class="textLink"  href="#">Remove {4} - future</a></li>
    </ul></div></div></asp:Literal>
</asp:Panel>

