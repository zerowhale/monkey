using BoardgamePlatform.Game.Utils;
using Monkey.Games.Agricola.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class FarmyardEntity
    {
        public FarmyardEntity(String type, int x, int y)
        {
            Location = new Point(x, y);
            Type = type;
        }

        public int LocationIndex { get { return Location.Y * Farmyard.WIDTH + Location.X; } }

        public Point Location { get; }

        public String Type { get; }
    }
    
}