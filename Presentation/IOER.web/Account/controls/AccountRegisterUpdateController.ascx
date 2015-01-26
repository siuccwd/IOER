<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AccountRegisterUpdateController.ascx.cs"
  Inherits="ILPathways.Account.controls.AccountRegisterUpdateController" %>


<script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />



<script type="text/javascript">
  //Simple password validation
  $(document).ready(function () {

    $(".password1").keyup(function () {
      validatePassword();
    });

    $(".password2").keyup(function () {
      validatePassword2();
    });

  });

  function jValidateUpdate() {
    var formIsValid = true;
    var errorMessage = "";
    //Check password matching
    if ($(".password1").val() != $(".password2").val()) { formIsValid = false; errorMessage += "Passwords do not match.\n"; }
    //Check email matching
    if ($(".email1").val() != $(".email2").val()) { formIsValid = false; errorMessage += "Email fields do not match.\n"; }

    if (!formIsValid) {
      //alert(errorMessage);
      $("#BodyContent_jValidateError").val(errorMessage);
      $("#errorContainer").html(errorMessage);
    }
    else {
      $("#BodyContent_jValidateError").val("");
      document.getElementById("<%=btnUpdateProfile.ClientID %>").click();
    }
  }

  function jValidate() {
    var formIsValid = true;
    var errorMessage = "";
    var missingFields = "";
    var fullBoxes = true;
    //Check password matching
    if ($(".password1").val() != $(".password2").val()) { formIsValid = false; errorMessage += "Passwords do not match.\n"; }
    //Check password validity
    if (!(validatePassword() && validatePassword2())) { formIsValid = false; errorMessage += "Password is invalid.\n"; }
    //Check email matching
    if ($(".email1").val() != $(".email2").val()) { formIsValid = false; errorMessage += "Email fields do not match.\n"; }
    //Check TOS agreement
    if (!$(".tosBox input").is(":checked")) { formIsValid = false; errorMessage += "You must agree to the Terms of Use to register.\n"; }
    //Check for empty fields 
    $("#registerTable input").each(function () {
      if (this.id == "<%=hdnWorknetId.ClientID %>"
      || this.id == "<%=txtJobTitle.ClientID %>"
      || this.id == "<%=txtProfile.ClientID %>"
      || this.id == "<%=txtOrganizationName.ClientID %>"
      || this.id == "ctl00_BodyContent_AccountRegisterUpdateController_TextBoxWatermarkExtender5_ClientState"
      || this.id.indexOf( "_ClientState") > -1
      || this.id == "<%=jValidateError.ClientID %>"
      ) { }
      else if (this.value.length < 1) {
        missingFields += this.id + ", ";
        formIsValid = false;
        fullBoxes = false;
      }
    });
    if (!fullBoxes) { errorMessage += "One or more required fields are empty.\n" + missingFields; }

    //Check username validity
    if (!validateUserName()) { formIsValid = false; errorMessage += "User Name must be at least 6 characters long with no spaces or symbols.\n"; }

    if (!formIsValid) {
      //alert(errorMessage);
      $("#BodyContent_jValidateError").val(errorMessage);
      $("#errorContainer").html(errorMessage);
    }
    else {
      $("#BodyContent_jValidateError").val("");
      document.getElementById("<%=btnSubmitRegister.ClientID %>").click();
    }
  }

  function validatePassword() {
    var validPass = false;
    var passwordText = $(".password1").val();
    var currentValue = passwordText.split("");
    var lowerCaseChars = "abcdefghijklmnopqrstuvwxyz"
    var upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
    var numberChars = "1234567890"
    var specialChars = "!@$%^&*_-(){}[]+?"
    var disallowedChars = "<> =+~`,.#/|\\";
    var hasLowerCase = false;
    var hasUpperCase = false;
    var hasNumber = false;
    var hasSpecial = false;
    var hasDisallowed = false;
    var passwordStrengthString = "";
    for (var i = 0; i < currentValue.length; i++) {
      if (lowerCaseChars.indexOf(currentValue[i]) > -1) { hasLowerCase = true; }
      if (upperCaseChars.indexOf(currentValue[i]) > -1) { hasUpperCase = true; }
      if (numberChars.indexOf(currentValue[i]) > -1) { hasNumber = true; }
      if (specialChars.indexOf(currentValue[i]) > -1) { hasSpecial = true; }
      if (disallowedChars.indexOf(currentValue[i]) > -1) { hasDisallowed = true; }
    }
    if (hasDisallowed) {
      passwordStrengthString = "<b style=\"color:#C00\">Error:</b> Password contains one or more invalid characters.";
    }
    else {
      if (hasLowerCase && hasUpperCase && hasNumber && passwordText.length >= 8) {
        validPass = true;

        if (hasSpecial) {
          passwordStrengthString = "<b style=\"color:#0C0\">Strong</b> Password.";
        }
        else {
          passwordStrengthString = "<b style=\"color:#CC0\">Good.</b> Add a special character ( ! @ $ % ^ * ( ) + ? ) for added security.";
        }
      }
      else {
        passwordStrengthString = "<b style=\"color:#C00\">Weak:</b> Requires " +
        (hasLowerCase ? "" : "Lowercase Letter, ") +
        (hasUpperCase ? "" : "Uppercase Letter, ") +
        (hasNumber ? "" : "Number, ") +
        (passwordText.length >= 8 ? "" : "8 or more characters");
      }
    }

    $("#passwordStrength").html(passwordStrengthString);
    return validPass;
  }

  function validatePassword2() {
    if ($(".password1").val() != $(".password2").val()) {
      $("#passwordMatch").html("Passwords do not match.");
      return false;
    }
    else {
      $("#passwordMatch").html("Passwords Match.");
      return true;
    }
  }

  function validateUserName() {
    name = $(".userName").val();
    //Check for 6+ characters
    if (name.length < 6) { return false; }
    //Check for spaces
    if (name.indexOf(" ") > -1) { return false; }
    //Check for special characters
    var specialChars = "$%^&()+?<> {}[]=+-~`,#/|\\".split("");
    for (var i = 0; i < specialChars.length; i++) {
      if (name.indexOf(specialChars[i]) > -1) { return false; }
    }
    return true;
  }
