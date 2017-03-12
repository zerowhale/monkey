function FinishedOverlay(display) {
    Overlay.call(this, display, {
        collapsable: false
    });

    var outcomeDisplay = display.find(".outcome"),
        messageDisplay = display.find(".message"),
        detailsDisplay = display.find(".details");

    this.setInfo = function(outcome, message, details){
        outcomeDisplay.text(outcome);
        messageDisplay.text(message);
        detailsDisplay.text(details);
    }


}

$(document).ready(function () {
    extend(Overlay, FinishedOverlay);
});
