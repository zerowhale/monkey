using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class TutorEvent: TriggeredEvent
    {
        public TutorEvent(XElement definition)
            : base(definition)
        {
        }

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            ImmutableDictionary<string, Object> metadata;
            Object fieldData;
            int cardCount = -1;
            if(player.TryGetCardMetadataField(card, "tutor", out metadata, out fieldData)){
                cardCount = (int)fieldData;
            }
            player.SetCardMetadataField(card, "tutor", cardCount + 1);
        }
    }
}