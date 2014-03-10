<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="MapCareerClusters.aspx.cs" Inherits="ILPathways.Admin.mapping.MapCareerClusters" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
<style type="text/css">
    .labelColumn {
        display: inline-block;
        width: 150px;
        text-align: right;
    }
    .dataColumn {
        display: inline-block;
        text-align: left;
    }
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    
<!-- content start -->
<div >
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />
<div>
<asp:panel id="addPanel" runat="server" Visible="false">
<div class="infoMessage" style="width: 90%;">
			<h2>Add New Mapping</h2>

			<!-- -->
			<div class="clearFloat"></div>	
			<div class="labelColumn requiredField">
			    <asp:label id="Label1" associatedcontrolid="ddlClusters" runat="server">Mapped Cluster</asp:label>*
			</div>
			<div class="dataColumn">
                  <asp:DropDownList id="ddlClusters" runat="server"></asp:DropDownList>
			</div>  
					<!-- -->
			<div class="clearFloat"></div>	
			<div class="labelColumn requiredField">
			    <asp:label id="Label5" associatedcontrolid="txtMapping" runat="server">Cluster Mapping</asp:label>*
			</div>
			<div class="dataColumn">
				<asp:textbox id="txtMapping" runat="server" Width="300px" MaxLength="150" ></asp:textbox>
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
    <hr />
<div class="clearFloat"></div>	
<h2>Existing Mappings</h2>
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
<div style="clear:both; padding: 10px; width:95%">
	<!-- -->
	<div class="clearFloat"></div>	
	<div class="labelColumn requiredField">
		<asp:label id="Label2" associatedcontrolid="ddlFilterCluster" runat="server">Filter Cluster</asp:label>*
	</div>
	<div class="dataColumn">
            <asp:DropDownList id="ddlFilterCluster"  runat="server"  AutoPostBack="True" OnSelectedIndexChanged="ddlFilterCluster_OnSelectedIndexChanged" ></asp:DropDownList>
	</div> 
    <div class="clearFloat"></div>
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
   <asp:TemplateField HeaderText="Career Cluster" >
        <EditItemTemplate>
        <asp:label ID="gridlblClusters2" runat="server" Text='<%# Bind("ClusterTitle") %>'></asp:label>
            <asp:DropDownList id="gridDdlClusters" visible="false" runat="server"></asp:DropDownList>
            <asp:RequiredFieldValidator id="RequiredFieldValidator4" runat="server" ErrorMessage="A Career Cluster is required!" ControlToValidate="gridDdlClusters"></asp:RequiredFieldValidator>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:label ID="gridlblClusters" runat="server" Text='<%# Bind("ClusterTitle") %>'></asp:label>
        </ItemTemplate>
    </asp:TemplateField>
    <asp:TemplateField HeaderText="Cluster Mapping"  >
        <EditItemTemplate>
            <asp:TextBox ID="gridFilterValue" runat="server" Text='<%# Bind("FilterValue") %>' width="300px" ></asp:TextBox>
            <asp:RequiredFieldValidator id="RequiredFieldValidator3" runat="server" ErrorMessage="A LR Value is required!" ControlToValidate="gridFilterValue"></asp:RequiredFieldValidator>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:label ID="gridlblFilterValue" runat="server" Text='<%# Bind("FilterValue") %>'></asp:label>
        </ItemTemplate>
    </asp:TemplateField>  
     
    <asp:boundfield datafield="MappedClusterId" Visible="false" ></asp:boundfield>	               
</Columns>
</asp:gridview> 
</div>
	    <div style="float:left;">
		    <wcl:PagerV2_8 ID="pager2" runat="server" 
          Visible="true" 
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

<asp:RequiredFieldValidator id="rfvCluster" runat="server" ErrorMessage="A Career Cluster is required!" ControlToValidate="ddlClusters" Display="None"></asp:RequiredFieldValidator>
<asp:RequiredFieldValidator id="rfvFilterValue" runat="server" ErrorMessage="An filter item is required!" ControlToValidate="txtMapping" Display="None"></asp:RequiredFieldValidator>

</asp:panel>

</div>

</div>

<asp:Panel ID="hiddenStuff"  runat="server" Visible="false">
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.Admin.mapping.MapClusters</asp:Literal>
</asp:Panel>

<asp:Panel ID="sqlPanel"  runat="server" Visible="false">

<asp:Literal ID="insertSql" runat="server" Visible="false">
INSERT INTO [dbo].[Map.CareerCluster] ([FilterValue],[MappedClusterId] ,[LastUpdatedBy]) VALUES ('{0}', {1}, '{2}' )
</asp:Literal>

<asp:Literal ID="updateSql2" runat="server" Visible="false">
UPDATE [dbo].[Map.CareerCluster] SET [MappedClusterId] = {1},[FilterValue] = '{2}', LastUpdatedBy= '{3}',[LastUpdated] = GETDATE()  WHERE Id = {0}
</asp:Literal>

<asp:Literal ID="updateSql" runat="server" Visible="false">
UPDATE [dbo].[Map.CareerCluster] SET [FilterValue] = '{1}', LastUpdatedBy= '{2}',[LastUpdated] = GETDATE()  WHERE Id = {0}
</asp:Literal>

<asp:Literal ID="selectSql" runat="server" Visible="false">
SELECT base.Id, MappedClusterId, cc.IlPathwayName As ClusterTitle, FilterValue, Created, LastUpdated, LastUpdatedBy FROM [Map.CareerCluster] base left join [dbo].[CareerCluster] cc on base.MappedClusterId = cc.Id {0} order by cc.IlPathwayName, base.FilterValue
</asp:Literal>


<asp:Literal ID="clustersSelect" runat="server" Visible="false">
SELECT [Id],[ShortName],[IlPathwayName] As Title FROM [dbo].[CareerCluster] where [IsIlPathway]= 1 AND [IsActive]= 1 Order by IlPathwayName
</asp:Literal>
</asp:Panel>



</asp:Content>
