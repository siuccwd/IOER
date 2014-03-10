<%@ Control Language="C#" ClassName="SearchConfig" %>


<script type="text/javascript" language="javascript">  var defaultFlyoutWidth = 280;</script>
<script src="/Scripts/flyout.js" type="text/javascript"></script>
<link rel="Stylesheet" href="/Styles/Flyout.css" />

<script type="text/javascript" language="javascript" src="/Scripts/toolTip.js"></script>
<link rel="Stylesheet" type="text/css" href="/Styles/ToolTip.css" />

<style type="text/css">
.ddlCheckOptions
{
  position: absolute;
  left: 0;
  top: 0;
}
.pageSizeController
{
  position: absolute;
  right: 0;
  top: 0;
}
.resultsHeader 
{
    margin: 8px;
    text-align: center;
}

.columns {
  vertical-align: top;
  display: inline-block;
  *display: inline;
}

.searchSection {
   display: inline-block; 
   padding: 3px; 
   margin: 3px;
}
.watermarked { text-align: center; }
.txtKeyword, .watermarked {
  height: 25px;
  border-radius: 5px;
  box-shadow: 3px 3px 4px #AAA;
}
.searchImgBtn {
  vertical-align: middle;
padding-bottom: 5px;
}
#LeftColumnOptions 
{
  padding: 5px;
  width: 225px;
  vertical-align: top;
  display: inline-block;
  *display: inline;
  zoom: 1;
}

/* filters */
.resultsApply { 
	width: 75px;	
	padding: 2px; height: 28px;
	color: #E2E5F3;
  background-color: #4F4E4F;
  border-radius: 5px;  
  text-decoration: none;   
  display: inline-block; 
  text-align: center;           
}
.resultsApply:hover, .resultsApply:focus {
  width: 75px;	
  background-color: #FF5707;
  cursor:pointer;
}
.actionApply { 
	height: 20px;         
}
#resultsMainColumn 
{
  min-height: 500px;
  width: 755px;
  padding: 5px;
  background-color: #fff;
  vertical-align: top;
  zoom: 1;
}
#resultsMainColumn td:first-child {
  border-top: none;
}
#resultsMainColumn .gridResultsHeader {
  background-color: #4F4E4F;
  color: #E2E5F3;
  padding: 2px 8px;
  margin: 10px 2px 15px 2px;
  border-radius: 5px;
  box-shadow: 3px 3px 4px #AAA;
  font-weight: bold;
}
#resultsMainColumn .gridResultsHeader th 
{
  background-color: #4F4E4F;
  color: #E2E5F3;
  padding: 2px 8px;
  margin: 10px 2px 15px 2px;
  border-radius: 5px;
  box-shadow: 3px 3px 4px #AAA;
  font-weight: bold;
  border:none;
}
#resultsMainColumn .gridResultsHeader th a
{
    padding: 3px;
    text-decoration: none;
    line-height: 15px;
}
#resultsMainColumn .gridResultsHeader th a:hover
{
    text-decoration: underline;
}    
#resultsMainColumn .gridResultsHeader th input {
  margin: 5px 0;
}
.result 
{
  padding: 5px;
  position: relative;
  overflow: hidden;
  min-height: 113px;
}
.result .resultData {
  width: 600px;
  vertical-align: top;
  display: inline-block;
  *display: inline;
  zoom: 1;
}
.gridItem input[type=checkbox] {
  margin-top: 15px;
  margin-left: 8px;
}
.gridAltItem input[type=checkbox] {
  margin-top: 15px;
  margin-left: 8px;
}
.result .tools 
{
  width: 90px;
  float: right;
  text-align: right;
  vertical-align: top;
  display: inline-block;
  *display: inline;
  zoom: 1;
}
.result .tools .taggingLink
{
    text-decoration: none;
}
.result .tools .taggingLink:hover
{
    text-decoration: underline;
}
.result .tools .ratings
{
  float:right;
  text-align: right;
}
.result .tools .ratings img {
  
  text-align: right;
}
.result .detail 
{
    word-wrap: break-word;
}

.result .metadata 
{
    list-style-type: none;
    color: #555;
}
.result .metadata li
{
    margin: 1px 10px;
    padding: 0;
    display: inline;
}
.result .metadata li span
{
    font-weight: bold;
}
.result .metadata li a
{
    text-decoration: none;
}
.result .metadata li a:hover
{
    text-decoration: underline;
}
.result .ratingBox {
  height: 25px;
  line-height: 25px;
  vertical-align: top;
}
a#backToTopRight {
	width:64px;
	height:64px;
	opacity:0.5;
	position:fixed;
	bottom:15px;
  right: 10px;
	display:none;
	text-indent:-10000px;
	outline:none !important;
	background-image: url('/images/icons/Top.png');
	background-repeat: no-repeat;
	z-index:500;
}

.flyoutList {
  width: 225px;
  position: relative;
}
ul.flyoutList > li {
  position: static;
}
.flyoutTrigger{
  color: #FFF;
  background-color: #9984BD;
  border-radius: 5px;
  font-size: 115%;
}
.flyoutTrigger:hover, .flyoutTrigger:focus {
  background-color: #FF5707;
  color: #FFF;
}
.flyoutContent {
  width: 280px;
}
.flyoutContent ul {
  list-style-type: none;
}

.flyoutContent ul li {
  display: inline-block;
  *display: inline;
  zoom: 1;
  vertical-align: top;
  margin: 0;
}
.flyoutContent ul li label {
  padding: 2px 5px 2px 10px;
  width: 225px;
  display: inline-block;
  *display: inline;
  zoom: 1;
  cursor: pointer;
}
.flyoutContent ul li input {
  margin-top: 2px;
  vertical-align: top;
  display: inline-block;
  *display: inline;
  zoom: 1;
}
.flyoutContent ul li:hover, .flyoutContent ul li:focus {
  background-color: #FF5707;
  color: #E2E5F3;
}
.flyoutContent ul li a:hover, .flyoutContent ul li a:focus {
  background-color: #FF5707;
  color: #fff;
}
.cbxlReset {
  color: #FFF;
  float:right;
}
.cbxlReset:hover, .cbxlReset:focus {
  text-decoration: underline;
}
/* override for collections flyout */
.flyoutContent .collectionList {
  font-size: 120%;
}
.flyoutContent .collectionList ul {
  list-style:square outside;
}
.flyoutContent .collectionList ul li {
  display:list-item;
}
.PagerContainerTable {
  border: none;
  font-size: 14px;
}
.PagerInfoCell {
  background-color: #B03D25;
  font-size: 14px;
}
.PagerCurrentPageCell, .PagerCurrentPageCell span:hover {
  background-color: #B03D25;
  color: #E2E5F3;
}
.PagerOtherPageCells, .PagerSSCCells {
  background-color: #4AA394;
  color: #E2E5F3;
}
.PagerOtherPageCells .PagerHyperlinkStyle, .PagerSSCCells .PagerHyperlinkStyle {
  color: #E2E5F3;
  font-size: 14px;
}
.PagerOtherPageCells:hover, .PagerSSCCells:hover {
  background-color: #FF5707;
}
</style>
<script runat="server">

</script>