
/**
    represents the action board each player gets to manage
    their farm and resources
 */
var PlayerBoard = function (display, player) {
    this.display = display;
    this.player = player;
    this.color = player.Color;
    this.popup = null;

    this.rooms = [];
    this.fields = [];
    this.stables = [];
    this.addedStables = [];
    this.addedRooms = [];
    this.addedFields = [];
    this.addedSows = [];
    this.addedFences = [];

    display.attr("data-player", this.player.Name);
    display.addClass(player.Color.toLowerCase());
    this.personalSupply = this.display.find(".personal-supply");
    this.farmyard = this.display.find(".farmyard");

    var plot = this.farmyard.find(".plot");
    if (plot.length == 1) {
        plot.remove();
        for (var i = 0; i < this.PLOT_GRID_WIDTH * this.PLOT_GRID_HEIGHT; i++) {
            var c = plot.clone();
            var x = i % this.PLOT_GRID_WIDTH;
            var y = Math.floor(i / this.PLOT_GRID_WIDTH);
            this.farmyard.append(c);

            c.find(".fence-slot.top").attr("data-id", y * this.FENCE_GRID_WIDTH * 2 + x);
            c.find(".fence-slot.left").attr("data-id", (y * 2 + 1) * this.FENCE_GRID_WIDTH + x);

            var r = c.find(".fence-slot.right");
            if (x != 4)
                r.remove();
            else
                r.attr("data-id", (y+1) * 2 * this.FENCE_GRID_WIDTH - 1);

            var b = c.find(".fence-slot.bottom");
            if (y != 2)
                b.remove();
            else
                b.attr("data-id", (y * 2 + 2) * this.FENCE_GRID_WIDTH + x)
        }
        this.farmyard.append("<div class='clear'></div>");
    }
    this.plots = this.farmyard.find(".plot");
}

