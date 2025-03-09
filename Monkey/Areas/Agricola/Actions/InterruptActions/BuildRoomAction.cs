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
    public class BuildRoomAction: InterruptAction
    {
        public BuildRoomAction(AgricolaPlayer player, int count, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.BuildRoom, resultingNotices)
        {
            Count = count;
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!ActionService.CanBuildRooms(
                player, 
                data.ActionId,
                ImmutableArray.Create(((BuildRoomData)data).RoomData)))
            {
                return false;
            }

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            ActionService.BuildRooms(player, eventTriggers, data.ActionId, ImmutableArray.Create(((BuildRoomData)data).RoomData), ResultingNotices);
            return this;
        }

        public int Count { get; }

    }
}