</script>

<style type="text/css">
  #registerTable { margin: 0 auto; padding: 5px 0 25px 0; width: 560px; }
  #registerTable td.label { font-weight: bold; text-align: right; padding-right: 5px; line-height: 1.7em; width: 240px; }
  #registerTable .textBox { width: 345px; }
  #registerTable .dropDownList { width: 350px; }
  #registerTable .buttons { text-align: right; }
  #registerTable .registerButton { padding: 2px 10px; font-weight: bold; }
  #registerTable #errorContainer { color: #D00; font-weight: bold; text-align: center; padding: 10px; }
  #registerTable #passwordStrength { line-height: 1.7em; }
  #registerTable select { white-space: nowrap; }
  #registerTable a { text-decoration: underline; }
  .toolTipLink { text-align: left; }
  .hiddenButton { display: none; }

  #avatarBox { padding: 2px; text-align: center; }
  #avatarBox img { width: 100%; border-radius: 5px; }
</style>

<style type="text/css">
    #thisContainer {  width: 100%; min-height:300px; margin: 50px auto;}
    #leftCol { width: 30%; display: inline-block; vertical-align: top; }
    #leftCol .dataColumn { width:100%; }
    #rightCol {width: 60%; display: inline-block; padding: 50px;  margin: 5px auto; background-color: #EEE; border-radius: 5px; }
    .labelColumn { width: 25%; }
    .dataColumn { width: 55%; }
    .imgClass { width: 100%; }

    .toolTipLink { text-align: left; }
    .dataColumn .btnUpload { width: 150px; }
    .hiddenButton { display: none; }
    .fileUpload { width: 80%; font-size: 12px; }

</style>
<style type="text/css">
@media screen and (max-width: 900px) {
    
    #leftCol { width: 90%; }
    #rightCol {width: 80%; }
    .fileUpload { width: 100%; }
    #rightCol .labelColumn { width: 90%; text-align: left; }
    #rightCol .dataColumn { width: 90%; margin: 10px;  }
}
</style>


<asp:Panel ID="registrationPanel" runat="server" Visible="false">
  <h2>
    Why create an account?</h2>
  <p>
    An account is not necessary to use this site, for example to search for education
    resources. However setting up an account will allow you to create, share, evaluate,
    and collaborate on career and education resources.<br />
    <span class="toolTipLink" id="learnMoreTip" title="Learn More:|This Pre-Alpha site is part of The Illinois Shared Learning Environment (ISLE). |The tools being created include Authoring, Publishing, Searching, Evaluating, and Collaborating. By setting up an account, you will have access to comment and rate resources, and create and use libraries. <br />|Currently, these tools are under development and being piloted with the Bloomington School District #87. Once this pilot is complete, additional race to the top districts and STEM learning exchanges will begin participating.<br />|Using the tools to create and publish career and education webpages requires you to be a member of or affiliated with a pilot organization.">
      Learn More
      <img src="/images/icons/infoBubble.gif" alt="information icon" />
    </span>
  </p>
  <p>Note - You must be 13 or older to register!</p>