PlayerBoard.prototype = {
    player: null,
    display: null,
    color: null,
    personalSupply: null,
    farmyard: null,
    familySize: 0,
    familyAtHome: 0,
    rooms: null,
    fields: null,
    stables: null,
    fences:null,
    addedStables: null,
    addedRooms: null,
    addedFields: null,
    addedSows: null,
    addedFences:null,
    plots: null,
    grid: null,
    animalManager: null,

    PLOT_GRID_WIDTH:5,
    PLOT_GRID_HEIGHT:3,

    FENCE_GRID_WIDTH: 6,
    FENCE_GRID_HEIGHT: 7,

    MAX_FENCES: 15,
    MAX_STABLES: 4,

    show: function () {
        this.display.show();
        this.updatePieces();
    },

    hide: function () {
        this.display.hide();
    },

    /**
        Partial update that redraws game pieces in the right places.
     */
    updatePieces: function () {
        this.display.find(".piece").remove();
        this.display.find(".family-member").remove()

        this.placeFamilyInHome();

        for (var f in this.fields) {
            var field = this.fields[f];
            var o = this.grid[field.x][field.y];
            for (var i = 0; i < o.Sown.Count; i++) {
                var plot = $(this.plots[field.y * 5 + field.x]);
                var p = Game.createResourcePiece(o.Sown.Type)
                plot.append(p);
                var aOffset = plot.offset();

                p.offset({
                    left: aOffset.left + plot.width() / 2 - p.width() / 2,
                    top: aOffset.top + plot.height() / 2 - p.height() / 2 - (i * (p.height() / 3.1))
                });

                useVh(p);


            }
        }

        if (this.animalManager) {
            var assignments = this.animalManager.getAssignments();
            for (var assignment in assignments) {
                assignment = assignments[assignment];

                var indices = this.animalManager.getIndicesFromId(assignment.id),
                count = assignment.count,
                type = assignment.type;

                if (assignment.id == "house") {
                    var piece = Game.createResourcePiece(type);
                    piece.css("left", ".5vh");
                    piece.css("top", ".5vh");

                    var plot = $(this.plots[10]);
                    plot.append(piece);

                }
                else {

                    for (var i = 0; i < count; i++) {
                        var piece = Game.createResourcePiece(type);
                        var plot = $(this.plots[indices[i % indices.length]]);
                        plot.append(piece);

                        var pOffset = plot.offset();
                        var padding = { w: plot.width() * .2, h: plot.height() *.2};

                        var pos = {
                            left: pOffset.left + padding.w + (Math.random() * (plot.width() - padding.w * 2)) - piece.width() / 2,
                            top: pOffset.top + padding.h + (Math.random() * (plot.height() - padding.h * 2)) - piece.height() / 2
                        }

                        piece.offset(pos);
                        useVh(piece);
                    }
                }
            }
        }
    },

    update: function (player) {
        if (!player) player = this.player;

        this.player = player;
        this.grid = player.Farmyard.Grid;
        this.familyAtHome = player.FamilyAtHome;
        this.familySize = player.FamilySize;

        this.personalSupply = new PersonalSupply(this.display.find(".personal-supply"), player);

        this.farmyard.addClass("house-" + player.Farmyard.HouseType.toLowerCase());

        var pf = player.Farmyard;

        this.rooms = [];
        this.fields = [];
        this.stables = [];

        this.fences = [];
        var fences = player.Farmyard.Fences;
        for (var fence in fences) {
            fence = fences[fence];
            this.fences[fence] = 1;
            this.plots.find(".fence-slot[data-id='" + fence + "']").addClass("fence");
        }
        this.updatePastures(pf.Pastures);

        for (var x = 0; x < this.PLOT_GRID_WIDTH; x++) {
            for (var y = 0; y < this.PLOT_GRID_HEIGHT; y++) {
                var o = this.grid[x][y];

                switch (o.Type) {
                    case "Room":
                        this.rooms.push({ x: x, y: y });
                        this.getPlot(x, y).addClass("room");
                        break;

                    case "Field":
                        this.fields.push({ x: x, y: y });
                        this.getPlot(x, y).addClass("field");
                        break;
                }

                if (o.HasStable) {
                    this.stables.push({ x: x, y: y });
                    this.getPlot(x, y).addClass("stable");
                }
            }
        }

        this.animalManager = new AnimalManager(this.grid, pf.Pastures, player.Farmyard.AnimalLocations);
        this.updatePieces();
    },

    /**
        Checks if this farmyards personal supply can afford the requested costs
    */
    canAfford: function(costs) {
        for (var c in costs) {
            var cost = costs[c];
        if (this.personalSupply[cost.type.toLowerCase()] < cost.amount)
            return false;
        }
        return true;
    },

    /**
        Deducts the given costs from the personal supply
    */
    payCosts: function(costs){
        for (var c in costs) {
            var cost = costs[c];
            this.personalSupply.modifyResource(cost.type, -cost.amount)
        }
    },

    /**
        Refunds the costs to the personal supply
    */
    refundCosts: function (costs) {
        for (var c in costs) {
            var cost = costs[c];
            this.personalSupply.modifyResource(cost.type, cost.amount)
        }
    },

    getPlot: function (x, y) {
        var index = y === undefined ? x : y * 5 + x;
        return $(this.plots[index]);
    },

    placeFamilyInHome: function () {
        this.farmyard.find(".family-member").remove();
        var plots = this.farmyard.find(".plot"),
            numRooms = this.rooms.length,
            index,
            stackOffset = 0;

        for (var i = 0; i < this.familyAtHome; i++) {
            var member = Game.createFamilyMember(this.player.Color);
            if (numRooms > i) 
                index = this.rooms[i].y * 5 + this.rooms[i].x;

            var plot = $(plots[index]);
            plot.append(member);

            if (numRooms <= i) 
                stackOffset -= member.height() / 7;

            var plotOffset = plot.offset();

            midpoint = {
                x: plotOffset.left + plot.width()/2 - member.width() / 2,
                y: plotOffset.top + plot.height()/2 + stackOffset - member.height() / 2
            }

            member.offset({ left: midpoint.x, top: midpoint.y });
            useVh(member);
        }
    },

    
    enablePlotsForStables: function (popup, actionId){
        var costs = Curator.getStableCost(this.player, actionId);
        var maxStables = Curator.getStableBuildLimit(this.player, actionId);

        console.info(costs, maxStables);

        this._enablePlotsForStables(popup, maxStables, costs);
    },


    _enablePlotsForStables: function (popup, maxNewStables, costs) {
        var obj = this;
        function act(index, plot) {
            var i = -1;
            if ((i = $.inArray(index, obj.addedStables)) >= 0) {
                obj.addedStables.splice(i, 1);
                plot.removeClass("stable");
                obj.refundCosts(costs);
            }
            else if (obj.canAfford(costs)) {
                obj.addedStables.push(index);
                plot.addClass("stable");
                obj.payCosts(costs);
            }
            if (popup && popup.updateSubmitButtonState)
                popup.updateSubmitButtonState();
            obj._enablePlotsForStables(popup, maxNewStables, costs);
        }

        function ri(index, plot) {
            return function () {
                act(index, plot);
            }
        }

        this.disablePlots();
        var numStables = this.addedStables.length + this.stables.length;
        for (var x = 0; x < this.PLOT_GRID_WIDTH; x++) {
            for (var y = 0; y < this.PLOT_GRID_HEIGHT; y++) {
                var index = y * this.PLOT_GRID_WIDTH + x;
                if ((((this.grid[x][y].Type == "Pasture" || this.grid[x][y].Type == "Empty") && !this.grid[x][y].HasStable
                    && numStables < this.MAX_STABLES && obj.canAfford(costs))
                    && this.addedStables.length < maxNewStables
                    || ($.inArray(index, this.addedStables) >= 0))
                    && ($.inArray(index, this.addedRooms) < 0)) {
                    var plot = $(this.plots[index]),
                        cell = plot.find(".cell");
                    cell.addClass("highlight").click( ri(index, plot) )
                }
            }
        }


    },

    enablePlotsForRooms: function (popup, actionId) {
        var maxRooms = null;
        if (actionId == 21 || actionId == 505 || actionId == 601)
            maxRooms = 1;

        var roomCost = Curator.getRoomCost(this.player, actionId);

        this._enablePlotsForRooms(popup, maxRooms, roomCost);
    },

    _enablePlotsForRooms: function (popup, maxRooms, roomCost) {
        var obj = this;

        function checkPlot(x, y) {
            var index = y * obj.PLOT_GRID_WIDTH + x;
            if (x >= 0 && y >= 0 && x < obj.PLOT_GRID_WIDTH && y < obj.PLOT_GRID_HEIGHT
                && obj.grid[x][y].Type == "Empty"
                && $.inArray(index, obj.addedStables) < 0) {

                var plot = $(obj.plots[index]),
                    cell = plot.find(".cell");
                if (!cell.hasClass("highlight"))
                    cell.addClass("highlight").click(ri(index, plot));
            }

        }

        function toggleRoom(index, plot) {

            var i = -1;
            if ((i = $.inArray(index, obj.addedRooms)) >= 0) {
                obj.addedRooms.splice(i, 1);
                plot.removeClass("room");
                obj.refundCosts(roomCost);
            }
            else if (obj.canAfford(roomCost)) {
                obj.addedRooms.push(index);
                plot.addClass("room");
                obj.payCosts(roomCost);

            }
            if (popup && popup.updateSubmitButtonState)
                popup.updateSubmitButtonState();
            obj._enablePlotsForRooms(popup, maxRooms, roomCost);
        }

        function ri(index, plot) {
            return function () {
                toggleRoom(index, plot);
            }
        }


        this.disablePlots();


        if ((maxRooms && this.addedRooms.length >= maxRooms)
            || !obj.canAfford(roomCost)) {
            for (var i in this.addedRooms) {
                var room = this.addedRooms[i];
                checkPlot(room % this.PLOT_GRID_WIDTH, Math.floor(room / this.PLOT_GRID_WIDTH));
            }
        }
        else {
            for (var f in this.rooms) {
                var room = this.rooms[f],
                    x = room.x,
                    y = room.y;

                checkPlot(x - 1, y);
                checkPlot(x + 1, y);
                checkPlot(x, y - 1);
                checkPlot(x, y + 1);
            }

            for (var f in this.addedRooms) {
                var index = this.addedRooms[f],
                    x = index % 5,
                    y = Math.floor(index / 5);

                checkPlot(x - 1, y);
                checkPlot(x + 1, y);
                checkPlot(x, y - 1);
                checkPlot(x, y + 1);
            }

        }
        if (obj.addedRooms.length > 1) {
            for (var f in obj.addedRooms) {
                var index = obj.addedRooms[f];
                if (obj.requiredAsPath(index, obj.rooms, obj.addedRooms)) {
                    var dplot = $(obj.plots[index]).find(".cell");
                    dplot.removeClass("highlight").off("click");
                }
            }
        }


    },

    enablePlotsForPlowing: function (numPlowable, popup) {
        function checkPlot(x, y) {
            var index = y * obj.PLOT_GRID_WIDTH + x;
            if (x >= 0 && y >= 0 && x < obj.PLOT_GRID_WIDTH && y < obj.PLOT_GRID_HEIGHT
                && obj.grid[x][y].Type == "Empty" && !obj.grid[x][y].HasStable) {
                var plot = $(obj.plots[index]),
                    cell = plot.find(".cell");
                if (!cell.hasClass("highlight")) {
                    cell.addClass("highlight").click(ri(index, plot));
                }
            }
        }

        function toggleField(index, plot) {
            var i = -1;
            if ((i = $.inArray(index, obj.addedFields)) >= 0) {
                var index = obj.addedFields[i];
                obj.addedFields.splice(i, 1);
                plot.removeClass("field");

                if (obj.addedSows[index]) {
                    var o = obj.addedSows[index];
                    if (o.type == "grain") {
                        obj.personalSupply.modifyGrain(1);
                        obj.addedSows[index] = null;
                    }
                    else if (o.type == "vegetables") {
                        obj.personalSupply.modifyVegetables(1);
                        obj.addedSows[index] = null;
                    }
                    plot.find(".piece").remove();
                }
            }
            else {
                obj.addedFields.push(index);
                plot.addClass("field");
            }
            obj.enablePlotsForPlowing(numPlowable, popup);
        }

        function ri(index, plot) {
            return function (event) {
                event.stopPropagation();
                toggleField(index, plot);
                if (popup) {
                    popup.updateSubmitButtonState();
                }
            }
        }


        var obj = this;
        
        // Start off by verifying that the number of newly plowed
        // fields is equal to or less than the number allowed to be plowed
        if (this.addedFields.length > numPlowable) {
            for (var n = this.addedFields.length - 1; n >= numPlowable; n--) {
                var index = this.addedFields[n];
                var plot = $(obj.plots[index]);
                toggleField(index, plot);
            }
        }




        this.disablePlots();


        if (this.fields.length == 0 && this.addedFields.length == 0) {
            this.plots.each(function () {
                var plot = $(this),
                    cell = plot.find(".cell"),
                    index = plot.index(),
                    y = Math.floor(index / obj.PLOT_GRID_WIDTH),
                    x = index % obj.PLOT_GRID_WIDTH;
                if (obj.grid[x][y].Type == "Empty" && !obj.grid[x][y].HasStable) {
                    cell.addClass("highlight").click(ri(index, plot));
                }
            });
            
        }

        else if (numPlowable <= this.addedFields.length) {
            for (var i in this.addedFields) {
                var index = this.addedFields[i];
                checkPlot(index % obj.PLOT_GRID_WIDTH, Math.floor(index / obj.PLOT_GRID_WIDTH));
            }
        }
        else {


            for (var f in this.fields) {
                var field = this.fields[f],
                    x = field.x,
                    y = field.y;

                checkPlot(x - 1, y);
                checkPlot(x + 1, y);
                checkPlot(x, y - 1);
                checkPlot(x, y + 1);
            }

            for (var f in this.addedFields) {
                var index = this.addedFields[f],
                    x = index % obj.PLOT_GRID_WIDTH,
                    y = Math.floor(index / obj.PLOT_GRID_WIDTH);

                checkPlot(x, y);
                checkPlot(x - 1, y);
                checkPlot(x + 1, y);
                checkPlot(x, y - 1);
                checkPlot(x, y + 1);
            }

        }

        if (obj.addedFields.length + obj.fields.length > 2) {
            for (var f in obj.addedFields) {
                var index = obj.addedFields[f];
                if (obj.requiredAsPath(index, obj.fields, obj.addedFields)) {
                    var cell = $(obj.plots[index]).find(".cell");
                    cell.removeClass("highlight").off("click");
                }
            }
        }

    },


    enablePlotsForAnimalManagement: function(){
        this.enableAnimalManagement();
    },


    enableAnimalManagement: function () {
        var obj = this;
        function emptyHousing(id) {
            obj.animalManager.emptyHousing(id);
            obj.updatePieces();

            obj.popup.animalStateChanged();
        }

        function ri(id) {
            return function (event) {
                emptyHousing(id);
                event.stopPropagation();
            }
        }

        this.disablePlots();
        this.updatePieces();

        for (var h in this.animalManager.housings) {
            var housing = this.animalManager.housings[h];
            var ids = this.animalManager.getIndicesFromId(housing.id);
            var id = ids[0];
            
            var plot = $(this.plots[id]);
            var cc = $("<div class='custom-controls manage-animals'></div>");
            plot.append(cc);

            if (housing.animalCount > 0) {
                var emptyButton = $("<div class='empty-button'>x</div>");
                cc.append(emptyButton);
                emptyButton.on("click", ri(housing.id));
            }
        }
    },

    getFenceCount: function () {
        var fenceCount = 0;
        for (var i in this.fences) {
            if (this.fences[i])
                fenceCount++;
        }
        return fenceCount;
    },


















    enablePlotForFencing: function (popup) {
        var obj = this;
        this.disablePlots();
        this.enableAnimalManagement();

        var activePlotData,
            activePlot,

        fenceCount = this.getFenceCount();


        function ri(plot, x, y, i) {
            return function (event) {
                event.stopPropagation();
                toggleFences(plot, x, y, i);
            }
        }

        // plot, plot x, plot y, plot index
        function toggleFences(plot, x, y, index) {
            console.info(activePlot, activePlotData, plot);
            if (obj.addedFences.length > 0){
                for(var f in obj.addedFences){
                    var fence = obj.addedFences[f];
                    fence.removeClass("fence");
                }
                obj.addedFences = [];
            }

            var newPlotData = FenceUtils.getPlotBorderingFenceData(x, y);
            for (var d in newPlotData) {
                var data = newPlotData[d];
                if (!obj.fences[data.index]) {
                    var plotData = FenceUtils.getPlotDataForFence(data.x, data.y);
                    var fence = $(obj.plots[plotData.index]).find(".fence-slot." + plotData.position);
                    var fenceIndex = parseInt(fence.attr("data-id"));
                    console.info(fence, fenceIndex);
                    fence.addClass("fence");
                    obj.addedFences[fenceIndex] = fence;
                }
            }

            var fv = new FenceValidator(obj),
            result = fv.validate(),
            valid = result.valid,
            pastures = result.pastures,
            pastureIndices = obj.updatePastures(pastures),
            pathable = true;

            if (pastureIndices.length > 1) {
                pathable = obj.allPathable([pastureIndices]);
            }


            var animalsFit = obj.animalManager.reconfigure(obj.grid, pastures);
            if (popup && popup.updateSubmitButtonState) {
                popup.animalStateChanged();
                popup.updateSubmitButtonState({ fencesValid: valid && pathable });
            }


            obj.enablePlotForFencing(popup);
        }


        this.plots.each(function () {
            var plot = $(this),
                index = plot.index(),
                y = Math.floor(index / obj.PLOT_GRID_WIDTH),
                x = index % obj.PLOT_GRID_WIDTH;

            if (obj.grid[x][y].Type == "Empty" || obj.grid[x][y].Type == "Pasture") {
                var cell = plot.find(".cell");
                cell.addClass("highlight").click(ri(plot, x, y, index));
            }
        });


    },















    /**
     Enables placement for individual fences
     */
    enablePlotsForFencing: function (popup) {
        var obj = this;
        function wireFence(x, y) {
            var data = FenceUtils.getPlotDataForFence(x, y);
            var index = data.y * obj.PLOT_GRID_WIDTH + data.x;
            var fence = $(obj.plots[index]).find(".fence-slot." + data.position);

            if (!fence.hasClass("highlight"))
                fence.addClass("highlight").click(ri(fence, x, y, data));
        }


        function checkFence(x, y) {
            var index = y * obj.FENCE_GRID_WIDTH + x;
            if (obj.fences[index])
                return false;

            var plots = FenceUtils.getNeighboringPlots(x, y);
            var plot1 = plots[0],
                x1 = plot1 % obj.PLOT_GRID_WIDTH,
                y1 = Math.floor(plot1 / obj.PLOT_GRID_WIDTH);

            if (obj.grid[x1][y1].Type == "Empty" || obj.grid[x1][y1].Type == "Pasture")
                return true;

            if (plots[1] != undefined) {
                var plot2 = plots[1],
                    x2 = plot2 % obj.PLOT_GRID_WIDTH,
                    y2 = Math.floor(plot2 / obj.PLOT_GRID_WIDTH);

                if (obj.grid[x2][y2].Type == "Empty" || obj.grid[x1][y1].Type == "Pasture")
                    return true;
            }
            return false;
        }


        function toggleFence(fence, x, y, fencePlotData) {
            var index = y * obj.FENCE_GRID_WIDTH + x;

            if (obj.addedFences[index]) {
                obj.addedFences[index] = null;
                fence.removeClass("fence");
                obj.personalSupply.modifyWood(+1);
            }
            else {
                obj.addedFences[index] = fence;
                fence.addClass("fence");
                obj.personalSupply.modifyWood(-1);
            }

            var fv = new FenceValidator(obj),
                result = fv.validate(),
                valid = result.valid,
                pastures = result.pastures,
                pastureIndices = obj.updatePastures(pastures),
                pathable = true;

            if (pastureIndices.length > 1) {
                pathable = obj.allPathable([pastureIndices]);
            }


            var animalsFit = obj.animalManager.reconfigure(obj.grid, pastures);
            if (popup && popup.updateSubmitButtonState) {
                popup.animalStateChanged();
                popup.updateSubmitButtonState({ fencesValid: valid && pathable });
            }

            obj.enablePlotsForFencing(popup);
        }


        function ri(fence, x, y, fencePlotData) {
            return function (event) {
                event.stopPropagation();
                toggleFence(fence, x, y, fencePlotData);

            }
        }

        this.disablePlots();
        this.enableAnimalManagement();

        this.plots.each(function () {
            var plot = $(this),
                index = plot.index(),
                y = Math.floor(index / obj.PLOT_GRID_WIDTH),
                x = index % obj.PLOT_GRID_WIDTH;

            if (obj.grid[x][y].Type == "Empty" || obj.grid[x][y].Type == "Pasture" ) {
                var cell = plot.find(".cell");
                cell.addClass("highlight");
            }
        });

        var fenceCount = 0;
        for (var i in this.addedFences) {
            if (this.addedFences[i])
                fenceCount++;
        }

        fenceCount += this.getFenceCount();


        if (fenceCount == 0) {
            for (var x = 0; x < this.FENCE_GRID_WIDTH; x++) {
                for (var y = 0; y < this.FENCE_GRID_HEIGHT; y++) {
                    if (!(x == this.FENCE_GRID_WIDTH - 1 && y % 2 == 0)) {
                        if (checkFence(x, y))
                            wireFence(x, y);
                    }
                }
            }
        }
        else {
            for (var i in this.addedFences) {
                if (this.addedFences[i]) {
                    i = parseInt(i);
                    wireFence(i % obj.FENCE_GRID_WIDTH, Math.floor(i / obj.FENCE_GRID_WIDTH));

                    if (fenceCount < obj.MAX_FENCES && obj.personalSupply.wood > 0) {
                        var neighbors = FenceUtils.getNeighboringFences(i);
                        for (var n in neighbors) {
                            var neighbor = neighbors[n],
                                x = neighbor % obj.FENCE_GRID_WIDTH,
                                y = Math.floor(neighbor / obj.FENCE_GRID_WIDTH);
                            if (checkFence(x, y)) {
                                wireFence(x, y);
                            }
                        }
                    }
                }
            }

            for (var i in this.fences) {
                if (this.fences[i]) {
                    i = parseInt(i);
                    if (fenceCount < obj.MAX_FENCES && obj.personalSupply.wood > 0) {
                        var neighbors = FenceUtils.getNeighboringFences(i);
                        for (var n in neighbors) {
                            var neighbor = neighbors[n],
                                x = neighbor % obj.FENCE_GRID_WIDTH,
                                y = Math.floor(neighbor / obj.FENCE_GRID_WIDTH);
                            if (checkFence(x, y)) {
                                wireFence(x, y);
                            }
                        }
                    }

                }
            }
        }
    },

    updatePastures: function (pastures) {
        this.plots.removeClass("pasture");
        var pastureIndices = [];
        for (var pasture in pastures) {
            pasture = pastures[pasture];
            for (var plotIndex in pasture) {
                plotIndex = pasture[plotIndex];
                pastureIndices.push(plotIndex);
                $(this.plots[plotIndex]).addClass("pasture");

            }
        }
        return pastureIndices;
    },





    enablePlotsForSowing: function (popup) {
        var obj = this;
        var last, lastIndex;
        function addControls(index, plot) {
            var cell = plot.find(".cell");

            if (obj.personalSupply.vegetables == 0) {
                obj.addedSows[index] = { index: index, type: "grain" };
                obj.personalSupply.modifyGrain(-1);
                obj.enablePlotsForSowing(popup);
                showPiece("grain", plot);
                if (popup)
                    popup.updateSubmitButtonState();
            }
            else if (obj.personalSupply.grain == 0) {
                event.stopPropagation();
                obj.addedSows[index] = { index: index, type: "vegetables" };
                obj.personalSupply.modifyVegetables(-1);
                obj.enablePlotsForSowing(popup);
                showPiece("vegetables", plot);
                if (popup)
                    popup.updateSubmitButtonState();
            }
            else {
                if (last) {
                    last.find(".cell").addClass("highlight").click(ri(lastIndex, last))
                }
                obj.removePlotCustomControls();

                var controls = $("<div class='custom-controls'></div>");

                last = plot;
                lastIndex = index;
                cell.prepend(controls);
                cell.removeClass("highlight").off("click");

                plot.addClass("sowing");

                var grainButton = $("<div class='button button-grain'><div>Grain</div></div>");
                controls.append(grainButton)
                grainButton.click(function (event) {
                    event.stopPropagation();
                    obj.addedSows[index] = { index: index, type: "grain" };
                    obj.personalSupply.modifyGrain(-1);
                    grainButton.off("click");
                    showPiece("grain", plot);
                    obj.enablePlotsForSowing(popup);
                    if (popup)
                        popup.updateSubmitButtonState();
                });

                var vegetablesButton = $("<div class='button button-vegetables'><div>Vegetables</div></div>");
                controls.append(vegetablesButton);
                vegetablesButton.click(function (event) {
                    event.stopPropagation();
                    obj.addedSows[index] = { index: index, type: "vegetables" };
                    obj.personalSupply.modifyVegetables(-1);
                    vegetablesButton.off("click");
                    showPiece("vegetables", plot);
                    obj.enablePlotsForSowing(popup);
                    if (popup)
                        popup.updateSubmitButtonState();
                });
            }
        }

        function showPiece(type, plot) {
            var p = Game.createResourcePiece(type)
            plot.append(p);
            var aOffset = plot.offset();

            p.offset({
                left: aOffset.left + plot.width() / 2 - p.width() / 2,
                top: aOffset.top + plot.height() / 2 - p.height() / 2
            });
        }

        function sowField(index, plot) {
            if (obj.addedSows[index]) {
                var o = obj.addedSows[index];
                if (o.type == "grain") {
                    obj.personalSupply.modifyGrain(1);
                    obj.addedSows[index] = null;
                }
                else if (o.type == "vegetables") {
                    obj.personalSupply.modifyVegetables(1);
                    obj.addedSows[index] = null;
                }

                plot.find(".piece").remove();
                obj.enablePlotsForSowing(popup);
                if (popup)
                    popup.updateSubmitButtonState();
            }
            else {
                addControls(index, plot);
            }
        }

        function ri(index, plot) {
            return function () {
                sowField(index, plot);
            }
        }

        this.disablePlots();

        var sowable = [];
        if (this.personalSupply.grain == 0 && this.personalSupply.vegetables == 0) {
            for (var i in this.addedSows) {
                if(this.addedSows[i])
                    sowable.push(i);
            }
        }
        else {
            for (var i in this.fields) {
                var field = this.fields[i];
                if (this.grid[field.x][field.y].Sown.Count == 0)
                    sowable.push(field.y * obj.PLOT_GRID_WIDTH  + field.x);
            }
            for (var i in this.addedFields) {
                sowable.push(this.addedFields[i]);
            }
        }

        for (var i in sowable) {
            var index = sowable[i],
                plot = $(this.plots[index]),
                cell = plot.find(".cell");

            cell.addClass("highlight").on("click", ri(index, plot));
        }


    },

    /**
        Disables all plots
     */
    disablePlots: function () {
        this.removePlotCustomControls();
        var cells = this.plots.find(".cell");
        cells.removeClass("highlight").off("click");
        this.plots.find(".fence-slot").removeClass("highlight").off("click");
    },

    /**
        Removes any custom controls taht were 
        added to a plot by an action
     */
    removePlotCustomControls: function () {
        this.plots.find(".custom-controls").remove();
        this.plots.removeClass("sowing");
    },

    canSowField: function () {
        if ((this.player.Grain > 0 || this.player.Vegetables > 0)
            && this.fields.length > 0) {
            for (var f in this.fields) {
                var pos = this.fields[f];
                var field = this.grid[pos.x][pos.y];
                if (field.Sown.Count == 0)
                    return true;
            }
        }
        return false;
    },

    canPlowField: function () {
        if (this.fields.length == 0) {
            for (var x = 0;x < this.PLOT_GRID_WIDTH ; x++) {
                for (var y = 0; y < this.PLOT_GRID_HEIGHT; y++) {
                    var o = this.grid[x][y];
                    if (o.Type == "Empty" && o.HasStable == false)
                        return true;
                }
            }

            return false;
        }
        for (var i in this.fields) {
            var field = this.fields[i],
                x = field.x,
                y = field.y;
            if (x - 1 >= 0 && this.grid[x - 1][y].Type == "Empty") return true;
            if (x + 1 < this.PLOT_GRID_WIDTH && this.grid[x + 1][y].Type == "Empty") return true;
            if (y - 1 >= 0 && this.grid[x][y - 1].Type == "Empty") return true;
            if (y + 1 < this.PLOT_GRID_HEIGHT && this.grid[x][y + 1].Type == "Empty") return true;
        }
        return false;
    },

    canBuildRoom: function () {
        for (var i in this.rooms) {
            var room = this.rooms[i],
                x = room.x,
                y = room.y;
            if (x - 1 >= 0 && this.grid[x - 1][y].Type == "Empty") return true;
            if (x + 1 < this.PLOT_GRID_WIDTH && this.grid[x + 1][y].Type == "Empty") return true;
            if (y - 1 >= 0 && this.grid[x][y - 1].Type == "Empty") return true;
            if (y + 1 < this.PLOT_GRID_HEIGHT && this.grid[x][y + 1].Type == "Empty") return true;
        }
        return false;
    },

    canBuildStable: function(){
        if (this.stables.length < this.MAX_STABLES) {
            for (var x = 0; x < this.PLOT_GRID_WIDTH ; x++) {
                for (var y = 0; y < this.PLOT_GRID_HEIGHT; y++) {
                    var o = this.grid[x][y];
                    if ((o.Type == "Empty" || o.Type == "Pasture") && o.HasStable == false)
                        return true;
                }
            }
        }
        return false;
    },

    /**
        Given a grid index, an array of existing plot items, and an array
        of plot items being added, this will verify that every plot can reach
        every other plot.
    */
    requiredAsPath: function (index, existing, added) {
        var obj = this;
        function isFieldAt(index) {
            if (index >= 0 && index < obj.PLOT_GRID_WIDTH * obj.PLOT_GRID_HEIGHT) {
                if ($.inArray(index, added) > -1 || $.inArray(index, existing) > -1) return true;
            }
            return false;
        }

        var toCheck = [];
        var newExisting = [];
        // convert existing to indexes
        for (var i in existing) {
            var exister = existing[i];
            newExisting.push(exister.y * 5 + exister.x);
        }
        existing = newExisting;

        if (isFieldAt(index - 1)) toCheck.push(index-1);
        if (isFieldAt(index + 1)) toCheck.push(index+1);
        if (isFieldAt(index - this.PLOT_GRID_WIDTH)) toCheck.push(index - this.PLOT_GRID_WIDTH);
        if (isFieldAt(index + this.PLOT_GRID_WIDTH)) toCheck.push(index + this.PLOT_GRID_WIDTH);

        if (toCheck.length < 2) return false;

        for (var start = 0; start < toCheck.length-1; start++) {
            for (var end = start + 1; end < toCheck.length; end++) {
                var si = toCheck[start];
                var ei = toCheck[end];
                var s = { x: si % this.PLOT_GRID_WIDTH, y: Math.floor(si / this.PLOT_GRID_WIDTH) };
                var e = { x: ei % this.PLOT_GRID_WIDTH, y: Math.floor(ei / this.PLOT_GRID_WIDTH) };

                if (!this.pathable(s,e,index,existing,added))
                    return true;
            }
        }
                


        return false;

    },


    allPathable: function (sets) {
        var toCheck = [];
        for (var set in sets) {
            set = sets[set];

            for (var element in set) {
                toCheck.push(set[element]);
            }
        }

        var pathable = true;
        for (var i = 1; i < toCheck.length; i++) {
            if(!this.pathable(toCheck[0], toCheck[i], [], [], toCheck))
                pathable = false;
        }
        return pathable;

    },


    pathable: function (start, end, excluding, existing, added) {
        var obj = this;
        if (start.x === undefined)
            start = { x: start % this.PLOT_GRID_WIDTH, y: Math.floor(start / this.PLOT_GRID_WIDTH) };

        if (end.y === undefined)
            end = { x: end % this.PLOT_GRID_WIDTH, y: Math.floor(end / this.PLOT_GRID_WIDTH) };


        var graphNodes = [];
        for (var x = 0; x < this.PLOT_GRID_WIDTH; x++) {
            graphNodes[x] = [];
            for (var y = 0; y < this.PLOT_GRID_HEIGHT; y++) {
                graphNodes[x][y] = 0;
            }
        }

        function prepGraph(array) {
            for (var i in array) {
                var index = array[i],
                    x = index % obj.PLOT_GRID_WIDTH,
                    y = Math.floor(index / obj.PLOT_GRID_WIDTH);
                if (index != excluding) 
                    graphNodes[x][y] = 1;
            }

        }

        prepGraph(existing);
        prepGraph(added);
            
        for (var i in added) {
            var index = added[i],
                x = index % this.PLOT_GRID_WIDTH,
                y = Math.floor(index / this.PLOT_GRID_WIDTH);

            if (index != excluding) {
                var graph = new Graph(graphNodes),
                    start = graph.grid[start.x][start.y],
                    end = graph.grid[end.x][end.y],
                    result = astar.search(graph, start, end);
                if (result.length == 0)
                    return false;
            }
        }

        return true;
    },

    clone: function () {
        var n = new PlayerBoard(this.display.clone(), this.player);
        n.update(this.player);
        return n;
    },

}


