<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="ILPathways.testing.Obout.WebForm1" %>
<%@ Register assembly="Obout.Ajax.UI" namespace="Obout.Ajax.UI.TreeView" tagprefix="obout" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Example Page - Binding With SqlDataSource</title>
     <style type="text/css">
        body
        {
            font-family: "Segoe UI" ,Arial,sans-serif;
            font-size: 12px;
        }
    </style>


</head>
<body>
    <form id="form1" runat="server">
    <br />
	
     <asp:ScriptManager runat="Server" EnablePartialRendering="true" ID="ScriptManager1" />
    <div>
        <h2>Curriculum testing - treeview</h2>

        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:contentConString_RO %>"
            ProviderName="<%$ ConnectionStrings:contentConString_RO.ProviderName %>"
            SelectCommand="SELECT [Id], [Title], [ParentId] FROM [Content] where TypeId=50">
        </asp:SqlDataSource>
        <div style="width: 30%; background-color: #f5f5f5; display: inline-block;">
        <obout:Tree ID="OBTreeview" CssClass="vista" DataSourceID="SqlDataSource1" runat="server"
            OnTreeNodeDataBound="OBTreeview_TreeNodeDataBound"  
            OnSelectedTreeNodeChanged="OBTreeview_SelectedTreeNodeChanged" 
            >
            <DataBindings>
                <obout:NodeBinding DataSourceColumnID="Id"  DataSourceColumnParentID="ParentId" Expanded="true"  />
            </DataBindings>
        </obout:Tree>
        </div>
       <div style="width: 55%; display: inline-block; vertical-align: top;">
        <asp:Label ID="nodeClick" runat="server"></asp:Label>
        </div>
    </div>
    </form>
</body>
</html>