</asp:Panel>


<div id="leftCol"  class="medLrgContent" >

<!-- --> 
  <div class="dataColumn" > 
    <h2>Profile Image</h2> 
  </div>
  <div class="dataColumn" style="width: 160px; height: 160px; text-align:center; vertical-align:central;" id="avatarBox">
    <asp:literal ID="currentImage" runat="server" Visible="false"></asp:literal>
	</div>
    <div class="dataColumn" style="width: 150px; padding-top:20px;">
    <asp:label ID="noProfileImagelabel" runat="server" Visible="false">You can upload an image to show on your profile and with site activity.</asp:label>
	</div>	

<div class="dataColumn" >Select an image <a class="toolTipLink" id="tipFile" title="Profile Image|Select a image for your Profile. It must be roughly square and should be 140px (width) x 140px (height) or it will be resized to a width of 140px and a height not taller than 140px. If the image is still taller than 140px, it will be cropped to fit the height."><img
			src="/images/icons/infoBubble.gif" alt="" /></a>
  </div>
<asp:Panel ID="pnlUpload" runat="server">
  <div class="dataColumn">
	<asp:FileUpload ID="fileUpload" CssClass="fileUpload upload " runat="server"  />
      <br />
          <asp:Button runat="server" ID="btnUpload" Visible="true" CssClass=" defaultButton btnUpload" 
        Text="Upload" OnClick="btnUpload_Click" />
    </div>
  <div class="dataColumn">
	<asp:Label ID="currentFileName" runat="server" Visible="false"></asp:Label>
  </div>
</asp:Panel>
    <asp:Panel ID="pnlCrop" runat="server" Visible="false">
      <table>
        <tr>
          <td>
            <asp:Image ID="imgCrop" runat="server" />
          </td>
          <td>
            <div style="margin: 50px; width: 500px; text-align: center">
              <div style="width: 400px; height: 100px; overflow: hidden; margin-left: 5px;">
                <asp:Image ID="previewImage" CssClass="" Style="width: 200px;" runat="server" />
              </div>
            </div>
          </td>
        </tr>
      </table>
      <br />
      <asp:HiddenField ID="X" runat="server" />
      <asp:HiddenField ID="Y" runat="server" />
      <asp:HiddenField ID="W" runat="server" />
      <asp:HiddenField ID="H" runat="server" />
      <asp:Button ID="btnCrop" runat="server" Text="View" OnClick="btnCrop_Click" />
              <hr />
    </asp:Panel>
    <asp:Panel ID="pnlCropped" runat="server" Visible="false">
      <asp:Image ID="imgCropped" runat="server" />
      <br />
      <asp:Image ID="imgResized" runat="server" />
      <br />
      <asp:Button ID="btnSave" runat="server" Text="Save - soon" OnClick="btnSave_Click" />
              <hr />
    </asp:Panel>

      <asp:Button ID="btnNew" runat="server" Text="Start Over" Visible="false" OnClick="btnNew_Click" />


