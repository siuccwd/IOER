<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ioer.ascx.cs" Inherits="IOER.Controls.SearchV7.Themes.ioer" %>

<div id="config" runat="server" visible="false">
	<asp:Literal ID="searchTitle" runat="server">IOER Resource Search</asp:Literal>
	<asp:Literal ID="themeColorMain" runat="server">#3572B8</asp:Literal>
	<asp:Literal ID="themeColorSelected" runat="server">#9984BD</asp:Literal>
	<asp:Literal ID="themeColorHeader" runat="server">#4AA394</asp:Literal>
	<asp:Literal ID="sortField" runat="server">_score</asp:Literal>
	<asp:Literal ID="sortOrder" runat="server">desc</asp:Literal>
	<asp:Literal ID="resultTagSchemas" runat="server">gradeLevel,learningResourceType,mediaType,k12Subject</asp:Literal>
	<asp:Literal ID="siteID" runat="server">1</asp:Literal>
	<asp:Literal ID="startAdvanced" runat="server">0</asp:Literal>
	<asp:Literal ID="hasStandards" runat="server">1</asp:Literal>
	<asp:Literal ID="useResourceUrl" runat="server">0</asp:Literal>
	<asp:Literal ID="doAutoSearch" runat="server">0</asp:Literal>
	<asp:Literal ID="doPreloadNewestSearch" runat="server">1</asp:Literal>
	<asp:Literal ID="showLibColInputs" runat="server">1</asp:Literal>
	<asp:Literal ID="fieldSchemas" runat="server">accessRights,educationalRole,careerCluster,educationalUse,gradeLevel,inLanguage,mediaType,learningResourceType,k12Subject,nrsEducationalFunctioningLevel,accessibilityControl,accessibilityFeature,accessibilityHazard,usageRights</asp:Literal>
	<asp:Literal ID="advancedFieldSchemas" runat="server">accessRights,inLanguage,accessibilityControl,accessibilityFeature,accessibilityHazard,usageRights</asp:Literal>
</div>