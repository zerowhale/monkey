using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class TravelingPlayersActionTrigger: GameEventTrigger
    {
        public TravelingPlayersActionTrigger()
            :base()
        {

        }

        public TravelingPlayersActionTrigger(XElement definition)
            : base(definition)
        {

        }

        public int FoodTaken
        {
            get;
            set;
        }
    
    }
}