</div>
<div id="rightCol"  class="medLrgContent" >
<h2 style="text-align:center;">Profile</h2> 
<asp:Panel ID="containerPanel" runat="server" Visible="false">
<table id="registerTable">
  <tr>
    <td colspan="2" id="errorContainer">
      <asp:Label ID="registerErrorMessage" runat="server"></asp:Label>
    </td>
  </tr>
  <tr>
    <td class="label requiredField">
    <asp:Label ID="Label1" AssociatedControlID="txtEmail" runat="server"><%=newPassword %>Email Address</asp:Label>
      
    </td>
    <td>
      <asp:TextBox runat="server" ID="txtEmail" CssClass="textBox email1 required" />
    </td>
  </tr>
  <tr>
    <td class="label">
      <asp:Label ID="Label2" AssociatedControlID="txtEmail2" runat="server">Confirm <%=newPassword %>Email Address</asp:Label>
    </td>
    <td>
      <asp:TextBox runat="server" ID="txtEmail2" CssClass="textBox email2 required" />
    </td>
  </tr>
  <asp:Panel ID="pnlChangePassword" runat="server" Visible="false">
    <tr>
      <td class="label">
        <span class="toolTipLink" style="color: Red;" id="oldPasswordTip" title="To Change your Password:|Enter your old password and your new password in the appropriate boxes.">
          Old Password
          <img src="/images/icons/infoBubble.gif" alt="information icon" /></span>
      </td>
      <td>
        <asp:TextBox runat="server" ID="txtChangePassword" CssClass="textBox" TextMode="Password" />
      </td>
    </tr>
  </asp:Panel>
  <tr>
    <td class="label">
      <span class="toolTipLink" style="color: Red;" id="passwordTip" title="Guidelines for a secure Password:
      |Your password must contain at least 8 characters.
      |Passwords may contain letters or numbers, and limited special characters (! @ $ % ^ & * ( ) + ?) but may not contain any spaces.
      |Password must contain at least one lowercase letter, an uppercase letter, and a number.">
        <asp:Label ID="Label5" AssociatedControlID="txtPassword1" runat="server"><%=newPassword %>Password</asp:Label>
        <img src="/images/icons/infoBubble.gif" />
      </span>
    </td>
    <td>
      <asp:TextBox TextMode="Password" runat="server" ID="txtPassword1" CssClass="textBox password1"
        MaxLength="32" />
    </td>
  </tr>
  <tr>
    <td class="label">
      <asp:Label ID="Label4" AssociatedControlID="txtPassword2" runat="server">Confirm <%=newPassword %>Password</asp:Label>
    </td>
    <td>
      <asp:TextBox TextMode="Password" runat="server" ID="txtPassword2" CssClass="textBox password2" />
    </td>
  </tr>
  <tr>
    <td class="label">
      Password Strength
    </td>
    <td>
      <span id="passwordStrength"></span><span id="passwordMatch"></span>
    </td>
  </tr>
  <tr>
    <td class="label requiredField">
      <asp:Label ID="Label3" AssociatedControlID="txtFirstName" runat="server">First Name</asp:Label>
    </td>
    <td>
      <asp:TextBox runat="server" ID="txtFirstName" CssClass="textBox required" />
    </td>
  </tr>
  <tr>
    <td class="label requiredField">
    <asp:Label ID="Label6" AssociatedControlID="txtLastName" runat="server">Last Name</asp:Label>
    </td>
    <td>
      <asp:TextBox runat="server" ID="txtLastName" CssClass="textBox required" />
    </td>
  </tr>
  <tr>
    <td class="label requiredField">
    <asp:Label ID="Label9" AssociatedControlID="ddlPubRole" runat="server">Interest</asp:Label>
      
    </td>
    <td>
      <asp:DropDownList runat="server" ID="ddlPubRole" CssClass="dropDownList" />
    </td>
  </tr>
  <tr>
    <td class="label" style="text-align: center; padding: 5px 0;" colspan="2">
        <asp:Label ID="lblPubProfileTitle" runat="server">Author Profile (optional - requires separate approval)</asp:Label> 
        <span class="toolTipLink" id="pubProfTip" title="Site publishing and authoring:
      |Approval is required in order to publish existing or author new resources. An administrator with your organization (which must be an approved organization) will be contacted to verify your relationship. Separate authorization may be granted for publishing, tagging, or authoring.
      |You will be notified by the administrator as to your status.">
        <img src="/images/icons/infoBubble.gif" alt="" />
      </span>
    </td>
  </tr>
  <tr>
    <td class="label" style="vertical-align:top;">      
      <asp:Label ID="Label7" AssociatedControlID="ddlOrganization" runat="server">Organization</asp:Label>
    </td>
    <td>
      <asp:Label ID="txtMyOrganization" runat="server" ></asp:Label>
      <asp:DropDownList runat="server" ID="ddlOrganization" Visible="false" CssClass="dropDownList" />
      <br />
      <div>
        <a href="javascript:void()" class="toggleTrigger" id="trigger_2">Request to be a member of or add an organization</a>
        <div class="toggleVictim hidden" id="victim_2">
        <asp:Label ID="Label11" AssociatedControlID="txtOrganizationName" runat="server">Search for your organization or request addition of a new organization (by entering the name)<br /></asp:Label>
          <asp:TextBox ID="txtOrganizationName" runat="server" Width="345px" MaxLength="100"></asp:TextBox>
          <br />
          <asp:LinkButton ID="requestAddOrgBtn" Visible="false" runat="server" CommandName="AddOrg"
            Text="Request"></asp:LinkButton>
          <asp:Label ID="addOrganizationMsg" runat="server"></asp:Label>
          			
        <ajaxToolkit:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender5" runat="server"
            TargetControlID="txtOrganizationName"
            WatermarkText="Type at least 3 letters of the organization"
            WatermarkCssClass="watermarked" />    
				<ajaxToolkit:AutoCompleteExtender ID="AutoCompleteExtender5" runat="server"
            MinimumPrefixLength="3" 
            ServiceMethod="OrganzationsAutoComplete" 
            ServicePath="/Services/SearchService.asmx" 
            TargetControlID="txtOrganizationName" 
						CompletionInterval="500"
            CompletionListElementID="Id"   >
        </ajaxToolkit:AutoCompleteExtender>     
