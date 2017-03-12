///
/// Anything displayed in the 3d viewport should extend DisplayObject
///
function DisplayObject() {

}

DisplayObject.prototype = {
    setDisplay: function (display) {
        this.display = display;
    },

    display: null
}