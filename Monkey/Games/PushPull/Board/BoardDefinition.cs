using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.PushPull.Board
{
    public class BoardDefinition
    {

        public BoardDefinition(string name, List<TileType> tiles, List<int> numbers, bool expandedMap = false)
            : this(name, tiles, numbers, expandedMap, false, false)
        {
        }

        public BoardDefinition(string name, List<TileType> tiles, List<int> numbers, bool expandedMap, bool randomizedTiles, bool randomizedNumbers)
        {
            Name = name;
            ExpandedMap = expandedMap;
            Tiles = tiles;
            Numbers = numbers;
            RandomizedTiles = randomizedTiles;
            RandomizedNumbers = randomizedNumbers;
        }

        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// For 5-6 players
        /// </summary>
        public bool ExpandedMap
        {
            get;
            private set;
        }

        public bool RandomizedTiles
        {
            get;
            private set;
        }

        public bool RandomizedNumbers
        {
            get;
            private set;
        }

        /// <summary>
        /// Order of the tiles in the map.
        /// </summary>
        public List<TileType> Tiles
        {
            get;
            private set;
        }


        /// <summary>
        /// order of the numbers
        /// </summary>
        public List<int> Numbers
        {
            get;
            private set;
        }

        
    }
}