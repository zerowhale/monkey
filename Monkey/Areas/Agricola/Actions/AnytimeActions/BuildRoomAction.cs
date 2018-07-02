using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Actions.AnytimeActions
{
    public sealed class BuildRoomAction: AnytimeAction
    {
        public BuildRoomAction(XElement definition, int cardId)
            : base(definition, (int)AnytimeActionId.BuildRoom, cardId)
        {
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var room = ((BuildRoomData)data).RoomData;

            if (!ActionService.CanBuildRooms(player, data.ActionId, ImmutableArray.Create<int>(room) ))
                return false;

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            var room = ((BuildRoomData)data).RoomData;
            ActionService.BuildRooms(player, data.ActionId, ImmutableArray.Create<int>(room), ResultingNotices);

            return this;
        }

    }
}