using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class TutorVictoryPoints: PointCalculator
    {
        public TutorVictoryPoints(XElement definition)
            :base(definition)
        {
            field = (string)definition.Attribute("Field");
        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            var cards = (List<Card>)OwningCard.Metadata[this.field];
            title = Title;
            return cards.Count - 1;
        }

        private string field;
    
    
    }
}