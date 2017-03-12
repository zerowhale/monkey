
function FarmyardWithAnimalsPopup(display) {
    FarmyardPopup.call(this, display);

    var animalControls = display.find(".animal-controls");

    this.unassignedAnimals = {
        Sheep: new Incrementor(animalControls.find(".unassigned-animals .incrementor.sheep")),
        Boar: new Incrementor(animalControls.find(".unassigned-animals .incrementor.boar")),
        Cattle: new Incrementor(animalControls.find(".unassigned-animals .incrementor.cattle"))
    };

    this.cookAnimals = {
        Sheep: new Incrementor(animalControls.find(".cook-animals .incrementor.sheep")),
        Boar: new Incrementor(animalControls.find(".cook-animals .incrementor.boar")),
        Cattle: new Incrementor(animalControls.find(".cook-animals .incrementor.cattle"))
    };


}
FarmyardWithAnimalsPopup.prototype = {
    manageButton: null,
    unassignedAnimals: null,


    _cookValues: null,

    show: function (player) {
        var obj = this;
        FarmyardPopup.prototype.show.call(this);
        var manager = this.farmyard.animalManager;

        var canCook = Curator.canCook(player);
        if (canCook) {
            this.display.find(".cook-animals").show();
            this._cookValues = {};
            var x = Curator.getAvailableResourceConversions(this.player);
            for (var i in x) {
                var data = x[i];
                if (data.outType === Resource.Food && data.inLimit === 0) {
                    if (data.inType === Resource.Sheep)
                        this._cookValues[Resource.Sheep] = data;
                    else if (data.inType === Resource.Boar)
                        this._cookValues[Resource.Boar] = data;
                    else if (data.inType === Resource.Cattle)
                        this._cookValues[Resource.Cattle] = data;
                }
            }
        }
        else
            this.display.find(".cook-animals").hide();

        wireIncrementors();
        this.submitButton.removeClass("disabled");

        function wireIncrementors() {
            for (var animal in AnimalResource) {
                var unassigned = obj.unassignedAnimals[animal];

                unassigned.unbind("incrementClick");
                unassigned.unbind("decrementClick");

                unassigned.incrementClick(incrementRI(animal));
                unassigned.decrementClick(decrementRI(animal));


                var cook = obj.cookAnimals[animal];

                cook.unbind("incrementClick");
                cook.unbind("decrementClick");

                if (canCook) {
                    cook.incrementClick(incrementCook(animal));
                    cook.decrementClick(decrementCook(animal))
                }

            }
            obj.animalStateChanged();
        }

        function decrementCook(animal) {
            return function () {
                if (manager.tryUncook(animal)) {
                    obj.animalStateChanged();
                }
            }
        }

        function incrementCook(animal) {
            return function () {
                if (manager.tryCook(animal)) {
                    obj.animalStateChanged();
                }
            }
        }


        function incrementRI(animal) {
            return function () {
                if (manager.tryUnassignAnimal(animal)) {
                    obj.animalStateChanged();
                }
            }
        }

        function decrementRI(animal) {
            return function () {
                if (manager.tryAssignAnimal(animal)) {
                    obj.animalStateChanged();
                }
            }
        }

    },

    animalStateChanged: function () {

        var manager = this.farmyard.animalManager;
        var totals = manager.getAnimalCounts();
        var unassigned = manager.unassigned;
        var cooked = manager.cooked;



        for (var animal in AnimalResource) {
            var incr = this.unassignedAnimals[animal];
            incr.setQuantity(unassigned[animal]);
            incr.incrementButton.enable(totals[animal] - unassigned[animal] - cooked[animal] > 0);
            incr.decrementButton.enable(unassigned[animal] > 0);

            /*
            if (this._cookValues) {
                var cook = this.cookAnimals[animal];
                cook.setQuantity(cooked[animal]);
                cook.incrementButton.enable(totals[animal] - cooked[animal] > 0);
                cook.decrementButton.enable(cooked[animal] > 0)

                cook.display.find(".foodCount").text(cook._quantity * this._cookValues[animal].outAmount);
            }*/
            this.farmyard.personalSupply.setResource(animal, totals[animal] - unassigned[animal] - cooked[animal]);
        }


        this.farmyardControlMethod();

    },

    farmyardControlMethod: function () {
        console.info("We're in here");
        this.farmyard.enableAnimalManagement();
    }



}
extend(FarmyardPopup, FarmyardWithAnimalsPopup);
