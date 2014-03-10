Type.registerNamespace("CustomToolbarButton");

// Declare the custom button class
CustomToolbarButton.InsertDate = function(element) {
    CustomToolbarButton.InsertDate.initializeBase(this, [element]);
}
CustomToolbarButton.InsertDate.prototype = {
    callMethod: function() {
        // check the base method
        if (!CustomToolbarButton.InsertDate.callBaseMethod(this, "callMethod")) return false;
        var editor = this.get_designPanel(); // 'DesignPanel' component
        var editPanel = this.get_editPanel(); // 'EditPAnel' component

        try {
            // For 'Undo'
            editor._saveContent();
            // What to do - insert date at current selection
            editor.insertHTML((new Date()).toLocaleDateString());
            // Notify Editor about content changed and update toolbars linked to the edit panel
            setTimeout(function() { editor.onContentChanged(); editPanel.updateToolbar(); }, 0);
            // Ensure focus in design panel
            editor.focusEditor();
        } catch (e) { }
        return true;
    }
}
// Register this class
CustomToolbarButton.InsertDate.registerClass("CustomToolbarButton.InsertDate", Obout.Ajax.UI.HTMLEditor.ToolbarButton.MethodButton);
Sys.Application.notifyScriptLoaded();
