using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class EstateManagerVictoryPoints: PointCalculator
    {
        public EstateManagerVictoryPoints(XElement definition)
            :base(definition)
        {

        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            title = "Most animals of each type:";
            var sheep = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Sheep);
            var boar = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Boar);
            var cattle = player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Cattle);

            foreach (var p in ((AgricolaGame)player.Game).AgricolaPlayers)
            {
                if (p.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Sheep) > sheep) return 0;
                if (p.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Boar) > boar) return 0;
                if (p.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Cattle) > cattle) return 0;
            }

            switch (player.Game.Players.Count())
            {
                case 3:
                    return 2;
                case 4:
                    return 3;
                case 5:
                    return 4;
                default:
                    return 0;
            }
        }
    }
}