using Monkey.Games.Agricola.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public class PlayerChoiceOption
    {

        public PlayerChoiceOption(int id, TriggeredEvent triggeredEvent){
            Id = id;
            Event = triggeredEvent;
        }

        public readonly int Id;

        public readonly TriggeredEvent Event;
    }
}