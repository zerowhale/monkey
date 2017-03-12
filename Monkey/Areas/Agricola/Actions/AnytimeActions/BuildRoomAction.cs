using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Actions.AnytimeActions
{
    public class BuildRoomAction: AnytimeAction
    {
        public BuildRoomAction()
            : base((int)AnytimeActionId.BuildRoom)
        {

        }

        public BuildRoomAction(XElement definition)
            : base((int)AnytimeActionId.BuildRoom, definition)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var room = ((BuildRoomData)data).RoomData;

            if (!ActionService.CanBuildRooms(player, data.ActionId, new int[]{ room }))
                return false;

            return true;
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            var room = ((BuildRoomData)data).RoomData;

            ActionService.BuildRooms(player, data.ActionId, new int[]{ room }, ResultingNotices);
        }

    }
}