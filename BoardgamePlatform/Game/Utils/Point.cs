using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoardgamePlatform.Game.Utils
{
    /// <summary>
    /// Json ready 2d point.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Create a new point.
        /// </summary>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        public Point(Int32 x, Int32 y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// X Coordinate
        /// </summary>
        [JsonProperty(PropertyName="x")]
        public Int32 X;

        /// <summary>
        /// Y Coordinate
        /// </summary>
        [JsonProperty(PropertyName = "y")]
        public Int32 Y;
    }
}