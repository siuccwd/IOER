//Initialization
$(document).ready(function () {
  if (typeof (hints) == "undefined") { return; }
  if (hintSystem.countMessages() == 0) { return; }
  hintSystem.setupHints();
  if (hideHints) { hintSystem.hideInstant(); }
});

var hintSystem = {
  setupHints: function() {
    //Create and attach the hint element
    var box = $("<div><h2 class=\"header hintHeader\"><input type=\"button\" value=\"Hide\" class=\"hintClose hide\" onclick=\"hintSystem.hide();\" />Attention!</h2><div class=\"hintList\"></div><h2 class=\"header hintFooter\"><input type=\"button\" value=\"Show\" class=\"hintClose hide\" onclick=\"hintSystem.show();\" />You have <span class=\"hintCount\"></span> Message<span class=\"hintCountPlural\"></span>.</h2></div>");
    box.addClass("hintBox");
    box.addClass("animated");
    $("body").append(box);
    $("body .hintBox").hide().fadeIn(1000);

    //Add hints
    this.renderHints();
  },

  renderHints: function () {
    var box = $("body .hintBox .hintList");
    box.html("");
    if (this.countMessages() == 0) {
      $("body .hintBox").fadeOut(1000);
      return;
    }
    for (i in hints) {
      //Skip if this hint has been hidden
      if (!hints[i].showing) { continue; }

      var data = hints[i];
      //Create and attach element
      var hint = $("<div></div>");
      hint.addClass("hint");
      hint.attr("data-hintID", i);
      hint.attr("data-hintName", data.name);
      box.append(hint);

      //Fetch the DOM-attached hint
      hint = $("body div.hint[data-hintID=" + i + "]");

      //Fill out the HTML
      var header = $("<h2 class=\"header\"></h2>");
      header.html(data.title);
      hint.append(header);
      $("body div.hint[data-hintID=" + i + "] .header").prepend("<input type=\"button\" value=\"X\" class=\"hintClose\" onclick=\"hintSystem.close('" + data.name + "')\" />");
      var text = $("<p class=\"text\"></p>");
      text.html(data.text);
      hint.append(text);
    }

    //Update the message count
    this.countMessages();
  },

  countMessages: function () {
    var length = 0;
    for (i in hints) {
      if (hints[i].showing) { length++; }
    }
    $("body .hintBox .hintCount").html(length == 0 ? "no" : length);
    $("body .hintBox .hintCountPlural").html(length == 1 ? "" : "s");
    return length;
  },

  hide: function() {
    var box = $("body .hintBox");
    box.css("top", "-" + (box.outerHeight() - 20) + "px");
    box.find(".hintHeader").slideUp();
    box.find(".hintFooter").slideDown();
    this.ajax("HideHints", {hide: true}, function () { });
  },

  hideInstant: function() {
    var box = $("body .hintBox");
    box.removeClass("animated");
    this.hide();
    box.addClass("animated");
  },

  show: function () {
    var box = $("body .hintBox");
    box.css("top", "5px");
    box.find(".hintHeader").slideDown();
    box.find(".hintFooter").slideUp();
    this.ajax("HideHints", {hide: false}, function () { });
  },

  close: function (name) {
    this.ajax("HideHint", { name: name }, hintSystem.successClose);
  },

  ajax: function (method, data, success) {
    $.ajax({
      url: "/Services/HintsService.asmx/" + method,
      type: "POST",
      dataType: "json",
      contentType: "application/json; charset=utf-8",
      data: JSON.stringify(data),
      success: function (msg) { success(msg.d); }
    });
  },

  successClose: function (raw) {
    var data = $.parseJSON(raw);
    if (data.isValid) {
      $("div[data-hintName=" + data.extra + "]").remove();
      hints = data.data;
    }
    else {
      alert(data.status);
    }
    hintSystem.renderHints();
  }
}