using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class MendicantVictoryPoints: PointCalculator
    {
        public MendicantVictoryPoints(XElement definition, Card owningCard)
            :base(definition, owningCard)
        {

        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            var x = player.BeggingCards;
            if (x == 0)
            {
                title = "";
                return 0;
            }

            if (x > 2)
                x = 2;

            title = "Discard " + x + " Begging Card";
            if (x > 1)
                title += "s";

            return x * 3;

        }
    }
}