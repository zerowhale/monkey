using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using State = System.Collections.Immutable.ImmutableDictionary<string, object>;

namespace Monkey.Games.Agricola.Actions.RoundActions
{

    public abstract class RoundAction : GameAction
    {
        public RoundAction(AgricolaGame game, int id, AgricolaPlayer owner, GameEventTrigger[] eventTriggers = null)
            : base(game, id, new List<GameActionNotice>(), eventTriggers)
        {

            this.Owner = owner;            InitializeState();
        }

        public RoundAction(AgricolaGame game, int id)
            : this(game, id, null) { }

        public RoundAction(AgricolaGame game, int id, GameEventTrigger[] eventTriggers)
            : this(game, id, null, eventTriggers) { }


        public void SetState(State state)
        {
            dirtyState = false;
            State = state;
        }

        public override bool TryExecute(AgricolaPlayer player, GameActionData data, out GameAction updatedAction)
        {
            if (dirtyState)
                throw new InvalidOperationException("Local state has no been updated prior to execution.");

            var returnAction = base.TryExecute(player, data, out updatedAction);
            dirtyState = true;
            return returnAction;
        }

        /// <summary>
        /// Called at the start of each round.  Any cumulative round effects
        /// should be added here.
        /// </summary>
        public virtual State RoundStart(State state)
        {
            return ClearUsers(state);
        }

        public virtual State AddUser(State state, AgricolaPlayer player)
        {
            return State = state.SetItem(StateKeyPlayers, ((ImmutableList<AgricolaPlayer>)State[StateKeyPlayers]).Add(player));
        }

        public State ClearUsers(State state)
        {
            return State = state.SetItem(StateKeyPlayers, ImmutableList<AgricolaPlayer>.Empty);

        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            return (this.Users.Count == 0 && player.HasFamilyMemberAvailable());
        }

        public override GameAction OnExecute(AgricolaPlayer player, GameActionData data)
        {
            State = AddUser(State, player);
            player.UseFamilyMember();
            return this;
        }

        public State DistributeDelayedResources(List<GameActionNotice> resultingNotices)
        {
            var delayedResources = GetDelayedResources();
            foreach (var kvp in delayedResources)
            {
                var player = kvp.Key;
                foreach (var cache in kvp.Value.Values)
                {
                    if (!(Enum.IsDefined(typeof(AnimalResource), cache.Type.ToString())))
                    {
                        player.AddResource(cache);
                    }
                    else
                    {
                        AnimalResource animalType = (AnimalResource)cache.Type;// (AnimalResource)Enum.Parse(typeof(AnimalResource), cache.Type.ToString());
                        ((AgricolaGame)player.Game).AddInterrupt(new AssignAnimalsAction(player, animalType, cache.Count, resultingNotices));
                    }
                    resultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.TakeDelayed.ToString(), cache));
                }
            }

