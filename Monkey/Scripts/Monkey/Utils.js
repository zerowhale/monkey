var Monkey = new (function(){

    this.windows = {
        splash: $("#splash"),
        lobby: $("#lobby"),
        game: $("#game")
    };

    this.hideSplash = function () {
        this.hideWindow(this.windows.splash);
    };

    this.hideWindow = function (window) {
        window.fadeOut(400);
    };

    this.showWindow = function (window) {
        if (typeof (window) == "string")
            window = this.windows[window];

        window.show();
    };

    this.mousePosition = { x: 0, y: 0 };
    this.mouseDelta = {x: 0, y:0 };

    this.game = null;

    {
        var obj = this;
        document.onmousemove = function (e) {
            var x = e.pageX,
                y = e.pageY;
            obj.mouseDelta.x = x - obj.mousePosition.x;
            obj.mouseDelta.y = y - obj.mousePosition.y;
            obj.mousePosition.x = x;
            obj.mousePosition.y = y;
        }

    }



})();




$.fn.removeClassLike = function (match) {
    this.each(function (i, el) {
        var classes = el.className.split(" ").filter(function (c) {
            return c.lastIndexOf(match, 0) !== 0;
        });
        el.className = $.trim(classes.join(" "));
    });
    return this;
};


function isArray(o) {
    return Object.prototype.toString.call(o) === '[object Array]';
}

function useVh(item) {
    offset = item.position();
    item.css("left", pxToVh(offset.left));
    item.css("top", pxToVh(offset.top));
}

function pxToVh(px) {
    var h = $(window).height();
    var p = px / h;
    return (p * 100) + "vh";
}

function pxToVw(px) {
    var w = $(window).width();
    var p = px / w;
    return (p * 100);
}

function getCdataAsText(obj) {
    if (obj["#cdata-section"])
        obj = obj["#cdata-section"];
    obj = $("<div/>").html(obj);
    obj.find("br").replaceWith(" ");
    return obj.text();
}

function extend(base, sub) {
    var origProto = sub.prototype;
    sub.prototype = Object.create(base.prototype);
    for (var key in origProto) {
        sub.prototype[key] = origProto[key];
    }

    sub.prototype.constructor = sub;

    Object.defineProperty(sub.prototype, 'constructor', {
        enumerable: false,
        value: sub
    });
}



function preloadImage(path) {
    (new Image()).src = path;
}

function enableEvents(obj, registeredEvents) {
    function binding(object, func) {
        this.object = object;
        this.func = func;
    }

    obj.prototype.__events = null;

    /**
        Methods that allow a handler to be bound to an event
    */
    obj.prototype.bind =
    obj.prototype.bindEvent = function (name, func) {
        var caller = obj.prototype.bindEvent.caller;
        if (!this.__events)
            this.__events = {};
        if (!this.__events[name])
            this.__events[name] = [];
        this.__events[name].push(new binding(caller, func));
    }

    /**
        Calls all handlers attached the specified event.
    */
    obj.prototype.fireEvent = function (name) {
        if (!this.__events) return;
        var events = this.__events[name];
        if (!events) return;

        var args = [];
        for (var i = 1; i < arguments.length; i++)
            args.push(arguments[i]);

        for (var i = 0; i < events.length; i++) {
            if (events[i] != null)
                events[i].func.apply(this, args);
        }
    }

    /**
        Clears all event handlers attached to the named event
    */
    obj.prototype.unbind = function (name) {
        if (!this.__events) return;

        var caller = obj.prototype.unbind.caller;
        for (var e in this.__events[name]) {
            this.__events[name][e] = null;
        }
    }

    /**
        Registers an event with a method, creating a function on
        that method that can be used as an alias to obj.bind("event", function(){ }),
        ie: obj.event(function(){ })
    */
    obj.prototype.registerEvent = function (name) {
        this[name] = function (handler) {
            this.bind(name, handler);
        }
    }

    // Create all pre-registered event binding aliases
    if (isArray(registeredEvents)) {
        for (var e in registeredEvents) {
            var event = registeredEvents[e];
            obj.prototype.registerEvent(event);
        }
    }
}

function playSound(file) {
    var snd = new Audio(file);
    snd.play();
}

