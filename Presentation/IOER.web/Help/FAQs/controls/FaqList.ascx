<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FaqList.ascx.cs" Inherits="IOER.Controls.FAQs.FaqList" %>

<%@ Register TagPrefix="uc1" TagName="AskQuestion" Src="~/Controls/FAQs/PostQuestion.ascx" %>

<style type="text/css">

a.reveal {
	font-size: 90%;	padding: 3px;		color: black;	font-style: italic;	font-weight: bold;	cursor: pointer;
}

a.hide {
	font-size: 90%;	padding: 3px;	color: black;	font-style: italic;
		font-weight: bold;	cursor: pointer;	align;right;}

</style>

<script type="text/javascript">
<!--
  $(document).ready(function () {
    $("span.hiddenSection").css('display', 'none');

    $("a.reveal").click(function () {
      $(this).parents("div").children("span.hiddenSection").slideDown(100);
      $(this).parents("div").children("a.reveal").slideUp(60);
    });

    $("a.hide").click(function () {
      $(this).parents("div").children("span.hiddenSection").slideUp(100);
      $(this).parents("div").children("a.reveal").slideDown(60);
    });

    //    $sh("a.expandAll").click(function() {	
    //    $sh("div").children("span.hiddenSection").slideDown(100);
    //    $sh("div").children("a.reveal").slideUp(60);
    //    });
    //    
    //    $sh("a.collapseAll").click(function() {	
    //    $sh("div").children("span.hiddenSection").slideUp(100);	
    //    $sh("div").children("a.reveal").slideDown(60);	        
    //	  });	 
  }); 
-->    

function ShowHideSection(target){
  $("#" + target).slideToggle();
}
</script>
<a name="faqTop"></a>

<asp:Panel ID="questionPanel" Visible="false" runat="server">
<div><h3><a onclick="ShowHideSection('divInstructions1');" href="javascript:void(0);"><img alt="Instructions" src="/images/icons/infoBubble.gif" /> Submit a question</a></h3> <br /></div>		
<div id="divInstructions1" class="infoMessage" style="display:none; margin:5px;">
<uc1:AskQuestion ID="AskQuestion1" runat="server" 
			DefaultCategory="IOER Search" 
			DefaultTargetPathways="3" 
             AllowingQuestions ="false"
			SubcategoryTitle="Subject" />
</div>

</asp:Panel>

<asp:Panel ID="searchPanel" runat="server" Visible="false">
<!-- --> 
<br class="clearFloat" />
  <div class="labelcolumn " > 
    <asp:label id="lblFaqView"  associatedcontrolid="rblFaqView" runat="server">FAQ View</asp:label> 
  </div>
  <div class="dataColumn"> 
    <asp:RadioButtonList id="rblFaqView" autopostback="false" causesvalidation="false"   runat="server" RepeatDirection="Horizontal">
	<asp:ListItem Text="FAQ Sheet"  value="0" Selected="True"></asp:ListItem>    
 	<asp:ListItem Text="Grid View" 	value="1"  ></asp:ListItem>
</asp:RadioButtonList>
  </div>
  
  	<!-- keyword -->  
	<br class="clearFloat" />			
  <div class="labelcolumn" >Keyword filter</div>	
  <div class="dataColumn">   		
		<asp:TextBox ID="txtKeyword" runat="server" Width="300px" ></asp:TextBox>
  </div>    
  
	<!-- -->
	<br class="clearFloat" />	
	<div class="labelcolumn">&nbsp;</div>
	<div class="dataColumn">			
			<asp:button id="btnSearch" runat="server" CssClass="defaultButton" width="130px" CommandName="Search" 
			OnCommand="FormButton_Click" Text="Search" causesvalidation="false"></asp:button> 
	</div>  
		<br class="clearFloat" />			
</asp:Panel>

<asp:Panel ID="faqSheetPanel" runat="server" Visible="false">
<h2>Subjects</h2>

<asp:Literal ID="tocSection" runat="server" ></asp:Literal>

<asp:Literal ID="subjectSection" runat="server" ></asp:Literal>

</asp:Panel>

