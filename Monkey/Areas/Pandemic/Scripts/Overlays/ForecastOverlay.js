function ForecastOverlay(display) {
    Overlay.call(this, display, {
       
    });

    var cardDisplay = display.find(".cards ul"),
        titleDisplay = display.find(".title"),
        submitButton = display.find(".button"),
        cancelButton = display.find(".cancel"),
        cards;


    this.setCards = function (cardList, params) {
        console.info(params);
        var controls = false,
            obj = this;

        if (params) {
            if (params.cancellable) {
                control = true;

                if (typeof (params.onCancel) == "function") {
                    cancelButton.off("click");
                    cancelButton.on("click", function () {
                        obj.hide();
                        params.onCancel();
                    })
                }
            }
        }

        cardDisplay.sortable(controls ? "enable" : "disable");
        submitButton.toggle(controls);
        this.display.toggleClass("controllable", controls);

        cardDisplay.empty();
        cards = [];

        for (var c = 0; c < cardList.length; c++) {
            var card = cardList[c].clone();
            card.display.addClass("hor");
            cards.push(card);
            console.info(card);
            var wrapper = $("<li></li>").append(card.display);
            cardDisplay.append(wrapper);

        }


    }

    this.updateOrder = function (ids) {
        console.info(">", ids);
        var elements = cardDisplay.find("li");
        cardDisplay.empty();
        for (var i in ids) {
            var id = ids[i];
            var display = elements.find(".card[data-id=" + id + "]");
            cardDisplay.append(display.parent());
        }
    }

    {

        cardDisplay.sortable({
            stop: function (e, ui) {

                console.info(e, ui);
                var cards = cardDisplay.find(".card"),
                    ids = cards.map(function () { return $(this).attr("data-id"); });
                
                gameConn.server.reorderForecastCards(ids[0], ids[1], ids[2], ids[3], ids[4], ids[5]);
            }
        });
        
        submitButton.on("click", function () {
            var ids = cardDisplay.find(".card").map(function () { return $(this).attr("data-id"); });
            gameConn.server.executeForecast(ids[0], ids[1], ids[2], ids[3], ids[4], ids[5]);
        })
    }
}

$(document).ready(function () {
    extend(Overlay, ForecastOverlay);
});
