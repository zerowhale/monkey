using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class LordOfTheManorVictoryPoints: PointCalculator
    {
        /// <summary>
        /// Constructor for loading from an xml definition.
        /// </summary>
        /// <param name="definition">The XML segment that defines this object.</param>
        public LordOfTheManorVictoryPoints(XElement definition)
            :base(definition)
        {

        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            var points = 0;
            if (player.Farmyard.FieldLocations.Count() >= 5)
                points++;

            if (player.Farmyard.Pastures.Count() >= 4)
                points++;

            if (player.Grain + player.Farmyard.PlantedResourceCount(Resource.Grain) >= 8)
                points++;

            if (player.Vegetables + player.Farmyard.PlantedResourceCount(Resource.Vegetables) >= 4)
                points++;

            if (player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Sheep) >= 8)
                points++;

            if (player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Boar) >= 7)
                points++;

            if (player.Farmyard.AnimalManager.GetAnimalCount(AnimalResource.Cattle) >= 6)
                points++;

            var pastures = player.Farmyard.PastureLocations;
            var stables = player.Farmyard.StableLocations;
            if (pastures.Intersect(stables).Count() >= 4)
                points++;

            title = points + " maxed scores";
            return points;
        }
    }
}