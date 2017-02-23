function Popup(display) {
    var obj = this;
    this.display = display;

    this.okButton = display.find(".ok-button .button");
    this.submitButton = display.find(".submit-button .button");
    this.cancelButton = display.find(".cancel-button .button");

    this.cancelButton.click(function () {
        obj.fireEvent("cancel");
    });

    this.submitButton.click(function(){
        obj.fireEvent("submit");
    });

    this.okButton.click(function () {
        obj.fireEvent("ok");
    });
}
Popup.prototype = {
    display: null,
    actionId: null,
    player: null,
    submitButton: null,
    cancelButton: null,
    visible: false,

    show: function () {
        var w = $(window);
        if (this.display) {
            this.display.show();
            var o = {
                left: w.width() / 2 - this.display.width() / 2,
                top: w.height() / 2 - this.display.height() / 2
            };
            this.display.offset(o);
            this.visible = true;
        }
    },

    hide: function () {
        this.display.hide();
        this.visible = false;
    },

    cancel: function (event) {
        this.bindEvent("cancel", event);
    },

    submit: function (event) {
        this.bindEvent("submit", event);
    },

    ok: function (event) {
        this.bindEvent("ok", event);
    }
};
enableEvents(Popup);





















