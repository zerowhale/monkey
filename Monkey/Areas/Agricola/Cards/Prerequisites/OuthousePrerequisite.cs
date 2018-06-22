using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    /// <summary>
    /// Prerequisite for the Outhouse improvement
    /// </summary>
    public class OuthousePrerequisite: Prerequisite
    {
        public OuthousePrerequisite(XElement definition)
            :base (definition)
        {

        }

        /// <summary>
        /// Returns true if at least one other player in a multiplayer game has no more than 1 occupations
        /// in play.  Returns true if the only player has at most 1 occupation in play in a single player game.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
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