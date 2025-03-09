using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class BuildRoomsTrigger : GameEventTrigger
    {
        public BuildRoomsTrigger(int roomsBuilt)
            :base()
        {
            RoomsBuilt = roomsBuilt;
        }

        public BuildRoomsTrigger(XElement definition)
            : base(definition)
        {

        }

        public int RoomsBuilt { get; }
    }
}