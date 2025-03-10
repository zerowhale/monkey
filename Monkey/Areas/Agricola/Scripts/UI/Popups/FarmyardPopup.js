function FarmyardPopup(display) {
    Popup.call(this, display);

    this._modules = [];
    this.buttons = this.display.find(".build-menu");
    this.containers = this.display.find(".containers");
}


FarmyardPopup.prototype = {
    farmyard: null,         // Farmyard data object
    buttons: null,          // Display for buttons
    containers: null,       // Display for containers
    submitActiveOnly: false,// Submits only the active tab if true
    _modules: null,         // Module data
    _buttons: null,         // list of button objects
    _containers: null,      // list of containers
    _selectedButton: null,  // currently selected button;
    _selectedModule: null,  // currently selected module;

    setFarmyard: function (farmyard) {
        this.farmyard = farmyard;
        farmyard.popup = this;
        var board = this.display.find(".board");
        board.empty();
        board.append(farmyard.display);
    },

    selectModule: function (button) {
        for (var i = 0; i < this._buttons.length; i++) {
            if (button == this._buttons[i]) {
                if (button == this._selectedButton)
                    return;

                if (this._selectedButton) {
                    var sIndex = $.inArray(this._selectedButton, this._buttons);

                    this._buttons[sIndex].removeClass("selected");
                    this._containers[sIndex].hide();
                    this._modules[sIndex].active = false;
                }

                this._modules[i].active = true;
                this._selectedButton = button;
                this._buttons[i].addClass("selected");
                this._containers[i].show();
                this._modules[i].onActivate(this);
                this._selectedModule = this._modules[i];

                this.updateSubmitButtonState();

                return;
            }
        }
    },

    show: function (x) {
        if (x) {
            this.submitActiveOnly = x.submitActiveOnly ? true : false;

            this.setModules(x.modules, x.moduleParams, x.moduleButtonText);
            
            if (x.dialogue)
                this.display.addClass("dialogue");

        }
        Popup.prototype.show.call(this);

        if (this.farmyard)
            this.farmyard.show();
    },


    updateSubmitButtonState: function (params) {
        if (!this.submitActiveOnly) {
            for (var i in this._modules) {
                if (this._modules[i].canSubmit(params)) {
                    this.submitButton.removeClass("disabled");
                    return;
                }
            }
        }
        else {
            if (this._selectedModule.canSubmit(params)) {
                this.submitButton.removeClass("disabled");
                return;
            }
        }

        this.submitButton.addClass("disabled");
    },

    getSubmitData: function(){
        var data = {};

        if (!this.submitActiveOnly) {
            for (var i in this._modules) {
                var m = this._modules[i];
                var sd = m.getSubmitData();
                for (var d in sd)
                    data[d] = sd[d];
            }
        }
        else {
            var sd = this._selectedModule.getSubmitData();
            for (var d in sd)
                data[d] = sd[d];
        }

        return data;
    },

    setTitle: function(title){
        this.display.find("h2").html(title);
    },

    setModules: function (modules, params, buttonTextOverrides) {
        this._reset();

        this._selectedModule = modules[0];
        for (var i in modules) {
            this._setModule(modules[i], params ? params[i] : null, buttonTextOverrides ? buttonTextOverrides[i] : null);
        }

        if (modules.length == 1)
            this._buttons[0].find(".button").addClass("only");

        this.selectModule(this._buttons[0]);
        
    },

    _setModule: function (module, params, buttonText) {
        var obj = this;
        this._modules.push(module);

        var button = module.getButton(buttonText);
        this._buttons.push(button);
        this.buttons.append(button);
        button.click(function () {
            obj.selectModule(button);
        });

        var sharedFarmyard = this.farmyard.display.find(".farmyard");

        var body = module.getBody();
        if (!body) {
            this._containers.push(sharedFarmyard);

            if(sharedFarmyard.hasClass("show-animal-controls"))
                this.containers.addClass("show-animal-controls");
            //module.display = sharedFarmyard;
        }
        else {
            this._containers.push(body);
            this.containers.append(body);
            body.hide();
        }
        sharedFarmyard.hide();

        this.display.removeClass(function (index, css) {
            return css.indexOf("popup") == -1;
        });

        var classes = module.sourceDisplay.attr("class").split(" ");
        for(var i in classes){
            if(classes[i].indexOf("popup") == -1)
                this.display.addClass(classes[i]);
        }

        module.actionId = this.actionId;
        module.farmyard = this.farmyard;
        module.player = this.player;
        module.popup = this;
        module.onLoad(params);
    },

    _reset: function () {

        var classes = this.display.attr("class").split(" ");
        for (var i in classes) {
            if (classes[i].indexOf("popup") == -1)
                this.display.removeClass(classes[i]);
        }

        if (this._containers) {
            for (var i in this._containers)
                this._containers[i].remove();
        }

        if (this._buttons) {
            for (var i in this._buttons)
                this._buttons[i].remove();
        }

        this._selectedButton = null;

        this._modules = [];
        this._containers = [];
        this._buttons = [];

        this.containers.removeClass("show-animal-controls");
        
        this.submitButton.find("div").text("Submit");
    }
}
extend(Popup, FarmyardPopup);








