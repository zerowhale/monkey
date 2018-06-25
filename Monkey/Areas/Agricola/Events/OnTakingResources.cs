using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Events
{
    public class OnTakingResources : GameEvent
    {
        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            return;
        }
    }
}