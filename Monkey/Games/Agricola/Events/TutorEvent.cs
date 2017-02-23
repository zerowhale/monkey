using Monkey.Game.Notification;
using Monkey.Games.Agricola.Cards;
using System;
using System.Collections.Generic;
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
            incrementingField = (string)definition.Attribute("Field");
        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            if (!OwningCard.Metadata.Keys.Contains(incrementingField))
                this.OwningCard.Metadata[incrementingField] = new List<Card>();

            var cards = (List<Card>)this.OwningCard.Metadata[incrementingField];
            cards.Add(this.OwningCard);
            
        }

        private string incrementingField;
    
    }
}