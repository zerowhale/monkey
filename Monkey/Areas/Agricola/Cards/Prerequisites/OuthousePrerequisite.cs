using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    public class OuthousePrerequisite: Prerequisite
    {
        public OuthousePrerequisite(XElement definition)
            :base (definition)
        {

        }

        public override bool IsMet(AgricolaPlayer player)
        {
            foreach (var p in ((AgricolaGame)player.Game).AgricolaPlayers)
            {
                if((player != p || (player == p && player.Game.Players.Length == 1))
                    && Curator.GetOwnedOccupationCount(p) < 2)
                    return true;
            }
            return false;
        }
    
    }
}