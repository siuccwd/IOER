<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ResourcePage.ascx.cs" Inherits="ILPathways.LRW.controls.ResourcePage" %>

<script type="text/javascript" language="javascript">var heightThreshhold = 150;</script>
<script type="text/javascript" language="javascript" src="/Scripts/fadeCollapse.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/fadeCollapse.css" />

<script type="text/javascript" language="javascript">
  $(document).ready(function () {

    var target = "<%=redirectTarget %>";

    if (target != "") {
      window.location = target;
    }

//    setTimeout(function () {
//      //alert($(".resourceFrame").contents().find("body").height());
//      $(".resourceFrame").css("height", $(".resourceFrame").contents().find("body").height() + 10 + "px").prop("scrolling", "no");
//    },
//    2000); //ensures iframe content is loaded before attempting to access it
  });
</script>

<style type="text/css">
.resourceFrame {
  width: 100%;
  height: 500px;
  border: none;
}
.buttons {
  text-align: left;
  margin-bottom: 5px;
}
.defaultButton {
  width: 200px;
}
.pageContent {
  width: 720px;
  display: inline-block;
  *display: inline;
  vertical-align: top;
  margin: 5px;
}

.column { float: right;
  display: inline-block;
  *display: inline;
  zoom: 1;
  vertical-align: top;
  width: 250px;
  margin: 10px;
}
a.showHideLink {
  background-image: url('/images/grayVerticalGradient50.png');
}
.publishButton {
  width: 100%;
  padding: 5px;
  background-color: #FF5707;
  cursor: pointer;
}
.publishButton:hover, .publishButton:focus {
  background-color: #4AA394;
}
.sectionContent {
  margin: 0 5px 5px 5px;
}
.inprogressStatus { background-color:#f5f5f5; color: #000;  padding: 0 25px; }
.submittedStatus {  background-color:YELLOW; color: #000; padding: 0 25px; }
.publishedStatus { background-color:Lime; color: #000; padding: 0 25px; }
.declinedStatus { background-color:red; color: #fff; padding: 0 25px; }
.requiresrevisionStatus { background-color:red; color: #fff; padding: 0 25px; }
</style>

<h1 class="isleH1"><%=resourceTitle %></h1>
<asp:Panel ID="notAllowedPanel" runat="server" Visible="false">
<h2>Private Content</h2>
<p>This content has been designated as not available for public viewing.</p>
<asp:label id="lblContentState" runat="server" />
</asp:Panel>
<asp:Panel ID="detailPanel" runat="server" Visible="false">

<div class="pageContent">
 <h2 >Summary</h2>
  <p><%=resourceSummary %></p>
<asp:label id="pageContent" runat="server"></asp:label>
</div>
<div class="isleBox column right">
  <div class="buttons" id="buttonBox" visible="false" runat="server">
    <asp:Panel ID="publishSection" Visible="false" runat="server" >
      <h2 class="isleBox_H2">I’m Finished</h2>
      <p>I’m finished creating this Resource. Now I want to tag it so others can find it:</p>
      <asp:Button ID="publishButton" runat="server" Visible="true" Text="Tag this Resource" CssClass="defaultButton publishButton" OnClick="PublishButton_Click" />
    </asp:Panel>
    <asp:Button ID="requestApprovalButton" runat="server" Visible="false" Text="Request Approval" CssClass="defaultButton publishButton" OnClick="RequestApprovalButton_Click" />
    <div id="approveSection" style="text-align:left;" visible="false" runat="server">
    <asp:Button ID="approveButton" runat="server" Visible="true " Text="Approve this Resource" CssClass="defaultButton publishButton" OnClick="ApproveButton_Click" />

        <a href="javascript:void()" class="toggleTrigger" id="trigger_2">Decline/Request updates</a>
        <div class="toggleVictim hidden" id="victim_2">
        Reason/Requested changes:&nbsp;<asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="3" Width="200px" MaxLength="500" ></asp:TextBox>
        <br /><asp:LinkButton id="declineButton" Visible="true" runat="server" OnClick="DeclineButton_Click" Text="Request Changes" ></asp:LinkButton>
        <hr />
      </div>
   </div>

  </div>
  <div id="statusDiv" runat="server" style="font-weight:bold; text-align: center; margin-bottom: 7px; ">
      <asp:Label ID="lblStatus" runat="server" >Draft</asp:Label>
  </div>
  <h2 class="isleBox_H2">Rights</h2>
   
  <h3 >Resource Usage Rights</h3>
  <asp:label CssClass="sectionContent" id="lblUsageRights" runat="server" />

  <h3 >Resource Access Rights</h3>
  <div class="sectionContent" >
    <asp:label id="lblPrivileges" runat="server" />
  </div>
  <h3 >Resource Authored by</h3>
  <div class="sectionContent" ><asp:Label ID="lblAuthor" runat="server" /></div>
  
  <asp:panel ID="communityViewPanel" runat="server" Visible="false">
  <h2 class="isleBox_H2">Community View</h2>
    <div class="sectionContent" >
    <asp:HyperLink ID="hlResourceVerionLink" runat="server" Text="View This Resource's Tags"></asp:HyperLink>
    </div>
</asp:panel>

 
  <asp:panel ID="standardsPanel" runat="server" Visible="false">
  <h2 class="isleBox_H2">Standards</h2>
  <div class="sectionContent" >
    <asp:Label ID="lblStandardsList" runat="server"></asp:Label>
    </div>
  </asp:panel>

  <asp:panel ID="supplementsPanel" runat="server" Visible="true">
    <h2 class="isleBox_H2">Supplements</h2>
  <asp:panel ID="attachmentsPanel" runat="server" Visible="false" CssClass="expandCollapseBox">
    <h3 class="isleH3_Block">Attachments</h3>
  <div class="sectionContent" >
    <asp:label id="supplements" runat="server" />
    </div>
  </asp:panel>

  
  <asp:panel ID="referencesPanel" runat="server" Visible="false" CssClass="expandCollapseBox">
  <h3 class="isleH3_Block">Resources</h3>
  <div class="sectionContent" >
    <asp:label id="lblReferences" runat="server" />
    </div>
  </asp:panel>


  </asp:panel>
</div>

<div class="clearFloat"></div>
</asp:Panel>
<asp:Panel ID="hiddenStuff" Visible="false" runat="server">
  <asp:Literal ID="txtFormSecurityName" runat="server">ILPathways.LRW.controls.ResourcePage</asp:Literal>
  <asp:Literal ID="showingRefUrl" runat="server">yes</asp:Literal>
  <asp:Literal ID="showingImageAttachmentsInline" runat="server">yes</asp:Literal>
  <asp:Literal ID="canAuthorApproveOwnContent" runat="server">no</asp:Literal>

  <asp:Literal ID="txtCurrentContentId" runat="server">0</asp:Literal>
  <asp:Literal ID="docLinkTemplate" runat="server"><a href="{0}" target="_blank">{1}</a></asp:Literal>
  <asp:Literal ID="imageLinkTemplate" runat="server"><img src="/Repository/Show.aspx?rid={0}" /></asp:Literal>
  <asp:Literal ID="ccouImageLinkTemplate" runat="server"><a href="{0}" title="{2}" target="_blank"><img src="{1}" alt="{2}" /></a></asp:Literal>
  <asp:Literal ID="rvLink" runat="server">/ResourceDetail.aspx?vid={0}</asp:Literal>

  <asp:Literal ID="privateContentMsg" runat="server"><h2>Page not Available</h2><p>This content is under construction or has been designated as not available for public viewing.</p></asp:Literal>


<%--<div id="resourceFrame" visible="false" class="resourceFrame" src="/Repository/ResourceFrame.aspx?rid=<%=resourceFrameSource %>"></div>--%>

</asp:Panel>
