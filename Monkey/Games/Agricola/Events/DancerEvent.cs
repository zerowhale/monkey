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
    public class DancerEvent: TriggeredEvent
    {
        public DancerEvent(XElement definition)
            :base(definition)
        {

        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            var trigger = (TravelingPlayersActionTrigger)this.ActiveTrigger;

            var dif = 4 - trigger.FoodTaken;

            if (dif > 0)
            {
                var resource = new ResourceCache(Resource.Food, dif);
                ActionService.AssignResource(player, resource, resultingNotices);
            }
        }
    
    }
}