using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class RenovationAction: RoundAction
    {
        public RenovationAction(AgricolaGame game, int id, RenovationActionMode mode)
            : base(game, id)
        {
            this.mode = mode;
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var renovationData = (RenovationActionData)data;

            ResourceCache[] renovationCosts;
            additionalCosts = null;

            if (!ActionService.CanRenovate(player, out renovationCosts))
                return false;

            switch (mode)
            {
                case RenovationActionMode.Fences:
                    if(!(renovationData.FenceData == null 
                        || renovationData.FenceData.Fences.Length == 0
                        || ActionService.CanBuildFences(player, Id, renovationData.FenceData, out pastures, out additionalCosts)))
                        return false;
                    break;
                
                case RenovationActionMode.Improvement:
                    if (!(renovationData.ImprovementData == null
                        || ActionService.CanBuyImprovement(player, renovationData.ImprovementData, out additionalCosts)))
                        return false;
                    break;
            }

            var totalCosts = new Dictionary<Resource, ResourceCache>();
            if (renovationCosts != null)
            {
                foreach(var r in renovationCosts)
                    totalCosts[r.Type] = r;
            }

            if (additionalCosts != null)
            {
                foreach (var cost in additionalCosts)
                {
                    if (totalCosts.ContainsKey(cost.Type))
                        totalCosts[cost.Type].Count += cost.Count;
                    else
                        totalCosts[cost.Type] = cost;
                }
            }

            return Curator.CanAfford(player, totalCosts.Values.ToArray());
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            ActionService.Renovate(player, ResultingNotices);

            switch (mode)
            {
                case RenovationActionMode.Fences:
                    player.PayCosts(additionalCosts);
                    ActionService.BuildFences(player, eventTriggers, ResultingNotices, ((RenovationActionData)data).FenceData, pastures);
                    break;
                
                case RenovationActionMode.Improvement:
                    var improvementData = ((RenovationActionData)data).ImprovementData;
                    if(improvementData != null)
                        ActionService.BuyImprovement(player, ((RenovationActionData)data).ImprovementData, ResultingNotices);
                    break;
            }
        }

        private RenovationActionMode mode;
        private List<int[]> pastures;
        private ResourceCache[] additionalCosts;
    }
}