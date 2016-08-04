<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LR_Detail.ascx.cs" Inherits="ILPathways.LRW.controls.LR_Detail" %>
<%@ Register TagPrefix="uc1" TagName="LoginPanel" Src="/Account/controls/LoginController.ascx" %>
<%@ Register
    Assembly="AjaxControlToolkit"
    Namespace="AjaxControlToolkit.HTMLEditor"
    TagPrefix="HTMLEditor" %>

<style type="text/css">
h2 {font-size: 85%; margin-bottom: 2px; }
.topHdg {margin-top: 0px; }
.left { float: left; }
.rightSide { float: left; width: 300px; padding-left: 8px; border-left: 1px solid #000; margin-left: -1px; }
.thumb { width: 210px; height: 158px; }
.details { float: left; width: 670px; min-height: 500px; border-right: 1px solid #000; word-wrap: break-word; padding: 5px; }
.details h2 { margin: 5px 0 1px 0; }
.details p { margin: 0; padding: 2px 5px; }
.clearFloat { clear: both; }

.rightSide h3 {
	background: url(/images/icons/open.png) no-repeat 0 11px;
	padding: 10px 0 0 25px;
	cursor: pointer;
}
.rightSide h3.close {
	background-image: url(/images/icons/close.png);
}
.collapsibleDiv {
	margin-left: 5px;	
}
</style>

<script type="text/javascript">
<!--
  var $lrs2 = jQuery.noConflict();
  $lrs2(document).ready(function () {
    $('.collapsibleDiv').hide();
    $('.rightSide h3').toggle(
  		function () {
  		  $(this).next('.collapsibleDiv').slideDown();
  		  $(this).addClass('close');
  		},
  		function () {
  		  $(this).next('.collapsibleDiv').slideUp();
  		  $(this).removeClass('close');
  		}
  	); // end toggle
  }); // end ready

  $(document).ready(function () {
    $('div.slide').next().css('display', 'none').end().click(function () {
      $(this).next().toggle('fast');
    });
  });
//-->	
</script>

<meta itemprop="url" content="<%=CurrentRecord.ResourceUrl %>" />
<meta itemprop="keywords" content="<%=CurrentRecord.Keywords %>" />

<link href="/fancybox/jquery.fancybox-1.3.4.css" rel="stylesheet" />
<script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.6.4/jquery.min.js"></script>
<script type="text/javascript" src="/scripts/jquery.easing.1.3.js"></script>
<script type="text/javascript" src="/fancybox/jquery.fancybox-1.3.4.min.js"></script>
<script type="text/javascript">
  $(document).ready(function () {
    $('.iframe').fancybox({
      width: '90%',
      height: '90%',
      titlePosition: 'outside'
    });
  }); // end ready
</script>

<script type="text/javascript" language="javascript">
    //Check to see if the current record resource URL is valid and displayable (ie not a file), and if so, show it via ABCdraw; if not, display some default text/image/etc
  $(document).ready(function () {
    //If the URL exists...
    if ("<%=urlStatusCode %>" == "200") { //URL is okay
      //Get the targetURL
      var url = "<%=CurrentRecord.ResourceUrl %>";
      //Decide which things are[n't] valid
      var invalidStuff = Array("pdf", "zip", "swf", "doc", "ppt", "xls", "docx", "pptx", "xlsx", "mov", "wmv", "avi", "mp3", "mp4", "ogg");

      //Find out what the URL's file is
      var urlParts = url.split("/");
      var fileandParams = urlParts[urlParts.length - 1].split("?");
      var fileParts = fileandParams[0].split(".");
      var fileType = fileParts[fileParts.length - 1];

      //Check the file against the [in]validStuff array
      for (var i = 0; i < invalidStuff.length; i++) {
        if (fileType == invalidStuff[i]) {
          //Invalid item. Pick a default
          $("#abcDrawIframe").css("display", "none");
          $("#abcDrawIframe").parent().append("<div id=\"noPreview\">Click to Preview</div>");
        }
        else {
          //Valid item. feed it to ABCDraw.
          $("#abcDrawIframe").attr("src", "http://edit.illinoisworknet.com/ABCDrawHTML/generator.asp?url=<%=CurrentRecord.ResourceUrl %>&ow=1024&oh=780&fw=200&fh=148");
        }
      }
    }
    else if ("<%=urlStatusCode %>" == "404" || "<%=urlStatusCode %>" == "403") {
      $("#abcDrawIframe").css("display", "none");
      $("#abcDrawIframe").parent().append("<div id=\"noPreview\">Invalid URL</div>");
    }
    else {
      $("#abcDrawIframe").css("display", "none");
      $("#abcDrawIframe").parent().append("<div id=\"noPreview\">Click to Preview</div>");
    }

  });
</script>

<style type="text/css">
#previewFrame
{
    float: right;
    margin: 5px;
}
#abcDrawIframe, #noPreview
{
    width: 210px;
    height: 158px;
}
#noPreview 
{
    background-color: #F5F5F5;
    text-align: center;
    line-height: 158px;
    /*display: table-cell;
    vertical-align: middle;*/
}
.iframe
{
    text-decoration: none;
}
.collapsibleDiv input 
{
  margin: 2px 5px;
}

</style>

<asp:Panel ID="detailPanel" Visible="true" runat="server" >
</asp:Panel>

  <div class="details">
    <h1 itemprop="name">
          <asp:Label ID="lblTitle" runat="server"></asp:Label>     
    </h1>
    <div id="previewFrame">
        <asp:Panel ID="abcPreview" CssClass="thumb" runat="server" Visible="false">
            <a href="<%=CurrentRecord.ResourceUrl %>" class="iframe" >
            <iframe id="abcDrawIframe" src="http://edit.illinoisworknet.com/ABCDrawHTML/generator.asp?url=<%=CurrentRecord.ResourceUrl %>&ow=1024&oh=780&fw=200&fh=148"></iframe></a>
        </asp:Panel>
        <asp:Panel ID="tmbPanel" runat="server" CssClass="thumb" Visible="false">
            <img src="/images/Stem/iPathways-left.jpg" width="200px" height="200px" alt="Popup target Url" />
        </asp:Panel>
    </div>

    <h2 class="topHdg">Subjects:</h2>
      <p><asp:Label ID="lblSubjects" runat="server"></asp:Label></p>
    <h2>Education Level(s):</h2>
      <p><asp:Label ID="lblEducationLevels" runat="server"></asp:Label></p>
    <h2>Publisher:</h2>
      <p><asp:Label ID="lblPublisher" runat="server"></asp:Label></p>
    <h2>Creator(s):</h2>
      <p><asp:Label ID="lblCreator" runat="server"></asp:Label></p>
    <h2>Access Rights:</h2>
      <p><asp:Label ID="lblAccessRights" runat="server"></asp:Label></p>
    <h2>Conditions of Use:</h2>
      <p><asp:Label ID="lblConditions" runat="server"></asp:Label></p>
    <h2>Abstract:</h2>
      <p itemprop="description"><asp:Label ID="lblDescription" runat="server"></asp:Label></p>
    <h2>Source Url:</h2>
      <p><a href="<%=CurrentRecord.ResourceUrl %>" target="_blank" title="Opens in new window"><asp:Label ID="lblResourceUrl" runat="server"></asp:Label></a></p>
    <h2>Language(s):</h2>
      <p><asp:Label ID="lblLanguageList" runat="server"></asp:Label></p>
    <h2>Intended Audience:</h2>
      <p><asp:Label ID="lblAudienceList" runat="server"></asp:Label></p>
    <h2>Material Type(s):</h2>
      <p><asp:Label ID="lblResourceTypesList" runat="server"></asp:Label></p>
    <h2>Keywords:</h2>
      <p><asp:Label ID="lblKeywords" runat="server"></asp:Label></p>

  </div>

<div class="rightSide">
<asp:Panel ID="pnlTaggingAndComments" runat="server" >

<asp:UpdatePanel runat="server" ID="UpdatePanel1" UpdateMode="Conditional">
  <ContentTemplate>

  <asp:Panel ID="clusterPanel" runat="server">
  	<!-- Cluster -->
  <h3>Clusters</h3>

  <div class="collapsibleDiv">   		
  <wcl:skmCheckBoxList ID="cbxlCluster" runat="server"  RepeatColumns="1" 
      AutoPostBack="true"
				AccessiblePrefix=""  Visible="true" 
      onselectedindexchanged="cbxlCluster_SelectedIndexChanged"></wcl:skmCheckBoxList>		

	   	<asp:DropDownList ID="ddlCluster" runat="server" Width="350px" Visible="false"  
   ></asp:DropDownList>

  </div>
	</asp:Panel> 
	  </ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel runat="server" ID="UpdatePanel2" UpdateMode="Conditional">
  <ContentTemplate> 
    <asp:Panel ID="gradeLevelsPanel" runat="server">
  	<!-- grade levels -->
  <h3>Grade Levels</h3>
  <div class="collapsibleDiv">   		
  <wcl:skmCheckBoxList ID="cbxlGradeLevels" runat="server"  RepeatColumns="1" 
      AutoPostBack="true" AccessiblePrefix=""  Visible="true" 
      onselectedindexchanged="cbxlGradeLevels_SelectedIndexChanged"></wcl:skmCheckBoxList>		

  </div>
	</asp:Panel>  	  
  </ContentTemplate>
</asp:UpdatePanel>



<asp:UpdatePanel runat="server" ID="UpdatePanel3" UpdateMode="Conditional">
  <ContentTemplate>
  <asp:Panel ID="intendedAudiencePanel" runat="server">
  	<!-- Intended Audience -->
  <h3>Intended Audience</h3>
  <div class="collapsibleDiv">	
  <wcl:skmCheckBoxList ID="cbxlIntendedAudience" runat="server"  RepeatColumns="1" 
      AutoPostBack="true" AccessiblePrefix=""  Visible="true" 
      onselectedindexchanged="cbxlIntendedAudience_SelectedIndexChanged"></wcl:skmCheckBoxList>
<asp:Button ID="Button1" OnClick="btnApplyAudienceUpdates_Click" runat="server" Text="Apply" CssClass="defaultButton" />
  
  <div class="clearFloat"></div>		
	</asp:Panel> 
  </ContentTemplate>
</asp:UpdatePanel>

<asp:UpdatePanel runat="server" ID="UpdatePanel4" UpdateMode="Conditional">
  <ContentTemplate> 
    <asp:Panel ID="educationUsePanel" runat="server">
  	<!-- cbxEducationUse -->
  <h3>Education Use</h3>
  <div class="collapsibleDiv">   		
  <wcl:skmCheckBoxList ID="cbxEducationUse" runat="server"  RepeatColumns="1" 
      AutoPostBack="true" AccessiblePrefix=""  Visible="true" 
      onselectedindexchanged="cbxEducationUse_SelectedIndexChanged"></wcl:skmCheckBoxList>		

  </div>
	</asp:Panel>  	  
  </ContentTemplate>
</asp:UpdatePanel>


  <asp:Panel ID="ratingPanel" runat="server">
  <h3>Rating</h3>
  <div class="collapsibleDiv" style="vertical-align:top;">
      <ajaxToolkit:Rating ID="ItemRating" runat="server"
          CurrentRating="2"
          MaxRating="5"
          StarCssClass="ratingStar"
          WaitingStarCssClass="savedRatingStar"
          FilledStarCssClass="filledRatingStar"
          EmptyStarCssClass="emptyRatingStar"
          OnChanged="ItemRating_Changed"
          style="float: left;" />
      </div>
</asp:Panel>
<br class="clearFloat" />
<div style="min-height: 150px;">
<h2>Comments</h2>
        <asp:UpdatePanel ID="commentsUpdatePanel" runat="server">
            <ContentTemplate>

            <asp:TextBox ID="txtComments" runat="server" TextMode="MultiLine" Rows="4" Width="300px"></asp:TextBox>
            <asp:Label ID="commentMessage" runat="server" Visible="false"></asp:Label>
                <asp:Button ID="btnAddComment" OnClick="btnAddComment_Click" runat="server" Text="Submit" CssClass="defaultButton addComment" />
            </ContentTemplate>
        </asp:UpdatePanel>
</div>      
<h2>Report Issues</h2>
  <!-- --> 
<br class="clearFloat" />
  <div class="labelTop" > 
    <asp:label id="lblInvalidLink"  associatedcontrolid="rblInvalidLink" runat="server">Select an Issue</asp:label> 
  </div>
  <div class="datacolumn"> 
    <asp:RadioButtonList id="rblInvalidLink" autopostback="false" causesvalidation="false"   runat="server" tooltip="True: User is active" RepeatDirection="Vertical">
	<asp:ListItem Text="Link is invalid/Not Found"  value="1"></asp:ListItem>    
 	<asp:ListItem Text="Resource is not appropriate" 	value="2" Selected="True"  ></asp:ListItem>
  <asp:ListItem Text="Other" 	value="3" Selected="True"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>
<asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="4" Width="300px"></asp:TextBox>
<asp:Button ID="btnReportIssue" runat="server" OnClick="btnReportIssue_Click" Text="Submit Report" CssClass="defaultButton" />

</asp:Panel><!--End tagging panel-->
<asp:Panel ID="pnlLoginToTag" runat="server" CssClass="loginDiv" Visible="false">

<p style="text-align: center; font-weight: bold;">Log In to Tag, Rate, and Comment.</p>
<uc1:LoginPanel id="loginWidget" runat="server" />

</asp:Panel>
</div>


<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="showingClustersAsCheckboxes" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="showingLoginSection" runat="server" Visible="false">no</asp:Literal>
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.LRW.Pages.ResourceDetail</asp:Literal>
	<asp:Literal ID="formattedSourceTemplate" runat="server" Visible="false"><a style="color:#000; " href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>
	<asp:Literal ID="formattedCCRightsUrl" runat="server" Visible="false"><a style="color:#000; " href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>

<asp:Literal ID="usingRightsSlider" runat="server" Visible="false">no</asp:Literal>
<asp:Literal ID="textRightsTemplate" runat="server" Visible="false">
<div class="slide"><img alt="Instructions" src="/images/icons/infoBubble.gif" />Show conditions of use</div>
<div class="infoMessage" style="display: none; width:90%;padding-left:5px;">{0}</div>
</asp:Literal>
</asp:Panel>

<asp:Panel ID="Panel4" runat="server" Visible="false">
	<asp:Literal ID="publisherSearchTemplate" runat="server" Visible="false"> ([Publisher] = '{0}') </asp:Literal>
	<asp:Literal ID="publisherDisplayTemplate" runat="server" Visible="false"><a href="/Search.aspx?pub={0}" target="_blank">{0}</a></asp:Literal>
  
<asp:Literal ID="noPublisherTemplate" runat="server" Visible="false">Unknown</asp:Literal>

	<asp:Literal ID="subjectSearchLinkTemplate" runat="server" Visible="false"><a href="/Search.aspx?subject={0}" target="_blank">{0}</a></asp:Literal>
  <asp:Literal ID="keywordSearchLinkTemplate" runat="server" Visible="false"><a href="/Search.aspx?keyword={0}" target="_blank">{0}</a></asp:Literal>
</asp:Panel>