</div>    
          <hr />

        </div>
      
    </td>
  </tr>
  <tr>
    <td class="label">
      <asp:Label ID="Label8" AssociatedControlID="txtJobTitle" runat="server">Job Title</asp:Label>
    </td>
    <td>
      <asp:TextBox runat="server" ID="txtJobTitle" CssClass="textBox" />
    </td>
  </tr>
  
  <tr>
    <td class="label">
    <asp:Label ID="Label10" AssociatedControlID="txtProfile" runat="server">Occupation profile (ex. grades taught)</asp:Label>
      
    </td>
    <td>
      <asp:TextBox runat="server" ID="txtProfile" TextMode="MultiLine" Rows="4" CssClass="textBox" />
    </td>
  </tr>
  <asp:HiddenField runat="server" ID="hdnWorknetId" Value="0" />
  <asp:HiddenField runat="server" ID="jValidateError" />
  <tr>
    <td colspan="2" class="buttons">
      <asp:Panel ID="pnlRegisterControls" Style="margin-top: 10px;" runat="server" Visible="true">
        <label class="requiredField">
          By creating an account, you agree to the ISLE:<br />
          <a target="_blank" href="http://ilsharedlearning.org/Pages/ISLE-Privacy-Policy.aspx">Privacy Policy</a> and 
          <a target="_blank" href="http://ilsharedlearning.org/Pages/ISLE-Terms-of-Use.aspx">Terms of Use</a>.
          <asp:CheckBox runat="server" ID="cbxTOS" CssClass="tosBox" />
        </label>
        <input type="button" value="Register" class="registerButton defaultButton" onclick="jValidate()" />
        <asp:Button runat="server" ID="btnSubmitRegister" CssClass="hiddenButton defaultButton"
          Text="Register" OnClick="btnSubmitRegister_Click" />
      </asp:Panel>
      <asp:Panel ID="pnlUpdateControls" runat="server" Visible="false">
        <input type="button" value="Update Profile" class="registerButton defaultButton"
          onclick="jValidateUpdate()" />
        <asp:Button runat="server" ID="btnUpdateProfile" CssClass="hiddenButton defaultButton"
          Text="Update" OnClick="btnUpdateProfile_Click" />
      </asp:Panel>
    </td>
  </tr>
</table>
<asp:RegularExpressionValidator ID="revEmail" runat="server" EnableClientScript="False"
  Display="None" ControlToValidate="txtEmail" ErrorMessage="Invalid Email Address."
  ValidationExpression="\w+([-+.]\w+)*@\w+([-_.]\w+)*\.\w+([-_.]\w+)*"></asp:RegularExpressionValidator>
</asp:Panel>
</div>
<asp:Panel ID="oldStuff" runat="server" Visible="false">
  <table>
    <tr>
      <td class="label">
        <span class="toolTipLink" style="color: Red;" id="userNameTip" title="User Name:|Choose a name that you will remember, at least 6 characters long, with no spaces or symbols.|For example consider the first letter of your first name and all of your last name (jsmith) as a user name that is easy to remember.">
          User Name
          <img src="/images/icons/infoBubble.gif" alt="information icon" />
        </span>
      </td>
      <td>
        <asp:TextBox runat="server" ID="txtUsername" CssClass="textBox userName" /><asp:Label
          runat="server" ID="lblUserName" Visible="false" />
      </td>
    </tr>
  </table>
</asp:Panel>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="txtCurrentView" runat="server">Registration</asp:Literal>
<asp:Literal ID="txtIsOrgRequired" runat="server">no</asp:Literal>
<asp:Literal ID="doBccOnRegistration" runat="server">yes</asp:Literal>
<asp:Literal ID="sendInfoEmailOnRegistration" runat="server">yes</asp:Literal>
<asp:Literal ID="imagePath" runat="server" ></asp:Literal>
<asp:Literal ID="workUrl" runat="server" ></asp:Literal>
<asp:Literal ID="workCroppedUrl" runat="server" ></asp:Literal>
<asp:Literal ID="workResizedUrl" runat="server" ></asp:Literal>

