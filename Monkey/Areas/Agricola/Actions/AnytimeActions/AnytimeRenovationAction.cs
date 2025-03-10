using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Actions.AnytimeActions
{
    public sealed class AnytimeRenovationAction : AnytimeAction
    {
        public AnytimeRenovationAction(XElement definition, int cardId)
            : base(definition, (int)AnytimeActionId.Renovation, cardId)
        {
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            if (player.Farmyard.HouseType != HouseType.Wood && this.CardId == (int)Monkey.Games.Agricola.Cards.CardId.BuildersTrowel)
                return false;

            if (!ActionService.CanRenovate(player))
                return false;

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            ActionService.Renovate(player, eventTriggers, ResultingNotices);
 
            return this;
        }


    }
}