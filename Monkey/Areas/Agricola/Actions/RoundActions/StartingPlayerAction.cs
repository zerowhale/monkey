using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class StartingPlayerAction: RoundAction
    {
        public StartingPlayerAction(AgricolaGame game, Int32 id, Boolean familyMode)
            : base(game, id)
        {
            this.familyMode = familyMode;
            if (familyMode)
                resourcesPerRound = new ResourceCache(Resource.Food, 1);
        }

        public override void RoundStart()
        {
            if(familyMode)
                AddCacheResources(resourcesPerRound);
            base.RoundStart();
        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            if (!familyMode)
            {
                var improvementData = ((StartingPlayerActionData)data).ImprovementData;
                if (improvementData != null && !ActionService.CanBuyImprovement(player, improvementData))
                    return false;
            }

            return true;
        }

        public override void OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            if (familyMode)
            {
                this.TakeCaches(player);
            }
            else
            {
                var improvementData = ((StartingPlayerActionData)data).ImprovementData;
                if (improvementData != null)
                    ActionService.BuyImprovement(player, improvementData, ResultingNotices);
                    
            }
            Game.StartingPlayer = player;
            this.ResultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Starts.ToString()));
        }

        private ResourceCache resourcesPerRound
        {
            get;
            set;
        }

        private Boolean familyMode;
    }
}