using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
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
                         select new PlayerChoiceOption(index++, TriggeredEvent.Create(item, this.OwningCard));

            Options = result.ToArray();
        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            ((AgricolaGame)player.Game).AddInterrupt(new PlayerChoiceAction(player, Options.ToArray(), resultingNotices));
        }

        private PlayerChoiceOption[] Options
        {
            get;
            set;
        }
    
    }
}