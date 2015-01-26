/* Commonly-used functionality */
var jsCommon = {

  //Fill an HTML template via regex from an object
  fillTemplate: function (template, kvp) {
    for (var i in kvp) {
      if (typeof (kvp[i]) != "object") {
        template = template.replace(new RegExp("{" + i + "}", "g"), (typeof(kvp[i]) == "undefined" ? "" : kvp[i].toString()));
      }
    }
    return template;
  }


};