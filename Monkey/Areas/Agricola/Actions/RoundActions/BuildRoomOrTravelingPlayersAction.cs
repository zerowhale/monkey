using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class BuildRoomOrTravelingPlayersAction: BasicCacheAction
    {
        public BuildRoomOrTravelingPlayersAction(AgricolaGame game, Int32 actionId, GameEventTrigger[] eventTriggers = null)
            :base(game, actionId, Resource.Food, 1, eventTriggers)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var brotpaData = (BuildRoomOrTravelingPlayersActionData)data;

            if (brotpaData.TakeFood == false
                && (!brotpaData.Room.HasValue || !ActionService.CanBuildRooms(player, data.ActionId, ImmutableArray.Create(brotpaData.Room.Value) )))
                return false;

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            var roomData = ((BuildRoomOrTravelingPlayersActionData)data).Room;
            var foodData = ((BuildRoomOrTravelingPlayersActionData)data).TakeFood;

            if (foodData)
                TakeCaches(State, player);
            else
            {
                ActionService.BuildRooms(player, eventTriggers, data.ActionId, ImmutableArray.Create(roomData.Value), ResultingNotices);
            }
            return this;
        }
    }
}