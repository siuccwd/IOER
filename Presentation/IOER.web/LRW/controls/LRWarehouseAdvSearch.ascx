<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LRWarehouseAdvSearch.ascx.cs" Inherits="IOER.LRW.controls.LRWarehouseAdvSearch" %>

<style type="text/css">
.narrowingSection h3 {
	background: url(/images/icons/open.png) no-repeat 0 11px;
	padding: 10px 0 0 25px;
	cursor: pointer;
}
.narrowingSection h3.close {
	background-image: url(/images/icons/close.png);
}
.narrowingValuesDiv {
	margin-left: 5px;	 
}

</style>

<script type="text/javascript">
<!--
  var $lrs2 = jQuery.noConflict();
  $lrs2(document).ready(function () {
    $('.narrowingValuesDiv').hide();
    $('.narrowingSection h3').toggle(
			function () {
			  $(this).next('.narrowingValuesDiv').slideDown();
			  $(this).addClass('close');
			},
			function () {
			  $(this).next('.narrowingValuesDiv').slideUp();
			  $(this).removeClass('close');
			}
		); // end toggle

			$('div.slide').next().css('display', 'none').end().click(function () {
			  $(this).next().toggle('fast');
			});
  }); // end ready
//-->	
</script>


<script type="text/javascript">
	<!--
  function ResetForm() {

    //
    //BodyContent_LRWarehouseSearch1_formContainer_tabSummary_txtKeyword
    document.getElementById('BodyContent_LRWarehouseSearch1_formContainer_tabSummary_txtKeyword').value = "";

    //alert('Reset cbxPathways');
    CheckBoxListSelect('<%=cbxlCluster.ClientID %>', false)
    CheckBoxListSelect('<%=cbxGradeLevel1.ClientID %>', false)
    CheckBoxListSelect('<%=cbxlResType2.ClientID %>', false)
    CheckBoxListSelect('<%=cbxlFormatType.ClientID %>', false)

    return false;
  } //end function

  function CheckBoxListSelect(cbControl, state) {
    // alert("CheckBoxListSelect for " + cbControl);
    var chkBoxList = document.getElementById(cbControl);
    var chkBoxCount = chkBoxList.getElementsByTagName("input");
    for (var i = 0; i < chkBoxCount.length; i++) {
      chkBoxCount[i].checked = state;
    }

    return false;
  }  
//-->
</script>	
<asp:Panel ID="pnlSearch" runat="server" CssClass="clearFloat">
<h2>Learning Registry Advanced Search (Pre-Alpha)</h2>
			<asp:validationsummary id="vsErrorSummary" HeaderText="Errors on page" 
		CssClass="errorMessage" runat="server"></asp:validationsummary>


<div class="slide"><img alt="Instructions" src="/images/icons/infoBubble.gif" />What is this?</div>
<div id="divHelpDescription" class="infoMessage" style="display: none; width:90%;padding-left:5px;">
<p>The search on this page is a pre-alpha release that is using the Learning Registry Index (LRI).  The search will be undergoing changes to improve the search specifically for Career Cluster resources and to allow for rating, evaluating, tagging, commenting, and aligning to standards.</p>      
<p>
The LRI is a joint effort of the Department of Education and the Department of Defense, with support of the White House and numerous federal agencies, non-profit organizations, international organizations and private companies.  We suggest reading their brief, new paper, <a href="http://www.learningregistry.org/news/newwhitepaperonthelearningregistryforstatedecisionmakersandstrategists" target="_blank">“Building a Network of Resource-Sharing States: An Overview of the Learning Registry for State Decision Makers and Strategists.”</a>.
Additional information about LRI and research done to begin <a href="http://www.illinoisworknet.com/vos_portal/STEM/en/Resources/Research/" target="_blank">developing this search is available</a>.
If you have any questions or comments please send them to <a href="mailto:info@siuccwd.com">info@siuccwd.com</a>.
</p>
</div>

