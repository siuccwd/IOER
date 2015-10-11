<%@ Page Title="OBOUT test"  Language="C#"  MasterPageFile="~/Masters/Pathway.Master" AutoEventWireup="true" CodeBehind="OBOUT.aspx.cs" Inherits="IOER.Content.OBOUT" %>
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
<%@ Register assembly="Obout.Ajax.UI" namespace="Obout.Ajax.UI.HTMLEditor.Popups" tagprefix="obout" %>
<%@ Register Assembly="Obout.Ajax.UI" Namespace="Obout.Ajax.UI.FileUpload" TagPrefix="obout" %>

<%@ Register TagPrefix="custom"	Namespace="CustomToolbarButton" Assembly="IOER"  %>
<%@ Register TagPrefix="custom" Namespace="CustomPopups"  %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
<h1>Authoring</h1>
<div class="isleMainSection">
	<div class="labelColumn  requiredField" > 
		<asp:label id="lblDescription"  associatedcontrolid="editor" runat="server">Description</asp:label> 
	</div>
<div class="dataColumn"> 

        <obout:Editor ID="editor" runat="server" Height="500px" Width="700px">
          <EditPanel ID="EditPanel1" FullHtml="false" runat="server"></EditPanel>

            <BottomToolBar ShowDesignButton="true" ShowHtmlTextButton="true" ShowPreviewButton="true" ShowFullScreenButton="true" >
      </BottomToolBar>
          </obout:Editor>


		<div class="clearFloat"></div>

	</div>	
</div>
  
</asp:Content>
