#region // using Directives
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#endregion
/* Source
 * http://www.codeproject.com/KB/custom-controls/ASPNETPagerControl.aspx
 * 
 * 
 * */
namespace ASPnetControls
{
	[ToolboxData( "<{0}:PagerV2_8 runat=\"server\"></{0}:PagerV2_8>" )]
	public class PagerV2_8 : WebControl, IPostBackEventHandler, INamingContainer
	{
		#region // Save/Load Control State
		protected override void OnInit( EventArgs e )
		{
			base.OnInit( e );
			Page.RegisterRequiresControlState( this );
		}

		protected override object SaveControlState()
		{
			object[] objState = new object[ 2 ];
			objState[ 0 ] = CurrentIndex;
			objState[ 1 ] = PageSize;

			return objState;
		}

		protected override void LoadControlState( object state )
		{
			object[] savedState = ( object[] ) state;
			CurrentIndex = ( int ) savedState[ 0 ];
			PageSize = ( int ) savedState[ 1 ];
		}
		#endregion

		#region // PostBack Stuff
		private static readonly object EventCommand = new object();

		public event CommandEventHandler Command
		{
			add { Events.AddHandler( EventCommand, value ); }
			remove { Events.RemoveHandler( EventCommand, value ); }
		}

		protected virtual void OnCommand( CommandEventArgs e )
		{
			CommandEventHandler clickHandler = ( CommandEventHandler ) Events[ EventCommand ];
			if ( clickHandler != null )
				clickHandler( this, e );
		}

		void IPostBackEventHandler.RaisePostBackEvent( string eventArgument )
		{
			OnCommand( new CommandEventArgs( this.UniqueID, Convert.ToInt32( eventArgument ) ) );
		}
		#endregion

		#region // Accessors (Behavioural)

		/// <summary>
		/// Gets or sets total number of rows.
		/// </summary>
		private double _itemCount;
		[Browsable( false )]
		public double ItemCount
		{
			get { return _itemCount; }
			set
			{
				_itemCount = value;

				double divide = ItemCount / PageSize;
				double ceiled = System.Math.Ceiling( divide );
				PageCount = Convert.ToInt32( ceiled );
			}
		}

		/// <summary>
		/// Gets or sets current page index.
		/// </summary>
		private int _currentIndex = 1;
		[Browsable( false )]
		public int CurrentIndex
		{
			get { return _currentIndex; }
			set { _currentIndex = value; }
		}

		/// <summary>
		/// Gets or sets page size (results per page).
		/// </summary>
		private int _pageSize = 15;
		[Category( "Behavioural" )]
		public int PageSize
		{
			get { return _pageSize; }
			set { _pageSize = value; }
		}

		/// <summary>
		/// Gets or sets the total number of pages.
		/// </summary>
		private int _pageCount;
		[Browsable( false )]
		private int PageCount
		{
			get { return _pageCount; }
			set { _pageCount = value; }
		}

		/// <summary>
		/// Gets or sets the value that indicates whether the Next and Last clause is rendered as UI on page.
		/// </summary>
		private bool _showFirstLast = false;
		[Category( "Behavioural" )]
		public bool GenerateFirstLastSection
		{
			get { return _showFirstLast; }
			set { _showFirstLast = value; }
		}

		/// <summary>
		/// Gets or sets the value that indicates whether the SmartShortcuts are rendered as UI on page.
		/// </summary>
		private bool _enableSSC = true;
		[Category( "Behavioural" )]
		public bool GenerateSmartShortCuts
		{
			get { return _enableSSC; }
			set { _enableSSC = value; }
		}

		/// <summary>
		/// Gets or sets the value that will be used to calculate SmartShortcuts.
		/// </summary>
		private double _sscRatio = 3.0D;
		[Category( "Behavioural" )]
		public double SmartShortCutRatio
		{
			get { return _sscRatio; }
			set { _sscRatio = value; }
		}

		/// <summary>
		/// Gets or sets maximum number of SmartShortcuts that can be rendered.
		/// </summary>
		private int _maxSmartShortCutCount = 6;
		[Category( "Behavioural" )]
		public int MaxSmartShortCutCount
		{
			get { return _maxSmartShortCutCount; }
			set { _maxSmartShortCutCount = value; }
		}

		/// <summary>
		/// Gets or sets a value that to have the SmartShortcuts rendered, the page count must be greater that this value.
		/// </summary>
		private int _sscThreshold = 30;
		[Category( "Behavioural" )]
		public int SmartShortCutThreshold
		{
			get { return _sscThreshold; }
			set { _sscThreshold = value; }
		}

