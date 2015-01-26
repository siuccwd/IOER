/* ---   ---   ---   --- Page Data Variables ---   ---   ---   --- */

/* ---   ---   ---   --- Page Status Variables ---   ---   ---   --- */

var nodes = [
  {
    id: 1, parentID: 0, depth: 0, notation: "", title: "Curriculum Title", description: "Description goes here", children: [
      {
        id: 2, parentID: 1, depth: 1, notation: "", title: "Unit 1", description: "Description goes here", children: [
          { id: 7, parentID: 2, depth: 2, notation: "", title: "Sample Module A1", description: "Description goes here", children: [] },
          { id: 8, parentID: 2, depth: 2, notation: "", title: "Sample Module A2", description: "Description goes here", children: [] },
        ]
      },
      {
        id: 3, parentID: 1, depth: 1, notation: "", title: "Unit 2", description: "Description goes here", children: [
          { id: 4, parentID: 3, depth: 2, notation: "", title: "Sample Module B1", description: "Description goes here", children: [] },
          { id: 5, parentID: 3, depth: 2, notation: "", title: "Sample Module B2", description: "Description goes here", children: [] },
          { id: 6, parentID: 3, depth: 2, notation: "", title: "Sample Module B3", description: "Description goes here", children: [] },
        ]
      },
    ]
  },
];

/* ---   ---   ---   --- Initialization ---   ---   ---   --- */
$(document).ready(function () {
  //renderAddNode();
  renderNodes();
});

/* ---   ---   ---   --- Page Functions ---   ---   ---   --- */
//Get node by ID
function getNode(id) {
  for (i in nodes) {
    return searchNodes(nodes[i], id);
  }
}
function searchNodes(node, targetID) {
  if (node.id == targetID) { return node; }
  for (i in node.children) {
    var found = searchNodes(node.children[i], targetID);
    if (typeof (found) != "undefined") {
      return found;
    }
  }
}

//Show Node and toggle curriculum/node details
function showNodeInfo(id, depth) {
  var node = getNode(id);
  if (depth == 0) {
    $("#curriculumDetails").addClass("showing");
    $("#nodeDetails").removeClass("showing");
    $("#txtMainTitle").val(node.title);
    $("#txtMainDescription").val(node.description);
  }
  else {
    $("#curriculumDetails").removeClass("showing");
    $("#nodeDetails").addClass("showing");
    $("#txtNodeTitle").val(node.title);
    $("#txtNodeDescription").val(node.description);
  }
}

/* ---   ---   ---   --- Rendering Functions ---   ---   ---   --- */
//Render the node adder
function renderAddNode() {
  var template = $("#template_create_nodeList_node").html();
  var data = { id: 0, parentID: 0, notation: "Add a Node", title: "" };
  $("#addNode").html(jsCommon.fillTemplate(template, data));
}
//Render nodes
function renderNodes() {
  var template = $("#template_create_nodeList_node").html();
  var list = $("#nodes");
  list.html("");
  for (i in nodes) {
    renderNodeTree(nodes[i], 0, "", list, template);
  }
}
//Recursive rendering
function renderNodeTree(parent, depth, notation, list, template) {
  parent.depth = depth;
  parent.notation = notation;
  parent.indent = depth * 25;
  list.append(jsCommon.fillTemplate(template, parent));
  for (i in parent.children) {
    var newNotation = notation + (notation == "" ? "" : ".") + (parseInt(i) + 1);
    renderNodeTree(parent.children[i], depth + 1, newNotation, list, template);
  }
}
