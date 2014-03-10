Type.registerNamespace("CustomPopup");

CustomPopup.CustomImageBrowser1 = function (element) {
    CustomPopup.CustomImageBrowser1.initializeBase(this, [element]);
    // set the delegate
    this._customaction1$delegate = Function.createDelegate(this, this._customaction1);
}

CustomPopup.CustomImageBrowser1.prototype = {

    // override method called on popup (ImageBrowser) open
    onReload: function (contentWindow) {
        // call the base method
        CustomPopup.CustomImageBrowser1.callBaseMethod(this, "onReload", [contentWindow]);

        // get the handler for our custom button
        this._customaction1$handler = contentWindow.popupMediator.getHandler("customaction1");
        // set the method to be called on the button's click
        contentWindow.popupMediator.setCallMethodByName("customaction1", this._customaction1$delegate);
    },

    // this method is called to check whether the toolbar buttons be enabled or disabled
    checkButtons: function () {
        // call the base method (default buttons are checked)
        CustomPopup.CustomImageBrowser1.callBaseMethod(this, "checkButtons");

        // check our custom button
        if (this._customaction1$handler != null) {
            var toDisable = true;
            if (this._currentItem != null) {
                if (!this._currentItem.isFolder) { // for images only, not for folders
                    if (this.getParent(this._currentItem) != null) { // if there is a parent folder
                        toDisable = false;
                    }
                }
            }
            if (toDisable) {
                this._customaction1$handler.disable();
            } else {
                this._customaction1$handler.enable();
            }
        }
    },

    // action on our custom button click
    _customaction1: function () {
        // get the currently selected node
        var item = this._currentItem;
        // get the file's URL
        var url = (item.virtualpath != null && typeof item.virtualpath != "undefined") ? item.virtualpath : item.url;

        /////////////////////////////////////////////////////////////
        // Here you can do everything you want with the file's URL //
        /////////////////////////////////////////////////////////////
        alert("The following image is selected:\n"+url);
    }
}

// register our custom ImageBrowser
CustomPopup.CustomImageBrowser1.registerClass("CustomPopup.CustomImageBrowser1", Obout.Ajax.UI.HTMLEditor.Popups.ImageBrowser);
Sys.Application.notifyScriptLoaded();
