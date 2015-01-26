<%@ Page Title="" Language="C#" MasterPageFile="~/Masters/Responsive.Master" AutoEventWireup="true" CodeBehind="gooruPlayer.aspx.cs" Inherits="ILPathways.Pages.gooruPlayer" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
<style>

#container { padding-left: 50px; min-height: 50px; transition: padding 1s; }
#playerFrameContainer { position: fixed; top: 150px; bottom: 0px; left: 0px; right: 0px; }
.playerFrame { height: 100%; width: 100%; }
.contributeLink { font-size: 16pt; color: #FFF; border-radius: 5px; background-color:#4AA394; padding:2px 5px;}
.contributeLink:hover {background-color:#FF5707; color:#FFF}
#at4m-dock { display: none; }
@media screen and (max-width: 975px){
	#container { padding-left: 0; }
}
</style>


</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContent" runat="server">
    <div id="container">
        <!--<h1 class="isleH1" style="font-size:228%" ><asp:literal ID="viewTitle" runat="server">gooru Resource</asp:literal></h1>-->
        <div id="publish">
            <a class='contributeLink' href = '/Contribute/?mode=tag&gooruID=<%=gooruID %>' target='gooruRes'>Tag this Resource</a>
        </div>
		<div id="playerFrameContainer">
			<iframe frameborder="0" id="playerFrame" class="playerFrame" runat="server"
				src="http://www.goorulearning.org/p/r/?id=78fa02fc-9e5f-4253-b58d-c46e392eb83c&api_key=960a9175-eaa7-453f-ba03-ecd07e1f1afc"></iframe>
		</div>
        <asp:Panel ID="messagePanel" runat="server" Visible="false">
            <h2>Missing/Invalid Identifier</h2>
            <p>A valid resource identifier was not supplied.</p>
            <p>Select a resource using the gooru search.</p>

        </asp:Panel>
    </div>

<asp:Panel ID="hiddenPanel" runat="server" >
    <!-- 
        http://www.goorulearning.org/embed/collection.htm?id=12f16eb2-fc89-4990-ae15-bcd30bb869cb&apiKey=xxxx
        0718b7b9-a935-425b-8667-6dcd05dd7c01

        valid from gooru site
        aaaa2d72-438f-4a7f-beba-af8f216f0808
        -->


    <asp:Literal ID="playerUrl" runat="server" Visible="false">http://www.goorulearning.org/p/{1}/?api_key=960a9175-eaa7-453f-ba03-ecd07e1f1afc&id={0}</asp:Literal>
    <asp:Literal ID="collectionPlayerUrl1" runat="server" Visible="false">http://www.goorulearning.org/embed/collection.htm?api_key=960a9175-eaa7-453f-ba03-ecd07e1f1afc&id={0}</asp:Literal>
    <asp:Literal ID="collectionPlayerUrl" runat="server" Visible="false">http://www.goorulearning.org/p/c/?api_key=960a9175-eaa7-453f-ba03-ecd07e1f1afc&id={0}</asp:Literal>
    <asp:Literal ID="collectionPlayerUrlConcept" runat="server" Visible="false">http://concept.goorulearning.org/embed/collection.htm?api_key=960a9175-eaa7-453f-ba03-ecd07e1f1afc&id={0}</asp:Literal>

</asp:Panel>
</asp:Content>