function PopupModule(displayPath) {
    this.sourceDisplay = $(displayPath);
}
PopupModule.prototype = {
    allowNoSelection: false,
    sourceDisplay: null,
    display: null,
    actionId: null,
    farmyard: null,
    player: null,
    popup: null,
    active: false,
    

    onLoad: function (params) {

        if (params && params.allowNoSelection) {
            this.allowNoSelection = true
            this.popup.updateSubmitButtonState();
        }
    },

    onActivate: function (popup) {
    },

    getButton: function (text) {
        var btn = this.sourceDisplay.find(".menu-button").clone();
        if(text)
            btn.find(".button div").html(text);

        return btn;
    },

    getBody: function () {
        var body = this.sourceDisplay.find(".body").clone();
        if (body.length == 0 || body.hasClass("use-farm"))
            return null;
        else {
            this.display = body;
            return body;
        }
    },

    getSubmitData: function(){
        return false;
    },

    canSubmit: function () {
        return this.allowNoSelection;
    }


}



function PlowPopupModule() {
    PopupModule.call(this, ".popup-module.plow");
}

PlowPopupModule.prototype = {
    optional: false,
    numPlowable: 1,
    availablePlows: null,
    usingPlow:null,

    onLoad: function (params) {
        var obj = this;
        if (params) {
            if(params.optional)
                this.optional = params.optional;

            if (params.plowInfo) {
                this.availablePlows = params.plowInfo.plows;

                if (this.availablePlows && this.availablePlows.length > 0) {
                    this.popup.display.addClass("plow-controls");
                    var controls = this.popup.display.find(".plow-controls");
                    
                    controls.find(".plow-row").remove();
                    var rowTemplate = controls.find(".template");
                    for (var index = 0; index < this.availablePlows.length; index++) {
                        let plow = this.availablePlows[index],
                            row = rowTemplate.clone();

                        row.removeClass("template");
                        row.addClass("plow-row");
                        row.find(".name").text(getCdataAsText(plow.name));
                        row.find(".descr").text(plow.Fields);
                        var uses = row.find(".uses");
                        uses.find(".remaining").text(plow.MaxUses - plow.Used);
                        uses.find(".total").text(plow.MaxUses);
                        row.show();

                        row.click(function () {
                            var me = $(this);
                            var check = me.find("input[type=checkbox]");
                            check.prop('checked', !check.prop('checked'));
                            check.change();
                        });

                        var check = row.find("input[type=checkbox]");
                        check.click(function (e) {
                            e.stopPropagation();
                        });

                        check.attr("data-plow-index", index).change(function () {
                            var current = this;
                            var others = controls.find("input[type=checkbox]");
                            others.each(function (index, element) {
                                if (element != current)
                                    element.checked = false;
                            })

                            if (this.checked)
                                obj.onPlowSelection(obj.availablePlows[parseInt($(this).attr("data-plow-index"))]);

                            else
                                obj.onPlowSelection(null)
                        });
                        controls.append(row);
                    }
                }
            }
        }
    },

    onPlowSelection: function (plow) {
        this.usingPlow = plow;
        var numFields = (plow == null ? 1 : plow.Fields);
        this.farmyard.enablePlotsForPlowing(numFields, this.popup);
    },

    onActivate: function (popup) {
        this.farmyard.enablePlotsForPlowing(this.numPlowable, popup);
    },

    canSubmit: function () {
        return this.optional || this.farmyard.addedFields.length > 0;
    },

    getSubmitData: function () {
        return { plowData: this.farmyard.addedFields, plowUsed: this.usingPlow ? this.usingPlow.cardId : null};
    },
}
extend(PopupModule, PlowPopupModule);


