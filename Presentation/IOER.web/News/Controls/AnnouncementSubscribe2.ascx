<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AnnouncementSubscribe2.ascx.cs" Inherits="ILPathways.Controls.AppItems.AnnouncementSubscribe2" %>


<style type="text/css">
  .peach { 
    background-color:#FFF8DC;
    border: solid 1px #F69240;
    padding: 10px;
  }
    .pnlSubscribe { 
    padding: 10px;
  }
  .blueButton {
    background-color:#0073AE; color: #fff;
  }
</style>
<style type="text/css">
/*Textbox Watermark*/

.unwatermarked {	height:18px;	width:148px;}

.watermarked {	height:20px;	width:150px;	padding:2px 0 0 2px;	border:1px solid #BEBEBE;	background-color:#F0F8FF;	color:gray;}	
</style>
<style type="text/css">
@media screen and (max-width: 600px) {

.peach { padding: 5px }
.pnlSubscribe { padding: 10px;  }
}
</style>
<asp:Panel ID="pnlSubscribeMsg" runat="server" Visible="false">
  <asp:Literal ID="txtDesc1" runat="server" />
</asp:Panel>
<asp:Panel ID="pnlGuestMsg" runat="server" Visible="false">
  <asp:Literal ID="txtDesc2" runat="server" />
</asp:Panel>
<asp:Panel ID="pnlUpdatedEmail" runat="server" Visible="false">
  <asp:Literal ID="txtDesc3" runat="server" />
</asp:Panel>
<asp:Panel ID="pnlDetail" runat="server" CssClass="peach" Width="100%">
<asp:Panel ID="pnlGuest" runat="server">
  <div style="float:left; margin-left:4px;">
    <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail">
      <span style="color:#0073AE;font-weight:bold;">SIGN UP TO RECEIVE UPDATES</span> or<br />manage your subscription. We will only<br />contact you when we have news to share.
    </asp:Label>
  </div>
  <div class="dataColumn">
    <br />
    <asp:TextBox ID="txtEmail" runat="server" Width="300px" ></asp:TextBox>   
        	    <ajaxToolkit:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender2" runat="server"
        TargetControlID="txtEmail"
        WatermarkText="Enter Email Address Here"
        WatermarkCssClass="watermarked" Enabled="True" />  
        <asp:requiredfieldvalidator id="rfvEmail" runat="server" ControlToValidate="txtEmail" Display="None" ValidationGroup="newsSubValGroup" 
    enabled="true" ErrorMessage="Please enter a valid email address."></asp:requiredfieldvalidator>
		<asp:regularexpressionvalidator id="revEmail" runat="server" enableclientscript="true" display="None"  ValidationGroup="newsSubValGroup"
controltovalidate="txtEmail" errormessage="The email address is invalid." validationexpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:regularexpressionvalidator>	

    <br />
    <asp:Button ID="btnIdentify" runat="server" CommandName="Identify" OnCommand="FormButton_Click" Visible="false" Text="Submit" CssClass="defaultButton blueButton" />
  </div>
  <br class="clearFloat" />
  <asp:Panel ID="optionsPanel2" runat="server" Visible="false">
  <asp:CheckBoxList ID="CBLCategoryNewsletter" runat="server" RepeatDirection="Horizontal">
  </asp:CheckBoxList>
  <br />
  </asp:Panel>
<br />
</asp:Panel>

<asp:Panel ID="pnlSubscribe" runat="server" Visible="false">
  <div class="labelTop">
    <asp:Label ID="Label7" AssociatedControlID="rbNotificationFrequency" runat="server">Select Notification Frequency</asp:Label>
  </div>
  <div class="dataColumn">
    <asp:RadioButtonList ID="rbNotificationFrequency" CausesValidation="false" runat="server"
      RepeatDirection="Vertical">
      <asp:ListItem Text="Receive individual items as they are posted." Value="0"></asp:ListItem>
      <asp:ListItem Text="Once-daily Digest" Value="1" Selected="True"></asp:ListItem>
      <asp:ListItem Text="Weekly Digest" Value="7"></asp:ListItem>
    </asp:RadioButtonList>
  </div>
  <br class="clearFloat" />
  <div class="dataColumn">
    <asp:Button ID="btnValidate" class="defaultButton blueButton" runat="server" CausesValidation="false" Text="Validate"
      CommandName="Validate" Visible="false" OnCommand="FormButton_Click" />
    <asp:Button ID="btnSubscribe" class="defaultButton blueButton" runat="server" CausesValidation="false" Text="Subscribe"
      CommandName="Subscribe" Visible="false" OnCommand="FormButton_Click" />
    <asp:Button ID="btnUpdate" class="defaultButton blueButton" runat="server" CausesValidation = "false" Text="Update"
      CommandName="Update" Visible = "false" OnCommand="FormButton_Click" />
    <asp:Button ID="btnUnSubscribe" class="defaultButton blueButton" runat="server" CausesValidation="false" Text="Unsubscribe"
      CommandName="UnSubscribe" Visible="false" OnCommand="FormButton_Click" />
  </div>
  <br class="clearFloat" />
</asp:Panel>
</asp:Panel>

<asp:Panel ID="Panel1" runat="server" Visible="false">
<asp:Literal ID="txtUsingMcmsDescriptions" runat="server" >yes</asp:Literal>
<asp:Literal ID="doingWfpCheck" runat="server" >no</asp:Literal>
<asp:Literal ID="txtNewsItemCode" runat="server"></asp:Literal>

<asp:Literal ID="txtSubscribeUrl" runat="server" >
<a style="COLOR: white; TEXT-DECORATION: none" href="{0}">Yes, subscribe me to this list.</a>
</asp:Literal>
<asp:Literal ID="txtSubscribeUpdateUrl" runat="server" >
<a style="COLOR: white; TEXT-DECORATION: none" href="{0}">Yes, update my email address.</a>
</asp:Literal>
<asp:Literal ID="txtREsubscribeUrl" runat="server" ><a style="COLOR: white; TEXT-DECORATION: none" href="{0}">Re-subscribe me 
to this list</a></asp:Literal>

</asp:Panel>

<asp:Panel ID="Panel2" runat="server" Visible="false">
<asp:Literal ID="defaultDesc1" runat="server" >
<H2>Almost finished...</H2><BR>We need to confirm your email address.<BR><BR>To complete the subscription process, please click the link in the email we just sent you.<BR><BR>
<DIV style="FLOAT: left"><A style="FLOAT: left" onclick="ShowHideSection('divInstructions1');" href="javascript:void(0);">Add us to your address book</A> <BR></DIV>
<DIV class=clear></DIV>
<DIV class=infoMessage id=divInstructions1 style="DISPLAY: none; PADDING-LEFT: 5px; WIDTH: 90%">
<P>While we're not sure what application you are using to read your email, they all operate in a similar fashion. When you add an email address to your address book, you add it to a "white list," meaning that you have specified that it is okay to receive email from that individual or company. News and Updates are sent from info@illinoisworknet.com. To add us to your address book, do the following: </P>
<OL>
<LI>Look in your email client settings, or in the side bar or header of your webmail client, to see if you have a contact list, address book, or something similar. Click to enter that section.</LI>
<LI>Choose the option that lets you add a new contact.</LI>
<LI>Enter the email address <A href="mailto:info@illinoisworknet.com">info@illinoisworknet.com</A> in the email address field, and complete any other required fields.</LI>
<LI>Click "Save" or "OK."</LI>
</OL>
</DIV><BR />
<SPAN style="PADDING-RIGHT: 5px; PADDING-LEFT: 5px"><A href="{0}">Return to our website</A> </SPAN>
</asp:Literal>

<!-- this could just have been a console message 
could save the referring url and use?
-->
<asp:Literal ID="websiteLink" runat="server" >
<SPAN style="PADDING-RIGHT: 5px; PADDING-LEFT: 5px"><A href="{0}">Return to our website</A> </SPAN>
</asp:Literal>

<!-- this could just have been a console message 
could save the referring url and use?
-->
<asp:Literal ID="defaultDesc2" runat="server" >
<H2>Subscription Confirmed</H2>
Your subscription to {0} has been confirmed.&nbsp; Thank you for subscribing!<BR><BR>
<SPAN style="PADDING-RIGHT: 5px; PADDING-LEFT: 5px"><A href="{1}">Return to our website</A></SPAN> or <SPAN style="PADDING-RIGHT: 5px; PADDING-LEFT: 5px"><A href="{2}?RowId=@RowId">Manage your preferences</A></SPAN>
</asp:Literal>

<asp:Literal ID="defaultDesc3" runat="server" >
<H2>Email Address Change Confirmed</H2>Your Email address change for {0} has been confirmed. Thank you for updating your subscription!<BR>
<SPAN style="PADDING-RIGHT: 5px; PADDING-LEFT: 5px"><A href="{1}">Return to our website</A> </SPAN>
</asp:Literal>
</asp:Panel>