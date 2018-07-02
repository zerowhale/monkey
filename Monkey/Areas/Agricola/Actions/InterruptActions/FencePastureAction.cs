using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class FencePastureAction: InterruptAction
    {
        public FencePastureAction(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.FencePasture, resultingNotices)
        {
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            ResourceCache[] costs;
            return ActionService.CanBuildFences(player, Id, (BuildFencesActionData)data, out pastures, out costs);
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            ActionService.BuildFences(player, eventTriggers, ResultingNotices, (BuildFencesActionData)data, pastures.ToImmutableArray());
            return this;
        }

        private List<int[]> pastures;
    
    }
}