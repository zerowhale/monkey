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
            title = Title;

            ImmutableDictionary<string, Object> metadata;
            Object fieldData;
            return player.TryGetCardMetadataField(this.OwningCard, "tutor", out metadata, out fieldData) ? (int)fieldData : 0;
        }
    
    }
}