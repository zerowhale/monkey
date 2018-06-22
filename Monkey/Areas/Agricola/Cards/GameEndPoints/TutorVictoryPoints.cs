using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class TutorVictoryPoints: PointCalculator
    {
        public TutorVictoryPoints(XElement definition, Card owningCard)
            :base(definition, owningCard)
        {
        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            Object cardCount;

            title = Title;
            if (player.TryGetCardMetadata(this.OwningCard, out cardCount))
            {
                return (int)cardCount;
            }
            return 0;
        }
    
    }
}