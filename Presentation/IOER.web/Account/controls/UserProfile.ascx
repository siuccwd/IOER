<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfile.ascx.cs" Inherits="ILPathways.Account.controls.UserProfile" %>

<%--<link href="../../fancybox/jquery.fancybox-1.3.4.css" rel="stylesheet" />
<script src="../../fancybox/jquery.fancybox-1.3.4.min.js"></script>--%>
<script type="text/javascript"  src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />
<style type="text/css">
    #thisContainer {  width: 100%; min-height:300px; margin: 50px auto;}
    #leftCol { width: 30%; display: inline-block; vertical-align: top; }
    #leftCol .dataColumn { width:100%; }
    #rightCol {width: 60%; display: inline-block; padding: 50px;  margin: 5px auto; background-color: #EEE; border-radius: 5px; }
    .labelColumn { width: 25%; }
    .dataColumn { width: 55%; }
    .imgClass { width: 100%; }

    .toolTipLink { text-align: left; btnUpload
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
            return false;
        }
        else {
            $("#BodyContent_jValidateError").val("");
            //document.getElementById("<%=btnUpdateProfile.ClientID %>").click();
            return true;
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

<script type="text/javascript">
    $(document).ready(function () {
        $(".inviteToggle").click(function () {
            $("#inviteInstructions").slideToggle('normal');
        });
    });

    function ShowHideSection(target) {
        $("#" + target).slideToggle();
    }
//var $fb2 = jQuery.noConflict();
//$fb2(document).ready(function () {
//    $fb2('.iframe').fancybox({
//        width: 550,
//        height: 700,
//        titlePosition: 'outside'
//    });
//});
</script>
<div id="thisContainer">
<div id="leftCol" class="lrgContent">

<!-- --> 
  <div class="dataColumn" > 
    <h2>Profile Image</h2>
  </div>
  <div class="dataColumn " style="width: 160px; height: 160px; text-align:center; vertical-align:central;">
    <asp:literal ID="currentImage" runat="server" Visible="false"></asp:literal>
  </div>

    <div class="dataColumn" style="width: 150px; padding-top:20px;">
    <asp:label ID="noProfileImagelabel" runat="server" Visible="false">You can upload an image to show on your profile and with site activity.</asp:label>
	</div>

  <div class="dataColumn" >Select an image <a class="toolTipLink" id="tipFile" title="Profile Image|Select a image for your Profile. It must be roughly square and should be 140px (width) x 140px (height) or it will be resized to a width of 140px and a height not taller than 140px. If the image is still taller than 140px, it will be cropped to fit the height."><img
			src="/images/icons/infoBubble.gif" alt="" /></a>
  </div>
  <div class="dataColumn">
	<asp:FileUpload ID="fileUpload" CssClass="fileUpload upload " runat="server"  />
      <br />
          <asp:Button runat="server" ID="btnUpload" Visible="true" CssClass=" defaultButton" 
        Text="Upload" OnClick="btnUpload_Click" />
    </div>
  <div class="dataColumn">
	<asp:Label ID="currentFileName" runat="server" Visible="false"></asp:Label>
  </div>
</div>
<div id="rightCol" class="lrgContent" >
  <h2>Profile</h2> 
<div id="registerTable">

      <asp:Label ID="registerErrorMessage" CssClass="label" runat="server"></asp:Label><br />
   
 
  <div class="labelColumn">
    <asp:Label ID="Label1" AssociatedControlID="txtEmail" runat="server"><%=newPassword %>Email Address</asp:Label>
    </div>
   <div class="dataColumn">
      <asp:TextBox runat="server" ID="txtEmail" CssClass="textBox email1 required" />
    </div>
  
<div class="labelColumn">
      <asp:Label ID="Label2" AssociatedControlID="txtEmail2" runat="server">Confirm <%=newPassword %>Email Address</asp:Label>
   </div>
   <div class="dataColumn">
      <asp:TextBox runat="server" ID="txtEmail2" CssClass="textBox email2 required" />
    </div>
 
<div class="labelColumn">
        <asp:Label ID="Label5" AssociatedControlID="txtPassword1" runat="server"><%=newPassword %>Password</asp:Label>
 <span class="toolTipLink" style="color: Red;" id="passwordTip" title="Guidelines for a secure Password:
      |Your password must contain at least 8 characters.
      |Passwords may contain letters or numbers, and limited special characters (! @ $ % ^ & * ( ) + ?) but may not contain any spaces.
      |Password must contain at least one lowercase letter, an uppercase letter, and a number.">
        <img src="/images/icons/infoBubble.gif" />
      </span>
    </div>
   <div class="dataColumn">
      <asp:TextBox TextMode="Password" runat="server" ID="txtPassword1" CssClass="textBox password1" AutoCompleteType="None" autocomplete="off" MaxLength="32" />
    </div>
    <div class="labelColumn">
      <asp:Label ID="Label4" AssociatedControlID="txtPassword2" runat="server">Confirm <%=newPassword %>Password</asp:Label>
    </div>
   <div class="dataColumn">
      <asp:TextBox TextMode="Password" runat="server" ID="txtPassword2" CssClass="textBox password2" AutoCompleteType="None" autocomplete="off"  />
    </div>
    <div class="labelColumn">
      Password Strength
    </div>
    <div class="dataColumn">
      <span id="passwordStrength"></span><span id="passwordMatch"></span>
    </div>
<div class="labelColumn">
      <asp:Label ID="Label3" AssociatedControlID="txtFirstName" runat="server">First Name</asp:Label>
    </div>
    <div class="dataColumn">
      <asp:TextBox runat="server" ID="txtFirstName" CssClass="textBox required" />
    </div>
<div class="labelColumn">
    <asp:Label ID="Label6" AssociatedControlID="txtLastName" runat="server">Last Name</asp:Label>
    </div>
    <div class="dataColumn">
      <asp:TextBox runat="server" ID="txtLastName" CssClass="textBox required" />
    </div>
<div class="labelColumn">
    <asp:Label ID="Label9" AssociatedControlID="ddlPubRole" runat="server">Interest</asp:Label>
    </div>
    <div class="dataColumn">
      <asp:DropDownList runat="server" ID="ddlPubRole" CssClass="dropDownList" />
    </div>
  
    </div>

    <h2><asp:Label ID="lblPubProfileTitle" CssClass="label" runat="server">Author Profile (optional - requires separate approval)</asp:Label>        <span class="toolTipLink" id="pubProfTip" title="Site publishing and authoring:<ul><li>Approval is required in order to publish existing or author new resources.</li><li> An administrator with your organization (which must be an approved organization) will be contacted to verify your relationship. </li><li>Separate authorization may be granted for publishing, tagging, or authoring.</li><li>You will be notified by the administrator as to your status.</li></ul>">
        <img src="/images/icons/infoBubble.gif" alt="" /></span> </h2>
  <div class="labelColumn">
      <asp:Label ID="Label8" AssociatedControlID="txtJobTitle" runat="server">Job Title</asp:Label>
    </div>
    <div class="dataColumn">
      <asp:TextBox runat="server" ID="txtJobTitle" CssClass="textBox" />
    </div>
 <div class="labelColumn">
    <asp:Label ID="Label10" AssociatedControlID="txtProfile" runat="server">Occupation profile (ex. grades taught)</asp:Label>
      
    </div>
    <div class="dataColumn">
      <asp:TextBox runat="server" ID="txtProfile" TextMode="MultiLine" Rows="3" CssClass="textBox" />
    </div>
    <div class="labelColumn" style="vertical-align:top;">      
      <asp:Label ID="Label7"  runat="server">Organization</asp:Label>
    </div>
    <div class="dataColumn">
      <asp:TextBox ID="txtMyOrganization" ReadOnly="true" runat="server" ></asp:TextBox>
    </div>
    <div class="dataColumn" style="width:90%;">
<a href="javascript:void()" onclick="ShowHideSection('orgSection');">Request to be a member of an organization or add a new organization</a>
</div>
    <div class="dataColumn" style="width:85%;">
          <a class="iframe" style="display:none;" href="/Admin/Org/OrgSearchAdd.aspx">Request to be a member of an organization or add a new organization (under construction).</a>

        
        <div id="orgSection" class="infoMessage" style="display:none;" >
        <asp:Label ID="Label11" AssociatedControlID="txtOrganizationName" CssClass="label" runat="server">Search for your organization or request addition of a new organization (by entering the name)<br /></asp:Label>
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
 
      </div>
      
    
  <asp:HiddenField runat="server" ID="hdnWorknetId" Value="0" />
  <asp:HiddenField runat="server" ID="jValidateError" />

 
    <asp:Panel ID="pnlUpdateControls" runat="server" CssClass="dataColumn"  style="text-align: center; width:85%;" Visible="false">
        <%--CssClass="hiddenButton offScreen" 
            OnClientClick="return jValidateUpdate();"
            --%>
    <input type="button" value="Update Profile1" class="hiddenButton offScreen registerButton defaultButton"
        onclick="jValidateUpdate()" />
    <asp:Button runat="server" ID="btnUpdateProfile" CssClass="registerButton defaultButton" 
        Text="Update Profile" OnClick="btnUpdateProfile_Click" />
    </asp:Panel>
   
    </div>
    <asp:RegularExpressionValidator ID="revEmail" runat="server" EnableClientScript="False"
  Display="None" ControlToValidate="txtEmail" ErrorMessage="Invalid Email Address."
  ValidationExpression="\w+([-+.]\w+)*@\w+([-_.]\w+)*\.\w+([-_.]\w+)*"></asp:RegularExpressionValidator>
</div>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="txtCurrentView" runat="server">Registration</asp:Literal>
<asp:Literal ID="txtIsOrgRequired" runat="server">no</asp:Literal>
<asp:Literal ID="doBccOnRegistration" runat="server">yes</asp:Literal>
<asp:Literal ID="sendInfoEmailOnRegistration" runat="server">yes</asp:Literal>

<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?g={0}&a=activate</asp:Literal>

<asp:Literal ID="showingImagePath" runat="server" Visible="false">no</asp:Literal>
<asp:Literal ID="txtImageTemplate" runat="server" Visible="false"><img src='{0}' class="imgClass" alt='profile image'/></asp:Literal>


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


