function ResourceConversionRow(display, resourceConversion, personalSupply) {
    var rc = resourceConversion;

    this.display = display;
    this.personalSupply = personalSupply;
    this.resourceConversion = rc;
    this._inType = rc.inType.toLowerCase();
    this._outType = rc.outType.toLowerCase();

    display.addClass(this._inType);
    if (rc.inType != "Beg")
        display.find(".inType.icon").addClass(this._inType);
    else
        display.find(".inType.icon").replaceWith("<span class='beg'>Beg</span>");

    display.find(".outType.icon").addClass(this._outType);

    if (rc.inType == Resource.Food)
        display.find(".note").remove();
    else {
        var s = rc.inAmount + " " + rc.inType + " for " + rc.outAmount + " " + rc.outType;
        if (rc.inLimit)
            s += "&nbsp;&nbsp;<span>Limit " + rc.inLimit + "</span>";
        display.find(".conversion-info").html(s);

    }

    var obj = this;
    this.personalSupply.bind("resourceUpdated", function () { obj.personalSupplyUpdatedHandler(); });


    this.incrementor = new Incrementor(display.find(".incrementor"), rc.inAmount);
    this.incrementor.incrementClick(this.increment(rc.inType));
    this.incrementor.decrementClick(this.decrement(rc.inType));

    this.updateButtonStates();

}

ResourceConversionRow.prototype = {
    display: null,
    personalSupply: null,
    incrementor: null,
    externalTotal: 0,
    count: 0,               // The amount being converted
    outCount: 0,
    _inType: null,
    _outType: null,

    setPersonalSupply: function (ps) {
        var obj = this;
        this.personalSupply.unbind("resourceUpdated");
        this.personalSupply = ps;
        this.personalSupply.bind("resourceUpdated", function () { obj.personalSupplyUpdateHandler(); });
    },

    personalSupplyUpdatedHandler: function(){
        this.updateButtonStates();
    },

    decrement: function (inType) {
        var obj = this;
        return function () {
            if (this.quantity > 0) {
                this.decrement();
                obj.fireEvent("decremented");
                obj.updateCounts();
            }
        }
    },

    increment: function (inType) {
        var obj = this;
        var rc = obj.resourceConversion;
        return function () {
            if (obj._inType == "beg"
                || obj.personalSupply[obj._inType] >= rc.inAmount) {
                this.increment();
                obj.fireEvent("incremented");
                obj.updateCounts();
            }
        }
    },


    updateCounts: function () {
        this.outCount = (this.incrementor.quantity / this.resourceConversion.inAmount) * this.resourceConversion.outAmount;
        this.incrementor.display.find(".worth").text(this.outCount);
        this.count = this.incrementor.quantity;
        this.updateButtonStates();
        this.fireEvent("quantityChanged");
    },

    quantityChanged: function (event) {
        this.bindEvent("quantityChanged", event);
    },

    updateButtonStates: function () {
        this.incrementor.decrementButton.enable(this.count != 0);

        var intype =  this._inType;
        if (this._inType != "beg") {
            this.display.find(".available").text(this.personalSupply[this._inType]);

            var rc = this.resourceConversion;
            this.incrementor.incrementButton.enable(
                this.personalSupply[intype] >= rc.inAmount
                && ((rc.inLimit != null && (this.count / rc.inAmount < rc.inLimit))
                || !rc.inLimit));

        }


    },

    setExternalMaximum: function (max) {
        this.externalMaximum = max;
        this.updateButtonStates();
    }
}

enableEvents(ResourceConversionRow);
