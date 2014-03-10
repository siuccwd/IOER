<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibraryMemberInvite.ascx.cs" Inherits="ILPathways.Controls.Libraries.LibraryMemberInvite" %>


<style type="text/css">
.floatRoundedPanel {float:left; min-height: 300px; font-size:90%; width:300px; margin-left: 50px; }
</style>

<script language="javascript" type="text/javascript">
<!--

    function confirmAccept(recordTitle, id) {
        // ===========================================================================
        // Function to prompt user to confirm a request to delete a record
        // Note - this could be made generic if the url is passed as well

        var bresult 
        bresult = confirm("Are you sure you want to accept this invitation?\n\n"
                + "Accepting means that you agree to be added the library and share information.\n\n""
                + "Click OK to accept this invitation or click Cancel to skip the accept.");
        var loc;

        loc = self.location;

        if (bresult) {
            //alert("delete requested for id " + id + "\nlocation = " + self.location);

            //location.href="?id=" + id + "&a=delete";
            return true;
        } else {
            return false;
        }
	

    } //  

    function confirmDeny(recordTitle, id) {
        // ===========================================================================
        // Function to prompt user to confirm a request to delete a record
        // Note - this could be made generic if the url is passed as well

        var bresult 
        bresult = confirm("Are you sure you want to DENY this invitation?\n\n"
                + "Denying means that you do not wish to be added the library.\n\n""
                + "Click OK to DENY this invitation or click Cancel to skip the DENY.");
        var loc;

        loc = self.location;

        if (bresult) {
            //alert("delete requested for id " + id + "\nlocation = " + self.location);

            //location.href="?id=" + id + "&a=delete";
            return true;
        } else {
            return false;
        }
	

    } //  
    -->
</script>
<asp:Panel ID="noInviteCodePanel" runat="server" Visible="false">
<p>An invitation code was not provided</p>
</asp:Panel>

<asp:Panel ID="keysPanel" runat="server" Visible="false">
<!-- --> 
<div class="clear"></div>
  <div class="labelcolumn  requiredField" > 
    <asp:label id="lblLibraryId"  associatedcontrolid="txtLibraryId" runat="server">LibraryId</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox id="txtLibraryId" runat="server">0</asp:textbox>
  </div>
<!-- --> 
<div class="clear"></div>
  <div class="labelcolumn " > 
    <asp:label id="lblRowId"  associatedcontrolid="txtRowId" runat="server">RowId</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox id="txtRowId" runat="server"></asp:textbox>
  </div>
</asp:Panel>

<asp:Panel ID="passcodePanel" runat="server" Visible="false">
<!-- -->
		<h2>Library Invitation Passcode</h2>
		<br class="clearFloat" />	
		<div class="labelcolumn"><asp:label id="Label1" runat="server">Enter the passcode provided by your contact/instructor</asp:label>
		</div>	
		<div class="dataColumn" ><asp:TextBox ID="txtPasscode" runat="server"  AutoCompleteType="disabled"   MaxLength="25"></asp:TextBox>
		</div>
		<!-- -->
		<br class="clearFloat" />	
		<div class="labelcolumn" style="margin-top: 20px;">&nbsp;</div>
		<div class="dataColumn">			
				<asp:button id="btnValidateCode" runat="server" CssClass="defaultButton" width="200px" Text="Validate Code"		
					CausesValidation="true" OnClick="btnValidateCode_Click"></asp:button>&nbsp;&nbsp;&nbsp;

		</div>	
</asp:Panel>

<asp:Panel ID="acceptPanel" runat="server" Visible="false">
<div class="clear"></div>
    <!-- -->
    <h2>Library Member Invitation</h2>

    <br class="clearFloat" />
    <div class="dataColumn">
        <asp:Label ID="lblInviteSummary" runat="server"></asp:Label>
    </div>
    <br class="clearFloat" />
    <div class="dataColumn" style="margin-top: 20px;">
        <asp:Label ID="lblInvite" runat="server">Click the <strong>Accept Invitation</strong> button to accept this invitation and be added to the library <br />OR click the <strong>Deny Invitation</strong> button to NOT accept this invitation</asp:Label>
    </div>
    <!-- -->
    <br class="clearFloat" />
    <div class="labelcolumn" style="margin-top: 20px;">&nbsp;</div>
    <div class="dataColumn">
        <asp:Button ID="btnAccept" runat="server" CssClass="defaultButton" Width="200px" Text="Accept Invitation"
            CausesValidation="true" OnClick="btnAccept_Click"></asp:Button>&nbsp;&nbsp;&nbsp;
				<asp:Button ID="btnDeny" runat="server" CssClass="defaultButton" Width="200px" Text="Deny Invitation"
                    CausesValidation="False" OnClick="btnDeny_Click"></asp:Button>


    </div>
</asp:Panel>


<asp:Panel ID="confirmationPanel" runat="server" Visible="false">
<h2>Thank you</h2>
<p>Next steps:</p>
</asp:Panel>


<asp:Panel ID="authCheckPanel" runat="server" Visible="false">
<div class="clear"></div>
<h2>Authorization Required</h2>
<p>You must have an Illinois workNet account in order to accept this invitation. </p> 

    <div id="RoundedPanel1"  >
        <h2>Do you have an Illinois workNet account?</h2>
    <p>If you have an account, <asp:HyperLink ID="loginLink" runat="server" NavigateUrl="/vos_portal/residents/en/admin/login.htm?nextUrl=">Login now</asp:HyperLink></p>
    <p>After logging in, you will be returned to this page.</p>
    <br />
    </div>
    
    <wcl:RoundedPanel ID="RoundedPanel2" runat="server" PanelBorderColor="#003aaa"  CssClass="floatRoundedPanel" Text="Don't have an Illinois workNet account?" >
    <p>If you do not have an account, <asp:HyperLink ID="registerLink" runat="server" NavigateUrl="/vos_portal/residents/en/admin/registration.htm?nextUrl=">click here to set up a free Illinois workNet account</asp:HyperLink></p>			
      
    <p>After successfully registering, you will be returned to this page.</p>
    </wcl:RoundedPanel>
    <div class="clear"></div>
</asp:Panel>  



<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="responseEmailCode" runat="server" >customer_Library_InvitationResponse</asp:Literal>

<asp:Literal ID="inviteUrl" runat="server" >/libraries/invitation</asp:Literal>
<asp:Literal ID="groupInviteUrl" runat="server" >/libraries/LibraryInvitation</asp:Literal>

<asp:Literal ID="profileUrl" runat="server" >/Account/profile.htm?authenticated=yes&</asp:Literal>
<asp:Literal ID="profileReturnUrl" runat="server" ></asp:Literal>
<asp:Literal ID="profileReturnTemplate" runat="server" ><a href='{0}'>Click here to update your profile.</a></asp:Literal>

<asp:Literal ID="afterAcceptUrl" runat="server" >/libraries/</asp:Literal>
<asp:Literal ID="acceptConfirmationMessage" runat="server" >Thank you for accepting this invitation. <br />Your response has been sent to {0}. You have been directed to the My ??? page. <br /> <br />
<ul>
<li>One</li>
<li>Two</li>
<li>Three.</li>
</ul> </asp:Literal>

<asp:Literal ID="denyConfirmationMessage" runat="server" >We are sorry that you could not accept this invitation. <br />Your response has been sent to {0}. You have been directed to the ?????. <br />Your My Illinois workNet activities are below. <br /></asp:Literal>

</asp:Panel>
