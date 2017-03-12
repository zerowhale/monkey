using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class MostOccupationsVictoryPoints : PointCalculator
    {
        public MostOccupationsVictoryPoints(XElement definition)
            :base(definition)
        {
            points = (int)definition.Attribute("Points");
        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            var players = ((AgricolaGame)player.Game).AgricolaPlayers;
            var mostPlayedOccupations = 0;
            foreach (var p in players)
            {
                var playedOccupations = Curator.GetOwnedOccupationCount(p);
                if (playedOccupations > mostPlayedOccupations)
                    mostPlayedOccupations = playedOccupations;
            }

            title = "Most Occupations";
            if (Curator.GetOwnedOccupationCount(player) == mostPlayedOccupations)
                return points;
            return 0;
        }

        private int points;



    }
}