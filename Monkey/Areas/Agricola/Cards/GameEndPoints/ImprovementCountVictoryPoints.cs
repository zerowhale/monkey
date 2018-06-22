using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class ImprovementCountVictoryPoints : PointCalculator
    {
        public ImprovementCountVictoryPoints(XElement definition, Card owningCard)
            :base(definition, owningCard)
        {
            var points = new List<RequiredImprovementPoints>();

            foreach (var row in definition.Descendants("ImprovementCount"))
            {
                var point = new RequiredImprovementPoints(
                    (int)row.Attribute("Required"), 
                    (int)row.Attribute("Points"));
                points.Add(point);
            }

            options = points.OrderByDescending(x => x.RequiredCount).ToArray();
        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            var cards = player.OwnedCards.Count(x => (x is MajorImprovement) || (x is MinorImprovement));

            foreach (var option in options)
            {
                if (option.RequiredCount <= cards)
                {
                    title = option.RequiredCount + " improvements";
                    return option.Points;
                }
            }
            title = null;
            return 0;
        }

        private RequiredImprovementPoints[] options;


        private struct RequiredImprovementPoints
        {
            public RequiredImprovementPoints(int requiredCount, int points)
            {
                RequiredCount = requiredCount;
                Points = points;
            }

            public int RequiredCount;
            public int Points;
        }
    }
}