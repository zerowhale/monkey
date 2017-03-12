using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
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

        public int RoomCount;
    }
}