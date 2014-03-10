Type.registerNamespace("CustomToolbarButton");

// Declare the custom button class
CustomToolbarButton.MakeCodeBlock = function (element) {
    CustomToolbarButton.MakeCodeBlock.initializeBase(this, [element]);
}
CustomToolbarButton.MakeCodeBlock.prototype = {
    onEditPanelActivity: function () {
        if (!CustomToolbarButton.MakeCodeBlock.callBaseMethod(this, "onEditPanelActivity")) return false;
        var editor = this._designPanel;
        if (!editor.isPopup() && this.testCodeBlock()) {
            this.enable();
            return true;
        } else {
            this.disable();
            return false;
        }
    },

    testCodeBlock: function () {
        var editor = this._designPanel;
        if (!editor.TextSelected()) return false;

        var sel = editor._getSelection();
        var range = editor._createRange(sel);
        var parent = Obout.Ajax.UI.HTMLEditor.getSelParent(editor);

        if (parent.nodeType == 3) {
            parent = parent.parentNode;
        }

        while (parent && parent.tagName && parent.tagName.toUpperCase() != "PRE" && !/prettyprint/i.test(parent.className)) {
            parent = parent.parentNode;
        }

        if (parent && parent.tagName && parent.tagName.toUpperCase() == "PRE" && /prettyprint/i.test(parent.className)) {
            return false;
        }
        return true;
    }
}
// Register this class
CustomToolbarButton.MakeCodeBlock.registerClass("CustomToolbarButton.MakeCodeBlock", Obout.Ajax.UI.HTMLEditor.ToolbarButton.OpenPopupButton);
Sys.Application.notifyScriptLoaded();
