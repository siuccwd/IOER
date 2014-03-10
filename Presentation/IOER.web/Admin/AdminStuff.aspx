<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="AdminStuff.aspx.cs" Inherits="ILPathways.Admin.AdminStuff" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="../Styles/common.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

    <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
    <h2>Copy Collection</h2>
    <asp:Panel ID="formContainer" Style="margin-left: 150px;" runat="server" >
    <asp:UpdatePanel ID="formUpdatePanel" runat="server">
        <ContentTemplate>
            <br class="clearFloat" />
            <div class="labelColumn">Source Library</div>
            <div class="dataColumn">
                <asp:DropDownList ID="sourceLibrary" runat="server" OnSelectedIndexChanged="sourceLibrary_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                <br /><asp:TextBox ID="sourceLibraryId" runat="server"></asp:TextBox>
            </div>
            <div class="dataColumn">
                <asp:DropDownList ID="ddlPersons" runat="server" OnSelectedIndexChanged="ddlPersons_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                <br /><asp:TextBox ID="personId" runat="server"></asp:TextBox>
            </div>


             <br class="clearFloat" />
            <div class="labelColumn">Source Collection</div>
            <div class="dataColumn">
                <asp:DropDownList ID="sourceCollection" runat="server" AutoPostBack="true"  OnSelectedIndexChanged="sourceCollection_SelectedIndexChanged"></asp:DropDownList>
                <br /><asp:TextBox ID="collectionId" runat="server"></asp:TextBox>
            </div>

            <br class="clearFloat" />
            <div class="labelColumn">Target Library Id</div>
            <div class="dataColumn">
                <asp:DropDownList ID="targetLibrary" runat="server" OnSelectedIndexChanged="targetLibrary_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                <br /><asp:TextBox ID="libraryId" runat="server"></asp:TextBox>
            </div>
            
        </ContentTemplate>

    </asp:UpdatePanel>

    <!-- -->
            <br class="clearFloat" />
            <div class="labelColumn">&nbsp;</div>
            <div class="dataColumn">
                <asp:Button ID="btnCopy" runat="server" CssClass="defaultButton" Text="Copy Collection" OnClick="btnCopy_Click" CausesValidation="false"></asp:Button>

            </div>

     <!-- -->
            <br class="clearFloat" />
            <div class="labelColumn">&nbsp;</div>
            <div class="dataColumn">
                <asp:Button ID="btnDeleteLibrary" runat="server" CssClass="defaultButton" Text="Delete Library" OnClick="btnDeleteLibrary_Click" CausesValidation="false"></asp:Button>

            </div>
            <br class="clearFloat" />
        <div class="labelColumn">Library Member type for:
            <br /><asp:label ID="lblPerson" runat="server"></asp:label>
        </div>
        <div class="dataColumn">
                <asp:DropDownList ID="ddlMemberType" runat="server">
                    <asp:ListItem Text="Reader"         Value="1" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Contributor"    Value="2" ></asp:ListItem>
                    <asp:ListItem Text="Curator"        Value="3" ></asp:ListItem>
                    <asp:ListItem Text="Administrator"  Value="4" ></asp:ListItem>
                </asp:DropDownList>
            </div>

        <div class="dataColumn">
                <asp:Button ID="btnAddLibraryMbr" runat="server" CssClass="defaultButton" Enabled="true" Text="Add Contributor" OnClick="btnAddLibraryMbr_Click" CausesValidation="false"></asp:Button>
            </div>


            <asp:Label ID="lblMessage" runat="server"></asp:Label>
</asp:Panel>

<asp:Panel ID="Panel1" runat="server" Visible="false">
<asp:Literal ID="formSecurityName" runat="server" Visible="false">ILPathways.Admin.QueryMgmt</asp:Literal>
</asp:Panel>	

</asp:Content>
