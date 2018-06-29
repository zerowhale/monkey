using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Field: FarmyardEntity
    {
        public Field(int x, int y)
            : base("Field", x, y)
        {
            Sown = new ResourceCache(Resource.Grain, 0);
        }

        public Field(ResourceCache sownResource, int x, int y)
            : base("Field", x, y)
        {
            Sown = sownResource;
        }

        public Field Sow(AgricolaPlayer player, Resource resource)
        {
            if (resource == Resource.Grain)
            {
                return new Field(new ResourceCache(Resource.Grain,3), Location.X, Location.Y);
            }
            else if (resource == Resource.Vegetables)
            {
                var count = 2;

                // This needs to be moved into the Curator
                // PotatoDibber
                if (player.OwnedCardIds.Contains(32))
                {
                    count++;
                }

                return new Field(new ResourceCache(Resource.Vegetables, count), Location.X, Location.Y);
            }
            throw new ArgumentException("Attempting to sow a resource that can not be sown");
        }

        public Field Harvest(out ResourceCache harvestedResources)
        {
            Field field;
            if (Sown.Count > 0)
            {
                field = new Field(Sown.updateCount(-1), Location.X, Location.Y);
                harvestedResources = new ResourceCache(Sown.Type, 1);
            }
            else
            {
                field = this;
                harvestedResources = null;
            }
            return field;
        }

        public ResourceCache Sown { get; }


    }
}