<!-- Cluster -->
	<!--<div class="labelColumn" >Select a Career Cluster</div>	-->	
	<br class="clearFloat" />			  

	<asp:Panel ID="clusterPanel" runat="server">
	<br class="clearFloat" />
	<!-- Cluster -->
		<div class="labelColumn" >Cluster</div>
	<div class="dataColumn">   		
	<asp:CheckBoxList ID="cbxlCluster" runat="server" Width="400px"  RepeatColumns="1" 
			AccessiblePrefix="" Visible="False"></asp:CheckBoxList>		
	</div>
	</asp:Panel>  

 <asp:Panel ID="Panel1" runat="server">
		<br class="clearFloat" />	
			<div class="labelColumn" >Language</div>
		<div class="dataColumn">   
		<asp:dropdownlist ID="ddlLanguages" runat="server"></asp:dropdownlist>	

		</div>  
	</asp:Panel>

<asp:Panel ID="Panel2" runat="server">
		<br class="clearFloat" />	
			<div class="labelColumn" >Grade Level</div>
		<div class="dataColumn">   
		<asp:CheckBoxList ID="cbxGradeLevel1" runat="server"  RepeatColumns="1" 
				AccessiblePrefix="" >
					<asp:ListItem Text="Primary" Value="Elementary School"></asp:ListItem>
			<asp:ListItem Text="Middle School"></asp:ListItem>
			<asp:ListItem Text="Secondary" Value="high school"></asp:ListItem>
			<asp:ListItem Text="Post-secondary" Value="Higher Education"></asp:ListItem>
		
		</asp:CheckBoxList>		

		</div>  
	</asp:Panel>
  <!---------->
<div class="narrowingSection"  >
	<!-- Resource Types -->
		<h3>Resource Types</h3>
		<div class="narrowingValuesDiv">
		<asp:CheckBoxList ID="cbxlResType2" runat="server" RepeatColumns="2" AccessiblePrefix=""
			Visible="true">
		</asp:CheckBoxList>
	</div>

	<!-- Resource format -->
		<h3>Resource Types</h3>
		<div class="narrowingValuesDiv">
		<asp:CheckBoxList ID="cbxlFormatType" runat="server" RepeatColumns="2" AccessiblePrefix=""
			Visible="true">
		</asp:CheckBoxList>
	</div>
</div>
<asp:Panel ID="showTotalsPanel1" runat="server" Visible="false">   
	<!-- --> 
