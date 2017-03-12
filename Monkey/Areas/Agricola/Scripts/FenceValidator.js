function FenceValidator(playerBoard) {
    this.fences = [];
    this.runs = [];
    this.grid = [];
    this.outside = [];
    this.playerBoard = playerBoard;
    this.invalidReasons = [];

    for (var i in playerBoard.fences) {
        if (playerBoard.fences[i])
            this.fences[i] = playerBoard.fences[i];
    }

    for (var i in playerBoard.addedFences) {
        if (playerBoard.addedFences[i])
            this.fences[i] = playerBoard.addedFences[i];
    }

}

FenceValidator.prototype = {
    fences: null,
    runs: null,
    grid: null,
    outside: null,
    valid: false,
    invalidReasons: null,
    playerBoard: null,

    validate: function () {
        this.valid = true;
        for (var i in this.fences) {
            i = parseInt(i);
            if (this.fences[i]) {

                // If the first fence index found is vertical the yard can not be valid.
                var y = Math.floor(i / PlayerBoard.prototype.FENCE_GRID_WIDTH);
                if (y % 2 != 0) {
                    this.invalidReasons.push("Fences must only border fully enclosed pastures.");
                    this.valid = false;
                    continue;
                }

                this.runs.push(new this.Run(new this.RunSegment(i, this.Direction.RIGHT)));
                break;
            }
        }

        for (var r = 0; r < this.runs.length; r++) {
            var run = this.runs[r];
            if (run) {
                var head = run.head;
                if (this.segmentClaimed(head, run)) {
                    this.runs.splice(r, 1);
                    r--;
                    continue;
                }
                if (!this.executeRun(r)) {
                    this.invalidReasons.push("Fences must only border fully enclosed pastures.");
                    this.valid = false;
                }
                
            }
        }


        for (var i in this.fences) {
            i = parseInt(i);
            if (this.fences[i]) {
                if (!this.segmentClaimed(i)) {
                    this.valid = false;
                    this.invalidReasons.push("Fences must only border fully enclosed pastures.");
                }
            }
        }

        return {
            pastures: this.getPastures(),
            valid: this.valid
        };
    },


    executeRun: function (runIndex) {
        var run = this.runs[runIndex];
        var segment = run.head;
        var newRuns = [];
        while (true) {
            var neighbors = segment.getNeighbors();
            for (var n = neighbors.length - 1; n >= 0; n--) {
                if (this.fences[neighbors[n].index] == undefined)
                    neighbors.splice(n, 1);
            }

            // if no neighbors the fences can't be valid
            if (neighbors.length == 0) {
                return false;
            }

            var matchingSegment;
            if (matchingSegment = run.matchingSegment(neighbors[0])) {
                if (matchingSegment.direction != neighbors[0].direction)
                    return false;

                var runs = [];
                for (var i = 0; i <= runIndex; i++)
                    runs[i] = this.runs[i];

                for (var i in newRuns)
                    runs.push(newRuns[i]);

                for (var i = runIndex + 1; i < this.runs.length; i++) {
                    runs.push(this.runs[i]);
                }
                this.runs = runs;

                return true;
            }

            segment = neighbors[0];
            run.addSegment(neighbors[0]);

            if (neighbors[1] && !this.segmentClaimed(neighbors[1]))
                newRuns.push(new this.Run(neighbors[1]));

            if (neighbors[2] && !this.segmentClaimed(neighbors[2]))
                newRuns.push(new this.Run(neighbors[2]));
        }
    },

    segmentClaimed: function (segment, owningRun) {

        for (var run in this.runs) {
            run = this.runs[run];
            if (run != owningRun && run.containsSegment(segment))
                return true;
        }
        return false;
    },

    getPastures: function () {
        var PlayerBoard = window.PlayerBoard.prototype,
            pastures = [];

        for (var i = 0; i < PlayerBoard.PLOT_GRID_WIDTH * PlayerBoard.PLOT_GRID_HEIGHT; i++)
            this.grid[i] = 0;


        var highestIndex = 0;
        for (var i = 0; i < this.grid.length; i++) {
            if (this.grid[i] == 0) {
                this.floodFill(i, ++highestIndex);
            }
        }

        for (var i = 0; i < this.grid.length; i++) {
            if ($.inArray(this.grid[i], pastures) == -1 && this.outside[this.grid[i]] == undefined)
                pastures.push(this.grid[i]);
        }

        for (var p in pastures) {
            pasture = pastures[p];
            var pastureIndices = [];
            for (var i = 0; i < this.grid.length; i++) {
                if (this.grid[i] == pasture) {
                    pastureIndices.push(i);
                }
            }

            var validPasture = true;
            for (var i in pastureIndices) {
                i = pastureIndices[i];
                var w = PlayerBoard.PLOT_GRID_WIDTH,
                    x = i % w,
                    y = Math.floor(i / w);
                
                if (this.playerBoard.grid[x][y].Type != "Empty"
                    && this.playerBoard.grid[x][y].Type != "Pasture"
                    && this.playerBoard.grid[x][y].Type != "Stable") {
                    this.invalidReasons.push("Fences may not surround rooms or fields.");
                    this.valid = false;
                    validPasture = false;
                }
                    
            }

            if(validPasture)
                pastures[p] = pastureIndices;
        }

        return pastures;
    },

    floodFill: function (index, value) {
        this.grid[index] = value;

        var neighbors = this.getUnfencedNeighbors(index, value);

        for (var n in neighbors) {
            if (this.grid[neighbors[n]] == 0)
                this.floodFill(neighbors[n], value);
        }
    },

    getUnfencedNeighbors: function (index, value) {
        var PlayerBoard = window.PlayerBoard.prototype,
            width = PlayerBoard.PLOT_GRID_WIDTH,
            Direction = this.Direction,
            neighbors = [],
            x = index % width,
            y = Math.floor(index / width);

        if (!this.isFenced(index, Direction.LEFT)) {
            if (x != 0)
                neighbors.push(index - 1);
            else
                this.outside[value] = 1;
        }

        if (!this.isFenced(index, Direction.RIGHT)) {
            if (x != width - 1)
                neighbors.push(index + 1);
            else
                this.outside[value] = 1;
        }

        if (!this.isFenced(index, Direction.UP)) {
            if (y != 0)
                neighbors.push(index - width);
            else
                this.outside[value] = 1;
        }

        if (!this.isFenced(index, Direction.DOWN)) {
            if (y != PlayerBoard.PLOT_GRID_HEIGHT - 1)
                neighbors.push(index + width);
            else
                this.outside[value] = 1;
        }

        return neighbors;
    },

    isFenced: function (plot, direction) {

        var fIndex = this.getFenceIndex(plot, direction);

        if (this.fences[fIndex]) {
            return true;
        }

        return false;
    },

    getFenceIndex: function (plot, direction) {
        var Direction = this.Direction,
            PlayerBoard = window.PlayerBoard.prototype,
            x = plot % PlayerBoard.PLOT_GRID_WIDTH,
            y = Math.floor(plot / PlayerBoard.PLOT_GRID_WIDTH);


        switch (direction) {
            case Direction.UP:
                return plot + (y * (PlayerBoard.FENCE_GRID_WIDTH + 1));
                break;
            case Direction.DOWN:
                return ((y * 2 + 2) * PlayerBoard.FENCE_GRID_WIDTH) + x;
                break;
            case Direction.LEFT:
                return (y * 2 + 1) * PlayerBoard.FENCE_GRID_WIDTH + x;
                break;
            case Direction.RIGHT:
                return (y * 2 + 1) * PlayerBoard.FENCE_GRID_WIDTH + x + 1;
                break;
        }
        return -1;
    },

    Run: function (head) {
        this.head = head;
        this.segments = [head];
    },

    RunSegment: function (index, direction) {
        this.direction = direction;

        this.index = index;
        this.x = index % PlayerBoard.prototype.FENCE_GRID_WIDTH;
        this.y = Math.floor(index / PlayerBoard.prototype.FENCE_GRID_WIDTH);
    },

    Direction: {
        UP: 0,
        RIGHT: 1,
        DOWN: 2,
        LEFT: 3,
        s: function (direction) {
            return direction == this.UP ? "UP" : direction == this.RIGHT ? "RIGHT" : direction == this.DOWN ? "DOWN" : direction == this.LEFT ? "LEFT" : "Invalid direction";
        }
    }
}