		/// <summary>
		/// Gets or sets the number of rendered page numbers in compact mode.
		/// </summary>
		private int _firstCompactedPageCount = 10;
		[Category( "Behavioural" )]
		public int CompactModePageCount
		{
			get { return _firstCompactedPageCount; }
			set { _firstCompactedPageCount = value; }
		}

		/// <summary>
		/// Gets or sets the number of rendered page numbers in standard mode.
		/// </summary>
		private int _notCompactedPageCount = 15;
		[Category( "Behavioural" )]
		public int NormalModePageCount
		{
			get { return _notCompactedPageCount; }
			set { _notCompactedPageCount = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether Pager renders Alt tooltip.
		/// </summary>
		private bool _altEnabled = true;
		[Category( "Behavioural" )]
		public bool GenerateToolTips
		{
			get { return _altEnabled; }
			set { _altEnabled = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether Pager information cell is rendered.
		/// </summary>
		private bool _infoCellVisible = true;
		[Category( "Behavioural" )]
		public bool GeneratePagerInfoSection
		{
			get { return _infoCellVisible; }
			set { _infoCellVisible = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicats whether GoTo section is rendered.
		/// </summary>
		private bool _generateGoToSection = false;
		[Category( "Behavioural" )]
		public bool GenerateGoToSection
		{
			get { return _generateGoToSection; }
			set { _generateGoToSection = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether hidden hyperlinks should render.
		/// </summary>
		private bool _generateHiddenHyperlinks = false;
		[Category( "Behavioural" )]
		public bool GenerateHiddenHyperlinks
		{
			get { return _generateHiddenHyperlinks; }
			set { _generateHiddenHyperlinks = value; }
		}

		/// <summary>
		/// Gets or sets the hidden hyperlinks' QueryString parameter name.
		/// </summary>
		private string _queryStringParameterName = "pagerControlCurrentPageIndex";
		[Category( "Behavioural" )]
		public string QueryStringParameterName
		{
			get { return _queryStringParameterName; }
			set { _queryStringParameterName = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether an image should be shown with the GoTo section 
		/// </summary>
		private bool _showGoToImage = false;
		[Category( "Behavioural" )]
		public bool ShowingGoToImage
		{
			get { return _showGoToImage; }
			set { _showGoToImage = value; }
		}
		#endregion

		#region // Accessors (Globalization)

		/// <summary>
		/// Gets or sets the text caption displayed as "go" in the pager control.
		/// Default value: go
		/// </summary>
		private string _GO = "go";
		[Category( "Globalization" )]
		public string GoClause
		{
			get { return _GO; }
			set { _GO = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "of" in the pager control.
		/// Default value: of
		/// </summary>
		private string _OF = "of";
		[Category( "Globalization" )]
		public string OfClause
		{
			get { return _OF; }
			set { _OF = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "from" in the pager control.
		/// Default value: From
		/// </summary>
		private string _FROM = "From";
		[Category( "Globalization" )]
		public string FromClause
		{
			get { return _FROM; }
			set { _FROM = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "page" in the pager control.
		/// Default value: Page
		/// </summary>
		private string _PAGE = "Page";
		[Category( "Globalization" )]
		public string PageClause
		{
			get { return _PAGE; }
			set { _PAGE = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "to" in the pager control.
		/// Default value: to
		/// </summary>
		private string _TO = "to";
		[Category( "Globalization" )]
		public string ToClause
		{
			get { return _TO; }
			set { _TO = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "Showing Results" in the pager control.
		/// Default value: Showing Results
		/// </summary>
		private string _SHOWING_RESULT = "Showing Results";
		[Category( "Globalization" )]
		public string ShowingResultClause
		{
			get { return _SHOWING_RESULT; }
			set { _SHOWING_RESULT = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "Show Result" in the pager control.
		/// Default value: Show Result
		/// </summary>
		private string _SHOW_RESULT = "Show Result";
		[Category( "Globalization" )]
		public string ShowResultClause
		{
			get { return _SHOW_RESULT; }
			set { _SHOW_RESULT = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "to First Page" in the pager control.
		/// Default value: to First Page
		/// </summary>
		private string _BACK_TO_FIRST = "to First Page";
		[Category( "Globalization" )]
		public string BackToFirstClause
		{
			get { return _BACK_TO_FIRST; }
			set { _BACK_TO_FIRST = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "to Last Page" in the pager control.
		/// Default value: to Last Page
		/// </summary>
		private string _GO_TO_LAST = "to Last Page";
		[Category( "Globalization" )]
		public string GoToLastClause
		{
			get { return _GO_TO_LAST; }
			set { _GO_TO_LAST = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "Back to Page" in the pager control.
		/// Default value: Back to Page
		/// </summary>
		private string _BACK_TO_PAGE = "Back to Page";
		[Category( "Globalization" )]
		public string BackToPageClause
		{
			get { return _BACK_TO_PAGE; }
			set { _BACK_TO_PAGE = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "Next to Page" in the pager control.
		/// Default value: Next to Page
		/// </summary>
		private string _NEXT_TO_PAGE = "Next to Page";
		[Category( "Globalization" )]
		public string NextToPageClause
		{
			get { return _NEXT_TO_PAGE; }
			set { _NEXT_TO_PAGE = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "Last Page" in the pager control.
		/// Default value: &gt;&gt;
		/// </summary>
		private string _LAST = "&gt;&gt;";
		[Category( "Globalization" )]
		public string LastClause
		{
			get { return _LAST; }
			set { _LAST = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "First Page" in the pager control.
		/// Default value: &lt;&lt;
		/// </summary>
		private string _FIRST = "&lt;&lt;";
		[Category( "Globalization" )]
		public string FirstClause
		{
			get { return _FIRST; }
			set { _FIRST = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "Previous Page" in the pager control.
		/// Default value: &lt;
		/// </summary>
		private string _previous = "&lt;";
		[Category( "Globalization" )]
		public string PreviousClause
		{
			get { return _previous; }
			set { _previous = value; }
		}

		/// <summary>
		/// Gets or sets the text caption displayed as "Next Page" in the pager control.
		/// Default value: &gt;
		/// </summary>
		private string _next = "&gt;";
		[Category( "Globalization" )]
		public string NextClause
		{
			get { return _next; }
			set { _next = value; }
		}

		/// <summary>
		/// Gets or sets a value that indicates whether pager control should render RTL or LTR.
		/// </summary>
		private bool _rightToLeft = false;
		[Category( "Globalization" )]
		public bool RTL
		{
			get { return _rightToLeft; }
			set { _rightToLeft = value; }
		}


		#endregion

		#region // Render Utilities
		private string GenerateAltMessage( int pageNumber )
		{
			StringBuilder altGen = new StringBuilder();
			altGen.Append( pageNumber == CurrentIndex ? ShowingResultClause : ShowResultClause );
			altGen.Append( " " );
			altGen.Append( ( ( pageNumber - 1 ) * PageSize ) + 1 );
			altGen.Append( " " );
			altGen.Append( ToClause );
			altGen.Append( " " );
			altGen.Append( pageNumber == PageCount ? ItemCount : pageNumber * PageSize );
			altGen.Append( " " );
			altGen.Append( OfClause );
			altGen.Append( " " );
			altGen.Append( ItemCount );

			return altGen.ToString();
		}

		private string GetAlternativeText( int pageNumber )
		{
			return GenerateToolTips ? string.Format( " title=\"{0}\"", GenerateAltMessage( pageNumber ) ) : "";
		}

		private string RenderFirst()
		{
			string templateCell = "<td class=\"PagerOtherPageCells\"><a class=\"PagerHyperlinkStyle\" href=\"{0}\" title=\"" + " " + BackToFirstClause + " " + "\"> " + FirstClause + " </a></td>";
			return String.Format( templateCell, Page.ClientScript.GetPostBackClientHyperlink( this, "1" ) );
		}

		private string RenderLast()
		{
			string templateCell = "<td class=\"PagerOtherPageCells\"><a class=\"PagerHyperlinkStyle\" href=\"{0}\" title=\"" + " " + GoToLastClause + " " + "\"> " + LastClause + " </a></td>";
			return String.Format( templateCell, Page.ClientScript.GetPostBackClientHyperlink( this, PageCount.ToString() ) );
		}

		private string RenderBack()
		{
			string templateCell = "<td class=\"PagerOtherPageCells\"><a class=\"PagerHyperlinkStyle\" href=\"{0}\" title=\"" + " " + BackToPageClause + " " + ( CurrentIndex - 1 ).ToString() + "\"> " + PreviousClause + " </a></td>";
			return String.Format( templateCell, Page.ClientScript.GetPostBackClientHyperlink( this, ( CurrentIndex - 1 ).ToString() ) );
		}

		private string RenderNext()
		{
			string templateCell = "<td class=\"PagerOtherPageCells\"><a class=\"PagerHyperlinkStyle\" href=\"{0}\" title=\"" + " " + NextToPageClause + " " + ( CurrentIndex + 1 ).ToString() + "\"> " + NextClause + " </a></td>";
			return String.Format( templateCell, Page.ClientScript.GetPostBackClientHyperlink( this, ( CurrentIndex + 1 ).ToString() ) );
		}

		private string RenderCurrent()
		{
			return "<td class=\"PagerCurrentPageCell\"><span class=\"PagerHyperlinkStyle\" " + GetAlternativeText( CurrentIndex ) + " ><strong> " + CurrentIndex.ToString() + " </strong></span></td>";
		}

		private string RenderOther( int pageNumber )
		{
			string templateCell = "<td class=\"PagerOtherPageCells\"><a class=\"PagerHyperlinkStyle\" href=\"{0}\" " + GetAlternativeText( pageNumber ) + " > " + pageNumber.ToString() + " </a></td>";
			return String.Format( templateCell, Page.ClientScript.GetPostBackClientHyperlink( this, pageNumber.ToString() ) );
		}

		private string RenderSSC( int pageNumber )
		{
			string templateCell = "<td class=\"PagerSSCCells\"><a class=\"PagerHyperlinkStyle\" href=\"{0}\" " + GetAlternativeText( pageNumber ) + " > " + pageNumber.ToString() + " </a></td>";
			return String.Format( templateCell, Page.ClientScript.GetPostBackClientHyperlink( this, pageNumber.ToString() ) );
		}

		private string RenderGoTo()
		{
			string imageScript = "";

			if ( ShowingGoToImage )
			{
				imageScript = "<img id=\"goto_img\" onclick=\"handleGoToVisibility()\" src=\""
				+ Page.ClientScript.GetWebResourceUrl( this.GetType(), "WebControlLibrary.Images.pager_Arrow_right.gif" )
				+ "\" alt=\"\" class=\"GoToArrow\"/>";
			}

			string templateCell = "<td style=\"padding:1px 1px 1px 1px;\" class=\"PagerOtherPageCells\"><div onclick=\"handleGoToVisibility()\" class=\"GoToLabel\">&nbsp;Go To&nbsp;</div>"
				+ imageScript
				+ "&nbsp;<div id=\"div_goto\" style=\"display:none;\"><select class=\"GoToSelect\" name=\"ddlTes\" id=\"ddlTes\" onchange=\"javascript:handleGoto(this);\">{0}</select></div></td>";
			string listItemTemplate = "<option {0} value=\"{1}\">{2}</option>";

			StringBuilder sb = new StringBuilder();
			for ( int i = 1; i <= this.PageCount; i++ )
			{
				sb.Append( string.Format( listItemTemplate, i == CurrentIndex ? "selected=\"selected\" class=\"GoToSelectedOption\"" : "", Page.ClientScript.GetPostBackClientHyperlink( this, i.ToString() ), i ) );
			}
			return string.Format( templateCell, sb.ToString() );
		}

		private string RenderGoToScript()
		{
			StringBuilder sb = new StringBuilder();
			string imageScript = "";

			if ( ShowingGoToImage )
			{
				sb.Append( @"
                                function handleGoto(selectObj) {
                                    eval(selectObj.options[selectObj.selectedIndex].value);
                                }

                                function handleGoToVisibility() {
                                    var gotoElem = document.getElementById('div_goto');
                                    gotoElem.style.display = gotoElem.style.display == 'none' ? 'inline' : 'none';
                                    var gotoImg = document.getElementById('goto_img');
                                    
                                " );

				sb.AppendFormat( "gotoImg.src = gotoElem.style.display == 'none' ? '{0}' : '{1}';",
																Page.ClientScript.GetWebResourceUrl( this.GetType(), "WebControlLibrary.Images.pager_Arrow_right.gif" ),
																Page.ClientScript.GetWebResourceUrl( this.GetType(), "WebControlLibrary.Images.pager_Arrow_left.gif" )
																);
			}
			else
			{
				sb.Append( @"
                                function handleGoto(selectObj) {
                                    eval(selectObj.options[selectObj.selectedIndex].value);
                                }

                                function handleGoToVisibility() {
                                    var gotoElem = document.getElementById('div_goto');
                                    gotoElem.style.display = gotoElem.style.display == 'none' ? 'inline' : 'none';
                                " );

				//sb.AppendFormat( "gotoImg.src = gotoElem.style.display == 'none' ? '{0}' : '{1}';",
				//                        Page.ClientScript.GetWebResourceUrl( this.GetType(), "WebControlLibrary.Images.pager_Arrow_right.gif" ),
				//                        Page.ClientScript.GetWebResourceUrl( this.GetType(), "WebControlLibrary.Images.pager_Arrow_left.gif" )
				//                        );
			}

			sb.Append( "}" );

			string goToScript = "<script type=\"text/javascript\">{0}</script>";

			return string.Format( goToScript, sb.ToString() );
		}
		#endregion

		#region // Smart ShortCut Stuff

		private List<int> _smartShortCutList;
		private List<int> SmartShortCutList
		{
			get { return _smartShortCutList; }
			set { _smartShortCutList = value; }
		}

		private void CalculateSmartShortcutAndFillList()
		{
			_smartShortCutList = new List<int>();
			double shortCutCount = this.PageCount * SmartShortCutRatio / 100;
			double shortCutCountRounded = System.Math.Round( shortCutCount, 0 );
			if ( shortCutCountRounded > MaxSmartShortCutCount )
				shortCutCountRounded = MaxSmartShortCutCount;
			if ( shortCutCountRounded == 1 )
				shortCutCountRounded++;

			for ( int i = 1; i < shortCutCountRounded + 1; i++ )
			{
				int calculatedValue = ( int ) ( System.Math.Round( ( this.PageCount * ( 100 / shortCutCountRounded ) * i / 100 ) * 0.1, 0 ) * 10 );
				if ( calculatedValue >= this.PageCount )
					break;
				SmartShortCutList.Add( calculatedValue );
			}
		}

		/* smart shortcut list calculator and list */
		private void RenderSmartShortCutByCriteria( int basePageNumber, bool getRightBand, HtmlTextWriter writer )
		{
			if ( IsSmartShortCutAvailable() )
			{

				List<int> lstSSC = this.SmartShortCutList;

				int rVal = -1;
				if ( getRightBand )
				{
					for ( int i = 0; i < lstSSC.Count; i++ )
					{
						if ( lstSSC[ i ] > basePageNumber )
						{
							rVal = i;
							break;
						}
					}
					if ( rVal >= 0 )
					{
						for ( int i = rVal; i < lstSSC.Count; i++ )
						{
							if ( lstSSC[ i ] != basePageNumber )
							{
								writer.Write( RenderSSC( lstSSC[ i ] ) );
							}
						}
					}
				}
				else if ( !getRightBand )
				{

					for ( int i = 0; i < lstSSC.Count; i++ )
					{
						if ( basePageNumber > lstSSC[ i ] )
						{
							rVal = i;
						}
					}

					if ( rVal >= 0 )
					{
						for ( int i = 0; i < rVal + 1; i++ )
						{
							if ( lstSSC[ i ] != basePageNumber )
							{
								writer.Write( RenderSSC( lstSSC[ i ] ) );
							}
						}
					}
				}
			}
		}

		bool IsSmartShortCutAvailable()
		{
			return this.GenerateSmartShortCuts && this.SmartShortCutList != null && this.SmartShortCutList.Count != 0;
		}
		#endregion

		#region // Render "SearchEngineFriendly" hyperlinks in HiddenDiv
		private string RenderHiddenDiv()
		{
			System.Text.RegularExpressions.Regex regEx;
			Uri theURL = System.Web.HttpContext.Current.Request.Url;
			bool hasQueryStringParam = !string.IsNullOrEmpty( System.Web.HttpContext.Current.Request.ServerVariables[ "QUERY_STRING" ] ) ? true : false;
			string tempHyperlink = "<a href=\"{0}\">page {1}</a>";
			string tempDiv = "<div style=\"display:none;\">{0}</div>";
			StringBuilder sb = new StringBuilder();

			if ( hasQueryStringParam && System.Web.HttpContext.Current.Request.QueryString[ this.QueryStringParameterName ] != null )
			{
				regEx = new Regex( this.QueryStringParameterName + @"\=\d*", RegexOptions.Compiled | RegexOptions.Singleline );
				for ( int i = 0; i < this.NormalModePageCount; i++ )
				{
					sb.Append( string.Format( tempHyperlink,
											regEx.Replace( theURL.ToString(), this.QueryStringParameterName + "=" + ( i + this.CurrentIndex ) ), i + this.CurrentIndex )
							);
				}
			}
			else
			{
				string qsParameterName = "";
				for ( int i = 0; i < this.NormalModePageCount; i++ )
				{
					qsParameterName = string.Format( "{0}={1}", this.QueryStringParameterName, i + this.CurrentIndex );
					sb.Append( string.Format( tempHyperlink,
											hasQueryStringParam ? theURL.ToString() + "&" + qsParameterName : theURL.ToString() + "?" + qsParameterName,
											i + this.CurrentIndex )
									);
				}

			}

			return string.Format( tempDiv, sb.ToString() );
		}
		#endregion

		#region // Override Control's Render operation
		protected override void Render( HtmlTextWriter writer )
		{

			if ( Page != null )
				Page.VerifyRenderingInServerForm( this );

			if ( this.PageCount > this.SmartShortCutThreshold && GenerateSmartShortCuts )
			{
				CalculateSmartShortcutAndFillList();
			}

			writer.AddAttribute( HtmlTextWriterAttribute.Cellpadding, "3" );
			writer.AddAttribute( HtmlTextWriterAttribute.Cellspacing, "1" );
			writer.AddAttribute( HtmlTextWriterAttribute.Border, "0" );
			writer.AddAttribute( HtmlTextWriterAttribute.Class, "PagerContainerTable" );
			if ( RTL )
				writer.AddAttribute( HtmlTextWriterAttribute.Dir, "rtl" );
			writer.RenderBeginTag( HtmlTextWriterTag.Table );
			writer.RenderBeginTag( HtmlTextWriterTag.Tr );

			if ( GeneratePagerInfoSection )
			{
				writer.AddAttribute( HtmlTextWriterAttribute.Class, "PagerInfoCell" );
				writer.RenderBeginTag( HtmlTextWriterTag.Td );
				writer.Write( PageClause + " " + CurrentIndex.ToString() + " " + OfClause + " " + PageCount.ToString() );
				writer.RenderEndTag();
			}

			if ( GenerateFirstLastSection && CurrentIndex != 1 )
				writer.Write( RenderFirst() );

			if ( CurrentIndex != 1 )
				writer.Write( RenderBack() );

			if ( CurrentIndex < CompactModePageCount )
			{

				if ( CompactModePageCount > PageCount )
					CompactModePageCount = PageCount;

				for ( int i = 1; i < CompactModePageCount + 1; i++ )
				{
					if ( i == CurrentIndex )
					{
						writer.Write( RenderCurrent() );
					}
					else
					{
						writer.Write( RenderOther( i ) );
					}
				}

				RenderSmartShortCutByCriteria( CompactModePageCount, true, writer );

			}
			else if ( CurrentIndex >= CompactModePageCount && CurrentIndex < NormalModePageCount )
			{

				if ( NormalModePageCount > PageCount )
					NormalModePageCount = PageCount;

				for ( int i = 1; i < NormalModePageCount + 1; i++ )
				{
					if ( i == CurrentIndex )
					{
						writer.Write( RenderCurrent() );
					}
					else
					{
						writer.Write( RenderOther( i ) );
					}
				}

				RenderSmartShortCutByCriteria( NormalModePageCount, true, writer );

			}
			else if ( CurrentIndex >= NormalModePageCount )
			{
				int gapValue = NormalModePageCount / 2;
				int leftBand = CurrentIndex - gapValue;
				int rightBand = CurrentIndex + gapValue;


				RenderSmartShortCutByCriteria( leftBand, false, writer );

				for ( int i = leftBand; ( i < rightBand + 1 ) && i < PageCount + 1; i++ )
				{
					if ( i == CurrentIndex )
					{
						writer.Write( RenderCurrent() );
					}
					else
					{
						writer.Write( RenderOther( i ) );
					}
				}

				if ( rightBand < this.PageCount )
				{

					RenderSmartShortCutByCriteria( rightBand, true, writer );
				}
			}

			if ( CurrentIndex != PageCount )
				writer.Write( RenderNext() );

			if ( GenerateFirstLastSection && CurrentIndex != PageCount )
				writer.Write( RenderLast() );

			if ( GenerateGoToSection )
				writer.Write( RenderGoTo() );

			writer.RenderEndTag();

			writer.RenderEndTag();

			if ( GenerateGoToSection )
				writer.Write( RenderGoToScript() );

			if ( GenerateHiddenHyperlinks )
				writer.Write( RenderHiddenDiv() );
		}
		#endregion
	}
}