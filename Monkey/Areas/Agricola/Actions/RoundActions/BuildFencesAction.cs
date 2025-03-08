using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Farm;
using Monkey.Games.Agricola.Notification;
using Monkey.Games.Agricola.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class BuildFencesAction: RoundAction
    {
        public BuildFencesAction(AgricolaGame game, int id)
            :base(game, id) { }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;
            return ActionService.CanBuildFences(player, Id, (BuildFencesActionData)data, out pastures, out costs);
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            player.PayCosts(costs);

            ActionService.BuildFences(player, eventTriggers, ResultingNotices, (BuildFencesActionData)data, pastures.ToImmutableArray());
            return this;
        }

        /// <summary>
        /// Non-state 
        /// </summary>
        private List<int[]> pastures;

        /// <summary>
        /// Non-state
        /// </summary>
        private ResourceCache[] costs;
    }
}