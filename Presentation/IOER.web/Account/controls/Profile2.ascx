<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Profile2.ascx.cs" Inherits="ILPathways.Account.controls.Profile2" %>

<script type="text/javascript"  src="/Scripts/toolTip.js"></script>
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


</script>
<style type="text/css">

  .toolTipLink { text-align: left; }
  .hiddenButton { display: none; }
</style>

<div style="width: 30%; display: inline-block; vertical-align: top;">

<!-- --> 
  <div class="labelColumn" > 
    <asp:label id="Label12"  runat="server">Profile Image</asp:label> 
  </div>
  <div class="dataColumn isleBox" style="width: 145px; height: 145px;">
    <asp:literal ID="currentImage" runat="server" Visible="false"></asp:literal>
	</div>
    <div class="dataColumn" style="width: 150px; padding-top:20px;">
    <asp:label ID="noProfileImagelabel" runat="server" Visible="false">You can upload an image to show on your profile and with site activity.</asp:label>
	</div>
  <div class="clearFloat"></div>	
  <div class="labelColumn" >&nbsp; </div>
  <div class="dataColumn">
  <asp:Label ID="currentFileName" runat="server" Visible="false"></asp:Label>
  
  
		<br /><span style="font-weight: bold;">Select an image</span><a class="toolTipLink" id="tipFile" title="Profile Image|Select a image for your Profile. It must be roughly square and should be 140px (width) x 140px (height) or it will be resized to a width of 140px and a height not taller than 140px. If the image is still taller than 140px, it will be cropped to fit the height."><img
			src="/images/icons/infoBubble.gif" alt="" /></a>
			<br />
	<asp:FileUpload ID="fileUpload" runat="server"  />
	</div>
	
</div>
<div style="width: 50%; display: inline-block;">
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
      <asp:TextBox runat="server" ID="txtProfile" TextMode="MultiLine" Rows="3" CssClass="textBox" />
    </td>
  </tr>
  <asp:HiddenField runat="server" ID="hdnWorknetId" Value="0" />
  <asp:HiddenField runat="server" ID="jValidateError" />
  <tr>
    <td colspan="2" class="buttons">
      
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
  ValidationExpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
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

<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?g={0}&a=activate</asp:Literal>

<asp:Literal ID="showingImagePath" runat="server" Visible="false">no</asp:Literal>
<asp:Literal ID="txtImageTemplate" runat="server" Visible="false"><img src='{0}' width='{1}px' height='{1}px' alt='libary icon'/></asp:Literal>


<asp:Literal ID="roleSelectTitle" runat="server">I will use this site as a ...</asp:Literal>
<asp:Literal ID="lblAuthorProfileTitle" runat="server">Author profile</asp:Literal>
<asp:Literal ID="pubRoleSql" runat="server">SELECT [Id],[Title] FROM [dbo].[Codes.AudienceType] where [IsActive]=1 and IsPublishingRole= 1  order by [Title]</asp:Literal>


<%-- The 90s HTML style message that the Word message is based on. --%>
<asp:Label ID="confirmMessageOriginal" runat="server">
  <div style="width:100%; background-color:#FFF; font-family:Calibri,Arial,Helvetica,Sans-serif;">
    <div style="width:600px; padding: 25px; background-color: #E6E6E6;">
      <h1 style="background-color: #4F4E4F; color: #E6E6E6;">Thank you for using Illinois Open Education Resources!</h1>
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
      style='mso-spacerun:yes'> </span>Thank you for using Illinois Open Education
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

