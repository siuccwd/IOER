<%@ Page Title="" Language="C#" MasterPageFile="/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="MapResourceFormat.aspx.cs" Inherits="ILPathways.Admin.mapping.MapResourceFormat" %>

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
			<asp:label id="Label5" associatedcontrolid="txtLR_ResourceFormat" runat="server">LR Resource Format</asp:label>*
			</div>
			<div class="dataColumn">
				<asp:textbox id="txtLR_ResourceFormat" runat="server" Width="300px" MaxLength="150" ></asp:textbox>
			</div>  	
			
			<!-- -->
			<div class="clearFloat"></div>	

			<div class="labelColumn requiredField">
			<asp:label id="Label1" associatedcontrolid="ddlResourceFormat" runat="server">Mapped Item</asp:label>*
			</div>
			<div class="dataColumn">
                  <asp:DropDownList id="ddlResourceFormat" runat="server"></asp:DropDownList>
                  <br />
				<asp:textbox id="txtResourceFormat" runat="server" Width="100px" Columns="20" MaxLength="50" Visible="false" ></asp:textbox>
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
          <div style="float:right;">
            Page Size <asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" visible="true" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
          </div>
	    <div style="float:left;">
		    <wcl:PagerV2_8 ID="pager1" runat="server" 
            Visible="false" 
            OnCommand="pager_Command" 
            GenerateFirstLastSection="true"
            FirstClause="First Page" 
            PreviousClause="Prev." 
            NextClause="Next" 
            LastClause="Last Page" 
            PageSize="15" 
            CompactModePageCount="4" 
            NormalModePageCount="5"  
            GenerateGoToSection="false" 
            GeneratePagerInfoSection="true"
            />
	    </div>
<div style="clear:both; padding: 10px; width:100%">
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
				 <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("Id") %>' CommandName="DeleteRow" Enabled="false" CausesValidation="false" runat="server">
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
     
    <asp:TemplateField HeaderText="Resource Format" ItemStyle-Width="250px" ControlStyle-Width="200px" >
        <EditItemTemplate>
            <asp:DropDownList id="gridDdlResourceFormat" runat="server"></asp:DropDownList>
            <asp:RequiredFieldValidator id="RequiredFieldValidator4" runat="server" ErrorMessage="A Resource Format is required!" ControlToValidate="gridDdlResourceFormat"></asp:RequiredFieldValidator>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:label ID="gridlblResourceFormat" runat="server" Text='<%# Bind("ResourceFormat") %>'></asp:label>
        </ItemTemplate>
    </asp:TemplateField>  
    <asp:boundfield datafield="CodeId" Visible="false" ></asp:boundfield>	               
</Columns>
</asp:gridview> 
</div>
	    <div style="float:left;">
		    <wcl:PagerV2_8 ID="pager2" runat="server" 
          Visible="false" 
          OnCommand="pager_Command" 
          GenerateFirstLastSection="true" 
          FirstClause="First Page" 
          PreviousClause="Prev." 
          NextClause="Next" 
          LastClause="Last Page" 
          PageSize="15" 
          CompactModePageCount="2"  GenerateToolTips="true" GenerateSmartShortCuts="true" MaxSmartShortCutCount="3"
          NormalModePageCount="5" 
          GenerateGoToSection="false" 
          GeneratePagerInfoSection="true" 
            />
	    </div>
<br />

<asp:RequiredFieldValidator id="rfvCategory" runat="server" ErrorMessage="A LR Resource Format is required!" ControlToValidate="txtLR_ResourceFormat" Display="None"></asp:RequiredFieldValidator>
<asp:RequiredFieldValidator id="rfvLRValue" runat="server" ErrorMessage="An Mapped Item is required!" ControlToValidate="ddlResourceFormat" Display="None"></asp:RequiredFieldValidator>

</asp:panel>

</div>

</div>

<asp:Panel ID="hiddenStuff"  runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.Admin.mapping.MapResourceFormat</asp:Literal>
</asp:Panel>

<asp:Panel ID="sqlPanel"  runat="server" Visible="false">

<asp:Literal ID="insertSql" runat="server" Visible="false">
INSERT INTO [dbo].[Map.ResourceFormat] ([LRValue],[CodeId]) VALUES ('{0}', {1} )
</asp:Literal>

<asp:Literal ID="updateSql" runat="server" Visible="false">
UPDATE [dbo].[Map.ResourceFormat] SET [LRValue] = '{0}', [CodeId] = {1}  WHERE id = {2} 
</asp:Literal>

<asp:Literal ID="selectSql" runat="server" Visible="false">
SELECT base.[Id] As Id,base.[LRValue] As Title,base.[CodeId],Codes.Title As ResourceFormat   FROM [dbo].[Map.ResourceFormat] base left join [dbo].[Codes.ResourceFormat] codes on base.[CodeId]= codes.Id   order by base.[LRValue]
</asp:Literal>


<asp:Literal ID="resourceFormatSelect" runat="server" Visible="false">
SELECT Id ,Title FROM [dbo].[Codes.ResourceFormat] Order by Title
</asp:Literal>
</asp:Panel>



</asp:Content>