using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Linq;
using State = System.Collections.Immutable.ImmutableDictionary<string, object>;

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

        public override State RoundStart(State state)
        {
            State = AddCacheResources(state, resourcesPerRound);
            return State = base.RoundStart(State);
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


            var cacheResources = GetCacheResources(State);
            var resources = cacheResources.Values.ToList();

            foreach (var cache in resources)
            {
                cacheResources = cacheResources.SetItem(cache.Type, new ResourceCache(cache.Type, 0));
            }

            foreach (var cache in TakeResourceCaches)
                resources.Add(cache);

            ActionService.AssignCacheResources(player, eventTriggers, ResultingNotices, resources.ToArray());
            SetCacheResources(cacheResources);


            /*
            var resources = CacheResources.Values.ToList();

            foreach (var cache in TakeResourceCaches)
                resources.Add(cache);

            ActionService.AssignCacheResources(player, eventTriggers, ResultingNotices, resources.ToArray());

            foreach (var cache in resources)
            {
                if (CacheResources.ContainsKey(cache.Type))
                {
                    CacheResources = CacheResources.SetItem(cache.Type, new ResourceCache(cache.Type, 0));
                }
            }
            */
            return this;
        }

        /// <summary>
        /// List of resource caches for the take portion of the action
        /// </summary>
        public ResourceCache[] TakeResourceCaches { get; }

    }
}