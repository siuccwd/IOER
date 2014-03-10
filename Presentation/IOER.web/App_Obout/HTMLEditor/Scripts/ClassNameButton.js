Type.registerNamespace("CustomToolbarButton");

// Declare the custom button class
CustomToolbarButton.ClassNameSelector = function (element) {
    CustomToolbarButton.ClassNameSelector.initializeBase(this, [element]);
}
CustomToolbarButton.ClassNameSelector.prototype = {
    _onmousedown: function (e) {
        // Trick for IE - against selection loosing
        if (Obout.Ajax.UI.HTMLEditor.isIE) {
            var designPanel = this.get_designPanel();
            var range = designPanel._getSelection().createRange();
            if (range.duplicate) { // text selected or none
                designPanel.__mySavedCurrentRange = range.duplicate();
            } else { // some object selected
                var rng = designPanel.get_doc().body.createControlRange();
                rng.add(range.item(0));
                designPanel.__mySavedCurrentRange = rng;
            }
        }
        return CustomToolbarButton.ClassNameSelector.callBaseMethod(this, "_onmousedown", [e]);
    },

    onEditPanelActivity: function () {
        if (!CustomToolbarButton.ClassNameSelector.callBaseMethod(this, "onEditPanelActivity")) return false;
        var relatedPopup = this.get_relatedPopup();
        if (relatedPopup && relatedPopup.handlers) {
            if (relatedPopup.availableItems().length == 0) {
                this.disable();
                return false;
            }
        }
        this.enable();
        return true;
    }
}
// Register this class
CustomToolbarButton.ClassNameSelector.registerClass("CustomToolbarButton.ClassNameSelector", Obout.Ajax.UI.HTMLEditor.ToolbarButton.OpenPopupButton);

CustomToolbarButton.ClassNameField = function (element) {
    CustomToolbarButton.ClassNameField.initializeBase(this, [element]);
    this._innerCell = null;
    this._selectorID = null;
    this.get_element$delegate = null;
}

CustomToolbarButton.ClassNameField.prototype = {

    get_innerCell: function () {
        return this._innerCell;
    },
    set_innerCell: function (value) {
        this._innerCell = value;
    },

    get_selectorID: function () {
        return this._selectorID;
    },
    set_selectorID: function (value) {
        this._selectorID = value;
    },

    isImage: function () {
        return false;
    },

    onEditPanelActivity: function () {
        if (!CustomToolbarButton.ClassNameField.callBaseMethod(this, "onEditPanelActivity")) return false;
        if (!this.checkRangeInDesign()) return false;
        var cell = this.get_innerCell();
        cell.innerHTML = "";
        var editor = this._designPanel;
        var selector = $find(this.get_selectorID());
        if (selector) {
            if (this.get_element$delegate == null) {
                this.get_element$delegate = Function.createDelegate(this, this.get_element);
                selector.get_attachedElement = this.get_element$delegate;
            }
            var relatedPopup = selector.get_relatedPopup();
            if (relatedPopup && relatedPopup.handlers) {
                var availableItems = relatedPopup.availableItems();
                if (availableItems.length > 0) {
                    for (var i = 0; i < availableItems.length; i++) {
                        var item = availableItems[i].get_styleItem();
                        if (item.className.indexOf("item_active") >= 0) {
                            while (cell.firstChild) cell.removeChild(cell.firstChild);
                            var div = document.createElement("div");
                            div.style.width = cell.clientWidth + "px";
                            div.style.height = cell.clientHeight + "px";
                            div.style.marginLeft = "-1px";
                            div.style.marginTop = "-1px";
                            div.innerHTML = item.innerHTML;
                            var table = div.firstChild;
                            while (table.nodeType != 1) table = table.nextSibling;
                            table.rows.item(0).cells.item(0).firstChild.align = "absMiddle";
                            table.rows.item(0).cells.item(1).firstChild.style.verticalAlign = "middle";
                            div.innerHTML = table.rows.item(0).cells.item(0).innerHTML + table.rows.item(0).cells.item(1).innerHTML;
                            cell.appendChild(div);
                            var component = $find(availableItems[i].get_styleItemID());
                            if (component && component.get_className().length == 0) {
                                continue; // empty class name - try to find more
                            }
                            break; // allow to show the first class only
                        }
                    }
                    return true;
                }
            }
        }
        return false;
    }
}

CustomToolbarButton.ClassNameField.registerClass("CustomToolbarButton.ClassNameField", Obout.Ajax.UI.HTMLEditor.ToolbarButton.DesignModeBoxButton);

Sys.Application.notifyScriptLoaded();
