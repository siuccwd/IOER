<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PostQuestion.ascx.cs" Inherits="ILPathways.Controls.FAQs.PostQuestion" %>

<style type="text/css">
.questionPanel table {
  width: 500px;
}
.questionPanel table input, .questionPanel table textarea, .questionPanel table select {
  width: 300px;
}
td.label {
  text-align: right;
  padding-right: 10px;
  width: 100px;
}
</style>
<asp:Panel ID="postPanel" runat="server" CssClass="questionPanel" Visible="true">
<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" ValidationGroup="postQuestion" forecolor="" CssClass="errorMessage" runat="server"></asp:validationsummary>
<!-- --> 
<asp:textbox  id="txtId" Width="20px" Visible="false"  runat="server">0</asp:textbox> 
<asp:Panel ID="catPanel" runat="server" Visible="true">
  <table>
    <tr>
      <td class="label"><asp:label id="lblCategory"  associatedcontrolid="ddlCategory" runat="server">Category</asp:label></td>
      <td><asp:dropdownlist id="ddlCategory" OnSelectedIndexChanged="Category_OnSelectedIndexChanged" runat="server" AutoPostBack="true"></asp:dropdownlist></td>
    </tr>
  </table>
</asp:Panel>				
<asp:Panel ID="subcatPanel" runat="server" Visible="true">		
  <table>
    <tr>
      <td class="label"><asp:label id="lblSubcategory"  associatedcontrolid="ddlSubcategory" runat="server">Subject</asp:label></td>
      <td><asp:dropdownlist id="ddlSubcategory" runat="server"></asp:dropdownlist></td>
    </tr>
  </table>	
</asp:Panel>	
<asp:Panel ID="emailPanel" runat="server" Visible="true">
  <table id="questionTable">
    <tr>
      <td class="label"><asp:label id="Label1"  associatedcontrolid="txtTitle" runat="server">Question</asp:label></td>
      <td><asp:textbox id="txtTitle" TextMode="MultiLine" Rows="4"  MaxLength="300" runat="server"></asp:textbox></td>
    </tr>
    <tr>
      <td class="label"><asp:label id="Label2"  associatedcontrolid="txtEmail" runat="server">Email</asp:label></td>
      <td><asp:textbox  id="txtEmail" MaxLength="200" runat="server"></asp:textbox></td>
    </tr>
    <tr>
      <td class="label"><asp:label id="lblConfirmEmail" associatedcontrolid="txtConfirmEmail" runat="server">Confirm Email</asp:label></td>
      <td><asp:textbox id="txtConfirmEmail" MaxLength="200" runat="server"></asp:textbox></td>
    </tr>
    <tr><td colspan="2"><asp:button id="btnSave" runat="server" CssClass="defaultButton" width="100px" CommandName="Update" OnCommand="FormButton_Click" Text="Submit" ValidationGroup="postQuestion" /></td></tr>
  </table>
</asp:Panel>									
<!-- --> 
	<input id="firstName" type="hidden" name="firstName" runat="server" />
	<input id="lastName" type="hidden" name="lastName" runat="server" />							

<div>
<!-- validators -->
<asp:requiredfieldvalidator id="rfvTitle" Display="None" ControlToValidate="txtTitle" ErrorMessage="Question is a required field" ValidationGroup="postQuestion" runat="server"></asp:requiredfieldvalidator>
<asp:requiredfieldvalidator id="rfvEmail" Display="None" ControlToValidate="txtEmail" ErrorMessage="Email is a required field" ValidationGroup="postQuestion" runat="server"></asp:requiredfieldvalidator>

<asp:regularexpressionvalidator id="revEmail" runat="server" enableclientscript="False" display="None" controltovalidate="txtEmail" errormessage="RegularExpressionValidator" ValidationGroup="postQuestion" validationexpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:regularexpressionvalidator>	

<asp:requiredfieldvalidator id="rfvConfirmEmail" Display="None" ControlToValidate="txtConfirmEmail" ErrorMessage="Confirmation Email is a required field" ValidationGroup="postQuestion" runat="server"></asp:requiredfieldvalidator>
<asp:regularexpressionvalidator id="revConfirmEmail" runat="server" enableclientscript="False" display="None" controltovalidate="txtConfirmEmail" errormessage="RegularExpressionValidator" ValidationGroup="postQuestion" validationexpression="\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:regularexpressionvalidator>
<asp:comparevalidator id="cvEmail" runat="server" enableclientscript="False" display="None" controltovalidate="txtConfirmEmail" controltocompare="txtEmail" ValidationGroup="postQuestion" errormessage="The confirmation email address does not match the email address."></asp:comparevalidator>	

