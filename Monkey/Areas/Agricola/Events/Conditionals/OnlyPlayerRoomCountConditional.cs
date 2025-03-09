using Monkey.Games.Agricola;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Conditionals
{
    public class OnlyPlayerRoomCountConditional : GameEventConditional
    {
        public OnlyPlayerRoomCountConditional() : base() { }

        public OnlyPlayerRoomCountConditional(XElement definition) : base(definition) {
            if (definition.Attribute("HouseType") != null)
                HouseType = (HouseType)Enum.Parse(typeof(HouseType), (string)definition.Attribute("HouseType"));

            if (definition.Attribute("RoomCount") != null)
                RoomCount = (int)definition.Attribute("RoomCount");
        }

        public override bool IsMet(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer)
        {
            var game = (AgricolaGame)triggeringPlayer.Game;
            var resolvingPlayerMatchesRoomCount = false;
            foreach (var player in game.AgricolaPlayers )
            {
                var houseTypeMatches = HouseType == HouseType.Any 
                    || player.Farmyard.HouseType == HouseType;

                if (player.Farmyard.RoomCount == RoomCount && houseTypeMatches)
                {
                    if(player != resolvingPlayer)
                        return false;
                    else 
                        resolvingPlayerMatchesRoomCount = true;
                }
            }

            return resolvingPlayerMatchesRoomCount;
        }

        public HouseType HouseType { get; set; } = HouseType.Any;

        public int RoomCount { get; set; } = 2;
    }
}