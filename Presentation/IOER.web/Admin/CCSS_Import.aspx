<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CCSS_Import.aspx.cs" Inherits="IOER.Admin.CCSS_Import" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    	<h3>Select Standards Body</h3>
	    <br class="clearFloat" />			
      <div class="labelColumn" >Select a Standards Body</div>	
      <div class="dataColumn">   		
		    <asp:DropDownList		 id="lstForm" runat="server" Width="200px" AutoPostBack="false" >
                <asp:ListItem Text="Select the Standard Body" Value="0" Selected="True"></asp:ListItem>
              <asp:ListItem Text="CCSS" Value="1" ></asp:ListItem>
              <asp:ListItem Text="CDC" Value="2"></asp:ListItem>
              <asp:ListItem Text="Illinois Learning Standards (ILS)" Value="3"></asp:ListItem>
            </asp:DropDownList>
      </div>
  	<!-- --> 
	<br class="clearFloat" />
  <div class="labelColumn requiredField">
    <asp:label id="Label1"  associatedcontrolid="txtStandardsFile" runat="server">File<br />absolute path or relative to app_Data (ex: /App_Data/D2589605.xml)</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:textbox  id="txtStandardsFile" Width="80%" runat="server">E:\Projects\IOER2\Presentation\IOER.web\App_Data\D2589605.xml</asp:textbox>
    <br />
    <asp:DropDownList		 id="ddlStandardsFile" runat="server" Width="300px"  >
        <asp:ListItem Text="Select the Standards files" Value="0" Selected="True"></asp:ListItem>
          <asp:ListItem Text="Math"             Value="/App_Data/D10003FB.xml" ></asp:ListItem>
          <asp:ListItem Text="English"          Value="/App_Data/D10003FC.xml"></asp:ListItem>
          <asp:ListItem Text="Social Science"   Value="/App_Data/D10003FC.xml"></asp:ListItem>
          <asp:ListItem Text="Arts"             Value="/App_Data/D10003FC.xml"></asp:ListItem>
          <asp:ListItem Text="PhysEd"           Value="/App_Data/D10003FC.xml"></asp:ListItem>
          <asp:ListItem Text="Health - CDC"     Value="/App_Data/D2589605.xml"></asp:ListItem>
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
    <asp:Literal ID="healthXml" runat="server">~\App_Data\D2589605.xml</asp:Literal>
    </asp:Panel>
    </form>
</body>
</html>
