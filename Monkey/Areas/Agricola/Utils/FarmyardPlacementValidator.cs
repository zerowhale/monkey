using BoardgamePlatform.Game.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Utils
{
    /// <summary>
    /// Determines if i a proposed set of placements is valid in the existing farmyard
    /// </summary>    
    public static class FarmyardPlacementValidator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="foundation">Existing elements of the same type as the proposed locations</param>
        /// <param name="closed">Points that are invalid for placement but do not count as foundation points</param>
        /// <param name="proposed">List of points where new items are trying to be added.</param>
        /// <returns></returns>
        public static Boolean AreValidPlots(Int32 width, Int32 height, List<Point> foundation, List<Point> closed, List<Point> proposed)
        {
            highestAssigned = 0;
            grid = new Int32[width, height];
            if (foundation != null && foundation.Count() > 0)
            {
                highestAssigned++;
                foreach (var p in foundation)
                {
                    grid[p.X, p.Y] = highestAssigned;
                }
            }

            foreach (var p in closed)
            {
                grid[p.X, p.Y] = -1;
            }

            foreach (var p in proposed)
            {
                if (p.X >= width || p.Y >= height || p.X < 0 || p.Y < 0)
                    return false;

                // If anything exists at the target location 
                // the placement is invalid
                if (grid[p.X, p.Y] != 0)
                    return false;

                PlaceNew(p);
            }

            for (var x = 0; x < grid.GetLength(0); x++)
            {
                for (var y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y] > 1)
                        return false;
                }
            }

            return true;
        }



        private static void PlaceNew(Point point){
            var up = new Point(point.X, point.Y - 1);
            var down = new Point(point.X, point.Y + 1);
            var left = new Point(point.X - 1, point.Y);
            var right = new Point(point.X + 1, point.Y);

            Int32 upVal = 0, 
                downVal = 0, 
                leftVal = 0, 
                rightVal = 0,
                lowestVal = Int32.MaxValue;

            if (ValidPoint(up))
            {
                upVal = grid[up.X, up.Y];
                if (upVal > 0 && upVal < lowestVal)
                    lowestVal = upVal;
            }
            if (ValidPoint(down))
            {
                downVal = grid[down.X, down.Y];
                if (downVal > 0 && downVal < lowestVal)
                    lowestVal = downVal;
            }
            if (ValidPoint(left))
            {
                leftVal = grid[left.X, left.Y];
                if (leftVal > 0 && leftVal < lowestVal)
                    lowestVal = leftVal;
            }
            if (ValidPoint(right))
            {
                rightVal = grid[right.X, right.Y];
                if (rightVal > 0 && rightVal < lowestVal)
                    lowestVal = rightVal;
            }

            if (lowestVal == Int32.MaxValue)
            {
                highestAssigned++;
                grid[point.X, point.Y] = highestAssigned;
            }
            else
            {
                grid[point.X, point.Y] = lowestVal;
                if (upVal > lowestVal) CollapseValue(upVal, lowestVal);
                if (downVal > lowestVal) CollapseValue(downVal, lowestVal);
                if (leftVal > lowestVal) CollapseValue(leftVal, lowestVal);
                if (rightVal > lowestVal) CollapseValue(rightVal, lowestVal);
            }
        }

        private static void CollapseValue(Int32 from, Int32 to)
        {
            for (var x = 0; x < grid.GetLength(0); x++)
            {
                for (var y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y] == from)
                        grid[x, y] = to;
                }
            }
        }

        private static Boolean ValidPoint(Point point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X < grid.GetLength(0) && point.Y < grid.GetLength(1);
        }

        private static Int32[,] grid;
        private static Int32 highestAssigned = 0;

    }
}