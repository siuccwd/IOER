<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Registration2.ascx.cs" Inherits="ILPathways.Account.controls.Registration2" %>

<script type="text/javascript">
  //Startup
  $(document).ready(function () {
    $("form").removeAttr("onsubmit");
    setupValidations();
  });

  function setupValidations() {
    //Email
    $(".email, .confirmEmail").on("keyup change", function () {
      doDoubleValidation(validations.email, validations.confirmEmail, groups.email);
    });

    //Password
    $(".password, .confirmPassword").on("keyup change", function () {
      doDoubleValidation(validations.password, validations.confirmPassword, groups.password);
    });

    //Name
    $(".firstName, .lastName").on("keyup change", function () {
      validateText(validations[$(this).attr("data-name")]);
    });

    updateValidations();
  }

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

  function successValidateText(result, group) {
    var message = $(group.message);
    if (result.isValid) {
      setValid(group, true);
      setVM(message, "green", group.data.fieldTitle + " is okay.");
    }
    else {
      setValid(group, false);
      setVM(message, "red", result.status);
    }
    updateValidations();
  }

</script>
<script type="text/javascript">
  //Validation
  var groups = {
    email: { valid: false, message: "#validation_email", minLength: 6, valid: false, timer: null, method: "ValidateEmail", data: { text: "", fieldTitle: "Email", mustBeNew: true }, success: successValidateText },
    password: { valid: false, message: "#validation_password", valid: false, timer: null, method: "ValidatePassword", data: { text: "", fieldTitle: "Password" }, success: successValidateText },
    firstName: { valid: false, message: "#validation_firstName", minLength: 1, valid: false, timer: null, method: "ValidateName", data: { text: "", fieldTitle: "First Name" }, success: successValidateText },
    lastName: { valid: false, message: "#validation_lastName", minLength: 1, valid: false, timer: null, method: "ValidateName", data: { text: "", fieldTitle: "Last Name" }, success: successValidateText }
  }
  var validations = {
    email: { name: "email", selector: ".email", group: groups.email, oldVal: "" },
    confirmEmail: { name: "confirmEmail", selector: ".confirmEmail", group: groups.email, oldVal: "" },
    password: { name: "password", selector: ".password", minLength: 6, group: groups.password, oldVal: "" },
    confirmPassword: { name: "confirmPassword", selector: ".confirmPassword", group: groups.password, oldVal: "" },
    firstName: { name: "firstName", selector: ".firstName", group: groups.firstName, oldVal: "" },
    lastName: { name: "lastName", selector: ".lastName", group: groups.lastName, oldVal: "" },
  };

  function setVM(box, css, text){
    box.attr("class", "vm " + css).html(text);
  }

  function doDoubleValidation(name1, name2, group) {
    var box1 = $(name1.selector);
    var box2 = $(name2.selector);
    var message = $(group.message);
    var val1 = box1.val();
    var val2 = box2.val();

    if (val1.length == 0 && val2.length == 0) {
      setValid(group, false);
      setVM(message, "", "");
      return;
    }

    if (box1.val() == box2.val()) {
      validateText(name1);
      validateText(name2);
    }
    else {
      setValid(group, false);
      name1.oldVal = box1.val();
      name2.oldVal = box2.val();
      setVM(message, "red", group.data.fieldTitle + "s do not match.");
    }
  }

  function validateText(data) {
    var box = $(data.selector);
    var val = box.val();
    var group = data.group;
    var message = $(group.message);

    if (data.oldVal != val) { //If the value changed...
      data.oldVal = val;

      if (val.length < group.minLength) { //First test the length.
        setValid(group, false);
        setVM(message, "red", "Please enter at least " + (group.minLength - val.length) + " more characters.");
        return;
      }

      setValid(group, false); //Send the data off for validation.
      setVM(message, "", "Checking " + group.data.fieldTitle + "...");
      group.data.text = val;

      clearTimeout(group.timer); //Then after a certain time...
      group.timer = setTimeout(function () {
        DoAjax(group.method, group.data, group.success, group);
      }, 800);
    }
  }

  function setValid(item, valid) {
    item.valid = valid;
    updateValidations();
  }

  function colorize(group) {
    var message = $(group.message);

    //Colorize boxes according to validity
    for (i in validations) {
      if (validations[i].group == group) {
        var box = $(validations[i].selector);
        if (box.val().length > 0 && group.valid) {
          box.attr("data-valid", "valid");
        }
        else if (box.val().length > 0 && !group.valid) {
          box.attr("data-valid", "invalid");
        }
        else {
          box.attr("data-valid", "neutral");
        }
      }
    }
  }

  function updateValidations() {
    var valid = true;
    for (i in groups) {
      if (!groups[i].valid) {
        valid = false;
      }
      colorize(groups[i]);
    }

    if (valid) {
      $(".btnSubmit").removeAttr("disabled");
      $("#preButtonMessage").hide();
    }
    else {
      //$(".btnSubmit").attr("disabled", "disabled");
      $("#preButtonMessage").show();
    }

    return valid;
  }

  function validatePage() {
    if (updateValidations()) {
      $("#preButtonMessage").hide();
      $("#processing").show();
      $(".btnSubmit").hide();
      $("form").removeAttr("onsubmit");
    }
  }

