using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class DelayedResourcesEvent : TriggeredEvent
    {
        public DelayedResourcesEvent(GameEventTrigger[] triggers)
            :base(triggers)
        {

        }

        public DelayedResourcesEvent(XElement definition)
            : base(definition)
        {
            var result = from item in definition.Elements("DelayedResourceCache")
                         select new DelayedResourceCache(
                             item.Attribute("OnRound") == null ? (int)item.Attribute("RoundsDelayed") : (int)item.Attribute("OnRound"),
                             (Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("ResourceType")),
                             (int)item.Attribute("ResourceCount"),
                             item.Attribute("OnRound") != null);

            Resources = result.ToImmutableArray();
        }

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).StoreDelayedResources(player, Resources);
        }

        private ImmutableArray<DelayedResourceCache> Resources { get; }
    }
}