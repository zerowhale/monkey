var Game = {
    _initialized: false,
    display: null,
    players: null,
    playerBoards: null,
    activePlayer: null,
    myBoard:null,
    familyMode: false,
    actions: [],
    mode: null,
    myControls:null,
    harvestPanel: null,
    majorImprovementOwners: null,
    interrupt: false,
    currentRound: 0,
    numPlayers: 0,
    myHand: null,

    tooltip: null,
    fullImage:null, 
    templates: {
        initialGameHtml: null,
        playerBoard: null,
        pieces: {
            familyMember: null
        },
        improvementCard: null,
        quickCard: null
    },

    chat:{
        system:null,
        players:null    
    },

    popups:{
        animalChoice: null,
        master: null
    },

    screen: null,
    currentMousePos: { x: -1, y: -1 },

    init: function () {
        var obj = this;
        if (this._initialized) {
            console.info("Agricola Already Initialized");
            return this.setup();
        }

        $(document).mousemove(function (event) {
            obj.currentMousePos.x = event.pageX;
            obj.currentMousePos.y = event.pageY;
        });


        this.templates.initialGameHtml = $("#game").clone();

        Curator.game = this;

        this._initialized = true;

        var preloads = $("*[data-image]");
        preloads.each(function () {
            preloadImage($(this).attr("data-image"))
        });

        console.info("Agricola initialization complete.");
        this.setup();
    },

    setup: function () {
        var obj = this;

        var _ch = this.templates.initialGameHtml.clone().children();
        $("#game").empty().append(_ch);

        this.display = $("#game");
        this.players = {};
        this.playerBoards = null;
        this.activePlayer = null;
        this.myBoard = null;
        this.familyMode = false;
        this.actions = [];
        this.mode = null;
        this.myControls = null;
        this.harvestPanel = null;
        this.majorImprovementOwners = null;
        this.interrupt = false;




        ////// DEBUG
        this.display.find(".leave-game-button").click(function () {
            gameConn.server.leaveFinishedGame();
        })
        ////// END DBUG

        this.chat.system = this.display.find(".chat .system");
        this.screen = this.display.find(".screen");
        this.myControls = this.display.find(".my-controls").tabs({
            active: 0,
            disabled: [2],
            activate: function (event, ui) {
                if (ui.newPanel.hasClass("farm")) {
                    obj.myBoard.updatePieces();
                }
            }
        });



        this.popups.animalChoice = new AnimalChoicePopup(this.display.find("div.popup.animal-choice"));
        this.popups.master = new FarmyardPopup(this.display.find("div.popup.master-popup"))

        var templates = this.display.find(".templates");
        this.templates.playerBoard = this.display.find("div.player-board").remove();
        this.templates.pieces.familyMember = templates.find("div.family-member").remove();
        this.templates.improvementCard = templates.find("div.card").remove();
        this.templates.quickCard = templates.find("div.quick-card").remove();


        var action = this.display.find("div.action");
        action.click(function () {
            obj.actionClickHandler(this);
        });
        action.hover(this.actionHoverOverHandler, this.actionHoverOutHandler);

        // setup harvest tab
        this.harvestPanel = new HarvestPanel(this.display.find("#my-controls-harvest "));

        // Build the blank round actions
        var roundTemplate = this.display.find(".rounds-board .round").remove();
        var roundsInStage = [4, 3, 2, 2, 2, 1];
        var roundNumber = 1;
        this.display.find(".rounds-board .stage").each(function (i, e) {
            for (var s = 0; s < roundsInStage[i]; s++) {
                var roundDisplay = roundTemplate.clone();
                roundDisplay.attr("data-round", s + 1);
                roundDisplay.find(".text strong").text("Round " + roundNumber);
                roundDisplay.find(".text .note").text("Stage " + (i + 1));
                $(e).append(roundDisplay);
                roundNumber++;
            }
            $(e).append("<div class='harvest'>Harvest</div>");
        });


        //templates.remove();


        this.tooltip = $("#game .tooltip");
        this.display.on("mouseenter", ".icon", function () {
            obj.showIconTooltip($(this));
        });
        this.display.on("mouseout", ".icon", function () {
            obj.hideTooltip();
        });

        this.fullImage = this.display.find(".fullImage");
        var showingFullImage = false;
        var overDataPicture = false;

        function showFullImage() {
            if (!showingFullImage && overDataPicture != null) {
                showingFullImage = true;
                obj.updateFullImage(overDataPicture);
                obj.fullImage.stop(true, true).fadeIn(240);
            }

            if (showingFullImage && overDataPicture) {
                var top = obj.currentMousePos.y + 30;
                var left = obj.currentMousePos.x + 30;


                if (top + obj.fullImage.outerHeight() > Monkey.windows.game.outerHeight())
                    top = Monkey.windows.game.outerHeight() - obj.fullImage.outerHeight();

                if (top < 0)
                    top = 0;

                if (left + obj.fullImage.outerWidth() > Monkey.windows.game.outerWidth())
                    left = Monkey.windows.game.outerWidth() - obj.fullImage.outerWidth();

                if (left < 0)
                    left = 0;

                obj.fullImage.css("top", top);
                obj.fullImage.css("left", left);
            }

        }

        function hideFullImage() {
            showingFullImage = false;

            setTimeout(function () {
                if(!showingFullImage)
                    obj.fullImage.stop(true, false).fadeOut(240);
            }, 50);
        }


        $(document).keydown(function (e) {
            if (e.keyCode == 17) {
                showFullImage();
            }
        });


        $(document).keyup(function (e) {
            if (e.keyCode == 17) {
                hideFullImage();
            }
            else if (e.keyCode == 27) {
                if(obj.popups.master.visible)
                    obj.hidePopup();
            }
        });
        
        // Resize the hand card container on window size change
        window.onresize = function () {
            obj.updateHand();
        }


        $("#game").on("mousemove", "*[data-image]", function () {
            var x = $(this);
            var img = x.data("image");
            if (overDataPicture != img) {
                overDataPicture = img;
                showingFullImage = false;
                if (x.data("always-show"))
                    showFullImage();
                
            }
        });
        $("#game").on("mouseout", "*[data-image]", function () {
            overDataPicture = null;
            hideFullImage();
        });
        console.info("Agricola new game setup complete.");


    },

    
    
    showIconTooltip: function (item) {
        var tooltip = this.tooltip;
        item = $(item);
        if (item.hasClass("food")) this.showTooltip("Food", item);
        else if (item.hasClass("grain")) this.showTooltip("Grain", item);
        else if (item.hasClass("clay")) this.showTooltip("Clay", item);
        else if (item.hasClass("reed")) this.showTooltip("Reed", item);
        else if (item.hasClass("wood")) this.showTooltip("Wood", item);
        else if (item.hasClass("sheep")) this.showTooltip("Sheep", item);
        else if (item.hasClass("boar")) this.showTooltip("Boar", item);
        else if (item.hasClass("cattle")) this.showTooltip("Cattle", item);
        else if (item.hasClass("vegetables")) this.showTooltip("Vegetables", item);
        else if (item.hasClass("stone")) this.showTooltip("Stone", item);
        else if (item.hasClass("starting-player")) this.showTooltip("Starting Player", item);
        else {
            return;
        }

    },

    showTooltip: function(content, item){
        var tooltip = this.tooltip;
        var pos = item.offset();
        tooltip.html(content);
        var top = pos.top - tooltip.outerHeight() - 4;
        var left = pos.left + item.outerWidth() / 2 - tooltip.outerWidth() / 2;

        if(top < 0) top = 0;
        if( left < 0) left = 0;
        tooltip.css("top", top);
        tooltip.css("left", left);
        tooltip.stop().fadeIn(0);

    },

    hideTooltip: function() {
        this.tooltip.fadeOut(0);
    },


    updateFullImage: function(item) {
        if (item) {
            this.fullImage.find("img").attr("src", item);
        }
    },

    buildPlayerBoards: function (players) {
        var obj = this;

        this.playerBoards = {};
        var myBoardDisplay = this.display.find(".my-board").empty();
        var playerInfo = this.display.find(".player-info");
        var playerTemplate = playerInfo.find(".player").remove();

        var myBoardAdded = false;
        var delayAddedBoards = [];

        console.info("in players setup");
        playerInfo.addClass("players" + players.length);

        function attachBoard(pb) {
            playerInfo.find(".players").append(pb);
            pb.tabs();
        }


        for (var i in players) {
            var player = players[i];
            var color = player.Color.toLowerCase();

            var board = new PlayerBoard(this.templates.playerBoard.clone(), player);
            board.show();

            playerTemplate.attr("data-player", player.Name);

            this.playerBoards[player.Name] = board;
            if (player.Name == profile.name) {
                myBoardAdded = true;
                this.myBoard = board;
                myBoardDisplay.append(board.display);
            }
            else {
                var pb = playerTemplate.clone();
                pb.addClass(color);
                pb.find(".board").prepend(board.display);
                pb.find(".name").append(player.Name)
                if (myBoardAdded)
                    attachBoard(pb);
                else
                    delayAddedBoards[delayAddedBoards.length] = pb;
            }
        }

        for (var i = 0; i < delayAddedBoards.length; i++) {
            attachBoard(delayAddedBoards[i]);
        }
    },

    buildMajorImprovements: function () {
        var obj = this;
        var display = this.myControls.find("#my-controls-majors");
        for (var d in MajorImprovementData) {
            var card = MajorImprovementData[d];
            var cardDisplay = this.templates.improvementCard.clone();
            cardDisplay.addClass("major");

            this.populateCardData(cardDisplay, card);

            cardDisplay.hover(function () {
                var x = $(this),
                    id = x.attr("data-id");

                if (obj.majorImprovementOwners[id] != null)
                    x.find(".owned-by").stop().fadeOut(200);
            }, function () {
                var x = $(this),
                    id = x.attr("data-id");

                if (obj.majorImprovementOwners[id] != null)
                    x.find(".owned-by").fadeIn(200);
            });
            display.append(cardDisplay);
        }
        display.append("<div class='clear'></div>")
    },



    join: function (game, params) {
        console.info("Joined game:", game, params);

        Curator.loadDeck(params.deck);

        this.display.removeClassLike("players");
        this.display.addClass("players" + game.Players.length);
        this.numPlayers = game.Players.length;
        this.display.show();
        
        this.familyMode = game.FamilyMode;
        if (this.familyMode)
            this.display.addClass("family-mode");

        this.buildPlayerBoards(game.Players);
        this.buildMajorImprovements();
        this.update(game);

        var s = this.formatNameForSystemMessage(game.ActivePlayerName) + "'s turn";
        this.systemMessage(s)

        if (game.ActivePlayerName == profile.name) {
            playSound("/Areas/Agricola/Content/audio/my-turn.wav");
        }

    },

    getPlayerColor: function (name) {
        return this.players[name].Color.toLowerCase();
    },


    update: function (game) {
        var oldMode = this.mode;
        if(game.Mode)
            this.mode = game.Mode;

        this.interrupt = game.Interrupt ? game.Interrupt : null;
        if (game.CurrentRound)
            this.currentRound = game.CurrentRound;

        // update the player objects
        this.updatePlayers(game);
        this.updateHand(game.MyHand);
        this.updateMajors(game);
        this.updateActivePlayer(game);
        this.updateRoundsBoard(game.Actions, game.ReservedResources);
        this.updateActions(game.Actions);

        if (this.mode == GameMode.Harvest) {
            this.updateHarvest(game, oldMode != GameMode.Harvest);
        }
        else {
            this.disableHarvestTab();
        }

        /*
        if (game.DirtyCards && game.DirtyCards.length > 0) {
            for (var c in game.DirtyCards) {
                var card = game.DirtyCards[c];
                Curator.updateCard(card);
            }
        }
        */
        
        if (!this.interrupt) {
            if (this.mode == GameMode.Over) {
                this.disableHarvestTab();
                this.display.addClass("game-over");
                this.myControls.find(".tab-score a").trigger("click");

                this.display.find(".leave-game-button").click(function () {
                    gameConn.server.leaveFinishedGame();
                });
            }
        }
        else {
            this.gameInterruptNotice(this.interrupt);
            if (this.interrupt.Player == profile.name) {
                this.actionHandler(this.interrupt.Id);
            }
        }
    },

    disableHarvestTab: function(){
        if (!this.myControls.find(".tab-harvest").hasClass("ui-state-disabled")) {
            this.myControls.tabs("disable", 2);
            var harvest = this.myControls.find(".tab-farm");
            harvest.find("a").trigger("click");
        }
    },

    updateMajors: function(game){
        if (game.MajorImprovementOwners) {
            this.majorImprovementOwners = game.MajorImprovementOwners;
            var display = this.myControls.find("#my-controls-majors");
            for (var i in game.MajorImprovementOwners) {
                var owner = game.MajorImprovementOwners[i];
                var major = $(display.find(".major[data-id='" + i + "']"));
                var label = major.find(".owned-by");
                label.hide();
                if (owner != null) {
                    var player = this.players[owner];
                    label.find("span").html(this.formatNameForSystemMessage(owner));
                    label.show();
                }
            }
        }
    },

    updateHarvest: function (game, newHarvest) {
        this.myControls.tabs("enable", 2);

        var harvest = this.myControls.find(".tab-harvest");
        if (newHarvest) {
            this.harvestPanel.startHarvest(game, this.myBoard.player, this.myBoard);
            harvest.find("a").trigger("click");
        }
        else {
            this.harvestPanel.update(game);
        }

        var popup;

        function close() {
            obj.screen.hide();
            popup.hide();
            popup.unbind("cancel");
            popup.unbind("submit");
        }

        var obj = this;
        obj.harvestPanel.unbind("submit");
        this.harvestPanel.submit(function (feedResources) {
            var farmyard = obj.myBoard;
            var fc = farmyard.clone();

            for (var r in feedResources) {
                var res = feedResources[r];
                if (AnimalResource[res.inType]) {
                    for (var i = 0; i < res.count; i++) {
                        if (!fc.animalManager.tryRemove(res.inType))
                            return false;
                    }
                }
                    
            }


            var animals = fc.animalManager.getAnimalCounts();
            var animalsFit = true;

            for (var res in AnimalResource) {
                if (animals[res] >= 2) {
                    animalsFit &= fc.animalManager.tryAssignNewAnimals(res, 1);
                    console.info("Breeding new ", res);
                }
            }

            if (!animalsFit) {
                console.info("Animals dont fit");

                var popup = obj.showPopup({
                    title: "Manage animals",
                    modules: [new ManageAnimalsPopupModule()],
                    farmyard: fc,
                    submit: function () {
                        var data = popup.getSubmitData();
                        var animalData = new AnimalCacheActionData(data.assignments, data.unassigned, data.cooked);
                        var harvestData = new HarvestData(feedResources, animalData);
                        gameConn.server.completeHarvest(harvestData);
                        obj.hidePopup();
                    }
                });
            }
            else {
                var animalData = new AnimalCacheActionData(fc.animalManager.getAssignments(), fc.animalManager.unassigned, fc.animalManager.cooked)
                var data = new HarvestData(feedResources, animalData);
                gameConn.server.completeHarvest(data);
            }

        })



    },

    /**
        If a new player is active update the player boards to reflect the active player
     */
    updateActivePlayer: function (data) {
        if (data.ActivePlayerName && this.activePlayer != data.ActivePlayerName) {
            this.activePlayer = data.ActivePlayerName;

            var playerInfos = this.display.find(".player-info .player");
            playerInfos.removeClass("active-player");
            playerInfos.filter("[data-player=\"" + data.ActivePlayerName + "\"]").addClass("active-player")

            /*
            if(this.isMyTurn()){
                var snd = new Audio("/Areas/Agricola/Content/audio/my-turn.wav");
                snd.play();
            }*/
        }

        if (this.isMyTurn() && this.mode == GameMode.Work)
            this.display.addClass("my-turn");
        else
            this.display.removeClass("my-turn");
    },

    updatePlayers: function (data) {
        if (data.Players) {
            for (var i in data.Players) {
                var player = data.Players[i];
                this.players[player.Name] = player;

                this.playerBoards[player.Name].update(player);

                if (player.Name == profile.name) {
                    this.updateScoreCard(player);
                }
                this.updateQuickCards(player.Name, player.OwnedCardIds, player.BeggingCards);

                player["farmyard"] = this.playerBoards[player.Name];
            }
        }

        if (data.StartingPlayer) {
            //this.display.find(".starting-player").html(this.formatNameForSystemMessage(data.StartingPlayer));
            var playerInfos = this.display.find(".player-info .player");
            playerInfos.removeClass("starting-player");
            playerInfos.filter("[data-player=\"" + data.StartingPlayer + "\"]").addClass("starting-player");
        }

    },

    updateQuickCards: function (player, ids, begCount) {
        var obj = this;
        var cards = player == profile.name 
            ? this.myControls.find(".quick-cards")
            : this.display.find(".player-info [data-player=\"" + player + "\"] #cards .quick-cards");
        cards.empty();

        if (begCount > 0) {
            var card = this.templates.quickCard.clone();
            card.find(".name").html("Begging Card");
            card.addClass("begging");
            card.attr("data-image", "/Areas/Agricola/Content/img/cards/begging-card.jpg");
            card.find(".other").html("x" + begCount);
            cards.append(card);
        }


        for (var id in ids) {
            var card = this.templates.quickCard.clone();
            var cardData = Curator.getCard(ids[id]);

            card.find(".name").html(cardData.Name)
            card.addClass(cardData.Type.toLowerCase());
            if (cardData.Image)
                card.attr("data-image", cardData.Image);
            cards.append(card);

            if (cardData.AnytimeAction) {
                card.addClass("active");
                card.click(wireQuickCardClick(cardData.Id, cardData.AnytimeAction.Id));
            }

            if (cardData.Points > 0) {
                var points = card.find(".points");
                points.find("div").text(cardData.Points);
                points.show();

            }
        }

        function wireQuickCardClick(cardId) {
            return function () {
                obj.anytimeActionHandler(cardId);
            }
        }

    },

    updateScoreCard: function(player){
        var scoreCard = player.ScoreCard;

        var card = this.myControls.find("#my-controls-score");
        
        for (var i in scoreCard) {
            var ui = card.find("." + i.toLowerCase());
            if (ui.length > 0) {
                ui.find(".points").text(scoreCard[i]);
            }
        }

        card.find(".base-total .points").text(
            scoreCard.Fields +
            scoreCard.Pastures +
            scoreCard.Grain +
            scoreCard.Vegetables +
            scoreCard.Sheep +
            scoreCard.Boar + 
            scoreCard.Cattle +
            scoreCard.UnusedSpace +
            scoreCard.FencedStables +
            scoreCard.Rooms +
            scoreCard.FamilyMembers +
            scoreCard.Begging);

        var bonusPoints = card.find(".bonus-points");
        bonusPoints.empty();
        var bonusTotal = card.find(".bonus-total");
        var bonusTotalPoints = 0;

        for (var i in scoreCard.BonusPoints) {
            var item = scoreCard.BonusPoints[i];
            var row = $("<div class='row'><label>" + getCdataAsText(item.Name) + ":</label><div>" + item.Count + "</div></div>")
            bonusPoints.append(row);
            bonusTotalPoints += parseInt(item.Count);
        }

       
        bonusTotal.find(".points").text(bonusTotalPoints);


        this.myControls.find(".tab-score .score").text(scoreCard.Total)

    },

    isMyTurn: function () {
        return this.activePlayer == profile.name;
    },

    actionClickHandler: function(action){
        if (this.mode == GameMode.Work && this.isMyTurn()){
            action = $(action);
            if (action.hasClass("disabled")) return;
            var actionId = action.attr("data-id");
            this.actionHandler(actionId, action);
        }
    },



    showPopup: function (params, actionId) {
        var obj = this;
        var player = this.players[profile.name];
        var farmyard = this.playerBoards[profile.name];

        this.screen.show();
        var popup = this.popups.master;
        popup.actionId = actionId;
        popup.player = player;

        popup.setFarmyard(params.farmyard ? params.farmyard : farmyard.clone());

        popup.setTitle(params.title ? params.title : "NO TITLE SET");

        if (params.submit)
            popup.submit(params.submit);

        var showParams = {
            submitActiveOnly: params.submitActiveOnly ? true : false,
            modules: params.modules,
            moduleParams: params.moduleParams,
            moduleButtonText: params.moduleButtonText,
            dialogue: params.dialogue
        };

        popup.show(showParams);
        popup.cancel(function () { obj.hidePopup(); });
        if(popup.ok)
            popup.ok(function () { obj.hidePopup(); })
        return popup;
    },

    hidePopup: function(){
        this.screen.hide();
        this.popups.master.hide();

        this.popups.master.unbind("ok");
        this.popups.master.unbind("cancel");
        this.popups.master.unbind("submit");
    },


    actionHandler: function (actionId, action) {
        if ((this.mode == GameMode.Work && this.isMyTurn())
            || (this.interrupt && this.interrupt.Player == profile.name)) {
            var obj = this;

            var player = this.players[profile.name];
            var farmyard = this.playerBoards[profile.name];
            var popup;
            var fc; //farmyard clone

            function close() {
                obj.hidePopup();
            }

            function showPopup() {
                obj.screen.show();
                popup.show.apply(popup, arguments);
                popup.cancel(close);

                $(document).keyup(keyClose)
            }

            function showMaster(params) {
                popup = obj.showPopup(params, actionId);
            }

            function keyClose(e) {
                if (e.keyCode == 27) {
                    close();
                }
            }

            if (!this.interrupt) {

                // build rooms
                if (actionId == 0) {
                    showMaster({
                        title: "Build rooms -and/or- build stables",
                        modules: [new BuildRoomsPopupModule(), new BuildStablesPopupModule()],
                        submit: function () {
                            var data = popup.getSubmitData();
                            gameConn.server.takeBuildRoomsAndStablesAction(actionId, new BuildRoomsAndStablesData(data.roomsData, data.stablesData));
                            close();
                        }
                    });
                }
                    // Starting player + minor Improvement
                else if (actionId == 1) {

                    if (!this.familyMode) {

                        showMaster({
                            title: "Starting player and also 1 minor improvement",
                            modules: [new CardPopupModule()],
                            moduleParams: [{
                                cards: this.myControls.find("#my-controls-cards").find(".minors .card").clone(),
                                allowNoSelection: true
                            }],
                            moduleButtonText: ["Minor <span>Improvement</span>"],
                            submit: function () {
                                var data = popup.getSubmitData();

                                var improvement = data.cardId != null ? new ImprovementActionData(data.cardId, data.paymentOption) : null;
                                gameConn.server.takeStartingPlayerAction(actionId, new StartingPlayerActionData(improvement));
                                close();
                            }
                        });
                    }
                    else {
                        gameConn.server.takeStartingPlayerAction(actionId, new StartingPlayerActionData(null));
                    }

                }

                    // Plow 1 Field
                else if (actionId == 3) {
                    var plowInfo = Curator.getPlowInfo(player, actionId);
                    var title = "Plow ";
                    if (plowInfo.maxFields > 1)
                        title += "up to "
                    title += plowInfo.maxFields + " field";
                    if (plowInfo.maxFields > 1)
                        title += "s";

                    showMaster({
                        title: title,
                        modules: [new PlowPopupModule()],
                        moduleParams: [{ plowInfo: plowInfo }],
                        submit: function () {
                            var data = popup.getSubmitData();
                            var sow = [];
                            gameConn.server.takePlowAndSowAction(actionId, new PlowAndSowActionData(data.plowData, sow, data.plowUsed));
                            close();
                        }
                    });
                }

                    // Buy occupation
                else if (actionId == 4 || actionId == 10 || actionId == 14 || actionId == 20) {
                    var occupationCost = Curator.getOccupationCost(player, actionId);
                    var title = "Play 1 occupation for " + (occupationCost == 0 ? "free." : occupationCost + " food.");

                    var modules = [new OccupationPopupModule()];
                    var moduleParams = [{ cards: this.myControls.find("#my-controls-cards").find(".occupations .card").clone() }];
                    var moduleButtonText = ["<span>Occupations</span>"];

                    if (actionId == 20 && this.currentRound >= 5 && Curator.canGrowFamily(player)) {
                        modules.push(new FamilyGrowthPopupModule());
                        moduleParams.push(null);
                        moduleButtonText.push(null);
                    }

                    showMaster({
                        title: title,
                        modules: modules,
                        moduleParams: moduleParams,
                        moduleButtonText: moduleButtonText,
                        submit: function () {
                            var data = popup.getSubmitData();

                            var cardId = data.cardId ? data.cardId : null;
                            var fg = data.familyGrowth ? true : false;
                            var occupation = new OccupationActionData(cardId, fg);

                            gameConn.server.takeOccupationAction(actionId, occupation);

                            close();
                        }
                    });
                }

                    // Build stables and bake 
                else if (actionId == 54) {
                    showMaster({
                        title: "Build 1 stable -and/or- bake",
                        modules: [
                            new BuildStablesPopupModule(),
                            new BakePopupModule()
                        ],
                        submit: function () {
                            var data = popup.getSubmitData();
                            var submitData = new BuildStableAndBakeActionData(data.stablesData, data.conversionData);
                            gameConn.server.takeBuildStableAndBakeAction(actionId, submitData);
                            close();
                        }
                    });
                }


                    // Take 1 building resource / "" and 1 food
                else if (actionId == 11 || actionId == 55) {
                    var title = actionId == 55
                            ? "Take 1 building resource of your choice and 1 food"
                            : "Take 1 building resource of your choice"

                    showMaster({
                        title: title,
                        modules: [new SelectResourcesPopupModule()],
                        moduleParams: [{
                            resources: [Resource.Wood, Resource.Clay, Resource.Reed, Resource.Stone],
                            numRequired: 1
                        }],
                        submit: function () {
                            close();
                            var data = popup.getSubmitData();
                            var rs = new BuildingResourcesActionData();
                            rs.resource1 = data.resources[0];
                            gameConn.server.takeBuildingResourcesAction(actionId, rs);
                        }
                    });

                }

                    // take 2 building resources
                else if (actionId == 60 || actionId == 64 || actionId == 70) {
                    
                    var title = (actionId == 60 || actionId == 64)
                            ? "Take 2 building resources of your choice"
                            : "Take 2 building resources of your choice -or- family growth";

                    var modules = [new SelectResourcesPopupModule()];

                    if (actionId == 70 && this.currentRound >= 5)
                        modules.push(this.popupModules.familyGrowth);

                    showMaster({
                        title: title,
                        modules: modules,
                        moduleParams: [{
                            resources: [Resource.Wood, Resource.Clay, Resource.Reed, Resource.Stone],
                            numRequired: 2
                        }],
                        submit: function () {
                            close();
                            var data = popup.getSubmitData();
                            var rs = new BuildingResourcesActionData();
                            rs.resource1 = data.resources[0];
                            rs.resource2 = data.resources[1];
                            rs.growth = data.familyGrowth;
                            gameConn.server.takeBuildingResourcesAction(actionId, rs);
                        }
                    });
                }

                    // Build room or traveling player
                else if (actionId == 21) {

                    showMaster({
                        title: "Build room -or- traveling players",
                        modules: [new BuildRoomsPopupModule(), new TravellingPlayersPopupModule()],
                        moduleParams: [null, { foodCount: this.actions[actionId].Cache[0].Count }],
                        submitActiveOnly: true,
                        submit: function () {
                            var data = popup.getSubmitData();
                            var takeFood = data.takeFood;
                            var actionData = new BuildRoomOrTravelingPlayersActionData(takeFood ? null : data.roomsData[0], takeFood ? true : false);

                            gameConn.server.takeBuildRoomOrTravelingPlayersAction(actionId, actionData);

                            close();
                        }
                    });
                }

                    // Animal Choice
                else if (actionId == 22) {
                    popup = this.popups.animalChoice;
                    fc = farmyard.clone();
                    popup.setFarmyard(fc);

                    popup.submit(function () {
                        obj.screen.hide();
                        popup.hide();
                        popup.unbind("cancel");
                        popup.unbind("submit");

                        var animalData = new AnimalCacheActionData(fc.animalManager.getAssignments(), fc.animalManager.unassigned, fc.animalManager.cooked);
                        var data = new AnimalChoiceActionData(popup.getSelection(), animalData);
                        gameConn.server.takeAnimalChoiceAction(actionId, data);

                    });

                    popup.cancel(function () {
                        obj.screen.hide();
                        popup.hide();

                        popup.unbind("cancel");
                        popup.unbind("submit");
                    });

                    showPopup(player);
                }

                    // Build fences
                else if (actionId == 100) {

                    showMaster({
                        title: "Build fences",
                        modules: [new BuildFencesPopupModule()],
                        submit: function () {
                            var data = popup.getSubmitData();
                            var animalData = new AnimalCacheActionData(data.assignments, data.unassigned, data.cooked);
                            var fences = [];
                            var f = data.fencesData;
                            for (var fence in f) {
                                if (f[fence]) {
                                    fences.push(parseInt(fence));
                                }
                            }

                            var submitData = new BuildFencesActionData(fences, animalData);
                            
                            gameConn.server.takeBuildFencesAction(actionId, submitData);
                            close();
                        }
                    });
                }

                    // take animals
                else if (actionId == 101 || actionId == 107 || actionId == 109) {
                    var num = this.actions[actionId].Cache[0].Count;
                    var type = this.actions[actionId].Cache[0].Type;
                    fc = farmyard.clone();

                    var x = fc.animalManager.tryAssignNewAnimals(type, num);

                    if (!x) {
                        showMaster({
                            title: "Manage animals",
                            modules: [new ManageAnimalsPopupModule()],
                            farmyard: fc, 
                            submit: function () {
                                var data = popup.getSubmitData();
                                gameConn.server.takeAnimalCacheAction(actionId, new AnimalCacheActionData(data.assignments, data.unassigned, data.cooked));
                                close();
                            }
                        });
                    }
                    else {
                        var data = new AnimalCacheActionData(fc.animalManager.getAssignments(), fc.animalManager.unassigned, fc.animalManager.cooked);
                        console.info(data);
                        gameConn.server.takeAnimalCacheAction(actionId, data);
                    }

                }

                    // 1 Major or Minor improvement
                else if (actionId == 102) {
                    var modules = [new CardPopupModule()],
                        moduleParams = [{ cards: this.myControls.find("#my-controls-majors").find(".major").clone() }],
                        moduleButtonText = ["Major <span>Improvement</span>"];

                    if (!this.familyMode) {
                        modules.push(new CardPopupModule());
                        moduleParams.push({ cards: this.myControls.find("#my-controls-cards").find(".minors .card").clone() });
                        moduleButtonText.push("Minor <span>Improvement</span>");
                    }

                    showMaster({
                        title: "Play 1 major or minor improvement",
                        modules: modules,
                        moduleParams: moduleParams,
                        moduleButtonText: moduleButtonText,
                        submitActiveOnly: true,
                        submit: function () {
                            var data = popup.getSubmitData();

                            var idata = new ImprovementActionData(data.cardId, data.paymentOption);

                            console.info(actionId, idata);
                            gameConn.server.takeImprovementAction(actionId, idata);
                            close();
                        }
                    });

                }

                    // sow and bake
                else if (actionId == 103) {
                    showMaster({
                        title: "Sow and/or bake bread",
                        modules: [new SowPopupModule(), new BakePopupModule()],
                        moduleButtonText: [null, "Bake Bread"],
                        submit: function () {
                            close();
                            var data = popup.getSubmitData();
                            console.info(data);
                            gameConn.server.takeSowAndBakeAction(actionId, new SowAndBakeActionData(data.sowData, data.conversionData));
                        }
                    });
                }

                    // Family grwoth also improvement
                else if (actionId == 104 || actionId == 111) {
                    var fgData = new FamilyGrowthActionData();
                    function submitFamilyGrowth() {
                        gameConn.server.takeFamilyGrowthAction(actionId, fgData);
                    }

                    if (actionId == 104 && !this.familyMode) {

                        showMaster({
                            title: "Family growth and also 1 minor improvement",
                            modules: [new CardPopupModule()],
                            moduleParams: [{
                                cards: this.myControls.find("#my-controls-cards").find(".minors .card").clone(),
                                allowNoSelection: true
                            }],
                            moduleButtonText: ["Minor <span>Improvement</span>"],
                            submit: function () {
                                var data = popup.getSubmitData();
                                var improvement = data.cardId != null ? new ImprovementActionData(data.cardId, data.paymentOption) : null;
                                fgData.improvementData = improvement;

                                submitFamilyGrowth();

                                close();
                            }
                        });
                    }
                    else {
                        submitFamilyGrowth();
                    }


                }

                    // Renovate also 1 Major or Minor improvement
                else if (actionId == 106 || actionId == 113) {
                    popup = this.popups.improvements;
                    fc = farmyard.clone();

                    var renoCost = Curator.getRenovationCost(player);
                    fc.payCosts(renoCost);


                    var modules = [],
                        moduleParams = [],
                        moduleButtonText = [],
                        title = "";


                    if (actionId == 106) {
                        title = "Renovation and play 1 major improvement";

                        modules.push(new CardPopupModule());
                        moduleParams.push({
                            cards: this.myControls.find("#my-controls-majors").find(".major").clone(),
                            allowNoSelection: true
                        });
                        moduleButtonText.push(["Major <span>Improvement</span>"]);

                        if (!this.familyMode) {
                            title = "Renovation and 1 major or minor improvement";

                            modules.push(new CardPopupModule());
                            moduleParams.push({
                                cards: this.myControls.find("#my-controls-cards").find(".minors .card").clone(),
                                allowNoSelection: true
                            });
                            moduleButtonText.push("Minor <span>Improvement</span>");
                        }
                    }
                    else if (actionId == 113) {
                        title = "Renovation and build fences";
                        modules.push(new BuildFencesPopupModule());
                    }

                    showMaster({
                        title: title,
                        farmyard: fc,
                        modules: modules,
                        moduleParams: moduleParams,
                        moduleButtonText: moduleButtonText,
                        submitActiveOnly: true,
                        submit: function () {
                            var data = popup.getSubmitData();
                            var submitData = new RenovationActionData();
                            submitData.improvementData = data.cardId == null ? null : new ImprovementActionData(data.cardId, data.paymentOption);

                            if (data.fencesData) {
                                var f = fc.addedFences;
                                var fences = [];

                                for (var fence in f) {
                                    if (f[fence]) {
                                        fences.push(parseInt(fence));
                                    }
                                }

                                var animalData = new AnimalCacheActionData(data.assignments, data.unassigned, data.cooked)
                                submitData.fenceData = fences.length == 0 ? null : new BuildFencesActionData(fences, animalData)
                            }


                            gameConn.server.takeRenovationAction(actionId, submitData);
                            close();
                        }
                    });
                }

                    // Plow and/or Sow
                else if (actionId == 112) {
                    var plowInfo = Curator.getPlowInfo(player, actionId);
                    var title = "Plow ";
                    if (plowInfo.maxFields > 1)
                        title += "up to "
                    title += plowInfo.maxFields + " field";
                    if (plowInfo.maxFields > 1)
                        title += "s";

                    showMaster({
                        title: title,
                        modules: [new PlowPopupModule(), new SowPopupModule()],
                        moduleParams: [{ plowInfo: plowInfo }],
                        submit : function () {
                            var data = popup.getSubmitData();
                            
                            gameConn.server.takePlowAndSowAction(actionId, new PlowAndSowActionData(data.plowData, data.sowData, data.plowUsed));
                            close();
                        }

                    });

                   
                }


                // Actions that have no requirement besides an available family member 
                else {
                    console.info("IN here");
                    var action = this.actions[actionId];

                    if (action.Cache && action.Cache.length > 0) {
                        var types = {};
                        var fc = farmyard.clone();
                        fc.personalSupply.clear();

                        for (var c in action.Cache) {
                            var cache = action.Cache[c];
                            var amount = types[cache.Type] ? types[cache.Type] : 0;
                            types[cache.Type] = amount + cache.Count;

                            fc.personalSupply.setResource(cache.Type, cache.Count);
                        }

                        var exchanges = Curator.getCacheExchanges(player, types, actionId);
                        if (exchanges) {


                            showMaster({
                                title: "You will gain these resources:",
                                modules: [new CacheExchangePopupModule()],
                                moduleParams: [{ exchanges: exchanges }],
                                moduleButtonText: ["Exchange Resources"],
                                farmyard: fc, 
                                submit: function () {
                                    close();
                                    var data = popup.getSubmitData();
                                    console.info(data);
                                    gameConn.server.takeCacheExchangeAction(actionId, new CacheExchangeData(data.conversionData) );
                                }
                            });
                            return;
                        }
                        
                    }

                        gameConn.server.takeAction(actionId);
                }
            }
            else {

                // Bake interrupt action
                if (actionId == InterruptActionId.Bake) {
                    showMaster({
                        title: "Bake bread",
                        modules: [new BakePopupModule()],
                        moduleParams: [{allowNoSelection: true}],
                        submit: function () {
                            close();
                            var data = popup.getSubmitData();
                            gameConn.server.takeBakeAction(actionId, new BakeActionData(data.conversionData));
                        }
                    });
                }
                    // Plow Field 
                else if (actionId == InterruptActionId.Plow) {
                    var optional = this.interrupt.Optional ? true : false;
                    showMaster({
                        title: "Plow 1 field",
                        modules: [new PlowPopupModule()],
                        moduleParams: [{optional: optional }],
                        submit: function () {
                            close();
                            var data = popup.getSubmitData();
                            gameConn.server.takePlowAndSowAction(actionId, new PlowAndSowActionData(data.plowData, null));
                        }
                    });
                }

                    // Build stable
                else if (actionId == InterruptActionId.BuildStable) {
                    showMaster({
                        title: "Build 1 stable",
                        modules: [new BuildStablesPopupModule()],
                        submit: function () {
                            close();
                            var data = popup.getSubmitData();
                            gameConn.server.takeBuildStableAction(actionId, new BuildStableActionData(data.stablesData));
                        }
                    });
                }

                    // Building Resource
                else if (actionId == InterruptActionId.SelectResources) {
                    var options = this.interrupt.Options;
                    var resources = [];
                    for (var i = 0; i < options.length; i++) {
                        resources.push(options[i].Type)
                    }

                    showMaster({
                        title: "Take 1 building resource",
                        modules: [new SelectResourcesPopupModule()],
                        moduleParams: [{
                            resources: resources,
                            numRequired: this.interrupt.NumRequired
                        }],
                        submit: function () {
                            close();
                            var data = popup.getSubmitData();
                            console.info(new SelectResourcesActionData(data.resources));
                            gameConn.server.takeSelectResourcesAction(actionId, new SelectResourcesActionData(data.resources));
                        }
                    });
                }

                // Occupation
                else if (actionId == InterruptActionId.Occupation) {
                    var occupationCost = Curator.getOccupationCost(player, actionId);

                    showMaster({
                        title: "Play 1 occupation for " + occupationCost + " food.",
                        modules: [new OccupationPopupModule],
                        moduleParams: [{
                            cards: this.myControls.find("#my-controls-cards").find(".occupations .card").clone(),
                            allowNoSelection: true
                        }],
                        moduleButtonText: ["<span>Occupations</span>"],
                        submit: function () {
                            var data = popup.getSubmitData();
                            var cardId = data.cardId ? data.cardId : null;
                            var occupation = new OccupationActionData(cardId, false);
                            console.info(occupation);
                            gameConn.server.takeOccupationAction(actionId, occupation);
                            close();
                        }
                    });
                }

                else if (actionId == InterruptActionId.BuildRoom) {
                    showMaster({
                        title: "Extend your stone house by 1 room.",
                        modules: [new BuildRoomsPopupModule()],
                        submit: function () {
                            var data = popup.getSubmitData();
                            var actionData = new BuildRoomData(data.roomsData[0]);

                            gameConn.server.takeBuildRoomAction(actionId, actionData);
                            close();
                        }
                    });
                }

                else if (actionId == InterruptActionId.AssignAnimals) {
                    fc = farmyard.clone();

                    var animals = this.interrupt.Animals;
                    var x = true;
                    for (var a in animals) {
                        var animal = animals[a];
                        var num = animal.Count;
                        var type = animal.Type;
                        x &= fc.animalManager.tryAssignNewAnimals(type, num);
                    }

                    if (!x) {
                        showMaster({
                            title: "Manage animals",
                            modules: [new ManageAnimalsPopupModule()],
                            farmyard: fc,
                            submit: function () {
                                close();

                                var data = popup.getSubmitData();
                                gameConn.server.takeAssignAnimalsAction(actionId, new AnimalCacheActionData(data.assignments, data.unassigned, data.cooked));
                            }
                        });
                    }
                    else {
                        var data = new AnimalCacheActionData(fc.animalManager.getAssignments(), fc.animalManager.unassigned, fc.animalManager.cooked);
                        gameConn.server.takeAssignAnimalsAction(actionId, data);
                    }

                }

                else if (actionId == InterruptActionId.PlayerChoice) {
                    var events = this.interrupt.Options;

                    showMaster({
                        title: "Player choice",
                        modules: [new PlayerChoicePopupModule()],
                        moduleParams: [{ events: events }],
                        submit: function () {
                            close();

                            var data = popup.getSubmitData();
                            console.info(data.option);
                            gameConn.server.takePlayerChoiceAction(actionId, new PlayerChoiceData(data.option));
                        }
                    })

                }
                else if (actionId == InterruptActionId.FencePasture) {
                    showMaster({
                        title: "Fence in 1 pasture",
                        modules: [new FencePasturePopupModule()],
                        submit: function () {
                            close();

                            var data = popup.getSubmitData();
                            var animalData = new AnimalCacheActionData(data.assignments, data.unassigned, data.cooked);
                            var fences = [];
                            var f = data.fencesData;
                            for (var fence in f) {
                                if (f[fence]) {
                                    fences.push(parseInt(fence));
                                }
                            }

                            var submitData = new BuildFencesActionData(fences, animalData);

                            gameConn.server.takeFencePastureAction(actionId, submitData);
                        }
                    })
                }

            }

        }
    },


    anytimeActionHandler: function (cardId ) {
        var obj = this;

        var player = this.players[profile.name];
        var farmyard = this.playerBoards[profile.name];
        var popup;


        var card = Curator.getCard(cardId);
        if (!card.AnytimeAction) return false;

        var anytimeActionData = card.AnytimeAction;
        
        if (!Curator.meetsPrereqs(player, card.AnytimeAction.Prerequisites)
            || (anytimeActionData.MaxUses > 0 && anytimeActionData.MaxUses <= anytimeActionData.Uses)) {
            popup = this.showPopup({
                title: "Requirements not met.",
                modules: [new PrereqNotMetPopupModule()],
                moduleParams: [{ card: card }],
                dialogue: true
            });


            return false;
        }

        var actionId = card.AnytimeAction.Id;

        // Cook action
        if (actionId == 600) {

            popup = this.showPopup({
                title: "Anytime resource conversions",
                modules: [new CookPopupModule()],
                submit: function () {
                    var data = popup.getSubmitData();
                    var fc = popup.farmyard;
          
                    var cookValues = data.conversionData;

                    for (var i in cookValues) {
                        var row = cookValues[i];
                        var type = row.inType;

                        if (AnimalResource[type]) {
                            for (var i = 0; i < row.count; i++) {
                                if (!fc.animalManager.tryCook(type)) {
                                    console.info("Failed to cook");
                                    return;
                                }
                            }
                        }
                    }

                    var animalData = new AnimalCacheActionData(fc.animalManager.getAssignments(), fc.animalManager.unassigned, fc.animalManager.cooked);
                    var submitData = new CookActionData(cookValues, animalData);

                    gameConn.server.takeCookAnytimeAction(actionId, cardId, submitData);
                    obj.hidePopup();
                }
            }, actionId);
        }
        else if (actionId == 601) {
            popup = this.showPopup({
                title: "Build 1 room for free",
                modules: [new BuildRoomsPopupModule()],
                submit: function () {
                    var data = popup.getSubmitData();
                    var actionData = new BuildRoomData(data.roomsData[0]);
           
                    gameConn.server.takeBuildRoomAnytimeAction(actionId, cardId, actionData);
                    obj.hidePopup();
                }
            }, actionId);
        }

    },


    updatePlayerBoard: function (player) {
        this.playerBoards[player.Name].update(player);
    },

    /**
      If a new round has been reached the new round action is added 
      to the round board.
     */
    updateRoundsBoard: function (actions, reserved) {
        console.info("reserved:", reserved);
        if (actions) {
            var roundActions = [];
            for (var i = 0; i < actions.length; i++) {
                var a = actions[i];
                if (a.Id >= 100 && a.Id <= 113) {
                    roundActions[roundActions.length] = actions[i].Id;
                }
            }

            var obj = this;
            var actionsDisplay = this.display.find(".rounds-board .round")
            var deck = this.display.find(".rounds-board .deck");
            for (var i = 0; i < roundActions.length; i++) {
                var action = $(actionsDisplay[i]);
                var round = action.attr("data-round")

                if (action.hasClass("empty")) {
                    action.find(".game-piece").remove();
                    action.removeClass("empty");
                    action.addClass("action");
                    var card = deck.find(".text-bubble[data-id='" + roundActions[i] + "']");
                    action.attr("data-id", roundActions[i]);
                    action.find(".text").replaceWith(card);
                    action.click(function () {
                        obj.actionClickHandler(this);
                    });

                    action.hover(this.actionHoverOverHandler, this.actionHoverOutHandler);
                }
            }
        }

        if (reserved) {
            actionsDisplay = this.display.find(".rounds-board .round.empty")

            for (var i = 0; i < reserved.length; i++) {
                var item = reserved[i];
                if (item != null) {
                    var action = $(actionsDisplay[i]);
                    this.placeReservedPieces(action, item);
                }
            }
        }
    },


    updateActions: function (actions) {
        var actionsDisplay = this.display.find(".action");
        actionsDisplay.removeClass("enabled");

        if (actions ) {
            for (var i = 0; i < actions.length; i++) {
                var action = actions[i];

                if (action.UserNames) {
                    this.actions[action.Id] = action;
                    var cache = action.Cache;

                    var display = actionsDisplay.filter("[data-id='" + action.Id + "']");

                    if (action.Id == 51) {
                        console.info(action);
                    }

                    if (cache && cache.length > 0) {
                        this.updateActionCache(display, cache);
                    }

                    if (action.UserNames.length > 0) {
                        var members = action.UserNames;
                        for (var b = 0; b < members.length; b++) {
                            // place family member
                            var c = this.players[members[b]].Color;
                            var m = this.createFamilyMember(c)
                            display.append(m);
                            display.addClass("disabled");
                            display.removeClass("enabled");
                            var aOffset = display.offset();
                            m.offset({
                                left: aOffset.left + display.width() / 2 - m.width() / 2,
                                top: aOffset.top + display.height() / 2 - m.height() / 2 - (m.height() * (b * .15))
                            })
                            useVh(m);
                        }
                    }
                    else {
                        display.find(".family-member").remove();
                    }
                }

            }
        }


        for (var i in this.actions) {
            action = this.actions[i];
            var display = actionsDisplay.filter("[data-id='" + action.Id + "']");

            var reason;
            if (action.UserNames) {
                if (action.UserNames.length == 0
                    && (reason = Curator.actionAvailable(this.players[profile.name], action))) {

                    if (!this.interrupt)
                        display.addClass("enabled");
                    display.removeClass("disabled");
                }
                else {
                    display.addClass("disabled");
                }
            }
        }

    },

    updateHand: function (handData) {
        var minorContainer = this.myControls.find(".card-list.minors");
        var occupationContainer = this.myControls.find(".card-list.occupations");
        var cardDisplay;
        if (handData) {
            this.myHand = handData;

            var obj = this;
            var minorData = handData.Minors;
            var occupationData = handData.Occupations;
            minorContainer.empty();
            for (var d in minorData) {
                var card = Curator.getCard(minorData[d]);
                cardDisplay = this.templates.improvementCard.clone();
                cardDisplay.addClass("minor");

                this.populateCardData(cardDisplay, card);

                minorContainer.append(cardDisplay);
            }
            minorContainer.append("<div class='clear'></div>")

            occupationContainer.empty();
            for (var d in occupationData) {
                var card = Curator.getCard(occupationData[d]);
                cardDisplay = this.templates.improvementCard.clone();
                cardDisplay.addClass("occupation");

                this.populateCardData(cardDisplay, card);

                occupationContainer.append(cardDisplay);
            }
            occupationContainer.append("<div class='clear'></div>")
        }

        var minors = minorContainer.find(".card");
        var occs = occupationContainer.find(".card");

        if (minors.length > 0 || occs.length > 0) {
            var card = $(minors.length > 0 ? minors[0] : occs[0]);
            minorContainer.width((card.outerWidth(true) * minors.length + 10) + "px");
            occupationContainer.width((card.outerWidth(true) * occs.length + 10) + "px");
        }

    },

    populateCardData: function (display, card) {
        var obj = this;
        if (card.Image) {
            preloadImage(card.Image);
            display.attr("data-image", card.Image);
        }

        var costs = display.find(".cost");
        var costRow = costs.find(".item").remove();

        var prereqs = card.Prerequisites;
        if (prereqs && prereqs.length > 0) {
            var prDisplay = display.find(".prerequisites");
            prDisplay.show();

            for (var i in prereqs) {
                var prereq = prereqs[i];
                switch (prereq.Type) {
                    case "OccupationPrerequisite":
                        var count = prereq.Count;
                        var text = "<br/>Occu-<br/>pation";
                        if (count > 1)
                            text += "s";

                        prDisplay.append("<div class='prereq'>" + count + " " + text + "</div>");
                        break;

                    case "ImprovementPrerequisite":
                        var count = prereq.Count;
                        var text = "<br/>improve-<br/>ment";
                        if (count > 1)
                            text += "s";

                        prDisplay.append("<div class='prereq'>" + count + " " + text + "</div>");
                        break;
                }
            }
        }

        var cardCosts = card.Costs;
        for (var i = 0; i < cardCosts.length; i++) {
            var cost = cardCosts[i];
            
            if (i != 0)
                costs.append("<div class='divider'></div>")

            if (cost.Resources) {
                for (var c in cost.Resources) {
                    var d = costRow.clone();
                    var resourceCost = cost.Resources[c];
                    d.find("span").text(resourceCost.Count);
                    d.find(".icon").addClass(resourceCost.Type.toLowerCase());
                    costs.append(d);
                }
            }
            else if (cost.Ids) {
                var d = costRow.clone();
                d.find(".icon").remove();
                d.addClass("text");
                d.find("span").html(cost.Text + "<br/>or");
                costs.append(d);
            }
        }

        var pointsDisplay = display.find(".points");
        if (card.Points == 0)
            pointsDisplay.hide();
        else
            pointsDisplay.find("div").text(card.Points);
        display.find(".name").html(card.Name["#cdata-section"] ? card.Name["#cdata-section"] : card.Name);
        display.find(".text-bubble").html(card.Text["#cdata-section"] ? card.Text["#cdata-section"] : card.Text);
        display.attr("data-id", card.Id);

    },


    /**
        Updates all actions that accumualte resources
        to display their resource counts
     */
    updateActionCache: function (action, cache) {
        cache = cache[0];
        action.find(".piece").remove();

        var count =  cache.Count;
        var countDisplay = action.find(".count");
        
        if (countDisplay.length == 0) {
            countDisplay = $("<div class='count'></div>");
            var n = action.find("div:first").append(countDisplay);
        }
        countDisplay.text("x" + count);
        
        
        for (var i = 0; i < count; i++) {

            var p = this.createResourcePiece(cache.Type.toLowerCase())
            action.append(p);
            var aOffset = action.offset();

            var leftOffset = action.width() / 2 - p.width() * 2;

            if (i < 12) {
                var c = (count > 12 ? 12 : count) - 1;
                leftOffset += p.width() * 1.5 - ((p.width() / 2) * Math.floor(c / 3));

                p.offset({
                    left: aOffset.left + p.width() * Math.floor(i/3) + leftOffset,
                    top: aOffset.top + action.height() - (p.height() * 1.4 + p.height() / 2) -
                        (i % 3 * (p.height() / 3.1)) + (action.hasClass("round") ? action.height() * .2 : 0)
                });
            }
            else if(i < 21){
                var ni = i - 12;
                var c = count - 12;
                if (c > 6) c = 6;

                leftOffset += p.width() * 1.5 - ((p.width()) * Math.floor(c / 4));
                if (ni >= 3 && ni < 6)
                    leftOffset += p.width();
                else if (ni > 5) 
                    leftOffset -= p.width();

                p.offset({
                    left: aOffset.left + p.width() * Math.floor(ni / 3) + leftOffset,
                    top: aOffset.top + action.height() - (p.height() * 1.4 - 2 ) - (ni % 3 * (p.height()/3.1))
                });
            }
            else if (i < 33) {
                var c = (count - 21) > 12 ? 12 : count - 21;
                var ni = i - 21;

                p.offset({
                    left: aOffset.left  + p.width() * Math.floor(ni / 3) + leftOffset,
                    top: aOffset.top + action.height() - (p.height() * 2 + p.height() / 2) - (ni % 3 * (p.height()/3.1) + p.height()*.38)
                });
            }
            else if (i < 42) {
                var ni = i - 33;
                var c = count - 12;
                if (c > 6) c = 6;
                c--;

                leftOffset += p.width() * 1.5 - ((p.width()) * Math.floor(c / 4));
                if (ni >= 3 && ni < 6)
                    leftOffset += p.width();
                else if (ni > 5)
                    leftOffset -= p.width();

                p.offset({
                    left: aOffset.left + p.width() * Math.floor(ni / 3) + leftOffset,
                    top: aOffset.top + action.height() - (p.height() * 2 - 2) - (ni % 3 * (p.height() / 3.1) + p.height() * .36)
                });
            }
            
            useVh(p);

        }

    },

    createFamilyMember: function (color, phantom) {
        var x = this.templates.pieces.familyMember.clone().show();
        x.addClass(color.toLowerCase());
        if (phantom) x.addClass("phantom");
        return x;
    },

    createResourcePiece: function (resource) {
        resource = resource.toLowerCase();
        var add = "";
        if (resource == "sheep" || resource == "boar" || resource == "cattle")
            add += "animal";
        return $("<div class='piece " + resource + " " + add + " game-piece'></div>");
    },

    actionHoverOverHandler: function () {
        $(this).find(".game-piece").stop().animate({
            opacity:.125
        }, 200);
    },

    actionHoverOutHandler: function () {
        $(this).find(".game-piece").stop().animate({
            opacity: 1
        }, 200);
    },

    gameInterruptNotice: function(interrupt){
        var s = "Waiting for " + this.formatNameForSystemMessage(interrupt.Player) + " to ";

        console.info("Interrupt notice:", interrupt);

        switch (interrupt.Type) {
            case "BakeAction":
                s += " bake bread.";
                break;

            case "BuildStableAction":
                s += " build a stable.";
                break;

            case "OccupationAction":
                s += " play an occupation.";
                break;

            case "PlowAction":
                s += " plow a field.";
                break;

            case "SelectResourcesAction":
                s += " select resources.";
                break;

            case "PlayerChoiceAction":
                s += " make a choice.";
                break;
            
            case "AssignAnimalsAction":
                s += " assign new animals.";
                break;
        }

        this.systemMessage(s);
    },

    /**
      Renders the delayed resources on the future round slots
    */
    placeReservedPieces: function(display, data){
        var index = 0;
        for (var player in data) {
            var color = this.getPlayerColor(player);
            var cacheList = data[player];

            var aOffset = display.offset();
            var stackSize = 0;
            for (var r in cacheList) {
                var cache = cacheList[r];

                for (var c = 0; c < cache.Count; c++) {
                    var p = this.createResourcePiece(cache.Type.toLowerCase())
                    display.append(p);
                    var leftOffset = display.width() / 2 - p.width() * 2.5 + (index * p.width());
                    var topOffset = display.height() - p.height() * 1.6 - (stackSize * p.height() * .33);

                    p.offset({ top: aOffset.top + topOffset, left: aOffset.left + leftOffset });
                    useVh(p);

                    if (stackSize == 0)
                        p.addClass("stack-base bb bg").addClass(color);
                    
                    stackSize++;
                }

            }

            index++;
        }
    },

    message: function (notice) {
        var obj = this;
        var player = notice.Subject;
        var verb = notice.Verb;
        var predicates = notice.Predicates;

        function processPredicates(func) {
            for (var i = 0; i < predicates.length; i++) {
                var predicate = predicates[i];
                func(predicate, i, predicates.length - 1);
            }
        }

        function delimit(currentIndex, lastIndex) {
            var s = "";
            if (currentIndex != 0) {
                if(lastIndex > 1)
                    s += ","
                s += " ";

                if (currentIndex == lastIndex) s += "and ";
            }
            return s;
        }

        var s = this.formatNameForSystemMessage(player);
        var first = true;
        switch (verb) {
            case "Debug":
                s += " uses unimplemented action.";
                break;

            case "Turn":
                
                if (player == profile.name && this.numPlayers > 1) {
                    playSound("/Areas/Agricola/Content/audio/my-turn.wav");
                }


                s += "'s turn.";
                break;

            case "Take":
                s += " takes ";
                processPredicates(function(predicate, current, last){
                    if (predicate.PredicateType == "ResourcePredicate") {
                        s += delimit(current, last);
                        s += predicate.Count + " ";
                        s += "<span class='icon medium " + predicate.Type.toLowerCase() + "'></span>";

                        if (current == last) s += ".";
                    }
                });
                break;
            case "TakeDelayed":
                s += " takes ";
                processPredicates(function (predicate, current, last) {
                    if (predicate.PredicateType == "ResourcePredicate") {
                        s += delimit(current, last);
                        s += predicate.Count + " ";
                        s += "<span class='icon medium " + predicate.Type.toLowerCase() + "'></span>";
                    }
                });
                s += " from delayed resources.";
                break;

            case "Starts":
                s += " goes first next round.";
                break;

            case "Build":
                s += " builds ";
                processPredicates(function (predicate) {
                    if (predicate.What == "Fence") {
                        s += predicate.Count + " fence";
                        if (predicate.Count > 1) s += "s";
                    }
                    else if (predicate.What == "Pasture" && predicate.Count > 0) {
                        s += " around " + predicate.Count + " pasture";
                        if (predicate.Count > 1) s += "s";
                        s += ".";
                    }
                    else if (predicate.What == "Room" && predicate.Count > 0) {
                        s += predicate.Count + " room";
                        if (predicate.Count > 1) s += "s";
                        s += ".";
                    }
                    else if (predicate.What == "Stable" && predicate.Count > 0) {
                        s += predicate.Count + " stable";
                        if (predicate.Count > 1) s += "s";
                        s += ".";
                    }

                });
                break;

            case "Plow":
                s += " plows ";

                processPredicates(function (predicate) {
                    s += predicate.Count + " field";
                    if (predicate.Count > 1) s += "s";
                    s += ".";
                });
                break;

            case "Sow":
                s += " sows ";
                processPredicates(function (predicate, current, last) {
                    if (current > 0) s += " and ";
                    s += predicate.Count + " <span class='icon medium " + predicate.Type.toLowerCase() + "'></span> field";
                    if (predicate.Count > 1) s += "s";
                    if (current == last) s += ".";
                });
                break;

            case "PurchaseImprovement":
            case "PlayOccupation":
                s += " purchases ";
                processPredicates(function (predicate) {
                    var card = IdLookup[predicate.Id];
                    var name = getCdataAsText(card.Name);
                    s += "<span data-image='" + card.Image + "' data-always-show='true'>" + name + ".";
                });
                break;

            case "GrowFamily":
                s += " grows their family.";
                break;

            case "Renovate":
                s += "'s home has been renovated to ";
                processPredicates(function (predicate) {
                    s += predicate.Value + ".";
                });
                break;

            case "Bake":
                s += " bakes ";
                processPredicates(function (predicate) {
                    s += predicate.Input.Count + " "
                        + predicate.Input.Type + " into "
                        + predicate.Output.Count + " "
                        + predicate.Output.Type + ".";
                });
                break;
            case "Fed":
                processPredicates(function (predicate, currentIndex, lastIndex) {
                    s += delimit(currentIndex, lastIndex);

                    if (predicate.PredicateType == "ResourcePredicate") {
                        if (currentIndex == 0)
                            s += " fed their family with ";

                        s += predicate.Count + " " + obj.formatResourceAsIcon(predicate.Type);
                    }
                    else if (predicate.PredicateType == "IdPredicate") {
                        s += " begged for " + predicate.Id + " " + obj.formatResourceAsIcon(Resource.Food);

                    }
                });

                s += ".";
                break;

            case "Converted":
                s += " converted ";
                processPredicates(function (predicate, currentIndex, lastIndex) {
                    s += delimit(currentIndex, lastIndex);

                    s += predicate.Input.Count + " "
                        + "<span class='icon medium " + predicate.Input.Type.toLowerCase() + "'></span> into "
                        + predicate.Output.Count + " "
                        + "<span class='icon medium " + predicate.Output.Type.toLowerCase() + "'></span>";
                });

                s += ".";
                break;

            case "Harvested":
                s += " harvested ";
                processPredicates(function (predicate, currentIndex, lastIndex) {
                    s += delimit(currentIndex, lastIndex);

                    s += predicate.Count + " ";
                    s += "<span class='icon medium " + predicate.Type.toLowerCase() + "'></span>";

                });
                s += ".";
                break;

            case "FreeAnimals":
                s += " freed ";
                processPredicates(function (predicate, currentIndex, lastIndex) {
                    s += delimit(currentIndex, lastIndex);

                    s += predicate.Count + " ";
                    s += "<span class='icon medium " + predicate.Type.toLowerCase() + "'></span>";

                });
                s += ".";
                break;
        }
       
        this.systemMessage(s)
    },

    formatResourceAsIcon: function( name){
        return "<span class='icon medium " + name.toLowerCase() + "'></span>";
    },

    formatNameForSystemMessage: function (name) {
        return "<span class='" + this.getPlayerColor(name) + " fg'>" + name + "</span>";
    },
    /**
        Adds a message to the system log and keeps the user autoscrolled
        if they are currently at the end of the log.
     */
    systemMessage: function (message) {
        var chat = this.chat.system;
        var oldScrollHeight = chat[0].scrollHeight;
        var outerHeight = chat.outerHeight();
        var scroll = (oldScrollHeight - chat.scrollTop() <= outerHeight + 10);

        this.chat.system.append("<p class='" + this.getPlayerColor(this.activePlayer) + "'>" + message + "</p>");

        if (scroll || oldScrollHeight < outerHeight) {
            chat.scrollTop( chat[0].scrollHeight );
        }
    },


    showPlayerError: function (error) {
        this.screen.show();
        console.info(error);
    }
}

