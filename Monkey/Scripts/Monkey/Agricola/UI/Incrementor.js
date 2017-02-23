
function Incrementor(display, amount) {
    var obj = this;
    this.display = display;
    this.amount = amount;

    this.decrementButton = display.find(".minus.button");
    this.incrementButton = display.find(".plus.button");
    this.countLabel = display.find(".label .count")
    this.decrementButton.click(function () {
        obj.fireEvent("decrementClick");
    });

    this.incrementButton.click(function () {
        obj.fireEvent("incrementClick");
    });

    this.incrementButton.enable = function (enabled) {
        if (enabled) obj.incrementButton.removeClass("disabled");
        else obj.incrementButton.addClass("disabled");
    };

    this.decrementButton.enable = function (enabled) {
        if (enabled) obj.decrementButton.removeClass("disabled");
        else obj.decrementButton.addClass("disabled");
    }
}
Incrementor.prototype = {
    display: null,
    decrementButton: null,
    incrementButton: null,
    countLabel: null,
    _quantity: 0,
    quantity: 0,            // external readonly

    setQuantity: function (amount) {
        this._quantity = this.quantity = amount;
        this.countLabel.text(this._quantity);
    },

    decrementClick: function (event) {
        this.bindEvent("decrementClick", event);
    },

    incrementClick: function (event) {
        this.bindEvent("incrementClick", event);
    },

    hide: function () {
        this.display.addClass("disabled");
    },

    show: function(){
        this.display.removeClass("disabled");
    },

    increment: function () {
        this.setQuantity(this._quantity + this.amount);
    },

    decrement: function () {
        this.setQuantity(this._quantity - this.amount);
    },


}
enableEvents(Incrementor);
