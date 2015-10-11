<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LoginController.ascx.cs" Inherits="IOER.Account.controls.LoginController" %>

<script type="text/javascript" language="javascript">
  $(document).ready(function () {
    //fix IE sometimes being stupid
    $("form").removeAttr("onsubmit");
  });

  function validateLoginForm() {
    var errorMessage = "";
    if ($("#mainSection #userName").val() == "") {
      errorMessage += "Please enter a User Name.\n";
    }
    if ($("#mainSection #password").val() == "") {
      errorMessage += "Please enter a Password.\n";
    }
    if (errorMessage != "") {
      alert(errorMessage);
      return false;
    }
    else {
      $("form").removeAttr("onsubmit");
      return true;
    }
  }
</script>

<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<style type="text/css">
  .isleH1 { text-align: center; }
	#content { padding-left: 15px; }
  #loginBox { margin: 50px auto; max-width: 500px; text-align: center; padding: 20px 10px 10px 10px; }
  #loginErrorMessage { background-color: #B03D25; color: #FFF; font-weight: bold; margin: 10px; max-width: 500px; }
  #loginBox label { width: 20%; text-align: right; }
  #loginBox input[type=text], #loginBox input[type=password] { width: 78%; }
  #loginBox input[type=button], #loginBox input[type=submit] { width: 46%; margin: 1%; }
  #loginBox * { margin-bottom: 10px; }
  #loginBox #forgotPasswordLink { font-weight: bold; color: #B03D25; display: block; }
  #content .grayBox.errorMessage { max-width: 500px; }
  @media screen and (max-width: 499px) {
    #loginBox label { display: block; margin-bottom: 0; width: 100%; text-align: left; line-height: 20px; height: 20px; }
    #loginBox input[type=text], #loginBox input[type=password], #loginBox input[type=button], #loginBox input[type=submit] { display: block; width: 100%; }
  }
</style>
<div id="content">
  <h1 class="isleH1">IOER Login</h1>
  <div class="grayBox errorMessage bgRed" id="loginErrorMessage" runat="server"><%=errorMessage %></div>
  <div id="loginBox" class="grayBox bigText">
    <label>Name/Email</label><asp:textbox runat="server" CssClass="textBox" id="txtUserName" name="userName" />
    <label>Password</label><asp:textbox runat="server" CssClass="textBox" TextMode="password" id="txtPassword" name="password" />
    <asp:Button CssClass="loginButton isleButton bgGreen" runat="server" ID="loginPathwaysButton" Text="Login" OnClientClick="validateLoginForm()" onclick="loginPathwaysButton_Click" />
    <input id="googleLogin" type="button" class="loginButton isleButton bgGreen" onclick="window.location='<%=ssoLoginUrl%>'" value="Login with Google" />
    <input id="registerButton" type="button" class="loginButton isleButton bgGreen" onclick="window.location = '/Account/Register.aspx'" value="Register" />
    <a id="forgotPasswordLink" class="textLink" href="/Account/ForgotPassword.aspx">Forgot Password?</a>  
  </div>
</div>

<asp:Panel ID="Panel1" runat="server" Visible="false">
  <asp:Button CssClass="hidden" runat="server" ID="loginLinkedInButton" OnClick="loginLinkedInButton_Click" />
  <asp:HiddenField runat="server" ID="hdnLinkedInID" Value="" />
  <br /><asp:Button width="145" CssClass="loginButton defaultButton" runat="server" ID="loginWorknetButton" Visible="false" Text="Login via workNet " OnClientClick="validateLoginForm()" onclick="loginWorknetButton_Click" /> 
<asp:Literal ID="txtReturnUrl" runat="server" Visible="false">/</asp:Literal>
    <asp:Literal ID="forcingNextUrlAsHttp" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="autoCreateLibraryOnActivate" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="libraryCreateMsg" runat="server" Visible="false">Your personal library was created. <br />To customize, <a href="/My/Library">navigate to My/Library</a>. Be sure to review the getting started guide for information on libraries</asp:Literal>

<asp:Literal ID="autoAddToOrg" runat="server" Visible="false"></asp:Literal>
    <asp:Literal ID="Literal1" runat="server" Visible="false">2015-09-30, 80</asp:Literal>
</asp:Panel>
