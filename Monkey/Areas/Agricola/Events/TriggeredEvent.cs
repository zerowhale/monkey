using Monkey.Games.Agricola.Events.Conditionals;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using BoardgamePlatform.Game.Notification;
using static Monkey.Games.Agricola.Events.GainResourcesEvent;
using System.Collections.Immutable;
using BoardgamePlatform.Game;

namespace Monkey.Games.Agricola.Events
{
    public abstract class TriggeredEvent : GameEvent
    {

        public TriggeredEvent(GameEventTrigger[] triggers)
            : base()
        {
            Triggers = triggers;
        }

        public TriggeredEvent(XElement definition)
            : base(definition)
        {
            Triggers = definition.Elements("Trigger").Select(GameEventTrigger.Create).ToArray();
        }

        new public static TriggeredEvent Create(XElement options)
        {
            var cls = (string)options.Attribute("Class");
            var type = Type.GetType(cls);

            return (TriggeredEvent)Activator.CreateInstance(type, options);
        }

        public static TriggeredEvent Create(XElement options, Card owningCard)
        {
            return TriggeredEvent.Create(options);
        }

        [JsonIgnore]
        public readonly GameEventTrigger[] Triggers;



    }
}