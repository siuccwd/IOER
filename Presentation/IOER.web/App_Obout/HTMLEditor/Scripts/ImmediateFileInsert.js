Type.registerNamespace("CustomToolbarButton");

// Declare the custom (common) button class
CustomToolbarButton.ImmediateFileInsert = function (element) {
    CustomToolbarButton.ImmediateFileInsert.initializeBase(this, [element]);
    this._uploadFolder = "";
    this._browserClassName = "";
    this._invalidFileExtensionMessage = "";
    this._availableExtensions = "";
    this._httpRoot = "";
    this._foundFileBrowser = null;
    this._relativeUrl = true;
}
CustomToolbarButton.ImmediateFileInsert.prototype = {

    // getters/setters for client-side properties (initialized from the server-side)

    get_relativeUrl: function () {
        return this._relativeUrl;
    },
    set_relativeUrl: function (value) {
        this._relativeUrl = value;
    },

    get_uploadFolder: function () {
        return this._uploadFolder;
    },
    set_uploadFolder: function (value) {
        this._uploadFolder = value;
    },

    get_browserClassName: function () {
        return this._browserClassName;
    },
    set_browserClassName: function (value) {
        this._browserClassName = value;
    },

    get_invalidFileExtensionMessage: function () {
        return this._invalidFileExtensionMessage;
    },
    set_invalidFileExtensionMessage: function (value) {
        this._invalidFileExtensionMessage = value;
    },

    get_availableExtensions: function () {
        return this._availableExtensions;
    },
    set_availableExtensions: function (value) {
        this._availableExtensions = value;
    },

    get_httpRoot: function () {
        return this._httpRoot;
    },
    set_httpRoot: function (value) {
        this._httpRoot = value;
    },

    // returns dummy Image Browser popup object
    createDummyFileBrowser: function () {
        if (this._foundFileBrowser == null) {
            // create a dummy Image Browser object
            var browserClassName = this.get_browserClassName();
            var invalidFileExtensionMessage = this.get_invalidFileExtensionMessage();
            var availableExtensions = this.get_availableExtensions().split(";");
            this._foundFileBrowser = {
                get_identity: function () {
                    return "";
                },
                get_className: function () {
                    return browserClassName;
                },
                get_availableExtensions: function () {
                    return availableExtensions;
                },
                get_invalidFileExtensionMessage: function () {
                    return invalidFileExtensionMessage;
                }
            };
        }
        return this._foundFileBrowser;
    },

    // override the base method called on button mousedown
    callMethod: function () {
        var relatedPopup = this.get_relatedPopup();
        if (relatedPopup) {
            // set the delegate of the popup init callback
            relatedPopup.initialSet = Function.createDelegate(this, this.initialSet);
        }
        // check the base method
        if (!CustomToolbarButton.ImmediateFileInsert.callBaseMethod(this, "callMethod")) return false;
        return true;
    },

    // override this base method to apply dummy FileBrowser (outer popup) object
    set_activeEditPanel: function (value) {
        var relatedPopup = this.get_relatedPopup();
        if (relatedPopup) {
            relatedPopup._outerPopup = null; // some trick
        }
        CustomToolbarButton.ImmediateFileInsert.callBaseMethod(this, "set_activeEditPanel", [value]);
        if (relatedPopup) {
            relatedPopup._outerPopup = this.createDummyFileBrowser();
        }
    },

    // called when popup is opened
    initialSet: function (contentWindow) {
        // disable 'OK' button of the popup till a file is not selected
        contentWindow.popupMediator.getHandler("OK").disable();
        // set the folder for uploading
        var folder = contentWindow.popupMediator.getField("folder");
        if (folder != null) {
            folder.value = this.get_uploadFolder();
        }
        var relatedPopup = this.get_relatedPopup();
        // set callback delegates
        if (relatedPopup) {
            relatedPopup.okCallback = Function.createDelegate(this, this.okCallback);
            relatedPopup.cancelCallback = Function.createDelegate(this, this.cancelCallback);
        }
    },

    // insert uploaded file reference
    okCallback: function (contentWindow, arrayOfNamedValues) {
        var value = [];
        var title = "";
        // get the filename
        var field = contentWindow.popupMediator.getField("location");
        if (field != null) {
            value = field.value;
            var ind = value.length - 1;
            while (ind >= 0) {
                var chr = value.substr(ind, 1);
                if (chr == "\\" || chr == "/") {
                    ind++;
                    break;
                }
                ind--;
            }
            if (ind < 0) ind = 0;
            value = [value.substr(ind)];
        } else {
            field = contentWindow.popupMediator.getField("filesarray");
            if (field != null) {
                value = field.value.split(":");
                if (value.length > 0 && !this.isMultiple()) {
                    value = [value[0]];
                }
            }
        }
        // get the title
        field = contentWindow.popupMediator.getField("description");
        if (field != null) {
            title = field.value;
        }

        var editor = this.get_designPanel();
        var editPanel = this.get_editPanel();
        // For 'Undo'
        editor._saveContent();

        // insert the file references
        for (var i = 0; i < value.length; i++) {
            var url = this.get_uploadFolder() + value[i];
            if (!this.get_relativeUrl()) {
                url = this.get_httpRoot() + url;
            }

            try {
                // What to do - insert file reference (link, img etc.) at current selection
                this.insertHtml(url, title);
            } catch (e) { alert(e.message) }
        }
        // Notify Editor about content changed and update toolbars linked to the edit panel
        setTimeout(function () { editor.onContentChanged(); editPanel.updateToolbar(); }, 0);
        // Ensure focus in design panel
        editor.focusEditor();
    },

    // insert HTML code
    insertHtml: function (url, title) {
    },

    // whether multiple files are allowed
    isMultiple: function () {
        return true;
    },

    // uploading canceled
    cancelCallback: function (contentWindow, arrayOfNamedValues) {
    }
}
// Register this class
CustomToolbarButton.ImmediateFileInsert.registerClass("CustomToolbarButton.ImmediateFileInsert", Obout.Ajax.UI.HTMLEditor.ToolbarButton.OpenPopupButton);

