<%@ Page Title="Illinois Open Educational Resources - Query Management" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="QueryMgmt.aspx.cs" Inherits="IOER.Admin.QueryMgmt" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="../Styles/common.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
      $("form").removeAttr("onsubmit");
  </script>

<script  type="text/javascript">
<!--
  function confirmDelete(recordTitle, id) {
    // ===========================================================================
    // Function to prompt user to confirm a request to delete a record
    // Note - this could be made generic if the url is passed as well

    var bresult
    bresult = confirm("Are you sure you want to delete this record?\n\n"
            + "Click OK to delete this record or click Cancel to skip the delete.");
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

  function OnClientResizeText(sender, eventArgs) {
    // This sample code isn't very efficient, but demonstrates the idea well enough
    var e = sender.get_element();
    // Make the font bigger until it's too big
    //    while((e.scrollWidth <= e.clientWidth) || (e.scrollHeight <= e.clientHeight)) {
    //        e.style.fontSize = (fontSize++)+'pt';
    //    }
    //    var lastScrollWidth = -1;
    //    var lastScrollHeight = -1;

    // Make the font smaller until it's not too big - or the last change had no effect
    // (for Opera where e.clientWidth and e.scrollWidth don't behave correctly)
    //    while (((e.clientWidth < e.scrollWidth) || (e.clientHeight < e.scrollHeight)) &&
    //        ((Sys.Browser.agent !== Sys.Browser.Opera) || (e.scrollWidth != lastScrollWidth) || (e.scrollHeight != lastScrollHeight))) {
    //        lastScrollWidth = e.scrollWidth;
    //        lastScrollHeight = e.scrollHeight;
    //        e.style.fontSize = (fontSize--)+'pt';
    //    }
  }
//-->
</script>

<style type="text/css">

#ctl00_BodyContent_TabContainer1_header .ajax__tab_tab, #BodyContent_TabContainer1_header .ajax__tab_tab  {
    height: 25px;     padding: 2px 12px;
}

  }
</style>
 <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
<div class="container-fluid" style="background-color: whitesmoke;">
    <h1 class="isleH1">Query Administration</h1>
<div class="row-fluid span12">
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
</div>

    <div class="span11" style="padding-left: 50px;">
        <ajaxToolkit:TabContainer ID="TabContainer1" runat="server" ActiveTabIndex="0">
<ajaxToolkit:TabPanel runat="server" ID="tabSummary" HeaderText="Search">
		<ContentTemplate>            
<asp:Panel ID="searchPanel" runat="server">
<div id="containerSearch">
		<h3>Select Existing queries</h3>
<!-- --> 
		<br class="clearFloat"/>
			<div class="labelColumn" > 
				<asp:label id="Label7"  associatedcontrolid="rbIsPublicFilter" runat="server">Is Public</asp:label> 
			</div>
			<div class="dataColumn"> 
				<asp:RadioButtonList id="rbIsPublicFilter" autopostback="true" causesvalidation="false" 
						OnSelectedIndexChanged="rbIsPublicFilter_SelectedIndexChanged"   runat="server" RepeatDirection="Horizontal">
						<asp:ListItem Text="Yes"	Value="1" Selected="True"></asp:ListItem>
						<asp:ListItem Text="No"		Value="0"></asp:ListItem>
						<asp:ListItem Text="Both" Value="2"></asp:ListItem>						
		</asp:RadioButtonList>
	</div> 			
<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" >Select a Category</div>
  <div class="dataColumn"> 
		<asp:DropDownList		 id="ddlCategoryFilter" runat="server" AutoPostBack="True" onselectedindexchanged="ddlCategoryFilter_SelectedIndexChanged" ></asp:DropDownList>
	</div>
	<!-- keyword -->
	<div class="clearFloat">
	</div>
	<div class="labelColumn">
		<asp:Label ID="lblKeyword" runat="server" AssociatedControlID="txtKeyword" Text="Keyword"></asp:Label></div>
	<div class="dataColumn">
		<asp:TextBox ID="txtKeyword" runat="server" Width="300px" enabled="true"></asp:TextBox>
		<asp:Label ID="lblKeyword_Help" runat="server" Visible="false" >Enter a partial query name, or query code with the wildcard character (asterisk). 
		</asp:Label>
	</div>	
		
		<!-- -->
		<div class="clearFloat"></div>	
		<div  class="labelColumn">&nbsp;</div >
		<div class="dataColumn">
				<asp:button id="SearchButton" runat="server" text="Search" cssclass="defaultButton" onclick="SearchButton_Click" CausesValidation="false"></asp:button>&nbsp;&nbsp;&nbsp; 
        				&nbsp;&nbsp;&nbsp;<asp:button id="Button1" runat="server" CssClass="defaultButton"  CommandName="New" 
				OnCommand="FormButton_Click" Text="New Query" causesvalidation="false"></asp:button>
		</div>	
	<asp:Panel ID="formListPanel" runat="server" Visible="false">
