<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="GroupPrivileges.ascx.cs" Inherits="IOER.Controls.GroupsManagment.GroupPrivileges" %>


<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" ValidationGroup="privilegesValGroup" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>

<asp:panel id="objectPrivilegesPanel" runat="server" cssclass="menuBranch"  visible="true" width="100%">
<div class="clear"></div>	
<br />
<div  id="Div1" >
  <div class="labelcolumn" >Select an existing privilege</div>	
  <div class="dataColumn">   
		<asp:DropDownList		 id="lstForm" runat="server" Width="400px" AutoPostBack="True" 
			onselectedindexchanged="lstForm_SelectedIndexChanged" >
		</asp:DropDownList>
  </div>
<!-- --> 
	<div class="clear"></div>
  <div class="labelcolumn" > 
    <asp:label id="Label8"  associatedcontrolid="txtGroupId" runat="server">Group Id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtGroupId"  runat="server">0</asp:label>  
	</div>    
<!-- --> 
	<div class="clear"></div>
  <div class="labelcolumn" > 
    <asp:label id="Label7"  associatedcontrolid="txtId" runat="server">id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId"  runat="server">0</asp:label>  
	</div>  
		<!-- -->
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label5" associatedcontrolid="ddlObject" runat="server">Object</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:label id="lblObjectName" runat="server" Visible="false"></asp:label>
			<asp:DropDownList		 id="ddlObject" runat="server" Width="400px"></asp:DropDownList>
					<asp:RequiredFieldValidator id="rfvObject" runat="server" ControlToValidate="ddlObject" Display="None"
					EnableClientScript="false" ErrorMessage="An Object must be selected.">
				</asp:RequiredFieldValidator>  
				&nbsp;		<asp:button id="refreshObjectsButton" runat="server" CssClass="defaultButton" width="100px" CommandName="refreshObjects" 
		OnCommand="FormButton_Click" Text="Refresh"		CausesValidation="false" Visible="true"></asp:button>		
		</div> 
		</div>  
		<!-- -->				
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label1" associatedcontrolid="ddlCreatePriv" runat="server">Create Privilege</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:DropDownList		 id="ddlCreatePriv" runat="server" Width="200px"></asp:DropDownList> 
		</div>  
		<!-- -->				
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label2" associatedcontrolid="ddlReadPriv" runat="server">Read Privilege</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:DropDownList		 id="ddlReadPriv" runat="server" Width="200px"></asp:DropDownList> 
		</div>  
		<!-- -->				
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label3" associatedcontrolid="ddlUpdatePriv" runat="server">Update Privilege</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:DropDownList		 id="ddlUpdatePriv" runat="server" Width="200px"></asp:DropDownList> 
		</div>  		
		<!-- -->				
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label4" associatedcontrolid="ddlDeletePriv" runat="server">Delete Privilege</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:DropDownList		 id="ddlDeletePriv" runat="server" Width="200px"></asp:DropDownList> 
		</div>  	
		<!-- -->
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label9" associatedcontrolid="ddlAssignPriv" runat="server">Assign Privilege</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:DropDownList		 id="ddlAssignPriv" runat="server" Width="200px"></asp:DropDownList> 
		</div>  					
		<!-- -->
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label6" associatedcontrolid="ddlAppendPriv" runat="server">Append Privilege</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:DropDownList		 id="ddlAppendPriv" runat="server" Width="200px"></asp:DropDownList> 
		</div>  	
		<!-- -->
		<div class="clear"></div>	
		<div class="labelcolumn requiredField">
		<asp:label id="Label10" associatedcontrolid="ddlApprovePriv" runat="server">Approve Privilege</asp:label>*
		</div>
		<div class="dataColumn">   
			<asp:DropDownList		 id="ddlApprovePriv" runat="server" Width="200px"></asp:DropDownList> 
		</div> 				
		<!-- -->		
		<div class="clear"></div>	
		<div class="labelcolumn">Actions</div>
		<div class="dataColumn">		
		<asp:button id="btnSave" runat="server" CssClass="defaultButton" width="100px" CommandName="Update" 
		OnCommand="FormButton_Click" Text="Save"		CausesValidation="true"></asp:button>			
		<asp:button id="saveNewPrivilegeButton" runat="server" CssClass="defaultButton" width="130px" CommandName="SaveNewPrivilege" 
		OnCommand="FormButton_Click" Text="Save and New"		CausesValidation="true"></asp:button>				
		<asp:button id="copyPrivilegeButton" runat="server" CssClass="defaultButton" width="100px" CommandName="CopyPrivilege" 
		OnCommand="FormButton_Click" Text="Copy"		CausesValidation="false" Visible="false"></asp:button>	
		<asp:button id="btnDelete" runat="server" CssClass="defaultButton" width="100px" CommandName="Delete" 
		OnCommand="FormButton_Click" Text="Delete"		CausesValidation="false" Visible="false"></asp:button>	
		
		&nbsp;&nbsp;<asp:button id="btnNew" runat="server" CssClass="defaultButton" width="200px" CommandName="New" 
		OnCommand="FormButton_Click" Text="New Group Privilege" causesvalidation="false"></asp:button>	
		</div>
		<div class="clear"></div>				


</asp:panel>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="selectAvailableObjectsSql" runat="server" >
SELECT  id, ObjectName, DisplayName FROM ApplicationObject
where id not in 
( SELECT [ObjectId] FROM [AppGroup.Privilege] Where (GroupId	= {0}) )
Order by DisplayName
</asp:Literal>


<asp:Literal ID="formSecurityName" runat="server" Visible="false">IOER.Controls.GroupsManagment.GroupPrivileges</asp:Literal>


</asp:Panel>	