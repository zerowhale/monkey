///
/// Creates a new game board based on the game parameters
///
/// Parameter Options:
///    map - preset map to load
///    numPlayers - number of players in the game
///    randomizeTiles - If the tiles should be randomized
///    randomizeNumbers - if the numbers should be randomized
function Board(params, onLoad) {

    {
        if (!params.numPlayers) 
            throw new Error("Must specify number of players to load a board.");
        
        if (params.map) {
            //this.loadMapLayout(params.map, onLoad);
            return;
        }
    }
}

Board.prototype.constructor = Board;

$(function () {
    var tiles = Catan.Tiles,
        P = tiles.pastures, W = tiles.forest, H = tiles.hills, M = tiles.mountains, F = tiles.fields, 
        D = tiles.desert;

    Board.prototype.tiles = [];

    Board.prototype.loadMapLayout = function (map, onLoad) {
        for (var tile in map.tiles) {

        }
    };


    Board.MapLayouts = {
        mountainRange4Player: {
            players: 4,
            tiles: [  P, W, H,
                     H, M, W, F,
                    F, F, M, F, P,
                     D, F, M, H,
                      P, F, P]
        }
    };

})
