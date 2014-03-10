<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UserProfile2.ascx.cs" Inherits="ILPathways.Account.controls.UserProfile2" %>

<script type="text/javascript">
  $(document).ready(function () {
    setupValidations();
  });


  function setupValidations() {
    //Prevent the dreaded press-enter thing
    $("input[type=text]").on("keydown", function (event) {
      if (event.keyCode == 13 || event.which == 13) {
        event.preventDefault();
      }
    });

    $("#email1, #email2").on("keyup change", function () {
      doDoubleValidation($("#email1"), $("#email2"), validations.email);
    });
    $("#password1, #password2").on("keyup change", function () {
      doDoubleValidation($("#password1"), $("#password2"), validations.password);
    });
    $("#firstName").on("keyup change", function () {
      validateText($("#firstName"), validations.firstName);
    });
    $("#lastName").on("keyup change", function () {
      validateText($("#lastName"), validations.lastName);
    });
    $("#jobTitle").on("keyup change", function () {
      validateText($("#jobTitle"), validations.lastName);
    });
    $("#profile").on("keyup change", function () {
      validateText($("#profile"), validations.lastName);
    });

  }

  function doDoubleValidation(box1, box2, data) {
    var messageBox = $(data.message);
    var val1 = box1.val();
    var val2 = box2.val();

    if (val1.length == 0 && val2.length == 0) {
      data.valid = false;
      updateValidations(data.group);
      return;
    }

    if (val1 == val2) {
      validateText(box1, data);
    }
    else {
      data.valid = false;
      updateValidations(data.group);
      setVM(messageBox, "red", data.data.fieldTitle + "s do not match.");
    }
  }

  function validateText(box, data) {
    var val = box.val();
    var messageBox = $(data.message);

    if (val == data.oldVal) {
      return;
    }

    data.valid = false;
    updateValidations(data.group);

    if (data.oldVal != val) {
      if (val.length < data.minLength) {
        setVM(messageBox, "red", "Please enter at least " + val.minLength - val.length + " more characters.");
      }

      data.data.text = val;
      setVM(messageBox, "", "Checking " + data.data.fieldTitle + "...");

      clearTimeout(data.timer);
      data.timer = setTimeout(function () {
        DoAjax(data.method, data.data, data.success, data);
      }, 800);
    }
  }

  function updateValidations(group) {
    var isValid = true;
    for (i in validations) {
      if (validations[i].group == group){
        if (!validations[i].valid) {
          isValid = false;
        }
      }
    }
    return isValid;
  }

  function validateAccount() {
    return updateValidations("#account");
  }

  function validateAvatar() {
    return $(".fileAvatar").val().length > 0;
  }

  function validateProfile() {
    return updateValidations("#profile");
  }

  function setVM(box, color, text) {
    box.attr("class", "vm " + color).html(text);
  }
</script>
<script>
  //AJAX Methods
  function DoAjax(method, data, success, passThruJSON) {
    $.ajax({
      url: "/Services/UtilityService.asmx/" + method,
      async: true,
      success: function (msg) {
        try {
          success($.parseJSON(msg.d), passThruJSON);
        }
        catch (e) {
          success(msg.d, passThruJSON);
        }
      },
      type: "POST",
      data: JSON.stringify(data),
      dataType: "json",
      contentType: "application/json; charset=utf-8"
    });
  }

  function successValidateText(result, data) {
    var message = $(data.message);
    console.log(result);
    if (result.isValid) {
      data.valid = true;
      setVM(message, "green", data.data.fieldTitle + " is okay.");
    }
    else {
      data.valid = false;
      setVM(message, "red", result.status);
    }
    updateValidations(data.group);
  }

  var validations = {
    email: { group: "#account", message: "#validation_email", minLength: 6, valid: false, timer: null, method: "ValidateEmail", data: { text: "", fieldTitle: "Email", mustBeNew: true }, success: successValidateText, oldVal: "" },
    password: { group: "#account", message: "#validation_password", minLength: 5, valid: false, timer: null, method: "ValidatePassword", data: { text: "", fieldTitle: "Password" }, success: successValidateText, oldVal: "" },
    firstName: { group: "#account", message: "#validation_firstName", minLength: 1, valid: false, timer: null, method: "ValidateName", data: { text: "", fieldTitle: "First Name" }, success: successValidateText, oldVal: "" },
    lastName: { group: "#account", message: "#validation_lastName", minLength: 1, valid: false, timer: null, method: "ValidateName", data: { text: "", fieldTitle: "Last Name" }, success: successValidateText, oldVal: "" },
    jobTitle: { group: "#profile", message: "#validation_jobTitle", minLength: 5, valid: false, timer: null, method: "ValidateText", data: { text: "", fieldTitle: "Job Title" }, success: successValidateText, oldVal: "" },
    profile: { group: "#profile", message: "#validation_profile", minLength: 20, valid: false, timer: null, method: "ValidateText", data: { text: "", fieldTitle: "Profile" }, success: successValidateText, oldVal: "" }
  };