<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?g={0}&a=activate</asp:Literal>

<asp:Literal ID="showingImagePath" runat="server" Visible="false">no</asp:Literal>
<asp:Literal ID="txtImageTemplate" runat="server" Visible="false"><img src='{0}' style="max-width: {1}px; max-height = {1}px;" alt='profile image'/></asp:Literal>


<asp:Literal ID="roleSelectTitle" runat="server">I will use this site as a ...</asp:Literal>
<asp:Literal ID="lblAuthorProfileTitle" runat="server">Author profile</asp:Literal>
<asp:Literal ID="pubRoleSql" runat="server">SELECT [Id],[Title] FROM [dbo].[Codes.AudienceType] where [IsActive]=1 and IsPublishingRole= 1  order by [Title]</asp:Literal>

<%-- 
<asp:Label ID="confirmMessage" runat="server" >
<div style="width: 800px; padding:50px; background-color: #f5f5f5; ">
<div style="margin-left: 100px; padding: 50px 15px; line-height: 130%;  width: 600px; background-color: #fff; border: 1px solid #000;">
Hello {0} <div style="margin-top: 10px;">Thank you for using Illinois Open Educational Resources!</div>
<div>Please click the link below to confirm your email address and activate your IOER account</div>
<div style="margin-left: 100px; margin-top: 25px; padding: 5px 10px 5px 30px; width: 300px; background-color: Blue; color: #fff; text-align:center;">

<a style=" color: #fff; text-align:center;" href="{1}">Activate my IOER account</a>
</div>
<br />
Sincerely the IOER Team
</div>
</div>
</asp:Label>
--%>

<%-- The 90s HTML style message that the Word message is based on. --%>
<asp:Label ID="confirmMessageOriginal" runat="server">
  <div style="width:100%; background-color:#FFF; font-family:Calibri,Arial,Helvetica,Sans-serif;">
    <div style="width:600px; padding: 25px; background-color: #E6E6E6;">
      <h1 style="background-color: #4F4E4F; color: #E6E6E6;">Thank you for using Illinois Open Educational Resources!</h1>
      <p>Hello, {0}</p>
      <p>Please click the link below to confirm your email address and activate your IOER account.</p>
      <p><a href="{1}">Activate my IOER account!</a></p>
      <p>Sincerely, the IOER Team</p>
    </div>
  </div>
</asp:Label>

<%-- The MS-Word formatted message. Needs to be tested --%>
<asp:Label ID="confirmMessage" runat="server">
  <div>
    <div>
      <h1 style='background:#4F4E4F'><span style='font-family:"Calibri","sans-serif";
      mso-fareast-font-family:Calibri;mso-fareast-theme-font:minor-latin;color:#E6E6E6'><span
      style='mso-spacerun:yes'> </span>Thank you for using Illinois Open Educational
      Resources!<o:p></o:p></span></h1>
      <div style="background:#E6E6E6;background-color:#E6E6E6">
        <p style='background:#E6E6E6'><span style='font-family:"Calibri","sans-serif"'><span
        style='mso-spacerun:yes'> </span>Hello, {0}<o:p></o:p></span></p>
        <p style='text-indent:.5in;background:#E6E6E6'><span style='font-family:"Calibri","sans-serif"'>Please
        click the link below to confirm your email address and activate your IOER
        account.<o:p></o:p></span></p>
        <p style='text-indent:.5in;background:#E6E6E6'><span style='font-family:"Calibri","sans-serif"'><a
        href="{1}">Activate my IOER account!</a><o:p></o:p></span></p>
        <p style='background:#E6E6E6'><span style='font-family:"Calibri","sans-serif"'><span
        style='mso-spacerun:yes'> </span>Sincerely, the IOER Team<o:p></o:p></span></p>
        <p style='background:#E6E6E6'><span style='font-family:"Calibri","sans-serif"'><o:p>&nbsp;</o:p></span></p>
      </div>
    </div>
  </div>
</asp:Label>


<asp:Literal ID="txtNewOrgRequest" runat="server" ><p>{0} is requesting the addition of the <b>new organization</b>: {1}</p><p>There should probably be a more elborate process, but that will come later.</p></asp:Literal>
<asp:Literal ID="txtAddUserToOrgRequest" runat="server" ><p>{0} is requesting to be added to organization: {1}</p></asp:Literal>
</asp:Panel>