function SowPopupModule() {
    PopupModule.call(this, ".popup-module.sow");
}

SowPopupModule.prototype = {
    onActivate: function (popup) {
        this.farmyard.enablePlotsForSowing(popup);
    },

    canSubmit: function () {
        for (var i in this.farmyard.addedSows) {
            if (this.farmyard.addedSows[i]) {
                return true;
            }
        }
        return false;
    },

    getSubmitData: function () {
        var sows = [];
        for (var i in this.farmyard.addedSows) {
            var sow = this.farmyard.addedSows[i];
            if (sow) {
                sows.push(sow);
            }
        }

        return { sowData: sows};
    },
}
extend(PopupModule, SowPopupModule);








function BuildStablesPopupModule() {
    PopupModule.call(this, ".popup-module.stables");
}

BuildStablesPopupModule.prototype = {
    onActivate: function (popup) {
        this.farmyard.enablePlotsForStables(popup, this.actionId);
    },

    canSubmit: function () {
        return this.farmyard.addedStables.length > 0;
    },

    getSubmitData: function () {
        return { stablesData: this.farmyard.addedStables };
    },
}
extend(PopupModule, BuildStablesPopupModule);








function FamilyGrowthPopupModule() {
    PopupModule.call(this, ".popup-module.family-growth");
}

FamilyGrowthPopupModule.prototype = {
    onActivate: function (popup) {
    },

    canSubmit: function () {
        return this.active;
    },

    getSubmitData: function () {
        return { familyGrowth: this.active};
    },
}
extend(PopupModule, FamilyGrowthPopupModule);











function SelectResourcesPopupModule() {
    PopupModule.call(this, ".popup-module.select-resources");

}

SelectResourcesPopupModule.prototype = {
    resourceButtontemplate: null,
    resourceButtons: null,
    numRequired: 1,

    onLoad: function (params) {
        var obj = this;

        this.resourceButtontemplate = this.display.find(".wrapper").remove();
        this.numRequired = params.numRequired;

        console.info(this.resourceButtontemplate);
        for (var r in params.resources) {
            var resource = params.resources[r];
            var button = this.resourceButtontemplate.clone();
            console.info(button);
            button.attr("data-resource", resource);
            button.find("span").text(resource);
            button.find(".icon").addClass(resource.toLowerCase());
            
            button.click(function (e) {
                var btns = obj.resourceButtons.filter(".selected");
                var cur = $(e.currentTarget);
                if (cur.hasClass("selected")) {
                    cur.removeClass("selected");
                }
                else {
                    if (btns.length < obj.numRequired) {
                    }
                    else if (btns.length == obj.numRequired && obj.numRequired == 1) {
                        btns.removeClass("selected");
                    }
                    else {
                        return;
                    }
                    cur.addClass("selected");
                }
                obj.popup.updateSubmitButtonState();
            });

            console.info(this.display);
            this.display.append(button);
        }

        this.resourceButtons = this.display.find(".wrapper");

    },

    onActivate: function () {
        
    },

    canSubmit: function () {
        var selected = this.resourceButtons.filter(".selected");
        
        return selected.length == this.numRequired;
    },

    getSubmitData: function () {
        var btns = this.resourceButtons.filter(".selected");
        var rs = [];
        for (var i = 0; i < btns.length; i++)
            rs.push($(btns[i]).attr("data-resource"));

        return {resources: rs};
            
    },
}
extend(PopupModule, SelectResourcesPopupModule);




function CardPopupModule() {
    PopupModule.call(this, ".popup-module.cards");
}

