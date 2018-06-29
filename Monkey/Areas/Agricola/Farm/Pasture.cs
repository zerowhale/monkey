using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Pasture: Empty
    {
        public Pasture(int x, int y)
            :this(false, x, y)
        {

        }

        public Pasture(bool hasStable, int x, int y)
            : base ("Pasture", hasStable, x, y)
        {
        }

        public override Empty AddStable()
        {
            return new Pasture(true, Location.X, Location.Y);
        }

    }
}