FenceValidator.prototype.Run.prototype = {
    head: null,
    segments: null,

    addSegment: function (segment) {
        this.segments.push(segment);
    },

    containsSegment: function (segment) {

        var index = segment.index != undefined ? segment.index : segment;
        for (var s in this.segments) {
            s = this.segments[s];
            if (s.index == index)
                return true;
        }
        return false;
    },

    matchingSegment: function (segment) {
        var index = segment.index != undefined ? segment.index : segment;
        for (var s in this.segments) {
            s = this.segments[s];
            if (s.index == index)
                return s;
        }
        return null;
    }
}

FenceValidator.prototype.RunSegment.prototype = {
    index: -1,
    direction: null,
    x: -1,
    y: -1,

    /**
        Neighbors will be ordered right to left.
        First neighbor is part of the run,
        additional neighbors are starting nodes of new runs.
     */
    getNeighbors: function () {

        var Direction = FenceValidator.prototype.Direction,
            PlayerBoard = window.PlayerBoard.prototype,
            RunSegment = FenceValidator.prototype.RunSegment,
            x = this.x,
            y = this.y,
            index = this.index,
            neighbors = [];

        if (y % 2 == 0) {
            if (this.direction == Direction.RIGHT) {
                if (y < PlayerBoard.FENCE_GRID_HEIGHT - 1) neighbors.push(new RunSegment(index + PlayerBoard.FENCE_GRID_WIDTH + 1, Direction.DOWN));
                if (x < PlayerBoard.FENCE_GRID_WIDTH - 2) neighbors.push(new RunSegment(index + 1, Direction.RIGHT));
                if (y > 0) neighbors.push(new RunSegment(index - PlayerBoard.FENCE_GRID_WIDTH + 1, Direction.UP));

            }
            else {
                if (y > 0) neighbors.push(new RunSegment(index - PlayerBoard.FENCE_GRID_WIDTH, Direction.UP));
                if (x > 0) neighbors.push(new RunSegment(index - 1, Direction.LEFT));
                if (y < PlayerBoard.FENCE_GRID_HEIGHT - 1) neighbors.push(new RunSegment(index + PlayerBoard.FENCE_GRID_WIDTH, Direction.DOWN));
            }
        }
        else {
            if (this.direction == Direction.UP) {
                if (x < PlayerBoard.FENCE_GRID_WIDTH - 1) neighbors.push(new RunSegment(index - PlayerBoard.FENCE_GRID_WIDTH, Direction.RIGHT));
                if (y > 1) neighbors.push(new RunSegment(index - PlayerBoard.FENCE_GRID_WIDTH * 2, Direction.UP));
                if (x > 0) neighbors.push(new RunSegment(index - PlayerBoard.FENCE_GRID_WIDTH - 1, Direction.LEFT));
            }
            else {
                if (x > 0) neighbors.push(new RunSegment(index + PlayerBoard.FENCE_GRID_WIDTH - 1, Direction.LEFT));
                if (y < PlayerBoard.FENCE_GRID_HEIGHT - 2) neighbors.push(new RunSegment(index + PlayerBoard.FENCE_GRID_WIDTH * 2, Direction.DOWN));
                if (x < PlayerBoard.FENCE_GRID_WIDTH - 1) neighbors.push(new RunSegment(index + PlayerBoard.FENCE_GRID_WIDTH, Direction.RIGHT));
            }
        }

        return neighbors;
    }
}


