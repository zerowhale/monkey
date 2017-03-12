using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class PerRoomVictoryPoints: PointCalculator
    {
        public PerRoomVictoryPoints(XElement definition)
            :base(definition)
        {
            HouseType = (HouseType)Enum.Parse(typeof(HouseType), (string)definition.Attribute("HouseType"));
        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            title = "Rooms in ";
            switch (HouseType)
            {
                case HouseType.Wood:
                    title += "Wood Hut";
                    break;

                case HouseType.Clay:
                    title += "Clay Hut";
                    break;

                case HouseType.Stone:
                    title += "Stone House";
                    break;
            }
            var points = 0;
            if (player.Farmyard.HouseType == HouseType)
                points = player.Farmyard.RoomCount;

            return points;
        }

        private HouseType HouseType
        {
            get;
            set;
        }
    }
}