<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="OrganizationMgmt.ascx.cs" Inherits="IOER.Organizations.controls.OrganizationMgmt" %>

 <script type="text/javascript" src="/Scripts/toolTip.js"></script>
  <link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />
<style type="text/css">
.labelColumn { width: 25%; border-right: 5px solid #fff;}
.dataColumn { width: 55%; }
    
#thisContainer {  width: 900px; min-height:300px; margin: 50px auto;}
.dataColumn .longTxt { width: 300px;}
.dataColumn .shortTxt { width: 100px;}

.required {
    border-right: 5px solid #B03D25;

}

  @media screen and (max-width: 700px){

    .labelColumn { width: 65%; }
    .dataColumn { width: 85%; }

    .dataColumn .longTxt { width: 60%;}
    .dataColumn .shortTxt { width: 100px;}
  }

</style>
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />
<div id="thisContainer">
<asp:Panel ID="detailPanel" runat="server">
<!-- --> 
<p ><span class="required" > Required fields are marked with a red border.</span></p>

<asp:Panel ID="adminPanel" runat="server" Visible="true">

  <div class="labelColumn " > 
    <asp:label id="lblId"  associatedcontrolid="txtId" runat="server">Organization Id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId" runat="server"></asp:label>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblIsActive"  associatedcontrolid="rblIsActive" runat="server">Is Active</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsActive" autopostback="false" causesvalidation="false"   runat="server" tooltip="Yes - organization is active, No - organization is no longer active and will not appear in searches" RepeatDirection="Horizontal">
<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>
<asp:ListItem Text="No"   value="No"></asp:ListItem>
</asp:RadioButtonList>
  </div>

    </asp:Panel>

<!-- --> 
  <div class="labelColumn required" > 
    <asp:label id="lblName"  associatedcontrolid="txtName" runat="server">Organization</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="100"  id="txtName" CssClass="longTxt"  runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn required" > 
    <asp:label id="lblOrgTypeId"  associatedcontrolid="ddlOrgTypeId" runat="server">Organization Type</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlOrgTypeId" runat="server"></asp:dropdownlist>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="Label1"  associatedcontrolid="rblIsIsleMember" runat="server">Is ISLE Member</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsIsleMember" enabled="false" causesvalidation="false"   runat="server" tooltip="Yes - organization is member of Isle in good standing, No - organization is not a member of ISLE - restricted update access" RepeatDirection="Horizontal">
<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>
<asp:ListItem Text="No"   value="No"></asp:ListItem>
</asp:RadioButtonList>
  </div>
<!-- --> 
  <div class="labelColumn " > 
          <span style="text-align:center;" >
                        <a class="toolTipLink" id="A1" title="Email Domain|Example: <strong>myDomain.com</strong><br>The Email domain can be used to automatically add people with the same domain as an organization member. Use carefully. Using illinois.gov, would result in all Illinois government users being added to this organization."><img
			src="/images/icons/infoBubble.gif" alt="" /></a>
                    </span>
    <asp:label id="Label2"  associatedcontrolid="txtEmailDomain" runat="server">Email Domain</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtEmailDomain" runat="server"></asp:textbox>
  </div>

<!-- --> 
  <div class="labelColumn " > 
    <asp:label id="Label3"  associatedcontrolid="txtWebsite" runat="server">Web Site</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="200"  id="txtWebsite" runat="server"></asp:textbox>
  </div>
<!-- --> 
  <div class="labelColumn " > 
    <asp:label id="lblMainPhone"  associatedcontrolid="txtMainPhone" runat="server">Main Phone</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="20"  id="txtMainPhone" runat="server"></asp:textbox>

    <asp:label id="lblMainExtension"  associatedcontrolid="txtMainExtension" runat="server">Ext.</asp:label> 

    <asp:textbox  maxLength="10"  id="txtMainExtension" CssClass="shortTxt" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblFax"  associatedcontrolid="txtFax" runat="server">Fax</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="15"  id="txtFax" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn required" > 
    <asp:label id="lblAddress"  associatedcontrolid="txtAddress1" runat="server">Address</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtAddress1" CssClass="longTxt" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn " > 
    <asp:label id="lblAddress2"  associatedcontrolid="txtAddress2" runat="server">Address2</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtAddress2" CssClass="longTxt" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn required" > 
    <asp:label id="lblCity"  associatedcontrolid="txtCity" runat="server">City</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtCity" CssClass="longTxt" runat="server"></asp:textbox>
  </div>
<!-- --> 

  <div class="labelColumn required" > 
    <asp:label id="lblState"  associatedcontrolid="ddlState" runat="server">State</asp:label> 
  </div>
  <div class="dataColumn"> 
      <asp:label  id="txtState" runat="server" Visible="false">IL</asp:label>
    <asp:dropdownlist  id="ddlState" runat="server" Visible="true"></asp:dropdownlist>
  </div>
<!-- --> 

  <div class="labelColumn required" > 
    <asp:label id="lblZipcode"  associatedcontrolid="txtZipcode" CssClass="shortTxt"  runat="server">Zipcode</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="5"  id="txtZipcode" runat="server"></asp:textbox> -
      <asp:textbox  maxLength="4"  id="txtZipcodePlus4" CssClass="shortTxt"  runat="server"></asp:textbox>
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
<asp:requiredfieldvalidator id="rfvMainPhone" Display="None" ControlToValidate="txtMainPhone" ErrorMessage="Phone is a required field"  Enabled="false" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvAddress" Display="None" ControlToValidate="txtAddress1" ErrorMessage="Address 1 is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvCity" Display="None" ControlToValidate="txtCity" ErrorMessage="City is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvState" Display="None" ControlToValidate="ddlState" ErrorMessage="State is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvZipcode" Display="None" ControlToValidate="txtZipcode" ErrorMessage="Zipcode is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvZipcodePlus4" Display="None"  ControlToValidate="txtZipcodePlus4" Enabled="false"   ErrorMessage="Zipcode plus4 is a required field" runat="server"></asp:requiredfieldvalidator>


</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="formSecurityName" runat="server" Visible="false">Site.Admin</asp:Literal>
<asp:Literal ID="currentOrgId" runat="server" Visible="false">0</asp:Literal>
<asp:Label ID="addAdminFailedMsg" runat="server"><h2>Attempt to add admin user to new org failed.</h2><p>OrgId: {0}<br />UserId: {1}</p><p>{2}</p></asp:Label>

<asp:label ID="confirmMsg1" runat="server" >Successfully created the organization.<br/>As well you have been added as the administrator.</asp:label>
<asp:label ID="confirmMsg2" runat="server" >Successfully created the organization.<br />NOTE: This organization request has been submitted for approval. You will be notified within one business day of the request.<br/>As well you have been added as the administrator.</asp:label>
<asp:label ID="confirmMsg3" runat="server" >Successfully created the organization.</asp:label>

<asp:Literal ID="litCommonDomains" runat="server" >gmail.com hotmail.com outlook.com comcast.com</asp:Literal>
</asp:Panel>




