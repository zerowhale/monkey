function CardSelectionOverlay(display) {
    Overlay.call(this, display, {
        collapsable: false
    });

    var cardDisplay = display.find(".cards-display"),
        titleDisplay = display.find(".title"),
        submitButton = display.find(".button"),
        hideCallback, successCallback, cancelCallback,
        cards,
        ids,
        selectionRequirement;

    this.show = function () {
        submitButton.hide();
        Overlay.prototype.show.call(this);
    }

    this.hide = function () {
        var obj = this;
        var delay = 0;
        if (cards.length > 0) {
            delay = 250;
            for (var c in cards) {
                cards[c].display.css({
                    "transition": "left .2s",
                    "left": "110vw"});
            }
        }

        setTimeout(function () {
            animateOut();
        }, delay);

        function animateOut(){
            Overlay.prototype.hide.call(obj);
            if (typeof (hideCallback) == "function")
                hideCallback(ids);
        }
    }
    
    this.setCards = function (cardList, selectionReq, title, success, cancel, canPlayEventCards) {
        successCallback = success;
        cancelCallback = cancel;
        selectionRequirement = selectionReq;
        var obj = this;
        cardDisplay.empty();
        cards = [];
        hideCallback = cancel;
        var num = cardList.length > 9 ? 9 : cardList.length;

        titleDisplay.text(title);

        for (var c = 0; c < cardList.length; c++) {
            var card = cardList[c].clone();
            cards.push(card);

            // hook up event card buttons
            if (card.id >= 50 && canPlayEventCards) {
                var btn = $("<div class='button'>Play</div>");
                card.display.append(btn);
                (function (card, btn) {
                    btn.on("click", function (e) {
                        e.stopPropagation();

                        // Cheese fix for one quiet night
                        setTimeout(function () {
                            gameConn.server.playEventCard(card.id);
                        }, card.id == 50 ? 500 : 0);

                        if (card.id == 50)
                            obj.hide();
                    });
                })(card,btn);
            }

            card.display.removeClass("disabled");
            card.display.addClass("flipped");
            cardDisplay.append(card.display);
            card.display.css("left", "110vw");
            (function (card, left) {
                setTimeout(function () {
                    card.display.css("left",left + "vw");
                }, 220);
            })(card, 50 + ((c - num / 2) * 11));

            card.display.on("click", function () {
                $(this).toggleClass("selected");
                checkSelection()
            })
        }


        function checkSelection() {
            var color = null,
                count = 0,
                selected = cardDisplay.find(".selected");

            ids = [];

            selected.each(function () {
                var id = $(this).attr("data-id");
                var city = Game.cityData[id];
                if (selectionReq >= 4) {
                    if (!city)
                        return;

                    if (color == null)
                        color = city.Color;
                    else if (color != city.Color)
                        return;
                }
                ids.push(id);
                count++;
            });

            if (count == selectionReq)
                submitButton.show();
            else
                submitButton.hide();
        }
    }

    function submit() {
        OverlayManager.hideOverlay(OverlayManager.overlays.cardSelection);
    }

    {
        submitButton.on("click", function () {
            hideCallback = successCallback;
            submit();
        })
    }
}

$(document).ready(function () {
    extend(Overlay, CardSelectionOverlay);
});