/**
    The Personal Supply holds all the information about resource
    quantities for a player.
 */

function PersonalSupply(display, player) {
    this.display = display
    if (player)
        this.update(player);

}

PersonalSupply.prototype = {
    display:null,
    food: 0,
    wood: 0,
    clay: 0,
    reed: 0,
    stone: 0,
    grain: 0,
    vegetables: 0,
    sheep: 0,
    boar: 0,
    cattle: 0,

    clone: function(){
        var ps = new PersonalSupply(this.display.clone())
        ps.food = this.food;
        ps.wood = this.wood;
        ps.clay = this.clay;
        ps.reed = this.reed;
        ps.stone = this.stone;
        ps.grain = this.grain;
        ps.vegetables = this.vegetables;
        ps.sheep = this.sheep;
        ps.boar = this.boar;
        ps.cattle = this.cattle;

        return ps;
    },

    clear: function(){
        this.setResource(Resource.Wood, 0);
        this.setResource(Resource.Food, 0);
        this.setResource(Resource.Clay, 0);
        this.setResource(Resource.Reed, 0);
        this.setResource(Resource.Stone, 0);
        this.setResource(Resource.Grain, 0);
        this.setResource(Resource.Vegetables, 0);
        this.setResource(Resource.Sheep, 0);
        this.setResource(Resource.Boar, 0);
        this.setResource(Resource.Cattle, 0);
    },

    update: function (player) {
        if (player) {
            for (var i in Resource) {
                console.info(i, Resource[i], player[i]);
                if (!isNaN(player[i])) {
                    this.setResource(i, player[i]);
                }
            }
            /*
            var vals = player.PersonalSupply
            for (var i in vals) {
                this.setResource(i, vals[i]);
            }
            */
        }

        if (player.Farmyard) {
            var animalCounts = {
                Sheep: 0,
                Boar: 0,
                Cattle: 0
            }

            for (var i in player.Farmyard.AnimalLocations) {
                var o = player.Farmyard.AnimalLocations[i];
                animalCounts[o.AnimalType] += o.AnimalCount;
            }

            for (var type in animalCounts) {
                this.setResource(type, animalCounts[type]);
            }
        }


    },

    modifyResource: function(name, value){
        name = name.toLowerCase();
        if (!isNaN(this[name])) {
            var x = this[name];
            this.setResource(name, x + parseInt(value));
        }
    },

    setResource: function(name, value){
        name = name.toLowerCase();
        console.info("Setting Resource", name, value);
        this[name] = value;
        this.display.find("." + name + " .count").text(value);
        this.fireEvent("resourceUpdated");

    },

    modifyWood: function(amount){
        this.setResource(Resource.Wood, this.wood + amount);
    },

    modifyFood: function(amount){
        this.setResource(Resource.Food, this.food + amount);
    },

    modifyClay: function(amount){
        this.setResource(Resource.Clay, this.clay + amount);
    },

    modifyReed: function(amount){
        this.setResource(Resource.Reed, this.reed + amount);
    },

    modifyStone: function(amount){
        this.setResource(Resource.Stone, this.stone + amount);
    },

    modifyGrain: function(amount){
        this.setResource(Resource.Grain, this.grain + amount);
    },

    modifyVegetables: function(amount){
        this.setResource(Resource.Vegetables, this.vegetables + amount);
    },

    modifySheep: function (amount) {
        this.setResource(Resource.Sheep, this.sheep + amount);
    },

    modifyBoar: function (amount) {
        this.setResource(Resource.Boar, this.boar + amount);
    },

    modifyCattle: function (amount) {
        this.setResource(Resource.Cattle, this.cattle + amount);
    },

}
enableEvents(PersonalSupply,
    ["resourceUpdated"]);

