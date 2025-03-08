using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    /// <summary>
    /// A Take Action is an action that always provides
    /// a static amount of resources when used.
    /// 
    /// A Take Action may contain more than one time of resource
    /// and each resource type may provide a different amount
    /// </summary>
    public class BasicTakeAction: RoundAction
    {

        public BasicTakeAction(AgricolaGame game, Int32 id, ResourceCache[] caches, GameEventTrigger[] eventTriggers = null)
            : base(game, id, eventTriggers)
        {
            this.Caches = caches.ToImmutableArray();
        }

        public BasicTakeAction(AgricolaGame game, Int32 id, Resource resource, Int32 count, GameEventTrigger[] eventTriggers = null)
            :base(game, id, eventTriggers)
        {
            this.Caches = ImmutableArray.Create(new ResourceCache(resource, count));
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
            ActionService.AssignTakeResources(player, eventTriggers, ResultingNotices, Caches);
            return this;
        }

        protected ImmutableArray<ResourceCache> Caches
        {
            get
            {
                return (ImmutableArray<ResourceCache>)State[StateKeyCaches];
            }
            set
            {
                State = State.SetItem(StateKeyCaches, value);
            }
        }

       

        private const string StateKeyCaches = "Caches";
    }
}