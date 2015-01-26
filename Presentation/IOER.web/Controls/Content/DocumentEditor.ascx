<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="DocumentEditor.ascx.cs" Inherits="ILPathways.Controls.Content.DocumentEditor" %>

<%@ Register TagPrefix="uc1" TagName="ConditionsOfUseSelector" Src="/LRW/controls/ConditionsOfUseSelector.ascx" %>


<script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />

<script type="text/javascript" language="javascript">
    //From server
    <%=userProxyId %>

    $(document).ready(function () {

        //Track textbox lengths
        $(".textTracker").on("keyup change", function () {
            updateTextLengths($(this), $("#" + $(this).attr("trackerName")));
        });
        $(".textTracker").change();

    }); //End document.ready


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

    //-->
</script>

<style type="text/css">

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
    width: 520px;
  }
   #statusDiv .column {
    width: 150px;
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
    width: 90%;
  }
  select.ddl {
    width: 100%;
  }
  .buttons {
    text-align: center;
    padding: 10px;
  }
  .nodeButton {width: 350px; }

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

  .publishButton {
  padding: 5px;
  background-color: #FF5707;
  cursor: pointer;
}

    .toolTipLink ul { padding: 2px 5px; }
</style>



<h1 class="isleH1">IOER Resource Authoring Tool</h1>
<asp:panel ID="containerPanel" runat="server">

<div id="Stage1Items" runat="server">
    <div id="statusDiv" style="display: inline-block; width: 100px;" >
    <a href="/My/Authored.aspx">Back to Search</a>
</div>
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
           <h3 class="isleH3_Block required">Status <a class="toolTipLink" id="tipStatus" title="Status|Current status of this resource:<ul><li><strong>In Progess</strong>: content is under construction, the content is not public.</li><li><strong>Submitted</strong>: Content authored on behalf of an organization requires approval. Status of Submitted indicates approval is pending.</li><li><strong>Requires Revision</strong>: For content submitted for approval, a status of Requires Revision indicates changes have been requested before content will be approved.</li><li><strong>Published</strong>: the content is now visible to the public (dependent on the privacy settings)</li><li><strong>Inactive</strong>: author has set content to inactive. An inactive item will no longer be available to the searches, libraries, etc.</li></ul>"><img src="/images/icons/infoBubble.gif" alt="" /> </a></h3>
        <asp:Label ID="lblStatus" runat="server" >Draft</asp:Label>

      <h3 class="isleH3_Block required">Title<a class="toolTipLink" id="tipTitle" title="Title|The name or title of the resource"><img src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_Title" minLength="10"></span></h3>
      <asp:TextBox ID="txtTitle" class="textBox textTracker" trackerName="lt_Title" runat="server" />

      <h3 class="isleH3_Block required">Description<a class="toolTipLink" id="tipDescription" title="Description|A brief description of the resource. This field will be displayed in search results and the resource detail page as well as in the Learning Registry metadata if you choose to Publish it."><img src="/images/icons/infoBubble.gif" alt="" /></a> <span class="lengthTracker" id="lt_Description" minLength="25"></span></h3>
      <asp:TextBox ID="txtDescription" class="textBox textTracker" trackerName="lt_Description" Rows="5" TextMode="MultiLine"  runat="server" />
    

      <h3 class="isleH3_Block required">Select Usage Rights<a class="toolTipLink" id="tipUsageRights" title="Usage Rights|The URL where the owner specifies permissions for using the resource.|<b>Remix and Share:</b> You may adapt, edit, or tweak resource before using and sharing.|<b>Share Only:</b> You may copy, distribute or transmit the resource in its original form only.|<b>No Strings Attached:</b> No restrictions are placed on usage.|<b>Read the Fine Print:</b> Specific restrictions may be in place; read usage rights carefully.|<b>Attribution:</b> You must attribute the work in the manner specified by the author or licensor."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <uc1:ConditionsOfUseSelector ID="conditionsSelector" runat="server" />

      <h3 class="isleH3_Block required">Who can access this resource?<a class="toolTipLink" id="tipPrivilege" title="Access Privilege|You can allow only specific groups of users to access this resource by selecting the appropriate group from the box below."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlPrivacyLevel" CssClass="ddl" runat="server"></asp:DropDownList>

      <h3 class="isleH3_Block required">This resource is being Authored on behalf of...<a class="toolTipLink" id="tipOrganization" title="Authoring|If you are authoring this resource for your own purposes, select 'Myself'. Otherwise, select the appropriate organization from the box below."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
      <asp:DropDownList ID="ddlIsOrganizationContent" runat="server" CssClass="ddl" >
        <asp:ListItem Value="1" Text="Myself"></asp:ListItem>
        <asp:ListItem Value="2" Text="My Organization"></asp:ListItem>
      </asp:DropDownList>

      <div class="buttons">
        <asp:Button ID="btnSave" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnSave_Click" Text="Save" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnDelete" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnDelete_Click"  Text="Delete" CausesValidation="false"></asp:Button>

        <asp:Button ID="btnUnPublish" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnUnPublish_Click" Text="Un-Publish" CausesValidation="false"></asp:Button>
        <asp:Button ID="btnSetInactive" runat="server" Visible="false" CssClass="defaultButton" OnClick="btnSetInactive_Click" Text="Set Inactive" CausesValidation="false"></asp:Button>