CardPopupModule.prototype = {

    
    paymentOption: 0,
    selected: null,

    onLoad: function (params) {
        this.setCards(params.cards);
        this.updateEnabledCards();

        PopupModule.prototype.onLoad.call(this, params);
    },

    onActivate: function () {

    },

    canSubmit: function () {
        return !!this.getSelected() || PopupModule.prototype.canSubmit.call(this);
    },

    getSubmitData: function () {
        return {cardId: this.getSelected(), paymentOption: this.getPaymentOption()};
    },


    setCards: function (cards) {
        var container = this.display;
        
        container.empty();
        container.append(cards);
        var obj = this;
        this.selected = null;

        cards.click(function () {
            var selected = cards.filter(".selected");
            var i = $(this);

            obj.display.find(".card").removeClass("selected");
            cards.find(".cost-options").hide();

            if (selected.attr("data-id") == i.attr("data-id")) {
                obj.setSelected(null);
                return;
            }

            i.addClass("selected");
            obj.setSelected(parseInt(i.attr("data-id")));

            if (i.find(".cost-option").length > 1)
                i.find(".cost-options").show();

        });
    },




    setSelected: function (id) {
        this.selected = id;
        if (id == null) {
            this.display.find(".cost-options").hide();
            this.paymentOption = 0;

        }
        else {
            var card = this.display.find(".card[data-id='" + id + "']");
            this.updateSelectedPaymentOption(card);
        }

        this.popup.updateSubmitButtonState();
    },

    getSelected: function () {
        return this.selected;
    },

    getPaymentOption: function(){
        return this.paymentOption;
    },

    updateSelectedPaymentOption: function (card) {
        var index = card.find(".cost-option.selected").index();

        // some hackery here to account for the title element in the cost option screen
        if (index < 0) {
            index = parseInt(card.find(".cost-options").attr("data-default")) + 1;
        }
        this.paymentOption = index < 0 ? 0 : index - 1;

    },

    getAffordableCosts: function (id) {
        
        var obj = this,

            affordableCostOptions = Curator.getAffordableCosts(this.player, id);

      
        return affordableCostOptions;
    },

    updateEnabledCards: function () {
        var container = this.display;
        var obj = this;
        function wireOptions(options, card) {
            return function (event) {
                options.removeClass("selected");
                $(this).addClass("selected");
                event.stopPropagation();
                obj.updateSelectedPaymentOption(card);
            }
        }

        var cards = container.find(".card");

        for (var m = 0; m < cards.length; m++) {
            var card = $(cards[m]),
                cardData = IdLookup[card.attr("data-id")],
                available = Curator.isImprovementAvailable(cardData.Id);

            if (available) {
                var costOptionsDisplay = card.find(".cost-options");


                if (!Curator.meetsCardPrereqs(this.farmyard.player, cardData)) {
                    card.addClass("disabled");
                    continue;
                }

                var costOptions = this.getAffordableCosts(cardData.Id);

                if (costOptions.length == 0) {
                    card.addClass("disabled");
                }
                else if (costOptions.length > 1) {

                    var costTemplate = costOptionsDisplay.find(".cost-option").remove();

                    for (var c = 0; c < costOptions.length; c++) {
                        var costDisplay = costTemplate.clone().show();
                        costDisplay.attr("data-payment-id", c);
                        var costOption = costOptions[c];

                        if (c == 0)
                            costDisplay.addClass("selected");

                        switch (costOption.Type) {
                            case CardCostType.ReturnCard:
                                costDisplay.text(costOption.Text);
                                break;

                            case CardCostType.Resources:

                                var firstResource = true;
                                for (var cost in costOption.Resources) {
                                    cost = costOption.Resources[cost];
                                    var d = $("<span>" + cost.Count + "<div class='icon medium " + cost.Type.toLowerCase() + "'></span>" + (firstResource ? "" : "&nbsp;"))
                                    costDisplay.append(d);
                                    firstResource = false;
                                }
                                break;
                        }
                        costOptionsDisplay.append(costDisplay);
                    }

                    // Empty handler to stop click through
                    costOptionsDisplay.click(function (e) {
                        e.stopPropagation();
                    })

                    var options = costOptionsDisplay.find(".cost-option");
                    options.click(wireOptions(options, card));
                }
                else {
                    costOptionsDisplay.attr("data-default", costOptions[0].costIndex)
                }

            }
            else {
                card.addClass("disabled");
            }
        }

    }
}
extend(PopupModule, CardPopupModule);


