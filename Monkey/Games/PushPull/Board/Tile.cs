using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.PushPull.Board
{
    /// <summary>
    /// A tile is a hex that makes up one segment of the game board
    /// </summary>
    public class Tile
    {

        /// <summary>
        /// Tiles must be created with a type.
        /// </summary>
        /// <param name="type"></param>
        public Tile(TileType type, int number)
        {
            Type = type;
            Number = number;
        }

        /// <summary>
        /// The type of tile.
        /// </summary>
        public TileType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// The dice roll number to trigger this tile
        /// </summary>
        public int Number
        {
            get;
            private set;
        }
    }
}