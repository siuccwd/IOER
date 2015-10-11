<%@ Page Title="Illinois Open Educational Resources - FAQs" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="FAQs.aspx.cs" Inherits="IOER.Help.FAQs" %>
<%@ Register TagPrefix="uc1" TagName="FaqList" Src="/Controls/FAQs/FaqList.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server"></asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
  <script type="text/javascript">
    $("#aspnetForm").removeAttr("onsubmit");
  </script>
  <style type="text/css">
    .mainContent { transition: padding-left 1s; -webkit-transition: padding-left 1s; }
    @media screen and (min-width: 980px) {
      .mainContent { padding-left: 50px; }
    }
  </style>
  <ajaxToolkit:ToolkitScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
  <div class="mainContent">
      <h1 class="isleH1">Frequently Asked Questions</h1>
<uc1:FaqList ID="FaqList1" runat="server"  
			DefaultCategory="IOER Search"   
      FaqViewType="FaqSheet"
			AllowingQuestions="true"
			DefaultTargetPathways="" 
			MustBeAuthenticated="false" 
      SubcategoryTitle="Subject" 
			/>
  </div>

</asp:Content>