function BuildingResourcesActionData() {
    this.growth = null;
    this.resource1 = null;
    this.resource2 = null;
}

function BuildRoomsAndStablesData(roomData, stableData) {
    this.roomData = roomData;
    this.stableData = stableData;
}

function PlowAndSowActionData(fields, sow, plowUsed) {
    this.fields = fields;
    this.sow = sow;
    this.plowUsed = plowUsed;
}

function BuildRoomOrTravelingPlayersActionData(room, takeFood) {
    this.room = room;
    this.takeFood = takeFood;
}

function BuildFencesActionData(fences, animalData) {
    this.fences = fences;
    this.animalData = animalData ? animalData : null;
}

function AnimalCacheActionData(assignments, free, cook) {
    this.assignments = assignments;
    this.free = free;
    this.cook = cook;
}

function HarvestData(feedResources, animalData) {
    this.feedResources = feedResources;
    this.animalData = animalData;
}

function SowAndBakeActionData(sow, bakeData) {
    this.sow = sow;
    this.bakeData = bakeData;
}

function ImprovementActionData(id, paymentOption) {
    this.id = id;
    this.paymentOption = paymentOption;
}

function RenovationActionData() {
    this.fenceData = null;
    this.improvementData = null;
}

function FamilyGrowthActionData() {
    this.improvementData = null;
}

function AnimalChoiceActionData(option, animalData){
    this.option = option;
    this.animalData = animalData;
}

function BuildStableAndBakeActionData(stableData, bakeData) {
    this.stableData = stableData;
    this.bakeData = bakeData;
}

function BakeActionData(bakeData) {
    this.bakeData = bakeData;
}

function CookActionData(resources, animalData) {
    this.resources = resources;
    this.animalData = animalData;
}

function OccupationActionData(id, familyGrowth) {
    this.id = id;
    this.familyGrowth = familyGrowth;
}

function StartingPlayerActionData(improvementData) {
    this.improvementData = improvementData;
}

function BuildStableActionData(stableData) {
    this.stableData = stableData;
}

function SelectResourcesActionData(resources) {
    this.resources = resources;
}

function BuildRoomData(roomData) {
    this.roomData = roomData;
}

function ResourceConversionData(id, count, inType, inAmount, outType) {
    this.id = id;
    this.count = count;
    this.inType = inType;
    this.inAmount = inAmount;
    this.outType = outType;
}

function PlayerChoiceData(choice) {
    this.choice = choice;
}

function CacheExchangeData(exchanges){
    this.exchanges = exchanges;
}