<br class="clearFloat" />
	<div class="labelColumn " > 
		<asp:label id="lblShowTotals"  associatedcontrolid="rblShowTotals" runat="server">Only Show Available Narrowing Options (warning</asp:label> 
	</div>
	<div class="dataColumn"> 
		<asp:RadioButtonList id="rblShowTotals" autopostback="false" causesvalidation="false"   runat="server" tooltip="True: User is active" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes" Selected="True"  ></asp:ListItem>    
	<asp:ListItem Text="No" 	value="No"></asp:ListItem>
</asp:RadioButtonList>
	</div>
		</asp:Panel> 
<asp:Panel ID="ftPanel" runat="server" Visible="false">   
	<!-- --> 
<br class="clearFloat" />
	<div class="labelColumn " > 
		<asp:label id="Label3"  associatedcontrolid="rblShowTotals" runat="server">Use Full Text Catalog</asp:label> 
	</div>
	<div class="dataColumn"> 
		<asp:RadioButtonList id="ftOptionList" autopostback="false" causesvalidation="false"   runat="server" tooltip="True: User is active" RepeatDirection="Horizontal">
	<asp:ListItem Text="Yes"  value="Yes"></asp:ListItem>    
	<asp:ListItem Text="No" 	value="No" Selected="True"  ></asp:ListItem>
</asp:RadioButtonList>
	</div>
		</asp:Panel> 
<asp:Panel ID="publisherPanel" Visible="false" runat="server" >
	<!-- --> 
  <br class="clearFloat" />
	<div class="labelColumn" >Publisher:</div>
	<div class="dataColumn">   
						<asp:TextBox ID="txtPublisher" runat="server" Width="350px"></asp:TextBox>
  </div>
</asp:Panel>
   
	<!-- --> 
  <br class="clearFloat" />
	<div class="labelColumn" >Keyword:</div>
	<div class="dataColumn">   
						<asp:TextBox ID="txtKeyword" runat="server" Width="350px"></asp:TextBox><br />
  </div>
	 <ajaxToolkit:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender3" runat="server"
				TargetControlID="txtKeyword"
				WatermarkText="Type a word, or a partial phrase (ex. comp)"
				WatermarkCssClass="watermarked" Enabled="True" /> 	        


	<br class="clearFloat" />	
	<p class="labelColumn">&nbsp;</p>
	<div class="dataColumn">			
	<asp:button id="Button1" runat="server" CssClass="defaultButton" width="100px" CommandName="Search" 
			OnCommand="FormButton_Click" Text="Search" causesvalidation="False"></asp:button>

	<asp:Button ID="btnExport" runat="server" CausesValidation="False" CommandName="Export"
		CssClass="defaultButton" OnCommand="FormButton_Click" Text="Export" Width="100px"
		Visible="False" />
    <button class="defaultButton" onclick="ResetForm()" title="Reset Form" style="width:100px;" name="btnReset2" type="button">Reset</button>
			
	</div>		
		<br class="clearFloat" />
	<br class="clearFloat" />

<!-------------->

</asp:Panel>


<asp:Panel ID="hiddenStuff" runat="server" Visible="false">

<asp:Literal ID="usingProperCl" runat="server" Visible="false">yes</asp:Literal>
<asp:Literal ID="formattedClusterTemplate" runat="server" Visible="false"><span style="padding:3px 4px;" class="{0}">{1}</span></asp:Literal>


</asp:Panel>

<asp:Panel ID="Panel4" runat="server" Visible="false">
	<asp:Literal ID="careerClusterTitleDisplay" runat="server" Visible="false">FormattedTitle</asp:Literal>
  <asp:Literal ID="usingResourceClusterId" runat="server" Visible="false">yes</asp:Literal>
	<asp:Literal ID="formattedSourceTemplate" runat="server" Visible="false"><a style="color:#000; " href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>
	<asp:Literal ID="formattedCCRightsUrl" runat="server" Visible="false"><a style="color:#000; " href="{0}" target="_blank" title="{0}">{1}</a></asp:Literal>

<asp:Literal ID="keywordTemplate" runat="server" Visible="false"> (lr.Title like '{0}'  OR lr.[Subjects] like '{0}' OR lr.[Description] like '{0}' OR lr.[ResourceUrl] like '{0}'   OR lrkey.Keyword like '{0}' ) </asp:Literal>

	<asp:Literal ID="keywordTemplate2" runat="server" Visible="false"> (lr.Title like '{0}' OR lr.[Description] like '{0}' OR lr.[ResourceUrl] like '{0}' OR lr.Subjects like '{0}'  OR lr.Keywords like '{0}'   OR lr.Rights like '{0}') </asp:Literal>

<asp:Literal ID="txtPublisherTemplate" runat="server" Visible="false"> (lr.Publisher like '{0}' ) </asp:Literal>
	<asp:Literal ID="Literal1" runat="server" Visible="false"> (lr.Title like '{0}' OR lr.[Description] like '{0}' OR lr.[Publisher] like '{0}' OR lr.[ResourceUrl] like '{0}' OR lrp.SubjectCsv like '{0}'  OR lrkey.OriginalValue like '{0}') </asp:Literal>

	<asp:Literal ID="publisherSearchTemplate" runat="server" Visible="false"> ([Publisher] = '{0}') </asp:Literal>
  <asp:Literal ID="creatorSearchTemplate" runat="server" Visible="false"> (lr.Creator = '{0}') </asp:Literal>
	<asp:Literal ID="publisherDisplayTemplate" runat="server" Visible="false"><br /><strong>Publisher:</strong> &nbsp;<a href="lrsearch.aspx?pub={0}" target="_blank">{0}</a></asp:Literal>

  <asp:Literal ID="keywordSearchTemplate" runat="server" Visible="false"> (lrkey.Keyword like '%{0}%' ) </asp:Literal>

  <asp:Literal ID="subjectSearchTemplate" runat="server" Visible="false"> (lr.Subjects like '%{0}%' ) </asp:Literal>
  <asp:Literal ID="subjectSearchTemplate2" runat="server" Visible="false"> (lrs.SubjectsIdx like '%{0}%' ) </asp:Literal>
  <asp:Literal ID="subjectSearchLinkTemplate" runat="server" Visible="false"><a href="/Search.aspx?subject={0}" target="_blank">{0}</a></asp:Literal>
  <asp:Literal ID="selectLanguages" runat="server" Visible="false">SELECT [Id],[Title] + ' (' + convert(varchar, isnull([WarehouseTotal],0)) + ')' As Title  FROM [dbo].[Codes.Language] where [IsPathwaysLanguage] = 1 order by id</asp:Literal>
</asp:Panel>
