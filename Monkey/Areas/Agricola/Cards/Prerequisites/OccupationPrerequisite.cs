using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    /// <summary>
    /// Prerequisite to playing a card that requires a certain number of
    /// Occupations to be owned by the player.
    /// </summary>
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

        public readonly int Count;

    }
}