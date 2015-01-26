<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ContactMgmt.ascx.cs" Inherits="ILPathways.Controls.OrgMgmt.ContactMgmt" %>

<%@ Register Src="~/Controls/OrgMgmt/Import.ascx" TagPrefix="uc1" TagName="Import" %>
<h2 style="text-align: center;"><asp:Literal ID="litOrgTitle" runat="server"></asp:Literal></h2>

<asp:Panel ID="memberPanel" CssClass="memberPanel" Visible="false" runat="server">
    <asp:Button ID="btnImportMembers" runat="server" CausesValidation="false" Text="Import People" OnClick="btnImportMembers_Click" CssClass="defaultButton" />
    <asp:Button ID="btnAddMember" runat="server" CausesValidation="false" Text="Add New User" OnClick="btnAddMember_Click" CssClass="defaultButton" />
    <br />
    <div class="labelColumn"><asp:Label ID="Label6" AssociatedControlID="ddlFilterMemberType" runat="server">Member Type </asp:Label></div>
    <div class="dataColumn">
        <asp:DropDownList ID="ddlFilterMemberType" runat="server"></asp:DropDownList>
    </div>
        <div class="labelColumn" >
            <asp:Label ID="Label1" runat="server">Keyword:</asp:Label></div>
        <div class="dataColumn">
            <asp:TextBox ID="txtMemberKeyword" runat="server" ></asp:TextBox>
        </div>
        <br />
        <div class="labelColumn">&nbsp;</div>
        <div class="dataColumn">
            <asp:Button ID="searchLink" runat="server" Text="Search" CausesValidation="false" OnClick="searchLink_Click" CssClass="defaultButton" ></asp:Button>
        </div>

<div style="width: 100%">
        <div class="clear" style="float: right;margin-top: 15px;">
            <div class="labelColumn" style="vertical-align: middle;">Page Size</div>
            <div class="dataColumn" style="text-align: center; font-size: 100%;">
                <asp:DropDownList ID="ddlMembersPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="MembersPageSizeList_OnSelectedIndexChanged"></asp:DropDownList>
            </div>
        </div>

        <br class="clearFloat" />
        <asp:GridView ID="membersGrid" runat="server" AutoGenerateColumns="False"
            AllowPaging="true" PageSize="50" AllowSorting="false"
            AutoGenerateEditButton="false" DataKeyNames="Id"
            OnRowCommand="membersGrid_RowCommand"
            OnPageIndexChanging="membersGrid_PageIndexChanging"
            OnSorting="membersGrid_Sorting"
            BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="Horizontal" Width="100%"
            CaptionAlign="Top"
            UseAccessibleHeader="true">
            <HeaderStyle CssClass="gridResultsHeader" HorizontalAlign="Left" />
            <Columns>
                <asp:TemplateField Visible="true" HeaderText="Select">
                    <ItemTemplate>
                        <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("UserId") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
	Select</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Remove"  ItemStyle-CssClass="removeCol">
                    <ItemTemplate>
                        <asp:LinkButton ID="deleteRowButton" CommandArgument='<%# Eval("Id") %>' CommandName="DeleteRow" CausesValidation="false" OnClientClick="return confirm('Are you certain that you want to remove this member?');" runat="server">Remove</asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="First Name" SortExpression="FirstName">
                    <ItemTemplate>
                        <asp:Label ID="gridlblFirstName" runat="server" Text='<%# Bind("FirstName") %>'></asp:Label>

                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Last Name" SortExpression="LastName">
                    <ItemTemplate>
                        <asp:Label ID="gridlblLastName" runat="server" Text='<%# Bind("LastName") %>'></asp:Label>

                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Org. Member Type"  >
                    <EditItemTemplate>
                        <asp:DropDownList ID="gridDdlOrgMbrType" Visible="false" runat="server"></asp:DropDownList>
                                   
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="gridlblOrgMbrType" runat="server" Text='<%# Bind("OrgMemberType") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>

                           
                <asp:BoundField DataField="OrgMemberTypeId" HeaderText="Member Type Id" Visible="false"></asp:BoundField>

                <asp:TemplateField HeaderText="Last Updated" SortExpression="LastUpdated">
                    <EditItemTemplate>
                        <asp:Label ID="gridlblLastUpdated" runat="server" Text='<%# Bind("LastUpdated", "{0:d}") %>'></asp:Label>
                    </EditItemTemplate>
                    <ItemTemplate>
                        <asp:Label ID="gridlblLastUpdated2" runat="server" Text='<%# Bind("LastUpdated","{0:d}") %>'></asp:Label>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Created" Visible="true" HeaderText="Added" SortExpression="Created" DataFormatString="{0:d}"></asp:BoundField>

            </Columns>
            <PagerSettings Visible="false"
                Mode="NumericFirstLast"
                FirstPageText="First"
                LastPageText="Last"
                PageButtonCount="5"
                Position="TopAndBottom" />

        </asp:GridView>
        <div style="float: left; margin-top: 10px;">
            <wcl:PagerV2_8 ID="membersPager2" runat="server" Visible="false" OnCommand="memberPager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next" LastClause="Last Page" PageSize="25" CompactModePageCount="10" NormalModePageCount="10" GenerateGoToSection="false" GeneratePagerInfoSection="true" />
        </div>
        <br class="clearFloat" />
    </div>

</asp:Panel>
            
