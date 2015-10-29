<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="BaseActivityFilter.ascx.cs" Inherits="IOER.Organizations.controls.Reports.BaseActivityFilter" %>

<script type="text/javascript">
    $(document).ready(function () {
        // set up date picker
        /*$(".pickYourDate").datepicker({
            dateFormat: 'm/d/yy',
            changeMonth: true,
            changeYear: true,
            minDate: new Date(2014, 2 - 1, 26)
        });*/
    });
</script>

<div id="divDate" class="form-group">
    <asp:Label ID="lblStartDate" runat="server" AssociatedControlID="txtStartDate" CssClass="control-label col-sm-1" Text="Start Date:" />
    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control pickYourDate" />
    <asp:Label ID="lblEndDate" runat="server" AssociatedControlID="txtEndDate" CssClass="control-label col-sm-1" Text="End Date:" />
    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control pickYourDate" />
</div>
