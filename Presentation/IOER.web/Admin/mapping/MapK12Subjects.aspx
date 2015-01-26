<%@ Page Title="Map K12 Subjects" Language="C#" MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="MapK12Subjects.aspx.cs" Inherits="ILPathways.Admin.mapping.MapK12Subjects" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="/Styles/common.css" rel="Stylesheet" type="text/css" />
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
    <h1 class="isleH1">Map K12 Subjects</h1>
<!-- content start -->
<div >
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<br />
<div style="width:90%; ">
<asp:panel id="addPanel" runat="server" Visible="false">
<div class="infoMessage" style="width: 100%;">
			<h2>Add New Mapping</h2>

			<!-- -->
			<div class="clearFloat"></div>	
			<div class="labelColumn requiredField">
			    <asp:label id="Label1" associatedcontrolid="ddlSubjects" runat="server">Mapped Subject</asp:label>*
			</div>
			<div class="dataColumn">
                  <asp:DropDownList id="ddlSubjects" runat="server"></asp:DropDownList>
			</div>  
					<!-- -->
			<div class="clearFloat"></div>	
			<div class="labelColumn requiredField">
			    <asp:label id="Label5" associatedcontrolid="txtMapping" runat="server">Subject Mapping</asp:label>*
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
<div style="clear:both; padding: 10px; width:100%">
	<!-- -->
	<div class="clearFloat"></div>	
	<div class="labelColumn requiredField">
		<asp:label id="Label2" associatedcontrolid="ddlFilterSubject" runat="server">Filter Subject</asp:label>*
	</div>
	<div class="dataColumn">
            <asp:DropDownList id="ddlFilterSubject"  runat="server"  AutoPostBack="True" OnSelectedIndexChanged="ddlFilterSubject_OnSelectedIndexChanged" ></asp:DropDownList>
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
        <asp:TemplateField HeaderText="Subject Mapping" >
        <EditItemTemplate>
            <asp:TextBox ID="gridFilterValue" runat="server" Text='<%# Bind("FilterValue") %>' width="300px" ></asp:TextBox>
            <asp:RequiredFieldValidator id="RequiredFieldValidator3" runat="server" ErrorMessage="A LR Value is required!" ControlToValidate="gridFilterValue"></asp:RequiredFieldValidator>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:label ID="gridlblFilterValue" runat="server" Text='<%# Bind("FilterValue") %>'></asp:label>
        </ItemTemplate>
    </asp:TemplateField>  
    <asp:TemplateField HeaderText="Subject"  >
        <EditItemTemplate>
            <asp:label ID="gridlblSubjects2" runat="server" Text='<%# Bind("SubjectTitle") %>'></asp:label>
        </EditItemTemplate>
        <ItemTemplate>
            <asp:label ID="gridlblSubjects" runat="server" Text='<%# Bind("SubjectTitle") %>'></asp:label>
        </ItemTemplate>
    </asp:TemplateField>

     
  
    <asp:boundfield datafield="MappedSubjectId" Visible="false" ></asp:boundfield>	               
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

<asp:RequiredFieldValidator id="rfvSubject" runat="server" ErrorMessage="A Subject is required!" ControlToValidate="ddlSubjects" Display="None"></asp:RequiredFieldValidator>
<asp:RequiredFieldValidator id="rfvFilterValue" runat="server" ErrorMessage="An filter item is required!" ControlToValidate="txtMapping" Display="None"></asp:RequiredFieldValidator>

</asp:panel>

</div>

</div>

<asp:Panel ID="hiddenStuff"  runat="server" Visible="false">
<!-- same as clusters -->
<asp:Literal ID="txtFormSecurityName" runat="server" Visible="false">ILPathways.Admin.mapping.MapClusters</asp:Literal>
</asp:Panel>

<asp:Panel ID="sqlPanel"  runat="server" Visible="false">

<asp:Literal ID="insertSql" runat="server" Visible="false">
INSERT INTO [dbo].[Map.K12Subject] ([FilterValue],[MappedSubjectId] ,[LastUpdatedBy]) VALUES ('{0}', {1}, '{2}' )
</asp:Literal>

<asp:Literal ID="updateSql" runat="server" Visible="false">
UPDATE [dbo].[Map.K12Subject] SET [FilterValue] = '{1}', LastUpdatedBy= '{2}',[LastUpdated] = GETDATE()  WHERE Id = {0}
</asp:Literal>

<asp:Literal ID="selectSql" runat="server" Visible="false">
SELECT base.Id, MappedSubjectId, cc.Title As SubjectTitle, FilterValue, Created, LastUpdated, LastUpdatedBy FROM [Map.K12Subject] base left join [dbo].[codes.Subject] cc on base.MappedSubjectId = cc.Id {0} order by cc.Title, base.FilterValue
</asp:Literal>


<asp:Literal ID="SubjectsSelect" runat="server" Visible="false">
SELECT [Id],Title FROM [dbo].[codes.Subject] where [IsActive]= 1 Order by 1
</asp:Literal>
</asp:Panel>



</asp:Content>