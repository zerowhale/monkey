var Catan, Game;
Game = Catan = {


    display: null,

    init: function () {
        console.info("Catan Initialized");
        var obj = this,
            doc = $(document),
            canvas = $("#gameCanvas");
        this.display = $("#game");

        document.oncontextmenu = function () { return false; }

        Renderer.init(canvas, doc.width(), doc.height());
        Renderer.onFirstDraw = function () {
            canvas.show();
        },

        Tile.getResourceTile(Catan.Resources.lumber, function (tile) {
            Renderer.addObject(tile);
        });      

        Tile.getResourceTile(Catan.Resources.ore, function (tile) {
            tile.display.position.set(0, 0, 0.86603 * 2);
            Renderer.addObject(tile);
        });

        Tile.getResourceTile(Catan.Resources.grain, function (tile) {
            tile.display.position.set(1.5, 0, 0.86603);
            Renderer.addObject(tile);
        });

        Tile.getResourceTile(Catan.Resources.brick, function (tile) {
            tile.display.position.set(-1.5, 0, 0.86603);
            Renderer.addObject(tile);
        });

        Tile.getResourceTile(Catan.Resources.wool, function (tile) {
            tile.display.position.set(1.5, 0, -0.86603);
            Renderer.addObject(tile);
        });

        Tile.getDesertTile(function (tile) {
            tile.display.position.set(-1.5, 0, -0.86603);
            Renderer.addObject(tile);
        });

        window.onresize = function () {
            Renderer.resize(doc.width(), doc.height());
        }
    },

    join: function (game) {
        console.info("Joining game", game);
        var obj = this;

        this.display.show();
    },

    update: function (data){
        console.info("update", data);
    },

    isResource: function (item) {
        if (this.Resources[item])
            return true;
        return false;
    }
}

///
/// Game states
///
Catan.GameStates = {
    boardSelection: "BoardSelection",
    finished: "Finished"
}

///
/// Resources types
///
Catan.Resources = {
    lumber: "lumber",
    wool: "wool",
    ore: "ore",
    brick: "brick",
    grain: "grain"
};
    
///
/// Tiles types
///
Catan.Tiles = {
    desert: "desert",
    fields: Catan.Resources.grain,
    forest: Catan.Resources.lumber,
    hills: Catan.Resources.brick,
    mountains: Catan.Resources.ore,
    pastures: Catan.Resources.wool
};

