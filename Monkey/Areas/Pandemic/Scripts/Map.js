function Map(display) {
    this.display = display;
    this.cityDisplaysContainer = display.find(".cities");
    this.cityDisplays = null;
    this.cities = null;
    this.citiesById = null;
    this.links = null;

 
    this.mode = Map.Mode.Player;

    var activePlayer = null;
    var menu = new RadialMenu(this.display.find(".menu"));

    this.init = function (definition) {
        var obj = this,
            width = $(window).width(),
            scale = width / 1920,
            template = this.cityDisplaysContainer.find(".city").remove();

        this.cities = {};
        this.citiesById = [];
        this.links = definition.Links;

        for (var cityName in definition.Cities) {
            var city = definition.Cities[cityName];
            this.cities[cityName] = city;
            this.citiesById[city.Id] = city;
            var hotspot = template.clone();
            hotspot.addClass(city.Color.toLowerCase());
            hotspot.attr("data-id", city.Id);

            if (city.ResearchStation)
                hotspot.addClass("research");

            this.cityDisplaysContainer.append(hotspot);
            city.display = hotspot;

            var left = pxToVw((city.X * scale) - hotspot.width() / 2);
            var top = pxToVw((city.Y * scale) - hotspot.height() / 2);
            hotspot.css("left", left + "vw");
            hotspot.css("top", top + "vw");


            (function (cityId) {
                var mouseDownTimeout;
                hotspot.on("click", function () {
                    if(obj.mode != Map.Mode.Player)
                        return;

                    var node = $(this)
                    var city = obj.citiesById[cityId];
                    var id = node.attr("data-id");
                    if (node.hasClass("active")) {
                        if (obj.areNeighbors(activePlayer.Location, id)) {
                            gameConn.server.move(id);
                        }
                        else if (activePlayer.Location == city.Id) {
                            processTreatDisease(city);
                        }
                    }
                });

                hotspot.on("mousedown", function (e) {
                    if(obj.mode != Map.Mode.Player)
                        return;
                        
                    var node = $(this);

                    mouseDownTimeout = setTimeout(function () {
                        if (node.hasClass("active")) {
                            obj.showMenu(node);
                        }
                    }, 0);
                });

                hotspot.on("mouseup", function () {
                    if(obj.mode != Map.Mode.Player)
                        return;

                    clearTimeout(mouseDownTimeout);
                });
            })(city.Id);
            this.updateDiseaseDisplay(city);
        }

        this.cityDisplays = this.cityDisplaysContainer.find(".city");

        this.updateDiseaseCounters();

        menu.selected( menuSelectionHandler );
    }


    this.updateCities = function (nodes) {
        for (var n in nodes) {
            var node = nodes[n];
            var city = this.citiesById[node.Id];
            if(node.DiseaseCounters)
                city.DiseaseCounters = node.DiseaseCounters;

            if (node.ResearchStation != undefined) {
                city.ResearchStation = node.ResearchStation;
                city.display.toggleClass("research", city.ResearchStation);
            }

            if(city.DiseaseCounters)
                this.updateDiseaseDisplay(city);
        }

        this.updateDiseaseCounters();

    }


    this.updateDiseaseCounters = function () {
        var red = 0, black = 0, blue = 0, yellow = 0;

        for (var c in this.cities) {
            var city = this.cities[c];
            var d = city.DiseaseCounters;

            red += d[DiseaseColor.Red];
            black += d[DiseaseColor.Black];
            blue += d[DiseaseColor.Blue];
            yellow += d[DiseaseColor.Yellow];
        }

        this.display.find(".ui .cure[data-color=Red] .count").text(24 - red);
        this.display.find(".ui .cure[data-color=Black] .count").text(24 - black);
        this.display.find(".ui .cure[data-color=Blue] .count").text(24 - blue);
        this.display.find(".ui .cure[data-color=Yellow] .count").text(24 - yellow);
    }

    /**
      *  Updates the disease displays on all cities
      */
    this.updateDiseaseDisplay = function(city){
        var id = city.Id;
        var cityDisplay = this.display.find(".city[data-id=" + id + "]");
        var container = cityDisplay.find(".diseases");
        container.empty();
        
        var data = city.DiseaseCounters;
        var total = data[0] + data[1] + data[2] + data[3];
        cityDisplay.removeClass("infected1 infected2 infected3");
        if (total > 0) {
            var color,
                count = 0;
                highest = 0;
            for (var x = 0; x < 4; x++) {
                if (x == 0) color = "yellow";
                else if (x == 1) color = "red";
                else if (x == 2) color = "blue";
                else if (x == 3) color = "black";

                for (var d = 0; d < data[x]; d++) {
                    count++;
                    var cube = $("<div class=\"disease " + color + "\"></div>");
                    var left = Math.sin(count * Math.PI / 6) * 1.22;
                    var top = Math.cos(count * Math.PI / 6) * 1.22;
                    cube.css("left", (left - .5) + "vw");
                    cube.css("top", (top - .5) + "vw");
                    container.append(cube);
                }

                if (data[x] > highest)
                    highest = data[x];
            }
            cityDisplay.addClass("infected" + highest);
        }
    }

    /**
        Deactivates all the nodes on the map.
    */
    this.deactivateNodes = function () {
        this.cityDisplays.removeClass("active");
    },
    
    /**
     Activates all nodes that are actionable by the given player
    */
    this.activateNodesForPlayer = function (player) {
        this.mode = Map.Mode.Player;
        activePlayer = player;
        this.deactivateNodes();

        var playerCity = Game.cityData[activePlayer.Location],
            hasCityCard = player.hasCityCard();

        var activateHomeNode = hasDisease(playerCity)
            || (playerCity.ResearchStation && activePlayer.canCureDisease())
            || (activePlayer.hasCard(playerCity.Id) || (activePlayer.Role.Name == PlayerRole.OperationsExpert && !playerCity.ResearchStation))
            || canTrade();

        if (player.Role.Name == PlayerRole.OperationsExpert && hasCityCard && playerCity.ResearchStation) {
            this.activateNodes();
        }
        else {
            for (var i in player.hand.cards) {
                var id = player.hand.cards[i].id;
                if (player.Location == id) {
                    this.cityDisplaysContainer.find(".city").addClass("active");

                    return;
                }
                else {
                    this.activateNode(id);
                }
            }
        }


        if (playerCity.ResearchStation) {
            for (var c in Game.cityData) {
                c = Game.cityData[c];
                if (c.ResearchStation)
                    this.activateNode(c.Id);
            }
        }

        var cons = this.getNeighbors(player.Location);
        for (var c in cons) {
            this.activateNode(cons[c]);
        }

        if (activateHomeNode){
            this.activateNode(player.Location);
        }
        else {
            this.cityDisplaysContainer.find("[data-id=\"" + activePlayer.Location + "\"]").removeClass("active");
        }
    }


    this.activateNodesForGovernmentGrant = function (callback) {
        this.mode = Map.Mode.CitySelection;
        var obj = this;

        for (var c in Game.cityData) {
            c = Game.cityData[c];
            if (!c.ResearchStation) {
                var node = this.activateNode(c.Id);
                node.one("click", cityClickHandler);
            }
        }

        function cityClickHandler() {
            var id = parseInt($(this).attr("data-id"));
            if (callback) callback(id);
            obj.cityDisplaysContainer.find(".city").off("click", cityClickHandler);
        }
    };

    this.activateNodesForResearchStationRemoval = function (callback) {
        this.mode = Map.Mode.CitySelection;
        var obj = this;

        for (var c in Game.cityData) {
            c = Game.cityData[c];
            if (c.ResearchStation) {
                var node = this.activateNode(c.Id);
                node.one("click", cityClickHandler);
            }
        }

        function cityClickHandler() {
            var id = parseInt($(this).attr("data-id"));
            if (callback) callback(id);
            obj.cityDisplaysContainer.find(".city").off("click", cityClickHandler);
        }
    };
    
    this.activateNodesForCitySelect = function (callback, exemptions) {
        this.mode = Map.Mode.CitySelection;
        var nodes = this.activateNodes();
        nodes.one("click", cityClickHandler);

        if(exemptions){
            for (var i in exemptions) {
                var id = exemptions[i];
                nodes.filter("[data-id=\"" + id + "\"]").removeClass("active").off("click", cityClickHandler);
            }
        }

        function cityClickHandler() {
            var id = parseInt($(this).attr("data-id"));
            if (callback) callback(id);
            nodes.off("click", cityClickHandler);
        }
    }



    this.activatePlayersForSelection = function (callback) {
        var obj = this;
        console.info("Activate players for selection");
        this.mode = Map.Mode.PlayerSelection;
        this.display.addClass("player-select");
        var players = this.display.find(".player-piece").on("click", playerClickHandler);

        function playerClickHandler(e) {
            console.info("IN here");
            e.stopPropagation();
            if (callback) {
                var name = $(this).attr("data-name");
                callback(name);
            }
            players.off("click", playerClickHandler);
            obj.display.removeClass("player-select");
        }

    }

    this.activateNodes = function(){
        return this.cityDisplaysContainer.find(".city").addClass("active");
    }

    this.activateNode = function (id) {
        var node = this.cityDisplaysContainer.find("[data-id=\"" + id + "\"]");
        node.addClass("active");
        return node;
    };

    this.getNeighbors = function (nodeId) {
        var cons = [];
        for (var l in this.links) {
            var link = this.links[l];
            if (link.A == nodeId)
                cons.push(link.B);
            if (link.B == nodeId)
                cons.push(link.A);
        }
        return cons;
    }

    this.areNeighbors = function (city1Id, city2Id) {
        var neighbors = this.getNeighbors(city1Id);
        for (var n in neighbors) {
            var neighbor = neighbors[n];
            if (neighbor == city2Id)
                return true;
        }
        return false;
    }

    /**
     * Positions the menu 
     */
    this.showMenu = function(node){
        var id = node.attr("data-id"),
            destination = Game.cityData[id],
            source = Game.cityData[activePlayer.Location],
            options = [PlayerAction.None, PlayerAction.None, PlayerAction.None, PlayerAction.None],
            hasCityCard = activePlayer.hasCityCard();
        

        if (source.Id == destination.Id) {
            if (hasDisease(source))
                options[RadialMenu.Up] = PlayerAction.TreatDisease;

            if (destination.ResearchStation) {
                if (activePlayer.canCureDisease())
                    options[RadialMenu.Right] = PlayerAction.DiscoverCure;
            }
            else if ((activePlayer.hasCard(destination.Id) || (activePlayer.Role.Name == PlayerRole.OperationsExpert))
                && !destination.ResearchStation)
                options[RadialMenu.Right] = PlayerAction.BuildResearchStation;

            if (canTrade())
                options[RadialMenu.Down] = PlayerAction.ShareKnowledge;

        }
        else if (this.areNeighbors(id, activePlayer.Location)) {
            options[RadialMenu.Down] = PlayerAction.Drive;
        }
        else  {
            if (source.ResearchStation && destination.ResearchStation) {
                options[RadialMenu.Down] = PlayerAction.ShuttleFlight;
            }
            else {
                if (activePlayer.hasCard(source.Id) || (activePlayer.Role.Name == PlayerRole.OperationsExpert && hasCityCard))
                    options[RadialMenu.Left] = PlayerAction.CharterFlight;

                if (activePlayer.hasCard(destination.Id))
                {
                    options[RadialMenu.Down] = PlayerAction.DirectFlight;
                }
            }
        }


        //else if()

        menu.prepare(destination, options);

    }


    function hasDisease(city) {
        for (var dc in city.DiseaseCounters) {
            if(city.DiseaseCounters[dc] > 0)
                return true;
        }
        return false;
    }

    function canTrade() {
        var together = false,
            otherHasCard = false,
            researcher = activePlayer.Role.Name == PlayerRole.Researcher ? activePlayer : null;

        for (var p in Game.players) {
            var player = Game.players[p];
            if (player != activePlayer && player.Location == activePlayer.Location) {
                if (player.Role.Name == PlayerRole.Researcher)
                    researcher = player;
                together = true;
                if (player.hasCard(activePlayer.Location)) {
                    otherHasCard = true;
                    break;
                }
            }
        }

        if (together
            && ((activePlayer.hasCard(activePlayer.Location) || otherHasCard)
            || (researcher != null && researcher.hand.cards.length > 0))) {
            return true;
        }
        return false;
    }

    function shuttlePossible(sourceCity) {
        if (sourceCity.ResearchStation) {
            for (var c in Game.cityData) {
                var dest = Game.cityData[c];
                if (dest.ResearchStation)
                    return true;
            }
            return false;
        }
    }

    function menuSelectionHandler(val, city) {
        switch (val) {
            case PlayerAction.Drive:
                gameConn.server.move(city.Id);
                break;

            case PlayerAction.CharterFlight:
                processCharterFlight(city);
                break;

            case PlayerAction.DirectFlight:
                gameConn.server.directFlight(city.Id);
                break;

            case PlayerAction.ShuttleFlight:
                gameConn.server.shuttleFlight(city.Id);
                break;

            case PlayerAction.BuildResearchStation:
                gameConn.server.buildResearchStation();
                break;

            case PlayerAction.TreatDisease:
                processTreatDisease(city);
                break;

            case PlayerAction.DiscoverCure:
                processDiscoverCure();
                break;

            case PlayerAction.ShareKnowledge:
                processTrade();
                break;
        }
    }


    function processCharterFlight(city) {
        if(activePlayer.Role.Name != PlayerRole.OperationsExpert)
            gameConn.server.charterFlight(city.Id, activePlayer.Location);
        else {
            activePlayer.hand.startSelection(1,
            "Charter a flight to " + city.Name + " by discarding one city card.",
            {
                cityOnly: true,
                selectionCallback: function (ids) {
                    gameConn.server.charterFlight(city.Id, ids[0]);
                }
            });
        }
    }

    function processTreatDisease(city) {
        console.info(city);
        var typeCount = 0;
        var single = [];
        for (var i in city.DiseaseCounters) {
            var count = city.DiseaseCounters[i];
            if (count > 0) {
                typeCount++;
                single = i;
            }
        }
        if (typeCount == 0)
            return;

        if (typeCount == 1) {
            gameConn.server.treatDiseases([single]);
        }
        else {
            var ui = OverlayManager.overlays.treat;
            ui.setCity(city);
            OverlayManager.showOverlay(ui)
        }
    }

    function processDiscoverCure() {
        var cardsNeeded = activePlayer.Role.Name == PlayerRole.Scientist ? 4 : 5;
        activePlayer.hand.startSelection(cardsNeeded,
            "Select " + cardsNeeded + " cards of the same color to cure the disease.",
            {
                cityOnly: true,
                selectionCallback: function (ids) {
                    gameConn.server.discoverCure(ids);
                }
            });
    }

    function processTrade() {

        var id = activePlayer.Location,
            ui = OverlayManager.overlays.trade,
            myCard = activePlayer.hasCard(id),
            players = [],
            myCards = [];
           
        if (activePlayer.Role.Name == PlayerRole.Researcher) {
            for (var c in activePlayer.hand.cards) {
                var card = activePlayer.hand.cards[c];
                myCards.push(card.id);
            }
        }
        else if (myCard) {
            myCards.push(id);
        }

        players.push({
            player: activePlayer,
            cards: myCards
        })

        for (var p in Game.players) {
            var player = Game.players[p];
            if (player.Location == activePlayer.Location && player != activePlayer)
            {
                var cards = [];

                if (player.Role.Name == PlayerRole.Researcher) {
                    for (var c in player.hand.cards) {
                        var card = player.hand.cards[c];
                        cards.push(card.id);
                    }
                }
                else if (!myCard) {
                    if (player.hasCard(id))
                        cards.push(id);
                }

                if (cards.length > 0 || myCards.length > 0) {
                    players.push({
                        player: player,
                        cards: cards
                    });
                }

            }
        }

        OverlayManager.showOverlay(ui);
        ui.setTradeOptions(players);
    }

}

Map.Mode = {
    Player: 0,
    CitySelection: 1,
    PlayerSelection: 2
}
var PlayerAction = 
{
    None: "",
    Drive: "Drive or Ferry",
    DirectFlight: "Direct Flight",
    CharterFlight: "Charter Flight",
    ShuttleFlight: "Shuttle Flight",
    BuildResearchStation: "Build Research Station",
    TreatDisease: "Treat Disease",
    ShareKnowledge: "Share Knowledge",
    DiscoverCure: "Discover Cure"
}