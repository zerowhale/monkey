using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class BuildRoomsAndStablesAction: RoundAction
    {
        public BuildRoomsAndStablesAction(AgricolaGame game, Int32 id)
            : base(game, id)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var rooms = ((BuildRoomsAndStablesActionData)data).RoomData.ToImmutableArray();
            var stables = ((BuildRoomsAndStablesActionData)data).StableData.ToImmutableArray();

            if(rooms.Length == 0 && stables.Length == 0
                || rooms.Intersect<int>(stables).Count() > 0)
                return false;

            if(rooms.Length > 0){
                if(!ActionService.CanBuildRooms(player, data.ActionId, rooms))
                    return false;
            }
            
            if(stables.Length > 0){
                if (!ActionService.CanBuildStables(player, stables, Id))
                    return false;
            }

            return true;
        }


        public override void OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            var rooms = ((BuildRoomsAndStablesActionData)data).RoomData.ToImmutableArray();
            var stables = ((BuildRoomsAndStablesActionData)data).StableData.ToImmutableArray();

            if (rooms.Length > 0)
                ActionService.BuildRooms(player, data.ActionId, rooms, ResultingNotices);

            if (stables.Length > 0)
                ActionService.BuildStables(player, stables, Id, ResultingNotices);
        }

    }
}