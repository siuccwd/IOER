<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibraryAdmin.ascx.cs" Inherits="IOER.Controls.Libraries.LibraryAdmin" %>
<%@ Register Src="~/Controls/Libraries/LibraryMtce.ascx" TagPrefix="uc1" TagName="LibraryMtce" %>

<%@ Register
    Assembly="Obout.Ajax.UI"
    Namespace="Obout.Ajax.UI.HTMLEditor"
    TagPrefix="obout" %>
<%@ Register
    Assembly="Obout.Ajax.UI"
    Namespace="Obout.Ajax.UI.HTMLEditor.ContextMenu"
    TagPrefix="obout" %>
<%@ Register
    Assembly="Obout.Ajax.UI"
    TagPrefix="obout"
    Namespace="Obout.Ajax.UI.HTMLEditor.ToolbarButton" %>
<%@ Register
    TagPrefix="custom"
    Namespace="CustomToolbarButton" %>
  <script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
  <link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />

<style type="text/css">
    .formContainer{ padding-top: 10px; margin: 0px auto; text-align: center;  }
    .libLink { padding: 0 0 5px 5px;}
    .actionsSectionPanel { padding: 15px; border-radius: 5px; min-height: 500px;
margin-top: 15px; }
    .actionsSectionPanel ul { margin-left: 20px; }
    .optionsSections { 
        width: 85%;
    }
    .optionLinks a { margin-bottom: 1px; background-color: #3572B8; color: #FFF; display: block; 
                     text-decoration: none; font-size: 1.2em;}

    .optionLinks a:hover, .optionLinks a:focus { margin-bottom: 1px; background-color: #3572B8; color: #FFF; display: block; text-decoration: none; }
    h2 { font-size: 24px; }
    h3 { font-size: 20px; }
    .leftCol {
        display: inline-block;
        width: 400px;
        margin-left: 10px;
        text-align: left;
    }
     .rightCol {
        display: inline-block;
        width: 900px;
        margin-left: 10px; vertical-align:top;
        text-align: left;
    }
    .libraryTitle { font-size: x-large; }
        .vsErrorSummary { 
        width: 70%;
    }
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
    .isleBox ul {
        padding-left: 22px;
    }
    #h2libraryTitle { text-align: center; }
  #processing, #preButtonMessage { display: none; text-align: center; font-weight: bold; font-size: 20px; padding: 10px; }
    .removeCol { padding: 0px 5px; }
#formInstructionsSection     { display: none; }
#toggleSection { background-color: #3572B8; color: #fff; text-align: center; border-radius: 5px; padding: 5px 10px; margin-bottom: 5px; width: 300px;}
#toggleSection a, #toggleSection a:visited, #toggleSection a:focus {color: #fff;}
#toggleSection:hover{background-color: #FF5707; color: #000;}
    #plusMinus { display: none;
        font-size: 14px; padding: 0 4px; color: #fff;
    }
</style>
<style type="text/css">
    
@media screen and (max-width: 1400px) {
.formContainer { padding-top: 5px; }
.leftCol {  width: 350px; }
.rightCol {  width: 800px }
}

@media screen and (max-width: 1100px) {
.leftCol {  width: 300px; }
.rightCol {  width: 790px }
}

@media screen and (max-width: 800px) {
#h2libraryTitle { text-align: left; }
#formInstructionsSection     { width: 500px; }
.leftCol {  width: 100%; }
.rightCol {  width: 100% }
#formInstructionsSection     { width: 100%; }
}
@media screen and (max-width: 600px) {
/*.leftCol {  width: 400px }
.rightCol {  width: 400px; }*/
#plusMinus { display: inline-block;}
#adminOptionsSection { display: none; }
#h2libraryTitle { text-align: left; }

}

