using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class FamilyGrowthAction: RoundAction
    {
        public FamilyGrowthAction(AgricolaGame game, int id, FamilyGrowthActionMode mode)
            :base(game, id)
        {
            this.mode = mode;
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var fgData = (FamilyGrowthActionData)data;

            if (!Curator.CanGrowFamily(player, mode == FamilyGrowthActionMode.WithoutSpace))
                return false;

            if (mode == FamilyGrowthActionMode.Improvement
                && fgData.ImprovementData != null
                && !ActionService.CanBuyImprovement(player, fgData.ImprovementData))
                return false;

            if (player.FamilySize == 5)
                return false;

            return true;
        }


        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            player.AddFamilyMember();
            AddUser(player);    // Add the baby to the action display

            ResultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.GrowFamily.ToString()));

            if (mode == FamilyGrowthActionMode.Improvement && ((FamilyGrowthActionData)data).ImprovementData != null)
                ActionService.BuyImprovement(player, ((FamilyGrowthActionData)data).ImprovementData, ResultingNotices);

            return this;
        }

        private FamilyGrowthActionMode mode;
    }
}