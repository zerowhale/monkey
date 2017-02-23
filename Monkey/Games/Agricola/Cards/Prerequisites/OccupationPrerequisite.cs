using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    public class OccupationPrerequisite: Prerequisite
    {
        public OccupationPrerequisite(XElement definition)
            :base (definition)
        {
            Count = (int)definition.Attribute("Count");

        }

        public override bool IsMet(AgricolaPlayer player)
        {
            var owned = Curator.GetOwnedOccupationCount(player);
            return owned >= Count;
        }

        public int Count
        {
            get;
            private set;
        }

    }
}