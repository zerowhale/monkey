using Monkey.Game.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Events
{
    public class OnTakingResources : GameEvent
    {
        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            return;
        }
    }
}