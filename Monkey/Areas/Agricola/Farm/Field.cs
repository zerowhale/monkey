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
        public Field()
            : base("Field")
        {
            Sown = new ResourceCache(Resource.Grain, 0);
        }

        public Field(ResourceCache sownResource)
            : base("Field")
        {
            Sown = sownResource;
        }

        public Field Sow(AgricolaPlayer player, Resource resource)
        {
            if (resource == Resource.Grain)
            {
                return new Field(new ResourceCache(Resource.Grain,3));
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

                return new Field(new ResourceCache(Resource.Vegetables, count));
            }
            throw new ArgumentException("Attempting to sow a resource that can not be sown");
        }

        public ResourceCache Harvest()
        {
            if (Sown.Count > 0)
            {
                Sown = Sown.updateCount(-1);
                return new ResourceCache(Sown.Type, 1);
            }
            return null;
        }

        public ResourceCache Sown;


    }
}