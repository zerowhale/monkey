using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using System;
using State = System.Collections.Immutable.ImmutableDictionary<string, object>;

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

        public override State RoundStart(State state)
        {
            State = state;
            if(familyMode)
                State = AddCacheResources(State, resourcesPerRound);
            return State = base.RoundStart(State);
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

        public override GameAction OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            if (familyMode)
            {
                State = TakeCaches(State, player);
            }
            else
            {
                var improvementData = ((StartingPlayerActionData)data).ImprovementData;
                if (improvementData != null)
                    ActionService.BuyImprovement(player, improvementData, ResultingNotices);
                    
            }

            Game.StartingPlayer = player;
            this.ResultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.Starts.ToString()));

            return this;
        }

        private Boolean familyMode { get; }
    }
}