<asp:Panel ID="addMemberPanel" CssClass="memberPanel" Visible="false" runat="server">
        <asp:Button ID="btnCloseAddPanel" CssClass="defaultButton" runat="server" CausesValidation="false" Text="Back to Search" OnClick="btnCloseAddPanel_Click" />
                
    <h3>Account:</h3>
    <div  class="labelColumn">Userid</div>
    <div  class="dataColumn"><asp:Label ID="lblUserId" runat="server" >0</asp:Label></div>
    
    <div class="clearFloat"></div>
        <div class="labelColumn"><asp:Label ID="Label4" AssociatedControlID="txtFirstName"  runat="server" >First Name</asp:Label></div>
        <div class="dataColumn"><asp:TextBox ID="txtFirstName" runat="server" CssClass="longTxt"></asp:TextBox> </div>
        <div class="clearFloat"></div>
        <div class="labelColumn"><asp:Label ID="Label5" AssociatedControlID="txtLastName"  runat="server" >Last Name</asp:Label></div>
        <div class="dataColumn"><asp:TextBox ID="txtLastName" runat="server" CssClass="longTxt"></asp:TextBox> </div>
        <div class="clearFloat"></div>
        <div class="labelColumn"><asp:Label ID="Label2" AssociatedControlID="txtEmail"  runat="server" >Email</asp:Label></div>
        <div class="dataColumn"><asp:TextBox ID="txtEmail" runat="server" CssClass="longTxt"></asp:TextBox> </div>
        <div class="clearFloat"></div>
        <div class="labelColumn"><asp:Label ID="Label7" AssociatedControlID="txtConfirmEmail"  runat="server" >Confirm Email</asp:Label></div>
        <div class="dataColumn"><asp:TextBox ID="txtConfirmEmail" runat="server" CssClass="longTxt"></asp:TextBox> </div>
    <div class="clearFloat"></div>
        <div class="labelColumn"><asp:Label ID="Label3" AssociatedControlID="rblMemberType"  runat="server" >Member Type</asp:Label></div>
        <div class="dataColumn">
            <asp:RadioButtonList ID="rblMemberType" runat="server">
                <asp:ListItem Text="Add as Administration"            Value="1"></asp:ListItem>
                <asp:ListItem Text="Add as Staff member/Employee" Selected="True"     Value="2"></asp:ListItem>
                <asp:ListItem Text="Add as Student"                   Value="3"></asp:ListItem>
                <asp:ListItem Text="Add as Contrator/External"        Value="4"></asp:ListItem>
            </asp:RadioButtonList>
        </div>

        <div class="clearFloat"></div>
                <!-- User should have some min org admin role to assign these.
                    - also should probably not be able to assign a role above their own? 
                    -->
                <div class="labelTop">You may optionally assign organization roles for this person. If applicable, select one or more roles to be assigned to this person.</div>
                <div class="labelColumn">&nbsp;</div>
                <div class="dataColumn">
                    <asp:CheckBoxList ID="cblOrgRoles" runat="server"></asp:CheckBoxList>
                </div>
                <br />
                      
        <div class="labelColumn">&nbsp;</div>
        <div class="dataColumn" style="margin-top:10px;"><asp:Button ID="btnAddUser" runat="server" Text="Save" OnClick="btnAddUser_Click" CssClass="defaultButton" ></asp:Button> 
    <asp:Button ID="btnAddAdnother" runat="server" Text="Add New User" OnClick="btnAddMember_Click" CssClass="defaultButton" ></asp:Button> </div>
                    
</asp:Panel>

<asp:Panel ID="importPanel" Visible="false" runat="server">
        <asp:Button ID="btnCloseImport" runat="server" CausesValidation="false" Text="Close Import" OnClick="btnCloseImport_Click" CssClass="defaultButton" />
    <uc1:Import runat="server" ID="memberImport" />
</asp:Panel>


<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:Literal ID="currentOrgId" runat="server" Visible="false">0</asp:Literal>
<asp:Literal ID="doingBccOnRegistration" runat="server" Visible="false">yes</asp:Literal>

<asp:Label ID="userAddConfirmation" runat="server">The user account for {0} was created and an email was sent with a link to activate the account.</asp:Label>
<asp:Label ID="noticeSubject" runat="server">{0} added you to an "Illinois Open Educational Resources" organization</asp:Label>
<asp:Label ID="noticeEmail" runat="server">
Welcome<br />An IOER account has been created for you:
    <br />Organization: <strong>{0}</strong> 
    <br />Member Type: <strong>{1}</strong>.
<p>Use the link below to activate your account. You will then have access to the latter organization.</p>
    <p>Upon activation a personal library will be created where you can store the resources that you find interesting.</p>
</asp:Label>

<asp:Literal ID="activateLink" runat="server" >/Account/Login.aspx?pg={0}&a=activate&nextUrl=/Account/Profile.aspx</asp:Literal>
<asp:Label ID="acctCreatedMsg" runat="server">
<p>An account has been created for you:</p>
User Name (email): {0}
<br />Password (temporary): {1} 
<p>Click on the following link to confirm your account and update your profile:</p>
    <a href="{2}">Confirm your IOER registration</a>
<p>Use the Profile page to:</p>
    <ul>
        <li>Set your password</li>
        <li>Upload a personal avatar</li>
        <li>Assign other optional properties</li>
    </ul>
</asp:Label>
</asp:Panel>
