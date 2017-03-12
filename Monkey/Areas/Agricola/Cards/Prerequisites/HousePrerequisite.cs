using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Prerequisites
{
    public class HousePrerequisite: Prerequisite
    {
        public HousePrerequisite(XElement definition)
            :base (definition)
        {
            HouseType = definition.Attribute("HouseType") == null ? HouseType.Wood : (HouseType)Enum.Parse(typeof(HouseType), (string)definition.Attribute("HouseType"));
        }

        public override bool IsMet(AgricolaPlayer player)
        {
            return player.Farmyard.HouseType == HouseType;
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public HouseType HouseType;
    }
}