<%@ Page Title="" Language="C#" MasterPageFile="/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="MapResourceType.aspx.cs" Inherits="ILPathways.Admin.mapping.MapResourceType" %>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<!-- content start -->
<div style="float:left; width:70%">
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />
<div>
<asp:panel id="addPanel" runat="server" Visible="false">
<div class="infoMessage" style="width: 100%;">
			<h2>Add New Mapping</h2>
			<!-- -->
			<div class="clearFloat"></div>	
			<div class="labelColumn requiredField">
			<asp:label id="Label5" associatedcontrolid="txtLR_ResourceType" runat="server">LR Resource Type</asp:label>*
			</div>
			<div class="dataColumn">
				<asp:textbox id="txtLR_ResourceType" runat="server" Width="300px" MaxLength="150" ></asp:textbox>
			</div>  	
			
			<!-- -->
			<div class="clearFloat"></div>	

			<div class="labelColumn requiredField">
			<asp:label id="Label1" associatedcontrolid="ddlResourceType" runat="server">Mapped Item</asp:label>*
			</div>
			<div class="dataColumn">
                  <asp:DropDownList id="ddlResourceType" runat="server"></asp:DropDownList>
                  <br />
				<asp:textbox id="txtResourceType" runat="server" Width="100px" Columns="20" MaxLength="50" Visible="false" ></asp:textbox>
			</div>  
		
			<!-- -->
			<div class="clearFloat"></div>	
			<div class="labelColumn">&nbsp;</div>
			<div class="dataColumn">			
			<asp:Button	 ID="btnNew" runat="server" CssClass="defaultButton" width="120px"  Text="Save" OnClick="addButton_Click"  />
			</div>
<br /><br />
</div>						
</asp:panel>
<div class="clearFloat"></div>	
<asp:panel id="gridPanel" runat="server" Visible="false">
<div style="padding: 10px; width:100%">
<asp:gridview ID="formGrid" runat="server" autogeneratecolumns="False" datakeynames="Id"
            showfooter="false" allowpaging="false" allowsorting="false"  Width="100%"
            AutoGenerateEditButton="True"
            OnRowEditing="EditRecord"
            OnRowCommand="formGrid_RowCommand" 
						OnRowDataBound="formGrid_RowDataBound"
						OnRowCancelingEdit="CancelRecord" 
						OnRowUpdating="UpdateRecord">
   <Columns>   
     <asp:TemplateField HeaderText="Delete" ItemStyle-Width="100px" ControlStyle-Width="100px">
			<ItemTemplate>
				 <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("Id") %>' Enabled="false" CommandName="DeleteRow" CausesValidation="false" runat="server">
					 Delete</asp:LinkButton>
			 </ItemTemplate>
		 </asp:TemplateField>

    <asp:TemplateField HeaderText="LR Value" ItemStyle-Width="250px" ControlStyle-Width="200px" >
        <EditItemTemplate>
            <asp:TextBox ID="gridLRValue" runat="server" Text='<%# Bind("Title") %>' MaxLength="10"></asp:TextBox>
            <asp:RequiredFieldValidator id="RequiredFieldValidator3" runat="server" ErrorMessage="A LR Value is required!" ControlToValidate="gridLRValue"></asp:RequiredFieldValidator>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:label ID="gridlblLRValue" runat="server" Text='<%# Bind("Title") %>'></asp:label>
        </ItemTemplate>
    </asp:TemplateField>  
     
    <asp:TemplateField HeaderText="Resource Type" ItemStyle-Width="250px" ControlStyle-Width="200px" >
        <EditItemTemplate>
            <asp:DropDownList id="gridDdlResourceType" runat="server"></asp:DropDownList>
            <asp:RequiredFieldValidator id="RequiredFieldValidator4" runat="server" ErrorMessage="A Resource Type is required!" ControlToValidate="gridDdlResourceType"></asp:RequiredFieldValidator>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:label ID="gridlblResourceType" runat="server" Text='<%# Bind("ResourceType") %>'></asp:label>
        </ItemTemplate>
    </asp:TemplateField>  
    <asp:boundfield datafield="CodeId" Visible="false" ></asp:boundfield>	               
</Columns>
</asp:gridview> 
</div>
<br />

<asp:RequiredFieldValidator id="rfvCategory" runat="server" ErrorMessage="A LR Resource Type is required!" ControlToValidate="txtLR_ResourceType" Display="None"></asp:RequiredFieldValidator>
<asp:RequiredFieldValidator id="rfvLRValue" runat="server" ErrorMessage="An Mapped Item is required!" ControlToValidate="ddlResourceType" Display="None"></asp:RequiredFieldValidator>

</asp:panel>

</div>

</div>

<asp:Panel ID="hiddenStuff"  runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.Admin.mapping.MapResourceType</asp:Literal>
</asp:Panel>

<asp:Panel ID="sqlPanel"  runat="server" Visible="false">

<asp:Literal ID="insertSql" runat="server" Visible="false">
INSERT INTO [dbo].[Map.ResourceType] ([LRValue],[CodeId]) VALUES ('{0}', {1} )
</asp:Literal>

<asp:Literal ID="updateSql" runat="server" Visible="false">
UPDATE [dbo].[Map.ResourceType] SET [LRValue] = '{0}', [CodeId] = {1}  WHERE id = {2} 
</asp:Literal>

<asp:Literal ID="selectSql" runat="server" Visible="false">
SELECT base.[Id] As Id,base.[LRValue] As Title,base.[CodeId],Codes.Title As ResourceType   FROM [dbo].[Map.ResourceType] base left join [dbo].[Codes.ResourceType] codes on base.[CodeId]= codes.Id   order by base.[LRValue]
</asp:Literal>


<asp:Literal ID="resourceTypeSelect" runat="server" Visible="false">
SELECT Id ,Title FROM [dbo].[Codes.ResourceType] Order by Title
</asp:Literal>
</asp:Panel>



</asp:Content>
