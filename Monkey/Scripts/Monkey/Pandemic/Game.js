var Game =  {
    display: null,
    actionsRemainingDisplay: null,
    playEventCardButton: null,

    map: null,
    cityIdsByName: null,
    cityData: null,

    playerDeck: null, 
    playerDeckDiscardPile: null,
    infectionDeck: null,
    infectionDeckDiscardPile: null,

    players: null,
    activePlayer: null,
    actionsRemaining: 0,
    myTurn: false,

    state: null,
    stateData: null,
    infectionRateCount: 0,
    outbreakCount: 0,

    interrupt: null,
    cures: {},
    orphanedCards: [],

    init: function(){
        console.info("Pandemic Initialized");
        var obj = this;

        this.display = $("#game");
        this.actionsRemainingDisplay = this.display.find(".ui .control-hud .actions-display .actions");
        this.playEventCardButton = this.display.find(".play-event-card-button");
        this.tickerDisplay = this.display.find(".ticker");

        var resizeClearTimer;
        window.onresize = function () {
            $("body").addClass("notransition");
            clearTimeout(resizeClearTimer);
            resizeClearTimer = setTimeout(function () {
                $("body").removeClass("notransition");
            }, 250);
        }

        $("body").delegate(".card", "dragstart", function (e) {
            e.preventDefault();
        });
    },


    join: function (game) {
        var obj = this;

        this.buildCityLookups(game.Map.Cities);
        this.map = new Map(this.display.find(".map"));
        this.map.init(game.Map);

        this.updateActivePlayer(game.ActivePlayer);
        this.updateActionsRemaining(game.ActionsRemaining);


        // Build decks
        var cardsDisplay = this.display.find(".map > .cards");
        this.infectionDeckDiscardPile = new Deck(cardsDisplay, { top: 3.3, left: 79.7 }, "infection", Orientation.Horizontal, true, 60);
        this.playerDeckDiscardPile = new Deck(cardsDisplay, { top: 3.3, left: 22.8 }, "player", Orientation.Vertical, true, 60);
        var infectionDiscardPileCards = [];
        for (var i in game.InfectionDiscardPileIds) {
            var id = game.InfectionDiscardPileIds[i];
            var card = new Card(id, this.getCardKey(id), "infection");
            infectionDiscardPileCards.push(card);
        }
        this.infectionDeckDiscardPile.setCards(infectionDiscardPileCards);

        var playerDiscardPileCards = [];
        for (var i in game.PlayerDiscardPileIds) {
            var id = game.PlayerDiscardPileIds[i];
            var card = new Card(id, this.getCardKey(id), "player");
            playerDiscardPileCards.push(card);
        }
        this.playerDeckDiscardPile.setCards(playerDiscardPileCards);

        this.infectionDeck = new Deck(cardsDisplay, {top:3.3, left:68}, "infection", Orientation.Horizontal, false);
        for (var i = 0; i < game.InfectionDeckCount; i++)
            this.infectionDeck.addCard(new Card(-1, "back", "infection"));

        this.playerDeck = new Deck(cardsDisplay, { top: 3.3, left: 13.6 }, "player", Orientation.Vertical, false);
        for (var i = 0; i < game.PlayerDeckCount; i++)
            this.playerDeck.addCard(new Card(-1, "back", "player"));

        // Build player objects
        this.players = {};
        var otherPlayerCount = 0;
        for (var p in game.Players) {
            var player = game.Players[p];
            this.players[player.Name] = player;

            if (player.Name == profile.name) {
                player["hand"] = new Hand(cardsDisplay, player, { top: 48.5, left: 46 }, Orientation.Horizontal, player.Role.Type);
            }
            else {
                player["hand"] = new Hand(cardsDisplay, player, { top: 54, left: otherPlayerCount * 5.5 }, Orientation.Vertical, player.Role.Type);
                otherPlayerCount ++ ;
            }

            for(var c in player.Hand){
                var id = player.Hand[c];
                player.hand.addCard(new Card(id, this.getCardKey(id), "player"));
            }
            delete player.Hand;  // Get rid of the hand provided by the server -- the client will manage it from here

            player["piece"] = $("<div class=\"player-piece\"></div>");
            player.piece.addClass(player.Color.toLowerCase());
            player.piece.attr("data-name", player.Name);

            obj.buildPlayerAPI(player);
        }


        OverlayManager.init(this.display.find(".overlays"))

        this.initGameState(game);


        if (game.Interrupt)
            this.interrupt = game.Interrupt;

        if (this.myTurn && this.interrupt == null) {
            this.display.addClass("my-turn");
            if (this.state == GameState.PlayerMove) {
                var player = this.players[this.activePlayer];
                this.map.activateNodesForPlayer(player);
            }
        }

        this.updatePlayerPieces();

        if (this.interrupt) {
            if (this.interrupt.Type == "DiscardInterrupt") {
                this.discard();
            }
            this.processInterrupt(false);
        }

        var yieldTurn =this.display.find(".control-hud .yield-turn");
        yieldTurn.on("mousedown", function () {
            $(this).find(".progress").addClass("filling");
        }).on("mouseup", function () {
            $(this).find(".progress").removeClass("filling");
        });

        yieldTurn.find(".progress").on("transitionend", function () {
            var progress = $(this);
            progress.addClass("active");
            setTimeout(function () {
                progress.removeClass("filling active");
                gameConn.server.pass();
            }, 200);
        })

        MessageManager.showLog();

        this.display.show();
        this.checkGameOver();
    },

    updateActivePlayer:function(name){
        this.activePlayer = name;
        this.myTurn = name == profile.name;

    },

    updateActionsRemaining: function(remaining){
        this.actionsRemaining = remaining;
        this.actionsRemainingDisplay.text(remaining);

        if (remaining == 0)
            this.map.deactivateNodes();
    },

    buildPlayerAPI: function (player) {
        var obj = this;
        player["hasCard"] = hasCard;
        player["canCureDisease"] = canCureDisease;
        player["hasCityCard"] = hasCityCard;

        function canCureDisease(){
            var city = Game.cityData[this.Location];
            var cardsNeeded = this.Role.Name == PlayerRole.Scientist ? 4 : 5;

            if(city.ResearchStation){
                var colors = {
                    Red:0, Blue:0, Black:0, Yellow:0
                }

                for (var h in this.hand.cards) {
                    var city = obj.cityData[this.hand.cards[h].id];
                    if (city) {
                        colors[city.Color]++;
                    }
                }

                if (colors.Red >= cardsNeeded || colors.Blue >= cardsNeeded || colors.Black >= cardsNeeded || colors.Yellow >= cardsNeeded)
                    return true;
            }
            return false;
        }

        function hasCard(id) {
            for (var c in this.hand.cards) {
                if (this.hand.cards[c].id == id)
                    return true;
            }
            return false;
        }

        function hasCityCard() {
            for (var c in this.hand.cards) {
                if (this.hand.cards[c].id < 48)
                    return true;
            }
            return false;
        }
    },

    update: function (data) {
        if (data.ActivePlayer)
            this.updateActivePlayer(data.ActivePlayer);

        if (!isNaN(data.ActionsRemaining))
            this.updateActionsRemaining(data.ActionsRemaining);

        if (data.StateData)
            this.stateData = data.StateData;

        if (data.State) {
            this.state = data.State;
            if(this.state == GameState.PlayerMove)
                OverlayManager.hideOverlay(OverlayManager.overlays.infect);

        }

        if (data.TradeAccepted != undefined) {
            if (!data.TradeAccepted) {
                OverlayManager.overlays.tradeConfirmation.rejected();
            }
            else {
                var id = this.interrupt.Card,
                    player1 = this.players[this.interrupt.Player1],
                    player2 = this.players[this.interrupt.Player2];

                OverlayManager.hideOverlay(OverlayManager.overlays.tradeConfirmation);
                this.animateTrade(player1, player2, id);
            }
            this.clearInterrupt();
        }

        if (data.Players) {
            for (var p in data.Players) {
                var playerData = data.Players[p];
                var player = this.players[playerData.Name];
                
                if (!isNaN(playerData.Location))
                    player.Location = playerData.Location;

                if (playerData.RemoveCards) {
                    this.discardPlayerCards(player, playerData.RemoveCards);
                }
            }
        }
        this.updatePlayerPieces();


        if (data.DrawnPlayerCards)
            this.drawPlayerCards(this.activePlayer, data.DrawnPlayerCards);


        if (!isNaN(data.DrawnInfectionCard) && this.state != GameState.Finished) {
            var cityData = data.Map ? data.Map.Cities : null;
            if (this.state == GameState.Epidemic)
                this.animateEpidemicInfect(data.DrawnInfectionCard, cityData);
            else {
                if (this.state == GameState.Infect) {
                    this.startInfectStage();
                }
                this.animateInfect(data.DrawnInfectionCard, cityData);
            }
        }
        else if (data.Map)
            this.map.updateCities(data.Map.Cities);

        if (data.EpidemicIntensified)
            this.animateEpidemicIntensify();

        if (data.OutbreakCount)
            this.updateOutbreakCount(data.OutbreakCount);

        if (data.Cure)
            this.updateCure(data.Cure);

        if (!isNaN(data.ExiledCardFromInfection)) {
            this.animateExileCardFromInfectionDiscardPile(parseInt(data.ExiledCardFromInfection));
            OverlayManager.hideOverlay(OverlayManager.overlays.message);
        }

        if (data.ForecastCardReorder) {
            OverlayManager.overlays.forecast.updateOrder(data.ForecastCardReorder);
        }

        if (data.ForecastComplete) {
            this.animateForecastComplete(data.ForecastComplete);
        }

        if (data.DiscardComplete) {
            this.clearInterrupt();
            OverlayManager.hideOverlay(OverlayManager.overlays.message);
        }

        if (data.EradicatedDiseases)
            this.updateEradicatedDiseases(data.EradicatedDiseases);

        if (data.Interrupt) {
            this.interrupt = data.Interrupt;

                this.processInterrupt(data.DrawnPlayerCards);

        }
        else {
            if (this.interrupt != null) {
                OverlayManager.hideOverlay(OverlayManager.overlays.message);
            }
        }



        if (this.myTurn && this.interrupt == null) {
            this.display.addClass("my-turn");
            if (this.state == GameState.PlayerMove) {
                var player = this.players[this.activePlayer];
                this.map.activateNodesForPlayer(player);
            }
            else this.map.deactivateNodes();
        }
        else {
            if (this.interrupt == null || this.interrupt != null && this.interrupt.Player != profile.name) {
                this.map.deactivateNodes();
            }
            this.display.removeClass("my-turn");
        }

        this.checkGameOver();

    },

    isCured: function(color){
        if (this.cures[color])
            return true;
        return false;
    },

    getPlayer: function(name){
        return this.players[name];
    },

    getCity: function(id){
        return this.cityData[id];
    },

    getPlayerCard: function (id) {
        var city = Game.cityData[id];
        return new Card(city.Id, city.key, "player");
    },

    processInterrupt: function (drawingCards) {
        if (this.interrupt) {
            this.display.addClass("interrupt");
            switch (this.interrupt.Type) {
                case "TradeInterrupt":
                    var ui = OverlayManager.overlays.tradeConfirmation;
                    ui.setTradeOptions(this.interrupt)
                    OverlayManager.showOverlay(ui);
                    break;

                case "EventCardInterrupt":
                    this.processEventCardInterrupt();
                    break;

                case "MoveResearchStationInterrupt":
                    var ui = OverlayManager.overlays.message,
                        myEvent = profile.name == this.interrupt.Player,
                        obj = this;
                    ui.setTitle(myEvent ? "You may only have 6 Research Stations, choose a Research Station to remove." : "Waiting for " + this.interrupt.Player + " to choose a Research Station to remove.");
                    OverlayManager.showOverlay(ui);

                    if (myEvent) {
                        this.map.activateNodesForResearchStationRemoval(function (id) {
                            gameConn.server.executeFinalizedResearchStation(id);
                            OverlayManager.hideOverlay(ui);
                            obj.clearInterrupt();
                        });
                    }
                    break;

                case "DiscardInterrupt":
                    console.info("In discard", drawingCards);
                    if(!drawingCards)
                        this.discard();
                    break;
            }
        }
        else {
            this.display.removeClass("interrupt");
        }
    },

    processEventCardInterrupt: function () {
        var obj = this,
            myEvent = profile.name == this.interrupt.Player;
        this.uncenterCards();
        OverlayManager.hideOverlay(OverlayManager.overlays.cardSelection);

        var messageUIParams = { cancellable: myEvent };

        switch (this.interrupt.CardId) {
            case EventCard.GovernmentGrant:
                if (myEvent) {
                    messageUIParams["onCancel"] = function () {
                        gameConn.server.cancelEventCard();
                        obj.clearInterrupt();
                    }
                }

                var ui = OverlayManager.overlays.message;
                ui.setTitle(myEvent ? "Choose the city where the new Research Station will be built." : "Waiting for " + this.interrupt.Player + " to build a Research Center with the Government Grant.", messageUIParams);
                OverlayManager.showOverlay(ui);

                if (myEvent) {
                    this.map.activateNodesForGovernmentGrant(function (id) {
                        gameConn.server.executeGovernmentGrant(id);
                        OverlayManager.hideOverlay(ui);
                        obj.clearInterrupt();
                    });
                }
                break;

            case EventCard.AirLift:
                if (myEvent) {
                    messageUIParams["onCancel"] = function () {
                        gameConn.server.cancelEventCard();
                        obj.clearInterrupt();
                    }
                }
                var ui = OverlayManager.overlays.message,
                    player;
                ui.setTitle(myEvent ? "Select a player piece to Air Lift." : "Waiting for " + this.interrupt.Player + " to Air Lift someone.", messageUIParams);
                OverlayManager.showOverlay(ui);

                if (myEvent) {
                    this.map.activatePlayersForSelection(function (name) {
                        player = name;
                        console.info(player);
                        ui.setTitle("Select a city to Air Lift " + name + " to.");
                        obj.map.activateNodesForCitySelect(function (id) {
                            gameConn.server.executeAirLift(name, id);
                            OverlayManager.hideOverlay(ui);
                            obj.clearInterrupt();
                        }, [obj.players[player].Location]);
                    });
                }
                break;

            case EventCard.ResilientPopulation:
                var obj = this,
                    ui = OverlayManager.overlays.message;
                if (myEvent) {
                    messageUIParams["onCancel"] = function () {
                        gameConn.server.cancelEventCard();
                        obj.infectionDeckDiscardPile.cancelSelectCard();
                        obj.clearInterrupt();
                    }
                }

                ui.setTitle(myEvent ? "Select a card to remove from the game." : "Waiting for " + this.interrupt.Player + " to remove a card from the game with Resilient Population.", messageUIParams);
                OverlayManager.showOverlay(ui);

                if (myEvent) {
                    this.infectionDeckDiscardPile.selectCard(function (card) {
                        gameConn.server.executeResilientPopulation(card);
                        OverlayManager.hideOverlay(ui);
                        obj.clearInterrupt()
                    })
                }
                break;

            case EventCard.Forecast:
                var ids = this.interrupt.Data.Cards;
                if (myEvent) {
                    messageUIParams["onCancel"] = function () {
                        gameConn.server.cancelEventCard();
                        obj.infectionDeckDiscardPile.cancelSelectCard();
                        obj.clearInterrupt();
                    }
                }

                this.animateForecast(ids, messageUIParams);
                break;
        }
    },

    centerCard: function (card) {
        if (card.display.hasClass("hand") || card.display.hasClass("my-hand")) {
            console.info("Center card");
            this.uncenterCards();

            this.display.addClass("card-centered");
            card.center();

            if (card.id >= 50 && card.id < 100 && this.players[profile.name].hand.hasCard(card.id) && this.interrupt == null) {
                this.showEventCardButton(card);
            }

            var obj = this;
            $(document).one("click", function () {
                obj.uncenterCards();
            })
        }
    },

    uncenterCards: function(){
        if (this.display.hasClass("card-centered")) {
            var centered = $(".cards .centered");
            var id = centered.attr("data-id");
            this.hideEventCardButton();

            for (var p in this.players) {
                if (this.players[p].hand.hasCard(id)) {
                    this.players[p].hand.orderDisplay();
                    break;
                }
            }

            this.display.removeClass("card-centered");
            centered.removeClass("centered");
        }
    },

    updateGameState: function (state, stateData) {
        this.state = state;
        this.stateData = stateData;

        switch (state) {
            case GameState.Epidemic:
                var epidemic = OverlayManager.overlays.epidemic;
                var step = stateData.Step;
                OverlayManager.showOverlay(epidemic);
                epidemic.activateStep(step, this.myTurn)

                break;
            case GameState.Infect:
                this.startInfectStage();
                break;
        }

    },



    startInfectStage: function () {
        // skipped if one quiet night
        if (this.stateData && this.stateData.TotalSteps) {
            var overlay = OverlayManager.overlays.infect;
            overlay.setStepCount(this.getInfectionRate());

            overlay.activateStep(this.stateData.Step, this.myTurn);
            OverlayManager.showOverlay(overlay);
        }
    },

    discardPlayerCards: function(player, ids){
        var obj = this;
        
        for (var id in ids) {
            id = ids[id];
            var card = player.hand.removeCard(id);
            if (card != null) {
                card.display.removeClass("centered");
                this.playerDeckDiscardPile.addCard(card);
            }
        }
    },

    showEventCardButton: function (card) {
        this.playEventCardButton.show();
        this.playEventCardButton.off("click");
        this.playEventCardButton.one("click", handleClick);

        var obj = this;
        function handleClick(e) {
            e.stopPropagation();
            gameConn.server.playEventCard(card.id);

            // Clear the document click watch and return the overlay to its starting state
            $(document).trigger("click");

            // Hack to handle OneQuietNight -- hide button
            if (card.id == 50)
                obj.hideEventCardButton();
        }
    },

    hideEventCardButton: function(){
        this.playEventCardButton.hide();
    },

    discard: function () {
        var player = this.players[this.interrupt.Player];
        if (player.Name == profile.name) {
            var player = this.players[this.interrupt.Player];
            var obj = this;
            player.hand.startSelection(this.interrupt.Count, "Discard down to 7 cards", {
                selectionCallback: function (ids) {
                    gameConn.server.discard(ids);
                    obj.clearInterrupt();
                },
                canPlayEventCards: true
            });
        }
        else {
            var ui = OverlayManager.overlays.message;
            ui.setTitle("Waiting for " + player.Name + " to discard.");
            OverlayManager.showOverlay(ui);
        }
    },

    /**
     * Prepares and sends the ids from the server to the deck draw animation function
     */
    drawPlayerCards: function (playerName, ids) {
        var obj = this,
            cards = [];
        for (var id in ids) {
            id = ids[id];
            var key = this.getCardKey(id);
            cards.push(new Card(id, key, "player"));
        } 

        var cardCount = cards.length,
            current = 0,
            pendingEpidemics = 0;
        this.animateCardDraw(cards, this.playerDeck,
            function (card) {
                setTimeout(function () {
                    current++;
                    if (card.id != CardIds.Epidemic)
                        obj.players[playerName].hand.addCard(card);
                    else {
                        pendingEpidemics++;
                        card.display.css({
                            "z-index": "",
                            "top": "",
                            "left": ""
                        });

                        card.display.css("top", "20vw");
                        card.display.css("left", "-10vw");
                    }

                    if (current == cardCount) {
                        setTimeout(function () {
                            card.display.one('transitionend', function (e) {
                                if (obj.state != GameState.Finished) {

                                    if (pendingEpidemics > 0) {
                                        OverlayManager.showOverlay(OverlayManager.overlays.epidemic);
                                        obj.animateEpidemic();
                                    }
                                    else {
                                        obj.startInfectStage();
                                    }

                                }
                                if (obj.interrupt && obj.interrupt.Type == "DiscardInterrupt")
                                    obj.discard();
                            });
                        }, 100);
                    }
                }, 500);
            }
        );
    },

    animateForecast: function (ids, params) {
        var obj = this,
            cards = [];
        for (var i = 0; i < ids.length;i++) {
            (function (id) {
                setTimeout(function () {
                    var card = obj.infectionDeck.removeTopCard();
                    obj.orphanedCards.push(card);
                    cards.push(card);
                    card.setFace(id, obj.getCardKey(id));
                    card.flip();
                    card.display.css({
                        "left": "102vw",
                        "top": "20vw"
                        });
                }, i * 100);
            })(ids[i]);
        }

        setTimeout(function () {
            obj.showForecast(cards, params);
        }, i * 110);
    },


    animateForecastComplete: function(ids){
        OverlayManager.hideOverlay(OverlayManager.overlays.forecast);
        for(var c in this.orphanedCards){
            var card = this.orphanedCards[c];
            card.flip();
            this.infectionDeck.addCard(card, true);
            this.clearInterrupt();
        }
    },

    clearInterrupt: function(){
        this.interrupt = null;
        this.display.removeClass("interrupt");
    },

    showForecast: function(cards, params){
        var ui = OverlayManager.overlays.forecast;
        OverlayManager.showOverlay(ui);
        ui.setCards(cards, params);
    },

    animateTrade: function(player1, player2, id){
        if (player1.hasCard(id)) {
            player2.hand.addCard(player1.hand.removeCard(id));
        }
        else {
            player1.hand.addCard(player2.hand.removeCard(id));
        }

        if (this.interrupt && this.interrupt.Type == "DiscardInterrupt") {
            this.discard();
        }
    },

    animateInfect: function (id, cityData) {
        var obj = this,
            key = this.getCardKey(id),
            cardData = { id: id, key: key };

        this.animateCardDraw([cardData], this.infectionDeck, function (card) {
            obj.infectionDeckDiscardPile.addCard(card);
            obj.map.updateCities(cityData);

            if (obj.state == GameState.PlayerMove) {
                OverlayManager.hideOverlay(OverlayManager.overlays.infect);
                if (obj.myTurn) {
                    var player = obj.players[obj.activePlayer];
                    obj.map.activateNodesForPlayer(player);
                }
            }
        });
    },

    animateEpidemicInfect: function (id, cityData) {
        var obj = this,
            key = this.getCardKey(id),
            cardData = { id: id, key: key };

        this.animateBottomCardDraw(cardData, function (card) {
            obj.infectionDeckDiscardPile.addCard(card);
            obj.map.updateCities(cityData);
            var ui = OverlayManager.overlays.epidemic;
            ui.activateStep(2, obj.myTurn);
        });
    },

    animateEpidemicIntensify: function () {
        var obj = this;
        while (this.infectionDeckDiscardPile.hasCard()) {
            var card = this.infectionDeckDiscardPile.removeTopCard();
            (function(c) {
                card.display.css({
                    "z-index": 2000,
                    "transition": "left .2s, top .2s",
                    "left": (Math.random() * 90) + "vw",
                    "top": (10 + Math.random() * 30) + "vw"
                })
                card.display.removeClass("flipped");
                setTimeout(function () {
                    c.display.css("transition", "");
                    obj.infectionDeck.addCard(c, true);
                }, Math.random() * 250);
            
            })(card);
        }

        setTimeout(function () {
            if (obj.state == GameState.Epidemic) {
                obj.animateEpidemic();
            }
            else {
                OverlayManager.hideOverlay(OverlayManager.overlays.epidemic);
                obj.startInfectStage();
            }
        }, 400);
    },

    animateEpidemic: function () {
        var ui = OverlayManager.overlays.epidemic,
            obj = this;
        ui.activateStep(0, this.myTurn);

        this.updateInfectionRate(this.infectionRateCount + 1, function () {
            ui.activateStep(1, obj.myTurn);
        });
    },


    animateCardDraw: function (cards, deck, callback) {
        var obj = this;

        var card = deck.removeTopCard();
        card.setFace(cards[0].id, cards[0].key);
        card.display.css("z-index", "2000");
        card.display.css("top", "20vw");
        card.display.css("left", "46vw");
        card.display.addClass("flipped");


        card.display.one('transitionend', function (e) {
            var rest = cards.slice(1);
            if (rest.length > 0) {
                setTimeout(function () {
                    obj.animateCardDraw(rest, deck, callback);
                }, 300);
            }

            if (callback) {
                callback(card);
            }
        });
    },

    animateBottomCardDraw: function(cardData, callback){
        var obj = this;

        var card = this.infectionDeck.removeBottomCard();
        card.setFace(cardData.id, cardData.key);
        card.display.css("left", "46vw");
        card.display.addClass("flipped");

        if (callback) {
            card.display.one('transitionend', function (e) {
                callback(card);
            });
        }
    },

    animateExileCardFromInfectionDiscardPile: function (id) {
        var card = this.infectionDeckDiscardPile.removeCardById(id);
        this.animateExileCard(card);
        this.clearInterrupt();
    },

    animateExileCard: function(card){
        card.display.css({
            "left": "120vw",
            "top": "25vw",
        })
        setTimeout(function () {
            card.display.remove();
        }, 3000)
    },

    buildCityLookups: function (cities) {
        this.cityIdsByName = {};
        this.cityData = [];

        for (var name in cities) {
            var id = cities[name].Id;
            cities[name]["key"] = name;
            this.cityIdsByName[name] = id;
            this.cityData[id] = cities[name];
        }
    },

    updatePlayerPieces: function () {
        var cities = [];

        for (var p in this.players) {
            var player = this.players[p];
            var piece = player.piece;
            if (!cities[player.Location])
                cities[player.Location] = { count: 0, pieces: [] };
            cities[player.Location].count++;
            cities[player.Location].pieces.push(piece);
        }

        for (var i = 0; i < cities.length; i++) {
            var data = cities[i];
            var positions = [];
            if (data) {
                switch (data.count) {
                    case 1:
                        positions.push({ x: 0, y: 0 });
                        break;
                    case 2:
                        positions.push({ x: -1, y: 0 });
                        positions.push({ x: 1, y: 0 });
                        break;
                    case 3:
                        positions.push({ x: -1, y: -1 });
                        positions.push({ x: 1, y: -1 });
                        positions.push({ x: 0, y: 1 });
                        break;
                    case 4:
                        positions.push({ x: -1, y: -1 });
                        positions.push({ x: 1, y: -1 });
                        positions.push({ x: -1, y: 1 });
                        positions.push({ x: 1, y: 1 });
                        break;
                }

                
                var cityNode = this.map.display.find(".cities [data-id=\"" + i + "\"]");
                var pieces = $("<div class=\"pieces\"></div>");
                cityNode.append(pieces);
                for (var p in data.pieces) {
                    var piece = data.pieces[p];
                    piece.css("left", (positions[p].x * .5) + "vw");
                    piece.css("top", (positions[p].y * .5) + "vw");
                    pieces.append(piece);
                }
            }
        }
    },

    

    initGameState: function(game){
        this.updateInfectionRate(game.InfectionRateCount);
        this.updateCures(game.Cures);
        this.updateOutbreakCount(game.OutbreakCount);
        this.updateGameState(game.State, game.StateData);
        this.updateEradicatedDiseases(game.EradicatedDiseases);
    },

    updateCures: function(cures){
        for (var c in cures) {
            if (cures[c] == true) {
                this.updateCure(c);
            }
        }
    },

    updateCure: function (cure) {
        var name =  DiseaseColor.getName(parseInt(cure));
        this.display.find(".ui .cures .cure[data-color=" + name + "]").addClass("cured");
        this.cures[name] = true;
    },

    updateEradicatedDiseases: function(eradications){
        if (eradications) {
            for (var c in eradications) {
                var color = DiseaseColor.getName(eradications[c]);
                this.display.find(".ui .cures .cure[data-color=" + color + "]").addClass("eradicated");

            }
        }
    },

    updateInfectionRate: function (rate, callback) {
        this.infectionRateCount = rate;

        rate = rate * 3.3;
        var marker = this.display.find(".infection-rate .rate-marker");
        marker.css("left", rate + "vw");

        if (typeof(callback) == "function") {
            marker.one("transitionend", function () {
                callback();
            })
        }
    },

    updateOutbreakCount: function(count){
        this.outbreakCount = count;

        var marker = this.display.find(".outbreak-rate .rate-marker"),
            top = 2.4 + count * 2.19,
            left = .78;

        if (count % 2 != 0)
            left =3.25;

        marker.css({
            "left": left + "vw",
            "top": top + "vw"
        })
    },

    checkGameOver: function(){
        if (this.state == GameState.Finished) {
            this.display.addClass("game-over");

            OverlayManager.hideOverlays();

            var ui = OverlayManager.overlays.finished,
                win = this.stateData.Win,
                outcome, message, details = "";


            if (win) {
                outcome = "Victory!";
                message = "The people rejoice.";
                details = "All diseases have been cured."
            }
            else{
                outcome = "Defeat!";
                message = "You have failed the world, billions die.";
                details = getLossReasonText(this.stateData.LossReason);
            }

            ui.setInfo(outcome, message, details);
            OverlayManager.showOverlay(ui);
        }

        function getLossReasonText(reason){
            switch (reason) {
                case "DeckEmpty":
                    return "Unable to draw from the Player Deck."
                case "Outbreaks":
                    return "8 or more Outbreaks."
                case "NoBlack":
                    return "Could not place a Black Disease Counter.";
                case "NoBlue":
                    return "Could not place a Blue Disease Counter.";
                case "NoYellow":
                    return "Could not place a Yellow Disease Counter.";
                case "NoRed":
                    return "Could not place a Red Disease Counter.";
            }
        }

    },

    getInfectionRate: function(){
        switch(this.infectionRateCount){
            case 0:
            case 1:
            case 2:
                return 2;
            case 3:
            case 4:
                return 3;
            default:
                return 4;
        }
    },

    getCardKey: function (id) {
        switch (id) {
            case 50:
                return "OneQuietNight";
            case 51:
                return "GovernmentGrant";
            case 52:
                return "AirLift";
            case 53:
                return "Forecast";
            case 54:
                return "ResilientPopulation";
            case 100:
                return "Epidemic";
        }

        if (this.cityData[id])
            return this.cityData[id].key;
    },

    getCardName: function(id){
        switch(id){
            case 50:
                return "One Quiet Night";
            case 51:
                return "Government Grant";
            case 52:
                return "Air Lift";
            case 53: 
                return "Forecast";
            case 54:
                return "Resilient Population";
        }

        if(this.cityData[id])
            return this.cityData[id].Name;

        return null;
    },


    message: function (obj) {
        MessageManager.addNotice(obj);
    },



}


