<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrganizationMgmt.ascx.cs" Inherits="ILPathways.Controls.OrgMgmt.OrganizationMgmt" %>

<style type="text/css">
    .labelColumn { width: 25%; }
    .dataColumn { width: 55%; }
    
    #thisContainer {  width: 900px; min-height:300px; margin: 50px auto;}

  @media screen and (max-width: 700px){

    .labelColumn { width: 65%; }
    .dataColumn { width: 85%; }
  }

</style>
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />
<div id="thisContainer">
<asp:Panel ID="detailPanel" runat="server">
<!-- --> 
<asp:Panel ID="adminPanel" runat="server" Visible="false">

  <div class="labelColumn " > 
    <asp:label id="lblId"  associatedcontrolid="txtId" runat="server">id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId" runat="server"></asp:label>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblIsActive"  associatedcontrolid="rblIsActive" runat="server">Is Active</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsActive" autopostback="false" causesvalidation="false"   runat="server" tooltip="[tooltip]" RepeatDirection="Horizontal">
<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>
<asp:ListItem Text="No"   value="No"></asp:ListItem>
</asp:RadioButtonList>
  </div>

    </asp:Panel>

<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblName"  associatedcontrolid="txtName" runat="server">Organization</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="100"  id="txtName" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblOrgTypeId"  associatedcontrolid="ddlOrgTypeId" runat="server">Organization Type</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlOrgTypeId" runat="server"></asp:dropdownlist>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblMainPhone"  associatedcontrolid="txtMainPhone" runat="server">Main Phone</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="20"  id="txtMainPhone" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblMainExtension"  associatedcontrolid="txtMainExtension" runat="server">Extension</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="10"  id="txtMainExtension" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblFax"  associatedcontrolid="txtFax" runat="server">Fax</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="10"  id="txtFax" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblAddress"  associatedcontrolid="txtAddress1" runat="server">Address</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtAddress1" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblAddress2"  associatedcontrolid="txtAddress2" runat="server">Address2</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtAddress2" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblCity"  associatedcontrolid="txtCity" runat="server">City</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtCity" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblState"  associatedcontrolid="ddlState" runat="server">State</asp:label> 
  </div>
  <div class="dataColumn"> 
      <asp:label  id="txtState" runat="server" Visible="false">IL</asp:label>
    <asp:dropdownlist  id="ddlState" runat="server" Visible="true"></asp:dropdownlist>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblZipcode"  associatedcontrolid="txtZipcode" runat="server">Zipcode</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="5"  id="txtZipcode" runat="server"></asp:textbox> -
      <asp:textbox  maxLength="4"  id="txtZipcodePlus4" runat="server"></asp:textbox>
  </div>
<!-- -->
<asp:Panel ID="historyPanel" runat="server" Visible="true">
		<div class="clearFloat"></div>	
		<div class="labelColumn">
		<asp:label id="Label12" runat="server">History</asp:label>
		</div>
		<div class="dataColumn">
			<asp:label id="lblHistory" runat="server"></asp:label>
		</div>
</asp:Panel>
<!-- --> 
		<div class="clearFloat"></div>	
		<div class="labelColumn">&nbsp;</div>
		<div class="dataColumn">			
				<asp:button id="btnSave" runat="server" CssClass="defaultButton" width="150px" CommandName="Update" 
				OnCommand="FormButton_Click" Text="Save"		CausesValidation="true"></asp:button>
				<asp:button id="btnDelete" runat="server" CssClass="defaultButton" width="150px" CommandName="Delete" 
				OnCommand="FormButton_Click" Text="Delete"	CausesValidation="False" visible="false"></asp:button> 
				<asp:button id="btnNew" runat="server" style="margin-left: 10px;" CssClass="defaultButton" width="150px" CommandName="New" visible="false"
				OnCommand="FormButton_Click" Text="New Record" causesvalidation="false"></asp:button>

		</div>
</asp:Panel>
</div>

<div>
<!-- validators -->
<asp:requiredfieldvalidator id="rfvTitle" Display="None" ControlToValidate="txtName" ErrorMessage="Name is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvMainPhone" Display="None" ControlToValidate="txtMainPhone" ErrorMessage="Phone is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvAddress" Display="None" ControlToValidate="txtAddress1" ErrorMessage="Address 1 is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvCity" Display="None" ControlToValidate="txtCity" ErrorMessage="City is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvState" Display="None" ControlToValidate="ddlState" ErrorMessage="State is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvZipcode" Display="None" ControlToValidate="txtZipcode" ErrorMessage="Zipcode is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvZipcodePlus4" Display="None" ControlToValidate="txtZipcodePlus4" ErrorMessage="Zipcode plus4 is a required field" runat="server"></asp:requiredfieldvalidator>


</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="formSecurityName" runat="server" Visible="false">ILPathways.Admin</asp:Literal>
<asp:Literal ID="currentOrgId" runat="server" Visible="false">0</asp:Literal>

</asp:Panel>




