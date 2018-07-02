using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Actions.AnytimeActions
{
    public class CookAction : AnytimeAction
    {
        public CookAction(XElement definition, int cardId)
            : base(definition, (int)AnytimeActionId.Cook, cardId)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            return base.CanExecute(player, data)
                && ActionService.CanCook(player, ((CookActionData)data).Resources)
                && ActionService.CanAssignAnimals(player, ((CookActionData)data).AnimalData, null);
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            ActionService.Cook(player, eventTriggers, ((CookActionData)data).Resources, ResultingNotices);
            ActionService.AssignAnimals(player, ((CookActionData)data).AnimalData, ResultingNotices);

            return this;
        }

        
    }
}