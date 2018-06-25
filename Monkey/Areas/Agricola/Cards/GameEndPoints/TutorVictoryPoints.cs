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
            ImmutableDictionary<string, Object> metadata;

            title = Title;
            if (player.TryGetCardMetadata(this.OwningCard, out metadata))
            {
                return metadata.Keys.Contains("tutor") ? (int)metadata["tutor"] : 0;
            }
            return 0;
        }
    
    }
}