using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
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
    public class PlayerChoiceEvent : TriggeredEvent
    {
        public PlayerChoiceEvent(XElement definition)
            : base(definition)
        {
            var index = 1;
            var result = from item in definition.Descendants("Option")
                         select new PlayerChoiceOption(index++, TriggeredEvent.Create(item));

            Options = result.ToImmutableArray<PlayerChoiceOption>();
        }

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).AddInterrupt(new PlayerChoiceAction(player, Options, resultingNotices));
        }

        private ImmutableArray<PlayerChoiceOption> Options { get; }
    
    }
}