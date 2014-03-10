Type.registerNamespace("CustomPopup");

CustomPopup.InsertCodeBlockPopup = function (element) {
    CustomPopup.InsertCodeBlockPopup.initializeBase(this, [element]);
}

CustomPopup.InsertCodeBlockPopup.prototype = {

    _onopened: function (contentWindow) {
        contentWindow.insertCodeBlock = Function.createDelegate(this, this.insertCodeBlock);
    },

    insertCodeBlock: function (cell) {
        this.closePopup();

        var editor = this.get_designPanel(); // 'DesignPanel' component
        var editPanel = this.get_editPanel(); // 'EditPAnel' component
        var className = "prettyprint lang-" + Obout.Ajax.UI.HTMLEditor.TrimAll(cell.innerHTML).toLowerCase();
        var leftHTML = "<pre class=\"" + className + "\" id=\"MyTempElementIdName\">";
        var rightHTML = "</pre>";
        var range;

        try {
            // For 'Undo'
            editor._saveContent();
            var sel = editor._getSelection();
            var range = editor._createRange(sel);
            // What to do - surround selected text
            editor.surroundHTML(leftHTML, rightHTML);
            // Just a moment later
            setTimeout(function () {
                // Notify Editor about content changed
                editor.onContentChanged();
                // Update toolbars linked to the edit panel
                editPanel.updateToolbar();
                // Ensure focus in design panel
                editor.focusEditor();
                // find our 'PRE' element
                var pre = editor.get_doc().getElementById("MyTempElementIdName");
                // some selection things for IE
                if (Obout.Ajax.UI.HTMLEditor.isIE) {
                    var sel = editor._getSelection();
                    var range = editor._createRange(sel);
                    range.moveToElementText(pre);
                    range.select();
                }
                // remove ID from the PRE
                try { pre.id = null; } catch (ex) { }
                try { pre.removeAttribute("id"); } catch (ex) { }
            }, 0);
            // Ensure focus in design panel
            editor.focusEditor();
            return true;
        } catch (e) { alert(e.message) }
    }
}

CustomPopup.InsertCodeBlockPopup.registerClass("CustomPopup.InsertCodeBlockPopup", Obout.Ajax.UI.HTMLEditor.Popups.Popup);
Sys.Application.notifyScriptLoaded();
