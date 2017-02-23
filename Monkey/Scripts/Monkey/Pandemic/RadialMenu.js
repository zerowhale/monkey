function RadialMenu(display) {
    this.display = display;

    var city;
    var optionValues = [];
    var mouseDownStart = { x: 0, y: 0 };
    var showTimeout;
    var pos = { x: 0, y: 0 };
    var active = false;

    this.prepare = function (cityData, options) {
        var obj = this;
        var width = $(window).width();
        var scale = width / 1920;
        var left = pxToVw((cityData.X * scale) - this.display.width() / 2);
        var top = pxToVw((cityData.Y * scale) - this.display.height() / 2);
        city = cityData;

        pos.x = cityData.X;
        pos.y = cityData.Y;
        this.display.css({
            "left": left + "vw",
            "top": top + "vw"
        })
        active = true;

        optionValues = options;
        var up = this.display.find(".up");
        up.find(".text").text(options[RadialMenu.Up]);
        var right = this.display.find(".right");
        right.find(".text").text(options[RadialMenu.Right]);
        var down = this.display.find(".down");
        down.find(".text").text(options[RadialMenu.Down]);
        var left = this.display.find(".left");
        left.find(".text").text(options[RadialMenu.Left]);

        up.toggleClass("enabled", options[RadialMenu.Up] != PlayerAction.None);
        right.toggleClass("enabled", options[RadialMenu.Right] != PlayerAction.None);
        down.toggleClass("enabled", options[RadialMenu.Down] != PlayerAction.None);
        left.toggleClass("enabled", options[RadialMenu.Left] != PlayerAction.None);

        mouseDownStart.x = Monkey.mousePosition.x;
        mouseDownStart.y = Monkey.mousePosition.y;
        //$(document).on("mousemove", trackMouse);

        $(document).one("mouseup", function (e) {
            clearShowTimeout();
            //$(document).off("mousemove", trackMouse);
            obj.selectionHandler(e.pageX, e.pageY);
        })

        showTimeout = setTimeout(function () {
            //$(document).off("mousemove", trackMouse);
            obj.show();
        }, 366);

    }

    this.show = function () {
        this.display.show();
    }

    this.selectionHandler = function (x, y) {
        if (active && gestureDistance() > 20) {
            var a = Math.atan2(y - pos.y, x - pos.x) * 180 / Math.PI,
                sel = RadialMenu.Left;
            if (a >= -45 && a < 45)
                sel = RadialMenu.Right;
            else if (a < -45 && a > -135)
                sel = RadialMenu.Up;
            else if (a >= 45 && a < 135)
                sel = RadialMenu.Down;

            console.info("selected", sel, a);
            this.fireEvent("selected",optionValues[sel], city);
        }
        active = false;
        this.hide();
    }


    this.hide = function () {
        this.display.hide();
    }

    this.selected = function (handler) {
        this.bindEvent("selected", handler);
    }

    {
        var obj = this;
        var menuTimeout;
        this.display.on("mousemove", function () {
            clearTimeout(menuTimeout);
        });

        this.display.find("> div")
            .on("mousemove", function () {
                var dir = $(this).attr("data-direction");
                var optionValue = optionValues[RadialMenu[dir]];
                if (optionValue != PlayerAction.None) {
                    if (!(active == this)) {
                        var option = $(this);
                        option.addClass("hover");
                        active = this;

                        if (optionValue == PlayerAction.CharterFlight || optionValue == PlayerAction.BuildResearchStation) {
                            var p = Game.players[Game.activePlayer];
                            p.hand.highlightCard(p.Location)
                        }
                        else if (optionValue == PlayerAction.DirectFlight) {
                            var p = Game.players[Game.activePlayer];
                            p.hand.highlightCard(city.Id)
                        }
                    }
                }
            })
            .on("mouseout", function () {
                $(this).removeClass("hover");
                var p = Game.players[Game.activePlayer];
                p.hand.removeHighlights();
                active = false;
            })


        /*
        this.display.on("mouseout", function () {
            menuTimeout = setTimeout(function () {
                active = false;
                obj.display.hide();
                clearTimeout(menuTimeout);
            }, 666)
        })
        */
    }

    function clearShowTimeout() {
        clearTimeout(showTimeout);
    }

    function gestureDistance() {
        var x1 = mouseDownStart.x,
            y1 = mouseDownStart.y,
            x2 = Monkey.mousePosition.x,
            y2 = Monkey.mousePosition.y,
            d = Math.sqrt((y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1));

        return d;
    }
}

RadialMenu.Up = 0;
RadialMenu.Right = 1;
RadialMenu.Down = 2;
RadialMenu.Left = 3;

$(document).ready(function () {
    enableEvents(RadialMenu);
})