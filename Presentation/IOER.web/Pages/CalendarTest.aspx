<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CalendarTest.aspx.cs" Inherits="ILPathways.Pages.CalendarTest" MasterPageFile="/Masters/Responsive.Master" %>

    <asp:Content ID="contentControl" ContentPlaceHolderID="BodyContent" runat="server">
    <div>
<!-- Begin calendar widget -->
        <!--[if lt IE 9]><iframe src="http://localhost:38075/Public/Events/CalendarWidget1.aspx?publicsep=Red" width="600" height="400">Your browser doesn't seem to support the "iframe" tag.</iframe><![endif]-->
        <!--[if gte IE 9]><object data="http://localhost:38075/Public/Events/CalendarWidget1.aspx?publicsep=Green" width="600" height="400"><embed src="http://localhost:38075/Public/Events/CalendarWidget1.aspx?eventseparator=Green" width="600" height="400" />Your browser doesn't seem to support the "object" or "embed" tags.</object><![endif]-->
        <!--[if !IE]>--><object data="http://localhost:38075/Public/Events/CalendarWidget1.aspx?publicsep=#C0C0FF&calendarid=2" width="600" height="400"><embed src="http://localhost:38075/Public/Events/CalendarWidget1.aspx?eventseparator=Blue" width="600" height="400" />Your browser doesn't seem to support the "object" or "embed" tags.</object><!--<![endif]-->
<!-- End calendar widget -->

    </div>
    </asp:Content>