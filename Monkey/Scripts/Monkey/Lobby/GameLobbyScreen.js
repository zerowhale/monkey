function GameLobby(display){
    var obj = this;

    this.id = null;
    this.display = display;
    this.playerList = display.find(".player-list");
    this.colorSelection = display.find(".colors");
    this.rowTemplate;
    this.ready = false;
    this.usedColors = null;

    this.init = function () {
        obj.rowTemplate = obj.display.find(".player-row");
        obj.rowTemplate.remove();

        obj.display.find(".leave-button").click(function () {
            lobbyConn.server.leaveGameLobby();
        });

        var ready = obj.display.find(".ready-button");

        ready.click(function () {
            lobbyConn.server.playerReady(!obj.ready);
        });
    }();


    this.show = function (game) {
        obj.update(game);
        obj.display.show();
    }

    this.hide = function() {
        obj.display.hide();
    }

    this.addPlayer = function (player, role) {
        var row = obj.rowTemplate.clone();
        var pt = getPlayerTemplate(player);
        var colors = row.find(".colors > div");
        colors.removeClass("disabled").removeClass("selected");

        if (player.Name == profile.name) {
            row.addClass("me");
            this.ready = player.Ready;

            colors.click(function () {
                lobbyConn.server.requestColor($(this).attr("data-color"));
            });

            for (var c in this.usedColors) {
                var color = this.usedColors[c];
                var colorOption = colors.filter("." + color.toLowerCase());
                if (color != player.Color) {
                    colorOption.addClass("disabled");
                }
                else {
                    colorOption.addClass("selected");
                }
            }
        }
        else {
            colors.hide();
            colors.filter("." + player.Color.toLowerCase()).show();
        }

        if (player.Ready) {
            row.addClass("ready")
        }


        row.find(".player-role").text(role);
        row.find(".player").replaceWith(pt);


        obj.playerList.append(row);
    }

    this.update = function (game) {
        console.info("GameLobby.update", game);
        obj.playerList.empty();
        obj.display.find(".game-name").text(game.Name);
        this.id = game.Id;

        var title = game.GameTitle;
        var manifest = games[title];
        console.info(manifest);
        
        if (manifest.ColorSelectionEnabled) {
            this.colorSelection.show();
            this.usedColors = [];
            for (var i = 0; i < game.Players.length; i++) {
                this.usedColors.push(game.Players[i].Color);
            }
        }
        else
            this.colorSelection.hide();

        for (var i = 0; i < game.Players.length; i++) {
            var title = game.Creator == game.Players[i].Name ? "Game Owner" : "Player " + (i + 2)
            obj.addPlayer(game.Players[i], title);
        }

        var info = this.display.find(".game-info");
        info.find(".game").text("");
        info.find(".name").text(game.Name);
        info.find(".max-players span").text(game.MaxPlayers);
        info.find(".family-mode span").text(game.Props.FamilyMode == false ? "Off" : "On");
    }

}