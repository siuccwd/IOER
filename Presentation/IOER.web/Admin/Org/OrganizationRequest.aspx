<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="OrganizationRequest.aspx.cs" Inherits="ILPathways.Admin.Org.OrganizationRequest" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">


<asp:Panel ID="keysPanel" runat="server" Visible="false">
<!-- --> 
<div class="clear"></div>
  <div class="labelColumn " > 
    <asp:label id="lblId"  associatedcontrolid="txtId" runat="server">Id</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="txtId" runat="server"></asp:label>
  </div>
<!-- --> 
<div class="clear"></div>
  <div class="labelColumn " > 
    <asp:label id="lblUserId"  associatedcontrolid="txtUserId" runat="server">Request by</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:label id="lblUser" runat="server"></asp:label> 
    &nbsp;(<asp:label id="txtUserId" runat="server"></asp:label>)
  </div>
<!-- --> 
<div class="clear"></div>
  <div class="labelColumn " > 
    <asp:label id="lblOrgList"  associatedcontrolid="ddlOrgId" runat="server">OrgId</asp:label> 
  </div>
  <div class="dataColumn"> 
      <asp:label id="lblOrgId"   runat="server"></asp:label>
    <asp:dropdownlist id="ddlOrgId" runat="server"></asp:dropdownlist>
  </div>
</asp:Panel>
<!-- --> 
<div class="clear"></div>
  <div class="labelColumn " > 
    <asp:label id="lblOrganzationName"  associatedcontrolid="txtOrganizationName" runat="server">Organization Name</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="100"  id="txtOrganizationName" runat="server"></asp:textbox>
  </div>
<!-- --> 
<div class="clear"></div>
  <div class="labelColumn " > 
    <asp:label id="lblAction"  associatedcontrolid="txtAction" runat="server">Action</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  maxLength="100"  id="txtAction" runat="server"></asp:textbox>
  </div>



    <asp:RadioButtonList ID="rblAddToOrg" runat="server">
        <asp:ListItem Text="Do NOT add To My Organization" Value="0" Selected="True"></asp:ListItem>
        <asp:ListItem Text="Add as Administration"            Value="1"></asp:ListItem>
        <asp:ListItem Text="Add as Staff member/Employee"     Value="2"></asp:ListItem>
        <asp:ListItem Text="Add as Student"                   Value="3"></asp:ListItem>
        <asp:ListItem Text="Add as Contrator/External"        Value="4"></asp:ListItem>
    </asp:RadioButtonList>
    <br />

    <asp:checkboxlist ID="cblOrgMbrRole" runat="server">
        <asp:ListItem Text="Administrator"        Value="1"></asp:ListItem>
        <asp:ListItem Text="Content Approval"     Value="2"></asp:ListItem>
        <asp:ListItem Text="Library Administrator" Value="3"></asp:ListItem>
        <asp:ListItem Text="Account Administrator" Value="4"></asp:ListItem>
    </asp:checkboxlist>
    <br />


<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
<asp:label ID="confirmationEmail" runat="server" >
Dear {0}
<h2>Welcome to IOER</h2>
<p>We have added your organization, and designated you as the administrator.
You, through your organization now have publish privileges. As well you can set up an organization level library and invite other members from your organization to join the library (and organization).</p>
</asp:label>
</asp:Panel>
</asp:Content>
