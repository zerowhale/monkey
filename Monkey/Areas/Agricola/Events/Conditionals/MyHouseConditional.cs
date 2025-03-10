using Monkey.Games.Agricola;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Conditionals
{
    public class MyHouseConditional : GameEventConditional
    {
        public MyHouseConditional() : base() { }

        public MyHouseConditional(XElement definition) : base(definition)
        {
            if (definition.Attribute("HouseType") != null)
                HouseType = (HouseType)Enum.Parse(typeof(HouseType), (string)definition.Attribute("HouseType"));

            if (definition.Attribute("MinimumRoomCount") != null)
                MinimumRoomCount = (int)definition.Attribute("MinimumRoomCount");
        }

        public override bool IsMet(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer)
        {
            var houseMatches = HouseType == HouseType.Any || resolvingPlayer.Farmyard.HouseType == HouseType;
            var roomsMatch = resolvingPlayer.Farmyard.RoomCount >= MinimumRoomCount;

            return houseMatches && roomsMatch;
        }

        public HouseType HouseType { get; set; } = HouseType.Any;

        public int MinimumRoomCount { get; set; } = 2;
    }
}