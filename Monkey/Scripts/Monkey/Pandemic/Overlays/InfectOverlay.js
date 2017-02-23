function InfectOverlay(display) {
    Overlay.call(this, display);

    var activeStep = 0;
    var stepsContainer = this.display.find(".steps");
    var stepTemplate = stepsContainer.find(".step").remove();
    var steps = null;

    this.setStepCount = function (stepCount) {
        stepsContainer.empty();
        for (var i = 0; i < stepCount; i++) {
            var step = stepTemplate.clone();
            step.addClass("step" + i);
            stepsContainer.append(step);
        }
        steps = stepsContainer.find(".step");
    }

    this.activateStep = function (step, myTurn) {
        var stepDisplay = this.setStep(step, myTurn);
        activeStep = step;
        stepDisplay.addClass("active");
        stepDisplay.addClass("activated");
        setTimeout(function () {
            stepDisplay.removeClass("activated");
        }, 1000);
    }


    this.setStep = function (step, myTurn) {
        console.info("my turn:", myTurn);
        steps.removeClass("active").removeClass("activated");
        steps.find(".title").removeClass("button");
        var stepDisplay = steps.filter(".step" + step);
        stepDisplay.addClass("active");

        if (myTurn) {
            var title = stepDisplay.find(".title");
            title.addClass("button");
            title.one("click", function () {
                gameConn.server.infect();
                title.removeClass("button");
            });

        }
        return stepDisplay;
    }

    this.takeAction = function () {
        this.display.find(".button").trigger("click");
    }

}
$(document).ready(function () {
    extend(Overlay, InfectOverlay);
});
