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
            this.Sown = new ResourceCache(Resource.Grain, 0);

        }

        public void Sow(AgricolaPlayer player, Resource resource)
        {
            if (resource == Resource.Grain)
            {
                this.Sown = new ResourceCache(Resource.Grain,3);
            }
            else if (resource == Resource.Vegetables)
            {
                var count = 2;

                // PotatoDibber
                if (player.OwnedCardIds.Contains(32))
                {
                    count++;
                }

                this.Sown = new ResourceCache(Resource.Vegetables, count);
            }
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