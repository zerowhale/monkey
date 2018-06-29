using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Room: FarmyardEntity
    {
        public Room(int x, int y)
            : base("Room", x, y)
        {

        }
    }
}