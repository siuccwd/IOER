<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Query.aspx.cs" Inherits="ILPathways.Admin.Query" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Illinois OERS Queries</title>
    	<style type="text/css">
		@import "/Styles/common.css";
	</style>
<style type="text/css">
body { background-color: #808080; } 
.roundedPanel
{	
	width:90%;
	background-color:aliceblue;
	color:black;
	font-weight:bold;
	border: 2px solid #000;
}
.dataColumn {width: 500px; }  
h1 { color: #fff;}
a, a:visited { color: #fff;}
#rowCount { color: #000;}
</style>

</head>
<body>
    <form id="form1" runat="server">
    <asp:ScriptManager ID="ScriptManager1" runat="server" />	
    <div>
    			<h1>Illinois OERS  Queries</h1>
		<asp:HyperLink ID="workNetAdminReturnLink" NavigateUrl="/" runat="server" Visible="false" >Back</asp:HyperLink>			
		
		<br /><asp:HyperLink ID="closeLink" NavigateUrl="javascript:window.close();" runat="server" Visible="true"  Text="Close Form"></asp:HyperLink>
	
		<br /><asp:HyperLink ID="newQueryLink" runat="server" NavigateUrl="Query.aspx" Target="_blank" Text="Open another query" Visible="false"></asp:HyperLink>
    </div>

    
<div style="padding: 8px; ">

<asp:Panel ID="parentPanel" CssClass="roundedPanel" runat="server" Visible="true">
<asp:Panel ID="searchPanel" runat="server">
	<h3>Saved Queries List</h3>
		<!-- --> 
		<div class="clearFloat"></div>
			<div class="labelColumn" > 
				<asp:label id="lblIsPublicFilter" Visible="false" associatedcontrolid="rbIsPublicFilter" runat="server">Is Public Query</asp:label> 
			</div>
			<div class="dataColumn"> 
				<asp:RadioButtonList id="rbIsPublicFilter" Visible="false" autopostback="true" causesvalidation="false" 
						OnSelectedIndexChanged="IsPublicFilter_SelectedIndexChanged"   runat="server" RepeatDirection="Horizontal">
						<asp:ListItem Text="Yes"	Value="1" Selected="True"></asp:ListItem>
						<asp:ListItem Text="No"		Value="0"></asp:ListItem>
						<asp:ListItem Text="Both" Value="2"></asp:ListItem>						
		</asp:RadioButtonList>
	</div> 		
	<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" >Query Category</div>
  <div class="dataColumn"> 
		<asp:DropDownList		 id="ddlCategoryFilter" runat="server" AutoPostBack="True" onselectedindexchanged="ddlCategoryFilter_SelectedIndexChanged" ></asp:DropDownList>
	</div>

	<!-- --> 
	<div class="clearFloat"></div>	
  <div class="labelColumn" ><asp:Label ID="Label18" runat="server" AssociatedControlID="lstForm">Select a query</asp:Label></div>
  <div class="dataColumn"> 
		<asp:DropDownList	id="lstForm" runat="server" Width="70%" AutoPostBack="True" onselectedindexchanged="lstForm_SelectedIndexChanged" ></asp:DropDownList>
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

<asp:panel ID="queryPanel" runat="server" Visible="false" Enabled="true">
		<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn">Query Title</div>
		<div class="dataColumn">
			<asp:Label id="txtQueryTitle" runat="server" Visible="true"></asp:Label>
		</div>  
		<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn">Description:</div>
		<div id="Div2" style="font-weight:bold; width:80%" class="dataColumn">
			<asp:Label ID="lblDescription" runat="server" ></asp:Label>
		</div>				
		<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn"></div>
		<div class="dataColumn">
			<asp:textbox id="txtQuery" runat="server" textmode="MultiLine" width="700px" cols="70" rows="15"></asp:textbox>
		</div>   
</asp:panel>		
		<!-- -->
		<div class="clearFloat"></div>	
		<p class="labelColumn"></p>
		<div class="dataColumn">
				<asp:button id="DisplayButton" runat="server" text="Display Results" cssclass="defaultButton" onclick="DisplayButton_Click"></asp:button>&nbsp;&nbsp;&nbsp; 
		<asp:button id="ExportButton" runat="server" text="Export Results" visible="True" cssclass="defaultButton" onclick="ExportButton_Click"></asp:button>
		</div>   		
		<!-- -->
		<div class="clearFloat"></div>	
		<div class="labelColumn">Records per page</div>
		<div class="dataColumn">
			<asp:dropdownlist id="pageSizeList" runat="server" autopostback="True" onselectedindexchanged="PageSizeList_SelectedIndexChanged">
										<asp:listitem>50</asp:listitem>
										<asp:listitem selected="True">100</asp:listitem>
										<asp:listitem>1000</asp:listitem>
										<asp:listitem>5000</asp:listitem>
			</asp:dropdownlist>		
		</div>   
		<!-- -->
		<div class="clearFloat"></div>			

<br/>
</asp:Panel>

</div>

<asp:panel ID="resultsPanel" runat="server" Visible="false" Enabled="true">
		<asp:label id="rowCount" runat="server"></asp:label>   
		            
		<asp:datagrid id="grid" runat="server" bordercolor="#CC9966" 
					borderstyle="None" borderwidth="1px" backcolor="White" 
					cellpadding="2" onpageindexchanged="doPagination"
					PageSize="100" >
					<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
					<footerstyle forecolor="#330099" backcolor="#FFFFCC"></footerstyle>
					<selecteditemstyle font-bold="True" forecolor="#663399" backcolor="#FFCC66"></selecteditemstyle>
					<alternatingitemstyle backcolor="WhiteSmoke"></alternatingitemstyle>
					<itemstyle font-size="Smaller" font-names="Arial" forecolor="#330099" backcolor="White"></itemstyle>

					<pagerstyle ForeColor="Black" cssclass="bodyGray" position="TopAndBottom" horizontalalign="Left" mode="NumericPages" visible="False" />
				</asp:datagrid>
			</asp:panel>
			
			<asp:literal ID="showingTitleToAll" runat="server" Visible="false">yes</asp:literal>


    </form>
</body>
</html>
