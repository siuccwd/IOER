<%@ Page Title="IOER - Registration Step One" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="Success.aspx.cs" Inherits="ILPathways.Account.Success" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="/Styles/Isle_large.css" type="text/css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="/Styles/common2.css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">

    <h1 class="isleH1" style="text-align: center;">Your Account Is Almost Ready</h1>
        <div id="content" style="min-height: 500px; width: 600px; margin: 20px auto; padding: 5px 25px;">
            <div>
                <h2 class="isleH2" >A confirmation of your email address is required</h2>
            <ul style="margin-left: 20px;">
                <li>An email was sent to your email address with a link to complete registration.</li>
                <li>Upon completing registration your account will be activated.</li>
                <li>If you do not receive an email, be sure to check your junk mail folder.</li>
            </ul>
            </div>
            <div>
                <h2 class="isleH2" style="margin-top: 50px;">Libraries</h2>
            <ul style="margin-left: 20px;">
                <li>Upon activation of your account, your personal library will be created, along with the starting default collection. For information on libraries, <a href="/Help/Guide.aspx">see the getting Started with IOER guide</a>.</li>
                <li>You can <a href="/Libraries/Default.aspx">search for existing libraries</a>, or you may want to view one of the STEM libraries, such as the <a href="/Library/69/Health_Sciences">Health Sciences library</a>. </li>
            </ul>
            </div>
            <h2 class="isleH2" style="margin-top: 50px;">Next Steps</h2>
            <ul style="margin-left: 20px;">
                <li><a href="/Help/Guide.aspx">Get Started with IOER</a></li>
                <li><a href="/">IOER Home</a></li>
            </ul>
        </div>

</asp:Content>