// Declare the custom button class
CustomToolbarButton.ImmediateImageInsert = function (element) {
    CustomToolbarButton.ImmediateImageInsert.initializeBase(this, [element]);
}
CustomToolbarButton.ImmediateImageInsert.prototype = {

    // override the base method - whether to enable the button
    onEditPanelActivity: function () {
        if (!CustomToolbarButton.ImmediateImageInsert.callBaseMethod(this, "onEditPanelActivity")) return false;
        var editor = this._designPanel;
        if (!editor.isControl()) {
            this.enable();
            return true;
        } else { // Image or some other object is selected - disable
            this.disable();
            return false;
        }
    },

    // insert HTML code
    insertHtml: function (url, title) {
        this.get_designPanel().insertHTML("<img src=\"" + url + "\" title=\"" + title + "\" />");
    }
}
// Register this class
CustomToolbarButton.ImmediateImageInsert.registerClass("CustomToolbarButton.ImmediateImageInsert", CustomToolbarButton.ImmediateFileInsert);

// Declare the custom button class
CustomToolbarButton.ImmediateDocumentInsert = function (element) {
    CustomToolbarButton.ImmediateDocumentInsert.initializeBase(this, [element]);
}
CustomToolbarButton.ImmediateDocumentInsert.prototype = {

    // override the base method - whether to enable the button
    onEditPanelActivity: function () {
        if (!CustomToolbarButton.ImmediateDocumentInsert.callBaseMethod(this, "onEditPanelActivity")) return false;
        var editor = this._designPanel;
        if (Obout.Ajax.UI.HTMLEditor.testLink(this._designPanel).canBeCreated) {
            this.enable();
            return true;
        } else { // Nothing selected - disable
            this.disable();
            return false;
        }
    },

    // whether multiple files are allowed
    isMultiple: function () {
        return false;
    },

    // insert HTML code
    insertHtml: function (url, title) {
        this.get_designPanel().surroundHTML("<a href=\"" + url + "\" title=\"" + title + "\" >", "</a>");
    }
}
// Register this class
CustomToolbarButton.ImmediateDocumentInsert.registerClass("CustomToolbarButton.ImmediateDocumentInsert", CustomToolbarButton.ImmediateFileInsert);


Sys.Application.notifyScriptLoaded();