            State = State.SetItem(StateKeyDelayedResources, delayedResources.Clear());
            return State;
        }

        public State AddDelayedResource(AgricolaPlayer player, ResourceCache cache)
        {
            var delayedResources = GetDelayedResources();
            if (!delayedResources.ContainsKey(player))
            {
                delayedResources = delayedResources.SetItem(player, ImmutableDictionary<Resource, ResourceCache>.Empty);
            }

            var playerCaches = delayedResources[player];
            if (playerCaches.ContainsKey(cache.Type))
            {
                playerCaches = playerCaches.SetItem(cache.Type, playerCaches[cache.Type].updateCount(cache.Count));
            }
            else
            {
                playerCaches = playerCaches.SetItem(cache.Type, new ResourceCache(cache.Type, cache.Count));
            }
            delayedResources = delayedResources.SetItem(player, playerCaches);
            State = State.SetItem(StateKeyDelayedResources, delayedResources);

            /*
            if (!delayedResources.ContainsKey(player))
                delayedResources = delayedResources.SetItem(player, ImmutableDictionary<Resource, ResourceCache>.Empty);

            var playersCaches = delayedResources[player];
            if (playersCaches.ContainsKey(cache.Type))
                playersCaches = playersCaches.SetItem(cache.Type, playersCaches[cache.Type].updateCount(cache.Count));
            else
                playersCaches = playersCaches.SetItem(cache.Type, new ResourceCache(cache.Type, cache.Count));

            delayedResources = delayedResources.SetItem(player, playersCaches);
            */
            return State;
        }

        public ImmutableDictionary<AgricolaPlayer, ImmutableDictionary<Resource, ResourceCache>> GetDelayedResources()
        {
            if (!State.ContainsKey(StateKeyDelayedResources))
                return ImmutableDictionary<AgricolaPlayer, ImmutableDictionary<Resource, ResourceCache>>.Empty;
            return (ImmutableDictionary<AgricolaPlayer, ImmutableDictionary<Resource, ResourceCache>>)State[StateKeyDelayedResources];
        }


        public ImmutableDictionary<Resource, ResourceCache> GetCacheResources(State state)
        {
            return (ImmutableDictionary<Resource, ResourceCache>)state[StateKeyCacheResources];
        }

        public State SetCacheResources(ImmutableDictionary<Resource, ResourceCache> cacheResources)
        {
            return State = State.SetItem(StateKeyCacheResources, cacheResources);
        }



        /// <summary>
        /// Name of the user using this action, or null if not in use.
        /// </summary>
        public String[] UserNames
        {
            get { return Users.Select(x => x.Name).ToArray(); }
        }

        /// <summary>
        /// The name of the owner, or null if unknowned.
        /// See Owner.
        /// </summary>
        public String OwnerName
        {
            get { return Owner == null ? null : Owner.Name; }
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
                var delayedResources = GetDelayedResources();
                if (delayedResources.Keys.Count() == 0) return null;
                var result = new Dictionary<String, ResourceCache[]>();
                foreach (var player in delayedResources.Keys)
                {
                    result[player.Name] = delayedResources[player].Select(x => x.Value).ToArray();
                }
                return result;
            }
        }

        public State TakeCaches(State state, AgricolaPlayer player, Dictionary<Resource, int> leaveBehind, Dictionary<Resource, int> gained)
        {
            var cacheResources = GetCacheResources(state);
            var taking = new Dictionary<Resource, int>();
            foreach (var cache in cacheResources)
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
                    if (taking.ContainsKey(cache.Key)) {
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
                ActionService.AssignCacheResources(player, eventTriggers, ResultingNotices, taking.Select(x => new ResourceCache(x.Key, x.Value)).ToArray());

                foreach (var kvp in taking)
                {
                    if (cacheResources.ContainsKey(kvp.Key))
                    {
                        var cache = cacheResources[kvp.Key];
                        cacheResources = cacheResources.SetItem(kvp.Key, cache.updateCount(-kvp.Value));
                    }
                }
            }
            else
            {
                var cache = cacheResources.Values.First();
                if (cache.Type.IsAnimal())
                {
                    //player.Game.AddInterrupt(new AssignAnimalsAction(player, cache, ResultingNotices));
                }
                else
                {
                    ActionService.AssignCacheResource(player, eventTriggers, ResultingNotices, new ResourceCache(cache.Type, taking[cache.Type]));
                }

                cacheResources = cacheResources.SetItem(cache.Type, cache.updateCount(-taking[cache.Type]));
            }
            return State = state.SetItem(StateKeyCacheResources, cacheResources);
        }

        public State TakeCaches(State state, AgricolaPlayer player)
        {
            return State = TakeCaches(state, player, null, null);
        }

        public State AddCacheResources(State state, ResourceCache cache)
        {

            var cacheResources = GetCacheResources(state);
            if (cacheResources.ContainsKey(cache.Type))
                cacheResources = cacheResources.SetItem(cache.Type, cacheResources[cache.Type].updateCount(cache.Count));
            else
                cacheResources = cacheResources.SetItem(cache.Type, new ResourceCache(cache.Type, cache.Count));
            return State = state.SetItem(StateKeyCacheResources, cacheResources);
        }

        /// <summary>
        /// Client visibile array of chache resources
        /// </summary>
        public ResourceCache[] Cache
        {
            get { return GetCacheResources(State).Values.ToArray(); }
        }

        [JsonIgnore]
        public ImmutableDictionary<Resource, ResourceCache> CacheResources
        {
            get
            {
                return (ImmutableDictionary<Resource, ResourceCache>)State[StateKeyCacheResources];
            }
            protected set
            {
                State = State.SetItem(StateKeyCacheResources, value);
            }
        }

        /// <summary>
        /// The user using this action, or null if not in use.
        /// </summary>
        [JsonIgnore]
        protected ImmutableList<AgricolaPlayer> Users
        {
            get
            {
                return (ImmutableList<AgricolaPlayer>)State[StateKeyPlayers];
            }
            private set
            {
                State = State.SetItem(StateKeyPlayers, value);
            }
        }

        protected ResourceCache resourcesPerRound
        {
            get
            {
                return (ResourceCache)State[StateKeyResourcesPerRound];
            }
            set
            {
                State = State.SetItem(StateKeyResourcesPerRound, value);
            }
        }

        public State State
        {
            get;
            set;
        }

        private void InitializeState()
        {
            State = State.Empty;
            State = State.SetItem(StateKeyCacheResources, ImmutableDictionary<Resource, ResourceCache>.Empty);
            State = State.SetItem(StateKeyPlayers, ImmutableList<AgricolaPlayer>.Empty);
            State = State.SetItem(StateKeyDelayedResources, ImmutableDictionary<AgricolaPlayer, ImmutableDictionary<Resource, ResourceCache>>.Empty);
            State = State.SetItem(StateKeyResourcesPerRound, null);
        }


        /*
        private ImmutableDictionary<AgricolaPlayer, ImmutableDictionary<Resource, ResourceCache>> delayedResources
        {
            get
            {
                return (ImmutableDictionary<AgricolaPlayer, ImmutableDictionary<Resource, ResourceCache>>)State[StateKeyDelayedResources];
            }
            set
            {
                State = State.SetItem(StateKeyDelayedResources, value);
            }
        }
        */

        private const string StateKeyCacheResources = "CacheResources";
        private const string StateKeyPlayers = "Players";
        private const string StateKeyDelayedResources = "DelayedResources";
        private const string StateKeyResourcesPerRound = "ResourcesPerRound";

        private bool dirtyState = false;
    }
}