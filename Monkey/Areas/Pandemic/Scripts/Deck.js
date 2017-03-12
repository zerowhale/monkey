function Deck(display, location, deck, orientation, viewable, zIndexBase) {
    this.display = display;
    this.orientation = orientation || Orientation.Vertical;
    this.viewable = viewable || false;
    this.cards = [];
    this.flipCardTemplate = null;
    this.floatersDisplay = null;

    this.selectionMode = false;

    this.setCards = function (cards) {
        this.cards = [];
        for (var c = 0; c < cards.length; c++){
            this.addCard(cards[c]);
        }
    }

    this.addCard = function (cardData, forceFaceDown) {
        var card;
        if (cardData instanceof Card) {
            card = cardData;
        }
        else
            card = new Card(cardData.id, cardData.key, deck);

        this.cards[this.cards.length] = card;

        if (!(card.display.parent()[0] == this.display[0])) {
            this.display.append(card.display);
        }

        var zIndex = zIndexBase || 0;
        var i = this.cards.length;
        card.display.css("left", location.left + "vw");
        card.display.css("top", (location.top - (i * .066)) + "vw");
        card.display.css("z-index", zIndex + i);
        card.display.addClass("in-deck");
        card.display.addClass(orientation);

        if (card.id >= 0 && !forceFaceDown)
            card.display.addClass("flipped");

        //this.display.find(".count").text("x" + (i + 1));
    }

    this.removeTopCard = function () {
        var card = this.cards[this.cards.length - 1]
        this.removeCard(card)
        return card;
    }

    this.removeBottomCard = function () {
        var card = this.cards[0];
        this.removeCard(card);
        return card;
    }

    this.removeCard = function (card) {
        var cards = [],
            i = 0,
            retCard = null;
        for (var c in this.cards) {
            i++;
            var curCard = this.cards[c];
            if (curCard != card) {
                cards.push(curCard);
                curCard.display.css("z-index", i);
            }
            else{
                curCard.display.removeClass("in-deck");
                retCard = curCard;
            }
        }
        this.cards = cards;
        return retCard;
        //this.display.find(".count").text("x" + (this.cards.length + 1));

    }
 
    this.removeCardById = function (id) {
        for (var c in this.cards) {
            var card = this.cards[c];
            if (card.id == id) {
                return this.removeCard(card);
            }
        }
        return null;
    }

    this.selectCard = function (callback) {
        var obj = this;
        this.selectionMode = true;
        this.fanCards();

        //this.display.addClass("selection-mode");
        //var cards = this.display.find(".card").one("click", clickHandler);
        for (var c in this.cards) {
            var card = this.cards[c];
            card.display.addClass("selectable");
            card.display.one("click", clickHandler);
        }


        function clickHandler() {
            obj.selectionMode = false;
            for (var c in obj.cards) {
                obj.cards[c].display.off("click");
            }
            if (callback) {
                var id = $(this).attr("data-id");
                callback(id);
            }
            $(document).trigger("click");
        }
    }

    this.cancelSelectCard = function () {
        this.selectionMode = false;
        for (var c in obj.cards) {
            obj.cards[c].display.off("click");
        }
        $(document).trigger("click");
    }

    this.hasCard = function () {
        return this.cards.length > 0;
    }

    this.fanCards = function () {
        _expanded = true;
        var align,
            dir,
            colWidth,
            cardsInCol = 12,
            cardCount = this.cards.length,
            colCount = Math.ceil(cardCount / cardsInCol);

        if (this.orientation == Orientation.Horizontal) {
            colWidth = 18.4;
        }
        else {
            colWidth = 14;
        }

        if (location.left >= 50) {
            align = 100 - (colWidth*2);
            dir = -1;
        }
        else {
            align = 0;
            dir = 1;
        }


        for (var c = 0; c < this.cards.length; c++) {
            var card = this.cards[c];
            card.display.css("left", (align + parseInt(c / cardsInCol) * colWidth * dir) + "vw");
            card.display.css("top", (2 + (c % cardsInCol) * 3) + "vw");
            card.display.css("z-index", 100 + c);
            card.display.addClass("fanned");
        };

    }

    {
        this.cards = [];
        this.flipCardTemplate = $("#game .templates .card").clone();
        this.floatersDisplay = $(".floaters");


        if (this.viewable) {
            var obj = this;

            var view = $("<div class=\"fan\"></div>");
            this.display.append(view);
            view.css("top", (location.top) + "vw");
            view.css("left", (location.left + (orientation == Orientation.Horizontal ? 10 : 7.5)) + "vw");

            var _expanded = false;
            view.show();
            view.click(function (e) {
                e.stopPropagation();

                if (_expanded = !_expanded) 
                    obj.fanCards();
                else 
                    collapse();
            });

            $(document).click(function (e) {
                if (_expanded && !obj.selectionMode) {
                    collapse();
                    _expanded = false;
                }
            })

            function collapse() {
                for (var c in obj.cards) {
                    var card = obj.cards[c];
                    card.display.css("left", location.left + "vw");
                    card.display.css("top", (location.top - (c * .066)) + "vw");
                    card.display.css("z-index", c);
                    card.display.removeClass("fanned");
                }
            }
        }
    }
}


var Orientation = {
    Horizontal: "hor",
    Vertical: "vert"
}