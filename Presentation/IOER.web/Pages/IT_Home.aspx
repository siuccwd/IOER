<%@ Page Title="Illinois Open Educational Resources - IT Home" Language="C#" AutoEventWireup="true" CodeBehind="IT_Home.aspx.cs" Inherits="IOER.Pages.IT_Home" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
    	html, body, form { min-height: 100%; }
      * { font-family: Calibri, Arial, Helvetica, sans-serif; box-sizing: border-box; -moz-box-sizing: border-box; }
			#content { width: 960px; margin: 0 auto; }
    	#main, #sidebar { display: inline-block; vertical-align: top; margin-right: -4px; padding: 5px; }
    	#main { width: 650px; }
    	#sidebar { width: 310px; }
    	.box { margin-bottom: 10px; }
    	#featured { width: 100%; height: 200px; }
    	iframe { border: 0; }

			/* Widget settings */
    	#widget_library, #widget_calendar, #widget_locator { width: 100%; }
			#widget_library { height: 850px; }
    	#widget_calendar { height: 525px; }
    	#widget_locator { height: 525px; }

			/* Colors */
    	body { background-color: #333; padding: 0; margin: 0; }
    	#content { background-color: #BBB; min-height: 100%; }
    	#main { min-height: 100%; }
    	.box { background-color: #DDD; }
			/* Temporary */
    	.demo { text-align: center; padding-top: 100px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="content">
    
			<div id="main">
				<div id="featured" class="box demo">
					Featured
				</div>
				<iframe id="widget_library" src="//ioer.ilsharedlearning.org/Widgets/Library/?library=87&collections=272,271,267" class="box" ></iframe>
			</div><!--/main-->
			<div id="sidebar">
				<div id="widget_calendar" class="box">
                    <!--[if lt IE 9]><iframe src="http://test.illinoisworknet.com/Public/Events/CalendarWidget1.aspx?publicsep=4AA394&fontFamily=Calibri,Arial,Helvetica,sans-serif" width="100%" height="100%">Your browser doesn't seem to support the "iframe" tag.</iframe><![endif]-->
                    <!--[if gte IE 9]><object data="http://test.illinoisworknet.com/Public/Events/CalendarWidget1.aspx?publicsep=4AA394&fontFamily=Calibri,Arial,Helvetica,sans-serif" width="100%" height="100%"><embed src="http://test.illinoisworknet.com/Public/Events/CalendarWidget1.aspx?eventseparator=4AA394&fontFamily=Calibri,Arial,Helvetica,sans-serif" width="100%" height="100%" />Your browser doesn't seem to support the "object" or "embed" tags.</object><![endif]-->
                    <!--[if !IE]>--><object data="http://test.illinoisworknet.com/Public/Events/CalendarWidget1.aspx?publicsep=4AA394&fontFamily=Calibri,Arial,Helvetica,sans-serif" width="100%" height="100%"><embed src="http://test.illinoisworknet.com/Public/Events/CalendarWidget1.aspx?eventseparator=4AA394&fontFamily=Calibri,Arial,Helvetica,sans-serif" width="100%" height="100%" />Your browser doesn't seem to support the "object" or "embed" tags.</object><!--<![endif]-->

				</div>
				<div id="widget_locator" class="box demo">
					Locator
				</div>
			</div><!--/sidebar-->

    </div>
    </form>
</body>
</html>
