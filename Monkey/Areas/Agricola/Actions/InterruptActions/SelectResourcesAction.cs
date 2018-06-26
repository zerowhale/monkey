using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class SelectResourcesAction : InterruptAction
    {
        public SelectResourcesAction(AgricolaPlayer player, ImmutableArray<ResourceCache> options, int count, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.SelectResources, resultingNotices)
        {
            this.Options = options;
            this.NumRequired = count;
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            var rsData = (SelectResourcesActionData)data;
            if (rsData.Resources.Length > NumRequired)
                return false;

            var ins = Options.Select(x => x.Type).Intersect(rsData.Resources).Count();
            if (ins == 0)
                return false;

            return true;
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            var rsData = (SelectResourcesActionData)data;
            var list = rsData.Resources.Select(x => new ResourceCache(x, 1)).ToArray();
            ActionService.AssignResources(player, list, ResultingNotices);
        }

        public ImmutableArray<ResourceCache> Options { get; }

        public int NumRequired { get; }
    }
}