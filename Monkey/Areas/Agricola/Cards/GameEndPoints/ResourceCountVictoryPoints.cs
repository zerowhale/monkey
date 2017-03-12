using Monkey.Games.Agricola.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public class ResourceCountVictoryPoints: PointCalculator
    {
        public ResourceCountVictoryPoints(XElement definition)
            :base(definition)
        {
            var rrps = new List<RequiredResourcePoints>();

            foreach (var row in definition.Descendants("ResourceCount"))
            {
                var resource = (Resource)Enum.Parse(typeof(Resource), (string)row.Attribute("Resource"));
                var required = (int)row.Attribute("Required");
                var points = (int)row.Attribute("Points");
                var rrp = new RequiredResourcePoints(resource, required, points);
                rrps.Add(rrp);
            }

            options = rrps.OrderByDescending(x => x.RequiredCount).ToArray();
        }

        public override int GetPoints(AgricolaPlayer player, out string title)
        {
            foreach (var option in options)
            {
                var count = player.PersonalSupply.GetResource(option.Resource);
                if (option.Resource == Resource.Vegetables || option.Resource == Resource.Grain)
                {
                    count += player.Farmyard.PlantedResourceCount(option.Resource);
                }


                if (option.RequiredCount <= count)
                {
                    title = option.RequiredCount + " " + option.Resource.ToString();
                    return option.Points;
                }
            }
            title = null;
            return 0;
        }

        private RequiredResourcePoints[] options;


        private struct RequiredResourcePoints
        {
            public RequiredResourcePoints(Resource resource, int requiredResourceCount, int points)
            {
                Resource = resource;
                RequiredCount = requiredResourceCount;
                Points = points;
            }

            public Resource Resource;
            public int RequiredCount;
            public int Points;
        }
    }
}