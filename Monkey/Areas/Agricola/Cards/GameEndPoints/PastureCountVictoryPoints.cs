using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class PastureCountVictoryPoints: PointCalculator
    {
        public PastureCountVictoryPoints(XElement definition, Card owningCard)
            :base(definition, owningCard)
        {
            var pcp = new List<PastureCountPoints>();

            foreach (var row in definition.Descendants("PastureCount"))
            {
                var count = (int)row.Attribute("RequiredCount");
                var points = (int)row.Attribute("Points");
                var rrp = new PastureCountPoints(count, points);
                pcp.Add(rrp);
            }

            options = pcp.OrderByDescending(x => x.RequiredCount).ToImmutableArray();
        }

        /// <summary>
        /// Rewards additional points based on the number of pastures
        /// </summary>
        /// <param name="player"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            var numPastures = player.Farmyard.PastureLocations.Count();
            foreach (var option in options)
            {
                if (option.RequiredCount <= numPastures)
                {
                    title = option.RequiredCount + " Pastures";
                    return option.Points;
                }
            }
            title = null;
            return 0;
        }

        private ImmutableArray<PastureCountPoints> options;

        private struct PastureCountPoints
        {
            public PastureCountPoints(int count, int points)
            {
                RequiredCount = count;
                Points = points;
            }

            public int RequiredCount;
            public int Points;
        }
    }
}