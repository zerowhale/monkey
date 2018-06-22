using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Cards;
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

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            Object cardCount;
            if(!player.TryGetCardMetadata(this.OwningCard, out cardCount))
                cardCount = -1;
            cardCount = (int)cardCount + 1;
            player.SetCardMetadata(this.OwningCard, cardCount);
        }
    }
}