</style>
<script type="text/javascript">

    $(document).ready(function () {
        $(".inviteToggle").click(function () {
            $("#inviteInstructions").slideToggle('normal');
        });
    });

    function ShowHideSection(target) {
        $("#" + target).slideToggle();
    }
    function setSubmitState() {
        $("#processing").show();
        $(".btnSubmit").hide();
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
    function ToggleLibraryOptions() {
        var current = $("#plusMinus").html();
        if (current == "+") {
            $("#plusMinus").html('-');
            $("#adminOptionsSection").show();

        }
        else {
            $("#plusMinus").html('+');
            $("#adminOptionsSection").hide();
        }
    }
</script>
<ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
<h1 class="isleH1"><asp:literal id="pageTitle" runat="server">Library Administration</asp:literal></h1>

<asp:Panel ID="formContainer" CssClass="formContainer" runat="server">
    <div>
    <div class="row-fluid span12">
    <asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" forecolor="" CssClass="errorMessage vsErrorSummary" runat="server"></asp:validationsummary>
    </div>

    <div class="leftCol">
     <div id="toggleSection">
            <a href="javascript:void(0);">
                <h2 id="toggleHeader" onclick="TogglePageGuide();" class="message">Show Guidance</h2>
            </a>
        </div>
        <h2>Library</h2>
        <div style="padding-left: 8px;">
            <asp:DropDownList ID="sourceLibrary" runat="server" OnSelectedIndexChanged="sourceLibrary_SelectedIndexChanged" width="100%" AutoPostBack="true"></asp:DropDownList>
            <br /><asp:LinkButton ID="refreshLibrariesLink" runat="server" Visible="false" Text="Refresh Libraries List" OnClick="refreshLibrariesLink_Click"></asp:LinkButton>
            <br /><asp:LinkButton ID="showAllLibraries" runat="server" Visible="false" Text="Show All Org. Libraries" OnClick="showAllLibraries_Click"></asp:LinkButton>
            <br /><asp:LinkButton ID="showAllUserLibraries" runat="server" Visible="false" Text="Show All User Libraries" OnClick="showAllUserLibraries_Click"></asp:LinkButton>
            <br /><asp:label ID="sourceLibraryId" Visible="false" runat="server"></asp:label>
        </div>
 
        <div style="display:none;">
        <div class="labelColumn">Collection</div>
        <div class="dataColumn">
            <asp:DropDownList ID="sourceCollection" runat="server" AutoPostBack="true"  OnSelectedIndexChanged="sourceCollection_SelectedIndexChanged"></asp:DropDownList>
            <br /><asp:TextBox ID="collectionId" Visible="false"  runat="server"></asp:TextBox>
        </div>
        </div>
        <div class="isleBox optionsSections">
            <h2 class="isleBox_H2"  onclick="ToggleLibraryOptions();"><span id="plusMinus">+</span>Library Options</h2>
            <div id="adminOptionsSection" class="optionLinks">
                <asp:LinkButton ID="newLibraryLink" runat="server" Text="New Library" CssClass="libLink" CausesValidation="false" OnClick="newLibraryLink_Click" ></asp:LinkButton>
                <asp:LinkButton ID="myLibMemberships" runat="server" Text="My Library Memberships" Visible="true" CssClass="libLink" CausesValidation="false" OnClick="myLibMemberships_Click" ></asp:LinkButton>
                <asp:LinkButton ID="editLibraryLink" runat="server" Visible="false" Text="Edit Library" CssClass="libLink" CausesValidation="false" OnClick="editLibraryLink_Click"></asp:LinkButton>
                <asp:LinkButton ID="libMbrsLink" runat="server" Visible="false" Text="Library Members" CssClass="libLink" CausesValidation="false" OnClick="libMbrsLink_Click"></asp:LinkButton>
                <asp:LinkButton ID="libInviteLink" runat="server" Visible="false" Text="Library Invitations" CssClass="libLink" CausesValidation="false" OnClick="libInviteLink_Click"></asp:LinkButton>
                <asp:LinkButton ID="libApprovalLink" runat="server" Visible="false" Text="Approve/Reject Pending Resources" CssClass="libLink" CausesValidation="false" OnClick="libApproveLink_Click"></asp:LinkButton>

            </div>

        </div>

        <asp:Panel ID="collectionPanel1" CssClass="optionLinks" visible="false" runat="server">
        <div class="isleBox optionsSections">
            <h2 class="isleBox_H2">Collection Options</h2>
            <asp:Panel ID="colOptionsPanel" CssClass="optionLinks" Enabled="false" runat="server">
                <asp:LinkButton ID="colMbrsLink" runat="server" Text="Collection Members" CssClass="libLink" CausesValidation="false" OnClick="colMbrsLink_Click"></asp:LinkButton>
                <asp:LinkButton ID="colInviteLink" runat="server" Text="Collection Invitations" CssClass="libLink" CausesValidation="false" OnClick="colInviteLink_Click"></asp:LinkButton>

            </asp:Panel>

        </div>
        </asp:Panel>
       

    
 <asp:Panel ID="startPanel" runat="server" Style="min-height: 500px; width: 500px; border-radius: 5px;" Visible="false">
        </asp:Panel>  
    </div>
    <div class="rightCol" >
        <div id="currContentSection">
            <div >
            <h2 id="h2libraryTitle" ><asp:Label ID="litCurrentLibrary" CssClass="libraryTitle" Text="No Library Selected" runat="server"></asp:Label></h2>
                <div style="display:none;">
            <h3>Current Collection: <asp:Literal ID="litCurrentCollection" Text="" runat="server"></asp:Literal></h3>
                    </div>
            </div>
            <div class="actionsSectionPanel">

        <!-- ================= Library======================================= -->
        <asp:Panel ID="LibraryPanel" runat="server" Visible="false">
             <asp:Panel ID="orgsPanel" runat="server" Visible="false">
                <div class="labelColumn">Organizations</div>
                <div class="dataColumn">
                    <asp:DropDownList ID="ddlOrgs" runat="server" ></asp:DropDownList>
                </div>
             </asp:Panel>
                 <uc1:LibraryMtce runat="server" ID="LibraryMtce1" DefaultLibraryTypeId="2" InitializeOnRequest="false" />


        </asp:Panel>    
        <!-- ================= collection===================================== -->
            <asp:Panel ID="collectionPanel" runat="server" Visible="false">

            </asp:Panel>                
        <!-- ================= members======================================== -->
            <asp:Panel ID="membersPanel" runat="server" Visible="false">
                <h2>Members</h2>
                <div class="labelColumn">&nbsp;</div>
                <div class="dataColumn">
                    <asp:LinkButton ID="pendingMembers" runat="server" Visible="true" Text="Show Pending Members" OnClick="pendingMembers_Click"></asp:LinkButton>
                </div>

                <hr />
                <asp:Panel ID="mbrSearchPanel" runat="server" Visible="true">
                    <div class="labelColumn">
                        <asp:Label ID="Label6" AssociatedControlID="ddlFilterMemberType" runat="server">Member Type </asp:Label></div>
                    <div class="dataColumn">
                        <asp:DropDownList ID="ddlFilterMemberType" runat="server">
                            <asp:ListItem Text="All" Value=""></asp:ListItem>
                            <asp:ListItem Text="Pending" Value="0"></asp:ListItem>
                            <asp:ListItem Text="Reader" Value="1"></asp:ListItem>
                            <asp:ListItem Text="Contributor" Value="2"></asp:ListItem>
                            <asp:ListItem Text="Curator" Value="3"></asp:ListItem>
                            <asp:ListItem Text="Administrator" Value="4"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div style="display: none;">
                        <asp:Label ID="Label1" runat="server">Keyword:</asp:Label>
                        <asp:TextBox ID="txtKeyword" runat="server" Width="300px"></asp:TextBox>
                    </div>
                    <br />
                    <div class="labelColumn">&nbsp;</div>
                    <div class="dataColumn">
                        <asp:Button ID="searchLink" runat="server" Text="Search"  CssClass="defaultButton" OnClick ="searchLink_Click"></asp:Button>
                    </div>

                </asp:Panel>

                <div style="width: 100%">
                    <div class="clear" style="float: right;">
                        <div class="labelColumn" style="vertical-align: middle;">Page Size</div>
                        <div class="dataColumn" style="text-align: center; font-size: 100%;">
                            <asp:DropDownList ID="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:DropDownList>
                        </div>
                    </div>

                    <br class="clearFloat" />
                    <asp:GridView ID="membersGrid" runat="server" AutoGenerateColumns="False"
                        AllowPaging="true" PageSize="50" AllowSorting="false"
                        AutoGenerateEditButton="True" DataKeyNames="Id"
                        OnRowCommand="membersGrid_RowCommand"
                        OnPageIndexChanging="membersGrid_PageIndexChanging"
                        OnSorting="membersGrid_Sorting"
                        OnRowEditing="EditRecord"
                        OnRowDataBound="membersGrid_RowDataBound"
                        OnRowCancelingEdit="CancelRecord"
                        OnRowUpdating="UpdateRecord"
                        BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="Horizontal" Width="100%"
                        CaptionAlign="Top"
                        UseAccessibleHeader="true">
                        <HeaderStyle CssClass="gridResultsHeader" HorizontalAlign="Left" />
                        <Columns>
                            <asp:TemplateField Visible="false" HeaderText="Select">
                                <ItemTemplate>
                                    <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("Id") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
			    Select</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Remove"  ItemStyle-CssClass="removeCol">
                                <ItemTemplate>
                                    <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("Id") %>' CommandName="DeleteRow" CausesValidation="false" OnClientClick="return confirm('Are you certain that you want to remove this member?');" runat="server">Remove</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Library" HeaderText="xxx" Visible="false"></asp:BoundField>
                            <asp:TemplateField HeaderText="" SortExpression="Library">
                                <ItemTemplate>
                                    <asp:Label ID="gridlblLibrary" runat="server" Text='<%# Bind("Library") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Member/Organization" SortExpression="SortName">
                                <EditItemTemplate>
                                    <asp:Label ID="gridlblMemberName" runat="server" Text='<%# Bind("MemberName") %>'></asp:Label>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="gridlblMemberName2" runat="server" Text='<%# Bind("MemberName") %>'></asp:Label>
                                    <br /><%# Eval( "Organization" )%>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Member Type" SortExpression="MemberType">
                                <EditItemTemplate>
                                    <asp:Label ID="gridlblLibMbrMsg" Visible="false" runat="server"></asp:Label>
                                    <asp:DropDownList ID="gridDdlTypes" Visible="true" runat="server"></asp:DropDownList>
                                    <%--  <asp:RequiredFieldValidator ID="rfvGridTypes" runat="server" ErrorMessage="A Member Type is required!" ControlToValidate="gridDdlTypes"></asp:RequiredFieldValidator>--%>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="gridlblMemberName" runat="server" Text='<%# Bind("MemberType") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Library Org. Association"  >
                                <EditItemTemplate>
                                    <asp:Label ID="gridlblNoOrgMbrMsg" Visible="false" runat="server" Text="N/A - no organization for this library"></asp:Label>
                                    <asp:DropDownList ID="gridDdlOrgMbrType" Visible="false" runat="server"></asp:DropDownList>
                                   
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="gridlblOrgMbrType" runat="server" Text='<%# Bind("OrgMemberType") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                           
                            <asp:BoundField DataField="MemberTypeId" HeaderText="Member Type Id" Visible="false"></asp:BoundField>
                            <asp:BoundField DataField="OrganizationId" HeaderText="Org. Id" Visible="false"></asp:BoundField>

                            <asp:TemplateField HeaderText="Last Updated" ItemStyle-HorizontalAlign="Right" SortExpression="LastUpdated">
                                <EditItemTemplate>
                                    <asp:Label ID="gridlblLastUpdated" runat="server" Text='<%# Bind("LastUpdated", "{0:d}") %>'></asp:Label>
                                </EditItemTemplate>
                                <ItemTemplate>
                                    <asp:Label ID="gridlblLastUpdated2" runat="server" Text='<%# Bind("LastUpdated","{0:d}") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:BoundField DataField="Created" Visible="false" HeaderText="Added" SortExpression="Created" DataFormatString="{0:d}"></asp:BoundField>

                        </Columns>
                        <PagerSettings Visible="false"
                            Mode="NumericFirstLast"
                            FirstPageText="First"
                            LastPageText="Last"
                            PageButtonCount="5"
                            Position="TopAndBottom" />

                    </asp:GridView>
                    <div style="float: left; margin-top: 10px;">
                        <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next" LastClause="Last Page" PageSize="25" CompactModePageCount="10" NormalModePageCount="10" GenerateGoToSection="false" GeneratePagerInfoSection="true" />
                    </div>
                    <br class="clearFloat" />
                </div>
            </asp:Panel>

        <!-- ================= invitations==================================== -->
            <asp:Panel ID="invitationsPanel" runat="server" Visible="false">
                <h3 >Invitations</h3>
                   <asp:LinkButton ID="showInvitations" runat="server" Visible="false" Text="Show Pending Invitations" OnClick="showInvitations_Click"></asp:LinkButton>
                <br />
                    <asp:Panel ID="invitePanelStart" runat="server" visible="true" >
                        <div id="inviteToggle" style="display:inline-block; width:100%;" class="instructionToggle" onclick="ShowHideSection('inviteInstructions');" >Show/Hide Instructions</div>
                        <div id="inviteInstructions" style="display:none;" >
                            You may invite people to:
                        <ul>
                            <li><strong>View</strong> the selected library (if not already publically available)</li>
                            <li><strong>Contribute</strong> to the selected library (if the library does not already have open contributions (see library settings)</li>
                            <li><strong>Curate</strong> the selected library. A curator can contribute to a library and may also perform administrative functions on the library - that is use this page to manager members, etc.</li>
                            <li><strong>Administer</strong> the selected library. An administrator has the same privileges as a curator, but may also add curators, and other administrators.</li>
                        </ul>
                   
                        <p>You start by entering the email address of the invitee and library role. The system will first search for an existing account for the email. If an account has not yet been created for the invitee, you can choose to create a starting account. This will result in an easier registration process for the invitee. You can also specify if the invitee should be added to your organization. <br />NOTE: this function will soon be available from an organization administration page and will ease managing library members.</p>
                        </div>

                        <div class="labelColumn"><asp:Label ID="Label2" AssociatedControlID="txtEmail"  runat="server" >Email</asp:Label></div>
                        <div class="dataColumn"><asp:TextBox ID="txtEmail" runat="server" Width="300px"></asp:TextBox> </div>
                        <div class="clearFloat"></div>

                        <div class="labelColumn"><asp:Label ID="Label3" AssociatedControlID="ddlMemberType"  runat="server" >Member Type </asp:Label></div>
                        <div class="dataColumn">
                            <asp:DropDownList ID="ddlMemberType" runat="server">
                                <asp:ListItem Text="Reader"         Value="1" Selected="True"></asp:ListItem>
                                <asp:ListItem Text="Contributor"    Value="2" ></asp:ListItem>
                                <asp:ListItem Text="Curator"        Value="3" ></asp:ListItem>
                                <asp:ListItem Text="Administrator"  Value="4" ></asp:ListItem>
                            </asp:DropDownList>
                            </div>
                            <div class="dataColumn">
                            <a class="toolTipLink" id="A1" title="Library Member Type|Valid member types:<ul><li>Reader - has read access to the library (typically used where library does not have public access).</li><li>Contributor - can add resources the library (typically used where the library does not have open contributions and user is not part of organization with implicit contribution to organization libraries)</li><li>Curator - a curator may manage the library, including managing members, and approving resources</li><li>Administrator - can manage curators and other administrators</li></ul>"><img
			    src="/images/icons/infoBubble.gif" alt="" /></a>
                        </div>
                        <div class="clearFloat"></div>
                        <div class="labelColumn">&nbsp;</div>
                        <div class="dataColumn" style="margin-top: 10px;">
                        <asp:LinkButton ID="inviteStep2Link" runat="server" Text="Continue" CssClass="defaultButton" OnClick="inviteStep2Link_Click"></asp:LinkButton>
                            </div>
                    </asp:Panel>
                <!-- ========================================================================================== -->
                    <asp:Panel ID="invitePanel2" runat="server" visible="false" >
                        <asp:Literal runat="server" ID="notFoundMsg"  Visible="false"><p>The requested email was not found in our system. You can create a quick account for the invitee (saving them time) or just have them create the account upon responding to this invitation. Either:</p>
                        <ul>
                            <li>Enter the first and last name below (a temporary password will be generated) and indicate if this user should also be added to your organization</li>
                            <li>Or, just indicate if the user should be added to your organization after registering</li>
                        </ul>
                    
                            </asp:literal>
                        <p>Indicate if this user should be added to your organization:</p>
                        <asp:label ID="userFoundMsg" runat="server" Visible="false"></asp:label>
                    
                        <div class="isleBox">
                        <h3>Why should I add a user to this organization?</h3>
                        <p>This is an optional step. However, organization members:</p>
                        <ul>
                            <li>Will have immediate access to all existing organization libraries that allow implicit access to organization members </li>
                            <li>Will have immediate access to new (created in the future) organization libraries that allow implicit access to organization members </li>
                            <li>Will have immediate <b>contribute</b> capability to any organization library that allows open contribute (with or without approval) to organization members </li>
                            <li>Immediate access to any organization related function added to the system in the future</li>
                        </ul>
                        </div>
                        <asp:RadioButtonList ID="rblAddToOrg" runat="server">
                            <asp:ListItem Text="Do NOT add To My Organization" Value="0" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Add as Administration"            Value="1"></asp:ListItem>
                            <asp:ListItem Text="Add as Staff member/Employee"     Value="2"></asp:ListItem>
                            <asp:ListItem Text="Add as Student"                   Value="3"></asp:ListItem>
                            <asp:ListItem Text="Add as Contrator/External"        Value="4"></asp:ListItem>
                        </asp:RadioButtonList>
                        <br />
                        <asp:Button ID="inviteStep3NoAcct" Visible="false" runat="server" Text="Continue (do not create account)" OnClick="inviteStep3NoAcct_Click" CssClass="defaultButton" CausesValidation="false" ></asp:Button>
                        <asp:Button ID="inviteStep4HasAcct" Visible="false" runat="server" Text="Continue" OnClick="inviteStep4HasAcct_Click" CssClass="defaultButton"  CausesValidation="false"></asp:Button>
                        <br />
                        <asp:Panel ID="createAcctPanel" runat="server" Visible="false">
                            <br />
                        <h3>Create Account:</h3>
                        <div class="labelColumn"><asp:Label ID="Label4" AssociatedControlID="txtFirstName"  runat="server" >First Name</asp:Label></div>
                        <div class="dataColumn"><asp:TextBox ID="txtFirstName" runat="server" Width="300px"></asp:TextBox> </div>
                        <div class="clearFloat"></div>
                        <div class="labelColumn"><asp:Label ID="Label5" AssociatedControlID="txtLastName"  runat="server" >Last Name</asp:Label></div>
                        <div class="dataColumn"><asp:TextBox ID="txtLastName" runat="server" Width="300px"></asp:TextBox> </div>
                        <div class="clearFloat"></div>
                        <div class="labelColumn">&nbsp;</div>
                        <div class="dataColumn" style="margin-top:10px;"><asp:Button ID="inviteStep5CreateAcct" runat="server" Text="Continue and Create Account" OnClick="inviteStep5CreateAcct_Click" CssClass="defaultButton" ></asp:Button> </div>
                    
                        </asp:Panel>
                    </asp:Panel>
            
                    <asp:Panel ID="orgRolePanel" runat="server" Visible="false">
                        <!-- User should have some min org admin role to assign these.
                            - also should probably not be able to assign a role above their own? 
                            -->
                       <p>You may optionally assign organization roles for this person. If applicable, select one or more roles to be assigned to this person.</p>
                    
                        <asp:CheckBoxList ID="cblOrgRoles" runat="server"></asp:CheckBoxList>
                        <br />
                        <asp:Button ID="btnOrgRoleContinue" Visible="true" runat="server" Text="Continue" OnClick="inviteOrgRole_Click" CssClass="defaultButton" CausesValidation="false" ></asp:Button>
                    </asp:Panel>
            
                    <asp:Panel ID="messagePanel" runat="server" Visible="false">
                        <h3>Optionally add a Message</h3>
                        <div class="dataColumn">Enter message <a class="toolTipLink" id="tipFile" title="Custom Message|You may optionally include a custom message. This will useful if the invitee is not familiar with this website or may not know how to start using the site."><img
			    src="/images/icons/infoBubble.gif" alt="" /></a></div>
                        <asp:TextBox ID="inviteMessage" Visible="false" runat="server" Rows="3" style="width:90%;" TextMode="MultiLine"></asp:TextBox>
                        <obout:EditorPopupHolder runat="server" id="popupHolder"  />
                	    <obout:Editor ID="txtMessage" runat="server" Height="200px" Width="95%">
                            <EditPanel ID="EditPanel1" FullHtml="false" runat="server"></EditPanel>
                            <TopToolBar Appearance="Lite" >
                                <AddButtons>
                                    <obout:HorizontalSeparator ID="HorizontalSeparator1" runat="server" />
                                    <obout:OrderedList /><obout:BulletedList />
                                    <obout:InsertLink /><obout:RemoveLink />
                                </AddButtons>
                            </TopToolBar>
                            <BottomToolBar ShowDesignButton="true" ShowHtmlTextButton="true" ShowPreviewButton="true" ShowFullScreenButton="true" ></BottomToolBar>
                         </obout:Editor>

                        <asp:Button ID="inviteStep6Final" Visible="true" runat="server" Text="Finish" OnClientClick="setSubmitState()" OnClick="inviteStep6Final_Click" CssClass="defaultButton btnSubmit" ></asp:Button>
                        <p id="processing">Processing, Please wait...</p>
                    </asp:Panel>
                    <asp:Panel ID="finshInvitePanel" runat="server" Visible="false">
                        <h3>Completed Invitation</h3>
                        <p>The invitation has been sent.</p>
                        <p>Click the <u>Start a New Invitation</u> link below to send another invitation.</p>
                         <asp:LinkButton ID="newInvitationLink" runat="server" Text="Start a New Invitation"  OnClick="newInvitationLink_Click"></asp:LinkButton>
                    </asp:Panel>

                    <asp:Panel ID="pendingInvitesPanel" runat="server" visible="false" >
                        <h3>Pending Invitations</h3>
                    </asp:Panel>

                <asp:RequiredFieldValidator id="rfvEmail" runat="server" Display="None" ControlToValidate="txtEmail" ErrorMessage="Please enter an email address"  Enabled="false"></asp:RequiredFieldValidator>
                <asp:RequiredFieldValidator id="rfvFirstName" runat="server" Display="None" ControlToValidate="txtFirstName"  ErrorMessage="Please enter a first name"  Enabled="false"></asp:RequiredFieldValidator>
                <asp:RequiredFieldValidator id="rfvLastName" runat="server" Display="None" ControlToValidate="txtLastName"  ErrorMessage="Please enter a last name"  Enabled="false"></asp:RequiredFieldValidator>
            </asp:Panel>

        <!-- ================= followers ==================================== -->
            <asp:Panel ID="followersPanel" runat="server" Visible="false">

            </asp:Panel>  

        <!-- ================= approvals ===================================== -->
            <asp:Panel ID="approvePanel" runat="server" Visible="false">
                <h3>Approve or Reject Pending Resources</h3>
                <script type="text/javascript">
                  var pendingResources = <%=pendingResources %>;
                  var userGUID = "<%=userGUID %>";
                  var libraryID = <%=CurrentLibraryId %>;
                  $(document).ready(function() {
                    loadApprovalData();
                  });

                  function loadApprovalData() {
                    var template = $("#template_pendingResource").html();
                    var pendingBox = $("#pendingResources");
                    pendingBox.html("");
                    for(i in pendingResources){
                      var item = pendingResources[i];
                      pendingBox.append(
                        template
                          .replace(/{id}/g, item.id)
                          .replace(/{title}/g, item.title)
                          .replace(/{submitter}/g, item.submittingUserName)
                          .replace(/{userID}/g, item.submittingUserID)
                          .replace(/{date}/g, item.submittedOnDate)
                          .replace(/{resourceURL}/g, item.resourceURL)
                          .replace(/{collection}/g, item.targetCollectionTitle)
                          .replace(/{libraryID}/g, item.targetLibraryID)
                          .replace(/{collectionID}/g, item.targetCollectionID)
                          .replace(/{thumbsrc}/g, "src=\"//ioer.ilsharedlearning.org/OERThumbs/large/" + item.resourceIntId + "-large.png\"")
                      );
                    }
                  }

                  function approveAll() {
                    $(".pendingResourceControls input[value=true]").prop("checked", "checked");
                  }
                  function rejectAll() {
                    $(".pendingResourceControls input[value=false]").prop("checked", "checked");
                  }
                  function ignoreAll() {
                    $(".pendingResourceControls input[value=none]").prop("checked", "checked");
                  }

                  function saveApprovals() {
                    //Hold the data 
                    var approvals = [];
                    //Fill out the data
                    $("#pendingResources .pendingResourceControls").each(function() {
                      var selection = $(this).find("input:checked");
                      var reason = $(this).find("input[type=text]").val();
                      var selectedValue = selection.attr("value");
                      var id = parseInt(selection.attr("data-resourceID"));
                      if(selectedValue == "true" || selectedValue == "false"){ //Only take action if needed
                        approvals.push({ id: id, approved: selectedValue == "true", reason: reason });
                      }
                    });
                    //AJAX call
                    doAjax("SaveApprovalsJSON", { libraryID: libraryID, userGUID: userGUID, approvals: approvals }, successSaveApprovals);
                  }

                  function doAjax(method, data, success){
                    $.ajax({
                      url: "/Services/LibraryService.asmx/" + method,
                      async: true,
                      success: function(msg){ success($.parseJSON(msg.d)); },
                      type: "POST",
                      data: JSON.stringify(data),
                      contentType: "application/json; charset=utf-8",
                      dataType: "json"
                    });
                  }

                  function successSaveApprovals(data){
                    if(data.isValid){
                      pendingResources = data.data;
                      loadApprovalData();
                    }
                    else {
                      alert(data.status);
                    }
                  }
                </script>
                <link rel="stylesheet" type="text/css" href="/styles/common2.css" />
                <style type="text/css">
                  .pendingResource { position: relative; margin-bottom: 5px; padding: 5px; background-color: #EEE; border-radius: 5px; padding: 5px 105px 5px 130px; min-height: 100px; }
                  .pendingResource img { position: absolute; top: 5px; right: 5px; width: 120px; }
                  .pendingResourceControls { position: absolute; top: 5px; left: 5px; width: 120px; }
                  .pendingResourceControls label { display: block; border-radius: 5px; padding: 1px 3px; }
                  .pendingResourceControls label:hover, .pendingResourceControls label:focus { cursor: pointer; background-color: #DDD; }
                  .pendingResourceControls input[type=text] { width: 100%; margin-top: 2px; }
                  #pendingResourceButtons { padding: 0 5px; margin-bottom: 5px; }
                  #pendingResourceButtons input { display: inline-block; width: 100px; vertical-align: top; }
                  #pendingResourceButtons input.bgLightGray { color: #333; }
                  #pendingResourceButtons input.bgLightGray:hover, #pendingResourceButtons input.bgLightGray:focus { color: #FFF; }
                </style>  

                <div id="pendingResourceButtons">
                  <input type="button" class="isleButton bgLightGray " onclick="ignoreAll()" value="No Actions" />
                  <input type="button" class="isleButton bgGreen" onclick="approveAll();" value="Approve All" />
                  <input type="button" class="isleButton bgRed" onclick="rejectAll();" value="Reject All" />
                  <input type="button" class="isleButton bgBlue" onclick="saveApprovals();" value="Confirm" />
                </div>
                <div id="pendingResources"></div>
            
                <div id="template_pendingResource" style="display: none;">
                  <div class="pendingResource" data-resourceID="{id}">
                    <img {thumbsrc} />
                    <div class="pendingResourceInfo">
                      <div class="submitter"><a href="/profile/{userID}/" target="_blank">{submitter}</a> Submitted on {date}:</div>
                      <div class="title"><b>{title}</b></div>
                      <div class="targetCollection">to <i><a href="/library/collection/{libraryID}/{collectionID}/">{collection}</a></i></div>
                      <div class="approveLinks">
                          <a href="{resourceURL}" target="_blank">View Resource</a> | <a href="/Resource/{id}" target="_blank">View Metadata</a>
                      </div>
                    </div>
                    <div class="pendingResourceControls">
                      <label for="noaction_{id}"><input type="radio" name="decide_{id}" id="noaction_{id}" data-resourceID="{id}" value="none" checked="checked" /> No Action</label>
                      <label for="approve_{id}"><input type="radio" name="decide_{id}" id="approve_{id}" data-resourceID="{id}" value="true" /> Approve</label>
                      <label for="reject_{id}"><input type="radio" name="decide_{id}" id="reject_{id}" data-resourceID="{id}" value="false" /> Reject</label>
                      <input type="text" name="reason_{id}" placeholder="Reason..." />
                    </div>
                  </div>
                </div>
            </asp:Panel>

            </div>
        </div>

        <div id="formInstructionsSection" >
            <div class="isleBox">
                <h2>Getting Started</h2>
                <p>The library administration page allows you to:</p>
                <ul>
                    <li>Access any library which you created or for which you have edit privileges.</li>
                    <li><strong>Library Memberships</strong>
                        <ul><li>View all of your library memberships.</li>
                            <li>You can optionally remove yourself from a member library.<br />Note: if you remove yourself from a library where you do not have administrator priviliges, you will not be able to restore your access/membership.</li></ul>
                    </li>
                    <li><strong>Send Library Invitations </strong>
                        <ul><li>Invite other people to contribute to your library including your own user library, or any organization library for which you are an administrator.</li>
                            <li>The invitation can be sent to users with existing accounts, or to new users.</li><li>If an account does not exist, the system will create the initial account and send an email to notify the new user to activate the account.</li><li>If you have administrator privileges for your organization, you can also choose to add the invitee to your organization.</li></ul> 
                    </li>
                    <li><strong>Manage your Library Members </strong>
                        <ul>
                            <li>Manage your library members by assigning specific roles such as contributor, versus reader.</li>
                            <li>Handle requests to join your library.<br />The administrator(s) for a library will receive an email whenever a user submits a request to join the library. The email will contain a link to log the administrator into the system, display the library members view on the Library Administration page, and list all members with type of <i>Pending</i>.<br />Click on the Edit link next to the user's name, select a library role, and optionally select an organization role.</li>
                        </ul> 

                    </li>
                    <li><strong>Approve Library Submissions </strong>
                        <ul>
                            <li>Manage submissions to your library by contributors (who require approval), or others if the library has open submissions.</li>
                            <li>Approve entries or reject entries (with an optional reason)</li>
                        </ul> 

                    </li>
                    <li><strong>Create Organization Libraries</strong>
                         <ul><li>Create one or more organization libraries.</li><li>This function is only available if the current user has the necessary administrator roles for the organization.</li></ul>  </li>
                    <li><strong>Library Updates</strong>
                         <ul><li>Update the properties for library (so you don't have to jump back to the library page to make quick updates).</li></ul></li>
                </ul>
            </div>
            <div class="isleBox">
                <h2>Next Steps</h2>
                <ul>
                    <li>Select a library from the dropdown list<br />
                        <b>No libraries?</b>
                        <ul>
                            <li><a href="/My/Library">Navigate to the Library page</a> and create your 'user' library.</li>
                            <li>Or, if you believe you should have edit access to an organization library, contact the administrator and request the appropriate access to the library.</li>
                        </ul>
                    </li>
                    <li>Then select one of the displayed options.</li>
                </ul>
            </div>
        </div>
    </div>
    </div>
</asp:Panel>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<!-- control variables -->
<asp:Literal ID="formSecurityName" runat="server" Visible="false">Site.Admin</asp:Literal>
<asp:Literal ID="doingBccOnRegistration" runat="server">yes</asp:Literal>
<asp:Literal ID="ccApproverWithMbrConfirm" runat="server">yes</asp:Literal>
<asp:Literal ID="txtCurrentLibraryId" runat="server">0</asp:Literal>
<asp:Literal ID="txtCurrentLibraryMemberId" runat="server">0</asp:Literal>
<asp:Literal ID="autoAddMbrAsOrgExternalMbr" runat="server">yes</asp:Literal>
<asp:Literal ID="allowingChangeToOrgMbr" runat="server">yes</asp:Literal>
<asp:Literal ID="libraryInviteExpiryDays" runat="server">30</asp:Literal>
<asp:Literal ID="isMyMembershipsSearch" runat="server">no</asp:Literal>
<asp:Literal ID="retainingCurrentSectionOnNewSelection" runat="server">yes</asp:Literal>
                        
<asp:Label ID="acctCreatedMsg" runat="server">
<p>An account has been created for you:</p>
User Name (email): {0}
<br />Password (temporary): {1} 
<p>Click on the following link to confirm your account and update your profile:</p>
    <a href="{2}">Confirm your IOER registration</a>
<br />
</asp:Label>
<asp:Label ID="invitationMessageContent" runat="server" >Following are some helpful links for this site:<ul><li><a href='{0}'>Visit our library</a></li><li><a href="{1}">View the getting started guide</a></li><li>Explore the site, <a href="{2}">perhaps starting with current libraries</a></li></ul></asp:Label>
    
<asp:Label ID="userFoundOrgMessage" runat="server" ><h3>Existing Account</h3><p>An account was found for <strong>{0}</strong>. Do you want to add this person to your organization?</p><p> Organization libraries can be configured to allow any organization member to contribute by just being a a member of the same organization. This will ease administration as an org member will have implicit access to any libraries or collections created in the future.</p> </asp:Label>
<asp:Literal ID="contributeLink" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Contribute</asp:Literal>
<asp:Literal ID="contributeLink1" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Libraries/Library.aspx?id={1}</asp:Literal>
<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?pg={0}&a=activate&nextUrl=/Account/Profile.aspx</asp:Literal>
<asp:Literal ID="activateLink1" runat="server" >/Account/Profile.aspx?pg={0}&a=activate</asp:Literal>
<asp:Literal ID="registerLink" runat="server" >/Account/Register.aspx?invite={0}</asp:Literal>


<asp:Literal ID="libraryLink" runat="server" >/Libraries/Library.aspx?id={0}</asp:Literal>
<asp:Literal ID="gettingStartedLink" runat="server" >/Help/Guide.aspx</asp:Literal>
<asp:Literal ID="librariesHome" runat="server" >/Libraries/Default.aspx</asp:Literal>

<asp:literal ID="autoLoginLinkLibrariesSearch" runat="server">/Account/Login.aspx?pg={0}&nextUrl=/Libraries/Default.aspx</asp:literal>
<asp:Literal ID="autoLoginLinkLibrary" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Library/{1}/WelcomeToLibrary</asp:Literal>
<asp:Literal ID="autoLoginLinkContribute" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Contribute</asp:Literal>
<asp:Literal ID="autoLoginLinkSearch" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Search.aspx</asp:Literal>
<asp:Literal ID="autoLoginLinkGuide" runat="server" >/Account/Login.aspx?pg={0}&nextUrl=/Help/Guide.aspx</asp:Literal>
</asp:Panel>

<asp:Panel ID="emailsPanel1" runat="server" Visible="false">   
         
<asp:Label ID="inviteSubject" runat="server">Invitation From  {0} to join a library</asp:Label>
<asp:Label ID="inviteEmail" runat="server">
Welcome<br />This is an invitation to access the IOER <strong>{0}</strong> library with a role of  {1}.
<p>You can log in and start contributing to this and other libraries.</p>
</asp:Label>
<asp:Label ID="doRegisterMsg" runat="server">
<p>First you must create a new account using your email address using the following link:</p><p>{0}</p></asp:Label>

<asp:Label ID="visitLibraryMsg" runat="server"><p>You may visit this library by clicking on the following link:<br /><a href="{0}" >Visit: <strong>{1}</strong></a></p></asp:Label>


<asp:Label ID="adLibrarySummaryDTOConfirmSubject" runat="server">Confirmation of addition as a member to library: "{0}"</asp:Label>
<asp:Label ID="adLibrarySummaryDTOConfirmMsg" runat="server">
<div style="max-width: 800px">
<b>Congratulations</b><br />
<p>{7} has added you as a member of the IOER <strong>"{0}"</strong> library with a role of {1}.</p>

<p>New to IOER? <br />You may want to start by visiting the <strong><a href="{6}">User Guide</a></strong></p>
<p>Ready to get started? </p>
    <ul>
        <li><a href="{2}">Contribute Resources</a></li>
        <li><a href="{3}">Search for Resources</a></li>
        <li><a href="{4}">Check out your new Library branch</a></li>
        <li><a href="{5}">Search for other libraries</a></li>
    </ul>
</div>

</asp:Label>



<asp:Label ID="ReaderlibMbrRole" runat="server">View resources in the library, and copy resources to your own library</asp:Label>
    <asp:Label ID="contributelibMbrRole" runat="server">Add resources to this library</asp:Label>
    <asp:Label ID="editorlibMbrRole" runat="server">Approve resources in the library, and add additional members to the library</asp:Label>
    <asp:Label ID="adminlibMbrRole" runat="server">Approve resources in the library, and copy resources to your own library</asp:Label>

</asp:Panel>