using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Empty: FarmyardEntity
    {

        public Empty(int x, int y)
            : this(false, x, y)
        {
        }

        public Empty(bool hasStable, int x, int y)
            : this("Empty", hasStable, x, y)
        {
        }

        public Empty(string type, bool hasStable, int x, int y)
            : base(type, x, y)
        {
            HasStable = hasStable;
        }

        public virtual Empty AddStable()
        {
            return new Empty(true, Location.X, Location.Y);
        }

        public Boolean HasStable { get; }

    }
}