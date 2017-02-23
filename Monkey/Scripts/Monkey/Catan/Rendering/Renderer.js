function loadMesh(mesh, callback) {
    var loader = new THREE.JSONLoader();
    loader.load(mesh, function (geometry, materials) {
        var m = new THREE.Mesh(geometry, new THREE.MeshFaceMaterial(materials));
        if(typeof(callback) == "function")
            callback(m);
    });
}

Renderer = {
    init: function (container, width, height) {
        this._renderer = new THREE.WebGLRenderer();
        this._renderer.setClearColor(0xefefa3, 1);
        this._renderer.setSize(width, height);
        this._scene = new THREE.Scene();

        this._boardCamera = new THREE.PerspectiveCamera(
            45,
            width / height,
            .1,
            1000);

        this._boardCamera.position.set(0, 5, 4);

        this._cameras.push(this._boardCamera);
        this._scene.add(this._boardCamera);


        var light = new THREE.AmbientLight(0xffffff); 
        this._scene.add(light);

        container.append(this._renderer.domElement);

        var obj = this;
        this._drawMethod = this._firstDraw;
        console.info(this._drawMethod);
        setInterval(function () { obj._drawMethod(); }, 100);

    },

    resize: function (width, height) {
        this._renderer.setSize(width, height);
        for (var c in this._cameras) {
            var cam = this._cameras[c];
            cam.aspect = width / height;
            cam.updateProjectionMatrix();
        }
    },

    addObject: function(object){
        this._objects.push(object);
        console.info(object.display);
        this._scene.add(object.display);
    },

    onFirstDraw: function(){},

    _update: function () {
    },


    _firstDraw: function (timestamp) {
        this.onFirstDraw();
        this._drawMethod = this._draw;
        this._draw();
    },

    _draw: function (timestamp) {
        if (this._objects[0])
            this._boardCamera.lookAt(this._objects[0].display.position);

        this._renderer.render(this._scene, this._boardCamera);
        //window.requestAnimationFrame(this._draw);
    },

    _drawMethod: null,


    _scene: null,
    _renderer: null,
    _boardCamera: null,
    _cameras: [],
    _objects: []

}