<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="Import.aspx.cs" Inherits="IOER.Organizations.Import" %>
<%@ Register Src="~/Organizations/controls/Import.ascx" TagPrefix="uc1" TagName="Import" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

    <script type="text/javascript">

    $(document).ready(function () {
        $(".inviteToggle").click(function () {
            $("#inviteInstructions").slideToggle('normal');
        });
    });

    function ShowHideSection(target) {
        $("#" + target).slideToggle();
    }
    

    function TogglePageGuide() {
        var current = $("#toggleHeader").html();
        if (current == "Show Page Content") {
            $("#toggleHeader").html('Show Guidance');
            $("#formInstructionsSection").hide();
            $("#currContentSection").show();

        }
        else {
            $("#toggleHeader").html('Show Page Content');
            $("#formInstructionsSection").show();
            $("#currContentSection").hide();
        }
    }

</script>

<style>
    .gridResultsHeader th {width: 200px; text-align: center;}
    .gridItem td, .gridAltItem td  {text-align: center;}
    .gridAltItem td  {background-color: #f5f5f5;}

        .instructionToggle { 
    color: #000;
    background-color: #F5F5F5;
    padding: 8px;
    margin: 8px auto;
    border-radius: 3px;
    -moz-box-sizing: border-box;
    -webkit-box-sizing: border-box;
    box-sizing: border-box;

}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    <div id="content" style="min-height: 500px;">
    <input type="hidden" name="CurrentOrgId" id="CurrentOrgId" value="0" />

    <h1 class="isleH1"><asp:Literal ID="litOrgTitle" runat="server" ></asp:Literal> - Import Users</h1>

        <div id="pageToggle" style="display:inline-block; width:200px;" class="instructionToggle" onclick="ShowHideSection('pageInstructions');" >Show/Hide Instructions</div>

        <div id="pageInstructions" style="display: none;">
            <ol>
                <li>Create a comma separated values (csv) file of your employee records</li>
                <li>Each record <strong>MUST</strong> include the following fields (for csv files use
	                field name headers)<ul>
                        <li>FirstName</li>
                        <li>LastName</li>
                        <li>Email</li>
                        <li>EmployeeTypeId (see valid types below</li>
                    </ul>
                </li>
                <li>Click "Upload CSV File" button</li>
                <li>Browse to computer and select file for upload and click "Open"</li>
                <li>Click "Upload File" button</li>
                <li>The system will import the file and display the results for review</li>
                <li>Click "Import Data" button</li>
                <li>Take note of system status. It will alert you to any errors and number of records
	imported.</li>
                <li>Close this window and return to the Organization Managment Page</li>
            </ol>
            <br />
            <br />
            <table class="DataWebControlStyle" cellspacing="0" rules="all" border="1">
                <tr class="gridResultsHeader">
                    <th >Field Name</th>
                    <th >Code</th>
                    <th >Description</th>
                </tr>

                <tr class="gridItem">
                    <td>EmployeeTypeId</td>
                    <td>1</td>
                    <td>Administrator</td>
                </tr>
                <tr class="gridAltItem">
                    <td></td>
                    <td>2</td>
                    <td>Employee</td>
                </tr>
                <tr class="gridItem">
                    <td></td>
                    <td>3</td>
                    <td>Student</td>
                </tr>
                <tr class="gridAltItem">
                    <td></td>
                    <td>4</td>
                    <td>External</td>
                </tr>
            </table>

            <br />
            <br />

            <a href="/Organizations/IOER_sample_Employee_import.csv" target="_blank">Click here to download a sample input file.</a>



        </div>
    

        <asp:Panel ID="importPanel" Visible="false" runat="server">
          <%--  <asp:Button ID="btnCloseImport" runat="server" CausesValidation="false" Text="Close Import" OnClick="btnCloseImport_Click" CssClass="defaultButton" />--%>
            <uc1:Import runat="server" ID="memberImport" />
        </asp:Panel>
</div>

    <asp:Panel ID="hiddenPanel" runat="server" Visible="false">


    </asp:Panel>
</asp:Content>
