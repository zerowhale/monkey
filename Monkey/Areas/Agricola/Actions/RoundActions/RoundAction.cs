using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    /// <summary>
    /// GameAction is the base class for all actions in the game.
    /// An action is any place (primary action, round action, or card arction) that
    /// a player can place a family member to cause some effect
    /// </summary>
    public abstract class RoundAction : GameAction
    {
        public RoundAction(AgricolaGame game, int id, AgricolaPlayer Owner, GameEventTrigger[] eventTriggers = null)
            :base (game, id, new List<GameActionNotice>(), eventTriggers)
        {
            this.Owner = Owner;
            this.Users = new List<AgricolaPlayer>();

            CacheResources = new Dictionary<Resource, ResourceCache>();
        }

        public RoundAction(AgricolaGame game, int id)
            : this(game, id, null)
        {
        }

        public RoundAction(AgricolaGame game, int id, GameEventTrigger[] eventTriggers)
            : this(game, id, null, eventTriggers)
        {
        }

        /// <summary>
        /// Called at the start of each round.  Any cumulative round effects
        /// should be added here.
        /// </summary>
        public virtual void RoundStart()
        {
            this.Users.Clear();
        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            return (this.Users.Count == 0 && player.HasFamilyMemberAvailable());
        }

        public override void OnExecute(AgricolaPlayer player, GameActionData data)
        {
            this.Users.Add(player);
            player.UseFamilyMember();
        }

        public void AddUser(AgricolaPlayer player)
        {
            this.Users.Add(player);
        }

        public void AddDelayedResource(AgricolaPlayer player, ResourceCache cache)
        {
            if (!delayedResources.ContainsKey(player))
                delayedResources[player] = new Dictionary<Resource, ResourceCache>();
            var playersCaches = delayedResources[player];
            if (playersCaches.ContainsKey(cache.Type))
                playersCaches[cache.Type] = playersCaches[cache.Type].updateCount(cache.Count);
            else
                playersCaches[cache.Type] = new ResourceCache(cache.Type, cache.Count);
        }

        public void DistributeDelayedResources(List<GameActionNotice> resultingNotices)
        {
            foreach (var kvp in delayedResources)
            {
                var player = kvp.Key;
                foreach (var cache in kvp.Value.Values)
                {
                    if (!(Enum.IsDefined(typeof(AnimalResource), cache.Type.ToString())))
                        player.AddResource(cache);
                    else
                    {
                        AnimalResource animalType = (AnimalResource)cache.Type;// (AnimalResource)Enum.Parse(typeof(AnimalResource), cache.Type.ToString());
                        ((AgricolaGame)player.Game).AddInterrupt(new AssignAnimalsAction(player, animalType, cache.Count, resultingNotices));
                    }
                }
            }
            delayedResources.Clear();
        }


        /// <summary>
        /// Name of the user using this action, or null if not in use.
        /// </summary>
        public String[] UserNames
        {
            get { return Users.Select(x => x.Name).ToArray(); }
        }

        /// <summary>
        /// The user using this action, or null if not in use.
        /// </summary>
        [JsonIgnore]
        public List<AgricolaPlayer> Users
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the owner, or null if unknowned.
        /// See Owner.
        /// </summary>
        public String OwnerName
        {
            get { return Owner == null ? null : Owner.Name;  }
        }

        /// <summary>
        /// The owner of an action.  The only time an action will have an
        /// owner is if the action was created as the result of a card played
        /// by a player.
        /// </summary>
        [JsonIgnore]
        public AgricolaPlayer Owner { get; }

        /// <summary>
        /// Get's the list of delayed resources to be sent to the front end
        /// </summary>
        public Dictionary<String, ResourceCache[]> DelayedResources
        {
            get
            {
                if (delayedResources.Keys.Count == 0) return null;
                var result = new Dictionary<String, ResourceCache[]>();
                foreach (var player in delayedResources.Keys)
                {
                    result[player.Name] = delayedResources[player].Select(x => x.Value).ToArray();
                }
                return result;
            }
        }

        public void TakeCaches(AgricolaPlayer player, Dictionary<Resource, int> leaveBehind, Dictionary<Resource, int>gained)
        {
            var taking = new Dictionary<Resource, int>();
            foreach (var cache in CacheResources)
            {
                if (!taking.ContainsKey(cache.Value.Type))
                    taking[cache.Value.Type] = cache.Value.Count;
                else
                    taking[cache.Value.Type] += cache.Value.Count;
            }

            if (leaveBehind != null)
            {
                foreach (var cache in leaveBehind)
                {
                    if(taking.ContainsKey(cache.Key)){
                        taking[cache.Key] -= leaveBehind[cache.Key];
                    }
                }
            }

            var takingCaches = taking.Select(x => new ResourceCache(x.Key, x.Value));

            if (eventTriggers != null)
            {
                foreach (var trigger in eventTriggers)
                {
                    if (trigger.GetType() == typeof(TravelingPlayersActionTrigger))
                    {
                        ((TravelingPlayersActionTrigger)trigger).FoodTaken = taking[Resource.Food];
                    }
                }
            }

            if (gained != null)
            {
                foreach (var kvp in gained)
                {
                    if (!taking.ContainsKey(kvp.Key))
                        taking[kvp.Key] = kvp.Value;
                    else
                        taking[kvp.Key] += kvp.Value;
                }
            }


            if (takingCaches.Count() > 1)
            {
                ActionService.AssignCacheResources(player,
                    eventTriggers,
                    ResultingNotices,
                    taking.Select(x => new ResourceCache(x.Key, x.Value)).ToArray());

                foreach(var kvp in taking)
                {
                    if (CacheResources.ContainsKey(kvp.Key))
                    {
                        var cache = CacheResources[kvp.Key];
                        CacheResources[kvp.Key] = cache.updateCount(-kvp.Value);
                    }
                }
            }
            else
            {
                var cache = CacheResources.Values.First();
                if (cache.Type.IsAnimal())
                {
                    //player.Game.AddInterrupt(new AssignAnimalsAction(player, cache, ResultingNotices));
                }
                else
                {
                    ActionService.AssignCacheResource(player, eventTriggers, ResultingNotices, new ResourceCache(cache.Type, taking[cache.Type]));
                }
                CacheResources[cache.Type] = cache.updateCount(-taking[cache.Type]);
            }
        }

        public void TakeCaches(AgricolaPlayer player)
        {
            TakeCaches(player, null, null);
        }

        public void AddCacheResources(ResourceCache cache)
        {
            if (CacheResources.ContainsKey(cache.Type))
                CacheResources[cache.Type] = CacheResources[cache.Type].updateCount(cache.Count);
            else
                CacheResources[cache.Type] = new ResourceCache(cache.Type, cache.Count);
        }

        /// <summary>
        /// Client visibile array of chache resources
        /// </summary>
        public ResourceCache[] Cache
        {
            get { return CacheResources.Values.ToArray();  }
        }

        [JsonIgnore]
        public Dictionary<Resource, ResourceCache> CacheResources
        {
            get;
            protected set;
        }


        private Dictionary<AgricolaPlayer, Dictionary<Resource, ResourceCache>> delayedResources = new Dictionary<AgricolaPlayer, Dictionary<Resource, ResourceCache>>();

    }
}