<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibraryAdmin.ascx.cs" Inherits="ILPathways.Controls.Libraries.LibraryAdmin" %>
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
    .formContainer{ padding-top: 10px; margin-left: 100px;  }
    .libLink { padding: 0 0 5px 5px;}
    .actionsSectionPanel { border: 1px #000 solid; padding: 15px; border-radius: 5px; background-color: #EEE; min-height: 500px;
margin-top: 15px; }
    .actionsSectionPanel ul { margin-left: 20px; }
    .optionsSections { 
        width: 85%;
    }
    .optionLinks a { margin-bottom: 1px; background-color: #3572B8; color: #FFF; display: block; 
                     text-decoration: none; font-size: 1.2em;}

    .optionLinks a:hover, .optionLinks a:focus { margin-bottom: 1px; background-color: #3572B8; color: #FFF; display: block; text-decoration: none; }
    
    .leftCol {
        display: inline-block;
        width: 40%;
        margin-left: 10px;
        text-align: left;
    }
     .rightCol {
        display: inline-block;
        width: 55%;
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
  #processing, #preButtonMessage { display: none; text-align: center; font-weight: bold; font-size: 20px; padding: 10px; }
</style>
<style type="text/css">

@media screen and (max-width: 550px) {
.formContainer{ padding-top: 5px; margin-left: 5px;}
.leftCol {  width: 75%; }
.rightCol {  width: 95% }

}
@media screen and (max-width: 800px) {
.formContainer{ padding-top: 5px; margin-left: 5px;}
.leftCol {  width: 75%; }
.rightCol {  width: 95% }

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
        $(".btnSubmit").hide();}
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
    
        <h2>Library</h2>
        <div style="padding-left: 8px;">
            <asp:DropDownList ID="sourceLibrary" runat="server" OnSelectedIndexChanged="sourceLibrary_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
            <br /><asp:LinkButton ID="refreshLibrariesLink" runat="server" Visible="false" Text="Refresh Libraries List" OnClick="refreshLibrariesLink_Click"></asp:LinkButton>
            <br /><asp:LinkButton ID="showAllLibraries" runat="server" Visible="false" Text="Show All Libraries" OnClick="showAllLibraries_Click"></asp:LinkButton>
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
            <h2 class="isleBox_H2">Library Options</h2>
            <asp:Panel ID="libOptionsPanel" CssClass="optionLinks"  runat="server">
                <asp:LinkButton ID="newLibraryLink" runat="server" Text="New Library" CssClass="libLink" CausesValidation="false" OnClick="newLibraryLink_Click" ></asp:LinkButton>
                <asp:LinkButton ID="editLibraryLink" runat="server" Visible="false" Text="Edit Library" CssClass="libLink" CausesValidation="false" OnClick="editLibraryLink_Click"></asp:LinkButton>
                <asp:LinkButton ID="libMbrsLink" runat="server" Visible="false" Text="Library Members" CssClass="libLink" CausesValidation="false" OnClick="libMbrsLink_Click"></asp:LinkButton>
                <asp:LinkButton ID="libInviteLink" runat="server" Visible="false" Text="Library Invitations" CssClass="libLink" CausesValidation="false" OnClick="libInviteLink_Click"></asp:LinkButton>
                

            </asp:Panel>

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
    </div>
    <div class="rightCol" >
        <div >
        <h2>Current Library: <asp:Label ID="litCurrentLibrary" CssClass="libraryTitle" Text="No Library Selected" runat="server"></asp:Label></h2>
            <div style="display:none;">
        <h3>Current Collection: <asp:Literal ID="litCurrentCollection" Text="" runat="server"></asp:Literal></h3>
                </div>
        </div>
        <div class="actionsSectionPanel">
        <asp:Panel ID="startPanel" runat="server" Style="min-height: 500px; width: 500px; border-radius: 5px;" Visible="true">
            <h2>Getting Started</h2>
            <ul>
                <li>Select a library and then select one of the displayed options</li>
            </ul>
        </asp:Panel>  
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
            <h2 >Members</h2>
        <asp:LinkButton ID="pendingMembers" runat="server" Visible="false" Text="Show Pending Members" OnClick="pendingMembers_Click"></asp:LinkButton>
        <hr />
            <asp:Panel ID="mbrSearchPanel" runat="server" Visible="false">
        <asp:Label ID="Label1" runat="server" >Keyword:</asp:Label>
        <asp:TextBox ID="txtKeyword" runat="server" Width="300px"></asp:TextBox>
        <br /><asp:Button ID="searchLink" runat="server" Text="Search" OnClick="searchLink_Click"></asp:Button>
            </asp:Panel>
<div style="width:100%">
<div class="clear" style="float:right;">	
		<div class="labelColumn" >Page Size</div>	
		<div class="dataColumn" style="text-align:center; font-size: 100%;">     
				<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
		</div>  
	</div>

	<br class="clearFloat" />
        <asp:GridView ID="membersGrid" runat="server" AutoGenerateColumns="False"
            AllowPaging="true" PageSize="50" AllowSorting="false"
            AutoGenerateEditButton="True" datakeynames="Id"
            OnRowCommand="membersGrid_RowCommand"
            OnPageIndexChanging="membersGrid_PageIndexChanging"
            OnSorting="membersGrid_Sorting"
            OnRowEditing="EditRecord"
			OnRowDataBound="membersGrid_RowDataBound"
			OnRowCancelingEdit="CancelRecord" 
			OnRowUpdating="UpdateRecord"
            BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%"
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
                <asp:TemplateField HeaderText="Remove">
                     <ItemTemplate>
                       <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("Id") %>' CommandName="DeleteRow" CausesValidation="false" OnClientClick="return confirm('Are you certain that you want to remove this member?');" runat="server">Remove</asp:LinkButton>
                     </ItemTemplate>
                   </asp:TemplateField>

                <asp:TemplateField HeaderText="Member"  SortExpression="SortName" >
                    <EditItemTemplate>
                        <asp:label ID="gridlblMemberName" runat="server" Text='<%# Bind("MemberName") %>'></asp:label>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:label ID="gridlblMemberName2" runat="server" Text='<%# Bind("MemberName") %>'></asp:label>
                    </ItemTemplate>
                </asp:TemplateField>

               <asp:TemplateField HeaderText="Member Type"  SortExpression="MemberType" >
                    <EditItemTemplate>
                        <asp:DropDownList id="gridDdlTypes" visible="true" runat="server"></asp:DropDownList>
                        <asp:RequiredFieldValidator id="rfvGridTypes" runat="server" ErrorMessage="A Member Type is required!" ControlToValidate="gridDdlTypes"></asp:RequiredFieldValidator>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:label ID="gridlblMemberName" runat="server" Text='<%# Bind("MemberType") %>'></asp:label>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="MemberTypeId" HeaderText="Member Type Id" visible="false"></asp:BoundField>
                <asp:TemplateField HeaderText="Last Updated" ItemStyle-HorizontalAlign="Right"  SortExpression="LastUpdated" >
                    <EditItemTemplate>
                        <asp:label ID="gridlblLastUpdated" runat="server" Text='<%# Bind("LastUpdated", "{0:d}") %>'></asp:label>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:label ID="gridlblLastUpdated2" runat="server" Text='<%# Bind("LastUpdated","{0:d}") %>'></asp:label>
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
            <div style="float:left; margin-top:10px;">
	  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="25" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="false" GeneratePagerInfoSection="true"   />
	</div>	
<br class="clearFloat" />
</div>
        </asp:Panel>

    <!-- ================= invitations==================================== -->
        <asp:Panel ID="invitationsPanel" runat="server" Visible="false">
            <h2 >Invitations</h2>
                <asp:LinkButton ID="newInvitationLink" runat="server" Text="Start a New Invitation"  OnClick="newInvitationLink_Click"></asp:LinkButton>
                <br /><asp:LinkButton ID="showInvitations" runat="server" Visible="false" Text="Show Pending Invitations" OnClick="showInvitations_Click"></asp:LinkButton>
            <br />
                <asp:Panel ID="invitePanelStart" runat="server" visible="true" >
                    <h3>Individual Invitation</h3>
                    <div id="inviteToggle" style="display:inline-block; width:100%;" class="instructionToggle" ><a href="javascript:void(0);" onclick="ShowHideSection('inviteInstructions');" style="display:inline-block; font-size: 150%; ">Instructions</a></div>
                    <div id="inviteInstructions" style="display:none;" >
                        You may invite people to:
                    <ul>
                        <li><strong>View</strong> the selected library (if not already publically available)</li>
                        <li><strong>Contribute</strong> to the selected library (if the library does not already have open contributions (see library settings)</li>
                        <li><strong>Curate</strong> the selected library. A curator can contribute to a library and may also perform administrative functions on the library - that is use this page to manager members, etc.</li>
                        <li><strong>Administer</strong> the selected library. An administrator has the same privileges as a curator, but may also add curators, and other administrators.</li>
                    </ul>
                   
                    <p>You start by entering the email address of the invitee and library role. The system will first search for an existing account for the email. If an account has not yet been created for the invitee, you can choose to create a starting account. This will result in a easier registration process for the invitee. You can also specify if the invitee should be added to your organization. <br />NOTE: this function will soon be available from an organzation administration page and will ease managing library members.</p>
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
                        <a class="toolTipLink" id="A1" title="Library Member Type|Valid member types:<ul><li>Reader - has read access to the library (typically used where library does not have public access).</li><li>Contributor - can add resources the libary (typically used where the library does not have open contributions and user is not part of organization with implicit contribution to organization libraries)</li><li>Curator - a curator may manage the library, including managing members, and approving resources</li><li><Administrator - can manage editors and other administrators</li></ul>"><img
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
                    <p>Regardless, indicate if this user should be added to your organization:</p>
                        </asp:literal>
                    <asp:label ID="userFoundMsg" runat="server" Visible="false"></asp:label>
                    
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
                    <p>Click the <u>Start a New Invitation</u> link above to send another invitation.</p>

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
        </div>
    </div>
    </div>
</asp:Panel>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<!-- control variables -->
<asp:Literal ID="formSecurityName" runat="server" Visible="false">ILPathways.Admin</asp:Literal>
<asp:Literal ID="doingBccOnRegistration" runat="server">yes</asp:Literal>
<asp:Literal ID="txtCurrentLibraryId" runat="server">0</asp:Literal>
<asp:Literal ID="txtCurrentLibraryMemberId" runat="server">0</asp:Literal>
        
<asp:Label ID="inviteSubject" runat="server">Invitation From  {0} to join a library</asp:Label>
<asp:Label ID="inviteEmail" runat="server">
Welcome<br />This is an invitation to access the IOER <strong>{0}</strong> library with a role of  {1}.
<p>You can log in and start contributing to this and other libraries.</p>
</asp:Label>
<asp:Label ID="doRegisterMsg" runat="server">
<p>First you must create a new account using your email address using the following link:</p><p>{0}</p></asp:Label>

<asp:Label ID="visitLibraryMsg" runat="server"><p>You may visit this library by clicking on the following link:<br /><a href="{0}" >Visit: <strong>{1}</strong></a></p></asp:Label>

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
<asp:Literal ID="contributeLink" runat="server" >/Account/Login.aspx?g={0}&nextUrl=/Contribute</asp:Literal>
<asp:Literal ID="contributeLink1" runat="server" >/Account/Login.aspx?g={0}&nextUrl=/Libraries/Library.aspx?id={1}</asp:Literal>
<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?g={0}&a=activate</asp:Literal>
<asp:Literal ID="activateLink1" runat="server" >/Account/Profile.aspx?g={0}&a=activate</asp:Literal>
<asp:Literal ID="registerLink" runat="server" >/Account/Register.aspx?invite={0}</asp:Literal>

<asp:Literal ID="loginToLibraryLink" runat="server" >/Account/Login.aspx?g={0}&nextUrl=/Libraries/Default.aspx</asp:Literal>
<asp:Literal ID="libraryLink" runat="server" >/Libraries/Library.aspx?id={0}</asp:Literal>
<asp:Literal ID="gettingStartedLink" runat="server" >/Help/Guide.aspx</asp:Literal>
<asp:Literal ID="librariesHome" runat="server" >/Libraries/Default.aspx</asp:Literal>
</asp:Panel>