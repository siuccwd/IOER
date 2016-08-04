<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="PublishersSearch.ascx.cs" Inherits="IOER.LRW.controls.PublishersSearch" %>
<%@ Register Namespace="ASPnetControls" Assembly="ILPathways.Utilities" TagPrefix="wcl" %>
 
<style type="text/css">
.watermarked { text-align: center; }
#txtKeyword, .watermarked {
  border-radius: 5px;
  box-shadow: 3px 3px 4px #AAA;
}
.resultData {
  vertical-align: top;
  padding: 2px 5px;
}

/*AutoComplete flyout */
.autocomplete_completionListElement 
{  
	margin : 0px!important;
	background-color : inherit;
	color : windowtext;
	border : buttonshadow;
	border-width : 1px;
	border-style : solid;
	cursor : 'default';
	overflow : auto;
	height : 200px;
    text-align : left; 
    list-style-type : none;
}
/* AutoComplete highlighted item */
.autocomplete_highlightedListItem
{
	background-color: #ffff99;
	color: black;
	padding: 1px;
}
/* AutoComplete item */
.autocomplete_listItem 
{
	background-color : window;
	color : windowtext;
	padding : 1px;
}
</style>

<script type="text/javascript">
<!--
	  //var $ct = jQuery.noConflict();
	  $(document).ready(function () {
	    $('a.cluetip').cluetip({ splitTitle: '|', width: 450 });

	    var $link = $('#mylink');

	    // first initialize the plugin
	    $link.cluetip({ splitTitle: '|', sticky: false,
	      mouseOutClose: true,
	      closePosition: 'title',
	      closeText: '<img src="/fancybox/fancy_close.png" alt="close" />',
	      width: 400
	    });

	    // trigger the event
	    // $link.mouseenter(); // or $link.click(); 

	  }); // end ready

	  $(document).ready(function () {
	    $('div.slide').next().css('display', 'none').end().click(function () {
	      $(this).next().toggle('fast');
	    });
	  });
//-->	
	</script>


<h1 class="isleH1">ISLE Open Education Resources Search by Publisher</h1>

<br />


<!--                                                                                              -->
<div>
        <div class="labelColumn">
        <a class="cluetip" href="#" title="Publisher Search|Type at least three letters to be presented with a list of candidate publishers.|Typing one or two letters and clicking search will list publishers that start with the entered letters. Use an asterisk within the entered text to do a 'Like' search.|The default view is the top publishers, descending by number of resources.">Publisher:</a>
          </div>
        <div class="dataColumn">
          <asp:TextBox ID="txtKeyword" runat="server" Width="600px" autocomplete="off" ></asp:TextBox>
        </div>
            <ajaxToolkit:AutoCompleteExtender
                runat="server" 
                BehaviorID="AutoCompleteEx"
                ID="autoComplete1" 
                TargetControlID="txtKeyword"
                ServicePath="PublishersAutoComplete.asmx" 
                ServiceMethod="GetPublishers"
                MinimumPrefixLength="3" 
                CompletionInterval="100"
                EnableCaching="true"
                CompletionSetCount="20"
                CompletionListCssClass="autocomplete_completionListElement" 
                CompletionListItemCssClass="autocomplete_listItem" 
                CompletionListHighlightedItemCssClass="autocomplete_highlightedListItem"
                DelimiterCharacters=";, :"
                ShowOnlyCurrentWordInCompletionListItem="true" >
                <Animations>
                    <OnShow>
                        <Sequence>
                            <%-- Make the completion list transparent and then show it --%>
                            <OpacityAction Opacity="0" />
                            <HideAction Visible="true" />
                           
                            <%--Cache the original size of the completion list the first time
                                the animation is played and then set it to zero --%>
                            <ScriptAction Script="
                                // Cache the size and setup the initial size
                                var behavior = $find('AutoCompleteEx');
                                if (!behavior._height) {
                                    var target = behavior.get_completionList();
                                    behavior._height = target.offsetHeight - 2;
                                    target.style.height = '0px';
                                }" />
                            
                            <%-- Expand from 0px to the appropriate size while fading in --%>
                            <Parallel Duration=".4">
                                <FadeIn />
                                <Length PropertyKey="height" StartValue="0" EndValueScript="$find('AutoCompleteEx')._height" />
                            </Parallel>
                        </Sequence>
                    </OnShow>
                    <OnHide><%-- --%>
                        <%-- Collapse down to 0px and fade out --%>
                        <Parallel Duration=".4">
                            <FadeOut />
                            <Length PropertyKey="height" StartValueScript="$find('AutoCompleteEx')._height" EndValue="0" />
                        </Parallel>
                    </OnHide>
                </Animations>
            </ajaxToolkit:AutoCompleteExtender>
            	<ajaxToolkit:TextBoxWatermarkExtender ID="TextBoxWatermarkExtender3" runat="server"
	              TargetControlID="txtKeyword"
		          WatermarkText="Search for publishers"
		          WatermarkCssClass="watermarked" Enabled="True"    /> 	