</script>
<link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
<style type="text/css">
  /* Big Stuff */
  .step { max-width: 600px; margin: 10px auto; text-align: center; }
  .step label { width: 25%; text-align: right; padding-right: 10px; font-weight: bold; }
  .step input[type=submit] { transition: height 0.5s, margin 0.5s, padding 0.5s, opacity 0.5s; height: auto; margin: inherit; padding: 2px; opacity: 1; }
  .step input[type=text], .step input[type=password] { width: 73%; }
  .step label, .step input { margin-bottom: 10px; }

  p.intro { font-size: 20px; text-align: center; }
  .infoMessage { width: 800px; margin: 5px auto; }
  p.vm { text-align: left; height: 1.3em; padding: 0 5px 5px 25%; margin-top: 0; font-style: italic; color: #555; transition: height 1s; -webkit-transition: height 1s; font-size: 20px; }
  p.vm.red { color: #B03D25; }
  p.vm.green { color: #4AA394; }
  p.final { font-size: 16px; text-align: center; padding: 5px 5px 10px 5px; color: #B03D25; font-weight: bold; }
  p.vm:empty { height: 0.2em; }
  p.requiredMessage { font-weight: bold; color: #B03D25; }

  #processing, #preButtonMessage { display: none; text-align: center; font-weight: bold; font-size: 18px; padding: 10px; }
  .step input.btnSubmit[disabled=disabled] { height: 0; margin: 0; padding: 0; opacity: 0; }
  input[data-valid=valid] { border: 1px solid #4AA394; }
  input[data-valid=invalid] { border: 1px solid #B03D25; }
  input[data-valid=neutral] { }

  @media screen and (max-width: 700px){
    .step label { display: block; width: 100%; padding-left: 5px; text-align: left; margin-bottom: 0; height: 20px; line-height: 20px; }
    .step input[type=text], .step input[type=password] { width: 100%; display: block; }
    .step input[type=submit] { font-size: 20px; }

  .infoMessage { width: 90%; margin: 5px auto; }
  }

</style>

<div id="content">
  <h1 class="isleH1">IOER Registration</h1>
  <p class="intro">An IOER account will allow you to create, share, evaluate, and collaborate on career and education Resources.
  </p>
    <div id="confirmMsg" class="infoMessage" >
      
            <h2>A confirmation of your email address is required.</h2>
            <ul style="text-align:left; margin-left: 50px;">
                <li>An email will be sent to the entered email address with a link to complete registration.</li>
                <li>Upon completing registration, your account will be activated.</li>
                <li>If you do not receive an email, be sure to check your junk email folder.</li>
            </ul>

    </div>



  <asp:Label ID="regMessage" Visible="false" CssClass="infoMessage" runat="server" ></asp:Label>
  <div class="grayBox step bigText">
    <p class="requiredMessage">All fields are required.</p>
    <label>Email Address</label><input type="text" id="email" class="email" runat="server" />
    <label>Confirm Email</label><input type="text" id="confirmEmail" class="confirmEmail" runat="server" />    
    <p id="validation_email" class="vm"></p>
    <label>Password</label><input type="password" id="password" class="password" runat="server" autocomplete="off" />
    <label>Confirm Password</label><input type="password" id="confirmPassword" class="confirmPassword" runat="server" autocomplete="off" />
    <p id="validation_password" class="vm"></p>
    <label>First Name</label><input type="text" id="firstName" class="firstName" data-name="firstName" runat="server" />
    <p id="validation_firstName" class="vm"></p>
    <label>Last Name</label><input type="text" id="lastName" class="lastName" data-name="lastName" runat="server" />
    <p id="validation_lastName" class="vm"></p>
  </div>
  <div class="grayBox step bigText">
    <p class="final">By creating an account, you confirm you are 13 years old or older, and you agree to the ISLE <a class="textLink" href="//ilsharedlearning.org/Pages/ISLE-Privacy-Policy.aspx" target="_blank">Privacy Policy</a> and <a class="textLink" href="//ilsharedlearning.org/Pages/ISLE-Terms-of-Use.aspx">Terms of Use</a>.</p>
    <asp:Button ID="btnSubmit" runat="server" OnClick="btnSubmit_Click" OnClientClick="validatePage()" CssClass="btnSubmit isleButton bgGreen" Text="I Agree. Create My Account!" />
    <p id="processing">Processing, Please wait...</p>
    <p id="preButtonMessage">One or more of the required fields above hasn't been completed yet. Please double-check!</p>
  </div>

</div>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="doingBccOnRegistration" runat="server">yes</asp:Literal>
<asp:Literal ID="sendInfoEmailOnRegistration" runat="server">yes</asp:Literal>
<asp:Literal ID="doImmediateConfirm" runat="server">no</asp:Literal>
<asp:Literal ID="prefillingEmailIfFound" runat="server">no</asp:Literal>
<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?g={0}&a=activate</asp:Literal>
<asp:Literal ID="autoActivateLink" runat="server" >/Account/Login.aspx?g={0}&a=autoactivate</asp:Literal>
<asp:Literal ID="activateLink1" runat="server" >/Account/Profile.aspx?g={0}&a=activate</asp:Literal>
<asp:Literal ID="txtReturnUrl" runat="server" Visible="false">/</asp:Literal>
<asp:Label ID="registerSuccessMsg" runat="server"><span style="background-color: #fff;color: #000;">A confirmation of your email address is required.</span> <ul><li>An email was sent to your address with a link to complete registration.</li><li>Upon completing registration your account will be activated.</li><li>If you do not receive an email, be sure to check your junk mail folder.</li></ul></asp:Label>
<!-- The MS-Word formatted message. Needs to be tested -->
<asp:Label ID="confirmMessage" runat="server">
  <div>
    <div>
      <h1 style='background:#4F4E4F'><span style='font-family:"Calibri","sans-serif";
      mso-fareast-font-family:Calibri;mso-fareast-theme-font:minor-latin;color:#f5f5f5'><span
      style='mso-spacerun:yes'> </span>Thank you for using Illinois Open Education
      Resources!<o:p></o:p></span></h1>
      <div style="background:#f5f5f5;background-color:#f5f5f5">
        <p style='background:#f5f5f5'><span style='font-family:"Calibri","sans-serif"'><span
        style='mso-spacerun:yes'> </span>Hello, {0}<o:p></o:p></span></p>
        <p style='text-indent:.5in;background:#f5f5f5'><span style='font-family:"Calibri","sans-serif"'>Please
        click the link below to confirm your email address and activate your IOER
        account.<o:p></o:p></span></p>
        <p style='text-indent:.5in;background:#f5f5f5'><span style='font-family:"Calibri","sans-serif"'><a
        href="{1}">Activate my IOER account!</a><o:p></o:p></span></p>
        <p style='background:#f5f5f5'><span style='font-family:"Calibri","sans-serif"'><span
        style='mso-spacerun:yes'> </span>Sincerely, the IOER Team<o:p></o:p></span></p>
        <p style='background:#f5f5f5'><span style='font-family:"Calibri","sans-serif"'><o:p>&nbsp;</o:p></span></p>
      </div>
    </div>
  </div>
</asp:Label>

</asp:Panel>
