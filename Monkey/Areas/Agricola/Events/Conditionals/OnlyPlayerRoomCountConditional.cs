using Monkey.Games.Agricola;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Areas.Agricola.Events.Conditionals
{
    public class LastPlayerRoomCountConditional : GameEventConditional
    {
        public LastPlayerRoomCountConditional() : base() { }

        public LastPlayerRoomCountConditional(XElement definition) : base(definition) {
            if (definition.Attribute("HouseType") != null)
                HouseType = (HouseType)Enum.Parse(typeof(HouseType), (string)definition.Attribute("HouseType"));

            if (definition.Attribute("RoomCount") != null)
                RoomCount = (int)definition.Attribute("RoomCount");
        }

        public HouseType HouseType { get; set; } = HouseType.Wood;

        public int RoomCount { get; set; } = 2;
    }
}