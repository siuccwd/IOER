<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestingEicarVirus.aspx.cs" Inherits="ILPathways.testing.TestingEicarVirus" MasterPageFile="~/Masters/Responsive.Master" %>

    <asp:Content id="content" ContentPlaceHolderID="BodyContent" runat="server">
    <p>Click the button below to send the EICar test virus as a test.  Note that the EICar test virus is not actually a virus, it just looks like one to scanners.  It is very useful for testing purposes.</p>
    <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" />
    </asp:Content>