<!-- --> 	
	<div class="clearFloat"></div>			
  <div class="labelColumn" >Select a query</div>	
  <div class="dataColumn">   		
		<!-- Dropdownlist or grid -->
		<asp:DropDownList		 id="lstForm" runat="server" Width="80%" AutoPostBack="True" onselectedindexchanged="lstForm_SelectedIndexChanged" ></asp:DropDownList>
  </div>
	<div class="clearFloat"></div>			
</asp:Panel>	
</div>
</asp:Panel>

<asp:Panel ID="resultsPanel" runat="server">
<br class="clearFloat" />		
<div style="width:95%">
	<div class="clearFloat" style="float:right;">	
		<div class="labelColumn" >Page Size</div>	
		<div class="dataColumn">     
				<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
		</div>  
	</div>
	<div style="float:left;">
	</div>
	<br class="clearFloat" />	
	<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
		allowpaging="true" PageSize="25" allowsorting="True"  
		OnRowCommand="formGrid_RowCommand"
		OnPageIndexChanging="formGrid_PageIndexChanging"
		onsorting="formGrid_Sorting"				
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
		captionalign="Top" 
		useaccessibleheader="true"  >
		<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
		<columns>
			<asp:TemplateField HeaderText="Select">
				<ItemTemplate>
				 <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("Id") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
					 Select</asp:LinkButton>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:TemplateField HeaderText="Run">
				<ItemTemplate>
				 <a href="/Admin/Queries/Query.aspx?id=<%#Eval("Id")%>" target="_blank"  >Open Query</a>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:boundfield datafield="Category" headertext="Category" sortexpression="Category"></asp:boundfield>	
			<asp:TemplateField HeaderText="Title/Code"  sortexpression="Title">
				<ItemTemplate>
					<%# Eval( "Title" )%><br /><%--<%# Eval( "QueryCode" )%>--%>
				</ItemTemplate>
			</asp:TemplateField>					
			<asp:boundfield datafield="LastUpdatedBy" headertext="Last Updated By" sortexpression="LastUpdatedBy"></asp:boundfield>			

			<asp:TemplateField HeaderText="Last Updated"  sortexpression="LastUpdated">
				<ItemTemplate>
					<asp:Label ID="lblLastUpdated" Text='<%# Eval( "LastUpdated" )%>' runat="server"></asp:Label>
				</ItemTemplate>
			</asp:TemplateField>				
											
		</columns>
		<pagersettings  visible="false" 
						mode="NumericFirstLast"
            firstpagetext="First"
            lastpagetext="Last"
            pagebuttoncount="5"  
            position="TopAndBottom"/> 
	
	</asp:gridview>

<br class="clearFloat" />
</div>			
</asp:Panel> 	
		</ContentTemplate>
	</ajaxToolkit:TabPanel>
<ajaxToolkit:TabPanel ID="tabResults"  HeaderText="Details" runat="server"  >
		<ContentTemplate>
<asp:Panel ID="detailsPanel" runat="server">

<div id="containerDetails">
	<h3>Query Details</h3>

<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" > 
    <asp:label id="lblId"  associatedcontrolid="txtId" runat="server">id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId"  runat="server"></asp:label>  
	</div>	
<!-- --> 
	<div class="clearFloat"></div>		
  <div class="labelColumn requiredField">
    <asp:label id="Label1"  associatedcontrolid="txtTitle" runat="server">Title</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  id="txtTitle" Width="500px"  MaxLength="125" runat="server"></asp:textbox>  
	</div>
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn" > 
    <asp:label id="Label2"  associatedcontrolid="sqlPreviewLink" runat="server">Go To Sql</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:HyperLink ID="sqlPreviewLink" runat="server" Visible="false" NavigateUrl="" Target="_blank">Run this query</asp:HyperLink>
  </div>  	
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn  requiredField" > 
    <asp:label id="lblCategory"  associatedcontrolid="ddlCategory" runat="server">Category</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:Label ID="lblDefaultCategory" runat="server" Visible="false"></asp:Label> 
    <asp:dropdownlist id="ddlCategory" runat="server"></asp:dropdownlist> 
    &nbsp;<asp:label id="lblNewCategory"  associatedcontrolid="txtNewCategory" runat="server">New Category: </asp:label><asp:textbox  maxLength="25"  id="txtNewCategory" width="200px" runat="server"></asp:textbox>
  </div>	
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn  requiredField" > 
    <asp:label id="lblQueryCode"  associatedcontrolid="txtQueryCode" runat="server">Query Code</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="50"  id="txtQueryCode" width="400px" runat="server"></asp:textbox><span style="margin-left:25px;"></span>
    <asp:Image ID="locktxtQueryCode" runat="server" AlternateText="Field locked" ImageUrl="/vos_portal/images/sslicon.gif" Visible="false" />
  </div>	
  <asp:Panel ID="isPublicPanel" runat="server" Visible="true">
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="lblIsPublic"  associatedcontrolid="rblIsPublic" runat="server">Is Public</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblIsPublic" autopostback="false" causesvalidation="false"   runat="server" tooltip="True: Query is Public" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes" Selected="true"></asp:ListItem>    
 	<asp:ListItem Text="No" 	value="No"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>  
