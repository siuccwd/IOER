<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibraryCollectionMtce.ascx.cs" Inherits="ILPathways.LRW.controls.LibraryCollectionMtce" %>

<%@ Register
    Assembly="AjaxControlToolkit"
    Namespace="AjaxControlToolkit.HTMLEditor"
    TagPrefix="HTMLEditor" %>

    <script language="javascript" type="text/javascript">
<!--
      function confirmDelete(recordTitle, id) {
        // ===========================================================================
        // Function to prompt user to confirm a request to delete a record
        // Note - this could be made generic if the url is passed as well

        var bresult
        bresult = confirm("Are you sure you want to delete this collection? All resources will be removed if you continue.\n\n"
            + "Click OK to delete this collection or click Cancel to skip the delete.");
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
//-->
</script>
<style type="text/css">
  #containerDetails labelColumn, #containerDetails dataColumn {padding-top: 3px; }
.txtTitle { width: 300px; }
.columnStyle { margin-top: 10px; }
.radioButton label {
  padding: 1px 5px;
  width: 25px;
  display: inline-block;
  *display: inline;
  }
</style>
<style type="text/css">
.libraryCollectionDetailsPanel {
  width: 500px;
  margin-top: -22px;
}
</style>
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />

<asp:Panel ID="detailsPanel" runat="server" CssClass="libraryCollectionDetailsPanel">

<div id="containerDetails">
	<h2 class="isleBox_H2"><asp:label ID="libraryName" runat="server">Collection</asp:label></h2>
  <asp:Panel ID="idPanel" runat="server" Visible="false">
	<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" > 
    <asp:label id="Label4"  associatedcontrolid="txtLibraryId" runat="server">LibraryId</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtLibraryId"  runat="server"></asp:label>  
	</div>
	<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" > 
    <asp:label id="lblId"  associatedcontrolid="txtId" runat="server">id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId"  runat="server"></asp:label>  
	</div>
  </asp:Panel>
	<!-- --> 
	<div class="clearFloat"></div>
  <div class="labelColumn requiredField">
    <asp:label id="Label1"  associatedcontrolid="txtTitle" runat="server">Collection</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  id="txtTitle" CssClass="txtTitle"  MaxLength="200" runat="server"></asp:textbox>  
	</div>	
<!-- --> 
<asp:Panel ID="sectionTypePanel" runat="server" Visible="false">
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="lblType"  associatedcontrolid="ddlSectionType" runat="server">Collection Type</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlSectionType" runat="server" Width="250px"></asp:dropdownlist>

  </div>
</asp:Panel>

<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn  requiredField" > 
    <asp:label id="lblDetails"  associatedcontrolid="txtDescription" runat="server">Description</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4" Width="300px"></asp:TextBox>
  </div>
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="Label2"  associatedcontrolid="rblIsPublic" runat="server">Is Public</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsPublic" autopostback="false" CssClass="radioButton"  causesvalidation="false"   runat="server" tooltip="True: Collection is publically accessible" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>    
 	<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>

<!-- --> 
<div class="clearFloat">  </div>
  <div class="labelColumn " > 
    <asp:label id="Label3"  associatedcontrolid="rblIsDefaultSection" runat="server">Is Default Section</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsDefaultSection" autopostback="false" CssClass="radioButton"  causesvalidation="false" runat="server" tooltip="True: The default section is used by system inserts, where user is not involved " RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>    
 	<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>

<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="lblIsActive"  associatedcontrolid="rblAreContentsReadOnly" runat="server">Are Contents Read Only</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblAreContentsReadOnly" autopostback="false" causesvalidation="false"   CssClass="radioButton"  runat="server" tooltip="True: collection is read only. Means created and maintained by system (ex. my published, authored" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>    
 	<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>
<asp:Panel ID="collectionImagePanel"		 runat="server" Visible="false">
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn" > 
    <asp:label id="Label5"  associatedcontrolid="txtLibraryImage" runat="server">Collection Image</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:TextBox ID="txtLibraryImage" runat="server" Width="300px"></asp:TextBox>
  </div>

</asp:Panel>
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
				<asp:button id="btnSave" runat="server" CssClass="defaultButton" width="100px" CommandName="Update" 
				OnCommand="FormButton_Click" Text="Save"		CausesValidation="true"></asp:button>
				<asp:button id="btnDelete" runat="server" CssClass="defaultButton" width="100px" CommandName="Delete" 
				OnCommand="FormButton_Click" Text="Delete"	CausesValidation="False" visible="false"></asp:button> 
				<asp:button id="btnNew" runat="server" style="margin-left: 10px;" CssClass="defaultButton" width="100px" CommandName="New" visible="false"
				OnCommand="FormButton_Click" Text="New Record" causesvalidation="false"></asp:button>

		</div>		
		<div class="clearFloat"></div>
		<br />
</div>

</asp:Panel>

<div>
<!-- validators -->
<asp:requiredfieldvalidator id="rfvTitle" Display="None" ControlToValidate="txtTitle" ErrorMessage="Title is a required field" runat="server"></asp:requiredfieldvalidator>


</div>

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.LRW.controls.LibraryCollectionMtce</asp:Literal>

<!-- control variables -->
<asp:Literal ID="txtDefaultSectionTypeId" runat="server" Visible="false">3</asp:Literal>
<asp:Literal ID="txtCurrentLibraryId" runat="server" Visible="false">0</asp:Literal>


</asp:Panel>