</div>

	<!-- -->
	<br class="clearFloat" />	
	<div class="labelColumn">&nbsp;</div>
	<div class="dataColumn">			
			<asp:button id="btnSearch" runat="server" CssClass="defaultButton offScreen" 
        width="100px" Text="Search" causesvalidation="false" 
        onclick="btnSearch_Click"></asp:button>
			<button type="reset" class="defaultButton" title="Reset"  value="Reset" id="resetButton" style="display:none;">Reset</button>
	</div>		
	<br class="clearFloat" />

<asp:Panel ID="resultsPanel" runat="server">
<br class="clearFloat" />		
<div style="width:100%">
	<div class="clear" style="float:right;">	
		<div class="labelColumn" >Page Size</div>	
		<div class="dataColumn">     
				<asp:dropdownlist id="ddlPageSizeList" runat="server" Enabled="false" AutoPostBack="True" OnSelectedIndexChanged="PageSizeList_OnSelectedIndexChanged"></asp:dropdownlist>
		</div>  
	</div>
	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager1" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="5" NormalModePageCount="5"  GenerateGoToSection="false" GeneratePagerInfoSection="true"   />
	</div>
	<br class="clearFloat" />	
	<asp:gridview id="formGrid" runat="server" autogeneratecolumns="False"
		allowpaging="true" PageSize="15" allowsorting="True" 
    RowStyle-CssClass="gridItem"
    AlternatingRowStyle-CssClass="gridAltItem"
    OnRowDataBound="formGrid_RowDataBound" 
		OnPageIndexChanging="formGrid_PageIndexChanging"
		onsorting="formGrid_Sorting"			
		BorderStyle="None" BorderWidth="0px" CellPadding="3" GridLines="None" Width="100%" 
		captionalign="Top" 
		useaccessibleheader="true"  >
		<HeaderStyle CssClass="gridResultsHeader isleH2" horizontalalign="Left" />
		<columns>
			<asp:TemplateField HeaderText="Publisher"  sortexpression="Publisher">
				<ItemTemplate>
        <div class="resultData">
					<asp:Label ID="lblPublisher" runat="server" Visible="false"><%# DataBinder.Eval( Container.DataItem, "Publisher" )%></asp:Label>
          </div>
				</ItemTemplate>
			</asp:TemplateField>	
      		
			<asp:TemplateField HeaderText="Resources Count"  sortexpression="ResourceTotal" ItemStyle-HorizontalAlign="Right">
				<ItemTemplate>
					<%# Eval( "ResourceTotal" )%>
				</ItemTemplate>
			</asp:TemplateField>	
			
											
		</columns>
		<pagersettings Visible="false" 
						mode="NumericFirstLast"
            firstpagetext="First"
            lastpagetext="Last"
            pagebuttoncount="5"  
            position="TopAndBottom"/> 
	
	</asp:gridview>
	<div style="float:left;">
	  <wcl:PagerV2_8 ID="pager2" runat="server" Visible="false" OnCommand="pager_Command" GenerateFirstLastSection="true" FirstClause="First Page" PreviousClause="Prev." NextClause="Next"  LastClause="Last Page" PageSize="15" CompactModePageCount="2" NormalModePageCount="5"  GenerateGoToSection="false" GeneratePagerInfoSection="true"   />
	</div>	
<br class="clearFloat" />
</div>			
</asp:Panel>



<asp:Panel ID="Panel4" runat="server" Visible="false">
<asp:Literal ID="doingAutoSearch" runat="server" Visible="false">yes</asp:Literal>
	<asp:Literal ID="publisherDisplayTemplate" runat="server" Visible="false">
  <strong><a href="/Search.aspx?pub={0}" >{0}</a></strong> &nbsp;<a href="/Search.aspx?pub={0}" target="_blank">Open in new window</a>
  
  </asp:Literal>
  	<asp:Literal ID="defaultSearchOrderBy" runat="server" Visible="false">[ResourceTotal] desc, Publisher</asp:Literal>

<asp:Literal ID="topPublisherSearchTemplate" runat="server" Visible="false">SELECT top 500 [Id] ,[Publisher] ,[ResourceTotal]
  FROM [dbo].[PublisherSummary] where [IsActive] = 1 {0}
  Order by {0}</asp:Literal>

	<asp:Literal ID="publisherSearchTemplate" runat="server" Visible="false">SELECT [Id] ,[Publisher] ,[ResourceTotal]
  FROM [dbo].[PublisherSummary] where [IsActive] = 1 {0}
  Order by {0}</asp:Literal>

</asp:Panel>