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
        public CookAction()
            : base((int)AnytimeActionId.Cook)
        {

        }

        public CookAction(XElement definition)
            : base((int)AnytimeActionId.Cook, definition)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {

            return base.CanExecute(player, data)
                && ActionService.CanCook(player, ((CookActionData)data).Resources)
                && ActionService.CanAssignAnimals(player, ((CookActionData)data).AnimalData, null);
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            ActionService.Cook(player, eventTriggers, ((CookActionData)data).Resources, ResultingNotices);
            ActionService.AssignAnimals(player, ((CookActionData)data).AnimalData, ResultingNotices);
        }

        
    }
}