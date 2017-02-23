var panels, screens,
    lobbyConn,
    inGame = false,
    playerTemplate;


$(window).bind("load", function () {

});

$(document).ready(function () {
    
    lobbyConn = $.connection.lobbyHub;

    screens = {
        lobby: $(".lobby-screen"),
        gameLobby: new GameLobby($(".game-lobby-screen"))
    }

    panels = {
        lobby: new Lobby($(".lobby-panel")),
        createGame: $(".create-game-panel")
    }

    playerTemplate = $(".templates .player");

    lobbyConn.client.error = function (e) {
        console.error("Server Error:", e);
    }

    lobbyConn.client.addGameLobby = function (game) {
        panels.lobby.addGame(game);
    }

    lobbyConn.client.removeLobbyGame = function (id) {
        console.info("client.removeLobbyGame", id);
        panels.lobby.removeGame(id);
    }

    lobbyConn.client.loadGamesList = function (list) {
        Monkey.showWindow("lobby");
        Monkey.hideSplash();
        
        for (var i in list) {
            panels.lobby.addGame(list[i]);
        }
    }

    lobbyConn.client.moveToGameLobby = function (game) {
        screens.lobby.stop().animate({ height: "0vh" }, 420, "easeInCubic");
        screens.gameLobby.display.stop().animate({ height: "100vh" }, 420, "easeInCubic");
    }

    lobbyConn.client.joinLobbyFailed = function (reason) {
        switch (reason) {
            case "GAME_FULL":
                displayMessage("Failed to join game, game is full.");
                break;
        }
    }

    lobbyConn.client.updateLobbyGameInfo = function (gameId, numPlayers) {
        panels.lobby.updateGame(gameId, numPlayers);
    }

    lobbyConn.client.updateGameLobby = function (game) {
        console.info("lobbyGame", game);
        screens.gameLobby.update(game);
    }

    lobbyConn.client.leaveGameLobby = function (reason) {
        console.info("Leave game lobby", reason);
        returnToLobby();

        displayMessage(reason);
    }

    lobbyConn.client.returnToLobby = function () {
        returnToLobby();
    }

    lobbyConn.client.launchGame = function (game) {
        console.info("launching game view", game);
        window.location.href = "/Game/" + game;
    }









    returnToLobby = function () {
        console.info("Returning to lobby");
        $("#lobby").show().stop().animate({ height: "100vh" }, 420, "easeInCubic");
        screens.lobby.stop().animate({ height: "100vh" }, 420, "easeInCubic");
        screens.gameLobby.display.stop().animate({ height: "0" }, 420, "easeInCubic");
    }



    panels.lobby.onJoinGameClick = function (gameId) {
        lobbyConn.server.joinGameLobby(gameId);
    }

    $("#username").focusout(function () {
        updateName();
    });

    $.connection.hub.start();


    wireCreateGamePanel();

    $('img').on('dragstart', function (event) { event.preventDefault(); });
});

function leave() {
    lobbyConn.server.leave();
}


function getPlayerTemplate(player) {
    var p = playerTemplate.clone();
    p.find(".name").text(player.Name);
    p.find(".id").text(player.Id);
    return p;
}

function displayMessage(message) {
    $(".messages-panel").text(message).show().fadeOut(2000, "easeInCubic", function () { });

}

function wireCreateGamePanel() {
    var gamesList = panels.createGame.find(".games-list");
    var gameTitle = panels.createGame.find(".game");
    var createGameButton = panels.createGame.find(".create-game-button");
    var createGameName = panels.createGame.find(".game-name");
    var createGameMaxPlayers = panels.createGame.find(".max-players");
    var gameOptions = panels.createGame.find(".game-options");

    createGameName.keyup(function () {
        var t = $(this).val();

        if (t.length > 0) {
            createGameButton.removeClass("disabled");
        }
        else {
            createGameButton.addClass("disabled");
        }
    });

    var opening = false;
    panels.createGame.find(".open-button").click(function () {
        if (!opening) {
            $(this).text("Cancel");
            panels.createGame.stop().animate({ height: "50vh" }, 420, "easeInCubic");
            panels.lobby.display.stop().animate({ height: "50vh" }, 420, "easeInCubic");
        }
        else {
            $(this).text("Create Game");
            panels.createGame.stop().animate({ height: "6vh" }, 420, "easeInCubic");
            panels.lobby.display.stop().animate({ height: "96vh" }, 420, "easeInCubic");
        }
        opening = !opening;
    });


    gamesList.find("li > div").mouseup(function () {
        gamesList.find("li > div").removeClass("selected");
        $(this).addClass("selected");
        var game = $(this).attr("data-game");
        updateCreateOptions(games[game]);
    });
    $(gamesList.find("li > div")[0]).trigger("click");

    createGameButton.click(function () {
        var gameData = games[gamesList.find(".selected").attr("data-game")];
        var params = {};

        for (var o in gameData.CreationProperties) {
            var option = gameData.CreationProperties[o];
            switch (option.Type) {
                case "Boolean":
                    params[option.Id] = gameOptions.find("." + option.Id).prop("checked");
                    break;

                case "List":
                    params[option.Id] = gameOptions.find("." + option.Id + " option:selected").text();
                    break;
            }
        }
        lobbyConn.server.createGameLobby(gameData.ViewPath, createGameName.val(), createGameMaxPlayers.val(), params);
    });

    function updateCreateOptions(gameData) {
        panels.createGame.find(".game").html(gameData.Name);

        var maxPlayers = panels.createGame.find(".max-players");
        maxPlayers.find("option").remove();
        for (var i = 0; i < gameData.MaxPlayers; i++) {
            var option = document.createElement("option");
            option.text = i+1;
            option.selected = i == gameData.MaxPlayers - 1;
            maxPlayers.append(option);
        }

        gameOptions.empty();

        for (var o in gameData.CreationProperties) {
            var option = gameData.CreationProperties[o];
            console.info(option);
            var element = null;
            switch (option.Type) {
                case "Boolean":
                    element = "<label>" + option.Name + ":</label> <input type=\"checkbox\" class=\"" + option.Id + "\" />";
                    break;
                case "List":
                    
                    element = "<label>" + option.Name + ":</label> <select class=\"" + option.Id + "\">";
                    for (var v in option.Data.Value) {
                        var value = option.Data.Value[v];
                        element += "<option>" + value + "</option>";
                    }
                    value += "</select>";
                    break;
                default:
                    console.error("Unsupported game option type:", option.Type);
            }
            if(element != null)
                gameOptions.append($("<div>" + element + "</div>"));
        }
    }
}