using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class PastureCountVictoryPoints: PointCalculator
    {
        public PastureCountVictoryPoints(XElement definition)
            :base(definition)
        {
            var pcp = new List<PastureCountPoints>();

            foreach (var row in definition.Descendants("PastureCount"))
            {
                var count = (int)row.Attribute("RequiredCount");
                var points = (int)row.Attribute("Points");
                var rrp = new PastureCountPoints(count, points);
                pcp.Add(rrp);
            }

            options = pcp.OrderByDescending(x => x.RequiredCount).ToArray();
        }

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

        private PastureCountPoints[] options;

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