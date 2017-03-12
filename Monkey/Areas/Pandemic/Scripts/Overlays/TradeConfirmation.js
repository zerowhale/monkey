function TradeConfirmationOverlay(display) {
    Overlay.call(this, display, {
        collapsable: false
    });

    var playersDisplay = display.find(".players"),
        cardDisplay = display.find(".cards-display"),
        titleDisplay = display.find(".title"),
        acceptButton = display.find(".button.accept"),
        rejectButton = display.find(".button.reject"),
        rejectedDisplay = display.find(".rejected");

    this.rejected = function () {
        rejectedDisplay.show();
        setTimeout(function () {
            OverlayManager.hideOverlay(OverlayManager.overlays.tradeConfirmation);
        }, 1250);
    }


    this.setTradeOptions = function (tradeData) {
        var obj = this;
        cardDisplay.empty();
        rejectedDisplay.hide();

        if (profile.name != tradeData.Player1 && profile.name != tradeData.Player2) {
            titleDisplay.text("Waiting for " + tradeData.Player1 + " and " + tradeData.Player2 + " to complete a trade for:");
            this.display.addClass("collapsed");
            rejectButton.hide();
            acceptButton.hide();
        }
        else {
            this.display.removeClass("collapsed");
            if (profile.name != tradeData.Player1) {
                titleDisplay.text("Give this card to " + tradeData.Player2 + "?");
            }
            else {
                titleDisplay.text("Take this card from " + tradeData.Player2 + "?");
            }
            rejectButton.show();
            acceptButton.show();
        }
        

        var card = Game.getPlayerCard(tradeData.Card);
        console.info(card);
        card.display.addClass("flipped");
        cardDisplay.append(card.display);
        card.display.css("left", (51.5 - 6) + "vw");

        

    }

    {
        acceptButton.on("click", function () {
            gameConn.server.acceptTrade();
        });

        rejectButton.on("click", function () {
            gameConn.server.rejectTrade();
        });
    }
}

$(document).ready(function () {
    extend(Overlay, TradeConfirmationOverlay);
});
