using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class BuildStableEvent: GameEvent
    {
        public BuildStableEvent(XElement definition)
            : base(definition)
        {
            this.count = definition.Attribute("Count") != null ? (int)definition.Attribute("Count") : 1;
        }

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).AddInterrupt(new BuildStableAction(player, count, resultingNotices));
        }

        private readonly int count;
    
    }
}