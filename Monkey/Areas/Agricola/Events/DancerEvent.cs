using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
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

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            var dif = 4 - ((TravelingPlayersActionTrigger)trigger).FoodTaken;

            if (dif > 0)
            {
                var resource = new ResourceCache(Resource.Food, dif);
                ActionService.AssignResource(player, resource, resultingNotices);
            }
        }
    
    }
}