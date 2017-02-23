function Hand(display, player, location, orientation, role) {
    this.display = display;
    this.cards = [];

    this.roleDisplay = new Card(role, role, "role");
    
    this.hasCard = function (id) {
        if (this.roleDisplay.id == id)
            return true;

        for (var i in this.cards) {
            var card = this.cards[i];
            if (card.id == id)
                return true;
        }

        return false;
    }

    this.getCard = function (id) {
        for (var i in this.cards) {
            var card = this.cards[i];
            if (card.id == id)
                return card;
        }
    }

    this.addCard = function (card) {
        var obj = this;
        this.cards.push(card);
        card.display.addClass("flipped");
        if (player.Name == profile.name) {
            card.display.addClass("my-hand");
            this.roleDisplay.display.addClass("my-hand role-card");
        }
        else {
            card.display.addClass("hand");
            this.roleDisplay.display.addClass("hand role-card");
        }

        card.display.css("z-index", this.cards.length);

        if(!(card.display.parent()[0] == this.display[0] ))
            this.display.append(card.display);

        this.orderDisplay();
    }


    this.removeCard = function (id) {
        var card = null;
        var cards = this.cards;
        this.cards = [];
        for (var c in cards) {
            if (cards[c].id != id)
                this.cards.push(cards[c])
            else {
                cards[c].display.removeClass("hand my-hand");

                card = cards[c];
            }
        }
        this.orderDisplay();
        return card;
    }

    this.highlightCard = function (id) {
        this.removeHighlights();
        for (var c in this.cards) {
            if (this.cards[c].id == id) {
                this.cards[c].display.addClass("highlight");
            }
        }
    }

    this.removeHighlights = function () {
        this.display.find(".highlight").removeClass("highlight");
    }

    this.startSelection = function (count, title, params) {
        var obj = this,
            ui = OverlayManager.overlays.cardSelection,
            cityOnly = params.cityOnly ? true : false,
            cardSet = [],
            canPlayEventCards = params.canPlayEventCards ? true : false;

        for (var i = 0; i < this.cards.length; i++) {
            var card = this.cards[i];
            card.display.addClass("selection-mode disabled");
            this.roleDisplay.display.addClass("disabled");
            card.display.css({
                "left": (110 + (i * 3)) + "vw",
            });

            if(!cityOnly || card.id < 48)
                cardSet.push(card);
        }

        OverlayManager.showOverlay(ui);

        ui.setCards(cardSet, count, title,
            function (x) {
                obj.orderDisplay();
                if(params.selectionCallback)
                    params.selectionCallback(x);
                for (var c in obj.cards)
                    obj.cards[c].display.removeClass("disabled");
                obj.roleDisplay.display.removeClass("disabled");
            },

            function () {
                obj.orderDisplay();
                if (params.cancelCallback)
                    params.cancelCallback();
                for (var c in obj.cards)
                    obj.cards[c].display.removeClass("disabled");
                obj.roleDisplay.display.removeClass("disabled");
            },

            canPlayEventCards
        );
    }

    this.orderDisplay = function () {
        for (var i = 0; i < this.cards.length; i++) {
            var card = this.cards[i],
                top = location.top,
                left = location.left;
            if (orientation == Orientation.Horizontal) 
                left += i * 3;
            else 
                top -= this.cards.length * 1.2 - i * 1.2;
            card.display.removeClass("selection-mode");

            card.display.css({
                "top": top + "vw",
                "left": left + "vw",
                "z-index": i + 1
            });

        }

        this.positionRole();
    }
    
    this.positionRole = function () {
        this.roleDisplay.display.removeClass("fanned");
        if (player.Name == profile.name) {
            this.roleDisplay.display.css({
                "left": (location.left - 9) + "vw",
                "top": location.top + "vw"
            });
        }
        else {
            this.roleDisplay.display.css({
                "left": location.left + "vw",
                "top": (location.top - (this.cards.length + 1) * 1.2) + "vw"
            });
        }
    }


    {
        var obj = this;
        var showingRole = false;

        this.roleDisplay.flip();
        this.display.append(this.roleDisplay.display);

        
        
    }

}