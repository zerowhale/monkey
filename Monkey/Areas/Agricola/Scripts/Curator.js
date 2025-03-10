var Curator = {
    game: null,



    /**
     * Checks if the given action can be used by the player
     */
    actionAvailable: function (player, action) {
        switch (action.Id) {
            case 0: // Build Room(s) and/or Stable(s)
                return this.canBuildRoom(player) || this.canBuildStable(player);

            case 3: // Plow 1 Field
                return this.canPlow(player);

            case 54: // Build Stable and/or Bake Bread
                return this.canBuildStable(player, 1) || this.canBake(player);

            case 100: // Build Fences
                return this.canBuildFences(player);

            case 102: // Buy Improvement
                return  this.canAffordAnyMajor(player) || (!this.game.familyMode && this.canAffordAnyMinor(player));

            case 103: // Sow and/or Bake
                return this.canSow(player) || this.canBake(player);

            case 104: // Family Growth also 1 Minor Improvement
                return this.canGrowFamily(player);

            case 106: // After Renovation also 1 Major or Minor improvement
                return this.canRenovate(player);

            case 111: // Family Growth without space
                return this.canGrowFamily(player, true);

            case 112: // Plow 1 Field and/or Sow
                return this.canPlow(player) || this.canSow(player);

            case 113: // After Renovation also Fences
                return this.canRenovate(player);
        }

        return true;
    },

    canGrowFamily: function (player, withoutSpace) {
        var board = this.game.playerBoards[player.Name];
        return (withoutSpace || board.rooms.length > player.FamilySize)
            && player.FamilySize < 5;
    },

    canRenovate: function (player) {
        let board = this.game.playerBoards[player.Name],
            roomCount = board.rooms.length,
            reedCost = 1;

        if (player.OwnedCardIds.includes(CardId.Thatcher))
            reedCost--;

        switch (player.Farmyard.HouseType) {
            case HouseType.Wood:
                return player.Clay >= roomCount && player.Reed >= reedCost;
                break;
            case HouseType.Clay:
                return player.Stone >= roomCount && player.Reed >= reedCost;
                break;
        }
        return false;

    },

    canBuildFences: function(player){
        var board = this.game.playerBoards[player.Name];
        var fenceCount = board.getFenceCount();
        return fenceCount < PlayerBoard.prototype.MAX_FENCES && player.Wood > 0;
    },

    canSow: function(player){
        var board = this.game.playerBoards[player.Name];
        return board.canSowField()
            && (player.Grain > 0 || player.Vegetables > 0);

    },

    canAffordAnyMajor: function(player){
        for (var id in this.game.majorImprovementOwners) {
            if (this.game.majorImprovementOwners[id] == null) {
                if(this.canAfford(player, id))
                    return true;
            }
        }
        return false;
    },

    canAffordAnyMinor: function (player) {
        var ms = this.game.myHand.Minors;
        for (var x in ms) {
            var id = ms[x];
            if (this.canAfford(player, id))
                return true;

        }
        return false;
    },

    /**
        Checks if the player can bake
    */
    canBake: function (player) {
        if (player.Grain <= 0)
            return false;

        for (var c in player.OwnedCardIds) {
            var item = IdLookup[player.OwnedCardIds[c]];
            if ((item.Type == CardType.Major || item.Type == CardType.Minor)
                && item.BakeProperties) {
                return true;
            }
        }

        
        return false;
    },

    canCook: function (player) {
        for (var c in player.OwnedCardIds) {
            var card = IdLookup[player.OwnedCardIds[c]];
            if (card.cooks) {
                return true;
            }
        }

        return false;
    },

    /**
        Checks that the player can plow.
    */
    canPlow: function (player) {
        let board = this.game.playerBoards[player.Name];
        return board.canPlowField();
    },

    canBuildRoom: function(player){
        let board = this.game.playerBoards[player.Name];
        return this.canAffordRoom(player) && board.canBuildRoom();
    },

    canAffordRoom: function (player) {
        let costs = this.getRoomCost(player);
        for (let c in costs) {
            let cost = costs[c];
            if (player[cost.type] < parseInt(cost.amount))
                return false;
        }

        return true;
    },

    canBuildStable: function (player, woodPerStable) {
        if (!player.farmyard.canBuildStable())
            return false;

        if (isNaN(woodPerStable))
            woodPerStable = 2;

        return player.Wood >= woodPerStable;
    },

    getStableBuildLimit: function(player, actionId){
        if (actionId == 54)
            return 1;
        else
            return PlayerBoard.prototype.MAX_STABLES;
    },

    isImprovementAvailable: function(id){
        var improvement = IdLookup[id];
        if(!improvement)
            return false;

        if (improvement.Type == CardType.Major) {
            return this.game.majorImprovementOwners[id] == null;
        }
        else {
            return true;
        }
        return false;
    },

    getBakeOptions: function (player) {
        function buildOption(id, inType, inAmount, inLimit, outType, outAmount) {
            return new ResourceConversion(id, inType, inAmount, outType, outAmount, inLimit);
        }

        var options = [];

        for (var i in player.OwnedCardIds) {
            var card = IdLookup[player.OwnedCardIds[i]];
            var bp = card.BakeProperties;
            if (bp) {
                options.push(new ResourceConversion(card.Id, bp.InType, bp.InAmount, bp.OutType, bp.OutAmount, bp.InLimit))
            }
        }

        return options;
    },

    getCard: function(id){
        return IdLookup[id];
    },

    /**
     * Returns all resource conversions available to a given player that result in food.
     */
    getHarvestResources: function (player) {
        function HarvestResourceConversion(id, inType, inAmount, available, outAmount, maxCook) {
            this.id = id;
            this.inType = inType;
            this.inAmount = inAmount;
            this.available = available;
            this.outAmount = outAmount;
            this.maxCook = maxCook;
        }

        var board = this.game.playerBoards[player.Name];
        var resources = [];
        var cookValues = this.getAvailableResourceConversions(player);
        for (var type in cookValues) {
            var data = cookValues[type];

            var resourceCount = board.personalSupply[data.inType.toLowerCase()];
            if (resourceCount >= data.inAmount && data.outAmount > 0 && data.outType === Resource.Food)
                resources.push(data);
        }

        return resources;
    },


    getAnytimeConversions: function(player){
        var options = this.getAvailableResourceConversions(player);
        var anytimeOptions = [];
        for (var o in options) {
            var option = options[o];
            if (option.inLimit == null)
                anytimeOptions.push(option);
        }
        return anytimeOptions;
    },

    getPlowInfo: function(player, actionId){
        var cards = Curator.getOwnedCards(player);
        var plowInfo = {
            maxFields: 1,
            plows: []
        }

        for (var c in cards) {
            var card = cards[c],
                plow;
            if (!card.Plow)
                continue;

            console.info(player, card);
            if (player.CardMetadata[card.Id])
                plow = player.CardMetadata[card.Id]
            else
                plow = card.Plow;

            console.info(player.CardMetadata[card.Id]);

            if (plow && plow.Used < plow.MaxUses) {
                for (var i in plow.OnActions) {
                    var id = plow.OnActions[i];
                    if (id == actionId) {
                        plow["name"] = card.Name;
                        plow["cardId"] = card.Id;
                        plowInfo.plows.push(plow);
                        if (plowInfo.maxFields < plow.Fields)
                            plowInfo.maxFields = plow.Fields;
                        break;
                    }
                }

            }
        }
        return plowInfo;
    },

    getAvailableResourceConversions: function (player) {
        function updateResourceConversions(check) {

            // If this is a server object convert it to a client version
            check = ResourceConversion.fromServer(check);

            for (var v in maxValues) {
                var rc = maxValues[v];
                if (rc.inType === check.inType && rc.inAmount === check.inAmount &&
                    rc.inLimit == null && check.inLimit == null &&
                    rc.outType == check.outType) {

                    if (rc.outAmount < check.outAmount) {
                        rc.outAmount = check.outAmount;
                        rc.id = check.id;
                    }
                    else if (rc.outAmount == check.outAmount && rc.id > check.id)
                        rc.id = check.id;

                    return;
                }
            }

            maxValues.push(check);
        }

        var maxValues = [];
        updateResourceConversions(new ResourceConversion(-1, Resource.Food, 1, Resource.Food, 1, null));
        updateResourceConversions(new ResourceConversion(-2, Resource.Grain, 1, Resource.Food, 1, null));
        updateResourceConversions(new ResourceConversion(-3, Resource.Vegetables, 1, Resource.Food, 1, null));

        if (player && player.OwnedCardIds) {
            for (var i in player.OwnedCardIds) {
                var id = player.OwnedCardIds[i],
                    card = Curator.getCard(id),
                    rcs = card.ResourceConversions;

                if (rcs) {
                    for (var c in rcs) {
                        var rc = rcs[c];
                        updateResourceConversions(rc);
                    }
                }
            }
        }

        return maxValues;
    },

    hasOven: function(player){
        var cards = player.OwnedCardIds;
        for (var c in cards) {
            var card = IdLookup[cards[c]];
            if (card.Oven)
                return true;
        }
        return false;
    },


    getCacheExchanges: function (player, forTypes, actionId) {
        console.info(forTypes);
        var exchanges = [];
        var cards = Curator.getOwnedCards(player);
        for (var c in cards) {
            var card = cards[c];
            if (card.CacheExchanges && card.CacheExchanges.length > 0) {
               
                for (var e in card.CacheExchanges) {
                    var exchange = card.CacheExchanges[e];
                    
                    if ( (exchange.OnAction == "TravelingPlayers" && (actionId == 18 || actionId == 21) 
                         || exchange.OnAction != "TravelingPlayers")
                        && (forTypes[exchange.InType] && forTypes[exchange.InType] >= exchange.InAmount))
                        exchanges.push(exchange);
                }
            }
        }
        return exchanges.length == 0 ? null : exchanges;
    },


    getRenovationCost: function (player) {
        let board = this.game.playerBoards[player.Name],
            roomCount = board.rooms.length,
            reedCost = 1,
            costs = [];

        switch (player.Farmyard.HouseType) {
            case HouseType.Wood:
                costs.push(this._buildCost(Resource.Clay, roomCount));
                break;
            case HouseType.Clay:
                costs.push(this._buildCost(Resource.Stone, roomCount));
                break;
        }

        if (player.OwnedCardIds.includes(CardId.Thatcher))
            reedCost--;

        if(reedCost > 0)
            costs.push(this._buildCost(Resource.Reed, reedCost));

        return costs;

    },

    getRoomCost: function (player, actionId) {
        let costs = [],
            reedCost = 2;

        if (actionId == 505 || actionId == 601)
            return costs;

        switch (player.Farmyard.HouseType) {
            case HouseType.Wood:
                costs.push(this._buildCost(Resource.Wood, 5))
                break;
            case HouseType.Clay:
                costs.push(this._buildCost(Resource.Clay, 5))
                break;
            case HouseType.Stone:
                costs.push(this._buildCost(Resource.Stone, 5))
                break;
        }

        if (player.OwnedCardIds.includes(CardId.Thatcher)) 
            reedCost--;

        if (reedCost > 0)
            costs.push(this._buildCost(Resource.Reed, reedCost));

        return costs;
    },

    getStableCost: function (player, actionId) {
        var numWood = actionId == 54 ? 1 : 2;
        if (actionId == 502)
            numWood = 0;
        return [this._buildCost(Resource.Wood, numWood)];
    },

    getOwnedCards: function (player) {
        var owned = [];
        for (var i in player.OwnedCardIds) {
            owned.push(IdLookup[player.OwnedCardIds[i]]);
        }
        return owned;
    },

    getOwnedImprovements: function(player){
        var owned = [];
        for (var i in player.OwnedCardIds) {
            var card = IdLookup[player.OwnedCardIds[i]];
            if (card.Type == CardType.Minor || card.Type == CardType.Major)
                owned.push(card)
        }
        return owned;
    },

    getOwnedOccupations: function(player){
        var owned = [];
        for (var i in player.OwnedCardIds) {
            var card = IdLookup[player.OwnedCardIds[i]];
            if (card.Type == CardType.Occupation)
                owned.push(card)
        }
        return owned;
    },

    getOwnedOccupationsCount: function (player) {
        var numOwned = 0;
        for (var i in player.OwnedCardIds) {
            var card = IdLookup[player.OwnedCardIds[i]];
            if (card.Type == CardType.Occupation) {
                numOwned++;
                if (card.Id == 148) // Academic
                    numOwned++;
            }
        }
        return numOwned;
    },
    
    getOccupationCost: function(player, actionId){
        var numOwnedOccupations = this.getOwnedOccupationsCount(player); 

        switch (parseInt(actionId)) {
            case 4:
                return numOwnedOccupations == 0 ? 0 : 1;

            case 14:
            case 20:
                return numOwnedOccupations < 2 ? 1 : 2;
        }

        return 2;
    },

    getAffordableCosts: function(player, item){

        var obj = this,
            id,
            affordableCostOptions = [];

        if (!isNaN(id = parseInt(item))) {
            item = IdLookup[id];
            if (!this.meetsCardPrereqs(player, item))
                return [];

            var costs = item.Costs;
            for (var c in costs) {
                if (checkCosts(player, costs[c], id)) {
                    costs[c]["costIndex"] = c;
                    affordableCostOptions.push(costs[c]);
                }
            }
        }

        function checkCosts(player, cost, id) {
            let type = cost.Type;
            switch (type) {
                case CardCostType.Resources:
                    for (let c in cost.Resources) {
                        let cache = cost.Resources[c],
                            count = cache.Count;

                        if (cache.Type == Resource.Reed && player.OwnedCardIds.includes(CardId.Thatcher) && ThatcherReedReductions.includes(id)) {
                            count--;
                        }
                        
                        if (player.farmyard.personalSupply[cache.Type.toLowerCase()] < parseInt(count))
                            return false;
                    }
                    break;

                case CardCostType.ReturnCard:
                    for (var c in costs) {
                        let major = costs[c],
                            ids = major.Ids;
                        for (let i in ids) {
                            let id = ids[i],
                                ownedCards = Curator.getOwnedCards(player);
                            for (var c in ownedCards) {
                                var card = ownedCards[c];
                                if (card.Id == id) {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
            }
            return true;
        }

        return affordableCostOptions;

    },

    // This is wrong, can afford functionality should only be limited to player
    // checks for enabling actions -- remove personalSupplyOverride
    canAfford: function (player, item) {
        var affordableCosts = this.getAffordableCosts(player, item);
        return affordableCosts.length > 0;
    },

    meetsCardPrereqs: function (player, cardData) {
        if (!cardData.Prerequisites)
            return true;

        if (!this.meetsPrereqs(player, cardData.Prerequisites))
            return false;

        switch (cardData.Id) {
            case 52:  // Stables
                if (!Curator.canBuildStable(player, 0))
                    return false;
                break;
            case 44: // Outhouse
                for (var p in this.game.players) {
                    p = this.game.players[p];
                    var count = this.getOwnedOccupationsCount(p);
                    if ((p != player || (p == player && this.game.numPlayers == 1))
                        && count < 2)
                        return true;
                }
                return false;
        }
        return true;
    },

    meetsPrereqs: function (player, prereqs) {
        if (!prereqs) return true;

        for (var p in prereqs) {
            var prereq = prereqs[p];
            var owned = 0;

            switch (prereq.Type) {
                case "OccupationPrerequisite":
                    var ownedOccs = this.getOwnedOccupationsCount(player);
                    if (ownedOccs < prereq.Count)
                        return false;
                    break;

                case "ImprovementPrerequisite":
                    var ownedImps = this.getOwnedImprovements(player);
                    if (ownedImps.length < prereq.Count)
                        return false;
                    break;

                case "HousePrerequisite":
                    var houseType = prereq.HouseType;
                    if (houseType !== player.Farmyard.HouseType)
                        return false;
                    break;

                case "RoomPrerequisite":
                    var roomCount = prereq.RoomCount;
                    if (roomCount > player.farmyard.rooms.length) {
                        return false;
                    }
                    break;
            }
        }
        return true;
    },

    loadDeck: function (deck) {
        for (var c in deck) {
            var card = deck[c];
            IdLookup[card.Id] = card;
            Curator.buildCardProperties(card);
        }
    },

    updateCard: function (card) {
        var type = IdLookup[card.Id].Type;
        IdLookup[card.Id] = card;
        card.Type = type;
        Curator.buildCardProperties(card);
    },

    buildCardProperties:function(card){
        if (card.ResourceConversions) {
            for (var rc in card.ResourceConversions) {
                if (rc.InLimit == null) {
                    card["cooks"] = true;
                    break;
                }
            }
        }
    },

    _buildCost: function(type, amount) {
        return { type: type, amount: amount };
    }


}

function ResourceConversion(id, inType, inAmount, outType, outAmount, inLimit) {
    this.id = id;
    this.inType = inType;
    this.inAmount = inAmount;
    this.outType = outType;
    this.outAmount = outAmount;
    this.inLimit = inLimit;

    this.toResourceConversionData = function (count) {
        return new ResourceConversionData(this.id, count, this.inType, this.inAmount, this.outType);
    }
}

/**
 * Checks if a given resource conversion item is from the server, and if so converts it to a client side ResourceConversion
 */
ResourceConversion.fromServer = function (item) {
    if (item.InType) {
        item = new ResourceConversion(item.Id, item.InType, item.InAmount, item.OutType, item.OutAmount, item.InLimit);
    }
    return item;
};


const CardCostType = {
    ReturnCard: "ReturnCardCardCost",
    Resources: "ResourceCardCost",
    Free: "FreeCardCost"
}

const CardType = {
    Major: "Major",
    Minor: "Minor",
    Occupation: "Occupation"
}


const Resource = {
    Food: "Food",
    Wood: "Wood",
    Clay: "Clay",
    Reed: "Reed",
    Stone: "Stone",
    Grain: "Grain",
    Vegetables: "Vegetables",
    Sheep: "Sheep",
    Boar: "Boar",
    Cattle: "Cattle"
}

const AnimalResource = {
    Sheep: Resource.Sheep,
    Boar: Resource.Boar,
    Cattle: Resource.Cattle
}

const GameMode = {
    Work: "Work",
    Harvest: "Harvest",
    Over: "Over"
}

const HouseType = {
    Wood: "Wood",
    Clay: "Clay",
    Stone: "Stone"
}

const InterruptActionId = {
    Bake: 500,
    Plow: 501,
    BuildStable: 502,
    SelectResources: 503,
    Occupation: 504,
    BuildRoom: 505,
    AssignAnimals: 506,
    PlayerChoice: 507,
    FencePasture: 508
}

const IdLookup = [];

const CardId = {
    // Major Improvements
    Fireplace: 1,

    // Basic Improvements
    HalfTimberedHouse: 21,
    Basket: 34,
    Spindle: 51,

    // Intermediate Improvements
    HolidayHouse: 71,
    ChickenCoop: 84,
    CornStorehouse: 86,
    WaterMill: 103,
    SwingPlow: 115,
    CrookedPlow: 119,

    // Advanced Improvements
    StoneExchange: 143,
    Mansion: 144,


    // Basic Occupations
    Mendicant: 153,
    MasterBrewer: 154,
    Thatcher: 157,
    YeomanFarmer: 165,
    DockWorker: 171,
    Tutor: 174,
    ClayDeliveryman: 187,
    Maid: 190,
    CattleWhisperer: 201,

    // Advanced Occupations
    SchnapsDistiller: 300
}

const ThatcherReedReductions = [CardId.Basket, CardId.WaterMill, CardId.HalfTimberedHouse, CardId.ChickenCoop, CardId.HolidayHouse, CardId.Mansion, CardId.CornStorehouse];