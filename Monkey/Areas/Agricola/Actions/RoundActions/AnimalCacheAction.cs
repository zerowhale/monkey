using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Farm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class AnimalCacheAction: BasicCacheAction 
    {
        public AnimalCacheAction(AgricolaGame game, int id, Resource resource, GameEventTrigger[] eventTriggers = null)
            : base(game, id, resource, 1, eventTriggers)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            foreach (var cache in CacheResources.Values)
            {
                var animalData = new Dictionary<AnimalResource, int>();
                animalData[(AnimalResource)Enum.Parse(typeof(Resource), cache.Type.ToString())] = cache.Count;
                if (!ActionService.CanAssignAnimals(player, (AnimalCacheActionData)data, animalData))
                    return false;
            }
            return base.CanExecute(player, data);
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);
            ActionService.AssignAnimals(player, (AnimalCacheActionData)data, ResultingNotices);
        }

    }
}