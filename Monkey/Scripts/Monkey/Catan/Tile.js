function Tile(type, onLoad) {
    DisplayObject.call(this);

    const MESH_ROOT = "/Content/Catan/models/tiles/";
    const MESH_EXTENSION = ".json";

    {
        var obj = this;
        if (onLoad)
            this.onLoad = onLoad;

        if (type) {
            this.type = type;

            loadMesh(MESH_ROOT + type + MESH_EXTENSION, function (mesh) {
                obj.setDisplay(mesh);
                if (typeof (obj.onLoad) == "function")
                    obj.onLoad(obj);
            });
        }
    }
}

Tile.prototype = Object.create(DisplayObject.prototype);
Tile.prototype.constructor = Tile;
Tile.prototype.type = null;
Tile.prototype.onLoad = null;

Tile.getResourceTile = function (resource, onLoad) {
    if (Catan.isResource(resource))
        return new Tile(resource, onLoad)
    else
        throw new Error("Not a resource");
}

Tile.getDesertTile = function (onLoad) {
    return new Tile("desert", onLoad);
}