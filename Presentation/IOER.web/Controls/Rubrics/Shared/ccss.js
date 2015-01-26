var ratings = [
  { value: 3, name: "Superior" },
  { value: 2, name: "Strong" },
  { value: 1, name: "Limited" },
  { value: 0, name: "Very Weak/None" },
  { value: "NA", name: "Not Applicable" },
];

function loadRBLs() {
  var rb_template = $("#template_radio").html();
  $(".rating").each(function () {
    var rid = $(this).attr("data-ratingID");
    for (i in ratings) {
      $(this).append(rb_template
        .replace(/{rid}/g, rid)
        .replace(/{value}/g, ratings[i].value)
        .replace(/{name}/g, ratings[i].name)
      );
    }
    $(this).find("input[type=radio]").on("change", function () {
      updateScore(this);
    });
  });
  $(".rating input[value=NA]").prop("checked", true).trigger("change");
}

function updateScore(radio) {
  var cbx = $(radio);
  var dimension = cbx.attr("name").replace("dimension", "");
  var value = parseInt(cbx.attr("value"));
  var rating = null;
  if (!isNaN(value)) {
    rating = Math.ceil((value / 3) * 100);
  }
  for (i in data.dimensions) {
    if (data.dimensions[i].id == dimension) { data.dimensions[i].score = rating; }
  }
  $(window).trigger("scoresUpdated");
}

$(document).ready(function () {
  loadRBLs();
});