</script>

<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<style type="text/css">
  #content { text-align: center; }
  .column { display: inline-block; vertical-align: top; width: 47%; margin: 10px 1%; text-align: left; max-width: 600px; }
  .grayBox { margin-bottom: 10px; }

  .bigText label { width: 35%; text-align: right; }
  .bigText input, .bigText select, .bigText textarea { width: 64%; margin-bottom: 10px; }
  .bigText input[type=submit] { width: 100%; }
  #avatar p { display: inline; }
  #avatar img { max-width: 140px; max-height: 140px; height: 140px; width: 140px; display: inline-block; float: left; margin-right: 10px; border-radius: 5px; }
  #avatar #clearFix { clear: both; }
  p.vm { text-align: left; max-height: 3em; padding: 0 5px 5px 35%; margin-top: 0; font-style: italic; color: #555; transition: max-height 1s; -webkit-transition: max-height 1s; font-size: 20px; }
  p.vm.red { color: #B03D25; }
  p.vm.green { color: #4AA394; }
  p.vm:empty { max-height: 0.2em; }


  @media screen and (max-width: 925px) {
    .bigText label { width: 100%; text-align: left; height: 20px; line-height: 20px; }
    .bigText input, .bigText select, .bigText textarea { width: 100%; }
    p.vm { padding-left: 5px; }
  }
  @media screen and (max-width: 600px) {
    .column { width: 100%; margin: 10px 0; }
  }
</style>

<div id="content">
  <h1 class="isleH1">My Account &amp; Profile</h1>
  <div class="column">
    <div class="grayBox bigText" id="account">
      <!-- Email, Password, Name -->
      <h2 class="header">Account Information</h2>
      <label>New Email Address</label><input type="text" id="email1" data-validation="email" />
      <label>Confirm New Email</label><input type="text" id="email2" data-validation="email" />
      <p class="vm" id="validation_email" data-validation="email"></p>
      <label>New Password</label><input type="password" id="password1" data-validation="password" />
      <label>Confirm Password</label><input type="password" id="password2" data-validation="password" />
      <p class="vm" id="validation_password" data-validation="password"></p>
      <label>Change First Name</label><input type="text" id="firstName" data-validation="firstName" />
      <p class="vm" id="validation_firstName" data-validation="firstName"></p>
      <label>Change Last Name</label><input type="text" id="lastName" data-validation="lastName" />
      <p class="vm" id="validation_lastName" data-validation="lastName"></p>
      <asp:Button runat="server" ID="btnUpdateAccount" OnClick="btnUpdateAccount_Click" Text="Update Account" CssClass="isleButton bgGreen" OnClientClick="validateAccount()" />
    </div>
  </div><!-- /account -->
  <div class="column">
    <div class="grayBox bigText" id="avatar">
      <!-- Avatar -->
      <h2 class="header">Profile Image</h2>
      <img src="" />
      <p>Profile Images should be square, roughly 140x140 pixels.</p>
      <div id="clearFix"></div>
      <label>Change Image</label><asp:FileUpload ID="fileAvatar" CssClass="fileAvatar" runat="server" />
      <asp:Button runat="server" ID="btnUpdateAvatar" OnClick="btnUpdateAvatar_Click" Text="Update Image" CssClass="isleButton bgGreen" OnClientClick="validateAvatar()" />
    </div><!-- /avatar -->
    <div class="grayBox bigText" id="profile">
      <!-- Interest, Org, Title, Desc, Image -->
      <h2 class="header">Profile Information</h2>
      <label>Site Role</label><asp:DropDownList runat="server" ID="ddlPubRole" CssClass="ddlPubRole" />
      <label>Organization</label><asp:TextBox ID="txtUserOrganization" Text="None" ReadOnly="true" Enabled="false" runat="server" />
      <label>Job Title</label><input type="text" id="jobTitle" data-validation="jobTitle" />
      <p class="vm" id="validation_jobTitle" data-validatio="jobTitle"></p>
      <label>Job Profile</label><asp:TextBox runat="server" ID="txtProfile" TextMode="MultiLine" Rows="3" CssClass="txtProfile" />
      <p class="vm" id="validation_profile" data-validation="profile"></p>
      <asp:Button runat="server" ID="btnUpdateProfile" OnClick="btnUpdateProfile_Click" Text="Update Profile" CssClass="isleButton bgGreen" OnClientClick="validateProfile()" />
    </div>
  </div><!-- /profile -->
</div>