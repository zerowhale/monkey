function MessageOverlay(display) {
    Overlay.call(this, display, {
        collapsable: false
    });

    var titleDisplay = display.find(".title"),
        cancelButton = display.find(".cancel");

    this.setTitle = function (title, params) {
        var obj = this;
        titleDisplay.text(title);
        cancelButton.hide();
        if (params) {
            if (params.cancellable) {
                cancelButton.show();
            }

            if (typeof(params.onCancel) == "function") {
                cancelButton.off("click");
                cancelButton.on("click", function () {
                    obj.hide();
                    params.onCancel();
                })
            }
        }
    }
}

$(document).ready(function () {
    extend(Overlay, MessageOverlay);
});
