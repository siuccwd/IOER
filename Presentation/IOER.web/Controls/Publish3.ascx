<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Publish3.ascx.cs" Inherits="ILPathways.Controls.Publish3" %>
<%@ Register TagPrefix="custom" TagName="StandardsBrowser" Src="/Controls/StandardsBrowser7.ascx" %>
<%@ Register TagPrefix="custom" TagName="UsageRightsSelector" Src="/LRW/Controls/ConditionsOfUseSelector.ascx" %>
<%@ Register TagPrefix="custom" TagName="CheckBoxList" Src="/LRW/Controls/ListPanel.ascx" %>


<div id="errorMessage" runat="server" visible="false">
  <p style="margin: 100px auto;">You don't have permission to tag Resources.</p>
</div>
<div id="tagger" runat="server">
  <link rel="stylesheet" type="text/css" href="/styles/tooltipv2.css" />
  <script type="text/javascript" src="/scripts/tooltipv2.js"></script>

  <script type="text/javascript">
      <%=myLibrariesString %>
      <%=orgData %>
      <%=selectedOrgOutput %>
  </script>
  <script type="text/javascript" src="/scripts/publish3.js"></script>

  <style type="text/css">
    #columns { white-space: nowrap; font-size: 16px; }
    #columns .column { white-space: normal; display: inline-block; vertical-align: top; margin-right: -4px; padding: 5px; }
    #columns #column1 { width: 40%; }
    #columns #column2, #columns #column3 { width: 30%; }
    #finish { display: block; width: 100%; }

    .column > .section { display: block; margin-bottom: 10px; position: relative; }
    .column > .section > span { display: block; font-weight: bold; font-size: 120%; }
    .column > .section > input, .column > .section > textarea, .column > .section > select { width: 100%; margin-bottom: 5px; }
    .column > .section > textarea { height: 5em; resize: none; }
    .column > .section.required > span:after { content: " Required"; color: #D33; margin-left: 20px; font-weight: normal; font-style: italic; font-size: 80%; opacity: 0.7; }

    .listPanel ul { list-style-type: none; }
    .listPanel ul li span { display: block; position: relative; }
    .listPanel ul li label { display: block; padding-left: 25px; border-radius: 5px; cursor: pointer; }
    .listPanel ul li label:hover { background-color: #EAEAEA; }
    .listPanel ul li input { position: absolute; top: 2px; left: 2px; }
    .listPanel select { width: 100%; }
    .listPanel ul li input[type=button] { position: static; width: 100%; }

    .toolTipContent { font-weight: normal; }

    #finish input { width: 100%; }

    .addedKeyword { display: inline-block; padding: 1px 2px; margin: 1px 2px; background-color: #EEE; border-radius: 5px; }
    .addedKeyword a { display: inline-block; float: right; height: 20px; width: 20px; text-align: center; line-height: 20px; color: #FFF; font-weight: bold; background-color: #D33; border-radius: 5px; margin-left: 5px; }
    .addedKeyword a:hover, .addedKeyword a:focus { background-color: #F00; }

    #content #SB7 #SBddlBox select { display: block; width: 100%; margin-bottom: 5px; }
    #content #SB7 #SBleftColumn { width: 65%; }
    #content #SB7 #SBrightColumn { width: 34.5%; }
    #content #SB7 #SBselected .selectedStandard select { display: block; width: 100%; }

    .vm { font-style: italic; max-height: 3em; transition: max-height 0.5s, color 1s; -webkit-transition: max-height 0.5s, color 1s; color: #FFF; }
    .vm:empty { max-height: 0; }
    .vm.gray { color: #555; }
    .vm.red { color: #D33; }
    .vm.green { color: #FFF; }
    .vm.green:after { content: "✓"; position: absolute; top: 2px; right: 2px; background-color: #1A1; border-radius: 20px; display: block; width: 20px; height: 20px; text-align: center; line-height: 20px; font-weight: bold; box-sizing: border-box; padding-right: 2px; }

    #btnFinish { padding: 5px; font-size: 130%; }

    @media screen and (max-width: 1500px) {
      /*#columns .column { width: 33.333332%; }*/
    }

    @media screen and (max-width: 1100px) {
      #columns { white-space: normal; }
      #columns #column2, #columns #column3 { width: 49.5%; margin-right: -4px; }
      #columns .column#column1 { width: 100%; }
    }

    @media screen and (max-width: 650px) {
      #content #columns .column { width: 100%; display: block; }
      #content .listPanel ul li label { padding: 2px 5px 2px 25px; }
      #content .listPanel ul li input { top: 5px; }
    }
  </style>

  <h1 class="isleH1">ISLE Resource Tagger</h1>
  <p>This tool will let you quickly and thoroughly tag a Resource.</p>

  <div id="columns">

    <div class="column" id="column1">
      <div class="section required validation_url" data-minLength="12" data-name="URL" data-unique="true">
        <span>Resource URL</span>
        <input type="text" id="txtURL" runat="server" placeholder="http://" />
        <div class="vm"></div>
      </div>
      <div class="section required validation_text" data-minLength="10" data-name="Title">
        <span>Resource Title</span>
        <input type="text" id="txtTitle" runat="server" />
        <div class="vm"></div>
      </div>
      <div class="section required validation_text" data-minLength="25" data-name="Description">
        <span>Description</span>
        <textarea id="txtDescription" runat="server" />
        <div class="vm"></div>
      </div>
      <div class="section required validation_keywords" data-minLength="1" data-name="Keywords">
        <span>Keywords <a class="toolTipLink" title="Keywords|Type a keyword or phrase and press Enter to add it.|Please use meaningful and unique keywords/phrases."></a></span>
        <input type="text" id="txtKeyword" placeholder="Type a keyword or phrase and press Enter." />
        <div class="vm"></div>
        <div id="enteredKeywords"></div>
        <input type="hidden" id="hdnKeywords" class="hdnKeywords" runat="server" />
      </div>
      <div class="section required">
        <span>Language <a class="toolTipLink" title="Language|The primary language the Resource itself is in--not, for example, the language it teaches."></a></span>
        <custom:CheckBoxList ID="ddlLanguage" runat="server" TargetTable="language" UpdateMode="raw" ListMode="dropdown" />
        <div class="vm green"></div>
      </div>
      <div class="section required">
        <span>Access Rights <a class="toolTipLink" title="Access Rights|These define how you access a Resource, including any restrictions on doing so."></a></span>
        <custom:CheckBoxList ID="ddlAccessRights" runat="server" TargetTable="accessRights" UpdateMode="raw" ListMode="dropdown" />
        <div class="vm green"></div>
      </div>
      <div class="section required validation_url" data-minLength="12" data-name="Usage Rights" data-unique="false">
        <span>Usage Rights <a class="toolTipLink" title="Usage Rights|These define what you can do with a Resource, including whether or not you can redistribute it (or a version of it)."></a></span>
        <custom:UsageRightsSelector ID="usageRightsSelector" runat="server" />
        <div class="vm"></div>
      </div>
      <div class="section validation_text" data-minLength="0" data-name="Creator">
        <span>Creator <a class="toolTipLink" title="Creator|The original creator of the Resource itself."></a></span>
        <input type="text" id="txtCreator" runat="server" />
        <div class="vm"></div>
      </div>
      <div class="section validation_text" data-minLength="0" data-name="Publisher">
        <span>Publisher <a class="toolTipLink" title="Publisher|The person or entity that makes the Resource available to the world."></a></span>
        <input type="text" id="txtPublisher" runat="server" />
        <div class="vm"></div>
      </div>
      <div class="section validation_text" data-minLength="0" data-name="Requirements">
        <span>Requirements <a class="toolTipLink" title="Requirements|Any hardware, software, equipment, or other prerequisite items needed to use this Resource."></a></span>
        <input type="text" id="txtRequirements" runat="server" />
        <div class="vm"></div>
      </div>
      <div class="section">
        <span>Learning Standards</span>
        <custom:StandardsBrowser ID="standardsBrowser" runat="server" />
        <input type="hidden" class="hdnStandards" runat="server" id="hdnStandards" />
      </div>

        <div class="section">
        <span>Publishing Resource for myself or an Organization</span>
        <p>This step is optional. If publishing the resource for yourself do nothing. If publishing for an organization, select the applicable one below. Only organizations where you are a member will be displayed.</p>
        <select id="ddlOrg" name="ddlOrg">
        </select>
       
      </div>

      <div class="section">
        <span>Library & Collection</span>
        <p>You can specify a Library and Collection to add this Resource to immediately after submission.</p>
        <select id="ddlLibrary" name="ddlLibrary">
          <option value="0" selected="selected">Select a Library...</option>
        </select>
        <select id="ddlCollection" name="ddlCollection">
          <option value="0" selected="selected">Select a Collection...</option>
        </select>
      </div>
    </div><!-- /column -->
    <div class="column" id="column2">
      <div class="section required validation_cbxl" data-minLength="1" data-name="Resource Type">
        <span>Resource Type</span>
        <div class="vm"></div>
        <custom:CheckBoxList ID="cbxlResourceType" runat="server" TargetTable="resourceType" UpdateMode="raw" />
      </div>
      <div class="section required validation_cbxl" data-minLength="1" data-name="Media Type">
        <span>Media Type</span>
        <div class="vm"></div>
        <custom:CheckBoxList ID="cbxlMediaType" runat="server" TargetTable="mediaType" UpdateMode="raw" />
      </div>
      <div class="section">
        <span>K-12 Subject</span>
        <custom:CheckBoxList ID="cbxlK12Subject" runat="server" TargetTable="subject" UpdateMode="raw" />
      </div>
      <div class="section">
        <span>Educational Use</span>
        <custom:CheckBoxList ID="cbxlEducationalUse" runat="server" TargetTable="educationalUse" UpdateMode="raw" />
      </div>
    </div><!-- /column -->
    <div class="column" id="column3">
      <div class="section">
        <span>Career Cluster</span>
        <custom:CheckBoxList ID="cbxlCareerCluster" runat="server" TargetTable="careerCluster" UpdateMode="raw" />
      </div>
      <div class="section">
        <span>Grade Level</span>
        <custom:CheckBoxList ID="cbxlGradeLevel" runat="server" TargetTable="gradeLevel" UpdateMode="raw" />
      </div>
      <div class="section">
        <span>End User</span>
        <custom:CheckBoxList ID="cbxlEndUser" runat="server" TargetTable="endUser" UpdateMode="raw" />
      </div>
      <div class="section">
        <span>Group Type</span>
        <custom:CheckBoxList ID="cbxlGroupType" runat="server" TargetTable="groupType" UpdateMode="raw" />
      </div>
      <div class="section" id="worknet_qualify" runat="server" visible="false">
        <span>workNet: Qualify</span>
        <div class="listPanel" id="cbxlWorknetQualify">
          <ul>
            <li><span><input type="checkbox" name="wnQualify1" id="wnQualify1" value="Prepare Resumes" /> <label for="wnQualify1">Prepare Resumes</label></span></li>
            <li><span><input type="checkbox" name="wnQualify2" id="wnQualify2" value="Navigate the Hiring Process Guide" /> <label for="wnQualify2">Navigate the Hiring Process Guide</label></span></li>
            <li><span><input type="checkbox" name="wnQualify3" id="wnQualify3" value="Employer Expectations Checklist (workplace skills, computer skills, and job specific skills)" /> <label for="wnQualify3">Employer Expectations Checklist (workplace skills, computer skills, and job specific skills)</label></span></li>
            <li><span><input type="checkbox" name="wnQualify4" id="wnQualify4" value="Advance in your Career" /> <label for="wnQualify4">Advance in Your Career</label></span></li>
            <li><span><input type="checkbox" name="wnQualify5" id="wnQualify5" value="Credentials and Assessments" /> <label for="wnQualify5">Credentials and Assessments</label></span></li>
            <li><span><input type="checkbox" name="wnQualify6" id="wnQualify6" value="Get Work Experience" /> <label for="wnQualify6">Get Work Experience</label></span></li>
            <li><span><input type="checkbox" name="wnQualify7" id="wnQualify7" value="Volunteer" /> <label for="wnQualify7">Volunteer</label></span></li>
          </ul>
        </div>
      </div>
    </div><!-- /column -->
    <div id="finish">
      <input type="button" id="btnFinish" class="isleButton bgGreen" value="Finish!" onclick="validateAndPublish()" />
    </div>
  </div><!-- /columns -->

  <div id="templates" style="display:none;">
    <div id="template_addedKeyword">
      <div class="addedKeyword" id="keyword_{id}"><a href="#" onclick="removeKeyword({id}); return false;">X</a> {word} </div>
    </div>
  </div>
</div>

<asp:Literal ID="createdContentItemId" runat="server" visible="false">0</asp:Literal>