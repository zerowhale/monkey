function AnimalManager(farmyardGrid, pastures, assignments){
    this.unassigned = {
        Sheep: 0,
        Boar: 0,
        Cattle: 0
    };

    this.cooked = {
        Sheep: 0,
        Boar: 0,
        Cattle: 0
    }

    this.configureHousings(farmyardGrid, pastures);

    if (assignments) {
        for (var assignment in assignments) {
            assignment = assignments[assignment];

            // server-side assignments
            if (assignment.Id) {
                var housing = this.housings[assignment.Id];
                housing.animalType = assignment.AnimalType;
                housing.animalCount = assignment.AnimalCount;
            }
            // client-side assignments
            else {
                console.error("Unknown animal assignment", assignment)
                /*var housing = this.housings[assignment.id];
                housing.animalType = assignment.type;
                housing.animalCount = assignment.count;*/
            }
        }
    }

}

AnimalManager.prototype = {
    housings: null,
    unassigned: null,
    cooked: null,

    getAnimalCounts: function () {
        var counts = { Sheep: 0, Boar: 0, Cattle: 0 };
        for (var h in this.housings) {
            var housing = this.housings[h];
            counts[housing.animalType] += housing.animalCount;
        }

        counts.Sheep += this.unassigned.Sheep + this.cooked.Sheep;
        counts.Boar += this.unassigned.Boar + this.cooked.Boar;
        counts.Cattle += this.unassigned.Cattle + this.cooked.Cattle;
        return counts;
    },

    emptyHousing: function(id){
        var housing;
        if (housing = this.housings[id]) {
            this.unassigned[housing.animalType] += housing.animalCount;
            housing.animalCount = 0;
        }
    },

    tryRemove: function (type) {
        this.tryUnassignAnimal(type);

        if(this.unassigned[type] > 0){
            this.unassigned[type]--;
            return true;
        }
        else if (this.cooked[type] > 0) {
            this.cooked[type]--;
            return true;
        }
        return false;

    },

    tryCook: function (type) {
        if (!AnimalResource[type]) return false;
        if (this.unassigned[type] > 0) {
            this.unassigned[type]--;
            this.cooked[type]++;
        }
        else {
            if (this.tryUnassignAnimal(type))
                return this.tryCook(type);
            else
                return false;
        }
        return true;
    },

    tryUncook: function(type){
        if(this.cooked[type] > 0){
            this.cooked[type]--;
            this.trySmartAssignAnimals(type, 1);
            return true;
        }

        return false;
    },

    configureHousings: function(farmyardGrid, pastures){
        this.housings = {};

        // Default house pet
        var id = "house";
        this.housings[id] = new AnimalHousing(id, 1);

        var stablesCounted = [];
        var stables = [];
        for (var p in pastures) {
            var pasture = pastures[p],
                compiledId = "",
                count = 0,
                stableCount = 0;
            for (var pid in pasture) {
                pid = pasture[pid];
                if (compiledId != "") compiledId += "_";
                compiledId += pid

                var plot = farmyardGrid[pid];

                count++;

                if (plot.HasStable) {
                    stableCount++;
                }

                stablesCounted[pid] = 1;
            }

            var capacity = (count * 2) << stableCount;
            var id = "pasture" + compiledId;
            this.housings[id] = (new AnimalHousing(id, capacity));
        }


        for (var x = 0; x < PlayerBoard.prototype.PLOT_GRID_WIDTH; x++) {
            for (var y = 0; y < PlayerBoard.prototype.PLOT_GRID_HEIGHT; y++) {
                var plot = farmyardGrid[y * PlayerBoard.prototype.PLOT_GRID_WIDTH + x];
                if (plot.Type != "Pasture" && plot.HasStable) {
                    stables.push({ x: x, y: y });
                }
            }
        }

        for (var i in stables) {
            var stable = stables[i];
            var index = stable.y * PlayerBoard.prototype.PLOT_GRID_WIDTH + stable.x;
            if (!stablesCounted[index]) {
                id = "stable" + index;
                this.housings[id] = (new AnimalHousing(id, 1));
            }
        }
    },

    reconfigure: function (farmyardGrid, pastures) {
        var oldHousings = this.housings;
        this.configureHousings(farmyardGrid, pastures);

        for (var h in oldHousings) {
            var oldHousing = oldHousings[h];
            var newHousing = null;

            /*console.info("Transfering housing id", oldHousing.id);
            if (newHousing = this.housings[oldHousing.id]) {
                newHousing.setAnimals(oldHousing.animalType, oldHousing.animalCount)
            }
            else {*/
                if(oldHousing.animalCount > 0)
                    this.unassigned[oldHousing.animalType] += oldHousing.animalCount;
            //}
        }
        
        return this.tryAutoAssignUnassignedAnimals();

    },

    

    getIndicesFromId: function (id) {
        var index = -1;
        if (id.indexOf("house") >= 0)
            return [10];    // bottom room of house for now
        else if ((index = id.indexOf("stable")) >= 0) {
            return [parseInt(id.substr(index + "stable".length, id.length - index))];
        }
        else if ((index = id.indexOf("pasture")) >= 0) {
            index = index + "pasture".length-1;
            var indices = [];
            do {
                var s = index+1;
                index = id.indexOf("_", s);
                var end = index == -1 ? id.length : index;
                indices.push(parseInt(id.substring(s, end)));

            }
            while (index >= 0)
            return indices;
        }

    },

    getHighestUnassigned: function(priority){
        var highest = {type: AnimalResource.Sheep, count:0};

        if (priority && this.unassigned[priority] > 0) {
            highest.type = priority;
            highest.count = this.unassigned[priority];
            return highest;
        }

        for(var type in this.unassigned){
            var count = this.unassigned[type];
            if(count > highest.count){
                highest.type = type;
                highest.count = count;
            }
        }
        return highest;
    },

    tryUnassignAnimal: function (type) {
        var smallest = null;
        for (var h in this.housings) {
            var housing = this.housings[h];
            if (housing.animalCount > 0 && housing.animalType == type) {
                if (smallest == null || smallest.animalCount > housing.animalCount) {
                    smallest = housing;
                }
            }
        }

        if (smallest != null) {
            smallest.animalCount--;
            this.unassigned[type]++;
            return true;
        }
        return false;
    },

    /**
        Attempts to assign one animal of the specified type from 
        the pool of unassigned animals
     */
    tryAssignAnimal: function(type){
        if (this.unassigned[type] > 0) {
            this.unassigned[type]--;
            return this.trySmartAssignAnimals(type, 1);
        }
        return false;
    },

    tryAutoAssignUnassignedAnimals: function (priority) {
        var newHomesFound = true;
        var highest = null;
        var cutoff = 30;
        while (newHomesFound && cutoff-- > 0
            && ( highest = this.getHighestUnassigned(priority) ).count > 0) {

            this.unassigned[highest.type] = 0;
            newHomesFound = this.trySmartAssignAnimals(highest.type, highest.count);
        }

        return newHomesFound;

    },

    tryAssignNewAnimals: function (animalType, count) {
        var foundRoom = true;
        while (foundRoom && count > 0) {
            foundRoom = this.trySmartAssignAnimals(animalType, count);
            if (foundRoom) {
                count = this.unassigned[animalType];
                this.unassigned[animalType] = 0;
            }
        }
        
        return this.unassigned[AnimalResource.Sheep] == 0
            && this.unassigned[AnimalResource.Boar] == 0
            && this.unassigned[AnimalResource.Cattle] == 0;
    },



    trySmartAssignAnimals: function(animalType, count){
        if (count <= 0) return true;
        var obj = this;

        this.unassigned[animalType] += count;

        var highestEmpty = null;
        var highestPartial = null;
        var remaining;
        // First check if a housing with this animal type has space
        for (var housing in this.housings) {
            housing = this.housings[housing];
            var capacity = housing.remainingCapacity();

            if (housing.isEmpty()
                && (highestEmpty == null ||
                (highestEmpty.capacity < count
                && capacity > highestEmpty.capacity))){
                highestEmpty = housing
            }

            if (!housing.isEmpty() && housing.animalType == animalType) {

                if (capacity > 0) {
                    if (highestPartial == null || 
                        highestPartial.capacity < capacity) {
                        highestPartial = housing;
                    }
                }
            }
        }
        
        if (highestPartial != null) {
            housing = this.housings[highestPartial.id];
            remaining = housing.addAnimals(animalType, count);
            this.unassigned[animalType] -= remaining;
            return true;
        }

        // If it couldn't be distributed among all the partially filled housings,
        // try to fit it into a single housing
        if (highestEmpty != null) {
            remaining = highestEmpty.addAnimals(animalType, count);
            this.unassigned[animalType] -= remaining;
            return true;
        }

        return false;

    },
   

    getAssignments: function () {
        var assignments = [];
        for (var housing in this.housings) {
            housing = this.housings[housing];
            if (housing.animalCount > 0) {
                assignments.push({
                    id: housing.id,
                    count: housing.animalCount,
                    type: housing.animalType
                });
            }
        }
        return assignments;
    },

}

function AnimalHousing(id, capacity) {
    this.id = id;
    this.capacity = capacity;
    this.animalType = "Sheep";
}
AnimalHousing.prototype = {
    id: null,
    capacity: 0,
    animalType: null,
    animalCount: 0,

    setAnimals: function (animal, count) {
        if(count > this.capacity)
            console.error("Can't add more animals than this housing fits", this.id)
        else {
            this.animalType = animal;
            this.animalCount = count;
        }
    },

    addAnimals: function (animal, count) {
        if (this.animalCount > 0 && animal != this.animalType)
            console.error("Can't mix animals types in one housing", this.id);
        else {
            this.animalType = animal;
            
            var cp = this.remainingCapacity();
            if (count > cp) {
                count = cp;
                this.animalCount += cp;
            }
            else {
                this.animalCount += count;
            }
            return count;
        }

    },

    remainingCapacity: function(){
        return this.capacity - this.animalCount;
    },

    isEmpty: function(){
        return this.animalCount == 0;
    }
}

var AnimalType = {
    Sheep: "Sheep",
    Boar: "Boar",
    Cattle: "Cattle"
}