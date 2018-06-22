

var gameConn = $.connection[loadedGame + "Hub"];


gameConn.client.update = function (game) {
    console.info("Game update:", game);
    if (Monkey.game)
        Monkey.game.update(game);
}

gameConn.client.message = function (notice) {
    console.info("Action Notice", notice);
    if (Monkey.game)
        Monkey.game.message(notice);
}

gameConn.client.startingGame = function (game, params) {
    console.info("\nStarting game ", game);

    Monkey.game = Game;
    Game.init();
    Game.join(game, params);
    Monkey.hideSplash();
}

gameConn.client.returnToLobby = function () {
    window.location = "/";
}

$.connection.hub.start();

