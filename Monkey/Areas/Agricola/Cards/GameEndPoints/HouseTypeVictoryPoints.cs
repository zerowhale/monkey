using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class HouseTypeVictoryPoints: PointCalculator
    {
        public HouseTypeVictoryPoints(XElement definition, Card owningCard)
            :base(definition, owningCard)
        {
            points = definition.Attribute("Points") == null ? 1 : (int)definition.Attribute("Points");
            houseType = (HouseType)Enum.Parse(typeof(HouseType), (string)definition.Attribute("HouseType"));
        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            switch (houseType)
            {
                case HouseType.Wood:
                    title = "Wood Hut";
                    break;

                case HouseType.Clay:
                    title = "Clay Hut";
                    break;

                case HouseType.Stone:
                    title = "Stone House";
                    break;
                default:
                    title = "";
                    break;
            }
            return player.Farmyard.HouseType == houseType ? points : 0;
        }

        private int points { get; }
        private HouseType houseType { get; }
    }
}