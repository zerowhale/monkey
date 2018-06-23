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
            ImmutableDictionary<string, Object> metadata;
            int cardCount = -1;
            if (player.TryGetCardMetadata(this.OwningCard, out metadata))
                cardCount = (int)metadata["tutor"];
            else
                metadata = ImmutableDictionary<string, Object>.Empty;
            player.SetCardMetadata(this.OwningCard, metadata.SetItem("tutor", cardCount + 1));
        }
    }
}