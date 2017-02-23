using Monkey.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class PlowEvent: TriggeredEvent
    {
        public PlowEvent(XElement definition)
            : base(definition)
        {
            this.count = definition.Attribute("Count") != null ? (int)definition.Attribute("Count") : 1;
            this.optional = definition.Attribute("Optional") != null ? (bool)definition.Attribute("Optional") : false;
        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).AddInterrupt(new PlowAction(player, resultingNotices, optional));
        }

        private int count;
        private bool optional;
    }
}