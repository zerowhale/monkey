using Monkey.Game.Notification;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class PerGrainBakedEvent: TriggeredEvent
    {
        public PerGrainBakedEvent(XElement definition)
            :base(definition)
        {
            var result = from item in definition.Descendants("ResourceCache")
                         select new ResourceCache(
                             (Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("Type")),
                             (int)item.Attribute("Count"));
            Resources = result.ToArray();

        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            var trigger = (BakeTrigger)ActiveTrigger;
            var resource = new ResourceCache(Resources[0].Type, Resources[0].Count * trigger.GrainBaked);
            ActionService.AssignResource(player, resource, resultingNotices);
        }

        public ResourceCache[] Resources
        {
            get;
            private set;
        }

    }
}