<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ListPanel.ascx.cs" Inherits="IOER.LRW.controls.ListPanel" %>

<div class="listPanel">
<h3 class="isleH3_Block" runat="server" visible="false" ID="header"><asp:Literal ID="title" runat="server" /></h3>
<asp:CheckBoxList ID="cbList" runat="server" RepeatLayout="UnorderedList" />
<asp:RadioButtonList ID="rbList" runat="server" Visible="false" RepeatLayout="UnorderedList" />
<asp:DropDownList ID="ddList" runat="server" Visible="false" />
<asp:Literal ID="ltlList" runat="server" Visible="false" />
</div>

<asp:Literal ID="template_itemCount" runat="server" Visible="false"> <span class="itemCount">(About {0} items)</span></asp:Literal>
<asp:Literal ID="template_itemCountCustom" runat="server" Visible="false"> <span class="itemCount">({0} items)</span></asp:Literal>