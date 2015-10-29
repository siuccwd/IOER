<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Import.ascx.cs" Inherits="IOER.Organizations.controls.Import" %>


<style type="text/css">
    .defaultButton { width: 200px; }
</style>

<div>&nbsp;</div>
<asp:Label ID="importInstructions" Visible="true" runat="server"></asp:Label>
<asp:Panel ID="Panel1" runat="server" Visible="false">
    <div class="labelColumn">
        <asp:label ID="labelOrgId2" runat="server" >Org. Id</asp:label>
    </div>
    <div class="dataColumn">
        <asp:label ID="lblOrgId" runat="server" ></asp:label>
    </div>
    <div class="labelColumn">
        <asp:label ID="label1" runat="server" >Organization</asp:label>
    </div>
    <div class="dataColumn">
        <asp:label ID="txtOrgName" runat="server" ></asp:label>
    </div>
            <asp:TextBox ID="txtOrgId" runat="server">0</asp:TextBox>
        <asp:DropDownList ID="ddlOrgList" runat="server"></asp:DropDownList>
</asp:Panel>


<asp:Panel ID="PanelOptions" runat="server" Visible="true">
    <div class="labelTop">
        <asp:label ID="label2" runat="server" >Use Default Password (optional, minimum of 8 characters)</asp:label>
    </div>
    <div class="labelColumn">&nbsp;</div>
    <div class="dataColumn">
        <asp:DropDownList ID="ddlPasswordTemplates" runat="server">
            <asp:ListItem Text="Select password template" Value="0"></asp:ListItem>
            <asp:ListItem Text="LastName_ plus datetime" Value="1"></asp:ListItem>
            <asp:ListItem Text="FirstName_ plus datetime" Value="2"></asp:ListItem>
            <asp:ListItem Text="ChangeMe_ plus some milliseconds" Value="3"></asp:ListItem>
        </asp:DropDownList>
        <asp:TextBox CssClass="password" ID="txtDefaultPassword" runat="server" ></asp:TextBox>

    </div>
    <div class="labelColumn">
        <asp:label ID="label4" runat="server" >Send Email On Upload</asp:label>
    </div>
    <div class="dataColumn">
        <asp:RadioButtonList id="rblSendEmail" autopostback="false" causesvalidation="false"   runat="server" tooltip="Yes - An email will be sent to each new member, with a (one time) link to login to IOER, No - no emails will be sent." RepeatDirection="Horizontal">
        <asp:ListItem Text="Yes"  value="Yes" Selected="True"></asp:ListItem>
        <asp:ListItem Text="No"   value="No"></asp:ListItem>
        </asp:RadioButtonList>
    </div>
</asp:Panel>
<asp:Panel ID="importPanel" runat="server" Visible="true">
<h2>Steps</h2>
<div >
<ol>
<li><asp:Button ID="btnShowUpload" runat="server" Text="Step 1 - Upload File" OnClick="btnShowUpload_Click" CausesValidation="false" enabled="false" CssClass="defaultButton" /></li>
<li><asp:Button ID="ViewDataButton2" enabled="false" runat="server" 
              Text="Step 2 - View Data" OnClick="ViewDataButton_Click" CausesValidation="false" CssClass="defaultButton" /></li>
<li><asp:Button ID="ImportDataButton2" enabled="false" Visible="true" runat="server" 
              Text="Step 3 - Import Data" Style="color: blue" OnClick="ImportDataButton_Click" CausesValidation="false" CssClass="defaultButton" /></li>
</ol>

</div>		  
<br class="clearFloat" />
<div style="float:left; width: 95%; margin-left: 10px;">
  <asp:Panel ID="PanelUpload" runat="server" Visible="true">
    <h2>Step 1 - Upload File</h2>
        Please click on the browse button and select a CSV file to import:<br />
		<br/><strong>NOTE: See instructions on limiting files to no more than 100 records</strong>.<br />
      <asp:FileUpload ID="FileUpload" Width="500px" runat="server"  />
      <br />Then click on the <strong>Upload File</strong> button to transfer the file to our server for <strong>preview</strong>.

      <br /><asp:Button ID="ButtonUploadFile" runat="server" 
          Text="Upload File" OnClick="ButtonUploadFile_Click" CssClass="defaultButton" /><br />

      
  </asp:Panel>
	<!-- -->     
  <asp:Panel ID="fileUploadSuccessfulPanel" runat="server" Visible="false">
    <asp:Label ID="LabelUpload" runat="server" Text=""></asp:Label>
		<p>File has been uploaded to our server. Next:</p>
		<ul>
		<li>You can choose to view the data first (<strong>Recommended</strong>) by clicking on the <strong>View Data</strong> link on the left.
		<br /><asp:Button ID="ViewDataButton" Visible="false" runat="server" 
        Text="View Data" Enabled="false"  OnClick="ViewDataButton_Click" />
		</li>
		<li>Or: You can import the data directly to the database - by clicking on the <strong>Import Data</strong> link on the left to import the data into our system.
		<br /><asp:Button ID="ImportDataButton" Visible="false" runat="server" 
        Text="Import Data" Enabled="false" OnClick="ImportDataButton_Click" />
		</li>
		</ul>       
  
	</asp:Panel>  
	<!-- -->
  <asp:Panel ID="PanelView" runat="server" Visible="False">
  <h2>Step 2 - View Data</h2>
  <img src="/Images/icons/warning.gif" alt="Warning Sign" /> <strong>NOTE: You are not done yet</strong><br />
      <asp:Label ID="lblDataViewMessage" runat="server">Please review the following data. If it looks OK, then click on the <strong>Step 3 - Import Data</strong> link on the left to import the data into our system.</asp:Label>
      <asp:GridView ID="previewGrid" runat="server"></asp:GridView>

  </asp:Panel>	
	<!-- -->   
  <asp:Panel ID="PanelImport" runat="server" Visible="False">
      <h2>Step 3 - Data has been imported</h2>

  </asp:Panel>	
  <asp:Panel ID="importDetailsPanel" style="font-size:90%;"   runat="server" Visible="false">
      <asp:Label ID="lblImportFile" runat="server" Text=""></asp:Label>
      <br /><asp:Label ID="LabelImport" runat="server" Text=""></asp:Label>  
