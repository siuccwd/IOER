<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GroupDetail.ascx.cs" Inherits="ILPathways.Controls.GroupsManagment.GroupDetail" %>


<script language="javascript" type="text/javascript" >
  // ===========================================================================
  function confirmDelete(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to delete a record
    // Note - this could be made generic if the url is passed as well
    // ===========================================================================

    var bresult
    bresult = confirm("Are you sure you want to delete this group? \n\nYou must be sure the group is not being used in production before deleting!\n\n"
            + "Click OK to delete this group or click Cancel to skip the delete.");
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


  } 
</script>
<asp:validationsummary id="vsErrorSummary" ValidationGroup="groupValGroup" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>


<asp:panel id="groupPanel" runat="server" cssclass="menuBranch"  visible="true" width="100%">				
<!-- --> 
	<div class="clearFloat"></div>
  <div class="labelColumn" > 
    <asp:label id="lblId"  associatedcontrolid="txtId" runat="server">id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId"  runat="server">0</asp:label>  
	</div>

<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn requiredField" > 
    <asp:label id="lblGroupName"  associatedcontrolid="groupName" runat="server">Title</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox id="groupName" width="400px" MaxLength="75" ValidationGroup="groupValGroup" runat="server"></asp:textbox>

  </div>
<asp:Panel ID="groupCodePanel" runat="server" Visible="false">
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn requiredField" > 
    <asp:label id="lblGroupCode"  associatedcontrolid="groupCode" runat="server">Group Code</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox id="groupCode" width="400px"  MaxLength="50"  ValidationGroup="groupValGroup" runat="server"></asp:textbox>
      <asp:Image ID="lockGroupCode" runat="server" AlternateText="Field locked" ImageUrl="/images/icons/sslicon.gif" Visible="false" />
    
  </div>
</asp:Panel>  
<asp:Panel ID="programPanel" runat="server" Visible="false">
<!-- --> 
	<div class="clearFloat"></div>			
  <div class="labelColumn" >Online Program</div>	
  <div class="dataColumn">   		
		<asp:DropDownList id="ddlOnlineProgram" runat="server" Width="80%" ></asp:DropDownList>
		<asp:label id="lblOnlineProgram" visible="false" runat="server"></asp:label> <asp:label id="lblProgramId" visible="false" runat="server"></asp:label> 
  </div> 
</asp:Panel>  
<asp:Panel ID="groupTypePanel" runat="server" Visible="false">  
<!-- --> 
<div class="clearFloat"></div>  
  <div class="labelColumn requiredField" >Group Type</div>	
  <div class="dataColumn">   
		<asp:DropDownList		 id="ddlGroupType" ValidationGroup="groupValGroup" runat="server" Width="200px"></asp:DropDownList>
  </div> 
 </asp:Panel>   
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn requiredField" > 
    <asp:label id="lblDescription"  associatedcontrolid="txtDescription" runat="server">Description</asp:label> 
  </div>
  <div class="dataColumn"> 		
  <asp:TextBox id="txtDescription" ValidationGroup="groupValGroup" TextMode="MultiLine" Rows="3" runat="server" MaxLength="300"   Width="400px"></asp:TextBox>
  </div>  
	<!-- --> 
	<asp:Panel ID="contactPanel" runat="server" Visible="false">
		<div class="clearFloat"></div>
		<div class="labelColumn" > 
		<asp:label id="Label1"  associatedcontrolid="lblContact" runat="server">Primary Contact</asp:label> 
		</div>
		<div class="dataColumn"> 
			<asp:label id="lblContact"  runat="server"></asp:label><br />
			<asp:label id="lblContactOrg"  runat="server"></asp:label> 
		</div>  
  </asp:Panel>
	<!-- --> 
	<div class="clearFloat"></div>
	<div class="labelColumn requiredField" > 
	<asp:label id="lblIsActive"  associatedcontrolid="rbIsActive" runat="server">Is Active</asp:label> 
	</div>
	<div class="dataColumn"> 
	<asp:RadioButtonList id="rbIsActive" autopostback="false" causesvalidation="false"   runat="server" tooltip="Is Active" RepeatDirection="Horizontal">
		<asp:ListItem Text="Yes"  value="Yes" Selected="True"></asp:ListItem>    
		<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
	</asp:RadioButtonList>
  </div>
