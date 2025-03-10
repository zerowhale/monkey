﻿using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.RoundActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class BakeAction: InterruptAction
    {
        public BakeAction(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.Bake, resultingNotices, new BakeTrigger())
        {
        }
        
        /// <summary>
        /// Checks if a player can bake.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            var bakeData = ImmutableArray.Create(((BakeActionData)data).BakeData);
            if (bakeData == null || bakeData.Length == 0)
                return true;

            return ActionService.CanBake(player, bakeData);
        }

        /// <summary>
        /// Executes the bake action for the player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            var bakeData = ImmutableArray.Create(((BakeActionData)data).BakeData);
            if(bakeData != null && bakeData.Length > 0)
                ActionService.Bake(player, eventTriggers, ResultingNotices, bakeData);
            return this;
        }


    }
}