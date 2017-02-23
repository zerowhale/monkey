function TradeOverlay(display) {
    Overlay.call(this, display, {
        collapsable: false
    });

    var playersDisplay = display.find(".players"),
        cardDisplay = display.find(".cards-display"),
        titleDisplay = display.find(".title"),
        submitButton = display.find(".button"),
        cards,
        ids,
        playerTemplate = playersDisplay.find(".player").remove();


    this.setTradeOptions = function (list) {
        var obj = this;
        cards = [];

        for (var p in list) {
            var player = list[p].player;
            var cards = list[p].cards;

            for (var c = cards.length - 1; c >= 0; c--){
                var id = cards[c];
                if (id < 50) 
                    cards[c] = Game.getPlayerCard(id);
                else
                    cards.splice(c, 1);
            }
        }

        titleDisplay.text("Choose a card to trade.");

        playersDisplay.empty();
        var first = true;
        for (var p = 1; p < list.length; p++) {
            var player = list[p].player;
            var playerDisplay = playerTemplate.clone();
            playerDisplay.find(".name").text(player.Name);
            playerDisplay.find(".role").text(player.Role.Name);
            playerDisplay.attr("data-color", player.Color);

            if (first) {
                first = false;
                playerDisplay.addClass("selected");
                displayCards(list[0], list[p]);
            }
            playersDisplay.append(playerDisplay);
            (function (list1, list2) {
                playerDisplay.click(function () {
                    var cur = $(this);
                    if (!cur.hasClass("selected")) {
                        playersDisplay.find(".player").removeClass("selected");
                        cur.addClass("selected");
                        displayCards(list1, list2);
                    }
                })
            })(list[0], list[p]);
        }

        function displayCards(mine, others) {
            cardDisplay.empty();
            var num = mine.cards.length + others.cards.length,
                centerFor = 6;
            
            if (num > centerFor) num = centerFor;
            var count = 0;

            for (var c = 0; c < mine.cards.length; c++) {
                var card = mine.cards[c];
                addCard(card, count);
                count++;
            }
            
            for (var c = 0; c < others.cards.length; c++) {
                var card = others.cards[c];
                addCard(card, count);
                count++;
            }
            
            function addCard(card, index) {
                card.display.addClass("flipped");
                cardDisplay.append(card.display);
                card.display.css("left", "110vw");
                var spacing = 9.5;

                var left = index < 6
                    ? (51 + ((index - num / 2) * spacing))
                    : 51 + (centerFor  /  2 * spacing) + (index - centerFor) * spacing;
                card.display.css("left", left + "vw");
                card.display.off("click");
                card.display.on("click", function () {
                    cardDisplay.find(".card").removeClass("selected");
                    $(this).addClass("selected");
                })
            }

        }
    }


    function submit() {
        OverlayManager.hideOverlay(OverlayManager.overlays.trade);
        var player = playersDisplay.find(".selected .name").text();
        var card = cardDisplay.find(".selected").attr("data-id");
        gameConn.server.requestTrade(player, card);
    }

    {
        submitButton.on("click", function () {
            submit();
        })
    }
}

$(document).ready(function () {
    extend(Overlay, TradeOverlay);
});
