<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive2.Master" AutoEventWireup="true" CodeBehind="PublishHierarchy.aspx.cs" Inherits="IOER.Admin.PublishHierarchy" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    <div style="min-height: 500px">


    
             <br class="clearFloat" />
            <div class="labelColumn">Source Learning List</div>
            <div class="dataColumn">
                <asp:DropDownList ID="ddlList" runat="server"></asp:DropDownList>
                <br /><asp:TextBox ID="txtCurriculumId" runat="server"></asp:TextBox>
            </div>

              <br class="clearFloat" />
            <div class="labelColumn">&nbsp;</div>
            <div class="dataColumn">
                <asp:Button ID="btnStart" runat="server" CssClass="defaultButton" Text="Publish Hierarchy" OnClick="btnStart_Click" CausesValidation="false"></asp:Button>

            </div>
    </div>

</asp:Content>
