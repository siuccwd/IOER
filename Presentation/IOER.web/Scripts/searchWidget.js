var $Q = null;
var es_searchBox = null;
var es_searchBar = null;
var es_narrowingBox = null;
var es_resultsBox = null;
var es_countdown = -1;
var es_countdownMax = 800;
var selectedNarrowing = {};
var es = {
	/* -- Functions -- */
	functions: {
		init: function() {
			$Q = jQuery;
			$Q(document).ready(function(){
				es_searchBox = $Q("#es_searchBox");
				es_narrowingBox = $Q("#es_narrowingBox");
				es_resultsBox = $Q("#es_resultsBox");
				es.functions.renderInterface();
				setInterval(es.functions.ticktock, 100);
				$Q("head").append(es.css);
			});
		}
		,
		renderInterface: function() {
			//Render searchbox
			if(es_searchBox){
				es_searchBox.html(es.templates.searchBar);
				es_searchBar = $Q("#es_searchBar");
				es_searchBar.bind("keyup change", es.functions.resetCounter);
			}
			//Render narrowing
			if(es_narrowingBox){
				es_narrowingBox.html("");
				for(i in pulledData.fields){
					es_narrowingBox.append(es.templates.narrowing.content
						.replace(/{es_id}/g, pulledData.fields[i].es_id)
						.replace(/{text}/g, pulledData.fields[i].text)
					);
					console.log(es.templates.narrowing.content
						.replace(/{es_id}/g, pulledData.fields[i].es_id)
						.replace(/{text}/g, pulledData.fields[i].text)
						);
					var content = "";
					for(j in pulledData.fields[i].items){
						content += es.templates.narrowing.narrowingItem
						.replace(/{id}/g, pulledData.fields[i].items[j].id)
						.replace(/{es_id}/g, pulledData.fields[i].es_id)
						.replace(/{text}/g, pulledData.fields[i].items[j].text);
					}
					es_narrowingBox.append(es.templates.narrowing.container
						.replace(/{es_id}/g, pulledData.fields[i].es_id)
						.replace(/{content}/g, content)
					);
					$Q(".es_flyout").hide();
					$Q(".narrowingLink").click(function() {
						es.functions.resetCounter();
					});
				}
			}
			//Render results
			if(es_resultsBox){
				
			}
		}
		,
		resetCounter: function() {
			es_countdown = es_countdownMax;
		}
		,
		ticktock: function() {
			if(es_countdown > 0){
				es_countdown = es_countdown - 100;
				if(es_countdown <= 0){
					es.functions.doSearch();
				}
			}
		}
		,
		showHideNarrowingList: function(esID) {
			var targetLink = $Q(".es_flyoutToggle[es_id=" + esID + "]");
			var targetPanel = $Q(".es_flyout[es_id=" + esID + "]");
			if(targetPanel.attr("state") == "open"){
				targetPanel.hide().attr("state","closed");
				targetLink.removeClass("selected");
			}
			else {
				$Q(".es_flyout").hide().attr("state","closed");
				targetPanel.attr("state","open").fadeIn("fast");
				$(".es_flyoutToggle").removeClass("selected");
				targetLink.addClass("selected");
			}
		}
		,
		toggleNarrowing: function(esID, itemID){
			var panel = $Q(".es_flyout[es_id=" + esID + "]");
			var link = panel.find(".narrowingLink[id=" + itemID + "]");
			if(link.attr("chosen") == "no"){
				link.attr("chosen", "yes").addClass("selected");
			}
			else {
				link.attr("chosen", "no").removeClass("selected");
			}
			var searchArray = [];
			panel.find(".narrowingLink[chosen=yes]").each(function() {
				searchArray.push($Q(this).attr("id"));
			});
			selectedNarrowing[esID] = searchArray;
		}
		,
		doSearch: function() {
			var resultsList = [];
			es_resultsBox.html("");
			
			//DEMO
			if(es_searchBar.val() != ""){
				resultsList = resultsList.concat(results.demo1);
			}
			if( $Q(".es_flyout[es_id=disability] .narrowingLink[chosen=yes]").length > 0){
				resultsList = resultsList.concat(results.demo2);
			}
			if( $Q(".es_flyout[es_id=jobprep] .narrowingLink[chosen=yes]").length > 0){
				resultsList = resultsList.concat(results.demo3);
			}
			//END DEMO
			
			$.ajax({
			  url: "http://localhost:99/Services/ElasticSearchService.asmx/GetCodeTables", //The elastic search location
			  async: true,
			  success: function (msg) { console.log(msg.d) },
			  type: "POST",
			  data: '{ }',
			  dataType: "json",
			  contentType: "application/json; charset=utf-8",
			  dataType: "json"
			});
			
			
			for(i in resultsList){
				es_resultsBox.append(
					es.templates.searchResult
					.replace(/{href}/g, resultsList[i].href)
					.replace(/{title}/g, resultsList[i].title)
					.replace(/{description}/g, resultsList[i].desc)
				);
			}
		}
	}
	,
	/* -- Data -- */
	data: {
		query: {
			text: "",
			offset: 0,
			sort: { field: "", order: "" },
			narrowing: []
		}
		,
		query_narrowing: { field: "", ids: [] }
		,
		
	}
	,
	/* -- Templates -- */
	templates: {
		searchBar:  "<input type=\"text\" id=\"es_searchBar\" placeholder=\"Search...\" />"
		,
		narrowing: {
			attributes: {
				es_id: "",
				text: "",
				cssClass: "es_narrowing",
			}
			,
			content: "<a href=\"javascript:void(null)\" class=\"es_flyoutToggle\" es_id=\"{es_id}\" onclick=\"es.functions.showHideNarrowingList('{es_id}')\">{text}</a>"
			,
			container: "<div es_id=\"{es_id}\" state=\"closed\" class=\"es_flyout\">{content}</div>"
			,
			narrowingItem: "<a href=\"javascript:void(null)\" id=\"{id}\" class=\"narrowingLink\" chosen=\"no\" onclick=\"es.functions.toggleNarrowing('{es_id}',{id})\">{text}</a>"
		}
		,
		searchResult: "<div class=\"es_result\"><h3><a href=\"{href}\" target=\"_blank\">{title}</a></h3><p>{description}</p></div>"
	}
	,
	/* -- CSS -- */
	css: "<style type=\"text/css\">.es_flyoutToggle, .narrowingLink, .es_result { display: block; font-family: Calibri, Arial, Helvetica, Sans-serif; } .narrowingLink { margin-left: 10px; } .es_flyoutToggle.selected, .narrowingLink.selected { font-weight: bold; }</style>"
	,
};
var pulledData = {
	/* -- Fields -- */
	fields: [
		{ es_id: "disability", text: "Disability Resources", items: [ { id: 0, text: "Disability" }, { id: 1, text: "Accessibility" }, { id: 2, text: "Benefits" } ] },
		{ es_id: "jobprep", text: "Job Preparation", items: [ {id: 0, text: "Job Preparation" }, {id: 1, text: "JobPrep App" } ] },
		
	]
};
var results = {
	demo1: [
		{ href: "#", title: "Demo Result 1", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result 2", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result 3", desc: "Description of Demo" },
	]
	,
	demo2: [
		{ href: "#", title: "Demo Result A", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result B", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result C", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result D", desc: "Description of Demo" },
	]
	,
	demo3: [
		{ href: "#", title: "Demo Result I", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result II", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result III", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result IV", desc: "Description of Demo" },
		{ href: "#", title: "Demo Result V", desc: "Description of Demo" },
	]
}
es.functions.init();