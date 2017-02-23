using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class BuildRoomOrTravelingPlayersActionData: GameActionData
    {
        public Int32? Room;

        public Boolean TakeFood;
    }
}