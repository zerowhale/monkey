using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Events
{
    public class EventData
    {
        public EventData(GameEventTrigger trigger, TriggeredEvent triggeredEvent, Card card)
        {
            Trigger = trigger;
            TriggeredEvent = triggeredEvent;
            Card = card;
        }

        public GameEventTrigger Trigger { get; }
        public TriggeredEvent TriggeredEvent { get; }
        public Card Card { get; }

    }
}