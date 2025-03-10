﻿using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class BuildStableAction: InterruptAction
    {
        public BuildStableAction(AgricolaPlayer player, int count, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.BuildStable, resultingNotices)
        {
            Count = count;
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            var stables = ImmutableArray.Create(((BuildStableActionData)data).StableData);

            if (stables.Length > Count)
                return false;
            
            if (stables.Length > 0)
            {
                if (!ActionService.CanBuildStables(player, stables, Id))
                    return false;
            }

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            ActionService.BuildStables(player, ImmutableArray.Create(((BuildStableActionData)data).StableData), Id, ResultingNotices);
            return this;
        }

        public int Count { get; }

    }
}