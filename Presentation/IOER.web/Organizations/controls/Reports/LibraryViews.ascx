<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="LibraryViews.ascx.cs" Inherits="IOER.Organizations.controls.Reports.LibraryViews" %>
<%@ Register TagPrefix="uc1" TagName="ActivityFilter" Src="~/Organizations/controls/Reports/BaseActivityFilter.ascx" %>
<style type="text/css">
    .altRowStyle { background-color: #EEEEEE; }
</style>
<p>All versions of this report contain library information.  You can optionally specify a date range for activity, and can include views of collections and 
    views of resources.</p><br />
<p><span style="font-weight:bold;">Note:</span> When viewing library, collection, and resource views, if a resource and count appears without a collection name, and
    that same resource also appears with a collection name, the resource view count without the collection name is the total number of views from within that library,
    including any collections the library contains.  The resource view count with the collection name is the total number of views from within that collection.
</p><br />
<uc1:ActivityFilter id="BaseFilters" runat="server" />
<h2>Optional Information</h2>
<asp:CheckBoxList ID="cblOptionalInfo" runat="server">
    <asp:ListItem Text="Collection Views" Value="Collection Views" />
    <asp:ListItem Text="Resource Views" Value="Resource Views" />
</asp:CheckBoxList>
<asp:Button ID="btnView1" runat="server" CssClass ="isleButton bgGreen" Text="View" OnClick="btnView_Click" style="width:auto;" />
<asp:Button ID="btnExport1" runat="server" CssClass="isleButton bgGreen" Text="Export" OnClick="btnExport_Click" style="width:auto;" />
<asp:Panel ID="pnlNoResults" runat="server" Visible="false">
    <span style="font-weight:bold;">No results were found.  Please check your parameters and try again.</span>
</asp:Panel>
<asp:GridView ID="gvResults" runat="server" AutoGenerateColumns="false" Style="width: 100%;"
    GridLines="None" EnableViewState="true">
    <RowStyle CssClass="rowStyle" VerticalAlign="Top" />
    <AlternatingRowStyle CssClass="altRowStyle" VerticalAlign="Top" />
    <HeaderStyle CssClass="header" VerticalAlign="Bottom" />
    <Columns>
        <asp:BoundField HeaderText="Title" DataField="Title" HtmlEncode="false" />
        <asp:BoundField HeaderText="Type" DataField="Type" />
        <asp:BoundField HeaderText="Views" DataField="Views" />
    </Columns>
</asp:GridView>

<asp:Literal ID="txtOrgId" runat="server" Visible="false">0</asp:Literal>
