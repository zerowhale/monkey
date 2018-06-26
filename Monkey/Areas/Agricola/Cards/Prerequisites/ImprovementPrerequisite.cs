using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    public class ImprovementPrerequisite: Prerequisite
    {
        public ImprovementPrerequisite(XElement definition)
            :base (definition)
        {
            Count = (int)definition.Attribute("Count");

        }

        public override bool IsMet(AgricolaPlayer player)
        {
            return player.OwnedCards.Count(x => x is Improvement) >= Count;
        }

        public int Count { get; }

    }
}