function OccupationPopupModule() {
    CardPopupModule.call(this);
}

OccupationPopupModule.prototype = {

    onLoad: function (params) {
        this.setCards(params.cards);
        this.updateEnabledCards();

        PopupModule.prototype.onLoad.call(this, params);
    },

    getAffordableCosts: function (id) {
        var obj = this,
            affordableCostOptions = [],
            occupationFee = Curator.getOccupationCost(this.player, this.actionId);

        // This will need to be changed to get teh card cost from the Curator
        // once minor improvements/occupations go in.
        var item = IdLookup[id];
        var costs = item.Costs;

        var occupationAdditionalCost = {
            Resources: [{
                Type: Resource.Food,
                Count: 0
            }],
            Type: CardCostType.Resources,
            costIndex: 0
        };

        if (costs.length == 0) {
            if (checkCosts(occupationAdditionalCost)) {
                return [occupationAdditionalCost];
            }
        }
        for (var c in costs) {
            if (checkCosts(costs[c])) {
                costs[c]["costIndex"] = c;
                affordableCostOptions.push(costs[c]);
            }
        }


        function checkCosts(cost) {
            var type = cost.Type;

            var foodFound = false;
            switch (type) {
                case CardCostType.Resources:
                    for (var c in cost.Resources) {
                        var cache = cost.Resources[c];
                        if (cache.Type == Resource.Food
                            && obj.farmyard.personalSupply[cache.Type.toLowerCase()] < cache.Count + occupationFee) {
                            foodFound = true;
                            return false;
                        }
                        else if (obj.farmyard.personalSupply[cache.Type.toLowerCase()] < cache.Count)
                            return false;
                    }

                    break;
            }
            if (!foodFound) {
                if (obj.farmyard.personalSupply[Resource.Food.toLowerCase()] < occupationFee)
                    return false;
            }
            return true;
        }

        return affordableCostOptions;

    },
}

extend(CardPopupModule, OccupationPopupModule);





function ManageAnimalsPopupModule(display) {
    PopupModule.call(this, display ? display : ".popup-module.manage-animals");
}

ManageAnimalsPopupModule.prototype = {
    unassignedAnimals: null,
    cookAnimals: null,
    _cookValues: null,

    onLoad: function (params) {
        PopupModule.prototype.onLoad.call(this, params);
        var obj = this;
        this.popup["animalStateChanged"] = function () {
            obj.animalStateChanged();
        }
    },

    onActivate: function () {
        var animalControls = this.popup.display.find(".animal-controls");

        this.unassignedAnimals = {
            Sheep: new Incrementor(animalControls.find(".unassigned-animals .incrementor.sheep"), 1),
            Boar: new Incrementor(animalControls.find(".unassigned-animals .incrementor.boar"), 1),
            Cattle: new Incrementor(animalControls.find(".unassigned-animals .incrementor.cattle"), 1)
        };

        this.cookAnimals = {
            Sheep: new Incrementor(animalControls.find(".cook-animals .incrementor.sheep"), 1),
            Boar: new Incrementor(animalControls.find(".cook-animals .incrementor.boar"), 1),
            Cattle: new Incrementor(animalControls.find(".cook-animals .incrementor.cattle"), 1)
        };


        var obj = this;
        FarmyardPopup.prototype.show.call(this);
        var manager = this.farmyard.animalManager;

        this._cookValues = {
            Sheep: 0,
            Boar: 0,
            Cattle: 0
        };
        var canCook = Curator.canCook(this.player);
        if (canCook) {
            this.popup.display.find(".cook-animals").show();

            var x = Curator.getAvailableResourceConversions(this.player);
            for(var i in x){
                var data = x[i];
                if (data.outType == Resource.Food && !data.inLimit) {
                    console.info("data", data, Resource.Food);
                    if (data.inType == Resource.Sheep)
                        this._cookValues[Resource.Sheep] = data;
                    else if (data.inType == Resource.Boar)
                        this._cookValues[Resource.Boar] = data;
                    else if (data.inType == Resource.Cattle)
                        this._cookValues[Resource.Cattle] = data;
                }
            }
            console.info("CooKValues", this._cookValues);
        }
        else
            this.popup.display.find(".cook-animals").hide();


        wireIncrementors();


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

            var cook = this.cookAnimals[animal];
            if (this._cookValues[animal]) {
                cook.incrementButton.enable(totals[animal] - cooked[animal] > 0);
                cook.decrementButton.enable(cooked[animal] > 0)
                cook.setQuantity(cooked[animal]);
                cook.display.find(".foodCount").text(cook._quantity * this._cookValues[animal].outAmount);
            }

            this.farmyard.personalSupply.setResource(animal, totals[animal] - unassigned[animal] - cooked[animal]);
        }


        this.farmyardControlMethod();

    },

    canSubmit: function () {
        return true;
    },

    getSubmitData: function () {
        return {
            assignments: this.farmyard.animalManager.getAssignments(),
            unassigned: this.farmyard.animalManager.unassigned,
            cooked: this.farmyard.animalManager.cooked
        };
    },





    farmyardControlMethod: function () {
        this.farmyard.enableAnimalManagement();
    }
}