</div>								
</asp:Panel>	
<asp:Panel ID="hiddenPanel"	runat="server" Visible="false">
<asp:Literal ID="defaultCategory" runat="server"></asp:Literal>
<asp:Literal ID="defaultTargetPathways" runat="server">3</asp:Literal>
<asp:Literal ID="showingSubcategory" runat="server">no</asp:Literal>
<asp:Literal ID="emailAppKey" runat="server">ioerContactLogin</asp:Literal>
<asp:Literal ID="subcategoryTitle" runat="server">Subject</asp:Literal>
<asp:Literal ID="doingBccInfoOnConfirmEmail" runat="server">yes</asp:Literal>
<asp:Literal ID="doingBccAdminOnConfirmEmail" runat="server">yes</asp:Literal>
<asp:Literal ID="doingFullDebug" runat="server">no</asp:Literal>
<!-- if user not authenticate, is email confirmation required-->
<asp:Literal ID="mustConfirmQuestion" runat="server">yes</asp:Literal>

<asp:Literal ID="guestUserConfirmMsg" runat="server">Thank you for submitting a question. An email will be sent to you in order to confirm your email address. <br/>Click the link in the email to submit your question. </asp:Literal>
<asp:Literal ID="authUserConfirmMsg" runat="server">Your question has been submitted. An email will be sent to confirm your request. <br/>If you have subscribed to the mailing list, you will be notified when an answer is posted. </asp:Literal>
<asp:Literal ID="qConfirmationMessage" runat="server">Thank you for confirming your question. Your question has been submitted. <br/>If you have subscribed to the mailing list, you will be notified when an answer is posted. </asp:Literal>
</asp:Panel>	

<asp:panel ID="Panel1" runat="server" Visible="false">
<!-- TODO: when an additional parm is added to the nextUrl, it may get stripped off. Check and accomodate! -->
<asp:Literal ID="autoLoginTemplate" runat="server"><a href="[LoginUrl]" >Please click here to log into Illinois workNet and address the posted question.</a></asp:Literal>
<asp:Literal ID="faqLink" runat="server">/vos_portal/advisors/en/DceoStaff/FaqMgmt/?id={0}</asp:Literal>
<asp:Literal ID="subjectTemplate" runat="server"><br /><strong>Subject:</strong>&nbsp;{0}</asp:Literal>
<!-- not sure if the email will go to info@, no sense going to dceo if not confirmed?-->
<asp:Literal ID="dceoEmailSubject" runat="server">{0} Question</asp:Literal>
<asp:Literal ID="dceoEmailTemplate" runat="server">
<p>The following question related to the {4} was submitted:</p> 
From: {0}
<br />
{1}
<strong>Question:</strong><br />{2}
<p>{3}</p>
</asp:Literal>

<asp:Literal ID="faqCategoryTitle" runat="server">Workforce Innovation Fund Partnerships</asp:Literal>
<asp:Literal ID="confirmLink" runat="server">/vos_portal/advisors/en/Resources/General+Partner+Information/WIF_Partnership/FAQ/default.htm?id={0}</asp:Literal>
<asp:Literal ID="userEmailSubject" runat="server">{0} question confirmation</asp:Literal>
<asp:Literal ID="userEmailTemplate" runat="server">
<div>Thank you for submitting a question related to the {3}:
<br />{0}
<br /><strong>Question:</strong> {1}</div> 
<p>In order to validate your email and to ensure someone else is not using your email address, please click on the following link to confirm the validity of your request.</p>
<a href="{2}" >Please click here to confirm your identity and submit the question.</a> <p>If you have subscribed to the mailing list, you will be notified when an answer is posted.</p>
</asp:Literal>
</asp:Panel>	

<asp:panel ID="sqlPanel1" runat="server" Visible="false">
<asp:Literal ID="sqlSelectDistinctPaths" runat="server" Visible="false">
SELECT Distinct [PathwayId] As Id, [SitePathName] As Title, [name] As DisplayName
FROM [dbo].[Faq.FaqPathway] fp
inner join [dbo].[AppPathway] ap on fp.PathwayId = ap.Id Order by 2</asp:Literal>

<asp:Literal ID="sqlSelectSearchCategories" runat="server" Visible="false">SELECT id, [Category]  FROM [dbo].[FAQ.Category] Order by Category</asp:Literal>
<asp:Literal ID="sqlSelectSubcategoriesById" runat="server" Visible="false">SELECT [Id] ,[SubCategory] As Title FROM [dbo].[FAQ.SubCategory] where [CategoryId] = {0} order by 2 </asp:Literal>

<asp:Literal ID="sqlSelectSubcategoriesByCategory" runat="server" Visible="false">
SELECT base.[id] ,[SubCategory] As Title 
  FROM [dbo].[FAQ.SubCategory] base
Inner Join [dbo].[FAQ.Category] cat on base.CategoryId = cat.Id
where cat.[Category] = '{0}'
Order by 2  </asp:Literal>

</asp:panel>						 
