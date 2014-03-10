Type.registerNamespace("CustomPopup");

CustomPopup.InsertIconPopup = function(element) {
    CustomPopup.InsertIconPopup.initializeBase(this, [element]);
}

CustomPopup.InsertIconPopup.prototype = {

    _onopened: function(contentWindow) {
        contentWindow.insertImage = Function.createDelegate(this, this.insertImage);
    },

    insertImage: function(url) {
        this.closePopup();

        var editor = this.get_designPanel();
        var editPanel = this.get_editPanel();

        try {
            // For 'Undo'
            editor._saveContent();

            // What to do - insert image at current selection
            //---------------------------------------------------
            editor.insertHTML("<img src=\"" + url + "\" />");
            //---------------------------------------------------

            // Notify Editor about content changed and update toolbars linked to the edit panel
            setTimeout(function() { editor.onContentChanged(); editPanel.updateToolbar(); }, 0);
            // Ensure focus in design panel
            editor.focusEditor();
            return true;
        } catch (e) { alert(e.message) }
    }
}

CustomPopup.InsertIconPopup.registerClass("CustomPopup.InsertIconPopup", Obout.Ajax.UI.HTMLEditor.Popups.Popup);
Sys.Application.notifyScriptLoaded();