extend(PopupModule, ManageAnimalsPopupModule);





function BuildRoomsPopupModule() {
    PopupModule.call(this, ".popup-module.build-rooms");
}

BuildRoomsPopupModule.prototype = {
    onActivate: function (popup) {
        this.farmyard.enablePlotsForRooms(popup, this.actionId);
    },

    canSubmit: function () {
        return this.farmyard.addedRooms.length > 0;
    },

    getSubmitData: function () {
        return { roomsData: this.farmyard.addedRooms };
    }
}

extend(PopupModule, BuildRoomsPopupModule);



function TravellingPlayersPopupModule() {
    PopupModule.call(this, ".popup-module.travelling-players");
}

TravellingPlayersPopupModule.prototype = {
    foodCount: 0,
    foodContainer: null,

    onLoad: function (params) {
        PopupModule.prototype.onLoad.call(this, params);
        this.foodCount = params.foodCount;



    },

    onActivate: function (popup) {
        var obj = this;

        this.foodContainer = {
            icons: obj.display.find(".icons"),
            count: obj.display.find(".text .count")

        }
        this.buildFoodDisplay();
    },

    canSubmit: function () {
        return true;
    },

    getSubmitData: function () {
        return { takeFood: true};
    },


    buildFoodDisplay: function () {
        this.foodContainer.icons.empty();
        for (var i = 0; i < this.foodCount; i++) {
            this.foodContainer.icons.append($("<div class='icon full food'></div>"));
        }

        this.foodContainer.count.text("x" + this.foodCount);
    },
}

extend(PopupModule, TravellingPlayersPopupModule);




function BuildFencesPopupModule() {
    ManageAnimalsPopupModule.call(this, ".popup-module.build-fences");
}

BuildFencesPopupModule.prototype = {
    onActivate: function (popup) {
        ManageAnimalsPopupModule.prototype.onActivate.call(this, popup);
        this.farmyard.popup = popup;
        this.farmyard.enablePlotsForFencing(popup, this.actionId);

    },

    canSubmit: function (params) {
        var fencesAdded = false;
        for (var i in this.farmyard.addedFences) {
            if (this.farmyard.addedFences[i] != null)
                fencesAdded = true;
        }

        return ManageAnimalsPopupModule.prototype.canSubmit.call(this)
            && (fencesAdded && params.fencesValid);
    },

    getSubmitData: function () {
        var data = ManageAnimalsPopupModule.prototype.getSubmitData.call(this);
        data["fencesData"] = this.farmyard.addedFences;
        return data;
    },

    farmyardControlMethod: function () {
        ManageAnimalsPopupModule.prototype.farmyardControlMethod.call(this);
        this.farmyard.enablePlotsForFencing(this.popup, this.actionId);
    }
}

extend(ManageAnimalsPopupModule, BuildFencesPopupModule);






function FencePasturePopupModule() {
    ManageAnimalsPopupModule.call(this, ".popup-module.fence-pasture");
}

