function Overlay(display, params) {
    this.display = display;
    this.collapsable = true;

    var COLLAPSED_CLASS = "collapsed";
    var COLLAPSED_COOKIE = "overlay-collapsed";

    {
        if (params) {
            if (params.collapsable != undefined)
                this.collapsable = params.collapsable;
        }

        var obj = this;

        if (this.collapsable) {
            var collapsed = $.cookie(COLLAPSED_COOKIE);
            if (collapsed == "true")
                obj.display.addClass(COLLAPSED_CLASS);

            var collapseBtn = this.display.find(".collapse");
            collapseBtn.on("click", function () {
                obj.display.toggleClass(COLLAPSED_CLASS);
                console.info(obj.display.hasClass(COLLAPSED_CLASS));
                $.cookie(COLLAPSED_COOKIE, obj.display.hasClass(COLLAPSED_CLASS));
            });
        }

        $(document).on("keyup", function (e) {
            switch (e.keyCode) {
                case 32:
                    if (obj.display.is(":visible")) {
                        obj.takeAction();
                    }
                    break;
            }
        })

        display.find(".cancel").on("click", function () {
            OverlayManager.hideOverlay(obj);
        })
    }
}

Overlay.prototype = {
    display: null,
    collapsable: null,

    show: function () {
        this.display.show();
    },

    hide: function () {
        this.display.hide();
    },


    takeAction: function () {}
}