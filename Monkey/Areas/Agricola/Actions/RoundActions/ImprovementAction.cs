using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class ImprovementAction: RoundAction
    {

        public ImprovementAction(AgricolaGame game, int id, bool major, bool minor)
            : base(game, id)
        {
            this.major = major;
            this.minor = minor;
        }



        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            return ActionService.CanBuyImprovement(player, (ImprovementActionData)data);
        }



        public override void OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            ActionService.BuyImprovement(player, (ImprovementActionData)data, ResultingNotices);
        }
        
        private bool major;
        private bool minor;


    }
}