FencePasturePopupModule.prototype = {
    onActivate: function (popup) {
        ManageAnimalsPopupModule.prototype.onActivate.call(this, popup);
        this.farmyard.popup = popup;
        this.farmyard.enablePlotForFencing(popup);

    },

    canSubmit: function (params) {
        var fencesAdded = false;
        for (var i in this.farmyard.addedFences) {
            if (this.farmyard.addedFences[i] != null)
                fencesAdded = true;
        }


        return ManageAnimalsPopupModule.prototype.canSubmit.call(this)
            && (fencesAdded && params.fencesValid);
    },

    getSubmitData: function () {
        var data = ManageAnimalsPopupModule.prototype.getSubmitData.call(this);
        data["fencesData"] = this.farmyard.addedFences;
        return data;
    },

    farmyardControlMethod: function () {
        ManageAnimalsPopupModule.prototype.farmyardControlMethod.call(this);
        this.farmyard.enablePlotsForFencing(this.popup, this.actionId);
    }
}

extend(ManageAnimalsPopupModule, FencePasturePopupModule);




/**
    Resource Conversion popup is used for all screens that convert any amount of one resource
    into any amount of one other resource.
*/
function ResourceConversionPopupModule(replacementDisplay) {
    PopupModule.call(this, replacementDisplay ? replacementDisplay : ".popup-module.cook");

}

ResourceConversionPopupModule.prototype = {
    template: null,
    rows: null,


    onLoad: function (params, conversionData) {
        var obj = this;



        if (!this.template)
            this.template = this.display.find(".resource-conversion").remove();


        var external = true;




        function addRow(row) {
            row.quantityChanged(updatePersonalSupply);

            row.bind("decremented", function (r) {
                return function () {
                    var ps = obj.farmyard.personalSupply,
                        rc = r.resourceConversion,
                        inType = rc.inType.toLowerCase(),
                        outType = rc.outType.toLowerCase();
                    ps.setResource(inType, ps[inType] + rc.inAmount);
                    ps.setResource(outType, ps[outType] - rc.outAmount);
                }
            }(row));

            row.bind("incremented", function (r) {
                return function () {
                    var ps = obj.farmyard.personalSupply,
                        rc = r.resourceConversion,
                        inType = rc.inType.toLowerCase(),
                        outType = rc.outType.toLowerCase();
                    ps.setResource(inType, ps[inType] - rc.inAmount);
                    ps.setResource(outType, ps[outType] + rc.outAmount);
                }
            }(row));

            if (rowNum % 2 == 0) row.display.addClass("alt-row");
            rowNum++;

            container.append(row.display);
            obj.rows.push(row);
        }

        function updatePersonalSupply() {
            obj.popup.updateSubmitButtonState();
            
        }

        var container = this.display.find(".resource-conversions");
        var rowNum = 0;
        obj.rows = [];
        container.empty();

        var rowAdded = false;
        for (var entry in conversionData) {
            var data = conversionData[entry];

            var available = obj.farmyard.personalSupply[data.inType.toLowerCase()];

            if (data.outAmount !== 0
                && data.outType !== data.inType
                && available >= data.inAmount) {

                rowAdded = true;
                var rowDisplay = this.template.clone();
                var row = new ResourceConversionRow(rowDisplay, data, obj.farmyard.personalSupply);



                addRow(row);
            }
        }

        if (!rowAdded) {
            this.display.find(".listing").hide();
            this.display.find(".empty-message").show();
        }
        else {
            this.display.find(".listing").show();
            this.display.find(".empty-message").hide();
        }


        PopupModule.prototype.onLoad.call(this, params);

    },

    canSubmit: function () {
        if (this.rows != null) {
            for (var r in this.rows) {
                var row = this.rows[r];
                if (row.count > 0)
                    return true;
            }
        }

        return PopupModule.prototype.canSubmit.call(this);
    },

    getSubmitData: function () {
        var data = [];

        if (this.rows != null) {
            for (var r in this.rows) {
                var row = this.rows[r];
                if (row.count > 0) {
                    var rc = row.resourceConversion;
                    data.push(rc.toResourceConversionData(row.count));

                }
            }
        }

        return { conversionData: data.length == 0 ? null : data };
    },
}
extend(PopupModule, ResourceConversionPopupModule);





function BakePopupModule() {
    ResourceConversionPopupModule.call(this);

}

BakePopupModule.prototype = {
    onLoad: function (params) {
        var options = Curator.getBakeOptions(this.player);
        ResourceConversionPopupModule.prototype.onLoad.call(this, params, options);
    },
}
extend(ResourceConversionPopupModule, BakePopupModule);




