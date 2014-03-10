<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CCSS_Import.aspx.cs" Inherits="ILPathways.Admin.CCSS_Import" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    	<h3>Select ...</h3>
	    <br class="clearFloat" />			
      <div class="labelColumn" >Select a Standards Body</div>	
      <div class="dataColumn">   		
		    <asp:DropDownList		 id="lstForm" runat="server" Width="200px" AutoPostBack="false" >
          <asp:ListItem Text="CCSS" Value="1" Selected="True"></asp:ListItem>
          <asp:ListItem Text="ASN" Value="2"></asp:ListItem>
        </asp:DropDownList>
      </div>
  	<!-- --> 
	<br class="clearFloat" />
  <div class="labelColumn requiredField">
    <asp:label id="Label1"  associatedcontrolid="txtStandardsFile" runat="server">File</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  id="txtStandardsFile" Width="300px" runat="server">C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FB.xml</asp:textbox>
    <br />
    <asp:DropDownList		 id="ddlFile" runat="server" Width="80%"  >
          <asp:ListItem Text="Math" Value="C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FB.xml" Selected="True"></asp:ListItem>
          <asp:ListItem Text="English" Value="C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FC.xml"></asp:ListItem>
          <asp:ListItem Text="Social Science" Value="C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FC.xml"></asp:ListItem>
          <asp:ListItem Text="Arts" Value="C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FC.xml"></asp:ListItem>
          <asp:ListItem Text="PhysEd" Value="C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FC.xml"></asp:ListItem>
    </asp:DropDownList>
	</div>
    	<!-- --> 
	<br class="clearFloat" />
  <div class="labelColumn">&nbsp;</div>
  <div class="dataColumn"> 
    <asp:Button ID="submitButton" runat="server" Text="Process" CssClass="defaultButton" OnClick="submitButton_Click" />
	</div>
  <br />
      <asp:Label ID="lblMessage" runat="server"></asp:Label>

    </div>

    <asp:Panel ID="hiddenPanel" runat="server" Visible="false">
    <asp:Literal ID="mathXml" runat="server">C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FB.xml</asp:Literal>
    <asp:Literal ID="englishXml" runat="server">C:\inetpub\wwwroot\VOS_2010\IllinoisPathways\IllinoisPathways\App_Data\D10003FC.xml</asp:Literal>
    </asp:Panel>
    </form>
</body>
</html>
