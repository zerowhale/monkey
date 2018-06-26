using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Pasture: Empty
    {
        public Pasture()
            :this(false)
        {

        }

        public Pasture(bool hasStable)
            : base ("Pasture", hasStable )
        {
        }

        public override Empty AddStable()
        {
            return new Pasture(true);
        }

    }
}