<asp:Panel ID="hasPrivateCredentialsPanel" runat="server" Visible="false">    
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="lblHasPrivateCredentials"  associatedcontrolid="rbHasPrivateCredentials" runat="server">Has Private Credentials</asp:label> 
  </div>
  <div class="dataColumn">   
    <asp:RadioButtonList id="rbHasPrivateCredentials" autopostback="false" causesvalidation="false"   runat="server" tooltip="Has Private Credentials" RepeatDirection="Horizontal">
			<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>    
 			<asp:ListItem Text="No" 	value="No" Selected="true"  ></asp:ListItem>
		</asp:RadioButtonList>
	</div>
</asp:Panel>
 
 <asp:Panel ID="OrganizationPanel" runat="server" Visible="false"> 
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn" > 
    <asp:label id="lblOrgId"  associatedcontrolid="ddlOrgList" runat="server">Organization</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlOrgList" ValidationGroup="groupValGroup" runat="server"></asp:dropdownlist>
  </div>

</asp:panel>
<!-- --> 
<div class="clearFloat"  style="display:none">
  <div class="labelColumn" > 
    <asp:label id="lblParentGroupId"  associatedcontrolid="parentGroupId" runat="server">ParentGroupId</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox id="parentGroupId" ValidationGroup="groupValGroup" runat="server"></asp:textbox>&nbsp;
        <asp:dropdownlist id="ddlParentGroupList" ValidationGroup="groupValGroup" runat="server"></asp:dropdownlist>

  </div>
</div>
<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn">
		<asp:label id="Label12" runat="server">History</asp:label>
		</div>
		<div class="dataColumn">
			<asp:label id="lblHistory" runat="server" Width="100%" ></asp:label>
		</div>
		<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn">&nbsp;</div>
		<div class="dataColumn">			
				<asp:button id="btnSave" ValidationGroup="groupValGroup" runat="server" CssClass="defaultButton" width="100px" CommandName="Update" 
				OnCommand="FormButton_Click" Text="Save"		CausesValidation="true"></asp:button>
				<asp:button id="btnDelete" runat="server" CssClass="defaultButton" width="100px" CommandName="Delete" 
				OnCommand="FormButton_Click" Text="Delete"	CausesValidation="False" visible="false"></asp:button> 
				<asp:button id="btnNew" runat="server" CssClass="defaultButton" width="100px" CommandName="New" 
				OnCommand="FormButton_Click" Text="New Group" causesvalidation="false"></asp:button>				
				<br /><asp:button id="btnUpdateOwner" runat="server" CssClass="defaultButton"  CommandName="UpdateOwner" 
				OnCommand="FormButton_Click" Text="Change Group Owner"	CausesValidation="False" visible="false"></asp:button>					<asp:button id="btnCopyGroup" runat="server" CssClass="defaultButton" width="150px" CommandName="CopyGroup" 
				OnCommand="FormButton_Click" Text="Copy Group"	CausesValidation="False" visible="false"></asp:button>				
		</div>		
</asp:panel>

<div>
	<asp:RequiredFieldValidator id="rfvGroupName" runat="server" ValidationGroup="groupValGroup" ControlToValidate="groupName" Display="None"
	EnableClientScript="false" ErrorMessage="Group name is missing.">
	</asp:RequiredFieldValidator>      
	<asp:RequiredFieldValidator id="rfvGroupCode" runat="server" ValidationGroup="groupValGroup" ControlToValidate="groupCode" Display="None"
	EnableClientScript="false" ErrorMessage="Group code is missing.">
	</asp:RequiredFieldValidator>  
	<asp:RequiredFieldValidator id="rfvGroupType" runat="server" ValidationGroup="groupValGroup" ControlToValidate="ddlGroupType" Display="None"
	EnableClientScript="false" ErrorMessage="Group type is missing.">
	</asp:RequiredFieldValidator>   
	<asp:RequiredFieldValidator id="rfvDescription" runat="server" ValidationGroup="groupValGroup" ControlToValidate="txtDescription" Display="None"
	EnableClientScript="false" ErrorMessage="Group description is missing.">
	</asp:RequiredFieldValidator>  	 			
</div>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.controls.GroupsManagement.GroupDetail</asp:Literal>
</asp:Panel>