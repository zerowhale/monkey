;function HarvestPanel(display) {
    this.display = display;
    this.status = display.find(".harvest-status .players");
    this.statusTemplate = this.status.find(".player").remove();
    this.resourceTemplate = display.find(".resource-conversion").remove();
    this.submitButton = display.find(".feed-family .button");

}

HarvestPanel.prototype = {
    display: null,
    status: null,
    statusTemplate: null,
    resourceTemplate: null,

    familyFed: false,
    submitButton: null,

    rows: null,
    _foodRequired: null,
    _board: null,
    _personalSupply: null,
    _snapshot: null,

    startHarvest: function (game, player, board) {
        console.info("Start harvest called");
        var obj = this;
        this._board = board;

        var foodRequired = this._foodRequired = board.familySize * 2 - player.NumBabies;

        this.display.find(".food-required").text(foodRequired);
        this.display.find(".family-size").text(board.familySize);

        var rows = this.rows = [];
        var resourcesDisplay = this.display.find(".resource-conversions").empty();
        var resources = Curator.getHarvestResources(player);
        var rowNum = 0;

        var personalSupply = this._personalSupply = this._snapshot = board.personalSupply.clone();

        this.submitButton.addClass("disabled");
        this.submitButton.show();
        this.display.find(".food-total .total").text(0);
        this.display.find(".feed-complete-message").hide();
        this.display.find(".resource-conversion").removeClass("disabled");
        this.familyFed = false;

        for (var r in resources) {
            var available = resources[r].available;
            if (resources[r].maxCook > 0 && resources[r].maxCook < available)
                available = resources[r].maxCook;
                
            addRow(new ResourceConversionRow(this.resourceTemplate.clone(), resources[r], personalSupply));
        }
        addRow(new ResourceConversionRow(this.resourceTemplate.clone(), new ResourceConversion(null, "Beg", 1, Resource.Food, 1, null), personalSupply));

        function addRow(row) {
            row.quantityChanged(function () { obj._rowQuantityChangedHandler(); });
            rows.push(row);

            if (rowNum % 2 == 0) row.display.addClass("alt-row");
            rowNum++;

            resourcesDisplay.append(row.display);
        }

        this.status.empty();
        for (var p in game.Players) {
            var player = game.Players[p];
            var name = Game.formatNameForSystemMessage(player.Name);
            var row = this.statusTemplate.clone();
            row.attr("data-name", player.Name);
            row.find(".ready").html(player.Harvesting ? "&#x2717;" : "&#10004;");
            row.find(".name").append(name);
            this.status.append(row);

            if (player.Name == profile.name && !player.Harvesting) {
                this._familyFedHandler();
            }
        }
        this.submitButton.unbind("click");
        this.submitButton.click(function () {
            var data = [];
            
            obj._snapshot = obj._personalSupply.clone();

            for (var r in rows) {
                var row = rows[r]
                if (row.count > 0 && row.resourceConversion.inType != "Beg") {
                    var rc = row.resourceConversion;
                    data.push(new ResourceConversionData(rc.id, row.count, rc.inType, rc.inAmount, rc.outType));
                }
            }

            obj.fireEvent("submit", data);
        });
    },

    update: function (game) {
        console.info("Update harvest called");
        for (var p in game.Players) {
            var player = game.Players[p];
            var row = this.status.find(".player[data-name='" + player.Name + "']");
            row.find(".ready").html(player.Harvesting ? "&#x2717;" : "&#10004;");
            
            if (player.Name == profile.name && !player.Harvesting) {
                this._familyFedHandler();
            }
        }

        this._rowQuantityChangedHandler();
    },

    _rowQuantityChangedHandler: function() {
        var resources = {},
            total = 0,
            rows = this.rows;

        for (var r in rows) {
            var row = rows[r];
            var type = row.resourceConversion.inType.toLowerCase();
            if (resources[type] === undefined) 
                resources[type] = this._board.personalSupply[row.resourceConversion.inType.toLowerCase()];
                
            resources[type] -= row.count;
        }

        for (var i in rows) {
            var row = rows[i];
            var type = row.resourceConversion.inType.toLowerCase();

            this._personalSupply.setResource(type, resources[type]);

            if (row.resourceConversion.outType == Resource.Food)
                total += row.outCount;

            row.updateButtonStates();
        }

        this.display.find(".total").text(total);

        if (total >= this._foodRequired)
            this.submitButton.removeClass("disabled");
        else
            this.submitButton.addClass("disabled");
    },
    

    _familyFedHandler: function () {
        if (!this.familyFed) {
            var rows = this.rows;

            for (var r in rows) {
                var row = rows[r];
                row.setPersonalSupply(this._snapshot);
            }

            this.familyFed = true;
            this.submitButton.hide();
            this.display.find(".feed-complete-message").fadeIn(340);
            this.display.find(".resource-conversion").addClass("disabled");
        }

    },

    submit: function (func) {
        this.bindEvent("submit", func);
    }

}


enableEvents(HarvestPanel);
