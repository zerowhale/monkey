using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class DelayedResourcesEvent : GameEvent
    {
        public DelayedResourcesEvent()
            :base()
        {

        }

        public DelayedResourcesEvent(XElement definition)
            : base(definition)
        {
            var result = from item in definition.Descendants("DelayedResourceCache")
                         select new DelayedResourceCache(
                             item.Attribute("OnRound") == null ? (int)item.Attribute("RoundsDelayed") : (int)item.Attribute("OnRound"),
                             (Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("ResourceType")),
                             (int)item.Attribute("ResourceCount"))
                             {
                                 OnRound = item.Attribute("OnRound") != null
                             };

            Resources = result.ToArray();
        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).StoreFutureResources(player, Resources);
        }

        private DelayedResourceCache[] Resources
        {
            get;
            set;
        }
    }
}