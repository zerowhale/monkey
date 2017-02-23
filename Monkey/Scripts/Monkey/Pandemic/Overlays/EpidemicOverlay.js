function EpidemicOverlay(display) {

    Overlay.call(this, display);

    this.steps = this.display.find(".step");



    this.activateStep = function (step, myTurn) {
        var stepDisplay = this.setStep(step, myTurn);
        stepDisplay.addClass("active");
        stepDisplay.addClass("activated");
        setTimeout(function () {
            stepDisplay.removeClass("activated");
        }, 1000);

    }

    this.setStep = function (step, myTurn) {
        console.info("Set Step", step);
        this.steps.removeClass("active").removeClass("activated");
        this.steps.find(".title").removeClass("button");
        var stepDisplay = this.steps.filter(".step" + step);
        stepDisplay.addClass("active");

        if (myTurn) {

            switch (step) {
                case 1:
                    var title = stepDisplay.find(".title");
                    title.addClass("button");
                    title.one("click", function () {
                        gameConn.server.epidemicInfect();
                        title.removeClass("button");
                    });
                    break;
                case 2:
                    var title = stepDisplay.find(".title");
                    title.addClass("button");
                    title.one("click", function () {
                        gameConn.server.epidemicIntensify();
                        title.removeClass("button");
                    });
                    break;

            }
        }
        return stepDisplay;
    }

    this.takeAction = function () {
        this.display.find(".button").trigger("click");
    }

}
$(document).ready(function () {
    extend(Overlay, EpidemicOverlay);
})
