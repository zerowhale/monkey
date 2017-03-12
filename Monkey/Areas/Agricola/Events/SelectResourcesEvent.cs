using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class SelectResourcesEvent : TriggeredEvent
    {
        public SelectResourcesEvent(XElement definition)
            :base(definition)
        {
            var result = from item in definition.Descendants("ResourceCache")
                         select new ResourceCache(
                             (Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("Type")),
                             (int)item.Attribute("Count"));
            ResourceOptions = result.ToArray();

        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).AddInterrupt(new SelectResourcesAction(player, ResourceOptions, 1, resultingNotices));
        }

        public ResourceCache[] ResourceOptions
        {
            get;
            private set;
        }
    
    }
}