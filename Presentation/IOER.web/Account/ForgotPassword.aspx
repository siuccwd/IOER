<%@ Page Title="Illinois Open Educational Resources - Forgot Password" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="ForgotPassword.aspx.cs" Inherits="IOER.Account.ForgotPassword" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="/Styles/Isle_large.css" type="text/css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<script type="text/javascript">
    $("form").removeAttr("onsubmit");
</script>    
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<h1 class="isleH1">Password Recovery</h1>
<div style="min-height: 450px; margin-left: 50px;">
    
	<!-- --> 
	<br class="clearFloat" />
  <div class="labelColumn requiredField">
    <asp:label id="Label1"  associatedcontrolid="txtEmail" runat="server">Enter Email Address</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  id="txtEmail" Width="300px" columns="100" MaxLength="100" runat="server"></asp:textbox>  
	</div>
	<!-- -->
		<br class="clearFloat" />	
		<div class="labelColumn">&nbsp;</div>
		<div class="dataColumn">			
				<asp:button id="btnSubmit" runat="server" CssClass="defaultButton" width="100px" 
				onclick="btnSubmit_Click"  Text="Submit"		CausesValidation="true"></asp:button>
				
		</div>
</div>
<!-- validators -->
<asp:requiredfieldvalidator id="rfvEmail" Display="None" ControlToValidate="txtEmail" ErrorMessage="Enter your Email address or login Id" runat="server"></asp:requiredfieldvalidator>
<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Label ID="recoverMessage" runat="server" >
Dear {0} <p>Click the following link to log into IOER and reset your password. <br /></p>
    <p><b>Note: For your protection, this is a one time use recovery link (so it cannot be used by others to access your account)</b>.</p>
<a href="{1}">Click here to login and be directed to the profile page to change your password</a>
</asp:Label>

<asp:Literal ID="loginLink" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Account/Profile.aspx</asp:Literal>
<asp:Literal ID="addToBcc" runat="server" >mparsons@siuccwd.com</asp:Literal>
<asp:Literal ID="attemptsCount" runat="server" >0</asp:Literal>
<asp:Literal ID="confirmationMessage" runat="server" >An email was sent to your account containing a link to reset your password. <br />If you do not see an email, be sure to check your junk mail folder (and add ilSharedLearning.org as a trusted partner!).</asp:Literal>
</asp:Panel>
</asp:Content>
