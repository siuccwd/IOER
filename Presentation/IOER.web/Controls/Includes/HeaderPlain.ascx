<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="HeaderPlain.ascx.cs" Inherits="ILPathways.Includes.HeaderPlain" %>

<link rel="stylesheet" type="text/css" href="/styles/superfish.css" media="screen" />
<script type="text/javascript" src="/Scripts/jquery-1.4.1.min.js"></script>
<script type="text/javascript" src="/Scripts/hoverIntent.js"></script>
<script type="text/javascript" src="/Scripts/superfish.js"></script>
<script type="text/javascript">
	var $sf = jQuery.noConflict();
	// initialise plugins
	$sf(function () {
		$sf('ul.sf-menu').superfish();
	});
</script>
<script type="text/javascript">
<!--
	function highlightCurrentPageLink() {
		//scan links only within the Menu container, the Table with ID header
		if (document.getElementById('ipMenu').getElementsByTagName('a')) {
			var linkIL;
			for (var i = 0; (linkIL = document.getElementById('ipMenu').getElementsByTagName('a')[i]); i++) {
				if (linkIL.href.indexOf(location.href) != -1) {
					linkIL.style.color = '#fff';
					linkIL.style.background = '#0073AE';

					if (linkIL.className == "childIL") {
						ancestorIL = linkIL.parentNode.parentNode.parentNode.getElementsByTagName('a');
						ancestorIL[0].style.color = '#fff';
						ancestorIL[0].style.background = '#0073AE';
					}
					//document.getElementById('ipMenu').getElementsByTagName('a')[i].style.color='#fff';
					//document.getElementById('ipMenu').getElementsByTagName('a')[i].style.background='#0073AE';
					break;
				}
			}

		}
	}

	window.onload = function () {
		//highlightCurrentPageLink();
	}


//-->
</script>
<script type="text/javascript" language="javascript">
<!--
	//function to handle when users presses 'enter'
	function KeyDownHandler() {
		// process only the Enter key
		if (event.keyCode == 13) {
			// cancel the default submit
			event.returnValue = false;
			event.cancel = true;
			//var btn = document.getElementById('XX=btnSearchClientId XX');
			// submit the form by programmatically clicking the specified button
			//btn.click();
		}
	}
    

//-->		
</script>
<div class="clearFloat">
	&nbsp;</div>

<asp:Panel ID="hiddenPanel" runat="server" Visible="false">
	<asp:Literal ID="rememberingContext" runat="server" Visible="false">yes</asp:Literal>
	<asp:Literal ID="regLinkTitle" runat="server" Visible="false">Sign-Up</asp:Literal>
	<asp:Literal ID="regUrl" runat="server" Visible="false">http://www.illinoisworknet.com/vos_portal/residents/en/home/setup_account_all.htm</asp:Literal>
	<asp:Literal ID="overrideUserNames" runat="server" Visible="false">programAdmin dane</asp:Literal>
	<asp:Literal ID="txtSitePrefix" runat="server"></asp:Literal>
</asp:Panel>
