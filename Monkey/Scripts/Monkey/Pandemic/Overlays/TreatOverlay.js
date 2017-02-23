function TreatOverlay(display) {
    Overlay.call(this, display, {
            collapsable: false
        });

    var step = this.display.find(".step");
    var diseaseDisplay = step.find(".diseases");
    var cityDisplay = this.display.find(".city");
    var cityName = this.display.find(".city-name");
    var submitButton = this.display.find(".button");
    var city;
    var treatmentsRemaining = 0;

    this.setCity = function (cityData) {
        city = cityData;
        this.display.find(".step").attr("data-title", "Treat Diseases in " + cityData.Name);
        cityDisplay.attr("data-color", city.Color);

        var data = city.DiseaseCounters;
        var count = 0;

        treatmentsRemaining = Game.actionsRemaining;
        for (var x = 0; x < 4; x++) {
            if (x == 0) color = "Yellow";
            else if (x == 1) color = "Red";
            else if (x == 2) color = "Blue";
            else if (x == 3) color = "Black";

            var diseaseDisplays = diseaseDisplay.find(".disease[data-color=" + color + "]");

            for (var d = 0; d < 3; d++) {
                count++;
                var left = Math.sin(count * Math.PI / 6 - Math.PI / 12) * 6;
                var top = Math.cos(count * Math.PI / 6 - Math.PI / 12) * 6;
                var disease = $(diseaseDisplays[d]);
                disease.css("left", (left -1.25)+ "vw");
                disease.css("top", (top - 1.25) + "vw");
                if (data[x] <= d)
                    disease.hide();
                else
                    disease.show();

            }
        }
        updateCountDisplay();

    }

    function updateCountDisplay() {

        if (treatmentsRemaining == 0) {
            step.addClass("no-moves");
            message = "No actions left"
        }
        else {
            step.removeClass("no-moves");
            message = "Cure up to <div>" + treatmentsRemaining + "</div> disease";
            if (treatmentsRemaining > 1)
                message += "s";

        }

        cityDisplay.find(".actions").html(message);
    }

    
    {
        var obj = this;
        diseaseDisplay.find(".disease").click(function () {
            var node = $(this);
            var color = node.attr("data-color");
            var isMedic = Game.players[Game.activePlayer].Role.Name == PlayerRole.Medic;
            var isCured = Game.isCured(color);
            if (isMedic || isCured) {
                node = diseaseDisplay.find(".disease[data-color=" + color + "]");
            }
            if (node.hasClass("selected")) {
                node.removeClass("selected");
                treatmentsRemaining++;
            }
            else if (treatmentsRemaining > 0) {
                node.addClass("selected");
                treatmentsRemaining--;
            }

            if (Game.actionsRemaining - treatmentsRemaining > 0)
                submitButton.show();
            else
                submitButton.hide();

            updateCountDisplay();
        })

        submitButton.on("click", function () {
            var selected = diseaseDisplay.find(".selected");
            var values = [];
            selected.each(function () {
                var val = $(this).attr("data-color");
                values.push(DiseaseColor[val]);
            });
            console.info(values);
            selected.removeClass("selected");
            gameConn.server.treatDiseases(values);
            obj.hide();
        })
    }

}

$(document).ready(function () {
    extend(Overlay, TreatOverlay);
});
