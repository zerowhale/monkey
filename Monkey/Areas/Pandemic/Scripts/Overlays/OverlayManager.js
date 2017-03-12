var OverlayManager = {
    display: null,
   
    overlays: {
        epidemic: null,
        infect: null,
        treat: null,
        cardSelection: null,
        trade: null,
        tradeConfirmation: null,
        message: null,
        forecast: null
    },

    init: function(display){
        this.display = display;

        this.overlays.epidemic = new EpidemicOverlay(display.find(".epidemic"));
        this.overlays.infect = new InfectOverlay(display.find(".infect"));
        this.overlays.treat = new TreatOverlay(display.find(".treat-diseases"));
        this.overlays.cardSelection = new CardSelectionOverlay(display.find(".card-selection"));
        this.overlays.trade = new TradeOverlay(display.find(".trade"));
        this.overlays.tradeConfirmation = new TradeConfirmationOverlay(display.find(".confirm-trade"));
        this.overlays.message = new MessageOverlay(display.find(".message"));
        this.overlays.forecast = new ForecastOverlay(display.find(".forecast"));
        this.overlays.finished = new FinishedOverlay(display.find(".finished"));
    },

    showOverlay: function (overlay, modal) {
        if (modal !== false)
            modal = true;
        this.display.addClass("active");
        overlay.show();
    },

    hideOverlay: function (overlay) {
        if (overlay.display.is(":visible")) {
            this.display.removeClass("active");
            overlay.hide();
        }
    },

    hideOverlays: function(){
        this.display.removeClass("active");
        for (var o in this.overlays) {
            var ov = this.overlays[o];
            if(ov.display.is(":visible"))
                this.hideOverlay(ov);
        }
    }
}