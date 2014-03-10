Type.registerNamespace("CustomToolbarButton");

// Declare the custom button class
CustomToolbarButton.RemoveCodeBlock = function (element) {
    CustomToolbarButton.RemoveCodeBlock.initializeBase(this, [element]);
    this._foundCodeBlock = null;
}
CustomToolbarButton.RemoveCodeBlock.prototype = {
    onEditPanelActivity: function () {
        if (!CustomToolbarButton.RemoveCodeBlock.callBaseMethod(this, "onEditPanelActivity")) return false;
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
        var editor = this.get_designPanel();

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
            this._foundCodeBlock = parent;
            return true;
        }
        this._foundCodeBlock = null;
        return false;
    },

    callMethod: function () {
        // check the base method
        if (!CustomToolbarButton.RemoveCodeBlock.callBaseMethod(this, "callMethod")) return false;
        if (this._foundCodeBlock != null) {
            var editor = this.get_designPanel(); ;
            var editPanel = this.get_editPanel();
            var _foundCodeBlock = this._foundCodeBlock;
            setTimeout(function () {
                editor._saveContent();
                while (_foundCodeBlock.firstChild) {
                    _foundCodeBlock.parentNode.insertBefore(_foundCodeBlock.firstChild, _foundCodeBlock);
                }
                _foundCodeBlock.parentNode.removeChild(_foundCodeBlock);
                // Notify Editor about content changed and update toolbars linked to the edit panel
                setTimeout(function () { editor.onContentChanged(); editPanel.updateToolbar(); editor.focusEditor(); }, 0);
                // Ensure focus in design panel
                editor.focusEditor();
            }, 0);
            this._foundCodeBlock = null;
        }
        return true;
    }
}
// Register this class
CustomToolbarButton.RemoveCodeBlock.registerClass("CustomToolbarButton.RemoveCodeBlock", Obout.Ajax.UI.HTMLEditor.ToolbarButton.MethodButton);
Sys.Application.notifyScriptLoaded();
