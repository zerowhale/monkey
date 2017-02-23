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
                Sown.Type = Resource.Grain;
                Sown.Count = 3;
            }
            else if (resource == Resource.Vegetables)
            {
                var count = 2;

                // PotatoDibber
                if (player.OwnedCardIds.Contains(1010))
                {
                    count++;
                }

                Sown.Type = Resource.Vegetables;
                Sown.Count = count;
            }
        }

        public ResourceCache Harvest()
        {
            if (Sown.Count > 0)
            {
                Sown.Count--;
                return new ResourceCache(Sown.Type, 1);
            }
            return null;
        }

        public ResourceCache Sown
        {
            get;
            private set;
        }


    }
}