var FenceUtils = {
    
   
    /**
        Returns an object containing the plot data
        for the plot that owns the fence at x,y.

        Plot data contains the plots x,y coordinates
        and the position of the fence in the plot ("left",
        "right", "top", or "bottom").
     */
    getPlotDataForFence: function (x, y) {
        var farmWidth = PlayerBoard.prototype.PLOT_GRID_WIDTH,
            width = PlayerBoard.prototype.FENCE_GRID_WIDTH,
            height = PlayerBoard.prototype.FENCE_GRID_HEIGHT,
            px, py,
            position;

        if (x <= width - 2)
            px = x;
        else {
            px = PlayerBoard.prototype.PLOT_GRID_WIDTH - 1;
            position = "right";
        }

        if (y < height - 1) {
            if (y % 2 == 0)
                py = y / 2;
            else
                py = (y + 1) / 2 - 1;
        }
        else {
            py = PlayerBoard.prototype.PLOT_GRID_HEIGHT -1;
            position = "bottom";
        }

        if (y != height-1 && x != width -1)
            position = y % 2 == 0 ? "top" : "left";

        return { x: px, y: py, index:px + py * farmWidth, position: position };
    },


    /**
        Gets the fence data for the 4 fences bordering the given plot
     */
    getPlotBorderingFenceData: function(x,y){
        var fencesWidth = PlayerBoard.prototype.FENCE_GRID_WIDTH,
            top = { x: x, y: (y * 2), index: 0},
            right = { x: x + 1, y: (y * 2 + 1), index: 0},
            bottom = { x: x, y: (y * 2 + 2), index: 0},
            left = { x: x, y: (y * 2 + 1), index: 0};

        top.index = top.x + top.y * fencesWidth;
        right.index = right.x + right.y * fencesWidth;
        bottom.index = bottom.x + bottom.y * fencesWidth;
        left.index = left.x + left.y * fencesWidth;

        return {
            top: top,
            right: right,
            bottom: bottom,
            left: left
        };
    },


    /**
        Gets the one or two plots that border the
        fence at the given coordinates

        Returns an array of either 1 or 2 plots that border
        the fence.
     */
    getNeighboringPlots: function (x, y) {
        var plotWidth = PlayerBoard.prototype.PLOT_GRID_WIDTH,
            plotHeight = PlayerBoard.prototype.PLOT_GRID_HEIGHT,
            fencesWidth = PlayerBoard.prototype.FENCE_GRID_WIDTH,
            fencesHeight = PlayerBoard.prototype.FENCE_GRID_HEIGHT;

        if (x == fencesWidth - 1 && y == fencesHeight - 1) return [plotWidth * plotHeight - 1];
        if (x == 0 && y % 2 != 0) return [(y - 1) / 2 * plotWidth];
        if (y == 0) return [x];
        if (x == fencesWidth - 1 && y % 2 != 0) return [((y - 1) / 2) * plotWidth + x - 1];
        if (y == fencesHeight - 1) return [(plotHeight - 1) * plotWidth + x];

        var index;
        // verticals
        if (y % 2 == 0) {
            index = (y / 2 - 1) * plotWidth + x;
            return [index, index + plotWidth];
        }
            // horizontals
        else {
            index = ((y - 1) / 2) * plotWidth + (x - 1);
            return [index, index + 1];
        }

    },

    /**
        Get's all the fences that touch this fence.
        
        Returns an array of up to 6 fence indices
     */
    getNeighboringFences: function (index) {
        var width = PlayerBoard.prototype.FENCE_GRID_WIDTH,
            height = PlayerBoard.prototype.FENCE_GRID_HEIGHT,
            x = index % width,
            y = Math.floor(index / width),
            indices = [];

        if (y % 2 == 0) {
            if (x > 0) indices.push(index - 1);
            if (x < width - 2) indices.push(index + 1);
            if (y > 0) {
                indices.push(index - width);
                indices.push(index - width + 1);
            }
            if (y < height - 1) {
                indices.push(index + width);
                indices.push(index + width + 1);
            }
        }
        else {
            if (y > 1) {
                indices.push(index - width * 2);
            }
            if (y < height - 2) {
                indices.push(index + width * 2);
            }
            if (x > 0) {
                indices.push(index - width - 1);
                indices.push(index + width - 1);
            }
            if (x < width - 1) {
                indices.push(index - width);
                indices.push(index + width);
            }
        }
        return indices;
    },

}