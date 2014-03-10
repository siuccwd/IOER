Type.registerNamespace("CustomPopup");

CustomPopup.ClassNamesPopup = function (element) {
    CustomPopup.ClassNamesPopup.initializeBase(this, [element]);
    this.handlers = null;
    this._forceUpdateToolbar = false;
    this._clicked = false;
}

CustomPopup.ClassNamesPopup.prototype = {
    close: function () {
        if (Obout.Ajax.UI.HTMLEditor.isIE && !this._clicked) {
            var designPanel = this.get_designPanel();
            designPanel.__mySavedCurrentRange = null;
            this._clicked = false;
        }
        CustomPopup.ClassNamesPopup.callBaseMethod(this, "close");
    },

    onReload: function (contentWindow) {
        this._contentWindow = contentWindow;
        if (this.handlers == null) {
            this.handlers = [];
            var classesNumber = 0;
            while (true) {
                var handler = contentWindow.popupMediator.getHandler("button_" + classesNumber.toString());
                if (handler != null) {
                    this.handlers.push(handler);
                    classesNumber++;
                } else {
                    break;
                }
            }
        }
        this.availableItems();
        if (this._forceUpdateToolbar) {
            this.get_editPanel().updateToolbar();
            this._forceUpdateToolbar = false;
        }
        this._clicked = false;
    },

    availableItems: function () {
        var designPanel = this.get_designPanel(); // 'DesignPanel' component
        var contentWindow = this._contentWindow;
        var classes = [];
        if (designPanel && contentWindow) {
            // Trick for IE - against selection loosing
            if (Obout.Ajax.UI.HTMLEditor.isIE && typeof designPanel.__mySavedCurrentRange != "undefined" && designPanel.__mySavedCurrentRange != null) {
                designPanel.__mySavedCurrentRange.select();
            }
            var editPanel = this.get_editPanel(); // 'EditPAnel' component
            for (var i = 0; i < this.handlers.length; i++) {
                var handler = this.handlers[i];
                handler.set_designPanel(designPanel);
                handler.set_editPanel(editPanel);
                handler.set_owner(this);
                if (handler.setActivity()) {
                    handler.callMethod = Function.createDelegate(handler, handler._styleSelected);
                    classes.push(handler);
                }
            }
        }
        return classes;
    }
}
CustomPopup.ClassNamesPopup.registerClass("CustomPopup.ClassNamesPopup", Obout.Ajax.UI.HTMLEditor.Popups.Popup);


CustomPopup.ClassNameItem = function (element) {
    CustomPopup.ClassNameItem.initializeBase(this, [element]);
    this._designPanel = null;
    this._editPanel = null;
    this._owner = null;
    this._styleItemID = "";
    this._styleItem = null;
}

CustomPopup.ClassNameItem.prototype = {
    get_designPanel: function () {
        return this._designPanel;
    },
    set_designPanel: function (value) {
        this._designPanel = value;
    },

    get_editPanel: function () {
        return this._editPanel;
    },
    set_editPanel: function (value) {
        this._editPanel = value;
    },

    get_owner: function () {
        return this._owner;
    },
    set_owner: function (value) {
        this._owner = value;
    },

    get_styleItemID: function () {
        return this._styleItemID;
    },
    set_styleItemID: function (value) {
        this._styleItemID = value;
    },
    get_styleItem: function () {
        return this.get_owner()._contentWindow.document.getElementById(this.get_styleItemID());
    },

    setActivity: function () {
        if (!this.get_owner()._contentWindow.document) {
            if (!this.get_owner().isLoading) {
                this.get_owner().reload();
                this.get_owner()._forceUpdateToolbar = true;
            }
            return false;
        }
        var styleItem = $find(this.get_styleItemID());
        var retval = false;
        if (styleItem) {
            styleItem.set_activeEditPanel(this.get_editPanel());
            var savedElement = styleItem.get_element();
            styleItem._element = this.get_styleItem();
            if (styleItem.onEditPanelActivity()) {
                //this.enable();
                retval = true;
            } else {
                //this.disable();
                this._activated.style.display = "none";
            }
            styleItem._element = savedElement;
        }
        return retval;
    },

    _styleSelected: function () {
        var me = this;
        setTimeout(function () {
            var styleItem = $find(me.get_styleItemID());
            if (styleItem) {
                var designPanel = me.get_designPanel();
                // Trick for IE - against selection loosing
                if (Obout.Ajax.UI.HTMLEditor.isIE && typeof designPanel.__mySavedCurrentRange != "undefined" && designPanel.__mySavedCurrentRange != null) {
                    designPanel.__mySavedCurrentRange.select();
                    designPanel.__mySavedCurrentRange = null;
                }
                styleItem.callMethod();
                // Trick for IE - against selection loosing
                if (Obout.Ajax.UI.HTMLEditor.isIE) {
                    var range = designPanel._getSelection().createRange();
                    if (range.duplicate) { // text selected or none
                        designPanel.__mySavedCurrentRange = range.duplicate();
                    } else { // some object selected
                        var rng = designPanel.get_doc().body.createControlRange();
                        rng.add(range.item(0));
                        designPanel.__mySavedCurrentRange = rng;
                    }
                }
            }
            me.get_owner()._clicked = true;
            me.get_owner().closePopup();
            // Trick for IE - against selection loosing
            if (Obout.Ajax.UI.HTMLEditor.isIE && typeof designPanel.__mySavedCurrentRange != "undefined" && designPanel.__mySavedCurrentRange != null) {
                designPanel.__mySavedCurrentRange.select();
                designPanel.__mySavedCurrentRange = null;
            }
        }, 0);
    },

    initialize: function () {
        CustomPopup.ClassNameItem.callBaseMethod(this, "initialize");
    },
    activate: function (element) {
        CustomPopup.ClassNameItem.callBaseMethod(this, "activate", [element]);
        //this.disable();
    }
}
CustomPopup.ClassNameItem.registerClass("CustomPopup.ClassNameItem", Obout.Ajax.UI.HTMLEditor.Popups.PopupBoxButton);


Sys.Application.notifyScriptLoaded();
