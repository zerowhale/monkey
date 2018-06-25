using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Events.Triggers;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class OccupationEvent: TriggeredEvent
    {
        public OccupationEvent(XElement definition)
            : base(definition)
        {
         
        }

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).AddInterrupt(new OccupationAction(player, resultingNotices));
        }
    }
}