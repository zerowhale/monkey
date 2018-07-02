using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class TakeCacheComboAction: RoundAction
    {
        public TakeCacheComboAction(AgricolaGame game, Int32 id, Resource cacheType, Int32 cacheCount, ResourceCache[] takeCaches, GameEventTrigger[] triggers = null)
            : base(game, id, triggers)
        {
            TakeResourceCaches = takeCaches;
            resourcesPerRound = new ResourceCache(cacheType, cacheCount);
        }

        public override void RoundStart()
        {
            AddCacheResources(resourcesPerRound);
            base.RoundStart();
        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            var resources = CacheResources.Values.ToList();

            foreach (var cache in TakeResourceCaches)
                resources.Add(cache);

            ActionService.AssignCacheResources(player, eventTriggers, ResultingNotices, resources.ToArray());

            foreach (var cache in resources)
            {
                if (CacheResources.ContainsKey(cache.Type))
                {
                    CacheResources[cache.Type] = new ResourceCache(cache.Type, 0);
                }
            }

            return this;
        }

        /// <summary>
        /// List of resource caches for the take portion of the action
        /// </summary>
        public readonly ResourceCache[] TakeResourceCaches;

        protected ResourceCache resourcesPerRound;
    }
}