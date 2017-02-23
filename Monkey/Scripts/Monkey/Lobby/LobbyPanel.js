function Lobby(display) {
    var obj = this;
    this.display = display;
    this.gameList = display.find(".game-list");
    this.gameEntryTemplate;
    this.emptyEntry;

    this.onCreateGameClick = null;
    this.onJoinGameClick = null;

    this.init = function () {
        obj.emptyEntry = obj.gameList.find(".game-entry");
        obj.emptyEntry.remove();
        obj.gameEntryTemplate = obj.emptyEntry.remove();
    }();

    this.addGame = function (game) {
        var entry = obj.gameEntryTemplate.clone();

        console.info(game);
        entry.find(".game").text(game.GameTitle);
        entry.find(".name").text(game.Name);
        entry.find(".id").text(game.Id);
        entry.find(".num-players").text(game.NumPlayers);
        entry.find(".max-players").text(game.MaxPlayers);
        //entry.find(".family-mode").text(game.Family)

        entry.click(function () {
            var id = $(this).find(".id").text();

            if (typeof (obj.onJoinGameClick) == "function")
                obj.onJoinGameClick(id);

        })

        if (game.IsFull)
            entry.hide();

        obj.gameList.append(entry);
        this.display.find(".count").text(obj.gameList.find(".game-entry").length);

        obj.gameList.find(".clear").remove();
        obj.gameList.append("<div class='clear'></div>");
    }

    this.updateGame = function (gameId, numPlayers) {
        var entry = obj.gameList.find(".game-entry div:contains('" + gameId + "')");
        if (entry.length > 0) {
            entry = entry.parent();
            entry.find(".num-players").text(numPlayers);
            var maxPlayers = entry.find(".max-players").text();

            if (maxPlayers == numPlayers)
                entry.hide();
            else
                entry.show();

        }
    }

    this.removeGame = function (id) {
        console.info("removing game: ", id);
        var entry = obj.gameList.find("div:contains('" + id + "')");
        entry.remove();
        this.display.find(".count").text(obj.gameList.find(".game-entry").length);

    }

    
}