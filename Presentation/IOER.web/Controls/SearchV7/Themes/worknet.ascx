<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="worknet.ascx.cs" Inherits="IOER.Controls.SearchV7.Themes.worknet" %>

<div id="config" runat="server" visible="false">
	<asp:Literal ID="searchTitle" runat="server"></asp:Literal>
	<asp:Literal ID="themeColorMain" runat="server">#B74900</asp:Literal>
	<asp:Literal ID="themeColorSelected" runat="server">#4D4D4D</asp:Literal>
	<asp:Literal ID="themeColorHeader" runat="server">#4D4D4D</asp:Literal>
	<asp:Literal ID="sortField" runat="server">_score</asp:Literal>
	<asp:Literal ID="sortOrder" runat="server">desc</asp:Literal>
	<asp:Literal ID="resultTagSchemas" runat="server">educationalRole,learningResourceType,mediaType,k12Subject</asp:Literal>
	<asp:Literal ID="siteID" runat="server">3</asp:Literal>
	<asp:Literal ID="startAdvanced" runat="server">0</asp:Literal>
	<asp:Literal ID="hasStandards" runat="server">0</asp:Literal>
	<asp:Literal ID="useResourceUrl" runat="server">1</asp:Literal>
	<asp:Literal ID="doAutoSearch" runat="server">1</asp:Literal>
	<asp:Literal ID="doPreloadNewestSearch" runat="server">1</asp:Literal>
	<asp:Literal ID="showLibColInputs" runat="server">0</asp:Literal>
	<asp:Literal ID="fieldSchemas" runat="server">accessRights,accessibilityControl,accessibilityFeature,accessibilityHazard,educationalRole,careerCluster,careerPlanning,disabilityTopic,employerProgram,jobPreparation,inLanguage,mediaType,learningResourceType,resources,wfePartner,wioaWorks,workplaceSkill,region,subject,layoffAssistance,qualify,ilPathway,workNetArea,guidanceScenario,wdqi,demandDrivenIT,nrsEducationalFunctioningLevel</asp:Literal>
	<asp:Literal ID="advancedFieldSchemas" runat="server">inLanguage</asp:Literal>
</div>