<asp:Panel ID="resultsPanel" runat="server">
<br class="clearFloat" />		
<div style="width:100%">
	<div class="clear" style="float:right;">	

		<div class="labelcolumn" >Page Size</div>	
		<div class="dataColumn">     
				<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
		</div>  
	</div>
	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
	</div>
	<br class="clearFloat" />	
	<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
		allowpaging="true" PageSize="15" allowsorting="True"  
		OnRowCommand="formGrid_RowCommand"
		OnRowDataBound="formGrid_RowDataBound"
		OnPageIndexChanging="formGrid_PageIndexChanging"
		onsorting="formGrid_Sorting"			
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
		captionalign="Top" 
		useaccessibleheader="true"  >
		<HeaderStyle CssClass="gridResultsHeader" horizontalalign="Left" />
		<columns>
			<asp:TemplateField HeaderText="Select" Visible="false">
				<ItemTemplate>
				 <asp:LinkButton ID="selectButton" CommandArgument='<%# Eval("RowId") %>' CommandName="SelectRow" CausesValidation="false" runat="server">
					 Select</asp:LinkButton>
				</ItemTemplate>
			</asp:TemplateField>
			<asp:BoundField HeaderText="Date" DataFormatString="{0:MMM dd, yyyy}" DataField="Created" sortexpression="base.Created" ItemStyle-Width="115px" />
			
			<asp:TemplateField HeaderText="Subject"  sortexpression="SubCategory" ItemStyle-Width="150px" HeaderStyle-Width="150" >
				<ItemTemplate>
					<%# Eval( "SubCategory" )%>
				</ItemTemplate>
			</asp:TemplateField>								
			
			<asp:TemplateField HeaderText="Question/Answer"  sortexpression="base.Title" >
				<ItemTemplate>
					<asp:Label ID="lblRowStatus" Text='<%# Eval( "Title" )%>' runat="server"></asp:Label>
					<div> 
					<a href="javascript:void(0);" class="reveal">Show Answer... &gt;&gt;</a> 

					<span class="hiddenSection">
							<a class="hide" href="javascript:void(0);" ><<...Hide Answer</a>	
					<br />
							<span>   
							<%# Eval( "Description" )%>
							</span>
							
								<br />	<a class="hide" href="javascript:void(0);" ><<...Hide Answer</a>	
						</span>
		
								</div>					
				</ItemTemplate>
			</asp:TemplateField>	
			
											
		</columns>
		<pagersettings  visible="true" 
						mode="NumericFirstLast"
            firstpagetext="First"
            lastpagetext="Last"
            pagebuttoncount="5"  
            position="TopAndBottom"/> 
	
	</asp:gridview>
	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="10" NormalModePageCount="10"  GenerateGoToSection="true" GeneratePagerInfoSection="true"   />
	</div>	
<br class="clearFloat" />
</div>			
</asp:Panel>
<asp:Panel ID="hiddenStuff" runat="server" Visible="false">
<asp:Literal ID="defaultCategory" runat="server"></asp:Literal>
<asp:Literal ID="defaultTargetPathways" runat="server">3</asp:Literal>
<asp:Literal ID="subcategoryTitle" runat="server">Subject</asp:Literal>
<asp:Literal ID="faqCategoryTitle" runat="server">IOER</asp:Literal>
<asp:Literal ID="confirmLink" runat="server">http://ioer.ilsharedlearning.org/Faqs.aspx?id={0}</asp:Literal>
<asp:Literal ID="txtAllowingQuestions" runat="server">no</asp:Literal>

<!-- control variables -->


<asp:Literal ID="openingDetailInNewWindow" runat="server" Visible="false">yes</asp:Literal>

<!-- templates -->

<asp:Literal ID="sectionTocTemplate" runat="server" Visible="false">
<li><a href="#{0}">{1}</a></li>
</asp:Literal>

<asp:Literal ID="sectionHeader" runat="server" Visible="false">
<br /><a name="{0}"></a><h2>{1}</h2>
</asp:Literal>

<!-- accordion template, need to close the section at end of section -->
<asp:Literal ID="accordionHeader" runat="server" Visible="false">
<br /><a name="{0}"></a><h2 style="margin: 5 0"><a onclick="ShowHideSection('catDiv{0}');" href="javascript:void(0);">{1}</a></h2>
<div class="clear"></div>
<div id="catDiv{0}" style="display: none; width:90%;padding-left:5px;">

<%--</div>--%>
</asp:Literal>
<!-- q# question, answer -->
<asp:Literal ID="faqTemplate" runat="server" Visible="false">
<div style="margin-top: 5px;"><strong>{0}.&nbsp;{1}</strong></div>
<div >{2}</div>
</asp:Literal>
<asp:Literal ID="sectionFooter" runat="server" Visible="false">
<br class="clearFloat" />
<div style="float:right;"><a href="#faqTop">Back to Top</a></div>
<br class="clearFloat" />
</asp:Literal>
<asp:Literal ID="accordionSectionFooter" runat="server" Visible="false">
</div>
<br class="clearFloat" />
<div style="float:right;"><a href="#faqTop">Back to Top</a></div>
<br class="clearFloat" />
</asp:Literal>
</asp:Panel>

