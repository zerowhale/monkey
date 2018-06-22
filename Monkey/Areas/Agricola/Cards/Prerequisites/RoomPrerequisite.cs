using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    /// <summary>
    /// A prerequisite requiring a player to have at least a certain number of rooms.
    /// </summary>
    public class RoomPrerequisite: Prerequisite
    {
        public RoomPrerequisite(XElement definition)
            :base (definition)
        {
            RoomCount = definition.Attribute("RoomCount") == null ? 0 : (int)definition.Attribute("RoomCount");
        }

        public override bool IsMet(AgricolaPlayer player)
        {
            return player.Farmyard.RoomCount >= RoomCount;
        }

        public readonly int RoomCount;
    }
}