function CookPopupModule() {
    ResourceConversionPopupModule.call(this);
}

CookPopupModule.prototype = {

    onLoad: function (params) {
        var options = Curator.getAnytimeConversions(this.player);
        ResourceConversionPopupModule.prototype.onLoad.call(this, params, options);
    }

}
extend(ResourceConversionPopupModule, CookPopupModule);









function PrereqNotMetPopupModule() {
    PopupModule.call(this, ".popup-module.prereq-not-met");

}

PrereqNotMetPopupModule.prototype = {
    onLoad: function(params){
        var card = params.card;
        var cardDisplay = Game.templates.improvementCard.clone();
        this.display.find(".card").replaceWith(cardDisplay);
        cardDisplay.addClass(card.Type.toLowerCase());


        Game.populateCardData(cardDisplay, card);


    },

    canSubmit: function () {
        return true;
    },

    getSubmitData: function () {
        return null;
    }
}

extend(PopupModule, PrereqNotMetPopupModule);






function CacheExchangePopupModule() {
    ResourceConversionPopupModule.call(this, ".popup-module.cache-exchange");
}

CacheExchangePopupModule.prototype = {

    onLoad: function (params) {
        var options = params.exchanges;
        for (var o in options) {
            options[o] = ResourceConversion.fromServer(options[o]);
        }
        ResourceConversionPopupModule.prototype.onLoad.call(this, params, options);
    },

    canSubmit: function () {
        return true;
    }

}
extend(ResourceConversionPopupModule, CacheExchangePopupModule);









function PlayerChoicePopupModule() {
    PopupModule.call(this, ".popup-module.player-choice");

}

PlayerChoicePopupModule.prototype = {
    submodulesTemplates: null,
    selected: null,

    onLoad: function (params) {
        var obj = this;
        
        var optionCount = params.events.length;

        this.display.addClass("options" + optionCount);

        for (var e = 0; e < optionCount; e++) {
            var option = params.events[e];
            var event = option.Event;
            var type = event.Type;

            var template = this.loadOptionTemplate(event.Type);
            if (template.length == 0)
                console.error("No template found for event type:", event.Type);

            if (e == optionCount - 1)
                template.addClass("last");

            template.find("h2").text(e + 1);
            template.attr("data-option", option.Id);
            switch (type) {
                case "GainResourcesEvent":
                    this.addGainResourcesOption(event, template);
                    break;
            }


            this.display.append(template);
            template.click(function () {
                obj.select($(this));
            });

        }

    },

    select: function(option){
        if (this.selected) {
            this.selected.removeClass("selected");
        }
        this.selected = option;
        this.selected.addClass("selected");

        this.popup.updateSubmitButtonState();
    },

    addGainResourcesOption: function (event, template) {
        var resources = event.Resources;
        if(resources.length == 1)
            template.find(".multiple").hide();
        
        var resourceDisplay = template.find(".resource").remove();

        for(var r in resources){
            var resource = resources[r].Resource;
            var rd = resourceDisplay.clone();
            
            rd.find(".count").text(resource.Count + "x");
            rd.find(".icon").addClass(resource.Type.toLowerCase());

            template.append(rd);
        }

    },

    loadOptionTemplate: function (templateName) {
        if (!this.submodulesTemplates)
            this.submodulesTemplates = $("#game").find(".player-choice-submodules");

        var formattedName = this.formatNameForHtml(templateName)
        return this.submodulesTemplates.find("." + formattedName).clone();
    },

    formatNameForHtml: function (name) {
        var index = name.indexOf("Event");
        if (index > 0) {
            name = name.substring(0, index);
        }
        var newName = "";
        for(var c=0;c<name.length;c++){
            if(c == 0)
                newName = name.charAt(c).toLowerCase();
            else{ 
                char = name.charAt(c);
                newName += /[A-Z]/.test(char) ? "-" + char.toLowerCase() : char;
            }
        }
        return newName;
    },

    canSubmit: function () {
        return this.selected;
    },

    getSubmitData: function () {
        return { option: this.selected.attr("data-option") };
    }
}

extend(PopupModule, PlayerChoicePopupModule);