</asp:Panel>  
<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn  requiredField" > 
    <asp:label id="lblDescription"  associatedcontrolid="txtDescription" runat="server">Description</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="500"  id="txtDescription" TextMode="MultiLine" Height="50px" width="500px" runat="server"></asp:textbox>
  </div>


<!-- --> 
<div class="clearFloat"></div>
  <div class="labelColumn  requiredField" > 
    <asp:label id="lblSQL"  associatedcontrolid="txtSQL" runat="server">SQL</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  id="txtSQL" TextMode="MultiLine" Height="400px" width="500px" runat="server"></asp:textbox>
  </div>
<!-- --> 
<asp:Panel ID="ownerPanel" runat="server" Visible="false">
<div class="clearFloat"></div>
  <div class="labelColumn " > 
    <asp:label id="lblOwnerId"  associatedcontrolid="ddlOwnerId" runat="server">Owner</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:dropdownlist id="ddlOwnerId" runat="server"></asp:dropdownlist>
  </div>
</asp:Panel>


<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn">
		<asp:label id="Label12" runat="server">History</asp:label>
		</div>
		<div class="dataColumn">
			<asp:label id="lblHistory" runat="server" Width="80%" CssClass="label_FormValueLeft"></asp:label>
		</div>
		<!-- -->
		<div class="clearFloat"></div>	
		<div  class="labelColumn">&nbsp;</div >
		<div class="dataColumn">			
				<asp:button id="btnSave" runat="server" CssClass="defaultButton" CommandName="Update" 
				OnCommand="FormButton_Click" Text="Save"		CausesValidation="true"></asp:button>
				<asp:button id="btnDelete" runat="server" CssClass="defaultButton" CommandName="Delete" 
				OnCommand="FormButton_Click" Text="Delete"	CausesValidation="False" visible="false"></asp:button> 
				&nbsp;&nbsp;&nbsp;<asp:button id="btnNew" runat="server" CssClass="defaultButton" CommandName="New" 
				OnCommand="FormButton_Click" Text="New Query" causesvalidation="false"></asp:button>
				&nbsp;&nbsp;&nbsp;<asp:button id="btnCopy" runat="server" CssClass="defaultButton" CommandName="Copy" 
				OnCommand="FormButton_Click" Text="Copy Query" causesvalidation="false"></asp:button>
		</div>		
</div>

</asp:Panel>
<div>
<!-- validators -->
<asp:requiredfieldvalidator id="rfvTitle" Display="None" ControlToValidate="txtTitle" ErrorMessage="Title is a required field" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator runat="server" Display="None"  id="rfvDescription" ControlToValidate="txtDescription"  ErrorMessage="Description is required"></asp:requiredfieldvalidator>

<asp:requiredfieldvalidator runat="server" Display="None"  id="rfvQueryCode" ControlToValidate="txtQueryCode"  ErrorMessage="Query Code is required"></asp:requiredfieldvalidator>

<asp:requiredfieldvalidator id="rfvCategory" runat="server" Display="None" ControlToValidate="ddlCategory" ErrorMessage="A Query Category must be selected or a new one entered"></asp:requiredfieldvalidator>

<asp:requiredfieldvalidator runat="server" Display="None"  id="rfvSQL" ControlToValidate="txtSQL"  ErrorMessage="SQL is required"></asp:requiredfieldvalidator>

</div>
<br />
		</ContentTemplate>
	</ajaxToolkit:TabPanel>
</ajaxToolkit:TabContainer>

    </div>
</div>



<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<!-- control variables -->
      <asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">Site.Admin.QueryMgmt</asp:Literal>
<asp:Literal ID="openingDetailInNewWindow" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="defaultCategory" runat="server" Visible="false"></asp:Literal>
</asp:Panel>


</asp:Content>
