using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    public class CanBuildFencePrerequisite: Prerequisite
    {
        public CanBuildFencePrerequisite(XElement definition)
            :base (definition)
        {
            
        }

        public override bool IsMet(AgricolaPlayer player)
        {
            return Curator.CanPlaceFences(player);
        }
    }
}