<a id="importGrid"></a>    
    <asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="Both" Width="95%" 
		captionalign="Top" 
		useaccessibleheader="true" >
		<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
		<columns>			
			<asp:boundfield datafield="Id" headertext="Row" ></asp:boundfield>	
			<asp:TemplateField HeaderText="Status" >
				<ItemTemplate>
					<%# Eval( "Status" )%>
				</ItemTemplate>
			</asp:TemplateField>								
			<asp:boundfield datafield="FirstName" headertext="First Name" ></asp:boundfield>	
			<asp:boundfield datafield="LastName" headertext="Last Name" ></asp:boundfield>				
			<asp:boundfield datafield="Email" headertext="Email" ></asp:boundfield>		
			<asp:boundfield datafield="EmployeeTypeId" headertext="Employee Type Id" ></asp:boundfield>			
				
			<asp:TemplateField >
			<HeaderTemplate>Messages</HeaderTemplate>
				<ItemTemplate>
					<%# Eval( "Message" )%>
				</ItemTemplate>
			</asp:TemplateField>									
		</columns>
	
	</asp:gridview>    
  
  </asp:Panel>
</div>  

<br class="clearFloat"/>

<asp:Label ID="lblError" runat="server" Text="" ForeColor="red"></asp:Label>
 
</asp:Panel>     

<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:literal ID="importFileName" runat="server" >4</asp:literal>
<asp:literal ID="importColumnsCount" runat="server" >4</asp:literal>
<asp:Literal ID="validColumns" runat="server" Visible="false">FirstName LastName Email EmployeeTypeId</asp:Literal>
		
<asp:literal ID="doAutoViewAfterLoad" runat="server" >yes</asp:literal>
<asp:Label ID="previewErrorsFoundMsg" runat="server"><a href="#importGrid">Errors were encountered in the import file</a>. <br/><br/>Please scroll down the page to review and fix these errors and then redo the import!</asp:Label>
<asp:Label ID="importErrorsFoundMsg" runat="server">Errors were encountered in the import file. <br/><br/>No records were imported.<br/><br/>Please review and fix these errors and then redo the import!</asp:Label>

<asp:Label ID="successfulViewMsg" runat="server">The import file has been reviewed and appears to be valid. To be sure, please review the following data. If it looks OK, click on <strong>Step 3 - Import Data</strong> to import the data into our system.</asp:Label>
<asp:Label ID="successfulImportMsg" runat="server">All records from the import file were successfully imported. </asp:Label>

<asp:literal ID="ignoreExtraColumns" runat="server" >yes</asp:literal>
<asp:literal ID="doingDuplicatesCheck" runat="server" >yes</asp:literal>
<asp:literal ID="showImportLinkOnErrors" runat="server" >no</asp:literal>
<asp:literal ID="usingCsvReaderToValidate" runat="server" >yes</asp:literal>

<asp:literal ID="txtOrgNameOLD" runat="server" ></asp:literal>

<asp:Label ID="userAddConfirmation" runat="server">The user accounts have been created and emails were sent with a link to activate the account.</asp:Label>
<asp:literal ID="sendEmailonImport" runat="server" >yes</asp:literal>

<asp:Label ID="noticeSubject" runat="server">{0} added you to an "Illinois Open Educational Resources" organization</asp:Label>
<asp:Label ID="noticeEmail" runat="server">
Welcome<br />An IOER account has been created for you:
    <br />Organization: <strong>{0}</strong> 
    <br />Member Type: <strong>{1}</strong>.
<p>Use the link below to activate your account. You will then have access to the latter organization.</p>
    <p>Upon activation a personal library will be created where you can store the resources that you find interesting.</p>
</asp:Label>

<asp:Label ID="noticeSubject_RI" runat="server">EdTechRI Meetup Today - Welcome to IOER</asp:Label>
<asp:Label ID="noticeEmail_RI" runat="server">
Welcome
<p>We're looking forward to seeing you at the EdTechRI Meetup today!   To get you started, we've added you to IOER as a Highlander Institute member. Bring a device (laptop or tablet) and headphones with you. We will be getting down and dirty with the ISLE OER platform, so you will need a wifi-accessible device to use.</p>    
    
<p>Use the link below to activate your account. You will then have access to add resources to the Highlander Institute library.</p>
    <p>Upon activation a personal library will be created where you can store the resources that you find interesting.</p>
</asp:Label>

<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?pg={0}&a=activate&nextUrl=/Account/Profile.aspx</asp:Literal>
<asp:Label ID="acctCreatedMsg" runat="server">
<p>An account has been created for you:</p>
User Name (email): {0}
<br />Password (temporary): {1} 
<p>Click on the following link to confirm your account and update your profile:</p>
    <a href="{2}">Confirm your IOER registration</a>
<br />
</asp:Label>
<asp:literal ID="errorMessage" runat="server" >
<br><br>For technical assistance, <a href='http://www.ilsharedlearning.org/Pages/Contact-Us.aspx' target='_blank'>enter a request here.</a> <br/>Office Hours: Monday - Friday 8:00 AM - 4:30 PM
</asp:literal>
</asp:Panel>     


