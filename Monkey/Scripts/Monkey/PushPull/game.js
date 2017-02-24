debugger;
var PushPull, Game;
Game = PushPull = {


    display: null,

    init: function () {
        console.info("PushPull Initialized");
        var obj = this,
            doc = $(document),
            canvas = $("#gameCanvas");
        this.display = $("#game");

        document.oncontextmenu = function () { return false; }


        window.onresize = function () {
            Renderer.resize(doc.width(), doc.height());
        }
    },

    join: function (game) {
        console.info("Joining game", game);
        var obj = this;

        this.display.show();
    },

    update: function (data) {
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
PushPull.GameStates = {
    boardSelection: "BoardSelection",
    finished: "Finished"
}

///
/// Resources types
///
PushPull.Resources = {
    lumber: "lumber",
    wool: "wool",
    ore: "ore",
    brick: "brick",
    grain: "grain"
};

///
/// Tiles types
///
PushPull.Tiles = {
    desert: "desert",
    fields: PushPull.Resources.grain,
    forest: PushPull.Resources.lumber,
    hills: PushPull.Resources.brick,
    mountains: PushPull.Resources.ore,
    pastures: PushPull.Resources.wool
};

