function AnimalChoicePopup(display) {
    var obj = this;
    FarmyardWithAnimalsPopup.call(this, display);

    this.buttons = display.find(".build-menu .wrapper");

    this.sheepButton = this.buttons.filter(".sheep-button");
    this.boarButton = this.buttons.filter(".boar-button");
    this.cattleButton = this.buttons.filter(".cattle-button");

    this.sheepButton.click(function () { obj.toggleButton(obj.sheepButton); });
    this.boarButton.click(function () { obj.toggleButton(obj.boarButton); });
    this.cattleButton.click(function () { obj.toggleButton(obj.cattleButton); });
}
AnimalChoicePopup.prototype = {
    buttons:null, 
    sheepButton: null,
    boarButton: null,
    cattleButton: null,

    current:null,

    show: function (player) {
        FarmyardWithAnimalsPopup.prototype.show.call(this, player);
        this.current = null;

        if (player.PersonalSupply.Food < 1)
            this.cattleButton.addClass("disabled");
        else
            this.cattleButton.removeClass("disabled");

        this.buttons.removeClass("selected");
        this.toggleButton(this.sheepButton);
    },


    toggleButton: function (btn) {
        if (!btn || btn.hasClass("selected"))
            return;

        if (!this.refundCurrent())
            return;

        switch(btn){
            case this.sheepButton:
                this.farmyard.animalManager.tryAssignNewAnimals(AnimalResource.Sheep, 1);
                this.farmyard.personalSupply.modifyFood(+1);
                break;

            case this.boarButton:
                this.farmyard.animalManager.tryAssignNewAnimals(AnimalResource.Boar, 1);
                break;

            case this.cattleButton:
                this.farmyard.animalManager.tryAssignNewAnimals(AnimalResource.Cattle, 1);
                this.farmyard.personalSupply.modifyFood(-1);
                break;

            default:
                return;
        }
        this.animalStateChanged();

        if (this.current)
            this.current.removeClass("selected");
        btn.addClass("selected");
        this.current = btn;

    },

    refundCurrent: function () {
        if (this.current != null) {
            switch (this.current) {
                case this.sheepButton:
                    if (this.farmyard.animalManager.tryRemove(AnimalResource.Sheep)) {
                        this.farmyard.personalSupply.modifyFood(-1);
                        return true;
                    }
                    return false;
                case this.boarButton:
                    if (this.farmyard.animalManager.tryRemove(AnimalResource.Boar))
                        return true;
                    return false;
                case this.cattleButton:
                    if (this.farmyard.animalManager.tryRemove(AnimalResource.Cattle)) {
                        this.farmyard.personalSupply.modifyFood(1);
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
        return true;
    },

    getSelection: function () {
        switch (this.current) {
            case this.sheepButton:
                return AnimalResource.Sheep;
            case this.boarButton:
                return AnimalResource.Boar;
            case this.cattleButton:
                return AnimalResource.Cattle;
        }
        return AnimalResource.Sheep;
    }
}
extend(FarmyardWithAnimalsPopup, AnimalChoicePopup);