<asp:button id="btnPublish" runat="server" Visible="false" CssClass="defaultButton publishButton" OnCommand="btnPublish_Click" CommandName="PublishUpdate" Text="Publish" causesvalidation="false"></asp:button>

    <br />
                    <asp:button id="btnTestPostback" runat="server" Visible="false" CssClass="defaultButton publishButton" OnCommand="btnTestPostback_Command" Text="Test tbutton" causesvalidation="false"></asp:button>
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
                  <h2 class="isleH2">Content File</h2>
            <div>
                You may only replace the exising file with the same name.
                <br />
                If you want to use a new file, then this content should be set inactive/deleted, and then you will need to create a new file resource <a href="/Contribute/?mode=upload">using the quick contribute tool</a>
            </div>

            <div class="column">
                <asp:Panel ID="docKeyPanel2" runat="server" Visible="false">
                    Document ID&nbsp;<asp:TextBox ID="txtDocumentRowId2" runat="server">0</asp:TextBox>
                    Doc url&nbsp;<asp:Label ID="fileContentUrl" runat="server">0</asp:Label>
                </asp:Panel>

                <asp:Panel ID="contentFilePanel" runat="server" Visible="true">
                    <h3 class="isleH3_Block">
                        <asp:Literal ID="contentFileName" runat="server"></asp:Literal></h3>
                    <div>
                        <asp:Label ID="contentFileDescription" runat="server" Visible="true"></asp:Label>
                    </div>
                    <asp:HyperLink ID="linkContentFile" runat="server" Target="_blank" Visible="true">View File</asp:HyperLink>

                </asp:Panel>


                <h3 class="isleH3_Block required">Select a replacement file to attach<a class="toolTipLink" id="A5" title="Attachment|Select a file to upload to replace the current file."><img src="/images/icons/infoBubble.gif" alt="" /></a></h3>
                <asp:FileUpload ID="contentFileUpload" runat="server" />

                <div class="buttons">
                    <asp:Button ID="btnUploadNewContentFile" runat="server" Visible="true" CssClass="defaultButton" OnClick="btnContentFileUpload_Click" Text="Save/Replace File" ToolTip="Saves the current attachment and ensures the same file name is used" CausesValidation="false"></asp:Button>

                </div>

            </div>
    </div>
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

  <asp:Literal ID="ltlFileContentTabName" runat="server" Visible="false">fileContent</asp:Literal>
  
  <asp:Literal ID="ltlMinTxtTitleLength" runat="server" Visible="false">10</asp:Literal>
  <asp:Literal ID="ltlMinTxtDescriptionLength" runat="server" Visible="false">20</asp:Literal>
  <asp:Literal ID="ltlMinUsageRightsURLLength" runat="server" Visible="false">15</asp:Literal>
  <asp:Literal ID="litPreviewUrlTemplate2" runat="server" Visible="false">/Repository/ResourcePage.aspx?rid={0}</asp:Literal>
  <asp:Literal ID="litPreviewUrlTemplate" runat="server" Visible="false">/Content/{0}/{1}</asp:Literal>
    <asp:Literal ID="showingTemplates" runat="server" Visible="false">no</asp:Literal>


  <asp:Literal id="previewLink" runat="server" Visible="false" />

<asp:Literal ID="docLinkTemplate" runat="server"><a href="{0}" target="_blank">{1}</a></asp:Literal>
</asp:Panel>





