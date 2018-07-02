using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class BuildStableAndBakeAction: RoundAction
    {
        public BuildStableAndBakeAction(AgricolaGame game, int actionId)
            :base(game, actionId)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var stables = ((BuildStableAndBakeActionData)data).StableData.ToImmutableArray();
            var bake = ((BuildStableAndBakeActionData)data).BakeData.ToImmutableArray();

            if ((bake == null || bake.Length == 0) 
                && (stables == null || stables.Length == 0))
                return false;

            if (bake != null && bake.Length > 0 && !ActionService.CanBake(player, bake))
                return false;
            

            if (stables != null && 
                (stables.Length > 1 ||
                (stables.Length > 0 && !ActionService.CanBuildStables(player, stables, Id))))
                return false;

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            var bake = ((BuildStableAndBakeActionData)data).BakeData.ToImmutableArray();
            var stables = ((BuildStableAndBakeActionData)data).StableData.ToImmutableArray();

            if (bake != null && bake.Length > 0)
                ActionService.Bake(player, eventTriggers, ResultingNotices, bake);

            if (stables != null && stables.Length > 0)
                ActionService.BuildStables(player, stables, Id, ResultingNotices);

            return this;
        }
    }
}